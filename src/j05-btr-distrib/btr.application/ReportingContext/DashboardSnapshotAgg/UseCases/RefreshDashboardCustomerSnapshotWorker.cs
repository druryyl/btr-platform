using System;
using System.Diagnostics;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardCustomerSnapshotWorker
        : INunaServiceVoid<RefreshDashboardCustomerSnapshotRequest>
    {
    }

    public class RefreshDashboardCustomerSnapshotWorker : IRefreshDashboardCustomerSnapshotWorker
    {
        private const string Domain = "Customer";
        private const int MaxErrorMessageLength = 500;

        private readonly IFakturViewDal _fakturViewDal;
        private readonly ICustomerLastFakturDal _lastFakturDal;
        private readonly IPiutangOpenBalanceDal _openBalanceDal;
        private readonly ICustomerDal _customerDal;
        private readonly ICustomerOmzetHistoryDal _customerOmzetHistoryDal;
        private readonly ICustomerPelunasanSummaryDal _customerPelunasanSummaryDal;
        private readonly ICustomerPaymentBehaviorDal _customerPaymentBehaviorDal;
        private readonly DashboardCustomerAggregator _aggregator;
        private readonly DashboardCustomerRiskForecastAggregator _forecastAggregator;
        private readonly DashboardCollectionOptimizationAggregator _optimizationAggregator;
        private readonly IDashboardCollectionSnapshotDal _collectionSnapshotDal;
        private readonly IDashboardCustomerSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly DashboardSnapshotOptions _options;

        public RefreshDashboardCustomerSnapshotWorker(
            IFakturViewDal fakturViewDal,
            ICustomerLastFakturDal lastFakturDal,
            IPiutangOpenBalanceDal openBalanceDal,
            ICustomerDal customerDal,
            ICustomerOmzetHistoryDal customerOmzetHistoryDal,
            ICustomerPelunasanSummaryDal customerPelunasanSummaryDal,
            ICustomerPaymentBehaviorDal customerPaymentBehaviorDal,
            DashboardCustomerAggregator aggregator,
            DashboardCustomerRiskForecastAggregator forecastAggregator,
            DashboardCollectionOptimizationAggregator optimizationAggregator,
            IDashboardCollectionSnapshotDal collectionSnapshotDal,
            IDashboardCustomerSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider,
            DashboardSnapshotOptions options)
        {
            _fakturViewDal = fakturViewDal;
            _lastFakturDal = lastFakturDal;
            _openBalanceDal = openBalanceDal;
            _customerDal = customerDal;
            _customerOmzetHistoryDal = customerOmzetHistoryDal;
            _customerPelunasanSummaryDal = customerPelunasanSummaryDal;
            _customerPaymentBehaviorDal = customerPaymentBehaviorDal;
            _aggregator = aggregator;
            _forecastAggregator = forecastAggregator;
            _optimizationAggregator = optimizationAggregator;
            _collectionSnapshotDal = collectionSnapshotDal;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public void Execute(RefreshDashboardCustomerSnapshotRequest request)
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
                var priorMonth = PriorMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;
                const int loadSteps = 7;
                var forecastOptions = CustomerRiskForecastOptions.FromDashboardOptions(_options);
                var optimizationOptions = CollectionOptimizationOptions.FromDashboardOptions(_options);

                WorkerProgressScope.Current.StepStarted($"{Domain}:LoadCollectionContext", "Load M20 collection snapshot");
                var collectionSnapshot = _collectionSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:LoadCollectionContext");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load faktur", 1, loadSteps);
                var fakturRows = _fakturViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<FakturView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load omzet history", 2, loadSteps);
                var omzetHistoryRows = _customerOmzetHistoryDal.ListOmzetByCustomer(periode, priorMonth)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerOmzetHistoryDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by customer", 3, loadSteps);
                var lastFakturRows = _lastFakturDal.ListLastFakturByCustomer()?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load open balances", 4, loadSteps);
                var piutangRows = _openBalanceDal.ListOpenBalances()?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load customers", 5, loadSteps);
                var customers = _customerDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.CustomerAgg.CustomerModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load pelunasan summary 30d", 6, loadSteps);
                var pelunasanSummaryRows = _customerPelunasanSummaryDal
                    .ListSummary(today.AddDays(-forecastOptions.NoPaymentRecencyDays), today)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerPelunasanSummaryDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load payment behavior 90d", 7, loadSteps);
                var paymentBehaviorRows = _customerPaymentBehaviorDal.ListPaymentBehavior(
                    today.AddDays(-forecastOptions.PaymentLagLookbackDays),
                    today,
                    forecastOptions.MinSettledFaktursForLag)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerPaymentBehaviorDto>();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = fakturRows.Count + omzetHistoryRows.Count + lastFakturRows.Count +
                                  piutangRows.Count + customers.Count + pelunasanSummaryRows.Count +
                                  paymentBehaviorRows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(
                    fakturRows,
                    piutangRows,
                    lastFakturRows,
                    customers,
                    periode,
                    today,
                    generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateForecast", "Aggregate customer risk forecast");
                var forecastAggregate = _forecastAggregator.Aggregate(
                    piutangRows,
                    omzetHistoryRows,
                    lastFakturRows,
                    customers,
                    pelunasanSummaryRows,
                    paymentBehaviorRows,
                    fakturRows,
                    today,
                    generatedAt,
                    forecastOptions);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateForecast");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateOptimization", "Aggregate collection optimization");
                var optimizationAggregate = _optimizationAggregator.Aggregate(
                    forecastAggregate.Contexts,
                    forecastAggregate,
                    collectionSnapshot,
                    piutangRows,
                    fakturRows,
                    customers,
                    today,
                    generatedAt,
                    optimizationOptions);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateOptimization");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, forecastAggregate, optimizationAggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardCustomerSnapshotResult
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

        private static Periode PriorMonthPeriode(DateTime today)
        {
            var prior = today.AddMonths(-1);
            var monthStart = new DateTime(prior.Year, prior.Month, 1);
            var monthEnd = new DateTime(prior.Year, prior.Month, DateTime.DaysInMonth(prior.Year, prior.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
