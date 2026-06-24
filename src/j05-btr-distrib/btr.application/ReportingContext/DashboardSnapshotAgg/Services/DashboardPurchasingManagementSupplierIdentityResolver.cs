using System;
using System.Collections.Generic;
using System.Linq;
using btr.domain.PurchaseContext.SupplierAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public sealed class SupplierIdentity
    {
        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public string PrincipalName { get; set; }
    }

    public static class DashboardPurchasingManagementSupplierIdentityResolver
    {
        public static IReadOnlyDictionary<string, SupplierIdentity> BuildLookup(
            IEnumerable<SupplierModel> suppliers)
        {
            var lookup = new Dictionary<string, SupplierIdentity>(StringComparer.Ordinal);
            foreach (var supplier in suppliers ?? Enumerable.Empty<SupplierModel>())
            {
                if (supplier == null || string.IsNullOrWhiteSpace(supplier.SupplierName))
                    continue;

                var principalName = DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(supplier.SupplierName);
                var key = DashboardPurchasingManagementKeyResolver.NormalizeKey(principalName);
                if (lookup.ContainsKey(key))
                    continue;

                var supplierId = string.IsNullOrWhiteSpace(supplier.SupplierId)
                    ? principalName
                    : supplier.SupplierId.Trim();
                var supplierCode = string.IsNullOrWhiteSpace(supplier.SupplierCode)
                    ? supplierId
                    : supplier.SupplierCode.Trim();

                lookup[key] = new SupplierIdentity
                {
                    SupplierId = supplierId,
                    SupplierCode = supplierCode,
                    SupplierName = supplier.SupplierName.Trim(),
                    PrincipalName = principalName
                };
            }

            return lookup;
        }

        public static SupplierIdentity Resolve(
            string principalName,
            IReadOnlyDictionary<string, SupplierIdentity> lookup)
        {
            var resolvedName = DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(principalName);
            var key = DashboardPurchasingManagementKeyResolver.NormalizeKey(resolvedName);
            if (lookup != null && lookup.TryGetValue(key, out var identity))
            {
                return new SupplierIdentity
                {
                    SupplierId = identity.SupplierId,
                    SupplierCode = identity.SupplierCode,
                    SupplierName = identity.SupplierName,
                    PrincipalName = resolvedName
                };
            }

            return new SupplierIdentity
            {
                SupplierId = resolvedName,
                SupplierCode = resolvedName,
                SupplierName = resolvedName,
                PrincipalName = resolvedName
            };
        }
    }
}
