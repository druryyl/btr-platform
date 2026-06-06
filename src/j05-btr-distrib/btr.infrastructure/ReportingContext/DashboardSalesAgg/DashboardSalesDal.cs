using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.DashboardSalesAgg
{
    public class DashboardSalesDal : IDashboardSalesDal
    {
        private readonly ISalesOmzetDal _salesOmzetDal;
        private readonly ISalesOmzetChartSummaryBuilder _chartSummaryBuilder;
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly ITglJamDal _tglJamDal;

        public DashboardSalesDal(
            ISalesOmzetDal salesOmzetDal,
            ISalesOmzetChartSummaryBuilder chartSummaryBuilder,
            ISalesOmzetTargetDal targetDal,
            ITglJamDal tglJamDal)
        {
            _salesOmzetDal = salesOmzetDal;
            _chartSummaryBuilder = chartSummaryBuilder;
            _targetDal = targetDal;
            _tglJamDal = tglJamDal;
        }

        public DashboardSalesResponse GetSummary()
        {
            var today = _tglJamDal.Now.Date;
            var periode = CurrentMonthPeriode();
            const SalesOmzetPeriodFilterMode mode = SalesOmzetPeriodFilterMode.OmzetPeriod;

            var rows = _salesOmzetDal.ListData(periode, mode)?.ToList()
                       ?? new System.Collections.Generic.List<SalesOmzetView>();

            var summary = _chartSummaryBuilder.Build(rows, periode, mode);

            var totalTarget = _targetDal.SumTargetAmountForMonth(today.Year, today.Month);
            var totalAchievement = summary.RecognizedOmzet;
            var achievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(
                totalAchievement,
                totalTarget);

            var rankingSlices = _chartSummaryBuilder.BuildManagerComparison(rows, topCount: 10);
            var ranking = rankingSlices
                .Select((slice, index) => new DashboardSalesRankingItem
                {
                    Rank = index + 1,
                    SalesPersonName = slice.SalesPersonName,
                    CompletedOmzet = slice.RecognizedOmzet
                })
                .ToList();

            return new DashboardSalesResponse
            {
                TotalOmzet = summary.RecognizedOmzet,
                CompletedOmzet = summary.RecognizedOmzet,
                PipelineOmzet = summary.PipelineOmzet,
                TotalFaktur = rows.Count(r => !string.IsNullOrWhiteSpace(r.FakturCode)),
                TotalCustomer = rows
                    .Select(ResolveCustomerKey)
                    .Where(key => key.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                GeneratedAt = _tglJamDal.Now,
                WeeklyTrend = summary.ByWeek
                    ?.Select(w => new DashboardSalesWeekTrendItem
                    {
                        WeekStart = w.WeekStart,
                        WeekEnd = w.WeekEnd,
                        WeekLabel = w.WeekLabel,
                        RecognizedAmount = w.RecognizedAmount
                    })
                    .ToList()
                    ?? new System.Collections.Generic.List<DashboardSalesWeekTrendItem>(),
                TotalTarget = totalTarget,
                TotalAchievement = totalAchievement,
                AchievementPercent = achievementPercent,
                TargetVsAchievement = new DashboardSalesTargetVsAchievement
                {
                    TargetAmount = totalTarget,
                    AchievementAmount = totalAchievement
                },
                TopSalesmanRanking = ranking
            };
        }

        private Periode CurrentMonthPeriode()
        {
            var today = _tglJamDal.Now.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }

        private static string ResolveCustomerKey(SalesOmzetView row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.Code))
                return row.Code.Trim();

            return row.CustomerName?.Trim() ?? string.Empty;
        }
    }
}
