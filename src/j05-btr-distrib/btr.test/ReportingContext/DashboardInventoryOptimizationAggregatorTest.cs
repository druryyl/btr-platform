using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.domain.InventoryContext.WarehouseAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryOptimizationAggregatorTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);

        [Fact]
        public void Aggregate_IO50_HealthScoreMatchesForecast()
        {
            var forecastResult = new DashboardInventoryForecastAggregateResult
            {
                InventoryHealthScore = 72,
                PlanningHorizonDays = 30
            };

            var item = BuildPurchaseItem("BRG001", "Active Item", 50m, 10m, 5m);
            var result = RunAggregate(new[] { item }, forecastResult);

            result.InventoryHealthScore.Should().Be(72);
        }

        [Fact]
        public void Aggregate_IO52_DeadStockExcludedFromReorderList()
        {
            var dead = new ForecastItemContext
            {
                Item = new DashboardInventoryItemGroup
                {
                    BrgId = "DEAD1",
                    BrgName = "Dead Item",
                    Qty = 100m,
                    InventoryValue = 1_000_000m,
                    SupplierName = "Sup A"
                },
                Calculation = InventoryForecastPolicy.ComputeItem(
                    100m, 10_000m, 0m, 0m, 30, 7, 14, BusinessDate),
                MovementSignalKey = DashboardInventoryRiskAggregator.SignalDeadStock,
                IsForecastEligible = false
            };

            var result = RunAggregate(new[] { dead }, new DashboardInventoryForecastAggregateResult());

            result.ReorderList.Should().BeEmpty();
            result.TopActions.Should().Contain(a =>
                a.ActionType == InventoryOptimizationPolicy.ActionDoNotReorder);
        }

        [Fact]
        public void Aggregate_CapsTopActionsAtConfiguredMax()
        {
            var items = Enumerable.Range(1, 30)
                .Select(i => BuildPurchaseItem($"BRG{i:D3}", $"Item {i}", 20m, 10m, 5m))
                .ToList();

            var options = new DashboardSnapshotOptions { InventoryOptimizationMaxTopActions = 25 };
            var result = RunAggregate(items, new DashboardInventoryForecastAggregateResult(), options);

            result.TopActions.Should().HaveCountLessOrEqualTo(25);
        }

        private static DashboardInventoryOptimizationAggregateResult RunAggregate(
            IEnumerable<ForecastItemContext> items,
            DashboardInventoryForecastAggregateResult forecast,
            DashboardSnapshotOptions options = null)
        {
            var aggregator = new DashboardInventoryOptimizationAggregator();
            return aggregator.Aggregate(
                items,
                new List<StokBalanceView>(),
                new List<btr.application.SalesContext.FakturInfo.BrgWarehouseConsumptionDto>(),
                new List<WarehouseModel>(),
                new DashboardInventoryRiskAggregateResult(),
                new DashboardPurchasingManagementAggregateResult(),
                forecast,
                BusinessDate,
                BusinessDate,
                options ?? new DashboardSnapshotOptions());
        }

        private static ForecastItemContext BuildPurchaseItem(
            string brgId,
            string name,
            decimal qty,
            decimal adc,
            decimal? dos)
        {
            var soldQty30 = adc * 30m;
            var calc = InventoryForecastPolicy.ComputeItem(
                qty, 10_000m, soldQty30, soldQty30 * 3, 30, 7, 14, BusinessDate);

            return new ForecastItemContext
            {
                Item = new DashboardInventoryItemGroup
                {
                    BrgId = brgId,
                    BrgName = name,
                    Qty = qty,
                    InventoryValue = qty * 10_000m,
                    SupplierName = "Supplier A"
                },
                Calculation = calc,
                MovementSignalKey = DashboardInventoryRiskAggregator.BucketActive,
                IsForecastEligible = true
            };
        }
    }
}
