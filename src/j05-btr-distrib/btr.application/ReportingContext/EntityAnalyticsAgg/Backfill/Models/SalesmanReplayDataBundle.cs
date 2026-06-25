using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class SalesmanReplayDataBundle
    {
        public IList<FakturView> FakturRows { get; set; } = new List<FakturView>();

        public IList<CustomerLastFakturWithSalesmanDto> LastFakturRows { get; set; }
            = new List<CustomerLastFakturWithSalesmanDto>();

        public IList<PiutangOpenBalanceWithSalesmanDto> PiutangRows { get; set; }
            = new List<PiutangOpenBalanceWithSalesmanDto>();

        public IList<SalesPersonModel> Salespeople { get; set; } = new List<SalesPersonModel>();

        public IReadOnlyDictionary<string, decimal?> Targets { get; set; }
            = new Dictionary<string, decimal?>();

        public IList<SalesPersonPrincipalTargetModel> PrincipalTargets { get; set; }
            = new List<SalesPersonPrincipalTargetModel>();

        public IReadOnlyList<FakturPrincipalOmzetDto> PrincipalOmzet { get; set; }
            = new List<FakturPrincipalOmzetDto>();

        public IList<SalesmanMtdItemRollupDto> ItemRollupRows { get; set; } = new List<SalesmanMtdItemRollupDto>();
    }
}
