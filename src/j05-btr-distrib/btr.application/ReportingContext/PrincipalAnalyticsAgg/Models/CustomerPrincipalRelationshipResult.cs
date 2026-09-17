using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class CustomerPrincipalRelationshipPairEvidence
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public DateTime FirstTransactionDate { get; set; }

        public DateTime LastTransactionDate { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }
    }

    public class CustomerPrincipalRelationshipResult
    {
        public string KpiId { get; set; }

        public DateTime AsOfDate { get; set; }

        public string HistoricalLimitation { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<CustomerPrincipalRelationshipRow> Pairs { get; set; }
            = new List<CustomerPrincipalRelationshipRow>();
    }

    public class CustomerPrincipalRelationshipRow
    {
        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public DateTime FirstTransactionDate { get; set; }

        public DateTime LastTransactionDate { get; set; }

        public string RelationshipStatus { get; set; }

        public string KpiId { get; set; }

        public decimal SalesOutAmount { get; set; }

        public int LineCount { get; set; }
    }
}
