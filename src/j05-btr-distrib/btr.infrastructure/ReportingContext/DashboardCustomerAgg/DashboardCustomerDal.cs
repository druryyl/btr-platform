using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerAgg.Contracts;
using btr.application.ReportingContext.DashboardCustomerAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCustomerAgg
{
    public class DashboardCustomerDal : IDashboardCustomerDal
    {
        private const string SalesReportRoute = "/reports/sales";
        private const string PiutangReportRoute = "/reports/piutang";

        private readonly IDashboardCustomerSnapshotDal _snapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardCustomerDal(
            IDashboardCustomerSnapshotDal snapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardCustomerResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot == null)
            {
                return new DashboardCustomerResponse
                {
                    IsAvailable = false,
                    IsDataFresh = false,
                    Navigation = BuildNavigation()
                };
            }

            return MapToResponse(snapshot);
        }

        private DashboardCustomerResponse MapToResponse(DashboardCustomerAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.CustomerIntervalMinutes, utcNow);

            var overdueCount = snapshot.OverdueCustomerCount;
            var agingOver90 = snapshot.AgingOver90Amount;
            var dormantCount = snapshot.DormantCustomerCount;
            var plafondBreachCount = snapshot.PlafondBreachCount;
            var suspendedCount = snapshot.SuspendedWithSalesCount;

            return new DashboardCustomerResponse
            {
                IsAvailable = true,
                IsDataFresh = isFresh,
                GeneratedAt = snapshot.GeneratedAt,
                PeriodYear = snapshot.PeriodYear,
                PeriodMonth = snapshot.PeriodMonth,
                AttentionCards = new DashboardCustomerAttentionCards
                {
                    OverdueCustomerCount = overdueCount,
                    AgingOver90Amount = agingOver90,
                    CollectionRequiresAttention = overdueCount > 0 || agingOver90 > 0,
                    TopOmzetCustomerPercent = snapshot.TopOmzetCustomerPercent,
                    TopPiutangCustomerPercent = snapshot.TopPiutangCustomerPercent,
                    ActiveCustomerCount = snapshot.ActiveCustomerCount,
                    DormantCustomerCount = dormantCount,
                    InactivityRequiresAttention = dormantCount > 0,
                    PlafondBreachCount = plafondBreachCount,
                    SuspendedWithSalesCount = suspendedCount,
                    CreditRequiresAttention = plafondBreachCount > 0 || suspendedCount > 0
                },
                AttentionList = snapshot.AttentionList?
                    .Select(MapAttentionItem)
                    .ToList() ?? new List<DashboardCustomerAttentionItem>(),
                Rankings = new DashboardCustomerRankings
                {
                    TopOmzet = snapshot.TopOmzet?
                        .Select(r => new DashboardCustomerRankingRow
                        {
                            Rank = r.Rank,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            Amount = r.OmzetAmount,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = SalesReportRoute,
                            Investigation = InvestigationMetadataBuilder.Build(
                                InvestigationRegistry.SignalRankingCustomerTopOmzet,
                                InvestigationMetadataBuilder.EntityTypeCustomer,
                                r.CustomerCode,
                                r.CustomerName)
                        })
                        .ToList() ?? new List<DashboardCustomerRankingRow>(),
                    TopPiutang = snapshot.TopPiutang?
                        .Select(r => new DashboardCustomerRankingRow
                        {
                            Rank = r.Rank,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            Amount = r.OutstandingBalance,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = PiutangReportRoute,
                            Investigation = InvestigationMetadataBuilder.Build(
                                InvestigationRegistry.SignalRankingCustomerTopPiutang,
                                InvestigationMetadataBuilder.EntityTypeCustomer,
                                r.CustomerCode,
                                r.CustomerName)
                        })
                        .ToList() ?? new List<DashboardCustomerRankingRow>()
                },
                Segmentation = MapSegmentation(snapshot.Segmentation),
                Navigation = BuildNavigation()
            };
        }

        private static DashboardCustomerAttentionItem MapAttentionItem(DashboardCustomerAttentionRow row)
        {
            return new DashboardCustomerAttentionItem
            {
                CustomerCode = row.CustomerCode,
                CustomerName = row.CustomerName,
                SignalKey = row.SignalKey,
                SignalLabel = row.SignalLabel,
                ValueAmount = row.ValueAmount,
                ValueText = row.ValueText,
                WilayahName = row.WilayahName,
                ReportRoute = ResolveReportRoute(row.SignalKey),
                RequiresAttention = true,
                Investigation = InvestigationMetadataBuilder.Build(
                    row.SignalKey,
                    InvestigationMetadataBuilder.EntityTypeCustomer,
                    row.CustomerCode,
                    row.CustomerName,
                    signalLabelOverride: row.SignalLabel,
                    reportRouteOverride: ResolveReportRoute(row.SignalKey))
            };
        }

        private static string ResolveReportRoute(string signalKey)
        {
            switch (signalKey)
            {
                case DashboardCustomerAggregator.SignalOverdue:
                case DashboardCustomerAggregator.SignalPlafondBreach:
                    return PiutangReportRoute;
                case DashboardCustomerAggregator.SignalDormant:
                case DashboardCustomerAggregator.SignalSuspendedWithSales:
                    return SalesReportRoute;
                default:
                    return SalesReportRoute;
            }
        }

        private static DashboardCustomerSegmentationSummary MapSegmentation(
            IList<DashboardCustomerSegmentationRow> rows)
        {
            var list = rows ?? new List<DashboardCustomerSegmentationRow>();

            return new DashboardCustomerSegmentationSummary
            {
                ByKlasifikasi = list
                    .Where(r => string.Equals(r.SegmentType, "Klasifikasi", StringComparison.OrdinalIgnoreCase))
                    .Select(MapSegmentRow)
                    .ToList(),
                ByWilayah = list
                    .Where(r => string.Equals(r.SegmentType, "Wilayah", StringComparison.OrdinalIgnoreCase))
                    .Select(MapSegmentRow)
                    .ToList(),
                ActiveSummary = MapSegmentRow(list.FirstOrDefault(r =>
                    string.Equals(r.SegmentType, "Activity", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.SegmentKey, "Active", StringComparison.OrdinalIgnoreCase))),
                DormantSummary = MapSegmentRow(list.FirstOrDefault(r =>
                    string.Equals(r.SegmentType, "Activity", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.SegmentKey, "Dormant", StringComparison.OrdinalIgnoreCase)))
            };
        }

        private static DashboardCustomerSegmentRow MapSegmentRow(DashboardCustomerSegmentationRow row)
        {
            if (row is null)
                return null;

            return new DashboardCustomerSegmentRow
            {
                SegmentType = row.SegmentType,
                SegmentLabel = row.SegmentLabel,
                CustomerCount = row.CustomerCount,
                ActiveCount = row.ActiveCount,
                DormantCount = row.DormantCount
            };
        }

        private static DashboardCustomerNavigationLinks BuildNavigation()
        {
            return new DashboardCustomerNavigationLinks
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
