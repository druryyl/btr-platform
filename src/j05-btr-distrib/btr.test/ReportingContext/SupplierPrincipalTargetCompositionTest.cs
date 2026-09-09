using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
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
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierPrincipalTargetCompositionTest
    {
        [Fact]
        public void Produce_ComposesStoredTargetAndAchievementForTheSamePrincipalAndPeriod()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                achievement: Achievements(2026, 6, AchievementRow("S001", "Principal A", 1000m, 800m, 200m, 1.25m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.TargetId
                && row.NumericValue == 800m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.AchievementAmountId
                && row.NumericValue == 200m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.AchievementPercentageId
                && row.NumericValue == 1.25m);
        }

        [Fact]
        public void Produce_DoesNotReplacePrincipalSalesOutWithAchievement()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                achievement: Achievements(2026, 6, AchievementRow("S001", "Principal A", 1000m, 800m, 200m, 1.25m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 1000m);
            repository.Rows.Where(row =>
                    row.EntityId == "S001"
                    && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .Should().HaveCount(1);
        }

        [Fact]
        public void Produce_DoesNotAddReturnGrowthPurchaseOrInventoryPacks()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                achievement: Achievements(2026, 6, AchievementRow("S001", "Principal A", 1000m, 800m, 200m, 1.25m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            var kpiIds = repository.Rows.Select(row => row.KpiId).ToList();
            kpiIds.Should().Contain(PrincipalKpiCatalog.TargetId);
            kpiIds.Should().Contain(PrincipalKpiCatalog.AchievementAmountId);
            kpiIds.Should().Contain(PrincipalKpiCatalog.AchievementPercentageId);
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-RET-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-GRW-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-PUR-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-INV-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-CUS-", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Produce_PurchaseRefreshRetainsPersistedTargetAndAchievement()
        {
            var repository = new RecordingRepository();
            var salesOut = new FakeSalesOutSnapshotDal(SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)));
            var target = new FakeTargetSnapshotDal(Targets(2026, 6, TargetRow("S001", "Principal A", 800m)));
            var achievement = new FakeAchievementSnapshotDal(Achievements(2026, 6, AchievementRow("S001", "Principal A", 1000m, 800m, 200m, 1.25m)));
            var producer = CreateProducer(repository, salesOut, target, achievement);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            salesOut.Current = null;
            target.Current = null;
            achievement.Current = null;

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 250m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 1000m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.TargetId
                && row.NumericValue == 800m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.AchievementAmountId
                && row.NumericValue == 200m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.AchievementPercentageId
                && row.NumericValue == 1.25m);
        }

        [Fact]
        public void SupplierTargetPack_MetadataIsFinancialAndSupportingRankingOnlyForPercentage()
        {
            var registry = CreateRegistry();

            var packIds = registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId).ToList();
            packIds.First().Should().Be(PrincipalKpiCatalog.SalesOutId);
            packIds.Should().Contain(new[]
            {
                PrincipalKpiCatalog.TargetId,
                PrincipalKpiCatalog.AchievementAmountId,
                PrincipalKpiCatalog.AchievementPercentageId
            });

            registry.TryGetMetadata(PrincipalKpiCatalog.TargetId, out var target).Should().BeTrue();
            target.DisplayName.Should().Be("Principal Target");
            target.RankEligible.Should().BeFalse();
            target.SourceDomain.Should().Be(PrincipalTargetSnapshot.Domain);

            registry.TryGetMetadata(PrincipalKpiCatalog.AchievementAmountId, out var amount).Should().BeTrue();
            amount.DisplayName.Should().Be("Achievement Amount");
            amount.RankEligible.Should().BeFalse();
            amount.SourceDomain.Should().Be(PrincipalAchievementSnapshot.Domain);
            amount.Description.Should().Contain("not Net Sales");

            registry.TryGetMetadata(PrincipalKpiCatalog.AchievementPercentageId, out var percentage).Should().BeTrue();
            percentage.DisplayName.Should().Be("Achievement Percentage");
            percentage.RankEligible.Should().BeTrue("PRN-TGT-003 is a supporting ranking KPI");
            percentage.SourceDomain.Should().Be(PrincipalAchievementSnapshot.Domain);
            percentage.Description.Should().Contain("not Net Sales");
        }

        private static PrincipalSalesOutAggregateResult SalesOut(int year, int month, params PrincipalSalesOutRow[] rows)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalSalesOutRow OwnedSalesOut(string supplierId, string name, decimal amount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = amount
            };
        }

        private static PrincipalTargetAggregateResult Targets(int year, int month, params PrincipalTargetRow[] rows)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalTargetRow TargetRow(string supplierId, string name, decimal targetAmount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = name,
                TargetAmount = targetAmount
            };
        }

        private static PrincipalAchievementResult Achievements(int year, int month, params PrincipalAchievementRow[] rows)
        {
            return new PrincipalAchievementResult
            {
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalAchievementRow AchievementRow(
            string supplierId, string name, decimal salesOut, decimal target, decimal? amount, decimal? percentage)
        {
            return new PrincipalAchievementRow
            {
                AchievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId,
                AchievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TargetKpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = salesOut,
                TargetAmount = target,
                AchievementAmount = amount,
                AchievementPercentage = percentage
            };
        }

        private static EntityAnalyticsKpiRegistry CreateRegistry()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId
            });
            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());
            return registry;
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(
            RecordingRepository repository,
            PrincipalSalesOutAggregateResult salesOut,
            PrincipalTargetAggregateResult target,
            PrincipalAchievementResult achievement)
        {
            return CreateProducer(
                repository,
                new FakeSalesOutSnapshotDal(salesOut),
                new FakeTargetSnapshotDal(target),
                new FakeAchievementSnapshotDal(achievement));
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(
            RecordingRepository repository,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            IPrincipalTargetSnapshotDal targetSnapshotDal,
            IPrincipalAchievementSnapshotDal achievementSnapshotDal)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SupplierRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.SupplierAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));
            var attentionSignals = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(attentionSignals);

            return new SupplierEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(
                    repository,
                    new EntityRelationshipDefinitionRegistry(entityTypes),
                    entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                attentionSignals,
                salesOutSnapshotDal,
                null,
                null,
                targetSnapshotDal,
                achievementSnapshotDal);
        }

        private static EntityAnalyticsProduceContext CreateContext(
            DateTime generatedAt,
            params DashboardPurchasingManagementPortfolioRow[] suppliers)
        {
            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-1",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new SupplierEntityAnalyticsProduceInput
                {
                    ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                    {
                        Portfolio = suppliers.ToList()
                    }
                }
            };
        }

        private static DashboardPurchasingManagementPortfolioRow Portfolio(
            string supplierId,
            string supplierCode,
            string name,
            decimal purchaseAmount = 100m)
        {
            return new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = supplierId,
                SupplierCode = supplierCode,
                SupplierName = name,
                PrincipalName = name,
                MtdPurchaseAmount = purchaseAmount,
                MtdInvoiceCount = 1,
                PostedPercent = 100m,
                IsActiveMtd = true
            };
        }

        private sealed class FakeSalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
        {
            public FakeSalesOutSnapshotDal(PrincipalSalesOutAggregateResult current)
            {
                Current = current;
            }

            public PrincipalSalesOutAggregateResult Current { get; set; }

            public PrincipalSalesOutAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeTargetSnapshotDal : IPrincipalTargetSnapshotDal
        {
            public FakeTargetSnapshotDal(PrincipalTargetAggregateResult current)
            {
                Current = current;
            }

            public PrincipalTargetAggregateResult Current { get; set; }

            public PrincipalTargetAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeAchievementSnapshotDal : IPrincipalAchievementSnapshotDal
        {
            public FakeAchievementSnapshotDal(PrincipalAchievementResult current)
            {
                Current = current;
            }

            public PrincipalAchievementResult Current { get; set; }

            public PrincipalAchievementResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalAchievementResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class RecordingRepository : EntityAnalyticsRepositoryStubBase
        {
            public List<EntityAnalyticsCurrentRow> Rows { get; } = new List<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => Rows.Count > 0;

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return Rows.Where(row =>
                        row.EntityType == entityType
                        && (string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(row.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            public override void ReplaceCurrentMetrics(
                string entityType,
                IEnumerable<EntityAnalyticsCurrentRow> rows,
                string refreshLogId)
            {
                Rows.Clear();
                Rows.AddRange(rows ?? Array.Empty<EntityAnalyticsCurrentRow>());
                CurrentRows.Clear();
                CurrentRows.AddRange(Rows);
            }
        }
    }
}
