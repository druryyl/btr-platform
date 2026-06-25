using System.Collections.Generic;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class CustomerReplayDataBundle
    {
        public IList<FakturView> FakturRows { get; set; } = new List<FakturView>();

        public IList<CustomerOmzetHistoryDto> OmzetHistoryRows { get; set; } = new List<CustomerOmzetHistoryDto>();

        public IList<CustomerLastFakturDto> LastFakturRows { get; set; } = new List<CustomerLastFakturDto>();

        public IList<PiutangOpenBalanceDto> PiutangRows { get; set; } = new List<PiutangOpenBalanceDto>();

        public IList<CustomerModel> Customers { get; set; } = new List<CustomerModel>();

        public IList<CustomerPelunasanSummaryDto> PelunasanSummaryRows { get; set; } = new List<CustomerPelunasanSummaryDto>();

        public IList<CustomerPaymentBehaviorDto> PaymentBehaviorRows { get; set; } = new List<CustomerPaymentBehaviorDto>();

        public IList<CustomerLastFakturWithSalesmanDto> LastFakturWithSalesman { get; set; }
            = new List<CustomerLastFakturWithSalesmanDto>();

        public IList<CustomerFirstFakturDto> FirstFakturRows { get; set; } = new List<CustomerFirstFakturDto>();

        public IList<CustomerPurchaseFrequencyDto> FrequencyRows { get; set; } = new List<CustomerPurchaseFrequencyDto>();

        public IList<CustomerMtdItemRollupDto> ItemRollupRows { get; set; } = new List<CustomerMtdItemRollupDto>();
    }
}
