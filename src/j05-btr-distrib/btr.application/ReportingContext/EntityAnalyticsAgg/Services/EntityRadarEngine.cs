using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Generic L5 radar engine. Peer-percentile normalization at refresh; profile reads L5 only.
    /// </summary>
    public class EntityRadarEngine : IEntityRadarEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;

        public EntityRadarEngine(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityTypeRegistry entityTypes)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
        }

        public void ComputeAndPersistScores(
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

            var radarAxes = ResolveRadarEligibleAxes(entityType);
            if (radarAxes.Count == 0)
                return;

            if (!_entityTypes.TryGet(entityType, out var registration)
                || string.IsNullOrWhiteSpace(registration.PeerGroupRuleId))
                return;

            var peerGroupRuleId = registration.PeerGroupRuleId;
            var dimensionKpiId = PeerGroupResolver.ResolveDimensionKpiId(peerGroupRuleId);
            var population = _repository.GetActivePopulation(entityType, dimensionKpiId);
            if (population.Count == 0)
                return;

            var peerGroupIndex = PeerGroupResolver.BuildPeerGroupIndex(peerGroupRuleId, population);
            var axisValueMaps = BuildAxisValueMaps(entityType, periodYear, periodMonth, radarAxes);
            var entityCodes = population.ToDictionary(
                p => p.EntityId,
                p => p.EntityCode ?? p.EntityId,
                StringComparer.OrdinalIgnoreCase);

            var allRows = new List<EntityAnalyticsRadarScoreRow>();

            foreach (var entity in population.Where(p => p.IsActive))
            {
                var peerResolution = PeerGroupResolver.ResolveForEntity(
                    entity.EntityId,
                    peerGroupRuleId,
                    peerGroupIndex,
                    population);

                if (!peerResolution.IsSufficient)
                    continue;

                var peerIdSet = new HashSet<string>(peerResolution.PeerEntityIds, StringComparer.OrdinalIgnoreCase);

                foreach (var axis in radarAxes)
                {
                    if (!axisValueMaps.TryGetValue(axis.KpiId, out var valueMap))
                        continue;

                    if (!valueMap.TryGetValue(entity.EntityId, out var entityValue) || !entityValue.HasValue)
                        continue;

                    var candidates = peerResolution.PeerEntityIds
                        .Where(id => valueMap.TryGetValue(id, out var value) && value.HasValue)
                        .Select(id => (EntityId: id, Value: valueMap[id].Value))
                        .ToList();

                    if (candidates.Count == 0)
                        continue;

                    var normalizationMethod = RadarNormalizationMethod.PeerPercentile;
                    decimal? score = null;

                    if (peerResolution.PeerGroupSize < EntityAnalyticsConstants.BandFallbackPeerGroupThreshold)
                    {
                        score = EntityRadarCalculator.TryResolveBandMidpointScore(
                            entityValue,
                            axis.NormalizationRule);
                        if (score.HasValue)
                            normalizationMethod = RadarNormalizationMethod.BandMidpoint;
                    }

                    if (!score.HasValue)
                    {
                        var results = EntityRadarCalculator.CalculatePeerPercentiles(candidates, axis.Direction);
                        score = results.FirstOrDefault(r =>
                            string.Equals(r.EntityId, entity.EntityId, StringComparison.OrdinalIgnoreCase))?.Score;
                    }

                    if (!score.HasValue)
                        continue;

                    allRows.Add(new EntityAnalyticsRadarScoreRow
                    {
                        EntityType = entityType,
                        EntityId = entity.EntityId,
                        EntityCode = entityCodes.TryGetValue(entity.EntityId, out var code) ? code : entity.EntityId,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth,
                        AxisKpiId = axis.KpiId,
                        Score = Math.Round(score.Value, 2, MidpointRounding.AwayFromZero),
                        PeerGroupRuleId = peerGroupRuleId,
                        PeerGroupSize = peerResolution.PeerGroupSize,
                        NormalizationMethod = normalizationMethod,
                        GeneratedAt = generatedAt,
                        LastRefreshLogId = refreshLogId
                    });
                }
            }

            if (replay != null)
                _repository.ReplaceRadarForPeriod(entityType, periodYear, periodMonth, allRows, refreshLogId);
            else
                _repository.SaveRadarScores(entityType, allRows, refreshLogId);
        }

        public ProfileRadarSectionDto BuildRadarSection(string entityType, string entityId)
        {
            var radarAxes = ResolveRadarEligibleAxes(entityType);
            if (radarAxes.Count == 0)
            {
                return new ProfileRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                    Axes = new List<ProfileRadarAxisDto>()
                };
            }

            var periods = _repository.GetLatestRadarPeriods(entityType, entityId, 1);
            if (periods.Count == 0)
            {
                return BuildUnavailableSection(entityType, entityId, radarAxes);
            }

            var period = periods[0];
            var rows = _repository.GetRadarScores(entityType, entityId, period.Year, period.Month);
            if (rows.Count == 0)
                return BuildUnavailableSection(entityType, entityId, radarAxes);

            var peerGroupSize = rows.Max(r => r.PeerGroupSize);
            var peerGroupRuleId = rows.Select(r => r.PeerGroupRuleId).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));

            if (peerGroupSize < EntityAnalyticsConstants.MinRadarPeerGroupSize)
            {
                return new ProfileRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.PeerGroupTooSmall,
                    PeerGroupRuleId = peerGroupRuleId,
                    PeerGroupSize = peerGroupSize,
                    PeriodYear = period.Year,
                    PeriodMonth = period.Month,
                    PeriodLabel = BuildPeriodLabel(period.Year, period.Month),
                    UnavailableExplanation = BuildPeerGroupTooSmallExplanation(peerGroupRuleId, peerGroupSize),
                    Axes = new List<ProfileRadarAxisDto>()
                };
            }

            var rowLookup = rows.ToDictionary(r => r.AxisKpiId, StringComparer.OrdinalIgnoreCase);
            var axes = new List<ProfileRadarAxisDto>();

            foreach (var axis in radarAxes)
            {
                if (!rowLookup.TryGetValue(axis.KpiId, out var row) || !row.Score.HasValue)
                    continue;

                axes.Add(new ProfileRadarAxisDto
                {
                    KpiId = axis.KpiId,
                    DisplayName = string.IsNullOrWhiteSpace(axis.RadarDisplayName) ? axis.DisplayName : axis.RadarDisplayName,
                    Score = row.Score,
                    Direction = axis.Direction
                });
            }

            return new ProfileRadarSectionDto
            {
                IsAvailable = axes.Count > 0,
                UnavailableReason = axes.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                PeerGroupRuleId = peerGroupRuleId,
                PeerGroupSize = peerGroupSize,
                PeriodYear = period.Year,
                PeriodMonth = period.Month,
                PeriodLabel = BuildPeriodLabel(period.Year, period.Month),
                Axes = axes
            };
        }

        private ProfileRadarSectionDto BuildUnavailableSection(
            string entityType,
            string entityId,
            IReadOnlyList<EntityKpiMetadata> radarAxes)
        {
            if (!_entityTypes.TryGet(entityType, out var registration)
                || string.IsNullOrWhiteSpace(registration.PeerGroupRuleId))
            {
                return new ProfileRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Axes = new List<ProfileRadarAxisDto>()
                };
            }

            var dimensionKpiId = PeerGroupResolver.ResolveDimensionKpiId(registration.PeerGroupRuleId);
            var population = _repository.GetActivePopulation(entityType, dimensionKpiId);
            var peerGroupIndex = PeerGroupResolver.BuildPeerGroupIndex(registration.PeerGroupRuleId, population);
            var resolution = PeerGroupResolver.ResolveForEntity(
                entityId,
                registration.PeerGroupRuleId,
                peerGroupIndex,
                population);

            if (!resolution.IsSufficient)
            {
                return new ProfileRadarSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.PeerGroupTooSmall,
                    PeerGroupRuleId = registration.PeerGroupRuleId,
                    PeerGroupSize = resolution.PeerGroupSize,
                    UnavailableExplanation = BuildPeerGroupTooSmallExplanation(
                        registration.PeerGroupRuleId,
                        resolution.PeerGroupSize),
                    Axes = new List<ProfileRadarAxisDto>()
                };
            }

            return new ProfileRadarSectionDto
            {
                IsAvailable = false,
                UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                PeerGroupRuleId = registration.PeerGroupRuleId,
                Axes = new List<ProfileRadarAxisDto>()
            };
        }

        internal static string BuildPeerGroupTooSmallExplanation(string peerGroupRuleId, int peerGroupSize)
        {
            var scope = string.Equals(peerGroupRuleId, PeerGroupResolver.CustomerWilayah, StringComparison.OrdinalIgnoreCase)
                ? "Wilayah peer group"
                : "Peer group";

            return $"{scope} has {peerGroupSize} eligible entities (minimum {EntityAnalyticsConstants.MinRadarPeerGroupSize}).";
        }

        private static string BuildPeriodLabel(int year, int month)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            var now = DateTime.Now;
            if (year == now.Year && month == now.Month)
                return $"{monthName} {year} (MTD)";

            return $"{monthName} {year}";
        }

        private Dictionary<string, Dictionary<string, decimal?>> BuildAxisValueMaps(
            string entityType,
            int periodYear,
            int periodMonth,
            IReadOnlyList<EntityKpiMetadata> radarAxes)
        {
            var maps = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.OrdinalIgnoreCase);
            var momPeriod = EntityComparisonCalculator.ShiftMonth(periodYear, periodMonth, -1);
            var attentionCounts = _repository.GetActiveAttentionCounts(entityType);

            foreach (var axis in radarAxes)
            {
                var valueSource = axis.RadarValueSource ?? RadarValueSource.L0Kpi;
                Dictionary<string, decimal?> valueMap;

                if (string.Equals(valueSource, RadarValueSource.L0Kpi, StringComparison.OrdinalIgnoreCase))
                {
                    var sourceKpiId = string.IsNullOrWhiteSpace(axis.RadarSourceKpiId)
                        ? axis.KpiId
                        : axis.RadarSourceKpiId;
                    valueMap = ToValueMap(_repository.GetCurrentKpiPopulation(entityType, sourceKpiId));
                }
                else if (string.Equals(valueSource, RadarValueSource.L0DimensionNumeric, StringComparison.OrdinalIgnoreCase))
                {
                    valueMap = ToValueMap(_repository.GetCurrentDimensionPopulation(entityType, axis.KpiId));
                }
                else if (string.Equals(valueSource, RadarValueSource.L1MomGrowthPercent, StringComparison.OrdinalIgnoreCase))
                {
                    var sourceKpiId = axis.RadarSourceKpiId;
                    if (string.IsNullOrWhiteSpace(sourceKpiId))
                        continue;

                    var currentRows = _repository.GetPeriodMetricsForPopulation(
                        entityType, periodYear, periodMonth, sourceKpiId);
                    var priorRows = _repository.GetPeriodMetricsForPopulation(
                        entityType, momPeriod.Year, momPeriod.Month, sourceKpiId);
                    var priorLookup = priorRows.ToDictionary(
                        r => r.EntityId,
                        r => r.NumericValue,
                        StringComparer.OrdinalIgnoreCase);

                    valueMap = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var row in currentRows.Where(r => r.IsActive))
                    {
                        priorLookup.TryGetValue(row.EntityId, out var priorValue);
                        valueMap[row.EntityId] = EntityComparisonCalculator.ComputeGrowthPercent(
                            row.NumericValue,
                            priorValue);
                    }
                }
                else if (string.Equals(valueSource, RadarValueSource.L3ActiveSignalCount, StringComparison.OrdinalIgnoreCase))
                {
                    valueMap = attentionCounts.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (decimal?)kvp.Value,
                        StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    continue;
                }

                maps[axis.KpiId] = valueMap;
            }

            return maps;
        }

        private static Dictionary<string, decimal?> ToValueMap(IReadOnlyList<EntityAnalyticsPeriodMetricRow> rows)
        {
            return (rows ?? Array.Empty<EntityAnalyticsPeriodMetricRow>())
                .Where(r => r.IsActive)
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().NumericValue,
                    StringComparer.OrdinalIgnoreCase);
        }

        private IReadOnlyList<EntityKpiMetadata> ResolveRadarEligibleAxes(string entityType)
        {
            var packId = _entityTypes.TryGet(entityType, out var registration)
                ? registration.KpiPackId
                : null;

            if (string.IsNullOrWhiteSpace(packId))
                return Array.Empty<EntityKpiMetadata>();

            return _kpiRegistry.ResolvePackMetadata(packId)
                .Where(m => m.RadarEligible
                    && !string.Equals(m.Direction, "Neutral", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.RadarAxisOrder)
                .ThenBy(m => m.KpiId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
