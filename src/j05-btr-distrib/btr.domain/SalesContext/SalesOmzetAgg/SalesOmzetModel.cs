using System;

namespace btr.domain.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetModel : ISalesOmzetKey
    {
        public SalesOmzetModel(string id) => SalesOmzetId = id;

        public SalesOmzetModel()
        {
        }

        public string SalesOmzetId { get; set; }
        public string OrderId { get; set; }
        public string FakturId { get; set; }
        public SaleKindEnum SaleKind { get; set; }

        /// <summary>Tanggal Jual — frozen at create (Phase 2 linker).</summary>
        public DateTime SalesDate { get; set; }

        /// <summary>Omzet recognition — KembaliFaktur StatusDate (Phase 2 snapshot).</summary>
        public DateTime OmzetDate { get; set; }

        public string SalesPersonName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string FakturCode { get; set; }
        public DateTime FakturDate { get; set; }
        public decimal FakturTotal { get; set; }
        public string CustomerName { get; set; }
        public string Code { get; set; }
        public string Alamat { get; set; }
        public SalesOmzetStatusEnum OmzetStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastReconciledAt { get; set; }
    }
}
