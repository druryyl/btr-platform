using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class ComparisonContext
    {
        public ComparisonMode Mode { get; set; }

        public string EntityType { get; set; }

        public string PrimaryEntityId { get; set; }

        public IReadOnlyList<string> EntityIds { get; set; }

        public IReadOnlyList<string> MetricKpiIds { get; set; }

        public int? PeriodYear { get; set; }

        public int? PeriodMonth { get; set; }
    }
}
