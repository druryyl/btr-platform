using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
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
            var salesWorker = new StubSalesWorker();
            var purchasingWorker = new StubPurchasingWorker();
            var purchasingManagementWorker = new StubPurchasingManagementWorker();
            var customerWorker = new StubCustomerWorker();
            var salesmanWorker = new StubSalesmanWorker();
            var collectionWorker = new StubCollectionWorker();
            var locationWorker = new StubLocationWorker();
            var worker = new RefreshAllDashboardSnapshotsWorker(
                piutangWorker,
                inventoryWorker,
                inventoryRiskWorker,
                salesWorker,
                purchasingWorker,
                purchasingManagementWorker,
                customerWorker,
                salesmanWorker,
                collectionWorker,
                locationWorker);

            var request = new RefreshAllDashboardSnapshotsRequest
            {
                TriggeredBy = "Manual"
            };

            worker.Execute(request);

            piutangWorker.WasCalled.Should().BeTrue();
            inventoryWorker.WasCalled.Should().BeTrue();
            inventoryRiskWorker.WasCalled.Should().BeTrue();
            salesWorker.WasCalled.Should().BeTrue();
            purchasingWorker.WasCalled.Should().BeTrue();
            purchasingManagementWorker.WasCalled.Should().BeTrue();
            customerWorker.WasCalled.Should().BeTrue();
            salesmanWorker.WasCalled.Should().BeTrue();
            collectionWorker.WasCalled.Should().BeTrue();
            locationWorker.WasCalled.Should().BeTrue();
            piutangWorker.CallOrder.Should().BeLessThan(inventoryWorker.CallOrder);
            inventoryWorker.CallOrder.Should().BeLessThan(inventoryRiskWorker.CallOrder);
            inventoryRiskWorker.CallOrder.Should().BeLessThan(salesWorker.CallOrder);
            salesWorker.CallOrder.Should().BeLessThan(purchasingWorker.CallOrder);
            purchasingWorker.CallOrder.Should().BeLessThan(purchasingManagementWorker.CallOrder);
            purchasingManagementWorker.CallOrder.Should().BeLessThan(customerWorker.CallOrder);
            customerWorker.CallOrder.Should().BeLessThan(salesmanWorker.CallOrder);
            salesmanWorker.CallOrder.Should().BeLessThan(collectionWorker.CallOrder);
            collectionWorker.CallOrder.Should().BeLessThan(locationWorker.CallOrder);
            request.Result.Domains.Should().HaveCount(10);
            request.Result.Domains[0].Domain.Should().Be("Piutang");
            request.Result.Domains[1].Domain.Should().Be("Inventory");
            request.Result.Domains[2].Domain.Should().Be("InventoryRisk");
            request.Result.Domains[3].Domain.Should().Be("Sales");
            request.Result.Domains[4].Domain.Should().Be("Purchasing");
            request.Result.Domains[5].Domain.Should().Be("PurchasingManagement");
            request.Result.Domains[6].Domain.Should().Be("Customer");
            request.Result.Domains[7].Domain.Should().Be("Salesman");
            request.Result.Domains[8].Domain.Should().Be("Collection");
            request.Result.Domains[9].Domain.Should().Be("Location");
        }

        [Fact]
        public void Execute_ThrowsAggregateException_WhenOneDomainFails()
        {
            var worker = new RefreshAllDashboardSnapshotsWorker(
                new StubPiutangWorker(),
                new StubInventoryWorker { ShouldFail = true },
                new StubInventoryRiskWorker(),
                new StubSalesWorker(),
                new StubPurchasingWorker(),
                new StubPurchasingManagementWorker(),
                new StubCustomerWorker(),
                new StubSalesmanWorker(),
                new StubCollectionWorker(),
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
