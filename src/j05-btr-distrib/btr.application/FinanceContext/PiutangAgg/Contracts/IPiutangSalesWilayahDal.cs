using btr.application.ReportingContext.PiutangReportAgg;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using System;
using System.Collections.Generic;

namespace btr.application.FinanceContext.PiutangAgg.Contracts
{
    public interface IPiutangSalesWilayahDal : IListData<PiutangSalesWilayahDto, Periode>
    {
        IEnumerable<PiutangSalesWilayahDto> ListData(Periode filter, PiutangReportDateField dateField);

        IEnumerable<PiutangSalesWilayahDto> ListAllOpenBalances();
    }

    public class PiutangSalesWilayahDto
    {
        public string SalesName { get; set; }
        public string WilayahName { get; set; }
        public string FakturCode { get; set; }
        public DateTime FakturDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Alamat { get; set; }
        public DateTime JatuhTempo { get; set; }

        public decimal TotalJual { get; set; }
        public decimal BayarTunai { get; set; }
        public decimal BayarGiro { get; set; }
        public decimal Retur { get; set; }
        public decimal Potongan { get; set; }
        public decimal MateraiAdmin { get; set; }

        public decimal KurangBayar { get; set; }
    }

}
