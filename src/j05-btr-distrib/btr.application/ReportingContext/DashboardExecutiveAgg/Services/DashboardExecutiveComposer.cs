using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Services;
using btr.application.ReportingContext.DashboardExecutiveAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;

namespace btr.application.ReportingContext.DashboardExecutiveAgg.Services
{
    public class DashboardExecutiveComposer
    {
        public const int ExecutiveRiskListCount = 5;

        private const string AgingOver90BucketKey = "DaysOver90";
        private const string PostingStatusBelumKey = "BELUM";

        public DashboardExecutiveResponse Compose(ExecutiveComposeInput input)
        {
            var utcNow = input.UtcNow;
            var options = input.Options ?? new DashboardSnapshotOptions();
            var refreshStatuses = input.RefreshStatuses ?? new List<DashboardSnapshotRefreshStatusModel>();

            var sales = input.Sales;
            var piutang = input.Piutang;
            var inventory = input.Inventory;
            var purchasing = input.Purchasing;

            var hasUnavailable = DashboardSnapshotHealthHelper.HasExecutiveUnavailableDomain(
                sales, piutang, inventory, purchasing);

            var salesAttention = ComposeSales(sales);
            var piutangAttention = ComposePiutang(piutang);
            var purchasingAttention = ComposePurchasing(purchasing, input.PurchasingManagement);
            var inventoryAttention = ComposeInventory(inventory);
            var criticalExposures = ComposeCriticalExposures(piutang, inventory, purchasing);

            var lastRefreshed = DashboardSnapshotHealthHelper.ResolveLastRefreshed(
                sales?.GeneratedAt,
                piutang?.GeneratedAt,
                inventory?.GeneratedAt,
                purchasing?.GeneratedAt);

            var isDataFresh = DashboardSnapshotHealthHelper.IsExecutiveDataFresh(
                sales, piutang, inventory, purchasing, options, utcNow);

            var overallHealth = DashboardSnapshotHealthHelper.ResolveOverallHealth(refreshStatuses);

            var domainSummaries = ComposeDomainSummaries(
                salesAttention, piutangAttention, purchasingAttention, inventoryAttention);

            return new DashboardExecutiveResponse
            {
                HasUnavailableDomain = hasUnavailable,
                IsDataFresh = isDataFresh,
                LastRefreshed = lastRefreshed,
                OverallHealthStatus = overallHealth,
                Sales = salesAttention,
                Piutang = piutangAttention,
                Purchasing = purchasingAttention,
                Inventory = inventoryAttention,
                CriticalExposures = criticalExposures,
                DomainSummaries = domainSummaries
            };
        }

        private static DashboardExecutiveSalesAttention ComposeSales(DashboardSalesAggregateResult sales)
        {
            if (sales == null)
            {
                return new DashboardExecutiveSalesAttention { IsAvailable = false };
            }

            var band = ExecutiveSalesAchievementBandResolver.Resolve(sales.AchievementPercent);
            var requiresAttention = band == ExecutiveSalesAchievementBandResolver.Warning
                || band == ExecutiveSalesAchievementBandResolver.Critical;

            return new DashboardExecutiveSalesAttention
            {
                IsAvailable = true,
                AchievementPercent = sales.AchievementPercent,
                TotalAchievement = sales.TotalAchievement,
                AchievementBand = band,
                RequiresAttention = requiresAttention
            };
        }

        private static DashboardExecutivePiutangAttention ComposePiutang(DashboardPiutangAggregateResult piutang)
        {
            if (piutang == null)
            {
                return new DashboardExecutivePiutangAttention { IsAvailable = false };
            }

            var over90Amount = piutang.AgingOver90Amount > 0
                ? piutang.AgingOver90Amount
                : piutang.AgingBuckets?
                    .FirstOrDefault(b => b.BucketKey == AgingOver90BucketKey)?.Amount ?? 0m;

            decimal? over90Percent = piutang.AgingOver90Percent
                ?? (piutang.TotalPiutang > 0 ? over90Amount / piutang.TotalPiutang * 100m : (decimal?)null);

            var topCustomer = piutang.TopCustomerRisk?
                .OrderBy(c => c.Rank)
                .FirstOrDefault();

            decimal? topCustomerPercent = topCustomer != null && piutang.TotalPiutang > 0
                ? topCustomer.TotalPiutang / piutang.TotalPiutang * 100m
                : (decimal?)null;

            return new DashboardExecutivePiutangAttention
            {
                IsAvailable = true,
                TotalPiutang = piutang.TotalPiutang,
                OverdueCustomer = piutang.OverdueCustomer,
                AgingOver90Amount = over90Amount,
                AgingOver90Percent = over90Percent,
                TopCustomerPercent = topCustomerPercent,
                RequiresAttention = piutang.OverdueCustomer > 0 || over90Amount > 0
            };
        }

