using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalTargetAggregateResult
    {
        public string KpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalTargetRow> Principals { get; set; }
            = new List<PrincipalTargetRow>();
    }

    public class PrincipalTargetRow
    {
        public string KpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal TargetAmount { get; set; }

        public int SourceCount { get; set; }

        public int SortOrder { get; set; }
    }
}
