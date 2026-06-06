using btr.application.ReportingContext.SalesReportAgg.Queries;

namespace btr.application.ReportingContext.SalesReportAgg.Contracts
{
    public interface ISalesReportDal
    {
        SalesReportResponse GetReport();
    }
}