        private static DashboardExecutivePurchasingAttention ComposePurchasing(
            DashboardPurchasingAggregateResult purchasing,
            DashboardPurchasingManagementAggregateResult purchasingManagement)
        {
            if (purchasing == null)
            {
                return new DashboardExecutivePurchasingAttention { IsAvailable = false };
            }

            var belumValue = purchasing.PostingStatus?
                .FirstOrDefault(p => string.Equals(p.StatusKey, PostingStatusBelumKey, StringComparison.OrdinalIgnoreCase))
                ?.PurchaseAmount ?? 0m;

            var topPrincipal = purchasing.TopPrincipal?
                .OrderBy(p => p.Rank)
                .FirstOrDefault();

            decimal? topPrincipalPercent = topPrincipal != null && purchasing.GrandTotalPurchase > 0
                ? topPrincipal.PurchaseAmount / purchasing.GrandTotalPurchase * 100m
                : (decimal?)null;

            var qualifiedBacklogCount = purchasingManagement?.QualifiedBacklogCount ?? 0;

            return new DashboardExecutivePurchasingAttention
            {
                IsAvailable = true,
                PendingPostingInvoiceCount = purchasing.PendingPostingInvoiceCount,
                PendingPostingValue = belumValue,
                QualifiedBacklogCount = qualifiedBacklogCount,
                TopPrincipalPercent = topPrincipalPercent,
                RequiresAttention = qualifiedBacklogCount > 0
            };
        }

        private static DashboardExecutiveInventoryAttention ComposeInventory(
            DashboardInventoryAggregateResult inventory)
        {
            if (inventory == null)
            {
                return new DashboardExecutiveInventoryAttention { IsAvailable = false };
            }

            var breakdown = inventory.Breakdown ?? new List<DashboardInventoryBreakdownRow>();

            var topCategory = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionCategory && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .FirstOrDefault();

            var topSupplier = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionSupplier && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .FirstOrDefault();

            decimal? topCategoryPercent = topCategory != null && inventory.TotalInventoryValue > 0
                ? topCategory.InventoryValue / inventory.TotalInventoryValue * 100m
                : (decimal?)null;

            decimal? topSupplierPercent = topSupplier != null && inventory.TotalInventoryValue > 0
                ? topSupplier.InventoryValue / inventory.TotalInventoryValue * 100m
                : (decimal?)null;

            return new DashboardExecutiveInventoryAttention
            {
                IsAvailable = true,
                TotalInventoryValue = inventory.TotalInventoryValue,
                TopCategoryPercent = topCategoryPercent,
                TopSupplierPercent = topSupplierPercent,
                RequiresAttention = topCategoryPercent.HasValue || topSupplierPercent.HasValue
            };
        }

        private static DashboardExecutiveCriticalExposures ComposeCriticalExposures(
            DashboardPiutangAggregateResult piutang,
            DashboardInventoryAggregateResult inventory,
            DashboardPurchasingAggregateResult purchasing)
        {
            var topCustomers = piutang?.TopCustomerRisk?
                .OrderBy(c => c.Rank)
                .Take(ExecutiveRiskListCount)
                .Select(c => new DashboardExecutiveRiskItem
                {
                    Rank = c.Rank,
                    Name = c.CustomerName,
                    Amount = c.TotalPiutang,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalExecutiveTopCustomerExposure,
                        InvestigationMetadataBuilder.EntityTypeCustomer,
                        c.CustomerCode,
                        c.CustomerName)
                })
                .ToList() ?? new List<DashboardExecutiveRiskItem>();

            var breakdown = inventory?.Breakdown ?? new List<DashboardInventoryBreakdownRow>();

