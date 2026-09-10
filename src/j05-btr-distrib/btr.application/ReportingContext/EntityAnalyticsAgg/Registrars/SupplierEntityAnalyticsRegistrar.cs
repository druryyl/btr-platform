using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public class SupplierEntityAnalyticsRegistrar : IEntityAnalyticsRegistrar
    {
        public const string KpiPackId = "supplier-default";

        public const string PrincipalSalesOutEvidenceRoute = "/dashboard/principal-performance/evidence";

        public const string PrincipalSalesOutEvidenceFilterDimension = "supplierId";

        public const string PrincipalReturnEvidenceRoute = "/dashboard/principal-performance/return-evidence";

        public const string PrincipalReturnEvidenceFilterDimension = "supplierId";

        public const string PrincipalTargetEvidenceRoute = "/dashboard/principal-performance";

        public const string PrincipalTargetEvidenceFilterDimension = "supplierId";

        public void Register(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels)
        {
            RegisterKpiMetadata(kpiRegistry);
            RegisterDimensions(dimensionLabels);

            kpiRegistry.RegisterPack(KpiPackId, new[]
            {
                PrincipalKpiCatalog.SalesOutId,
                PrincipalKpiCatalog.GoodReturnAmountId,
                PrincipalKpiCatalog.BrokenReturnAmountId,
                PrincipalKpiCatalog.TotalReturnAmountId,
                PrincipalKpiCatalog.ReturnPercentageId,
                PrincipalKpiCatalog.TargetId,
                PrincipalKpiCatalog.AchievementAmountId,
                PrincipalKpiCatalog.AchievementPercentageId,
                PrincipalKpiCatalog.MomGrowthId,
                PrincipalKpiCatalog.YoyGrowthId,
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
                KpiId = PrincipalKpiCatalog.SalesOutId,
                Category = EntityKpiCategory.Financial,
                DisplayName = "Principal Sales-Out",
                Description = "PRN-SALES-001 Principal Sales-Out (DPP) from Faktur Item. Returns, Claims, and Inventory Adjustments are not deducted. Returns do not reduce or redefine Principal Sales-Out. Tax and header totals are excluded. Totals are not required to reconcile to Faktur GrandTotal. This is the commercial performance and default ranking KPI. It is not Purchase-In, Net Sales, or a Principal Health Score.",
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
                EvidenceRoute = PrincipalSalesOutEvidenceRoute,
                EvidenceFilterDimension = PrincipalSalesOutEvidenceFilterDimension,
                SourceDomain = PrincipalSalesOutSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-015"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                Category = EntityKpiCategory.Quality,
                DisplayName = "Good Return Amount",
                Description = "PRN-RET-001 Good Return Amount from Return Item. Returns are independent KPIs and never reduce, replace, or redefine PRN-SALES-001. Evidence grain is Return Item. This is not Principal Sales-Out and not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "LowerIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalReturnEvidenceRoute,
                EvidenceFilterDimension = PrincipalReturnEvidenceFilterDimension,
                SourceDomain = PrincipalReturnSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-047"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                Category = EntityKpiCategory.Quality,
                DisplayName = "Broken Return Amount",
                Description = "PRN-RET-002 Broken Return Amount from Return Item. Returns are independent KPIs and never reduce, replace, or redefine PRN-SALES-001. Evidence grain is Return Item. This is not Principal Sales-Out and not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "LowerIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalReturnEvidenceRoute,
                EvidenceFilterDimension = PrincipalReturnEvidenceFilterDimension,
                SourceDomain = PrincipalReturnSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-047"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                Category = EntityKpiCategory.Quality,
                DisplayName = "Total Return Amount",
                Description = "PRN-RET-003 Total Return Amount (Good Return + Broken Return) from Return Item. Returns are independent KPIs and never reduce, replace, or redefine PRN-SALES-001. Evidence grain is Return Item. This is not Principal Sales-Out and not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "LowerIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalReturnEvidenceRoute,
                EvidenceFilterDimension = PrincipalReturnEvidenceFilterDimension,
                SourceDomain = PrincipalReturnSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-047"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.ReturnPercentageId,
                Category = EntityKpiCategory.Quality,
                DisplayName = "Return Percentage",
                Description = "PRN-RET-004 Return Percentage (Return Amount ÷ Sales-Out) when stored PRN-SALES-001 is greater than zero; otherwise null. Return Percentage is a quality and supporting ranking indicator only. It is not a deduction from Sales-Out and not Net Sales. Returns never reduce Principal Sales-Out.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Ratio",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "LowerIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 6,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalReturnEvidenceRoute,
                EvidenceFilterDimension = PrincipalReturnEvidenceFilterDimension,
                SourceDomain = PrincipalReturnPercentageSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-047"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                Category = EntityKpiCategory.Financial,
                DisplayName = "Principal Target",
                Description = "PRN-TGT-001 Principal Target (Sum of Salesman Principal Targets) from SalesPersonPrincipalTarget. Target is derived, not independently maintained. This is not Principal Sales-Out and not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalTargetEvidenceRoute,
                EvidenceFilterDimension = PrincipalTargetEvidenceFilterDimension,
                SourceDomain = PrincipalTargetSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-048"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.AchievementAmountId,
                Category = EntityKpiCategory.Financial,
                DisplayName = "Achievement Amount",
                Description = "PRN-TGT-002 Achievement Amount (Principal Sales-Out versus Target) from stored PRN-SALES-001 and stored PRN-TGT-001. This is not a copy of Sales-Out, does not deduct returns, and is not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "IDR",
                ValueType = "Numeric",
                AggregationType = "Sum",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = false,
                RadarEligible = false,
                DisplayPrecision = 0,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalTargetEvidenceRoute,
                EvidenceFilterDimension = PrincipalTargetEvidenceFilterDimension,
                SourceDomain = PrincipalAchievementSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-048"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.AchievementPercentageId,
                Category = EntityKpiCategory.Financial,
                DisplayName = "Achievement Percentage",
                Description = "PRN-TGT-003 Achievement Percentage (Principal Sales-Out ÷ Principal Target) when stored PRN-TGT-001 is greater than zero; otherwise null. Achievement Percentage is a supporting ranking KPI only. It is not a replacement for Principal Sales-Out and not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Ratio",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = true,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 6,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalTargetEvidenceRoute,
                EvidenceFilterDimension = PrincipalTargetEvidenceFilterDimension,
                SourceDomain = PrincipalAchievementSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-048"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.MomGrowthId,
                Category = EntityKpiCategory.Growth,
                DisplayName = "Month-over-Month Growth Percentage",
                Description = "PRN-GRW-001 Month-over-Month Growth Percentage from stored PRN-SALES-001 history. Growth is computed from Principal Sales-Out only. It does not use Purchase-In, returns, claims, or inventory adjustments. PRN-GRW-001 is a supporting ranking KPI and is not a replacement for Principal Sales-Out. It is not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Percent",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = false,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 4,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalSalesOutEvidenceRoute,
                EvidenceFilterDimension = PrincipalSalesOutEvidenceFilterDimension,
                SourceDomain = PrincipalMomGrowthSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-046"
            });

            kpiRegistry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = PrincipalKpiCatalog.YoyGrowthId,
                Category = EntityKpiCategory.Growth,
                DisplayName = "Year-over-Year Growth Percentage",
                Description = "PRN-GRW-002 Year-over-Year Growth Percentage from stored PRN-SALES-001 history. Growth is computed from Principal Sales-Out only. It does not use Purchase-In, returns, claims, or inventory adjustments. PRN-GRW-002 is a supporting ranking KPI and is not a replacement for Principal Sales-Out. It is not Net Sales.",
                PeriodSemantics = "MTD",
                TimeGrain = "Month",
                Unit = "Percent",
                ValueType = "Numeric",
                AggregationType = "LastValue",
                Direction = "HigherIsBetter",
                NormalizationRule = "None",
                VisualizationType = "Card",
                TrendEligible = false,
                RankEligible = true,
                RadarEligible = false,
                DisplayPrecision = 4,
                NullableBehavior = "ShowEmpty",
                EvidenceRoute = PrincipalSalesOutEvidenceRoute,
                EvidenceFilterDimension = PrincipalSalesOutEvidenceFilterDimension,
                SourceDomain = PrincipalYoyGrowthSnapshot.Domain,
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 1,
                IntroducedVersion = "PCM-046"
            });

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
                RadarDisplayName = "Quality",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Quality,
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
                Description = "MoM Principal Sales-Out growth percentile within peer group from PRN-GRW-001.",
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
                RadarValueSource = RadarValueSource.L0Kpi,
                RadarSourceKpiId = PrincipalKpiCatalog.MomGrowthId,
                DisplayPrecision = 1,
                NullableBehavior = "Omit",
                ApplicableEntityTypes = new[] { EntityTypeCode.Supplier },
                DefinitionVersion = 2,
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
                RadarDisplayName = "Stability",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Stability,
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
                RadarAxisOrder = 6,
                RadarDisplayName = "Risk",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Risk,
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
                RadarAxisOrder = 5,
                RadarDisplayName = "Reach",
                SignatureDimensionKey = EntityAnalyticsSignatureDimensions.Reach,
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
