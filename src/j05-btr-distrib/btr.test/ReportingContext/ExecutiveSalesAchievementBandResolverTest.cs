using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class ExecutiveSalesAchievementBandResolverTest
    {
        [Fact]
        public void Resolve_WhenNull_ReturnsUnknown()
        {
            ExecutiveSalesAchievementBandResolver.Resolve(null)
                .Should().Be(ExecutiveSalesAchievementBandResolver.Unknown);
        }

        [Theory]
        [InlineData(79.9, ExecutiveSalesAchievementBandResolver.Critical)]
        [InlineData(80, ExecutiveSalesAchievementBandResolver.Warning)]
        [InlineData(99.9, ExecutiveSalesAchievementBandResolver.Warning)]
        [InlineData(100, ExecutiveSalesAchievementBandResolver.Healthy)]
        [InlineData(150, ExecutiveSalesAchievementBandResolver.Healthy)]
        public void Resolve_ReturnsExpectedBand(decimal percent, string expectedBand)
        {
            ExecutiveSalesAchievementBandResolver.Resolve(percent)
                .Should().Be(expectedBand);
        }
    }
}
