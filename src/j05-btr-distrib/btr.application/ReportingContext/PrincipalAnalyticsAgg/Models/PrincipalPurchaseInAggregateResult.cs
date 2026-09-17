using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalPurchaseInAggregateResult
    {
        public string KpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalPurchaseInRow> Principals { get; set; }
            = new List<PrincipalPurchaseInRow>();
    }

    public class PrincipalPurchaseInRow
    {
        public string KpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal PurchaseInAmount { get; set; }

        public int LineCount { get; set; }

        public int SortOrder { get; set; }
    }
}
