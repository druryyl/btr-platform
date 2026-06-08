using btr.application.ReportingContext.SalesReportAgg.Queries;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.SalesReportAgg.Contracts
{
    public interface ISalesReportDal
    {
        SalesReportResponse GetReport(Periode periode);
    }
}
