using btr.application.ReportingContext.DashboardPiutangAgg.Queries;

namespace btr.application.ReportingContext.DashboardPiutangAgg.Contracts
{
    public interface IDashboardPiutangDal
    {
        DashboardPiutangResponse GetSummary();
    }
}
