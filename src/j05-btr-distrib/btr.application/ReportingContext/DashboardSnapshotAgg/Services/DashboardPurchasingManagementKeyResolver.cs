using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardPurchasingManagementKeyResolver
    {
        public const string UnknownPrincipal = "Unknown";

        public static string ResolvePrincipalName(string supplierName)
        {
            return string.IsNullOrWhiteSpace(supplierName)
                ? UnknownPrincipal
                : supplierName.Trim();
        }

        public static string NormalizeKey(string name)
        {
            return ResolvePrincipalName(name).ToUpperInvariant();
        }

        public static bool NamesMatch(string left, string right)
        {
            return string.Equals(
                NormalizeKey(left),
                NormalizeKey(right),
                StringComparison.Ordinal);
        }
    }
}
