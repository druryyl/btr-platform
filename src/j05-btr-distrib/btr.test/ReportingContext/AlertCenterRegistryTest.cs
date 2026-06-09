using System.Linq;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class AlertCenterRegistryTest
    {
        [Theory]
        [InlineData(DashboardCustomerAggregator.SignalOverdue)]
        [InlineData(DashboardCustomerAggregator.SignalDormant)]
        [InlineData(DashboardCustomerAggregator.SignalPlafondBreach)]
        [InlineData(DashboardCustomerAggregator.SignalSuspendedWithSales)]
        [InlineData(DashboardSalesmanAggregator.SignalBelowTarget)]
        [InlineData(DashboardSalesmanAggregator.SignalNoTarget)]
        [InlineData(DashboardSalesmanAggregator.SignalHighOverdueExposure)]
        [InlineData(DashboardSalesmanAggregator.SignalHighPiutangExposure)]
        [InlineData(DashboardSalesmanAggregator.SignalCustomerConcentration)]
        [InlineData(DashboardSalesmanAggregator.SignalDormantCustomerPortfolio)]
        [InlineData(DashboardCollectionAggregator.SignalChronicOverdue)]
        [InlineData(DashboardCollectionAggregator.SignalLegacyDebt)]
        [InlineData(DashboardCollectionAggregator.SignalPlafondBreachOverdue)]
        [InlineData(DashboardCollectionAggregator.SignalHighOverdueWorkload)]
        [InlineData(DashboardCollectionAggregator.SignalLowRecoveryVsBilling)]
        [InlineData(DashboardCollectionAggregator.SignalWilayahHotspot)]
        [InlineData(DashboardInventoryRiskAggregator.SignalDeadStock)]
        [InlineData(DashboardInventoryRiskAggregator.SignalSlowMoving)]
        [InlineData(DashboardInventoryRiskAggregator.SignalNeverSold)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalQualifiedBacklog)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalCompoundDependency)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalPurchasingInactivity)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase)]
        [InlineData(DashboardPurchasingManagementAggregator.SignalUnknownPrincipal)]
        [InlineData(DashboardLocationAggregator.SignalWarehouseInventoryConcentration)]
        [InlineData(DashboardLocationAggregator.SignalWarehouseAtRiskConcentration)]
        [InlineData(DashboardLocationAggregator.SignalWarehouseSalesConcentration)]
        [InlineData(DashboardLocationAggregator.SignalWarehousePurchasingConcentration)]
        [InlineData(DashboardLocationAggregator.SignalWarehouseNoSalesWithInventory)]
        [InlineData(DashboardLocationAggregator.SignalWarehouseInactiveWithStock)]
        public void Registry_ContainsAllProducerSignalKeys(string signalKey)
        {
            AlertCenterRegistry.TryGet(signalKey, out var entry).Should().BeTrue();
            entry.SignalKey.Should().Be(signalKey);
        }

        [Theory]
        [InlineData(DashboardInventoryRiskAggregator.SignalDeadStock)]
        [InlineData(DashboardInventoryRiskAggregator.SignalSlowMoving)]
        [InlineData(DashboardInventoryRiskAggregator.SignalNeverSold)]
        public void Registry_M19ItemSignals_AreNotInAlertsSection(string signalKey)
        {
            AlertCenterRegistry.TryGet(signalKey, out var entry).Should().BeTrue();
            entry.Section.Should().Be(AlertCenterSection.InventorySummaryExcluded);
        }

        [Fact]
        public void Registry_M20Overdue_ResolvesToCollectionCategory()
        {
            AlertCenterRegistry.TryGetForProducer("M20", DashboardCollectionAggregator.SignalOverdue, out var entry)
                .Should().BeTrue();
            entry.Category.Should().Be(AlertCenterRegistry.CategoryCollection);
        }

        [Fact]
        public void Registry_ConcentrationSignals_AreInConcentrationsSection()
        {
            AlertCenterRegistry.TryGet(DashboardSalesmanAggregator.SignalCustomerConcentration, out var entry).Should().BeTrue();
            entry.Section.Should().Be(AlertCenterSection.Concentrations);
        }

        [Fact]
        public void Registry_PlatformSignals_AreInPlatformSection()
        {
            AlertCenterRegistry.TryGet(AlertCenterRegistry.SignalSnapshotStale, out var entry).Should().BeTrue();
            entry.Section.Should().Be(AlertCenterSection.Platform);
        }

        [Fact]
        public void Registry_CategoryDisplayOrder_HasSixCategories()
        {
            AlertCenterRegistry.CategoryDisplayOrder.Should().HaveCount(6);
            AlertCenterRegistry.CategoryDisplayOrder.First().Should().Be(AlertCenterRegistry.CategorySales);
        }
    }
}
