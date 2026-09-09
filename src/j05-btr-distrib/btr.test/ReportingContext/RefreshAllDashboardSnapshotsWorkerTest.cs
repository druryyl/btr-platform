using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class RefreshAllDashboardSnapshotsWorkerTest
    {
        private const string StubRefreshLogId = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
        [Fact]
        public void Execute_RunsDomainsInOrder_AndReturnsResults()
        {
            _callSequence = 0;
            var piutangWorker = new StubPiutangWorker();
            var inventoryWorker = new StubInventoryWorker();
            var inventoryRiskWorker = new StubInventoryRiskWorker();
            var principalInventoryWorker = new StubPrincipalInventoryWorker();
            var salesWorker = new StubSalesWorker();
            var principalSalesOutWorker = new StubPrincipalSalesOutWorker();
            var principalReturnWorker = new StubPrincipalReturnWorker();
            var principalReturnPercentageWorker = new StubPrincipalReturnPercentageWorker();
            var principalSalesOutHistoryWorker = new StubPrincipalSalesOutHistoryWorker();
            var principalReturnHistoryWorker = new StubPrincipalReturnHistoryWorker();
            var principalTargetWorker = new StubPrincipalTargetWorker();
            var principalAchievementWorker = new StubPrincipalAchievementWorker();
            var principalMomGrowthWorker = new StubPrincipalMomGrowthWorker();
            var principalSalesmanContributionWorker = new StubPrincipalSalesmanContributionWorker();
            var customerPrincipalRelationshipWorker = new StubCustomerPrincipalRelationshipWorker();
            var principalActiveCustomerWorker = new StubPrincipalActiveCustomerWorker();
            var purchasingWorker = new StubPurchasingWorker();
            var principalPurchaseInWorker = new StubPrincipalPurchaseInWorker();
            var purchasingManagementWorker = new StubPurchasingManagementWorker();
            var customerWorker = new StubCustomerWorker();
            var salesmanWorker = new StubSalesmanWorker();
            var collectionWorker = new StubCollectionWorker();
            var fieldActivityWorker = new StubFieldActivityWorker();
            var locationWorker = new StubLocationWorker();
            var worker = new RefreshAllDashboardSnapshotsWorker(
                piutangWorker,
                inventoryWorker,
                inventoryRiskWorker,
                principalInventoryWorker,
                salesWorker,
                principalSalesOutWorker,
                principalReturnWorker,
                principalReturnPercentageWorker,
                principalSalesOutHistoryWorker,
                principalReturnHistoryWorker,
                principalTargetWorker,
                principalAchievementWorker,
                principalMomGrowthWorker,
                principalSalesmanContributionWorker,
                customerPrincipalRelationshipWorker,
                principalActiveCustomerWorker,
                purchasingWorker,
                principalPurchaseInWorker,
                purchasingManagementWorker,
                customerWorker,
                salesmanWorker,
                collectionWorker,
                fieldActivityWorker,
                locationWorker);

            var request = new RefreshAllDashboardSnapshotsRequest
            {
                TriggeredBy = "Manual"
            };

            worker.Execute(request);

            piutangWorker.WasCalled.Should().BeTrue();
            inventoryWorker.WasCalled.Should().BeTrue();
            inventoryRiskWorker.WasCalled.Should().BeTrue();
            principalInventoryWorker.WasCalled.Should().BeTrue();
            salesWorker.WasCalled.Should().BeTrue();
            principalSalesOutWorker.WasCalled.Should().BeTrue();
            principalReturnWorker.WasCalled.Should().BeTrue();
            principalReturnPercentageWorker.WasCalled.Should().BeTrue();
            principalSalesOutHistoryWorker.WasCalled.Should().BeTrue();
            principalReturnHistoryWorker.WasCalled.Should().BeTrue();
            principalTargetWorker.WasCalled.Should().BeTrue();
            principalAchievementWorker.WasCalled.Should().BeTrue();
            principalMomGrowthWorker.WasCalled.Should().BeTrue();
            principalSalesmanContributionWorker.WasCalled.Should().BeTrue();
            customerPrincipalRelationshipWorker.WasCalled.Should().BeTrue();
            principalActiveCustomerWorker.WasCalled.Should().BeTrue();
            purchasingWorker.WasCalled.Should().BeTrue();
            principalPurchaseInWorker.WasCalled.Should().BeTrue();
            purchasingManagementWorker.WasCalled.Should().BeTrue();
            customerWorker.WasCalled.Should().BeTrue();
            salesmanWorker.WasCalled.Should().BeTrue();
            collectionWorker.WasCalled.Should().BeTrue();
            fieldActivityWorker.WasCalled.Should().BeTrue();
            locationWorker.WasCalled.Should().BeTrue();
            piutangWorker.CallOrder.Should().BeLessThan(inventoryWorker.CallOrder);
            inventoryWorker.CallOrder.Should().BeLessThan(inventoryRiskWorker.CallOrder);
            inventoryRiskWorker.CallOrder.Should().BeLessThan(principalInventoryWorker.CallOrder);
            principalInventoryWorker.CallOrder.Should().BeLessThan(salesWorker.CallOrder);
            salesWorker.CallOrder.Should().BeLessThan(principalSalesOutWorker.CallOrder);
            principalSalesOutWorker.CallOrder.Should().BeLessThan(principalReturnWorker.CallOrder);
            principalReturnWorker.CallOrder.Should().BeLessThan(principalReturnPercentageWorker.CallOrder);
            principalReturnPercentageWorker.CallOrder.Should().BeLessThan(principalSalesOutHistoryWorker.CallOrder);
            principalSalesOutHistoryWorker.CallOrder.Should().BeLessThan(principalReturnHistoryWorker.CallOrder);
            principalReturnHistoryWorker.CallOrder.Should().BeLessThan(principalTargetWorker.CallOrder);
            principalTargetWorker.CallOrder.Should().BeLessThan(principalAchievementWorker.CallOrder);
            principalAchievementWorker.CallOrder.Should().BeLessThan(principalMomGrowthWorker.CallOrder);
            principalMomGrowthWorker.CallOrder.Should().BeLessThan(principalSalesmanContributionWorker.CallOrder);
            principalSalesmanContributionWorker.CallOrder.Should().BeLessThan(customerPrincipalRelationshipWorker.CallOrder);
            customerPrincipalRelationshipWorker.CallOrder.Should().BeLessThan(principalActiveCustomerWorker.CallOrder);
            principalActiveCustomerWorker.CallOrder.Should().BeLessThan(purchasingWorker.CallOrder);
            purchasingWorker.CallOrder.Should().BeLessThan(principalPurchaseInWorker.CallOrder);
            principalPurchaseInWorker.CallOrder.Should().BeLessThan(purchasingManagementWorker.CallOrder);
            purchasingManagementWorker.CallOrder.Should().BeLessThan(customerWorker.CallOrder);
            customerWorker.CallOrder.Should().BeLessThan(salesmanWorker.CallOrder);
            salesmanWorker.CallOrder.Should().BeLessThan(collectionWorker.CallOrder);
            collectionWorker.CallOrder.Should().BeLessThan(fieldActivityWorker.CallOrder);
            fieldActivityWorker.CallOrder.Should().BeLessThan(locationWorker.CallOrder);
            request.Result.Domains.Should().HaveCount(24);
            request.Result.Domains[0].Domain.Should().Be("Piutang");
            request.Result.Domains[1].Domain.Should().Be("Inventory");
            request.Result.Domains[2].Domain.Should().Be("InventoryRisk");
            request.Result.Domains[3].Domain.Should().Be("PrincipalInventory");
            request.Result.Domains[4].Domain.Should().Be("Sales");
            request.Result.Domains[5].Domain.Should().Be("PrincipalSalesOut");
            request.Result.Domains[6].Domain.Should().Be("PrincipalReturn");
            request.Result.Domains[7].Domain.Should().Be("PrnReturnPercentage");
            request.Result.Domains[8].Domain.Should().Be("PrnSalesOutHistory");
            request.Result.Domains[9].Domain.Should().Be("PrnReturnHistory");
            request.Result.Domains[10].Domain.Should().Be("PrincipalTarget");
            request.Result.Domains[11].Domain.Should().Be("PrnAchievement");
            request.Result.Domains[12].Domain.Should().Be("PrnMomGrowth");
            request.Result.Domains[13].Domain.Should().Be("PrnSalesmanContribution");
            request.Result.Domains[14].Domain.Should().Be("PrnCusRelationship");
            request.Result.Domains[15].Domain.Should().Be("PrnActiveCustomer");
            request.Result.Domains[16].Domain.Should().Be("Purchasing");
            request.Result.Domains[17].Domain.Should().Be("PrincipalPurchaseIn");
            request.Result.Domains[18].Domain.Should().Be("PurchasingManagement");
            request.Result.Domains[19].Domain.Should().Be("Customer");
            request.Result.Domains[20].Domain.Should().Be("Salesman");
            request.Result.Domains[21].Domain.Should().Be("Collection");
            request.Result.Domains[22].Domain.Should().Be("FieldActivity");
            request.Result.Domains[23].Domain.Should().Be("Location");
        }

        [Fact]
        public void Execute_ThrowsAggregateException_WhenOneDomainFails()
        {
            var worker = new RefreshAllDashboardSnapshotsWorker(
                new StubPiutangWorker(),
                new StubInventoryWorker { ShouldFail = true },
                new StubInventoryRiskWorker(),
                new StubPrincipalInventoryWorker(),
                new StubSalesWorker(),
                new StubPrincipalSalesOutWorker(),
                new StubPrincipalReturnWorker(),
                new StubPrincipalReturnPercentageWorker(),
                new StubPrincipalSalesOutHistoryWorker(),
                new StubPrincipalReturnHistoryWorker(),
                new StubPrincipalTargetWorker(),
                new StubPrincipalAchievementWorker(),
                new StubPrincipalMomGrowthWorker(),
                new StubPrincipalSalesmanContributionWorker(),
                new StubCustomerPrincipalRelationshipWorker(),
                new StubPrincipalActiveCustomerWorker(),
                new StubPurchasingWorker(),
                new StubPrincipalPurchaseInWorker(),
                new StubPurchasingManagementWorker(),
                new StubCustomerWorker(),
                new StubSalesmanWorker(),
                new StubCollectionWorker(),
                new StubFieldActivityWorker(),
                new StubLocationWorker());

            Action act = () => worker.Execute(new RefreshAllDashboardSnapshotsRequest());

            act.Should().Throw<AggregateException>()
                .WithMessage("*One or more dashboard snapshot refreshes failed*");
        }

        private static int _callSequence;

        private sealed class StubPiutangWorker : IRefreshDashboardPiutangSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardPiutangSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardPiutangSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 100
                };
            }
        }

        private sealed class StubInventoryWorker : IRefreshDashboardInventorySnapshotWorker
        {
            public bool ShouldFail { get; set; }

            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardInventorySnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;

                if (ShouldFail)
                    throw new InvalidOperationException("Inventory refresh failed.");

                request.Result = new RefreshDashboardInventorySnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 200
                };
            }
        }

        private sealed class StubInventoryRiskWorker : IRefreshDashboardInventoryRiskSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardInventoryRiskSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardInventoryRiskSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 250
                };
            }
        }

        private sealed class StubPrincipalInventoryWorker : IRefreshPrincipalInventorySnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalInventorySnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalInventorySnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 260
                };
            }
        }

        private sealed class StubSalesWorker : IRefreshDashboardSalesSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardSalesSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardSalesSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 300
                };
            }
        }

        private sealed class StubPrincipalSalesOutHistoryWorker : IRefreshPrincipalSalesOutHistoryWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalSalesOutHistoryRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalSalesOutHistoryResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 360
                };
            }
        }

        private sealed class StubPrincipalReturnHistoryWorker : IRefreshPrincipalReturnHistoryWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalReturnHistoryRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalReturnHistoryResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 362
                };
            }
        }

        private sealed class StubCustomerPrincipalRelationshipWorker : IRefreshCustomerPrincipalRelationshipWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshCustomerPrincipalRelationshipRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshCustomerPrincipalRelationshipResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 380
                };
            }
        }

        private sealed class StubPrincipalPurchaseInWorker : IRefreshPrincipalPurchaseInSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalPurchaseInSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalPurchaseInSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 375
                };
            }
        }

        private sealed class StubPrincipalTargetWorker : IRefreshPrincipalTargetSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalTargetSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalTargetSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 370
                };
            }
        }

        private sealed class StubPrincipalReturnWorker : IRefreshPrincipalReturnSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalReturnSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalReturnSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 360
                };
            }
        }

        private sealed class StubPrincipalReturnPercentageWorker : IRefreshPrincipalReturnPercentageSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalReturnPercentageSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalReturnPercentageSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 365
                };
            }
        }

        private sealed class StubPrincipalAchievementWorker : IRefreshPrincipalAchievementSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalAchievementSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalAchievementSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 372
                };
            }
        }

        private sealed class StubPrincipalMomGrowthWorker : IRefreshPrincipalMomGrowthSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalMomGrowthSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalMomGrowthSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 373
                };
            }
        }

        private sealed class StubPrincipalSalesmanContributionWorker : IRefreshPrincipalSalesmanContributionSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalSalesmanContributionSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalSalesmanContributionSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 373
                };
            }
        }

        private sealed class StubPrincipalActiveCustomerWorker : IRefreshPrincipalActiveCustomerSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalActiveCustomerSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalActiveCustomerSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 381
                };
            }
        }

        private sealed class StubPrincipalSalesOutWorker : IRefreshPrincipalSalesOutSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshPrincipalSalesOutSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshPrincipalSalesOutSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 350
                };
            }
        }

        private sealed class StubPurchasingWorker : IRefreshDashboardPurchasingSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardPurchasingSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardPurchasingSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 400
                };
            }
        }

        private sealed class StubPurchasingManagementWorker : IRefreshDashboardPurchasingManagementSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardPurchasingManagementSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardPurchasingManagementSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 425
                };
            }
        }

        private sealed class StubCustomerWorker : IRefreshDashboardCustomerSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardCustomerSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardCustomerSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 500
                };
            }
        }

        private sealed class StubSalesmanWorker : IRefreshDashboardSalesmanSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardSalesmanSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardSalesmanSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 600
                };
            }
        }

        private sealed class StubCollectionWorker : IRefreshDashboardCollectionSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardCollectionSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardCollectionSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 700
                };
            }
        }

        private sealed class StubFieldActivityWorker : IRefreshDashboardFieldActivitySnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardFieldActivitySnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardFieldActivitySnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 725
                };
            }
        }

        private sealed class StubLocationWorker : IRefreshDashboardLocationSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public int CallOrder { get; private set; }

            public void Execute(RefreshDashboardLocationSnapshotRequest request)
            {
                WasCalled = true;
                CallOrder = ++_callSequence;
                request.Result = new RefreshDashboardLocationSnapshotResult
                {
                    RefreshLogId = StubRefreshLogId,
                    DurationMs = 750
                };
            }
        }
    }
}
