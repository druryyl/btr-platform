using btr.application.ReportingContext.PurchasingReportAgg.Queries;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PurchasingReportAgg.Contracts
{
    public interface IPurchasingReportDal
    {
        PurchasingReportResponse GetReport(Periode periode);
    }
}
