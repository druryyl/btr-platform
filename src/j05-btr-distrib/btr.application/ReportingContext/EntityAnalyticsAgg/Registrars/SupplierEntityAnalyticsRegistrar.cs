using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public class SupplierEntityAnalyticsRegistrar : IEntityAnalyticsRegistrar
    {
        public const string KpiPackId = "supplier-default";

        public void Register(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels)
        {
            RegisterKpiMetadata(kpiRegistry);
            RegisterDimensions(dimensionLabels);

            kpiRegistry.RegisterPack(KpiPackId, new[]
            {
                "PU-KPI-001",
                "PU-KPI-002",
                "PU-KPI-003",
                EntityAnalyticsRadarAxisIds.GrowthMom,
                EntityAnalyticsRadarAxisIds.AttentionRisk,
                EntityAnalyticsMetaKpiIds.InventoryValue,
                EntityAnalyticsMetaKpiIds.ActiveSkuCount,
                EntityAnalyticsMetaKpiIds.CatalogPenetration
            });
        }

        private static void RegisterDimensions(IDimensionLabelRegistry dimensionLabels)
        {
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.PurchaseShare, "Purchase Share");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.InventoryValue, "Inventory Value");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AtRiskValue, "At-Risk Value");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.ActiveMtd, "Active MTD");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.ActiveSkuCount, "Active SKU Count");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.CatalogPenetration, "Catalog Penetration");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AttentionSignals, "Attention Signals");
        }

        private static void RegisterDim(IDimensionLabelRegistry registry, string kpiId, string label)
        {
            registry.Register(EntityTypeCode.Supplier, kpiId, label);
        }

        private static void RegisterKpiMetadata(IKpiRegistry kpiRegistry)
        {
            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "PU-KPI-001",
                Category = EntityKpiCategory.Financial,
                DisplayName = "MTD Purchase",
                Description = "Supplier MTD purchase spend (same semantics as PU01 principal exposure row).",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = true,
                RadarAxisOrder = 1,
                RadarDisplayName = "Revenue",
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/purchasing",
                EvidenceFilterDimension = "supplierCode",
                SourceDomain = "PurchasingManagement",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M21"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "PU-KPI-002",
                Category = EntityKpiCategory.Activity,
                DisplayName = "MTD Invoice Count",
                Description = "Supplier MTD purchase invoice count.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "Count",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/purchasing",
                EvidenceFilterDimension = "supplierCode",
                SourceDomain = "PurchasingManagement",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M21"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "PU-KPI-003",
                Category = EntityKpiCategory.Quality,
                DisplayName = "Posted %",
                Description = "Supplier MTD purchase posting percent.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Percent",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = true,
                RadarAxisOrder = 3,
                RadarDisplayName = "Stability",
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 1,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/purchasing",
                EvidenceFilterDimension = "supplierCode",
                SourceDomain = "PurchasingManagement",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M21"
            });

            RegisterRadarAxisMetadata(kpiRegistry);
        }

        private static void RegisterRadarAxisMetadata(IKpiRegistry kpiRegistry)
        {
            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsRadarAxisIds.GrowthMom,
                Category = EntityKpiCategory.Growth,
                DisplayName = "Growth",
                Description = "MoM purchase growth percentile within peer group.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Percent",
                ValueType = "Numeric",
                AggregationType = "None",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 2,
                RadarValueSource = RadarValueSource.L1MomGrowthPercent,
                RadarSourceKpiId = "PU-KPI-001",
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.10"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.ActiveSkuCount,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Portfolio Strength",
                Description = "Active SKU count percentile within peer group.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 4,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.10"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsRadarAxisIds.AttentionRisk,
                Category = EntityKpiCategory.Risk,
                DisplayName = "Attention Risk",
                Description = "Active attention signal count percentile within peer group (lower is better).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "Count",
                Direction = "LowerIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 5,
                RadarValueSource = RadarValueSource.L3ActiveSignalCount,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.10"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.CatalogPenetration,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Product Penetration",
                Description = "Catalog sales penetration percentile within peer group.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Percent",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 6,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.10"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.InventoryValue,
                Category = EntityKpiCategory.Financial,
                DisplayName = "Inventory Value",
                Description = "Inventory value attributed to supplier (same semantics as PU01 principal exposure row).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "Neutral",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = false,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/inventory",
                EvidenceFilterDimension = "supplierCode",
                SourceDomain = "Inventory",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "M15"
            });
        }
    }
}
