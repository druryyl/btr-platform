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
    public class DashboardLocationSnapshotDal : IDashboardLocationSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardLocationSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardLocationAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
       Top1WarehouseInventoryPercent, Top3WarehouseInventoryPercent, Top1WarehouseAtRiskPercent,
       Top1WarehouseSalesPercent, Top1WilayahSalesPercent,
       InactiveWarehouseWithStockCount, WarehouseNoSalesWithInventoryCount,
       TotalInventoryValue, TotalAtRiskValue, TotalOmzet, TotalPurchase, LastRefreshLogId
FROM BTRPD_LocationKpi
WHERE SnapshotKey = @SnapshotKey";

            const string topInventorySql = @"
SELECT Rank, WarehouseId, WarehouseName, InventoryValue, PercentOfTotal, ReportRoute
FROM BTRPD_LocationTopWarehouseInventory
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topAtRiskSql = @"
SELECT Rank, WarehouseId, WarehouseName, AtRiskValue, PercentOfTotal, ReportRoute
FROM BTRPD_LocationTopWarehouseAtRisk
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topSalesSql = @"
SELECT Rank, WarehouseId, WarehouseName, MtdOmzet, PercentOfTotal, ReportRoute
FROM BTRPD_LocationTopWarehouseSales
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topPurchasingSql = @"
SELECT Rank, WarehouseId, WarehouseName, MtdPurchaseAmount, PercentOfTotal, ReportRoute
FROM BTRPD_LocationTopWarehousePurchasing
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topWilayahSql = @"
SELECT Rank, WilayahId, WilayahName, MtdOmzet, PercentOfTotal, DashboardRoute
FROM BTRPD_LocationTopWilayahSales
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string attentionSql = @"
SELECT EntityType, EntityCode, EntityName, SignalKey, SignalLabel,
       ValueAmount, ValueText, ReportRoute, SortOrder
