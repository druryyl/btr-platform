using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public class ItemEntityAnalyticsRegistrar : IEntityAnalyticsRegistrar
    {
        public const string KpiPackId = "item-default";

        public void Register(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels)
        {
            RegisterKpiMetadata(kpiRegistry);
            RegisterDimensions(dimensionLabels);

            kpiRegistry.RegisterPack(KpiPackId, new[]
            {
                "IN-KPI-001",
                "IN-KPI-020",
                "IN-KPI-021",
                EntityAnalyticsRadarAxisIds.GrowthMom,
                EntityAnalyticsRadarAxisIds.AttentionRisk,
                EntityAnalyticsMetaKpiIds.CustomerCount,
                EntityAnalyticsMetaKpiIds.DaysSinceLastFaktur,
                EntityAnalyticsMetaKpiIds.Category,
                EntityAnalyticsMetaKpiIds.MovementClass,
                EntityAnalyticsMetaKpiIds.QtyOnHand
            });
        }

        private static void RegisterDimensions(IDimensionLabelRegistry dimensionLabels)
        {
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Category, "Category");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.SupplierName, "Supplier");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.MovementClass, "Movement Class");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.DaysSinceLastFaktur, "Days Since Last Faktur");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.ActiveMtd, "Active");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AttentionSignals, "Attention Signals");
        }

        private static void RegisterDim(IDimensionLabelRegistry registry, string kpiId, string label)
        {
            registry.Register(EntityTypeCode.Item, kpiId, label);
        }

        private static void RegisterKpiMetadata(IKpiRegistry kpiRegistry)
        {
            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "IN-KPI-001",
                Category = EntityKpiCategory.Financial,
                DisplayName = "Inventory Value",
                Description = "Item inventory value (BrgId-first; same semantics as IN02 attention/top rows).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = true,
                RadarAxisOrder = 1,
                RadarDisplayName = "Performance",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Performance,
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/inventory",
                EvidenceFilterDimension = "brgCode",
                SourceDomain = "InventoryRisk",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M19"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "IN-KPI-020",
                Category = EntityKpiCategory.Risk,
                DisplayName = "Days of Supply",
                Description = "Per-SKU days of supply (same semantics as IN03 forecast item row).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Days",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = true,
                RadarAxisOrder = 3,
                RadarDisplayName = "Quality",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Quality,
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 1,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/inventory",
                EvidenceFilterDimension = "brgCode",
                SourceDomain = "InventoryForecast",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M28"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "IN-KPI-021",
                Category = EntityKpiCategory.Activity,
                DisplayName = "Recommended Purchase Qty",
                Description = "Indicative recommended purchase quantity (same semantics as IN03).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "Neutral",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/inventory",
                EvidenceFilterDimension = "brgCode",
                SourceDomain = "InventoryForecast",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M28"
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
                Description = "MoM inventory value growth percentile within category peer group.",
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
                RadarDisplayName = "Growth",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Growth,
                RadarValueSource = RadarValueSource.L1MomGrowthPercent,
                RadarSourceKpiId = "IN-KPI-001",
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.11"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.CustomerCount,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Market Reach",
                Description = "Distinct MTD buyer count percentile within category peer group.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "Count",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 5,
                RadarDisplayName = "Reach",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Reach,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.11"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsRadarAxisIds.AttentionRisk,
                Category = EntityKpiCategory.Risk,
                DisplayName = "Attention Risk",
                Description = "Active attention signal count percentile within category peer group (lower is better).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "Count",
                Direction = "LowerIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 6,
                RadarDisplayName = "Risk",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Risk,
                RadarValueSource = RadarValueSource.L3ActiveSignalCount,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.11"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.DaysSinceLastFaktur,
                Category = EntityKpiCategory.Activity,
                DisplayName = "Days Since Last Faktur",
                Description = "Days since last sale percentile within category peer group (lower is better).",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Days",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "LowerIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 4,
                RadarDisplayName = "Stability",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Stability,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.11"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.QtyOnHand,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Supplier Strength",
                Description = "On-hand quantity percentile within category peer group.",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = false,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Item },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.11"
            });
        }
    }
}
