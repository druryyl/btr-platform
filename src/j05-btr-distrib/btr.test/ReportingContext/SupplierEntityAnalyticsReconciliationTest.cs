using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierEntityAnalyticsReconciliationTest
    {
        [Fact]
        public void ProducedKpis_MatchPu01PrincipalExposureRow()
        {
            var repository = new InMemoryRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);

            var portfolioSupplier = new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = "S001",
                SupplierCode = "SUPA",
                SupplierName = "Principal A",
                PrincipalName = "Principal A",
                MtdPurchaseAmount = 1_250_000m,
                MtdInvoiceCount = 3,
                PostedPercent = 66.6667m,
                PercentOfPurchase = 25m,
                InventoryValue = 400_000m,
                AtRiskValue = 50_000m,
                IsActiveMtd = true
            };

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-reconcile",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new SupplierEntityAnalyticsProduceInput
                {
                    ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                    {
                        TopPrincipal = new List<DashboardPurchasingManagementTopPrincipalRow>
                        {
                            new DashboardPurchasingManagementTopPrincipalRow
                            {
                                Rank = 1,
                                SupplierId = "S001",
                                SupplierCode = "SUPA",
                                PrincipalName = "Principal A",
                                MtdPurchaseAmount = 1_250_000m,
                                PercentOfPurchase = 25m,
                                InventoryValue = 400_000m,
                                AtRiskValue = 50_000m
                            }
                        },
                        Portfolio = new List<DashboardPurchasingManagementPortfolioRow> { portfolioSupplier }
                    }
                }
            });

            var metrics = repository.GetCurrentMetrics(EntityTypeCode.Supplier, "SUPA");
            metrics.Single(r => r.KpiId == "PU-KPI-001").NumericValue.Should().Be(1_250_000m);
            metrics.Single(r => r.KpiId == "PU-KPI-002").NumericValue.Should().Be(3m);
            metrics.Single(r => r.KpiId == "PU-KPI-003").NumericValue.Should().Be(66.6667m);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.InventoryValue).NumericValue
                .Should().Be(400_000m);

            metrics.Single(r => r.KpiId == "PU-KPI-001").NumericValue
                .Should().Be(portfolioSupplier.MtdPurchaseAmount);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.InventoryValue).NumericValue
                .Should().Be(portfolioSupplier.InventoryValue);
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(InMemoryRepository repository)
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
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SupplierEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            return new SupplierEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                new EntityRankingEngine(
                    repository,
                    registry,
                    entityTypes,
                    Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions())),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(
                    repository,
                    new EntityRelationshipDefinitionRegistry(entityTypes),
                    entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                new EntityAttentionSignalRegistry());
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class InMemoryRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return CurrentRows.Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Count > 0;
        }
    }
}
