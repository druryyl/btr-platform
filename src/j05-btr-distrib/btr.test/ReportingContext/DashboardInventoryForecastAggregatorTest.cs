using System;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryForecastAggregatorTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 6, 15, 10, 0, 0);

        private readonly DashboardInventoryForecastAggregator _forecastAggregator = new DashboardInventoryForecastAggregator();
        private readonly DashboardInventoryRiskAggregator _riskAggregator = new DashboardInventoryRiskAggregator();
        private readonly DashboardInventoryAggregator _inventoryAggregator = new DashboardInventoryAggregator();

        [Fact]
        public void Aggregate_IFR50_CurrentInventoryValueMatchesInventoryAggregator()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Fast Mover", 100, 10_000m),
                Row("BRG002", "G002", "Slow Mover", 50, 5_000m),
            };
            var lastFaktur = new[]
            {
                LastFaktur("BRG001", BusinessDate.AddDays(-5)),
                LastFaktur("BRG002", BusinessDate.AddDays(-5)),
            };
            var consumption = new[]
            {
                Consumption("BRG001", 300m, 900m),
                Consumption("BRG002", 30m, 90m),
            };

            var inventory = _inventoryAggregator.Aggregate(rows, GeneratedAt);
            var risk = _riskAggregator.Aggregate(rows, lastFaktur, BusinessDate, GeneratedAt);
            var forecast = Aggregate(rows, lastFaktur, consumption, risk);

            forecast.CurrentInventoryValue.Should().Be(inventory.TotalInventoryValue);
        }

        [Fact]
        public void Aggregate_IFR51_AtRiskPercentMatchesRiskAggregator()
        {
            var rows = new[] { Row("BRG001", "G001", "Never Sold", 10, 1_000m) };
            var risk = _riskAggregator.Aggregate(rows, Enumerable.Empty<BrgLastFakturDto>(), BusinessDate, GeneratedAt);
            var forecast = Aggregate(rows, Array.Empty<BrgLastFakturDto>(), Array.Empty<BrgConsumptionDto>(), risk);

            forecast.AtRiskInventoryPercent.Should().Be(risk.AtRiskInventoryPercent);
        }

        [Fact]
        public void Aggregate_ExcludesNeverSoldAndDeadStockFromStockOutCount()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Never", 10, 1_000m),
                Row("BRG002", "G002", "Dead", 10, 1_000m),
                Row("BRG003", "G003", "Active Fast", 30, 1_000m),
            };
            var lastFaktur = new[]
            {
                LastFaktur("BRG002", BusinessDate.AddDays(-200)),
                LastFaktur("BRG003", BusinessDate.AddDays(-5)),
            };
            var consumption = new[] { Consumption("BRG003", 900m, 900m) };
            var risk = _riskAggregator.Aggregate(rows, lastFaktur, BusinessDate, GeneratedAt);
            var forecast = Aggregate(rows, lastFaktur, consumption, risk);

            forecast.StockOutRiskItemCount.Should().Be(1);
            forecast.TopRisks.Should().NotBeEmpty();
            forecast.TopRisks.Should().OnlyContain(r => r.BrgId == "BRG003");
        }

        [Fact]
        public void Aggregate_TopRisksAndRecommendations_CappedAtTen()
        {
            var rows = Enumerable.Range(1, 15)
                .Select(i => Row($"BRG{i:D3}", $"G{i:D3}", $"Item {i}", 100, 1_000m))
                .ToArray();
            var lastFaktur = rows.Select(r => LastFaktur(r.BrgId, BusinessDate.AddDays(-5))).ToArray();
            var consumption = rows.Select(r => Consumption(r.BrgId, 3000m, 9000m)).ToArray();
            var risk = _riskAggregator.Aggregate(rows, lastFaktur, BusinessDate, GeneratedAt);
            var forecast = Aggregate(rows, lastFaktur, consumption, risk);

            forecast.TopRisks.Should().HaveCountLessOrEqualTo(10);
            forecast.PurchaseRecommendations.Should().HaveCountLessOrEqualTo(10);
        }

        private btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardInventoryForecastAggregateResult Aggregate(
            StokBalanceView[] rows,
            BrgLastFakturDto[] lastFaktur,
            BrgConsumptionDto[] consumption,
            btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardInventoryRiskAggregateResult risk)
        {
            return _forecastAggregator.Aggregate(
                rows,
                lastFaktur,
                consumption,
                Array.Empty<DailyCompanyConsumptionDto>(),
                risk,
                BusinessDate,
                GeneratedAt,
                planningHorizonDays: 30,
                defaultLeadTimeDays: 7,
                coverageDays: 14,
                overstockDosDays: 90,
                minDosHealthy: 30);
        }

        private static BrgConsumptionDto Consumption(string brgId, decimal sold30, decimal sold90) =>
            new BrgConsumptionDto
            {
                BrgId = brgId,
                SoldQty30 = sold30,
                SoldQty90 = sold90,
                IsAktif = true,
                FirstFakturDate = BusinessDate.AddDays(-60)
            };

        private static BrgLastFakturDto LastFaktur(string brgId, DateTime date) =>
            new BrgLastFakturDto { BrgId = brgId, BrgCode = brgId, BrgName = brgId, LastFakturDate = date };

        private static StokBalanceView Row(string brgId, string brgCode, string brgName, int qty, decimal hpp) =>
            new StokBalanceView
            {
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                WarehouseName = "Gudang Utama",
                Qty = qty,
                Hpp = hpp,
                KategoriName = "Cat",
                SupplierName = "Sup"
            };
    }
}
