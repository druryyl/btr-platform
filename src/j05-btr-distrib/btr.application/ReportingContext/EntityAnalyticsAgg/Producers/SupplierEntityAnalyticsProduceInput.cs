using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SupplierEntityAnalyticsProduceInput
    {
        public DashboardPurchasingManagementAggregateResult ManagementAggregate { get; set; }

        public DashboardSupplierRelationshipAggregateResult RelationshipAggregate { get; set; }

        public CustomerPrincipalRelationshipResult RelationshipProjection { get; set; }

        /// <summary>
        /// Replay-only Principal KPI aggregates for the replay month (closed months).
        /// Null in live contexts; the producer then reads the Principal snapshot DALs.
        /// </summary>
        public SupplierPrincipalReplayResult PrincipalReplay { get; set; }
    }
}
