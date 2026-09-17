using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Maps PRN-RET-001, PRN-RET-002, and PRN-RET-003 from Return Item evidence.
    /// Does not write PRN-SALES-001 or PRN-RET-004.
    /// </summary>
    public class PrincipalReturnAggregator
    {
        public PrincipalReturnAggregateResult Aggregate(
            IEnumerable<ReturnItemEvidence> returnItems,
            int year,
            int month,
            DateTime generatedAt)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            var known = new List<KnownReturnItem>();
            foreach (var line in returnItems ?? Enumerable.Empty<ReturnItemEvidence>())
            {
                if (line is null)
                    continue;

                var jenisRetur = (line.JenisRetur ?? string.Empty).Trim();
                var isGood = string.Equals(
                    jenisRetur,
                    PrincipalReturnSnapshot.GoodReturnJenisRetur,
                    StringComparison.OrdinalIgnoreCase);
                var isBroken = string.Equals(
                    jenisRetur,
                    PrincipalReturnSnapshot.BrokenReturnJenisRetur,
                    StringComparison.OrdinalIgnoreCase);
                if (!isGood && !isBroken)
                    continue;

                var itemSupplierId = (line.ItemSupplierId ?? string.Empty).Trim();
                var supplierId = (line.SupplierId ?? string.Empty).Trim();
                if (itemSupplierId.Length == 0 || supplierId.Length == 0)
                    continue;

                var amount = line.SubTotal - line.DiscRp;
                known.Add(new KnownReturnItem
                {
                    SupplierId = supplierId,
                    SupplierName = (line.SupplierName ?? string.Empty).Trim(),
                    GoodReturnAmount = isGood ? amount : 0m,
                    BrokenReturnAmount = isBroken ? amount : 0m
                });
            }

            var principals = known
                .GroupBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(group => MapPrincipal(group))
                .OrderByDescending(row => row.TotalReturnAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = generatedAt,
                Principals = principals
            };
        }

        private static PrincipalReturnRow MapPrincipal(IGrouping<string, KnownReturnItem> group)
        {
            var goodReturnAmount = group.Sum(row => row.GoodReturnAmount);
            var brokenReturnAmount = group.Sum(row => row.BrokenReturnAmount);

            return new PrincipalReturnRow
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = group.Key,
                SupplierName = group
                    .Select(row => row.SupplierName)
                    .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty,
                GoodReturnAmount = goodReturnAmount,
                BrokenReturnAmount = brokenReturnAmount,
                TotalReturnAmount = goodReturnAmount + brokenReturnAmount,
                LineCount = group.Count()
            };
        }

        private sealed class KnownReturnItem
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal GoodReturnAmount { get; set; }

            public decimal BrokenReturnAmount { get; set; }
        }
    }
}
