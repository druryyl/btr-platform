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
    public interface IRefreshPrincipalCustomerCoverageSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalCustomerCoverageSnapshotRequest>
    {
    }

    public class RefreshPrincipalCustomerCoverageSnapshotWorker : IRefreshPrincipalCustomerCoverageSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly ICustomerPrincipalRelationshipDal _projectionDal;
        private readonly IPrincipalActiveCustomerSnapshotDal _activeCustomerSnapshotDal;
        private readonly PrincipalCustomerCoverageComposer _composer;
        private readonly IPrincipalCustomerCoverageSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshPrincipalCustomerCoverageSnapshotWorker(
            ICustomerPrincipalRelationshipDal projectionDal,
            IPrincipalActiveCustomerSnapshotDal activeCustomerSnapshotDal,
            PrincipalCustomerCoverageComposer composer,
            IPrincipalCustomerCoverageSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _projectionDal = projectionDal;
            _activeCustomerSnapshotDal = activeCustomerSnapshotDal;
            _composer = composer;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshPrincipalCustomerCoverageSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalCustomerCoverageSnapshot.Domain;

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
                WorkerProgressScope.Current.StepStarted($"{domain}:LoadProjection", "Load stored Customer-Principal relationship projection");
                var storedProjection = _projectionDal.GetProjection();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadProjection");

                WorkerProgressScope.Current.StepStarted($"{domain}:LoadActiveCustomer", "Load stored PRN-CUS-001 Active Customer Count");
                var storedActiveCustomers = _activeCustomerSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{domain}:LoadActiveCustomer");

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Compute Customer Coverage Percentage from projection and PRN-CUS-001");
                var coverage = _composer.Compose(storedProjection, storedActiveCustomers, _tglJamDal.Now);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Customer Coverage snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(coverage, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalCustomerCoverageSnapshotResult
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
