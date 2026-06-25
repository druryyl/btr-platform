using System;
using System.Collections.Generic;
using System.Globalization;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Sources
{
    public class SalesmanRepHistoryBackfillSource : ISalesmanRepHistoryBackfillSource
    {
        private readonly ISalesmanRepHistoryBackfillDal _dal;

        public SalesmanRepHistoryBackfillSource(ISalesmanRepHistoryBackfillDal dal)
        {
            _dal = dal;
        }

        public bool HasCoverage(int periodYear, int periodMonth)
        {
            return _dal.HasCoverage(periodYear, periodMonth);
        }

        public IReadOnlyList<EntityAnalyticsMonthlyRow> MapToL1Rows(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt)
        {
            var rows = _dal.ListForPeriod(periodYear, periodMonth);
            var result = new List<EntityAnalyticsMonthlyRow>();

            foreach (var rep in rows)
            {
                if (string.IsNullOrWhiteSpace(rep.SalesPersonId))
                    continue;

                var entityId = rep.SalesPersonId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(rep.SalesPersonCode)
                    ? entityId
                    : rep.SalesPersonCode.Trim();

                result.Add(CreateMonthlyRow(
                    entityId,
                    entityCode,
                    periodYear,
                    periodMonth,
                    "SF-KPI-008",
                    "MTD",
                    rep.CompletedOmzet,
                    generatedAt,
                    refreshLogId));

                if (rep.AchievementPercent.HasValue)
                {
                    result.Add(CreateMonthlyRow(
                        entityId,
                        entityCode,
                        periodYear,
                        periodMonth,
                        "SF-KPI-009",
                        "MTD",
                        rep.AchievementPercent.Value,
                        generatedAt,
                        refreshLogId));
                }

                result.Add(CreateMonthlyRow(
                    entityId,
                    entityCode,
                    periodYear,
                    periodMonth,
                    "SF-KPI-010",
                    "AllTimeOpen",
                    rep.OpenBalance,
                    generatedAt,
                    refreshLogId));
            }

            return result;
        }

        private static EntityAnalyticsMonthlyRow CreateMonthlyRow(
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            string kpiId,
            string periodSemantics,
            decimal numericValue,
            DateTime generatedAt,
            string refreshLogId)
        {
            return new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Salesman,
                EntityId = entityId,
                EntityCode = entityCode,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                KpiId = kpiId,
                PeriodSemantics = periodSemantics,
                NumericValue = numericValue,
                TextValue = numericValue.ToString(CultureInfo.InvariantCulture),
                DefinitionVersion = 1,
                IsClosed = true,
                GeneratedAt = generatedAt,
                LastRefreshLogId = refreshLogId ?? string.Empty
            };
        }
    }
}
