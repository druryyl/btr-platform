using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardCollectionAgg.Contracts;
using btr.application.ReportingContext.DashboardCollectionAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCollectionAgg
{
    public class DashboardCollectionDal : IDashboardCollectionDal
    {
        private const string PiutangReportRoute = "/reports/piutang";

        private readonly IDashboardCollectionSnapshotDal _snapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardCollectionDal(
            IDashboardCollectionSnapshotDal snapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardCollectionResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot == null)
            {
                return new DashboardCollectionResponse
                {
                    IsAvailable = false,
                    IsDataFresh = false,
                    Navigation = BuildNavigation()
                };
            }

            return MapToResponse(snapshot);
        }

        private DashboardCollectionResponse MapToResponse(DashboardCollectionAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.CollectionIntervalMinutes, utcNow);

            var exposureRequiresAttention = snapshot.OverdueExposure > 0 || snapshot.AgingOver90Exposure > 0;
            var recoveryRequiresAttention = snapshot.MonthFakturOmzet > 0 &&
                (snapshot.RecoveryVsBillingPercent.GetValueOrDefault(100m) < 100m ||
                 snapshot.MonthCollections == 0);
            var portfolioRequiresAttention = snapshot.LegacyDebtCount > 0;

            return new DashboardCollectionResponse
            {
                IsAvailable = true,
                IsDataFresh = isFresh,
                GeneratedAt = snapshot.GeneratedAt,
                AttentionCards = new DashboardCollectionAttentionCards
                {
                    OverdueExposure = snapshot.OverdueExposure,
                    AgingOver90Exposure = snapshot.AgingOver90Exposure,
                    OverdueConcentrationPercent = snapshot.OverdueConcentrationPercent,
                    ExposureRequiresAttention = exposureRequiresAttention,
                    CashCollectedMtd = snapshot.CashCollectedMtd,
                    RecoveryVsBillingPercent = snapshot.RecoveryVsBillingPercent,
                    RecoveryRequiresAttention = recoveryRequiresAttention,
                    LegacyDebtCount = snapshot.LegacyDebtCount,
                    PortfolioRequiresAttention = portfolioRequiresAttention
                },
                RecoverySummary = new DashboardCollectionRecoverySummary
                {
                    CashCollectedMtd = snapshot.CashCollectedMtd,
                    RecoveryVsBillingPercent = snapshot.RecoveryVsBillingPercent,
                    PaymentMixCashAmount = snapshot.PaymentMixCashAmount,
                    PaymentMixGiroAmount = snapshot.PaymentMixGiroAmount,
                    PaymentMixAdjustmentAmount = snapshot.PaymentMixAdjustmentAmount,
                    PaymentMixCashPercent = snapshot.PaymentMixCashPercent,
                    PaymentMixGiroPercent = snapshot.PaymentMixGiroPercent,
                    PaymentMixAdjustmentPercent = snapshot.PaymentMixAdjustmentPercent
                },
                AgingRiskSummary = snapshot.AgingRiskSummary?
                    .Select(r => new DashboardCollectionAgingBucket
                    {
                        BucketKey = r.BucketKey,
                        BucketLabel = r.BucketLabel,
                        Amount = r.Amount,
                        SortOrder = r.SortOrder
                    })
                    .ToList() ?? new List<DashboardCollectionAgingBucket>(),
                AttentionList = snapshot.AttentionList?
                    .Select(MapAttentionItem)
                    .ToList() ?? new List<DashboardCollectionAttentionItem>(),
                TopOverdueCustomers = snapshot.TopOverdueCustomers?
                    .Select(r => new DashboardCollectionRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.CustomerCode,
                        EntityName = r.CustomerName,
                        Amount = r.OverdueBalance,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = PiutangReportRoute
                    })
                    .ToList() ?? new List<DashboardCollectionRankingRow>(),
                TopOverdueSalesmen = snapshot.TopOverdueSalesmen?
                    .Select(r => new DashboardCollectionRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.SalesPersonCode,
                        EntityName = r.SalesPersonName,
                        Amount = r.OverdueBalance,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = PiutangReportRoute
                    })
                    .ToList() ?? new List<DashboardCollectionRankingRow>(),
                TopOverdueWilayah = snapshot.TopOverdueWilayah?
                    .Select(r => new DashboardCollectionRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WilayahId,
                        EntityName = r.WilayahName,
                        Amount = r.OverdueBalance,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = null
                    })
                    .ToList() ?? new List<DashboardCollectionRankingRow>(),
                Navigation = BuildNavigation()
            };
        }

        private static DashboardCollectionAttentionItem MapAttentionItem(DashboardCollectionAttentionRow row)
        {
            return new DashboardCollectionAttentionItem
            {
                EntityType = row.EntityType,
                EntityCode = row.EntityCode,
                EntityName = row.EntityName,
                SignalKey = row.SignalKey,
                SignalLabel = row.SignalLabel,
                ValueAmount = row.ValueAmount,
                ValueText = row.ValueText,
                WilayahName = row.WilayahName,
                ReportRoute = row.ReportRoute
            };
        }

        private static DashboardCollectionNavigationLinks BuildNavigation()
        {
            return new DashboardCollectionNavigationLinks
            {
                PiutangDashboardRoute = "/dashboard/piutang",
                CustomerDashboardRoute = "/dashboard/customers",
                SalesmanDashboardRoute = "/dashboard/salesmen",
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
