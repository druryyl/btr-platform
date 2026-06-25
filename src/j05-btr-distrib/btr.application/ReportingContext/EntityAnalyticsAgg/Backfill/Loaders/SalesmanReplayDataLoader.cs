using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class SalesmanReplayDataLoader : IEntityAnalyticsReplayDataLoader
    {
        private readonly IFakturViewDal _fakturViewDal;
        private readonly ICustomerLastFakturDal _lastFakturDal;
        private readonly IPiutangOpenBalanceWithSalesmanDal _openBalanceDal;
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly ISalesPersonPrincipalTargetDal _principalTargetDal;
        private readonly IFakturPrincipalOmzetDal _principalOmzetDal;
        private readonly ISalesmanMtdItemRollupDal _salesmanMtdItemRollupDal;

        public SalesmanReplayDataLoader(
            IFakturViewDal fakturViewDal,
            ICustomerLastFakturDal lastFakturDal,
            IPiutangOpenBalanceWithSalesmanDal openBalanceDal,
            ISalesPersonDal salesPersonDal,
            ISalesOmzetTargetDal targetDal,
            ISalesPersonPrincipalTargetDal principalTargetDal,
            IFakturPrincipalOmzetDal principalOmzetDal,
            ISalesmanMtdItemRollupDal salesmanMtdItemRollupDal)
        {
            _fakturViewDal = fakturViewDal;
            _lastFakturDal = lastFakturDal;
            _openBalanceDal = openBalanceDal;
            _salesPersonDal = salesPersonDal;
            _targetDal = targetDal;
            _principalTargetDal = principalTargetDal;
            _principalOmzetDal = principalOmzetDal;
            _salesmanMtdItemRollupDal = salesmanMtdItemRollupDal;
        }

        public string EntityType => EntityTypeCode.Salesman;

        public object Load(EntityAnalyticsReplayContext replayContext)
        {
            if (replayContext is null)
                throw new ArgumentNullException(nameof(replayContext));

            var periodEnd = replayContext.PeriodEnd.Date;
            var periode = new Periode(replayContext.PeriodStart, periodEnd);
            var stepPrefix = $"Backfill:{EntityType}:{replayContext.PeriodYear:D4}-{replayContext.PeriodMonth:D2}";

            WorkerProgressScope.Current?.StepStarted($"{stepPrefix}:Load", "Load historical salesman source data");

            var bundle = new SalesmanReplayDataBundle
            {
                FakturRows = _fakturViewDal.ListData(periode)?.ToList() ?? new System.Collections.Generic.List<FakturView>(),
                LastFakturRows = _lastFakturDal.ListLastFakturWithSalesmanByCustomerAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturWithSalesmanDto>(),
                PiutangRows = _openBalanceDal.ListOpenBalancesAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceWithSalesmanDto>(),
                Salespeople = _salesPersonDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>(),
                Targets = _targetDal.ListTargetsForMonth(periode.Tgl1.Year, periode.Tgl1.Month),
                PrincipalTargets = _principalTargetDal.ListByPeriod(periode.Tgl1.Year, periode.Tgl1.Month)?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.SalesPersonPrincipalTargetAgg.SalesPersonPrincipalTargetModel>(),
                PrincipalOmzet = _principalOmzetDal.ListOmzetBySalesPersonPrincipal(periode),
                ItemRollupRows = _salesmanMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SalesmanMtdItemRollupDto>()
            };

            WorkerProgressScope.Current?.StepCompleted($"{stepPrefix}:Load", new WorkerProgressStepInfo
            {
                RecordCount = bundle.FakturRows.Count + bundle.Salespeople.Count
            });

            return bundle;
        }
    }
}
