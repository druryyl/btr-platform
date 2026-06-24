using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class CustomerRelationshipCatalog
    {
        public const string PackId = "customer-relationships";
        public const string AssignedSalesman = "AssignedSalesman";
        public const string TopItemsByOmzet = "TopItemsByOmzet";
        public const string TopPrincipalsByOmzet = "TopPrincipalsByOmzet";

        public static void Register(IRelationshipDefinitionRegistry registry)
        {
            if (registry == null)
                return;

            registry.Register(EntityTypeCode.Customer, new RelationshipDefinition
            {
                RelationshipCode = AssignedSalesman,
                DisplayName = "Assigned Salesman",
                TargetEntityType = EntityTypeCode.Salesman,
                PeriodSemantics = "MTD",
                TopN = 1
            });

            registry.Register(EntityTypeCode.Customer, new RelationshipDefinition
            {
                RelationshipCode = TopItemsByOmzet,
                DisplayName = "Top Items",
                TargetEntityType = EntityTypeCode.Item,
                MetricKpiId = "CU-KPI-009",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.Register(EntityTypeCode.Customer, new RelationshipDefinition
            {
                RelationshipCode = TopPrincipalsByOmzet,
                DisplayName = "Top Principals",
                TargetEntityType = EntityTypeCode.Supplier,
                MetricKpiId = "CU-KPI-009",
                PeriodSemantics = "MTD",
                TopN = 10
            });

            registry.RegisterPack(PackId, new List<string>
            {
                AssignedSalesman,
                TopItemsByOmzet,
                TopPrincipalsByOmzet
            });
        }
    }
}
