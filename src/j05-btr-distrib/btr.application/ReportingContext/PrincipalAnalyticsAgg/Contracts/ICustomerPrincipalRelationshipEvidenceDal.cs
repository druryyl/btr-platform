using System.Collections.Generic;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface ICustomerPrincipalRelationshipEvidenceDal
    {
        IReadOnlyList<CustomerPrincipalRelationshipPairEvidence> ListHistoricalPairEvidence();
    }
}
