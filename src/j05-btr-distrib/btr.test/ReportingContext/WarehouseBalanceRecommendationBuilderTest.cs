using System.Collections.Generic;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class WarehouseBalanceRecommendationBuilderTest
    {
        [Fact]
        public void BuildTransferPairs_ExcludesInTransitWarehouse()
        {
            var balances = new List<StokBalanceView>
            {
                new StokBalanceView
                {
                    BrgId = "B1",
                    BrgName = "Item",
                    WarehouseId = "W1",
                    WarehouseName = "In-Transit",
                    Qty = 100
                }
            };

            var pairs = WarehouseBalanceRecommendationBuilder.BuildTransferPairs(
                balances,
                new List<BrgWarehouseConsumptionDto>(),
                new List<WarehouseModel>(),
                new HashSet<string>(),
                14,
                60,
                7,
                10);

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void BuildTransferPairs_RespectsMaxCap()
        {
            var pairs = WarehouseBalanceRecommendationBuilder.BuildTransferPairs(
                new List<StokBalanceView>(),
                new List<BrgWarehouseConsumptionDto>(),
                new List<WarehouseModel>(),
                new HashSet<string>(),
                14,
                60,
                7,
                3);

            pairs.Should().HaveCountLessOrEqualTo(3);
        }
    }
}
