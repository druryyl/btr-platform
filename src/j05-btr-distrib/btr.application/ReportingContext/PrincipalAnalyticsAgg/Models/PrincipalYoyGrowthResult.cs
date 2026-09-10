using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalYoyGrowthResult
    {
        public string YoyGrowthKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int PriorYear { get; set; }

        public int PriorMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalYoyGrowthRow> Principals { get; set; }
            = new List<PrincipalYoyGrowthRow>();
    }

    public class PrincipalYoyGrowthRow
    {
        public string YoyGrowthKpiId { get; set; }

        public string SalesOutKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? CurrentSalesOutAmount { get; set; }

        public decimal? PriorSalesOutAmount { get; set; }

        public decimal? YoyGrowthPercentage { get; set; }

        public int SortOrder { get; set; }
    }
}
