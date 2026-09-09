using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class PrincipalSalesOutHistoryComposer
    {
        public PrincipalSalesOutHistoryResult Compose(
            IEnumerable<PrincipalSalesOutHistoryMonthEvidence> months,
            PrincipalSalesOutAggregateResult currentSnapshot,
            DateTime generatedAt)
        {
            var rows = new List<PrincipalSalesOutHistoryRow>();
            foreach (var month in months ?? Enumerable.Empty<PrincipalSalesOutHistoryMonthEvidence>())
            {
                if (month is null || !IsKnownPrincipal(month.SupplierId))
                    continue;

                if (month.PeriodYear <= 0 || month.PeriodMonth < 1 || month.PeriodMonth > 12)
                    continue;

                rows.Add(new PrincipalSalesOutHistoryRow
                {
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PeriodYear = month.PeriodYear,
                    PeriodMonth = month.PeriodMonth,
                    SupplierId = month.SupplierId.Trim(),
                    SupplierName = (month.SupplierName ?? string.Empty).Trim(),
                    SalesOutAmount = month.SalesOutAmount,
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

                foreach (var principal in currentSnapshot.Principals ?? new List<PrincipalSalesOutRow>())
                {
                    if (principal is null || !IsKnownPrincipal(principal.SupplierId))
                        continue;

                    rows.Add(new PrincipalSalesOutHistoryRow
                    {
                        KpiId = PrincipalKpiCatalog.SalesOutId,
                        PeriodYear = currentSnapshot.PeriodYear,
                        PeriodMonth = currentSnapshot.PeriodMonth,
                        SupplierId = principal.SupplierId.Trim(),
                        SupplierName = (principal.SupplierName ?? string.Empty).Trim(),
                        SalesOutAmount = principal.SalesOutAmount,
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

            return new PrincipalSalesOutHistoryResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                HistoricalLimitation = PrincipalSalesOutHistory.HistoricalLimitation,
                GeneratedAt = generatedAt,
                Months = ordered
            };
        }

        private static IEnumerable<PrincipalSalesOutHistoryRow> AssignSortOrder(
            IEnumerable<PrincipalSalesOutHistoryRow> rows)
        {
            var list = rows.ToList();
            var needsSort = list.Any(row => row.SortOrder <= 0);
            if (!needsSort)
                return list.OrderBy(row => row.SortOrder);

            return list
                .OrderByDescending(row => row.SalesOutAmount)
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
