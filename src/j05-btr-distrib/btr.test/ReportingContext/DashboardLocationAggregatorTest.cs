using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardLocationKeyResolverTest
    {
        [Fact]
        public void ResolveWarehouseGroupKey_UsesTrimmedName()
        {
            DashboardLocationKeyResolver.ResolveWarehouseGroupKey("  Main  ", "W1")
                .Should().Be("Main");
        }

        [Fact]
        public void IsInTransitWarehouse_IsCaseSensitive()
        {
            DashboardLocationKeyResolver.IsInTransitWarehouse("In-Transit").Should().BeTrue();
            DashboardLocationKeyResolver.IsInTransitWarehouse("in-transit").Should().BeFalse();
        }

        [Fact]
        public void IsRankingEligible_ExcludesInactiveSpecialAndInTransit()
        {
            DashboardLocationKeyResolver.IsRankingEligible(new WarehouseModel
            {
                WarehouseId = "W1",
                WarehouseName = "Main",
                IsAktif = true,
                IsSpecial = false
            }).Should().BeTrue();

            DashboardLocationKeyResolver.IsRankingEligible(new WarehouseModel
            {
                WarehouseId = "W2",
                WarehouseName = "In-Transit",
                IsAktif = true,
                IsSpecial = false
            }).Should().BeFalse();

            DashboardLocationKeyResolver.IsRankingEligible(new WarehouseModel
            {
                WarehouseId = "W3",
                WarehouseName = "Special",
                IsAktif = true,
                IsSpecial = true
            }).Should().BeFalse();
        }

        [Fact]
        public void ResolveWilayahName_BlankBecomesUnknown()
        {
            DashboardLocationKeyResolver.ResolveWilayahName("")
                .Should().Be(DashboardInventoryItemGroupBuilder.UnknownLabel);
        }
    }

    public class DashboardInventoryRiskClassifierTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);

        [Fact]
        public void BuildAtRiskBrgIdSet_MatchesInventoryRiskAggregatorNeverSoldAndSlowMoving()
        {
            var itemGroups = new List<DashboardInventoryItemGroup>
            {
                new DashboardInventoryItemGroup { BrgId = "B1", InventoryValue = 100m },
                new DashboardInventoryItemGroup { BrgId = "B2", InventoryValue = 200m },
                new DashboardInventoryItemGroup { BrgId = "B3", InventoryValue = 300m },
            };

            var lastFakturRows = new[]
            {
                new BrgLastFakturDto { BrgId = "B2", LastFakturDate = FixedToday.AddDays(-100) },
                new BrgLastFakturDto { BrgId = "B3", LastFakturDate = FixedToday.AddDays(-30) },
            };

            var classifierSet = DashboardInventoryRiskClassifier.BuildAtRiskBrgIdSet(
                itemGroups,
                lastFakturRows,
                FixedToday);

            var riskAggregator = new DashboardInventoryRiskAggregator();
            var riskResult = riskAggregator.Aggregate(
                Enumerable.Empty<StokBalanceView>(),
                lastFakturRows,
                FixedToday,
                FixedToday);

            classifierSet.Should().Contain("B1");
            classifierSet.Should().Contain("B2");
            classifierSet.Should().NotContain("B3");
            riskResult.NeverSoldItemCount.Should().Be(1);
            riskResult.SlowMovingItemCount.Should().Be(1);
        }
    }

    public class DashboardLocationAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode FixedPeriode = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardLocationAggregator _aggregator = new DashboardLocationAggregator();

        [Fact]
        public void Aggregate_ExcludesInTransitFromInventoryRollup()
        {
            var result = Aggregate(
                stokRows: new[]
                {
                    Stok("W1", "Main", "B1", 10m, 10),
                    Stok("W2", "In-Transit", "B2", 5m, 5),
                },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 100m });

            result.TopWarehouseInventory.Should().ContainSingle();
            result.TopWarehouseInventory[0].WarehouseName.Should().Be("Main");
            result.TopWarehouseInventory[0].InventoryValue.Should().Be(100m);
        }

        [Fact]
        public void Aggregate_ExcludesSpecialAndInactiveFromTopInventoryRanking()
        {
            var result = Aggregate(
                stokRows: new[]
                {
                    Stok("W1", "Main", "B1", 10m, 10),
                    Stok("W2", "Special", "B2", 10m, 5),
                    Stok("W3", "Closed", "B3", 10m, 8),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "Main", isAktif: true, isSpecial: false),
                    Warehouse("W2", "Special", isAktif: true, isSpecial: true),
                    Warehouse("W3", "Closed", isAktif: false, isSpecial: false),
                },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 230m });

            result.TopWarehouseInventory.Should().ContainSingle();
            result.TopWarehouseInventory[0].WarehouseName.Should().Be("Main");
        }

        [Fact]
        public void Aggregate_InactiveWarehouseWithStock_AddsAttentionRow()
        {
            var result = Aggregate(
                stokRows: new[] { Stok("W3", "Closed", "B1", 10m, 10) },
                warehouses: new[] { Warehouse("W3", "Closed", isAktif: false, isSpecial: false) },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 100m });

            result.InactiveWarehouseWithStockCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardLocationAggregator.SignalWarehouseInactiveWithStock &&
                a.EntityName == "Closed");
        }

        [Fact]
        public void Aggregate_WarehouseNoSalesWithInventory_AddsSignal()
        {
            var result = Aggregate(
                stokRows: new[] { Stok("W1", "Main", "B1", 10m, 10) },
                warehouses: new[] { Warehouse("W1", "Main") },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 100m });

            result.WarehouseNoSalesWithInventoryCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardLocationAggregator.SignalWarehouseNoSalesWithInventory);
        }

        [Fact]
        public void Aggregate_SumWarehouseOmzet_EqualsSalesSnapshotTotal()
        {
            var result = Aggregate(
                fakturRows: new[]
                {
                    Faktur("Main", "North", 300m),
                    Faktur("East", "South", 200m),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "Main"),
                    Warehouse("W2", "East"),
                },
                salesSnapshot: new DashboardSalesAggregateResult { TotalOmzet = 500m });

            result.TotalOmzet.Should().Be(500m);
            result.TopWarehouseSales.Sum(x => x.MtdOmzet).Should().Be(500m);
        }

        [Fact]
        public void Aggregate_WarehouseInventoryConcentration_EmitsForTop10()
        {
            var warehouses = Enumerable.Range(1, 10)
                .Select(i => Warehouse($"W{i}", $"Wh{i}"))
                .ToArray();
            var stokRows = Enumerable.Range(1, 10)
                .Select(i => Stok($"W{i}", $"Wh{i}", $"B{i}", 100m * i, 1))
                .ToArray();

            var result = Aggregate(
                stokRows: stokRows,
                warehouses: warehouses,
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 5_500m });

            result.AttentionList.Count(a =>
                    a.SignalKey == DashboardLocationAggregator.SignalWarehouseInventoryConcentration)
                .Should().Be(10);
        }

        [Fact]
        public void Aggregate_WarehouseAtRiskConcentration_AllocatesAtRiskBrgIdToWarehouse()
        {
            var result = Aggregate(
                stokRows: new[] { Stok("W1", "Main", "B1", 50m, 10) },
                warehouses: new[] { Warehouse("W1", "Main") },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 500m },
                inventoryRiskSnapshot: new DashboardInventoryRiskAggregateResult { AtRiskInventoryValue = 500m });

            result.TopWarehouseAtRisk.Should().ContainSingle();
            result.TopWarehouseAtRisk[0].WarehouseName.Should().Be("Main");
            result.TopWarehouseAtRisk[0].AtRiskValue.Should().Be(500m);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardLocationAggregator.SignalWarehouseAtRiskConcentration &&
                a.EntityName == "Main");
        }

        [Fact]
        public void Aggregate_SumWarehousePurchase_EqualsPurchasingSnapshotTotal()
        {
            var result = Aggregate(
                invoiceRows: new[]
                {
                    Invoice("Main", 300m),
                    Invoice("East", 200m),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "Main"),
                    Warehouse("W2", "East"),
                },
                purchasingSnapshot: new DashboardPurchasingAggregateResult { GrandTotalPurchase = 500m });

            result.TotalPurchase.Should().Be(500m);
            result.TopWarehousePurchasing.Sum(x => x.MtdPurchaseAmount).Should().Be(500m);
        }

        [Fact]
        public void Aggregate_SumWarehouseInventory_FallbackEqualsBucketSum()
        {
            var result = Aggregate(
                stokRows: new[]
                {
                    Stok("W1", "Main", "B1", 10m, 10),
                    Stok("W2", "East", "B2", 5m, 20),
                    Stok("W3", "In-Transit", "B3", 100m, 1),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "Main"),
                    Warehouse("W2", "East"),
                },
                inventorySnapshot: null);

            result.TotalInventoryValue.Should().Be(200m);
        }

        [Fact]
        public void Aggregate_WilayahOmzet_GroupsByWilayahName()
        {
            var result = Aggregate(
                fakturRows: new[]
                {
                    Faktur("Main", "North", 300m),
                    Faktur("Main", "South", 200m),
                },
                salesSnapshot: new DashboardSalesAggregateResult { TotalOmzet = 500m });

            result.TopWilayahSales.Should().HaveCount(2);
            result.TopWilayahSales.Should().Contain(w => w.WilayahName == "North" && w.MtdOmzet == 300m);
            result.TopWilayahSales.Should().Contain(w => w.WilayahName == "South" && w.MtdOmzet == 200m);
        }

        [Fact]
        public void Aggregate_Top1AndTop3InventoryPercent_ComputesCorrectly()
        {
            var result = Aggregate(
                stokRows: new[]
                {
                    Stok("W1", "A", "B1", 600m, 1),
                    Stok("W2", "B", "B2", 300m, 1),
                    Stok("W3", "C", "B3", 100m, 1),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "A"),
                    Warehouse("W2", "B"),
                    Warehouse("W3", "C"),
                },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 1_000m });

            result.Top1WarehouseInventoryPercent.Should().Be(60m);
            result.Top3WarehouseInventoryPercent.Should().Be(100m);
        }

        [Fact]
        public void Aggregate_AttentionList_NoDuplicateWarehouseSignalPairs()
        {
            var result = Aggregate(
                stokRows: new[] { Stok("W1", "Main", "B1", 100m, 10) },
                warehouses: new[] { Warehouse("W1", "Main") },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 1_000m });

            var pairs = result.AttentionList
                .Select(a => $"{a.EntityName}|{a.SignalKey}")
                .ToList();

            pairs.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void Aggregate_MissingPurchasingSnapshot_StillLoadsRankingsWithFallbackPercent()
        {
            var result = Aggregate(
                invoiceRows: new[]
                {
                    Invoice("Main", 300m),
                    Invoice("East", 200m),
                },
                warehouses: new[]
                {
                    Warehouse("W1", "Main"),
                    Warehouse("W2", "East"),
                },
                purchasingSnapshot: null);

            result.TotalPurchase.Should().Be(500m);
            result.TopWarehousePurchasing.Should().HaveCount(2);
            result.TopWarehousePurchasing.Should().OnlyContain(r => r.PercentOfTotal.HasValue);
        }

        [Fact]
        public void Aggregate_WilayahOmzet_NormalizesBlankToUnknown()
        {
            var result = Aggregate(
                fakturRows: new[] { Faktur("Main", "", 150m) },
                salesSnapshot: new DashboardSalesAggregateResult { TotalOmzet = 150m });

            result.TopWilayahSales.Should().ContainSingle();
            result.TopWilayahSales[0].WilayahName.Should().Be("Unknown");
        }

        [Fact]
        public void Aggregate_AttentionSortOrder_PlacesInactiveBeforeConcentration()
        {
            var result = Aggregate(
                stokRows: new[]
                {
                    Stok("W1", "Main", "B1", 100m, 10),
                    Stok("W3", "Closed", "B2", 50m, 10),
                },
                fakturRows: new[] { Faktur("Main", "North", 1000m) },
                warehouses: new[]
                {
                    Warehouse("W1", "Main"),
                    Warehouse("W3", "Closed", isAktif: false),
                },
                inventorySnapshot: new DashboardInventoryAggregateResult { TotalInventoryValue = 1500m },
                salesSnapshot: new DashboardSalesAggregateResult { TotalOmzet = 1000m });

            var inactiveIndex = result.AttentionList.FindIndex(a =>
                a.SignalKey == DashboardLocationAggregator.SignalWarehouseInactiveWithStock);
            var concentrationIndex = result.AttentionList.FindIndex(a =>
                a.SignalKey == DashboardLocationAggregator.SignalWarehouseSalesConcentration);

            inactiveIndex.Should().BeGreaterOrEqualTo(0);
            concentrationIndex.Should().BeGreaterOrEqualTo(0);
            inactiveIndex.Should().BeLessThan(concentrationIndex);
        }

        private DashboardLocationAggregateResult Aggregate(
            IEnumerable<StokBalanceView> stokRows = null,
            IEnumerable<BrgLastFakturDto> lastFakturRows = null,
            IEnumerable<FakturView> fakturRows = null,
            IEnumerable<InvoiceView> invoiceRows = null,
            IEnumerable<WarehouseModel> warehouses = null,
            DashboardInventoryAggregateResult inventorySnapshot = null,
            DashboardInventoryRiskAggregateResult inventoryRiskSnapshot = null,
            DashboardSalesAggregateResult salesSnapshot = null,
            DashboardPurchasingAggregateResult purchasingSnapshot = null)
        {
            return _aggregator.Aggregate(
                stokRows ?? Enumerable.Empty<StokBalanceView>(),
                lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>(),
                fakturRows ?? Enumerable.Empty<FakturView>(),
                invoiceRows ?? Enumerable.Empty<InvoiceView>(),
                warehouses ?? Enumerable.Empty<WarehouseModel>(),
                inventorySnapshot,
                inventoryRiskSnapshot,
                salesSnapshot,
                purchasingSnapshot,
                FixedPeriode,
                FixedToday,
                FixedGeneratedAt);
        }

        private static StokBalanceView Stok(
            string warehouseId,
            string warehouseName,
            string brgId,
            decimal hpp,
            int qty)
        {
            return new StokBalanceView
            {
                WarehouseId = warehouseId,
                WarehouseName = warehouseName,
                BrgId = brgId,
                Hpp = hpp,
                Qty = qty
            };
        }

        private static WarehouseModel Warehouse(
            string id,
            string name,
            bool isAktif = true,
            bool isSpecial = false)
        {
            return new WarehouseModel
            {
                WarehouseId = id,
                WarehouseName = name,
                IsAktif = isAktif,
                IsSpecial = isSpecial
            };
        }

        private static FakturView Faktur(string warehouseName, string wilayahName, decimal grandTotal)
        {
            return new FakturView
            {
                WarehouseName = warehouseName,
                WilayahName = wilayahName,
                GrandTotal = grandTotal
            };
        }

        private static InvoiceView Invoice(string warehouseName, decimal grandTotal)
        {
            return new InvoiceView
            {
                WarehouseName = warehouseName,
                GrandTotal = grandTotal
            };
        }
    }
}
