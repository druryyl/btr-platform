using System;
using System.Diagnostics;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardSalesmanSnapshotWorker
        : INunaServiceVoid<RefreshDashboardSalesmanSnapshotRequest>
    {
    }

    public class RefreshDashboardSalesmanSnapshotWorker : IRefreshDashboardSalesmanSnapshotWorker
    {
        private const string Domain = "Salesman";
        private const int MaxErrorMessageLength = 500;

        private readonly IFakturViewDal _fakturViewDal;
        private readonly ICustomerLastFakturDal _lastFakturDal;
        private readonly IPiutangOpenBalanceWithSalesmanDal _openBalanceDal;
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly ISalesPersonPrincipalTargetDal _principalTargetDal;
        private readonly IFakturPrincipalOmzetDal _principalOmzetDal;
        private readonly DashboardSalesmanAggregator _aggregator;
        private readonly DashboardSalesmanRelationshipAggregator _relationshipAggregator;
        private readonly ISalesmanMtdItemRollupDal _salesmanMtdItemRollupDal;
        private readonly IDashboardSalesmanSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly DashboardSnapshotOptions _options;
        private readonly EntityAnalyticsProducerOrchestrator _entityAnalyticsOrchestrator;
        private readonly ICustomerDal _customerDal;

        public RefreshDashboardSalesmanSnapshotWorker(
            IFakturViewDal fakturViewDal,
            ICustomerLastFakturDal lastFakturDal,
            IPiutangOpenBalanceWithSalesmanDal openBalanceDal,
            ISalesPersonDal salesPersonDal,
            ISalesOmzetTargetDal targetDal,
            ISalesPersonPrincipalTargetDal principalTargetDal,
            IFakturPrincipalOmzetDal principalOmzetDal,
            DashboardSalesmanAggregator aggregator,
            DashboardSalesmanRelationshipAggregator relationshipAggregator,
            ISalesmanMtdItemRollupDal salesmanMtdItemRollupDal,
            IDashboardSalesmanSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider,
            DashboardSnapshotOptions options,
            EntityAnalyticsProducerOrchestrator entityAnalyticsOrchestrator,
            ICustomerDal customerDal)
        {
            _fakturViewDal = fakturViewDal;
            _lastFakturDal = lastFakturDal;
            _openBalanceDal = openBalanceDal;
            _salesPersonDal = salesPersonDal;
            _targetDal = targetDal;
            _principalTargetDal = principalTargetDal;
            _principalOmzetDal = principalOmzetDal;
            _aggregator = aggregator;
            _relationshipAggregator = relationshipAggregator;
            _salesmanMtdItemRollupDal = salesmanMtdItemRollupDal;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
            _options = options ?? new DashboardSnapshotOptions();
            _entityAnalyticsOrchestrator = entityAnalyticsOrchestrator;
            _customerDal = customerDal;
        }

        public void Execute(RefreshDashboardSalesmanSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;

            WorkerProgressScope.Current.StepStarted($"{Domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{Domain}:Initialize");

            try
            {
                var today = _businessDateProvider.Today;
                var periode = CurrentMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;
                const int loadSteps = 8;

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load faktur", 1, loadSteps);
                var fakturRows = _fakturViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<FakturView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by customer", 2, loadSteps);
                var lastFakturRows = _lastFakturDal.ListLastFakturWithSalesmanByCustomer()?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturWithSalesmanDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load open balances", 3, loadSteps);
                var piutangRows = _openBalanceDal.ListOpenBalances()?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceWithSalesmanDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load salespeople", 4, loadSteps);
                var salespeople = _salesPersonDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load targets", 5, loadSteps);
                var targets = _targetDal.ListTargetsForMonth(periode.Tgl1.Year, periode.Tgl1.Month);
                var targetCount = targets == null ? 0 : System.Linq.Enumerable.Count(targets);

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load principal targets", 6, loadSteps);
                var principalTargets = _principalTargetDal.ListByPeriod(periode.Tgl1.Year, periode.Tgl1.Month)?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.SalesPersonPrincipalTargetAgg.SalesPersonPrincipalTargetModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load principal omzet", 7, loadSteps);
                var principalOmzet = _principalOmzetDal.ListOmzetBySalesPersonPrincipal(periode);

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load item rollups", 8, loadSteps);
                var itemRollupRows = _salesmanMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SalesmanMtdItemRollupDto>();

                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = fakturRows.Count + lastFakturRows.Count + piutangRows.Count + salespeople.Count + targetCount + principalTargets.Count + principalOmzet.Count() + itemRollupRows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(
                    fakturRows,
                    piutangRows,
                    lastFakturRows,
                    salespeople,
                    targets,
                    periode,
                    today,
                    generatedAt,
                    _options.SalesmanExposureTopPercent,
                    principalTargets,
                    principalOmzet);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateRelationships", "Aggregate salesman relationship rollups");
                var relationshipAggregate = _relationshipAggregator.Aggregate(itemRollupRows, today, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateRelationships");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                var customers = _customerDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.CustomerAgg.CustomerModel>();
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);

                    WorkerProgressScope.Current.StepStarted($"{Domain}:EntityAnalytics", "Produce entity analytics L0+L1+L2+L3+L4+L5 snapshot");
                    _entityAnalyticsOrchestrator.ProduceForDomain(Domain, new EntityAnalyticsProduceContext
                    {
                        RefreshLogId = refreshLogId,
                        GeneratedAt = generatedAt,
                        BusinessDate = today,
                        CustomerIdentityLookup = EntityAnalyticsCustomerIdentityResolver.BuildLookup(customers),
                        DomainInput = new SalesmanEntityAnalyticsProduceInput
                        {
                            SalesmanAggregate = aggregate,
                            RelationshipAggregate = relationshipAggregate
                        }
                    });
                    WorkerProgressScope.Current.StepCompleted($"{Domain}:EntityAnalytics");

                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardSalesmanSnapshotResult
                {
                    RefreshLogId = refreshLogId,
                    DurationMs = (int)sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                var message = ex.Message ?? ex.GetType().Name;
                if (message.Length > MaxErrorMessageLength)
                    message = message.Substring(0, MaxErrorMessageLength);

                _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                WorkerProgressScope.Current.StepFailed($"{Domain}:Execute", message);
                throw;
            }
        }

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
