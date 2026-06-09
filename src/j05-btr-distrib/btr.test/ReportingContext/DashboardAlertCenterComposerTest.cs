using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Services;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardAlertCenterComposerTest
    {
        private readonly DashboardAlertCenterComposer _composer = new DashboardAlertCenterComposer();
        private readonly DateTime _utcNow = new DateTime(2026, 6, 9, 10, 0, 0, DateTimeKind.Utc);
        private readonly DashboardSnapshotOptions _options = new DashboardSnapshotOptions();

        [Fact]
        public void Compose_M20ChronicOverdue_SuppressesM17OverdueForSameCustomer()
        {
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C001",
                    CustomerName = "CV Maju",
                    SignalKey = DashboardCustomerAggregator.SignalOverdue,
                    SignalLabel = "Overdue",
                    ValueAmount = 100m,
                    SortOrder = 1
                }
            };
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer",
                    EntityId = "C001",
                    EntityCode = "C001",
                    EntityName = "CV Maju",
                    SignalKey = DashboardCollectionAggregator.SignalChronicOverdue,
                    SignalLabel = "Chronic Overdue",
                    ValueAmount = 450m,
                    SortOrder = 1
                }
            };

            var result = Compose(input);
            var collectionAlerts = GetCategoryAlerts(result, AlertCenterRegistry.CategoryCollection);
            var customerAlerts = GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer);

            collectionAlerts.Should().ContainSingle(a => a.EntityName == "CV Maju");
            customerAlerts.Should().BeEmpty();
        }

        [Fact]
        public void Compose_M20LegacyDebt_SuppressesM17DormantForSameCustomer()
        {
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C002",
                    CustomerName = "PT ABC",
                    SignalKey = DashboardCustomerAggregator.SignalDormant,
                    SignalLabel = "Dormant",
                    SortOrder = 2
                }
            };
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer",
                    EntityId = "C002",
                    EntityCode = "C002",
                    EntityName = "PT ABC",
                    SignalKey = DashboardCollectionAggregator.SignalLegacyDebt,
                    SignalLabel = "Legacy Debt",
                    ValueAmount = 200m,
                    SortOrder = 3
                }
            };

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer).Should().BeEmpty();
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCollection)
                .Should().ContainSingle(a => a.SignalKey == DashboardCollectionAggregator.SignalLegacyDebt);
        }

        [Fact]
        public void Compose_M20PlafondBreachOverdue_SuppressesM17PlafondBreach()
        {
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C003",
                    CustomerName = "Toko X",
                    SignalKey = DashboardCustomerAggregator.SignalPlafondBreach,
                    SignalLabel = "Plafond Breach",
                    SortOrder = 3
                }
            };
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer",
                    EntityId = "C003",
                    EntityCode = "C003",
                    EntityName = "Toko X",
                    SignalKey = DashboardCollectionAggregator.SignalPlafondBreachOverdue,
                    SignalLabel = "Plafond Breach + Overdue",
                    SortOrder = 2
                }
            };

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer).Should().BeEmpty();
        }

        [Fact]
        public void Compose_M20HighOverdueWorkload_SuppressesM18HighOverdueExposure()
        {
            var input = FullInput();
            input.Salesman.AttentionList = new List<DashboardSalesmanAttentionRow>
            {
                new DashboardSalesmanAttentionRow
                {
                    SalesPersonId = "S001",
                    SalesPersonCode = "S001",
                    SalesPersonName = "Budi",
                    SignalKey = DashboardSalesmanAggregator.SignalHighOverdueExposure,
                    SignalLabel = "High Overdue Exposure",
                    SortOrder = 3
                }
            };
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Salesman",
                    EntityId = "S001",
                    EntityCode = "S001",
                    EntityName = "Budi",
                    SignalKey = DashboardCollectionAggregator.SignalHighOverdueWorkload,
                    SignalLabel = "High Overdue Workload",
                    SortOrder = 5
                }
            };

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategorySales)
                .Should().NotContain(a => a.SignalKey == DashboardSalesmanAggregator.SignalHighOverdueExposure);
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCollection)
                .Should().ContainSingle(a => a.SignalKey == DashboardCollectionAggregator.SignalHighOverdueWorkload);
        }

        [Fact]
        public void Compose_M17SuspendedWithSales_NotSuppressedByM20()
        {
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C004",
                    CustomerName = "Suspended Co",
                    SignalKey = DashboardCustomerAggregator.SignalSuspendedWithSales,
                    SignalLabel = "Suspended + Sales",
                    SortOrder = 4
                }
            };
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer",
                    EntityId = "C004",
                    EntityCode = "C004",
                    EntityName = "Suspended Co",
                    SignalKey = DashboardCollectionAggregator.SignalOverdue,
                    SignalLabel = "Overdue",
                    SortOrder = 4
                }
            };

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer)
                .Should().ContainSingle(a => a.SignalKey == DashboardCustomerAggregator.SignalSuspendedWithSales);
        }

        [Fact]
        public void Compose_DifferentEntityGrains_BothAppear()
        {
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C005",
                    CustomerName = "Customer Only",
                    SignalKey = DashboardCustomerAggregator.SignalDormant,
                    SortOrder = 2
                }
            };
            input.Salesman.AttentionList = new List<DashboardSalesmanAttentionRow>
            {
                new DashboardSalesmanAttentionRow
                {
                    SalesPersonId = "S002",
                    SalesPersonCode = "S002",
                    SalesPersonName = "Andi",
                    SignalKey = DashboardSalesmanAggregator.SignalBelowTarget,
                    SortOrder = 2
                }
            };

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer).Should().HaveCount(1);
            GetCategoryAlerts(result, AlertCenterRegistry.CategorySales).Should().HaveCount(1);
        }

        [Fact]
        public void Compose_CategoryCap_TwentyFiveRows_ShowsTwentyWithHasMore()
        {
            var input = FullInput();
            input.Collection.AttentionList = Enumerable.Range(1, 25)
                .Select(i => new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer",
                    EntityId = $"C{i:D3}",
                    EntityCode = $"C{i:D3}",
                    EntityName = $"Customer {i}",
                    SignalKey = DashboardCollectionAggregator.SignalOverdue,
                    SignalLabel = "Overdue",
                    ValueAmount = 1000m - i,
                    SortOrder = i
                })
                .ToList();

            var result = Compose(input);
            var summary = result.CategorySummaries
                .First(s => s.Category == AlertCenterRegistry.CategoryCollection);

            summary.TotalCount.Should().Be(25);
            summary.DisplayedCount.Should().Be(20);
            summary.HasMore.Should().BeTrue();
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCollection).Should().HaveCount(20);
        }

        [Fact]
        public void Compose_M19ItemRows_NeverAppearInAlertGroups()
        {
            var input = FullInput();
            input.InventoryRisk.AttentionList = new List<DashboardInventoryRiskAttentionRow>
            {
                new DashboardInventoryRiskAttentionRow
                {
                    BrgId = "B001",
                    BrgName = "SKU Dead",
                    SignalKey = DashboardInventoryRiskAggregator.SignalDeadStock,
                    SortOrder = 1
                }
            };

            var result = Compose(input);
            result.AlertGroups.SelectMany(g => g.Alerts)
                .Should().NotContain(a => a.SignalKey == DashboardInventoryRiskAggregator.SignalDeadStock);
        }

        [Fact]
        public void Compose_M19Summary_PopulatedFromKpi()
        {
            var input = FullInput();
            input.InventoryRisk.DeadStockItemCount = 142;
            input.InventoryRisk.DeadStockValue = 5000000m;
            input.InventoryRisk.SlowMovingItemCount = 386;
            input.InventoryRisk.AtRiskInventoryPercent = 18m;

            var result = Compose(input);

            result.InventoryRiskSummary.IsAvailable.Should().BeTrue();
            result.InventoryRiskSummary.DeadStockItemCount.Should().Be(142);
            result.InventoryRiskSummary.AtRiskInventoryPercent.Should().Be(18m);
            result.InventoryRiskSummary.DashboardRoute.Should().Be("/dashboard/inventory-risk");
        }

        [Fact]
        public void Compose_SalesCriticalBand_CreatesSyntheticAlert()
        {
            var input = FullInput();
            input.Sales.AchievementPercent = 75m;

            var result = Compose(input);
            var salesAlerts = GetCategoryAlerts(result, AlertCenterRegistry.CategorySales);

            salesAlerts.Should().ContainSingle(a =>
                a.SignalKey == AlertCenterRegistry.SignalSalesAchievementCritical
                && a.AchievementBand == ExecutiveSalesAchievementBandResolver.Critical);
        }

        [Fact]
        public void Compose_SalesWarningBand_CreatesSyntheticAlert()
        {
            var input = FullInput();
            input.Sales.AchievementPercent = 92m;

            var result = Compose(input);
            GetCategoryAlerts(result, AlertCenterRegistry.CategorySales)
                .Should().ContainSingle(a =>
                    a.SignalKey == AlertCenterRegistry.SignalSalesAchievementWarning
                    && a.AchievementBand == ExecutiveSalesAchievementBandResolver.Warning);
        }

        [Fact]
        public void Compose_PlatformDegraded_PinnedInPlatformSection()
        {
            var input = FullInput();
            input.RefreshStatuses = new List<DashboardSnapshotRefreshStatusModel>
            {
                new DashboardSnapshotRefreshStatusModel { Domain = "Sales", Status = "Failed" }
            };

            var result = Compose(input);

            result.PlatformAlerts.Should().Contain(a =>
                a.SignalKey == AlertCenterRegistry.SignalSnapshotDegraded);
            result.PlatformAlerts.First().SignalKey.Should().Be(AlertCenterRegistry.SignalSnapshotDegraded);
        }

        [Fact]
        public void Compose_CustomerConcentration_InConcentrationsNotAlerts()
        {
            var input = FullInput();
            input.Salesman.AttentionList = new List<DashboardSalesmanAttentionRow>
            {
                new DashboardSalesmanAttentionRow
                {
                    SalesPersonId = "S003",
                    SalesPersonCode = "S003",
                    SalesPersonName = "Citra",
                    SignalKey = DashboardSalesmanAggregator.SignalCustomerConcentration,
                    SignalLabel = "Customer Concentration",
                    ValueAmount = 65m,
                    ValueText = "65%",
                    SortOrder = 5
                }
            };

            var result = Compose(input);

            result.AlertGroups.SelectMany(g => g.Alerts)
                .Should().NotContain(a => a.SignalKey == DashboardSalesmanAggregator.SignalCustomerConcentration);
            result.Concentrations.Should().Contain(c => c.Label.Contains("Customer Concentration"));
        }

        [Fact]
        public void Compose_WarehouseConcentration_InConcentrationsOnly()
        {
            var input = FullInput();
            input.Location.AttentionList = new List<DashboardLocationAttentionRow>
            {
                new DashboardLocationAttentionRow
                {
                    EntityType = "Warehouse",
                    EntityCode = "WH01",
                    EntityName = "Gudang Timur",
                    SignalKey = DashboardLocationAggregator.SignalWarehouseInventoryConcentration,
                    SignalLabel = "Inventory Concentration",
                    ValueAmount = 40m,
                    SortOrder = 4
                },
                new DashboardLocationAttentionRow
                {
                    EntityType = "Warehouse",
                    EntityCode = "WH02",
                    EntityName = "Gudang Barat",
                    SignalKey = DashboardLocationAggregator.SignalWarehouseInactiveWithStock,
                    SignalLabel = "Inactive With Stock",
                    ValueAmount = 80000000m,
                    SortOrder = 1
                }
            };

            var result = Compose(input);

            GetCategoryAlerts(result, AlertCenterRegistry.CategoryLocation)
                .Should().ContainSingle(a => a.SignalKey == DashboardLocationAggregator.SignalWarehouseInactiveWithStock);
            result.Concentrations.Should().Contain(c => c.Label.Contains("Inventory Concentration"));
        }

        [Fact]
        public void Compose_ProducerSortOrder_PreservedWithinCategory()
        {
            var input = FullInput();
            input.Collection.AttentionList = new List<DashboardCollectionAttentionRow>
            {
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer", EntityId = "C1", EntityCode = "C1", EntityName = "Zeta",
                    SignalKey = DashboardCollectionAggregator.SignalOverdue, SortOrder = 10
                },
                new DashboardCollectionAttentionRow
                {
                    EntityType = "Customer", EntityId = "C2", EntityCode = "C2", EntityName = "Alpha",
                    SignalKey = DashboardCollectionAggregator.SignalChronicOverdue, SortOrder = 1
                }
            };

            var result = Compose(input);
            var alerts = GetCategoryAlerts(result, AlertCenterRegistry.CategoryCollection);

            alerts.First().SignalKey.Should().Be(DashboardCollectionAggregator.SignalChronicOverdue);
        }

        [Fact]
        public void Compose_CompoundDependency_RouteIsPurchasingDashboard()
        {
            var input = FullInput();
            input.PurchasingManagement.AttentionList = new List<DashboardPurchasingManagementAttentionRow>
            {
                new DashboardPurchasingManagementAttentionRow
                {
                    EntityType = "Principal",
                    EntityName = "Principal X",
                    SignalKey = DashboardPurchasingManagementAggregator.SignalCompoundDependency,
                    SignalLabel = "Compound Dependency",
                    SortOrder = 2
                }
            };

            var result = Compose(input);
            var alert = GetCategoryAlerts(result, AlertCenterRegistry.CategoryPurchasing).Single();

            alert.DashboardRoute.Should().Be("/dashboard/purchasing");
        }

        [Fact]
        public void Compose_UnknownSignalKey_SkippedComposeSucceeds()
        {
            // Unknown keys are skipped and a warning is logged at runtime (Plan §5.3).
            var input = FullInput();
            input.Customer.AttentionList = new List<DashboardCustomerAttentionRow>
            {
                new DashboardCustomerAttentionRow
                {
                    CustomerCode = "C999",
                    CustomerName = "Unknown Signal",
                    SignalKey = "FutureSignalKey",
                    SortOrder = 1
                }
            };

            var result = Compose(input);

            result.IsAvailable.Should().BeTrue();
            GetCategoryAlerts(result, AlertCenterRegistry.CategoryCustomer).Should().BeEmpty();
        }

        private AlertCenterComposeInput FullInput()
        {
            var generatedAt = _utcNow.AddMinutes(-5);

            return new AlertCenterComposeInput
            {
                UtcNow = _utcNow,
                Options = _options,
                RefreshStatuses = new List<DashboardSnapshotRefreshStatusModel>
                {
                    new DashboardSnapshotRefreshStatusModel { Domain = "Sales", Status = "Success" }
                },
                Sales = new DashboardSalesAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AchievementPercent = 105m,
                    TotalAchievement = 5000000m
                },
                Piutang = new DashboardPiutangAggregateResult
                {
                    GeneratedAt = generatedAt,
                    TotalPiutang = 10000000m,
                    TopCustomers = new List<DashboardPiutangTopCustomer>()
                },
                Inventory = new DashboardInventoryAggregateResult { GeneratedAt = generatedAt },
                Purchasing = new DashboardPurchasingAggregateResult { GeneratedAt = generatedAt },
                Customer = new DashboardCustomerAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardCustomerAttentionRow>()
                },
                Salesman = new DashboardSalesmanAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardSalesmanAttentionRow>()
                },
                Collection = new DashboardCollectionAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardCollectionAttentionRow>()
                },
                InventoryRisk = new DashboardInventoryRiskAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardInventoryRiskAttentionRow>()
                },
                PurchasingManagement = new DashboardPurchasingManagementAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardPurchasingManagementAttentionRow>()
                },
                Location = new DashboardLocationAggregateResult
                {
                    GeneratedAt = generatedAt,
                    AttentionList = new List<DashboardLocationAttentionRow>()
                }
            };
        }

        private static IList<btr.application.ReportingContext.DashboardAlertCenterAgg.Queries.DashboardAlertCenterAlertRow> GetCategoryAlerts(
            btr.application.ReportingContext.DashboardAlertCenterAgg.Queries.DashboardAlertCenterResponse result,
            string category)
        {
            return result.AlertGroups
                .FirstOrDefault(g => g.Category == category)
                ?.Alerts ?? new List<btr.application.ReportingContext.DashboardAlertCenterAgg.Queries.DashboardAlertCenterAlertRow>();
        }

        private btr.application.ReportingContext.DashboardAlertCenterAgg.Queries.DashboardAlertCenterResponse Compose(
            AlertCenterComposeInput input)
        {
            return _composer.Compose(input);
        }
    }
}
