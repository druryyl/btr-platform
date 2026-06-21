using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesForecastPolicyTest
    {
        private static readonly DateTime MonthStart = new DateTime(2026, 6, 1);
        private static readonly DateTime MonthEnd = new DateTime(2026, 6, 30);

        [Fact]
        public void Compute_MidMonthWithSalesAndTarget_CalculatesForecastAndRequiredDaily()
        {
            var businessDate = new DateTime(2026, 6, 15);
            var result = SalesForecastPolicy.Compute(
                currentSales: 3_000_000m,
                target: 10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.DaysElapsed.Should().Be(15);
            result.DaysRemaining.Should().Be(15);
            result.DaysInMonth.Should().Be(30);
            result.DailyAverageSales.Should().Be(200_000m);
            result.ForecastSales.Should().Be(6_000_000m);
            result.RequiredDailySales.Should().Be(466_666.67m);
            result.TargetGap.Should().Be(4_000_000m);
            result.ForecastVariance.Should().Be(3_000_000m);
            result.CurrentAchievementPercent.Should().Be(30.0m);
            result.ForecastAchievementPercent.Should().Be(60.0m);
            result.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Critical);
        }

        [Fact]
        public void Compute_Day1WithZeroSales_ForecastIsZero_ConfidenceLow()
        {
            var businessDate = new DateTime(2026, 6, 1);
            var result = SalesForecastPolicy.Compute(
                0m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.ForecastSales.Should().Be(0m);
            result.DailyAverageSales.Should().Be(0m);
            result.ForecastConfidence.Should().Be(SalesForecastPolicy.ConfidenceLow);
        }

        [Fact]
        public void Compute_Day1WithSales_Extrapolates_ConfidenceLow()
        {
            var businessDate = new DateTime(2026, 6, 1);
            var result = SalesForecastPolicy.Compute(
                500_000m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.DailyAverageSales.Should().Be(500_000m);
            result.ForecastSales.Should().Be(15_000_000m);
            result.ForecastConfidence.Should().Be(SalesForecastPolicy.ConfidenceLow);
        }

        [Fact]
        public void Compute_TargetAlreadyAchieved_RequiredDailyIsZero()
        {
            var businessDate = new DateTime(2026, 6, 20);
            var result = SalesForecastPolicy.Compute(
                12_000_000m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.RequiredDailySales.Should().Be(0m);
            result.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Healthy);
        }

        [Fact]
        public void Compute_TargetNullOrZero_AchievementNull_RequiredDailyNull_BandUnknown()
        {
            var businessDate = new DateTime(2026, 6, 10);
            var nullTarget = SalesForecastPolicy.Compute(
                1_000_000m,
                null,
                businessDate,
                MonthStart,
                MonthEnd);

            nullTarget.CurrentAchievementPercent.Should().BeNull();
            nullTarget.ForecastAchievementPercent.Should().BeNull();
            nullTarget.RequiredDailySales.Should().BeNull();
            nullTarget.ForecastRiskBand.Should().Be(ExecutiveSalesAchievementBandResolver.Unknown);

            var zeroTarget = SalesForecastPolicy.Compute(
                1_000_000m,
                0m,
                businessDate,
                MonthStart,
                MonthEnd);

            zeroTarget.CurrentAchievementPercent.Should().BeNull();
            zeroTarget.RequiredDailySales.Should().BeNull();
        }

        [Fact]
        public void Compute_LastDayOfMonth_ForecastEqualsCurrentSales_DaysRemainingZero()
        {
            var businessDate = MonthEnd;
            var result = SalesForecastPolicy.Compute(
                8_000_000m,
                10_000_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.ForecastSales.Should().Be(8_000_000m);
            result.DaysRemaining.Should().Be(0);
            result.RequiredDailySales.Should().Be(0m);
        }

        [Fact]
        public void Compute_DaysElapsedGuard_NoDivideByZeroOnMonthStart()
        {
            var businessDate = new DateTime(2026, 6, 1);
            var result = SalesForecastPolicy.Compute(
                100m,
                1_000m,
                businessDate,
                MonthStart,
                MonthEnd);

            result.DaysElapsed.Should().Be(1);
            result.DailyAverageSales.Should().Be(100m);
        }

        [Theory]
        [InlineData(3, SalesForecastPolicy.ConfidenceLow)]
        [InlineData(5, SalesForecastPolicy.ConfidenceLow)]
        [InlineData(6, SalesForecastPolicy.ConfidenceMedium)]
        [InlineData(20, SalesForecastPolicy.ConfidenceMedium)]
        [InlineData(21, SalesForecastPolicy.ConfidenceHigh)]
        public void ResolveConfidence_FollowsDayThresholds(int daysElapsed, string expected)
        {
            SalesForecastPolicy.ResolveConfidence(daysElapsed, 30).Should().Be(expected);
        }

        [Theory]
        [InlineData(300_000, 100_000, SalesForecastPolicy.RequiredDailySeverityCritical)]
        [InlineData(160_000, 100_000, SalesForecastPolicy.RequiredDailySeverityWarning)]
        [InlineData(100_000, 100_000, SalesForecastPolicy.RequiredDailySeverityNormal)]
        public void ResolveRequiredDailySeverity_FollowsRatioThresholds(
            decimal required,
            decimal dailyAvg,
            string expected)
        {
            SalesForecastPolicy.ResolveRequiredDailySeverity(required, dailyAvg).Should().Be(expected);
        }
    }
}
