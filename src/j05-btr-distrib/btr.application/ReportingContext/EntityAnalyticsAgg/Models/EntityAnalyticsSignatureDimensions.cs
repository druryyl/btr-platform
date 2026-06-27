namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Universal Performance Signature dimensions shared across all entity types.
    /// </summary>
    public static class EntityAnalyticsSignatureDimensions
    {
        public const string Performance = "EA-SIG-PERFORMANCE";
        public const string Growth = "EA-SIG-GROWTH";
        public const string Quality = "EA-SIG-QUALITY";
        public const string Stability = "EA-SIG-STABILITY";
        public const string Reach = "EA-SIG-REACH";
        public const string Risk = "EA-SIG-RISK";

        public static readonly string[] OrderedKeys =
        {
            Performance,
            Growth,
            Quality,
            Stability,
            Reach,
            Risk
        };

        public static readonly string[] OrderedLabels =
        {
            "Performance",
            "Growth",
            "Quality",
            "Stability",
            "Reach",
            "Risk"
        };

        public static int GetOrderIndex(string signatureDimensionKey)
        {
            if (string.IsNullOrWhiteSpace(signatureDimensionKey))
                return int.MaxValue;

            for (var i = 0; i < OrderedKeys.Length; i++)
            {
                if (string.Equals(OrderedKeys[i], signatureDimensionKey, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return int.MaxValue;
        }

        public static string GetDisplayLabel(string signatureDimensionKey)
        {
            var index = GetOrderIndex(signatureDimensionKey);
            return index < OrderedLabels.Length ? OrderedLabels[index] : null;
        }
    }
}
