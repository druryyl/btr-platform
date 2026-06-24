using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.Shared;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InvestigationRegistryTest
    {
        [Fact]
        public void InvestigationRegistry_ContainsAllAttentionSignalKeys()
        {
            var requiredKeys = new[]
            {
                // Customer (M17)
                DashboardCustomerAggregator.SignalOverdue,
                DashboardCustomerAggregator.SignalDormant,
                DashboardCustomerAggregator.SignalPlafondBreach,
                DashboardCustomerAggregator.SignalSuspendedWithSales,

                // Salesman (M18)
                DashboardSalesmanAggregator.SignalBelowTarget,
                DashboardSalesmanAggregator.SignalMissingTargetSetup,
                DashboardSalesmanAggregator.SignalHighOverdueExposure,
                DashboardSalesmanAggregator.SignalHighPiutangExposure,
                DashboardSalesmanAggregator.SignalCustomerConcentration,
                DashboardSalesmanAggregator.SignalDormantCustomerPortfolio,

                // Collection (M20)
                DashboardCollectionAggregator.SignalChronicOverdue,
                DashboardCollectionAggregator.SignalLegacyDebt,
                DashboardCollectionAggregator.SignalPlafondBreachOverdue,
                DashboardCollectionAggregator.SignalOverdue,
                DashboardCollectionAggregator.SignalHighOverdueWorkload,
                DashboardCollectionAggregator.SignalLowRecoveryVsBilling,
                DashboardCollectionAggregator.SignalWilayahHotspot,

                // Inventory risk (M19)
                DashboardInventoryRiskAggregator.SignalDeadStock,
                DashboardInventoryRiskAggregator.SignalSlowMoving,
                DashboardInventoryRiskAggregator.SignalNeverSold,

                // Purchasing (M21)
                DashboardPurchasingManagementAggregator.SignalQualifiedBacklog,
                DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration,
                DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration,
                DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure,
                DashboardPurchasingManagementAggregator.SignalCompoundDependency,
                DashboardPurchasingManagementAggregator.SignalPurchasingInactivity,
                DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase,
                DashboardPurchasingManagementAggregator.SignalUnknownPrincipal,

                // Location (M22)
                DashboardLocationAggregator.SignalWarehouseInventoryConcentration,
                DashboardLocationAggregator.SignalWarehouseAtRiskConcentration,
                DashboardLocationAggregator.SignalWarehouseSalesConcentration,
                DashboardLocationAggregator.SignalWarehousePurchasingConcentration,
                DashboardLocationAggregator.SignalWarehouseNoSalesWithInventory,
                DashboardLocationAggregator.SignalWarehouseInactiveWithStock,

                // Executive synthetic (M16)
                InvestigationRegistry.SignalExecutiveTopCustomerExposure,
                InvestigationRegistry.SignalExecutiveTopCategoryExposure,
                InvestigationRegistry.SignalExecutiveTopSupplierExposure,
                InvestigationRegistry.SignalExecutiveTopPrincipalExposure,

                // Legacy rankings (M11/M14/M15)
                InvestigationRegistry.SignalLegacyTopSalesman,
                InvestigationRegistry.SignalLegacyTopCustomer,
                InvestigationRegistry.SignalLegacyTopCategory,
                InvestigationRegistry.SignalLegacyTopSupplier,

                // Domain dashboard rankings (M17–M22)
                InvestigationRegistry.SignalRankingCustomerTopOmzet,
                InvestigationRegistry.SignalRankingCustomerTopPiutang,
                InvestigationRegistry.SignalRankingSalesmanTopOmzet,
                InvestigationRegistry.SignalRankingSalesmanTopAchievement,
                InvestigationRegistry.SignalRankingSalesmanTopPiutang,
                InvestigationRegistry.SignalRankingCollectionTopOverdueCustomer,
                InvestigationRegistry.SignalRankingCollectionTopOverdueSalesman,
                InvestigationRegistry.SignalRankingTopPrincipal,
            };

            foreach (var key in requiredKeys)
            {
                InvestigationRegistry.TryGet(key, out var entry).Should().BeTrue($"missing registry entry for {key}");
                entry.SignalKey.Should().Be(key);
            }
        }

        [Theory]
        [MemberData(nameof(PiutangAllOpenBalanceSignalKeys))]
        public void PiutangBoundSignals_RouteToAllOpenBalances(string signalKey)
        {
            InvestigationRegistry.TryGet(signalKey, out var entry).Should().BeTrue();
            entry.ReportRoute.Should().Be(InvestigationRegistry.PiutangReportRoute);
            entry.DefaultPeriodMode.Should().Be(InvestigationRegistry.PeriodModeAllOpenBalances);
        }

        public static IEnumerable<object[]> PiutangAllOpenBalanceSignalKeys()
        {
            yield return new object[] { DashboardCustomerAggregator.SignalOverdue };
            yield return new object[] { DashboardCollectionAggregator.SignalChronicOverdue };
            yield return new object[] { InvestigationRegistry.SignalExecutiveTopCustomerExposure };
            yield return new object[] { InvestigationRegistry.SignalLegacyTopCustomer };
            yield return new object[] { InvestigationRegistry.SignalRankingCustomerTopPiutang };
            yield return new object[] { InvestigationRegistry.SignalRankingCollectionTopOverdueCustomer };
        }

        [Fact]
        public void CompoundDependency_HasThreeInvestigationSteps()
        {
            InvestigationRegistry.TryGet(DashboardPurchasingManagementAggregator.SignalCompoundDependency, out var entry)
                .Should().BeTrue();

            entry.Steps.Should().HaveCount(3);
            entry.Steps.Select(s => s.Order).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            entry.Steps.Should().OnlyContain(step => step.Label.Contains(" · "));
        }

        [Fact]
        public void QualifiedBacklog_DefaultPostingFilter_IsBelum()
        {
            InvestigationRegistry.TryGet(DashboardPurchasingManagementAggregator.SignalQualifiedBacklog, out var entry)
                .Should().BeTrue();

            entry.DefaultPostingFilter.Should().Be(InvestigationRegistry.PostingFilterBelum);
            entry.ReportRoute.Should().Be(InvestigationRegistry.PurchasingReportRoute);
        }

        [Fact]
        public void ExecutiveTopCustomerExposure_RoutesToPiutangAllOpen()
        {
            InvestigationRegistry.TryGet(InvestigationRegistry.SignalExecutiveTopCustomerExposure, out var entry)
                .Should().BeTrue();

            entry.ReportRoute.Should().Be(InvestigationRegistry.PiutangReportRoute);
            entry.DefaultPeriodMode.Should().Be(InvestigationRegistry.PeriodModeAllOpenBalances);
        }

        [Fact]
        public void ExecutiveTopPrincipalExposure_RoutesToPurchasingReport()
        {
            InvestigationRegistry.TryGet(InvestigationRegistry.SignalExecutiveTopPrincipalExposure, out var entry)
                .Should().BeTrue();

            entry.ReportRoute.Should().Be(InvestigationRegistry.PurchasingReportRoute);
        }

        [Theory]
        [MemberData(nameof(CurrentMonthSalesSignalKeys))]
        public void SalesCurrentMonthSignals_RouteToSalesReport(string signalKey)
        {
            InvestigationRegistry.TryGet(signalKey, out var entry).Should().BeTrue();
            entry.ReportRoute.Should().Be(InvestigationRegistry.SalesReportRoute);
            entry.DefaultPeriodMode.Should().Be(InvestigationRegistry.PeriodModeCurrentMonth);
        }

        public static IEnumerable<object[]> CurrentMonthSalesSignalKeys()
        {
            yield return new object[] { DashboardSalesmanAggregator.SignalBelowTarget };
            yield return new object[] { InvestigationRegistry.SignalLegacyTopSalesman };
            yield return new object[] { InvestigationRegistry.SignalRankingCustomerTopOmzet };
        }
    }
}
