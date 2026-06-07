using System.Data.SqlClient;
using btr.application.ReportingContext.DashboardOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardOverviewAgg.Queries;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardOverviewDal : IDashboardOverviewDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardOverviewDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardOverviewResponse GetOverview()
        {
            const string salesSql = @"
SELECT TotalOmzet, TotalFaktur, TotalCustomer, GeneratedAt
FROM BTR_PortalDashboardSalesKpi
WHERE SnapshotKey = @SnapshotKey";

            const string piutangSql = @"
SELECT TotalPiutang, TotalCustomer, GeneratedAt
FROM BTR_PortalDashboardPiutangKpi
WHERE SnapshotKey = @SnapshotKey";

            const string inventorySql = @"
SELECT TotalInventoryValue, TotalItem, GeneratedAt
FROM BTR_PortalDashboardInventoryKpi
WHERE SnapshotKey = @SnapshotKey";

            const string purchasingSql = @"
SELECT GrandTotalPurchase, TotalInvoice, GeneratedAt
FROM BTR_PortalDashboardPurchasingKpi
WHERE SnapshotKey = @SnapshotKey";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var sales = conn.QueryFirstOrDefault<SalesKpiRow>(salesSql, new { SnapshotKey });
                var piutang = conn.QueryFirstOrDefault<PiutangKpiRow>(piutangSql, new { SnapshotKey });
                var inventory = conn.QueryFirstOrDefault<InventoryKpiRow>(inventorySql, new { SnapshotKey });
                var purchasing = conn.QueryFirstOrDefault<PurchasingKpiRow>(purchasingSql, new { SnapshotKey });

                var hasUnavailable = sales is null || piutang is null || inventory is null || purchasing is null;

                return new DashboardOverviewResponse
                {
                    Sales = sales is null
                        ? null
                        : new DashboardOverviewSalesSection
                        {
                            TotalOmzet = sales.TotalOmzet,
                            TotalFaktur = sales.TotalFaktur,
                            TotalCustomer = sales.TotalCustomer,
                            GeneratedAt = sales.GeneratedAt
                        },
                    Piutang = piutang is null
                        ? null
                        : new DashboardOverviewPiutangSection
                        {
                            TotalPiutang = piutang.TotalPiutang,
                            TotalCustomer = piutang.TotalCustomer,
                            GeneratedAt = piutang.GeneratedAt
                        },
                    Inventory = inventory is null
                        ? null
                        : new DashboardOverviewInventorySection
                        {
                            TotalInventoryValue = inventory.TotalInventoryValue,
                            TotalItem = inventory.TotalItem,
                            GeneratedAt = inventory.GeneratedAt
                        },
                    Purchasing = purchasing is null
                        ? null
                        : new DashboardOverviewPurchasingSection
                        {
                            GrandTotalPurchase = purchasing.GrandTotalPurchase,
                            TotalInvoice = purchasing.TotalInvoice,
                            GeneratedAt = purchasing.GeneratedAt
                        },
                    HasUnavailableDomain = hasUnavailable
                };
            }
        }

        private sealed class SalesKpiRow
        {
            public decimal TotalOmzet { get; set; }

            public int TotalFaktur { get; set; }

            public int TotalCustomer { get; set; }

            public System.DateTime GeneratedAt { get; set; }
        }

        private sealed class PiutangKpiRow
        {
            public decimal TotalPiutang { get; set; }

            public int TotalCustomer { get; set; }

            public System.DateTime GeneratedAt { get; set; }
        }

        private sealed class InventoryKpiRow
        {
            public decimal TotalInventoryValue { get; set; }

            public int TotalItem { get; set; }

            public System.DateTime GeneratedAt { get; set; }
        }

        private sealed class PurchasingKpiRow
        {
            public decimal GrandTotalPurchase { get; set; }

            public int TotalInvoice { get; set; }

            public System.DateTime GeneratedAt { get; set; }
        }
    }
}
