using btr.application.ReportingContext.PiutangReportAgg.Queries;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PiutangReportAgg.Contracts
{
    public interface IPiutangReportDal
    {
        PiutangReportResponse GetReport(Periode periode, PiutangReportDateField dateField);

        PiutangReportResponse GetAllOpenBalancesReport(PiutangReportDateField dateField);
    }
}
