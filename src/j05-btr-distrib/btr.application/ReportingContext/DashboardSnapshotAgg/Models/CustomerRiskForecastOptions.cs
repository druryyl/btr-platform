using btr.application.ReportingContext.DashboardSnapshotAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class CustomerRiskForecastOptions
    {
        public int HorizonDays { get; set; } = 30;

        public decimal PriorMonthOmzetFloorIdr { get; set; } = 1_000_000m;

        public int NoPaymentRecencyDays { get; set; } = 30;

        public int PaymentLagLookbackDays { get; set; } = 90;

        public int MinSettledFaktursForLag { get; set; } = 2;

        public int MaxTopCustomers { get; set; } = 20;

        public int MaxAttentionRows { get; set; } = 25;

        public int MaxRecommendations { get; set; } = 15;

        public static CustomerRiskForecastOptions FromDashboardOptions(DashboardSnapshotOptions options)
        {
            var source = options ?? new DashboardSnapshotOptions();
            return new CustomerRiskForecastOptions
            {
                HorizonDays = source.CustomerRiskForecastHorizonDays,
                PriorMonthOmzetFloorIdr = source.CustomerRiskForecastPriorMonthOmzetFloorIdr,
                NoPaymentRecencyDays = source.CustomerRiskForecastNoPaymentRecencyDays,
                PaymentLagLookbackDays = source.CustomerRiskForecastPaymentLagLookbackDays,
                MinSettledFaktursForLag = source.CustomerRiskForecastMinSettledFaktursForLag,
                MaxTopCustomers = source.CustomerRiskForecastMaxTopCustomers,
                MaxAttentionRows = source.CustomerRiskForecastMaxAttentionRows,
                MaxRecommendations = source.CustomerRiskForecastMaxRecommendations
            };
        }
    }
}
