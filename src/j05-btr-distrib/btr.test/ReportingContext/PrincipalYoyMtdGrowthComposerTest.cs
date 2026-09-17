using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalYoyMtdGrowthComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 15, 8, 0, 0);

        [Fact]
        public void Compose_ComputesPositiveAndNegativeGrowth_FromAlignedWindows()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[]
                {
                    Row("SUPA", "Alpha", 1000m),
                    Row("SUPB", "Beta", 40m)
                },
                new[]
                {
                    Row("SUPA", "Alpha", 800m),
                    Row("SUPB", "Beta", 50m)
                });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            var result = composer.Compose(period, GeneratedAt);

            result.YoyMtdGrowthKpiId.Should().Be("PRN-GRW-003");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(9);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.CurrentYearMtdSalesOutAmount.Should().Be(1000m);
            alpha.PriorYearMtdSalesOutAmount.Should().Be(800m);
            // (1000 − 800) ÷ 800 × 100 = 25.000000
            alpha.YoyMtdGrowthPercentage.Should().Be(25.000000m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.CurrentYearMtdSalesOutAmount.Should().Be(40m);
            beta.PriorYearMtdSalesOutAmount.Should().Be(50m);
            // (40 − 50) ÷ 50 × 100 = -20.000000
            beta.YoyMtdGrowthPercentage.Should().Be(-20.000000m);
        }

        [Fact]
        public void Compose_SortOrderDescendsByGrowth_AndNullsLast()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[]
                {
                    Row("SUPA", "Alpha", 1000m),
                    Row("SUPB", "Beta", 40m),
                    Row("SUPC", "Charlie", 100m)
                },
                new[]
                {
                    Row("SUPA", "Alpha", 800m),
                    Row("SUPB", "Beta", 50m)
                });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            var result = composer.Compose(period, GeneratedAt);

            result.Principals.Select(row => row.SupplierId)
                .Should().Equal("SUPA", "SUPB", "SUPC");
            result.Principals[0].SortOrder.Should().Be(1);
            result.Principals[2].YoyMtdGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_ReturnsNull_WhenPriorYearMtdIsNotGreaterThanZero()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[]
                {
                    Row("ZERO", "Zero", 40m),
                    Row("NEG", "Negative", 10m),
                    Row("MISS", "Missing", 100m)
                },
                new[]
                {
                    Row("ZERO", "Zero", 0m),
                    Row("NEG", "Negative", -5m)
                });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            var result = composer.Compose(period, GeneratedAt);

            result.Principals.Single(row => row.SupplierId == "ZERO").YoyMtdGrowthPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").YoyMtdGrowthPercentage.Should().BeNull();
            var missing = result.Principals.Single(row => row.SupplierId == "MISS");
            missing.CurrentYearMtdSalesOutAmount.Should().Be(100m);
            missing.PriorYearMtdSalesOutAmount.Should().BeNull();
            missing.YoyMtdGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_SourcesBothWindowsViaRangeEvidenceDal_UsingEquivalentElapsedDays()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[] { Row("SUPA", "Alpha", 1000m) },
                new[] { Row("SUPA", "Alpha", 800m) });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            composer.Compose(period, GeneratedAt);

            dal.RequestedRanges.Should().HaveCount(2);
            dal.RequestedRanges[0].Item1.Should().Be(new DateTime(2026, 9, 1));
            dal.RequestedRanges[0].Item2.Should().Be(new DateTime(2026, 9, 15));
            // Prior-year window uses equivalent elapsed days (Sep 1..Sep 15),
            // not the full prior-year month (which would end Sep 30).
            dal.RequestedRanges[1].Item1.Should().Be(new DateTime(2025, 9, 1));
            dal.RequestedRanges[1].Item2.Should().Be(new DateTime(2025, 9, 15));
            dal.RequestedRanges[1].Item2.Should().NotBe(new DateTime(2025, 9, 30));
        }

        [Fact]
        public void Compose_ClampsPriorYearWindowEnd_ToLastValidPriorYearDay()
        {
            // 2028-02-29 (leap): prior-year window 2027-02-01..2027-02-28.
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2028, 2, 29), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[] { Row("SUPA", "Alpha", 1000m) },
                new[] { Row("SUPA", "Alpha", 800m) });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            composer.Compose(period, GeneratedAt);

            dal.RequestedRanges[0].Item1.Should().Be(new DateTime(2028, 2, 1));
            dal.RequestedRanges[0].Item2.Should().Be(new DateTime(2028, 2, 29));
            dal.RequestedRanges[1].Item1.Should().Be(new DateTime(2027, 2, 1));
            dal.RequestedRanges[1].Item2.Should().Be(new DateTime(2027, 2, 28));
        }

        [Fact]
        public void Compose_NullPeriod_ReturnsEmptyResult()
        {
            var dal = new RecordingRangeEvidenceDal(
                new[] { Row("SUPA", "Alpha", 1000m) },
                new[] { Row("SUPA", "Alpha", 800m) });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            var result = composer.Compose(null, GeneratedAt);

            result.YoyMtdGrowthKpiId.Should().Be("PRN-GRW-003");
            result.PeriodYear.Should().Be(0);
            result.Principals.Should().BeEmpty();
            dal.RequestedRanges.Should().BeEmpty();
        }

        [Fact]
        public void Calculate_CoversPositiveNegativeAndZeroBaseCases()
        {
            PrincipalYoyMtdGrowthComposer.Calculate(1000m, 800m).Should().Be(25.000000m);
            PrincipalYoyMtdGrowthComposer.Calculate(40m, 50m).Should().Be(-20.000000m);
            PrincipalYoyMtdGrowthComposer.Calculate(1000m, 1000m).Should().Be(0.000000m);
            PrincipalYoyMtdGrowthComposer.Calculate(40m, 0m).Should().BeNull();
            PrincipalYoyMtdGrowthComposer.Calculate(40m, -5m).Should().BeNull();
            PrincipalYoyMtdGrowthComposer.Calculate(null, 800m).Should().BeNull();
            PrincipalYoyMtdGrowthComposer.Calculate(100m, null).Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotPopulateMonthGrainYoyGrowthKpi()
        {
            var period = AnalyticsPeriodCalculator.Create(new DateTime(2026, 9, 15), GeneratedAt);
            var dal = new RecordingRangeEvidenceDal(
                new[] { Row("SUPA", "Alpha", 1000m) },
                new[] { Row("SUPA", "Alpha", 800m) });
            var composer = new PrincipalYoyMtdGrowthComposer(dal);

            var result = composer.Compose(period, GeneratedAt);

            PrincipalKpiCatalog.YoyGrowthId.Should().Be("PRN-GRW-002");
            result.YoyMtdGrowthKpiId.Should().Be(PrincipalKpiCatalog.YoyMtdGrowthId);
            result.YoyMtdGrowthKpiId.Should().NotBe(PrincipalKpiCatalog.YoyGrowthId);
        }

        private static PrincipalSalesOutRangeEvidenceRow Row(
            string supplierId,
            string supplierName,
            decimal salesOutAmount)
        {
            return new PrincipalSalesOutRangeEvidenceRow
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount,
                LineCount = 1
            };
        }

        private sealed class RecordingRangeEvidenceDal : IPrincipalSalesOutRangeEvidenceDal
        {
            private readonly IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> _currentRows;
            private readonly IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> _priorRows;

            public RecordingRangeEvidenceDal(
                IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> currentRows,
                IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> priorRows)
            {
                _currentRows = currentRows;
                _priorRows = priorRows;
            }

            public List<Tuple<DateTime, DateTime>> RequestedRanges { get; }
                = new List<Tuple<DateTime, DateTime>>();

            public IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> ListSalesOutByRange(
                DateTime startDate,
                DateTime endDate)
            {
                RequestedRanges.Add(Tuple.Create(startDate, endDate));
                return RequestedRanges.Count == 1 ? _currentRows : _priorRows;
            }
        }
    }
}
