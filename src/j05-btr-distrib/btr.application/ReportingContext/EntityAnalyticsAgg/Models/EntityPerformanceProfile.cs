using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityPerformanceProfile
    {
        public bool IsAvailable { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public EntityOverview Overview { get; set; }

        public EntitySummary KpiSummary { get; set; }
    }
}
