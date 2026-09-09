using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Counts PRN-CUS-001 from stored BTRPD_CustomerPrincipalRelationship rows
    /// whose stored status is Active. Does not scan raw transactions,
    /// does not write projection status, and does not write PRN-SALES-001.
    /// </summary>
    public class PrincipalActiveCustomerComposer
    {
        public PrincipalActiveCustomerResult Compose(
            CustomerPrincipalRelationshipResult storedProjection,
            DateTime generatedAt)
        {
            var asOfDate = storedProjection?.AsOfDate ?? generatedAt.Date;
            var pairs = storedProjection?.Pairs ?? new List<CustomerPrincipalRelationshipRow>();

            var bySupplier = new Dictionary<string, SupplierAccumulator>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in pairs)
            {
                if (pair is null)
                    continue;

                var supplierId = (pair.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                SupplierAccumulator accumulator;
                if (!bySupplier.TryGetValue(supplierId, out accumulator))
                {
                    accumulator = new SupplierAccumulator
                    {
                        SupplierId = supplierId,
                        SupplierName = (pair.SupplierName ?? string.Empty).Trim()
                    };
                    bySupplier[supplierId] = accumulator;
                }

                if (string.IsNullOrWhiteSpace(accumulator.SupplierName))
                    accumulator.SupplierName = (pair.SupplierName ?? string.Empty).Trim();

                if (IsActive(pair.RelationshipStatus))
                    accumulator.ActiveCustomerCount++;
            }

            var ordered = bySupplier.Values
                .OrderByDescending(accumulator => accumulator.ActiveCustomerCount)
                .ThenBy(accumulator => accumulator.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((accumulator, index) => new PrincipalActiveCustomerRow
                {
                    ActiveCustomerKpiId = PrincipalKpiCatalog.ActiveCustomerCountId,
                    SupplierId = accumulator.SupplierId,
                    SupplierName = accumulator.SupplierName ?? string.Empty,
                    ActiveCustomerCount = accumulator.ActiveCustomerCount,
                    SortOrder = index + 1
                })
                .ToList();

            return new PrincipalActiveCustomerResult
            {
                ActiveCustomerKpiId = PrincipalKpiCatalog.ActiveCustomerCountId,
                AsOfDate = asOfDate,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        public static bool IsActive(string relationshipStatus)
        {
            return string.Equals(
                (relationshipStatus ?? string.Empty).Trim(),
                CustomerPrincipalRelationship.StatusActive,
                StringComparison.Ordinal);
        }

        private sealed class SupplierAccumulator
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public int ActiveCustomerCount { get; set; }
        }
    }
}
