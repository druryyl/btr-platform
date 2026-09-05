using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class PeerGroupLabelFormatter
    {
        public static string Format(
            string entityType,
            string peerGroupRuleId,
            int peerGroupSize,
            string dimensionValue)
        {
            if (peerGroupSize <= 0)
                return "No peers available";

            var countLabel = peerGroupSize.ToString();
            var dimension = string.IsNullOrWhiteSpace(dimensionValue) ? "—" : dimensionValue.Trim();

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.CustomerWilayah, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} {Pluralize(entityType, peerGroupSize)} in Wilayah: {dimension}";

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.CustomerKlasifikasi, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} {Pluralize(entityType, peerGroupSize)} in Klasifikasi: {dimension}";

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.ItemPrincipal, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} {Pluralize(entityType, peerGroupSize)} in Principal: {dimension}";

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.ItemCategory, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} {Pluralize(entityType, peerGroupSize)} in Category: {dimension}";

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.SalesmanAllActive, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} active {Pluralize(entityType, peerGroupSize)}";

            if (string.Equals(peerGroupRuleId, PeerGroupResolver.SupplierAllActive, StringComparison.OrdinalIgnoreCase))
                return $"{countLabel} active {Pluralize(entityType, peerGroupSize)}";

            return $"{countLabel} {Pluralize(entityType, peerGroupSize)}";
        }

        private static string Pluralize(string entityType, int count)
        {
            var singular = count == 1;

            if (string.Equals(entityType, EntityTypeCode.Customer, StringComparison.OrdinalIgnoreCase))
                return singular ? "customer" : "customers";

            if (string.Equals(entityType, EntityTypeCode.Item, StringComparison.OrdinalIgnoreCase))
                return singular ? "item" : "items";

            if (string.Equals(entityType, EntityTypeCode.Salesman, StringComparison.OrdinalIgnoreCase))
                return singular ? "salesman" : "salesmen";

            if (string.Equals(entityType, EntityTypeCode.Supplier, StringComparison.OrdinalIgnoreCase))
                return singular ? "supplier" : "suppliers";

            return singular ? "peer" : "peers";
        }
    }
}
