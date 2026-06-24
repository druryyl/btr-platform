using btr.application.ReportingContext.DashboardInventoryForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryForecastExecutiveSummaryBuilderTest
    {
        [Fact]
        public void Build_WhenUnavailable_ReturnsUnavailableMessage()
        {
            var text = InventoryForecastExecutiveSummaryBuilder.Build(new DashboardInventoryForecastResponse
            {
                IsAvailable = false
            });

            text.Should().Contain("not yet available");
        }

        [Fact]
        public void Build_WithStockOutRisk_IncludesCountsAndValues()
        {
            var text = InventoryForecastExecutiveSummaryBuilder.Build(new DashboardInventoryForecastResponse
            {
                IsAvailable = true,
                ForecastConfidence = InventoryForecastPolicy.ConfidenceMedium,
                StockOutRiskItemCount = 3,
                PlanningHorizonDays = 30,
                UnderstockValue = 5_000_000m,
                CurrentInventoryValue = 100_000_000m,
                ProjectedInventoryValue = 80_000_000m,
                TopRisks =
                {
                    new DashboardInventoryForecastRiskItem { BrgName = "Item A" },
                    new DashboardInventoryForecastRiskItem { BrgName = "Item B" }
                }
            });

            text.Should().Contain("3 active items");
            text.Should().Contain("Item A");
        }
    }
}
