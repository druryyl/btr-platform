using btr.application.ReportingContext.PurchasingReportAgg.Queries;

namespace btr.application.ReportingContext.PurchasingReportAgg.Contracts
{
    public interface IPurchasingReportDal
    {
        PurchasingReportResponse GetReport();
    }
}
