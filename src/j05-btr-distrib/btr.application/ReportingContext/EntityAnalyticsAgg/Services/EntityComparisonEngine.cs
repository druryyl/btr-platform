using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityComparisonEngine : IEntityComparisonEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly EntityKpiEnvelopeFormatter _envelopeFormatter;
        private readonly IEntityTrendEngine _trendEngine;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityRadarEngine _radarEngine;

        public EntityComparisonEngine(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityTypeRegistry entityTypes,
            EntityKpiEnvelopeFormatter envelopeFormatter,
            IEntityTrendEngine trendEngine,
            IEntityRankingEngine rankingEngine,
            IEntityAttentionEngine attentionEngine,
            IEntityRelationshipEngine relationshipEngine,
            IEntityRadarEngine radarEngine)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
            _envelopeFormatter = envelopeFormatter ?? new EntityKpiEnvelopeFormatter();
            _trendEngine = trendEngine;
            _rankingEngine = rankingEngine;
            _attentionEngine = attentionEngine;
            _relationshipEngine = relationshipEngine;
            _radarEngine = radarEngine;
        }

        public ProfileComparisonSectionDto BuildCrossPeriodSection(string entityType, string entityId)
        {
            var currentMetrics = _repository.GetCurrentMetrics(entityType, entityId);
            var comparisonKpis = ResolveComparisonKpis(entityType, null);

            if (comparisonKpis.Count == 0)
            {
                return new ProfileComparisonSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                    Metrics = new List<ComparisonMetricDto>()
                };
            }

            if (currentMetrics.Count == 0)
            {
                return new ProfileComparisonSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Metrics = new List<ComparisonMetricDto>()
                };
            }

            var anchor = ResolveAnchorPeriod(entityType, entityId);
            var momPeriod = EntityComparisonCalculator.ShiftMonth(anchor.Year, anchor.Month, -1);
            var yoyYear = anchor.Year - 1;
            var yoyMonth = anchor.Month;

            var momRows = _repository.GetMonthlyMetrics(entityType, entityId, momPeriod.Year, momPeriod.Month);
            var yoyRows = _repository.GetMonthlyMetrics(entityType, entityId, yoyYear, yoyMonth);

            var metrics = new List<ComparisonMetricDto>();
            foreach (var kpi in comparisonKpis)
            {
                var currentRow = currentMetrics.FirstOrDefault(r =>
                    string.Equals(r.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase));
                if (currentRow == null)
                    continue;

                var momRow = momRows.FirstOrDefault(r =>
                    string.Equals(r.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase));
                var yoyRow = yoyRows.FirstOrDefault(r =>
                    string.Equals(r.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase));

                metrics.Add(BuildComparisonMetric(
                    kpi,
                    currentRow.NumericValue,
                    momRow?.NumericValue,
                    yoyRow?.NumericValue,
                    BuildCurrentPeriodLabel(anchor.Year, anchor.Month, anchor.IsOpen, kpi.PeriodSemantics),
                    BuildPeriodLabel(momPeriod.Year, momPeriod.Month, momRow?.IsClosed ?? true, kpi.PeriodSemantics),
                    BuildPeriodLabel(yoyYear, yoyMonth, yoyRow?.IsClosed ?? true, kpi.PeriodSemantics)));
            }

            return new ProfileComparisonSectionDto
            {
                IsAvailable = metrics.Count > 0,
                UnavailableReason = metrics.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Metrics = metrics
            };
        }

        public EntityCompareResponse BuildMultiEntityComparison(ComparisonContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var normalizedType = _entityTypes.NormalizeEntityTypeCode(context.EntityType);
            if (normalizedType is null)
                throw new ArgumentException($"Unknown entity type: {context.EntityType}", nameof(context));

            var entityIds = NormalizeEntityIds(context.EntityIds);
            if (entityIds.Count < 2 || entityIds.Count > 5)
                throw new ArgumentException("Between 2 and 5 entity IDs are required.", nameof(context));

            var comparisonKpis = ResolveComparisonKpis(normalizedType, context.MetricKpiIds);
            var columns = BuildEntityColumns(normalizedType, entityIds);
            var warnings = BuildWarnings(columns);
            var internalEntityIds = columns.Select(c => c.EntityId).ToList();

            return new EntityCompareResponse
            {
                EntityType = normalizedType,
                ContractVersion = EntityAnalyticsConstants.ProfileContractVersion,
                Entities = columns,
                KpiComparison = BuildKpiComparison(normalizedType, internalEntityIds, columns, comparisonKpis),
                TrendComparison = BuildTrendComparison(normalizedType, internalEntityIds, columns, comparisonKpis),
                RankingComparison = BuildRankingComparison(normalizedType, internalEntityIds, columns),
                AttentionComparison = BuildAttentionComparison(normalizedType, internalEntityIds, columns),
                RelationshipComparison = BuildRelationshipComparison(normalizedType, internalEntityIds, columns),
                RadarComparison = BuildRadarComparison(normalizedType, internalEntityIds, columns, context),
                PeerComparison = BuildPeerComparison(normalizedType, internalEntityIds, columns),
                Warnings = warnings
            };
        }

        private CompareKpiSectionDto BuildKpiComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns,
            IReadOnlyList<EntityKpiMetadata> comparisonKpis)
        {
            if (comparisonKpis.Count == 0)
            {
                return new CompareKpiSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                    Rows = new List<CompareKpiRowDto>()
                };
            }

            var batchMetrics = _repository.GetCurrentMetricsBatch(entityType, entityIds);
            var metricsByEntity = batchMetrics
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var rows = new List<CompareKpiRowDto>();
            foreach (var kpi in comparisonKpis)
            {
                var cells = new List<CompareKpiCellDto>();
                foreach (var column in columns)
                {
                    metricsByEntity.TryGetValue(column.EntityId, out var entityMetrics);
                    var row = entityMetrics?
                        .FirstOrDefault(r => string.Equals(r.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase));

                    cells.Add(new CompareKpiCellDto
                    {
                        EntityCode = column.EntityCode,
                        DisplayName = column.DisplayName,
                        Value = row?.NumericValue,
                        FormattedValue = row != null
                            ? _envelopeFormatter.FormatValue(row.NumericValue, row.TextValue, kpi)
                            : _envelopeFormatter.FormatValue(null, null, kpi),
                        PeriodLabel = kpi.PeriodSemantics
                    });
                }

                if (cells.Any(c => c.Value.HasValue))
                {
                    rows.Add(new CompareKpiRowDto
                    {
                        KpiId = kpi.KpiId,
                        DisplayName = kpi.DisplayName,
                        Unit = kpi.Unit,
                        Direction = kpi.Direction,
                        Values = cells
                    });
                }
            }

            return new CompareKpiSectionDto
            {
                IsAvailable = rows.Count > 0,
                UnavailableReason = rows.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Rows = rows
            };
        }

        private CompareTrendSectionDto BuildTrendComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns,
            IReadOnlyList<EntityKpiMetadata> comparisonKpis)
        {
            if (comparisonKpis.Count == 0)
            {
                return new CompareTrendSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                    Overlays = new List<CompareTrendOverlayDto>()
                };
            }

            var trendByEntity = entityIds.ToDictionary(
                id => id,
                id => _trendEngine.BuildTrendSection(entityType, id),
                StringComparer.OrdinalIgnoreCase);

            var overlays = new List<CompareTrendOverlayDto>();
            foreach (var kpi in comparisonKpis)
            {
                var entitySeries = new List<CompareTrendEntitySeriesDto>();
                foreach (var column in columns)
                {
                    if (!trendByEntity.TryGetValue(column.EntityId, out var trendSection)
                        || trendSection?.Series == null)
                    {
                        continue;
                    }

                    var series = trendSection.Series.FirstOrDefault(s =>
                        string.Equals(s.KpiId, kpi.KpiId, StringComparison.OrdinalIgnoreCase));
                    if (series == null || series.Points == null || series.Points.Count == 0)
                        continue;

                    entitySeries.Add(new CompareTrendEntitySeriesDto
                    {
                        EntityCode = column.EntityCode,
                        DisplayName = column.DisplayName,
                        Points = series.Points
                    });
                }

                if (entitySeries.Count > 0)
                {
                    overlays.Add(new CompareTrendOverlayDto
                    {
                        KpiId = kpi.KpiId,
                        DisplayName = kpi.DisplayName,
                        Unit = kpi.Unit,
                        EntitySeries = entitySeries
                    });
                }
            }

            return new CompareTrendSectionDto
            {
                IsAvailable = overlays.Count > 0,
                UnavailableReason = overlays.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Overlays = overlays
            };
        }

        private CompareRankingSectionDto BuildRankingComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns)
        {
            var entities = columns
                .Select(column => new CompareRankingEntityDto
                {
                    EntityCode = column.EntityCode,
                    DisplayName = column.DisplayName,
                    Ranking = _rankingEngine.BuildRankingSection(entityType, column.EntityId)
                })
                .Where(e => e.Ranking != null)
                .ToList();

            return new CompareRankingSectionDto
            {
                IsAvailable = entities.Any(e => e.Ranking.IsAvailable),
                UnavailableReason = entities.Any(e => e.Ranking.IsAvailable)
                    ? null
                    : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Entities = entities
            };
        }

        private CompareAttentionSectionDto BuildAttentionComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns)
        {
            var entities = columns
                .Select(column => new CompareAttentionEntityDto
                {
                    EntityCode = column.EntityCode,
                    DisplayName = column.DisplayName,
                    Attention = _attentionEngine.BuildAttentionSection(entityType, column.EntityId)
                })
                .ToList();

            return new CompareAttentionSectionDto
            {
                IsAvailable = entities.Any(e => e.Attention != null && e.Attention.IsAvailable),
                UnavailableReason = entities.Any(e => e.Attention != null && e.Attention.IsAvailable)
                    ? null
                    : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Entities = entities
            };
        }

        private CompareRelationshipSectionDto BuildRelationshipComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns)
        {
            var entities = columns
                .Select(column => new CompareRelationshipEntityDto
                {
                    EntityCode = column.EntityCode,
                    DisplayName = column.DisplayName,
                    RelatedEntities = _relationshipEngine.BuildRelatedEntitiesSection(entityType, column.EntityId)
                })
                .ToList();

            return new CompareRelationshipSectionDto
            {
                IsAvailable = entities.Any(e => e.RelatedEntities != null && e.RelatedEntities.IsAvailable),
                UnavailableReason = entities.Any(e => e.RelatedEntities != null && e.RelatedEntities.IsAvailable)
                    ? null
                    : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Entities = entities
            };
        }

        private CompareRadarSectionDto BuildRadarComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns,
            ComparisonContext context)
        {
            var period = ResolveRadarPeriod(entityType, entityIds, context);
            if (!period.HasValue)
            {
                return new CompareRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Axes = new List<CompareRadarAxisDto>(),
                    Overlays = new List<CompareRadarOverlayDto>()
                };
            }

            var radarRows = _repository.GetRadarScoresBatch(
                entityType,
                entityIds,
                period.Value.Year,
                period.Value.Month);

            if (radarRows.Count == 0)
            {
                return new CompareRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Axes = new List<CompareRadarAxisDto>(),
                    Overlays = new List<CompareRadarOverlayDto>()
                };
            }

            var peerGroupSize = radarRows.Max(r => r.PeerGroupSize);
            var peerGroupRuleId = radarRows.Select(r => r.PeerGroupRuleId).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));
            if (peerGroupSize < EntityAnalyticsConstants.MinRadarPeerGroupSize)
            {
                return new CompareRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.PeerGroupTooSmall,
                    PeerGroupRuleId = peerGroupRuleId,
                    PeerGroupSize = peerGroupSize,
                    PeriodYear = period.Value.Year,
                    PeriodMonth = period.Value.Month,
                    Axes = new List<CompareRadarAxisDto>(),
                    Overlays = new List<CompareRadarOverlayDto>()
                };
            }

            var radarAxes = ResolveRadarAxes(entityType);
            var axisOrder = radarAxes
                .Select(axis => axis.KpiId)
                .ToList();

            var compareAxes = radarAxes
                .Select(axis => new CompareRadarAxisDto
                {
                    KpiId = axis.KpiId,
                    SignatureDimensionKey = axis.SignatureDimensionKey,
                    DisplayName = EntityRadarEngine.ResolveAxisDisplayName(axis),
                    Direction = axis.Direction
                })
                .ToList();

            var rowsByEntity = radarRows
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var overlays = new List<CompareRadarOverlayDto>();
            foreach (var column in columns)
            {
                if (!rowsByEntity.TryGetValue(column.EntityId, out var entityRows))
                    continue;

                var scoreLookup = entityRows.ToDictionary(r => r.AxisKpiId, r => r.Score, StringComparer.OrdinalIgnoreCase);
                overlays.Add(new CompareRadarOverlayDto
                {
                    EntityCode = column.EntityCode,
                    DisplayName = column.DisplayName,
                    Scores = axisOrder
                        .Select(axisId => scoreLookup.TryGetValue(axisId, out var score) ? score : null)
                        .ToList()
                });
            }

            List<decimal?> peerAverageScores = null;
            if (_radarEngine is EntityRadarEngine concreteRadar && columns.Count > 0)
            {
                peerAverageScores = concreteRadar.ComputePeerAverageScores(
                    entityType,
                    columns[0].EntityId,
                    period.Value.Year,
                    period.Value.Month,
                    radarAxes,
                    peerGroupRuleId,
                    peerGroupSize);
            }

            return new CompareRadarSectionDto
            {
                IsAvailable = overlays.Count > 0 && compareAxes.Count > 0,
                UnavailableReason = overlays.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                PeerGroupRuleId = peerGroupRuleId,
                PeerGroupSize = peerGroupSize,
                PeriodYear = period.Value.Year,
                PeriodMonth = period.Value.Month,
                PeriodLabel = BuildRadarPeriodLabel(period.Value.Year, period.Value.Month),
                Axes = compareAxes,
                Overlays = overlays,
                PeerAverageScores = peerAverageScores
            };
        }

        private ComparePeerSectionDto BuildPeerComparison(
            string entityType,
            IReadOnlyList<string> entityIds,
            IReadOnlyList<CompareEntityColumnDto> columns)
        {
            if (columns.Count == 0)
            {
                return new ComparePeerSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData
                };
            }

            var primary = columns[0];
            var radar = _radarEngine.BuildRadarSection(entityType, primary.EntityId);

            return new ComparePeerSectionDto
            {
                IsAvailable = radar.IsAvailable,
                UnavailableReason = radar.UnavailableReason,
                EntityCode = primary.EntityCode,
                DisplayName = primary.DisplayName,
                PeerGroupRuleId = radar.PeerGroupRuleId,
                PeerGroupSize = radar.PeerGroupSize,
                Radar = radar
            };
        }

        private (int Year, int Month)? ResolveRadarPeriod(
            string entityType,
            IReadOnlyList<string> entityIds,
            ComparisonContext context)
        {
            if (context?.PeriodYear.HasValue == true && context.PeriodMonth.HasValue)
                return (context.PeriodYear.Value, context.PeriodMonth.Value);

            foreach (var entityId in entityIds)
            {
                var periods = _repository.GetLatestRadarPeriods(entityType, entityId, 1);
                if (periods.Count > 0)
                    return periods[0];
            }

            return null;
        }

        private IReadOnlyList<EntityKpiMetadata> ResolveRadarAxes(string entityType)
        {
            var packId = _entityTypes.TryGet(entityType, out var registration)
                ? registration.KpiPackId
                : null;

            if (string.IsNullOrWhiteSpace(packId))
                return Array.Empty<EntityKpiMetadata>();

            return _kpiRegistry.ResolvePackMetadata(packId)
                .Where(m => m.RadarEligible
                    && !string.Equals(m.Direction, "Neutral", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => EntityAnalyticsSignatureDimensions.GetOrderIndex(m.SignatureDimensionKey))
                .ThenBy(m => m.RadarAxisOrder)
                .ThenBy(m => m.KpiId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildRadarPeriodLabel(int year, int month)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            var now = DateTime.Now;
            if (year == now.Year && month == now.Month)
                return $"{monthName} {year} (MTD)";

            return $"{monthName} {year}";
        }

        private List<CompareEntityColumnDto> BuildEntityColumns(string entityType, IReadOnlyList<string> entityIds)
        {
            var columns = new List<CompareEntityColumnDto>();
            foreach (var entityId in entityIds)
            {
                var identity = _repository.TryResolveIdentity(entityType, entityId);
                if (identity == null)
                    continue;

                var internalId = identity.EntityId;
                var businessCode = identity.EntityCode;
                columns.Add(new CompareEntityColumnDto
                {
                    EntityType = entityType,
                    EntityId = internalId,
                    EntityCode = businessCode,
                    DisplayName = identity.DisplayName,
                    IsActive = identity.IsActive,
                    GeneratedAt = _repository.GetLatestGeneratedAt(entityType, internalId),
                    ProfileRoute = EntityAnalyticsRouteBuilder.BuildProfileRoute(
                        _entityTypes,
                        entityType,
                        internalId)
                });
            }

            return columns;
        }

        private static List<string> BuildWarnings(IReadOnlyList<CompareEntityColumnDto> columns)
        {
            var warnings = new List<string>();
            var timestamps = columns
                .Where(c => c.GeneratedAt.HasValue)
                .Select(c => c.GeneratedAt.Value)
                .Distinct()
                .ToList();

            if (timestamps.Count > 1)
                warnings.Add(EntityComparisonWarnings.GeneratedAtMismatch);

            return warnings;
        }

        private IReadOnlyList<EntityKpiMetadata> ResolveComparisonKpis(
            string entityType,
            IReadOnlyList<string> metricKpiIds)
        {
            if (metricKpiIds != null && metricKpiIds.Count > 0)
            {
                return metricKpiIds
                    .Select(id => _kpiRegistry.TryGetMetadata(id, out var metadata) ? metadata : null)
                    .Where(metadata => metadata != null)
                    .ToList();
            }

            var packId = _entityTypes.TryGet(entityType, out var registration)
                ? registration.KpiPackId
                : null;

            if (string.IsNullOrWhiteSpace(packId))
                return Array.Empty<EntityKpiMetadata>();

            return _kpiRegistry.ResolvePackMetadata(packId)
                .Where(m => m.TrendEligible)
                .ToList();
        }

        private static List<string> NormalizeEntityIds(IReadOnlyList<string> entityIds)
        {
            return (entityIds ?? Array.Empty<string>())
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private (int Year, int Month, bool IsOpen) ResolveAnchorPeriod(string entityType, string entityId)
        {
            var periods = _repository.GetLatestPeriods(entityType, entityId, 1);
            if (periods.Count > 0)
            {
                var latest = periods[periods.Count - 1];
                var monthly = _repository.GetMonthlyMetrics(entityType, entityId, latest.Year, latest.Month);
                var isOpen = monthly.Any() && monthly.Any(r => !r.IsClosed);
                return (latest.Year, latest.Month, isOpen);
            }

            var now = DateTime.Now;
            return (now.Year, now.Month, true);
        }

        private ComparisonMetricDto BuildComparisonMetric(
            EntityKpiMetadata kpi,
            decimal? currentValue,
            decimal? priorMonthValue,
            decimal? priorYearValue,
            string currentPeriodLabel,
            string priorMonthPeriodLabel,
            string priorYearPeriodLabel)
        {
            return new ComparisonMetricDto
            {
                KpiId = kpi.KpiId,
                DisplayName = kpi.DisplayName,
                Unit = kpi.Unit,
                Direction = kpi.Direction,
                CurrentValue = currentValue,
                CurrentFormatted = _envelopeFormatter.FormatValue(currentValue, null, kpi),
                CurrentPeriodLabel = currentPeriodLabel,
                PriorMonthValue = priorMonthValue,
                PriorMonthFormatted = priorMonthValue.HasValue
                    ? _envelopeFormatter.FormatValue(priorMonthValue, null, kpi)
                    : null,
                PriorMonthPeriodLabel = priorMonthPeriodLabel,
                MomDelta = EntityComparisonCalculator.ComputeDelta(currentValue, priorMonthValue),
                MomGrowthPercent = EntityComparisonCalculator.ComputeGrowthPercent(currentValue, priorMonthValue),
                PriorYearValue = priorYearValue,
                PriorYearFormatted = priorYearValue.HasValue
                    ? _envelopeFormatter.FormatValue(priorYearValue, null, kpi)
                    : null,
                PriorYearPeriodLabel = priorYearPeriodLabel,
                YoyDelta = EntityComparisonCalculator.ComputeDelta(currentValue, priorYearValue),
                YoyGrowthPercent = EntityComparisonCalculator.ComputeGrowthPercent(currentValue, priorYearValue)
            };
        }

        private static string BuildCurrentPeriodLabel(int year, int month, bool isOpen, string periodSemantics)
        {
            return BuildPeriodLabel(year, month, !isOpen, periodSemantics);
        }

        private static string BuildPeriodLabel(int year, int month, bool isClosed, string periodSemantics)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            var label = $"{monthName} {year}";

            if (!isClosed && string.Equals(periodSemantics, "MTD", StringComparison.OrdinalIgnoreCase))
                return $"{label} (MTD)";

            return label;
        }
    }
}
