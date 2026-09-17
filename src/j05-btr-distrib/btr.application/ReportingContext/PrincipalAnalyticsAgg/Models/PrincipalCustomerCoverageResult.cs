using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalCustomerCoverageResult
    {
        public string CustomerCoverageKpiId { get; set; }

        public DateTime AsOfDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalCustomerCoverageRow> Principals { get; set; }
            = new List<PrincipalCustomerCoverageRow>();
    }

    public class PrincipalCustomerCoverageRow
    {
        public string CustomerCoverageKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public int ActiveCustomerCount { get; set; }

        public int TotalCustomerCount { get; set; }

        public decimal? CoveragePercentage { get; set; }

        public int SortOrder { get; set; }
    }
}
