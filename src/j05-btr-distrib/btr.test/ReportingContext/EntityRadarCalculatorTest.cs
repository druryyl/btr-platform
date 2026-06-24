using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityRadarCalculatorTest
    {
        [Fact]
        public void CalculatePeerPercentiles_HigherIsBetter_RankOneIs100()
        {
            var candidates = new List<(string EntityId, decimal Value)>
            {
                ("A", 100m),
                ("B", 50m),
                ("C", 10m)
            };

            var results = EntityRadarCalculator.CalculatePeerPercentiles(candidates, "HigherIsBetter");

            results.First(r => r.EntityId == "A").Score.Should().Be(100m);
            results.First(r => r.EntityId == "C").Score.Should().BeApproximately(33.33m, 0.01m);
        }

        [Fact]
        public void CalculatePeerPercentiles_LowerIsBetter_RankOneIs100()
        {
            var candidates = new List<(string EntityId, decimal Value)>
            {
                ("A", 10m),
                ("B", 50m),
                ("C", 100m)
            };

            var results = EntityRadarCalculator.CalculatePeerPercentiles(candidates, "LowerIsBetter");

            results.First(r => r.EntityId == "A").Score.Should().Be(100m);
            results.First(r => r.EntityId == "C").Score.Should().BeApproximately(33.33m, 0.01m);
        }

        [Fact]
        public void TryResolveBandMidpointScore_AchievementBand_UsesPortalBands()
        {
            EntityRadarCalculator.TryResolveBandMidpointScore(120m, "AchievementBand")
                .Should().Be(100m);
            EntityRadarCalculator.TryResolveBandMidpointScore(90m, "AchievementBand")
                .Should().Be(50m);
            EntityRadarCalculator.TryResolveBandMidpointScore(70m, "AchievementBand")
                .Should().Be(0m);
        }

        [Fact]
        public void ResolveBandMidpoint_MapsKnownBands()
        {
            EntityRadarCalculator.ResolveBandMidpoint(ExecutiveSalesAchievementBandResolver.Healthy)
                .Should().Be(100m);
            EntityRadarCalculator.ResolveBandMidpoint(ExecutiveSalesAchievementBandResolver.Warning)
                .Should().Be(50m);
            EntityRadarCalculator.ResolveBandMidpoint(ExecutiveSalesAchievementBandResolver.Critical)
                .Should().Be(0m);
        }
    }
}
