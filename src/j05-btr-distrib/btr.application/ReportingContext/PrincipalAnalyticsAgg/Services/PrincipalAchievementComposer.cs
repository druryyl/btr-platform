using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Calculates PRN-TGT-002 and PRN-TGT-003 from stored PRN-SALES-001 and PRN-TGT-001.
    /// Does not write either source KPI.
    /// </summary>
    public class PrincipalAchievementComposer
    {
        public PrincipalAchievementResult Compose(
            PrincipalTargetAggregateResult storedTargets,
            PrincipalSalesOutAggregateResult storedSalesOut,
            DateTime generatedAt)
        {
            var periodYear = storedTargets?.PeriodYear ?? storedSalesOut?.PeriodYear ?? 0;
            var periodMonth = storedTargets?.PeriodMonth ?? storedSalesOut?.PeriodMonth ?? 0;
            var salesOutBySupplier = IndexMatchingSalesOut(storedSalesOut, periodYear, periodMonth);
            var targetsBySupplier = IndexMatchingTargets(storedTargets, periodYear, periodMonth);

            var supplierIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var supplierId in salesOutBySupplier.Keys)
                supplierIds.Add(supplierId);
            foreach (var supplierId in targetsBySupplier.Keys)
                supplierIds.Add(supplierId);

            var principals = new List<PrincipalAchievementRow>();
            foreach (var supplierId in supplierIds)
            {
                decimal? salesOutAmount = null;
                string supplierName = string.Empty;
                if (salesOutBySupplier.TryGetValue(supplierId, out var salesOut))
                {
                    salesOutAmount = salesOut.SalesOutAmount;
                    supplierName = salesOut.SupplierName;
                }

                decimal? targetAmount = null;
                if (targetsBySupplier.TryGetValue(supplierId, out var target))
                {
                    targetAmount = target.TargetAmount;
                    supplierName = target.SupplierName;
                }

                principals.Add(new PrincipalAchievementRow
                {
                    AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                    AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    TargetKpiId = PrincipalKpiCatalog.TargetId,
                    SupplierId = supplierId,
                    SupplierName = (supplierName ?? string.Empty).Trim(),
                    SalesOutAmount = salesOutAmount,
                    TargetAmount = targetAmount,
                    AchievementAmount = CalculateAmount(salesOutAmount, targetAmount),
                    AchievementPercentage = CalculatePercentage(salesOutAmount, targetAmount)
                });
            }

            var ordered = principals
                .OrderBy(row => row.AchievementPercentage.HasValue ? 0 : 1)
                .ThenByDescending(row => row.AchievementPercentage ?? 0m)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalAchievementResult
            {
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static decimal? CalculateAmount(decimal? salesOutAmount, decimal? targetAmount)
        {
            if (!salesOutAmount.HasValue || !targetAmount.HasValue || targetAmount.Value <= 0m)
                return null;

            return salesOutAmount.Value - targetAmount.Value;
        }

        public static decimal? CalculatePercentage(decimal? salesOutAmount, decimal? targetAmount)
        {
            if (!salesOutAmount.HasValue || !targetAmount.HasValue || targetAmount.Value <= 0m)
                return null;

            return Math.Round(
                salesOutAmount.Value / targetAmount.Value,
                PrincipalAchievementSnapshot.PercentageScale,
                MidpointRounding.AwayFromZero);
        }

        private static Dictionary<string, PrincipalSalesOutRow> IndexMatchingSalesOut(
            PrincipalSalesOutAggregateResult storedSalesOut,
            int periodYear,
            int periodMonth)
        {
            var salesOutBySupplier = new Dictionary<string, PrincipalSalesOutRow>(StringComparer.OrdinalIgnoreCase);
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

                salesOutBySupplier[supplierId] = row;
            }

            return salesOutBySupplier;
        }

        private static Dictionary<string, PrincipalTargetRow> IndexMatchingTargets(
            PrincipalTargetAggregateResult storedTargets,
            int periodYear,
            int periodMonth)
        {
            var targetsBySupplier = new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
            if (storedTargets is null)
                return targetsBySupplier;

            if (storedTargets.PeriodYear != periodYear || storedTargets.PeriodMonth != periodMonth)
                return targetsBySupplier;

            foreach (var row in storedTargets.Principals ?? new List<PrincipalTargetRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(row.KpiId, PrincipalKpiCatalog.TargetId, StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || targetsBySupplier.ContainsKey(supplierId))
                    continue;

                targetsBySupplier[supplierId] = row;
            }

            return targetsBySupplier;
        }
    }
}
