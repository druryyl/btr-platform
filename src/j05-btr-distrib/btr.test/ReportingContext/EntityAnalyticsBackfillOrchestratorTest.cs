using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        [Fact]
        public void Run_WithConfirm_Salesman_WritesCompletedCheckpoint()
        {
            var store = new FakeCheckpointStore();
            var producer = new RecordingBackfillProducer(EntityTypeCode.Salesman);
            var orchestrator = CreateOrchestrator(store, producer);

            var result = orchestrator.Run(CreateRequest(
                entityTypeScope: EntityTypeCode.Salesman,
                dryRun: false,
                confirmToken: "BACKFILL"), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            result.PeriodsProcessed.Should().Be(3);
            store.Checkpoints.Should().HaveCount(3);
            store.Checkpoints.Should().OnlyContain(c =>
                c.Status == EntityAnalyticsBackfillCheckpointStatus.Completed);
            producer.ProduceCount.Should().Be(3);
        }

        [Fact]
        public void Run_WithConfirm_Supplier_WritesCompletedCheckpoint()
        {
            var store = new FakeCheckpointStore();
            var producer = new RecordingBackfillProducer(EntityTypeCode.Supplier);
            var orchestrator = CreateOrchestrator(store, producer);

            var result = orchestrator.Run(CreateRequest(
                entityTypeScope: EntityTypeCode.Supplier,
                dryRun: false,
                confirmToken: "BACKFILL"), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            result.PeriodsProcessed.Should().Be(3);
            store.Checkpoints.Should().HaveCount(3);
            store.Checkpoints.Should().OnlyContain(c =>
                c.Status == EntityAnalyticsBackfillCheckpointStatus.Completed);
            producer.ProduceCount.Should().Be(3);
        }

        [Fact]
        public void Run_WithConfirm_Item_WritesCompletedCheckpoint()
        {
            var store = new FakeCheckpointStore();
            var producer = new RecordingBackfillProducer(EntityTypeCode.Item);
            var orchestrator = CreateOrchestrator(store, producer);

            var result = orchestrator.Run(CreateRequest(
                entityTypeScope: EntityTypeCode.Item,
                dryRun: false,
                confirmToken: "BACKFILL"), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            result.PeriodsProcessed.Should().Be(3);
            store.Checkpoints.Should().HaveCount(3);
            store.Checkpoints.Should().OnlyContain(c =>
                c.Status == EntityAnalyticsBackfillCheckpointStatus.Completed);
            producer.ProduceCount.Should().Be(3);
        }

        [Fact]
        public void Run_DryRun_Salesman_FastPath_SkipsLoader()
        {
            var store = new FakeCheckpointStore();
            var loader = new TrackingLoader(EntityTypeCode.Salesman);
            var orchestrator = CreateOrchestrator(
                store,
                salesmanHandler: new FakeSalesmanReplayPeriodHandler(useFastPath: true, entityCount: 5),
                loaderResolver: new SingleLoaderResolver(loader));

            var result = orchestrator.Run(CreateRequest(
                entityTypeScope: EntityTypeCode.Salesman,
                dryRun: true), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Succeeded);
            loader.LoadCount.Should().Be(0);
            store.Checkpoints.Should().OnlyContain(c => c.EntityCount == 5);
        }

        [Fact]
        public void Run_CrossJobResume_SkipsPriorCompleted()
        {
            var store = new FakeCheckpointStore();
            store.SeedGlobal(EntityTypeCode.Customer, Period1.Year, Period1.Month,
                EntityAnalyticsBackfillCheckpointStatus.Completed);

            var orchestrator = CreateOrchestrator(store);
            var result = orchestrator.Run(CreateRequest(dryRun: true, resume: true), default);

            result.PeriodsProcessed.Should().Be(2);
            result.PeriodsSkipped.Should().Be(1);
        }

        [Fact]
        public void Run_ContinueOnError_ProcessesRemainingPeriods()
        {
            var store = new FakeCheckpointStore
            {
                FailOnPeriod = Period2
            };
            var orchestrator = CreateOrchestrator(store);

            var result = orchestrator.Run(CreateRequest(dryRun: true, continueOnError: true), default);

            result.Status.Should().Be(EntityAnalyticsBackfillJobStatus.Failed);
            result.PeriodsProcessed.Should().Be(2);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period2.Year
                && c.PeriodMonth == Period2.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.Failed);
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period3.Year
                && c.PeriodMonth == Period3.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.DryRunCompleted);
        }

        [Fact]
        public void Run_Cancellation_MarksCheckpointCancelled()
        {
            var store = new FakeCheckpointStore();
            var orchestrator = CreateOrchestrator(
                store,
                aggregateService: new CancellingAggregateService(Period2));

            Action act = () => orchestrator.Run(CreateRequest(dryRun: true), default);

            act.Should().Throw<OperationCanceledException>();
            store.Checkpoints.Should().Contain(c =>
                c.PeriodYear == Period2.Year
                && c.PeriodMonth == Period2.Month
                && c.Status == EntityAnalyticsBackfillCheckpointStatus.Cancelled);
        }

        [Fact]
        public void Run_Force_RerunPreservesRowCounts()
        {
            var store = new FakeCheckpointStore();
            var orchestrator = CreateOrchestrator(store);
            var request = CreateRequest(dryRun: true, resume: true, force: true);

            orchestrator.Run(request, default);
            var firstRunCheckpoint = store.Checkpoints
                .Single(c => c.PeriodYear == Period1.Year && c.PeriodMonth == Period1.Month);

            orchestrator.Run(request, default);
            var secondRunCheckpoint = store.Checkpoints
                .Where(c => c.PeriodYear == Period1.Year && c.PeriodMonth == Period1.Month)
                .OrderByDescending(c => c.StartedAt)
                .First();

            secondRunCheckpoint.EntityCount.Should().Be(firstRunCheckpoint.EntityCount);
            secondRunCheckpoint.RowCountsJson.Should().Be(firstRunCheckpoint.RowCountsJson);
        }

        private static EntityAnalyticsBackfillOrchestrator CreateOrchestrator(
            FakeCheckpointStore store,
            RecordingBackfillProducer producer = null,
            ISalesmanReplayPeriodHandler salesmanHandler = null,
            IEntityAnalyticsReplayDataLoaderResolver loaderResolver = null,
            IEntityAnalyticsReplayAggregateService aggregateService = null)
        {
            producer = producer ?? new RecordingBackfillProducer();
            salesmanHandler = salesmanHandler ?? new FakeSalesmanReplayPeriodHandler();
            loaderResolver = loaderResolver ?? new FakeLoaderResolver();
            aggregateService = aggregateService ?? new FakeAggregateService();
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                DisplayName = "Item"
            });

            var producerOrchestrator = new EntityAnalyticsProducerOrchestrator(
                new IEntityAnalyticsProducer[] { producer },
                entityTypes);

            return new EntityAnalyticsBackfillOrchestrator(
                store,
                new FakeMutex(),
                new FixedBusinessDateProvider(new DateTime(2024, 6, 15)),
                loaderResolver,
                aggregateService,
                salesmanHandler,
                producerOrchestrator,
                new FixedTglJamDal(new DateTime(2026, 6, 25, 12, 0, 0)),
                new EmptyCustomerDal(),
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));
        }

        private static EntityAnalyticsBackfillRequest CreateRequest(
            string entityTypeScope = EntityTypeCode.Customer,
            bool dryRun = false,
            bool resume = true,
            bool force = false,
            bool restart = false,
            bool continueOnError = false,
            string confirmToken = null)
        {
            return new EntityAnalyticsBackfillRequest
            {
                EntityTypeScope = entityTypeScope,
                FromPeriodYear = Period1.Year,
                FromPeriodMonth = Period1.Month,
                ToPeriodYear = Period3.Year,
                ToPeriodMonth = Period3.Month,
                DryRun = dryRun,
                Resume = resume,
                Force = force,
                Restart = restart,
                ContinueOnError = continueOnError,
                ConfirmToken = confirmToken,
                SkipLiveMutexCheck = true,
                TriggeredBy = "Manual"
            };
        }

        private sealed class RecordingBackfillProducer : IEntityAnalyticsProducer
        {
            public RecordingBackfillProducer(string entityType = EntityTypeCode.Customer)
            {
                EntityType = entityType;
            }

            public string EntityType { get; }

            public string WorkerDomain => EntityType;

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

        private sealed class SingleLoaderResolver : IEntityAnalyticsReplayDataLoaderResolver
        {
            private readonly IEntityAnalyticsReplayDataLoader _loader;

            public SingleLoaderResolver(IEntityAnalyticsReplayDataLoader loader)
            {
                _loader = loader;
            }

            public IEntityAnalyticsReplayDataLoader Resolve(string entityType) => _loader;
        }

        private sealed class TrackingLoader : IEntityAnalyticsReplayDataLoader
        {
            public TrackingLoader(string entityType)
            {
                EntityType = entityType;
            }

            public string EntityType { get; }

            public int LoadCount { get; private set; }

            public object Load(EntityAnalyticsReplayContext replayContext)
            {
                LoadCount++;
                return new CustomerReplayDataBundle();
            }
        }

        private sealed class FakeSalesmanReplayPeriodHandler : ISalesmanReplayPeriodHandler
        {
            private readonly bool _useFastPath;
            private readonly int _entityCount;

            public FakeSalesmanReplayPeriodHandler(bool useFastPath = false, int entityCount = 0)
            {
                _useFastPath = useFastPath;
                _entityCount = entityCount;
            }

            public bool CanUseFastPath(int periodYear, int periodMonth) => _useFastPath;

            public EntityAnalyticsReplayAggregateResult BuildFastPathPlan(
                int periodYear,
                int periodMonth,
                string refreshLogId,
                DateTime generatedAt)
            {
                return new EntityAnalyticsReplayAggregateResult
                {
                    EntityType = EntityTypeCode.Salesman,
                    ProduceInput = CreateLayersOnlyInput(),
                    EntityCount = _entityCount,
                    RowCounts = new EntityAnalyticsReplayRowCounts
                    {
                        MasterRowCount = _entityCount
                    }
                };
            }

            public void PersistFastPathL1(
                int periodYear,
                int periodMonth,
                string refreshLogId,
                DateTime generatedAt)
            {
            }

            public SalesmanEntityAnalyticsProduceInput CreateLayersOnlyInput()
            {
                return new SalesmanEntityAnalyticsProduceInput
                {
                    SalesmanAggregate = new DashboardSalesmanAggregateResult
                    {
                        Portfolio = new List<DashboardSalesmanPortfolioRow>()
                    }
                };
            }
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

        private class FakeAggregateService : IEntityAnalyticsReplayAggregateService
        {
            public virtual EntityAnalyticsReplayAggregateResult Aggregate(
                EntityAnalyticsReplayContext replayContext,
                object bundle,
                DateTime generatedAt)
            {
                return BuildResult(replayContext.EntityTypeCode);
            }

            protected EntityAnalyticsReplayAggregateResult BuildResult(string entityTypeCode)
            {
                if (string.Equals(entityTypeCode, EntityTypeCode.Salesman, StringComparison.OrdinalIgnoreCase))
                {
                    return new EntityAnalyticsReplayAggregateResult
                    {
                        EntityType = entityTypeCode,
                        ProduceInput = new SalesmanEntityAnalyticsProduceInput
                        {
                            SalesmanAggregate = new DashboardSalesmanAggregateResult
                            {
                                Portfolio = Enumerable.Range(1, 42)
                                    .Select(_ => new DashboardSalesmanPortfolioRow())
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

                if (string.Equals(entityTypeCode, EntityTypeCode.Supplier, StringComparison.OrdinalIgnoreCase))
                {
                    return new EntityAnalyticsReplayAggregateResult
                    {
                        EntityType = entityTypeCode,
                        ProduceInput = new SupplierEntityAnalyticsProduceInput
                        {
                            ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                            {
                                Portfolio = Enumerable.Range(1, 42)
                                    .Select(_ => new DashboardPurchasingManagementPortfolioRow())
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

                if (string.Equals(entityTypeCode, EntityTypeCode.Item, StringComparison.OrdinalIgnoreCase))
                {
                    return new EntityAnalyticsReplayAggregateResult
                    {
                        EntityType = entityTypeCode,
                        ProduceInput = new ItemEntityAnalyticsProduceInput
                        {
                            Portfolio = Enumerable.Range(1, 42)
                                .Select(_ => new DashboardItemPortfolioRow())
                                .ToList()
                        },
                        EntityCount = 42,
                        RowCounts = new EntityAnalyticsReplayRowCounts
                        {
                            TransactionRowCount = 1,
                            MasterRowCount = 42
                        }
                    };
                }

                return new EntityAnalyticsReplayAggregateResult
                {
                    EntityType = entityTypeCode,
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

        private sealed class CancellingAggregateService : FakeAggregateService
        {
            private readonly YearMonthPeriod _cancelOnPeriod;

            public CancellingAggregateService(YearMonthPeriod cancelOnPeriod)
            {
                _cancelOnPeriod = cancelOnPeriod;
            }

            public override EntityAnalyticsReplayAggregateResult Aggregate(
                EntityAnalyticsReplayContext replayContext,
                object bundle,
                DateTime generatedAt)
            {
                if (replayContext.PeriodYear == _cancelOnPeriod.Year
                    && replayContext.PeriodMonth == _cancelOnPeriod.Month)
                {
                    throw new OperationCanceledException();
                }

                return base.Aggregate(replayContext, bundle, generatedAt);
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

            public EntityAnalyticsBackfillCheckpointModel GetLatestCheckpoint(
                string entityType,
                int year,
                int month)
            {
                return _jobCheckpoints
                    .Where(c =>
                        c.EntityType == entityType
                        && c.PeriodYear == year
                        && c.PeriodMonth == month)
                    .Concat(_globalCheckpoints.TryGetValue(BuildKey(entityType, year, month), out var seeded)
                        ? new[] { seeded }
                        : Array.Empty<EntityAnalyticsBackfillCheckpointModel>())
                    .OrderByDescending(c => c.StartedAt)
                    .FirstOrDefault();
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

        private sealed class EmptyCustomerDal : btr.application.SalesContext.CustomerAgg.Contracts.ICustomerDal
        {
            public IEnumerable<btr.domain.SalesContext.CustomerAgg.CustomerModel> ListData()
                => Array.Empty<btr.domain.SalesContext.CustomerAgg.CustomerModel>();

            public IEnumerable<btr.application.SalesContext.CustomerAgg.Contracts.CustomerLocationView> ListLocation()
                => Array.Empty<btr.application.SalesContext.CustomerAgg.Contracts.CustomerLocationView>();

            public void Insert(btr.domain.SalesContext.CustomerAgg.CustomerModel data) => throw new NotSupportedException();
            public void Update(btr.domain.SalesContext.CustomerAgg.CustomerModel data) => throw new NotSupportedException();
            public void Delete(btr.domain.SalesContext.CustomerAgg.ICustomerKey key) => throw new NotSupportedException();
            public btr.domain.SalesContext.CustomerAgg.CustomerModel GetData(btr.domain.SalesContext.CustomerAgg.ICustomerKey key)
                => throw new NotSupportedException();
        }
    }
}