FROM BTRPD_LocationAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                return new DashboardLocationAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    Top1WarehouseInventoryPercent = kpi.Top1WarehouseInventoryPercent,
                    Top3WarehouseInventoryPercent = kpi.Top3WarehouseInventoryPercent,
                    Top1WarehouseAtRiskPercent = kpi.Top1WarehouseAtRiskPercent,
                    Top1WarehouseSalesPercent = kpi.Top1WarehouseSalesPercent,
                    Top1WilayahSalesPercent = kpi.Top1WilayahSalesPercent,
                    InactiveWarehouseWithStockCount = kpi.InactiveWarehouseWithStockCount,
                    WarehouseNoSalesWithInventoryCount = kpi.WarehouseNoSalesWithInventoryCount,
                    TotalInventoryValue = kpi.TotalInventoryValue,
                    TotalAtRiskValue = kpi.TotalAtRiskValue,
                    TotalOmzet = kpi.TotalOmzet,
                    TotalPurchase = kpi.TotalPurchase,
                    GeneratedAt = kpi.GeneratedAt,
                    TopWarehouseInventory = conn.Query<TopInventoryRow>(topInventorySql, new { SnapshotKey })
                        .Select(r => new DashboardLocationTopWarehouseInventoryRow
                        {
                            Rank = r.Rank,
                            WarehouseId = r.WarehouseId,
                            WarehouseName = r.WarehouseName,
                            InventoryValue = r.InventoryValue,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = r.ReportRoute
                        }).ToList(),
                    TopWarehouseAtRisk = conn.Query<TopAtRiskRow>(topAtRiskSql, new { SnapshotKey })
                        .Select(r => new DashboardLocationTopWarehouseAtRiskRow
                        {
                            Rank = r.Rank,
                            WarehouseId = r.WarehouseId,
                            WarehouseName = r.WarehouseName,
                            AtRiskValue = r.AtRiskValue,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = r.ReportRoute
                        }).ToList(),
                    TopWarehouseSales = conn.Query<TopSalesRow>(topSalesSql, new { SnapshotKey })
                        .Select(r => new DashboardLocationTopWarehouseSalesRow
                        {
                            Rank = r.Rank,
                            WarehouseId = r.WarehouseId,
                            WarehouseName = r.WarehouseName,
                            MtdOmzet = r.MtdOmzet,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = r.ReportRoute
                        }).ToList(),
                    TopWarehousePurchasing = conn.Query<TopPurchasingRow>(topPurchasingSql, new { SnapshotKey })
                        .Select(r => new DashboardLocationTopWarehousePurchasingRow
                        {
                            Rank = r.Rank,
                            WarehouseId = r.WarehouseId,
                            WarehouseName = r.WarehouseName,
                            MtdPurchaseAmount = r.MtdPurchaseAmount,
                            PercentOfTotal = r.PercentOfTotal,
                            ReportRoute = r.ReportRoute
                        }).ToList(),
                    TopWilayahSales = conn.Query<TopWilayahRow>(topWilayahSql, new { SnapshotKey })
                        .Select(r => new DashboardLocationTopWilayahSalesRow
                        {
                            Rank = r.Rank,
                            WilayahId = r.WilayahId,
                            WilayahName = r.WilayahName,
                            MtdOmzet = r.MtdOmzet,
                            PercentOfTotal = r.PercentOfTotal,
                            DashboardRoute = r.DashboardRoute
                        }).ToList(),
                    AttentionList = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey })
                        .Select(r => new DashboardLocationAttentionRow
                        {
                            EntityType = r.EntityType,
                            EntityCode = r.EntityCode,
                            EntityName = r.EntityName,
                            SignalKey = r.SignalKey,
                            SignalLabel = r.SignalLabel,
                            ValueAmount = r.ValueAmount,
                            ValueText = r.ValueText,
                            ReportRoute = r.ReportRoute,
                            SortOrder = r.SortOrder
                        }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardLocationAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

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
            DashboardLocationAggregateResult result,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_LocationAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_LocationTopWarehouseInventory WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_LocationTopWarehouseAtRisk WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_LocationTopWarehouseSales WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_LocationTopWarehousePurchasing WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_LocationTopWilayahSales WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTRPD_LocationKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        Top1WarehouseInventoryPercent = @Top1WarehouseInventoryPercent,
        Top3WarehouseInventoryPercent = @Top3WarehouseInventoryPercent,
        Top1WarehouseAtRiskPercent = @Top1WarehouseAtRiskPercent,
        Top1WarehouseSalesPercent = @Top1WarehouseSalesPercent,
        Top1WilayahSalesPercent = @Top1WilayahSalesPercent,
        InactiveWarehouseWithStockCount = @InactiveWarehouseWithStockCount,
        WarehouseNoSalesWithInventoryCount = @WarehouseNoSalesWithInventoryCount,
        TotalInventoryValue = @TotalInventoryValue,
        TotalAtRiskValue = @TotalAtRiskValue,
        TotalOmzet = @TotalOmzet,
        TotalPurchase = @TotalPurchase,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
        Top1WarehouseInventoryPercent, Top3WarehouseInventoryPercent, Top1WarehouseAtRiskPercent,
        Top1WarehouseSalesPercent, Top1WilayahSalesPercent,
        InactiveWarehouseWithStockCount, WarehouseNoSalesWithInventoryCount,
        TotalInventoryValue, TotalAtRiskValue, TotalOmzet, TotalPurchase, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth,
        @Top1WarehouseInventoryPercent, @Top3WarehouseInventoryPercent, @Top1WarehouseAtRiskPercent,
        @Top1WarehouseSalesPercent, @Top1WilayahSalesPercent,
        @InactiveWarehouseWithStockCount, @WarehouseNoSalesWithInventoryCount,
        @TotalInventoryValue, @TotalAtRiskValue, @TotalOmzet, @TotalPurchase, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                result.GeneratedAt,
                result.PeriodYear,
                result.PeriodMonth,
                result.Top1WarehouseInventoryPercent,
                result.Top3WarehouseInventoryPercent,
                result.Top1WarehouseAtRiskPercent,
                result.Top1WarehouseSalesPercent,
                result.Top1WilayahSalesPercent,
                result.InactiveWarehouseWithStockCount,
                result.WarehouseNoSalesWithInventoryCount,
                result.TotalInventoryValue,
                result.TotalAtRiskValue,
                result.TotalOmzet,
                result.TotalPurchase,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            InsertTopInventory(conn, transaction, result.TopWarehouseInventory);
            InsertTopAtRisk(conn, transaction, result.TopWarehouseAtRisk);
            InsertTopSales(conn, transaction, result.TopWarehouseSales);
            InsertTopPurchasing(conn, transaction, result.TopWarehousePurchasing);
            InsertTopWilayah(conn, transaction, result.TopWilayahSales);
            InsertAttention(conn, transaction, result.AttentionList);
        }

        private void InsertTopInventory(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationTopWarehouseInventoryRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationTopWarehouseInventory (
    LocationTopWarehouseInventoryId, SnapshotKey, Rank, WarehouseId, WarehouseName,
    InventoryValue, PercentOfTotal, ReportRoute)
VALUES (
    @LocationTopWarehouseInventoryId, @SnapshotKey, @Rank, @WarehouseId, @WarehouseName,
    @InventoryValue, @PercentOfTotal, @ReportRoute)";

            foreach (var row in rows ?? new List<DashboardLocationTopWarehouseInventoryRow>())
            {
                conn.Execute(sql, new
                {
                    LocationTopWarehouseInventoryId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WarehouseId = row.WarehouseId ?? string.Empty,
                    WarehouseName = row.WarehouseName ?? string.Empty,
                    row.InventoryValue,
                    row.PercentOfTotal,
                    ReportRoute = row.ReportRoute
                }, transaction);
            }
        }

        private void InsertTopAtRisk(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationTopWarehouseAtRiskRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationTopWarehouseAtRisk (
    LocationTopWarehouseAtRiskId, SnapshotKey, Rank, WarehouseId, WarehouseName,
    AtRiskValue, PercentOfTotal, ReportRoute)
VALUES (
    @LocationTopWarehouseAtRiskId, @SnapshotKey, @Rank, @WarehouseId, @WarehouseName,
    @AtRiskValue, @PercentOfTotal, @ReportRoute)";

            foreach (var row in rows ?? new List<DashboardLocationTopWarehouseAtRiskRow>())
            {
                conn.Execute(sql, new
                {
                    LocationTopWarehouseAtRiskId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WarehouseId = row.WarehouseId ?? string.Empty,
                    WarehouseName = row.WarehouseName ?? string.Empty,
                    row.AtRiskValue,
                    row.PercentOfTotal,
                    ReportRoute = row.ReportRoute
                }, transaction);
            }
        }

        private void InsertTopSales(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationTopWarehouseSalesRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationTopWarehouseSales (
    LocationTopWarehouseSalesId, SnapshotKey, Rank, WarehouseId, WarehouseName,
    MtdOmzet, PercentOfTotal, ReportRoute)
VALUES (
    @LocationTopWarehouseSalesId, @SnapshotKey, @Rank, @WarehouseId, @WarehouseName,
    @MtdOmzet, @PercentOfTotal, @ReportRoute)";

            foreach (var row in rows ?? new List<DashboardLocationTopWarehouseSalesRow>())
            {
                conn.Execute(sql, new
                {
                    LocationTopWarehouseSalesId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WarehouseId = row.WarehouseId ?? string.Empty,
                    WarehouseName = row.WarehouseName ?? string.Empty,
                    row.MtdOmzet,
                    row.PercentOfTotal,
                    ReportRoute = row.ReportRoute
                }, transaction);
            }
        }

        private void InsertTopPurchasing(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationTopWarehousePurchasingRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationTopWarehousePurchasing (
    LocationTopWarehousePurchasingId, SnapshotKey, Rank, WarehouseId, WarehouseName,
    MtdPurchaseAmount, PercentOfTotal, ReportRoute)
VALUES (
    @LocationTopWarehousePurchasingId, @SnapshotKey, @Rank, @WarehouseId, @WarehouseName,
    @MtdPurchaseAmount, @PercentOfTotal, @ReportRoute)";

            foreach (var row in rows ?? new List<DashboardLocationTopWarehousePurchasingRow>())
            {
                conn.Execute(sql, new
                {
                    LocationTopWarehousePurchasingId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WarehouseId = row.WarehouseId ?? string.Empty,
                    WarehouseName = row.WarehouseName ?? string.Empty,
                    row.MtdPurchaseAmount,
                    row.PercentOfTotal,
                    ReportRoute = row.ReportRoute
                }, transaction);
            }
        }

        private void InsertTopWilayah(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationTopWilayahSalesRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationTopWilayahSales (
    LocationTopWilayahSalesId, SnapshotKey, Rank, WilayahId, WilayahName,
    MtdOmzet, PercentOfTotal, DashboardRoute)
VALUES (
    @LocationTopWilayahSalesId, @SnapshotKey, @Rank, @WilayahId, @WilayahName,
    @MtdOmzet, @PercentOfTotal, @DashboardRoute)";

            foreach (var row in rows ?? new List<DashboardLocationTopWilayahSalesRow>())
            {
                conn.Execute(sql, new
                {
                    LocationTopWilayahSalesId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    WilayahId = row.WilayahId,
                    WilayahName = row.WilayahName ?? string.Empty,
                    row.MtdOmzet,
                    row.PercentOfTotal,
                    DashboardRoute = row.DashboardRoute
                }, transaction);
            }
        }

        private void InsertAttention(
            SqlConnection conn,
            SqlTransaction transaction,
            IEnumerable<DashboardLocationAttentionRow> rows)
        {
            const string sql = @"
INSERT INTO BTRPD_LocationAttention (
    LocationAttentionId, SnapshotKey, EntityType, EntityCode, EntityName,
    SignalKey, SignalLabel, ValueAmount, ValueText, ReportRoute, SortOrder)
VALUES (
    @LocationAttentionId, @SnapshotKey, @EntityType, @EntityCode, @EntityName,
    @SignalKey, @SignalLabel, @ValueAmount, @ValueText, @ReportRoute, @SortOrder)";

            foreach (var row in rows ?? new List<DashboardLocationAttentionRow>())
            {
                conn.Execute(sql, new
                {
                    LocationAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    EntityType = row.EntityType ?? string.Empty,
                    EntityCode = row.EntityCode,
                    EntityName = row.EntityName ?? string.Empty,
                    row.SignalKey,
                    row.SignalLabel,
                    row.ValueAmount,
                    ValueText = row.ValueText,
                    ReportRoute = row.ReportRoute,
                    row.SortOrder
                }, transaction);
            }
        }

        private sealed class KpiRow
        {
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal? Top1WarehouseInventoryPercent { get; set; }
            public decimal? Top3WarehouseInventoryPercent { get; set; }
            public decimal? Top1WarehouseAtRiskPercent { get; set; }
            public decimal? Top1WarehouseSalesPercent { get; set; }
            public decimal? Top1WilayahSalesPercent { get; set; }
            public int InactiveWarehouseWithStockCount { get; set; }
            public int WarehouseNoSalesWithInventoryCount { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public decimal TotalAtRiskValue { get; set; }
            public decimal TotalOmzet { get; set; }
            public decimal TotalPurchase { get; set; }
            public DateTime GeneratedAt { get; set; }
        }

        private sealed class TopInventoryRow
        {
            public int Rank { get; set; }
            public string WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public decimal InventoryValue { get; set; }
            public decimal? PercentOfTotal { get; set; }
            public string ReportRoute { get; set; }
        }

        private sealed class TopAtRiskRow
        {
            public int Rank { get; set; }
            public string WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public decimal AtRiskValue { get; set; }
            public decimal? PercentOfTotal { get; set; }
            public string ReportRoute { get; set; }
        }

        private sealed class TopSalesRow
        {
            public int Rank { get; set; }
            public string WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public decimal MtdOmzet { get; set; }
            public decimal? PercentOfTotal { get; set; }
            public string ReportRoute { get; set; }
        }

        private sealed class TopPurchasingRow
        {
            public int Rank { get; set; }
            public string WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public decimal MtdPurchaseAmount { get; set; }
            public decimal? PercentOfTotal { get; set; }
            public string ReportRoute { get; set; }
        }

        private sealed class TopWilayahRow
        {
            public int Rank { get; set; }
            public string WilayahId { get; set; }
            public string WilayahName { get; set; }
            public decimal MtdOmzet { get; set; }
            public decimal? PercentOfTotal { get; set; }
            public string DashboardRoute { get; set; }
        }

        private sealed class AttentionRow
        {
            public string EntityType { get; set; }
            public string EntityCode { get; set; }
            public string EntityName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public decimal? ValueAmount { get; set; }
            public string ValueText { get; set; }
            public string ReportRoute { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
