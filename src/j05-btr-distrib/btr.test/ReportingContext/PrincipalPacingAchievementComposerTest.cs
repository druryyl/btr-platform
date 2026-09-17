using System;
using btr.application.Portal;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalPacingAchievementComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 15, 8, 0, 0);

        private readonly PrincipalPacingAchievementComposer _composer = new PrincipalPacingAchievementComposer();

        [Fact]
        public void Compose_MidMonth_CalculatesExpectedTargetMtdAndPacing()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);

            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m), TargetRow("SUPB", "Beta", 50m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPB", "Beta", 40m)),
                period,
                GeneratedAt);

            result.PacingAchievementKpiId.Should().Be("PRN-TGT-004");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.TargetKpiId.Should().Be("PRN-TGT-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);

            var alpha = result.Principals[0];
            alpha.SupplierId.Should().Be("SUPA");
            alpha.SalesOutAmount.Should().Be(1000m);
            alpha.TargetAmount.Should().Be(800m);
            // expected target = 800 × 15/30 = 400; pacing = 1000 / 400 × 100 = 250.000000
            alpha.PacingAchievementPercentage.Should().Be(250.000000m);

            var beta = result.Principals[1];
            beta.SupplierId.Should().Be("SUPB");
            // expected target = 50 × 15/30 = 25; pacing = 40 / 25 × 100 = 160.000000
            beta.PacingAchievementPercentage.Should().Be(160.000000m);
        }

        [Fact]
        public void Compose_SortOrderDescendsByPacingAchievement()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);

            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m), TargetRow("SUPB", "Beta", 50m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPB", "Beta", 40m)),
                period,
                GeneratedAt);

            // SUPA: 250.000000; SUPB: 160.000000 — sorted descending
            result.Principals[0].SupplierId.Should().Be("SUPA");
            result.Principals[0].SortOrder.Should().Be(1);
            result.Principals[1].SupplierId.Should().Be("SUPB");
            result.Principals[1].SortOrder.Should().Be(2);
        }

        [Fact]
        public void Calculate_MonthEnd_EqualsFullMonthAchievementPercentTimesHundred()
        {
            // month-end: elapsed=30, days=30 → expected target = target; pacing = sales/target × 100
            var value = PrincipalPacingAchievementComposer.Calculate(
                salesOutAmount: 1000m,
                targetAmount: 800m,
                elapsedDays: 30,
                daysInMonth: 30);

            value.Should().Be(125.000000m);
        }

        [Fact]
        public void Calculate_Day1_ExpectedPaceIsTargetDividedByDaysInMonth()
        {
            // day 1: expected target = 800 / 30 = 26.6666666667; pacing = 1000 / 26.6666666667 × 100 = 3750.000000
            var value = PrincipalPacingAchievementComposer.Calculate(
                salesOutAmount: 1000m,
                targetAmount: 800m,
                elapsedDays: 1,
                daysInMonth: 30);

            value.Should().Be(3750.000000m);
        }

        [Fact]
        public void Calculate_ReturnsNull_WhenMonthlyTargetIsNotGreaterThanZero()
        {
            PrincipalPacingAchievementComposer.Calculate(1000m, 0m, 15, 30).Should().BeNull();
            PrincipalPacingAchievementComposer.Calculate(1000m, -5m, 15, 30).Should().BeNull();
        }

        [Fact]
        public void Calculate_ReturnsNull_WhenSalesOutIsNull()
        {
            PrincipalPacingAchievementComposer.Calculate(null, 800m, 15, 30).Should().BeNull();
        }

        [Fact]
        public void Calculate_ReturnsNull_WhenElapsedDaysOrDaysInMonthIsNonPositive()
        {
            PrincipalPacingAchievementComposer.Calculate(1000m, 800m, 0, 30).Should().BeNull();
            PrincipalPacingAchievementComposer.Calculate(1000m, 800m, 15, 0).Should().BeNull();
        }

        [Fact]
        public void Calculate_RoundsToPercentageScale6()
        {
            // expected = 800 × 7 / 30 = 186.6666666667; pacing = 1000 / 186.6666666667 × 100 = 535.714285714
            var value = PrincipalPacingAchievementComposer.Calculate(
                salesOutAmount: 1000m,
                targetAmount: 800m,
                elapsedDays: 7,
                daysInMonth: 30);

            value.Should().Be(535.714286m);
        }

        [Fact]
        public void Compose_NullPeriod_ResultsInNullPercentages()
        {
            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m)),
                null,
                GeneratedAt);

            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.Principals.Should().ContainSingle()
                .Which.PacingAchievementPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotUseSourcesFromADifferentPeriod()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);

            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)),
                SalesOut(2026, 8, SalesRow("SUPA", "Alpha", 1000m)),
                period,
                GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.TargetAmount.Should().Be(800m);
            alpha.SalesOutAmount.Should().BeNull();
            alpha.PacingAchievementPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_IncludesSalesOnlyPrincipal_WithNullPacing()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);

            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPC", "Charlie", 300m)),
                period,
                GeneratedAt);

            var charlie = result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPC").Subject;
            charlie.SalesOutAmount.Should().Be(300m);
            charlie.TargetAmount.Should().BeNull();
            charlie.PacingAchievementPercentage.Should().BeNull();
        }

        private static PrincipalTargetAggregateResult Targets(
            int year,
            int month,
            params PrincipalTargetRow[] rows)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = GeneratedAt,
                Principals = rows
            };
        }

        private static PrincipalSalesOutAggregateResult SalesOut(
            int year,
            int month,
            params PrincipalSalesOutRow[] rows)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = GeneratedAt,
                Principals = rows
            };
        }

        private static PrincipalTargetRow TargetRow(
            string supplierId,
            string supplierName,
            decimal targetAmount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TargetAmount = targetAmount
            };
        }

        private static PrincipalSalesOutRow SalesRow(
            string supplierId,
            string supplierName,
            decimal salesOutAmount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount
            };
        }
    }
}