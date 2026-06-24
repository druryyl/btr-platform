using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Queries;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.Shared;
using NLog;

namespace btr.application.ReportingContext.DashboardAlertCenterAgg.Services
{
    public class DashboardAlertCenterComposer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string SalesReportRoute = "/reports/sales";
        private const string PiutangReportRoute = "/reports/piutang";
        private const string InventoryReportRoute = "/reports/inventory";
        private const string PurchasingReportRoute = "/reports/purchasing";
        private const string EntityTypeCustomer = "Customer";
        private const string EntityTypeSalesman = "Salesman";
        private const string EntityTypeWilayah = "Wilayah";
        private const string EntityTypePrincipal = "Principal";
        private const string EntityTypeWarehouse = "Warehouse";
        private const string EntityTypeCompany = "Company";
        private const string AgingOver90BucketKey = "DaysOver90";

        public DashboardAlertCenterResponse Compose(AlertCenterComposeInput input)
        {
            var utcNow = input.UtcNow;
            var options = input.Options ?? new DashboardSnapshotOptions();
            var refreshStatuses = input.RefreshStatuses ?? new List<DashboardSnapshotRefreshStatusModel>();

            var hasUnavailable = DashboardSnapshotHealthHelper.HasAlertCenterUnavailableDomain(input);
            var isDataFresh = DashboardSnapshotHealthHelper.IsAlertCenterDataFresh(input, utcNow);
            var overallHealth = DashboardSnapshotHealthHelper.ResolveOverallHealth(refreshStatuses);

            var lastRefreshed = DashboardSnapshotHealthHelper.ResolveLastRefreshed(
                input.Sales?.GeneratedAt,
                input.Piutang?.GeneratedAt,
                input.Inventory?.GeneratedAt,
                input.Purchasing?.GeneratedAt,
                input.Customer?.GeneratedAt,
                input.Salesman?.GeneratedAt,
                input.Collection?.GeneratedAt,
                input.InventoryRisk?.GeneratedAt,
                input.PurchasingManagement?.GeneratedAt,
                input.Location?.GeneratedAt);

            var platformAlerts = BuildPlatformAlerts(hasUnavailable, isDataFresh, overallHealth);
            var salesAchievementAlerts = BuildSalesAchievementAlerts(input.Sales);
            var producerAlerts = CollectProducerAlerts(input);
            var dedupedAlerts = ApplyDeduplication(producerAlerts, input);
            var allAlerts = salesAchievementAlerts.Concat(dedupedAlerts).ToList();

            var alertGroups = BuildAlertGroups(allAlerts);
            var categorySummaries = BuildCategorySummaries(allAlerts, alertGroups);
            var inventorySummary = BuildInventoryRiskSummary(input.InventoryRisk);
            var concentrations = BuildConcentrations(input, dedupedAlerts);

            var isAvailable = input.Sales != null
                || input.Customer != null
                || input.Salesman != null
                || input.Collection != null
                || input.PurchasingManagement != null
                || input.Location != null
                || input.InventoryRisk != null;

            return new DashboardAlertCenterResponse
            {
                IsAvailable = isAvailable,
                IsDataFresh = isDataFresh,
                OverallHealthStatus = overallHealth,
                HasUnavailableDomain = hasUnavailable,
                LastRefreshed = lastRefreshed,
                PlatformAlerts = platformAlerts,
                CategorySummaries = categorySummaries,
                AlertGroups = alertGroups,
                InventoryRiskSummary = inventorySummary,
                Concentrations = concentrations,
                Navigation = BuildNavigation()
            };
        }

        private static IList<DashboardAlertCenterPlatformAlert> BuildPlatformAlerts(
            bool hasUnavailable,
            bool isDataFresh,
            string overallHealth)
        {
            var alerts = new List<DashboardAlertCenterPlatformAlert>();

            if (string.Equals(overallHealth, "degraded", StringComparison.OrdinalIgnoreCase))
            {
                alerts.Add(CreatePlatformAlert(AlertCenterRegistry.SignalSnapshotDegraded, "One or more snapshot domains failed to refresh"));
            }

            if (!isDataFresh)
            {
                alerts.Add(CreatePlatformAlert(AlertCenterRegistry.SignalSnapshotStale, "Dashboard data exceeds configured freshness interval"));
            }

            if (hasUnavailable)
            {
                alerts.Add(CreatePlatformAlert(AlertCenterRegistry.SignalDomainUnavailable, "One or more core domain snapshots are unavailable"));
            }

            return alerts
                .OrderBy(a => PlatformAlertOrder(a.SignalKey))
                .ToList();
        }

