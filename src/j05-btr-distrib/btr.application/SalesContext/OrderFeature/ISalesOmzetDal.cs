using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.OrderFeature
{
    public interface ISalesOmzetDal :
        IListData<SalesOmzetView, Periode>
    {
        IEnumerable<SalesOmzetView> ListData(Periode periode, SalesOmzetPeriodFilterMode mode);
    }

    public class SalesOmzetView
    {
        public string SalesPersonName { get; set; }
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string FakturCode { get; set; }
        public DateTime FakturDate { get; set; }
        public decimal FakturTotal { get; set; }
        public string CustomerName { get; set; }
        public string Code { get; set; }
        public string Alamat { get; set; }
        public DateTime SalesDate { get; set; }
        public DateTime OmzetDate { get; set; }
        public SalesOmzetStatusEnum OmzetStatus { get; set; }
        public SaleKindEnum SaleKind { get; set; }
    }
}
