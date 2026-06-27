using System.Collections.Generic;

using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

using btr.application.ReportingContext.EntityAnalyticsAgg.Services;



namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars

{

    public class CustomerEntityAnalyticsRegistrar : IEntityAnalyticsRegistrar

    {

        public const string KpiPackId = "customer-default";



        public void Register(

            IEntityTypeRegistry entityTypes,

            IKpiRegistry kpiRegistry,

            IDimensionLabelRegistry dimensionLabels)

        {

            RegisterKpiMetadata(kpiRegistry);

            RegisterDimensions(dimensionLabels);

            kpiRegistry.RegisterPack(KpiPackId, new[]

            {

                "CU-KPI-009",

                "CU-KPI-010",

                "FI-KPI-013",

                EntityAnalyticsRadarAxisIds.GrowthMom,

                EntityAnalyticsRadarAxisIds.AttentionRisk,

                EntityAnalyticsMetaKpiIds.PortfolioPriorityScore,

                EntityAnalyticsMetaKpiIds.FakturCount6Mo

            });

        }



        private static void RegisterDimensions(IDimensionLabelRegistry dimensionLabels)

        {

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Wilayah, "Wilayah");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Klasifikasi, "Klasifikasi");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Salesman, "Salesman");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Lifecycle, "Lifecycle");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.Tier, "Tier");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.PortfolioAction, "Portfolio Action");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.M29Category, "Risk Category");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.M29PrimarySignal, "Primary Risk Signal");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.AttentionSignals, "Attention Signals");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.LastPurchaseDate, "Last Purchase");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.FakturCount6Mo, "Faktur Count (6 Mo)");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.ActiveMtd, "Active MTD");

            RegisterDim(dimensionLabels, EntityAnalyticsMetaKpiIds.PortfolioPriorityScore, "Portfolio Priority Score");

        }



        private static void RegisterDim(IDimensionLabelRegistry registry, string kpiId, string label)

        {

            registry.Register(EntityTypeCode.Customer, kpiId, label);

        }



        private static void RegisterKpiMetadata(IKpiRegistry kpiRegistry)

        {

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata

            {

                KpiId = "CU-KPI-009",

                Category = EntityKpiCategory.Financial,

                DisplayName = "MTD Omzet",

                Description = "Customer MTD invoiced omzet (same semantics as CU01 Top Omzet ranking row).",

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

                EvidenceFilterDimension = "customerCode",

                SourceDomain = "Customer",

                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },

                DefinitionVersion = 1,

                IntroducedVersion = "M17"

            });



            kpiRegistry.RegisterMetadata(new EntityKpiMetadata

            {

                KpiId = "CU-KPI-010",

                Category = EntityKpiCategory.Financial,

                DisplayName = "Open Balance",

                Description = "Customer all-time open piutang balance (same semantics as CU01 Top Piutang ranking row).",

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

                RadarAxisOrder = 3,

                RadarDisplayName = "Quality",

                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Quality,

                RadarValueSource = RadarValueSource.L0Kpi,

                DisplayPrecision = 0,

                NullableBehavior = "ShowEmpty",

                EvidenceRoute = "/reports/piutang",

                EvidenceFilterDimension = "customerCode",

                SourceDomain = "Finance",

                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },

                DefinitionVersion = 1,

                IntroducedVersion = "M17"

            });



            kpiRegistry.RegisterMetadata(new EntityKpiMetadata

            {

                KpiId = "FI-KPI-013",

                Category = EntityKpiCategory.Risk,

                DisplayName = "Overdue Exposure",

                Description = "Customer overdue open balance exposure.",

                PeriodSemantics = "AllTimeOpen",

                TimeGrain = "PointInTime",

                Unit = "IDR",

                ValueType = "Numeric",

                AggregationType = "LastValue",

                Direction = "LowerIsBetter",

                NormalizationRule = "None",

                VisualizationType = "Card",

                TrendEligible = true,

                RankEligible = false,

                DisplayPrecision = 0,

                NullableBehavior = "ShowEmpty",

                EvidenceRoute = "/reports/piutang",

                EvidenceFilterDimension = "customerCode",

                SourceDomain = "Finance",

                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },

                DefinitionVersion = 1,

                IntroducedVersion = "M17"

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
                RadarSourceKpiId = "CU-KPI-009",
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.8"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.PortfolioPriorityScore,
                Category = EntityKpiCategory.Portfolio,
                DisplayName = "Portfolio Strength",
                Description = "Portfolio priority score percentile within peer group.",
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
                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.8"
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
                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.8"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = EntityAnalyticsMetaKpiIds.FakturCount6Mo,
                Category = EntityKpiCategory.Activity,
                DisplayName = "Engagement",
                Description = "Six-month faktur count percentile within peer group.",
                PeriodSemantics = "PointInTime",
                TimeGrain = "PointInTime",
                Unit = "Count",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "PeerPercentile",
                VisualizationType = "RadarAxis",
                RadarEligible = true,
                RadarAxisOrder = 4,
                RadarDisplayName = "Stability",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Stability,
                RadarValueSource = RadarValueSource.L0DimensionNumeric,
                DisplayPrecision = 0,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Customer },
                DefinitionVersion = 1,
                IntroducedVersion = "M32.8"
            });
        }

    }

}


