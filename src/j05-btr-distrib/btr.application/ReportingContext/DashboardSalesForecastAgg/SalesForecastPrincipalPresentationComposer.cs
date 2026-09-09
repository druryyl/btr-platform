using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.DashboardSalesForecastAgg
{
    public static class SalesForecastPrincipalPresentationComposer
    {
        public const string NotRegistryKpiText =
            "This Principal forecast is a presentation. It is not a registry KPI and is not used to rank Principals.";

        public const string SumNotRequiredToEqualCompanyForecastText =
            "The sum of Principal forecasts is not required to equal the company forecast.";

        public const string NotNetSalesText =
            "This Principal forecast is not Net Sales.";

        public const string InputsText =
            "Principal forecast uses monthly Principal Sales-Out (PRN-SALES-001) history compared with Principal Target (PRN-TGT-001). Purchase-In, Returns, and Faktur header GrandTotal are not inputs.";

        public static DashboardSalesPrincipalForecastPresentation Compose(
            int periodYear,
            int periodMonth,
            DateTime businessDate,
            PrincipalSalesOutHistoryResult history,
            PrincipalTargetAggregateResult target)
        {
            var presentation = new DashboardSalesPrincipalForecastPresentation
            {
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                BusinessDate = businessDate,
                CompanyForecastNote = SumNotRequiredToEqualCompanyForecastText,
                Disclosures = new List<string>
                {
                    NotRegistryKpiText,
                    SumNotRequiredToEqualCompanyForecastText,
                    NotNetSalesText,
                    InputsText
                }
            };

            if (!IsValidPeriod(periodYear, periodMonth))
                return presentation;

            if (history is null || history.KpiId != PrincipalKpiCatalog.SalesOutId)
                return presentation;

            var historyBySupplier = (history.Months ?? new List<PrincipalSalesOutHistoryRow>())
                .Where(row => IsCurrentSalesOutRow(row, periodYear, periodMonth))
                .GroupBy(row => row.SupplierId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

            if (historyBySupplier.Count == 0)
                return presentation;

            var targetsBySupplier = MatchingTargets(target, periodYear, periodMonth);
            var supplierIds = historyBySupplier.Keys
                .Concat(targetsBySupplier.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var monthStart = new DateTime(periodYear, periodMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var items = new List<DashboardSalesPrincipalForecastItem>();

            foreach (var supplierId in supplierIds)
            {
                historyBySupplier.TryGetValue(supplierId, out var historyRow);
                targetsBySupplier.TryGetValue(supplierId, out var targetRow);

                var currentSales = historyRow?.SalesOutAmount ?? 0m;
                var storedTarget = targetRow?.TargetAmount;
                var calculation = SalesForecastPolicy.Compute(
                    currentSales,
                    storedTarget.HasValue && storedTarget.Value > 0 ? storedTarget : null,
                    businessDate,
                    monthStart,
                    monthEnd);

                items.Add(new DashboardSalesPrincipalForecastItem
                {
                    PrincipalName = ResolveName(historyRow, targetRow),
                    SupplierId = supplierId,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = currentSales,
                    TargetKpiId = targetRow == null ? null : PrincipalKpiCatalog.TargetId,
                    PrincipalTargetAmount = storedTarget,
                    DailyAverageSales = calculation.DailyAverageSales,
                    ForecastAmount = calculation.ForecastSales,
                    ForecastAchievementPercent = calculation.ForecastAchievementPercent,
                    RequiredDailySales = calculation.RequiredDailySales,
                    TargetGap = calculation.TargetGap,
                    RequiredDailySeverity = calculation.RequiredDailySales.HasValue
                        ? SalesForecastPolicy.ResolveRequiredDailySeverity(
                            calculation.RequiredDailySales.Value,
                            calculation.DailyAverageSales)
                        : string.Empty
                });
            }

            var ordered = items
                .OrderBy(item => item.PrincipalName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var calendar = SalesForecastPolicy.Compute(
                0m,
                null,
                businessDate,
                monthStart,
                monthEnd);

            presentation.IsAvailable = true;
            presentation.DaysInMonth = calendar.DaysInMonth;
            presentation.DaysElapsed = calendar.DaysElapsed;
            presentation.DaysRemaining = calendar.DaysRemaining;
            presentation.SumOfPrincipalForecasts = ordered.Sum(item => item.ForecastAmount);
            presentation.Items = ordered;
            return presentation;
        }

        private static bool IsCurrentSalesOutRow(
            PrincipalSalesOutHistoryRow row,
            int periodYear,
            int periodMonth)
        {
            return row != null
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.PeriodYear == periodYear
                && row.PeriodMonth == periodMonth
                && !string.IsNullOrWhiteSpace(row.SupplierId);
        }

        private static Dictionary<string, PrincipalTargetRow> MatchingTargets(
            PrincipalTargetAggregateResult target,
            int periodYear,
            int periodMonth)
        {
            if (target == null
                || target.KpiId != PrincipalKpiCatalog.TargetId
                || target.PeriodYear != periodYear
                || target.PeriodMonth != periodMonth)
            {
                return new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
            }

            return (target.Principals ?? new List<PrincipalTargetRow>())
                .Where(row => row != null
                    && row.KpiId == PrincipalKpiCatalog.TargetId
                    && !string.IsNullOrWhiteSpace(row.SupplierId))
                .GroupBy(row => row.SupplierId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static string ResolveName(
            PrincipalSalesOutHistoryRow historyRow,
            PrincipalTargetRow targetRow)
        {
            if (!string.IsNullOrWhiteSpace(historyRow?.SupplierName))
                return historyRow.SupplierName.Trim();

            if (!string.IsNullOrWhiteSpace(targetRow?.SupplierName))
                return targetRow.SupplierName.Trim();

            return string.Empty;
        }

        private static bool IsValidPeriod(int periodYear, int periodMonth)
        {
            return periodYear > 0 && periodMonth >= 1 && periodMonth <= 12;
        }
    }
}