        private static int PlatformAlertOrder(string signalKey)
        {
            if (string.Equals(signalKey, AlertCenterRegistry.SignalSnapshotDegraded, StringComparison.OrdinalIgnoreCase))
                return 0;
            if (string.Equals(signalKey, AlertCenterRegistry.SignalSnapshotStale, StringComparison.OrdinalIgnoreCase))
                return 1;
            return 2;
        }

        private static DashboardAlertCenterPlatformAlert CreatePlatformAlert(string signalKey, string valueText)
        {
            AlertCenterRegistry.TryGet(signalKey, out var entry);
            return new DashboardAlertCenterPlatformAlert
            {
                SignalKey = signalKey,
                SignalLabel = entry?.DefaultLabel ?? signalKey,
                ValueText = valueText,
                DashboardRoute = entry?.DashboardRoute ?? "/dashboard"
            };
        }

        private static IList<DashboardAlertCenterAlertRow> BuildSalesAchievementAlerts(
            DashboardSalesAggregateResult sales)
        {
            if (sales == null)
                return new List<DashboardAlertCenterAlertRow>();

            var band = ExecutiveSalesAchievementBandResolver.Resolve(sales.AchievementPercent);
            if (band != ExecutiveSalesAchievementBandResolver.Warning
                && band != ExecutiveSalesAchievementBandResolver.Critical)
            {
                return new List<DashboardAlertCenterAlertRow>();
            }

            var signalKey = band == ExecutiveSalesAchievementBandResolver.Critical
                ? AlertCenterRegistry.SignalSalesAchievementCritical
                : AlertCenterRegistry.SignalSalesAchievementWarning;

            AlertCenterRegistry.TryGet(signalKey, out var entry);

            var percentText = sales.AchievementPercent.HasValue
                ? $"{sales.AchievementPercent.Value.ToString("0.#", CultureInfo.InvariantCulture)}%"
                : "N/A";

            return new List<DashboardAlertCenterAlertRow>
            {
                new DashboardAlertCenterAlertRow
                {
                    Category = AlertCenterRegistry.CategorySales,
                    EntityType = EntityTypeCompany,
                    EntityCode = "COMPANY",
                    EntityName = "Company",
                    SignalKey = signalKey,
                    SignalLabel = entry?.DefaultLabel ?? signalKey,
                    ValueAmount = sales.AchievementPercent,
                    ValueText = percentText,
                    AchievementBand = band,
                    DashboardRoute = entry?.DashboardRoute ?? "/dashboard/sales",
                    ReportRoute = SalesReportRoute,
                    EntityFilterQuery = null,
                    SortOrder = entry?.Priority ?? 0
                }
            };
        }

