using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCustomerRiskForecastAgg
{
    public class DashboardCustomerRiskForecastDal : IDashboardCustomerRiskForecastDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCustomerRiskForecastDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCustomerRiskForecastResponse GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, BusinessDate, HorizonDays,
       CustomersForecastedAtRisk, HighRiskCustomerCount, CriticalCustomerCount,
       ElevatedRiskReceivable, ElevatedRiskReceivablePercent, PortfolioHealthScore,
       TotalPiutang, ForecastConfidence, PaymentDelaySignalCount, CreditLimitSignalCount,
       InactivitySignalCount, PurchaseDeclineSignalCount, CollectionRiskSignalCount,
       HealthyCount, WatchCount, AttentionCount, HighRiskCount, CriticalCount,
       ExecutiveSummaryText
FROM BTRPD_CustomerRiskForecastKpi
WHERE SnapshotKey = @SnapshotKey";

            const string distSql = @"
SELECT Category, CategoryLabel, CustomerCount, SortOrder
FROM BTRPD_CustomerRiskForecastDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string wilayahSql = @"
SELECT WilayahName, ElevatedRiskCustomerCount, SortOrder
FROM BTRPD_CustomerRiskForecastWilayah
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string signalMixSql = @"
SELECT SignalFamilyKey, SignalFamilyLabel, CustomerCount, SortOrder
FROM BTRPD_CustomerRiskForecastSignalMix
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string customerSql = @"
SELECT SortOrder, RiskPriorityScore, Category, CategoryLabel, CustomerCode, CustomerName,
       WilayahName, SalesPersonName, OpenBalance, OverdueBalance, DueWithinHorizon, Plafond,
       ProjectedOpenBalance, MtdOmzet, PriorMonthOmzet, DeclineRatio, DaysSinceLastFaktur,
       AvgPaymentLagDays, PrimarySignalKey, PrimarySignalLabel, ReasonText, RecommendationKey,
       RecommendationLabel, ReportRoute, DrillDownRoute
FROM BTRPD_CustomerRiskForecastCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string attentionSql = @"
SELECT SortOrder, CustomerCode, CustomerName, SignalKey, SignalLabel, Severity,
       Amount, HorizonText, RuleId, Explanation, ReportRoute
FROM BTRPD_CustomerRiskForecastAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string recommendationSql = @"
SELECT SortOrder, RecommendationKey, RecommendationLabel, CustomerCode, CustomerName,
       Category, ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_CustomerRiskForecastRecommendation
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardCustomerRiskForecastResponse
                    {
                        IsAvailable = false
                    };
                }

                return new DashboardCustomerRiskForecastResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    Kpi = new DashboardCustomerRiskForecastKpiDto
                    {
                        HorizonDays = kpi.HorizonDays,
                        CustomersForecastedAtRisk = kpi.CustomersForecastedAtRisk,
                        HighRiskCustomerCount = kpi.HighRiskCustomerCount,
                        CriticalCustomerCount = kpi.CriticalCustomerCount,
                        ElevatedRiskReceivable = kpi.ElevatedRiskReceivable,
                        ElevatedRiskReceivablePercent = kpi.ElevatedRiskReceivablePercent,
                        PortfolioHealthScore = kpi.PortfolioHealthScore,
                        TotalPiutang = kpi.TotalPiutang,
                        ForecastConfidence = kpi.ForecastConfidence ?? string.Empty,
                        PaymentDelaySignalCount = kpi.PaymentDelaySignalCount,
                        CreditLimitSignalCount = kpi.CreditLimitSignalCount,
                        InactivitySignalCount = kpi.InactivitySignalCount,
                        PurchaseDeclineSignalCount = kpi.PurchaseDeclineSignalCount,
                        CollectionRiskSignalCount = kpi.CollectionRiskSignalCount,
                        HealthyCount = kpi.HealthyCount,
                        WatchCount = kpi.WatchCount,
                        AttentionCount = kpi.AttentionCount,
                        HighRiskCount = kpi.HighRiskCount,
                        CriticalCount = kpi.CriticalCount,
                        ExecutiveSummaryText = kpi.ExecutiveSummaryText ?? string.Empty
                    },
                    CategoryDistribution = conn.Query<DistRow>(distSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastDistDto
                        {
                            Category = r.Category,
                            CategoryLabel = r.CategoryLabel,
                            CustomerCount = r.CustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    TopWilayah = conn.Query<WilayahRow>(wilayahSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastWilayahDto
                        {
                            WilayahName = r.WilayahName,
                            ElevatedRiskCustomerCount = r.ElevatedRiskCustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    SignalMix = conn.Query<SignalMixRow>(signalMixSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastSignalMixDto
                        {
                            SignalFamilyKey = r.SignalFamilyKey,
                            SignalFamilyLabel = r.SignalFamilyLabel,
                            CustomerCount = r.CustomerCount,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    TopCustomers = conn.Query<CustomerRow>(customerSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastCustomerDto
                        {
                            SortOrder = r.SortOrder,
                            RiskPriorityScore = r.RiskPriorityScore,
                            Category = r.Category,
                            CategoryLabel = r.CategoryLabel,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            WilayahName = r.WilayahName,
                            SalesPersonName = r.SalesPersonName,
                            OpenBalance = r.OpenBalance,
                            OverdueBalance = r.OverdueBalance,
                            DueWithinHorizon = r.DueWithinHorizon,
                            Plafond = r.Plafond,
                            ProjectedOpenBalance = r.ProjectedOpenBalance,
                            MtdOmzet = r.MtdOmzet,
                            PriorMonthOmzet = r.PriorMonthOmzet,
                            DeclineRatio = r.DeclineRatio,
                            DaysSinceLastFaktur = r.DaysSinceLastFaktur,
                            AvgPaymentLagDays = r.AvgPaymentLagDays,
                            PrimarySignalKey = r.PrimarySignalKey,
                            PrimarySignalLabel = r.PrimarySignalLabel,
                            ReasonText = r.ReasonText,
                            RecommendationKey = r.RecommendationKey,
                            RecommendationLabel = r.RecommendationLabel,
                            ReportRoute = r.ReportRoute,
                            DrillDownRoute = r.DrillDownRoute
                        }).ToList(),
                    AttentionList = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastAttentionDto
                        {
                            SortOrder = r.SortOrder,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            SignalKey = r.SignalKey,
                            SignalLabel = r.SignalLabel,
                            Severity = r.Severity,
                            Amount = r.Amount,
                            HorizonText = r.HorizonText,
                            RuleId = r.RuleId,
                            Explanation = r.Explanation,
                            ReportRoute = r.ReportRoute
                        }).ToList(),
                    Recommendations = conn.Query<RecommendationRow>(recommendationSql, new { SnapshotKey })
                        .Select(r => new DashboardCustomerRiskForecastRecommendationDto
                        {
                            SortOrder = r.SortOrder,
                            RecommendationKey = r.RecommendationKey,
                            RecommendationLabel = r.RecommendationLabel,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            Category = r.Category,
                            ReasonText = r.ReasonText,
                            RuleId = r.RuleId,
                            ReportRoute = r.ReportRoute,
                            DrillDownRoute = r.DrillDownRoute
                        }).ToList()
                };
            }
        }

        private sealed class KpiRow
        {
            public System.DateTime GeneratedAt { get; set; }
            public System.DateTime BusinessDate { get; set; }
            public int HorizonDays { get; set; }
            public int CustomersForecastedAtRisk { get; set; }
            public int HighRiskCustomerCount { get; set; }
            public int CriticalCustomerCount { get; set; }
            public decimal ElevatedRiskReceivable { get; set; }
            public decimal? ElevatedRiskReceivablePercent { get; set; }
            public decimal PortfolioHealthScore { get; set; }
            public decimal TotalPiutang { get; set; }
            public string ForecastConfidence { get; set; }
            public int PaymentDelaySignalCount { get; set; }
            public int CreditLimitSignalCount { get; set; }
            public int InactivitySignalCount { get; set; }
            public int PurchaseDeclineSignalCount { get; set; }
            public int CollectionRiskSignalCount { get; set; }
            public int HealthyCount { get; set; }
            public int WatchCount { get; set; }
            public int AttentionCount { get; set; }
            public int HighRiskCount { get; set; }
            public int CriticalCount { get; set; }
            public string ExecutiveSummaryText { get; set; }
        }

        private sealed class DistRow
        {
            public string Category { get; set; }
            public string CategoryLabel { get; set; }
            public int CustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class WilayahRow
        {
            public string WilayahName { get; set; }
            public int ElevatedRiskCustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class SignalMixRow
        {
            public string SignalFamilyKey { get; set; }
            public string SignalFamilyLabel { get; set; }
            public int CustomerCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class CustomerRow
        {
            public int SortOrder { get; set; }
            public int RiskPriorityScore { get; set; }
            public string Category { get; set; }
            public string CategoryLabel { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string WilayahName { get; set; }
            public string SalesPersonName { get; set; }
            public decimal OpenBalance { get; set; }
            public decimal OverdueBalance { get; set; }
            public decimal DueWithinHorizon { get; set; }
            public decimal Plafond { get; set; }
            public decimal ProjectedOpenBalance { get; set; }
            public decimal MtdOmzet { get; set; }
            public decimal PriorMonthOmzet { get; set; }
            public decimal? DeclineRatio { get; set; }
            public int? DaysSinceLastFaktur { get; set; }
            public decimal? AvgPaymentLagDays { get; set; }
            public string PrimarySignalKey { get; set; }
            public string PrimarySignalLabel { get; set; }
            public string ReasonText { get; set; }
            public string RecommendationKey { get; set; }
            public string RecommendationLabel { get; set; }
            public string ReportRoute { get; set; }
            public string DrillDownRoute { get; set; }
        }

        private sealed class AttentionRow
        {
            public int SortOrder { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public string Severity { get; set; }
            public decimal? Amount { get; set; }
            public string HorizonText { get; set; }
            public string RuleId { get; set; }
            public string Explanation { get; set; }
            public string ReportRoute { get; set; }
        }

        private sealed class RecommendationRow
        {
            public int SortOrder { get; set; }
            public string RecommendationKey { get; set; }
            public string RecommendationLabel { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string Category { get; set; }
            public string ReasonText { get; set; }
            public string RuleId { get; set; }
            public string ReportRoute { get; set; }
            public string DrillDownRoute { get; set; }
        }
    }
}
