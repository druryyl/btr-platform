using System;
using System.Diagnostics;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public interface IRefreshPrincipalReturnHistoryWorker
        : INunaServiceVoid<RefreshPrincipalReturnHistoryRequest>
    {
    }

    public class RefreshPrincipalReturnHistoryWorker : IRefreshPrincipalReturnHistoryWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalReturnHistoryEvidenceDal _evidenceDal;
        private readonly IPrincipalReturnSnapshotDal _currentSnapshotDal;
        private readonly PrincipalReturnHistoryComposer _composer;
        private readonly IPrincipalReturnHistoryDal _historyDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshPrincipalReturnHistoryWorker(
            IPrincipalReturnHistoryEvidenceDal evidenceDal,
            PrincipalReturnHistoryComposer composer,
            IPrincipalReturnSnapshotDal currentSnapshotDal,
            IPrincipalReturnHistoryDal historyDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _evidenceDal = evidenceDal;
            _composer = composer;
            _currentSnapshotDal = currentSnapshotDal;
            _historyDal = historyDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshPrincipalReturnHistoryRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalReturnHistory.Domain;

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
                WorkerProgressScope.Current.StepStarted($"{domain}:LoadCurrent", "Load current Principal return snapshot");
                var current = _currentSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadCurrent");

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load Return Item return history");
                var months = _evidenceDal.ListMonthlyReturnHistory();
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = months?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Compose Principal return history");
                var history = _composer.Compose(months, current, _tglJamDal.Now);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Principal return history");
                using (var trans = TransHelper.NewScope())
                {
                    _historyDal.ReplaceHistory(history, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalReturnHistoryResult
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
