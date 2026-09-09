using System;
using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesOutEvidenceDal
    {
        IReadOnlyList<PrincipalSalesOutFakturItemEvidence> ListFakturItemEvidence(Periode periode);

        IReadOnlyList<PrincipalSalesOutFakturItemEvidenceLine> ListFakturItemEvidenceForPrincipal(
            Periode periode,
            string supplierId);
    }

    public class PrincipalSalesOutFakturItemEvidence
    {
        public string FakturId { get; set; }

        public string FakturItemId { get; set; }

        public string ItemSupplierId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscRp { get; set; }

        public decimal PpnRp { get; set; }

        public decimal LineTotal { get; set; }

        public decimal DppRp { get; set; }

        public decimal HeaderGrandTotal { get; set; }
    }

    public class PrincipalSalesOutFakturItemEvidenceLine
    {
        public string FakturId { get; set; }

        public string FakturCode { get; set; }

        public DateTime FakturDate { get; set; }

        public string FakturItemId { get; set; }

        public string BrgId { get; set; }

        public string ItemSupplierId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscRp { get; set; }
    }
}
