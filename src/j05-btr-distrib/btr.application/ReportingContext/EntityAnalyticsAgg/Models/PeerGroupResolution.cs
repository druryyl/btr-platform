using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class PeerGroupResolution
    {
        public string EntityId { get; set; }

        public string PeerGroupRuleId { get; set; }

        public IReadOnlyList<string> PeerEntityIds { get; set; } = new List<string>();

        public int PeerGroupSize { get; set; }

        public bool IsSufficient { get; set; }

        public string DimensionValue { get; set; }
    }
}
