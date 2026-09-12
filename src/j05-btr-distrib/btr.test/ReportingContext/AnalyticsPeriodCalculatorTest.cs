using System;
using btr.application.Portal;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class AnalyticsPeriodCalculatorTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 6, 15, 9, 30, 0);

        [Fact]
        public void Create_Day1_ElapsedDaysIsOne()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2026, 6, 1), GeneratedAt);

            result.AsOfDate.Should().Be(new DateTime(2026, 6, 1));
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(6);
            result.MonthStart.Should().Be(new DateTime(2026, 6, 1));
            result.MonthEnd.Should().Be(new DateTime(2026, 6, 30));
            result.ElapsedDays.Should().Be(1);
            result.DaysInMonth.Should().Be(30);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(6);
            result.PriorYearStart.Should().Be(new DateTime(2025, 6, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2025, 6, 1));
            result.GeneratedAt.Should().Be(GeneratedAt);
        }

        [Fact]
        public void Create_MidMonth_AlignsPriorYearElapsedDays()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2026, 6, 15), GeneratedAt);

            result.ElapsedDays.Should().Be(15);
            result.DaysInMonth.Should().Be(30);
            result.PriorYearStart.Should().Be(new DateTime(2025, 6, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2025, 6, 15));
        }

        [Fact]
        public void Create_MonthEnd_PriorYearEndIsLastDay()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2026, 6, 30), GeneratedAt);

            result.ElapsedDays.Should().Be(30);
            result.DaysInMonth.Should().Be(30);
            result.PriorYearStart.Should().Be(new DateTime(2025, 6, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2025, 6, 30));
        }

        [Fact]
        public void Create_LeapYearFeb29_ClampsPriorYearToLastValidDay()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2028, 2, 29), GeneratedAt);

            result.ElapsedDays.Should().Be(29);
            result.DaysInMonth.Should().Be(29);
            result.PriorYear.Should().Be(2027);
            result.PriorMonth.Should().Be(2);
            result.PriorYearStart.Should().Be(new DateTime(2027, 2, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2027, 2, 28));
        }

        [Fact]
        public void Create_LeapYearFebMidMonth_KeepsAlignedDay()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2028, 2, 20), GeneratedAt);

            result.ElapsedDays.Should().Be(20);
            result.DaysInMonth.Should().Be(29);
            result.PriorYearStart.Should().Be(new DateTime(2027, 2, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2027, 2, 20));
        }

        [Fact]
        public void Create_January_AlignedPriorYearCrossesYearBoundary()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2026, 1, 5), GeneratedAt);

            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(1);
            result.ElapsedDays.Should().Be(5);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(1);
            result.PriorYearStart.Should().Be(new DateTime(2025, 1, 1));
            result.PriorYearEnd.Should().Be(new DateTime(2025, 1, 5));
        }

        [Fact]
        public void Create_31DayMonth_UsesCorrectDaysInMonth()
        {
            var result = AnalyticsPeriodCalculator.Create(new DateTime(2026, 12, 31), GeneratedAt);

            result.DaysInMonth.Should().Be(31);
            result.ElapsedDays.Should().Be(31);
            result.MonthEnd.Should().Be(new DateTime(2026, 12, 31));
            result.PriorYearEnd.Should().Be(new DateTime(2025, 12, 31));
        }

        [Fact]
        public void Resolve_SourcesBusinessDateAndGeneratedAtFromProviders()
        {
            var businessDate = new DateTime(2026, 6, 15);
            var now = new DateTime(2026, 6, 15, 14, 5, 0);
            var calculator = new AnalyticsPeriodCalculator(
                new StubBusinessDateProvider(businessDate),
                new StubTglJamDal(now));

            var result = calculator.Resolve();

            result.AsOfDate.Should().Be(businessDate);
            result.GeneratedAt.Should().Be(now);
        }

        private sealed class StubBusinessDateProvider : IBusinessDateProvider
        {
            public StubBusinessDateProvider(DateTime today)
            {
                Today = today;
            }

            public DateTime Today { get; }

            public bool IsPresentationActive => false;
        }

        private sealed class StubTglJamDal : btr.application.SupportContext.TglJamAgg.ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }
    }
}
