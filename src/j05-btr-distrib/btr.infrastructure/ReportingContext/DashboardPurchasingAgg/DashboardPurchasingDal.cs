using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardPurchasingAgg.Contracts;
using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardPurchasingAgg
{
    public class DashboardPurchasingDal : IDashboardPurchasingDal
    {
        private const string PurchasingReportRoute = "/reports/purchasing";
        private const string InventoryDashboardRoute = "/dashboard/inventory";
        private const string InventoryRiskDashboardRoute = "/dashboard/inventory-risk";

        private readonly IDashboardPurchasingSnapshotDal _snapshotDal;
        private readonly IDashboardPurchasingManagementSnapshotDal _managementSnapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardPurchasingDal(
            IDashboardPurchasingSnapshotDal snapshotDal,
            IDashboardPurchasingManagementSnapshotDal managementSnapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _managementSnapshotDal = managementSnapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        private static string BuildSupplierProfileRoute(string supplierCode)
        {
            if (string.IsNullOrWhiteSpace(supplierCode))
                return null;

            return $"/analytics/suppliers/{Uri.EscapeDataString(supplierCode.Trim())}";
        }

        public DashboardPurchasingResponse GetSummary()
        {
            var v1Snapshot = _snapshotDal.GetCurrent();
            if (v1Snapshot == null)
                throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");

            var response = DashboardPurchasingSnapshotDal.MapToResponse(v1Snapshot);
            var management = _managementSnapshotDal.GetCurrent();
            var utcNow = DateTime.UtcNow;

            response.Navigation = BuildNavigation();
            response.IsManagementAvailable = management != null;

            if (management == null)
            {
                response.IsDataFresh = IsDomainFresh(v1Snapshot.GeneratedAt, _options.PurchasingIntervalMinutes, utcNow);
                return response;
            }

            var freshnessAnchor = v1Snapshot.GeneratedAt <= management.GeneratedAt
                ? v1Snapshot.GeneratedAt
                : management.GeneratedAt;
            response.GeneratedAt = freshnessAnchor;
            response.IsDataFresh = IsDomainFresh(
                freshnessAnchor,
                _options.PurchasingManagementIntervalMinutes,
                utcNow);

            var postingRequiresAttention = management.QualifiedBacklogCount > 0;
            var dependencyRequiresAttention = management.CompoundDependencyCount > 0
                || management.UnknownPrincipalCount > 0;
            var paceRequiresAttention = management.PurchasingInactivityFlag;
            var crossRiskRequiresAttention = management.PrincipalAtRiskExposureCount > 0
                || management.PrincipalInventoryNoPurchaseCount > 0;

            response.Summary = new DashboardPurchasingSummaryRow
            {
                GrandTotalPurchase = response.GrandTotalPurchase,
                TotalInvoice = response.TotalInvoice,
                PostedPercent = management.PostedPercent,
                PendingPostingValue = management.PendingPostingValue,
                QualifiedBacklogCount = management.QualifiedBacklogCount,
                QualifiedBacklogValue = management.QualifiedBacklogValue
            };

            response.AttentionCards = new DashboardPurchasingAttentionCards
            {
                PostingExposure = new DashboardPurchasingAttentionCardGroup
                {
                    RequiresAttention = postingRequiresAttention,
                    Metrics = new Dictionary<string, string>
                    {
                        ["Qualified Backlog Count"] = management.QualifiedBacklogCount.ToString(CultureInfo.InvariantCulture),
                        ["Qualified Backlog Value"] = FormatCurrency(management.QualifiedBacklogValue),
                        ["Pending Posting Value"] = FormatCurrency(management.PendingPostingValue)
                    }
                },
                PrincipalDependency = new DashboardPurchasingAttentionCardGroup
                {
                    RequiresAttention = dependencyRequiresAttention,
                    Metrics = new Dictionary<string, string>
                    {
                        ["Top 1 Principal %"] = FormatPercent(management.Top1PrincipalPercent),
                        ["Top 3 Principal %"] = FormatPercent(management.Top3PrincipalPercent),
                        ["Compound Dependency Count"] = management.CompoundDependencyCount.ToString(CultureInfo.InvariantCulture)
                    }
                },
                PurchasingPace = new DashboardPurchasingAttentionCardGroup
                {
                    RequiresAttention = paceRequiresAttention,
                    Metrics = new Dictionary<string, string>
                    {
                        ["Total Invoice"] = response.TotalInvoice.ToString(CultureInfo.InvariantCulture),
                        ["Purchasing Inactivity"] = management.PurchasingInactivityFlag ? "Active" : "None"
                    }
                },
                InventoryCrossRisk = new DashboardPurchasingAttentionCardGroup
                {
                    RequiresAttention = crossRiskRequiresAttention,
                    Metrics = new Dictionary<string, string>
                    {
                        ["Top 1 Supplier Inventory %"] = FormatPercent(management.Top1SupplierInventoryPercent),
                        ["Principal At-Risk Count"] = management.PrincipalAtRiskExposureCount.ToString(CultureInfo.InvariantCulture),
                        ["Inventory, No Purchase Count"] = management.PrincipalInventoryNoPurchaseCount.ToString(CultureInfo.InvariantCulture)
                    }
                }
            };

            response.AttentionList = management.AttentionList?
                .Select(row => new DashboardPurchasingAttentionItem
                {
                    EntityType = row.EntityType,
                    EntityName = row.EntityName,
                    SignalKey = row.SignalKey,
                    SignalLabel = row.SignalLabel,
                    ValueAmount = row.ValueAmount,
                    ValueText = row.ValueText,
                    ReportRoute = row.ReportRoute,
                    ProfileRoute = string.Equals(
                        row.EntityType,
                        DashboardPurchasingManagementAggregator.EntityTypePrincipal,
                        StringComparison.OrdinalIgnoreCase)
                        ? BuildSupplierProfileRoute(row.SupplierCode)
                        : null,
                    Investigation = InvestigationMetadataBuilder.Build(
                        row.SignalKey,
                        row.EntityType ?? InvestigationMetadataBuilder.EntityTypePrincipal,
                        row.EntityName,
                        row.EntityName,
                        signalLabelOverride: row.SignalLabel,
                        reportRouteOverride: row.ReportRoute)
                })
                .ToList() ?? new List<DashboardPurchasingAttentionItem>();

            response.PrincipalExposure = management.TopPrincipal?
                .Select(row => new DashboardPurchasingPrincipalExposureItem
                {
                    Rank = row.Rank,
                    SupplierCode = row.SupplierCode,
                    PrincipalName = row.PrincipalName,
                    MtdPurchaseAmount = row.MtdPurchaseAmount,
                    PercentOfPurchase = row.PercentOfPurchase,
                    InventoryValue = row.InventoryValue,
                    PercentOfInventory = row.PercentOfInventory,
                    AtRiskValue = row.AtRiskValue,
                    PercentOfAtRisk = row.PercentOfAtRisk,
                    IsCompoundDependency = row.IsCompoundDependency,
                    IsInventoryNoPurchase = row.IsInventoryNoPurchase,
                    ReportRoute = row.ReportRoute ?? PurchasingReportRoute,
                    ProfileRoute = BuildSupplierProfileRoute(row.SupplierCode),
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalRankingTopPrincipal,
                        InvestigationMetadataBuilder.EntityTypePrincipal,
                        null,
                        row.PrincipalName)
                })
                .ToList() ?? new List<DashboardPurchasingPrincipalExposureItem>();

            return response;
        }

        private static DashboardPurchasingNavigationLinks BuildNavigation()
        {
            return new DashboardPurchasingNavigationLinks
            {
                PurchasingReportRoute = PurchasingReportRoute,
                InventoryDashboardRoute = InventoryDashboardRoute,
                InventoryRiskDashboardRoute = InventoryRiskDashboardRoute
            };
        }

        private static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }

        private static string FormatCurrency(decimal value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string FormatPercent(decimal? value)
        {
            return value.HasValue
                ? $"{value.Value.ToString("0.##", CultureInfo.InvariantCulture)}%"
                : "—";
        }
    }
}
