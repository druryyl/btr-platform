using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class PrincipalPurchaseInAggregator
    {
        public PrincipalPurchaseInAggregateResult Aggregate(
            IEnumerable<PurchaseDetailEvidence> purchaseDetails,
            int year,
            int month,
            DateTime generatedAt)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            var known = new List<KnownPurchaseDetail>();
            foreach (var line in purchaseDetails ?? Enumerable.Empty<PurchaseDetailEvidence>())
            {
                if (line is null)
                    continue;

                var supplierId = (line.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                known.Add(new KnownPurchaseDetail
                {
                    SupplierId = supplierId,
                    SupplierName = (line.SupplierName ?? string.Empty).Trim(),
                    PurchaseDetailTotal = line.PurchaseDetailTotal
                });
            }

            var principals = known
                .GroupBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PrincipalPurchaseInRow
                {
                    KpiId = PrincipalKpiCatalog.PurchaseInId,
                    SupplierId = group.Key,
                    SupplierName = group
                        .Select(row => row.SupplierName)
                        .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty,
                    PurchaseInAmount = group.Sum(row => row.PurchaseDetailTotal),
                    LineCount = group.Count()
                })
                .OrderByDescending(row => row.PurchaseInAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalPurchaseInAggregateResult
            {
                KpiId = PrincipalKpiCatalog.PurchaseInId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = generatedAt,
                Principals = principals
            };
        }

        private sealed class KnownPurchaseDetail
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal PurchaseDetailTotal { get; set; }
        }
    }
}
