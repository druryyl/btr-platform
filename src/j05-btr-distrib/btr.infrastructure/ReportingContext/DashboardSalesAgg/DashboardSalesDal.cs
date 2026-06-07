using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSalesAgg
{
    public class DashboardSalesDal : IDashboardSalesDal
    {
        private readonly IDashboardSalesSnapshotDal _snapshotDal;
        private readonly DashboardSalesLiveDal _liveDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardSalesDal(
            IDashboardSalesSnapshotDal snapshotDal,
            DashboardSalesLiveDal liveDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _liveDal = liveDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardSalesResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return DashboardSalesSnapshotDal.MapToResponse(snapshot);

            if (_options.AllowLiveFallback)
                return _liveDal.GetSummary();

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }
    }
}
