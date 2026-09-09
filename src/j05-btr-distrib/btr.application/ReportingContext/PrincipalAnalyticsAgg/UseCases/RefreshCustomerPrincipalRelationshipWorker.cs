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
    public interface IRefreshCustomerPrincipalRelationshipWorker
        : INunaServiceVoid<RefreshCustomerPrincipalRelationshipRequest>
    {
    }

    public class RefreshCustomerPrincipalRelationshipWorker : IRefreshCustomerPrincipalRelationshipWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly ICustomerPrincipalRelationshipEvidenceDal _evidenceDal;
        private readonly CustomerPrincipalRelationshipComposer _composer;
        private readonly ICustomerPrincipalRelationshipDal _projectionDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshCustomerPrincipalRelationshipWorker(
            ICustomerPrincipalRelationshipEvidenceDal evidenceDal,
            CustomerPrincipalRelationshipComposer composer,
            ICustomerPrincipalRelationshipDal projectionDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _evidenceDal = evidenceDal;
            _composer = composer;
            _projectionDal = projectionDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshCustomerPrincipalRelationshipRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = CustomerPrincipalRelationship.Domain;

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
                var asOfDate = _businessDateProvider.Today;
                var generatedAt = _tglJamDal.Now;

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load historical Customer-Principal pair evidence");
                var pairs = _evidenceDal.ListHistoricalPairEvidence();
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = pairs?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Compose", "Compose relationship projection");
                var projection = _composer.Compose(pairs, asOfDate, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Compose", new WorkerProgressStepInfo
                {
                    RecordCount = projection.Pairs?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Customer-Principal relationship projection");
                using (var trans = TransHelper.NewScope())
                {
                    _projectionDal.ReplaceProjection(projection, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshCustomerPrincipalRelationshipResult
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