            var topCategories = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionCategory && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Take(ExecutiveRiskListCount)
                .Select(r => new DashboardExecutiveRiskItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    Amount = r.InventoryValue,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalExecutiveTopCategoryExposure,
                        InvestigationMetadataBuilder.EntityTypeCategory,
                        null,
                        r.Name)
                })
                .ToList();

            var topSuppliers = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionSupplier && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Take(ExecutiveRiskListCount)
                .Select(r => new DashboardExecutiveRiskItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    Amount = r.InventoryValue,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalExecutiveTopSupplierExposure,
                        InvestigationMetadataBuilder.EntityTypeSupplier,
                        null,
                        r.Name)
                })
                .ToList();

            var topPrincipals = purchasing?.TopPrincipal?
                .OrderBy(p => p.Rank)
                .Take(ExecutiveRiskListCount)
                .Select(p => new DashboardExecutiveRiskItem
                {
                    Rank = p.Rank,
                    Name = p.PrincipalName,
                    Amount = p.PurchaseAmount,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalExecutiveTopPrincipalExposure,
                        InvestigationMetadataBuilder.EntityTypePrincipal,
                        null,
                        p.PrincipalName)
                })
                .ToList() ?? new List<DashboardExecutiveRiskItem>();

            return new DashboardExecutiveCriticalExposures
            {
                TopCustomers = topCustomers,
                TopCategories = topCategories,
                TopSuppliers = topSuppliers,
                TopPrincipals = topPrincipals
            };
        }

        private static IList<DashboardExecutiveDomainSummary> ComposeDomainSummaries(
            DashboardExecutiveSalesAttention sales,
            DashboardExecutivePiutangAttention piutang,
            DashboardExecutivePurchasingAttention purchasing,
            DashboardExecutiveInventoryAttention inventory)
        {
            return new List<DashboardExecutiveDomainSummary>
            {
                new DashboardExecutiveDomainSummary
                {
                    Domain = "Sales",
                    DetailDashboardRoute = "/dashboard/sales",
                    IsAvailable = sales.IsAvailable,
                    SummaryText = sales.IsAvailable
                        ? FormatSalesSummary(sales)
                        : "Sales data unavailable"
                },
                new DashboardExecutiveDomainSummary
                {
                    Domain = "Piutang",
                    DetailDashboardRoute = "/dashboard/piutang",
                    IsAvailable = piutang.IsAvailable,
                    SummaryText = piutang.IsAvailable
                        ? FormatPiutangSummary(piutang)
                        : "Piutang data unavailable"
                },
                new DashboardExecutiveDomainSummary
                {
                    Domain = "Purchasing",
                    DetailDashboardRoute = "/dashboard/purchasing",
                    IsAvailable = purchasing.IsAvailable,
                    SummaryText = purchasing.IsAvailable
                        ? FormatPurchasingSummary(purchasing)
                        : "Purchasing data unavailable"
                },
                new DashboardExecutiveDomainSummary
                {
                    Domain = "Inventory",
                    DetailDashboardRoute = "/dashboard/inventory",
                    IsAvailable = inventory.IsAvailable,
                    SummaryText = inventory.IsAvailable
                        ? FormatInventorySummary(inventory)
                        : "Inventory data unavailable"
                }
            };
        }

        private static string FormatSalesSummary(DashboardExecutiveSalesAttention sales)
        {
            var percentText = sales.AchievementPercent.HasValue
                ? $"{sales.AchievementPercent.Value.ToString("0.#", CultureInfo.InvariantCulture)}% achievement"
                : "No target configured";

            return $"{percentText} · {sales.AchievementBand} · {FormatCurrency(sales.TotalAchievement)}";
        }

        private static string FormatPiutangSummary(DashboardExecutivePiutangAttention piutang)
        {
            var over90Text = piutang.AgingOver90Percent.HasValue
                ? $" · >90d {FormatCurrency(piutang.AgingOver90Amount)} ({piutang.AgingOver90Percent.Value.ToString("0.#", CultureInfo.InvariantCulture)}%)"
                : string.Empty;

            return $"{FormatCurrency(piutang.TotalPiutang)} · {piutang.OverdueCustomer} overdue{over90Text}";
        }

        private static string FormatPurchasingSummary(DashboardExecutivePurchasingAttention purchasing)
        {
            if (purchasing.QualifiedBacklogCount > 0)
            {
                return $"Qualified Backlog · {purchasing.QualifiedBacklogCount} invoices · Pending {FormatCurrency(purchasing.PendingPostingValue)}";
            }

            return $"Pending Posting · {purchasing.PendingPostingInvoiceCount} invoices · {FormatCurrency(purchasing.PendingPostingValue)}";
        }

        private static string FormatInventorySummary(DashboardExecutiveInventoryAttention inventory)
        {
            var categoryText = inventory.TopCategoryPercent.HasValue
                ? $" · Top category {inventory.TopCategoryPercent.Value.ToString("0.#", CultureInfo.InvariantCulture)}%"
                : string.Empty;

            return $"{FormatCurrency(inventory.TotalInventoryValue)}{categoryText}";
        }

        private static string FormatCurrency(decimal amount)
        {
            return amount.ToString("N0", CultureInfo.InvariantCulture);
        }
    }

    public class ExecutiveComposeInput
    {
        public DashboardSalesAggregateResult Sales { get; set; }

        public DashboardPiutangAggregateResult Piutang { get; set; }

        public DashboardInventoryAggregateResult Inventory { get; set; }

        public DashboardPurchasingAggregateResult Purchasing { get; set; }

        public DashboardPurchasingManagementAggregateResult PurchasingManagement { get; set; }

        public IReadOnlyList<DashboardSnapshotRefreshStatusModel> RefreshStatuses { get; set; }

        public DashboardSnapshotOptions Options { get; set; }

        public DateTime UtcNow { get; set; }
    }
}
