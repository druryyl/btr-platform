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
    public class SupplierPrincipalGrowthCompositionTest
    {
        [Fact]
        public void Produce_ComposesStoredMomGrowthAndYoyGrowthForTheSamePrincipalAndPeriod()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                MomGrowth(2026, 6, MomGrowthRow("S001", "Principal A", 0.12m)),
                YoyGrowth(2026, 6, YoyGrowthRow("S001", "Principal A", 0.25m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.MomGrowthId
                && row.NumericValue == 0.12m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.YoyGrowthId
                && row.NumericValue == 0.25m);
        }

        [Fact]
        public void Produce_DoesNotChangeStoredSalesOutValue()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                MomGrowth(2026, 6, MomGrowthRow("S001", "Principal A", 0.12m)),
                YoyGrowth(2026, 6, YoyGrowthRow("S001", "Principal A", 0.25m)));

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
        public void Produce_GrowthDoesNotUsePurchaseIn()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                MomGrowth(2026, 6, MomGrowthRow("S001", "Principal A", 0.12m)),
                YoyGrowth(2026, 6, YoyGrowthRow("S001", "Principal A", 0.25m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            var growthRows = repository.Rows.Where(row =>
                    row.EntityId == "S001"
                    && (row.KpiId == PrincipalKpiCatalog.MomGrowthId
                        || row.KpiId == PrincipalKpiCatalog.YoyGrowthId))
                .ToList();
            growthRows.Should().HaveCount(2);
            repository.Rows.Should().NotContain(row =>
                row.EntityId == "S001"
                && row.KpiId.StartsWith("PRN-PUR-", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Produce_PurchaseRefreshRetainsPersistedGrowth()
        {
            var repository = new RecordingRepository();
            var salesOut = new FakeSalesOutSnapshotDal(SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)));
            var momGrowth = new FakeMomGrowthSnapshotDal(MomGrowth(2026, 6, MomGrowthRow("S001", "Principal A", 0.12m)));
            var yoyGrowth = new FakeYoyGrowthSnapshotDal(YoyGrowth(2026, 6, YoyGrowthRow("S001", "Principal A", 0.25m)));
            var producer = CreateProducer(repository, salesOut, momGrowth, yoyGrowth);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            salesOut.Current = null;
            momGrowth.Current = null;
            yoyGrowth.Current = null;

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 250m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 1000m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.MomGrowthId
                && row.NumericValue == 0.12m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.YoyGrowthId
                && row.NumericValue == 0.25m);
        }

        [Fact]
        public void SupplierGrowthPack_MetadataIsGrowthCategoryAndEvidenceIsSalesOut()
        {
            var registry = CreateRegistry();

            var packIds = registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId).ToList();
            packIds.Should().Contain(PrincipalKpiCatalog.MomGrowthId);
            packIds.Should().Contain(PrincipalKpiCatalog.YoyGrowthId);

            registry.TryGetMetadata(PrincipalKpiCatalog.MomGrowthId, out var mom).Should().BeTrue();
            mom.DisplayName.Should().Be("Month-over-Month Growth Percentage");
            mom.Category.Should().Be(EntityKpiCategory.Growth);
            mom.RankEligible.Should().BeTrue();
            mom.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute);
            mom.SourceDomain.Should().Be(PrincipalMomGrowthSnapshot.Domain);
            mom.Description.Should().Contain("PRN-SALES-001");
            mom.Description.Should().Contain("does not use Purchase-In");
            mom.Description.Should().Contain("not Net Sales");

            registry.TryGetMetadata(PrincipalKpiCatalog.YoyGrowthId, out var yoy).Should().BeTrue();
            yoy.DisplayName.Should().Be("Year-over-Year Growth Percentage");
            yoy.Category.Should().Be(EntityKpiCategory.Growth);
            yoy.RankEligible.Should().BeTrue();
            yoy.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute);
            yoy.SourceDomain.Should().Be(PrincipalYoyGrowthSnapshot.Domain);
            yoy.Description.Should().Contain("PRN-SALES-001");
            yoy.Description.Should().Contain("does not use Purchase-In");
            yoy.Description.Should().Contain("not Net Sales");

            registry.TryGetMetadata(EntityAnalyticsRadarAxisIds.GrowthMom, out var radar).Should().BeTrue();
            radar.RadarSourceKpiId.Should().Be(PrincipalKpiCatalog.MomGrowthId);
            radar.RadarValueSource.Should().Be(RadarValueSource.L0Kpi);
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

        private static PrincipalMomGrowthResult MomGrowth(int year, int month, params PrincipalMomGrowthRow[] rows)
        {
            return new PrincipalMomGrowthResult
            {
                MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalMomGrowthRow MomGrowthRow(string supplierId, string name, decimal? percentage)
        {
            return new PrincipalMomGrowthRow
            {
                MomGrowthKpiId = PrincipalKpiCatalog.MomGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                MomGrowthPercentage = percentage
            };
        }

        private static PrincipalYoyGrowthResult YoyGrowth(int year, int month, params PrincipalYoyGrowthRow[] rows)
        {
            return new PrincipalYoyGrowthResult
            {
                YoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalYoyGrowthRow YoyGrowthRow(string supplierId, string name, decimal? percentage)
        {
            return new PrincipalYoyGrowthRow
            {
                YoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                YoyGrowthPercentage = percentage
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
            PrincipalMomGrowthResult momGrowth,
            PrincipalYoyGrowthResult yoyGrowth)
        {
            return CreateProducer(
                repository,
                new FakeSalesOutSnapshotDal(salesOut),
                new FakeMomGrowthSnapshotDal(momGrowth),
                new FakeYoyGrowthSnapshotDal(yoyGrowth));
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(
            RecordingRepository repository,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            IPrincipalMomGrowthSnapshotDal momGrowthSnapshotDal,
            IPrincipalYoyGrowthSnapshotDal yoyGrowthSnapshotDal)
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
                momGrowthSnapshotDal: momGrowthSnapshotDal,
                yoyGrowthSnapshotDal: yoyGrowthSnapshotDal);
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

        private sealed class FakeMomGrowthSnapshotDal : IPrincipalMomGrowthSnapshotDal
        {
            public FakeMomGrowthSnapshotDal(PrincipalMomGrowthResult current)
            {
                Current = current;
            }

            public PrincipalMomGrowthResult Current { get; set; }

            public PrincipalMomGrowthResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalMomGrowthResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeYoyGrowthSnapshotDal : IPrincipalYoyGrowthSnapshotDal
        {
            public FakeYoyGrowthSnapshotDal(PrincipalYoyGrowthResult current)
            {
                Current = current;
            }

            public PrincipalYoyGrowthResult Current { get; set; }

            public PrincipalYoyGrowthResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalYoyGrowthResult result, string refreshLogId)
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
