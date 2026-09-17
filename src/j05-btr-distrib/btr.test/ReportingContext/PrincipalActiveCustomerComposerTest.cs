using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalActiveCustomerComposerTest
    {
        private static readonly DateTime AsOfDate = new DateTime(2026, 9, 9);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalActiveCustomerComposer _composer = new PrincipalActiveCustomerComposer();

        [Fact]
        public void Compose_CountsOnlyActiveRowsFromStoredProjection()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C003", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant),
                Pair("C001", "SUPB", "Beta", CustomerPrincipalRelationship.StatusDormant));

            var result = _composer.Compose(projection, GeneratedAt);

            result.ActiveCustomerKpiId.Should().Be("PRN-CUS-001");
            result.AsOfDate.Should().Be(AsOfDate);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.ActiveCustomerKpiId.Should().Be("PRN-CUS-001");
            alpha.ActiveCustomerCount.Should().Be(2);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.ActiveCustomerCount.Should().Be(0);
        }

        [Fact]
        public void Compose_ExcludesDormantFromCount_ButDoesNotDeleteSupplier()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant));

            var result = _composer.Compose(projection, GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.SupplierId.Should().Be("SUPA");
            alpha.ActiveCustomerCount.Should().Be(0);
            alpha.SortOrder.Should().Be(1);
        }

        [Fact]
        public void Compose_IgnoresBlankSupplier_AndOrdersByCountThenSupplier()
        {
            var projection = Projection(
                Pair("C001", "SUPB", "Beta", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C003", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C004", " ", "Blank", CustomerPrincipalRelationship.StatusActive),
                Pair("C005", null, "Null", CustomerPrincipalRelationship.StatusActive));

            var result = _composer.Compose(projection, GeneratedAt);

            result.Principals.Should().HaveCount(2);
            result.Principals[0].SupplierId.Should().Be("SUPA");
            result.Principals[0].ActiveCustomerCount.Should().Be(2);
            result.Principals[1].SupplierId.Should().Be("SUPB");
            result.Principals[1].ActiveCustomerCount.Should().Be(1);
        }

        [Fact]
        public void PersistActiveCustomer_ReadsStoredProjection_AndDoesNotUpdateProjectionOrSalesOut()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant));
            var projectionDal = new RecordingProjectionDal(projection);
            var snapshotDal = new RecordingActiveCustomerSnapshotDal();

            var worker = new RefreshPrincipalActiveCustomerSnapshotWorker(
                projectionDal,
                new PrincipalActiveCustomerComposer(),
                snapshotDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalActiveCustomerSnapshotRequest { TriggeredBy = "Manual" });

            projectionDal.WriteCount.Should().Be(0);
            projectionDal.ReadCount.Should().Be(1);
            snapshotDal.WriteCount.Should().Be(1);
            snapshotDal.LastResult.ActiveCustomerKpiId.Should().Be("PRN-CUS-001");
            snapshotDal.LastResult.AsOfDate.Should().Be(AsOfDate);
            snapshotDal.LastResult.Principals.Should().ContainSingle()
                .Which.ActiveCustomerCount.Should().Be(1);
            projectionDal.StoredProjection.Pairs.Should().HaveCount(2);

            var writerSql = string.Join(
                " ",
                PrincipalActiveCustomerSnapshotDal.WrittenTables,
                PrincipalActiveCustomerSnapshotDal.DeletePrincipalSql,
                PrincipalActiveCustomerSnapshotDal.MergeKpiSql,
                PrincipalActiveCustomerSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalActiveCustomer");
            writerSql.Should().Contain("ActiveCustomerCount");
            writerSql.Should().NotContain("BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("INSERT INTO BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("UPDATE BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("DELETE FROM BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-RET-");
            writerSql.Should().NotContain("PRN-CUS-002");
            writerSql.Should().NotContain("Net Sales");
        }

        private static CustomerPrincipalRelationshipResult Projection(params CustomerPrincipalRelationshipRow[] pairs)
        {
            return new CustomerPrincipalRelationshipResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                AsOfDate = AsOfDate,
                HistoricalLimitation = CustomerPrincipalRelationship.HistoricalLimitation,
                GeneratedAt = GeneratedAt,
                Pairs = pairs.ToList()
            };
        }

        private static CustomerPrincipalRelationshipRow Pair(
            string customerId,
            string supplierId,
            string supplierName,
            string status)
        {
            return new CustomerPrincipalRelationshipRow
            {
                CustomerId = customerId,
                CustomerName = "Customer " + customerId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                FirstTransactionDate = new DateTime(2026, 1, 5),
                LastTransactionDate = status == CustomerPrincipalRelationship.StatusActive
                    ? new DateTime(2026, 9, 1)
                    : new DateTime(2025, 1, 5),
                RelationshipStatus = status,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SalesOutAmount = 100m,
                LineCount = 1
            };
        }

        private sealed class RecordingProjectionDal : ICustomerPrincipalRelationshipDal
        {
            private readonly CustomerPrincipalRelationshipResult _projection;

            public RecordingProjectionDal(CustomerPrincipalRelationshipResult projection)
            {
                _projection = projection;
            }

            public int ReadCount { get; private set; }

            public int WriteCount { get; private set; }

            public CustomerPrincipalRelationshipResult StoredProjection
            {
                get { return _projection; }
            }

            public CustomerPrincipalRelationshipResult GetProjection()
            {
                ReadCount++;
                return _projection;
            }

            public CustomerPrincipalRelationshipResult ListPairsForCustomers(IEnumerable<string> customerIds)
            {
                return _projection;
            }

            public CustomerPrincipalRelationshipResult ListPairsForCustomerCodes(IEnumerable<string> customerCodes)
            {
                return _projection;
            }

            public void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId)
            {
                WriteCount++;
            }
        }

        private sealed class RecordingActiveCustomerSnapshotDal : IPrincipalActiveCustomerSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalActiveCustomerResult LastResult { get; private set; }

            public PrincipalActiveCustomerResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalActiveCustomerResult result, string refreshLogId)
            {
                LastResult = result;
                WriteCount++;
            }
        }

        private sealed class StubRefreshLogDal : IDashboardSnapshotRefreshLogDal
        {
            public void InsertRunning(DashboardSnapshotRefreshLogModel model)
            {
            }

            public void MarkSuccess(string refreshLogId, int durationMs)
            {
            }

            public void MarkFailed(string refreshLogId, int durationMs, string errorMessage)
            {
            }

            public IReadOnlyList<DashboardSnapshotRefreshStatusModel> GetLatestPerDomain()
            {
                return new List<DashboardSnapshotRefreshStatusModel>();
            }
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }
    }
}
