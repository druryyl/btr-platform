namespace btr.application.ReportingContext.DashboardExecutiveAgg.Services
{
    public static class ExecutiveSalesAchievementBandResolver
    {
        public const string Healthy = "Healthy";
        public const string Warning = "Warning";
        public const string Critical = "Critical";
        public const string Unknown = "Unknown";

        public static string Resolve(decimal? achievementPercent)
        {
            if (!achievementPercent.HasValue)
                return Unknown;

            if (achievementPercent.Value >= 100m)
                return Healthy;

            if (achievementPercent.Value >= 80m)
                return Warning;

            return Critical;
        }
    }
}
