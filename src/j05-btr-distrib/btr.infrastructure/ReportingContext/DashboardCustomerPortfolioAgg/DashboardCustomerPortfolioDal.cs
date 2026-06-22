using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Contracts;
using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCustomerPortfolioAgg
{
    public class DashboardCustomerPortfolioDal : IDashboardCustomerPortfolioDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCustomerPortfolioDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCustomerPortfolioResponse GetCurrent()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(KpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardCustomerPortfolioResponse
                    {
                        IsAvailable = false
                    };
                }

                var concentration = conn.Query<ConcentrationRow>(@"
SELECT SortOrder, Rank, CustomerCode, CustomerName, Amount, PercentOfTotal, ConcentrationType
FROM BTRPD_CustomerPortfolioConcentration
WHERE SnapshotKey = @SnapshotKey
ORDER BY ConcentrationType, SortOrder", new { SnapshotKey }).ToList();

                return new DashboardCustomerPortfolioResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    Kpi = MapKpi(kpi),
                    LifecycleDistribution = conn.Query<LifecycleDistRow>(@"
SELECT LifecycleStage, LifecycleLabel, CustomerCount, SortOrder
FROM BTRPD_CustomerPortfolioLifecycleDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCustomerPortfolioLifecycleDistDto
                        {
                            LifecycleStage = r.LifecycleStage,
                            LifecycleLabel = r.LifecycleLabel,
                            CustomerCount = r.CustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    TierDistribution = conn.Query<TierDistRow>(@"
SELECT PortfolioTier, TierLabel, CustomerCount, SortOrder
FROM BTRPD_CustomerPortfolioTierDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCustomerPortfolioTierDistDto
                        {
                            PortfolioTier = r.PortfolioTier,
                            TierLabel = r.TierLabel,
                            CustomerCount = r.CustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    ActionDistribution = conn.Query<ActionDistRow>(@"
SELECT PrimaryActionKey, PrimaryActionLabel, CustomerCount, SortOrder
FROM BTRPD_CustomerPortfolioActionDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCustomerPortfolioActionDistDto
                        {
                            PrimaryActionKey = r.PrimaryActionKey,
                            PrimaryActionLabel = r.PrimaryActionLabel,
                            CustomerCount = r.CustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    PriorityQueue = conn.Query<PriorityRow>(PrioritySql, new { SnapshotKey })
                        .Select(MapPriority).ToList(),
                    Customers = conn.Query<CustomerRow>(CustomerSql, new { SnapshotKey })
                        .Select(MapCustomer).ToList(),
                    TopOmzet = MapConcentration(concentration, DashboardCustomerPortfolioAggregator.ConcentrationOmzet),
                    TopPiutang = MapConcentration(concentration, DashboardCustomerPortfolioAggregator.ConcentrationPiutang),
                    WilayahBreakdown = conn.Query<WilayahRow>(@"
SELECT SortOrder, WilayahName, CustomerCount, AttentionCustomerCount
FROM BTRPD_CustomerPortfolioWilayah
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCustomerPortfolioWilayahDto
                        {
                            SortOrder = r.SortOrder,
                            WilayahName = r.WilayahName,
                            CustomerCount = r.CustomerCount,
                            AttentionCustomerCount = r.AttentionCustomerCount
                        }).ToList()
                };
            }
        }

        public DashboardCustomerPortfolioKpiDto GetCurrentKpi()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(KpiSql, new { SnapshotKey });
                return kpi is null ? null : MapKpi(kpi);
            }
        }

        private const string KpiSql = @"
SELECT GeneratedAt, BusinessDate, PortfolioHealthScore, PortfolioHealthyPercent, TotalCustomerCount,
       AttentionCustomerCount, StrategicCustomerCount, StrategicAtRiskCount, CustomersAtRiskCount,
       WorkingCapitalTiedAmount, TotalMtdOmzet, TotalOpenBalance, NeverPurchasedCount, DormantCount,
       DecliningCount, ExecutiveSummaryText, ValueDisclaimerText
FROM BTRPD_CustomerPortfolioKpi
WHERE SnapshotKey = @SnapshotKey";

        private const string PrioritySql = @"
SELECT SortOrder, PortfolioPriorityScore, CustomerKey, CustomerCode, CustomerName, WilayahName, Klasifikasi,
       LifecycleStage, LifecycleLabel, PortfolioTier, TierLabel, PrimaryActionKey, PrimaryActionLabel,
       ActionOwner, ActionReasonText, TriggeredRuleIds, MtdOmzet, OpenBalance, OverdueBalance, M29Category,
       SalesPersonName, SalesmanAchievementPercent, SalesmanHighPiutangExposure, IsAttention, M30LinkRoute,
       CustomerReportRoute, DrillDownRouteM17, DrillDownRouteM29
FROM BTRPD_CustomerPortfolioPriority
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

        private const string CustomerSql = @"
SELECT SortOrder, CustomerKey, CustomerCode, CustomerName, WilayahName, Klasifikasi, LifecycleStage, LifecycleLabel,
       PortfolioTier, TierLabel, PrimaryActionKey, PrimaryActionLabel, ActionOwner, ActionReasonText,
       TriggeredRuleIds, MtdOmzet, OpenBalance, OverdueBalance, FakturCount6Mo, IsActiveMtd, LastPurchaseDate,
       FirstPurchaseDate, M29Category, M29PrimarySignalKey, SalesPersonName, SalesmanAchievementPercent,
       SalesmanHighPiutangExposure, IsAttention, PortfolioPriorityScore, M30LinkRoute, CustomerReportRoute,
       DrillDownRouteM17, DrillDownRouteM29, ValueDisclaimer
FROM BTRPD_CustomerPortfolioCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

        private static DashboardCustomerPortfolioKpiDto MapKpi(KpiRow kpi) =>
            new DashboardCustomerPortfolioKpiDto
            {
                PortfolioHealthScore = kpi.PortfolioHealthScore,
                PortfolioHealthyPercent = kpi.PortfolioHealthyPercent,
                TotalCustomerCount = kpi.TotalCustomerCount,
                AttentionCustomerCount = kpi.AttentionCustomerCount,
                StrategicCustomerCount = kpi.StrategicCustomerCount,
                StrategicAtRiskCount = kpi.StrategicAtRiskCount,
                CustomersAtRiskCount = kpi.CustomersAtRiskCount,
                WorkingCapitalTiedAmount = kpi.WorkingCapitalTiedAmount,
                TotalMtdOmzet = kpi.TotalMtdOmzet,
                TotalOpenBalance = kpi.TotalOpenBalance,
                NeverPurchasedCount = kpi.NeverPurchasedCount,
                DormantCount = kpi.DormantCount,
                DecliningCount = kpi.DecliningCount,
                ExecutiveSummaryText = kpi.ExecutiveSummaryText ?? string.Empty,
                ValueDisclaimerText = kpi.ValueDisclaimerText ?? string.Empty
            };

        private static DashboardCustomerPortfolioPriorityDto MapPriority(PriorityRow r) =>
            new DashboardCustomerPortfolioPriorityDto
            {
                SortOrder = r.SortOrder,
                PortfolioPriorityScore = r.PortfolioPriorityScore,
                CustomerKey = r.CustomerKey,
                CustomerCode = r.CustomerCode,
                CustomerName = r.CustomerName,
                WilayahName = r.WilayahName,
                Klasifikasi = r.Klasifikasi,
                LifecycleStage = r.LifecycleStage,
                LifecycleLabel = r.LifecycleLabel,
                PortfolioTier = r.PortfolioTier,
                TierLabel = r.TierLabel,
                PrimaryActionKey = r.PrimaryActionKey,
                PrimaryActionLabel = r.PrimaryActionLabel,
                ActionOwner = r.ActionOwner,
                ActionReasonText = r.ActionReasonText,
                TriggeredRuleIds = r.TriggeredRuleIds,
                MtdOmzet = r.MtdOmzet,
                OpenBalance = r.OpenBalance,
                OverdueBalance = r.OverdueBalance,
                M29Category = r.M29Category,
                SalesPersonName = r.SalesPersonName,
                SalesmanAchievementPercent = r.SalesmanAchievementPercent,
                SalesmanHighPiutangExposure = r.SalesmanHighPiutangExposure,
                IsAttention = r.IsAttention,
                M30LinkRoute = r.M30LinkRoute,
                CustomerReportRoute = r.CustomerReportRoute,
                DrillDownRouteM17 = r.DrillDownRouteM17,
                DrillDownRouteM29 = r.DrillDownRouteM29
            };

        private static DashboardCustomerPortfolioCustomerDto MapCustomer(CustomerRow r) =>
            new DashboardCustomerPortfolioCustomerDto
            {
                SortOrder = r.SortOrder,
                CustomerKey = r.CustomerKey,
                CustomerCode = r.CustomerCode,
                CustomerName = r.CustomerName,
                WilayahName = r.WilayahName,
                Klasifikasi = r.Klasifikasi,
                LifecycleStage = r.LifecycleStage,
                LifecycleLabel = r.LifecycleLabel,
                PortfolioTier = r.PortfolioTier,
                TierLabel = r.TierLabel,
                PrimaryActionKey = r.PrimaryActionKey,
                PrimaryActionLabel = r.PrimaryActionLabel,
                ActionOwner = r.ActionOwner,
                ActionReasonText = r.ActionReasonText,
                TriggeredRuleIds = r.TriggeredRuleIds,
                MtdOmzet = r.MtdOmzet,
                OpenBalance = r.OpenBalance,
                OverdueBalance = r.OverdueBalance,
                FakturCount6Mo = r.FakturCount6Mo,
                IsActiveMtd = r.IsActiveMtd,
                LastPurchaseDate = r.LastPurchaseDate,
                FirstPurchaseDate = r.FirstPurchaseDate,
                M29Category = r.M29Category,
                M29PrimarySignalKey = r.M29PrimarySignalKey,
                SalesPersonName = r.SalesPersonName,
                SalesmanAchievementPercent = r.SalesmanAchievementPercent,
                SalesmanHighPiutangExposure = r.SalesmanHighPiutangExposure,
                IsAttention = r.IsAttention,
                PortfolioPriorityScore = r.PortfolioPriorityScore,
                M30LinkRoute = r.M30LinkRoute,
                CustomerReportRoute = r.CustomerReportRoute,
                DrillDownRouteM17 = r.DrillDownRouteM17,
                DrillDownRouteM29 = r.DrillDownRouteM29,
                ValueDisclaimer = r.ValueDisclaimer
            };

        private static List<DashboardCustomerPortfolioConcentrationDto> MapConcentration(
            IReadOnlyList<ConcentrationRow> rows,
            string concentrationType) =>
            rows
                .Where(r => string.Equals(r.ConcentrationType, concentrationType, System.StringComparison.OrdinalIgnoreCase))
                .Select(r => new DashboardCustomerPortfolioConcentrationDto
                {
                    SortOrder = r.SortOrder,
                    Rank = r.Rank,
                    CustomerCode = r.CustomerCode,
                    CustomerName = r.CustomerName,
                    Amount = r.Amount,
                    PercentOfTotal = r.PercentOfTotal
                }).ToList();

        private sealed class KpiRow
        {
            public System.DateTime GeneratedAt { get; set; }
            public System.DateTime BusinessDate { get; set; }
            public decimal PortfolioHealthScore { get; set; }
            public decimal PortfolioHealthyPercent { get; set; }
            public int TotalCustomerCount { get; set; }
            public int AttentionCustomerCount { get; set; }
            public int StrategicCustomerCount { get; set; }
            public int StrategicAtRiskCount { get; set; }
            public int CustomersAtRiskCount { get; set; }
            public decimal WorkingCapitalTiedAmount { get; set; }
            public decimal TotalMtdOmzet { get; set; }
            public decimal TotalOpenBalance { get; set; }
            public int NeverPurchasedCount { get; set; }
            public int DormantCount { get; set; }
            public int DecliningCount { get; set; }
            public string ExecutiveSummaryText { get; set; }
            public string ValueDisclaimerText { get; set; }
        }

        private sealed class LifecycleDistRow
        {
            public string LifecycleStage { get; set; }
            public string LifecycleLabel { get; set; }
            public int CustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class TierDistRow
        {
            public string PortfolioTier { get; set; }
            public string TierLabel { get; set; }
            public int CustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class ActionDistRow
        {
            public string PrimaryActionKey { get; set; }
            public string PrimaryActionLabel { get; set; }
            public int CustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class PriorityRow
        {
            public int SortOrder { get; set; }
            public int PortfolioPriorityScore { get; set; }
            public string CustomerKey { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string WilayahName { get; set; }
            public string Klasifikasi { get; set; }
            public string LifecycleStage { get; set; }
            public string LifecycleLabel { get; set; }
            public string PortfolioTier { get; set; }
            public string TierLabel { get; set; }
            public string PrimaryActionKey { get; set; }
            public string PrimaryActionLabel { get; set; }
            public string ActionOwner { get; set; }
            public string ActionReasonText { get; set; }
            public string TriggeredRuleIds { get; set; }
            public decimal MtdOmzet { get; set; }
            public decimal OpenBalance { get; set; }
            public decimal? OverdueBalance { get; set; }
            public string M29Category { get; set; }
            public string SalesPersonName { get; set; }
            public decimal? SalesmanAchievementPercent { get; set; }
            public bool SalesmanHighPiutangExposure { get; set; }
            public bool IsAttention { get; set; }
            public string M30LinkRoute { get; set; }
            public string CustomerReportRoute { get; set; }
            public string DrillDownRouteM17 { get; set; }
            public string DrillDownRouteM29 { get; set; }
        }

        private sealed class CustomerRow
        {
            public int SortOrder { get; set; }
            public string CustomerKey { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string WilayahName { get; set; }
            public string Klasifikasi { get; set; }
            public string LifecycleStage { get; set; }
            public string LifecycleLabel { get; set; }
            public string PortfolioTier { get; set; }
            public string TierLabel { get; set; }
            public string PrimaryActionKey { get; set; }
            public string PrimaryActionLabel { get; set; }
            public string ActionOwner { get; set; }
            public string ActionReasonText { get; set; }
            public string TriggeredRuleIds { get; set; }
            public decimal MtdOmzet { get; set; }
            public decimal OpenBalance { get; set; }
            public decimal? OverdueBalance { get; set; }
            public int FakturCount6Mo { get; set; }
            public bool IsActiveMtd { get; set; }
            public System.DateTime? LastPurchaseDate { get; set; }
            public System.DateTime? FirstPurchaseDate { get; set; }
            public string M29Category { get; set; }
            public string M29PrimarySignalKey { get; set; }
            public string SalesPersonName { get; set; }
            public decimal? SalesmanAchievementPercent { get; set; }
            public bool SalesmanHighPiutangExposure { get; set; }
            public bool IsAttention { get; set; }
            public int PortfolioPriorityScore { get; set; }
            public string M30LinkRoute { get; set; }
            public string CustomerReportRoute { get; set; }
            public string DrillDownRouteM17 { get; set; }
            public string DrillDownRouteM29 { get; set; }
            public string ValueDisclaimer { get; set; }
        }

        private sealed class ConcentrationRow
        {
            public string ConcentrationType { get; set; }
            public int SortOrder { get; set; }
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal Amount { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class WilayahRow
        {
            public int SortOrder { get; set; }
            public string WilayahName { get; set; }
            public int CustomerCount { get; set; }
            public int AttentionCustomerCount { get; set; }
        }
    }
}
