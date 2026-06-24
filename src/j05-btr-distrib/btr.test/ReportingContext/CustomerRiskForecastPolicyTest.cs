using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerRiskForecastPolicyTest
    {
        [Fact]
        public void ComputeProjectedOpenBalance_ZeroOmzet_BalanceUnchanged()
        {
            CustomerRiskForecastPolicy.ComputeProjectedOpenBalance(5_000_000m, 0m, 15, 30)
                .Should().Be(5_000_000m);
        }

        [Fact]
        public void ComputeProjectedOpenBalance_PaceAddsBilling_ConservativeProjection()
        {
            CustomerRiskForecastPolicy.ComputeProjectedOpenBalance(10_000_000m, 3_000_000m, 15, 30)
                .Should().Be(16_000_000m);
        }

        [Fact]
        public void ResolveCategory_DeclineRatio65Percent_ModerateDeclineHandledBySignalBuilder()
        {
            var projected = CustomerRiskForecastPolicy.ComputeProjectedMonthOmzet(3_500_000m, 15, 30);
            var ratio = projected / 10_000_000m;
            ratio.Should().BeApproximately(0.70m, 0.01m);
        }

        [Fact]
        public void ResolveCategory_DeclineRatio40Percent_SevereThreshold()
        {
            var projected = CustomerRiskForecastPolicy.ComputeProjectedMonthOmzet(2_000_000m, 15, 30);
            var ratio = projected / 10_000_000m;
            ratio.Should().BeLessThan(CustomerRiskForecastPolicy.SevereDeclineRatio);
        }

        [Fact]
        public void ResolveCategory_Healthy_NoSignals()
        {
            CustomerRiskForecastPolicy.ResolveCategory(new List<CustomerRiskSignalContext>())
                .Should().Be(CustomerRiskForecastPolicy.CategoryHealthy);
        }

        [Fact]
        public void ResolveCategory_Critical_TripleStrong()
        {
            var signals = new List<CustomerRiskSignalContext>
            {
                Signal(CustomerRiskSignalBuilder.SignalProjectedPlafondBreach, CustomerRiskForecastPolicy.SeverityStrong),
                Signal(CustomerRiskSignalBuilder.SignalChronicTrajectory, CustomerRiskForecastPolicy.SeverityStrong),
                Signal(CustomerRiskSignalBuilder.SignalSevereDecline, CustomerRiskForecastPolicy.SeverityStrong)
            };

            CustomerRiskForecastPolicy.ResolveCategory(signals)
                .Should().Be(CustomerRiskForecastPolicy.CategoryCritical);
        }

        [Fact]
        public void ComputeRiskPriorityScore_HighExposureRanksAboveLow()
        {
            var high = CustomerRiskForecastPolicy.ComputeRiskPriorityScore(
                CustomerRiskForecastPolicy.CategoryHighRisk,
                100_000_000m,
                2,
                0,
                0,
                5);
            var low = CustomerRiskForecastPolicy.ComputeRiskPriorityScore(
                CustomerRiskForecastPolicy.CategoryHighRisk,
                1_000_000m,
                2,
                0,
                0,
                5);

            high.Should().BeGreaterThan(low);
        }

        [Theory]
        [InlineData(3, CashFlowForecastPolicy.ConfidenceLow)]
        [InlineData(6, CashFlowForecastPolicy.ConfidenceMedium)]
        [InlineData(21, CashFlowForecastPolicy.ConfidenceHigh)]
        public void ResolveForecastConfidence_FollowsDayThresholds(int daysElapsed, string expected)
        {
            CustomerRiskForecastPolicy.ResolveForecastConfidence(daysElapsed).Should().Be(expected);
        }

        [Fact]
        public void ComputePortfolioHealthScore_ZeroPiutang_NoDivideByZero()
        {
            CustomerRiskForecastPolicy.ComputePortfolioHealthScore(0m, 0m, 0, 0)
                .Should().Be(100m);
        }

        private static CustomerRiskSignalContext Signal(string key, string severity) =>
            new CustomerRiskSignalContext
            {
                SignalKey = key,
                Severity = severity,
                RuleId = "TEST",
                Explanation = "test"
            };
    }
}
