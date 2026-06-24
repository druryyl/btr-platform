using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CashFlowForecastPolicyTest
    {
        private static readonly DateTime MonthStart = new DateTime(2026, 6, 1);
        private static readonly DateTime MonthEnd = new DateTime(2026, 6, 30);

        [Fact]
        public void Compute_MidMonth_CalculatesCashAndCollectionForecast()
        {
            var businessDate = new DateTime(2026, 6, 15);
            var result = CashFlowForecastPolicy.Compute(
                cashCollectedMtd: 3_000_000m,
                monthCollections: 3_500_000m,
                monthFakturOmzet: 10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.DaysElapsed.Should().Be(15);
            result.DaysRemaining.Should().Be(15);
            result.DailyCashCollectionAverage.Should().Be(200_000m);
            result.DailyCollectionAverage.Should().Be(233_333.33m);
            result.ExpectedCashCollection.Should().Be(6_000_000m);
            result.ProjectedMonthEndTotalCollections.Should().Be(7_000_000m);
            result.CollectionForecastPercent.Should().Be(70.0m);
            result.RemainingCollectionTarget.Should().Be(6_500_000m);
            result.RequiredDailyCollection.Should().Be(433_333.33m);
            result.CollectionGap.Should().Be(3_000_000m);
            result.ForecastVarianceCash.Should().Be(3_000_000m);
            result.RecoveryVsBillingPercent.Should().Be(35.0m);
            result.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Critical);
        }

        [Fact]
        public void Compute_Day1WithZeroCash_ForecastIsZero_ConfidenceLow()
        {
            var businessDate = new DateTime(2026, 6, 1);
            var result = CashFlowForecastPolicy.Compute(
                0m,
                0m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.ExpectedCashCollection.Should().Be(0m);
            result.DailyCashCollectionAverage.Should().Be(0m);
            result.ForecastConfidence.Should().Be(CashFlowForecastPolicy.ConfidenceLow);
        }

        [Fact]
        public void Compute_BillingZero_CollectionForecastPercentNull()
        {
            var businessDate = new DateTime(2026, 6, 10);
            var result = CashFlowForecastPolicy.Compute(
                1_000_000m,
                1_200_000m,
                0m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.CollectionForecastPercent.Should().BeNull();
            result.RequiredDailyCollection.Should().BeNull();
            result.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Unknown);
        }

        [Fact]
        public void Compute_CollectionsExceedBilling_RemainingTargetZero_RequiredDailyZero()
        {
            var businessDate = new DateTime(2026, 6, 20);
            var result = CashFlowForecastPolicy.Compute(
                12_000_000m,
                12_000_000m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.RemainingCollectionTarget.Should().Be(0m);
            result.RequiredDailyCollection.Should().Be(0m);
            result.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Healthy);
        }

        [Fact]
        public void Compute_LastDayOfMonth_ForecastEqualsActual()
        {
            var businessDate = MonthEnd;
            var result = CashFlowForecastPolicy.Compute(
                8_000_000m,
                9_000_000m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.ExpectedCashCollection.Should().Be(8_000_000m);
            result.ProjectedMonthEndTotalCollections.Should().Be(9_000_000m);
            result.DaysRemaining.Should().Be(0);
            result.RequiredDailyCollection.Should().Be(0m);
        }

        [Fact]
        public void Compute_DaysElapsedGuard_NoDivideByZeroOnMonthStart()
        {
            var businessDate = new DateTime(2026, 6, 1);
            var result = CashFlowForecastPolicy.Compute(
                100m,
                100m,
                1_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.DaysElapsed.Should().Be(1);
            result.DailyCashCollectionAverage.Should().Be(100m);
        }

        [Fact]
        public void ComputeBestCaseAndWorstCase_UseMaxMinOfPace()
        {
            CashFlowForecastPolicy.ComputeBestCaseCash(200_000m, 300_000m, 30)
                .Should().Be(9_000_000m);
            CashFlowForecastPolicy.ComputeWorstCaseCash(200_000m, 300_000m, 30)
                .Should().Be(6_000_000m);
        }

        [Theory]
        [InlineData(3, CashFlowForecastPolicy.ConfidenceLow)]
        [InlineData(5, CashFlowForecastPolicy.ConfidenceLow)]
        [InlineData(6, CashFlowForecastPolicy.ConfidenceMedium)]
        [InlineData(20, CashFlowForecastPolicy.ConfidenceMedium)]
        [InlineData(21, CashFlowForecastPolicy.ConfidenceHigh)]
        public void ResolveConfidence_FollowsDayThresholds(int daysElapsed, string expected)
        {
            CashFlowForecastPolicy.ResolveConfidence(daysElapsed, 30).Should().Be(expected);
        }

        [Theory]
        [InlineData(300_000, 100_000, CashFlowForecastPolicy.RequiredDailySeverityCritical)]
        [InlineData(160_000, 100_000, CashFlowForecastPolicy.RequiredDailySeverityWarning)]
        [InlineData(100_000, 100_000, CashFlowForecastPolicy.RequiredDailySeverityNormal)]
        public void ResolveRequiredDailySeverity_FollowsRatioThresholds(
            decimal required,
            decimal dailyAvg,
            string expected)
        {
            CashFlowForecastPolicy.ResolveRequiredDailySeverity(required, dailyAvg).Should().Be(expected);
        }
    }
}
