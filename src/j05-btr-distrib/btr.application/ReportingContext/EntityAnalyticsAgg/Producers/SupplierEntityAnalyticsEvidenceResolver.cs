using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SupplierEntityAnalyticsEvidenceResolver : IEntityProfileEvidenceResolver
    {
        public string EntityType => EntityTypeCode.Supplier;

        public ProfileEvidenceSectionDto BuildEvidence(string entityId, EntityIdentity identity)
        {
            var supplierCode = identity?.EntityCode ?? entityId;
            if (string.IsNullOrWhiteSpace(supplierCode))
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = "NoSnapshotData",
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            var query = $"?supplierCode={UriEncode(supplierCode)}";
            var links = new List<ProfileEvidenceLinkDto>
            {
                CreatePrincipalSalesOutLink(identity, entityId, null, "Faktur Item evidence"),
                CreatePrincipalReturnLink(identity, entityId, "Return Item evidence")
            };

            links.Add(ResolveOmzetRelationshipEvidence(
                SupplierRelationshipCatalog.TopCustomersByOmzet, entityId, identity));
            links.Add(ResolveOmzetRelationshipEvidence(
                SupplierRelationshipCatalog.TopSalesmenByOmzet, entityId, identity));
            links.Add(ResolveOmzetRelationshipEvidence(
                SupplierRelationshipCatalog.TopProductsByOmzet, entityId, identity));

            links.Add(new ProfileEvidenceLinkDto
            {
                Category = "Purchasing",
                Label = "Purchasing Report",
                ReportRoute = "/reports/purchasing" + query,
                FilterDimension = "supplierCode"
            });
            links.Add(new ProfileEvidenceLinkDto
            {
                Category = "Inventory",
                Label = "Inventory Report",
                ReportRoute = "/reports/inventory" + query,
                FilterDimension = "supplierCode"
            });

            return new ProfileEvidenceSectionDto
            {
                IsAvailable = true,
                Links = links
            };
        }

        public ProfileEvidenceLinkDto ResolveOmzetRelationshipEvidence(
            string relationshipCode,
            string entityId,
            EntityIdentity identity)
        {
            if (!SupplierRelationshipCatalog.IsSalesOmzetRelationship(relationshipCode))
                return null;

            return CreatePrincipalSalesOutLink(
                identity,
                entityId,
                relationshipCode,
                ResolveRelationshipEvidenceLabel(relationshipCode));
        }

        private static ProfileEvidenceLinkDto CreatePrincipalSalesOutLink(
            EntityIdentity identity,
            string entityId,
            string relationshipCode,
            string label)
        {
            var supplierId = identity?.EntityId;
            if (string.IsNullOrWhiteSpace(supplierId))
                supplierId = entityId;

            var route = SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute
                + $"?{SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceFilterDimension}={UriEncode(supplierId)}";
            if (!string.IsNullOrWhiteSpace(relationshipCode))
                route += "&relationshipCode=" + UriEncode(relationshipCode);

            return new ProfileEvidenceLinkDto
            {
                Category = "Financial",
                Label = label,
                ReportRoute = route,
                FilterDimension = SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceFilterDimension,
                RelationshipCode = relationshipCode,
                MetricKpiId = PrincipalKpiCatalog.SalesOutId
            };
        }

        private static ProfileEvidenceLinkDto CreatePrincipalReturnLink(
            EntityIdentity identity,
            string entityId,
            string label)
        {
            var supplierId = identity?.EntityId;
            if (string.IsNullOrWhiteSpace(supplierId))
                supplierId = entityId;

            var route = SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceRoute
                + $"?{SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceFilterDimension}={UriEncode(supplierId)}";

            return new ProfileEvidenceLinkDto
            {
                Category = "Quality",
                Label = label,
                ReportRoute = route,
                FilterDimension = SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceFilterDimension,
                MetricKpiId = PrincipalKpiCatalog.TotalReturnAmountId
            };
        }

        private static string ResolveRelationshipEvidenceLabel(string relationshipCode)
        {
            if (string.Equals(relationshipCode, SupplierRelationshipCatalog.TopCustomersByOmzet, System.StringComparison.OrdinalIgnoreCase))
                return "Top Customers omzet evidence";

            if (string.Equals(relationshipCode, SupplierRelationshipCatalog.TopSalesmenByOmzet, System.StringComparison.OrdinalIgnoreCase))
                return "Top Salesmen omzet evidence";

            return "Top Products omzet evidence";
        }

        private static string UriEncode(string value)
        {
            return System.Uri.EscapeDataString(value ?? string.Empty);
        }
    }
}
