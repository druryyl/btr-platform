using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalSalesOutAggregateResult
    {
        public string KpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalSalesOutRow> Principals { get; set; }
            = new List<PrincipalSalesOutRow>();

        public IList<PrincipalSalesOutDataQualityRow> DataQuality { get; set; }
            = new List<PrincipalSalesOutDataQualityRow>();
    }

    public class PrincipalSalesOutRow
    {
        public string KpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class PrincipalSalesOutDataQualityRow
    {
        public string ExceptionCode { get; set; }

        public decimal Amount { get; set; }

        public int LineCount { get; set; }
    }
}
