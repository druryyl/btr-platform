using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class SupplierRelationshipCatalog
    {
        public const string PackId = "supplier-relationships";
        public const string TopCustomersByOmzet = "TopCustomersByOmzet";
        public const string TopSalesmenByOmzet = "TopSalesmenByOmzet";
        public const string TopProductsByOmzet = "TopProductsByOmzet";

        public static void Register(IRelationshipDefinitionRegistry registry)
        {
            if (registry == null)
                return;

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopCustomersByOmzet,
                DisplayName = "Top Customers",
                TargetEntityType = EntityTypeCode.Customer,
                MetricKpiId = "PU-KPI-001",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopSalesmenByOmzet,
                DisplayName = "Top Salesmen",
                TargetEntityType = EntityTypeCode.Salesman,
                MetricKpiId = "SF-KPI-008",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Supplier, new RelationshipDefinition
            {
                RelationshipCode = TopProductsByOmzet,
                DisplayName = "Top Products",
                TargetEntityType = EntityTypeCode.Item,
                MetricKpiId = "SF-KPI-008",
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
