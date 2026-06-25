using System;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class ItemReplayDataLoader : IEntityAnalyticsReplayDataLoader
    {
        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly IBrgLastFakturDal _brgLastFakturDal;
        private readonly IBrgConsumptionDal _brgConsumptionDal;
        private readonly IBrgWarehouseConsumptionDal _brgWarehouseConsumptionDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly ISalesmanMtdItemRollupDal _salesmanMtdItemRollupDal;

        public ItemReplayDataLoader(
            IStokBalanceViewDal stokBalanceViewDal,
            IBrgLastFakturDal brgLastFakturDal,
            IBrgConsumptionDal brgConsumptionDal,
            IBrgWarehouseConsumptionDal brgWarehouseConsumptionDal,
            IWarehouseDal warehouseDal,
            ISalesmanMtdItemRollupDal salesmanMtdItemRollupDal)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _brgLastFakturDal = brgLastFakturDal;
            _brgConsumptionDal = brgConsumptionDal;
            _brgWarehouseConsumptionDal = brgWarehouseConsumptionDal;
            _warehouseDal = warehouseDal;
            _salesmanMtdItemRollupDal = salesmanMtdItemRollupDal;
        }

        public string EntityType => EntityTypeCode.Item;

        public object Load(EntityAnalyticsReplayContext replayContext)
        {
            if (replayContext is null)
                throw new ArgumentNullException(nameof(replayContext));

            var periodEnd = replayContext.PeriodEnd.Date;
            var periode = new Periode(replayContext.PeriodStart, periodEnd);
            var window30Start = periodEnd.AddDays(-(InventoryForecastPolicy.AdcWindow30Days - 1));
            var window90Start = periodEnd.AddDays(-(InventoryForecastPolicy.AdcWindow90Days - 1));
            var stepPrefix = $"Backfill:{EntityType}:{replayContext.PeriodYear:D4}-{replayContext.PeriodMonth:D2}";

            WorkerProgressScope.Current?.StepStarted($"{stepPrefix}:Load", "Load historical item source data");

            var stokRows = _stokBalanceViewDal.ListDataAsOf(periodEnd)?.ToList()
                ?? new System.Collections.Generic.List<StokBalanceView>();

            var bundle = new ItemReplayDataBundle
            {
                StokBalanceRows = stokRows,
                LastFakturRows = _brgLastFakturDal.ListLastFakturByBrgAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<BrgLastFakturDto>(),
                ConsumptionRows = _brgConsumptionDal.ListConsumptionByBrg(window30Start, window90Start, periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<BrgConsumptionDto>(),
                DailyConsumptionRows = _brgConsumptionDal.ListDailyCompanyConsumption(window30Start, periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<DailyCompanyConsumptionDto>(),
                WarehouseConsumptionRows = _brgWarehouseConsumptionDal.ListConsumptionByBrgWarehouse(window30Start, periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<BrgWarehouseConsumptionDto>(),
                Warehouses = _warehouseDal.ListAllForPortal()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.InventoryContext.WarehouseAgg.WarehouseModel>(),
                ItemRollupRows = _salesmanMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SalesmanMtdItemRollupDto>()
            };

            WorkerProgressScope.Current?.StepCompleted($"{stepPrefix}:Load", new WorkerProgressStepInfo
            {
                RecordCount = bundle.StokBalanceRows.Count + bundle.LastFakturRows.Count
            });

            return bundle;
        }
    }
}
