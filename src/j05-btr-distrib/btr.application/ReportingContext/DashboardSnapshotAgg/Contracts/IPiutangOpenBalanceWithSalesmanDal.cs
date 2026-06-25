using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IPiutangOpenBalanceWithSalesmanDal
    {
        IReadOnlyList<PiutangOpenBalanceWithSalesmanDto> ListOpenBalances();

        IReadOnlyList<PiutangOpenBalanceWithSalesmanDto> ListOpenBalancesAsOf(DateTime asOfDate);
    }

    public class PiutangOpenBalanceWithSalesmanDto
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime JatuhTempo { get; set; }

        public decimal KurangBayar { get; set; }
    }
}
