using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class ItemEntityAnalyticsProduceInput
    {
        public DashboardInventoryRiskAggregateResult RiskAggregate { get; set; }

        public DashboardInventoryForecastAggregateResult ForecastAggregate { get; set; }

        public DashboardItemRelationshipAggregateResult RelationshipAggregate { get; set; }

        public IList<DashboardItemPortfolioRow> Portfolio { get; set; }
            = new List<DashboardItemPortfolioRow>();
    }
}
