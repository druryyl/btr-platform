using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Contracts;
using btr.application.ReportingContext.DashboardExecutiveAgg.Queries;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardExecutiveAgg
{
    public class DashboardExecutiveDal : IDashboardExecutiveDal
    {
        private readonly IDashboardSalesSnapshotDal _salesSnapshotDal;
        private readonly IDashboardPiutangSnapshotDal _piutangSnapshotDal;
        private readonly IDashboardInventorySnapshotDal _inventorySnapshotDal;
        private readonly IDashboardPurchasingSnapshotDal _purchasingSnapshotDal;
        private readonly IDashboardPurchasingManagementSnapshotDal _purchasingManagementSnapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly DashboardExecutiveComposer _composer;
        private readonly DashboardSnapshotOptions _options;

        public DashboardExecutiveDal(
            IDashboardSalesSnapshotDal salesSnapshotDal,
            IDashboardPiutangSnapshotDal piutangSnapshotDal,
            IDashboardInventorySnapshotDal inventorySnapshotDal,
            IDashboardPurchasingSnapshotDal purchasingSnapshotDal,
            IDashboardPurchasingManagementSnapshotDal purchasingManagementSnapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            DashboardExecutiveComposer composer,
            IOptions<DashboardSnapshotOptions> options)
        {
            _salesSnapshotDal = salesSnapshotDal;
            _piutangSnapshotDal = piutangSnapshotDal;
            _inventorySnapshotDal = inventorySnapshotDal;
            _purchasingSnapshotDal = purchasingSnapshotDal;
            _purchasingManagementSnapshotDal = purchasingManagementSnapshotDal;
            _refreshLogDal = refreshLogDal;
            _composer = composer;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardExecutiveResponse GetExecutive()
        {
            return _composer.Compose(new ExecutiveComposeInput
            {
                Sales = _salesSnapshotDal.GetCurrent(),
                Piutang = _piutangSnapshotDal.GetCurrent(),
                Inventory = _inventorySnapshotDal.GetCurrent(),
                Purchasing = _purchasingSnapshotDal.GetCurrent(),
                PurchasingManagement = _purchasingManagementSnapshotDal.GetCurrent(),
                RefreshStatuses = _refreshLogDal.GetLatestPerDomain(),
                Options = _options,
                UtcNow = DateTime.UtcNow
            });
        }
    }
}
