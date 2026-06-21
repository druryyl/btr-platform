using btr.application.ReportingContext.DashboardSnapshotAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class CollectionOptimizationOptions
    {
        public decimal SalesRecoveryOverdueFloorIdr { get; set; } = 500_000m;

        public decimal StrategicPriorMonthOmzetFloorIdr { get; set; } = 5_000_000m;

        public decimal LargeDueSoonFloorIdr { get; set; } = 10_000_000m;

        public int MaxPriorityRows { get; set; } = 30;

        public int MaxQueueRows { get; set; } = 15;

        public int MaxImpactRows { get; set; } = 15;

        public int MaxWorkloadRows { get; set; } = 10;

        public static CollectionOptimizationOptions FromDashboardOptions(DashboardSnapshotOptions options)
        {
            var source = options ?? new DashboardSnapshotOptions();
            return new CollectionOptimizationOptions
            {
                SalesRecoveryOverdueFloorIdr = source.CollectionOptimizationSalesRecoveryOverdueFloorIdr,
                StrategicPriorMonthOmzetFloorIdr = source.CollectionOptimizationStrategicPriorMonthOmzetFloorIdr,
                LargeDueSoonFloorIdr = source.CollectionOptimizationLargeDueSoonFloorIdr,
                MaxPriorityRows = source.CollectionOptimizationMaxPriorityRows,
                MaxQueueRows = source.CollectionOptimizationMaxQueueRows,
                MaxImpactRows = source.CollectionOptimizationMaxImpactRows,
                MaxWorkloadRows = source.CollectionOptimizationMaxWorkloadRows
            };
        }
    }
}
