using System.Collections.Generic;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class ItemReplayDataBundle
    {
        public IList<StokBalanceView> StokBalanceRows { get; set; } = new List<StokBalanceView>();

        public IList<BrgLastFakturDto> LastFakturRows { get; set; } = new List<BrgLastFakturDto>();

        public IList<BrgConsumptionDto> ConsumptionRows { get; set; } = new List<BrgConsumptionDto>();

        public IList<DailyCompanyConsumptionDto> DailyConsumptionRows { get; set; } = new List<DailyCompanyConsumptionDto>();

        public IList<BrgWarehouseConsumptionDto> WarehouseConsumptionRows { get; set; }
            = new List<BrgWarehouseConsumptionDto>();

        public IList<WarehouseModel> Warehouses { get; set; } = new List<WarehouseModel>();

        public IList<SalesmanMtdItemRollupDto> ItemRollupRows { get; set; } = new List<SalesmanMtdItemRollupDto>();
    }
}
