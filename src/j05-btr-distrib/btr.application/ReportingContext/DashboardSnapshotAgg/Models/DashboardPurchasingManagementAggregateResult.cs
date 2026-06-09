using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPurchasingManagementAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int QualifiedBacklogCount { get; set; }

        public decimal QualifiedBacklogValue { get; set; }

        public decimal PendingPostingValue { get; set; }

        public decimal? PostedPercent { get; set; }

        public decimal? Top1PrincipalPercent { get; set; }

        public decimal? Top3PrincipalPercent { get; set; }

        public decimal? Top1SupplierInventoryPercent { get; set; }

        public int CompoundDependencyCount { get; set; }

        public int PrincipalInventoryNoPurchaseCount { get; set; }

        public int UnknownPrincipalCount { get; set; }

        public bool PurchasingInactivityFlag { get; set; }

        public int QualifiedBacklogPrincipalCount { get; set; }

        public int PrincipalAtRiskExposureCount { get; set; }

        public DateTime GeneratedAt { get; set; }

        public System.Collections.Generic.List<DashboardPurchasingManagementAttentionRow> AttentionList { get; set; }
            = new System.Collections.Generic.List<DashboardPurchasingManagementAttentionRow>();

        public System.Collections.Generic.List<DashboardPurchasingManagementTopPrincipalRow> TopPrincipal { get; set; }
            = new System.Collections.Generic.List<DashboardPurchasingManagementTopPrincipalRow>();
    }

    public class DashboardPurchasingManagementAttentionRow
    {
        public string EntityType { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string ReportRoute { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardPurchasingManagementTopPrincipalRow
    {
        public int Rank { get; set; }

        public string PrincipalName { get; set; }

        public decimal MtdPurchaseAmount { get; set; }

        public decimal? PercentOfPurchase { get; set; }

        public decimal? InventoryValue { get; set; }

        public decimal? PercentOfInventory { get; set; }

        public decimal? AtRiskValue { get; set; }

        public decimal? PercentOfAtRisk { get; set; }

        public bool IsCompoundDependency { get; set; }

        public bool IsInventoryNoPurchase { get; set; }

        public string ReportRoute { get; set; }
    }
}
