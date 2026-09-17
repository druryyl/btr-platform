using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalInventorySnapshotDal : IPrincipalInventorySnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalInventoryKpi, BTRPD_PrincipalInventory";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalInventory WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalInventoryKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        InventoryValueKpiId = @InventoryValueKpiId,
        InventoryDaysKpiId = @InventoryDaysKpiId,
        BusinessDate = @BusinessDate,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, InventoryValueKpiId, InventoryDaysKpiId, BusinessDate, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @InventoryValueKpiId, @InventoryDaysKpiId, @BusinessDate, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalInventory (
    PrincipalInventoryId, SnapshotKey, InventoryValueKpiId, InventoryDaysKpiId,
    SupplierId, SupplierName, InventoryValue, InventoryDays, ItemCount, SortOrder,
    BusinessDate, GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalInventoryId, @SnapshotKey, @InventoryValueKpiId, @InventoryDaysKpiId,
    @SupplierId, @SupplierName, @InventoryValue, @InventoryDays, @ItemCount, @SortOrder,
    @BusinessDate, @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalInventorySnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalInventoryAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, InventoryValueKpiId, InventoryDaysKpiId, BusinessDate, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalInventoryKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT InventoryValueKpiId, InventoryDaysKpiId, SupplierId, SupplierName,
       InventoryValue, InventoryDays, ItemCount, SortOrder
FROM BTRPD_PrincipalInventory
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalInventorySnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalInventoryRow>(principalSql, new
                {
                    SnapshotKey = PrincipalInventorySnapshot.SnapshotKey
                }).ToList();

                return new PrincipalInventoryAggregateResult
                {
                    InventoryValueKpiId = kpi.InventoryValueKpiId,
                    InventoryDaysKpiId = kpi.InventoryDaysKpiId,
                    BusinessDate = kpi.BusinessDate,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalInventoryAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var inventoryValueKpiId = PrincipalKpiCatalog.InventoryValueId;
            var inventoryDaysKpiId = PrincipalKpiCatalog.InventoryDaysId;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalInventorySnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalInventorySnapshot.SnapshotKey,
                        InventoryValueKpiId = inventoryValueKpiId,
                        InventoryDaysKpiId = inventoryDaysKpiId,
                        result.BusinessDate,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalInventoryRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalInventoryId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalInventorySnapshot.SnapshotKey,
                            InventoryValueKpiId = inventoryValueKpiId,
                            InventoryDaysKpiId = inventoryDaysKpiId,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.InventoryValue,
                            row.InventoryDays,
                            row.ItemCount,
                            row.SortOrder,
                            result.BusinessDate,
                            result.GeneratedAt,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private sealed class KpiRow
        {
            public string InventoryValueKpiId { get; set; }

            public string InventoryDaysKpiId { get; set; }

            public DateTime BusinessDate { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
