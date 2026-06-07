using btr.application.ReportingContext.DashboardInventoryAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardInventoryAgg
{
    public class DashboardInventoryDal : IDashboardInventoryDal
    {
        private readonly IDashboardInventorySnapshotDal _snapshotDal;
        private readonly DashboardInventoryLiveDal _liveDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardInventoryDal(
            IDashboardInventorySnapshotDal snapshotDal,
            DashboardInventoryLiveDal liveDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _liveDal = liveDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardInventoryResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return DashboardInventorySnapshotDal.MapToResponse(snapshot);

            if (_options.AllowLiveFallback)
                return _liveDal.GetSummary();

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }
    }
}
