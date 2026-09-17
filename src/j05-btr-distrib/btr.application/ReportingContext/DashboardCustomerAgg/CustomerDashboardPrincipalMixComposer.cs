using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.DashboardCustomerAgg
{
    public static class CustomerDashboardPrincipalMixComposer
    {
        public const string ProjectionNote =
            "Principal mix reads the Customer–Principal relationship projection only. It does not recompute relationships from raw transactions.";

        public const string CustomerLevelNote =
            "Customer total sales, credit, and piutang remain Customer-level and are not allocated to Principals.";

        public const string ProjectionOnlyPrincipalsNote =
            "The mix lists Principals present on that Customer's projection only. No pre-purchase assigned Principal is shown.";

        public static IList<string> CollectRankingCustomerIds(DashboardCustomerResponse customer)
        {
            return CollectRankingCustomers(customer)
                .Select(row => row.CustomerId)
                .ToList();
        }

        public static DashboardCustomerPrincipalMix Compose(
            DashboardCustomerResponse customer,
            CustomerPrincipalRelationshipResult projection)
        {
            var mix = new DashboardCustomerPrincipalMix
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                Note = ProjectionNote,
                Disclosures = new List<string>
                {
                    ProjectionNote,
                    CustomerLevelNote,
                    ProjectionOnlyPrincipalsNote,
                    CustomerPrincipalRelationship.PairSalesOutIsPrincipalSalesOut
                }
            };

            if (projection is null || projection.KpiId != PrincipalKpiCatalog.SalesOutId)
                return mix;

            var pairsByCustomer = (projection.Pairs ?? new List<CustomerPrincipalRelationshipRow>())
                .Where(IsProjectionPrincipal)
                .GroupBy(row => row.CustomerId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            mix.IsAvailable = true;
            mix.Customers = CollectRankingCustomers(customer)
                .Select(row => BuildCustomerMix(row, pairsByCustomer))
                .ToList();
            return mix;
        }

        private static DashboardCustomerPrincipalMixCustomer BuildCustomerMix(
            RankingCustomer customer,
            IReadOnlyDictionary<string, List<CustomerPrincipalRelationshipRow>> pairsByCustomer)
        {
            List<CustomerPrincipalRelationshipRow> pairs;
            if (!pairsByCustomer.TryGetValue(customer.CustomerId, out pairs))
                pairs = new List<CustomerPrincipalRelationshipRow>();

            var principals = pairs
                .OrderByDescending(row => row.SalesOutAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(row => new DashboardCustomerPrincipalMixItem
                {
                    SupplierId = row.SupplierId.Trim(),
                    PrincipalName = row.SupplierName ?? string.Empty,
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PairSalesOutAmount = row.SalesOutAmount
                })
                .ToList();

            var pairTotal = principals.Sum(row => row.PairSalesOutAmount);
            foreach (var principal in principals)
            {
                principal.PercentOfPairSalesOut = pairTotal > 0
                    ? principal.PairSalesOutAmount / pairTotal * 100m
                    : (decimal?)null;
            }

            return new DashboardCustomerPrincipalMixCustomer
            {
                CustomerId = customer.CustomerId,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                Principals = principals
            };
        }

        private static bool IsProjectionPrincipal(CustomerPrincipalRelationshipRow row)
        {
            return row != null
                && !string.IsNullOrWhiteSpace(row.CustomerId)
                && !string.IsNullOrWhiteSpace(row.SupplierId)
                && row.KpiId == PrincipalKpiCatalog.SalesOutId;
        }

        private static IList<RankingCustomer> CollectRankingCustomers(DashboardCustomerResponse customer)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var customers = new List<RankingCustomer>();

            AddRankingCustomers(
                customer?.Rankings?.TopOmzet,
                row => row.CustomerId,
                row => row.CustomerCode,
                row => row.CustomerName,
                seen,
                customers);
            AddRankingCustomers(
                customer?.Rankings?.TopPiutang,
                row => row.CustomerId,
                row => row.CustomerCode,
                row => row.CustomerName,
                seen,
                customers);

            return customers;
        }

        private static void AddRankingCustomers<T>(
            IEnumerable<T> rows,
            Func<T, string> customerId,
            Func<T, string> customerCode,
            Func<T, string> customerName,
            ISet<string> seen,
            IList<RankingCustomer> customers)
        {
            foreach (var row in rows ?? Enumerable.Empty<T>())
            {
                if (row == null)
                    continue;

                var id = customerId(row);
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                id = id.Trim();
                if (!seen.Add(id))
                    continue;

                customers.Add(new RankingCustomer
                {
                    CustomerId = id,
                    CustomerCode = customerCode(row) ?? string.Empty,
                    CustomerName = customerName(row) ?? string.Empty
                });
            }
        }

        private sealed class RankingCustomer
        {
            public string CustomerId { get; set; }

            public string CustomerCode { get; set; }

            public string CustomerName { get; set; }
        }
    }
}
