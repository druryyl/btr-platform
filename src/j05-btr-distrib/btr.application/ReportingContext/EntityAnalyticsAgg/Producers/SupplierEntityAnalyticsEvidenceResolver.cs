using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

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

            return new ProfileEvidenceSectionDto
            {
                IsAvailable = true,
                Links = new List<ProfileEvidenceLinkDto>
                {
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Purchasing",
                        Label = "Purchasing Report",
                        ReportRoute = "/reports/purchasing" + query,
                        FilterDimension = "supplierCode"
                    },
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Inventory",
                        Label = "Inventory Report",
                        ReportRoute = "/reports/inventory" + query,
                        FilterDimension = "supplierCode"
                    }
                }
            };
        }

        private static string UriEncode(string value)
        {
            return System.Uri.EscapeDataString(value ?? string.Empty);
        }
    }
}
