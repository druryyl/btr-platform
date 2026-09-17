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
    public class SupplierPrincipalPurchaseInCompositionTest
    {
        [Fact]
        public void Produce_ComposesStoredPurchaseInForTheSamePrincipalAndPeriod()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                PurchaseIn(2026, 6, PurchaseInRow("S001", "Principal A", 750m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.PurchaseInId
                && row.NumericValue == 750m);
        }

        [Fact]
        public void Produce_DoesNotChangeSalesOutOrGrowth()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                PurchaseIn(2026, 6, PurchaseInRow("S001", "Principal A", 750m)));

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
            repository.Rows.Should().NotContain(row =>
                row.KpiId == PrincipalKpiCatalog.MomGrowthId
                || row.KpiId == PrincipalKpiCatalog.YoyGrowthId);
        }

        [Fact]
        public void Produce_DoesNotAddReturnTargetInventoryOrCoveragePacks()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                PurchaseIn(2026, 6, PurchaseInRow("S001", "Principal A", 750m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            var kpiIds = repository.Rows.Select(row => row.KpiId).ToList();
            kpiIds.Should().Contain(PrincipalKpiCatalog.PurchaseInId);
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-RET-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-TGT-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-GRW-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-INV-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-CUS-", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Produce_PurchaseRefreshDoesNotErasePersistedSalesOut()
        {
            var repository = new RecordingRepository();
            var salesOut = new FakeSalesOutSnapshotDal(SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)));
            var purchaseIn = new FakePurchaseInSnapshotDal(PurchaseIn(2026, 6, PurchaseInRow("S001", "Principal A", 750m)));
            var producer = CreateProducer(repository, salesOut, purchaseIn);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            salesOut.Current = null;
            purchaseIn.Current = null;

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 250m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 1000m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.PurchaseInId
                && row.NumericValue == 750m);
        }

        [Fact]
        public void SupplierPurchaseInPack_MetadataIsLabeledPurchaseInAndNotARankingKpi()
        {
            var registry = CreateRegistry();

            var packIds = registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId).ToList();
            packIds.First().Should().Be(PrincipalKpiCatalog.SalesOutId);
            packIds.Should().Contain(PrincipalKpiCatalog.PurchaseInId);

            registry.TryGetMetadata(PrincipalKpiCatalog.PurchaseInId, out var purchaseIn).Should().BeTrue();
            purchaseIn.DisplayName.Should().Be("Purchase-In");
            purchaseIn.RankEligible.Should().BeFalse("PRN-PUR-001 is never a Principal performance ranking KPI");
            purchaseIn.RadarEligible.Should().BeFalse();
            purchaseIn.SourceDomain.Should().Be(PrincipalPurchaseInSnapshot.Domain);
            purchaseIn.EvidenceRoute.Should().Be("/reports/purchasing");
            purchaseIn.Description.Should().Contain("not used as the Principal performance ranking KPI");
            purchaseIn.Description.Should().Contain("does not change PRN-SALES-001");

            registry.TryGetMetadata(PrincipalKpiCatalog.SalesOutId, out var salesOut).Should().BeTrue();
            salesOut.RankEligible.Should().BeTrue("PRN-SALES-001 remains the authoritative ranking KPI");
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

        private static PrincipalPurchaseInAggregateResult PurchaseIn(int year, int month, params PrincipalPurchaseInRow[] rows)
        {
            return new PrincipalPurchaseInAggregateResult
            {
                KpiId = PrincipalKpiCatalog.PurchaseInId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalPurchaseInRow PurchaseInRow(string supplierId, string name, decimal amount)
        {
            return new PrincipalPurchaseInRow
            {
                KpiId = PrincipalKpiCatalog.PurchaseInId,
                SupplierId = supplierId,
                SupplierName = name,
                PurchaseInAmount = amount
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
            PrincipalPurchaseInAggregateResult purchaseIn)
        {
            return CreateProducer(
                repository,
                new FakeSalesOutSnapshotDal(salesOut),
                new FakePurchaseInSnapshotDal(purchaseIn));
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(
            RecordingRepository repository,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            IPrincipalPurchaseInSnapshotDal purchaseInSnapshotDal)
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
                purchaseInSnapshotDal: purchaseInSnapshotDal);
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

        private sealed class FakePurchaseInSnapshotDal : IPrincipalPurchaseInSnapshotDal
        {
            public FakePurchaseInSnapshotDal(PrincipalPurchaseInAggregateResult current)
            {
                Current = current;
            }

            public PrincipalPurchaseInAggregateResult Current { get; set; }

            public PrincipalPurchaseInAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalPurchaseInAggregateResult result, string refreshLogId)
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
