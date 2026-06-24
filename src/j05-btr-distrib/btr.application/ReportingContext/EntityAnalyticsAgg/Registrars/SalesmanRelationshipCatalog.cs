using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class SalesmanRelationshipCatalog
    {
        public const string PackId = "salesman-relationships";
        public const string ManagedCustomers = "ManagedCustomers";
        public const string TopCustomersByOmzet = "TopCustomersByOmzet";
        public const string TopPrincipalsByOmzet = "TopPrincipalsByOmzet";
        public const string TopItemsByOmzet = "TopItemsByOmzet";

        public static void Register(IRelationshipDefinitionRegistry registry)
        {
            if (registry == null)
                return;

            registry.Register(EntityTypeCode.Salesman, new RelationshipDefinition
            {
                RelationshipCode = ManagedCustomers,
                DisplayName = "Assigned Customers",
                TargetEntityType = EntityTypeCode.Customer,
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Salesman, new RelationshipDefinition
            {
                RelationshipCode = TopCustomersByOmzet,
                DisplayName = "Top Customers",
                TargetEntityType = EntityTypeCode.Customer,
                MetricKpiId = "SF-KPI-008",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Salesman, new RelationshipDefinition
            {
                RelationshipCode = TopPrincipalsByOmzet,
                DisplayName = "Top Principals",
                TargetEntityType = EntityTypeCode.Supplier,
                MetricKpiId = "SF-KPI-011",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Salesman, new RelationshipDefinition
            {
                RelationshipCode = TopItemsByOmzet,
                DisplayName = "Top Products",
                TargetEntityType = EntityTypeCode.Item,
                MetricKpiId = "SF-KPI-008",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.RegisterPack(PackId, new List<string>
            {
                ManagedCustomers,
                TopCustomersByOmzet,
                TopPrincipalsByOmzet,
                TopItemsByOmzet
            });
        }
    }
}
