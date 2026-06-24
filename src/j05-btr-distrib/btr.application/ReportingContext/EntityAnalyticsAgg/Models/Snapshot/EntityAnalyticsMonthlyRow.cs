using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    /// <summary>L1 monthly entity history row (M32.3+). Model defined now to stabilize repository seam.</summary>
    public class EntityAnalyticsMonthlyRow
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string KpiId { get; set; }

        public string PeriodSemantics { get; set; }

        public decimal? NumericValue { get; set; }

        public string TextValue { get; set; }

        public int? DefinitionVersion { get; set; }

        public bool IsClosed { get; set; }

        public DateTime GeneratedAt { get; set; }

        public string LastRefreshLogId { get; set; }
    }
}
