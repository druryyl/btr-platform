namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPiutangTopCustomerRiskRow
    {
        public int Rank { get; set; }

        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal TotalPiutang { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal Aging30Amount { get; set; }

        public decimal Aging60Amount { get; set; }

        public decimal Aging90Amount { get; set; }

        public decimal AgingOver90Amount { get; set; }
    }
}
