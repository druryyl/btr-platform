using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCollectionAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardCollectionAgg.Queries
{
    public class GetDashboardCollectionQuery : IRequest<DashboardCollectionResponse>
    {
    }

    public class DashboardCollectionResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public DashboardCollectionAttentionCards AttentionCards { get; set; }

        public DashboardCollectionRecoverySummary RecoverySummary { get; set; }

        public IList<DashboardCollectionAgingBucket> AgingRiskSummary { get; set; }
            = new List<DashboardCollectionAgingBucket>();

        public IList<DashboardCollectionAttentionItem> AttentionList { get; set; }
            = new List<DashboardCollectionAttentionItem>();

        public IList<DashboardCollectionRankingRow> TopOverdueCustomers { get; set; }
            = new List<DashboardCollectionRankingRow>();

        public IList<DashboardCollectionRankingRow> TopOverdueSalesmen { get; set; }
            = new List<DashboardCollectionRankingRow>();

        public IList<DashboardCollectionRankingRow> TopOverdueWilayah { get; set; }
            = new List<DashboardCollectionRankingRow>();

        public DashboardCollectionNavigationLinks Navigation { get; set; }
    }

    public class DashboardCollectionAttentionCards
    {
        public decimal OverdueExposure { get; set; }

        public decimal AgingOver90Exposure { get; set; }

        public decimal? OverdueConcentrationPercent { get; set; }

        public bool ExposureRequiresAttention { get; set; }

        public decimal CashCollectedMtd { get; set; }

        public decimal? RecoveryVsBillingPercent { get; set; }

        public bool RecoveryRequiresAttention { get; set; }

        public int LegacyDebtCount { get; set; }

        public bool PortfolioRequiresAttention { get; set; }
    }

    public class DashboardCollectionRecoverySummary
    {
        public decimal CashCollectedMtd { get; set; }

        public decimal? RecoveryVsBillingPercent { get; set; }

        public decimal PaymentMixCashAmount { get; set; }

        public decimal PaymentMixGiroAmount { get; set; }

        public decimal PaymentMixAdjustmentAmount { get; set; }

        public decimal? PaymentMixCashPercent { get; set; }

        public decimal? PaymentMixGiroPercent { get; set; }

        public decimal? PaymentMixAdjustmentPercent { get; set; }
    }

    public class DashboardCollectionAgingBucket
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCollectionAttentionItem
    {
        public string EntityType { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public string ReportRoute { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardCollectionRankingRow
    {
        public int Rank { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardCollectionNavigationLinks
    {
        public string PiutangDashboardRoute { get; set; }

        public string CustomerDashboardRoute { get; set; }

        public string SalesmanDashboardRoute { get; set; }

        public string PiutangReportRoute { get; set; }
    }

    public class GetDashboardCollectionHandler
        : IRequestHandler<GetDashboardCollectionQuery, DashboardCollectionResponse>
    {
        private readonly IDashboardCollectionDal _dal;

        public GetDashboardCollectionHandler(IDashboardCollectionDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCollectionResponse> Handle(
            GetDashboardCollectionQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
