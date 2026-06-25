using System;
using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityAnalyticsServiceTest
    {
        [Fact]
        public void GetProfile_DisabledEntityType_ReturnsDisabledProfile()
        {
            var service = CreateService(enabledTypes: new[] { EntityTypeCode.Customer });
            var result = service.GetProfile(EntityTypeCode.Salesman, "S001");

            result.IsAvailable.Should().BeFalse();
            result.Overview.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.EntityTypeDisabled);
            result.KpiSummary.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.EntityTypeDisabled);
        }

        [Fact]
        public void GetProfile_UnknownEntityType_Throws()
        {
            var service = CreateService(enabledTypes: new[] { EntityTypeCode.Customer });

            Action act = () => service.GetProfile("Unknown", "X001");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetEnabledTypes_ReflectsEnabledAndSnapshotAvailability()
        {
            var repository = new ServiceTestRepository { HasMetrics = true };
            var service = CreateService(
                enabledTypes: new[] { EntityTypeCode.Customer },
                repository: repository);

            var types = service.GetEnabledTypes();

            types.Should().Contain(t =>
                t.EntityType == EntityTypeCode.Customer && t.IsEnabled && t.IsAvailable);
            types.Should().Contain(t =>
                t.EntityType == EntityTypeCode.Salesman && !t.IsEnabled);
        }

        [Fact]
        public void GetEnabledTypes_ReflectsPerEntitySnapshotAvailability()
        {
            var repository = new ServiceTestRepository(EntityTypeCode.Customer);
            var service = CreateService(
                enabledTypes: new[] { EntityTypeCode.Customer, EntityTypeCode.Salesman },
                repository: repository);

            var types = service.GetEnabledTypes();

            types.Single(t => t.EntityType == EntityTypeCode.Customer).IsAvailable.Should().BeTrue();
            types.Single(t => t.EntityType == EntityTypeCode.Salesman).IsAvailable.Should().BeFalse();
        }

        [Fact]
        public void GetEnabledTypes_WhenConfigEmpty_EnablesAllRegisteredTypes()
        {
            var repository = new ServiceTestRepository { HasMetrics = true };
            var service = CreateService(enabledTypes: new string[0], repository: repository);

            var types = service.GetEnabledTypes();

            types.Should().OnlyContain(t => t.IsEnabled);
            types.Should().Contain(t => t.EntityType == EntityTypeCode.Customer);
            types.Should().Contain(t => t.EntityType == EntityTypeCode.Salesman);
        }

        private static EntityAnalyticsService CreateService(
            string[] enabledTypes,
            IEntityAnalyticsRepository repository = null)
        {
            var entityTypes = new EntityTypeRegistry();
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            var kpiRegistry = new EntityAnalyticsKpiRegistry(entityTypes);
            new EntityAnalyticsPlatformRegistrar().Register(entityTypes, kpiRegistry, dimensionLabels);
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, kpiRegistry, dimensionLabels);

            repository = repository ?? new ServiceTestRepository();
            var options = Options.Create(new EntityAnalyticsOptions
            {
                EnabledEntityTypes = enabledTypes
            });
            var radarEngine = new EntityRadarEngine(repository, kpiRegistry, entityTypes);
            var composer = new EntityPerformanceProfileComposer(
                repository,
                kpiRegistry,
                entityTypes,
                Array.Empty<IEntityProfileEvidenceResolver>(),
                new EntityKpiEnvelopeFormatter(),
                new EntityTrendEngine(repository, kpiRegistry, entityTypes, options),
                new EntityRankingEngine(repository, kpiRegistry, entityTypes, options),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(
                    repository,
                    CreateRelationshipRegistry(entityTypes),
                    entityTypes),
                new EntityComparisonEngine(
                    repository,
                    kpiRegistry,
                    entityTypes,
                    new EntityKpiEnvelopeFormatter(),
                    new EntityTrendEngine(repository, kpiRegistry, entityTypes, options),
                    new EntityRankingEngine(repository, kpiRegistry, entityTypes, options),
                    new EntityAttentionEngine(repository),
                    new EntityRelationshipEngine(
                        repository,
                        CreateRelationshipRegistry(entityTypes),
                        entityTypes),
                    radarEngine),
                radarEngine);

            return new EntityAnalyticsService(composer, entityTypes, repository, options);
        }

        private static EntityRelationshipDefinitionRegistry CreateRelationshipRegistry(EntityTypeRegistry entityTypes)
        {
            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);
            return relationships;
        }

        private sealed class ServiceTestRepository : EntityAnalyticsRepositoryStubBase
        {
            private readonly HashSet<string> _typesWithMetrics;

            public ServiceTestRepository(params string[] typesWithMetrics)
            {
                _typesWithMetrics = new HashSet<string>(
                    typesWithMetrics ?? Array.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);
            }

            public bool HasMetrics { get; set; }

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => Array.Empty<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType)
                => HasMetrics || _typesWithMetrics.Contains(entityType);
        }
    }
}
