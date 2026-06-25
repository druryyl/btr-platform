using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.Portal;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SupportContext.TglJamAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityAnalyticsBackfillOrchestratorTest
    {
        private static readonly YearMonthPeriod Period1 = new YearMonthPeriod(2023, 1);
        private static readonly YearMonthPeriod Period2 = new YearMonthPeriod(2023, 2);
        private static readonly YearMonthPeriod Period3 = new YearMonthPeriod(2023, 3);

        [Fact]
        public void Run_DryRun_WritesEntityCountFromAggregate()
        {
            var store = new FakeCheckpointStore();
            var orchestrator = CreateOrchestrator(store);

            var result = orchestrator.Run(CreateRequest(dryRun: true), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            store.Checkpoints.Should().OnlyContain(c => c.EntityCount == 42);
            store.Checkpoints.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.RowCountsJson));
        }

        [Fact]
        public void Run_DryRun_WritesDryRunCompletedForEachPeriod()
        {
            var store = new FakeCheckpointStore();
            var orchestrator = CreateOrchestrator(store);

            var result = orchestrator.Run(CreateRequest(dryRun: true), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            result.PeriodsProcessed.Should().Be(3);
            result.PeriodsSkipped.Should().Be(0);
            store.Checkpoints.Should().HaveCount(3);
            store.Checkpoints.Should().OnlyContain(c =>
                c.Status == EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted);
            store.Checkpoints.Should().OnlyContain(c => c.EntityCount == 42);
        }

        [Fact]
        public void Run_Resume_SkipsCompletedCheckpoints()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);

            var orchestrator = CreateOrchestrator(store);
            var result = orchestrator.Run(CreateRequest(dryRun: true, resume: true), default);

            result.PeriodsProcessed.Should().Be(2);
            result.PeriodsSkipped.Should().Be(1);
            store.Checkpoints.Count(c =>
                    c.PeriodYear == Period1.Year
                    && c.PeriodMonth == Period1.Month
                    && c.Status == EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted)
                .Should().Be(0);
        }

        [Fact]
        public void Run_Force_ReprocessesCompleted()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);

            var orchestrator = CreateOrchestrator(store);
            var result = orchestrator.Run(CreateRequest(dryRun: true, resume: true, force: true), default);

            result.PeriodsProcessed.Should().Be(3);
            result.PeriodsSkipped.Should().Be(0);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period1.Year
                && c.PeriodMonth == Period1.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted);
        }

        [Fact]
        public void Run_Restart_ClearsCheckpointsThenRuns()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);
            store.SeedGlobal(EntityTypeCode.Customer, Period2.Year, Period2.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);

            var orchestrator = CreateOrchestrator(store);
            var result = orchestrator.Run(CreateRequest(dryRun: true, restart: true), default);

            result.PeriodsProcessed.Should().Be(3);
            store.GlobalCheckpoints.Should().BeEmpty();
            store.Checkpoints.Should().HaveCount(3);
        }

        [Fact]
        public void Run_Resume_TreatsRunningAsFailed()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Running);

            var orchestrator = CreateOrchestrator(store);
            var result = orchestrator.Run(CreateRequest(dryRun: true, resume: true), default);

            result.PeriodsProcessed.Should().Be(3);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period1.Year
                && c.PeriodMonth == Period1.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted);
        }

        [Fact]
        public void Run_WithoutConfirm_ThrowsWhenNotDryRun()
        {
            var orchestrator = CreateOrchestrator(new FakeCheckpointStore());

            Action act = () => orchestrator.Run(CreateRequest(dryRun: false), default);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*--confirm BACKFILL*");
        }

        [Fact]
        public void Run_StopOnFailure_Default()
        {
            var store = new FakeCheckpointStore
            {
                FailOnPeriod = Period2
            };
            var orchestrator = CreateOrchestrator(store);

            var result = orchestrator.Run(CreateRequest(dryRun: true), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Failed);
            result.PeriodsProcessed.Should().Be(1);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period2.Year
                && c.PeriodMonth == Period2.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.Failed);
            store.Checkpoints.Should().NotContain(c =>
                c.PeriodYear == Period3.Year && c.PeriodMonth == Period3.Month);
        }

        [Fact]
        public void Run_WithConfirm_WritesCompletedCheckpoint()
        {
            var store = new FakeCheckpointStore();
            var producer = new RecordingBackfillProducer();
            var orchestrator = CreateOrchestrator(store, producer);

            var result = orchestrator.Run(CreateRequest(dryRun: false, confirmToken: "BACKFILL"), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            result.PeriodsProcessed.Should().Be(3);
            store.Checkpoints.Should().HaveCount(3);
            store.Checkpoints.Should().OnlyContain(c =>
                c.Status == EntityAnalyticsBackfillCheckpointStatus.Completed);
            producer.ProduceCount.Should().Be(3);
        }

        [Fact]
        public void Run_Force_RerunsProducerOnCompletedCheckpoint()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);

            var producer = new RecordingBackfillProducer();
            var orchestrator = CreateOrchestrator(store, producer);
            var result = orchestrator.Run(CreateRequest(
                dryRun: false,
                resume: true,
                force: true,
                confirmToken: "BACKFILL"), default);

            result.PeriodsProcessed.Should().Be(3);
            result.PeriodsSkipped.Should().Be(0);
            producer.ProduceCount.Should().Be(3);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period1.Year
                && c.PeriodMonth == Period1.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.Completed);
        }

        private static EntityAnalyticsBackfillOrchestrator CreateOrchestrator(
            FakeCheckpointStore store,
            RecordingBackfillProducer producer = null)
        {
            producer = producer ?? new RecordingBackfillProducer();
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer"
            });

            var producerOrchestrator = new EntityAnalyticsProducerOrchestrator(
                new IEntityAnalyticsProducer[] { producer },
                entityTypes);

            return new EntityAnalyticsBackfillOrchestrator(
                store,
                new FakeMutex(),
                new FixedBusinessDateProvider(new DateTime(2024, 6, 15)),
                new FakeLoaderResolver(),
                new FakeAggregateService(),
                producerOrchestrator,
                new FixedTglJamDal(new DateTime(2026, 6, 25, 12, 0, 0)),
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));
        }

        private static EntityAnalyticsBackfillRequest CreateRequest(
            bool dryRun = false,
            bool resume = true,
            bool force = false,
            bool restart = false,
            string confirmToken = null)
        {
            return new EntityAnalyticsBackfillRequest
            {
                EntityTypeScope = EntityTypeCode.Customer,
                FromPeriodYear = Period1.Year,
                FromPeriodMonth = Period1.Month,
                ToPeriodYear = Period3.Year,
                ToPeriodMonth = Period3.Month,
                DryRun = dryRun,
                Resume = resume,
                Force = force,
                Restart = restart,
                ConfirmToken = confirmToken,
                SkipLiveMutexCheck = true,
                TriggeredBy = "Manual"
            };
        }

        private sealed class RecordingBackfillProducer : IEntityAnalyticsProducer
        {
            public string EntityType => EntityTypeCode.Customer;

            public string WorkerDomain => "Customer";

            public int ProduceCount { get; private set; }

            public void Produce(EntityAnalyticsProduceContext context)
            {
                ProduceCount++;
            }
        }

        private sealed class FixedBusinessDateProvider : IBusinessDateProvider
        {
            public FixedBusinessDateProvider(DateTime today)
            {
                Today = today.Date;
            }

            public DateTime Today { get; }
            public bool IsPresentationActive => false;
        }

        private sealed class FixedTglJamDal : ITglJamDal
        {
            public FixedTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }

        private sealed class FakeLoaderResolver : IEntityAnalyticsReplayDataLoaderResolver
        {
            public IEntityAnalyticsReplayDataLoader Resolve(string entityType) => new FakeLoader(entityType);
        }

        private sealed class FakeLoader : IEntityAnalyticsReplayDataLoader
        {
            public FakeLoader(string entityType)
            {
                EntityType = entityType;
            }

            public string EntityType { get; }

            public object Load(EntityAnalyticsReplayContext replayContext) =>
                new CustomerReplayDataBundle
                {
                    FakturRows = new List<FakturView> { new FakturView() }
                };
        }

        private sealed class FakeAggregateService : IEntityAnalyticsReplayAggregateService
        {
            public EntityAnalyticsReplayAggregateResult Aggregate(
                EntityAnalyticsReplayContext replayContext,
                object bundle,
                DateTime generatedAt)
            {
                return new EntityAnalyticsReplayAggregateResult
                {
                    EntityType = replayContext.EntityTypeCode,
                    ProduceInput = new CustomerEntityAnalyticsProduceInput
                    {
                        PortfolioAggregate = new btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardCustomerPortfolioAggregateResult
                        {
                            Customers = Enumerable.Range(1, 42)
                                .Select(_ => new btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardCustomerPortfolioCustomerRow())
                                .ToList()
                        }
                    },
                    EntityCount = 42,
                    RowCounts = new EntityAnalyticsReplayRowCounts
                    {
                        TransactionRowCount = 1,
                        MasterRowCount = 42
                    }
                };
            }
        }

        private sealed class FakeMutex : IEntityAnalyticsBackfillMutex
        {
            public void Acquire(string entityType, string jobId, bool skipLiveMutexCheck)
            {
            }

            public void Release(string entityType, string jobId)
            {
            }
        }

        private sealed class FakeCheckpointStore : IEntityAnalyticsBackfillCheckpointStore
        {
            private readonly Dictionary<string, EntityAnalyticsBackfillJobModel> _jobs =
                new Dictionary<string, EntityAnalyticsBackfillJobModel>(StringComparer.Ordinal);

            private readonly List<EntityAnalyticsBackfillCheckpointModel> _jobCheckpoints =
                new List<EntityAnalyticsBackfillCheckpointModel>();

            private readonly Dictionary<string, EntityAnalyticsBackfillCheckpointModel> _globalCheckpoints =
                new Dictionary<string, EntityAnalyticsBackfillCheckpointModel>(StringComparer.Ordinal);

            public YearMonthPeriod? FailOnPeriod { get; set; }

            public IReadOnlyList<EntityAnalyticsBackfillCheckpointModel> Checkpoints => _jobCheckpoints;

            public IReadOnlyDictionary<string, EntityAnalyticsBackfillCheckpointModel> GlobalCheckpoints =>
                _globalCheckpoints;

            public void SeedGlobal(string entityType, int year, int month, string status)
            {
                _globalCheckpoints[BuildKey(entityType, year, month)] = new EntityAnalyticsBackfillCheckpointModel
                {
                    BackfillCheckpointId = Guid.NewGuid().ToString("N").Substring(0, 26),
                    BackfillJobId = "seed",
                    EntityType = entityType,
                    PeriodYear = year,
                    PeriodMonth = month,
                    Status = status,
                    LayersCompleted = "L1,L2,L5",
                    EntityCount = 1,
                    StartedAt = DateTime.Now.AddHours(-1),
                    CompletedAt = DateTime.Now.AddHours(-1)
                };
            }

            public string CreateJob(EntityAnalyticsBackfillJobModel job)
            {
                _jobs[job.BackfillJobId] = job;
                return job.BackfillJobId;
            }

            public EntityAnalyticsBackfillJobModel GetJob(string jobId)
            {
                return _jobs.TryGetValue(jobId, out var job) ? job : null;
            }

            public void UpdateJob(string jobId, Action<EntityAnalyticsBackfillJobModel> mutate)
            {
                if (!_jobs.TryGetValue(jobId, out var job))
                    throw new InvalidOperationException($"Job '{jobId}' not found.");

                mutate(job);
            }

            public EntityAnalyticsBackfillCheckpointModel GetCheckpoint(
                string jobId,
                string entityType,
                int year,
                int month)
            {
                var jobCheckpoint = _jobCheckpoints.LastOrDefault(c =>
                    c.BackfillJobId == jobId
                    && c.EntityType == entityType
                    && c.PeriodYear == year
                    && c.PeriodMonth == month);

                if (jobCheckpoint != null)
                    return jobCheckpoint;

                return _globalCheckpoints.TryGetValue(BuildKey(entityType, year, month), out var seeded)
                    ? seeded
                    : null;
            }

            public IReadOnlyList<EntityAnalyticsBackfillCheckpointModel> GetCheckpointsForJob(
                string jobId,
                string entityType)
            {
                return _jobCheckpoints
                    .Where(c => c.BackfillJobId == jobId && c.EntityType == entityType)
                    .ToList();
            }

            public void UpsertCheckpoint(EntityAnalyticsBackfillCheckpointModel checkpoint)
            {
                if (FailOnPeriod.HasValue
                    && checkpoint.PeriodYear == FailOnPeriod.Value.Year
                    && checkpoint.PeriodMonth == FailOnPeriod.Value.Month
                    && checkpoint.Status == EntityAnalyticsBackfillCheckpointStatus.Running)
                {
                    throw new InvalidOperationException("Simulated period failure.");
                }

                _jobCheckpoints.RemoveAll(c =>
                    c.BackfillJobId == checkpoint.BackfillJobId
                    && c.EntityType == checkpoint.EntityType
                    && c.PeriodYear == checkpoint.PeriodYear
                    && c.PeriodMonth == checkpoint.PeriodMonth);

                _jobCheckpoints.Add(checkpoint);
            }

            public void DeleteCheckpointsForScope(
                string jobId,
                string entityType,
                int fromYear,
                int fromMonth,
                int toYear,
                int toMonth)
            {
                var keysToRemove = _globalCheckpoints.Keys
                    .Where(key => MatchesScope(key, entityType, fromYear, fromMonth, toYear, toMonth))
                    .ToList();

                foreach (var key in keysToRemove)
                    _globalCheckpoints.Remove(key);

                _jobCheckpoints.RemoveAll(c =>
                    (string.IsNullOrWhiteSpace(jobId) || c.BackfillJobId == jobId)
                    && (string.IsNullOrWhiteSpace(entityType) || c.EntityType == entityType)
                    && IsWithinScope(c.PeriodYear, c.PeriodMonth, fromYear, fromMonth, toYear, toMonth));
            }

            private static string BuildKey(string entityType, int year, int month) =>
                $"{entityType}:{year:D4}-{month:D2}";

            private static bool MatchesScope(
                string key,
                string entityType,
                int fromYear,
                int fromMonth,
                int toYear,
                int toMonth)
            {
                var parts = key.Split(':');
                if (parts.Length != 2)
                    return false;

                if (!string.IsNullOrWhiteSpace(entityType)
                    && !string.Equals(parts[0], entityType, StringComparison.Ordinal))
                {
                    return false;
                }

                var periodParts = parts[1].Split('-');
                if (periodParts.Length != 2
                    || !int.TryParse(periodParts[0], out var year)
                    || !int.TryParse(periodParts[1], out var month))
                {
                    return false;
                }

                return IsWithinScope(year, month, fromYear, fromMonth, toYear, toMonth);
            }

            private static bool IsWithinScope(
                int year,
                int month,
                int fromYear,
                int fromMonth,
                int toYear,
                int toMonth)
            {
                var period = new YearMonthPeriod(year, month);
                var from = new YearMonthPeriod(fromYear, fromMonth);
                var to = new YearMonthPeriod(toYear, toMonth);
                return period >= from && period <= to;
            }
        }
    }
}
