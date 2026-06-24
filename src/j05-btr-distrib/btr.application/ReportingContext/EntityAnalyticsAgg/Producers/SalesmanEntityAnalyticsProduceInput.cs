using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SalesmanEntityAnalyticsProduceInput
    {
        public DashboardSalesmanAggregateResult SalesmanAggregate { get; set; }

        public DashboardSalesmanRelationshipAggregateResult RelationshipAggregate { get; set; }
    }
}
