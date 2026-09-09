using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class CustomerPrincipalRelationshipComposer
    {
        public CustomerPrincipalRelationshipResult Compose(
            IEnumerable<CustomerPrincipalRelationshipPairEvidence> pairs,
            DateTime asOfDate,
            DateTime generatedAt)
        {
            var asOf = asOfDate.Date;
            var cutoff = ActiveCutoff(asOf);
            var retained = new Dictionary<string, CustomerPrincipalRelationshipRow>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in pairs ?? Enumerable.Empty<CustomerPrincipalRelationshipPairEvidence>())
            {
                if (pair is null || !IsKnownPair(pair.CustomerId, pair.SupplierId))
                    continue;

                if (pair.FirstTransactionDate >= VoidSentinelDate || pair.LastTransactionDate >= VoidSentinelDate)
                    continue;

                var customerId = pair.CustomerId.Trim();
                var supplierId = pair.SupplierId.Trim();
                var lastTransaction = pair.LastTransactionDate.Date;
                var firstTransaction = pair.FirstTransactionDate.Date;
                if (firstTransaction > lastTransaction)
                    firstTransaction = lastTransaction;

                var key = customerId + "\u001f" + supplierId;
                CustomerPrincipalRelationshipRow existing;
                if (!retained.TryGetValue(key, out existing))
                {
                    retained.Add(key, new CustomerPrincipalRelationshipRow
                    {
                        CustomerId = customerId,
                        CustomerName = (pair.CustomerName ?? string.Empty).Trim(),
                        SupplierId = supplierId,
                        SupplierName = (pair.SupplierName ?? string.Empty).Trim(),
                        FirstTransactionDate = firstTransaction,
                        LastTransactionDate = lastTransaction,
                        KpiId = PrincipalKpiCatalog.SalesOutId,
                        SalesOutAmount = pair.SalesOutAmount,
                        LineCount = pair.LineCount
                    });
                    continue;
                }

                if (firstTransaction < existing.FirstTransactionDate)
                    existing.FirstTransactionDate = firstTransaction;
                if (lastTransaction > existing.LastTransactionDate)
                    existing.LastTransactionDate = lastTransaction;
                if (string.IsNullOrEmpty(existing.CustomerName))
                    existing.CustomerName = (pair.CustomerName ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(existing.SupplierName))
                    existing.SupplierName = (pair.SupplierName ?? string.Empty).Trim();
                existing.SalesOutAmount += pair.SalesOutAmount;
                existing.LineCount += pair.LineCount;
            }

            var ordered = retained.Values
                .Select(row =>
                {
                    row.RelationshipStatus = row.LastTransactionDate.Date >= cutoff
                        ? CustomerPrincipalRelationship.StatusActive
                        : CustomerPrincipalRelationship.StatusDormant;
                    row.KpiId = PrincipalKpiCatalog.SalesOutId;
                    return row;
                })
                .OrderBy(row => row.CustomerId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new CustomerPrincipalRelationshipResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                AsOfDate = asOf,
                HistoricalLimitation = CustomerPrincipalRelationship.HistoricalLimitation,
                GeneratedAt = generatedAt,
                Pairs = ordered
            };
        }

        public static DateTime ActiveCutoff(DateTime asOfDate)
        {
            return asOfDate.Date.AddMonths(-CustomerPrincipalRelationship.ActiveWindowMonths);
        }

        private static bool IsKnownPair(string customerId, string supplierId)
        {
            return !string.IsNullOrWhiteSpace(customerId) && !string.IsNullOrWhiteSpace(supplierId);
        }

        private static readonly DateTime VoidSentinelDate = new DateTime(3000, 1, 1);
    }
}
