using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class ItemEntityAnalyticsEvidenceResolver : IEntityProfileEvidenceResolver
    {
        public string EntityType => EntityTypeCode.Item;

        public ProfileEvidenceSectionDto BuildEvidence(string entityId, EntityIdentity identity)
        {
            var brgCode = identity?.EntityCode ?? entityId;
            if (string.IsNullOrWhiteSpace(brgCode))
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = "NoSnapshotData",
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            var query = $"?brgCode={UriEncode(brgCode)}";

            return new ProfileEvidenceSectionDto
            {
                IsAvailable = true,
                Links = new List<ProfileEvidenceLinkDto>
                {
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Inventory",
                        Label = "Inventory Report",
                        ReportRoute = "/reports/inventory" + query,
                        FilterDimension = "brgCode"
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
