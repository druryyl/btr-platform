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
    public class SupplierPrincipalReturnCompositionTest
    {
        [Fact]
        public void Produce_ComposesStoredReturnAmountsForTheSamePrincipalAndPeriod()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 900m)),
                Returns(2026, 6, ReturnRow("S001", "Principal A", 60m, 50m, 110m)),
                Percentages(2026, 6, PercentageRow("S001", "Principal A", 110m, 900m, 0.122222m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.GoodReturnAmountId
                && row.NumericValue == 60m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.BrokenReturnAmountId
                && row.NumericValue == 50m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.TotalReturnAmountId
                && row.NumericValue == 110m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.ReturnPercentageId
                && row.NumericValue == 0.122222m);
        }

        [Fact]
        public void Produce_DoesNotWriteOrReplacePrincipalSalesOut()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 900m)),
                Returns(2026, 6, ReturnRow("S001", "Principal A", 60m, 50m, 110m)),
                Percentages(2026, 6, PercentageRow("S001", "Principal A", 110m, 900m, 0.122222m)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 900m);
            repository.Rows.Where(row =>
                    row.EntityId == "S001"
                    && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .Should().HaveCount(1);
        }

        [Fact]
        public void Produce_ReturnPercentageIsAbsentOrNullWhenSalesOutNotPositive()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(
                repository,
                salesOut: null,
                returns: Returns(2026, 6, ReturnRow("S001", "Principal A", 10m, 5m, 15m)),
                percentages: Percentages(2026, 6, PercentageRow("S001", "Principal A", 15m, 0m, null)));

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            var percentageRows = repository.Rows.Where(row =>
                    row.EntityId == "S001"
                    && row.KpiId == PrincipalKpiCatalog.ReturnPercentageId)
                .ToList();
            percentageRows.Should().HaveCountLessOrEqualTo(1);
            if (percentageRows.Count == 1)
                percentageRows[0].NumericValue.Should().BeNull();

            repository.Rows.Should().Contain(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.TotalReturnAmountId
                && row.NumericValue == 15m);
        }

        [Fact]
        public void Produce_PurchaseRefreshRetainsPersistedReturns()
        {
            var repository = new RecordingRepository();
            var salesOut = new FakeSalesOutSnapshotDal(SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 900m)));
            var returns = new FakeReturnSnapshotDal(Returns(2026, 6, ReturnRow("S001", "Principal A", 60m, 50m, 110m)));
            var percentages = new FakeReturnPercentageSnapshotDal(Percentages(2026, 6, PercentageRow("S001", "Principal A", 110m, 900m, 0.122222m)));
            var producer = CreateProducer(repository, salesOut, returns, percentages);

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A")));

            salesOut.Current = null;
            returns.Current = null;
            percentages.Current = null;

            producer.Produce(CreateContext(
                new DateTime(2026, 6, 15),
                Portfolio("S001", "SUPA", "Principal A", purchaseAmount: 250m)));

            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.NumericValue == 900m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.TotalReturnAmountId
                && row.NumericValue == 110m);
            repository.Rows.Should().ContainSingle(row =>
                row.EntityId == "S001"
                && row.KpiId == PrincipalKpiCatalog.ReturnPercentageId
                && row.NumericValue == 0.122222m);
        }

        [Fact]
        public void SupplierReturnPack_MetadataIsQualityAndEvidenceIsReturnItem()
        {
            var registry = CreateRegistry();

            var packIds = registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId).ToList();
            packIds.First().Should().Be(PrincipalKpiCatalog.SalesOutId);
            packIds.Should().Contain(new[]
            {
                PrincipalKpiCatalog.GoodReturnAmountId,
                PrincipalKpiCatalog.BrokenReturnAmountId,
                PrincipalKpiCatalog.TotalReturnAmountId,
                PrincipalKpiCatalog.ReturnPercentageId
            });

            registry.TryGetMetadata(PrincipalKpiCatalog.GoodReturnAmountId, out var good).Should().BeTrue();
            good.DisplayName.Should().Be("Good Return Amount");
            good.RankEligible.Should().BeFalse();
            good.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceRoute);
            good.EvidenceFilterDimension.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceFilterDimension);
            good.EvidenceRoute.Should().Contain("return-evidence");
            good.SourceDomain.Should().Be(PrincipalReturnSnapshot.Domain);

            registry.TryGetMetadata(PrincipalKpiCatalog.TotalReturnAmountId, out var total).Should().BeTrue();
            total.RankEligible.Should().BeFalse();
            total.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceRoute);

            registry.TryGetMetadata(PrincipalKpiCatalog.ReturnPercentageId, out var percentage).Should().BeTrue();
            percentage.DisplayName.Should().Be("Return Percentage");
            percentage.RankEligible.Should().BeTrue("PRN-RET-004 is a supporting ranking indicator");
            percentage.EvidenceRoute.Should().Be(SupplierEntityAnalyticsRegistrar.PrincipalReturnEvidenceRoute);
            percentage.SourceDomain.Should().Be(PrincipalReturnPercentageSnapshot.Domain);
            percentage.Description.Should().Contain("not a deduction");
            percentage.Description.Should().Contain("not Net Sales");

            var resolver = new SupplierEntityAnalyticsEvidenceResolver();
            var evidence = resolver.BuildEvidence("S001", new EntityIdentity
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = "S001",
                EntityCode = "SUPA"
            });

            evidence.Links.Should().Contain(link =>
                link.Label == "Return Item evidence"
                && link.ReportRoute == "/dashboard/principal-performance/return-evidence?supplierId=S001");
            evidence.Links.Should().Contain(link =>
                link.Label == "Faktur Item evidence"
                && link.ReportRoute.Contains("/dashboard/principal-performance/evidence"));
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

        private static PrincipalReturnAggregateResult Returns(int year, int month, params PrincipalReturnRow[] rows)
        {
            return new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalReturnRow ReturnRow(string supplierId, string name, decimal good, decimal broken, decimal total)
        {
            return new PrincipalReturnRow
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = supplierId,
                SupplierName = name,
                GoodReturnAmount = good,
                BrokenReturnAmount = broken,
                TotalReturnAmount = total
            };
        }

        private static PrincipalReturnPercentageResult Percentages(int year, int month, params PrincipalReturnPercentageRow[] rows)
        {
            return new PrincipalReturnPercentageResult
            {
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalReturnPercentageRow PercentageRow(
            string supplierId, string name, decimal totalReturn, decimal? salesOut, decimal? percentage)
        {
            return new PrincipalReturnPercentageRow
            {
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SalesOutKpiId = PrincipalKpiCatalog.SalesOutId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = supplierId,
                SupplierName = name,
                TotalReturnAmount = totalReturn,
                SalesOutAmount = salesOut,
                ReturnPercentage = percentage
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
            PrincipalReturnAggregateResult returns,
            PrincipalReturnPercentageResult percentages)
        {
            return CreateProducer(
                repository,
                new FakeSalesOutSnapshotDal(salesOut),
                new FakeReturnSnapshotDal(returns),
                new FakeReturnPercentageSnapshotDal(percentages));
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(
            RecordingRepository repository,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            IPrincipalReturnSnapshotDal returnSnapshotDal,
            IPrincipalReturnPercentageSnapshotDal returnPercentageSnapshotDal)
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
                returnSnapshotDal,
                returnPercentageSnapshotDal);
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

        private sealed class FakeReturnSnapshotDal : IPrincipalReturnSnapshotDal
        {
            public FakeReturnSnapshotDal(PrincipalReturnAggregateResult current)
            {
                Current = current;
            }

            public PrincipalReturnAggregateResult Current { get; set; }

            public PrincipalReturnAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeReturnPercentageSnapshotDal : IPrincipalReturnPercentageSnapshotDal
        {
            public FakeReturnPercentageSnapshotDal(PrincipalReturnPercentageResult current)
            {
                Current = current;
            }

            public PrincipalReturnPercentageResult Current { get; set; }

            public PrincipalReturnPercentageResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalReturnPercentageResult result, string refreshLogId)
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
