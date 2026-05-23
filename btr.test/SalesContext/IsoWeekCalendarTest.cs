using System;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class IsoWeekCalendarTest
    {
        private readonly IsoWeekCalendar _calendar = new IsoWeekCalendar();

        [Fact]
        public void GetIsoWeek_2026_03_17_IsWeek12()
        {
            var id = _calendar.GetIsoWeek(new DateTime(2026, 3, 17));

            id.YearNumber.Should().Be(2026);
            id.WeekNumber.Should().Be(12);
        }

        [Fact]
        public void GetWeekBounds_2026_Week12_IsMon17_ToSun23Mar()
        {
            var (start, end) = _calendar.GetWeekBounds(2026, 12);

            start.Should().Be(new DateTime(2026, 3, 16));
            end.Should().Be(new DateTime(2026, 3, 22));
        }

        [Fact]
        public void GetIsoWeek_YearBoundary_2020_12_31_IsWeek53Of2020()
        {
            var id = _calendar.GetIsoWeek(new DateTime(2020, 12, 31));

            id.YearNumber.Should().Be(2020);
            id.WeekNumber.Should().Be(53);
        }

        [Fact]
        public void GetIsoWeek_YearBoundary_2021_01_01_IsWeek53Of2020()
        {
            var id = _calendar.GetIsoWeek(new DateTime(2021, 1, 1));

            id.YearNumber.Should().Be(2020);
            id.WeekNumber.Should().Be(53);
        }

        [Fact]
        public void ListWeeksIntersecting_SpanningTwoWeeks_ReturnsBoth()
        {
            var periode = new Periode(new DateTime(2026, 3, 20), new DateTime(2026, 3, 25));

            var weeks = _calendar.ListWeeksIntersecting(periode);

            weeks.Should().HaveCount(2);
            weeks[0].WeekNumber.Should().Be(12);
            weeks[1].WeekNumber.Should().Be(13);
        }

        [Fact]
        public void ListWeeksIntersecting_SingleDay_ReturnsOneWeek()
        {
            var periode = new Periode(new DateTime(2026, 3, 17), new DateTime(2026, 3, 17));

            var weeks = _calendar.ListWeeksIntersecting(periode);

            weeks.Should().ContainSingle().Which.WeekNumber.Should().Be(12);
        }
    }
}
