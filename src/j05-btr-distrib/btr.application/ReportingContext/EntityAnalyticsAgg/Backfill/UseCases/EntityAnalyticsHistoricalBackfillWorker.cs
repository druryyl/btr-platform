using System;
using System.Diagnostics;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.UseCases
{
    public interface IEntityAnalyticsHistoricalBackfillWorker
        : INunaServiceVoid<EntityAnalyticsBackfillRequest>
    {
    }

    public class EntityAnalyticsHistoricalBackfillWorker : IEntityAnalyticsHistoricalBackfillWorker
    {
        private const string Domain = "EntityAnalyticsHistoricalBackfill";
        private const int MaxErrorMessageLength = 500;

        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IEntityAnalyticsBackfillOrchestrator _orchestrator;

        public EntityAnalyticsHistoricalBackfillWorker(
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IEntityAnalyticsBackfillOrchestrator orchestrator)
        {
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _orchestrator = orchestrator;
        }

        public void Execute(EntityAnalyticsBackfillRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = string.IsNullOrWhiteSpace(request.RefreshLogId)
                ? Ulid.NewUlid().ToString()
                : request.RefreshLogId;
            request.RefreshLogId = refreshLogId;

            WorkerProgressScope.Current?.StepStarted($"{Domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = _tglJamDal.Now,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current?.StepCompleted($"{Domain}:Initialize");

            try
            {
                WorkerProgressScope.Current?.StepStarted($"{Domain}:Run", "Run historical backfill orchestrator");
                var result = _orchestrator.Run(request, System.Threading.CancellationToken.None);
                request.Result = result;
                WorkerProgressScope.Current?.StepCompleted($"{Domain}:Run", new WorkerProgressStepInfo
                {
                    RecordCount = result.PeriodsProcessed,
                    Detail = $"Processed={result.PeriodsProcessed}, Skipped={result.PeriodsSkipped}"
                });

                sw.Stop();

                if (string.Equals(result.Status, EntityAnalyticsBackfillJobStatus.Failed, StringComparison.Ordinal))
                {
                    var message = Truncate(result.ErrorMessage ?? "Backfill failed.");
                    _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                    throw new InvalidOperationException(message);
                }

                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                sw.Stop();
                var message = Truncate(ex.Message);
                _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                WorkerProgressScope.Current?.StepFailed($"{Domain}:Run", message);
                throw;
            }
        }

        private static string Truncate(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length <= MaxErrorMessageLength)
                return message ?? string.Empty;

            return message.Substring(0, MaxErrorMessageLength);
        }
    }
}
