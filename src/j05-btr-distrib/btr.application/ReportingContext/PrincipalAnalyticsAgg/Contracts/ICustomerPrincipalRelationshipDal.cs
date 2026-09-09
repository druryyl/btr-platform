using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface ICustomerPrincipalRelationshipDal
    {
        CustomerPrincipalRelationshipResult GetProjection();

        void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId);
    }
}
