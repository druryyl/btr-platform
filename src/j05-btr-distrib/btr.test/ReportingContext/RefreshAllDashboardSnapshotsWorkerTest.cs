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
            var salesWorker = new StubSalesWorker();
            var worker = new RefreshAllDashboardSnapshotsWorker(
                piutangWorker,
                inventoryWorker,
                salesWorker);

            var request = new RefreshAllDashboardSnapshotsRequest
            {
                TriggeredBy = "Manual"
            };

            worker.Execute(request);

            piutangWorker.WasCalled.Should().BeTrue();
            inventoryWorker.WasCalled.Should().BeTrue();
            salesWorker.WasCalled.Should().BeTrue();
            piutangWorker.CallOrder.Should().BeLessThan(inventoryWorker.CallOrder);
            inventoryWorker.CallOrder.Should().BeLessThan(salesWorker.CallOrder);
            request.Result.Domains.Should().HaveCount(3);
            request.Result.Domains[0].Domain.Should().Be("Piutang");
            request.Result.Domains[1].Domain.Should().Be("Inventory");
            request.Result.Domains[2].Domain.Should().Be("Sales");
        }

        [Fact]
        public void Execute_ThrowsAggregateException_WhenOneDomainFails()
        {
            var worker = new RefreshAllDashboardSnapshotsWorker(
                new StubPiutangWorker(),
                new StubInventoryWorker { ShouldFail = true },
                new StubSalesWorker());

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
    }
}
