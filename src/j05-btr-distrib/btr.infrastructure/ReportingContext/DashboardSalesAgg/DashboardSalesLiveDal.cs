using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.DashboardSalesAgg
{
    public class DashboardSalesLiveDal
    {
        private readonly IFakturViewDal _fakturViewDal;
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly DashboardSalesFakturAggregator _aggregator;

        public DashboardSalesLiveDal(
            IFakturViewDal fakturViewDal,
            ISalesOmzetTargetDal targetDal,
            ITglJamDal tglJamDal,
            DashboardSalesFakturAggregator aggregator)
        {
            _fakturViewDal = fakturViewDal;
            _targetDal = targetDal;
            _tglJamDal = tglJamDal;
            _aggregator = aggregator;
        }

        public DashboardSalesResponse GetSummary()
        {
            var today = _tglJamDal.Now.Date;
            var periode = CurrentMonthPeriode(today);
            var rows = _fakturViewDal.ListData(periode)?.ToList()
                       ?? new List<FakturView>();
            var totalTarget = _targetDal.SumTargetAmountForMonth(today.Year, today.Month);
            var aggregate = _aggregator.Aggregate(rows, periode, totalTarget, _tglJamDal.Now);
            return MapToResponse(aggregate);
        }

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }

        private static DashboardSalesResponse MapToResponse(DashboardSalesAggregateResult aggregate)
        {
            return new DashboardSalesResponse
            {
                TotalOmzet = aggregate.TotalOmzet,
                CompletedOmzet = aggregate.CompletedOmzet,
                PipelineOmzet = aggregate.PipelineOmzet,
                TotalFaktur = aggregate.TotalFaktur,
                TotalCustomer = aggregate.TotalCustomer,
                GeneratedAt = aggregate.GeneratedAt,
                WeeklyTrend = (aggregate.WeekTrend ?? new List<DashboardSalesWeekTrendRow>())
                    .Select(w => new DashboardSalesWeekTrendItem
                    {
                        WeekStart = w.WeekStart,
                        WeekEnd = w.WeekEnd,
                        WeekLabel = w.WeekLabel,
                        RecognizedAmount = w.RecognizedAmount
                    })
                    .ToList(),
                TotalTarget = aggregate.TotalTarget,
                TotalAchievement = aggregate.TotalAchievement,
                AchievementPercent = aggregate.AchievementPercent,
                TargetVsAchievement = new DashboardSalesTargetVsAchievement
                {
                    TargetAmount = aggregate.TotalTarget,
                    AchievementAmount = aggregate.TotalAchievement
                },
                TopSalesmanRanking = (aggregate.TopSalesman ?? new List<DashboardSalesTopSalesmanRow>())
                    .OrderBy(r => r.Rank)
                    .Select(r => new DashboardSalesRankingItem
                    {
                        Rank = r.Rank,
                        SalesPersonId = r.SalesPersonId,
                        SalesPersonName = r.SalesPersonName,
                        CompletedOmzet = r.CompletedOmzet,
                        Investigation = InvestigationMetadataBuilder.Build(
                            InvestigationRegistry.SignalLegacyTopSalesman,
                            InvestigationMetadataBuilder.EntityTypeSalesman,
                            r.SalesPersonId,
                            r.SalesPersonName)
                    })
                    .ToList()
            };
        }
    }
}
