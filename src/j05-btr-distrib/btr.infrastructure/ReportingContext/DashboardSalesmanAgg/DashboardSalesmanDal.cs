using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesmanAgg.Queries;
using btr.application.ReportingContext.Shared;
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

        private static string BuildSalesmanProfileRoute(string salesPersonId)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
                return null;

            return $"/analytics/salesmen/{Uri.EscapeDataString(salesPersonId.Trim())}";
        }

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
                    Navigation = BuildNavigation(),
                    Filters = BuildFilters()
                };
            }

            return MapToResponse(snapshot);
        }

        public SalesmanPrincipalAchievementResponse GetPrincipalAchievement(string salesPersonId)
        {
            var snapshot = _snapshotDal.GetCurrent();
            var rows = _snapshotDal.ListPrincipalAchievement(salesPersonId) ?? new List<DashboardSalesmanPrincipalAchievementRow>();

            var salesPersonName = rows.FirstOrDefault()?.SalesPersonName ?? string.Empty;

            return new SalesmanPrincipalAchievementResponse
            {
                SalesPersonId = salesPersonId,
                SalesPersonName = salesPersonName,
                PeriodYear = snapshot?.PeriodYear ?? 0,
                PeriodMonth = snapshot?.PeriodMonth ?? 0,
                Principals = rows.Select(r => new SalesmanPrincipalAchievementRow
                {
                    SupplierId = r.SupplierId,
                    SupplierName = r.SupplierName,
                    TargetAmount = r.TargetAmount,
                    CompletedOmzet = r.CompletedOmzet,
                    AchievementPercent = r.AchievementPercent,
                    AchievementBand = ExecutiveSalesAchievementBandResolver.Resolve(r.AchievementPercent)
                }).ToList()
            };
        }

        public SalesmanAchievementTrendResponse GetAchievementTrend(string salesPersonId, int months)
        {
            var rows = _snapshotDal.ListRepHistory(salesPersonId, months) ?? new List<DashboardSalesmanRepHistoryRow>();
            var salesPersonName = rows.LastOrDefault()?.SalesPersonName ?? string.Empty;

            return new SalesmanAchievementTrendResponse
            {
                SalesPersonId = salesPersonId,
                SalesPersonName = salesPersonName,
                Points = rows.Select(r => new SalesmanAchievementTrendPoint
                {
                    PeriodYear = r.PeriodYear,
                    PeriodMonth = r.PeriodMonth,
                    PeriodLabel = FormatPeriodLabel(r.PeriodYear, r.PeriodMonth),
                    TargetAmount = r.TargetAmount,
                    CompletedOmzet = r.CompletedOmzet,
                    AchievementPercent = r.AchievementPercent,
                    AchievementBand = r.AchievementBand
                }).ToList()
            };
        }

        private DashboardSalesmanResponse MapToResponse(DashboardSalesmanAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.SalesmanIntervalMinutes, utcNow);

            var belowTarget = snapshot.BelowTargetCount;
            var missingTargetSetup = snapshot.MissingTargetSetupCount;
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
                Filters = BuildFilters(),
                AttentionCards = new DashboardSalesmanAttentionCards
                {
                    BelowTargetCount = belowTarget,
                    MissingTargetSetupCount = missingTargetSetup,
                    HighOverdueExposureCount = highOverdue,
                    HighPiutangExposureCount = highPiutang,
                    CustomerConcentrationCount = concentration,
                    DormantPortfolioCount = dormant,
                    TopOmzetSalesmanPercent = snapshot.TopOmzetSalesmanPercent,
                    TopPiutangSalesmanPercent = snapshot.TopPiutangSalesmanPercent,
                    PerformanceRequiresAttention = belowTarget > 0 || missingTargetSetup > 0,
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
                            IsActive = r.IsActive,
                            ReportRoute = SalesReportRoute,
                            ProfileRoute = BuildSalesmanProfileRoute(r.SalesPersonId),
                            Investigation = InvestigationMetadataBuilder.Build(
                                InvestigationRegistry.SignalRankingSalesmanTopOmzet,
                                InvestigationMetadataBuilder.EntityTypeSalesman,
                                r.SalesPersonId,
                                r.SalesPersonName)
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
                            IsActive = r.IsActive,
                            ReportRoute = SalesReportRoute,
                            ProfileRoute = BuildSalesmanProfileRoute(r.SalesPersonId),
                            Investigation = InvestigationMetadataBuilder.Build(
                                InvestigationRegistry.SignalRankingSalesmanTopAchievement,
                                InvestigationMetadataBuilder.EntityTypeSalesman,
                                r.SalesPersonId,
                                r.SalesPersonName)
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
                            IsActive = r.IsActive,
                            ReportRoute = PiutangReportRoute,
                            ProfileRoute = BuildSalesmanProfileRoute(r.SalesPersonId),
                            Investigation = InvestigationMetadataBuilder.Build(
                                InvestigationRegistry.SignalRankingSalesmanTopPiutang,
                                InvestigationMetadataBuilder.EntityTypeSalesman,
                                r.SalesPersonId,
                                r.SalesPersonName)
                        })
                        .ToList() ?? new List<DashboardSalesmanRankingRow>()
                },
                Segmentation = MapSegmentation(snapshot.Segmentation),
                Navigation = BuildNavigation()
            };
        }

        private DashboardSalesmanFilterDefaults BuildFilters()
        {
            return new DashboardSalesmanFilterDefaults
            {
                DefaultActiveOnly = true,
                ExposureTopPercent = _options.SalesmanExposureTopPercent
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
                IsActive = row.IsActive,
                ReportRoute = ResolveReportRoute(row.SignalKey),
                ProfileRoute = BuildSalesmanProfileRoute(row.SalesPersonId),
                RequiresAttention = true,
                Investigation = InvestigationMetadataBuilder.Build(
                    row.SignalKey,
                    InvestigationMetadataBuilder.EntityTypeSalesman,
                    row.SalesPersonId,
                    row.SalesPersonName,
                    signalLabelOverride: row.SignalLabel,
                    reportRouteOverride: ResolveReportRoute(row.SignalKey))
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
                case DashboardSalesmanAggregator.SignalMissingTargetSetup:
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

        private static string FormatPeriodLabel(int year, int month)
        {
            return new DateTime(year, month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
        }

        private static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }
    }
}
