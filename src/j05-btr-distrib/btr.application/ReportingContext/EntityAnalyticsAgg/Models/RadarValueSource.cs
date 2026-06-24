namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public static class RadarValueSource
    {
        public const string L0Kpi = "L0Kpi";
        public const string L1MomGrowthPercent = "L1MomGrowthPercent";
        public const string L0DimensionNumeric = "L0DimensionNumeric";
        public const string L3ActiveSignalCount = "L3ActiveSignalCount";
    }

    public static class RadarNormalizationMethod
    {
        public const string PeerPercentile = "PeerPercentile";
        public const string BandMidpoint = "BandMidpoint";
    }

    public static class EntityAnalyticsRadarAxisIds
    {
        public const string GrowthMom = "EA-RADAR-GROWTH-MOM";
        public const string AttentionRisk = "EA-RADAR-ATTENTION-RISK";
    }
}
