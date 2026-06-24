namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Reserved L0 row keys for entity identity and overview dimensions (not catalog KPIs).
    /// </summary>
    public static class EntityAnalyticsMetaKpiIds
    {
        public const string DisplayName = "EA-META-DISPLAY-NAME";
        public const string IsActive = "EA-META-IS-ACTIVE";

        public const string DimPrefix = "EA-DIM-";

        public const string Wilayah = DimPrefix + "WILAYAH";
        public const string Klasifikasi = DimPrefix + "KLASIFIKASI";
        public const string Salesman = DimPrefix + "SALESMAN";
        public const string Lifecycle = DimPrefix + "LIFECYCLE";
        public const string Tier = DimPrefix + "TIER";
        public const string PortfolioAction = DimPrefix + "PORTFOLIO-ACTION";
        public const string M29Category = DimPrefix + "M29-CATEGORY";
        public const string M29PrimarySignal = DimPrefix + "M29-PRIMARY-SIGNAL";
        public const string AttentionSignals = DimPrefix + "ATTENTION-SIGNALS";
        public const string LastPurchaseDate = DimPrefix + "LAST-PURCHASE";
        public const string FakturCount6Mo = DimPrefix + "FAKTUR-COUNT-6MO";
        public const string ActiveMtd = DimPrefix + "ACTIVE-MTD";
        public const string PortfolioPriorityScore = DimPrefix + "PORTFOLIO-PRIORITY-SCORE";

        public static bool IsMetaOrDimension(string kpiId)
        {
            if (string.IsNullOrWhiteSpace(kpiId))
                return false;

            return kpiId.StartsWith("EA-META-") || kpiId.StartsWith(DimPrefix);
        }
    }
}
