using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityPeerDistributionEngine : IEntityPeerDistributionEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly EntityKpiEnvelopeFormatter _formatter;

        public EntityPeerDistributionEngine(
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

        public PeerDistributionResponseDto BuildDistribution(PeerDistributionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.EntityId))
                throw new ArgumentException("EntityType and EntityId are required.");

            if (string.IsNullOrWhiteSpace(request.KpiId))
                throw new ArgumentException("KpiId is required.");

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            if (!_kpiRegistry.TryGetMetadata(request.KpiId, out var metadata))
                throw new ArgumentException($"Unknown KPI: {request.KpiId}");

            var peerGroupRuleId = PeerGroupRuleCatalog.ResolveEffectiveRuleId(
                request.EntityType,
                request.PeerGroupRuleId,
                _entityTypes);

            var dimensionKpiId = PeerGroupResolver.ResolveDimensionKpiId(peerGroupRuleId);
            var population = _repository.GetActivePopulation(request.EntityType, dimensionKpiId);
            var valueRows = EntityAnalyticsMetaKpiIds.IsMetaOrDimension(request.KpiId)
                ? _repository.GetCurrentDimensionPopulation(request.EntityType, request.KpiId)
                : _repository.GetCurrentKpiPopulation(request.EntityType, request.KpiId);
            var valueMap = valueRows
                .Where(r => r.NumericValue.HasValue)
                .ToDictionary(r => r.EntityId, r => r.NumericValue.Value, StringComparer.OrdinalIgnoreCase);

            var peerGroupIndex = PeerGroupResolver.BuildPeerGroupIndex(peerGroupRuleId, population);
            var peerResolution = PeerGroupResolver.ResolveForEntity(
                request.EntityId,
                peerGroupRuleId,
                peerGroupIndex,
                population);

            var peerIds = new HashSet<string>(peerResolution.PeerEntityIds, StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(request.DimensionFilter))
            {
                var filter = request.DimensionFilter.Trim();
                peerIds = population
                    .Where(p => peerIds.Contains(p.EntityId)
                        && string.Equals(p.DimensionValue?.Trim(), filter, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.EntityId)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var peerValues = peerIds
                .Where(id => valueMap.ContainsKey(id))
                .Select(id => valueMap[id])
                .OrderBy(v => v)
                .ToList();

            decimal? selectedValue = null;
            if (valueMap.TryGetValue(request.EntityId, out var selectedRaw))
                selectedValue = selectedRaw;
            var rankings = EntityRankingCalculator.Calculate(
                peerIds.Where(id => valueMap.ContainsKey(id)).Select(id => (id, valueMap[id])),
                metadata.Direction);

            var selectedRanking = rankings.FirstOrDefault(r =>
                string.Equals(r.EntityId, request.EntityId, StringComparison.OrdinalIgnoreCase));

            var bins = EntityPeerDistributionBinBuilder.BuildBins(
                peerValues,
                EntityPeerDistributionBinBuilder.DefaultBinCount,
                metadata.Unit);

            return new PeerDistributionResponseDto
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                KpiId = request.KpiId,
                KpiDisplayName = metadata.DisplayName,
                Unit = metadata.Unit,
                PeerGroupSize = peerValues.Count,
                PeerGroupRuleId = peerGroupRuleId,
                PeerGroupDimensionValue = peerResolution.DimensionValue,
                FormattedPeerGroupLabel = PeerGroupLabelFormatter.Format(
                    request.EntityType,
                    peerGroupRuleId,
                    peerValues.Count,
                    peerResolution.DimensionValue),
                SelectedValue = selectedValue,
                FormattedSelectedValue = _formatter.FormatValue(selectedValue, null, metadata),
                SelectedPercentile = selectedRanking?.Percentile,
                PeerMin = peerValues.Count > 0 ? peerValues.First() : (decimal?)null,
                PeerMax = peerValues.Count > 0 ? peerValues.Last() : (decimal?)null,
                FormattedPeerRange = FormatPeerRange(peerValues, metadata),
                Bins = bins
            };
        }

        private string FormatPeerRange(IReadOnlyList<decimal> peerValues, EntityKpiMetadata metadata)
        {
            if (peerValues == null || peerValues.Count == 0)
                return "—";

            var min = _formatter.FormatValue(peerValues.First(), null, metadata);
            var max = _formatter.FormatValue(peerValues.Last(), null, metadata);
            return $"{min} – {max}";
        }
    }
}
