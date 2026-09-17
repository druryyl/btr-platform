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

            var distributionSummary = BuildDistributionSummaryForBins(
                request.EntityType,
                peerValues.Count,
                bins,
                value => _formatter.FormatValue(value, null, metadata));

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
                Bins = bins,
                DistributionSummary = distributionSummary
            };
        }

        private static string BuildDistributionSummaryForBins(
            string entityType,
            int peerCount,
            IReadOnlyList<PeerDistributionBinDto> bins,
            Func<decimal, string> formatValue)
        {
            if (bins == null || bins.Count == 0)
                return null;

            var hasUnderflow = bins[0].IsOverflow;
            var hasOverflow = bins[bins.Count - 1].IsOverflow && bins.Count > 1;
            if (!hasUnderflow && !hasOverflow)
                return null;

            var underflowCount = hasUnderflow ? bins[0].Count : 0;
            var overflowCount = hasOverflow ? bins[bins.Count - 1].Count : 0;
            var floorText = hasUnderflow ? formatValue(bins[0].BinEnd) : null;
            var capText = hasOverflow ? formatValue(bins[bins.Count - 1].BinStart) : null;

            return BuildDistributionSummary(
                entityType,
                peerCount,
                underflowCount,
                overflowCount,
                floorText,
                capText);
        }

        /// <summary>
        /// Executive one-liner describing a fenced distribution (pure; unit-testable).
        /// Returns null when no overflow/underflow bucket exists.
        /// </summary>
        public static string BuildDistributionSummary(
            string entityType,
            int peerCount,
            int underflowCount,
            int overflowCount,
            string floorText,
            string capText)
        {
            if (underflowCount <= 0 && overflowCount <= 0)
                return null;

            var bulkCount = peerCount - underflowCount - overflowCount;
            var bulkNoun = PeerGroupLabelFormatter.Pluralize(entityType, bulkCount);
            var sentences = new List<string>();

            if (overflowCount > 0 && underflowCount > 0)
                sentences.Add($"{bulkCount} of {peerCount} {bulkNoun} between {floorText} and {capText}.");
            else if (overflowCount > 0)
                sentences.Add($"{bulkCount} of {peerCount} {bulkNoun} below {capText}.");
            else
                sentences.Add($"{bulkCount} of {peerCount} {bulkNoun} above {floorText}.");

            if (overflowCount > 0)
            {
                var noun = PeerGroupLabelFormatter.Pluralize(entityType, overflowCount);
                var verb = overflowCount == 1 ? "exceeds" : "exceed";
                sentences.Add($"{overflowCount} {noun} {verb} {capText}.");
            }

            if (underflowCount > 0)
            {
                var noun = PeerGroupLabelFormatter.Pluralize(entityType, underflowCount);
                sentences.Add($"{underflowCount} {noun} below {floorText}.");
            }

            return string.Join(" ", sentences);
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
