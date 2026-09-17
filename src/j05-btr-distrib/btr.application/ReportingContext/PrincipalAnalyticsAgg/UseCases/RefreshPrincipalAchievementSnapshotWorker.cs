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
    public interface IRefreshPrincipalAchievementSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalAchievementSnapshotRequest>
    {
    }

    public class RefreshPrincipalAchievementSnapshotWorker : IRefreshPrincipalAchievementSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;
        private readonly IPrincipalSalesOutSnapshotDal _salesOutSnapshotDal;
        private readonly PrincipalAchievementComposer _composer;
        private readonly IPrincipalAchievementSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshPrincipalAchievementSnapshotWorker(
            IPrincipalTargetSnapshotDal targetSnapshotDal,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            PrincipalAchievementComposer composer,
            IPrincipalAchievementSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _targetSnapshotDal = targetSnapshotDal;
            _salesOutSnapshotDal = salesOutSnapshotDal;
            _composer = composer;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshPrincipalAchievementSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalAchievementSnapshot.Domain;

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
                WorkerProgressScope.Current.StepStarted($"{domain}:LoadTargets", "Load stored Principal Target");
                var storedTargets = _targetSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadTargets");

                WorkerProgressScope.Current.StepStarted($"{domain}:LoadSalesOut", "Load stored Principal Sales-Out");
                var storedSalesOut = _salesOutSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadSalesOut");

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Calculate Achievement Amount and Percentage");
                var achievement = _composer.Compose(storedTargets, storedSalesOut, _tglJamDal.Now);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Achievement snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(achievement, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalAchievementSnapshotResult
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
