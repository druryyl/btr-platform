using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityPopulationMapEngine : IEntityPopulationMapEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly EntityKpiEnvelopeFormatter _formatter;

        public EntityPopulationMapEngine(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityTypeRegistry entityTypes,
            EntityKpiEnvelopeFormatter formatter)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
            _formatter = formatter;
        }

        public PopulationMapResponseDto BuildPopulationMap(PopulationMapRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.", nameof(request));

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            var preset = string.IsNullOrWhiteSpace(request.PresetId)
                ? EntityMapPresetRegistry.ResolveDefaultPreset(request.EntityType)
                : EntityMapPresetRegistry.TryGetPreset(request.EntityType, request.PresetId);

            if (preset == null)
                throw new ArgumentException($"Unknown preset: {request.PresetId}");

            var axisXMeta = ResolveMetadata(preset.AxisXKpiId);
            var axisYMeta = ResolveMetadata(preset.AxisYKpiId);

            var dimensionKpiId = preset.FilterDimensionKpiId
                ?? PeerGroupResolver.ResolveDimensionKpiId(
                    _entityTypes.TryGet(request.EntityType, out var reg) ? reg.PeerGroupRuleId : null);

            var population = _repository.GetActivePopulation(request.EntityType, dimensionKpiId);
            var axisXValues = GetKpiValueMap(request.EntityType, preset.AxisXKpiId);
            var axisYValues = GetKpiValueMap(request.EntityType, preset.AxisYKpiId);
            var attentionCounts = _repository.GetActiveAttentionCounts(request.EntityType);
            var generatedAt = _repository.GetLatestGeneratedAtForEntityType(request.EntityType);

            var xPercentiles = BuildPercentileMap(axisXValues, axisXMeta?.Direction);
            var yPercentiles = BuildPercentileMap(axisYValues, axisYMeta?.Direction);

            var points = new List<PopulationMapPointDto>();

            foreach (var row in population)
            {
                var displayName = row.DisplayName ?? row.EntityCode ?? row.EntityId;

                axisXValues.TryGetValue(row.EntityId, out var axisX);
                axisYValues.TryGetValue(row.EntityId, out var axisY);
                attentionCounts.TryGetValue(row.EntityId, out var attentionCount);

                var matchesFilter = MatchesFilter(
                    row,
                    attentionCount,
                    request.DimensionFilter,
                    request.AttentionOnly);

                var point = new PopulationMapPointDto
                {
                    EntityId = row.EntityId,
                    EntityCode = row.EntityCode ?? row.EntityId,
                    DisplayName = displayName,
                    AxisX = axisX,
                    AxisY = axisY,
                    FormattedAxisX = FormatValue(axisX, axisXMeta),
                    FormattedAxisY = FormatValue(axisY, axisYMeta),
                    AxisXPercentile = xPercentiles.TryGetValue(row.EntityId, out var xp) ? xp : (decimal?)null,
                    AxisYPercentile = yPercentiles.TryGetValue(row.EntityId, out var yp) ? yp : (decimal?)null,
                    DimensionValue = row.DimensionValue,
                    IsActive = row.IsActive,
                    ActiveAttentionCount = attentionCount,
                    MatchesFilter = matchesFilter
                };

                points.Add(point);
            }

            var filteredCount = points.Count(p => p.MatchesFilter);

            return new PopulationMapResponseDto
            {
                EntityType = request.EntityType,
                PresetId = preset.PresetId,
                PresetDisplayName = preset.DisplayName,
                AxisXKpiId = preset.AxisXKpiId,
                AxisYKpiId = preset.AxisYKpiId,
                AxisXLabel = axisXMeta?.DisplayName ?? preset.AxisXKpiId,
                AxisYLabel = axisYMeta?.DisplayName ?? preset.AxisYKpiId,
                AxisXUnit = axisXMeta?.Unit,
                AxisYUnit = axisYMeta?.Unit,
                TotalPopulationCount = population.Count,
                FilteredPopulationCount = filteredCount,
                ActiveFilterDescription = BuildFilterDescription(request, filteredCount, population.Count),
                GeneratedAt = generatedAt,
                Points = points
            };
        }

        private Dictionary<string, decimal?> GetKpiValueMap(string entityType, string kpiId)
        {
            var rows = EntityAnalyticsMetaKpiIds.IsMetaOrDimension(kpiId)
                ? _repository.GetCurrentDimensionPopulation(entityType, kpiId)
                : _repository.GetCurrentKpiPopulation(entityType, kpiId);

            return ToValueMap(rows);
        }

        private static Dictionary<string, decimal?> ToValueMap(IReadOnlyList<EntityAnalyticsPeriodMetricRow> rows)
        {
            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.EntityId))
                .GroupBy(r => r.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().NumericValue,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, decimal> BuildPercentileMap(
            Dictionary<string, decimal?> values,
            string direction)
        {
            var candidates = values
                .Where(kv => kv.Value.HasValue)
                .Select(kv => (kv.Key, kv.Value.Value))
                .ToList();

            var rankings = EntityRankingCalculator.Calculate(candidates, direction ?? "HigherIsBetter");
            return rankings.ToDictionary(
                r => r.EntityId,
                r => r.Percentile,
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool MatchesFilter(
            EntityPopulationRow row,
            int attentionCount,
            string dimensionFilter,
            bool? attentionOnly)
        {
            if (!string.IsNullOrWhiteSpace(dimensionFilter)
                && !string.Equals(row.DimensionValue?.Trim(), dimensionFilter.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (attentionOnly == true && attentionCount <= 0)
                return false;

            return true;
        }

        private string FormatValue(decimal? value, EntityKpiMetadata metadata)
        {
            if (!value.HasValue || metadata == null)
                return value?.ToString(CultureInfo.InvariantCulture) ?? "—";

            return _formatter.FormatValue(value, null, metadata);
        }

        private EntityKpiMetadata ResolveMetadata(string kpiId)
        {
            return _kpiRegistry.TryGetMetadata(kpiId, out var metadata) ? metadata : null;
        }

        private static string BuildFilterDescription(
            PopulationMapRequest request,
            int filteredCount,
            int totalCount)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.DimensionFilter))
                parts.Add(request.DimensionFilter.Trim());

            if (request.AttentionOnly == true)
                parts.Add("Active attention signals");

            if (parts.Count == 0)
                return null;

            return $"Showing {filteredCount} of {totalCount} — {string.Join(", ", parts)}";
        }
    }
}
