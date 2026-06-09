using System;
using System.Collections.Generic;
using btr.nuna.Application;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshAllDashboardSnapshotsWorker
        : INunaServiceVoid<RefreshAllDashboardSnapshotsRequest>
    {
    }

    public class RefreshAllDashboardSnapshotsWorker : IRefreshAllDashboardSnapshotsWorker
    {
        private readonly IRefreshDashboardPiutangSnapshotWorker _piutangWorker;
        private readonly IRefreshDashboardInventorySnapshotWorker _inventoryWorker;
        private readonly IRefreshDashboardInventoryRiskSnapshotWorker _inventoryRiskWorker;
        private readonly IRefreshDashboardSalesSnapshotWorker _salesWorker;
        private readonly IRefreshDashboardPurchasingSnapshotWorker _purchasingWorker;
        private readonly IRefreshDashboardPurchasingManagementSnapshotWorker _purchasingManagementWorker;
        private readonly IRefreshDashboardCustomerSnapshotWorker _customerWorker;
        private readonly IRefreshDashboardSalesmanSnapshotWorker _salesmanWorker;
        private readonly IRefreshDashboardCollectionSnapshotWorker _collectionWorker;
        private readonly IRefreshDashboardLocationSnapshotWorker _locationWorker;

        public RefreshAllDashboardSnapshotsWorker(
            IRefreshDashboardPiutangSnapshotWorker piutangWorker,
            IRefreshDashboardInventorySnapshotWorker inventoryWorker,
            IRefreshDashboardInventoryRiskSnapshotWorker inventoryRiskWorker,
            IRefreshDashboardSalesSnapshotWorker salesWorker,
            IRefreshDashboardPurchasingSnapshotWorker purchasingWorker,
            IRefreshDashboardPurchasingManagementSnapshotWorker purchasingManagementWorker,
            IRefreshDashboardCustomerSnapshotWorker customerWorker,
            IRefreshDashboardSalesmanSnapshotWorker salesmanWorker,
            IRefreshDashboardCollectionSnapshotWorker collectionWorker,
            IRefreshDashboardLocationSnapshotWorker locationWorker)
        {
            _piutangWorker = piutangWorker;
            _inventoryWorker = inventoryWorker;
            _inventoryRiskWorker = inventoryRiskWorker;
            _salesWorker = salesWorker;
            _purchasingWorker = purchasingWorker;
            _purchasingManagementWorker = purchasingManagementWorker;
            _customerWorker = customerWorker;
            _salesmanWorker = salesmanWorker;
            _collectionWorker = collectionWorker;
            _locationWorker = locationWorker;
        }

        public void Execute(RefreshAllDashboardSnapshotsRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var triggeredBy = request.TriggeredBy ?? "Scheduler";
            var domainResults = new List<RefreshDashboardDomainResult>();
            var failures = new List<Exception>();

            RunDomain(
                "Piutang",
                () =>
                {
                    var piutangRequest = new RefreshDashboardPiutangSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _piutangWorker.Execute(piutangRequest);
                    return piutangRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Inventory",
                () =>
                {
                    var inventoryRequest = new RefreshDashboardInventorySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _inventoryWorker.Execute(inventoryRequest);
                    return inventoryRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "InventoryRisk",
                () =>
                {
                    var inventoryRiskRequest = new RefreshDashboardInventoryRiskSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _inventoryRiskWorker.Execute(inventoryRiskRequest);
                    return inventoryRiskRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Sales",
                () =>
                {
                    var salesRequest = new RefreshDashboardSalesSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _salesWorker.Execute(salesRequest);
                    return salesRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Purchasing",
                () =>
                {
                    var purchasingRequest = new RefreshDashboardPurchasingSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _purchasingWorker.Execute(purchasingRequest);
                    return purchasingRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "PurchasingManagement",
                () =>
                {
                    var purchasingManagementRequest = new RefreshDashboardPurchasingManagementSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _purchasingManagementWorker.Execute(purchasingManagementRequest);
                    return purchasingManagementRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Customer",
                () =>
                {
                    var customerRequest = new RefreshDashboardCustomerSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _customerWorker.Execute(customerRequest);
                    return customerRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Salesman",
                () =>
                {
                    var salesmanRequest = new RefreshDashboardSalesmanSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _salesmanWorker.Execute(salesmanRequest);
                    return salesmanRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Collection",
                () =>
                {
                    var collectionRequest = new RefreshDashboardCollectionSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _collectionWorker.Execute(collectionRequest);
                    return collectionRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                "Location",
                () =>
                {
                    var locationRequest = new RefreshDashboardLocationSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _locationWorker.Execute(locationRequest);
                    return locationRequest.Result;
                },
                domainResults,
                failures);

            request.Result = new RefreshAllDashboardSnapshotsResult
            {
                Domains = domainResults
            };

            if (failures.Count > 0)
                throw new AggregateException("One or more dashboard snapshot refreshes failed.", failures);
        }

        private static void RunDomain<T>(
            string domain,
            Func<T> action,
            IList<RefreshDashboardDomainResult> domainResults,
            IList<Exception> failures)
            where T : class
        {
            try
            {
                var result = action();
                domainResults.Add(MapDomainResult(domain, result));
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        }

        private static RefreshDashboardDomainResult MapDomainResult(string domain, object result)
        {
            switch (result)
            {
                case RefreshDashboardPiutangSnapshotResult piutang:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = piutang.RefreshLogId,
                        DurationMs = piutang.DurationMs
                    };
                case RefreshDashboardInventorySnapshotResult inventory:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = inventory.RefreshLogId,
                        DurationMs = inventory.DurationMs
                    };
                case RefreshDashboardInventoryRiskSnapshotResult inventoryRisk:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = inventoryRisk.RefreshLogId,
                        DurationMs = inventoryRisk.DurationMs
                    };
                case RefreshDashboardSalesSnapshotResult sales:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = sales.RefreshLogId,
                        DurationMs = sales.DurationMs
                    };
                case RefreshDashboardPurchasingSnapshotResult purchasing:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = purchasing.RefreshLogId,
                        DurationMs = purchasing.DurationMs
                    };
                case RefreshDashboardPurchasingManagementSnapshotResult purchasingManagement:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = purchasingManagement.RefreshLogId,
                        DurationMs = purchasingManagement.DurationMs
                    };
                case RefreshDashboardCustomerSnapshotResult customer:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = customer.RefreshLogId,
                        DurationMs = customer.DurationMs
                    };
                case RefreshDashboardSalesmanSnapshotResult salesman:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = salesman.RefreshLogId,
                        DurationMs = salesman.DurationMs
                    };
                case RefreshDashboardCollectionSnapshotResult collection:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = collection.RefreshLogId,
                        DurationMs = collection.DurationMs
                    };
                case RefreshDashboardLocationSnapshotResult location:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = location.RefreshLogId,
                        DurationMs = location.DurationMs
                    };
                default:
                    return new RefreshDashboardDomainResult { Domain = domain };
            }
        }
    }
}
