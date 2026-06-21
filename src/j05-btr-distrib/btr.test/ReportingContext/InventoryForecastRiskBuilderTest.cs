using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryForecastRiskBuilderTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);

        [Fact]
        public void BuildTopRisks_CriticalStockOutRanksBeforeOverstock()
        {
            var items = new[]
            {
                Context("BRG001", "Critical Item", 10_000m, 10m, 5m, 5),
                Context("BRG002", "Overstock Item", 50_000m, 500m, 1m, 100),
            };

            var risks = InventoryForecastRiskBuilder.BuildTopRisks(items, 30, 90, BusinessDate);

            risks.Should().NotBeEmpty();
            risks.First().SignalKey.Should().Be(InventoryForecastRiskBuilder.SignalCriticalStockOut);
        }

        [Fact]
        public void BuildPurchaseRecommendations_OrdersByUrgency()
        {
            var items = new[]
            {
                Context("BRG001", "Low Urgency", 10_000m, 100m, 1m, 50),
                Context("BRG002", "Critical", 10_000m, 10m, 10m, 5),
            };

            var recommendations = InventoryForecastRiskBuilder.BuildPurchaseRecommendations(items, BusinessDate);

            recommendations.Should().NotBeEmpty();
            recommendations.First().Urgency.Should().Be(InventoryForecastPolicy.UrgencyCritical);
        }

        private static ForecastItemContext Context(
            string brgId,
            string name,
            decimal inventoryValue,
            decimal qty,
            decimal adc,
            decimal? dos)
        {
            var calc = new InventoryForecastCalculation
            {
                AdcUsed = adc,
                DaysOfSupply = dos,
                PurchaseUrgency = InventoryForecastPolicy.ResolvePurchaseUrgency(dos, BusinessDate.AddDays(3), BusinessDate),
                RecommendedPurchaseQty = 50m,
                ProjectedStockOutDate = dos.HasValue ? BusinessDate.AddDays((int)Math.Ceiling(dos.Value)) : (DateTime?)null,
                ReorderDate = BusinessDate.AddDays(3)
            };

            return new ForecastItemContext
            {
                Item = new DashboardInventoryItemGroup
                {
                    BrgId = brgId,
                    BrgCode = brgId,
                    BrgName = name,
                    Qty = qty,
                    InventoryValue = inventoryValue,
                    SupplierName = "Sup"
                },
                Calculation = calc,
                MovementSignalKey = DashboardInventoryRiskAggregator.BucketActive,
                IsForecastEligible = true
            };
        }
    }
}
