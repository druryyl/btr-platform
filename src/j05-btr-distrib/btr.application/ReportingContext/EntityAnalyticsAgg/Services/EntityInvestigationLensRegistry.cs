using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Declarative lens configuration for the Principal Investigation Workspace.
    /// Two lenses (Sales-Out default, Purchasing) investigate the same Supplier identity.
    /// The configuration introduces no new entity type, KPI ID, or profile.
    /// </summary>
    public static class EntityInvestigationLensRegistry
    {
        public const string PrincipalSalesOutMapPresetId = "principal-sales-out-map";

        public const string PurchaseExposureMapPresetId = "purchase-exposure-map";

        private static readonly IReadOnlyList<EntityInvestigationLensDefinition> Lenses = new[]
        {
            new EntityInvestigationLensDefinition
            {
                LensId = EntityInvestigationLensIds.SalesOut,
                EntityType = EntityTypeCode.Supplier,
                DisplayName = "Sales-Out",
                IsDefault = true,
                DefaultPresetId = PrincipalSalesOutMapPresetId,
                KpiIds = new[]
                {
                    PrincipalKpiCatalog.SalesOutId,
                    PrincipalKpiCatalog.MomGrowthId,
                    PrincipalKpiCatalog.YoyGrowthId,
                    PrincipalKpiCatalog.YoyMtdGrowthId,
                    PrincipalKpiCatalog.GoodReturnAmountId,
                    PrincipalKpiCatalog.BrokenReturnAmountId,
                    PrincipalKpiCatalog.TotalReturnAmountId,
                    PrincipalKpiCatalog.ReturnPercentageId,
                    PrincipalKpiCatalog.AchievementAmountId,
                    PrincipalKpiCatalog.AchievementPercentageId,
                    PrincipalKpiCatalog.PacingAchievementPercentageId,
                    PrincipalKpiCatalog.ActiveCustomerCountId,
                    PrincipalKpiCatalog.CustomerCoverageId
                },
                DerivedMetricIds = Array.Empty<string>(),
                AttentionCategories = new[]
                {
                    "Sales-Out Decline",
                    "Growth Deterioration",
                    "Target Miss",
                    "Return Risk",
                    "Coverage Deterioration"
                },
                RelationshipDrivers = new[]
                {
                    SupplierRelationshipCatalog.TopCustomersByOmzet,
                    SupplierRelationshipCatalog.TopSalesmenByOmzet,
                    SupplierRelationshipCatalog.TopProductsByOmzet
                },
                EvidenceRoutes = new[]
                {
                    SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute,
                    SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceRoute,
                    SupplierEntityAnalyticsRegistrar.PrincipalTargetEvidenceRoute
                }
            },
            new EntityInvestigationLensDefinition
            {
                LensId = EntityInvestigationLensIds.Purchasing,
                EntityType = EntityTypeCode.Supplier,
                DisplayName = "Purchasing",
                IsDefault = false,
                DefaultPresetId = PurchaseExposureMapPresetId,
                KpiIds = new[]
                {
                    PrincipalKpiCatalog.PurchaseInId,
                    "PU-KPI-001",
                    PrincipalKpiCatalog.InventoryValueId,
                    PrincipalKpiCatalog.InventoryDaysId
                },
                DerivedMetricIds = new[]
                {
                    EntityInvestigationDerivedMetricIds.PurchaseToSalesOutRatio
                },
                AttentionCategories = new[]
                {
                    "Qualified Backlog",
                    "Spend Concentration",
                    "Inventory Concentration",
                    "At-Risk Exposure",
                    "Compound Dependency",
                    "Inventory, No Purchase",
                    "Unknown Principal"
                },
                RelationshipDrivers = new[]
                {
                    SupplierRelationshipCatalog.TopPurchasedItems,
                    SupplierRelationshipCatalog.PurchaseHistory
                },
                EvidenceRoutes = new[]
                {
                    "/reports/purchasing",
                    "/reports/inventory"
                }
            }
        };

        public static IReadOnlyList<EntityInvestigationLensDefinition> GetLensesForEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<EntityInvestigationLensDefinition>();

            return Lenses
                .Where(l => string.Equals(l.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static EntityInvestigationLensDefinition TryGetLens(string entityType, string lensId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(lensId))
                return null;

            return Lenses.FirstOrDefault(l =>
                string.Equals(l.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(l.LensId, lensId, StringComparison.OrdinalIgnoreCase));
        }

        public static EntityInvestigationLensDefinition ResolveDefaultLens(string entityType)
        {
            var lenses = GetLensesForEntityType(entityType);
            return lenses.FirstOrDefault(l => l.IsDefault) ?? lenses.FirstOrDefault();
        }
    }
}
