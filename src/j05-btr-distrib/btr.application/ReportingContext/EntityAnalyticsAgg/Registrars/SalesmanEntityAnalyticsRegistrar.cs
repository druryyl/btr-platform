using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public class SalesmanEntityAnalyticsRegistrar : IEntityAnalyticsRegistrar
    {
        public const string KpiPackId = "salesman-default";

        public void Register(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels)
        {
            RegisterKpiMetadata(kpiRegistry);
            RegisterDimensions(dimensionLabels);

            kpiRegistry.RegisterPack(KpiPackId, new[]
            {
                "SF-KPI-008",
                "SF-KPI-009",
                "SF-KPI-010",
                EntityAnalyticsRadarAxisIds.GrowthMom,
                EntityAnalyticsRadarAxisIds.AttentionRisk,
                EntityAnalyticsMetaKpiIds.CustomerCount,
                EntityAnalyticsMetaKpiIds.CustomerEngagement
            });
        }

        private static void RegisterDimensions(IDimensionLabelRegistry dimensionLabels)
        {
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Wilayah, "Wilayah");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Segment, "Segment");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AchievementBand, "Achievement Band");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.ActiveMtd, "Active MTD");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.CustomerCount, "Customer Count");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.DormantCustomerCount, "Dormant Customer Count");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.OverdueBalance, "Overdue Balance");
            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AttentionSignals, "Attention Signals");
        }

        private static void RegisterDim(IDimensionLabelRegistry registry, string kpiId, string label)
        {
            registry.Register(EntityTypeCode.Salesman, kpiId, label);
        }

        private static void RegisterKpiMetadata(IKpiRegistry kpiRegistry)
        {
            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "SF-KPI-008",
                Category = EntityKpiCategory.Financial,
                DisplayName = "MTD Omzet",
                Description = "Salesman MTD completed omzet (same semantics as SF01 Top Omzet ranking row).",
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
                RadarDisplayName = "Performance",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Performance,
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/sales",
                EvidenceFilterDimension = "salesPersonCode",
                SourceDomain = "Salesman",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M18"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "SF-KPI-009",
                Category = EntityKpiCategory.Financial,
                DisplayName = "Achievement %",
                Description = "Salesman MTD achievement percent (same semantics as SF01 Top Achievement ranking row).",
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
                RadarDisplayName = "Quality",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Quality,
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 1,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/sales",
                EvidenceFilterDimension = "salesPersonCode",
                SourceDomain = "Salesman",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M18"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "SF-KPI-010",
                Category = EntityKpiCategory.Financial,
                DisplayName = "Open Balance",
                Description = "Salesman all-time open piutang balance (same semantics as SF01 Top Piutang ranking row).",
                PeriodSemantics = "AllTimeOpen",
                TimeGrain = "PointInTime",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "LowerIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = true,
                RadarAxisOrder = 4,
                RadarDisplayName = "Stability",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Stability,
                RadarValueSource = RadarValueSource.L0Kpi,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = "/reports/piutang",
                EvidenceFilterDimension = "salesPersonCode",
                SourceDomain = "Finance",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M18"
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
                Description = "MoM omzet growth percentile within peer group.",
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
                RadarSourceKpiId = "SF-KPI-008",
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.9"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.CustomerCount,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Portfolio",
                Description = "Managed customer count percentile within peer group.",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
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
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.9"
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
                RadarAxisOrder = 6,
                RadarDisplayName = "Risk",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Risk,
                RadarValueSource = RadarValueSource.L3ActiveSignalCount,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.9"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.CustomerEngagement,
                Category = EntityKpiCategory.Activity,
                DisplayName = "Engagement",
                Description = "MTD active customer count percentile within peer group.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Salesman },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.9"
            });
        }
    }
}
