using System;
using btr.application.ReportingContext.DashboardOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardOverviewAgg.Queries;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardOverviewDalTest
    {
        private static readonly DateTime SalesGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);
        private static readonly DateTime PiutangGeneratedAt = new DateTime(2026, 6, 6, 8, 15, 0);
        private static readonly DateTime InventoryGeneratedAt = new DateTime(2026, 6, 6, 9, 0, 0);

        [Fact]
        public void GetOverview_ReturnsLayerAKpisOnly_WhenAllDomainsPopulated()
        {
            var dal = new StubOverviewDal(
                sales: new DashboardOverviewSalesSection
                {
                    TotalOmzet = 5_000_000m,
                    TotalFaktur = 10,
                    TotalCustomer = 8,
                    GeneratedAt = SalesGeneratedAt
                },
                piutang: new DashboardOverviewPiutangSection
                {
                    TotalPiutang = 1_000_000m,
                    TotalCustomer = 3,
                    GeneratedAt = PiutangGeneratedAt
                },
                inventory: new DashboardOverviewInventorySection
                {
                    TotalInventoryValue = 2_500_000m,
                    TotalItem = 5,
                    GeneratedAt = InventoryGeneratedAt
                });

            var result = dal.GetOverview();

            result.HasUnavailableDomain.Should().BeFalse();
            result.Sales.TotalOmzet.Should().Be(5_000_000m);
            result.Piutang.TotalPiutang.Should().Be(1_000_000m);
            result.Inventory.TotalItem.Should().Be(5);
            result.Sales.GeneratedAt.Should().Be(SalesGeneratedAt);
            result.Piutang.GeneratedAt.Should().Be(PiutangGeneratedAt);
            result.Inventory.GeneratedAt.Should().Be(InventoryGeneratedAt);
        }

        [Fact]
        public void GetOverview_FlagsUnavailableDomain_WhenAnySnapshotMissing()
        {
            var dal = new StubOverviewDal(
                sales: new DashboardOverviewSalesSection
                {
                    TotalOmzet = 5_000_000m,
                    TotalFaktur = 10,
                    TotalCustomer = 8,
                    GeneratedAt = SalesGeneratedAt
                },
                piutang: null,
                inventory: new DashboardOverviewInventorySection
                {
                    TotalInventoryValue = 2_500_000m,
                    TotalItem = 5,
                    GeneratedAt = InventoryGeneratedAt
                });

            var result = dal.GetOverview();

            result.HasUnavailableDomain.Should().BeTrue();
            result.Sales.Should().NotBeNull();
            result.Piutang.Should().BeNull();
            result.Inventory.Should().NotBeNull();
        }

        private sealed class StubOverviewDal : IDashboardOverviewDal
        {
            private readonly DashboardOverviewSalesSection _sales;
            private readonly DashboardOverviewPiutangSection _piutang;
            private readonly DashboardOverviewInventorySection _inventory;

            public StubOverviewDal(
                DashboardOverviewSalesSection sales,
                DashboardOverviewPiutangSection piutang,
                DashboardOverviewInventorySection inventory)
            {
                _sales = sales;
                _piutang = piutang;
                _inventory = inventory;
            }

            public DashboardOverviewResponse GetOverview()
            {
                var hasUnavailable = _sales is null || _piutang is null || _inventory is null;

                return new DashboardOverviewResponse
                {
                    Sales = _sales,
                    Piutang = _piutang,
                    Inventory = _inventory,
                    HasUnavailableDomain = hasUnavailable
                };
            }
        }
    }
}
