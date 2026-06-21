using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardCollectionSnapshotDal : IDashboardCollectionSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCollectionSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCollectionAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, OverdueExposure, AgingOver90Exposure,
       OverdueConcentrationPercent, CashCollectedMtd, MonthCollections, MonthFakturOmzet,
       RecoveryVsBillingPercent, PaymentMixCashAmount, PaymentMixGiroAmount, PaymentMixAdjustmentAmount,
       PaymentMixCashPercent, PaymentMixGiroPercent, PaymentMixAdjustmentPercent,
       LegacyDebtCount, ChronicOverdueCount, WilayahHotspotCount, LowRecoveryVsBillingCount, LastRefreshLogId
FROM BTRPD_CollectionKpi
WHERE SnapshotKey = @SnapshotKey";

            const string agingSql = @"
SELECT BucketKey, BucketLabel, Amount, SortOrder
FROM BTRPD_CollectionAging
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string attentionSql = @"
SELECT EntityType, EntityId, EntityCode, EntityName, SignalKey, SignalLabel,
       ValueAmount, ValueText, WilayahName, ReportRoute, SortOrder
FROM BTRPD_CollectionAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topCustomerSql = @"
SELECT Rank, CustomerCode, CustomerName, OverdueBalance, PercentOfTotal
FROM BTRPD_CollectionTopOverdueCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topSalesmanSql = @"
SELECT Rank, SalesPersonId, SalesPersonCode, SalesPersonName, OverdueBalance, PercentOfTotal
FROM BTRPD_CollectionTopOverdueSalesman
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topWilayahSql = @"
SELECT Rank, WilayahId, WilayahName, OverdueBalance, PercentOfTotal
FROM BTRPD_CollectionTopOverdueWilayah
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                return new DashboardCollectionAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    OverdueExposure = kpi.OverdueExposure,
                    AgingOver90Exposure = kpi.AgingOver90Exposure,
                    OverdueConcentrationPercent = kpi.OverdueConcentrationPercent,
                    CashCollectedMtd = kpi.CashCollectedMtd,
                    MonthCollections = kpi.MonthCollections,
                    MonthFakturOmzet = kpi.MonthFakturOmzet,
                    RecoveryVsBillingPercent = kpi.RecoveryVsBillingPercent,
                    PaymentMixCashAmount = kpi.PaymentMixCashAmount,
                    PaymentMixGiroAmount = kpi.PaymentMixGiroAmount,
                    PaymentMixAdjustmentAmount = kpi.PaymentMixAdjustmentAmount,
                    PaymentMixCashPercent = kpi.PaymentMixCashPercent,
                    PaymentMixGiroPercent = kpi.PaymentMixGiroPercent,
                    PaymentMixAdjustmentPercent = kpi.PaymentMixAdjustmentPercent,
                    LegacyDebtCount = kpi.LegacyDebtCount,
                    ChronicOverdueCount = kpi.ChronicOverdueCount,
                    WilayahHotspotCount = kpi.WilayahHotspotCount,
                    LowRecoveryVsBillingCount = kpi.LowRecoveryVsBillingCount,
                    GeneratedAt = kpi.GeneratedAt,
                    AgingRiskSummary = conn.Query<AgingRow>(agingSql, new { SnapshotKey })
                        .Select(r => new DashboardCollectionAgingRow
                        {
                            BucketKey = r.BucketKey,
                            BucketLabel = r.BucketLabel,
                            Amount = r.Amount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    AttentionList = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey })
                        .Select(r => new DashboardCollectionAttentionRow
                        {
                            EntityType = r.EntityType,
                            EntityId = r.EntityId,
                            EntityCode = r.EntityCode,
                            EntityName = r.EntityName,
                            SignalKey = r.SignalKey,
                            SignalLabel = r.SignalLabel,
                            ValueAmount = r.ValueAmount,
                            ValueText = r.ValueText,
                            WilayahName = r.WilayahName,
                            ReportRoute = r.ReportRoute,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    TopOverdueCustomers = conn.Query<TopCustomerRow>(topCustomerSql, new { SnapshotKey })
                        .Select(r => new DashboardCollectionTopOverdueCustomerRow
                        {
                            Rank = r.Rank,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            OverdueBalance = r.OverdueBalance,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList(),
                    TopOverdueSalesmen = conn.Query<TopSalesmanRow>(topSalesmanSql, new { SnapshotKey })
                        .Select(r => new DashboardCollectionTopOverdueSalesmanRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            OverdueBalance = r.OverdueBalance,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList(),
                    TopOverdueWilayah = conn.Query<TopWilayahRow>(topWilayahSql, new { SnapshotKey })
                        .Select(r => new DashboardCollectionTopOverdueWilayahRow
                        {
                            Rank = r.Rank,
                            WilayahId = r.WilayahId,
                            WilayahName = r.WilayahName,
                            OverdueBalance = r.OverdueBalance,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList()
                };
            }
        }

        public void ReplaceCurrent(
            DashboardCollectionAggregateResult result,
            DashboardCashFlowForecastAggregateResult forecast,
            string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));
            if (forecast is null)
                throw new System.ArgumentNullException(nameof(forecast));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        ReplaceCurrentCore(conn, transaction, result, forecast, refreshLogId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ReplaceCurrentCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardCollectionAggregateResult result,
            DashboardCashFlowForecastAggregateResult forecast,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_CollectionAging WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CollectionAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CollectionTopOverdueCustomer WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CollectionTopOverdueSalesman WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CollectionTopOverdueWilayah WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTRPD_CollectionKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        OverdueExposure = @OverdueExposure,
        AgingOver90Exposure = @AgingOver90Exposure,
        OverdueConcentrationPercent = @OverdueConcentrationPercent,
        CashCollectedMtd = @CashCollectedMtd,
        MonthCollections = @MonthCollections,
        MonthFakturOmzet = @MonthFakturOmzet,
        RecoveryVsBillingPercent = @RecoveryVsBillingPercent,
        PaymentMixCashAmount = @PaymentMixCashAmount,
        PaymentMixGiroAmount = @PaymentMixGiroAmount,
        PaymentMixAdjustmentAmount = @PaymentMixAdjustmentAmount,
        PaymentMixCashPercent = @PaymentMixCashPercent,
        PaymentMixGiroPercent = @PaymentMixGiroPercent,
        PaymentMixAdjustmentPercent = @PaymentMixAdjustmentPercent,
        LegacyDebtCount = @LegacyDebtCount,
        ChronicOverdueCount = @ChronicOverdueCount,
        WilayahHotspotCount = @WilayahHotspotCount,
        LowRecoveryVsBillingCount = @LowRecoveryVsBillingCount,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, OverdueExposure, AgingOver90Exposure,
        OverdueConcentrationPercent, CashCollectedMtd, MonthCollections, MonthFakturOmzet,
        RecoveryVsBillingPercent, PaymentMixCashAmount, PaymentMixGiroAmount, PaymentMixAdjustmentAmount,
        PaymentMixCashPercent, PaymentMixGiroPercent, PaymentMixAdjustmentPercent,
        LegacyDebtCount, ChronicOverdueCount, WilayahHotspotCount, LowRecoveryVsBillingCount, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @OverdueExposure, @AgingOver90Exposure,
        @OverdueConcentrationPercent, @CashCollectedMtd, @MonthCollections, @MonthFakturOmzet,
        @RecoveryVsBillingPercent, @PaymentMixCashAmount, @PaymentMixGiroAmount, @PaymentMixAdjustmentAmount,
        @PaymentMixCashPercent, @PaymentMixGiroPercent, @PaymentMixAdjustmentPercent,
        @LegacyDebtCount, @ChronicOverdueCount, @WilayahHotspotCount, @LowRecoveryVsBillingCount, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                result.GeneratedAt,
                result.PeriodYear,
                result.PeriodMonth,
                result.OverdueExposure,
                result.AgingOver90Exposure,
                result.OverdueConcentrationPercent,
                result.CashCollectedMtd,
                result.MonthCollections,
                result.MonthFakturOmzet,
                result.RecoveryVsBillingPercent,
                result.PaymentMixCashAmount,
                result.PaymentMixGiroAmount,
                result.PaymentMixAdjustmentAmount,
                result.PaymentMixCashPercent,
                result.PaymentMixGiroPercent,
                result.PaymentMixAdjustmentPercent,
                result.LegacyDebtCount,
                result.ChronicOverdueCount,
                result.WilayahHotspotCount,
                result.LowRecoveryVsBillingCount,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertAgingSql = @"
INSERT INTO BTRPD_CollectionAging (
    CollectionAgingId, SnapshotKey, BucketKey, BucketLabel, Amount, SortOrder)
VALUES (
    @CollectionAgingId, @SnapshotKey, @BucketKey, @BucketLabel, @Amount, @SortOrder)";

            foreach (var row in result.AgingRiskSummary ?? new List<DashboardCollectionAgingRow>())
            {
                conn.Execute(insertAgingSql, new
                {
                    CollectionAgingId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.BucketKey,
                    BucketLabel = row.BucketLabel ?? string.Empty,
                    row.Amount,
                    row.SortOrder
                }, transaction);
            }

            const string insertAttentionSql = @"
INSERT INTO BTRPD_CollectionAttention (
    CollectionAttentionId, SnapshotKey, EntityType, EntityId, EntityCode, EntityName,
    SignalKey, SignalLabel, ValueAmount, ValueText, WilayahName, ReportRoute, SortOrder)
VALUES (
    @CollectionAttentionId, @SnapshotKey, @EntityType, @EntityId, @EntityCode, @EntityName,
    @SignalKey, @SignalLabel, @ValueAmount, @ValueText, @WilayahName, @ReportRoute, @SortOrder)";

            foreach (var row in result.AttentionList ?? new List<DashboardCollectionAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    CollectionAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    EntityType = row.EntityType ?? string.Empty,
                    EntityId = row.EntityId ?? string.Empty,
                    EntityCode = row.EntityCode ?? string.Empty,
                    EntityName = row.EntityName ?? string.Empty,
                    row.SignalKey,
                    row.SignalLabel,
                    row.ValueAmount,
                    ValueText = row.ValueText ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    ReportRoute = row.ReportRoute,
                    row.SortOrder
                }, transaction);
            }

            const string insertTopCustomerSql = @"
INSERT INTO BTRPD_CollectionTopOverdueCustomer (
    CollectionTopOverdueCustomerId, SnapshotKey, Rank, CustomerCode, CustomerName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueCustomerId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueCustomers ?? new List<DashboardCollectionTopOverdueCustomerRow>())
            {
                conn.Execute(insertTopCustomerSql, new
                {
                    CollectionTopOverdueCustomerId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    row.OverdueBalance,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertTopSalesmanSql = @"
INSERT INTO BTRPD_CollectionTopOverdueSalesman (
    CollectionTopOverdueSalesmanId, SnapshotKey, Rank, SalesPersonId, SalesPersonCode, SalesPersonName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueSalesmanId, @SnapshotKey, @Rank, @SalesPersonId, @SalesPersonCode, @SalesPersonName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueSalesmen ?? new List<DashboardCollectionTopOverdueSalesmanRow>())
            {
                conn.Execute(insertTopSalesmanSql, new
                {
                    CollectionTopOverdueSalesmanId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    SalesPersonId = row.SalesPersonId ?? string.Empty,
                    SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.OverdueBalance,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertTopWilayahSql = @"
INSERT INTO BTRPD_CollectionTopOverdueWilayah (
    CollectionTopOverdueWilayahId, SnapshotKey, Rank, WilayahId, WilayahName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueWilayahId, @SnapshotKey, @Rank, @WilayahId, @WilayahName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueWilayah ?? new List<DashboardCollectionTopOverdueWilayahRow>())
            {
                conn.Execute(insertTopWilayahSql, new
                {
                    CollectionTopOverdueWilayahId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WilayahId = row.WilayahId ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    row.OverdueBalance,
                    row.PercentOfTotal
                }, transaction);
            }

            conn.Execute(
                "DELETE FROM BTRPD_CashFlowDailyPace WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CashFlowRecoveryTrend WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_CashFlowCollectionRisk WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeForecastKpiSql = @"
MERGE BTRPD_CashFlowForecastKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        BusinessDate = @BusinessDate,
        DaysInMonth = @DaysInMonth,
        DaysElapsed = @DaysElapsed,
        DaysRemaining = @DaysRemaining,
        CashCollectedMtd = @CashCollectedMtd,
        MonthCollections = @MonthCollections,
        MonthFakturOmzet = @MonthFakturOmzet,
        DailyCashCollectionAverage = @DailyCashCollectionAverage,
        DailyCollectionAverage = @DailyCollectionAverage,
        ExpectedCashCollection = @ExpectedCashCollection,
        ProjectedMonthEndTotalCollections = @ProjectedMonthEndTotalCollections,
        CollectionForecastPercent = @CollectionForecastPercent,
        RecoveryVsBillingPercent = @RecoveryVsBillingPercent,
        RecoveryVsBillingForecastPercent = @RecoveryVsBillingForecastPercent,
        RemainingCollectionTarget = @RemainingCollectionTarget,
        RequiredDailyCollection = @RequiredDailyCollection,
        OutstandingDueRemaining = @OutstandingDueRemaining,
        OverdueOutstanding = @OverdueOutstanding,
        CollectionGap = @CollectionGap,
        ForecastVarianceCash = @ForecastVarianceCash,
        ExpectedCollectionRatePercent = @ExpectedCollectionRatePercent,
        BestCaseCash = @BestCaseCash,
        WorstCaseCash = @WorstCaseCash,
        ForecastConfidence = @ForecastConfidence,
        ForecastRiskBand = @ForecastRiskBand,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, BusinessDate,
        DaysInMonth, DaysElapsed, DaysRemaining, CashCollectedMtd, MonthCollections,
        MonthFakturOmzet, DailyCashCollectionAverage, DailyCollectionAverage,
        ExpectedCashCollection, ProjectedMonthEndTotalCollections, CollectionForecastPercent,
        RecoveryVsBillingPercent, RecoveryVsBillingForecastPercent, RemainingCollectionTarget,
        RequiredDailyCollection, OutstandingDueRemaining, OverdueOutstanding, CollectionGap,
        ForecastVarianceCash, ExpectedCollectionRatePercent, BestCaseCash, WorstCaseCash,
        ForecastConfidence, ForecastRiskBand, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @BusinessDate,
        @DaysInMonth, @DaysElapsed, @DaysRemaining, @CashCollectedMtd, @MonthCollections,
        @MonthFakturOmzet, @DailyCashCollectionAverage, @DailyCollectionAverage,
        @ExpectedCashCollection, @ProjectedMonthEndTotalCollections, @CollectionForecastPercent,
        @RecoveryVsBillingPercent, @RecoveryVsBillingForecastPercent, @RemainingCollectionTarget,
        @RequiredDailyCollection, @OutstandingDueRemaining, @OverdueOutstanding, @CollectionGap,
        @ForecastVarianceCash, @ExpectedCollectionRatePercent, @BestCaseCash, @WorstCaseCash,
        @ForecastConfidence, @ForecastRiskBand, @LastRefreshLogId);";

            conn.Execute(mergeForecastKpiSql, new
            {
                SnapshotKey,
                forecast.GeneratedAt,
                forecast.PeriodYear,
                forecast.PeriodMonth,
                forecast.BusinessDate,
                forecast.DaysInMonth,
                forecast.DaysElapsed,
                forecast.DaysRemaining,
                forecast.CashCollectedMtd,
                forecast.MonthCollections,
                forecast.MonthFakturOmzet,
                forecast.DailyCashCollectionAverage,
                forecast.DailyCollectionAverage,
                forecast.ExpectedCashCollection,
                forecast.ProjectedMonthEndTotalCollections,
                forecast.CollectionForecastPercent,
                forecast.RecoveryVsBillingPercent,
                forecast.RecoveryVsBillingForecastPercent,
                forecast.RemainingCollectionTarget,
                forecast.RequiredDailyCollection,
                forecast.OutstandingDueRemaining,
                forecast.OverdueOutstanding,
                forecast.CollectionGap,
                forecast.ForecastVarianceCash,
                forecast.ExpectedCollectionRatePercent,
                forecast.BestCaseCash,
                forecast.WorstCaseCash,
                ForecastConfidence = forecast.ForecastConfidence ?? string.Empty,
                ForecastRiskBand = forecast.ForecastRiskBand ?? string.Empty,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertDailyPaceSql = @"
INSERT INTO BTRPD_CashFlowDailyPace (
    CashFlowDailyPaceId, SnapshotKey, PaceDate, DayOfMonth, IsElapsed,
    ActualCashAmount, ActualCollectionAmount, ProjectedDailyCashAmount)
VALUES (
    @CashFlowDailyPaceId, @SnapshotKey, @PaceDate, @DayOfMonth, @IsElapsed,
    @ActualCashAmount, @ActualCollectionAmount, @ProjectedDailyCashAmount)";

            foreach (var row in forecast.DailyPace ?? new List<DashboardCashFlowDailyPaceRow>())
            {
                conn.Execute(insertDailyPaceSql, new
                {
                    CashFlowDailyPaceId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.PaceDate,
                    row.DayOfMonth,
                    IsElapsed = row.IsElapsed ? 1 : 0,
                    row.ActualCashAmount,
                    row.ActualCollectionAmount,
                    row.ProjectedDailyCashAmount
                }, transaction);
            }

            const string insertRecoveryTrendSql = @"
INSERT INTO BTRPD_CashFlowRecoveryTrend (
    CashFlowRecoveryTrendId, SnapshotKey, TrendDate, DayOfMonth, IsElapsed,
    CumulativeCollections, CumulativeBilling)
VALUES (
    @CashFlowRecoveryTrendId, @SnapshotKey, @TrendDate, @DayOfMonth, @IsElapsed,
    @CumulativeCollections, @CumulativeBilling)";

            foreach (var row in forecast.RecoveryTrend ?? new List<DashboardCashFlowRecoveryTrendRow>())
            {
                conn.Execute(insertRecoveryTrendSql, new
                {
                    CashFlowRecoveryTrendId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    TrendDate = row.TrendDate,
                    row.DayOfMonth,
                    IsElapsed = row.IsElapsed ? 1 : 0,
                    row.CumulativeCollections,
                    row.CumulativeBilling
                }, transaction);
            }

            const string insertCollectionRiskSql = @"
INSERT INTO BTRPD_CashFlowCollectionRisk (
    CashFlowCollectionRiskId, SnapshotKey, SortOrder, RiskKey, RiskLabel,
    EntityType, EntityId, EntityName, Amount, DueOrAgingText, RuleExplanation, ReportRoute)
VALUES (
    @CashFlowCollectionRiskId, @SnapshotKey, @SortOrder, @RiskKey, @RiskLabel,
    @EntityType, @EntityId, @EntityName, @Amount, @DueOrAgingText, @RuleExplanation, @ReportRoute)";

            foreach (var row in forecast.CollectionRisks ?? new List<DashboardCashFlowCollectionRiskRow>())
            {
                conn.Execute(insertCollectionRiskSql, new
                {
                    CashFlowCollectionRiskId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    RiskKey = row.RiskKey ?? string.Empty,
                    RiskLabel = row.RiskLabel ?? string.Empty,
                    EntityType = row.EntityType ?? string.Empty,
                    EntityId = row.EntityId ?? string.Empty,
                    EntityName = row.EntityName ?? string.Empty,
                    row.Amount,
                    DueOrAgingText = row.DueOrAgingText ?? string.Empty,
                    RuleExplanation = row.RuleExplanation ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty
                }, transaction);
            }
        }

        private sealed class KpiRow
        {
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal OverdueExposure { get; set; }
            public decimal AgingOver90Exposure { get; set; }
            public decimal? OverdueConcentrationPercent { get; set; }
            public decimal CashCollectedMtd { get; set; }
            public decimal MonthCollections { get; set; }
            public decimal MonthFakturOmzet { get; set; }
            public decimal? RecoveryVsBillingPercent { get; set; }
            public decimal PaymentMixCashAmount { get; set; }
            public decimal PaymentMixGiroAmount { get; set; }
            public decimal PaymentMixAdjustmentAmount { get; set; }
            public decimal? PaymentMixCashPercent { get; set; }
            public decimal? PaymentMixGiroPercent { get; set; }
            public decimal? PaymentMixAdjustmentPercent { get; set; }
            public int LegacyDebtCount { get; set; }
            public int ChronicOverdueCount { get; set; }
            public int WilayahHotspotCount { get; set; }
            public int LowRecoveryVsBillingCount { get; set; }
            public System.DateTime GeneratedAt { get; set; }
        }

        private sealed class AgingRow
        {
            public string BucketKey { get; set; }
            public string BucketLabel { get; set; }
            public decimal Amount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class AttentionRow
        {
            public string EntityType { get; set; }
            public string EntityId { get; set; }
            public string EntityCode { get; set; }
            public string EntityName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public decimal? ValueAmount { get; set; }
            public string ValueText { get; set; }
            public string WilayahName { get; set; }
            public string ReportRoute { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class TopCustomerRow
        {
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal OverdueBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopSalesmanRow
        {
            public int Rank { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonCode { get; set; }
            public string SalesPersonName { get; set; }
            public decimal OverdueBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopWilayahRow
        {
            public int Rank { get; set; }
            public string WilayahId { get; set; }
            public string WilayahName { get; set; }
            public decimal OverdueBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }
    }
}
