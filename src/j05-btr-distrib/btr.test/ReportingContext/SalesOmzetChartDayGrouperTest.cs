using System;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesOmzetChartDayGrouperTest
    {
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private static readonly Periode Feb2024 = new Periode(
            new DateTime(2024, 2, 1),
            new DateTime(2024, 2, 29));

        [Fact]
        public void BuildBuckets_ReturnsOneBucketPerCalendarDay()
        {
            var buckets = SalesOmzetChartDayGrouper.BuildBuckets(June2026);

            buckets.Should().HaveCount(30);
            buckets[0].PaceDate.Should().Be(new DateTime(2026, 6, 1));
            buckets[0].DayOfMonth.Should().Be(1);
            buckets[29].PaceDate.Should().Be(new DateTime(2026, 6, 30));
        }

        [Fact]
        public void BuildBuckets_HandlesLeapYearFebruary()
        {
            var buckets = SalesOmzetChartDayGrouper.BuildBuckets(Feb2024);

            buckets.Should().HaveCount(29);
        }

        [Fact]
        public void FormatDayLabel_UsesIndonesianCulture()
        {
            var label = SalesOmzetChartDayGrouper.FormatDayLabel(new DateTime(2026, 6, 15));

            label.Should().Contain("15");
            label.Should().Contain("Jun");
        }

        [Fact]
        public void FindBucket_ReturnsMatchingDay()
        {
            var buckets = SalesOmzetChartDayGrouper.BuildBuckets(June2026);

            var found = SalesOmzetChartDayGrouper.FindBucket(buckets, new DateTime(2026, 6, 10));

            found.Should().NotBeNull();
            found.DayOfMonth.Should().Be(10);
        }

        [Fact]
        public void FindBucket_ReturnsNull_WhenDateOutsideRange()
        {
            var buckets = SalesOmzetChartDayGrouper.BuildBuckets(June2026);

            SalesOmzetChartDayGrouper.FindBucket(buckets, new DateTime(2026, 7, 1))
                .Should().BeNull();
        }
    }
}
