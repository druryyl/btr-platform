using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Stores Principal x Salesman commercial contribution and missing-target
    /// responsibility exceptions for the current period.
    /// Contribution decomposes PRN-SALES-001 by Faktur.SalesPersonId and never
    /// writes PRN-SALES-001. Contribution itself is not a registry KPI.
    /// A sold Salesman x Principal pair without a target record for the transaction
    /// month remains in the contribution and is listed as a responsibility exception.
    /// </summary>
    public class PrincipalSalesmanContributionComposer
    {
        public PrincipalSalesmanContributionResult Compose(
            IEnumerable<PrincipalContributionEvidenceLine> lines,
            IEnumerable<SalesmanPrincipalTargetEvidence> targets,
            int year,
            int month,
            DateTime generatedAt)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            var responsibilities = IndexResponsibilities(targets, year, month);

            var groups = new Dictionary<string, ContributionGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines ?? Enumerable.Empty<PrincipalContributionEvidenceLine>())
            {
                if (line is null)
                    continue;

                var itemSupplierId = (line.ItemSupplierId ?? string.Empty).Trim();
                if (itemSupplierId.Length == 0)
                    continue;

                var supplierId = (line.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0)
                    continue;

                var salesPersonId = (line.SalesPersonId ?? string.Empty).Trim();
                var key = supplierId.ToUpperInvariant() + "|" + salesPersonId.ToUpperInvariant();
                if (!groups.TryGetValue(key, out var group))
                {
                    group = new ContributionGroup
                    {
                        SupplierId = supplierId,
                        SupplierName = (line.SupplierName ?? string.Empty).Trim(),
                        SalesPersonId = salesPersonId,
                        SalesPersonCode = (line.SalesPersonCode ?? string.Empty).Trim(),
                        SalesPersonName = (line.SalesPersonName ?? string.Empty).Trim()
                    };
                    groups[key] = group;
                }

                group.Amount += line.SubTotal - line.DiscRp;
                group.LineCount++;
                if (group.SupplierName.Length == 0)
                    group.SupplierName = (line.SupplierName ?? string.Empty).Trim();
                if (group.SalesPersonCode.Length == 0)
                    group.SalesPersonCode = (line.SalesPersonCode ?? string.Empty).Trim();
                if (group.SalesPersonName.Length == 0)
                    group.SalesPersonName = (line.SalesPersonName ?? string.Empty).Trim();
            }

            var contributions = groups.Values
                .OrderBy(group => group.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(group => group.Amount)
                .ThenBy(group => group.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .Select((group, index) => new PrincipalSalesmanContributionRow
                {
                    SourceSalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                    SupplierId = group.SupplierId,
                    SupplierName = group.SupplierName,
                    SalesPersonId = group.SalesPersonId,
                    SalesPersonCode = group.SalesPersonCode,
                    SalesPersonName = group.SalesPersonName,
                    ContributionAmount = group.Amount,
                    LineCount = group.LineCount,
                    HasTargetResponsibility = responsibilities.Contains(
                        group.SalesPersonId.ToUpperInvariant() + "|" + group.SupplierId.ToUpperInvariant()),
                    SortOrder = index + 1
                })
                .ToList();

            var exceptions = contributions
                .Where(row => !row.HasTargetResponsibility)
                .OrderBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) => new PrincipalSalesmanContributionExceptionRow
                {
                    SupplierId = row.SupplierId,
                    SupplierName = row.SupplierName,
                    SalesPersonId = row.SalesPersonId,
                    SalesPersonCode = row.SalesPersonCode,
                    SalesPersonName = row.SalesPersonName,
                    ContributionAmount = row.ContributionAmount,
                    LineCount = row.LineCount,
                    TargetYear = year,
                    TargetMonth = month,
                    SortOrder = index + 1
                })
                .ToList();

            return new PrincipalSalesmanContributionResult
            {
                SourceSalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = generatedAt,
                Contributions = contributions,
                Exceptions = exceptions
            };
        }

        private static HashSet<string> IndexResponsibilities(
            IEnumerable<SalesmanPrincipalTargetEvidence> targets,
            int year,
            int month)
        {
            var responsibilities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets ?? Enumerable.Empty<SalesmanPrincipalTargetEvidence>())
            {
                if (target is null)
                    continue;

                if (target.TargetYear != year || target.TargetMonth != month)
                    continue;

                var salesPersonId = (target.SalesPersonId ?? string.Empty).Trim();
                var supplierId = (target.SupplierId ?? string.Empty).Trim();
                if (salesPersonId.Length == 0 || supplierId.Length == 0)
                    continue;

                responsibilities.Add(
                    salesPersonId.ToUpperInvariant() + "|" + supplierId.ToUpperInvariant());
            }

            return responsibilities;
        }

        private sealed class ContributionGroup
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public string SalesPersonId { get; set; }

            public string SalesPersonCode { get; set; }

            public string SalesPersonName { get; set; }

            public decimal Amount { get; set; }

            public int LineCount { get; set; }
        }
    }
}
