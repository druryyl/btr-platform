using System;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalInventoryAggregatorTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 9, 9);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalInventoryAggregator _aggregator = new PrincipalInventoryAggregator();

        [Fact]
        public void Aggregate_MapsInventoryValueFromInventorySnapshotEvidence()
        {
            var result = Aggregate(
                new[]
                {
                    Stock("SUPA", "Alpha", "BRG1", "Gudang", 4, 10m),
                    Stock("SUPA", "Alpha", "BRG1", "Gudang", 6, 10m),
                    Stock("SUPB", "Beta", "BRG2", "Gudang", 2, 25m)
                },
                new[]
                {
                    LastFaktur("BRG1", BusinessDate.AddDays(-2)),
                    LastFaktur("BRG2", BusinessDate.AddDays(-2))
                },
                new[]
                {
                    Consumption("BRG1", 30m),
                    Consumption("BRG2", 30m)
                });

            result.InventoryValueKpiId.Should().Be(PrincipalKpiCatalog.InventoryValueId);
            result.InventoryDaysKpiId.Should().Be(PrincipalKpiCatalog.InventoryDaysId);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.InventoryValue.Should().Be(100m);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPB")
                .Which.InventoryValue.Should().Be(50m);
            result.Principals.Should().OnlyContain(row =>
                row.InventoryValueKpiId == PrincipalKpiCatalog.InventoryValueId &&
                row.InventoryDaysKpiId == PrincipalKpiCatalog.InventoryDaysId);
        }

        [Fact]
        public void Aggregate_UsesExistingAverageDaysOfSupply_AndExcludesNeverSoldAndDeadStock()
        {
            var result = Aggregate(
                new[]
                {
                    Stock("SUPA", "Alpha", "ELIG", "Gudang", 30, 10m),
                    Stock("SUPA", "Alpha", "NEVER", "Gudang", 10, 5m),
                    Stock("SUPA", "Alpha", "DEAD", "Gudang", 8, 4m)
                },
                new[]
                {
                    LastFaktur("ELIG", BusinessDate.AddDays(-2)),
                    LastFaktur("DEAD", BusinessDate.AddDays(-DashboardInventoryRiskAggregator.DeadStockDaysThreshold))
                },
                new[]
                {
                    Consumption("ELIG", 30m),
                    Consumption("NEVER", 30m),
                    Consumption("DEAD", 30m)
                });

            var principal = result.Principals.Should().ContainSingle().Subject;
            principal.InventoryValue.Should().Be(30m * 10m + 10m * 5m + 8m * 4m);
            principal.InventoryDays.Should().Be(30m);
            principal.ItemCount.Should().Be(3);
        }

        [Fact]
        public void Aggregate_InventoryDaysIsNullWhenCoverageAdcIsNotPositive()
        {
            var result = Aggregate(
                new[] { Stock("SUPA", "Alpha", "BRG1", "Gudang", 12, 10m) },
                new[] { LastFaktur("BRG1", BusinessDate.AddDays(-2)) },
                new[] { Consumption("BRG1", 0m) });

            result.Principals.Should().ContainSingle().Which.InventoryValue.Should().Be(120m);
            result.Principals.Should().ContainSingle().Which.InventoryDays.Should().BeNull();
        }

        [Fact]
        public void Aggregate_SkipsInTransitZeroQtyAndBlankPrincipal()
        {
            var result = Aggregate(
                new[]
                {
                    Stock("SUPA", "Alpha", "BRG1", "Gudang", 5, 10m),
                    Stock("SUPA", "Alpha", "BRG2", DashboardInventoryItemGroupBuilder.InTransitWarehouseName, 9, 10m),
                    Stock("SUPA", "Alpha", "BRG3", "Gudang", 0, 10m),
                    Stock("   ", "", "BRG4", "Gudang", 7, 10m)
                },
                new[] { LastFaktur("BRG1", BusinessDate.AddDays(-2)) },
                new[] { Consumption("BRG1", 30m) });

            var principal = result.Principals.Should().ContainSingle().Subject;
            principal.SupplierId.Should().Be("SUPA");
            principal.InventoryValue.Should().Be(50m);
            principal.ItemCount.Should().Be(1);
            result.Principals.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
        }

        [Fact]
        public void Aggregate_DoesNotWriteSalesOutOrChangeInventoryViews()
        {
            var result = Aggregate(
                new[] { Stock("SUPA", "Alpha", "BRG1", "Gudang", 5, 10m) },
                new[] { LastFaktur("BRG1", BusinessDate.AddDays(-2)) },
                new[] { Consumption("BRG1", 30m) });

            result.InventoryValueKpiId.Should().Be("PRN-INV-001");
            result.InventoryDaysKpiId.Should().Be("PRN-INV-002");

            var resultProperties = typeof(PrincipalInventoryAggregateResult)
                .GetProperties()
                .Select(property => property.Name);
            resultProperties.Should().NotContain(name => name.IndexOf("SalesOut", StringComparison.OrdinalIgnoreCase) >= 0);

            var writerSql = string.Join(
                " ",
                PrincipalInventorySnapshotDal.WrittenTables,
                PrincipalInventorySnapshotDal.DeletePrincipalSql,
                PrincipalInventorySnapshotDal.MergeKpiSql,
                PrincipalInventorySnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalInventory");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("BTRPD_InventoryKpi");
            writerSql.Should().NotContain("BTRPD_InventoryBreakdown");
            writerSql.Should().NotContain("PRN-SALES-001");
        }

        private PrincipalInventoryAggregateResult Aggregate(
            StokBalanceView[] stock,
            BrgLastFakturDto[] lastFaktur,
            BrgConsumptionDto[] consumption)
        {
            return _aggregator.Aggregate(
                stock,
                lastFaktur,
                consumption,
                BusinessDate,
                GeneratedAt,
                planningHorizonDays: 30,
                defaultLeadTimeDays: 7,
                coverageDays: 14);
        }

        private static StokBalanceView Stock(
            string supplierId,
            string supplierName,
            string brgId,
            string warehouseName,
            int qty,
            decimal hpp)
        {
            return new StokBalanceView
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                BrgId = brgId,
                WarehouseName = warehouseName,
                Qty = qty,
                Hpp = hpp
            };
        }

        private static BrgLastFakturDto LastFaktur(string brgId, DateTime lastFakturDate)
        {
            return new BrgLastFakturDto
            {
                BrgId = brgId,
                LastFakturDate = lastFakturDate
            };
        }

        private static BrgConsumptionDto Consumption(string brgId, decimal soldQty30)
        {
            return new BrgConsumptionDto
            {
                BrgId = brgId,
                SoldQty30 = soldQty30,
                SoldQty90 = soldQty30,
                IsAktif = true,
                FirstFakturDate = BusinessDate.AddDays(-60)
            };
        }
    }
}
