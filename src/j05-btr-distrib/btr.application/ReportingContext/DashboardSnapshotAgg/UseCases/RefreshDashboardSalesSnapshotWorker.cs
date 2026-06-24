using System;
using System.Diagnostics;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardSalesSnapshotWorker
        : INunaServiceVoid<RefreshDashboardSalesSnapshotRequest>
    {
    }

    public class RefreshDashboardSalesSnapshotWorker : IRefreshDashboardSalesSnapshotWorker
    {
        private const string Domain = "Sales";
        private const int MaxErrorMessageLength = 500;

        private readonly IFakturViewDal _fakturViewDal;
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly DashboardSalesFakturAggregator _aggregator;
        private readonly DashboardSalesForecastAggregator _forecastAggregator;
        private readonly IDashboardSalesSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshDashboardSalesSnapshotWorker(
            IFakturViewDal fakturViewDal,
            ISalesOmzetTargetDal targetDal,
            DashboardSalesFakturAggregator aggregator,
            DashboardSalesForecastAggregator forecastAggregator,
            IDashboardSalesSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _fakturViewDal = fakturViewDal;
            _targetDal = targetDal;
            _aggregator = aggregator;
            _forecastAggregator = forecastAggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshDashboardSalesSnapshotRequest request)
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

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                var rows = _fakturViewDal.ListData(periode)?.ToList()
                           ?? new System.Collections.Generic.List<FakturView>();
                var totalTarget = _targetDal.SumTargetAmountForMonth(today.Year, today.Month);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = rows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(rows, periode, totalTarget, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateForecast", "Aggregate forecast metrics");
                var forecast = _forecastAggregator.Aggregate(rows, periode, totalTarget, today, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateForecast");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, forecast, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardSalesSnapshotResult
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
