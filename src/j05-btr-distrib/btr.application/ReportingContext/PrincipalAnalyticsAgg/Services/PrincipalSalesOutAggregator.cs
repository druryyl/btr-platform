using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class PrincipalSalesOutAggregator
    {
        public PrincipalSalesOutAggregateResult Aggregate(
            IEnumerable<PrincipalSalesOutFakturItemEvidence> lines,
            Periode periode,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var periodStart = periode.Tgl1.Date;
            var known = new List<KnownLine>();
            var blankAmount = 0m;
            var blankCount = 0;
            var unknownAmount = 0m;
            var unknownCount = 0;

            foreach (var line in lines ?? Enumerable.Empty<PrincipalSalesOutFakturItemEvidence>())
            {
                if (line is null)
                    continue;

                var amount = line.SubTotal - line.DiscRp;
                var itemSupplierId = (line.ItemSupplierId ?? string.Empty).Trim();
                var supplierId = (line.SupplierId ?? string.Empty).Trim();

                if (itemSupplierId.Length == 0)
                {
                    blankAmount += amount;
                    blankCount++;
                    continue;
                }

                if (supplierId.Length == 0)
                {
                    unknownAmount += amount;
                    unknownCount++;
                    continue;
                }

                known.Add(new KnownLine
                {
                    SupplierId = supplierId,
                    SupplierName = (line.SupplierName ?? string.Empty).Trim(),
                    Amount = amount
                });
            }

            var principals = known
                .GroupBy(line => line.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PrincipalSalesOutRow
                {
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    SupplierId = group.Key,
                    SupplierName = group
                        .Select(line => line.SupplierName)
                        .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty,
                    SalesOutAmount = group.Sum(line => line.Amount),
                    LineCount = group.Count()
                })
                .OrderByDescending(row => row.SalesOutAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            var dataQuality = new List<PrincipalSalesOutDataQualityRow>();
            if (blankCount > 0)
            {
                dataQuality.Add(new PrincipalSalesOutDataQualityRow
                {
                    ExceptionCode = PrincipalSalesOutSnapshot.BlankSupplierExceptionCode,
                    Amount = blankAmount,
                    LineCount = blankCount
                });
            }

            if (unknownCount > 0)
            {
                dataQuality.Add(new PrincipalSalesOutDataQualityRow
                {
                    ExceptionCode = PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode,
                    Amount = unknownAmount,
                    LineCount = unknownCount
                });
            }

            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                GeneratedAt = generatedAt,
                Principals = principals,
                DataQuality = dataQuality
            };
        }

        private sealed class KnownLine
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal Amount { get; set; }
        }
    }
}
