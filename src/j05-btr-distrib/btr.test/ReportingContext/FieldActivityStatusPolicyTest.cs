using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class FieldActivityStatusPolicyTest
    {
        [Fact]
        public void Resolve_WhenNoEmail_ReturnsNoFieldData()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult(), false)
                .Should().Be(FieldActivitySalesmanStatus.NoFieldData);
        }

        [Fact]
        public void Resolve_WhenNoPlanAndNoActual_ReturnsNoPlan()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult(), true)
                .Should().Be(FieldActivitySalesmanStatus.NoPlan);
        }

        [Fact]
        public void Resolve_WhenPlannedButNoActual_ReturnsCritical()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult
            {
                PlannedVisits = 3,
                ActualVisits = 0
            }, true).Should().Be(FieldActivitySalesmanStatus.Critical);
        }

        [Fact]
        public void Resolve_WhenExecutionBelow50_ReturnsCritical()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult
            {
                PlannedVisits = 10,
                ActualVisits = 4,
                VisitExecutionPercent = 40
            }, true).Should().Be(FieldActivitySalesmanStatus.Critical);
        }

        [Fact]
        public void Resolve_WhenOnTrackThresholds_ReturnsOnTrack()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult
            {
                PlannedVisits = 10,
                ActualVisits = 8,
                VisitExecutionPercent = 80,
                EffectiveCallRate = 50
            }, true).Should().Be(FieldActivitySalesmanStatus.OnTrack);
        }

        [Fact]
        public void Resolve_WhenUnplannedVisitsHigh_ReturnsNeedsAttention()
        {
            FieldActivityStatusPolicy.Resolve(new FieldActivityKpiInputResult
            {
                PlannedVisits = 10,
                ActualVisits = 10,
                VisitExecutionPercent = 100,
                EffectiveCallRate = 60,
                UnplannedVisits = 3
            }, true).Should().Be(FieldActivitySalesmanStatus.NeedsAttention);
        }

        [Fact]
        public void ToStatusCode_MapsKnownValues()
        {
            FieldActivityStatusPolicy.ToStatusCode(FieldActivitySalesmanStatus.OnTrack)
                .Should().Be("OnTrack");
            FieldActivityStatusPolicy.ToStatusCode(FieldActivitySalesmanStatus.NoFieldData)
                .Should().Be("NoFieldData");
        }
    }
}
