using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    /// <summary>
    /// Principal x Salesman commercial contribution for the current period.
    /// Contribution is a decomposition of stored PRN-SALES-001, not a registry KPI.
    /// No KPI ID is minted for contribution itself; rows carry the source KPI ID as provenance.
    /// </summary>
    public class PrincipalSalesmanContributionResult
    {
        public string SourceSalesOutKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalSalesmanContributionRow> Contributions { get; set; }
            = new List<PrincipalSalesmanContributionRow>();

        public IList<PrincipalSalesmanContributionExceptionRow> Exceptions { get; set; }
            = new List<PrincipalSalesmanContributionExceptionRow>();
    }

    public class PrincipalSalesmanContributionRow
    {
        public string SourceSalesOutKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal ContributionAmount { get; set; }

        public int LineCount { get; set; }

        public bool HasTargetResponsibility { get; set; }

        public int SortOrder { get; set; }
    }

    public class PrincipalSalesmanContributionExceptionRow
    {
        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal ContributionAmount { get; set; }

        public int LineCount { get; set; }

        public int TargetYear { get; set; }

        public int TargetMonth { get; set; }

        public int SortOrder { get; set; }
    }
}
