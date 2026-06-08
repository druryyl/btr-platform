using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
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
            input.Piutang.AgingBuckets = new List<DashboardPiutangAgingBucket>();

            var result = Compose(input);

            result.Piutang.RequiresAttention.Should().BeTrue();
        }

        [Fact]
        public void Compose_CalculatesAgingOver90Percent()
        {
            var input = FullInput();
            input.Piutang.TotalPiutang = 1000m;
            input.Piutang.AgingBuckets = new List<DashboardPiutangAgingBucket>
            {
                new DashboardPiutangAgingBucket { BucketKey = "DaysOver90", Amount = 250m }
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
            input.Piutang.TopCustomers = new List<DashboardPiutangTopCustomer>
            {
                new DashboardPiutangTopCustomer { Rank = 1, CustomerName = "A", OutstandingBalance = 400m }
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
        public void Compose_TruncatesCriticalExposuresToTop5()
        {
            var input = FullInput();
            input.Piutang.TopCustomers = Enumerable.Range(1, 10)
                .Select(i => new DashboardPiutangTopCustomer
                {
                    Rank = i,
                    CustomerName = $"Customer {i}",
                    OutstandingBalance = 1000m - i
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
                    AgingBuckets = new List<DashboardPiutangAgingBucket>(),
                    TopCustomers = new List<DashboardPiutangTopCustomer>()
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

        private btr.application.ReportingContext.DashboardExecutiveAgg.Queries.DashboardExecutiveResponse Compose(
            ExecutiveComposeInput input)
        {
            return _composer.Compose(input);
        }
    }
}
