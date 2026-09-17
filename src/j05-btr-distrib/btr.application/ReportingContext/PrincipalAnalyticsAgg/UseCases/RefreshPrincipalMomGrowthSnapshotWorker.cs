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
    public interface IRefreshPrincipalMomGrowthSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalMomGrowthSnapshotRequest>
    {
    }

    public class RefreshPrincipalMomGrowthSnapshotWorker : IRefreshPrincipalMomGrowthSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalSalesOutHistoryDal _salesOutHistoryDal;
        private readonly PrincipalMomGrowthComposer _composer;
        private readonly IPrincipalMomGrowthSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshPrincipalMomGrowthSnapshotWorker(
            IPrincipalSalesOutHistoryDal salesOutHistoryDal,
            PrincipalMomGrowthComposer composer,
            IPrincipalMomGrowthSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _salesOutHistoryDal = salesOutHistoryDal;
            _composer = composer;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshPrincipalMomGrowthSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalMomGrowthSnapshot.Domain;

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
                WorkerProgressScope.Current.StepStarted($"{domain}:LoadHistory", "Load stored Principal Sales-Out history");
                var storedHistory = _salesOutHistoryDal.GetHistory();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadHistory");

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Calculate Month-over-Month Growth Percentage");
                var growth = _composer.Compose(storedHistory, _tglJamDal.Now);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Month-over-Month Growth snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(growth, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalMomGrowthSnapshotResult
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
