namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityKpiMetadata
    {
        public string KpiId { get; set; }

        public EntityKpiCategory Category { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        /// <summary>MTD · AllTimeOpen · PointInTime · MonthClosed</summary>
        public string PeriodSemantics { get; set; }

        /// <summary>Time grain for history layers: Month · PointInTime · None</summary>
        public string TimeGrain { get; set; }

        public string Unit { get; set; }

        /// <summary>Numeric · Text · Boolean</summary>
        public string ValueType { get; set; }

        /// <summary>Sum · LastValue · Count · None</summary>
        public string AggregationType { get; set; }

        /// <summary>HigherIsBetter · LowerIsBetter · Neutral</summary>
        public string Direction { get; set; }

        public string NormalizationRule { get; set; }

        public string VisualizationType { get; set; }

        public bool TrendEligible { get; set; }

        public bool RankEligible { get; set; }

        public bool RadarEligible { get; set; }

        /// <summary>Display order on radar chart (1-based).</summary>
        public int RadarAxisOrder { get; set; }

        /// <summary>L0Kpi · L1MomGrowthPercent · L0DimensionNumeric · L3ActiveSignalCount</summary>
        public string RadarValueSource { get; set; }

        /// <summary>Underlying KPI for derived radar value sources.</summary>
        public string RadarSourceKpiId { get; set; }

        /// <summary>Optional per-axis peer group rule override.</summary>
        public string PeerGroupRule { get; set; }

        /// <summary>Optional radar axis label override.</summary>
        public string RadarDisplayName { get; set; }

        /// <summary>Universal Performance Signature dimension key (EA-SIG-*).</summary>
        public string SignatureDimensionKey { get; set; }

        /// <summary>Decimal places for numeric display (0 for IDR, 2 for percent).</summary>
        public int DisplayPrecision { get; set; }

        /// <summary>When no value: Omit · ShowEmpty · ShowDash</summary>
        public string NullableBehavior { get; set; }

        public string EvidenceRoute { get; set; }

        /// <summary>Query-string filter dimension for evidence drill-down (e.g. customerCode).</summary>
        public string EvidenceFilterDimension { get; set; }

        /// <summary>Domain aggregator that produces this KPI (e.g. Customer, Finance).</summary>
        public string SourceDomain { get; set; }

        public string[] ApplicableEntityTypes { get; set; }

        public int DefinitionVersion { get; set; } = 1;

        public string IntroducedVersion { get; set; }

        public string DeprecatedVersion { get; set; }
    }
}
