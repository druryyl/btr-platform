using System;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetHealthPolicyTest
    {
        private readonly SalesOmzetHealthPolicy _policy = new SalesOmzetHealthPolicy();

        [Theory]
        [InlineData(90, SalesOmzetHealthLevelEnum.Good)]
        [InlineData(100, SalesOmzetHealthLevelEnum.Good)]
        [InlineData(70, SalesOmzetHealthLevelEnum.Warning)]
        [InlineData(89, SalesOmzetHealthLevelEnum.Warning)]
        [InlineData(69, SalesOmzetHealthLevelEnum.Poor)]
        [InlineData(0, SalesOmzetHealthLevelEnum.Poor)]
        public void ResolveLevel_UsesScoreThresholds(int score, SalesOmzetHealthLevelEnum expected)
        {
            _policy.ResolveLevel(score).Should().Be(expected);
        }

        [Fact]
        public void ComputeScore_ReturnsZero_WhenManyMissing()
        {
            var metrics = new SalesOmzetHealthMetrics
            {
                MissingOrders = 50,
                LastReconciledMax = DateTime.Today
            };
            var weekEnd = new DateTime(2026, 3, 22);
            var weekStart = new DateTime(2026, 3, 16);

            _policy.ComputeScore(metrics, weekStart, weekEnd).Should().Be(0);
            _policy.ResolveLevel(0).Should().Be(SalesOmzetHealthLevelEnum.Poor);
        }

        [Fact]
        public void ComputeScore_ReturnsHighScore_WhenNoGapsAndFreshReconcile()
        {
            var weekEnd = new DateTime(2026, 3, 22);
            var weekStart = new DateTime(2026, 3, 16);
            var metrics = new SalesOmzetHealthMetrics
            {
                MissingOrders = 0,
                MissingDirectFakturs = 0,
                UnlinkedFakturs = 0,
                StaleFakturEstimate = 0,
                LastReconciledMax = weekEnd.AddDays(-2)
            };

            var score = _policy.ComputeScore(metrics, weekStart, weekEnd);

            score.Should().BeGreaterOrEqualTo(90);
            _policy.ResolveLevel(score).Should().Be(SalesOmzetHealthLevelEnum.Good);
        }
    }
}
