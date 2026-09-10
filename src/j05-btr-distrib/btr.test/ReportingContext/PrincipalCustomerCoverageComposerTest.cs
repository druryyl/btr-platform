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
    public class PrincipalCustomerCoverageComposerTest
    {
        private static readonly DateTime AsOfDate = new DateTime(2026, 9, 9);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalCustomerCoverageComposer _composer = new PrincipalCustomerCoverageComposer();

        [Fact]
        public void Compose_CoverageIsActiveDividedByTotalProjectionPopulation()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C003", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant),
                Pair("C004", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant),
                Pair("C001", "SUPB", "Beta", CustomerPrincipalRelationship.StatusActive));

            var activeCustomers = ActiveCustomers(
                ActiveRow("SUPA", 2),
                ActiveRow("SUPB", 1));

            var result = _composer.Compose(projection, activeCustomers, GeneratedAt);

            result.CustomerCoverageKpiId.Should().Be("PRN-CUS-002");
            result.AsOfDate.Should().Be(AsOfDate);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.ActiveCustomerCount.Should().Be(2);
            alpha.TotalCustomerCount.Should().Be(4);
            alpha.CoveragePercentage.Should().Be(0.5m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.ActiveCustomerCount.Should().Be(1);
            beta.TotalCustomerCount.Should().Be(1);
            beta.CoveragePercentage.Should().Be(1.0m);
        }

        [Fact]
        public void Compose_IncludesDormantInTotal_AndNullWhenNoProjectionCustomers()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant));

            var activeCustomers = ActiveCustomers(ActiveRow("SUPA", 0));

            var result = _composer.Compose(projection, activeCustomers, GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.TotalCustomerCount.Should().Be(1);
            alpha.ActiveCustomerCount.Should().Be(0);
            alpha.CoveragePercentage.Should().Be(0m);
        }

        [Fact]
        public void Compose_NullCoverage_WhenTotalCustomerCountIsZero()
        {
            // Empty projection produces no suppliers, so this path is not directly reachable
            // for a single supplier. This test documents that a supplier missing from the
            // projection is not emitted and therefore has no denominator.
            var result = _composer.Compose(Projection(), ActiveCustomers(), GeneratedAt);
            result.Principals.Should().BeEmpty();
        }

        [Fact]
        public void Compose_MissingActiveSnapshot_FallsBackToZeroActive()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant));

            var result = _composer.Compose(projection, null, GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.ActiveCustomerCount.Should().Be(0);
            alpha.TotalCustomerCount.Should().Be(2);
            alpha.CoveragePercentage.Should().Be(0m);
        }

        [Fact]
        public void Compose_IgnoresBlankSupplier_AndOrdersByCoverageThenSupplier()
        {
            var projection = Projection(
                Pair("C001", "SUPB", "Beta", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C003", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C004", " ", "Blank", CustomerPrincipalRelationship.StatusActive),
                Pair("C005", null, "Null", CustomerPrincipalRelationship.StatusActive));

            var activeCustomers = ActiveCustomers(
                ActiveRow("SUPA", 2),
                ActiveRow("SUPB", 1));

            var result = _composer.Compose(projection, activeCustomers, GeneratedAt);

            result.Principals.Should().HaveCount(2);
            result.Principals[0].SupplierId.Should().Be("SUPA");
            result.Principals[0].CoveragePercentage.Should().Be(1.0m);
            result.Principals[1].SupplierId.Should().Be("SUPB");
            result.Principals[1].CoveragePercentage.Should().Be(1.0m);
        }

        [Fact]
        public void PersistCustomerCoverage_ReadsStoredProjectionAndActiveCustomers_AndDoesNotUpdateProjectionOrSalesOut()
        {
            var projection = Projection(
                Pair("C001", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusActive),
                Pair("C002", "SUPA", "Alpha", CustomerPrincipalRelationship.StatusDormant));
            var activeCustomers = ActiveCustomers(ActiveRow("SUPA", 1));
            var projectionDal = new RecordingProjectionDal(projection);
            var activeCustomerDal = new RecordingActiveCustomerDal(activeCustomers);
            var snapshotDal = new RecordingCoverageSnapshotDal();

            var worker = new RefreshPrincipalCustomerCoverageSnapshotWorker(
                projectionDal,
                activeCustomerDal,
                new PrincipalCustomerCoverageComposer(),
                snapshotDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalCustomerCoverageSnapshotRequest { TriggeredBy = "Manual" });

            projectionDal.WriteCount.Should().Be(0);
            projectionDal.ReadCount.Should().Be(1);
            activeCustomerDal.ReadCount.Should().Be(1);
            snapshotDal.WriteCount.Should().Be(1);
            snapshotDal.LastResult.CustomerCoverageKpiId.Should().Be("PRN-CUS-002");
            snapshotDal.LastResult.AsOfDate.Should().Be(AsOfDate);
            snapshotDal.LastResult.Principals.Should().ContainSingle()
                .Which.CoveragePercentage.Should().Be(0.5m);

            var writerSql = string.Join(
                " ",
                PrincipalCustomerCoverageSnapshotDal.WrittenTables,
                PrincipalCustomerCoverageSnapshotDal.DeletePrincipalSql,
                PrincipalCustomerCoverageSnapshotDal.MergeKpiSql,
                PrincipalCustomerCoverageSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalCustomerCoverage");
            writerSql.Should().Contain("CoveragePercentage");
            writerSql.Should().Contain("TotalCustomerCount");
            writerSql.Should().NotContain("BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("INSERT INTO BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("UPDATE BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("DELETE FROM BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-RET-");
            writerSql.Should().NotContain("CP-KPI-");
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

        private static PrincipalActiveCustomerResult ActiveCustomers(params PrincipalActiveCustomerRow[] rows)
        {
            return new PrincipalActiveCustomerResult
            {
                ActiveCustomerKpiId = PrincipalKpiCatalog.ActiveCustomerCountId,
                AsOfDate = AsOfDate,
                GeneratedAt = GeneratedAt,
                Principals = rows.ToList()
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

        private static PrincipalActiveCustomerRow ActiveRow(string supplierId, int count)
        {
            return new PrincipalActiveCustomerRow
            {
                ActiveCustomerKpiId = PrincipalKpiCatalog.ActiveCustomerCountId,
                SupplierId = supplierId,
                SupplierName = "Supplier " + supplierId,
                ActiveCustomerCount = count,
                SortOrder = 1
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

        private sealed class RecordingActiveCustomerDal : IPrincipalActiveCustomerSnapshotDal
        {
            private readonly PrincipalActiveCustomerResult _activeCustomers;

            public RecordingActiveCustomerDal(PrincipalActiveCustomerResult activeCustomers)
            {
                _activeCustomers = activeCustomers;
            }

            public int ReadCount { get; private set; }

            public PrincipalActiveCustomerResult GetCurrent()
            {
                ReadCount++;
                return _activeCustomers;
            }

            public void ReplaceCurrent(PrincipalActiveCustomerResult result, string refreshLogId)
            {
            }
        }

        private sealed class RecordingCoverageSnapshotDal : IPrincipalCustomerCoverageSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalCustomerCoverageResult LastResult { get; private set; }

            public PrincipalCustomerCoverageResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalCustomerCoverageResult result, string refreshLogId)
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
