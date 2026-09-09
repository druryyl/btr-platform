using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class SupplierRelationshipCatalog
    {
        public const string PackId = "supplier-relationships";
        public const string TopCustomersByOmzet = "TopCustomersByOmzet";
        public const string TopSalesmenByOmzet = "TopSalesmenByOmzet";
        public const string TopProductsByOmzet = "TopProductsByOmzet";

        public const string SalesOmzetMetricKpiId = PrincipalKpiCatalog.SalesOutId;

        public static bool IsSalesOmzetRelationship(string relationshipCode)
        {
            return string.Equals(relationshipCode, TopCustomersByOmzet, StringComparison.OrdinalIgnoreCase)
                || string.Equals(relationshipCode, TopSalesmenByOmzet, StringComparison.OrdinalIgnoreCase)
                || string.Equals(relationshipCode, TopProductsByOmzet, StringComparison.OrdinalIgnoreCase);
        }

        public static void Register(IRelationshipDefinitionRegistry registry)
        {
            if (registry == null)
                return;

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopCustomersByOmzet,
                DisplayName = "Top Customers",
                TargetEntityType = EntityTypeCode.Customer,
                MetricKpiId = SalesOmzetMetricKpiId,
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopSalesmenByOmzet,
                DisplayName = "Top Salesmen",
                TargetEntityType = EntityTypeCode.Salesman,
                MetricKpiId = SalesOmzetMetricKpiId,
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopProductsByOmzet,
                DisplayName = "Top Products",
                TargetEntityType = EntityTypeCode.Item,
                MetricKpiId = SalesOmzetMetricKpiId,
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.RegisterPack(PackId, new List<string>
            {
                TopCustomersByOmzet,
                TopSalesmenByOmzet,
                TopProductsByOmzet
            });
        }
    }
}
