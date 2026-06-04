using System;
using System.Collections.Generic;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetReportHealthResolverTest
    {
        private readonly SalesOmzetReportHealthResolver _resolver = new SalesOmzetReportHealthResolver();

        [Fact]
        public void Resolve_WorstBucket_PoorWhenAnyWeekPoor()
        {
            var weeks = new List<IsoWeekIdentifier>
            {
                new IsoWeekIdentifier(2026, 10),
                new IsoWeekIdentifier(2026, 11),
                new IsoWeekIdentifier(2026, 12)
            };
            var rows = new List<SalesOmzetHealthWeeklyModel>
            {
                Row(2026, 10, SalesOmzetHealthLevelEnum.Good, 95),
                Row(2026, 11, SalesOmzetHealthLevelEnum.Good, 92),
                Row(2026, 12, SalesOmzetHealthLevelEnum.Poor, 40)
            };

            var result = _resolver.Resolve(
                new Periode(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)),
                weeks,
                rows);

            result.FinalLevel.Should().Be(SalesOmzetHealthLevelEnum.Poor);
            result.AverageScore.Should().Be(76);
        }

        [Fact]
        public void Resolve_WorstBucket_WarningWhenAnyWarningAndNoPoor()
        {
            var weeks = new List<IsoWeekIdentifier> { new IsoWeekIdentifier(2026, 10), new IsoWeekIdentifier(2026, 11) };
            var rows = new List<SalesOmzetHealthWeeklyModel>
            {
                Row(2026, 10, SalesOmzetHealthLevelEnum.Good, 95),
                Row(2026, 11, SalesOmzetHealthLevelEnum.Warning, 75)
            };

            var result = _resolver.Resolve(
                new Periode(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)),
                weeks,
                rows);

            result.FinalLevel.Should().Be(SalesOmzetHealthLevelEnum.Warning);
        }

        [Fact]
        public void Resolve_MissingWeek_CountsAsPoor()
        {
            var weeks = new List<IsoWeekIdentifier>
            {
                new IsoWeekIdentifier(2026, 10),
                new IsoWeekIdentifier(2026, 11)
            };
            var rows = new List<SalesOmzetHealthWeeklyModel>
            {
                Row(2026, 10, SalesOmzetHealthLevelEnum.Good, 95)
            };

            var result = _resolver.Resolve(
                new Periode(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)),
                weeks,
                rows);

            result.FinalLevel.Should().Be(SalesOmzetHealthLevelEnum.Poor);
            result.WeekDetails.Should().Contain(d => !d.IsCalculated && d.WeekNumber == 11);
        }

        [Fact]
        public void Resolve_AllGood_ReturnsGood()
        {
            var weeks = new List<IsoWeekIdentifier> { new IsoWeekIdentifier(2026, 10) };
            var rows = new List<SalesOmzetHealthWeeklyModel> { Row(2026, 10, SalesOmzetHealthLevelEnum.Good, 95) };

            var result = _resolver.Resolve(
                new Periode(new DateTime(2026, 3, 1), new DateTime(2026, 3, 7)),
                weeks,
                rows);

            result.FinalLevel.Should().Be(SalesOmzetHealthLevelEnum.Good);
        }

        private static SalesOmzetHealthWeeklyModel Row(int year, int week, SalesOmzetHealthLevelEnum level, int score)
        {
            return new SalesOmzetHealthWeeklyModel
            {
                YearNumber = year,
                WeekNumber = week,
                HealthLevel = level,
                HealthScore = score
            };
        }
    }
}
