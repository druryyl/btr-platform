using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface ISupplierMtdItemRollupDal
    {
        IEnumerable<SupplierMtdItemRollupDto> ListMtdItemRollups(Periode periode);

        IEnumerable<SupplierCatalogCountDto> ListSupplierCatalogCounts();
    }

    public class SupplierMtdItemRollupDto
    {
        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public string CustomerCode { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public decimal LineTotal { get; set; }
    }

    public class SupplierCatalogCountDto
    {
        public string SupplierId { get; set; }

        public int TotalSkuCount { get; set; }
    }
}
