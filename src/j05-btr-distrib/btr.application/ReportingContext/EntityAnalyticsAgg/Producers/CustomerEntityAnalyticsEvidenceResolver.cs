using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class CustomerEntityAnalyticsEvidenceResolver : IEntityProfileEvidenceResolver
    {
        public string EntityType => EntityTypeCode.Customer;

        public ProfileEvidenceSectionDto BuildEvidence(string entityId, EntityIdentity identity)
        {
            var customerCode = identity?.EntityCode ?? entityId;
            if (string.IsNullOrWhiteSpace(customerCode))
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = "NoSnapshotData",
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            var query = $"?customerCode={UriEncode(customerCode)}";

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
                        FilterDimension = "customerCode"
                    },
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Finance",
                        Label = "Piutang Report",
                        ReportRoute = "/reports/piutang" + query,
                        FilterDimension = "customerCode"
                    },
                    new ProfileEvidenceLinkDto
                    {
                        Category = "Portfolio",
                        Label = "Customer Report",
                        ReportRoute = "/reports/customers" + query,
                        FilterDimension = "customerCode"
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
