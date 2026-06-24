using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.domain.PurchaseContext.SupplierAgg;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardPurchasingManagementAggregator
    {
        public const int TopPrincipalCount = 10;
        public const int PurchasingInactivityDayThreshold = 15;

        public const string SignalQualifiedBacklog = "QualifiedBacklog";
        public const string SignalPrincipalSpendConcentration = "PrincipalSpendConcentration";
        public const string SignalPrincipalInventoryConcentration = "PrincipalInventoryConcentration";
        public const string SignalPrincipalAtRiskExposure = "PrincipalAtRiskExposure";
        public const string SignalCompoundDependency = "CompoundDependency";
        public const string SignalPurchasingInactivity = "PurchasingInactivity";
        public const string SignalPrincipalInventoryNoPurchase = "PrincipalInventoryNoPurchase";
        public const string SignalUnknownPrincipal = "UnknownPrincipal";

        public const string EntityTypePrincipal = "Principal";
        public const string EntityTypeCompany = "Company";

        private const string PurchasingReportRoute = "/reports/purchasing";
        private const string PostingStatusBelumKey = "BELUM";
        private const string PostingStatusSudahKey = "SUDAH";

        private static readonly Dictionary<string, int> SignalPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { SignalQualifiedBacklog, 1 },
            { SignalCompoundDependency, 2 },
            { SignalPrincipalInventoryNoPurchase, 3 },
            { SignalUnknownPrincipal, 4 },
            { SignalPrincipalAtRiskExposure, 5 },
            { SignalPrincipalSpendConcentration, 6 },
            { SignalPrincipalInventoryConcentration, 7 },
            { SignalPurchasingInactivity, 8 },
        };

        private static readonly Dictionary<string, string> SignalLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { SignalQualifiedBacklog, "Qualified Backlog" },
            { SignalPrincipalSpendConcentration, "Spend Concentration" },
            { SignalPrincipalInventoryConcentration, "Inventory Concentration" },
            { SignalPrincipalAtRiskExposure, "At-Risk Exposure" },
            { SignalCompoundDependency, "Compound Dependency" },
            { SignalPurchasingInactivity, "Purchasing Inactivity" },
            { SignalPrincipalInventoryNoPurchase, "Inventory, No Purchase" },
            { SignalUnknownPrincipal, "Unknown Principal" },
        };

        public DashboardPurchasingManagementAggregateResult Aggregate(
            IEnumerable<InvoiceView> invoiceRows,
            DashboardPurchasingAggregateResult purchasingSnapshot,
            DashboardInventoryAggregateResult inventorySnapshot,
            DashboardInventoryRiskAggregateResult inventoryRiskSnapshot,
            Periode periode,
            DateTime today,
            DateTime generatedAt,
            int qualifiedBacklogDays,
            IEnumerable<SupplierModel> suppliers = null,
            IEnumerable<SupplierMtdItemRollupDto> salesRollups = null,
            IEnumerable<SupplierCatalogCountDto> catalogCounts = null)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var invoices = (invoiceRows ?? Enumerable.Empty<InvoiceView>()).ToList();
            var periodStart = periode.Tgl1.Date;
            var supplierLookup = DashboardPurchasingManagementSupplierIdentityResolver.BuildLookup(suppliers);
            var salesStatsBySupplierId = BuildSalesStatsBySupplierId(salesRollups);
            var catalogCountsBySupplierId = BuildCatalogCountsBySupplierId(catalogCounts);
            var grandTotalPurchase = purchasingSnapshot?.GrandTotalPurchase
                ?? invoices.Sum(r => r.GrandTotal);
            var totalInvoice = purchasingSnapshot?.TotalInvoice ?? invoices.Count;

            var qualifiedInvoices = invoices
                .Where(r => IsBelum(r.PostingStok))
                .Where(r => (today - r.LastUpdate.Date).TotalDays >= qualifiedBacklogDays)
                .ToList();

            var qualifiedBacklogCount = qualifiedInvoices.Count;
            var qualifiedBacklogValue = qualifiedInvoices.Sum(r => r.GrandTotal);

            var pendingPostingValue = purchasingSnapshot?.PostingStatus?
                .FirstOrDefault(p => string.Equals(p.StatusKey, PostingStatusBelumKey, StringComparison.OrdinalIgnoreCase))
                ?.PurchaseAmount
                ?? invoices.Where(r => IsBelum(r.PostingStok)).Sum(r => r.GrandTotal);

            var postedPercent = ComputePostedPercent(purchasingSnapshot?.PostingStatus, invoices);

            var purchaseByPrincipal = invoices
                .GroupBy(r => DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(r.SupplierName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.GrandTotal),
                    StringComparer.OrdinalIgnoreCase);

            var invoiceCountByPrincipal = invoices
                .GroupBy(r => DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(r.SupplierName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count(),
                    StringComparer.OrdinalIgnoreCase);

            var postedPercentByPrincipal = invoices
                .GroupBy(r => DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(r.SupplierName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => ComputePostedPercentForInvoices(g.ToList()),
                    StringComparer.OrdinalIgnoreCase);

            var topPurchasePrincipals = purchaseByPrincipal
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Take(TopPrincipalCount)
                .Select((x, index) => new
                {
                    Rank = index + 1,
                    PrincipalName = x.Key,
                    MtdPurchaseAmount = x.Value,
                    PercentOfPurchase = grandTotalPurchase > 0
                        ? Math.Round(x.Value / grandTotalPurchase * 100m, 4)
                        : (decimal?)null
                })
                .ToList();

            var top1PrincipalPercent = topPurchasePrincipals.FirstOrDefault()?.PercentOfPurchase;
            var top3PrincipalPercent = grandTotalPurchase > 0 && topPurchasePrincipals.Count > 0
                ? Math.Round(
                    topPurchasePrincipals.Take(3).Sum(x => x.MtdPurchaseAmount) / grandTotalPurchase * 100m,
                    4)
                : (decimal?)null;

            var inventoryTop10 = BuildInventoryTop10Lookup(inventorySnapshot);
            var atRiskTop10 = BuildAtRiskTop10Lookup(inventoryRiskSnapshot);
            var totalInventoryValue = inventorySnapshot?.TotalInventoryValue ?? 0m;
            var totalAtRiskValue = inventoryRiskSnapshot?.AtRiskInventoryValue ?? 0m;

            var top1SupplierInventoryPercent = inventoryTop10.Values
                .OrderBy(x => x.Top10Rank ?? int.MaxValue)
                .FirstOrDefault()?.PercentOfInventory;

            var qualifiedByPrincipal = qualifiedInvoices
                .GroupBy(r => DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(r.SupplierName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => new QualifiedBacklogPrincipalStats
                    {
                        Count = g.Count(),
                        Value = g.Sum(r => r.GrandTotal)
                    },
                    StringComparer.OrdinalIgnoreCase);

            var topPrincipalRows = topPurchasePrincipals
                .Select(x =>
                {
                    inventoryTop10.TryGetValue(
                        DashboardPurchasingManagementKeyResolver.NormalizeKey(x.PrincipalName),
                        out var inventoryMatch);
                    atRiskTop10.TryGetValue(
                        DashboardPurchasingManagementKeyResolver.NormalizeKey(x.PrincipalName),
                        out var atRiskMatch);

                    var inInventoryTop10 = inventoryMatch != null;
                    var inAtRiskTop10 = atRiskMatch != null;
                    var isCompound = inInventoryTop10 || inAtRiskTop10;
                    var mtdPurchase = x.MtdPurchaseAmount;
                    var isInventoryNoPurchase = inInventoryTop10 && mtdPurchase == 0m;
                    var identity = DashboardPurchasingManagementSupplierIdentityResolver.Resolve(
                        x.PrincipalName,
                        supplierLookup);

                    return new DashboardPurchasingManagementTopPrincipalRow
                    {
                        Rank = x.Rank,
                        SupplierId = identity.SupplierId,
                        SupplierCode = identity.SupplierCode,
                        PrincipalName = x.PrincipalName,
                        MtdPurchaseAmount = x.MtdPurchaseAmount,
                        PercentOfPurchase = x.PercentOfPurchase,
                        InventoryValue = inventoryMatch?.InventoryValue,
                        PercentOfInventory = inventoryMatch?.PercentOfInventory,
                        AtRiskValue = atRiskMatch?.AtRiskValue,
                        PercentOfAtRisk = atRiskMatch?.PercentOfAtRisk,
                        IsCompoundDependency = isCompound,
                        IsInventoryNoPurchase = isInventoryNoPurchase,
                        ReportRoute = PurchasingReportRoute
                    };
                })
                .ToList();

            var attentionCandidates = new List<AttentionCandidate>();

            foreach (var kvp in qualifiedByPrincipal)
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = kvp.Key,
                    SignalKey = SignalQualifiedBacklog,
                    ValueAmount = kvp.Value.Value,
                    ValueText = $"{kvp.Value.Count} invoice(s)",
                    ReportRoute = PurchasingReportRoute
                });
            }

            foreach (var row in topPurchasePrincipals)
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = row.PrincipalName,
                    SignalKey = SignalPrincipalSpendConcentration,
                    ValueAmount = row.MtdPurchaseAmount,
                    ValueText = row.PercentOfPurchase.HasValue
                        ? $"{row.PercentOfPurchase.Value:0.##}% of purchase"
                        : null,
                    ReportRoute = PurchasingReportRoute
                });
            }

            foreach (var inventory in inventoryTop10.Values.OrderBy(x => x.Top10Rank ?? int.MaxValue))
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = inventory.Name,
                    SignalKey = SignalPrincipalInventoryConcentration,
                    ValueAmount = inventory.InventoryValue,
                    ValueText = inventory.PercentOfInventory.HasValue
                        ? $"{inventory.PercentOfInventory.Value:0.##}% of inventory"
                        : null,
                    ReportRoute = PurchasingReportRoute
                });
            }

            foreach (var atRisk in atRiskTop10.Values.OrderBy(x => x.Rank))
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = atRisk.Name,
                    SignalKey = SignalPrincipalAtRiskExposure,
                    ValueAmount = atRisk.AtRiskValue,
                    ValueText = atRisk.PercentOfAtRisk.HasValue
                        ? $"{atRisk.PercentOfAtRisk.Value:0.##}% of at-risk"
                        : null,
                    ReportRoute = PurchasingReportRoute
                });
            }

            foreach (var row in topPrincipalRows.Where(r => r.IsCompoundDependency))
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = row.PrincipalName,
                    SignalKey = SignalCompoundDependency,
                    ValueAmount = row.MtdPurchaseAmount,
                    ValueText = "Purchase + inventory/at-risk concentration",
                    ReportRoute = PurchasingReportRoute
                });
            }

            foreach (var inventory in inventoryTop10.Values)
            {
                purchaseByPrincipal.TryGetValue(inventory.Name, out var mtdPurchase);
                if (mtdPurchase != 0m)
                    continue;

                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = inventory.Name,
                    SignalKey = SignalPrincipalInventoryNoPurchase,
                    ValueAmount = inventory.InventoryValue,
                    ValueText = "Zero MTD purchase",
                    ReportRoute = PurchasingReportRoute
                });
            }

            var unknownInTop10 = topPurchasePrincipals
                .Any(x => string.Equals(
                    x.PrincipalName,
                    DashboardPurchasingManagementKeyResolver.UnknownPrincipal,
                    StringComparison.OrdinalIgnoreCase));
            var unknownHasQualifiedBacklog = qualifiedByPrincipal.ContainsKey(
                DashboardPurchasingManagementKeyResolver.UnknownPrincipal);

            if (unknownInTop10 || unknownHasQualifiedBacklog)
            {
                purchaseByPrincipal.TryGetValue(
                    DashboardPurchasingManagementKeyResolver.UnknownPrincipal,
                    out var unknownPurchase);
                qualifiedByPrincipal.TryGetValue(
                    DashboardPurchasingManagementKeyResolver.UnknownPrincipal,
                    out var unknownQualified);

                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypePrincipal,
                    EntityName = DashboardPurchasingManagementKeyResolver.UnknownPrincipal,
                    SignalKey = SignalUnknownPrincipal,
                    ValueAmount = unknownQualified?.Value ?? unknownPurchase,
                    ValueText = unknownQualified != null
                        ? $"{unknownQualified.Count} qualified invoice(s)"
                        : "In Top 10 purchase ranking",
                    ReportRoute = PurchasingReportRoute
                });
            }

            var purchasingInactivityFlag = totalInvoice == 0 && today.Day >= PurchasingInactivityDayThreshold;
            if (purchasingInactivityFlag)
            {
                attentionCandidates.Add(new AttentionCandidate
                {
                    EntityType = EntityTypeCompany,
                    EntityName = "(All)",
                    SignalKey = SignalPurchasingInactivity,
                    ValueAmount = null,
                    ValueText = "No purchase invoices recorded by mid-month",
                    ReportRoute = null
                });
            }

            var attentionList = attentionCandidates
                .GroupBy(c => $"{c.EntityType}|{c.EntityName}|{c.SignalKey}", StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(c => SignalPriority.TryGetValue(c.SignalKey, out var priority) ? priority : 99)
                .ThenByDescending(c => c.ValueAmount ?? decimal.MinValue)
                .ThenBy(c => c.EntityName, StringComparer.OrdinalIgnoreCase)
                .Select((c, index) =>
                {
                    SupplierIdentity identity = null;
                    if (string.Equals(c.EntityType, EntityTypePrincipal, StringComparison.OrdinalIgnoreCase))
                    {
                        identity = DashboardPurchasingManagementSupplierIdentityResolver.Resolve(
                            c.EntityName,
                            supplierLookup);
                    }

                    return new DashboardPurchasingManagementAttentionRow
                    {
                        SupplierId = identity?.SupplierId,
                        SupplierCode = identity?.SupplierCode,
                        EntityType = c.EntityType,
                        EntityName = c.EntityName,
                        SignalKey = c.SignalKey,
                        SignalLabel = SignalLabels.TryGetValue(c.SignalKey, out var label) ? label : c.SignalKey,
                        ValueAmount = c.ValueAmount,
                        ValueText = c.ValueText,
                        ReportRoute = c.ReportRoute,
                        SortOrder = index + 1
                    };
                })
                .ToList();

            var portfolio = BuildPortfolio(
                purchaseByPrincipal,
                invoiceCountByPrincipal,
                postedPercentByPrincipal,
                qualifiedByPrincipal,
                inventoryTop10,
                atRiskTop10,
                grandTotalPurchase,
                supplierLookup,
                salesStatsBySupplierId,
                catalogCountsBySupplierId);

            var compoundDependencyCount = topPrincipalRows.Count(r => r.IsCompoundDependency);
            var principalInventoryNoPurchaseCount = inventoryTop10.Values.Count(inventory =>
            {
                purchaseByPrincipal.TryGetValue(inventory.Name, out var mtdPurchase);
                return mtdPurchase == 0m;
            });
            var unknownPrincipalCount = attentionList.Count(a =>
                string.Equals(a.SignalKey, SignalUnknownPrincipal, StringComparison.OrdinalIgnoreCase));
            var principalAtRiskExposureCount = attentionList.Count(a =>
                string.Equals(a.SignalKey, SignalPrincipalAtRiskExposure, StringComparison.OrdinalIgnoreCase));

            return new DashboardPurchasingManagementAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                QualifiedBacklogCount = qualifiedBacklogCount,
                QualifiedBacklogValue = qualifiedBacklogValue,
                PendingPostingValue = pendingPostingValue,
                PostedPercent = postedPercent,
                Top1PrincipalPercent = top1PrincipalPercent,
                Top3PrincipalPercent = top3PrincipalPercent,
                Top1SupplierInventoryPercent = top1SupplierInventoryPercent,
                CompoundDependencyCount = compoundDependencyCount,
                PrincipalInventoryNoPurchaseCount = principalInventoryNoPurchaseCount,
                UnknownPrincipalCount = unknownPrincipalCount,
                PurchasingInactivityFlag = purchasingInactivityFlag,
                QualifiedBacklogPrincipalCount = qualifiedByPrincipal.Count,
                PrincipalAtRiskExposureCount = principalAtRiskExposureCount,
                GeneratedAt = generatedAt,
                AttentionList = attentionList,
                TopPrincipal = topPrincipalRows,
                Portfolio = portfolio
            };
        }

        private static List<DashboardPurchasingManagementPortfolioRow> BuildPortfolio(
            IReadOnlyDictionary<string, decimal> purchaseByPrincipal,
            IReadOnlyDictionary<string, int> invoiceCountByPrincipal,
            IReadOnlyDictionary<string, decimal?> postedPercentByPrincipal,
            IReadOnlyDictionary<string, QualifiedBacklogPrincipalStats> qualifiedByPrincipal,
            IReadOnlyDictionary<string, InventoryTop10Entry> inventoryTop10,
            IReadOnlyDictionary<string, AtRiskTop10Entry> atRiskTop10,
            decimal grandTotalPurchase,
            IReadOnlyDictionary<string, SupplierIdentity> supplierLookup,
            IReadOnlyDictionary<string, SupplierSalesStats> salesStatsBySupplierId,
            IReadOnlyDictionary<string, int> catalogCountsBySupplierId)
        {
            var principalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in purchaseByPrincipal.Keys)
                principalNames.Add(name);
            foreach (var entry in inventoryTop10.Values)
                principalNames.Add(entry.Name);
            foreach (var entry in atRiskTop10.Values)
                principalNames.Add(entry.Name);

            var portfolio = new List<DashboardPurchasingManagementPortfolioRow>();
            foreach (var principalName in principalNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
            {
                purchaseByPrincipal.TryGetValue(principalName, out var mtdPurchase);
                invoiceCountByPrincipal.TryGetValue(principalName, out var invoiceCount);
                postedPercentByPrincipal.TryGetValue(principalName, out var postedPercent);

                inventoryTop10.TryGetValue(
                    DashboardPurchasingManagementKeyResolver.NormalizeKey(principalName),
                    out var inventoryMatch);
                atRiskTop10.TryGetValue(
                    DashboardPurchasingManagementKeyResolver.NormalizeKey(principalName),
                    out var atRiskMatch);

                var qualifiedCount = 0;
                var qualifiedValue = 0m;
                if (qualifiedByPrincipal.TryGetValue(principalName, out var qualified))
                {
                    qualifiedCount = qualified.Count;
                    qualifiedValue = qualified.Value;
                }

                var inInventoryTop10 = inventoryMatch != null;
                var isCompound = inInventoryTop10 || atRiskMatch != null;
                var isInventoryNoPurchase = inInventoryTop10 && mtdPurchase == 0m;
                var identity = DashboardPurchasingManagementSupplierIdentityResolver.Resolve(
                    principalName,
                    supplierLookup);

                salesStatsBySupplierId.TryGetValue(identity.SupplierId, out var salesStats);
                catalogCountsBySupplierId.TryGetValue(identity.SupplierId, out var totalSkuCount);

                var activeSkuCount = salesStats?.ActiveSkuCount ?? 0;
                var salesOutAmount = salesStats?.SalesOutAmount ?? 0m;
                decimal? catalogPenetration = null;
                if (totalSkuCount > 0)
                {
                    catalogPenetration = Math.Round(activeSkuCount / (decimal)totalSkuCount * 100m, 4);
                }

                var inventoryValue = inventoryMatch?.InventoryValue;
                var isActiveMtd = mtdPurchase > 0m
                    || (inventoryValue ?? 0m) > 0m
                    || salesOutAmount > 0m
                    || (atRiskMatch?.AtRiskValue ?? 0m) > 0m;

                if (!isActiveMtd)
                    continue;

                portfolio.Add(new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = identity.SupplierId,
                    SupplierCode = identity.SupplierCode,
                    SupplierName = identity.SupplierName,
                    PrincipalName = identity.PrincipalName,
                    MtdPurchaseAmount = mtdPurchase,
                    MtdInvoiceCount = invoiceCount,
                    PostedPercent = postedPercent,
                    QualifiedBacklogCount = qualifiedCount,
                    QualifiedBacklogValue = qualifiedValue,
                    PercentOfPurchase = grandTotalPurchase > 0
                        ? Math.Round(mtdPurchase / grandTotalPurchase * 100m, 4)
                        : (decimal?)null,
                    InventoryValue = inventoryValue,
                    PercentOfInventory = inventoryMatch?.PercentOfInventory,
                    AtRiskValue = atRiskMatch?.AtRiskValue,
                    PercentOfAtRisk = atRiskMatch?.PercentOfAtRisk,
                    SalesOutAmount = salesOutAmount,
                    ActiveSkuCount = activeSkuCount,
                    TotalSkuCount = totalSkuCount,
                    CatalogPenetrationPercent = catalogPenetration,
                    IsCompoundDependency = isCompound,
                    IsInventoryNoPurchase = isInventoryNoPurchase,
                    IsActiveMtd = isActiveMtd
                });
            }

            return portfolio;
        }

        private static Dictionary<string, SupplierSalesStats> BuildSalesStatsBySupplierId(
            IEnumerable<SupplierMtdItemRollupDto> salesRollups)
        {
            var stats = new Dictionary<string, SupplierSalesStats>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in (salesRollups ?? Enumerable.Empty<SupplierMtdItemRollupDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SupplierId))
                .GroupBy(r => r.SupplierId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var skuIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                decimal total = 0m;
                foreach (var row in group)
                {
                    total += row.LineTotal;
                    if (!string.IsNullOrWhiteSpace(row.BrgId))
                        skuIds.Add(row.BrgId.Trim());
                }

                stats[group.Key] = new SupplierSalesStats
                {
                    SalesOutAmount = total,
                    ActiveSkuCount = skuIds.Count
                };
            }

            return stats;
        }

        private static Dictionary<string, int> BuildCatalogCountsBySupplierId(
            IEnumerable<SupplierCatalogCountDto> catalogCounts)
        {
            return (catalogCounts ?? Enumerable.Empty<SupplierCatalogCountDto>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SupplierId))
                .GroupBy(r => r.SupplierId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().TotalSkuCount,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static decimal? ComputePostedPercentForInvoices(List<InvoiceView> invoices)
        {
            var sudah = invoices.Where(r => IsSudah(r.PostingStok)).Sum(r => r.GrandTotal);
            var belum = invoices.Where(r => IsBelum(r.PostingStok)).Sum(r => r.GrandTotal);
            var total = sudah + belum;
            return total > 0 ? Math.Round(sudah / total * 100m, 4) : (decimal?)null;
        }

        private static decimal? ComputePostedPercent(
            IList<DashboardPurchasingPostingStatusRow> postingStatus,
            List<InvoiceView> invoices)
        {
            decimal sudah;
            decimal belum;

            if (postingStatus != null && postingStatus.Count > 0)
            {
                sudah = postingStatus
                    .FirstOrDefault(p => string.Equals(p.StatusKey, PostingStatusSudahKey, StringComparison.OrdinalIgnoreCase))
                    ?.PurchaseAmount ?? 0m;
                belum = postingStatus
                    .FirstOrDefault(p => string.Equals(p.StatusKey, PostingStatusBelumKey, StringComparison.OrdinalIgnoreCase))
                    ?.PurchaseAmount ?? 0m;
            }
            else
            {
                sudah = invoices.Where(r => IsSudah(r.PostingStok)).Sum(r => r.GrandTotal);
                belum = invoices.Where(r => IsBelum(r.PostingStok)).Sum(r => r.GrandTotal);
            }

            var total = sudah + belum;
            return total > 0 ? Math.Round(sudah / total * 100m, 4) : (decimal?)null;
        }

        private static Dictionary<string, InventoryTop10Entry> BuildInventoryTop10Lookup(
            DashboardInventoryAggregateResult inventorySnapshot)
        {
            var lookup = new Dictionary<string, InventoryTop10Entry>(StringComparer.Ordinal);
            if (inventorySnapshot?.Breakdown == null)
                return lookup;

            var totalInventory = inventorySnapshot.TotalInventoryValue;
            foreach (var row in inventorySnapshot.Breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionSupplier && r.IsTop10))
            {
                var key = DashboardPurchasingManagementKeyResolver.NormalizeKey(row.Name);
                lookup[key] = new InventoryTop10Entry
                {
                    Name = DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(row.Name),
                    InventoryValue = row.InventoryValue,
                    PercentOfInventory = totalInventory > 0
                        ? Math.Round(row.InventoryValue / totalInventory * 100m, 4)
                        : (decimal?)null,
                    Top10Rank = row.Top10Rank
                };
            }

            return lookup;
        }

        private static Dictionary<string, AtRiskTop10Entry> BuildAtRiskTop10Lookup(
            DashboardInventoryRiskAggregateResult inventoryRiskSnapshot)
        {
            var lookup = new Dictionary<string, AtRiskTop10Entry>(StringComparer.Ordinal);
            if (inventoryRiskSnapshot?.Breakdown == null)
                return lookup;

            foreach (var row in inventoryRiskSnapshot.Breakdown
                .Where(r => r.DimensionType == DashboardInventoryRiskAggregator.DimensionSupplier && r.Rank <= TopPrincipalCount))
            {
                var key = DashboardPurchasingManagementKeyResolver.NormalizeKey(row.Name);
                lookup[key] = new AtRiskTop10Entry
                {
                    Name = DashboardPurchasingManagementKeyResolver.ResolvePrincipalName(row.Name),
                    AtRiskValue = row.AtRiskValue,
                    PercentOfAtRisk = row.PercentOfAtRisk,
                    Rank = row.Rank
                };
            }

            return lookup;
        }

        private static bool IsBelum(string postingStok)
        {
            return string.Equals(postingStok?.Trim(), PostingStatusBelumKey, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSudah(string postingStok)
        {
            return string.Equals(postingStok?.Trim(), PostingStatusSudahKey, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class QualifiedBacklogPrincipalStats
        {
            public int Count { get; set; }

            public decimal Value { get; set; }
        }

        private sealed class SupplierSalesStats
        {
            public decimal SalesOutAmount { get; set; }

            public int ActiveSkuCount { get; set; }
        }

        private sealed class AttentionCandidate
        {
            public string EntityType { get; set; }

            public string EntityName { get; set; }

            public string SignalKey { get; set; }

            public decimal? ValueAmount { get; set; }

            public string ValueText { get; set; }

            public string ReportRoute { get; set; }
        }

        private sealed class InventoryTop10Entry
        {
            public string Name { get; set; }

            public decimal InventoryValue { get; set; }

            public decimal? PercentOfInventory { get; set; }

            public int? Top10Rank { get; set; }
        }

        private sealed class AtRiskTop10Entry
        {
            public string Name { get; set; }

            public decimal AtRiskValue { get; set; }

            public decimal? PercentOfAtRisk { get; set; }

            public int Rank { get; set; }
        }
    }
}
