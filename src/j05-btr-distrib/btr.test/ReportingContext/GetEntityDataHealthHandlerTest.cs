using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class GetEntityDataHealthHandlerTest
    {
        private static IEntityTypeRegistry SupplierRegistry()
        {
            var registry = new EntityTypeRegistry();
            registry.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Principal",
                KpiPackId = "supplier-default"
            });
            return registry;
        }

        private static PrincipalSalesOutAggregateResult SalesOutSnapshot(
            IList<PrincipalSalesOutRow> principals,
            IList<PrincipalSalesOutDataQualityRow> dataQuality)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                GeneratedAt = new DateTime(2026, 7, 1),
                Principals = principals,
                DataQuality = dataQuality
            };
        }

        private static PrincipalTargetAggregateResult TargetSnapshot(
            IList<PrincipalTargetRow> principals)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = 2026,
                PeriodMonth = 6,
                GeneratedAt = new DateTime(2026, 7, 1),
                Principals = principals
            };
        }

        private sealed class FakeSalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
        {
            private readonly PrincipalSalesOutAggregateResult _result;

            public FakeSalesOutSnapshotDal(PrincipalSalesOutAggregateResult result)
            {
                _result = result;
            }

            public PrincipalSalesOutAggregateResult GetCurrent() => _result;

            public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class FakeTargetSnapshotDal : IPrincipalTargetSnapshotDal
        {
            private readonly PrincipalTargetAggregateResult _result;

            public FakeTargetSnapshotDal(PrincipalTargetAggregateResult result)
            {
                _result = result;
            }

            public PrincipalTargetAggregateResult GetCurrent() => _result;

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Supplier_DataHealth_ExposesFourIndicators()
        {
            var principals = new List<PrincipalSalesOutRow>
            {
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S1", SalesOutAmount = 100m },
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S2", SalesOutAmount = 200m },
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S3", SalesOutAmount = 300m },
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S4", SalesOutAmount = 400m }
            };
            var targets = new List<PrincipalTargetRow>
            {
                new PrincipalTargetRow { KpiId = PrincipalKpiCatalog.TargetId, SupplierId = "S1", TargetAmount = 90m },
                new PrincipalTargetRow { KpiId = PrincipalKpiCatalog.TargetId, SupplierId = "S2", TargetAmount = 180m }
            };
            var dataQuality = new List<PrincipalSalesOutDataQualityRow>
            {
                new PrincipalSalesOutDataQualityRow { ExceptionCode = PrincipalSalesOutSnapshot.BlankSupplierExceptionCode, Amount = 100m, LineCount = 5 },
                new PrincipalSalesOutDataQualityRow { ExceptionCode = PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode, Amount = 50m, LineCount = 3 },
                new PrincipalSalesOutDataQualityRow { ExceptionCode = "OTHER_EXCEPTION", Amount = 999m, LineCount = 99 }
            };

            var response = GetEntityDataHealthHandler.Compose(
                SalesOutSnapshot(principals, dataQuality),
                TargetSnapshot(targets),
                EntityTypeCode.Supplier);

            response.IsAvailable.Should().BeTrue();
            response.TargetCoveragePercentage.Should().Be(50.00m);
            response.PrincipalsMissingTargetCount.Should().Be(2);
            response.UnknownPrincipalExceptionCount.Should().Be(8);
            response.UnknownPrincipalExceptionAmount.Should().Be(150m);
        }

        [Fact]
        public void NoTargets_AllPrincipalsMissing_TargetCoverageZero()
        {
            var principals = new List<PrincipalSalesOutRow>
            {
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S1", SalesOutAmount = 100m }
            };
            var dataQuality = new List<PrincipalSalesOutDataQualityRow>();

            var response = GetEntityDataHealthHandler.Compose(
                SalesOutSnapshot(principals, dataQuality),
                TargetSnapshot(new List<PrincipalTargetRow>()),
                EntityTypeCode.Supplier);

            response.TargetCoveragePercentage.Should().Be(0.00m);
            response.PrincipalsMissingTargetCount.Should().Be(1);
        }

        [Fact]
        public void EmptySalesOut_NotAvailable_WithZeroedIndicators()
        {
            var response = GetEntityDataHealthHandler.Compose(
                null,
                TargetSnapshot(new List<PrincipalTargetRow>()),
                EntityTypeCode.Supplier);

            response.IsAvailable.Should().BeFalse();
            response.TargetCoveragePercentage.Should().BeNull();
            response.PrincipalsMissingTargetCount.Should().Be(0);
            response.UnknownPrincipalExceptionCount.Should().Be(0);
            response.UnknownPrincipalExceptionAmount.Should().Be(0m);
        }

        private static IEntityTypeRegistry SupplierAndCustomerRegistry()
        {
            var registry = SupplierRegistry();
            registry.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = "customer-default"
            });
            return registry;
        }

        [Fact]
        public async Task NonSupplier_ReturnsUnavailable_WithoutException()
        {
            var handler = new GetEntityDataHealthHandler(
                SupplierAndCustomerRegistry(),
                new FakeSalesOutSnapshotDal(SalesOutSnapshot(new List<PrincipalSalesOutRow>(), new List<PrincipalSalesOutDataQualityRow>())),
                new FakeTargetSnapshotDal(TargetSnapshot(new List<PrincipalTargetRow>())));

            var result = await handler.Handle(
                new GetEntityDataHealthQuery { EntityType = EntityTypeCode.Customer },
                CancellationToken.None);

            result.EntityType.Should().Be(EntityTypeCode.Customer);
            result.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task UnknownEntityType_Throws()
        {
            var handler = new GetEntityDataHealthHandler(
                SupplierRegistry(),
                new FakeSalesOutSnapshotDal(SalesOutSnapshot(new List<PrincipalSalesOutRow>(), new List<PrincipalSalesOutDataQualityRow>())),
                new FakeTargetSnapshotDal(TargetSnapshot(new List<PrincipalTargetRow>())));

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
                new GetEntityDataHealthQuery { EntityType = "UnknownType" },
                CancellationToken.None));
        }

        [Fact]
        public async Task Supplier_Handler_ExposesFourIndicators()
        {
            var principals = new List<PrincipalSalesOutRow>
            {
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S1", SalesOutAmount = 100m },
                new PrincipalSalesOutRow { KpiId = PrincipalKpiCatalog.SalesOutId, SupplierId = "S2", SalesOutAmount = 200m }
            };
            var targets = new List<PrincipalTargetRow>
            {
                new PrincipalTargetRow { KpiId = PrincipalKpiCatalog.TargetId, SupplierId = "S1", TargetAmount = 90m }
            };
            var dataQuality = new List<PrincipalSalesOutDataQualityRow>
            {
                new PrincipalSalesOutDataQualityRow { ExceptionCode = PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode, Amount = 40m, LineCount = 4 }
            };

            var handler = new GetEntityDataHealthHandler(
                SupplierRegistry(),
                new FakeSalesOutSnapshotDal(SalesOutSnapshot(principals, dataQuality)),
                new FakeTargetSnapshotDal(TargetSnapshot(targets)));

            var result = await handler.Handle(
                new GetEntityDataHealthQuery { EntityType = EntityTypeCode.Supplier },
                CancellationToken.None);

            result.IsAvailable.Should().BeTrue();
            result.TargetCoveragePercentage.Should().Be(50.00m);
            result.PrincipalsMissingTargetCount.Should().Be(1);
            result.UnknownPrincipalExceptionCount.Should().Be(4);
            result.UnknownPrincipalExceptionAmount.Should().Be(40m);
        }
    }
}
