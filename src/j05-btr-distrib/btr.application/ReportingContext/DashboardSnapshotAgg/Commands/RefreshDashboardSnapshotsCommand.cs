using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using MediatR;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Commands
{
    public class RefreshDashboardSnapshotsCommand : IRequest<RefreshDashboardSnapshotsResponse>
    {
        public string Domain { get; set; } = "All";
    }

    public class RefreshDashboardSnapshotsResponse
    {
        public string Domain { get; set; }

        public string TriggeredBy { get; set; }

        public IList<RefreshDashboardDomainResult> Domains { get; set; }
            = new List<RefreshDashboardDomainResult>();
    }

    public class RefreshDashboardSnapshotsHandler
        : IRequestHandler<RefreshDashboardSnapshotsCommand, RefreshDashboardSnapshotsResponse>
    {
        private readonly IRefreshAllDashboardSnapshotsWorker _allWorker;
        private readonly IRefreshDashboardPiutangSnapshotWorker _piutangWorker;
        private readonly IRefreshDashboardInventorySnapshotWorker _inventoryWorker;
        private readonly IRefreshDashboardInventoryRiskSnapshotWorker _inventoryRiskWorker;
        private readonly IRefreshDashboardSalesSnapshotWorker _salesWorker;
        private readonly IRefreshDashboardPurchasingSnapshotWorker _purchasingWorker;
        private readonly IRefreshDashboardCustomerSnapshotWorker _customerWorker;
        private readonly IRefreshDashboardSalesmanSnapshotWorker _salesmanWorker;

        public RefreshDashboardSnapshotsHandler(
            IRefreshAllDashboardSnapshotsWorker allWorker,
            IRefreshDashboardPiutangSnapshotWorker piutangWorker,
            IRefreshDashboardInventorySnapshotWorker inventoryWorker,
            IRefreshDashboardInventoryRiskSnapshotWorker inventoryRiskWorker,
            IRefreshDashboardSalesSnapshotWorker salesWorker,
            IRefreshDashboardPurchasingSnapshotWorker purchasingWorker,
            IRefreshDashboardCustomerSnapshotWorker customerWorker,
            IRefreshDashboardSalesmanSnapshotWorker salesmanWorker)
        {
            _allWorker = allWorker;
            _piutangWorker = piutangWorker;
            _inventoryWorker = inventoryWorker;
            _inventoryRiskWorker = inventoryRiskWorker;
            _salesWorker = salesWorker;
            _purchasingWorker = purchasingWorker;
            _customerWorker = customerWorker;
            _salesmanWorker = salesmanWorker;
        }

        public Task<RefreshDashboardSnapshotsResponse> Handle(
            RefreshDashboardSnapshotsCommand request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var domain = NormalizeDomain(request.Domain);
            const string triggeredBy = "Manual";

            if (string.Equals(domain, "All", StringComparison.OrdinalIgnoreCase))
            {
                var allRequest = new RefreshAllDashboardSnapshotsRequest
                {
                    TriggeredBy = triggeredBy
                };

                _allWorker.Execute(allRequest);

                return Task.FromResult(new RefreshDashboardSnapshotsResponse
                {
                    Domain = "All",
                    TriggeredBy = triggeredBy,
                    Domains = allRequest.Result?.Domains ?? new List<RefreshDashboardDomainResult>()
                });
            }

            var domainResult = ExecuteSingleDomain(domain, triggeredBy);

            return Task.FromResult(new RefreshDashboardSnapshotsResponse
            {
                Domain = domain,
                TriggeredBy = triggeredBy,
                Domains = new List<RefreshDashboardDomainResult> { domainResult }
            });
        }

        private RefreshDashboardDomainResult ExecuteSingleDomain(string domain, string triggeredBy)
        {
            switch (domain)
            {
                case "Piutang":
                    var piutangRequest = new RefreshDashboardPiutangSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _piutangWorker.Execute(piutangRequest);
                    return MapResult("Piutang", piutangRequest.Result);

                case "Inventory":
                    var inventoryRequest = new RefreshDashboardInventorySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _inventoryWorker.Execute(inventoryRequest);
                    return MapResult("Inventory", inventoryRequest.Result);

                case "InventoryRisk":
                    var inventoryRiskRequest = new RefreshDashboardInventoryRiskSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _inventoryRiskWorker.Execute(inventoryRiskRequest);
                    return MapResult("InventoryRisk", inventoryRiskRequest.Result);

                case "Sales":
                    var salesRequest = new RefreshDashboardSalesSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _salesWorker.Execute(salesRequest);
                    return MapResult("Sales", salesRequest.Result);

                case "Purchasing":
                    var purchasingRequest = new RefreshDashboardPurchasingSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _purchasingWorker.Execute(purchasingRequest);
                    return MapResult("Purchasing", purchasingRequest.Result);

                case "Customer":
                    var customerRequest = new RefreshDashboardCustomerSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _customerWorker.Execute(customerRequest);
                    return MapResult("Customer", customerRequest.Result);

                case "Salesman":
                    var salesmanRequest = new RefreshDashboardSalesmanSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _salesmanWorker.Execute(salesmanRequest);
                    return MapResult("Salesman", salesmanRequest.Result);

                default:
                    throw new ArgumentException(
                        "Domain must be All, Piutang, Inventory, InventoryRisk, Sales, Purchasing, Customer, or Salesman.",
                        nameof(RefreshDashboardSnapshotsCommand.Domain));
            }
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardPiutangSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardInventorySnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardInventoryRiskSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardSalesSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardPurchasingSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardCustomerSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardSalesmanSnapshotResult result)
        {
            return new RefreshDashboardDomainResult
            {
                Domain = domain,
                RefreshLogId = result?.RefreshLogId,
                DurationMs = result?.DurationMs ?? 0
            };
        }

        private static string NormalizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return "All";

            var trimmed = domain.Trim();

            if (string.Equals(trimmed, "All", StringComparison.OrdinalIgnoreCase))
                return "All";

            if (string.Equals(trimmed, "Piutang", StringComparison.OrdinalIgnoreCase))
                return "Piutang";

            if (string.Equals(trimmed, "Inventory", StringComparison.OrdinalIgnoreCase))
                return "Inventory";

            if (string.Equals(trimmed, "InventoryRisk", StringComparison.OrdinalIgnoreCase))
                return "InventoryRisk";

            if (string.Equals(trimmed, "Sales", StringComparison.OrdinalIgnoreCase))
                return "Sales";

            if (string.Equals(trimmed, "Purchasing", StringComparison.OrdinalIgnoreCase))
                return "Purchasing";

            if (string.Equals(trimmed, "Customer", StringComparison.OrdinalIgnoreCase))
                return "Customer";

            if (string.Equals(trimmed, "Salesman", StringComparison.OrdinalIgnoreCase))
                return "Salesman";

            return trimmed;
        }
    }
}
