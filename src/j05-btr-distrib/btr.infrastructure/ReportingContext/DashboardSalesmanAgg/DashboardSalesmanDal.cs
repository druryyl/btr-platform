using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesmanAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSalesmanAgg
{
    public class DashboardSalesmanDal : IDashboardSalesmanDal
    {
        private const string SalesReportRoute = "/reports/sales";
        private const string PiutangReportRoute = "/reports/piutang";

        private readonly IDashboardSalesmanSnapshotDal _snapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardSalesmanDal(
            IDashboardSalesmanSnapshotDal snapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardSalesmanResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot == null)
            {
                return new DashboardSalesmanResponse
                {
                    IsAvailable = false,
                    IsDataFresh = false,
                    Navigation = BuildNavigation()
                };
            }

            return MapToResponse(snapshot);
        }

        private DashboardSalesmanResponse MapToResponse(DashboardSalesmanAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.SalesmanIntervalMinutes, utcNow);

            var belowTarget = snapshot.BelowTargetCount;
            var noTarget = snapshot.NoTargetCount;
            var highOverdue = snapshot.HighOverdueExposureCount;
            var highPiutang = snapshot.HighPiutangExposureCount;
            var concentration = snapshot.CustomerConcentrationCount;
            var dormant = snapshot.DormantPortfolioCount;

            return new DashboardSalesmanResponse
            {
                IsAvailable = true,
                IsDataFresh = isFresh,
                GeneratedAt = snapshot.GeneratedAt,
                PeriodYear = snapshot.PeriodYear,
                PeriodMonth = snapshot.PeriodMonth,
                AttentionCards = new DashboardSalesmanAttentionCards
                {
                    BelowTargetCount = belowTarget,
                    NoTargetCount = noTarget,
                    HighOverdueExposureCount = highOverdue,
                    HighPiutangExposureCount = highPiutang,
                    CustomerConcentrationCount = concentration,
                    DormantPortfolioCount = dormant,
                    TopOmzetSalesmanPercent = snapshot.TopOmzetSalesmanPercent,
                    TopPiutangSalesmanPercent = snapshot.TopPiutangSalesmanPercent,
                    PerformanceRequiresAttention = belowTarget > 0 || noTarget > 0,
                    CollectionRequiresAttention = highOverdue > 0 || highPiutang > 0,
                    PortfolioRequiresAttention = dormant > 0 || concentration > 0
                },
                AttentionList = snapshot.AttentionList?
                    .Select(MapAttentionItem)
                    .ToList() ?? new List<DashboardSalesmanAttentionItem>(),
                PerformanceRankings = new DashboardSalesmanPerformanceRankings
                {
                    TopOmzet = snapshot.TopOmzet?
                        .Select(r => new DashboardSalesmanRankingRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            Amount = r.CompletedOmzet,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = SalesReportRoute
                        })
                        .ToList() ?? new List<DashboardSalesmanRankingRow>(),
                    TopAchievement = snapshot.TopAchievement?
                        .Select(r => new DashboardSalesmanRankingRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            Amount = r.CompletedOmzet,
                            PercentOfTotal = r.PercentOfTotal,
                            AchievementPercent = r.AchievementPercent,
                            TargetAmount = r.TargetAmount,
                            ReportRoute = SalesReportRoute
                        })
                        .ToList() ?? new List<DashboardSalesmanRankingRow>()
                },
                ExposureRankings = new DashboardSalesmanExposureRankings
                {
                    TopPiutang = snapshot.TopPiutang?
                        .Select(r => new DashboardSalesmanRankingRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            Amount = r.OutstandingBalance,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = PiutangReportRoute
                        })
                        .ToList() ?? new List<DashboardSalesmanRankingRow>()
                },
                Segmentation = MapSegmentation(snapshot.Segmentation),
                Navigation = BuildNavigation()
            };
        }

        private static DashboardSalesmanAttentionItem MapAttentionItem(DashboardSalesmanAttentionRow row)
        {
            return new DashboardSalesmanAttentionItem
            {
                SalesPersonId = row.SalesPersonId,
                SalesPersonCode = row.SalesPersonCode,
                SalesPersonName = row.SalesPersonName,
                SignalKey = row.SignalKey,
                SignalLabel = row.SignalLabel,
                ValueAmount = row.ValueAmount,
                ValueText = row.ValueText,
                WilayahName = row.WilayahName,
                ReportRoute = ResolveReportRoute(row.SignalKey),
                RequiresAttention = true
            };
        }

        private static string ResolveReportRoute(string signalKey)
        {
            switch (signalKey)
            {
                case DashboardSalesmanAggregator.SignalHighOverdueExposure:
                case DashboardSalesmanAggregator.SignalHighPiutangExposure:
                    return PiutangReportRoute;
                case DashboardSalesmanAggregator.SignalBelowTarget:
                case DashboardSalesmanAggregator.SignalNoTarget:
                case DashboardSalesmanAggregator.SignalCustomerConcentration:
                case DashboardSalesmanAggregator.SignalDormantCustomerPortfolio:
                    return SalesReportRoute;
                default:
                    return SalesReportRoute;
            }
        }

        private static DashboardSalesmanSegmentationSummary MapSegmentation(
            IList<DashboardSalesmanSegmentationRow> rows)
        {
            var list = rows ?? new List<DashboardSalesmanSegmentationRow>();

            return new DashboardSalesmanSegmentationSummary
            {
                ByWilayah = list
                    .Where(r => string.Equals(r.SegmentType, "Wilayah", StringComparison.OrdinalIgnoreCase))
                    .Select(MapSegmentRow)
                    .ToList(),
                BySegment = list
                    .Where(r => string.Equals(r.SegmentType, "Segment", StringComparison.OrdinalIgnoreCase))
                    .Select(MapSegmentRow)
                    .ToList(),
                ActiveSummary = MapSegmentRow(list.FirstOrDefault(r =>
                    string.Equals(r.SegmentType, "Activity", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.SegmentKey, "Active", StringComparison.OrdinalIgnoreCase))),
                InactiveSummary = MapSegmentRow(list.FirstOrDefault(r =>
                    string.Equals(r.SegmentType, "Activity", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.SegmentKey, "Inactive", StringComparison.OrdinalIgnoreCase)))
            };
        }

        private static DashboardSalesmanSegmentRow MapSegmentRow(DashboardSalesmanSegmentationRow row)
        {
            if (row is null)
                return null;

            return new DashboardSalesmanSegmentRow
            {
                SegmentType = row.SegmentType,
                SegmentLabel = row.SegmentLabel,
                SalesmanCount = row.SalesmanCount,
                ActiveCount = row.ActiveCount,
                InactiveCount = row.InactiveCount
            };
        }

        private static DashboardSalesmanNavigationLinks BuildNavigation()
        {
            return new DashboardSalesmanNavigationLinks
            {
                SalesDashboardRoute = "/dashboard/sales",
                PiutangDashboardRoute = "/dashboard/piutang",
                SalesReportRoute = SalesReportRoute,
                PiutangReportRoute = PiutangReportRoute
            };
        }

        private static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }
    }
}
