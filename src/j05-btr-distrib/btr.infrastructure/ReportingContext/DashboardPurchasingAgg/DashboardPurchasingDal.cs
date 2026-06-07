using btr.application.ReportingContext.DashboardPurchasingAgg.Contracts;
using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;

namespace btr.infrastructure.ReportingContext.DashboardPurchasingAgg
{
    public class DashboardPurchasingDal : IDashboardPurchasingDal
    {
        private readonly IDashboardPurchasingSnapshotDal _snapshotDal;

        public DashboardPurchasingDal(IDashboardPurchasingSnapshotDal snapshotDal)
        {
            _snapshotDal = snapshotDal;
        }

        public DashboardPurchasingResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return DashboardPurchasingSnapshotDal.MapToResponse(snapshot);

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }
    }
}
