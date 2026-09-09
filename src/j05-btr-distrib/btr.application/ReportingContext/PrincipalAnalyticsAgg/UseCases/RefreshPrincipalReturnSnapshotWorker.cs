using System;
using System.Diagnostics;
using btr.application.Portal;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public interface IRefreshPrincipalReturnSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalReturnSnapshotRequest>
    {
    }

    public class RefreshPrincipalReturnSnapshotWorker : IRefreshPrincipalReturnSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalReturnEvidenceDal _evidenceDal;
        private readonly PrincipalReturnAggregator _aggregator;
        private readonly IPrincipalReturnSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshPrincipalReturnSnapshotWorker(
            IPrincipalReturnEvidenceDal evidenceDal,
            PrincipalReturnAggregator aggregator,
            IPrincipalReturnSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _evidenceDal = evidenceDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshPrincipalReturnSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalReturnSnapshot.Domain;

            WorkerProgressScope.Current.StepStarted($"{domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{domain}:Initialize");

            try
            {
                var today = _businessDateProvider.Today;
                var generatedAt = _tglJamDal.Now;

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load Return Item evidence");
                var returnItems = _evidenceDal.ListReturnItemEvidence(today.Year, today.Month);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = returnItems?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Aggregate", "Aggregate Principal return amounts");
                var aggregate = _aggregator.Aggregate(
                    returnItems,
                    today.Year,
                    today.Month,
                    generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Principal return snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalReturnSnapshotResult
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
                WorkerProgressScope.Current.StepFailed($"{domain}:Execute", message);
                throw;
            }
        }
    }
}
