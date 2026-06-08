using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Commands;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class RefreshDashboardSnapshotsHandlerTest
    {
        [Fact]
        public void Handle_AllDomain_UsesOrchestratorWithManualTrigger()
        {
            var allWorker = new StubAllWorker();
            var handler = CreateHandler(allWorker);

            var response = handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "All" },
                default).GetAwaiter().GetResult();

            allWorker.WasCalled.Should().BeTrue();
            allWorker.LastTriggeredBy.Should().Be("Manual");
            response.Domain.Should().Be("All");
            response.TriggeredBy.Should().Be("Manual");
            response.Domains.Should().HaveCount(2);
        }

        [Fact]
        public void Handle_SingleDomain_UsesDomainWorker()
        {
            var piutangWorker = new StubPiutangWorker();
            var handler = CreateHandler(piutangWorker: piutangWorker);

            var response = handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "Piutang" },
                default).GetAwaiter().GetResult();

            piutangWorker.WasCalled.Should().BeTrue();
            piutangWorker.LastTriggeredBy.Should().Be("Manual");
            response.Domain.Should().Be("Piutang");
            response.Domains.Should().ContainSingle()
                .Which.RefreshLogId.Should().Be("PDR0001");
        }

        [Fact]
        public void Handle_SingleDomain_AcceptsCaseInsensitiveDomain()
        {
            var piutangWorker = new StubPiutangWorker();
            var handler = CreateHandler(piutangWorker: piutangWorker);

            var response = handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "piutang" },
                default).GetAwaiter().GetResult();

            piutangWorker.WasCalled.Should().BeTrue();
            response.Domain.Should().Be("Piutang");
        }

        [Fact]
        public void Handle_InvalidDomain_ThrowsArgumentException()
        {
            var handler = CreateHandler();

            Action act = () => handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "Unknown" },
                default).GetAwaiter().GetResult();

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Domain must be All, Piutang, Inventory, Sales, Purchasing, or Customer*");
        }

        [Fact]
        public void Handle_PurchasingDomain_UsesPurchasingWorker()
        {
            var purchasingWorker = new StubPurchasingWorker();
            var handler = CreateHandler(purchasingWorker: purchasingWorker);

            var response = handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "Purchasing" },
                default).GetAwaiter().GetResult();

            purchasingWorker.WasCalled.Should().BeTrue();
            purchasingWorker.LastTriggeredBy.Should().Be("Manual");
            response.Domain.Should().Be("Purchasing");
            response.Domains.Should().ContainSingle()
                .Which.RefreshLogId.Should().Be("PDR0001");
        }

        [Fact]
        public void Handle_CustomerDomain_UsesCustomerWorker()
        {
            var customerWorker = new StubCustomerWorker();
            var handler = CreateHandler(customerWorker: customerWorker);

            var response = handler.Handle(
                new RefreshDashboardSnapshotsCommand { Domain = "Customer" },
                default).GetAwaiter().GetResult();

            customerWorker.WasCalled.Should().BeTrue();
            customerWorker.LastTriggeredBy.Should().Be("Manual");
            response.Domain.Should().Be("Customer");
            response.Domains.Should().ContainSingle()
                .Which.RefreshLogId.Should().Be("PDC0001");
        }

        private static RefreshDashboardSnapshotsHandler CreateHandler(
            StubAllWorker allWorker = null,
            StubPiutangWorker piutangWorker = null,
            StubInventoryWorker inventoryWorker = null,
            StubSalesWorker salesWorker = null,
            StubPurchasingWorker purchasingWorker = null,
            StubCustomerWorker customerWorker = null)
        {
            return new RefreshDashboardSnapshotsHandler(
                allWorker ?? new StubAllWorker(),
                piutangWorker ?? new StubPiutangWorker(),
                inventoryWorker ?? new StubInventoryWorker(),
                salesWorker ?? new StubSalesWorker(),
                purchasingWorker ?? new StubPurchasingWorker(),
                customerWorker ?? new StubCustomerWorker());
        }

        private sealed class StubAllWorker : IRefreshAllDashboardSnapshotsWorker
        {
            public bool WasCalled { get; private set; }

            public string LastTriggeredBy { get; private set; }

            public void Execute(RefreshAllDashboardSnapshotsRequest request)
            {
                WasCalled = true;
                LastTriggeredBy = request.TriggeredBy;
                request.Result = new RefreshAllDashboardSnapshotsResult
                {
                    Domains =
                    {
                        new RefreshDashboardDomainResult { Domain = "Piutang" },
                        new RefreshDashboardDomainResult { Domain = "Sales" }
                    }
                };
            }
        }

        private sealed class StubPiutangWorker : IRefreshDashboardPiutangSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public string LastTriggeredBy { get; private set; }

            public void Execute(RefreshDashboardPiutangSnapshotRequest request)
            {
                WasCalled = true;
                LastTriggeredBy = request.TriggeredBy;
                request.Result = new RefreshDashboardPiutangSnapshotResult
                {
                    RefreshLogId = "PDR0001",
                    DurationMs = 120
                };
            }
        }

        private sealed class StubInventoryWorker : IRefreshDashboardInventorySnapshotWorker
        {
            public void Execute(RefreshDashboardInventorySnapshotRequest request)
            {
            }
        }

        private sealed class StubSalesWorker : IRefreshDashboardSalesSnapshotWorker
        {
            public void Execute(RefreshDashboardSalesSnapshotRequest request)
            {
            }
        }

        private sealed class StubPurchasingWorker : IRefreshDashboardPurchasingSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public string LastTriggeredBy { get; private set; }

            public void Execute(RefreshDashboardPurchasingSnapshotRequest request)
            {
                WasCalled = true;
                LastTriggeredBy = request.TriggeredBy;
                request.Result = new RefreshDashboardPurchasingSnapshotResult
                {
                    RefreshLogId = "PDR0001",
                    DurationMs = 90
                };
            }
        }

        private sealed class StubCustomerWorker : IRefreshDashboardCustomerSnapshotWorker
        {
            public bool WasCalled { get; private set; }

            public string LastTriggeredBy { get; private set; }

            public void Execute(RefreshDashboardCustomerSnapshotRequest request)
            {
                WasCalled = true;
                LastTriggeredBy = request.TriggeredBy;
                request.Result = new RefreshDashboardCustomerSnapshotResult
                {
                    RefreshLogId = "PDC0001",
                    DurationMs = 95
                };
            }
        }
    }
}
