using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class PeerGroupResolver
    {
        public const string CustomerWilayah = "customer-wilayah";
        public const string SalesmanAllActive = "salesman-all-active";
        public const string ItemCategory = "item-category";
        public const string SupplierAllActive = "supplier-all-active";

        public static string ResolveDimensionKpiId(string peerGroupRuleId)
        {
            if (string.Equals(peerGroupRuleId, CustomerWilayah, StringComparison.OrdinalIgnoreCase))
                return EntityAnalyticsMetaKpiIds.Wilayah;

            if (string.Equals(peerGroupRuleId, ItemCategory, StringComparison.OrdinalIgnoreCase))
                return EntityAnalyticsMetaKpiIds.DimPrefix + "CATEGORY";

            return null;
        }

        public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildPeerGroupIndex(
            string peerGroupRuleId,
            IReadOnlyList<EntityPopulationRow> population)
        {
            var activePopulation = (population ?? Array.Empty<EntityPopulationRow>())
                .Where(p => p != null && p.IsActive && !string.IsNullOrWhiteSpace(p.EntityId))
                .ToList();

            if (activePopulation.Count == 0)
                return new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

            if (string.Equals(peerGroupRuleId, CustomerWilayah, StringComparison.OrdinalIgnoreCase)
                || string.Equals(peerGroupRuleId, ItemCategory, StringComparison.OrdinalIgnoreCase))
            {
                return BuildDimensionGroupedIndex(activePopulation);
            }

            if (string.Equals(peerGroupRuleId, SalesmanAllActive, StringComparison.OrdinalIgnoreCase)
                || string.Equals(peerGroupRuleId, SupplierAllActive, StringComparison.OrdinalIgnoreCase))
            {
                var allIds = activePopulation
                    .Select(p => p.EntityId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return activePopulation.ToDictionary(
                    p => p.EntityId,
                    _ => (IReadOnlyList<string>)allIds,
                    StringComparer.OrdinalIgnoreCase);
            }

            return new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public static PeerGroupResolution ResolveForEntity(
            string entityId,
            string peerGroupRuleId,
            IReadOnlyDictionary<string, IReadOnlyList<string>> peerGroupIndex,
            IReadOnlyList<EntityPopulationRow> population)
        {
            var resolution = new PeerGroupResolution
            {
                EntityId = entityId,
                PeerGroupRuleId = peerGroupRuleId
            };

            if (string.IsNullOrWhiteSpace(entityId)
                || peerGroupIndex == null
                || !peerGroupIndex.TryGetValue(entityId, out var peerIds))
            {
                resolution.PeerEntityIds = Array.Empty<string>();
                resolution.PeerGroupSize = 0;
                resolution.IsSufficient = false;
                return resolution;
            }

            var distinctPeers = peerIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            resolution.PeerEntityIds = distinctPeers;
            resolution.PeerGroupSize = distinctPeers.Count;
            resolution.IsSufficient = distinctPeers.Count >= EntityAnalyticsConstants.MinRadarPeerGroupSize;
            resolution.DimensionValue = population?
                .FirstOrDefault(p => string.Equals(p.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                ?.DimensionValue;

            return resolution;
        }

        private static Dictionary<string, IReadOnlyList<string>> BuildDimensionGroupedIndex(
            IReadOnlyList<EntityPopulationRow> activePopulation)
        {
            var groups = activePopulation
                .Where(p => !string.IsNullOrWhiteSpace(p.DimensionValue))
                .GroupBy(
                    p => p.DimensionValue.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(p => p.EntityId).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var index = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in activePopulation)
            {
                if (string.IsNullOrWhiteSpace(row.DimensionValue))
                    continue;

                if (groups.TryGetValue(row.DimensionValue.Trim(), out var peers))
                    index[row.EntityId] = peers;
            }

            return index;
        }
    }
}
