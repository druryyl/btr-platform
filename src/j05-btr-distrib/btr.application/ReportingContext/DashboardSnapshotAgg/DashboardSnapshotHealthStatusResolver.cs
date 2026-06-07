using System;
using System.Collections.Generic;
using System.Linq;

namespace btr.application.ReportingContext.DashboardSnapshotAgg
{
    public static class DashboardSnapshotHealthStatusResolver
    {
        public static string ResolveOverallStatus(IReadOnlyList<string> lastRefreshStatuses)
        {
            if (lastRefreshStatuses == null || lastRefreshStatuses.Count == 0)
                return "unknown";

            if (lastRefreshStatuses.All(string.IsNullOrEmpty))
                return "unknown";

            if (lastRefreshStatuses.Any(s =>
                    string.Equals(s, "Failed", StringComparison.OrdinalIgnoreCase)))
                return "degraded";

            if (lastRefreshStatuses.Any(s =>
                    string.Equals(s, "Running", StringComparison.OrdinalIgnoreCase)))
                return "refreshing";

            return "ok";
        }
    }
}
