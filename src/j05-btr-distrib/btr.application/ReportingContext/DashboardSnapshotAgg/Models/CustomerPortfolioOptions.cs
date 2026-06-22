using btr.application.ReportingContext.DashboardSnapshotAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class CustomerPortfolioOptions
    {
        public int NewCustomerDaysThreshold { get; set; } = 90;

        public int PurchaseFrequencyLookbackMonths { get; set; } = 6;

        public int HighFrequencyFakturCountMin { get; set; } = 4;

        public decimal StrategicOpenBalanceFloorIdr { get; set; } = 10_000_000m;

        public decimal HighValueMtdOmzetFloorIdr { get; set; } = 5_000_000m;

        public int MaxPriorityRows { get; set; } = 50;

        public int MaxWilayahRows { get; set; } = 15;

        public static CustomerPortfolioOptions FromDashboardOptions(DashboardSnapshotOptions options)
        {
            var source = options ?? new DashboardSnapshotOptions();
            return new CustomerPortfolioOptions
            {
                NewCustomerDaysThreshold = source.CustomerPortfolioNewCustomerDaysThreshold,
                PurchaseFrequencyLookbackMonths = source.CustomerPortfolioPurchaseFrequencyLookbackMonths,
                HighFrequencyFakturCountMin = source.CustomerPortfolioHighFrequencyFakturCountMin,
                StrategicOpenBalanceFloorIdr = source.CustomerPortfolioStrategicOpenBalanceFloorIdr,
                HighValueMtdOmzetFloorIdr = source.CustomerPortfolioHighValueMtdOmzetFloorIdr,
                MaxPriorityRows = source.CustomerPortfolioMaxPriorityRows,
                MaxWilayahRows = source.CustomerPortfolioMaxWilayahRows
            };
        }
    }
}
