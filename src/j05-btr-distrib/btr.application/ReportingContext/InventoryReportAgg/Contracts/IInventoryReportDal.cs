using btr.application.ReportingContext.InventoryReportAgg.Queries;

namespace btr.application.ReportingContext.InventoryReportAgg.Contracts
{
    public interface IInventoryReportDal
    {
        InventoryReportResponse GetReport();
    }
}
