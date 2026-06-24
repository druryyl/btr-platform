namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Platform-wide constants for Entity Analytics snapshots and API contracts.
    /// </summary>
    public static class EntityAnalyticsConstants
    {
        public const string CurrentSnapshotKey = "CURRENT";

        /// <summary>L0 CURRENT snapshot schema version. Bump when row semantics change.</summary>
        public const string CurrentSnapshotVersion = "L0-v1";

        /// <summary>L1 monthly history default trend window (months).</summary>
        public const int DefaultTrendWindowMonths = 12;

        /// <summary>L2 ranking history default window (months).</summary>
        public const int DefaultRankingWindowMonths = 12;

        /// <summary>Profile API response contract version (additive changes only).</summary>
        public const string ProfileContractVersion = "M32.8-v1";

        /// <summary>Minimum peer group size before radar section is shown.</summary>
        public const int MinRadarPeerGroupSize = 5;

        /// <summary>Peer group size below which band-midpoint fallback may apply.</summary>
        public const int BandFallbackPeerGroupThreshold = 10;
    }

    /// <summary>
    /// Canonical <see cref="ProfileSectionDtoBase.UnavailableReason"/> values exposed by the profile API.
    /// </summary>
    public static class EntityAnalyticsUnavailableReasons
    {
        public const string NotImplemented = "NotImplemented";
        public const string NoSnapshotData = "NoSnapshotData";
        public const string NoRegisteredKpis = "NoRegisteredKpis";
        public const string EntityTypeDisabled = "EntityTypeDisabled";
        public const string InvalidEntityCount = "InvalidEntityCount";
        public const string PeerGroupTooSmall = "PeerGroupTooSmall";
    }

    public static class EntityComparisonWarnings
    {
        public const string GeneratedAtMismatch = "GeneratedAtMismatch";
    }
}
