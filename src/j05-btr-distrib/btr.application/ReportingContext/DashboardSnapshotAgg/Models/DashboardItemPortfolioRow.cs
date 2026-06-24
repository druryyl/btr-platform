namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardItemPortfolioRow
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string CategoryName { get; set; }

        public string SupplierName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public decimal Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public string MovementClass { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public decimal? RecommendedPurchaseQty { get; set; }

        public int DistinctCustomerCount { get; set; }

        public bool IsTrendEligible { get; set; }

        public bool IsActive { get; set; }
    }
}
