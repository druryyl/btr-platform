using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services
{
    public class SalesmanReplayPeriodHandler : ISalesmanReplayPeriodHandler
    {
        private readonly ISalesmanRepHistoryBackfillSource _repHistorySource;
        private readonly IEntityAnalyticsRepository _repository;

        public SalesmanReplayPeriodHandler(
            ISalesmanRepHistoryBackfillSource repHistorySource,
            IEntityAnalyticsRepository repository)
        {
            _repHistorySource = repHistorySource;
            _repository = repository;
        }

        public bool CanUseFastPath(int periodYear, int periodMonth)
        {
            return _repHistorySource.HasCoverage(periodYear, periodMonth);
        }

        public EntityAnalyticsReplayAggregateResult BuildFastPathPlan(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt)
        {
            var l1Rows = _repHistorySource.MapToL1Rows(periodYear, periodMonth, refreshLogId, generatedAt);
            var entityCount = l1Rows
                .Select(r => r.EntityId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return new EntityAnalyticsReplayAggregateResult
            {
                EntityType = EntityTypeCode.Salesman,
                ProduceInput = CreateLayersOnlyInput(),
                EntityCount = entityCount,
                RowCounts = new EntityAnalyticsReplayRowCounts
                {
                    MasterRowCount = entityCount
                }
            };
        }

        public void PersistFastPathL1(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt)
        {
            var l1Rows = _repHistorySource.MapToL1Rows(periodYear, periodMonth, refreshLogId, generatedAt);
            _repository.ReplaceMonthlyHistoryForPeriod(
                EntityTypeCode.Salesman,
                periodYear,
                periodMonth,
                l1Rows,
                refreshLogId,
                batchSize: 0);
        }

        public SalesmanEntityAnalyticsProduceInput CreateLayersOnlyInput()
        {
            return new SalesmanEntityAnalyticsProduceInput
            {
                SalesmanAggregate = new DashboardSalesmanAggregateResult
                {
                    Portfolio = new System.Collections.Generic.List<DashboardSalesmanPortfolioRow>()
                }
            };
        }
    }
}
