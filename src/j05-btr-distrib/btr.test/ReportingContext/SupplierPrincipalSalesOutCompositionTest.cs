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
    public class SupplierPrincipalSalesOutCompositionTest
    {
        [Fact]
        public void Produce_ComposesOwnedPrincipalSalesOutForTheSamePrincipalAndPeriod()
        {
            var repository = new RecordingRepository();
            var snapshot = new FakeSalesOutSnapshotDal(new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = new List<PrincipalSalesOutRow>
                {
                    new PrincipalSalesOutRow
                    {
                        KpiId = PrincipalKpiCatalog.SalesOutId,
                        SupplierId = "S001",
                        SupplierName = "Principal A",
                        SalesOutAmount = 900m
                    }
                }
            });
            var producer = CreateProducer(repository, snapshot);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 1_500_000m, purchasingSalesOutAmount: 999m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 900m);
            repository.Rows.Should().NotContain(row =>
                row.KpiId == PrincipalKpiCatalog.SalesOutId && row.NumericValue == 999m);
            repository.MonthlyRows.Should().Contain(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 900m
                && row.PeriodYear == 2026
                && row.PeriodMonth == 6);
        }

        [Fact]
        public void Produce_DefaultRankingUsesPrincipalSalesOutNotPurchaseAmount()
        {
            var repository = new RecordingRepository();
            var snapshot = new FakeSalesOutSnapshotDal(new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = new List<PrincipalSalesOutRow>
                {
                    Owned("S001", "Principal A", 500m),
                    Owned("S002", "Principal B", 200m)
                }
            });
            var producer = CreateProducer(repository, snapshot);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 100m, purchasingSalesOutAmount: 1m),
                Portfolio("S002", "SUPB", "Principal B", purchaseAmount: 900m, purchasingSalesOutAmount: 2m)));

            var salesRanks = repository.RankingRows
                .Where(row => row.RankMetricKpiId == PrincipalKpiCatalog.SalesOutId)
                .ToDictionary(row => row.EntityId, row => row.RankPosition);
            salesRanks["S001"].Should().Be(1);
            salesRanks["S002"].Should().Be(2);

            var purchaseRanks = repository.RankingRows
                .Where(row => row.RankMetricKpiId == "PU-KPI-001")
                .ToDictionary(row => row.EntityId, row => row.RankPosition);
            purchaseRanks["S002"].Should().Be(1);
            purchaseRanks["S001"].Should().Be(2);
        }

        [Fact]
        public void Produce_DoesNotComposeOtherPrincipalPacks()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository, new FakeSalesOutSnapshotDal(new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = new List<PrincipalSalesOutRow> { Owned("S001", "Principal A", 900m) }
            }));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 100m, purchasingSalesOutAmount: 50m)));

            var kpiIds = repository.Rows.Select(row => row.KpiId).ToList();
            kpiIds.Should().Contain(PrincipalKpiCatalog.SalesOutId);
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-RET-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-TGT-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-GRW-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-PUR-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-INV-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain(id => id.StartsWith("PRN-CUS-", StringComparison.OrdinalIgnoreCase));
            kpiIds.Should().NotContain("PRN-NET-001");
            kpiIds.Should().NotContain(id => id.IndexOf("HEALTH", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void Produce_PurchaseRefreshDoesNotErasePersistedPrincipalSalesOut()
        {
            var repository = new RecordingRepository();
            var snapshot = new FakeSalesOutSnapshotDal(new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                Principals = new List<PrincipalSalesOutRow> { Owned("S001", "Principal A", 900m) }
            });
            var producer = CreateProducer(repository, snapshot);
            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 100m, purchasingSalesOutAmount: 10m)));

            snapshot.Current = null;
            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 250m, purchasingSalesOutAmount: 777m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 900m);
            repository.Rows.Should().Contain(row =>
                row.KpiId == "PU-KPI-001" && row.NumericValue == 250m);
        }

        [Fact]
        public void SupplierPack_PrincipalSalesOutIsCommercialPerformanceAndFakturItemEvidence()
        {
            var registry = CreateRegistry();

            registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId)
                .First().Should().Be(PrincipalKpiCatalog.SalesOutId);

            registry.TryGetMetadata(PrincipalKpiCatalog.SalesOutId, out var salesOut).Should().BeTrue();
            salesOut.DisplayName.Should().Be("Principal Sales-Out");
            salesOut.RankEligible.Should().BeTrue();
            salesOut.SignatureDimensionKey.Should().Be(EntityAnalyticsSignatureDimensions.Performance);
            salesOut.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute);
            salesOut.EvidenceFilterDimension.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceFilterDimension);
            salesOut.EvidenceRoute.Should().NotContain("/reports/purchasing");
            salesOut.SourceDomain.Should().Be(PrincipalSalesOutSnapshot.Domain);

            registry.TryGetMetadata("PU-KPI-001", out var purchase).Should().BeTrue();
            purchase.SignatureDimensionKey.Should().BeNull();
            purchase.EvidenceRoute.Should().Be("/reports/purchasing");

            var resolver = new SupplierEntityAnalyticsEvidenceResolver();
            var evidence = resolver.BuildEvidence("SUPA", new EntityIdentity
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = "S001",
                EntityCode = "SUPA"
            });

            evidence.Links.Should().Contain(link =>
                link.Label == "Faktur Item evidence"
                && link.ReportRoute == "/dashboard/principal-performance/evidence?supplierId=S001");
            evidence.Links.Should().NotContain(link =>
                link.Label == "Faktur Item evidence" && link.ReportRoute.Contains("/reports/purchasing"));
        }

        private static PrincipalSalesOutRow Owned(string supplierId, string name, decimal amount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = amount
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
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal)
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
                salesOutSnapshotDal);
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
            decimal purchaseAmount,
            decimal purchasingSalesOutAmount)
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
                SalesOutAmount = purchasingSalesOutAmount,
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
