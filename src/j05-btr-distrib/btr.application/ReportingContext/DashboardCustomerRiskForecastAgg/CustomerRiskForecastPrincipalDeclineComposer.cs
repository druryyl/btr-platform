using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.DashboardCustomerRiskForecastAgg
{
    public static class CustomerRiskForecastPrincipalDeclineComposer
    {
        public const string ProjectionNote =
            "Principal decline or inactivity reads the Customer–Principal relationship projection only. It does not recompute relationships from raw transactions.";

        public const string CustomerLevelNote =
            "Customer risk, credit, piutang, and decline measures remain Customer-level and are not allocated to Principals.";

        public const string ProjectionOnlyPairsNote =
            "The decline list shows Principals present on that Customer's projection only. No pre-purchase assigned Principal is shown.";

        public static IList<string> CollectRiskCustomerCodes(DashboardCustomerRiskForecastResponse forecast)
        {
            return (forecast?.TopCustomers ?? new List<DashboardCustomerRiskForecastCustomerDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => row.CustomerCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static DashboardCustomerRiskForecastPrincipalDecline Compose(
            DashboardCustomerRiskForecastResponse forecast,
            CustomerPrincipalRelationshipResult projection)
        {
            var decline = new DashboardCustomerRiskForecastPrincipalDecline
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
                return decline;

            var pairsByCode = (projection.Pairs ?? new List<CustomerPrincipalRelationshipRow>())
                .Where(IsProjectionPrincipal)
                .Where(row => !string.IsNullOrWhiteSpace(row.CustomerCode))
                .GroupBy(row => row.CustomerCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            decline.IsAvailable = true;
            decline.Customers = (forecast?.TopCustomers ?? new List<DashboardCustomerRiskForecastCustomerDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.CustomerCode))
                .Select(row => BuildCustomerDecline(row, pairsByCode))
                .ToList();
            return decline;
        }

        private static DashboardCustomerRiskForecastPrincipalDeclineCustomer BuildCustomerDecline(
            DashboardCustomerRiskForecastCustomerDto row,
            IReadOnlyDictionary<string, List<CustomerPrincipalRelationshipRow>> pairsByCode)
        {
            List<CustomerPrincipalRelationshipRow> pairs;
            if (!pairsByCode.TryGetValue(row.CustomerCode.Trim(), out pairs))
                pairs = new List<CustomerPrincipalRelationshipRow>();

            return new DashboardCustomerRiskForecastPrincipalDeclineCustomer
            {
                CustomerCode = row.CustomerCode,
                CustomerName = row.CustomerName,
                Principals = pairs
                    .OrderBy(pair => pair.SupplierId, StringComparer.OrdinalIgnoreCase)
                    .Select(pair => new DashboardCustomerRiskForecastPrincipalDeclinePrincipal
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
