using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardPiutangAgg
{
    public class DashboardPiutangDal : IDashboardPiutangDal
    {
        private readonly IDashboardPiutangSnapshotDal _snapshotDal;
        private readonly DashboardPiutangLiveDal _liveDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardPiutangDal(
            IDashboardPiutangSnapshotDal snapshotDal,
            DashboardPiutangLiveDal liveDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _liveDal = liveDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardPiutangResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return MapToResponse(snapshot);

            if (_options.AllowLiveFallback)
                return _liveDal.GetSummary();

            throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
        }

        private static DashboardPiutangResponse MapToResponse(DashboardPiutangAggregateResult snapshot)
        {
            return new DashboardPiutangResponse
            {
                TotalPiutang = snapshot.TotalPiutang,
                TotalCustomer = snapshot.TotalCustomer,
                GeneratedAt = snapshot.GeneratedAt,
                OverdueCustomer = snapshot.OverdueCustomer,
                AgingBuckets = snapshot.AgingBuckets?.ToList()
                    ?? new System.Collections.Generic.List<DashboardPiutangAgingBucket>(),
                TopCustomers = snapshot.TopCustomers?.ToList()
                    ?? new System.Collections.Generic.List<DashboardPiutangTopCustomer>()
            };
        }
    }
}
