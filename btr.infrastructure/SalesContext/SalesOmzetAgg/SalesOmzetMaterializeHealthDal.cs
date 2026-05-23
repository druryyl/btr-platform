using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetMaterializeHealthDal : ISalesOmzetMaterializeHealthDal
    {
        private readonly DatabaseOptions _opt;

        public SalesOmzetMaterializeHealthDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public SalesOmzetMaterializeHealth GetHealth(Periode window)
        {
            if (window is null)
                throw new ArgumentNullException(nameof(window));

            const string sql = @"
            SELECT
                (SELECT COUNT(*)
                 FROM BTR_Order o
                 WHERE o.OrderDate BETWEEN @OrderTgl1 AND @OrderTgl2
                   AND NOT EXISTS (
                       SELECT 1 FROM BTR_SalesOmzet s
                       WHERE s.OrderId = o.OrderId AND s.OrderId <> '')) AS MissingOrders,

                (SELECT COUNT(*)
                 FROM BTR_Faktur f
                 WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
                   AND f.VoidDate = '3000-01-01'
                   AND (f.OrderId IS NULL OR f.OrderId = '')
                   AND NOT EXISTS (
                       SELECT 1 FROM BTR_SalesOmzet s
                       WHERE s.FakturId = f.FakturId AND s.FakturId <> '')) AS MissingDirectFakturs,

                (SELECT COUNT(*)
                 FROM BTR_Faktur f
                 WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
                   AND f.VoidDate = '3000-01-01'
                   AND f.OrderId <> ''
                   AND EXISTS (
                       SELECT 1 FROM BTR_SalesOmzet s
                       WHERE s.OrderId = f.OrderId AND s.OrderId <> '')
                   AND NOT EXISTS (
                       SELECT 1 FROM BTR_SalesOmzet s
                       WHERE s.FakturId = f.FakturId AND s.FakturId <> '')) AS UnlinkedFakturs,

                (SELECT COUNT(*)
                 FROM BTR_SalesOmzet
                 WHERE SalesDate BETWEEN @Tgl1 AND @Tgl2
                    OR OmzetDate BETWEEN @Tgl1 AND @Tgl2
                    OR OrderDate BETWEEN @Tgl1 AND @Tgl2
                    OR FakturDate BETWEEN @Tgl1 AND @Tgl2) AS AggregateRowsInScope,

                (SELECT MAX(LastReconciledAt)
                 FROM BTR_SalesOmzet
                 WHERE SalesDate BETWEEN @Tgl1 AND @Tgl2
                    OR OmzetDate BETWEEN @Tgl1 AND @Tgl2
                    OR OrderDate BETWEEN @Tgl1 AND @Tgl2
                    OR FakturDate BETWEEN @Tgl1 AND @Tgl2) AS LastReconciledMax,

                (SELECT COUNT(*)
                 FROM BTR_Faktur f
                 INNER JOIN BTR_SalesOmzet s ON s.FakturId = f.FakturId AND s.FakturId <> ''
                 WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
                   AND f.VoidDate = '3000-01-01'
                   AND f.LastUpdate > s.LastReconciledAt) AS StaleFakturEstimate";

            var orderTgl1 = window.Tgl1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var orderTgl2 = window.Tgl2.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", window.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", window.Tgl2, SqlDbType.DateTime);
            dp.AddParam("@OrderTgl1", orderTgl1, SqlDbType.VarChar);
            dp.AddParam("@OrderTgl2", orderTgl2, SqlDbType.VarChar);

            HealthRow row;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                row = conn.ReadSingle<HealthRow>(sql, dp);
            }

            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = row?.MissingOrders ?? 0,
                MissingDirectFakturs = row?.MissingDirectFakturs ?? 0,
                UnlinkedFakturs = row?.UnlinkedFakturs ?? 0,
                AggregateRowsInScope = row?.AggregateRowsInScope ?? 0,
                LastReconciledMax = row?.LastReconciledMax,
                StaleFakturEstimate = row?.StaleFakturEstimate ?? 0
            };

            var level = SalesOmzetMaterializeHealthPolicy.Evaluate(metrics, window);

            return new SalesOmzetMaterializeHealth
            {
                Window = window,
                MissingOrders = metrics.MissingOrders,
                MissingDirectFakturs = metrics.MissingDirectFakturs,
                UnlinkedFakturs = metrics.UnlinkedFakturs,
                AggregateRowsInScope = metrics.AggregateRowsInScope,
                LastReconciledMax = metrics.LastReconciledMax,
                StaleFakturEstimate = metrics.StaleFakturEstimate,
                Level = level
            };
        }

        private sealed class HealthRow
        {
            public int MissingOrders { get; set; }
            public int MissingDirectFakturs { get; set; }
            public int UnlinkedFakturs { get; set; }
            public int AggregateRowsInScope { get; set; }
            public DateTime? LastReconciledMax { get; set; }
            public int StaleFakturEstimate { get; set; }
        }
    }
}
