using System;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Contracts;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Queries;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardAlertCenterAgg
{
    public class DashboardAlertCenterDal : IDashboardAlertCenterDal
    {
        private readonly IDashboardSalesSnapshotDal _salesSnapshotDal;
        private readonly IDashboardPiutangSnapshotDal _piutangSnapshotDal;
        private readonly IDashboardInventorySnapshotDal _inventorySnapshotDal;
        private readonly IDashboardPurchasingSnapshotDal _purchasingSnapshotDal;
        private readonly IDashboardPurchasingManagementSnapshotDal _purchasingManagementSnapshotDal;
        private readonly IDashboardCustomerSnapshotDal _customerSnapshotDal;
        private readonly IDashboardSalesmanSnapshotDal _salesmanSnapshotDal;
        private readonly IDashboardCollectionSnapshotDal _collectionSnapshotDal;
        private readonly IDashboardInventoryRiskSnapshotDal _inventoryRiskSnapshotDal;
        private readonly IDashboardLocationSnapshotDal _locationSnapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly DashboardAlertCenterComposer _composer;
        private readonly DashboardSnapshotOptions _options;

        public DashboardAlertCenterDal(
            IDashboardSalesSnapshotDal salesSnapshotDal,
            IDashboardPiutangSnapshotDal piutangSnapshotDal,
            IDashboardInventorySnapshotDal inventorySnapshotDal,
            IDashboardPurchasingSnapshotDal purchasingSnapshotDal,
            IDashboardPurchasingManagementSnapshotDal purchasingManagementSnapshotDal,
            IDashboardCustomerSnapshotDal customerSnapshotDal,
            IDashboardSalesmanSnapshotDal salesmanSnapshotDal,
            IDashboardCollectionSnapshotDal collectionSnapshotDal,
            IDashboardInventoryRiskSnapshotDal inventoryRiskSnapshotDal,
            IDashboardLocationSnapshotDal locationSnapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            DashboardAlertCenterComposer composer,
            IOptions<DashboardSnapshotOptions> options)
        {
            _salesSnapshotDal = salesSnapshotDal;
            _piutangSnapshotDal = piutangSnapshotDal;
            _inventorySnapshotDal = inventorySnapshotDal;
            _purchasingSnapshotDal = purchasingSnapshotDal;
            _purchasingManagementSnapshotDal = purchasingManagementSnapshotDal;
            _customerSnapshotDal = customerSnapshotDal;
            _salesmanSnapshotDal = salesmanSnapshotDal;
            _collectionSnapshotDal = collectionSnapshotDal;
            _inventoryRiskSnapshotDal = inventoryRiskSnapshotDal;
            _locationSnapshotDal = locationSnapshotDal;
            _refreshLogDal = refreshLogDal;
            _composer = composer;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardAlertCenterResponse GetAlerts()
        {
            return _composer.Compose(new AlertCenterComposeInput
            {
                Sales = _salesSnapshotDal.GetCurrent(),
                Piutang = _piutangSnapshotDal.GetCurrent(),
                Inventory = _inventorySnapshotDal.GetCurrent(),
                Purchasing = _purchasingSnapshotDal.GetCurrent(),
                PurchasingManagement = _purchasingManagementSnapshotDal.GetCurrent(),
                Customer = _customerSnapshotDal.GetCurrent(),
                Salesman = _salesmanSnapshotDal.GetCurrent(),
                Collection = _collectionSnapshotDal.GetCurrent(),
                InventoryRisk = _inventoryRiskSnapshotDal.GetCurrent(),
                Location = _locationSnapshotDal.GetCurrent(),
                RefreshStatuses = _refreshLogDal.GetLatestPerDomain(),
                Options = _options,
                UtcNow = DateTime.UtcNow
            });
        }
    }
}
