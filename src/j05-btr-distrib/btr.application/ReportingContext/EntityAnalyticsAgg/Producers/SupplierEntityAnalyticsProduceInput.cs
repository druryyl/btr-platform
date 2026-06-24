using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SupplierEntityAnalyticsProduceInput
    {
        public DashboardPurchasingManagementAggregateResult ManagementAggregate { get; set; }

        public DashboardSupplierRelationshipAggregateResult RelationshipAggregate { get; set; }
    }
}
