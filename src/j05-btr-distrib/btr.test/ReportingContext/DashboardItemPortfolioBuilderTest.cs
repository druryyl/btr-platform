using System;
using System.Collections.Generic;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardItemPortfolioBuilderTest
    {
        private readonly DashboardItemPortfolioBuilder _builder = new DashboardItemPortfolioBuilder();

        [Fact]
        public void Build_ComputesRecommendedPurchaseValueFromOnHandCost()
        {
            var itemGroups = new List<DashboardInventoryItemGroup>
            {
                new DashboardInventoryItemGroup
                {
                    BrgId = "B001",
                    BrgCode = "BRG1",
                    BrgName = "Item One",
                    Qty = 100m,
                    InventoryValue = 250_000m,
                    MasterHpp = 2_500m,
                    CategoryName = "Category A",
                    SupplierName = "Supplier A"
                }
            };

            var forecastContexts = new List<ForecastItemContext>
            {
                new ForecastItemContext
                {
                    Item = itemGroups[0],
                    Calculation = new InventoryForecastCalculation
                    {
                        RecommendedPurchaseQty = 10m,
                        DaysOfSupply = 12m
                    },
                    IsForecastEligible = true
                }
            };

            var portfolio = _builder.Build(
                itemGroups,
                forecastContexts,
                new DashboardInventoryRiskAggregateResult(),
                Array.Empty<SalesmanMtdItemRollupDto>(),
                Array.Empty<BrgLastFakturDto>(),
                new DateTime(2026, 6, 24));

            portfolio.Should().ContainSingle();
            portfolio[0].RecommendedPurchaseQty.Should().Be(10m);
            portfolio[0].RecommendedPurchaseValue.Should().Be(25_000m);
        }

        [Fact]
        public void Build_UsesMasterHppFallbackWhenStockOut()
        {
            var forecastContexts = new List<ForecastItemContext>
            {
                new ForecastItemContext
                {
                    Item = new DashboardInventoryItemGroup
                    {
                        BrgId = "B002",
                        BrgCode = "BRG2",
                        BrgName = "Item Two"
                    },
                    Calculation = new InventoryForecastCalculation
                    {
                        RecommendedPurchaseQty = 8m,
                        DaysOfSupply = 2m
                    },
                    IsForecastEligible = true
                }
            };

            var stockRows = new List<StokBalanceView>
            {
                new StokBalanceView
                {
                    BrgId = "B002",
                    BrgCode = "BRG2",
                    BrgName = "Item Two",
                    Hpp = 4_500m,
                    Qty = 0
                }
            };

            var portfolio = _builder.Build(
                Array.Empty<DashboardInventoryItemGroup>(),
                forecastContexts,
                new DashboardInventoryRiskAggregateResult(),
                Array.Empty<SalesmanMtdItemRollupDto>(),
                new List<BrgLastFakturDto>
                {
                    new BrgLastFakturDto
                    {
                        BrgId = "B002",
                        BrgCode = "BRG2",
                        BrgName = "Item Two",
                        LastFakturDate = new DateTime(2026, 6, 20)
                    }
                },
                new DateTime(2026, 6, 24),
                stockRows);

            portfolio.Should().ContainSingle();
            portfolio[0].Qty.Should().Be(0m);
            portfolio[0].MasterHpp.Should().Be(4_500m);
            portfolio[0].RecommendedPurchaseValue.Should().Be(36_000m);
        }
    }
}
