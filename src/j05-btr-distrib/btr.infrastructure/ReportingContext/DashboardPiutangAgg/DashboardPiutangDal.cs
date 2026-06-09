using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.Shared;

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
                TopCustomers = snapshot.TopCustomers?
                    .Select(MapTopCustomer)
                    .ToList()
                    ?? new System.Collections.Generic.List<DashboardPiutangTopCustomer>()
            };
        }

        private static DashboardPiutangTopCustomer MapTopCustomer(DashboardPiutangTopCustomer customer)
        {
            return new DashboardPiutangTopCustomer
            {
                Rank = customer.Rank,
                CustomerName = customer.CustomerName,
                CustomerCode = customer.CustomerCode,
                OutstandingBalance = customer.OutstandingBalance,
                Investigation = InvestigationMetadataBuilder.Build(
                    InvestigationRegistry.SignalLegacyTopCustomer,
                    InvestigationMetadataBuilder.EntityTypeCustomer,
                    customer.CustomerCode,
                    customer.CustomerName)
            };
        }
    }
}
