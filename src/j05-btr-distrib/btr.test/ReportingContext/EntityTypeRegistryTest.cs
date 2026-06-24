using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityTypeRegistryTest
    {
        [Fact]
        public void Register_AndTryGet_ReturnsCanonicalRegistration()
        {
            var registry = new EntityTypeRegistry();
            registry.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = "customer-default"
            });

            registry.TryGet("customer", out var registration).Should().BeTrue();
            registration.EntityTypeCode.Should().Be(EntityTypeCode.Customer);
            registry.NormalizeEntityTypeCode("CUSTOMER").Should().Be(EntityTypeCode.Customer);
        }

        [Fact]
        public void GetAll_ReturnsRegisteredTypesSorted()
        {
            var registry = new EntityTypeRegistry();
            registry.Register(new EntityTypeRegistration { EntityTypeCode = EntityTypeCode.Supplier, DisplayName = "Supplier" });
            registry.Register(new EntityTypeRegistration { EntityTypeCode = EntityTypeCode.Customer, DisplayName = "Customer" });

            registry.GetAll().Should().HaveCount(2);
            registry.GetAll()[0].EntityTypeCode.Should().Be(EntityTypeCode.Customer);
        }
    }

    public class EntityAnalyticsProducerOrchestratorTest
    {
        [Fact]
        public void ProduceForDomain_InvokesMatchingProducerOnly()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                WorkerDomainHook = "Customer"
            });

            var customerProducer = new RecordingProducer(EntityTypeCode.Customer, "Customer");
            var salesmanProducer = new RecordingProducer(EntityTypeCode.Salesman, "Salesman");
            var orchestrator = new EntityAnalyticsProducerOrchestrator(
                new IEntityAnalyticsProducer[] { customerProducer, salesmanProducer },
                entityTypes);

            orchestrator.ProduceForDomain("Customer", new EntityAnalyticsProduceContext());

            customerProducer.ProduceCount.Should().Be(1);
            salesmanProducer.ProduceCount.Should().Be(0);
        }

        [Fact]
        public void ProduceForEntityType_InvokesMatchingProducerOnly()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer"
            });

            var customerProducer = new RecordingProducer(EntityTypeCode.Customer, "Customer");
            var orchestrator = new EntityAnalyticsProducerOrchestrator(
                new IEntityAnalyticsProducer[] { customerProducer },
                entityTypes);

            orchestrator.ProduceForEntityType("customer", new EntityAnalyticsProduceContext());

            customerProducer.ProduceCount.Should().Be(1);
        }

        private sealed class RecordingProducer : IEntityAnalyticsProducer
        {
            public RecordingProducer(string entityType, string workerDomain)
            {
                EntityType = entityType;
                WorkerDomain = workerDomain;
            }

            public string EntityType { get; }

            public string WorkerDomain { get; }

            public int ProduceCount { get; private set; }

            public void Produce(EntityAnalyticsProduceContext context)
            {
                ProduceCount++;
            }
        }
    }
}
