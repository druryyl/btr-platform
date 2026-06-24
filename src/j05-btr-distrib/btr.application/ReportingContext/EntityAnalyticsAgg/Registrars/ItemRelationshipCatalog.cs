using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class ItemRelationshipCatalog
    {
        public const string PackId = "item-relationships";
        public const string TopCustomersByOmzet = "TopCustomersByOmzet";
        public const string TopSalesmenByOmzet = "TopSalesmenByOmzet";
        public const string PrimarySupplier = "PrimarySupplier";

        public static void Register(IRelationshipDefinitionRegistry registry)
        {
            if (registry == null)
                return;

            registry.Register(EntityTypeCode.Item, new RelationshipDefinition
            {
                RelationshipCode = TopCustomersByOmzet,
                DisplayName = "Top Customers",
                TargetEntityType = EntityTypeCode.Customer,
                MetricKpiId = "SF-KPI-008",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Item, new RelationshipDefinition
            {
                RelationshipCode = TopSalesmenByOmzet,
                DisplayName = "Top Salesmen",
                TargetEntityType = EntityTypeCode.Salesman,
                MetricKpiId = "SF-KPI-008",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Item, new RelationshipDefinition
            {
                RelationshipCode = PrimarySupplier,
                DisplayName = "Primary Supplier",
                TargetEntityType = EntityTypeCode.Supplier,
                PeriodSemantics = "PointInTime",
                TopN = 1
            });

            registry.RegisterPack(PackId, new List<string>
            {
                TopCustomersByOmzet,
                TopSalesmenByOmzet,
                PrimarySupplier
            });
        }
    }
}
