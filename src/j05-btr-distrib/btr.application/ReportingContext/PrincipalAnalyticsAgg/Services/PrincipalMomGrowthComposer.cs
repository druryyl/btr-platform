using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Calculates PRN-GRW-001 from stored PRN-SALES-001 month history only.
    /// Does not write PRN-SALES-001 and does not write PRN-GRW-002.
    /// </summary>
    public class PrincipalMomGrowthComposer
    {
        public PrincipalMomGrowthResult Compose(
            PrincipalSalesOutHistoryResult storedHistory,
            DateTime generatedAt)
        {
            var months = storedHistory?.Months ?? new List<PrincipalSalesOutHistoryRow>();
            var indexed = IndexHistory(months);

            var currentPeriod = FindLatestPeriod(indexed.Keys);
            var priorPeriod = AddMonths(currentPeriod, -1);

            var principals = new List<PrincipalMomGrowthRow>();
            if (currentPeriod.HasValue)
            {
                var suppliers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (indexed.TryGetValue(currentPeriod.Value, out var currentRows))
                {
                    foreach (var supplierId in currentRows.Keys)
                        suppliers.Add(supplierId);
                }

                if (indexed.TryGetValue(priorPeriod, out var priorRows))
                {
                    foreach (var supplierId in priorRows.Keys)
                        suppliers.Add(supplierId);
                }

                foreach (var supplierId in suppliers)
                {
                    decimal? currentAmount = null;
                    string supplierName = string.Empty;
                    if (indexed.TryGetValue(currentPeriod.Value, out var currentBySupplier)
                        && currentBySupplier.TryGetValue(supplierId, out var current))
                    {
                        currentAmount = current.SalesOutAmount;
                        supplierName = current.SupplierName;
                    }

                    decimal? priorAmount = null;
                    if (indexed.TryGetValue(priorPeriod, out var priorBySupplier)
                        && priorBySupplier.TryGetValue(supplierId, out var prior))
                    {
                        priorAmount = prior.SalesOutAmount;
                        if (string.IsNullOrWhiteSpace(supplierName))
                            supplierName = prior.SupplierName;
                    }

                    principals.Add(new PrincipalMomGrowthRow
                    {
                        MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId,
                        SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                        SupplierId = supplierId,
                        SupplierName = (supplierName ?? string.Empty).Trim(),
                        CurrentSalesOutAmount = currentAmount,
                        PriorSalesOutAmount = priorAmount,
                        MomGrowthPercentage = Calculate(currentAmount, priorAmount)
                    });
                }
            }

            var ordered = principals
                .OrderBy(row => row.MomGrowthPercentage.HasValue ? 0 : 1)
                .ThenByDescending(row => row.MomGrowthPercentage ?? 0m)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalMomGrowthResult
            {
                MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = currentPeriod?.Year ?? 0,
                PeriodMonth = currentPeriod?.Month ?? 0,
                PriorYear = currentPeriod.HasValue ? priorPeriod.Year : 0,
                PriorMonth = currentPeriod.HasValue ? priorPeriod.Month : 0,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static decimal? Calculate(decimal? currentAmount, decimal? priorAmount)
        {
            if (!currentAmount.HasValue || !priorAmount.HasValue || priorAmount.Value <= 0m)
                return null;

            return Math.Round(
                (currentAmount.Value - priorAmount.Value) / priorAmount.Value,
                PrincipalMomGrowthSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static Dictionary<PeriodKey, Dictionary<string, PrincipalSalesOutHistoryRow>> IndexHistory(
            IEnumerable<PrincipalSalesOutHistoryRow> months)
        {
            var indexed = new Dictionary<PeriodKey, Dictionary<string, PrincipalSalesOutHistoryRow>>();
            foreach (var row in months ?? Enumerable.Empty<PrincipalSalesOutHistoryRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(row.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                if (row.PeriodYear <= 0 || row.PeriodMonth < 1 || row.PeriodMonth > 12)
                    continue;

                var period = new PeriodKey(row.PeriodYear, row.PeriodMonth);
                if (!indexed.TryGetValue(period, out var bySupplier))
                {
                    bySupplier = new Dictionary<string, PrincipalSalesOutHistoryRow>(StringComparer.OrdinalIgnoreCase);
                    indexed[period] = bySupplier;
                }

                if (bySupplier.ContainsKey(supplierId))
                    continue;

                bySupplier[supplierId] = row;
            }

            return indexed;
        }

        private static PeriodKey? FindLatestPeriod(IEnumerable<PeriodKey> periods)
        {
            PeriodKey? latest = null;
            foreach (var period in periods ?? Enumerable.Empty<PeriodKey>())
            {
                if (!latest.HasValue
                    || period.Year > latest.Value.Year
                    || (period.Year == latest.Value.Year && period.Month > latest.Value.Month))
                {
                    latest = period;
                }
            }

            return latest;
        }

        private static PeriodKey AddMonths(PeriodKey? period, int months)
        {
            if (!period.HasValue)
                return new PeriodKey(0, 0);

            var total = (period.Value.Year * 12 + (period.Value.Month - 1)) + months;
            var year = total / 12;
            var month = (total % 12) + 1;
            return new PeriodKey(year, month);
        }

        private struct PeriodKey : IEquatable<PeriodKey>
        {
            public PeriodKey(int year, int month)
            {
                Year = year;
                Month = month;
            }

            public int Year { get; }

            public int Month { get; }

            public bool Equals(PeriodKey other)
            {
                return Year == other.Year && Month == other.Month;
            }

            public override bool Equals(object obj)
            {
                return obj is PeriodKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Year * 397) ^ Month;
            }
        }
    }
}
