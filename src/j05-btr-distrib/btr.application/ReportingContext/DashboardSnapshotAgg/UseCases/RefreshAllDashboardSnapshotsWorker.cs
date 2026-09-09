using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases;
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
        private readonly IRefreshDashboardPurchasingSnapshotWorker _purchasingWorker;
        private readonly IRefreshPrincipalPurchaseInSnapshotWorker _principalPurchaseInWorker;
        private readonly IRefreshDashboardPurchasingManagementSnapshotWorker _purchasingManagementWorker;
        private readonly IRefreshDashboardCustomerSnapshotWorker _customerWorker;
        private readonly IRefreshDashboardSalesmanSnapshotWorker _salesmanWorker;
        private readonly IRefreshDashboardCollectionSnapshotWorker _collectionWorker;
        private readonly IRefreshDashboardFieldActivitySnapshotWorker _fieldActivityWorker;
        private readonly IRefreshDashboardLocationSnapshotWorker _locationWorker;

        public RefreshAllDashboardSnapshotsWorker(
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
            IRefreshDashboardPurchasingSnapshotWorker purchasingWorker,
            IRefreshPrincipalPurchaseInSnapshotWorker principalPurchaseInWorker,
            IRefreshDashboardPurchasingManagementSnapshotWorker purchasingManagementWorker,
            IRefreshDashboardCustomerSnapshotWorker customerWorker,
            IRefreshDashboardSalesmanSnapshotWorker salesmanWorker,
            IRefreshDashboardCollectionSnapshotWorker collectionWorker,
            IRefreshDashboardFieldActivitySnapshotWorker fieldActivityWorker,
            IRefreshDashboardLocationSnapshotWorker locationWorker)
        {
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
            _purchasingWorker = purchasingWorker;
            _principalPurchaseInWorker = principalPurchaseInWorker;
            _purchasingManagementWorker = purchasingManagementWorker;
            _customerWorker = customerWorker;
            _salesmanWorker = salesmanWorker;
            _collectionWorker = collectionWorker;
            _fieldActivityWorker = fieldActivityWorker;
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
                PrincipalInventorySnapshot.Domain,
                () =>
                {
                    var principalInventoryRequest = new RefreshPrincipalInventorySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalInventoryWorker.Execute(principalInventoryRequest);
                    return principalInventoryRequest.Result;
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
                PrincipalSalesOutSnapshot.Domain,
                () =>
                {
                    var principalSalesOutRequest = new RefreshPrincipalSalesOutSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesOutWorker.Execute(principalSalesOutRequest);
                    return principalSalesOutRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalReturnSnapshot.Domain,
                () =>
                {
                    var principalReturnRequest = new RefreshPrincipalReturnSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnWorker.Execute(principalReturnRequest);
                    return principalReturnRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalReturnPercentageSnapshot.Domain,
                () =>
                {
                    var principalReturnPercentageRequest = new RefreshPrincipalReturnPercentageSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnPercentageWorker.Execute(principalReturnPercentageRequest);
                    return principalReturnPercentageRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalSalesOutHistory.Domain,
                () =>
                {
                    var principalSalesOutHistoryRequest = new RefreshPrincipalSalesOutHistoryRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesOutHistoryWorker.Execute(principalSalesOutHistoryRequest);
                    return principalSalesOutHistoryRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalReturnHistory.Domain,
                () =>
                {
                    var principalReturnHistoryRequest = new RefreshPrincipalReturnHistoryRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalReturnHistoryWorker.Execute(principalReturnHistoryRequest);
                    return principalReturnHistoryRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalTargetSnapshot.Domain,
                () =>
                {
                    var principalTargetRequest = new RefreshPrincipalTargetSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalTargetWorker.Execute(principalTargetRequest);
                    return principalTargetRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalAchievementSnapshot.Domain,
                () =>
                {
                    var principalAchievementRequest = new RefreshPrincipalAchievementSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalAchievementWorker.Execute(principalAchievementRequest);
                    return principalAchievementRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalMomGrowthSnapshot.Domain,
                () =>
                {
                    var principalMomGrowthRequest = new RefreshPrincipalMomGrowthSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalMomGrowthWorker.Execute(principalMomGrowthRequest);
                    return principalMomGrowthRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                PrincipalSalesmanContributionSnapshot.Domain,
                () =>
                {
                    var principalSalesmanContributionRequest = new RefreshPrincipalSalesmanContributionSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalSalesmanContributionWorker.Execute(principalSalesmanContributionRequest);
                    return principalSalesmanContributionRequest.Result;
                },
                domainResults,
                failures);

            RunDomain(
                CustomerPrincipalRelationship.Domain,
                () =>
                {
                    var relationshipRequest = new RefreshCustomerPrincipalRelationshipRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _customerPrincipalRelationshipWorker.Execute(relationshipRequest);
                    return relationshipRequest.Result;
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
                PrincipalPurchaseInSnapshot.Domain,
                () =>
                {
                    var principalPurchaseInRequest = new RefreshPrincipalPurchaseInSnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _principalPurchaseInWorker.Execute(principalPurchaseInRequest);
                    return principalPurchaseInRequest.Result;
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
                "FieldActivity",
                () =>
                {
                    var fieldActivityRequest = new RefreshDashboardFieldActivitySnapshotRequest
                    {
                        TriggeredBy = triggeredBy
                    };
                    _fieldActivityWorker.Execute(fieldActivityRequest);
                    return fieldActivityRequest.Result;
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
            var stepId = WorkerProgressStepIds.DomainStep(domain);
            WorkerProgressScope.Current.StepStarted(stepId, $"Refresh {domain} Snapshot");

            try
            {
                var result = action();
                var mapped = MapDomainResult(domain, result);
                domainResults.Add(mapped);
                WorkerProgressScope.Current.StepCompleted(stepId, new WorkerProgressStepInfo
                {
                    Duration = TimeSpan.FromMilliseconds(mapped.DurationMs)
                });
            }
            catch (Exception ex)
            {
                WorkerProgressScope.Current.StepFailed(stepId, ex.Message);
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
                case RefreshPrincipalInventorySnapshotResult principalInventory:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalInventory.RefreshLogId,
                        DurationMs = principalInventory.DurationMs
                    };
                case RefreshDashboardSalesSnapshotResult sales:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = sales.RefreshLogId,
                        DurationMs = sales.DurationMs
                    };
                case RefreshPrincipalSalesOutSnapshotResult principalSalesOut:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalSalesOut.RefreshLogId,
                        DurationMs = principalSalesOut.DurationMs
                    };
                case RefreshPrincipalReturnSnapshotResult principalReturn:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalReturn.RefreshLogId,
                        DurationMs = principalReturn.DurationMs
                    };
                case RefreshPrincipalReturnPercentageSnapshotResult principalReturnPercentage:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalReturnPercentage.RefreshLogId,
                        DurationMs = principalReturnPercentage.DurationMs
                    };
                case RefreshPrincipalSalesOutHistoryResult principalSalesOutHistory:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalSalesOutHistory.RefreshLogId,
                        DurationMs = principalSalesOutHistory.DurationMs
                    };
                case RefreshPrincipalReturnHistoryResult principalReturnHistory:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalReturnHistory.RefreshLogId,
                        DurationMs = principalReturnHistory.DurationMs
                    };
                case RefreshPrincipalTargetSnapshotResult principalTarget:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalTarget.RefreshLogId,
                        DurationMs = principalTarget.DurationMs
                    };
                case RefreshPrincipalAchievementSnapshotResult principalAchievement:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalAchievement.RefreshLogId,
                        DurationMs = principalAchievement.DurationMs
                    };
                case RefreshPrincipalMomGrowthSnapshotResult principalMomGrowth:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalMomGrowth.RefreshLogId,
                        DurationMs = principalMomGrowth.DurationMs
                    };
                case RefreshPrincipalSalesmanContributionSnapshotResult principalSalesmanContribution:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = principalSalesmanContribution.RefreshLogId,
                        DurationMs = principalSalesmanContribution.DurationMs
                    };
                case RefreshCustomerPrincipalRelationshipResult customerPrincipalRelationship:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = customerPrincipalRelationship.RefreshLogId,
                        DurationMs = customerPrincipalRelationship.DurationMs
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
                case RefreshDashboardFieldActivitySnapshotResult fieldActivity:
                    return new RefreshDashboardDomainResult
                    {
                        Domain = domain,
                        RefreshLogId = fieldActivity.RefreshLogId,
                        DurationMs = fieldActivity.DurationMs
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
