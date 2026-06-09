using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardCollectionAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal OverdueExposure { get; set; }

        public decimal AgingOver90Exposure { get; set; }

        public decimal? OverdueConcentrationPercent { get; set; }

        public decimal CashCollectedMtd { get; set; }

        public decimal MonthCollections { get; set; }

        public decimal MonthFakturOmzet { get; set; }

        public decimal? RecoveryVsBillingPercent { get; set; }

        public decimal PaymentMixCashAmount { get; set; }

        public decimal PaymentMixGiroAmount { get; set; }

        public decimal PaymentMixAdjustmentAmount { get; set; }

        public decimal? PaymentMixCashPercent { get; set; }

        public decimal? PaymentMixGiroPercent { get; set; }

        public decimal? PaymentMixAdjustmentPercent { get; set; }

        public int LegacyDebtCount { get; set; }

        public int ChronicOverdueCount { get; set; }

        public int WilayahHotspotCount { get; set; }

        public int LowRecoveryVsBillingCount { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardCollectionAgingRow> AgingRiskSummary { get; set; }
            = new List<DashboardCollectionAgingRow>();

        public List<DashboardCollectionAttentionRow> AttentionList { get; set; }
            = new List<DashboardCollectionAttentionRow>();

        public List<DashboardCollectionTopOverdueCustomerRow> TopOverdueCustomers { get; set; }
            = new List<DashboardCollectionTopOverdueCustomerRow>();

        public List<DashboardCollectionTopOverdueSalesmanRow> TopOverdueSalesmen { get; set; }
            = new List<DashboardCollectionTopOverdueSalesmanRow>();

        public List<DashboardCollectionTopOverdueWilayahRow> TopOverdueWilayah { get; set; }
            = new List<DashboardCollectionTopOverdueWilayahRow>();
    }

    public class DashboardCollectionAgingRow
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCollectionAttentionRow
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public string ReportRoute { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCollectionTopOverdueCustomerRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCollectionTopOverdueSalesmanRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCollectionTopOverdueWilayahRow
    {
        public int Rank { get; set; }

        public string WilayahId { get; set; }

        public string WilayahName { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }
}