        private static List<ProducerAlertRow> CollectProducerAlerts(AlertCenterComposeInput input)
        {
            var rows = new List<ProducerAlertRow>();

            if (input.Customer?.AttentionList != null)
            {
                foreach (var row in input.Customer.AttentionList)
                {
                    rows.Add(new ProducerAlertRow
                    {
                        Source = "M17",
                        EntityType = EntityTypeCustomer,
                        EntityId = row.CustomerCode,
                        EntityCode = row.CustomerCode,
                        EntityName = row.CustomerName,
                        SignalKey = row.SignalKey,
                        SignalLabel = row.SignalLabel,
                        ValueAmount = row.ValueAmount,
                        ValueText = row.ValueText,
                        ReportRoute = ResolveCustomerReportRoute(row.SignalKey),
                        SortOrder = row.SortOrder
                    });
                }
            }

            if (input.Salesman?.AttentionList != null)
            {
                foreach (var row in input.Salesman.AttentionList)
                {
                    rows.Add(new ProducerAlertRow
                    {
                        Source = "M18",
                        EntityType = EntityTypeSalesman,
                        EntityId = row.SalesPersonId,
                        EntityCode = row.SalesPersonCode,
                        EntityName = row.SalesPersonName,
                        SignalKey = row.SignalKey,
                        SignalLabel = row.SignalLabel,
                        ValueAmount = row.ValueAmount,
                        ValueText = row.ValueText,
                        ReportRoute = ResolveSalesmanReportRoute(row.SignalKey),
                        SortOrder = row.SortOrder
                    });
                }
            }

            if (input.Collection?.AttentionList != null)
            {
                foreach (var row in input.Collection.AttentionList)
                {
                    rows.Add(new ProducerAlertRow
                    {
                        Source = "M20",
                        EntityType = row.EntityType,
                        EntityId = row.EntityId,
                        EntityCode = row.EntityCode,
                        EntityName = row.EntityName,
                        SignalKey = row.SignalKey,
                        SignalLabel = row.SignalLabel,
                        ValueAmount = row.ValueAmount,
                        ValueText = row.ValueText,
                        ReportRoute = row.ReportRoute,
                        SortOrder = row.SortOrder
                    });
                }
            }

            if (input.PurchasingManagement?.AttentionList != null)
            {
                foreach (var row in input.PurchasingManagement.AttentionList)
                {
                    rows.Add(new ProducerAlertRow
                    {
                        Source = "M21",
                        EntityType = row.EntityType ?? EntityTypePrincipal,
                        EntityId = row.EntityName,
                        EntityCode = null,
                        EntityName = row.EntityName,
                        SignalKey = row.SignalKey,
                        SignalLabel = row.SignalLabel,
                        ValueAmount = row.ValueAmount,
                        ValueText = row.ValueText,
                        ReportRoute = row.ReportRoute,
                        SortOrder = row.SortOrder
                    });
                }
            }

            if (input.Location?.AttentionList != null)
            {
                foreach (var row in input.Location.AttentionList)
                {
                    rows.Add(new ProducerAlertRow
                    {
                        Source = "M22",
                        EntityType = row.EntityType ?? EntityTypeWarehouse,
                        EntityId = row.EntityCode,
                        EntityCode = row.EntityCode,
                        EntityName = row.EntityName,
                        SignalKey = row.SignalKey,
                        SignalLabel = row.SignalLabel,
                        ValueAmount = row.ValueAmount,
                        ValueText = row.ValueText,
                        ReportRoute = row.ReportRoute,
                        SortOrder = row.SortOrder
                    });
                }
            }

            return rows;
        }

