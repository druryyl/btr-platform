using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases;
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
        private readonly IRefreshPrincipalInventorySnapshotWorker _principalInventoryWorker;
        private readonly IRefreshDashboardSalesSnapshotWorker _salesWorker;
        private readonly IRefreshPrincipalSalesOutSnapshotWorker _principalSalesOutWorker;
        private readonly IRefreshPrincipalReturnSnapshotWorker _principalReturnWorker;
        private readonly IRefreshPrincipalReturnPercentageSnapshotWorker _principalReturnPercentageWorker;
        private readonly IRefreshPrincipalSalesOutHistoryWorker _principalSalesOutHistoryWorker;
        private readonly IRefreshPrincipalReturnHistoryWorker _principalReturnHistoryWorker;
        private readonly IRefreshPrincipalTargetSnapshotWorker _principalTargetWorker;
        private readonly IRefreshPrincipalAchievementSnapshotWorker _principalAchievementWorker;
        private readonly IRefreshPrincipalMomGrowthSnapshotWorker _principalMomGrowthWorker;
        private readonly IRefreshPrincipalSalesmanContributionSnapshotWorker _principalSalesmanContributionWorker;
        private readonly IRefreshCustomerPrincipalRelationshipWorker _customerPrincipalRelationshipWorker;
        private readonly IRefreshPrincipalActiveCustomerSnapshotWorker _principalActiveCustomerWorker;
        private readonly IRefreshPrincipalCustomerCoverageSnapshotWorker _principalCustomerCoverageWorker;
        private readonly IRefreshDashboardPurchasingSnapshotWorker _purchasingWorker;
        private readonly IRefreshPrincipalPurchaseInSnapshotWorker _principalPurchaseInWorker;
        private readonly IRefreshDashboardPurchasingManagementSnapshotWorker _purchasingManagementWorker;
        private readonly IRefreshDashboardCustomerSnapshotWorker _customerWorker;
        private readonly IRefreshDashboardSalesmanSnapshotWorker _salesmanWorker;
        private readonly IRefreshDashboardCollectionSnapshotWorker _collectionWorker;
        private readonly IRefreshDashboardFieldActivitySnapshotWorker _fieldActivityWorker;
        private readonly IRefreshDashboardLocationSnapshotWorker _locationWorker;

        public RefreshDashboardSnapshotsHandler(
            IRefreshAllDashboardSnapshotsWorker allWorker,
            IRefreshDashboardPiutangSnapshotWorker piutangWorker,
            IRefreshDashboardInventorySnapshotWorker inventoryWorker,
            IRefreshDashboardInventoryRiskSnapshotWorker inventoryRiskWorker,
            IRefreshPrincipalInventorySnapshotWorker principalInventoryWorker,
            IRefreshDashboardSalesSnapshotWorker salesWorker,
            IRefreshPrincipalSalesOutSnapshotWorker principalSalesOutWorker,
            IRefreshPrincipalReturnSnapshotWorker principalReturnWorker,
            IRefreshPrincipalReturnPercentageSnapshotWorker principalReturnPercentageWorker,
            IRefreshPrincipalSalesOutHistoryWorker principalSalesOutHistoryWorker,
            IRefreshPrincipalReturnHistoryWorker principalReturnHistoryWorker,
            IRefreshPrincipalTargetSnapshotWorker principalTargetWorker,
            IRefreshPrincipalAchievementSnapshotWorker principalAchievementWorker,
            IRefreshPrincipalMomGrowthSnapshotWorker principalMomGrowthWorker,
            IRefreshPrincipalSalesmanContributionSnapshotWorker principalSalesmanContributionWorker,
            IRefreshCustomerPrincipalRelationshipWorker customerPrincipalRelationshipWorker,
            IRefreshPrincipalActiveCustomerSnapshotWorker principalActiveCustomerWorker,
            IRefreshPrincipalCustomerCoverageSnapshotWorker principalCustomerCoverageWorker,
            IRefreshDashboardPurchasingSnapshotWorker purchasingWorker,
            IRefreshPrincipalPurchaseInSnapshotWorker principalPurchaseInWorker,
            IRefreshDashboardPurchasingManagementSnapshotWorker purchasingManagementWorker,
            IRefreshDashboardCustomerSnapshotWorker customerWorker,
            IRefreshDashboardSalesmanSnapshotWorker salesmanWorker,
            IRefreshDashboardCollectionSnapshotWorker collectionWorker,
            IRefreshDashboardFieldActivitySnapshotWorker fieldActivityWorker,
            IRefreshDashboardLocationSnapshotWorker locationWorker)
        {
            _allWorker = allWorker;
            _piutangWorker = piutangWorker;
            _inventoryWorker = inventoryWorker;
            _inventoryRiskWorker = inventoryRiskWorker;
            _principalInventoryWorker = principalInventoryWorker;
            _salesWorker = salesWorker;
            _principalSalesOutWorker = principalSalesOutWorker;
            _principalReturnWorker = principalReturnWorker;
            _principalReturnPercentageWorker = principalReturnPercentageWorker;
            _principalSalesOutHistoryWorker = principalSalesOutHistoryWorker;
            _principalReturnHistoryWorker = principalReturnHistoryWorker;
            _principalTargetWorker = principalTargetWorker;
            _principalAchievementWorker = principalAchievementWorker;
            _principalMomGrowthWorker = principalMomGrowthWorker;
            _principalSalesmanContributionWorker = principalSalesmanContributionWorker;
            _customerPrincipalRelationshipWorker = customerPrincipalRelationshipWorker;
            _principalActiveCustomerWorker = principalActiveCustomerWorker;
            _principalCustomerCoverageWorker = principalCustomerCoverageWorker;
            _purchasingWorker = purchasingWorker;
            _principalPurchaseInWorker = principalPurchaseInWorker;
            _purchasingManagementWorker = purchasingManagementWorker;
            _customerWorker = customerWorker;
            _salesmanWorker = salesmanWorker;
            _collectionWorker = collectionWorker;
            _fieldActivityWorker = fieldActivityWorker;
            _locationWorker = locationWorker;
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

                case PrincipalSalesOutSnapshot.Domain:
                    var principalSalesOutRequest = new RefreshPrincipalSalesOutSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesOutWorker.Execute(principalSalesOutRequest);
                    return MapResult(PrincipalSalesOutSnapshot.Domain, principalSalesOutRequest.Result);

                case PrincipalReturnSnapshot.Domain:
                    var principalReturnRequest = new RefreshPrincipalReturnSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnWorker.Execute(principalReturnRequest);
                    return MapResult(PrincipalReturnSnapshot.Domain, principalReturnRequest.Result);

                case PrincipalReturnPercentageSnapshot.Domain:
                    var principalReturnPercentageRequest = new RefreshPrincipalReturnPercentageSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnPercentageWorker.Execute(principalReturnPercentageRequest);
                    return MapResult(PrincipalReturnPercentageSnapshot.Domain, principalReturnPercentageRequest.Result);

                case PrincipalSalesOutHistory.Domain:
                    var principalSalesOutHistoryRequest = new RefreshPrincipalSalesOutHistoryRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesOutHistoryWorker.Execute(principalSalesOutHistoryRequest);
                    return MapResult(PrincipalSalesOutHistory.Domain, principalSalesOutHistoryRequest.Result);

                case PrincipalReturnHistory.Domain:
                    var principalReturnHistoryRequest = new RefreshPrincipalReturnHistoryRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnHistoryWorker.Execute(principalReturnHistoryRequest);
                    return MapResult(PrincipalReturnHistory.Domain, principalReturnHistoryRequest.Result);

                case PrincipalTargetSnapshot.Domain:
                    var principalTargetRequest = new RefreshPrincipalTargetSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalTargetWorker.Execute(principalTargetRequest);
                    return MapResult(PrincipalTargetSnapshot.Domain, principalTargetRequest.Result);

                case PrincipalAchievementSnapshot.Domain:
                    var principalAchievementRequest = new RefreshPrincipalAchievementSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalAchievementWorker.Execute(principalAchievementRequest);
                    return MapResult(PrincipalAchievementSnapshot.Domain, principalAchievementRequest.Result);

                case PrincipalMomGrowthSnapshot.Domain:
                    var principalMomGrowthRequest = new RefreshPrincipalMomGrowthSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalMomGrowthWorker.Execute(principalMomGrowthRequest);
                    return MapResult(PrincipalMomGrowthSnapshot.Domain, principalMomGrowthRequest.Result);

                case PrincipalSalesmanContributionSnapshot.Domain:
                    var principalSalesmanContributionRequest = new RefreshPrincipalSalesmanContributionSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesmanContributionWorker.Execute(principalSalesmanContributionRequest);
                    return MapResult(PrincipalSalesmanContributionSnapshot.Domain, principalSalesmanContributionRequest.Result);

                case CustomerPrincipalRelationship.Domain:
                    var relationshipRequest = new RefreshCustomerPrincipalRelationshipRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _customerPrincipalRelationshipWorker.Execute(relationshipRequest);
                    return MapResult(CustomerPrincipalRelationship.Domain, relationshipRequest.Result);

                case PrincipalActiveCustomerSnapshot.Domain:
                    var activeCustomerRequest = new RefreshPrincipalActiveCustomerSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalActiveCustomerWorker.Execute(activeCustomerRequest);
                    return MapResult(PrincipalActiveCustomerSnapshot.Domain, activeCustomerRequest.Result);

                case PrincipalCustomerCoverageSnapshot.Domain:
                    var customerCoverageRequest = new RefreshPrincipalCustomerCoverageSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalCustomerCoverageWorker.Execute(customerCoverageRequest);
                    return MapResult(PrincipalCustomerCoverageSnapshot.Domain, customerCoverageRequest.Result);

                case PrincipalInventorySnapshot.Domain:
                    var principalInventoryRequest = new RefreshPrincipalInventorySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalInventoryWorker.Execute(principalInventoryRequest);
                    return MapResult(PrincipalInventorySnapshot.Domain, principalInventoryRequest.Result);

                case "Purchasing":
                    var purchasingRequest = new RefreshDashboardPurchasingSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _purchasingWorker.Execute(purchasingRequest);
                    return MapResult("Purchasing", purchasingRequest.Result);

                case PrincipalPurchaseInSnapshot.Domain:
                    var principalPurchaseInRequest = new RefreshPrincipalPurchaseInSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalPurchaseInWorker.Execute(principalPurchaseInRequest);
                    return MapResult(PrincipalPurchaseInSnapshot.Domain, principalPurchaseInRequest.Result);

                case "PurchasingManagement":
                    var purchasingManagementRequest = new RefreshDashboardPurchasingManagementSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _purchasingManagementWorker.Execute(purchasingManagementRequest);
                    return MapResult("PurchasingManagement", purchasingManagementRequest.Result);

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

                case "Collection":
                    var collectionRequest = new RefreshDashboardCollectionSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _collectionWorker.Execute(collectionRequest);
                    return MapResult("Collection", collectionRequest.Result);

                case "FieldActivity":
                    var fieldActivityRequest = new RefreshDashboardFieldActivitySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _fieldActivityWorker.Execute(fieldActivityRequest);
                    return MapResult("FieldActivity", fieldActivityRequest.Result);

                case "Location":
                    var locationRequest = new RefreshDashboardLocationSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _locationWorker.Execute(locationRequest);
                    return MapResult("Location", locationRequest.Result);

                default:
                    throw new ArgumentException(
                        "Domain must be All, Piutang, Inventory, InventoryRisk, PrincipalInventory, Sales, PrincipalSalesOut, PrincipalReturn, PrnReturnPercentage, PrnSalesOutHistory, PrnReturnHistory, PrincipalTarget, PrnAchievement, PrnMomGrowth, PrnCusRelationship, PrnActiveCustomer, PrnCustomerCoverage, Purchasing, PrincipalPurchaseIn, PurchasingManagement, Customer, Salesman, Collection, FieldActivity, or Location.",
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
            RefreshPrincipalSalesOutSnapshotResult result)
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
            RefreshPrincipalReturnSnapshotResult result)
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
            RefreshPrincipalReturnPercentageSnapshotResult result)
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
            RefreshPrincipalReturnHistoryResult result)
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
            RefreshPrincipalSalesOutHistoryResult result)
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
            RefreshPrincipalInventorySnapshotResult result)
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
            RefreshPrincipalPurchaseInSnapshotResult result)
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
            RefreshPrincipalTargetSnapshotResult result)
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
            RefreshPrincipalAchievementSnapshotResult result)
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
            RefreshPrincipalMomGrowthSnapshotResult result)
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
            RefreshPrincipalSalesmanContributionSnapshotResult result)
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
            RefreshCustomerPrincipalRelationshipResult result)
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
            RefreshPrincipalActiveCustomerSnapshotResult result)
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
            RefreshPrincipalCustomerCoverageSnapshotResult result)
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
            RefreshDashboardPurchasingManagementSnapshotResult result)
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

        private static RefreshDashboardDomainResult MapResult(
            string domain,
            RefreshDashboardCollectionSnapshotResult result)
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
            RefreshDashboardFieldActivitySnapshotResult result)
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
            RefreshDashboardLocationSnapshotResult result)
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

            if (string.Equals(trimmed, PrincipalInventorySnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalInventorySnapshot.Domain;

            if (string.Equals(trimmed, "Sales", StringComparison.OrdinalIgnoreCase))
                return "Sales";

            if (string.Equals(trimmed, PrincipalSalesOutSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalSalesOutSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalReturnSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalReturnSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalReturnPercentageSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalReturnPercentageSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalSalesOutHistory.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalSalesOutHistory.Domain;

            if (string.Equals(trimmed, PrincipalReturnHistory.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalReturnHistory.Domain;

            if (string.Equals(trimmed, PrincipalTargetSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalTargetSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalAchievementSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalAchievementSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalMomGrowthSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalMomGrowthSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalSalesmanContributionSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalSalesmanContributionSnapshot.Domain;

            if (string.Equals(trimmed, CustomerPrincipalRelationship.Domain, StringComparison.OrdinalIgnoreCase))
                return CustomerPrincipalRelationship.Domain;

            if (string.Equals(trimmed, PrincipalActiveCustomerSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalActiveCustomerSnapshot.Domain;

            if (string.Equals(trimmed, PrincipalCustomerCoverageSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalCustomerCoverageSnapshot.Domain;

            if (string.Equals(trimmed, "Purchasing", StringComparison.OrdinalIgnoreCase))
                return "Purchasing";

            if (string.Equals(trimmed, PrincipalPurchaseInSnapshot.Domain, StringComparison.OrdinalIgnoreCase))
                return PrincipalPurchaseInSnapshot.Domain;

            if (string.Equals(trimmed, "PurchasingManagement", StringComparison.OrdinalIgnoreCase))
                return "PurchasingManagement";

            if (string.Equals(trimmed, "Customer", StringComparison.OrdinalIgnoreCase))
                return "Customer";

            if (string.Equals(trimmed, "Salesman", StringComparison.OrdinalIgnoreCase))
                return "Salesman";

            if (string.Equals(trimmed, "Collection", StringComparison.OrdinalIgnoreCase))
                return "Collection";

            if (string.Equals(trimmed, "FieldActivity", StringComparison.OrdinalIgnoreCase))
                return "FieldActivity";

            if (string.Equals(trimmed, "Location", StringComparison.OrdinalIgnoreCase))
                return "Location";

            return trimmed;
        }
    }
}
