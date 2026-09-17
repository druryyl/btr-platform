using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.PurchaseContext.SupplierAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.domain.PurchaseContext.SupplierAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierPrincipalReplayTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2025, 6);

        private const string TargetedSupplier = "SP02C";
        private const string UntargetedSupplier = "SP09X";

        [Fact]
        public void ReplayAggregator_UsesLiveFormulas_ForAllInScopeKpis()
        {
            var aggregator = CreateReplayAggregator();
            var bundle = CreateBundle(withTarget: true);
            var periode = new Periode(new DateTime(2025, 6, 1), new DateTime(2025, 6, 30));
            var generatedAt = new DateTime(2025, 6, 30, 12, 0, 0);

            var result = aggregator.Aggregate(bundle, periode, 2025, 6, generatedAt);

            result.MatchesPeriod(2025, 6).Should().BeTrue();

            var sales = result.SalesOut.Principals.Single(r => r.SupplierId == TargetedSupplier);
            sales.SalesOutAmount.Should().Be(1_450_000m);

            var returns = result.Returns.Principals.Single(r => r.SupplierId == TargetedSupplier);
            returns.GoodReturnAmount.Should().Be(95_000m);
            returns.BrokenReturnAmount.Should().Be(48_000m);
            returns.TotalReturnAmount.Should().Be(143_000m);

            var percentage = result.ReturnPercentage.Principals.Single(r => r.SupplierId == TargetedSupplier);
            percentage.ReturnPercentage.Should().Be(PrincipalReturnPercentageComposer.Calculate(143_000m, 1_450_000m));

            var target = result.Targets.Principals.Single(r => r.SupplierId == TargetedSupplier);
            target.TargetAmount.Should().Be(1_000_000m);

            var achievement = result.Achievement.Principals.Single(r => r.SupplierId == TargetedSupplier);
            achievement.AchievementAmount.Should().Be(450_000m);
            achievement.AchievementPercentage.Should().Be(
                PrincipalAchievementComposer.CalculatePercentage(1_450_000m, 1_000_000m));

            var purchase = result.PurchaseIn.Principals.Single(r => r.SupplierId == TargetedSupplier);
            purchase.PurchaseInAmount.Should().Be(1_000_000m);
        }

        [Fact]
        public void ReplayAggregator_OmitsTargetAndAchievement_WhenNoTargetEvidence()
        {
            var aggregator = CreateReplayAggregator();
            var bundle = CreateBundle(withTarget: true);
            var periode = new Periode(new DateTime(2025, 6, 1), new DateTime(2025, 6, 30));

            var result = aggregator.Aggregate(
                bundle, periode, 2025, 6, new DateTime(2025, 6, 30, 12, 0, 0));

            result.Targets.Principals.Should().NotContain(r => r.SupplierId == UntargetedSupplier);
            result.Achievement.Principals.Should().NotContain(r => r.SupplierId == UntargetedSupplier);

            result.SalesOut.Principals.Should().Contain(r => r.SupplierId == UntargetedSupplier);
            result.PurchaseIn.Principals.Should().Contain(r => r.SupplierId == UntargetedSupplier);
        }

        [Fact]
        public void ReplayProduce_WritesClosedMonthlyRows_ForAllInScopePrnKpis()
        {
            var repository = new ReplayPrincipalRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2025, 6, 30, 12, 0, 0);

            var replay = CreateReplayAggregator().Aggregate(
                CreateBundle(withTarget: true),
                new Periode(new DateTime(2025, 6, 1), new DateTime(2025, 6, 30)),
                2025, 6, generatedAt);

            producer.Produce(CreateReplayContext(generatedAt, Portfolio(), replay));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();

            var rows = repository.MonthlyRows
                .Where(r => r.EntityId == TargetedSupplier)
                .ToList();
            rows.Should().OnlyContain(r =>
                r.PeriodYear == 2025 && r.PeriodMonth == 6 && r.IsClosed);

            ValueOf(rows, PrincipalKpiCatalog.SalesOutId).Should().Be(1_450_000m);
            ValueOf(rows, PrincipalKpiCatalog.GoodReturnAmountId).Should().Be(95_000m);
            ValueOf(rows, PrincipalKpiCatalog.BrokenReturnAmountId).Should().Be(48_000m);
            ValueOf(rows, PrincipalKpiCatalog.TotalReturnAmountId).Should().Be(143_000m);
            ValueOf(rows, PrincipalKpiCatalog.ReturnPercentageId).Should().Be(
                PrincipalReturnPercentageComposer.Calculate(143_000m, 1_450_000m));
            ValueOf(rows, PrincipalKpiCatalog.TargetId).Should().Be(1_000_000m);
            ValueOf(rows, PrincipalKpiCatalog.AchievementAmountId).Should().Be(450_000m);
            ValueOf(rows, PrincipalKpiCatalog.AchievementPercentageId).Should().Be(
                PrincipalAchievementComposer.CalculatePercentage(1_450_000m, 1_000_000m));
            ValueOf(rows, PrincipalKpiCatalog.PurchaseInId).Should().Be(1_000_000m);

            rows.Should().NotContain(r => r.KpiId == PrincipalKpiCatalog.PacingAchievementPercentageId);
        }

        [Fact]
        public void ReplayProduce_OmitsTgtRows_ForSupplierWithoutTarget()
        {
            var repository = new ReplayPrincipalRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2025, 6, 30, 12, 0, 0);

            var replay = CreateReplayAggregator().Aggregate(
                CreateBundle(withTarget: true),
                new Periode(new DateTime(2025, 6, 1), new DateTime(2025, 6, 30)),
                2025, 6, generatedAt);

            producer.Produce(CreateReplayContext(generatedAt, Portfolio(), replay));

            var rows = repository.MonthlyRows
                .Where(r => r.EntityId == UntargetedSupplier)
                .ToList();

            rows.Should().NotContain(r => r.KpiId == PrincipalKpiCatalog.TargetId);
            rows.Should().NotContain(r => r.KpiId == PrincipalKpiCatalog.AchievementAmountId);
            rows.Should().NotContain(r => r.KpiId == PrincipalKpiCatalog.AchievementPercentageId);

            ValueOf(rows, PrincipalKpiCatalog.SalesOutId).Should().Be(200_000m);
            ValueOf(rows, PrincipalKpiCatalog.PurchaseInId).Should().Be(50_000m);
        }

        [Fact]
        public void ReplayProduce_WithoutPrincipalReplay_WritesNoPrnRows()
        {
            var repository = new ReplayPrincipalRepository();
            var producer = CreateProducer(repository);

            producer.Produce(CreateReplayContext(
                new DateTime(2025, 6, 30, 12, 0, 0), Portfolio(), principalReplay: null));

            repository.MonthlyRows.Should().NotBeEmpty();
            repository.MonthlyRows.Should().OnlyContain(r => r.KpiId.StartsWith("PU-KPI-"));
        }

        [Fact]
        public void Loader_LoadsPrincipalEvidence_ForReplayMonth()
        {
            var salesOutDal = new RecordingSalesOutEvidenceDal();
            var returnDal = new RecordingReturnEvidenceDal();
            var targetDal = new RecordingTargetEvidenceDal();
            var purchaseDal = new RecordingPurchaseEvidenceDal();

            var loader = new SupplierReplayDataLoader(
                new StubInvoiceViewDal(),
                new StubSupplierDal(),
                new StubSupplierMtdItemRollupDal(),
                salesOutDal,
                returnDal,
                targetDal,
                purchaseDal);

            var bundle = (SupplierReplayDataBundle)loader.Load(new EntityAnalyticsReplayContext
            {
                PeriodYear = 2025,
                PeriodMonth = 6,
                PeriodStart = new DateTime(2025, 6, 1),
                PeriodEnd = new DateTime(2025, 6, 30),
                EntityTypeCode = EntityTypeCode.Supplier
            });

            salesOutDal.LastPeriode.Should().NotBeNull();
            salesOutDal.LastPeriode.Tgl1.Should().Be(new DateTime(2025, 6, 1));
            returnDal.LastYear.Should().Be(2025);
            returnDal.LastMonth.Should().Be(6);
            targetDal.LastYear.Should().Be(2025);
            targetDal.LastMonth.Should().Be(6);
            purchaseDal.LastYear.Should().Be(2025);
            purchaseDal.LastMonth.Should().Be(6);

            bundle.SalesOutEvidence.Should().HaveCount(1);
            bundle.ReturnEvidence.Should().HaveCount(1);
            bundle.TargetEvidence.Should().HaveCount(1);
            bundle.PurchaseEvidence.Should().HaveCount(1);
        }

        private static decimal? ValueOf(
            IEnumerable<EntityAnalyticsMonthlyRow> rows, string kpiId)
        {
            return rows.Single(r => r.KpiId == kpiId).NumericValue;
        }

        private static List<DashboardPurchasingManagementPortfolioRow> Portfolio()
        {
            return new List<DashboardPurchasingManagementPortfolioRow>
            {
                new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = TargetedSupplier,
                    SupplierCode = "GOC",
                    SupplierName = "PT. GLORIA ORIGITA COSMETICS",
                    MtdPurchaseAmount = 10m,
                    MtdInvoiceCount = 1,
                    PostedPercent = 100m,
                    IsActiveMtd = true
                },
                new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = UntargetedSupplier,
                    SupplierCode = "UNX",
                    SupplierName = "Untargeted Supplier",
                    MtdPurchaseAmount = 5m,
                    MtdInvoiceCount = 1,
                    PostedPercent = 100m,
                    IsActiveMtd = true
                }
            };
        }

        private static SupplierReplayDataBundle CreateBundle(bool withTarget)
        {
            var bundle = new SupplierReplayDataBundle
            {
                SalesOutEvidence = new List<PrincipalSalesOutFakturItemEvidence>
                {
                    SalesLine(TargetedSupplier, "Principal C", 1_000_000m, 50_000m),
                    SalesLine(TargetedSupplier, "Principal C", 500_000m, 0m),
                    SalesLine(UntargetedSupplier, "Principal X", 200_000m, 0m)
                },
                ReturnEvidence = new List<ReturnItemEvidence>
                {
                    ReturnLine(TargetedSupplier, "Principal C", PrincipalReturnSnapshot.GoodReturnJenisRetur, 100_000m, 5_000m),
                    ReturnLine(TargetedSupplier, "Principal C", PrincipalReturnSnapshot.BrokenReturnJenisRetur, 50_000m, 2_000m)
                },
                PurchaseEvidence = new List<PurchaseDetailEvidence>
                {
                    PurchaseLine(TargetedSupplier, "Principal C", 700_000m),
                    PurchaseLine(TargetedSupplier, "Principal C", 300_000m),
                    PurchaseLine(UntargetedSupplier, "Principal X", 50_000m)
                }
            };

            bundle.TargetEvidence = withTarget
                ? new List<SalesmanPrincipalTargetEvidence>
                {
                    TargetRow(TargetedSupplier, "Principal C", 600_000m),
                    TargetRow(TargetedSupplier, "Principal C", 400_000m)
                }
                : new List<SalesmanPrincipalTargetEvidence>();

            return bundle;
        }

        private static PrincipalSalesOutFakturItemEvidence SalesLine(
            string supplierId, string supplierName, decimal subTotal, decimal disc)
        {
            return new PrincipalSalesOutFakturItemEvidence
            {
                FakturId = Guid.NewGuid().ToString("N"),
                FakturItemId = Guid.NewGuid().ToString("N"),
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = disc
            };
        }

        private static ReturnItemEvidence ReturnLine(
            string supplierId, string supplierName, string jenis, decimal subTotal, decimal disc)
        {
            return new ReturnItemEvidence
            {
                ReturJualId = Guid.NewGuid().ToString("N"),
                ReturJualItemId = Guid.NewGuid().ToString("N"),
                JenisRetur = jenis,
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = disc
            };
        }

        private static SalesmanPrincipalTargetEvidence TargetRow(
            string supplierId, string supplierName, decimal amount)
        {
            return new SalesmanPrincipalTargetEvidence
            {
                SalesPersonId = "SP001",
                SupplierId = supplierId,
                SupplierName = supplierName,
                TargetYear = 2025,
                TargetMonth = 6,
                TargetAmount = amount
            };
        }

        private static PurchaseDetailEvidence PurchaseLine(
            string supplierId, string supplierName, decimal total)
        {
            return new PurchaseDetailEvidence
            {
                InvoiceId = Guid.NewGuid().ToString("N"),
                InvoiceItemId = Guid.NewGuid().ToString("N"),
                SupplierId = supplierId,
                SupplierName = supplierName,
                PurchaseDetailTotal = total
            };
        }

        private static SupplierPrincipalReplayAggregator CreateReplayAggregator()
        {
            return new SupplierPrincipalReplayAggregator(
                new PrincipalSalesOutAggregator(),
                new PrincipalReturnAggregator(),
                new PrincipalReturnPercentageComposer(),
                new PrincipalTargetAggregator(),
                new PrincipalAchievementComposer(),
                new PrincipalPurchaseInAggregator());
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            IReadOnlyList<DashboardPurchasingManagementPortfolioRow> suppliers,
            SupplierPrincipalReplayResult principalReplay)
        {
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Supplier,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            return EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new SupplierEntityAnalyticsProduceInput
                {
                    ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                    {
                        Portfolio = suppliers.ToList()
                    },
                    PrincipalReplay = principalReplay
                },
                "refresh-replay",
                generatedAt);
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(ReplayPrincipalRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.SupplierAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SupplierRelationshipCatalog.Register(relationships);

            return new SupplierEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                new EntityRankingEngine(
                    repository,
                    registry,
                    entityTypes,
                    Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 })),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(repository, relationships, entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                attentionSignals);
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class ReplayPrincipalRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return CurrentRows.Where(r => r.EntityType == entityType && r.EntityId == entityId).ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(
                string entityType,
                IEnumerable<EntityAnalyticsCurrentRow> rows,
                string refreshLogId)
            {
                ReplaceCurrentMetricsCalled = true;
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Count > 0;
        }

        private sealed class RecordingSalesOutEvidenceDal : IPrincipalSalesOutEvidenceDal
        {
            public Periode LastPeriode { get; private set; }

            public IReadOnlyList<PrincipalSalesOutFakturItemEvidence> ListFakturItemEvidence(Periode periode)
            {
                LastPeriode = periode;
                return new List<PrincipalSalesOutFakturItemEvidence>
                {
                    SalesLine(TargetedSupplier, "Principal C", 10m, 0m)
                };
            }

            public IReadOnlyList<PrincipalSalesOutFakturItemEvidenceLine> ListFakturItemEvidenceForPrincipal(
                Periode periode, string supplierId)
            {
                return Array.Empty<PrincipalSalesOutFakturItemEvidenceLine>();
            }
        }

        private sealed class RecordingReturnEvidenceDal : IPrincipalReturnEvidenceDal
        {
            public int LastYear { get; private set; }

            public int LastMonth { get; private set; }

            public IReadOnlyList<ReturnItemEvidence> ListReturnItemEvidence(int year, int month)
            {
                LastYear = year;
                LastMonth = month;
                return new List<ReturnItemEvidence>
                {
                    ReturnLine(TargetedSupplier, "Principal C", PrincipalReturnSnapshot.GoodReturnJenisRetur, 10m, 0m)
                };
            }

            public IReadOnlyList<PrincipalReturnItemEvidenceLine> ListReturnItemEvidenceForPrincipal(
                int year, int month, string supplierId)
            {
                return Array.Empty<PrincipalReturnItemEvidenceLine>();
            }
        }

        private sealed class RecordingTargetEvidenceDal : IPrincipalTargetEvidenceDal
        {
            public int LastYear { get; private set; }

            public int LastMonth { get; private set; }

            public IReadOnlyList<SalesmanPrincipalTargetEvidence> ListSalesmanPrincipalTargets(int year, int month)
            {
                LastYear = year;
                LastMonth = month;
                return new List<SalesmanPrincipalTargetEvidence>
                {
                    TargetRow(TargetedSupplier, "Principal C", 10m)
                };
            }
        }

        private sealed class RecordingPurchaseEvidenceDal : IPrincipalPurchaseInEvidenceDal
        {
            public int LastYear { get; private set; }

            public int LastMonth { get; private set; }

            public IReadOnlyList<PurchaseDetailEvidence> ListPurchaseDetail(int year, int month)
            {
                LastYear = year;
                LastMonth = month;
                return new List<PurchaseDetailEvidence>
                {
                    PurchaseLine(TargetedSupplier, "Principal C", 10m)
                };
            }
        }

        private sealed class StubInvoiceViewDal : IInvoiceViewDal
        {
            public IEnumerable<InvoiceView> ListData(Periode periode) => Array.Empty<InvoiceView>();
        }

        private sealed class StubSupplierDal : ISupplierDal
        {
            public void Insert(SupplierModel model) => throw new NotSupportedException();
            public void Update(SupplierModel model) => throw new NotSupportedException();
            public void Delete(ISupplierKey key) => throw new NotSupportedException();
            public SupplierModel GetData(ISupplierKey key) => throw new NotSupportedException();
            public IEnumerable<SupplierModel> ListData() => Array.Empty<SupplierModel>();
        }

        private sealed class StubSupplierMtdItemRollupDal : ISupplierMtdItemRollupDal
        {
            public IEnumerable<SupplierMtdItemRollupDto> ListMtdItemRollups(Periode periode) =>
                Array.Empty<SupplierMtdItemRollupDto>();

            public IEnumerable<SupplierCatalogCountDto> ListSupplierCatalogCounts() =>
                Array.Empty<SupplierCatalogCountDto>();
        }
    }
}
