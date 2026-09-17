using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.DashboardCustomerPortfolioAgg
{
    public static class CustomerPortfolioPrincipalMixComposer
    {
        public const string ProjectionNote =
            "Portfolio mix reads the Customer–Principal relationship projection only. It does not recompute relationships from raw transactions.";

        public const string CustomerLevelNote =
            "Customer portfolio measures remain Customer-level and are not allocated to Principals.";

        public const string ProjectionOnlyPairsNote =
            "The mix lists Principals present on that Customer's projection only. No pre-purchase assigned Principal is shown.";

        public static IList<string> CollectPriorityCustomerCodes(DashboardCustomerPortfolioResponse portfolio)
        {
            return (portfolio?.PriorityQueue ?? new List<DashboardCustomerPortfolioPriorityDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => row.CustomerCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static DashboardCustomerPortfolioPrincipalMix Compose(
            DashboardCustomerPortfolioResponse portfolio,
            CustomerPrincipalRelationshipResult projection)
        {
            var mix = new DashboardCustomerPortfolioPrincipalMix
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
                return mix;

            var pairsByCode = (projection.Pairs ?? new List<CustomerPrincipalRelationshipRow>())
                .Where(IsProjectionPrincipal)
                .Where(row => !string.IsNullOrWhiteSpace(row.CustomerCode))
                .GroupBy(row => row.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            mix.IsAvailable = true;
            mix.Customers = (portfolio?.PriorityQueue ?? new List<DashboardCustomerPortfolioPriorityDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => BuildCustomerMix(row, pairsByCode))
                .ToList();
            return mix;
        }

        private static DashboardCustomerPortfolioPrincipalMixCustomer BuildCustomerMix(
            DashboardCustomerPortfolioPriorityDto row,
            IReadOnlyDictionary<string, List<CustomerPrincipalRelationshipRow>> pairsByCode)
        {
            List<CustomerPrincipalRelationshipRow> pairs;
            if (!pairsByCode.TryGetValue(row.CustomerCode.Trim(), out pairs))
                pairs = new List<CustomerPrincipalRelationshipRow>();

            return new DashboardCustomerPortfolioPrincipalMixCustomer
            {
                CustomerCode = row.CustomerCode,
                CustomerName = row.CustomerName,
                Principals = pairs
                    .OrderByDescending(pair => pair.SalesOutAmount)
                    .ThenBy(pair => pair.SupplierId, StringComparer.OrdinalIgnoreCase)
                    .Select(pair => new DashboardCustomerPortfolioPrincipalMixPair
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
