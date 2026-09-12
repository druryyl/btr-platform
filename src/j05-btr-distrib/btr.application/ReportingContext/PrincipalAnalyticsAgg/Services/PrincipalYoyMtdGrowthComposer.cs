using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Calculates PRN-GRW-003 Year-over-Year MTD Growth Percentage from the current-year and
    /// prior-year equivalent MTD windows sourced via the PSOM-04 Sales-Out range evidence query.
    /// Does not write any source KPI and does not change PRN-GRW-002.
    /// </summary>
    public class PrincipalYoyMtdGrowthComposer
    {
        private readonly IPrincipalSalesOutRangeEvidenceDal _rangeEvidenceDal;

        public PrincipalYoyMtdGrowthComposer(IPrincipalSalesOutRangeEvidenceDal rangeEvidenceDal)
        {
            _rangeEvidenceDal = rangeEvidenceDal;
        }

        public PrincipalYoyMtdGrowthResult Compose(
            AnalyticsPeriodContext period,
            DateTime generatedAt)
        {
            if (period == null || _rangeEvidenceDal == null)
                return ComposeWindows(period, null, null, generatedAt);

            var currentYearMtdRows = _rangeEvidenceDal.ListSalesOutByRange(period.MonthStart, period.AsOfDate);
            var priorYearMtdRows = _rangeEvidenceDal.ListSalesOutByRange(period.PriorYearStart, period.PriorYearEnd);

            return ComposeWindows(period, currentYearMtdRows, priorYearMtdRows, generatedAt);
        }

        public PrincipalYoyMtdGrowthResult ComposeWindows(
            AnalyticsPeriodContext period,
            IEnumerable<PrincipalSalesOutRangeEvidenceRow> currentYearMtdRows,
            IEnumerable<PrincipalSalesOutRangeEvidenceRow> priorYearMtdRows,
            DateTime generatedAt)
        {
            var currentAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var currentNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var priorAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var priorNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Accumulate(currentYearMtdRows, currentAmounts, currentNames);
            Accumulate(priorYearMtdRows, priorAmounts, priorNames);

            var supplierIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var supplierId in currentAmounts.Keys)
                supplierIds.Add(supplierId);
            foreach (var supplierId in priorAmounts.Keys)
                supplierIds.Add(supplierId);

            var principals = new List<PrincipalYoyMtdGrowthRow>();
            foreach (var supplierId in supplierIds)
            {
                var hasCurrent = currentAmounts.TryGetValue(supplierId, out var currentAmount);
                var hasPrior = priorAmounts.TryGetValue(supplierId, out var priorAmount);

                var supplierName = currentNames.TryGetValue(supplierId, out var currentName)
                    ? currentName
                    : (priorNames.TryGetValue(supplierId, out var priorName) ? priorName : string.Empty);

                principals.Add(new PrincipalYoyMtdGrowthRow
                {
                    YoyMtdGrowthKpiId = PrincipalKpiCatalog.YoyMtdGrowthId,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    SupplierId = supplierId,
                    SupplierName = (supplierName ?? string.Empty).Trim(),
                    CurrentYearMtdSalesOutAmount = hasCurrent ? currentAmount : (decimal?)null,
                    PriorYearMtdSalesOutAmount = hasPrior ? priorAmount : (decimal?)null,
                    YoyMtdGrowthPercentage = Calculate(
                        hasCurrent ? currentAmount : (decimal?)null,
                        hasPrior ? priorAmount : (decimal?)null)
                });
            }

            var ordered = principals
                .OrderBy(row => row.YoyMtdGrowthPercentage.HasValue ? 0 : 1)
                .ThenByDescending(row => row.YoyMtdGrowthPercentage ?? 0m)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalYoyMtdGrowthResult
            {
                YoyMtdGrowthKpiId = PrincipalKpiCatalog.YoyMtdGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = period?.PeriodYear ?? 0,
                PeriodMonth = period?.PeriodMonth ?? 0,
                PriorYear = period?.PriorYear ?? 0,
                PriorMonth = period?.PriorMonth ?? 0,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static decimal? Calculate(decimal? currentYearMtdAmount, decimal? priorYearMtdAmount)
        {
            if (!currentYearMtdAmount.HasValue || !priorYearMtdAmount.HasValue || priorYearMtdAmount.Value <= 0m)
                return null;

            return Math.Round(
                (currentYearMtdAmount.Value - priorYearMtdAmount.Value) / priorYearMtdAmount.Value * 100m,
                PrincipalAchievementSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static void Accumulate(
            IEnumerable<PrincipalSalesOutRangeEvidenceRow> rows,
            IDictionary<string, decimal> amounts,
            IDictionary<string, string> names)
        {
            foreach (var row in rows ?? Enumerable.Empty<PrincipalSalesOutRangeEvidenceRow>())
            {
                if (row is null)
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                amounts.TryGetValue(supplierId, out var amount);
                amounts[supplierId] = amount + row.SalesOutAmount;

                if (!names.TryGetValue(supplierId, out var name) || string.IsNullOrWhiteSpace(name))
                    names[supplierId] = (row.SupplierName ?? string.Empty).Trim();
            }
        }
    }
}
