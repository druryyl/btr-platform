using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalActiveCustomerResult
    {
        public string ActiveCustomerKpiId { get; set; }

        public DateTime AsOfDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalActiveCustomerRow> Principals { get; set; }
            = new List<PrincipalActiveCustomerRow>();
    }

    public class PrincipalActiveCustomerRow
    {
        public string ActiveCustomerKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public int ActiveCustomerCount { get; set; }

        public int SortOrder { get; set; }
    }
}
