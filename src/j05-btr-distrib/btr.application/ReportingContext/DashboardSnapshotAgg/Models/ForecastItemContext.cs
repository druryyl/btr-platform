using btr.application.ReportingContext.DashboardSnapshotAgg.Services;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class ForecastItemContext
    {
        public DashboardInventoryItemGroup Item { get; set; }

        public InventoryForecastCalculation Calculation { get; set; }

        public string MovementSignalKey { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public bool IsForecastEligible { get; set; }
    }
}
