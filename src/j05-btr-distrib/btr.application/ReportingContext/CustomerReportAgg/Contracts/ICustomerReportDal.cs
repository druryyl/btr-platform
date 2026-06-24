using btr.application.ReportingContext.CustomerReportAgg.Queries;

namespace btr.application.ReportingContext.CustomerReportAgg.Contracts
{
    public interface ICustomerReportDal
    {
        CustomerReportResponse GetReport(string customerCode = null);
    }
}
