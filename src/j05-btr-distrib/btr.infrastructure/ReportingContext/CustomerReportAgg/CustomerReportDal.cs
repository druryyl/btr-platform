using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.CustomerReportAgg.Contracts;
using btr.application.ReportingContext.CustomerReportAgg.Queries;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.CustomerReportAgg
{
    public class CustomerReportDal : ICustomerReportDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public CustomerReportDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public CustomerReportResponse GetReport(string customerCode = null)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(@"
SELECT GeneratedAt, BusinessDate, ValueDisclaimerText
FROM BTRPD_CustomerPortfolioKpi
WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey });

                if (kpi is null)
                {
                    return new CustomerReportResponse
                    {
                        IsAvailable = false
                    };
                }

                var sql = @"
SELECT SortOrder, CustomerCode, CustomerName, WilayahName, Klasifikasi, LifecycleStage, LifecycleLabel,
       PortfolioTier, TierLabel, PrimaryActionKey, PrimaryActionLabel, ActionOwner, ActionReasonText,
       MtdOmzet, OpenBalance, OverdueBalance, LastPurchaseDate, FirstPurchaseDate, M29Category,
       SalesPersonName, SalesmanAchievementPercent, IsAttention, ValueDisclaimer
FROM BTRPD_CustomerPortfolioCustomer
WHERE SnapshotKey = @SnapshotKey";

                if (!string.IsNullOrWhiteSpace(customerCode))
                {
                    sql += " AND CustomerCode = @CustomerCode";
                }

                sql += " ORDER BY SortOrder";

                var rows = conn.Query<CustomerRow>(sql, new { SnapshotKey, CustomerCode = customerCode?.Trim() })
                    .Select(r => new CustomerReportRowDto
                    {
                        SortOrder = r.SortOrder,
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
                        MtdOmzet = r.MtdOmzet,
                        OpenBalance = r.OpenBalance,
                        OverdueBalance = r.OverdueBalance,
                        LastPurchaseDate = r.LastPurchaseDate,
                        FirstPurchaseDate = r.FirstPurchaseDate,
                        M29Category = r.M29Category,
                        SalesPersonName = r.SalesPersonName,
                        SalesmanAchievementPercent = r.SalesmanAchievementPercent,
                        IsAttention = r.IsAttention,
                        ValueDisclaimer = r.ValueDisclaimer
                    }).ToList();

                return new CustomerReportResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    Rows = rows,
                    Summary = new CustomerReportSummaryDto
                    {
                        TotalCustomers = rows.Count,
                        TotalMtdOmzet = rows.Sum(r => r.MtdOmzet),
                        TotalOpenBalance = rows.Sum(r => r.OpenBalance),
                        ValueDisclaimerText = kpi.ValueDisclaimerText ?? string.Empty
                    }
                };
            }
        }

        private sealed class KpiRow
        {
            public System.DateTime GeneratedAt { get; set; }
            public System.DateTime BusinessDate { get; set; }
            public string ValueDisclaimerText { get; set; }
        }

        private sealed class CustomerRow
        {
            public int SortOrder { get; set; }
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
            public decimal MtdOmzet { get; set; }
            public decimal OpenBalance { get; set; }
            public decimal? OverdueBalance { get; set; }
            public System.DateTime? LastPurchaseDate { get; set; }
            public System.DateTime? FirstPurchaseDate { get; set; }
            public string M29Category { get; set; }
            public string SalesPersonName { get; set; }
            public decimal? SalesmanAchievementPercent { get; set; }
            public bool IsAttention { get; set; }
            public string ValueDisclaimer { get; set; }
        }
    }
}
