using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using btr.application.Portal;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using Microsoft.Extensions.Options;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services
{
    public class EntityAnalyticsBackfillOrchestrator : IEntityAnalyticsBackfillOrchestrator
    {
        private const string ConfirmTokenValue = "BACKFILL";
        private const int MaxErrorLength = 500;

        private readonly IEntityAnalyticsBackfillCheckpointStore _checkpointStore;
        private readonly IEntityAnalyticsBackfillMutex _mutex;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly IEntityAnalyticsReplayDataLoaderResolver _loaderResolver;
        private readonly IEntityAnalyticsReplayAggregateService _aggregateService;
        private readonly ISalesmanReplayPeriodHandler _salesmanReplayPeriodHandler;
        private readonly EntityAnalyticsProducerOrchestrator _producerOrchestrator;
        private readonly ITglJamDal _tglJamDal;
        private readonly EntityAnalyticsOptions _options;

        public EntityAnalyticsBackfillOrchestrator(
            IEntityAnalyticsBackfillCheckpointStore checkpointStore,
            IEntityAnalyticsBackfillMutex mutex,
            IBusinessDateProvider businessDateProvider,
            IEntityAnalyticsReplayDataLoaderResolver loaderResolver,
            IEntityAnalyticsReplayAggregateService aggregateService,
            ISalesmanReplayPeriodHandler salesmanReplayPeriodHandler,
            EntityAnalyticsProducerOrchestrator producerOrchestrator,
            ITglJamDal tglJamDal,
            IOptions<EntityAnalyticsOptions> options)
        {
            _checkpointStore = checkpointStore;
            _mutex = mutex;
            _businessDateProvider = businessDateProvider;
            _loaderResolver = loaderResolver;
            _aggregateService = aggregateService;
            _salesmanReplayPeriodHandler = salesmanReplayPeriodHandler;
            _producerOrchestrator = producerOrchestrator;
            _tglJamDal = tglJamDal;
            _options = options?.Value ?? new EntityAnalyticsOptions();
        }

        public EntityAnalyticsBackfillResult Run(
            EntityAnalyticsBackfillRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var jobId = Ulid.NewUlid().ToString();
            var entityTypes = ResolveEntityTypes(request.EntityTypeScope);
            var fromPeriod = ResolveFromPeriod(request);
            var toPeriod = ResolveToPeriod(request);

            ValidateRequest(request, entityTypes, fromPeriod, toPeriod);

            if (request.Restart)
            {
                foreach (var entityType in entityTypes)
                {
                    _checkpointStore.DeleteCheckpointsForScope(
                        jobId: null,
                        entityType,
                        fromPeriod.Year,
                        fromPeriod.Month,
                        toPeriod.Year,
                        toPeriod.Month);
                }
            }

            var optionsJson = JsonConvert.SerializeObject(new
            {
                request.EntityTypeScope,
                FromPeriod = fromPeriod.Label,
                ToPeriod = toPeriod.Label,
                request.Layers,
                request.Resume,
                request.Restart,
                request.Force,
                request.DryRun,
                request.BatchSize
            });

            _checkpointStore.CreateJob(new EntityAnalyticsBackfillJobModel
            {
                BackfillJobId = jobId,
                EntityTypeScope = request.EntityTypeScope,
                FromPeriodYear = fromPeriod.Year,
                FromPeriodMonth = fromPeriod.Month,
                ToPeriodYear = toPeriod.Year,
                ToPeriodMonth = toPeriod.Month,
                Layers = request.Layers,
                OptionsJson = optionsJson,
                Status = EntityAnalyticsBackfillJobStatus.Running,
                StartedAt = DateTime.Now,
                TriggeredBy = request.TriggeredBy ?? "Scheduler",
                MachineName = Environment.MachineName
            });

            var periods = EntityAnalyticsBackfillPeriodHelper.EnumeratePeriods(fromPeriod, toPeriod);
            var periodsProcessed = 0;
            var periodsSkipped = 0;
            string failureMessage = null;

            try
            {
                foreach (var entityType in entityTypes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _mutex.Acquire(entityType, jobId, request.SkipLiveMutexCheck);

                    try
                    {
                        foreach (var period in periods)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (ShouldSkipPeriod(request, jobId, entityType, period, out var existing))
                            {
                                periodsSkipped++;
                                continue;
                            }

                            var periodStepId = $"Backfill:{entityType}:{period.Label}";
                            WorkerProgressScope.Current?.StepStarted($"{periodStepId}:Plan", "Plan historical replay period");

                            var checkpointId = existing?.BackfillCheckpointId ?? Ulid.NewUlid().ToString();
                            var startedAt = DateTime.Now;

                            try
                            {
                                _checkpointStore.UpsertCheckpoint(new EntityAnalyticsBackfillCheckpointModel
                                {
                                    BackfillCheckpointId = checkpointId,
                                    BackfillJobId = jobId,
                                    EntityType = entityType,
                                    PeriodYear = period.Year,
                                    PeriodMonth = period.Month,
                                    Status = EntityAnalyticsBackfillCheckpointStatus.Running,
                                    LayersCompleted = string.Empty,
                                    EntityCount = 0,
                                    RowCountsJson = string.Empty,
                                    StartedAt = startedAt,
                                    CompletedAt = null,
                                    LastError = string.Empty,
                                    LastRefreshLogId = request.RefreshLogId ?? string.Empty
                                });

                                var aggregateResult = ExecutePeriodReplay(
                                    periodStepId,
                                    period,
                                    entityType,
                                    jobId,
                                    request,
                                    out var replayContext,
                                    out var generatedAt);

                                if (request.DryRun)
                                {
                                    WorkerProgressScope.Current?.StepCompleted($"{periodStepId}:Plan");

                                    _checkpointStore.UpsertCheckpoint(new EntityAnalyticsBackfillCheckpointModel
                                    {
                                        BackfillCheckpointId = checkpointId,
                                        BackfillJobId = jobId,
                                        EntityType = entityType,
                                        PeriodYear = period.Year,
                                        PeriodMonth = period.Month,
                                        Status = EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted,
                                        LayersCompleted = request.Layers,
                                        EntityCount = aggregateResult.EntityCount,
                                        RowCountsJson = JsonConvert.SerializeObject(aggregateResult.RowCounts),
                                        StartedAt = startedAt,
                                        CompletedAt = DateTime.Now,
                                        LastError = string.Empty,
                                        LastRefreshLogId = request.RefreshLogId ?? string.Empty
                                    });
                                }
                                else
                                {
                                    EnsureWriteSupportedForEntityType(entityType);

                                    var produceInput = aggregateResult.ProduceInput;
                                    if (string.Equals(entityType, EntityTypeCode.Salesman, StringComparison.OrdinalIgnoreCase)
                                        && replayContext.SkipL1Persist)
                                    {
                                        _salesmanReplayPeriodHandler.PersistFastPathL1(
                                            period.Year,
                                            period.Month,
                                            request.RefreshLogId ?? string.Empty,
                                            generatedAt);
                                        produceInput = _salesmanReplayPeriodHandler.CreateLayersOnlyInput();
                                    }

                                    WorkerProgressScope.Current?.StepStarted($"{periodStepId}:Produce", "Produce entity analytics snapshot");
                                    var produceContext = EntityAnalyticsReplayContextFactory.CreateProduceContext(
                                        replayContext,
                                        produceInput,
                                        request.RefreshLogId ?? string.Empty,
                                        generatedAt);
                                    _producerOrchestrator.ProduceForEntityType(entityType, produceContext);
                                    WorkerProgressScope.Current?.StepCompleted($"{periodStepId}:Produce");
                                    WorkerProgressScope.Current?.StepCompleted($"{periodStepId}:Plan");

                                    _checkpointStore.UpsertCheckpoint(new EntityAnalyticsBackfillCheckpointModel
                                    {
                                        BackfillCheckpointId = checkpointId,
                                        BackfillJobId = jobId,
                                        EntityType = entityType,
                                        PeriodYear = period.Year,
                                        PeriodMonth = period.Month,
                                        Status = EntityAnalyticsBackfillCheckpointStatus.Completed,
                                        LayersCompleted = request.Layers,
                                        EntityCount = aggregateResult.EntityCount,
                                        RowCountsJson = JsonConvert.SerializeObject(aggregateResult.RowCounts),
                                        StartedAt = startedAt,
                                        CompletedAt = DateTime.Now,
                                        LastError = string.Empty,
                                        LastRefreshLogId = request.RefreshLogId ?? string.Empty
                                    });
                                }

                                periodsProcessed++;
                            }
                            catch (Exception ex)
                            {
                                var message = Truncate(ex.Message);
                                _checkpointStore.UpsertCheckpoint(new EntityAnalyticsBackfillCheckpointModel
                                {
                                    BackfillCheckpointId = checkpointId,
                                    BackfillJobId = jobId,
                                    EntityType = entityType,
                                    PeriodYear = period.Year,
                                    PeriodMonth = period.Month,
                                    Status = EntityAnalyticsBackfillCheckpointStatus.Failed,
                                    LayersCompleted = string.Empty,
                                    EntityCount = 0,
                                    RowCountsJson = string.Empty,
                                    StartedAt = startedAt,
                                    CompletedAt = DateTime.Now,
                                    LastError = message,
                                    LastRefreshLogId = request.RefreshLogId ?? string.Empty
                                });

                                WorkerProgressScope.Current?.StepFailed($"{periodStepId}:Plan", message);
                                failureMessage = message;
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        _mutex.Release(entityType, jobId);
                    }
                }

                _checkpointStore.UpdateJob(jobId, job =>
                {
                    job.Status = EntityAnalyticsBackfillJobStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                    job.LastError = string.Empty;
                });

                sw.Stop();
                return BuildResult(jobId, EntityAnalyticsBackfillJobStatus.Succeeded, periodsProcessed, periodsSkipped, sw, null);
            }
            catch (OperationCanceledException)
            {
                _checkpointStore.UpdateJob(jobId, job =>
                {
                    job.Status = EntityAnalyticsBackfillJobStatus.Cancelled;
                    job.CompletedAt = DateTime.Now;
                    job.LastError = "Cancelled.";
                });

                sw.Stop();
                throw;
            }
            catch (Exception ex)
            {
                _checkpointStore.UpdateJob(jobId, job =>
                {
                    job.Status = EntityAnalyticsBackfillJobStatus.Failed;
                    job.CompletedAt = DateTime.Now;
                    job.LastError = Truncate(failureMessage ?? ex.Message);
                });

                sw.Stop();
                return BuildResult(
                    jobId,
                    EntityAnalyticsBackfillJobStatus.Failed,
                    periodsProcessed,
                    periodsSkipped,
                    sw,
                    failureMessage ?? ex.Message);
            }
        }

        private EntityAnalyticsReplayAggregateResult ExecutePeriodReplay(
            string periodStepId,
            YearMonthPeriod period,
            string entityType,
            string jobId,
            EntityAnalyticsBackfillRequest request,
            out EntityAnalyticsReplayContext replayContext,
            out DateTime generatedAt)
        {
            replayContext = EntityAnalyticsReplayContextFactory.Create(
                period,
                entityType,
                jobId,
                request);

            if (string.Equals(entityType, EntityTypeCode.Salesman, StringComparison.OrdinalIgnoreCase)
                && _salesmanReplayPeriodHandler.CanUseFastPath(period.Year, period.Month))
            {
                replayContext.SkipL1Persist = true;
                generatedAt = _tglJamDal.Now;
                return _salesmanReplayPeriodHandler.BuildFastPathPlan(
                    period.Year,
                    period.Month,
                    request.RefreshLogId ?? string.Empty,
                    generatedAt);
            }

            var loader = _loaderResolver.Resolve(entityType);
            generatedAt = _tglJamDal.Now;

            WorkerProgressScope.Current?.StepStarted($"{periodStepId}:Load", "Load historical replay data");
            var bundle = loader.Load(replayContext);
            WorkerProgressScope.Current?.StepCompleted($"{periodStepId}:Load");

            WorkerProgressScope.Current?.StepStarted($"{periodStepId}:Aggregate", "Aggregate historical replay data");
            var aggregateResult = _aggregateService.Aggregate(replayContext, bundle, generatedAt);
            WorkerProgressScope.Current?.StepCompleted($"{periodStepId}:Aggregate");

            return aggregateResult;
        }

        private static void EnsureWriteSupportedForEntityType(string entityType)
        {
            if (string.Equals(entityType, EntityTypeCode.Customer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, EntityTypeCode.Salesman, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, EntityTypeCode.Supplier, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, EntityTypeCode.Item, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new NotSupportedException(
                $"Historical backfill writes for entity type '{entityType}' are not supported.");
        }

        private bool ShouldSkipPeriod(
            EntityAnalyticsBackfillRequest request,
            string jobId,
            string entityType,
            YearMonthPeriod period,
            out EntityAnalyticsBackfillCheckpointModel existing)
        {
            existing = _checkpointStore.GetCheckpoint(jobId, entityType, period.Year, period.Month);

            if (!request.Resume || request.Force)
                return false;

            if (existing == null)
                return false;

            if (string.Equals(existing.Status, EntityAnalyticsBackfillCheckpointStatus.Completed, StringComparison.Ordinal)
                || string.Equals(existing.Status, EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted, StringComparison.Ordinal))
            {
                return true;
            }

            // ADR-005: Running checkpoints are re-executed.
            return false;
        }

        private void ValidateRequest(
            EntityAnalyticsBackfillRequest request,
            IReadOnlyList<string> entityTypes,
            YearMonthPeriod fromPeriod,
            YearMonthPeriod toPeriod)
        {
            if (entityTypes.Count == 0)
                throw new ArgumentException("No entity types selected for backfill.");

            if (fromPeriod > toPeriod)
                throw new ArgumentException("From period must be less than or equal to to period.");

            if (!EntityAnalyticsBackfillPeriodHelper.IsWithinRetention(
                fromPeriod,
                toPeriod,
                _businessDateProvider.Today,
                _options.HistoryRetentionMonths))
            {
                throw new ArgumentException(
                    $"Period range must fall within the last {_options.HistoryRetentionMonths} months " +
                    "and not beyond the last closed month.");
            }

            if (!request.DryRun
                && !string.Equals(request.ConfirmToken, ConfirmTokenValue, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Non-dry-run backfill requires --confirm {ConfirmTokenValue}.");
            }

            EnsureEntityTypesEnabled(entityTypes);
        }

        private void EnsureEntityTypesEnabled(IReadOnlyList<string> entityTypes)
        {
            var enabled = _options.EnabledEntityTypes ?? Array.Empty<string>();
            if (enabled.Length == 0)
                return;

            var enabledSet = new HashSet<string>(enabled, StringComparer.OrdinalIgnoreCase);
            foreach (var entityType in entityTypes)
            {
                if (!enabledSet.Contains(entityType))
                {
                    throw new InvalidOperationException(
                        $"Entity type '{entityType}' is not enabled in EntityAnalytics configuration.");
                }
            }
        }

        private YearMonthPeriod ResolveFromPeriod(EntityAnalyticsBackfillRequest request)
        {
            if (request.FromPeriodYear.HasValue && request.FromPeriodMonth.HasValue)
                return new YearMonthPeriod(request.FromPeriodYear.Value, request.FromPeriodMonth.Value);

            return EntityAnalyticsBackfillPeriodHelper.GetDefaultFromPeriod(
                _businessDateProvider.Today,
                _options.HistoryRetentionMonths);
        }

        private YearMonthPeriod ResolveToPeriod(EntityAnalyticsBackfillRequest request)
        {
            if (request.ToPeriodYear.HasValue && request.ToPeriodMonth.HasValue)
                return new YearMonthPeriod(request.ToPeriodYear.Value, request.ToPeriodMonth.Value);

            return EntityAnalyticsBackfillPeriodHelper.GetDefaultToPeriod(_businessDateProvider.Today);
        }

        private static IReadOnlyList<string> ResolveEntityTypes(string entityTypeScope)
        {
            if (string.IsNullOrWhiteSpace(entityTypeScope)
                || string.Equals(entityTypeScope, "All", StringComparison.OrdinalIgnoreCase))
            {
                return EntityAnalyticsBackfillExecutionOrder.EntityTypes;
            }

            var match = EntityAnalyticsBackfillExecutionOrder.EntityTypes
                .FirstOrDefault(t => string.Equals(t, entityTypeScope, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                throw new ArgumentException(
                    $"Invalid entity type scope '{entityTypeScope}'. Expected Customer, Salesman, Supplier, Item, or All.");
            }

            return new[] { match };
        }

        private static EntityAnalyticsBackfillResult BuildResult(
            string jobId,
            string status,
            int periodsProcessed,
            int periodsSkipped,
            Stopwatch sw,
            string errorMessage)
        {
            return new EntityAnalyticsBackfillResult
            {
                BackfillJobId = jobId,
                Status = status,
                PeriodsProcessed = periodsProcessed,
                PeriodsSkipped = periodsSkipped,
                DurationMs = (int)sw.ElapsedMilliseconds,
                ErrorMessage = errorMessage
            };
        }

        private static string Truncate(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length <= MaxErrorLength)
                return message ?? string.Empty;

            return message.Substring(0, MaxErrorLength);
        }
    }
}
