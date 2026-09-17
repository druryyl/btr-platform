using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using AggregateAgingBucket = btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardPiutangAgingBucket;
using AggregateTopCustomerRiskRow = btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardPiutangTopCustomerRiskRow;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.Shared;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardExecutiveComposerTest
    {
        private readonly DashboardExecutiveComposer _composer = new DashboardExecutiveComposer();
        private readonly DateTime _utcNow = new DateTime(2026, 6, 8, 10, 0, 0, DateTimeKind.Utc);
        private readonly DashboardSnapshotOptions _options = new DashboardSnapshotOptions
        {
            PiutangIntervalMinutes = 15,
            SalesIntervalMinutes = 30,
            PurchasingIntervalMinutes = 30,
            InventoryIntervalMinutes = 60
        };

        [Fact]
        public void Compose_WhenAllSnapshotsPresent_AllSectionsAvailable()
        {
            var result = Compose(FullInput());

            result.HasUnavailableDomain.Should().BeFalse();
            result.Sales.IsAvailable.Should().BeTrue();
            result.Piutang.IsAvailable.Should().BeTrue();
            result.Inventory.IsAvailable.Should().BeTrue();
            result.Purchasing.IsAvailable.Should().BeTrue();
        }

        [Fact]
        public void Compose_WhenPiutangMissing_HasUnavailableDomainAndPiutangUnavailable()
        {
            var input = FullInput();
            input.Piutang = null;

            var result = Compose(input);

            result.HasUnavailableDomain.Should().BeTrue();
            result.Piutang.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public void Compose_WhenAchievementCritical_RequiresAttention()
        {
            var input = FullInput();
            input.Sales.AchievementPercent = 75m;

            var result = Compose(input);

            result.Sales.AchievementBand.Should().Be(ExecutiveSalesAchievementBandResolver.Critical);
            result.Sales.RequiresAttention.Should().BeTrue();
        }

        [Fact]
        public void Compose_WhenAchievementHealthy_DoesNotRequireAttention()
        {
            var input = FullInput();
            input.Sales.AchievementPercent = 105m;

            var result = Compose(input);

            result.Sales.AchievementBand.Should().Be(ExecutiveSalesAchievementBandResolver.Healthy);
            result.Sales.RequiresAttention.Should().BeFalse();
        }

        [Fact]
        public void Compose_WhenOverdueCustomers_RequiresPiutangAttention()
        {
            var input = FullInput();
            input.Piutang.OverdueCustomer = 3;
            input.Piutang.AgingBuckets = new List<AggregateAgingBucket>();

            var result = Compose(input);

            result.Piutang.RequiresAttention.Should().BeTrue();
        }

        [Fact]
        public void Compose_CalculatesAgingOver90Percent()
        {
            var input = FullInput();
            input.Piutang.TotalPiutang = 1000m;
            input.Piutang.AgingBuckets = new List<AggregateAgingBucket>
            {
                new AggregateAgingBucket { BucketKey = "DaysOver90", Amount = 250m }
            };

            var result = Compose(input);

            result.Piutang.AgingOver90Amount.Should().Be(250m);
            result.Piutang.AgingOver90Percent.Should().Be(25m);
        }

        [Fact]
        public void Compose_CalculatesTopCustomerPercent()
        {
            var input = FullInput();
            input.Piutang.TotalPiutang = 1000m;
            input.Piutang.TopCustomerRisk = new List<AggregateTopCustomerRiskRow>
            {
                new AggregateTopCustomerRiskRow { Rank = 1, CustomerName = "A", TotalPiutang = 400m }
            };

            var result = Compose(input);

            result.Piutang.TopCustomerPercent.Should().Be(40m);
        }

        [Fact]
        public void Compose_UsesBelumValueForPendingPosting()
        {
            var input = FullInput();
            input.Purchasing.PostingStatus = new List<DashboardPurchasingPostingStatusRow>
            {
                new DashboardPurchasingPostingStatusRow { StatusKey = "SUDAH", PurchaseAmount = 500m },
                new DashboardPurchasingPostingStatusRow { StatusKey = "BELUM", PurchaseAmount = 750m }
            };

            var result = Compose(input);

            result.Purchasing.PendingPostingValue.Should().Be(750m);
        }

        [Fact]
        public void Compose_Purchasing_UnqualifiedBelumOnly_DoesNotRequireAttention()
        {
            var input = FullInput();
            input.PurchasingManagement = new DashboardPurchasingManagementAggregateResult
            {
                QualifiedBacklogCount = 0
            };

            var result = Compose(input);

            result.Purchasing.RequiresAttention.Should().BeFalse();
        }

        [Fact]
        public void Compose_Purchasing_QualifiedBacklogPresent_RequiresAttention()
        {
            var input = FullInput();
            input.PurchasingManagement = new DashboardPurchasingManagementAggregateResult
            {
                QualifiedBacklogCount = 2
            };

            var result = Compose(input);

            result.Purchasing.RequiresAttention.Should().BeTrue();
            result.Purchasing.QualifiedBacklogCount.Should().Be(2);
        }

        [Fact]
        public void Compose_Purchasing_QualifiedZeroWithBelumValue_DoesNotRequireAttention()
        {
            var input = FullInput();
            input.Purchasing.PostingStatus = new List<DashboardPurchasingPostingStatusRow>
            {
                new DashboardPurchasingPostingStatusRow { StatusKey = "BELUM", PurchaseAmount = 500000m }
            };
            input.PurchasingManagement = new DashboardPurchasingManagementAggregateResult
            {
                QualifiedBacklogCount = 0
            };

            var result = Compose(input);

            result.Purchasing.PendingPostingValue.Should().Be(500000m);
            result.Purchasing.RequiresAttention.Should().BeFalse();
        }

        [Fact]
        public void Compose_TruncatesCriticalExposuresToTop5()
        {
            var input = FullInput();
            input.Piutang.TopCustomerRisk = Enumerable.Range(1, 10)
                .Select(i => new AggregateTopCustomerRiskRow
                {
                    Rank = i,
                    CustomerName = $"Customer {i}",
                    TotalPiutang = 1000m - i
                })
                .ToList();

            var result = Compose(input);

            result.CriticalExposures.TopCustomers.Should().HaveCount(5);
            result.CriticalExposures.TopCustomers.First().Rank.Should().Be(1);
        }

        [Fact]
        public void Compose_LastRefreshedIsMinGeneratedAt()
        {
            var input = FullInput();
            input.Sales.GeneratedAt = _utcNow.AddMinutes(-10);
            input.Piutang.GeneratedAt = _utcNow.AddMinutes(-5);
            input.Inventory.GeneratedAt = _utcNow.AddMinutes(-20);
            input.Purchasing.GeneratedAt = _utcNow.AddMinutes(-3);

            var result = Compose(input);

            result.LastRefreshed.Should().Be(_utcNow.AddMinutes(-20));
        }

        [Fact]
        public void Compose_WhenAllDomainsWithinInterval_IsDataFreshTrue()
        {
            var input = FullInput();
            input.Sales.GeneratedAt = _utcNow.AddMinutes(-10);
            input.Piutang.GeneratedAt = _utcNow.AddMinutes(-5);
            input.Inventory.GeneratedAt = _utcNow.AddMinutes(-30);
            input.Purchasing.GeneratedAt = _utcNow.AddMinutes(-10);

            var result = Compose(input);

            result.IsDataFresh.Should().BeTrue();
        }

        [Fact]
        public void Compose_WhenOneDomainExceedsInterval_IsDataFreshFalse()
        {
            var input = FullInput();
            input.Sales.GeneratedAt = _utcNow.AddMinutes(-10);
            input.Piutang.GeneratedAt = _utcNow.AddMinutes(-5);
            input.Inventory.GeneratedAt = _utcNow.AddMinutes(-90);
            input.Purchasing.GeneratedAt = _utcNow.AddMinutes(-10);

            var result = Compose(input);

            result.IsDataFresh.Should().BeFalse();
        }

        [Fact]
        public void Compose_TopPrincipalPercentUsesGrandTotalPurchase()
        {
            var input = FullInput();
            input.Purchasing.GrandTotalPurchase = 1000m;
            input.Purchasing.TopPrincipal = new List<DashboardPurchasingTopPrincipalRow>
            {
                new DashboardPurchasingTopPrincipalRow { Rank = 1, PrincipalName = "P1", PurchaseAmount = 350m }
            };

            var result = Compose(input);

            result.Purchasing.TopPrincipalPercent.Should().Be(35m);
        }

        private ExecutiveComposeInput FullInput()
        {
            var generatedAt = _utcNow.AddMinutes(-5);

            return new ExecutiveComposeInput
            {
                UtcNow = _utcNow,
                Options = _options,
                RefreshStatuses = new List<DashboardSnapshotRefreshStatusModel>
                {
                    new DashboardSnapshotRefreshStatusModel { Domain = "Sales", Status = "Success" },
                    new DashboardSnapshotRefreshStatusModel { Domain = "Piutang", Status = "Success" },
                    new DashboardSnapshotRefreshStatusModel { Domain = "Inventory", Status = "Success" },
                    new DashboardSnapshotRefreshStatusModel { Domain = "Purchasing", Status = "Success" }
                },
                Sales = new DashboardSalesAggregateResult
                {
                    GeneratedAt = generatedAt,
                    TotalAchievement = 5000000m,
                    AchievementPercent = 95m
                },
                Piutang = new DashboardPiutangAggregateResult
                {
                    GeneratedAt = generatedAt,
                    TotalPiutang = 10000000m,
                    OverdueCustomer = 0,
                    AgingBuckets = new List<AggregateAgingBucket>(),
                    TopCustomerRisk = new List<AggregateTopCustomerRiskRow>()
                },
                Inventory = new DashboardInventoryAggregateResult
                {
                    GeneratedAt = generatedAt,
                    TotalInventoryValue = 20000000m,
                    Breakdown = new List<DashboardInventoryBreakdownRow>
                    {
                        new DashboardInventoryBreakdownRow
                        {
                            DimensionType = DashboardInventoryAggregator.DimensionCategory,
                            Name = "Cat A",
                            InventoryValue = 8000000m,
                            IsTop10 = true,
                            Top10Rank = 1
                        }
                    }
                },
                Purchasing = new DashboardPurchasingAggregateResult
                {
                    GeneratedAt = generatedAt,
                    GrandTotalPurchase = 3000000m,
                    PendingPostingInvoiceCount = 2,
                    PostingStatus = new List<DashboardPurchasingPostingStatusRow>
                    {
                        new DashboardPurchasingPostingStatusRow { StatusKey = "BELUM", PurchaseAmount = 500000m }
                    },
                    TopPrincipal = new List<DashboardPurchasingTopPrincipalRow>()
                }
            };
        }

        [Fact]
        public void Compose_CriticalExposures_AttachInvestigationMetadata()
        {
            var input = FullInput();
            input.Piutang.TopCustomerRisk = new List<AggregateTopCustomerRiskRow>
            {
                new AggregateTopCustomerRiskRow
                {
                    Rank = 1,
                    CustomerName = "Alpha Corp",
                    CustomerCode = "C001",
                    TotalPiutang = 5_000_000m
                }
            };
            input.Purchasing.TopPrincipal = new List<DashboardPurchasingTopPrincipalRow>
            {
                new DashboardPurchasingTopPrincipalRow
                {
                    Rank = 1,
                    PrincipalName = "Principal A",
                    PurchaseAmount = 1_000_000m
                }
            };

            var result = Compose(input);

            result.CriticalExposures.TopCustomers[0].Investigation.ReportRoute
                .Should().Be(InvestigationRegistry.PiutangReportRoute);
            result.CriticalExposures.TopCustomers[0].Investigation.SuggestedQuery.PeriodMode
                .Should().Be(InvestigationRegistry.PeriodModeAllOpenBalances);
            result.CriticalExposures.TopPrincipals[0].Investigation.ReportRoute
                .Should().Be(InvestigationRegistry.PurchasingReportRoute);
            result.CriticalExposures.TopPrincipals[0].DashboardRoute.Should().BeNull();
        }

        [Fact]
        public void Compose_PrincipalSalesAttention_UsesStoredPrincipalSalesOutAndRoutesToSa04()
        {
            var input = FullInput();
            input.PrincipalSalesOut = PrincipalSalesOut(
                ("SUP-B", "Beta", 40m, 2),
                ("SUP-A", "Alpha", 60m, 1));
            input.Purchasing.GrandTotalPurchase = 1000m;
            input.Purchasing.TopPrincipal = new List<DashboardPurchasingTopPrincipalRow>
            {
                new DashboardPurchasingTopPrincipalRow
                {
                    Rank = 1,
                    PrincipalName = "Purchase Principal",
                    PurchaseAmount = 900m
                }
            };

            var result = Compose(input);

            result.Sales.TotalAchievement.Should().Be(5000000m);
            result.Purchasing.TopPrincipalPercent.Should().Be(90m);
            result.Inventory.TopCategoryPercent.Should().NotBeNull();
            result.PrincipalSales.IsAvailable.Should().BeTrue();
            result.PrincipalSales.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            result.PrincipalSales.PrincipalSalesOutAmount.Should().Be(100m);
            result.PrincipalSales.TopPrincipalName.Should().Be("Alpha");
            result.PrincipalSales.TopPrincipalPercent.Should().Be(60m);
            result.PrincipalSales.DashboardRoute.Should().Be(DashboardExecutiveComposer.PrincipalSalesDashboardRoute);
            result.PrincipalSales.DashboardRoute.Should().NotBe(DashboardExecutiveComposer.PurchasingDashboardRoute);
            result.PrincipalSales.Disclosures.Should().Contain(PrincipalSalesOutDisclosure.ReturnsDoNotReduceOrRedefinePrincipalSalesOut);
            result.CriticalExposures.TopPrincipalSales.Should().HaveCount(2);
            result.CriticalExposures.TopPrincipalSales[0].Name.Should().Be("Alpha");
            result.CriticalExposures.TopPrincipalSales[0].Amount.Should().Be(60m);
            result.CriticalExposures.TopPrincipalSales[0].SupplierId.Should().Be("SUP-A");
            result.CriticalExposures.TopPrincipalSales[0].DashboardRoute
                .Should().Be(DashboardExecutiveComposer.PrincipalSalesDashboardRoute);
            result.CriticalExposures.TopPrincipalSales[0].Investigation.Should().BeNull();
            result.DomainSummaries.First().Domain.Should().Be("Principal Sales");
            result.DomainSummaries.First().DetailDashboardRoute
                .Should().Be(DashboardExecutiveComposer.PrincipalSalesDashboardRoute);
            result.DomainSummaries.Single(summary => summary.Domain == "Sales").DetailDashboardRoute
                .Should().Be("/dashboard/sales");
        }

        [Fact]
        public void Compose_PrincipalSalesAttention_WhenSalesOutMissing_DoesNotUsePurchaseOrInventory()
        {
            var input = FullInput();
            input.Purchasing.GrandTotalPurchase = 1000m;
            input.Purchasing.TopPrincipal = new List<DashboardPurchasingTopPrincipalRow>
            {
                new DashboardPurchasingTopPrincipalRow
                {
                    Rank = 1,
                    PrincipalName = "Purchase Principal",
                    PurchaseAmount = 250m
                }
            };

            var result = Compose(input);

            result.PrincipalSales.IsAvailable.Should().BeFalse();
            result.PrincipalSales.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            result.PrincipalSales.PrincipalSalesOutAmount.Should().Be(0m);
            result.PrincipalSales.RequiresAttention.Should().BeFalse();
            result.PrincipalSales.DashboardRoute.Should().Be(DashboardExecutiveComposer.PrincipalSalesDashboardRoute);
            result.CriticalExposures.TopPrincipalSales.Should().BeEmpty();
            result.CriticalExposures.TopPrincipals.Should().ContainSingle();
            result.CriticalExposures.TopPrincipals[0].Amount.Should().Be(250m);
            result.Purchasing.IsAvailable.Should().BeTrue();
            result.Sales.IsAvailable.Should().BeTrue();
        }

        [Fact]
        public void Compose_PrincipalSalesAttention_TruncatesExposureToTop5AndIgnoresOtherKpi()
        {
            var input = FullInput();
            input.PrincipalSalesOut = new PrincipalSalesOutAggregateResult
            {
                KpiId = "PRN-RET-004",
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = new List<PrincipalSalesOutRow>
                {
                    new PrincipalSalesOutRow
                    {
                        KpiId = "PRN-RET-004",
                        SupplierId = "SUP-R",
                        SupplierName = "Return Principal",
                        SalesOutAmount = 99m,
                        SortOrder = 1
                    }
                }
            };

            var result = Compose(input);

            result.PrincipalSales.IsAvailable.Should().BeFalse();
            result.CriticalExposures.TopPrincipalSales.Should().BeEmpty();

            input.PrincipalSalesOut = PrincipalSalesOut(
                Enumerable.Range(1, 7)
                    .Select(i => ($"SUP-{i}", $"Principal {i}", 100m - i, i))
                    .ToArray());

            result = Compose(input);

            result.CriticalExposures.TopPrincipalSales.Should().HaveCount(5);
            result.CriticalExposures.TopPrincipalSales.Select(row => row.Amount)
                .Should().Equal(99m, 98m, 97m, 96m, 95m);
        }

        private static PrincipalSalesOutAggregateResult PrincipalSalesOut(
            params (string SupplierId, string Name, decimal Amount, int SortOrder)[] rows)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = rows.Select(row => new PrincipalSalesOutRow
                {
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    SupplierId = row.SupplierId,
                    SupplierName = row.Name,
                    SalesOutAmount = row.Amount,
                    SortOrder = row.SortOrder
                }).ToList()
            };
        }

        private btr.application.ReportingContext.DashboardExecutiveAgg.Queries.DashboardExecutiveResponse Compose(
            ExecutiveComposeInput input)
        {
            return _composer.Compose(input);
        }
    }
}
