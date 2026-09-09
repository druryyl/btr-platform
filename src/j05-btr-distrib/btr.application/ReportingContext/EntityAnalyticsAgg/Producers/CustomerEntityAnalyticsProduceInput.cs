using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public sealed class CustomerEntityAnalyticsProduceInput
    {
        public DashboardCustomerAggregateResult CustomerAggregate { get; set; }

        public DashboardCustomerRiskForecastAggregateResult ForecastAggregate { get; set; }

        public DashboardCustomerPortfolioAggregateResult PortfolioAggregate { get; set; }

        public DashboardSalesmanAggregateResult SalesmanSnapshot { get; set; }

        public DashboardCustomerRelationshipAggregateResult RelationshipAggregate { get; set; }

        public CustomerPrincipalRelationshipResult RelationshipProjection { get; set; }
    }
}
