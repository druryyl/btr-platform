using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardCollectionSnapshotDal : IDashboardCollectionSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;
        private readonly INunaCounterBL _counter;

        public DashboardCollectionSnapshotDal(
            IOptions<DatabaseOptions> opt,
            INunaCounterBL counter)
        {
            _opt = opt.Value;
            _counter = counter;
        }

        public DashboardCollectionAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, OverdueExposure, AgingOver90Exposure,
       OverdueConcentrationPercent, CashCollectedMtd, MonthCollections, MonthFakturOmzet,
       RecoveryVsBillingPercent, PaymentMixCashAmount, PaymentMixGiroAmount, PaymentMixAdjustmentAmount,
       PaymentMixCashPercent, PaymentMixGiroPercent, PaymentMixAdjustmentPercent,
       LegacyDebtCount, ChronicOverdueCount, WilayahHotspotCount, LowRecoveryVsBillingCount, LastRefreshLogId
FROM BTR_PortalDashboardCollectionKpi
WHERE SnapshotKey = @SnapshotKey";

            const string agingSql = @"
SELECT BucketKey, BucketLabel, Amount, SortOrder
FROM BTR_PortalDashboardCollectionAging
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string attentionSql = @"
SELECT EntityType, EntityId, EntityCode, EntityName, SignalKey, SignalLabel,
       ValueAmount, ValueText, WilayahName, ReportRoute, SortOrder
FROM BTR_PortalDashboardCollectionAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topCustomerSql = @"
SELECT Rank, CustomerCode, CustomerName, OverdueBalance, PercentOfTotal
FROM BTR_PortalDashboardCollectionTopOverdueCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topSalesmanSql = @"
SELECT Rank, SalesPersonId, SalesPersonCode, SalesPersonName, OverdueBalance, PercentOfTotal
FROM BTR_PortalDashboardCollectionTopOverdueSalesman
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topWilayahSql = @"
SELECT Rank, WilayahId, WilayahName, OverdueBalance, PercentOfTotal
FROM BTR_PortalDashboardCollectionTopOverdueWilayah
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

        public void ReplaceCurrent(DashboardCollectionAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        ReplaceCurrentCore(conn, transaction, result, refreshLogId);
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
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTR_PortalDashboardCollectionAging WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTR_PortalDashboardCollectionAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTR_PortalDashboardCollectionTopOverdueCustomer WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTR_PortalDashboardCollectionTopOverdueSalesman WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTR_PortalDashboardCollectionTopOverdueWilayah WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTR_PortalDashboardCollectionKpi AS target
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
INSERT INTO BTR_PortalDashboardCollectionAging (
    CollectionAgingId, SnapshotKey, BucketKey, BucketLabel, Amount, SortOrder)
VALUES (
    @CollectionAgingId, @SnapshotKey, @BucketKey, @BucketLabel, @Amount, @SortOrder)";

            foreach (var row in result.AgingRiskSummary ?? new List<DashboardCollectionAgingRow>())
            {
                conn.Execute(insertAgingSql, new
                {
                    CollectionAgingId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                    SnapshotKey,
                    row.BucketKey,
                    BucketLabel = row.BucketLabel ?? string.Empty,
                    row.Amount,
                    row.SortOrder
                }, transaction);
            }

            const string insertAttentionSql = @"
INSERT INTO BTR_PortalDashboardCollectionAttention (
    CollectionAttentionId, SnapshotKey, EntityType, EntityId, EntityCode, EntityName,
    SignalKey, SignalLabel, ValueAmount, ValueText, WilayahName, ReportRoute, SortOrder)
VALUES (
    @CollectionAttentionId, @SnapshotKey, @EntityType, @EntityId, @EntityCode, @EntityName,
    @SignalKey, @SignalLabel, @ValueAmount, @ValueText, @WilayahName, @ReportRoute, @SortOrder)";

            foreach (var row in result.AttentionList ?? new List<DashboardCollectionAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    CollectionAttentionId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
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
INSERT INTO BTR_PortalDashboardCollectionTopOverdueCustomer (
    CollectionTopOverdueCustomerId, SnapshotKey, Rank, CustomerCode, CustomerName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueCustomerId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueCustomers ?? new List<DashboardCollectionTopOverdueCustomerRow>())
            {
                conn.Execute(insertTopCustomerSql, new
                {
                    CollectionTopOverdueCustomerId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                    SnapshotKey,
                    row.Rank,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    row.OverdueBalance,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertTopSalesmanSql = @"
INSERT INTO BTR_PortalDashboardCollectionTopOverdueSalesman (
    CollectionTopOverdueSalesmanId, SnapshotKey, Rank, SalesPersonId, SalesPersonCode, SalesPersonName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueSalesmanId, @SnapshotKey, @Rank, @SalesPersonId, @SalesPersonCode, @SalesPersonName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueSalesmen ?? new List<DashboardCollectionTopOverdueSalesmanRow>())
            {
                conn.Execute(insertTopSalesmanSql, new
                {
                    CollectionTopOverdueSalesmanId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
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
INSERT INTO BTR_PortalDashboardCollectionTopOverdueWilayah (
    CollectionTopOverdueWilayahId, SnapshotKey, Rank, WilayahId, WilayahName, OverdueBalance, PercentOfTotal)
VALUES (
    @CollectionTopOverdueWilayahId, @SnapshotKey, @Rank, @WilayahId, @WilayahName, @OverdueBalance, @PercentOfTotal)";

            foreach (var row in result.TopOverdueWilayah ?? new List<DashboardCollectionTopOverdueWilayahRow>())
            {
                conn.Execute(insertTopWilayahSql, new
                {
                    CollectionTopOverdueWilayahId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                    SnapshotKey,
                    row.Rank,
                    WilayahId = row.WilayahId ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    row.OverdueBalance,
                    row.PercentOfTotal
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
