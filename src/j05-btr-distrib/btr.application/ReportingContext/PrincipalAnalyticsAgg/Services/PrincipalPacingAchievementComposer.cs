using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Calculates PRN-TGT-004 Pacing Achievement Percentage from stored PRN-SALES-001,
    /// stored PRN-TGT-001, and the shared analytics period context (PSOM-01).
    /// Does not write either source KPI and does not change PRN-TGT-003.
    /// </summary>
    public class PrincipalPacingAchievementComposer
    {
        public PrincipalPacingAchievementResult Compose(
            PrincipalTargetAggregateResult storedTargets,
            PrincipalSalesOutAggregateResult storedSalesOut,
            AnalyticsPeriodContext period,
            DateTime generatedAt)
        {
            var periodYear = period?.PeriodYear ?? storedTargets?.PeriodYear ?? storedSalesOut?.PeriodYear ?? 0;
            var periodMonth = period?.PeriodMonth ?? storedTargets?.PeriodMonth ?? storedSalesOut?.PeriodMonth ?? 0;
            var elapsedDays = period?.ElapsedDays ?? 0;
            var daysInMonth = period?.DaysInMonth ?? 0;
            var salesOutBySupplier = IndexMatchingSalesOut(storedSalesOut, periodYear, periodMonth);
            var targetsBySupplier = IndexMatchingTargets(storedTargets, periodYear, periodMonth);

            var supplierIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var supplierId in salesOutBySupplier.Keys)
                supplierIds.Add(supplierId);
            foreach (var supplierId in targetsBySupplier.Keys)
                supplierIds.Add(supplierId);

            var principals = new List<PrincipalPacingAchievementRow>();
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

                principals.Add(new PrincipalPacingAchievementRow
                {
                    PacingAchievementKpiId = PrincipalKpiCatalog.PacingAchievementPercentageId,
                    SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    TargetKpiId = PrincipalKpiCatalog.TargetId,
                    SupplierId = supplierId,
                    SupplierName = (supplierName ?? string.Empty).Trim(),
                    SalesOutAmount = salesOutAmount,
                    TargetAmount = targetAmount,
                    PacingAchievementPercentage = Calculate(
                        salesOutAmount,
                        targetAmount,
                        elapsedDays,
                        daysInMonth)
                });
            }

            var ordered = principals
                .OrderBy(row => row.PacingAchievementPercentage.HasValue ? 0 : 1)
                .ThenByDescending(row => row.PacingAchievementPercentage ?? 0m)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalPacingAchievementResult
            {
                PacingAchievementKpiId = PrincipalKpiCatalog.PacingAchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static decimal? Calculate(
            decimal? salesOutAmount,
            decimal? targetAmount,
            int elapsedDays,
            int daysInMonth)
        {
            if (!salesOutAmount.HasValue || !targetAmount.HasValue || targetAmount.Value <= 0m)
                return null;

            if (elapsedDays <= 0 || daysInMonth <= 0)
                return null;

            var expectedTargetMtd = targetAmount.Value * elapsedDays / daysInMonth;
            if (expectedTargetMtd <= 0m)
                return null;

            return Math.Round(
                salesOutAmount.Value / expectedTargetMtd * 100m,
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