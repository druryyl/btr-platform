using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.infrastructure.ReportingContext.DashboardPiutangAgg
{
    public class DashboardPiutangDal : IDashboardPiutangDal
    {
        private readonly IDashboardPiutangSnapshotDal _snapshotDal;

        public DashboardPiutangDal(IDashboardPiutangSnapshotDal snapshotDal)
        {
            _snapshotDal = snapshotDal;
        }

        public DashboardPiutangResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot != null)
                return MapToResponse(snapshot);

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
