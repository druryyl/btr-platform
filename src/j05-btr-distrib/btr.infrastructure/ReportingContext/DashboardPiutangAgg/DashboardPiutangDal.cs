using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.Shared;
using AggregateTopCustomerRiskRow = btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardPiutangTopCustomerRiskRow;
using ApiAgingBucket = btr.application.ReportingContext.DashboardPiutangAgg.Queries.DashboardPiutangAgingBucket;
using ApiTopCustomerRiskRow = btr.application.ReportingContext.DashboardPiutangAgg.Queries.DashboardPiutangTopCustomerRiskRow;

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
                OverduePiutang = snapshot.OverduePiutang,
                AgingOver90Amount = snapshot.AgingOver90Amount,
                AgingOver90Percent = snapshot.AgingOver90Percent,
                Top10CustomerConcentrationPercent = snapshot.Top10CustomerConcentrationPercent,
                Top20CustomerConcentrationPercent = snapshot.Top20CustomerConcentrationPercent,
                AgingBuckets = snapshot.AgingBuckets?
                    .Select(b => new ApiAgingBucket
                    {
                        BucketKey = b.BucketKey,
                        BucketLabel = b.BucketLabel,
                        Amount = b.Amount,
                        SortOrder = b.SortOrder
                    })
                    .ToList()
                    ?? new System.Collections.Generic.List<ApiAgingBucket>(),
                TopCustomerRisk = snapshot.TopCustomerRisk?
                    .Select(MapTopCustomerRisk)
                    .ToList()
                    ?? new System.Collections.Generic.List<ApiTopCustomerRiskRow>()
            };
        }

        private static ApiTopCustomerRiskRow MapTopCustomerRisk(
            AggregateTopCustomerRiskRow customer)
        {
            return new ApiTopCustomerRiskRow
            {
                Rank = customer.Rank,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                TotalPiutang = customer.TotalPiutang,
                CurrentAmount = customer.CurrentAmount,
                Aging30Amount = customer.Aging30Amount,
                Aging60Amount = customer.Aging60Amount,
                Aging90Amount = customer.Aging90Amount,
                AgingOver90Amount = customer.AgingOver90Amount,
                Investigation = InvestigationMetadataBuilder.Build(
                    InvestigationRegistry.SignalLegacyTopCustomer,
                    InvestigationMetadataBuilder.EntityTypeCustomer,
                    customer.CustomerCode,
                    customer.CustomerName)
            };
        }
    }
}
