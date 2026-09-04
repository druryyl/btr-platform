using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityPeerDistributionBinBuilderTest
    {
        [Fact]
        public void BuildBins_Empty_ReturnsEmpty()
        {
            EntityPeerDistributionBinBuilder.BuildBins(Array.Empty<decimal>(), 20, "IDR")
                .Should().BeEmpty();
        }

        [Fact]
        public void BuildBins_IdenticalValues_ReturnsSingleBin()
        {
            var values = Enumerable.Repeat(50_000_000m, 10).ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");

            bins.Should().HaveCount(1);
            bins[0].Count.Should().Be(10);
            bins[0].BinStart.Should().Be(50_000_000m);
            bins[0].BinEnd.Should().Be(50_000_000m);
        }

        [Fact]
        public void BuildBins_Idr_FirstBinEndsAt100000()
        {
            var values = new List<decimal> { 0m, 50_000m, 200_000m, 5_000_000m, 50_000_000m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");

            bins.Should().NotBeEmpty();
            bins[0].BinStart.Should().Be(0m);
            bins[0].BinEnd.Should().Be(EntityPeerDistributionBinBuilder.IdrFirstBinEnd);
            bins[0].Count.Should().Be(2); // 0 and 50_000
            bins.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void BuildBins_Idr_InternalEdgesAreOnNiceLadder()
        {
            var values = new List<decimal>();
            for (var i = 0; i < 40; i++)
                values.Add(200_000m + (i * 2_500_000m));
            values.Add(500_000_000m);
            values.Add(1_000_000_000m);
            values = values.OrderBy(v => v).ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");

            bins[0].BinEnd.Should().Be(100_000m);
            // Every edge except last bin end (peer max) must be on the 1-2-5 ladder.
            for (var i = 0; i < bins.Count - 1; i++)
            {
                IsNiceIdrEdge(bins[i].BinEnd).Should().BeTrue(
                    $"internal end {bins[i].BinEnd:N0} should be on ladder");
            }

            for (var i = 1; i < bins.Count; i++)
            {
                IsNiceIdrEdge(bins[i].BinStart).Should().BeTrue(
                    $"bin start {bins[i].BinStart:N0} should be on ladder");
            }

            bins.Last().BinEnd.Should().Be(values.Last());
        }

        [Fact]
        public void BuildBins_SkewedIdr_SpreadsAbove100kWithRoundLabels()
        {
            var values = new List<decimal>();
            for (var i = 0; i < 30; i++)
                values.Add(50_000m); // below first floor
            for (var i = 0; i < 50; i++)
                values.Add(5_000_000m + (i * 1_000_000m)); // 5M .. 54M
            values.Add(500_000_000m);
            values.Add(800_000_000m);
            values.Add(1_500_000_000m);
            values = values.OrderBy(v => v).ToList();

            var linear = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "%");
            var idr = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");

            idr[0].BinStart.Should().Be(0m);
            idr[0].BinEnd.Should().Be(100_000m);
            idr[0].Count.Should().Be(30);

            idr.Count(b => b.Count > 0).Should().BeGreaterThan(3);
            idr.Sum(b => b.Count).Should().Be(values.Count);
            idr.Last().BinEnd.Should().Be(values.Last());

            // More occupied segments than collapsed linear left mass pattern for amount span
            idr.Skip(1).Count(b => b.Count > 0).Should().BeGreaterThan(1);
            linear[0].Count.Should().BeGreaterThan(idr[0].Count);
        }

        [Fact]
        public void BuildBins_Idr_AllBelow100k_SingleFirstBin()
        {
            var values = new List<decimal> { 0m, 10_000m, 50_000m, 99_999m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");

            bins.Should().HaveCount(1);
            bins[0].BinStart.Should().Be(0m);
            bins[0].BinEnd.Should().Be(100_000m);
            bins[0].Count.Should().Be(4);
        }

        [Fact]
        public void BuildBins_NonIdr_UsesLinearEqualWidth()
        {
            var values = new List<decimal> { 0m, 10m, 20m, 30m, 40m, 50m, 60m, 70m, 80m, 90m, 100m };

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 10, "%");

            bins.Should().HaveCount(10);
            bins[0].BinStart.Should().Be(0m);
            bins[0].BinEnd.Should().Be(10m);
            bins[1].BinStart.Should().Be(10m);
            bins[1].BinEnd.Should().Be(20m);
            bins.Last().BinEnd.Should().Be(100m);
            bins.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void BuildBins_IdrWithZeros_DoesNotThrow()
        {
            var values = new List<decimal> { 0m, 0m, 5_000_000m, 50_000_000m, 500_000_000m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "idr");

            bins.Should().NotBeEmpty();
            bins.Sum(b => b.Count).Should().Be(values.Count);
            bins.First().BinStart.Should().Be(0m);
            bins.First().BinEnd.Should().Be(100_000m);
            bins.Last().BinEnd.Should().Be(500_000_000m);
        }

        [Fact]
        public void BuildBins_Idr_HighEndLadder_SplitsWideTailBeforeMax()
        {
            var values = new List<decimal>();
            for (var i = 0; i < 40; i++)
                values.Add(5_000_000m + (i * 1_000_000m));
            values.Add(60_000_000m);
            values.Add(150_000_000m);
            values.Add(350_000_000m);
            values.Add(600_000_000m);
            values.Add(1_400_000_000m);
            values = values.OrderBy(v => v).ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");
            var ends = bins.Select(b => b.BinEnd).ToList();
            var starts = bins.Select(b => b.BinStart).ToList();

            starts.Should().Contain(50_000_000m);
            starts.Should().Contain(100_000_000m);
            starts.Should().Contain(200_000_000m);
            starts.Should().Contain(500_000_000m);
            ends.Should().Contain(50_000_000m);
            ends.Should().Contain(100_000_000m);
            ends.Should().Contain(200_000_000m);
            ends.Should().Contain(500_000_000m);

            bins.Last().BinEnd.Should().Be(1_400_000_000m);
            bins.Should().NotContain(b => b.BinStart <= 50_000_000m && b.BinEnd >= 1_400_000_000m,
                "must not keep a single bin spanning 50M → max");
            bins.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void BuildBins_Idr_HighEndLadder_SkipsStepsAtOrAboveMax()
        {
            var values = new List<decimal> { 1_000_000m, 20_000_000m, 80_000_000m, 300_000_000m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "IDR");
            var starts = bins.Select(b => b.BinStart).ToList();

            starts.Should().Contain(50_000_000m);
            starts.Should().Contain(100_000_000m);
            starts.Should().Contain(200_000_000m);
            starts.Should().NotContain(500_000_000m);
            bins.Last().BinEnd.Should().Be(300_000_000m);
        }

        [Fact]
        public void BuildBins_Days_UsesWeekMonthCalendarLadder()
        {
            var values = new List<decimal> { 3m, 10m, 25m, 45m, 80m, 200m, 400m, 800m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "Days");

            bins[0].BinStart.Should().Be(0m);
            bins[0].BinEnd.Should().Be(7m);
            bins.Select(b => b.BinEnd).Should().Contain(new[] { 7m, 14m, 21m, 30m, 60m, 90m, 120m, 180m, 270m, 365m });
            bins.Last().BinStart.Should().Be(365m);
            bins.Last().BinEnd.Should().Be(800m);
            bins.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void BuildBins_Days_LastBinStartsAt365_WhenMaxAboveOneYear()
        {
            var values = new List<decimal>();
            for (var i = 0; i < 30; i++)
                values.Add(5m + i); // ~1 week cluster
            for (var i = 0; i < 20; i++)
                values.Add(40m + i); // ~1-2 months
            values.Add(500m);
            values.Add(1_200m);
            values = values.OrderBy(v => v).ToList();

            var linear = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "%");
            var days = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "days");

            days.Last().BinStart.Should().Be(EntityPeerDistributionBinBuilder.DaysYearEdge);
            days.Last().BinEnd.Should().Be(1_200m);
            days.Should().NotContain(b => b.BinStart < 30m && b.BinEnd >= 1_200m,
                "must not keep one linear-style bin spanning weeks → year+");
            linear[0].Count.Should().BeGreaterThan(days[0].Count);
            days.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void BuildBins_Days_ClipsLadderWhenMaxBelow365()
        {
            var values = new List<decimal> { 2m, 8m, 16m, 40m, 100m }
                .OrderBy(v => v)
                .ToList();

            var bins = EntityPeerDistributionBinBuilder.BuildBins(values, 20, "Days");

            bins.Select(b => b.BinStart).Should().NotContain(365m);
            bins.Last().BinEnd.Should().Be(100m);
            bins.Select(b => b.BinEnd).Should().Contain(new[] { 7m, 14m, 21m, 30m, 60m, 90m });
            bins.Sum(b => b.Count).Should().Be(values.Count);
        }

        [Fact]
        public void SnapToNiceIdr_SnapsToLadder()
        {
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(50_000m).Should().Be(100_000m);
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(100_000m).Should().Be(100_000m);
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(1_400_000m).Should().Be(1_000_000m);
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(1_600_000m).Should().Be(2_000_000m);
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(7_000_000m).Should().Be(5_000_000m);
            EntityPeerDistributionBinBuilder.SnapToNiceIdr(8_000_000m).Should().Be(10_000_000m);
        }

        private static bool IsNiceIdrEdge(decimal value)
        {
            if (value < EntityPeerDistributionBinBuilder.IdrFirstBinEnd)
                return false;
            if (value == EntityPeerDistributionBinBuilder.IdrFirstBinEnd)
                return true;

            return EntityPeerDistributionBinBuilder.SnapToNiceIdr(value) == value;
        }
    }
}
