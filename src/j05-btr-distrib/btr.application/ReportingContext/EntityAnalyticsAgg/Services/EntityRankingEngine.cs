using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using Microsoft.Extensions.Options;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Generic L2 ranking engine. Reads L1 population metrics, applies KPI metadata direction,
    /// persists rank history. Entity-agnostic — no domain-specific rules.
    /// </summary>
    public class EntityRankingEngine : IEntityRankingEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly EntityAnalyticsOptions _options;

        public EntityRankingEngine(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityTypeRegistry entityTypes,
            IOptions<EntityAnalyticsOptions> options)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
            _options = options?.Value ?? new EntityAnalyticsOptions();
        }

        public void ComputeAndPersistRanks(
            string entityType,
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt,
            EntityAnalyticsReplayContext replay = null)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return;

            if (replay == null && _repository.IsMonthClosed(entityType, periodYear, periodMonth))
                return;

            var rankKpis = ResolveRankEligibleKpis(entityType);
            if (rankKpis.Count == 0)
                return;

            var allRows = new List<EntityAnalyticsRankingRow>();

            foreach (var kpi in rankKpis)
            {
                var population = _repository.GetPeriodMetricsForPopulation(
                    entityType, periodYear, periodMonth, kpi.KpiId);

                var candidates = population
                    .Where(p => p.IsActive && p.NumericValue.HasValue)
                    .Select(p => (p.EntityId, Value: p.NumericValue.Value))
                    .ToList();

                var entityCodes = population.ToDictionary(
                    p => p.EntityId,
                    p => p.EntityCode ?? p.EntityId,
                    StringComparer.OrdinalIgnoreCase);

                var rankings = EntityRankingCalculator.Calculate(candidates, kpi.Direction);

                foreach (var rank in rankings)
                {
                    allRows.Add(new EntityAnalyticsRankingRow
                    {
                        EntityType = entityType,
                        EntityId = rank.EntityId,
                        EntityCode = entityCodes.TryGetValue(rank.EntityId, out var code) ? code : rank.EntityId,
                        RankMetricKpiId = kpi.KpiId,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth,
                        RankPosition = rank.RankPosition,
                        PopulationSize = rank.PopulationSize,
                        Percentile = rank.Percentile,
                        GeneratedAt = generatedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }

            if (replay != null)
                _repository.ReplaceRankingForPeriod(entityType, periodYear, periodMonth, allRows, refreshLogId);
            else
                _repository.SaveRankingHistory(entityType, allRows, refreshLogId);
        }

        public ProfileRankingSectionDto BuildRankingSection(string entityType, string entityId)
        {
            var rankKpis = ResolveRankEligibleKpis(entityType);
            if (rankKpis.Count == 0)
            {
                return new ProfileRankingSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                    Series = new List<ProfileRankingSeriesDto>()
                };
            }

            var windowMonths = Math.Min(
                EntityAnalyticsConstants.DefaultRankingWindowMonths,
                _options.HistoryRetentionMonths > 0 ? _options.HistoryRetentionMonths : 36);

            var series = new List<ProfileRankingSeriesDto>();

            foreach (var kpi in rankKpis)
            {
                var periods = _repository.GetRankingPeriods(entityType, entityId, kpi.KpiId, windowMonths);
                if (periods.Count == 0)
                    continue;

                var from = periods[0];
                var to = periods[periods.Count - 1];
                var history = _repository.GetRankingHistory(
                    entityType,
                    entityId,
                    kpi.KpiId,
                    from.Year,
                    from.Month,
                    to.Year,
                    to.Month);

                if (history.Count == 0)
                    continue;

                var points = history
                    .OrderBy(r => r.PeriodYear)
                    .ThenBy(r => r.PeriodMonth)
                    .Select(r => new ProfileRankingPointDto
                    {
                        PeriodYear = r.PeriodYear,
                        PeriodMonth = r.PeriodMonth,
                        PeriodLabel = BuildPeriodLabel(r.PeriodYear, r.PeriodMonth, kpi.PeriodSemantics),
                        RankPosition = r.RankPosition,
                        PopulationSize = r.PopulationSize,
                        Percentile = r.Percentile
                    })
                    .ToList();

                var latest = points[points.Count - 1];

                series.Add(new ProfileRankingSeriesDto
                {
                    KpiId = kpi.KpiId,
                    DisplayName = kpi.DisplayName,
                    RankingDirection = kpi.Direction,
                    Unit = kpi.Unit,
                    CurrentRank = latest.RankPosition,
                    BestRank = points.Min(p => p.RankPosition),
                    WorstRank = points.Max(p => p.RankPosition),
                    CurrentPercentile = latest.Percentile,
                    CurrentPopulationSize = latest.PopulationSize,
                    Points = points
                });
            }

            return new ProfileRankingSectionDto
            {
                IsAvailable = series.Count > 0,
                UnavailableReason = series.Count > 0
                    ? null
                    : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Series = series
            };
        }

        private IReadOnlyList<EntityKpiMetadata> ResolveRankEligibleKpis(string entityType)
        {
            var packId = _entityTypes.TryGet(entityType, out var registration)
                ? registration.KpiPackId
                : null;

            if (string.IsNullOrWhiteSpace(packId))
                return Array.Empty<EntityKpiMetadata>();

            return _kpiRegistry.ResolvePackMetadata(packId)
                .Where(m => m.RankEligible
                    && !string.Equals(m.Direction, "Neutral", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static string BuildPeriodLabel(int year, int month, string periodSemantics)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            var label = $"{monthName} {year}";

            if (string.Equals(periodSemantics, "MTD", StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTime.Now;
                if (year == now.Year && month == now.Month)
                    return $"{label} (MTD)";
            }

            return label;
        }
    }
}
