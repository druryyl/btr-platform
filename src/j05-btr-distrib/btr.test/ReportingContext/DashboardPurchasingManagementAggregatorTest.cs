using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPurchasingManagementKeyResolverTest
    {
        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData("", "Unknown")]
        [InlineData("  ", "Unknown")]
        [InlineData("  Acme  ", "Acme")]
        public void ResolvePrincipalName_NormalizesBlankToUnknown(string input, string expected)
        {
            DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(input).Should().Be(expected);
        }

        [Fact]
        public void NamesMatch_IsCaseInsensitiveAndTrimAware()
        {
            DashboardPurchasingManagementKeyResolver.NamesMatch(" Acme ", "acme").Should().BeTrue();
            DashboardPurchasingManagementKeyResolver.NamesMatch("", "Unknown").Should().BeTrue();
        }
    }

    public class DashboardPurchasingManagementAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 20);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 20, 10, 0, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardPurchasingManagementAggregator _aggregator =
            new DashboardPurchasingManagementAggregator();

        [Fact]
        public void Aggregate_FreshBelum_ExcludedFromQualifiedBacklog()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "BELUM", FixedToday.AddDays(-1)) },
                qualifiedBacklogDays: 3);

            result.QualifiedBacklogCount.Should().Be(0);
            result.AttentionList.Should().NotContain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalQualifiedBacklog);
        }

        [Fact]
        public void Aggregate_StaleBelum_IncludedInQualifiedBacklog()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "BELUM", FixedToday.AddDays(-4)) },
                qualifiedBacklogDays: 3);

            result.QualifiedBacklogCount.Should().Be(1);
            result.QualifiedBacklogValue.Should().Be(1_000m);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalQualifiedBacklog);
        }

        [Fact]
        public void Aggregate_QualifiedBacklogAgeBoundary_ExactlyThreeDaysIncluded()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "BELUM", FixedToday.AddDays(-3)) },
                qualifiedBacklogDays: 3);

            result.QualifiedBacklogCount.Should().Be(1);
        }

        [Fact]
        public void Aggregate_SudahInvoice_ExcludedFromQualifiedBacklog()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "SUDAH", FixedToday.AddDays(-10)) },
                qualifiedBacklogDays: 3);

            result.QualifiedBacklogCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_Top10PurchasePercent_ComputesTop1AndTop3()
        {
            var result = Aggregate(new[]
            {
                Invoice("A", 600m, "SUDAH", FixedToday, "Principal A"),
                Invoice("B", 300m, "SUDAH", FixedToday, "Principal B"),
                Invoice("C", 100m, "SUDAH", FixedToday, "Principal C")
            });

            result.Top1PrincipalPercent.Should().Be(60m);
            result.Top3PrincipalPercent.Should().Be(100m);
            result.TopPrincipal.Should().HaveCount(3);
            result.TopPrincipal[0].PercentOfPurchase.Should().Be(60m);
        }

        [Fact]
        public void Aggregate_PrincipalSpendConcentration_EmitsForTop10()
        {
            var rows = Enumerable.Range(1, 10)
                .Select(i => Invoice($"INV-{i}", 100m * i, "SUDAH", FixedToday, $"Principal {i}"))
                .ToArray();

            var result = Aggregate(invoices: rows);

            result.AttentionList.Count(a =>
                    a.SignalKey == DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration)
                .Should().Be(10);
        }

        [Fact]
        public void Aggregate_CompoundDependency_WhenPurchaseAndInventoryTop10()
        {
            var inventory = InventorySnapshot(
                ("Principal A", 5_000m, 1));

            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "SUDAH", FixedToday, "Principal A") },
                inventorySnapshot: inventory);

            result.CompoundDependencyCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalCompoundDependency
                && a.EntityName == "Principal A");
        }

        [Fact]
        public void Aggregate_CompoundDependency_NotEmittedWhenPurchaseTop10Only()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "SUDAH", FixedToday, "Principal A") });

            result.CompoundDependencyCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_PrincipalInventoryNoPurchase_EmitsWhenInventoryTop10ZeroPurchase()
        {
            var inventory = InventorySnapshot(
                ("Principal A", 5_000m, 1));

            var result = Aggregate(inventorySnapshot: inventory);

            result.PrincipalInventoryNoPurchaseCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase
                && a.EntityName == "Principal A");
        }

        [Fact]
        public void Aggregate_UnknownPrincipal_EmitsWhenInTop10()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "SUDAH", FixedToday, "") });

            result.UnknownPrincipalCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalUnknownPrincipal);
        }

        [Fact]
        public void Aggregate_PurchasingInactivity_WhenZeroInvoicesAfterDay15()
        {
            var result = Aggregate();

            result.PurchasingInactivityFlag.Should().BeTrue();
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalPurchasingInactivity);
        }

        [Fact]
        public void Aggregate_PurchasingInactivity_NotEmittedEarlyMonth()
        {
            var result = Aggregate(today: new DateTime(2026, 6, 5));

            result.PurchasingInactivityFlag.Should().BeFalse();
        }

        [Fact]
        public void Aggregate_MissingInventorySnapshot_PurchaseSignalsStillEmit()
        {
            var result = Aggregate(
                new[] { Invoice("A", 1_000m, "SUDAH", FixedToday, "Principal A") },
                inventorySnapshot: null,
                inventoryRiskSnapshot: null);

            result.TopPrincipal.Should().ContainSingle();
            result.TopPrincipal[0].InventoryValue.Should().BeNull();
            result.TopPrincipal[0].AtRiskValue.Should().BeNull();
        }

        [Fact]
        public void Aggregate_AttentionSortOrder_QualifiedBacklogBeforeConcentration()
        {
            var inventory = InventorySnapshot(
                ("Principal A", 5_000m, 1));

            var result = Aggregate(
                new[]
                {
                    Invoice("A", 1_000m, "BELUM", FixedToday.AddDays(-4), "Principal A"),
                    Invoice("B", 500m, "SUDAH", FixedToday, "Principal B")
                },
                inventorySnapshot: inventory);

            var qualifiedIndex = result.AttentionList.FindIndex(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalQualifiedBacklog);
            var spendIndex = result.AttentionList.FindIndex(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration);

            qualifiedIndex.Should().BeGreaterOrEqualTo(0);
            spendIndex.Should().BeGreaterOrEqualTo(0);
            qualifiedIndex.Should().BeLessThan(spendIndex);
        }

        [Fact]
        public void Aggregate_OneRowPerPrincipalSignal_NoDuplicates()
        {
            var result = Aggregate(new[]
            {
                Invoice("A", 500m, "BELUM", FixedToday.AddDays(-4), "Principal A"),
                Invoice("B", 500m, "BELUM", FixedToday.AddDays(-5), "Principal A")
            });

            result.AttentionList.Count(a =>
                    a.EntityName == "Principal A"
                    && a.SignalKey == DashboardPurchasingManagementAggregator.SignalQualifiedBacklog)
                .Should().Be(1);
        }

        [Fact]
        public void Aggregate_PrincipalInventoryConcentration_EmitsForInventoryTop10()
        {
            var inventory = InventorySnapshot(
                ("Principal A", 5_000m, 1));

            var result = Aggregate(inventorySnapshot: inventory);

            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration
                && a.EntityName == "Principal A");
        }

        [Fact]
        public void Aggregate_PrincipalAtRiskExposure_EmitsForAtRiskTop10()
        {
            var atRisk = AtRiskSnapshot(
                ("Principal A", 2_000m, 1));

            var result = Aggregate(inventoryRiskSnapshot: atRisk);

            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure
                && a.EntityName == "Principal A");
        }

        private DashboardPurchasingManagementAggregateResult Aggregate(
            InvoiceView[] invoices = null,
            DashboardInventoryAggregateResult inventorySnapshot = null,
            DashboardInventoryRiskAggregateResult inventoryRiskSnapshot = null,
            DateTime? today = null,
            int qualifiedBacklogDays = 3)
        {
            var invoiceList = invoices ?? Array.Empty<InvoiceView>();
            var purchasingSnapshot = new DashboardPurchasingAggregateResult
            {
                GrandTotalPurchase = invoiceList.Sum(i => i.GrandTotal),
                TotalInvoice = invoiceList.Length,
                PostingStatus = new List<DashboardPurchasingPostingStatusRow>
                {
                    new DashboardPurchasingPostingStatusRow
                    {
                        StatusKey = "BELUM",
                        PurchaseAmount = invoiceList.Where(i => i.PostingStok == "BELUM").Sum(i => i.GrandTotal)
                    },
                    new DashboardPurchasingPostingStatusRow
                    {
                        StatusKey = "SUDAH",
                        PurchaseAmount = invoiceList.Where(i => i.PostingStok == "SUDAH").Sum(i => i.GrandTotal)
                    }
                }
            };

            return _aggregator.Aggregate(
                invoiceList,
                purchasingSnapshot,
                inventorySnapshot,
                inventoryRiskSnapshot,
                June2026,
                today ?? FixedToday,
                FixedGeneratedAt,
                qualifiedBacklogDays);
        }

        private static InvoiceView Invoice(
            string code,
            decimal amount,
            string postingStok,
            DateTime lastUpdate,
            string supplierName = "Principal A")
        {
            return new InvoiceView
            {
                InvoiceCode = code,
                Tgl = lastUpdate,
                SupplierName = supplierName,
                GrandTotal = amount,
                PostingStok = postingStok,
                CreateTime = lastUpdate,
                LastUpdate = lastUpdate
            };
        }

        private static DashboardInventoryAggregateResult InventorySnapshot(
            params (string Name, decimal Value, int Rank)[] suppliers)
        {
            return new DashboardInventoryAggregateResult
            {
                TotalInventoryValue = suppliers.Sum(s => s.Value),
                Breakdown = suppliers.Select(s => new DashboardInventoryBreakdownRow
                {
                    DimensionType = DashboardInventoryAggregator.DimensionSupplier,
                    Name = s.Name,
                    InventoryValue = s.Value,
                    IsTop10 = true,
                    Top10Rank = s.Rank
                }).ToList()
            };
        }

        private static DashboardInventoryRiskAggregateResult AtRiskSnapshot(
            params (string Name, decimal Value, int Rank)[] suppliers)
        {
            var totalAtRisk = suppliers.Sum(s => s.Value);
            return new DashboardInventoryRiskAggregateResult
            {
                AtRiskInventoryValue = totalAtRisk,
                Breakdown = suppliers.Select(s => new DashboardInventoryRiskBreakdownRow
                {
                    DimensionType = DashboardInventoryRiskAggregator.DimensionSupplier,
                    Name = s.Name,
                    AtRiskValue = s.Value,
                    Rank = s.Rank,
                    PercentOfAtRisk = totalAtRisk > 0
                        ? Math.Round(s.Value / totalAtRisk * 100m, 4)
                        : (decimal?)null
                }).ToList()
            };
        }
    }
}
