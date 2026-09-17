using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Computes PRN-CUS-002 from the stored Customer-Principal relationship projection
    /// and stored PRN-CUS-001. Does not scan raw transactions, does not write projection
    /// status, and does not write PRN-SALES-001.
    /// </summary>
    public class PrincipalCustomerCoverageComposer
    {
        public PrincipalCustomerCoverageResult Compose(
            CustomerPrincipalRelationshipResult storedProjection,
            PrincipalActiveCustomerResult storedActiveCustomers,
            DateTime generatedAt)
        {
            var asOfDate = storedProjection?.AsOfDate ?? generatedAt.Date;
            var pairs = storedProjection?.Pairs ?? new List<CustomerPrincipalRelationshipRow>();

            var activeBySupplier = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var activeRow in storedActiveCustomers?.Principals ?? new List<PrincipalActiveCustomerRow>())
            {
                if (activeRow is null)
                    continue;

                var supplierId = (activeRow.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                activeBySupplier[supplierId] = activeRow.ActiveCustomerCount;
            }

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

                accumulator.TotalCustomerCount++;
            }

            foreach (var accumulator in bySupplier.Values)
            {
                int activeCount;
                accumulator.ActiveCustomerCount = activeBySupplier.TryGetValue(accumulator.SupplierId, out activeCount)
                    ? activeCount
                    : 0;

                accumulator.CoveragePercentage = accumulator.TotalCustomerCount > 0
                    ? (decimal)accumulator.ActiveCustomerCount / accumulator.TotalCustomerCount
                    : (decimal?)null;
            }

            var ordered = bySupplier.Values
                .OrderByDescending(accumulator => accumulator.CoveragePercentage ?? -1m)
                .ThenBy(accumulator => accumulator.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((accumulator, index) => new PrincipalCustomerCoverageRow
                {
                    CustomerCoverageKpiId = PrincipalKpiCatalog.CustomerCoverageId,
                    SupplierId = accumulator.SupplierId,
                    SupplierName = accumulator.SupplierName ?? string.Empty,
                    ActiveCustomerCount = accumulator.ActiveCustomerCount,
                    TotalCustomerCount = accumulator.TotalCustomerCount,
                    CoveragePercentage = accumulator.CoveragePercentage,
                    SortOrder = index + 1
                })
                .ToList();

            return new PrincipalCustomerCoverageResult
            {
                CustomerCoverageKpiId = PrincipalKpiCatalog.CustomerCoverageId,
                AsOfDate = asOfDate,
                GeneratedAt = generatedAt,
                Principals = ordered
            };
        }

        private sealed class SupplierAccumulator
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public int ActiveCustomerCount { get; set; }

            public int TotalCustomerCount { get; set; }

            public decimal? CoveragePercentage { get; set; }
        }
    }
}
