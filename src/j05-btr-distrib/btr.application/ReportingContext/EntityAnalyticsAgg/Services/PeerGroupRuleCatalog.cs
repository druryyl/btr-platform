using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class PeerGroupRuleCatalog
    {
        private static readonly IReadOnlyList<PeerGroupRuleDefinition> Rules = new[]
        {
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.CustomerWilayah,
                EntityType = EntityTypeCode.Customer,
                DisplayLabel = "Wilayah",
                DimensionLabel = "Wilayah",
                IsDefault = true
            },
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.CustomerKlasifikasi,
                EntityType = EntityTypeCode.Customer,
                DisplayLabel = "Klasifikasi",
                DimensionLabel = "Klasifikasi",
                IsDefault = false
            },
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.ItemPrincipal,
                EntityType = EntityTypeCode.Item,
                DisplayLabel = "Principal",
                DimensionLabel = "Principal",
                IsDefault = true
            },
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.ItemCategory,
                EntityType = EntityTypeCode.Item,
                DisplayLabel = "Category",
                DimensionLabel = "Category",
                IsDefault = false
            },
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.SalesmanAllActive,
                EntityType = EntityTypeCode.Salesman,
                DisplayLabel = "All active",
                DimensionLabel = null,
                IsDefault = true
            },
            new PeerGroupRuleDefinition
            {
                RuleId = PeerGroupResolver.SupplierAllActive,
                EntityType = EntityTypeCode.Supplier,
                DisplayLabel = "All active",
                DimensionLabel = null,
                IsDefault = true
            }
        };

        public static IReadOnlyList<PeerGroupRuleDefinition> GetRulesForEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<PeerGroupRuleDefinition>();

            return Rules
                .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static PeerGroupRuleDefinition TryResolveRule(string entityType, string ruleId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(ruleId))
                return null;

            return Rules.FirstOrDefault(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(r.RuleId, ruleId, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetDefaultRuleId(string entityType, IEntityTypeRegistry entityTypes)
        {
            if (entityTypes != null && entityTypes.TryGet(entityType, out var registration)
                && !string.IsNullOrWhiteSpace(registration.PeerGroupRuleId))
            {
                return registration.PeerGroupRuleId;
            }

            return GetRulesForEntityType(entityType)
                .FirstOrDefault(r => r.IsDefault)
                ?.RuleId;
        }

        public static string ResolveEffectiveRuleId(
            string entityType,
            string requestedRuleId,
            IEntityTypeRegistry entityTypes)
        {
            if (!string.IsNullOrWhiteSpace(requestedRuleId))
            {
                var resolved = TryResolveRule(entityType, requestedRuleId);
                if (resolved == null)
                    throw new ArgumentException(
                        $"Unknown peer group rule '{requestedRuleId}' for entity type '{entityType}'.",
                        nameof(requestedRuleId));

                return resolved.RuleId;
            }

            return GetDefaultRuleId(entityType, entityTypes);
        }
    }
}
