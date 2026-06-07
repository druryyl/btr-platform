using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;

namespace btr.infrastructure.ReportingContext.DashboardSalesAgg
{
    public class DashboardSalesDal : IDashboardSalesDal
    {
        private readonly IDashboardSalesSnapshotDal _snapshotDal;

        public DashboardSalesDal(IDashboardSalesSnapshotDal snapshotDal)
        {
            _snapshotDal = snapshotDal;
        }

        public DashboardSalesResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return DashboardSalesSnapshotDal.MapToResponse(snapshot);

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }
    }
}
