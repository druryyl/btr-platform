using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesForecastExecutiveSummaryBuilderTest
    {
        [Fact]
        public void Build_OnTrack_IncludesTargetMetMessage()
        {
            var response = BaseResponse();
            response.ForecastAchievementPercent = 105m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Healthy;

            var summary = SalesForecastExecutiveSummaryBuilder.Build(response);

            summary.Should().Contain("on track");
        }

        [Fact]
        public void Build_AtRisk_IncludesRequiredDaily()
        {
            var response = BaseResponse();
            response.ForecastAchievementPercent = 85m;
            response.RequiredDailySales = 500_000m;

            var summary = SalesForecastExecutiveSummaryBuilder.Build(response);

            summary.Should().Contain("500,000");
            summary.Should().Contain("close the gap");
        }

        [Fact]
        public void Build_Critical_IncludesCorrectiveAction()
        {
            var response = BaseResponse();
            response.ForecastAchievementPercent = 60m;
            response.ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Critical;
            response.RequiredDailySales = 800_000m;

            var summary = SalesForecastExecutiveSummaryBuilder.Build(response);

            summary.Should().Contain("Immediate corrective action");
        }

        [Fact]
        public void Build_UnknownTarget_StatesComparisonUnavailable()
        {
            var response = BaseResponse();
            response.TotalTarget = 0m;

            var summary = SalesForecastExecutiveSummaryBuilder.Build(response);

            summary.Should().Contain("No monthly target is configured");
        }

        [Fact]
        public void Build_LowConfidence_IncludesPreliminaryPrefix()
        {
            var response = BaseResponse();
            response.ForecastConfidence = SalesForecastPolicy.ConfidenceLow;
            response.DaysElapsed = 3;
            response.DaysInMonth = 30;

            var summary = SalesForecastExecutiveSummaryBuilder.Build(response);

            summary.Should().Contain("preliminary");
            summary.Should().Contain("day 3 of 30");
        }

        private static DashboardSalesForecastResponse BaseResponse() =>
            new DashboardSalesForecastResponse
            {
                GeneratedAt = new DateTime(2026, 6, 15),
                PeriodYear = 2026,
                PeriodMonth = 6,
                BusinessDate = new DateTime(2026, 6, 15),
                DaysInMonth = 30,
                DaysElapsed = 15,
                DaysRemaining = 15,
                CurrentSales = 3_000_000m,
                TotalTarget = 10_000_000m,
                ForecastSales = 6_000_000m,
                ForecastAchievementPercent = 60m,
                ForecastConfidence = SalesForecastPolicy.ConfidenceMedium,
                ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Critical,
                RequiredDailySales = 466_667m
            };
    }
}
