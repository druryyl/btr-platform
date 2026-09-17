using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalSalesOutHistoryResult
    {
        public string KpiId { get; set; }

        public string HistoricalLimitation { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalSalesOutHistoryRow> Months { get; set; }
            = new List<PrincipalSalesOutHistoryRow>();
    }

    public class PrincipalSalesOutHistoryRow
    {
        public string KpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class PrincipalSalesOutHistoryMonthEvidence
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }
    }
}
