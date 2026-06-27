using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btr.application.FinanceContext.PiutangAgg.Contracts
{
    public interface IPenerimaanPelunasanSalesDal : IListData<PenerimaanPelunasanSalesDto, Periode>
    {
        IEnumerable<PenerimaanPelunasanSalesLunasSourceDto> ListPiutangLunasSource(Periode filter);

        IEnumerable<PenerimaanPelunasanSalesElementDto> ListPiutangElementTotals(IEnumerable<string> piutangIds);
    }

    public class PenerimaanPelunasanSalesLunasSourceDto
    {
        public string PiutangId { get; set; }
        public DateTime LunasDate { get; set; }
        public int JenisLunas { get; set; }
        public decimal Nilai { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesName { get; set; }
    }

    public class PenerimaanPelunasanSalesElementDto
    {
        public string PiutangId { get; set; }
        public decimal Retur { get; set; }
        public decimal Potongan { get; set; }
        public decimal MateraiAdmin { get; set; }
    }

    public class PenerimaanPelunasanSalesDto
    {
        public string SalesPersonId { get; set; }
        public string SalesName { get; set; }
        public DateTime LunasDate { get; set; }

        public decimal BayarTunai { get; set; }
        public decimal BayarGiro { get; set; }
        public decimal Retur { get; set; }
        public decimal Potongan { get; set; }
        public decimal MateraiAdmin { get; set; }

        public decimal TotalBayar{ get; set; }
    }
}
