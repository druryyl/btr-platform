using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalReturnPercentageResult
    {
        public string ReturnPercentageKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TotalReturnKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalReturnPercentageRow> Principals { get; set; }
            = new List<PrincipalReturnPercentageRow>();
    }

    public class PrincipalReturnPercentageRow
    {
        public string ReturnPercentageKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string TotalReturnKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal TotalReturnAmount { get; set; }

        public decimal? SalesOutAmount { get; set; }

        public decimal? ReturnPercentage { get; set; }

        public int SortOrder { get; set; }
    }
}
