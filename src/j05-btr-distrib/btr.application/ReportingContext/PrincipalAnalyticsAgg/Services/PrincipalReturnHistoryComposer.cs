using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Composes PRN-RET-001, PRN-RET-002, and PRN-RET-003 monthly history.
    /// Stores return amounts only. Does not store or overwrite PRN-SALES-001 history.
    /// Does not calculate growth KPIs.
    /// </summary>
    public class PrincipalReturnHistoryComposer
    {
        public PrincipalReturnHistoryResult Compose(
            IEnumerable<PrincipalReturnHistoryMonthEvidence> months,
            PrincipalReturnAggregateResult currentSnapshot,
            DateTime generatedAt)
        {
            var rows = new List<PrincipalReturnHistoryRow>();
            foreach (var month in months ?? Enumerable.Empty<PrincipalReturnHistoryMonthEvidence>())
            {
                if (month is null || !IsKnownPrincipal(month.SupplierId))
                    continue;

                if (month.PeriodYear <= 0 || month.PeriodMonth < 1 || month.PeriodMonth > 12)
                    continue;

                rows.Add(new PrincipalReturnHistoryRow
                {
                    GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                    BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                    TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                    PeriodYear = month.PeriodYear,
                    PeriodMonth = month.PeriodMonth,
                    SupplierId = month.SupplierId.Trim(),
                    SupplierName = (month.SupplierName ?? string.Empty).Trim(),
                    GoodReturnAmount = month.GoodReturnAmount,
                    BrokenReturnAmount = month.BrokenReturnAmount,
                    TotalReturnAmount = month.GoodReturnAmount + month.BrokenReturnAmount,
                    LineCount = month.LineCount
                });
            }

            if (currentSnapshot != null
                && currentSnapshot.PeriodYear > 0
                && currentSnapshot.PeriodMonth >= 1
                && currentSnapshot.PeriodMonth <= 12)
            {
                rows.RemoveAll(row =>
                    row.PeriodYear == currentSnapshot.PeriodYear
                    && row.PeriodMonth == currentSnapshot.PeriodMonth);

                foreach (var principal in currentSnapshot.Principals ?? new List<PrincipalReturnRow>())
                {
                    if (principal is null || !IsKnownPrincipal(principal.SupplierId))
                        continue;

                    rows.Add(new PrincipalReturnHistoryRow
                    {
                        GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                        BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                        TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                        PeriodYear = currentSnapshot.PeriodYear,
                        PeriodMonth = currentSnapshot.PeriodMonth,
                        SupplierId = principal.SupplierId.Trim(),
                        SupplierName = (principal.SupplierName ?? string.Empty).Trim(),
                        GoodReturnAmount = principal.GoodReturnAmount,
                        BrokenReturnAmount = principal.BrokenReturnAmount,
                        TotalReturnAmount = principal.GoodReturnAmount + principal.BrokenReturnAmount,
                        LineCount = principal.LineCount,
                        SortOrder = principal.SortOrder
                    });
                }
            }

            var ordered = rows
                .GroupBy(row => new { row.PeriodYear, row.PeriodMonth })
                .SelectMany(group => AssignSortOrder(group))
                .OrderBy(row => row.PeriodYear)
                .ThenBy(row => row.PeriodMonth)
                .ThenBy(row => row.SortOrder)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new PrincipalReturnHistoryResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                GeneratedAt = generatedAt,
                Months = ordered
            };
        }

        private static IEnumerable<PrincipalReturnHistoryRow> AssignSortOrder(
            IEnumerable<PrincipalReturnHistoryRow> rows)
        {
            var list = rows.ToList();
            var needsSort = list.Any(row => row.SortOrder <= 0);
            if (!needsSort)
                return list.OrderBy(row => row.SortOrder);

            return list
                .OrderByDescending(row => row.TotalReturnAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                });
        }

        private static bool IsKnownPrincipal(string supplierId)
        {
            return !string.IsNullOrWhiteSpace(supplierId);
        }
    }
}
