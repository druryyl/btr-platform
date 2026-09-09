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
    public interface IRefreshPrincipalReturnPercentageSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalReturnPercentageSnapshotRequest>
    {
    }

    public class RefreshPrincipalReturnPercentageSnapshotWorker : IRefreshPrincipalReturnPercentageSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalReturnSnapshotDal _returnSnapshotDal;
        private readonly IPrincipalSalesOutSnapshotDal _salesOutSnapshotDal;
        private readonly PrincipalReturnPercentageComposer _composer;
        private readonly IPrincipalReturnPercentageSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshPrincipalReturnPercentageSnapshotWorker(
            IPrincipalReturnSnapshotDal returnSnapshotDal,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            PrincipalReturnPercentageComposer composer,
            IPrincipalReturnPercentageSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _returnSnapshotDal = returnSnapshotDal;
            _salesOutSnapshotDal = salesOutSnapshotDal;
            _composer = composer;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshPrincipalReturnPercentageSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalReturnPercentageSnapshot.Domain;

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
                WorkerProgressScope.Current.StepStarted($"{domain}:LoadReturns", "Load stored Principal Total Return Amount");
                var storedReturns = _returnSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadReturns");

                WorkerProgressScope.Current.StepStarted($"{domain}:LoadSalesOut", "Load stored Principal Sales-Out");
                var storedSalesOut = _salesOutSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadSalesOut");

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Calculate Return Percentage");
                var percentage = _composer.Compose(storedReturns, storedSalesOut, _tglJamDal.Now);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Return Percentage snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(percentage, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalReturnPercentageSnapshotResult
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
