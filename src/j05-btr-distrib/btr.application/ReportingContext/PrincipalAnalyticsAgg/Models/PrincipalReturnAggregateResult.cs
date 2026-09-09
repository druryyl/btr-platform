using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalReturnAggregateResult
    {
        public string GoodReturnKpiId { get; set; }

        public string BrokenReturnKpiId { get; set; }

        public string TotalReturnKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalReturnRow> Principals { get; set; }
            = new List<PrincipalReturnRow>();
    }

    public class PrincipalReturnRow
    {
        public string GoodReturnKpiId { get; set; }

        public string BrokenReturnKpiId { get; set; }

        public string TotalReturnKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal GoodReturnAmount { get; set; }

        public decimal BrokenReturnAmount { get; set; }

        public decimal TotalReturnAmount { get; set; }

        public int LineCount { get; set; }

        public int SortOrder { get; set; }
    }
}
