using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityRankingCalculatorTest
    {
        [Fact]
        public void Calculate_HigherIsBetter_OrdersDescending()
        {
            var results = EntityRankingCalculator.Calculate(
                new[] { ("A", 100m), ("B", 300m), ("C", 200m) },
                "HigherIsBetter");

            results.Should().HaveCount(3);
            results.Single(r => r.EntityId == "B").RankPosition.Should().Be(1);
            results.Single(r => r.EntityId == "C").RankPosition.Should().Be(2);
            results.Single(r => r.EntityId == "A").RankPosition.Should().Be(3);
        }

        [Fact]
        public void Calculate_LowerIsBetter_OrdersAscending()
        {
            var results = EntityRankingCalculator.Calculate(
                new[] { ("A", 100m), ("B", 300m), ("C", 200m) },
                "LowerIsBetter");

            results.Single(r => r.EntityId == "A").RankPosition.Should().Be(1);
            results.Single(r => r.EntityId == "C").RankPosition.Should().Be(2);
            results.Single(r => r.EntityId == "B").RankPosition.Should().Be(3);
        }

        [Fact]
        public void Calculate_Ties_UseCompetitionRanking()
        {
            var results = EntityRankingCalculator.Calculate(
                new[] { ("A", 100m), ("B", 200m), ("C", 200m), ("D", 50m) },
                "HigherIsBetter");

            results.Single(r => r.EntityId == "B").RankPosition.Should().Be(1);
            results.Single(r => r.EntityId == "C").RankPosition.Should().Be(1);
            results.Single(r => r.EntityId == "A").RankPosition.Should().Be(3);
            results.Single(r => r.EntityId == "D").RankPosition.Should().Be(4);
        }

        [Fact]
        public void CalculatePercentile_RankOne_Is100()
        {
            EntityRankingCalculator.CalculatePercentile(1, 100).Should().Be(100m);
        }

        [Fact]
        public void CalculatePercentile_LastRank_IsLowest()
        {
            EntityRankingCalculator.CalculatePercentile(100, 100).Should().Be(1m);
        }

        [Fact]
        public void Calculate_SetsPopulationSizeOnAllRows()
        {
            var results = EntityRankingCalculator.Calculate(
                new[] { ("A", 1m), ("B", 2m), ("C", 3m) },
                "HigherIsBetter");

            results.Should().OnlyContain(r => r.PopulationSize == 3);
        }

        [Fact]
        public void Calculate_Empty_ReturnsEmpty()
        {
            EntityRankingCalculator.Calculate(Array.Empty<(string, decimal)>(), "HigherIsBetter")
                .Should().BeEmpty();
        }
    }
}
