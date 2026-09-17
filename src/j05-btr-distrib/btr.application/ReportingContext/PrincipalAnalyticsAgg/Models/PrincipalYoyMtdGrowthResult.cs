using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalYoyMtdGrowthResult
    {
        public string YoyMtdGrowthKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int PriorYear { get; set; }

        public int PriorMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalYoyMtdGrowthRow> Principals { get; set; }
            = new List<PrincipalYoyMtdGrowthRow>();
    }

    public class PrincipalYoyMtdGrowthRow
    {
        public string YoyMtdGrowthKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? CurrentYearMtdSalesOutAmount { get; set; }

        public decimal? PriorYearMtdSalesOutAmount { get; set; }

        public decimal? YoyMtdGrowthPercentage { get; set; }

        /// <summary>Normal or LowConfidence; set by the confidence guard (PSOM-07).</summary>
        public string ConfidenceStatus { get; set; }

        public int SortOrder { get; set; }
    }
}
