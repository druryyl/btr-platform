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
        private readonly IRefreshDashboardSalesSnapshotWorker _salesWorker;
        private readonly IRefreshDashboardPurchasingSnapshotWorker _purchasingWorker;

        public RefreshAllDashboardSnapshotsWorker(
            IRefreshDashboardPiutangSnapshotWorker piutangWorker,
            IRefreshDashboardInventorySnapshotWorker inventoryWorker,
            IRefreshDashboardSalesSnapshotWorker salesWorker,
            IRefreshDashboardPurchasingSnapshotWorker purchasingWorker)
        {
            _piutangWorker = piutangWorker;
            _inventoryWorker = inventoryWorker;
            _salesWorker = salesWorker;
            _purchasingWorker = purchasingWorker;
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
                default:
                    return new RefreshDashboardDomainResult { Domain = domain };
            }
        }
    }
}
