using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class RefreshAllDashboardSnapshotsWorkerTest
    {
        [Fact]
        public void Execute_RunsDomainsInOrder_AndReturnsResults()
        {
            _callSequence = 0;
            var piutangWorker = new StubPiutangWorker();
            var inventoryWorker = new StubInventoryWorker();
            var inventoryRiskWorker = new StubInventoryRiskWorker();
            var salesWorker = new StubSalesWorker();
            var purchasingWorker = new StubPurchasingWorker();
            var customerWorker = new StubCustomerWorker();
            var salesmanWorker = new StubSalesmanWorker();
            var worker = new RefreshAllDashboardSnapshotsWorker(
                piutangWorker,
                inventoryWorker,
                inventoryRiskWorker,
                salesWorker,
                purchasingWorker,
                customerWorker,
                salesmanWorker);

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
            customerWorker.WasCalled.Should().BeTrue();
            salesmanWorker.WasCalled.Should().BeTrue();
            piutangWorker.CallOrder.Should().BeLessThan(inventoryWorker.CallOrder);
            inventoryWorker.CallOrder.Should().BeLessThan(inventoryRiskWorker.CallOrder);
            inventoryRiskWorker.CallOrder.Should().BeLessThan(salesWorker.CallOrder);
            salesWorker.CallOrder.Should().BeLessThan(purchasingWorker.CallOrder);
            purchasingWorker.CallOrder.Should().BeLessThan(customerWorker.CallOrder);
            customerWorker.CallOrder.Should().BeLessThan(salesmanWorker.CallOrder);
            request.Result.Domains.Should().HaveCount(7);
            request.Result.Domains[0].Domain.Should().Be("Piutang");
            request.Result.Domains[1].Domain.Should().Be("Inventory");
            request.Result.Domains[2].Domain.Should().Be("InventoryRisk");
            request.Result.Domains[3].Domain.Should().Be("Sales");
            request.Result.Domains[4].Domain.Should().Be("Purchasing");
            request.Result.Domains[5].Domain.Should().Be("Customer");
            request.Result.Domains[6].Domain.Should().Be("Salesman");
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
                new StubCustomerWorker(),
                new StubSalesmanWorker());

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
                    RefreshLogId = "PDR0001",
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
                    RefreshLogId = "PDI0001",
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
                    RefreshLogId = "PDIR001",
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
                    RefreshLogId = "PDS0001",
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
                    RefreshLogId = "PDP0001",
                    DurationMs = 400
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
                    RefreshLogId = "PDC0001",
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
                    RefreshLogId = "PDM0001",
                    DurationMs = 600
                };
            }
        }
    }
}
