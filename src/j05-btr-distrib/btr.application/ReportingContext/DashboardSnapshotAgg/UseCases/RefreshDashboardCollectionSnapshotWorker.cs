using System;
using System.Diagnostics;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardCollectionSnapshotWorker
        : INunaServiceVoid<RefreshDashboardCollectionSnapshotRequest>
    {
    }

    public class RefreshDashboardCollectionSnapshotWorker : IRefreshDashboardCollectionSnapshotWorker
    {
        private const string Domain = "Collection";
        private const int MaxErrorMessageLength = 500;

        private readonly IPiutangOpenBalanceDal _openBalanceDal;
        private readonly IPiutangOpenBalanceWithSalesmanDal _openBalanceWithSalesmanDal;
        private readonly IPiutangOpenBalanceWithWilayahDal _openBalanceWithWilayahDal;
        private readonly IPenerimaanPelunasanSalesDal _pelunasanDal;
        private readonly IFakturViewDal _fakturViewDal;
        private readonly ICustomerLastFakturDal _lastFakturDal;
        private readonly ICustomerDal _customerDal;
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly DashboardCollectionAggregator _aggregator;
        private readonly IDashboardCollectionSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshDashboardCollectionSnapshotWorker(
            IPiutangOpenBalanceDal openBalanceDal,
            IPiutangOpenBalanceWithSalesmanDal openBalanceWithSalesmanDal,
            IPiutangOpenBalanceWithWilayahDal openBalanceWithWilayahDal,
            IPenerimaanPelunasanSalesDal pelunasanDal,
            IFakturViewDal fakturViewDal,
            ICustomerLastFakturDal lastFakturDal,
            ICustomerDal customerDal,
            ISalesPersonDal salesPersonDal,
            DashboardCollectionAggregator aggregator,
            IDashboardCollectionSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _openBalanceDal = openBalanceDal;
            _openBalanceWithSalesmanDal = openBalanceWithSalesmanDal;
            _openBalanceWithWilayahDal = openBalanceWithWilayahDal;
            _pelunasanDal = pelunasanDal;
            _fakturViewDal = fakturViewDal;
            _lastFakturDal = lastFakturDal;
            _customerDal = customerDal;
            _salesPersonDal = salesPersonDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshDashboardCollectionSnapshotRequest request)
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
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load open balances", 1, loadSteps);
                var openBalanceRows = _openBalanceDal.ListOpenBalances()?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load open balances by salesman", 2, loadSteps);
                var openBalanceWithSalesmanRows = _openBalanceWithSalesmanDal.ListOpenBalances()?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceWithSalesmanDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load open balances by wilayah", 3, loadSteps);
                var openBalanceWithWilayahRows = _openBalanceWithWilayahDal.ListOpenBalances()?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceWithWilayahDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load pelunasan", 4, loadSteps);
                var pelunasanRows = _pelunasanDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<PenerimaanPelunasanSalesDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load faktur", 5, loadSteps);
                var fakturRows = _fakturViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<FakturView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by customer", 6, loadSteps);
                var lastFakturRows = _lastFakturDal.ListLastFakturByCustomer()?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load customers", 7, loadSteps);
                var customers = _customerDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.CustomerAgg.CustomerModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load salespeople", 8, loadSteps);
                var salespeople = _salesPersonDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = openBalanceRows.Count + openBalanceWithSalesmanRows.Count +
                                  openBalanceWithWilayahRows.Count + pelunasanRows.Count +
                                  fakturRows.Count + lastFakturRows.Count + customers.Count + salespeople.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(
                    openBalanceRows,
                    openBalanceWithSalesmanRows,
                    openBalanceWithWilayahRows,
                    pelunasanRows,
                    fakturRows,
                    lastFakturRows,
                    customers,
                    salespeople,
                    periode,
                    today,
                    generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardCollectionSnapshotResult
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
