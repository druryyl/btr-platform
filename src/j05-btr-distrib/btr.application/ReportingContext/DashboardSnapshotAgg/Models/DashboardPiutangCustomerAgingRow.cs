using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPiutangCustomerAgingRow
    {
        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal Aging30Amount { get; set; }

        public decimal Aging60Amount { get; set; }

        public decimal Aging90Amount { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public DateTime LastUpdate { get; set; }

        public decimal TotalPiutang =>
            CurrentAmount + Aging30Amount + Aging60Amount + Aging90Amount + AgingOver90Amount;
    }
}
