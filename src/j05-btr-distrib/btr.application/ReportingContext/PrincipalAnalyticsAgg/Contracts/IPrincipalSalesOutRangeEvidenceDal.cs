using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesOutRangeEvidenceDal
    {
        IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> ListSalesOutByRange(
            DateTime startDate,
            DateTime endDate);
    }

    public class PrincipalSalesOutRangeEvidenceRow
    {
        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }
    }
}
