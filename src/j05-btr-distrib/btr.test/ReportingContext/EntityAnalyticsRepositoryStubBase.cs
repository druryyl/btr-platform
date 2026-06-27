using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.test.ReportingContext
{
    /// <summary>Shared in-memory repository stub for Entity Analytics unit tests.</summary>
    internal abstract class EntityAnalyticsRepositoryStubBase : IEntityAnalyticsRepository
    {
        public readonly List<EntityAnalyticsMonthlyRow> MonthlyRows = new List<EntityAnalyticsMonthlyRow>();
        public readonly List<EntityAnalyticsRankingRow> RankingRows = new List<EntityAnalyticsRankingRow>();
        public readonly List<EntityAnalyticsAttentionEventRow> AttentionRows = new List<EntityAnalyticsAttentionEventRow>();
        public readonly List<EntityAnalyticsRelationshipRow> RelationshipRows = new List<EntityAnalyticsRelationshipRow>();
        public readonly List<EntityAnalyticsRadarScoreRow> RadarRows = new List<EntityAnalyticsRadarScoreRow>();
        public readonly List<EntityAnalyticsCurrentRow> CurrentRows = new List<EntityAnalyticsCurrentRow>();
        protected readonly HashSet<(string EntityType, int Year, int Month)> ClosedMonths =
            new HashSet<(string, int, int)>();

        public int ReplaceMonthlyHistoryCallCount { get; protected set; }
        public int ReplaceRankingCallCount { get; protected set; }
        public int ReplaceAttentionCallCount { get; protected set; }
        public int ReplaceRelationshipCallCount { get; protected set; }
        public int ReplaceRadarCallCount { get; protected set; }
        public bool ReplaceCurrentMetricsCalled { get; protected set; }

        public abstract IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId);

        public virtual IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetricsBatch(
            string entityType,
            IEnumerable<string> entityIds)
        {
            var idSet = new HashSet<string>(
                (entityIds ?? Array.Empty<string>()).Select(id => id?.Trim()).Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase);

            return CurrentRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && idSet.Contains(r.EntityId)
                    && !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                .ToList();
        }

        public virtual IReadOnlyList<EntityIdentity> SearchEntities(string entityType, string searchText, int top)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(searchText))
                return Array.Empty<EntityIdentity>();

            var query = searchText.Trim();
            var matches = CurrentRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var entityCode = group.Select(r => r.EntityCode).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c))
                        ?? group.Key;
                    var displayName = group.FirstOrDefault(r =>
                            string.Equals(r.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase))
                        ?.TextValue ?? entityCode;
                    var isActiveNumeric = group.FirstOrDefault(r =>
                            string.Equals(r.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase))
                        ?.NumericValue;
                    var isActive = !isActiveNumeric.HasValue || isActiveNumeric.Value > 0m;

                    return new
                    {
                        EntityId = group.Key,
                        EntityCode = entityCode,
                        DisplayName = displayName,
                        IsActive = isActive
                    };
                })
                .Where(x => x.EntityCode.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    || x.DisplayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.EntityCode, StringComparer.OrdinalIgnoreCase)
                .Take(top)
                .Select(x => new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = x.EntityId,
                    EntityCode = x.EntityCode,
                    DisplayName = x.DisplayName,
                    IsActive = x.IsActive
                })
                .ToList();

            return matches;
        }

        public abstract EntityIdentity TryResolveIdentity(string entityType, string entityId);

        public abstract void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId);

        public abstract DateTime? GetLatestGeneratedAt(string entityType, string entityId);

        public abstract bool HasAnyCurrentMetrics(string entityType);

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetHistory(
            string entityType,
            string entityId,
            int? fromYear = null,
            int? fromMonth = null,
            int? toYear = null,
            int? toMonth = null)
        {
            return MonthlyRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                .Where(r => !fromYear.HasValue || !fromMonth.HasValue
                    || r.PeriodYear > fromYear.Value
                    || (r.PeriodYear == fromYear.Value && r.PeriodMonth >= fromMonth.Value))
                .Where(r => !toYear.HasValue || !toMonth.HasValue
                    || r.PeriodYear < toYear.Value
                    || (r.PeriodYear == toYear.Value && r.PeriodMonth <= toMonth.Value))
                .OrderBy(r => r.PeriodYear)
                .ThenBy(r => r.PeriodMonth)
                .ThenBy(r => r.KpiId)
                .ToList();
        }

        public IReadOnlyList<(int Year, int Month)> GetLatestPeriods(
            string entityType,
            string entityId,
            int maxPeriods)
        {
            return MonthlyRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                .Select(r => (r.PeriodYear, r.PeriodMonth))
                .Distinct()
                .OrderByDescending(p => p.PeriodYear)
                .ThenByDescending(p => p.PeriodMonth)
                .Take(maxPeriods)
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .ToList();
        }

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyMetrics(
            string entityType, string entityId, int periodYear, int periodMonth)
            => GetHistory(entityType, entityId, periodYear, periodMonth, periodYear, periodMonth);

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyRange(
            string entityType, string entityId, int fromYear, int fromMonth, int toYear, int toMonth)
            => GetHistory(entityType, entityId, fromYear, fromMonth, toYear, toMonth);

        public virtual void SaveMonthlyHistory(
            string entityType,
            IEnumerable<EntityAnalyticsMonthlyRow> rows,
            string refreshLogId)
        {
            foreach (var row in rows ?? Array.Empty<EntityAnalyticsMonthlyRow>())
            {
                var existing = MonthlyRows.FirstOrDefault(r =>
                    string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, row.EntityId, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == row.PeriodYear
                    && r.PeriodMonth == row.PeriodMonth
                    && string.Equals(r.KpiId, row.KpiId, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    if (existing.IsClosed)
                        continue;

                    existing.NumericValue = row.NumericValue;
                    existing.TextValue = row.TextValue;
                    existing.PeriodSemantics = row.PeriodSemantics;
                    existing.GeneratedAt = row.GeneratedAt;
                    existing.LastRefreshLogId = refreshLogId;
                }
                else
                {
                    MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                    {
                        EntityType = entityType,
                        EntityId = row.EntityId,
                        EntityCode = row.EntityCode,
                        PeriodYear = row.PeriodYear,
                        PeriodMonth = row.PeriodMonth,
                        KpiId = row.KpiId,
                        NumericValue = row.NumericValue,
                        TextValue = row.TextValue,
                        PeriodSemantics = row.PeriodSemantics,
                        DefinitionVersion = row.DefinitionVersion,
                        IsClosed = row.IsClosed,
                        GeneratedAt = row.GeneratedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }
        }

        public virtual void CloseMonth(string entityType, int periodYear, int periodMonth, string refreshLogId)
        {
            ClosedMonths.Add((entityType, periodYear, periodMonth));
            foreach (var row in MonthlyRows.Where(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth))
            {
                row.IsClosed = true;
            }
        }

        public bool IsMonthClosed(string entityType, int periodYear, int periodMonth)
            => ClosedMonths.Contains((entityType, periodYear, periodMonth));

        public virtual void PurgeHistoryOlderThan(string entityType, int retentionMonths)
        {
        }

        public virtual void SaveRankingHistory(
            string entityType,
            IEnumerable<EntityAnalyticsRankingRow> rows,
            string refreshLogId)
        {
            foreach (var row in rows ?? Array.Empty<EntityAnalyticsRankingRow>())
            {
                var existing = RankingRows.FirstOrDefault(r =>
                    string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, row.EntityId, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == row.PeriodYear
                    && r.PeriodMonth == row.PeriodMonth
                    && string.Equals(r.RankMetricKpiId, row.RankMetricKpiId, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.RankPosition = row.RankPosition;
                    existing.PopulationSize = row.PopulationSize;
                    existing.Percentile = row.Percentile;
                    existing.GeneratedAt = row.GeneratedAt;
                    existing.LastRefreshLogId = refreshLogId;
                }
                else
                {
                    RankingRows.Add(new EntityAnalyticsRankingRow
                    {
                        EntityType = entityType,
                        EntityId = row.EntityId,
                        EntityCode = row.EntityCode,
                        RankMetricKpiId = row.RankMetricKpiId,
                        PeriodYear = row.PeriodYear,
                        PeriodMonth = row.PeriodMonth,
                        RankPosition = row.RankPosition,
                        PopulationSize = row.PopulationSize,
                        Percentile = row.Percentile,
                        GeneratedAt = row.GeneratedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }
        }

        public IReadOnlyList<EntityAnalyticsRankingRow> GetRankingHistory(
            string entityType,
            string entityId,
            string rankMetricKpiId,
            int? fromYear = null,
            int? fromMonth = null,
            int? toYear = null,
            int? toMonth = null)
        {
            return RankingRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.RankMetricKpiId, rankMetricKpiId, StringComparison.OrdinalIgnoreCase))
                .Where(r => !fromYear.HasValue || !fromMonth.HasValue
                    || r.PeriodYear > fromYear.Value
                    || (r.PeriodYear == fromYear.Value && r.PeriodMonth >= fromMonth.Value))
                .Where(r => !toYear.HasValue || !toMonth.HasValue
                    || r.PeriodYear < toYear.Value
                    || (r.PeriodYear == toYear.Value && r.PeriodMonth <= toMonth.Value))
                .OrderBy(r => r.PeriodYear)
                .ThenBy(r => r.PeriodMonth)
                .ToList();
        }

        public EntityAnalyticsRankingRow GetLatestRanking(
            string entityType,
            string entityId,
            string rankMetricKpiId)
        {
            return RankingRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.RankMetricKpiId, rankMetricKpiId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.PeriodYear)
                .ThenByDescending(r => r.PeriodMonth)
                .FirstOrDefault();
        }

        public IReadOnlyList<(int Year, int Month)> GetRankingPeriods(
            string entityType,
            string entityId,
            string rankMetricKpiId,
            int maxPeriods)
        {
            return RankingRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.RankMetricKpiId, rankMetricKpiId, StringComparison.OrdinalIgnoreCase))
                .Select(r => (r.PeriodYear, r.PeriodMonth))
                .Distinct()
                .OrderByDescending(p => p.PeriodYear)
                .ThenByDescending(p => p.PeriodMonth)
                .Take(maxPeriods)
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .Select(p => (p.PeriodYear, p.PeriodMonth))
                .ToList();
        }

        public IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetPeriodMetricsForPopulation(
            string entityType,
            int periodYear,
            int periodMonth,
            string kpiId)
        {
            var metrics = MonthlyRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == periodYear
                    && r.PeriodMonth == periodMonth
                    && string.Equals(r.KpiId, kpiId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return metrics.Select(m =>
            {
                var isActiveRow = CurrentRows.FirstOrDefault(c =>
                    string.Equals(c.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(c.EntityId, m.EntityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(c.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase));

                var isActive = (isActiveRow?.NumericValue ?? 1m) > 0m;

                return new EntityAnalyticsPeriodMetricRow
                {
                    EntityId = m.EntityId,
                    EntityCode = m.EntityCode,
                    NumericValue = m.NumericValue,
                    IsActive = isActive
                };
            }).ToList();
        }

        public IReadOnlyList<EntityAnalyticsAttentionEventRow> GetAttentionEvents(string entityType, string entityId)
        {
            return AttentionRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.IsActive)
                .ThenByDescending(r => r.LastSeenPeriodYear)
                .ThenByDescending(r => r.LastSeenPeriodMonth)
                .ThenBy(r => r.SignalTitle, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public virtual void SaveAttentionRecords(
            string entityType,
            IEnumerable<EntityAnalyticsAttentionEventRow> rows,
            string refreshLogId)
        {
            foreach (var row in rows ?? Array.Empty<EntityAnalyticsAttentionEventRow>())
            {
                var existing = AttentionRows.FirstOrDefault(r =>
                    string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, row.EntityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.SignalCode, row.SignalCode, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.EntityCode = row.EntityCode;
                    existing.SignalCategory = row.SignalCategory;
                    existing.SignalTitle = row.SignalTitle;
                    existing.FirstSeenPeriodYear = row.FirstSeenPeriodYear;
                    existing.FirstSeenPeriodMonth = row.FirstSeenPeriodMonth;
                    existing.LastSeenPeriodYear = row.LastSeenPeriodYear;
                    existing.LastSeenPeriodMonth = row.LastSeenPeriodMonth;
                    existing.ConsecutivePeriods = row.ConsecutivePeriods;
                    existing.TotalOccurrences = row.TotalOccurrences;
                    existing.IsActive = row.IsActive;
                    existing.GeneratedAt = row.GeneratedAt;
                    existing.UpdatedAt = row.UpdatedAt;
                    existing.LastRefreshLogId = refreshLogId;
                }
                else
                {
                    AttentionRows.Add(new EntityAnalyticsAttentionEventRow
                    {
                        EntityAnalyticsAttentionId = row.EntityAnalyticsAttentionId,
                        EntityType = entityType,
                        EntityId = row.EntityId,
                        EntityCode = row.EntityCode,
                        SignalCode = row.SignalCode,
                        SignalCategory = row.SignalCategory,
                        SignalTitle = row.SignalTitle,
                        FirstSeenPeriodYear = row.FirstSeenPeriodYear,
                        FirstSeenPeriodMonth = row.FirstSeenPeriodMonth,
                        LastSeenPeriodYear = row.LastSeenPeriodYear,
                        LastSeenPeriodMonth = row.LastSeenPeriodMonth,
                        ConsecutivePeriods = row.ConsecutivePeriods,
                        TotalOccurrences = row.TotalOccurrences,
                        IsActive = row.IsActive,
                        GeneratedAt = row.GeneratedAt,
                        CreatedAt = row.CreatedAt == default ? row.GeneratedAt : row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }
        }

        public IReadOnlyList<EntityAnalyticsRelationshipRow> GetRelationshipRollups(
            string entityType, string entityId, string relationshipCode = null)
        {
            var query = RelationshipRows.Where(r =>
                string.Equals(r.SourceEntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(r.SourceEntityId, entityId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(relationshipCode))
            {
                query = query.Where(r => string.Equals(r.RelationshipCode, relationshipCode, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderBy(r => r.RelationshipCode).ThenBy(r => r.Rank).ToList();
        }

        public virtual void ReplaceRelationshipRollups(
            string sourceEntityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRelationshipRow> rows,
            string refreshLogId)
        {
            RelationshipRows.RemoveAll(r =>
                string.Equals(r.SourceEntityType, sourceEntityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth);

            if (rows == null)
                return;

            foreach (var row in rows)
            {
                RelationshipRows.Add(new EntityAnalyticsRelationshipRow
                {
                    SourceEntityType = sourceEntityType,
                    SourceEntityId = row.SourceEntityId,
                    SourceEntityCode = row.SourceEntityCode,
                    RelationshipCode = row.RelationshipCode,
                    TargetEntityType = row.TargetEntityType,
                    TargetEntityId = row.TargetEntityId,
                    TargetEntityCode = row.TargetEntityCode,
                    TargetDisplayName = row.TargetDisplayName,
                    Rank = row.Rank,
                    MetricValue = row.MetricValue,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    GeneratedAt = row.GeneratedAt
                });
            }
        }

        public IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScores(
            string entityType, string entityId, int periodYear, int periodMonth)
        {
            return RadarRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == periodYear
                    && r.PeriodMonth == periodMonth)
                .OrderBy(r => r.AxisKpiId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScoresBatch(
            string entityType,
            IEnumerable<string> entityIds,
            int periodYear,
            int periodMonth)
        {
            var idSet = new HashSet<string>(
                (entityIds ?? Array.Empty<string>()).Select(id => id?.Trim()).Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase);

            return RadarRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && idSet.Contains(r.EntityId)
                    && r.PeriodYear == periodYear
                    && r.PeriodMonth == periodMonth)
                .OrderBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.AxisKpiId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<(int Year, int Month)> GetLatestRadarPeriods(
            string entityType,
            string entityId,
            int maxPeriods)
        {
            return RadarRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                .Select(r => (r.PeriodYear, r.PeriodMonth))
                .Distinct()
                .OrderByDescending(p => p.PeriodYear)
                .ThenByDescending(p => p.PeriodMonth)
                .Take(maxPeriods)
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .ToList();
        }

        public virtual void SaveRadarScores(
            string entityType,
            IEnumerable<EntityAnalyticsRadarScoreRow> rows,
            string refreshLogId)
        {
            foreach (var row in rows ?? Array.Empty<EntityAnalyticsRadarScoreRow>())
            {
                var existing = RadarRows.FirstOrDefault(r =>
                    string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, row.EntityId, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == row.PeriodYear
                    && r.PeriodMonth == row.PeriodMonth
                    && string.Equals(r.AxisKpiId, row.AxisKpiId, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Score = row.Score;
                    existing.PeerGroupRuleId = row.PeerGroupRuleId;
                    existing.PeerGroupSize = row.PeerGroupSize;
                    existing.NormalizationMethod = row.NormalizationMethod;
                    existing.GeneratedAt = row.GeneratedAt;
                    existing.LastRefreshLogId = refreshLogId;
                }
                else
                {
                    RadarRows.Add(new EntityAnalyticsRadarScoreRow
                    {
                        EntityType = entityType,
                        EntityId = row.EntityId,
                        EntityCode = row.EntityCode ?? row.EntityId,
                        PeriodYear = row.PeriodYear,
                        PeriodMonth = row.PeriodMonth,
                        AxisKpiId = row.AxisKpiId,
                        Score = row.Score,
                        PeerGroupRuleId = row.PeerGroupRuleId,
                        PeerGroupSize = row.PeerGroupSize,
                        NormalizationMethod = row.NormalizationMethod,
                        GeneratedAt = row.GeneratedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }
        }

        public virtual void ReplaceMonthlyHistoryForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsMonthlyRow> rows,
            string refreshLogId,
            int batchSize = 0)
        {
            ReplaceMonthlyHistoryCallCount++;
            MonthlyRows.RemoveAll(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth);

            foreach (var row in rows ?? Array.Empty<EntityAnalyticsMonthlyRow>())
            {
                MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = entityType,
                    EntityId = row.EntityId,
                    EntityCode = row.EntityCode,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    KpiId = row.KpiId,
                    NumericValue = row.NumericValue,
                    TextValue = row.TextValue,
                    PeriodSemantics = row.PeriodSemantics,
                    DefinitionVersion = row.DefinitionVersion,
                    IsClosed = true,
                    GeneratedAt = row.GeneratedAt,
                    LastRefreshLogId = refreshLogId
                });
            }
        }

        public virtual void ReplaceRankingForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRankingRow> rows,
            string refreshLogId)
        {
            ReplaceRankingCallCount++;
            RankingRows.RemoveAll(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth);

            foreach (var row in rows ?? Array.Empty<EntityAnalyticsRankingRow>())
            {
                RankingRows.Add(new EntityAnalyticsRankingRow
                {
                    EntityType = entityType,
                    EntityId = row.EntityId,
                    EntityCode = row.EntityCode ?? row.EntityId,
                    RankMetricKpiId = row.RankMetricKpiId,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    RankPosition = row.RankPosition,
                    PopulationSize = row.PopulationSize,
                    Percentile = row.Percentile,
                    GeneratedAt = row.GeneratedAt,
                    LastRefreshLogId = refreshLogId
                });
            }
        }

        public virtual void ReplaceAttentionForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsAttentionEventRow> rows,
            string refreshLogId)
        {
            ReplaceAttentionCallCount++;
            SaveAttentionRecords(entityType, rows, refreshLogId);
        }

        public virtual void ReplaceRelationshipForPeriod(
            string sourceEntityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRelationshipRow> rows,
            string refreshLogId)
        {
            ReplaceRelationshipCallCount++;
            RelationshipRows.RemoveAll(r =>
                string.Equals(r.SourceEntityType, sourceEntityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth);

            foreach (var row in rows ?? Array.Empty<EntityAnalyticsRelationshipRow>())
            {
                RelationshipRows.Add(new EntityAnalyticsRelationshipRow
                {
                    SourceEntityType = sourceEntityType,
                    SourceEntityId = row.SourceEntityId,
                    SourceEntityCode = row.SourceEntityCode,
                    RelationshipCode = row.RelationshipCode,
                    TargetEntityType = row.TargetEntityType,
                    TargetEntityId = row.TargetEntityId,
                    TargetEntityCode = row.TargetEntityCode,
                    TargetDisplayName = row.TargetDisplayName,
                    Rank = row.Rank,
                    MetricValue = row.MetricValue,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    GeneratedAt = row.GeneratedAt
                });
            }
        }

        public virtual void ReplaceRadarForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRadarScoreRow> rows,
            string refreshLogId)
        {
            ReplaceRadarCallCount++;
            RadarRows.RemoveAll(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && r.PeriodYear == periodYear
                && r.PeriodMonth == periodMonth);

            foreach (var row in rows ?? Array.Empty<EntityAnalyticsRadarScoreRow>())
            {
                RadarRows.Add(new EntityAnalyticsRadarScoreRow
                {
                    EntityType = entityType,
                    EntityId = row.EntityId,
                    EntityCode = row.EntityCode ?? row.EntityId,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    AxisKpiId = row.AxisKpiId,
                    Score = row.Score,
                    PeerGroupRuleId = row.PeerGroupRuleId,
                    PeerGroupSize = row.PeerGroupSize,
                    NormalizationMethod = row.NormalizationMethod,
                    GeneratedAt = row.GeneratedAt,
                    LastRefreshLogId = refreshLogId
                });
            }
        }

        public virtual IReadOnlyList<EntityPopulationRow> GetActivePopulation(string entityType, string dimensionKpiId = null)
        {
            var groups = CurrentRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase);

            var population = new List<EntityPopulationRow>();
            foreach (var group in groups)
            {
                var isActiveNumeric = group.FirstOrDefault(r =>
                    string.Equals(r.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase))?.NumericValue;
                var isActive = !isActiveNumeric.HasValue || isActiveNumeric.Value > 0m;
                if (!isActive)
                    continue;

                var entityCode = group.Select(r => r.EntityCode).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? group.Key;
                string dimensionValue = null;
                if (!string.IsNullOrWhiteSpace(dimensionKpiId))
                {
                    dimensionValue = group.FirstOrDefault(r =>
                        string.Equals(r.KpiId, dimensionKpiId, StringComparison.OrdinalIgnoreCase))?.TextValue;
                }

                population.Add(new EntityPopulationRow
                {
                    EntityId = group.Key,
                    EntityCode = entityCode,
                    IsActive = true,
                    DimensionValue = dimensionValue
                });
            }

            return population;
        }

        public virtual IReadOnlyList<EntityPopulationRow> GetPeriodActivePopulation(
            string entityType,
            int periodYear,
            int periodMonth,
            string dimensionKpiId = null)
        {
            var groups = MonthlyRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && r.PeriodYear == periodYear
                    && r.PeriodMonth == periodMonth)
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase);

            var population = new List<EntityPopulationRow>();
            foreach (var group in groups)
            {
                var isActiveNumeric = CurrentRows.FirstOrDefault(r =>
                    string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, group.Key, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase))?.NumericValue;
                var isActive = !isActiveNumeric.HasValue || isActiveNumeric.Value > 0m;
                if (!isActive)
                    continue;

                var entityCode = group.Select(r => r.EntityCode).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? group.Key;
                string dimensionValue = null;
                if (!string.IsNullOrWhiteSpace(dimensionKpiId))
                {
                    dimensionValue = CurrentRows.FirstOrDefault(r =>
                        string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.EntityId, group.Key, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.KpiId, dimensionKpiId, StringComparison.OrdinalIgnoreCase))?.TextValue;
                }

                population.Add(new EntityPopulationRow
                {
                    EntityId = group.Key,
                    EntityCode = entityCode,
                    IsActive = true,
                    DimensionValue = dimensionValue
                });
            }

            return population;
        }

        public virtual IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentKpiPopulation(string entityType, string kpiId)
        {
            return CurrentRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.KpiId, kpiId, StringComparison.OrdinalIgnoreCase))
                .Select(r => new EntityAnalyticsPeriodMetricRow
                {
                    EntityId = r.EntityId,
                    EntityCode = r.EntityCode,
                    NumericValue = r.NumericValue,
                    IsActive = true
                })
                .ToList();
        }

        public virtual IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentDimensionPopulation(
            string entityType,
            string dimensionKpiId)
        {
            return CurrentRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.KpiId, dimensionKpiId, StringComparison.OrdinalIgnoreCase))
                .Select(r =>
                {
                    decimal? numeric = null;
                    if (!string.IsNullOrWhiteSpace(r.TextValue)
                        && decimal.TryParse(r.TextValue, out var parsed))
                    {
                        numeric = parsed;
                    }

                    return new EntityAnalyticsPeriodMetricRow
                    {
                        EntityId = r.EntityId,
                        EntityCode = r.EntityCode,
                        NumericValue = numeric,
                        IsActive = true
                    };
                })
                .ToList();
        }

        public virtual IReadOnlyDictionary<string, int> GetActiveAttentionCounts(string entityType)
        {
            return AttentionRows
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase) && r.IsActive)
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
