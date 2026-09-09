using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Calculates PRN-RET-004 from stored PRN-RET-003 and PRN-SALES-001.
    /// Does not write either source KPI.
    /// </summary>
    public class PrincipalReturnPercentageComposer
    {
        public PrincipalReturnPercentageResult Compose(
            PrincipalReturnAggregateResult storedReturns,
            PrincipalSalesOutAggregateResult storedSalesOut,
            DateTime generatedAt)
        {
            var periodYear = storedReturns?.PeriodYear ?? 0;
            var periodMonth = storedReturns?.PeriodMonth ?? 0;
            var salesOutBySupplier = IndexMatchingSalesOut(storedSalesOut, periodYear, periodMonth);

            var principals = new List<PrincipalReturnPercentageRow>();
            foreach (var row in storedReturns?.Principals ?? new List<PrincipalReturnRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(
                    row.TotalReturnKpiId,
                    PrincipalKpiCatalog.TotalReturnAmountId,
                    StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                decimal? salesOutAmount = null;
                if (salesOutBySupplier.TryGetValue(supplierId, out var salesOut))
                    salesOutAmount = salesOut;

                principals.Add(new PrincipalReturnPercentageRow
                {
                    ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                    SupplierId = supplierId,
                    SupplierName = (row.SupplierName ?? string.Empty).Trim(),
                    TotalReturnAmount = row.TotalReturnAmount,
                    SalesOutAmount = salesOutAmount,
                    ReturnPercentage = Calculate(row.TotalReturnAmount, salesOutAmount)
                });
            }

            var ordered = principals
                .OrderBy(row => row.ReturnPercentage.HasValue ? 0 : 1)
                .ThenByDescending(row => row.ReturnPercentage ?? 0m)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalReturnPercentageResult
            {
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static decimal? Calculate(decimal totalReturnAmount, decimal? salesOutAmount)
        {
            if (!salesOutAmount.HasValue || salesOutAmount.Value <= 0m)
                return null;

            return Math.Round(
                totalReturnAmount / salesOutAmount.Value,
                PrincipalReturnPercentageSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static Dictionary<string, decimal> IndexMatchingSalesOut(
            PrincipalSalesOutAggregateResult storedSalesOut,
            int periodYear,
            int periodMonth)
        {
            var salesOutBySupplier = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (storedSalesOut is null)
                return salesOutBySupplier;

            if (storedSalesOut.PeriodYear != periodYear || storedSalesOut.PeriodMonth != periodMonth)
                return salesOutBySupplier;

            foreach (var row in storedSalesOut.Principals ?? new List<PrincipalSalesOutRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(row.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || salesOutBySupplier.ContainsKey(supplierId))
                    continue;

                salesOutBySupplier[supplierId] = row.SalesOutAmount;
            }

            return salesOutBySupplier;
        }
    }
}
