using System.Collections.Generic;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface ICustomerPrincipalRelationshipDal
    {
        CustomerPrincipalRelationshipResult GetProjection();

        CustomerPrincipalRelationshipResult ListPairsForCustomers(IEnumerable<string> customerIds);

        void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId);
    }
}
