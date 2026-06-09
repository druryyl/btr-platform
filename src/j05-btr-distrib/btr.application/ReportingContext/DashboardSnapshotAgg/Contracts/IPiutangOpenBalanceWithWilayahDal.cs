using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IPiutangOpenBalanceWithWilayahDal
    {
        IReadOnlyList<PiutangOpenBalanceWithWilayahDto> ListOpenBalances();
    }

    public class PiutangOpenBalanceWithWilayahDto
    {
        public string WilayahId { get; set; }

        public string WilayahName { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime JatuhTempo { get; set; }

        public decimal KurangBayar { get; set; }
    }
}
