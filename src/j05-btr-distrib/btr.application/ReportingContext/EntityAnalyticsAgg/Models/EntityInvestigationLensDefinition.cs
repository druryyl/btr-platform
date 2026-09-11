using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>
    /// Declarative investigation lens over a single entity type. The lens owns which KPI group,
    /// default map preset, attention categories, relationship drivers, and evidence routes are
    /// presented. It does not introduce a new entity type, KPI ID, or profile.
    /// </summary>
    public class EntityInvestigationLensDefinition
    {
        public string LensId { get; set; }

        public string EntityType { get; set; }

        public string DisplayName { get; set; }

        public bool IsDefault { get; set; }

        public string DefaultPresetId { get; set; }

        /// <summary>Registered KPI IDs shown by this lens.</summary>
        public IReadOnlyList<string> KpiIds { get; set; } = new List<string>();

        /// <summary>Investigation-only derived metrics shown by this lens (never KPI IDs).</summary>
        public IReadOnlyList<string> DerivedMetricIds { get; set; } = new List<string>();

        public IReadOnlyList<string> AttentionCategories { get; set; } = new List<string>();

        public IReadOnlyList<string> RelationshipDrivers { get; set; } = new List<string>();

        public IReadOnlyList<string> EvidenceRoutes { get; set; } = new List<string>();
    }
}
