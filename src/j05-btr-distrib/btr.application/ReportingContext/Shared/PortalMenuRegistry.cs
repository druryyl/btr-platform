using System;
using System.Collections.Generic;
using System.Linq;

namespace btr.application.ReportingContext.Shared
{
    public sealed class PortalMenuLinkModel
    {
        public PortalMenuLinkModel(string code, string label, string route)
        {
            Code = code;
            Label = label;
            Route = route;
        }

        public string Code { get; }

        public string Label { get; }

        public string Route { get; }
    }

    internal sealed class PortalMenuEntry
    {
        public PortalMenuEntry(
            string code,
            string label,
            string route,
            int groupOrder,
            int itemOrder,
            bool isDashboard = true)
        {
            Code = code;
            Label = label;
            Route = route;
            GroupOrder = groupOrder;
            ItemOrder = itemOrder;
            IsDashboard = isDashboard;
        }

        public string Code { get; }

        public string Label { get; }

        public string Route { get; }

        public int GroupOrder { get; }

        public int ItemOrder { get; }

        public bool IsDashboard { get; }
    }

    public static class PortalMenuRegistry
    {
        public const string AlertCenterCode = "EX02";

        private static readonly IReadOnlyList<PortalMenuEntry> Entries = BuildEntries();

        public static IReadOnlyList<PortalMenuLinkModel> GetAllLinks() =>
            Entries
                .OrderBy(e => e.GroupOrder)
                .ThenBy(e => e.ItemOrder)
                .Select(ToLink)
                .ToList();

        public static IReadOnlyList<PortalMenuLinkModel> GetDomainDashboardLinks() =>
            Entries
                .Where(e => e.IsDashboard && !string.Equals(e.Code, AlertCenterCode, StringComparison.Ordinal))
                .OrderBy(e => e.GroupOrder)
                .ThenBy(e => e.ItemOrder)
                .Select(ToLink)
                .ToList();

        public static string FormatMenuLabel(string route, string fallbackLabel = null)
        {
            if (TryGetByRoute(route, out var entry))
                return $"{entry.Code} · {entry.Label}";

            return fallbackLabel ?? route;
        }

        public static bool TryGetByRoute(string route, out PortalMenuLinkModel link)
        {
            link = null;
            if (!TryGetEntryByRoute(route, out var entry))
                return false;

            link = ToLink(entry);
            return true;
        }

        private static bool TryGetEntryByRoute(string route, out PortalMenuEntry entry)
        {
            entry = Entries.FirstOrDefault(e =>
                string.Equals(NormalizeRoute(e.Route), NormalizeRoute(route), StringComparison.OrdinalIgnoreCase));
            return entry != null;
        }

        private static PortalMenuLinkModel ToLink(PortalMenuEntry entry) =>
            new PortalMenuLinkModel(entry.Code, entry.Label, entry.Route);

        private static string NormalizeRoute(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                return route;

            return route.EndsWith("/", StringComparison.Ordinal) && route.Length > 1
                ? route.TrimEnd('/')
                : route;
        }

        private static IReadOnlyList<PortalMenuEntry> BuildEntries()
        {
            return new List<PortalMenuEntry>
            {
                Entry("EX01", "Executive", "/dashboard", 1, 1),
                Entry("EX02", "Alert Center", "/alerts", 1, 2),
                Entry("SA01", "Sales", "/dashboard/sales", 2, 1),
                Entry("SA02", "Sales Forecast", "/dashboard/sales-forecast", 2, 2),
                Entry("SA03", "Sales Report", "/reports/sales", 2, 3, isDashboard: false),
                Entry("CU01", "Customers", "/dashboard/customers", 3, 1),
                Entry("CU02", "Customer Risk Forecast", "/dashboard/customer-risk-forecast", 3, 2),
                Entry("CU03", "Collection Optimization", "/dashboard/collection-optimization", 3, 3),
                Entry("CU04", "Customer Portfolio", "/dashboard/customer-portfolio", 3, 4),
                Entry("CU05", "Customer Report", "/reports/customers", 3, 5, isDashboard: false),
                Entry("FI01", "Piutang", "/dashboard/piutang", 4, 1),
                Entry("FI02", "Collection", "/dashboard/collection", 4, 2),
                Entry("FI03", "Cash Flow Forecast", "/dashboard/cash-flow-forecast", 4, 3),
                Entry("FI04", "Piutang Report", "/reports/piutang", 4, 4, isDashboard: false),
                Entry("SF01", "Salesmen", "/dashboard/salesmen", 5, 1),
                Entry("SF02", "Sales Force Overview", "/dashboard/field-activity", 5, 2),
                Entry("SF03", "Salesman Field Activity", "/dashboard/field-activity/detail", 5, 3),
                Entry("IN01", "Inventory", "/dashboard/inventory", 6, 1),
                Entry("IN02", "Inventory Risk", "/dashboard/inventory-risk", 6, 2),
                Entry("IN03", "Inventory Forecast", "/dashboard/inventory-forecast", 6, 3),
                Entry("IN04", "Inventory Optimization", "/dashboard/inventory-optimization", 6, 4),
                Entry("IN05", "Inventory Report", "/reports/inventory", 6, 5, isDashboard: false),
                Entry("PU01", "Purchasing", "/dashboard/purchasing", 7, 1),
                Entry("PU02", "Purchasing Report", "/reports/purchasing", 7, 2, isDashboard: false),
                Entry("OP01", "Locations", "/dashboard/locations", 8, 1),
            };
        }

        private static PortalMenuEntry Entry(
            string code,
            string label,
            string route,
            int groupOrder,
            int itemOrder,
            bool isDashboard = true) =>
            new PortalMenuEntry(code, label, route, groupOrder, itemOrder, isDashboard);
    }
}
