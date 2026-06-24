using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using Microsoft.Extensions.Options;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityAnalyticsMonthCloseService : IEntityAnalyticsMonthCloseService
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly EntityAnalyticsOptions _options;

        public EntityAnalyticsMonthCloseService(
            IEntityAnalyticsRepository repository,
            IOptions<EntityAnalyticsOptions> options)
        {
            _repository = repository;
            _options = options?.Value ?? new EntityAnalyticsOptions();
        }

        public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
        {
            if (string.IsNullOrWhiteSpace(entityType) || context == null)
                return;

            var businessDate = context.BusinessDate == default
                ? context.GeneratedAt.Date
                : context.BusinessDate.Date;

            var prior = businessDate.AddMonths(-1);
            var priorYear = prior.Year;
            var priorMonth = prior.Month;

            if (!_repository.IsMonthClosed(entityType, priorYear, priorMonth))
            {
                _repository.CloseMonth(entityType, priorYear, priorMonth, context.RefreshLogId);
            }

            var retentionMonths = _options.HistoryRetentionMonths > 0
                ? _options.HistoryRetentionMonths
                : 36;

            _repository.PurgeHistoryOlderThan(entityType, retentionMonths);
        }
    }
}
