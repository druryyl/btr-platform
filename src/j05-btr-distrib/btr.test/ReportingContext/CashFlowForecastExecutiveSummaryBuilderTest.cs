using System;
using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Queries;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CashFlowForecastExecutiveSummaryBuilderTest
    {
        [Fact]
        public void Build_NoBilling_ReturnsUnavailableMessage()
        {
            var response = new DashboardCashFlowForecastResponse { MonthFakturOmzet = 0 };

            CashFlowForecastExecutiveSummaryBuilder.Build(response)
                .Should().Be("No billing recorded this month — collection forecast unavailable.");
        }

        [Fact]
        public void Build_LowConfidence_IncludesEarlyMonthPrefix()
        {
            var response = BaseResponse();
            response.ForecastConfidence = CashFlowForecastPolicy.ConfidenceLow;
            response.CollectionForecastPercent = 95m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Warning;

            CashFlowForecastExecutiveSummaryBuilder.Build(response)
                .Should().StartWith("Early-month forecast");
        }

        [Fact]
        public void Build_HealthyForecast_MentionsPaceWithBilling()
        {
            var response = BaseResponse();
            response.CollectionForecastPercent = 105m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Healthy;

            CashFlowForecastExecutiveSummaryBuilder.Build(response)
                .Should().Contain("keep pace with billing");
        }

        [Fact]
        public void Build_WarningBand_MentionsIncreasedEffort()
        {
            var response = BaseResponse();
            response.CollectionForecastPercent = 85m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Warning;

            CashFlowForecastExecutiveSummaryBuilder.Build(response)
                .Should().Contain("increased collection effort needed");
        }

        [Fact]
        public void Build_CriticalBand_MentionsLiquidityRisk()
        {
            var response = BaseResponse();
            response.CollectionForecastPercent = 60m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Critical;

            CashFlowForecastExecutiveSummaryBuilder.Build(response)
                .Should().Contain("Liquidity risk");
        }

        private static DashboardCashFlowForecastResponse BaseResponse() =>
            new DashboardCashFlowForecastResponse
            {
                MonthFakturOmzet = 10_000_000m,
                ExpectedCashCollection = 6_000_000m,
                ProjectedMonthEndTotalCollections = 7_000_000m,
                ForecastConfidence = CashFlowForecastPolicy.ConfidenceMedium
            };
    }
}
