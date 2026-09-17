using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class PrincipalTargetAggregator
    {
        public PrincipalTargetAggregateResult Aggregate(
            IEnumerable<SalesmanPrincipalTargetEvidence> targets,
            int year,
            int month,
            DateTime generatedAt)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            var known = new List<KnownTarget>();
            foreach (var target in targets ?? Enumerable.Empty<SalesmanPrincipalTargetEvidence>())
            {
                if (target is null)
                    continue;

                var supplierId = (target.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                known.Add(new KnownTarget
                {
                    SupplierId = supplierId,
                    SupplierName = (target.SupplierName ?? string.Empty).Trim(),
                    TargetAmount = target.TargetAmount
                });
            }

            var principals = known
                .GroupBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PrincipalTargetRow
                {
                    KpiId = PrincipalKpiCatalog.TargetId,
                    SupplierId = group.Key,
                    SupplierName = group
                        .Select(row => row.SupplierName)
                        .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty,
                    TargetAmount = group.Sum(row => row.TargetAmount),
                    SourceCount = group.Count()
                })
                .OrderByDescending(row => row.TargetAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = generatedAt,
                Principals = principals
            };
        }

        private sealed class KnownTarget
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal TargetAmount { get; set; }
        }
    }
}
