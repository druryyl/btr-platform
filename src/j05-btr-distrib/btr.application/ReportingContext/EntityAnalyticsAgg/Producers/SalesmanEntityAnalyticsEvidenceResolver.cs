using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SalesmanEntityAnalyticsEvidenceResolver : IEntityProfileEvidenceResolver
    {
        public string EntityType => EntityTypeCode.Salesman;

        public ProfileEvidenceSectionDto BuildEvidence(string entityId, EntityIdentity identity)
        {
            var salesPersonCode = identity?.EntityCode ?? entityId;
            if (string.IsNullOrWhiteSpace(salesPersonCode))
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = "NoSnapshotData",
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            var query = $"?salesPersonCode={UriEncode(salesPersonCode)}";

            return new ProfileEvidenceSectionDto
            {
                IsAvailable = true,
                Links = new List<ProfileEvidenceLinkDto>
                {
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Sales",
                        Label = "Sales Report",
                        ReportRoute = "/reports/sales" + query,
                        FilterDimension = "salesPersonCode"
                    },
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Finance",
                        Label = "Piutang Report",
                        ReportRoute = "/reports/piutang" + query,
                        FilterDimension = "salesPersonCode"
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
