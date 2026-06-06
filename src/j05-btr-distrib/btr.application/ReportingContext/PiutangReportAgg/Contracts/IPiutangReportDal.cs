using btr.application.ReportingContext.PiutangReportAgg.Queries;

namespace btr.application.ReportingContext.PiutangReportAgg.Contracts
{
    public interface IPiutangReportDal
    {
        PiutangReportResponse GetReport();
    }
}
