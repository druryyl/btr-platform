using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class PiutangOpenBalanceDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime JatuhTempo { get; set; }

        public decimal KurangBayar { get; set; }
    }
}
