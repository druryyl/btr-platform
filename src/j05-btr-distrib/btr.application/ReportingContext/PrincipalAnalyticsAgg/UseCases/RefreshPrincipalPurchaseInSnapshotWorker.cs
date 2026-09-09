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
    public interface IRefreshPrincipalPurchaseInSnapshotWorker
        : INunaServiceVoid<RefreshPrincipalPurchaseInSnapshotRequest>
    {
    }

    public class RefreshPrincipalPurchaseInSnapshotWorker : IRefreshPrincipalPurchaseInSnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IPrincipalPurchaseInEvidenceDal _evidenceDal;
        private readonly PrincipalPurchaseInAggregator _aggregator;
        private readonly IPrincipalPurchaseInSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshPrincipalPurchaseInSnapshotWorker(
            IPrincipalPurchaseInEvidenceDal evidenceDal,
            PrincipalPurchaseInAggregator aggregator,
            IPrincipalPurchaseInSnapshotDal snapshotDal,
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

        public void Execute(RefreshPrincipalPurchaseInSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalPurchaseInSnapshot.Domain;

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

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load Purchase Detail evidence");
                var purchaseDetails = _evidenceDal.ListPurchaseDetail(today.Year, today.Month);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = purchaseDetails?.Count ?? 0
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Aggregate", "Aggregate Principal Purchase-In");
                var aggregate = _aggregator.Aggregate(
                    purchaseDetails,
                    today.Year,
                    today.Month,
                    generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Principal Purchase-In snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalPurchaseInSnapshotResult
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
