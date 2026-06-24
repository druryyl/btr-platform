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
    public class SalesmanEntityAnalyticsReconciliationTest
    {
        [Fact]
        public void ProducedKpis_MatchSf01TopOmzetAndRepHistoryRow()
        {
            var repository = new InMemoryRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);

            var portfolioRep = new DashboardSalesmanPortfolioRow
            {
                SalesPersonId = "SP-ID-100",
                SalesPersonCode = "SP100",
                SalesPersonName = "Reconcile Rep",
                CompletedOmzet = 2500000m,
                AchievementPercent = 88m,
                OpenBalance = 750000m,
                IsActive = true
            };

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-reconcile",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new SalesmanEntityAnalyticsProduceInput
                {
                    SalesmanAggregate = new DashboardSalesmanAggregateResult
                    {
                        TopOmzet = new List<DashboardSalesmanTopOmzetRow>
                        {
                            new DashboardSalesmanTopOmzetRow
                            {
                                SalesPersonId = "SP-ID-100",
                                SalesPersonCode = "SP100",
                                CompletedOmzet = 2500000m
                            }
                        },
                        TopPiutang = new List<DashboardSalesmanTopPiutangRow>
                        {
                            new DashboardSalesmanTopPiutangRow
                            {
                                SalesPersonId = "SP-ID-100",
                                SalesPersonCode = "SP100",
                                OutstandingBalance = 750000m
                            }
                        },
                        RepHistory = new List<DashboardSalesmanRepHistoryRow>
                        {
                            new DashboardSalesmanRepHistoryRow
                            {
                                PeriodYear = 2026,
                                PeriodMonth = 6,
                                SalesPersonId = "SP-ID-100",
                                SalesPersonCode = "SP100",
                                CompletedOmzet = 2500000m,
                                AchievementPercent = 88m,
                                OpenBalance = 750000m
                            }
                        },
                        Portfolio = new List<DashboardSalesmanPortfolioRow> { portfolioRep }
                    }
                }
            });

            var metrics = repository.GetCurrentMetrics(EntityTypeCode.Salesman, "SP100");
            metrics.Single(r => r.KpiId == "SF-KPI-008").NumericValue.Should().Be(2500000m);
            metrics.Single(r => r.KpiId == "SF-KPI-009").NumericValue.Should().Be(88m);
            metrics.Single(r => r.KpiId == "SF-KPI-010").NumericValue.Should().Be(750000m);

            metrics.Single(r => r.KpiId == "SF-KPI-008").NumericValue
                .Should().Be(portfolioRep.CompletedOmzet);
            metrics.Single(r => r.KpiId == "SF-KPI-010").NumericValue
                .Should().Be(portfolioRep.OpenBalance);

            var monthly = repository.GetHistory(EntityTypeCode.Salesman, "SP100", 2026, 6, 2026, 6);
            monthly.Single(r => r.KpiId == "SF-KPI-008").NumericValue.Should().Be(2500000m);
            monthly.Single(r => r.KpiId == "SF-KPI-009").NumericValue.Should().Be(88m);
            monthly.Single(r => r.KpiId == "SF-KPI-010").NumericValue.Should().Be(750000m);
        }

        private static SalesmanEntityAnalyticsProducer CreateProducer(InMemoryRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = SalesmanEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SalesmanRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.SalesmanAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SalesmanEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));

            var attentionSignals = new EntityAttentionSignalRegistry();
            SalesmanAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SalesmanRelationshipCatalog.Register(relationships);

            return new SalesmanEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
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

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
            {
                var rows = CurrentRows.Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (rows.Count == 0)
                    return null;

                var entityCode = rows.Select(r => r.EntityCode).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? entityId;
                return new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = rows[0].EntityId,
                    EntityCode = entityCode,
                    DisplayName = rows.FirstOrDefault(r => r.KpiId == EntityAnalyticsMetaKpiIds.DisplayName)?.TextValue ?? entityCode,
                    IsActive = true
                };
            }

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                CurrentRows.RemoveAll(r => r.EntityType == entityType);
                CurrentRows.AddRange(rows);
            }

            public override void SaveMonthlyHistory(
                string entityType,
                IEnumerable<EntityAnalyticsMonthlyRow> rows,
                string refreshLogId)
            {
                MonthlyRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId)
            {
                return CurrentRows
                    .Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .Select(r => r.GeneratedAt)
                    .Cast<DateTime?>()
                    .FirstOrDefault();
            }

            public override bool HasAnyCurrentMetrics(string entityType)
            {
                return CurrentRows.Any(r => r.EntityType == entityType);
            }

            public List<EntityAnalyticsMonthlyRow> GetHistory(
                string entityType,
                string entityId,
                int fromYear,
                int fromMonth,
                int toYear,
                int toMonth)
            {
                return MonthlyRows
                    .Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)) &&
                        r.PeriodYear >= fromYear && r.PeriodYear <= toYear)
                    .ToList();
            }
        }
    }
}
