using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CollectionOptimizationActionBuilderTest
    {
        [Fact]
        public void BuildSelectionReasonText_IncludesOverdueAmount()
        {
            var forecast = new CustomerRiskForecastContext
            {
                OverdueBalance = 45_000_000m,
                Category = CustomerRiskForecastPolicy.CategoryHighRisk
            };

            var text = CollectionOptimizationActionBuilder.BuildSelectionReasonText(
                forecast,
                null,
                "Chronic Overdue");

            text.Should().Contain("45");
            text.Should().Contain("High Risk");
        }

        [Fact]
        public void BuildTriggeredRuleIds_IncludesM29AndCatRules()
        {
            var forecast = new CustomerRiskForecastContext
            {
                Signals =
                {
                    new CustomerRiskSignalContext
                    {
                        RuleId = "CRF-L03",
                        Severity = CustomerRiskForecastPolicy.SeverityStrong,
                        SignalKey = CustomerRiskSignalBuilder.SignalChronicTrajectory
                    }
                }
            };

            var ids = CollectionOptimizationActionBuilder.BuildTriggeredRuleIds(
                forecast,
                "COL-OPT-CAT-01",
                "CRF-REC-06");

            ids.Should().Contain("CRF-REC-06");
            ids.Should().Contain("CRF-L03");
            ids.Should().Contain("COL-OPT-CAT-01");
            ids.Should().Contain("COL-OPT-REC-02");
        }
    }
}