        private static List<DashboardAlertCenterAlertRow> ApplyDeduplication(
            List<ProducerAlertRow> producerRows,
            AlertCenterComposeInput input)
        {
            var collectionRows = producerRows
                .Where(r => string.Equals(r.Source, "M20", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var canonicalCustomers = collectionRows
                .Where(r => string.Equals(r.EntityType, EntityTypeCustomer, StringComparison.OrdinalIgnoreCase))
                .GroupBy(r => NormalizeCustomerKey(r.EntityId, r.EntityCode))
                .ToDictionary(g => g.Key, g => g.First());

            var workloadSalesmen = collectionRows
                .Where(r => string.Equals(r.EntityType, EntityTypeSalesman, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.SignalKey, DashboardCollectionAggregator.SignalHighOverdueWorkload, StringComparison.OrdinalIgnoreCase))
                .Select(r => NormalizeSalesmanKey(r.EntityId, r.EntityCode))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = new List<DashboardAlertCenterAlertRow>();

            foreach (var row in producerRows)
            {
                if (!AlertCenterRegistry.TryGetForProducer(row.Source, row.SignalKey, out var entry))
                {
                    WarnUnknownSignal(row);
                    continue;
                }

                if (entry.Section != AlertCenterSection.Alerts)
                    continue;

                if (string.Equals(row.Source, "M17", StringComparison.OrdinalIgnoreCase))
                {
                    var customerKey = NormalizeCustomerKey(row.EntityId, row.EntityCode);
                    if (canonicalCustomers.ContainsKey(customerKey))
                    {
                        if (string.Equals(row.SignalKey, DashboardCustomerAggregator.SignalOverdue, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (string.Equals(row.SignalKey, DashboardCustomerAggregator.SignalPlafondBreach, StringComparison.OrdinalIgnoreCase))
                        {
                            var m20Row = canonicalCustomers[customerKey];
                            if (string.Equals(m20Row.SignalKey, DashboardCollectionAggregator.SignalPlafondBreachOverdue, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        if (string.Equals(row.SignalKey, DashboardCustomerAggregator.SignalDormant, StringComparison.OrdinalIgnoreCase))
                        {
                            var m20Row = canonicalCustomers[customerKey];
                            if (string.Equals(m20Row.SignalKey, DashboardCollectionAggregator.SignalLegacyDebt, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }
                    }
                }

                if (string.Equals(row.Source, "M18", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.SignalKey, DashboardSalesmanAggregator.SignalHighOverdueExposure, StringComparison.OrdinalIgnoreCase))
                {
                    var salesmanKey = NormalizeSalesmanKey(row.EntityId, row.EntityCode);
                    if (workloadSalesmen.Contains(salesmanKey))
                        continue;
                }

                result.Add(MapToAlertRow(row, entry));
            }

            return result;
        }

        private static DashboardAlertCenterAlertRow MapToAlertRow(
            ProducerAlertRow row,
            AlertCenterRegistryEntry entry)
        {
            return new DashboardAlertCenterAlertRow
            {
                Category = entry.Category,
                EntityType = row.EntityType,
                EntityCode = row.EntityCode,
                EntityName = row.EntityName,
                SignalKey = row.SignalKey,
                SignalLabel = string.IsNullOrWhiteSpace(row.SignalLabel) ? entry.DefaultLabel : row.SignalLabel,
                ValueAmount = row.ValueAmount,
                ValueText = row.ValueText,
                AchievementBand = null,
                DashboardRoute = entry.DashboardRoute,
                ReportRoute = row.ReportRoute,
                EntityFilterQuery = ResolveEntityFilterQuery(row),
                SortOrder = row.SortOrder > 0 ? row.SortOrder : entry.Priority,
                Investigation = BuildAlertInvestigation(row, entry)
            };
        }

        private static InvestigationMetadata BuildAlertInvestigation(
            ProducerAlertRow row,
            AlertCenterRegistryEntry entry)
        {
            var entityId = row.EntityId ?? row.EntityCode;
            var reportRoute = string.Equals(row.EntityType, EntityTypeWilayah, StringComparison.OrdinalIgnoreCase)
                || string.Equals(row.EntityType, EntityTypeCompany, StringComparison.OrdinalIgnoreCase)
                ? null
                : row.ReportRoute;

            return InvestigationMetadataBuilder.Build(
                row.SignalKey,
                row.EntityType,
                entityId,
                row.EntityName,
                signalLabelOverride: string.IsNullOrWhiteSpace(row.SignalLabel) ? entry.DefaultLabel : row.SignalLabel,
                reportRouteOverride: reportRoute);
        }

        private static IList<DashboardAlertCenterCategoryGroup> BuildAlertGroups(
            IList<DashboardAlertCenterAlertRow> allAlerts)
        {
            var groups = new List<DashboardAlertCenterCategoryGroup>();

            foreach (var category in AlertCenterRegistry.CategoryDisplayOrder)
            {
                var categoryAlerts = allAlerts
                    .Where(a => string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(a => a.SortOrder)
                    .ThenByDescending(a => a.ValueAmount ?? decimal.MinValue)
                    .ThenBy(a => a.EntityName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Take(AlertCenterRegistry.AlertsPerCategoryCap)
                    .ToList();

                if (categoryAlerts.Count == 0)
                    continue;

                groups.Add(new DashboardAlertCenterCategoryGroup
                {
                    Category = category,
                    Alerts = categoryAlerts
                });
            }

            return groups;
        }

        private static IList<DashboardAlertCenterCategorySummary> BuildCategorySummaries(
            IList<DashboardAlertCenterAlertRow> allAlerts,
            IList<DashboardAlertCenterCategoryGroup> alertGroups)
        {
            var summaries = new List<DashboardAlertCenterCategorySummary>();

            foreach (var category in AlertCenterRegistry.CategoryDisplayOrder)
            {
                var totalCount = allAlerts.Count(a =>
                    string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase));

                if (totalCount == 0)
                    continue;

                var displayedCount = alertGroups
                    .FirstOrDefault(g => string.Equals(g.Category, category, StringComparison.OrdinalIgnoreCase))
                    ?.Alerts.Count ?? 0;

                summaries.Add(new DashboardAlertCenterCategorySummary
                {
                    Category = category,
                    TotalCount = totalCount,
                    DisplayedCount = displayedCount,
                    HasMore = totalCount > AlertCenterRegistry.AlertsPerCategoryCap
                });
            }

            return summaries;
        }

        private static DashboardAlertCenterInventoryRiskSummary BuildInventoryRiskSummary(
            DashboardInventoryRiskAggregateResult inventoryRisk)
        {
            if (inventoryRisk == null)
            {
                return new DashboardAlertCenterInventoryRiskSummary
                {
                    IsAvailable = false,
                    DashboardRoute = "/dashboard/inventory-risk"
                };
            }

            return new DashboardAlertCenterInventoryRiskSummary
            {
                IsAvailable = true,
                DeadStockItemCount = inventoryRisk.DeadStockItemCount,
                DeadStockValue = inventoryRisk.DeadStockValue,
                SlowMovingItemCount = inventoryRisk.SlowMovingItemCount,
                SlowMovingValue = inventoryRisk.SlowMovingValue,
                NeverSoldItemCount = inventoryRisk.NeverSoldItemCount,
                NeverSoldValue = inventoryRisk.NeverSoldValue,
                AtRiskInventoryPercent = inventoryRisk.AtRiskInventoryPercent,
                DashboardRoute = "/dashboard/inventory-risk"
            };
        }

        private static IList<DashboardAlertCenterConcentrationItem> BuildConcentrations(
            AlertCenterComposeInput input,
            List<DashboardAlertCenterAlertRow> dedupedAlerts)
        {
            var items = new List<DashboardAlertCenterConcentrationItem>();

            if (input.Collection?.RecoveryVsBillingPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalRecoveryVsBillingPercent,
                    input.Collection.RecoveryVsBillingPercent,
                    "M20",
                    1));
            }

            if (input.Collection?.OverdueConcentrationPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalOverdueConcentrationPercent,
                    input.Collection.OverdueConcentrationPercent,
                    "M20",
                    2));
            }

            if (input.Customer?.TopOmzetCustomerPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTopOmzetCustomerPercent,
                    input.Customer.TopOmzetCustomerPercent,
                    "M17",
                    3));
            }

            if (input.Customer?.TopPiutangCustomerPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTopPiutangCustomerPercent,
                    input.Customer.TopPiutangCustomerPercent,
                    "M17",
                    4));
            }

            if (input.InventoryRisk?.AtRiskInventoryPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalAtRiskInventoryPercent,
                    input.InventoryRisk.AtRiskInventoryPercent,
                    "M19",
                    5));
            }

            if (input.Location?.Top1WarehouseInventoryPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTop1WarehouseInventoryPercent,
                    input.Location.Top1WarehouseInventoryPercent,
                    "M22",
                    6));
            }

            if (input.Location?.Top1WarehouseAtRiskPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTop1WarehouseAtRiskPercent,
                    input.Location.Top1WarehouseAtRiskPercent,
                    "M22",
                    7));
            }

            if (input.Location?.Top1WarehouseSalesPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTop1WarehouseSalesPercent,
                    input.Location.Top1WarehouseSalesPercent,
                    "M22",
                    8));
            }

            if (input.Location?.Top1WilayahSalesPercent.HasValue == true)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTop1WilayahSalesPercent,
                    input.Location.Top1WilayahSalesPercent,
                    "M22",
                    9));
            }

            var topCustomerPercent = ResolvePiutangTopCustomerPercent(input.Piutang);
            if (topCustomerPercent.HasValue)
            {
                items.Add(CreateKpiConcentration(
                    AlertCenterRegistry.SignalTopCustomerPiutangPercent,
                    topCustomerPercent,
                    "M16",
                    3));
            }

            var producerRows = CollectProducerAlerts(input);
            foreach (var row in producerRows)
            {
                if (!AlertCenterRegistry.TryGetForProducer(row.Source, row.SignalKey, out var entry))
                {
                    WarnUnknownSignal(row);
                    continue;
                }

                if (entry.Section != AlertCenterSection.Concentrations)
                    continue;

                items.Add(new DashboardAlertCenterConcentrationItem
                {
                    Label = $"{row.EntityName} · {entry.DefaultLabel}",
                    ValueText = row.ValueText,
                    ValuePercent = row.ValueAmount,
                    DashboardRoute = entry.DashboardRoute,
                    SourceDomain = row.Source,
                    SortOrder = entry.Priority
                });
            }

            return items
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static DashboardAlertCenterConcentrationItem CreateKpiConcentration(
            string signalKey,
            decimal? percent,
            string sourceDomain,
            int sortOrder)
        {
            AlertCenterRegistry.TryGet(signalKey, out var entry);
            var label = entry?.DefaultLabel ?? signalKey;
            var valueText = percent.HasValue
                ? $"{percent.Value.ToString("0.#", CultureInfo.InvariantCulture)}%"
                : null;

            return new DashboardAlertCenterConcentrationItem
            {
                Label = label,
                ValueText = valueText,
                ValuePercent = percent,
                DashboardRoute = entry?.DashboardRoute ?? "/dashboard",
                SourceDomain = sourceDomain,
                SortOrder = sortOrder
            };
        }

        private static decimal? ResolvePiutangTopCustomerPercent(DashboardPiutangAggregateResult piutang)
        {
            if (piutang == null || piutang.TotalPiutang <= 0)
                return null;

            var topCustomer = piutang.TopCustomerRisk?
                .OrderBy(c => c.Rank)
                .FirstOrDefault();

            if (topCustomer == null)
                return null;

            return topCustomer.TotalPiutang / piutang.TotalPiutang * 100m;
        }

        private static DashboardAlertCenterNavigationLinks BuildNavigation()
        {
            return new DashboardAlertCenterNavigationLinks
            {
                ExecutiveDashboardRoute = "/dashboard",
                SalesDashboardRoute = "/dashboard/sales",
                PiutangDashboardRoute = "/dashboard/piutang",
                CustomerDashboardRoute = "/dashboard/customers",
                SalesmanDashboardRoute = "/dashboard/salesmen",
                CollectionDashboardRoute = "/dashboard/collection",
                InventoryDashboardRoute = "/dashboard/inventory",
                InventoryRiskDashboardRoute = "/dashboard/inventory-risk",
                PurchasingDashboardRoute = "/dashboard/purchasing",
                LocationDashboardRoute = "/dashboard/locations",
                DomainDashboards = PortalMenuRegistry.GetDomainDashboardLinks().ToList()
            };
        }

        private static string ResolveCustomerReportRoute(string signalKey)
        {
            switch (signalKey)
            {
                case DashboardCustomerAggregator.SignalOverdue:
                case DashboardCustomerAggregator.SignalPlafondBreach:
                    return PiutangReportRoute;
                default:
                    return SalesReportRoute;
            }
        }

        private static string ResolveSalesmanReportRoute(string signalKey)
        {
            switch (signalKey)
            {
                case DashboardSalesmanAggregator.SignalHighOverdueExposure:
                case DashboardSalesmanAggregator.SignalHighPiutangExposure:
                    return PiutangReportRoute;
                default:
                    return SalesReportRoute;
            }
        }

        private static string ResolveEntityFilterQuery(ProducerAlertRow row)
        {
            if (string.Equals(row.EntityType, EntityTypeWilayah, StringComparison.OrdinalIgnoreCase))
                return row.EntityName;

            if (string.Equals(row.EntityType, EntityTypeCompany, StringComparison.OrdinalIgnoreCase))
                return null;

            return row.EntityName;
        }

        private static string NormalizeCustomerKey(string entityId, string entityCode)
        {
            var id = (entityId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(id))
                return id;

            return (entityCode ?? string.Empty).Trim();
        }

        private static string NormalizeSalesmanKey(string entityId, string entityCode)
        {
            var id = (entityId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(id))
                return id;

            return (entityCode ?? string.Empty).Trim();
        }

        private static void WarnUnknownSignal(ProducerAlertRow row)
        {
            Logger.Warn(
                "Alert Center skipped unknown signal. Source={Source} SignalKey={SignalKey} EntityType={EntityType} EntityName={EntityName}",
                row.Source,
                row.SignalKey,
                row.EntityType,
                row.EntityName);
        }

        private sealed class ProducerAlertRow
        {
            public string Source { get; set; }

            public string EntityType { get; set; }

            public string EntityId { get; set; }

            public string EntityCode { get; set; }

            public string EntityName { get; set; }

            public string SignalKey { get; set; }

            public string SignalLabel { get; set; }

            public decimal? ValueAmount { get; set; }

            public string ValueText { get; set; }

            public string ReportRoute { get; set; }

            public int SortOrder { get; set; }
        }
    }

    public class AlertCenterComposeInput
    {
        public DashboardSalesAggregateResult Sales { get; set; }

        public DashboardPiutangAggregateResult Piutang { get; set; }

        public DashboardInventoryAggregateResult Inventory { get; set; }

        public DashboardPurchasingAggregateResult Purchasing { get; set; }

        public DashboardPurchasingManagementAggregateResult PurchasingManagement { get; set; }

        public DashboardCustomerAggregateResult Customer { get; set; }

        public DashboardSalesmanAggregateResult Salesman { get; set; }

        public DashboardCollectionAggregateResult Collection { get; set; }

        public DashboardInventoryRiskAggregateResult InventoryRisk { get; set; }

        public DashboardLocationAggregateResult Location { get; set; }

        public IReadOnlyList<DashboardSnapshotRefreshStatusModel> RefreshStatuses { get; set; }

        public DashboardSnapshotOptions Options { get; set; }

        public DateTime UtcNow { get; set; }
    }
}
