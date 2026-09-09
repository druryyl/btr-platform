using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.CustomerReportAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.CustomerReportAgg
{
    public static class CustomerReportPrincipalPairComposer
    {
        public const string ProjectionNote =
            "Pair evidence reads the Customer–Principal relationship projection only. It does not recompute relationships from raw transactions.";

        public const string CustomerLevelNote =
            "Customer totals remain Customer-level and are not allocated to Principals.";

        public const string ProjectionOnlyPairsNote =
            "The pair list shows Principals present on that Customer's projection only. No pre-purchase assigned Principal is shown.";

        public static IList<string> CollectReportCustomerCodes(CustomerReportResponse report)
        {
            return (report?.Rows ?? new List<CustomerReportRowDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => row.CustomerCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static CustomerReportPrincipalPairEvidence Compose(
            CustomerReportResponse report,
            CustomerPrincipalRelationshipResult projection)
        {
            var evidence = new CustomerReportPrincipalPairEvidence
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                Note = ProjectionNote,
                Disclosures = new List<string>
                {
                    ProjectionNote,
                    CustomerLevelNote,
                    ProjectionOnlyPairsNote,
                    CustomerPrincipalRelationship.PairSalesOutIsPrincipalSalesOut
                }
            };

            if (projection is null || projection.KpiId != PrincipalKpiCatalog.SalesOutId)
                return evidence;

            var pairsByCode = (projection.Pairs ?? new List<CustomerPrincipalRelationshipRow>())
                .Where(IsProjectionPrincipal)
                .Where(row => !string.IsNullOrWhiteSpace(row.CustomerCode))
                .GroupBy(row => row.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            evidence.IsAvailable = true;
            evidence.Customers = (report?.Rows ?? new List<CustomerReportRowDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => BuildCustomerPairs(row, pairsByCode))
                .ToList();
            return evidence;
        }

        private static CustomerReportPrincipalPairCustomer BuildCustomerPairs(
            CustomerReportRowDto row,
            IReadOnlyDictionary<string, List<CustomerPrincipalRelationshipRow>> pairsByCode)
        {
            List<CustomerPrincipalRelationshipRow> pairs;
            if (!pairsByCode.TryGetValue(row.CustomerCode.Trim(), out pairs))
                pairs = new List<CustomerPrincipalRelationshipRow>();

            return new CustomerReportPrincipalPairCustomer
            {
                CustomerCode = row.CustomerCode,
                CustomerName = row.CustomerName,
                Principals = pairs
                    .OrderByDescending(pair => pair.SalesOutAmount)
                    .ThenBy(pair => pair.SupplierId, StringComparer.OrdinalIgnoreCase)
                    .Select(pair => new CustomerReportPrincipalPair
                    {
                        SupplierId = pair.SupplierId.Trim(),
                        PrincipalName = pair.SupplierName ?? string.Empty,
                        RelationshipStatus = pair.RelationshipStatus ?? string.Empty,
                        KpiId = PrincipalKpiCatalog.SalesOutId,
                        PairSalesOutAmount = pair.SalesOutAmount
                    })
                    .ToList()
            };
        }

        private static bool IsProjectionPrincipal(CustomerPrincipalRelationshipRow row)
        {
            return row != null
                && !string.IsNullOrWhiteSpace(row.CustomerId)
                && !string.IsNullOrWhiteSpace(row.SupplierId)
                && row.KpiId == PrincipalKpiCatalog.SalesOutId;
        }
    }
}
