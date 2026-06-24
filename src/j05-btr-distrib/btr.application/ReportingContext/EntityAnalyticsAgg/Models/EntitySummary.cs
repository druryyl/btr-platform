using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntitySummary
    {
        public IReadOnlyList<KpiEnvelope> Activity { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Financial { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Growth { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Contribution { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Portfolio { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Quality { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Risk { get; set; } = new List<KpiEnvelope>();

        public IReadOnlyList<KpiEnvelope> Trend { get; set; } = new List<KpiEnvelope>();
    }
}
