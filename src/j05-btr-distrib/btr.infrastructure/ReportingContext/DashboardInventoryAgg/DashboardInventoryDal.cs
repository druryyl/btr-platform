using btr.application.ReportingContext.DashboardInventoryAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;

namespace btr.infrastructure.ReportingContext.DashboardInventoryAgg
{
    public class DashboardInventoryDal : IDashboardInventoryDal
    {
        private readonly IDashboardInventorySnapshotDal _snapshotDal;

        public DashboardInventoryDal(IDashboardInventorySnapshotDal snapshotDal)
        {
            _snapshotDal = snapshotDal;
        }

        public DashboardInventoryResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return DashboardInventorySnapshotDal.MapToResponse(snapshot);

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }
    }
}
