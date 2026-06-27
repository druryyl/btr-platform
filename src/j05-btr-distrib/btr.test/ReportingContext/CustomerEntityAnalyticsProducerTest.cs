using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerEntityAnalyticsProducerTest
    {
        [Fact]
        public void Produce_MapsPortfolioCustomersToL0Rows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioCustomer()));

            repository.EntityType.Should().Be(EntityTypeCode.Customer);
            repository.Rows.Should().NotBeEmpty();
            repository.Rows.Should().OnlyContain(r =>
                r.EntityId == "CUST-001" && r.EntityCode == "C001");

            var kpiRows = repository.Rows
                .Where(r => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                .ToList();

            kpiRows.Should().Contain(r => r.KpiId == "CU-KPI-009" && r.NumericValue == 1500000m);
            kpiRows.Should().Contain(r => r.KpiId == "CU-KPI-010" && r.NumericValue == 500000m);
            kpiRows.Should().Contain(r => r.KpiId == "FI-KPI-013" && r.NumericValue == 100000m);

            repository.Rows.Should().Contain(r =>
                r.KpiId == EntityAnalyticsMetaKpiIds.DisplayName && r.TextValue == "Alpha Customer");
            repository.Rows.Should().Contain(r =>
                r.KpiId == EntityAnalyticsMetaKpiIds.AttentionSignals && r.TextValue == "Overdue");
            repository.Rows.Should().OnlyContain(r => r.DefinitionVersion == 1);
        }

        [Fact]
        public void Produce_WritesL1MonthlyRowsForTrendEligibleKpis()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioCustomer()));

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == 2026 && r.PeriodMonth == 6 && r.IsClosed == false);
            repository.MonthlyRows.Should().Contain(r =>
                r.KpiId == "CU-KPI-009" && r.NumericValue == 1500000m && r.PeriodSemantics == "MTD");
            repository.MonthlyRows.Should().Contain(r =>
                r.KpiId == "CU-KPI-010" && r.NumericValue == 500000m && r.PeriodSemantics == "AllTimeOpen");
            repository.MonthlyRows.Should().Contain(r =>
                r.KpiId == "FI-KPI-013" && r.NumericValue == 100000m);
        }

        [Fact]
        public void Produce_L1Upsert_IsIdempotentForOpenMonth()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);
            var context = CreateContext(generatedAt, CreatePortfolioCustomer());

            producer.Produce(context);
            producer.Produce(context);

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "CU-KPI-009").NumericValue.Should().Be(1500000m);
        }

        [Fact]
        public void Produce_WritesL2RankingForRankEligibleKpis()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioCustomer()));

            repository.RankingRows.Should().HaveCount(2);
            repository.RankingRows.Should().OnlyContain(r => r.RankPosition == 1 && r.PopulationSize == 1);
            repository.RankingRows.Select(r => r.RankMetricKpiId).Should().BeEquivalentTo("CU-KPI-009", "CU-KPI-010");
        }

        [Fact]
        public void Produce_WritesL5RadarWhenPeerGroupIsSufficient()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);
            var customers = Enumerable.Range(1, 5)
                .Select(i => new DashboardCustomerPortfolioCustomerRow
                {
                    CustomerCode = $"C00{i}",
                    CustomerName = $"Customer {i}",
                    WilayahName = "Jakarta",
                    MtdOmzet = 1000000m * i,
                    OpenBalance = 100000m * i,
                    OverdueBalance = 10000m * i,
                    IsActiveMtd = true,
                    FakturCount6Mo = i * 2,
                    PortfolioPriorityScore = i * 10
                })
                .ToList();

            producer.Produce(CreateContext(generatedAt, customers[0], customers));

            repository.RadarRows.Should().NotBeEmpty();
            repository.RadarRows.Should().OnlyContain(r => r.PeerGroupSize >= EntityAnalyticsConstants.MinRadarPeerGroupSize);
        }

        [Fact]
        public void Produce_WritesL3AttentionLifecycleRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioCustomer()));

            repository.AttentionRows.Should().ContainSingle();
            repository.AttentionRows[0].SignalCode.Should().Be("Overdue");
            repository.AttentionRows[0].IsActive.Should().BeTrue();
            repository.AttentionRows[0].FirstSeenPeriodYear.Should().Be(2026);
            repository.AttentionRows[0].FirstSeenPeriodMonth.Should().Be(6);
        }

        [Fact]
        public void Produce_WritesL4RelationshipRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioCustomer()));

            repository.RelationshipRows.Should().NotBeEmpty();
            repository.RelationshipRows.Should().Contain(r =>
                r.SourceEntityId == "CUST-001" && r.SourceEntityCode == "C001");
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == CustomerRelationshipCatalog.TopItemsByOmzet);
        }

        [Fact]
        public void Produce_EmptyPortfolio_ClearsCurrentMetrics()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-empty",
                GeneratedAt = DateTime.UtcNow,
                DomainInput = new CustomerEntityAnalyticsProduceInput
                {
                    PortfolioAggregate = new DashboardCustomerPortfolioAggregateResult
                    {
                        Customers = new List<DashboardCustomerPortfolioCustomerRow>()
                    }
                }
            });

            repository.EntityType.Should().Be(EntityTypeCode.Customer);
            repository.Rows.Should().BeEmpty();
        }

        [Fact]
        public void CustomerDefaultPack_RegistersCatalogKpiIds()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.GetPackKpiIds(CustomerEntityAnalyticsRegistrar.KpiPackId)
                .Should().Equal("CU-KPI-009", "CU-KPI-010", "FI-KPI-013");

            registry.TryGetMetadata("CU-KPI-009", out _).Should().BeTrue();
            registry.TryGetMetadata("CU-KPI-010", out _).Should().BeTrue();
            registry.TryGetMetadata("FI-KPI-013", out var overdue).Should().BeTrue();
            overdue.TrendEligible.Should().BeTrue();
        }

        private static CustomerEntityAnalyticsProducer CreateProducer(RecordingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));

            var attentionSignals = new EntityAttentionSignalRegistry();
            CustomerAttentionSignalCatalog.Register(attentionSignals);
            var attentionEngine = new EntityAttentionEngine(repository);

            var entityTypesForRelationships = new EntityTypeRegistry();
            entityTypesForRelationships.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                RelationshipPackId = CustomerRelationshipCatalog.PackId,
                ProfileRouteTemplate = "/analytics/customers/{id}"
            });
            entityTypesForRelationships.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                ProfileRouteTemplate = "/analytics/salesmen/{id}"
            });
            entityTypesForRelationships.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                ProfileRouteTemplate = "/analytics/items/{id}"
            });
            entityTypesForRelationships.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                ProfileRouteTemplate = "/analytics/suppliers/{id}"
            });
            var relationships = new EntityRelationshipDefinitionRegistry(entityTypesForRelationships);
            CustomerRelationshipCatalog.Register(relationships);
            var relationshipEngine = new EntityRelationshipEngine(repository, relationships, entityTypesForRelationships);
            var radarEngine = new EntityRadarEngine(repository, registry, entityTypes);

            return new CustomerEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
                attentionEngine,
                relationshipEngine,
                radarEngine,
                attentionSignals);
        }

        private static EntityAnalyticsProduceContext CreateContext(
            DateTime generatedAt,
            DashboardCustomerPortfolioCustomerRow customer,
            IList<DashboardCustomerPortfolioCustomerRow> customers = null)
        {
            var portfolioCustomers = customers != null
                ? new List<DashboardCustomerPortfolioCustomerRow>(customers)
                : new List<DashboardCustomerPortfolioCustomerRow> { customer };
            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-1",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new CustomerEntityAnalyticsProduceInput
                {
                    CustomerAggregate = new DashboardCustomerAggregateResult
                    {
                        AttentionList = new List<DashboardCustomerAttentionRow>
                        {
                            new DashboardCustomerAttentionRow
                            {
                                CustomerCode = customer.CustomerCode,
                                SignalKey = "Overdue"
                            }
                        }
                    },
                    PortfolioAggregate = new DashboardCustomerPortfolioAggregateResult
                    {
                        Customers = portfolioCustomers
                    },
                    ForecastAggregate = new DashboardCustomerRiskForecastAggregateResult
                    {
                        Contexts = new List<CustomerRiskForecastContext>
                        {
                            new CustomerRiskForecastContext
                            {
                                CustomerCode = customer.CustomerCode,
                                SalesPersonId = "SP1",
                                SalesPersonName = "Sales Rep 1"
                            }
                        }
                    },
                    SalesmanSnapshot = new DashboardSalesmanAggregateResult
                    {
                        TopOmzet = new List<DashboardSalesmanTopOmzetRow>
                        {
                            new DashboardSalesmanTopOmzetRow
                            {
                                SalesPersonId = "SP1",
                                SalesPersonCode = "SP01"
                            }
                        }
                    },
                    RelationshipAggregate = new DashboardCustomerRelationshipAggregateResult
                    {
                        ByCustomerCode = new Dictionary<string, DashboardCustomerRelationshipCustomerRollup>
                        {
                            [customer.CustomerCode] = new DashboardCustomerRelationshipCustomerRollup
                            {
                                CustomerCode = customer.CustomerCode,
                                TopItems = new List<DashboardCustomerRelationshipItemRow>
                                {
                                    new DashboardCustomerRelationshipItemRow
                                    {
                                        Rank = 1,
                                        BrgId = "I1",
                                        BrgCode = "BRG1",
                                        BrgName = "Item 1",
                                        MetricValue = 1000m
                                    }
                                },
                                TopPrincipals = new List<DashboardCustomerRelationshipPrincipalRow>
                                {
                                    new DashboardCustomerRelationshipPrincipalRow
                                    {
                                        Rank = 1,
                                        SupplierId = "SUP1",
                                        SupplierCode = "P01",
                                        SupplierName = "Principal 1",
                                        MetricValue = 1000m
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static DashboardCustomerPortfolioCustomerRow CreatePortfolioCustomer()
        {
            return new DashboardCustomerPortfolioCustomerRow
            {
                CustomerId = "CUST-001",
                CustomerCode = "C001",
                CustomerName = "Alpha Customer",
                WilayahName = "Jakarta",
                MtdOmzet = 1500000m,
                OpenBalance = 500000m,
                OverdueBalance = 100000m,
                IsActiveMtd = true,
                LifecycleStage = "Active",
                PortfolioTier = "Strategic"
            };
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class RecordingRepository : EntityAnalyticsRepositoryStubBase
        {
            public string EntityType { get; private set; }

            public List<EntityAnalyticsCurrentRow> Rows { get; } = new List<EntityAnalyticsCurrentRow>();

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return Rows.Where(r => r.EntityType == entityType && r.EntityId == entityId).ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                EntityType = entityType;
                Rows.Clear();
                Rows.AddRange(rows);
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => Rows.Count > 0;
        }
    }
}
