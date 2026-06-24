using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityRelationshipEngineTest
    {
        [Fact]
        public void PersistRollups_WritesTopNRowsPerRelationship()
        {
            var repository = new RelationshipRepository();
            var engine = CreateEngine(repository);
            var generatedAt = new DateTime(2026, 6, 24, 8, 0, 0);

            engine.PersistRollups(
                EntityTypeCode.Customer,
                2026,
                6,
                new List<EntityRelationshipSnapshot>
                {
                    Snapshot("C001", CustomerRelationshipCatalog.TopItemsByOmzet, "I1", "BRG1", "Item 1", 100m),
                    Snapshot("C001", CustomerRelationshipCatalog.TopItemsByOmzet, "I2", "BRG2", "Item 2", 200m),
                    Snapshot("C001", CustomerRelationshipCatalog.AssignedSalesman, "S1", "SP01", "Salesman 1", null)
                },
                "r1",
                generatedAt);

            repository.RelationshipRows.Should().HaveCount(3);
            repository.RelationshipRows
                .Where(r => r.RelationshipCode == CustomerRelationshipCatalog.TopItemsByOmzet)
                .Select(r => r.Rank)
                .Should().Equal(1, 2);
            repository.RelationshipRows
                .Single(r => r.RelationshipCode == CustomerRelationshipCatalog.AssignedSalesman)
                .Rank.Should().Be(1);
        }

        [Fact]
        public void PersistRollups_ReplacesExistingPeriodRows()
        {
            var repository = new RelationshipRepository();
            var engine = CreateEngine(repository);
            var generatedAt = new DateTime(2026, 6, 24, 8, 0, 0);

            engine.PersistRollups(
                EntityTypeCode.Customer,
                2026,
                6,
                new List<EntityRelationshipSnapshot>
                {
                    Snapshot("C001", CustomerRelationshipCatalog.TopItemsByOmzet, "I1", "BRG1", "Item 1", 100m)
                },
                "r1",
                generatedAt);

            engine.PersistRollups(
                EntityTypeCode.Customer,
                2026,
                6,
                new List<EntityRelationshipSnapshot>
                {
                    Snapshot("C001", CustomerRelationshipCatalog.TopItemsByOmzet, "I2", "BRG2", "Item 2", 200m)
                },
                "r2",
                generatedAt);

            repository.RelationshipRows.Should().ContainSingle();
            repository.RelationshipRows[0].TargetEntityCode.Should().Be("BRG2");
        }

        [Fact]
        public void PersistRollups_IsolatesEntityTypes()
        {
            var repository = new RelationshipRepository();
            var engine = CreateEngine(repository);

            engine.PersistRollups(
                EntityTypeCode.Customer,
                2026,
                6,
                new List<EntityRelationshipSnapshot>
                {
                    Snapshot("C001", CustomerRelationshipCatalog.AssignedSalesman, "S1", "SP01", "Salesman 1", null)
                },
                "r1",
                DateTime.UtcNow);

            engine.PersistRollups(
                EntityTypeCode.Salesman,
                2026,
                6,
                new List<EntityRelationshipSnapshot>
                {
                    new EntityRelationshipSnapshot
                    {
                        SourceEntityId = "S1",
                        SourceEntityCode = "SP01",
                        RelationshipCode = "ManagedCustomers",
                        TargetEntityType = EntityTypeCode.Customer,
                        TargetEntityId = "C001",
                        TargetEntityCode = "C001",
                        TargetDisplayName = "Customer 1",
                        MetricValue = 1000m
                    }
                },
                "r2",
                DateTime.UtcNow);

            repository.RelationshipRows.Should().HaveCount(2);
            repository.RelationshipRows.Should().Contain(r => r.SourceEntityType == EntityTypeCode.Customer);
            repository.RelationshipRows.Should().Contain(r => r.SourceEntityType == EntityTypeCode.Salesman);
        }

        [Fact]
        public void BuildRelatedEntitiesSection_ReadsL4OnlyAndBuildsRoutes()
        {
            var repository = new RelationshipRepository();
            repository.RelationshipRows.Add(new EntityAnalyticsRelationshipRow
            {
                SourceEntityType = EntityTypeCode.Customer,
                SourceEntityId = "C001",
                SourceEntityCode = "C001",
                RelationshipCode = CustomerRelationshipCatalog.TopItemsByOmzet,
                TargetEntityType = EntityTypeCode.Item,
                TargetEntityId = "I1",
                TargetEntityCode = "BRG1",
                TargetDisplayName = "Item 1",
                Rank = 1,
                MetricValue = 100m,
                PeriodYear = 2026,
                PeriodMonth = 6
            });

            var engine = CreateEngine(repository);
            var section = engine.BuildRelatedEntitiesSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeTrue();
            section.Blocks.Should().ContainSingle();
            section.Blocks[0].RelationshipLabel.Should().Be("Top Items");
            section.Blocks[0].Rows[0].ProfileRoute.Should().Be("/analytics/items/BRG1");
            section.Blocks[0].Rows[0].TargetEntityName.Should().Be("Item 1");
        }

        private static EntityRelationshipEngine CreateEngine(RelationshipRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                RelationshipPackId = CustomerRelationshipCatalog.PackId,
                ProfileRouteTemplate = "/analytics/customers/{code}"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                ProfileRouteTemplate = "/analytics/items/{code}"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                RelationshipPackId = "salesman-relationships",
                ProfileRouteTemplate = "/analytics/salesmen/{code}"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                ProfileRouteTemplate = "/analytics/suppliers/{code}"
            });

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);
            relationships.Register(EntityTypeCode.Salesman, new RelationshipDefinition
            {
                RelationshipCode = "ManagedCustomers",
                DisplayName = "Managed Customers",
                TargetEntityType = EntityTypeCode.Customer,
                TopN = 10
            });
            relationships.RegisterPack("salesman-relationships", new List<string> { "ManagedCustomers" });

            return new EntityRelationshipEngine(repository, relationships, entityTypes);
        }

        private static EntityRelationshipSnapshot Snapshot(
            string sourceEntityId,
            string relationshipCode,
            string targetEntityId,
            string targetEntityCode,
            string targetDisplayName,
            decimal? metricValue)
        {
            return new EntityRelationshipSnapshot
            {
                SourceEntityId = sourceEntityId,
                SourceEntityCode = sourceEntityId,
                RelationshipCode = relationshipCode,
                TargetEntityType = relationshipCode == CustomerRelationshipCatalog.AssignedSalesman
                    ? EntityTypeCode.Salesman
                    : EntityTypeCode.Item,
                TargetEntityId = targetEntityId,
                TargetEntityCode = targetEntityCode,
                TargetDisplayName = targetDisplayName,
                MetricValue = metricValue
            };
        }

        private sealed class RelationshipRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => Array.Empty<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => false;
        }
    }
}
