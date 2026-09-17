using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class CustomerPrincipalRelationshipDal : ICustomerPrincipalRelationshipDal
    {
        public const string WrittenTables =
            "BTRPD_CustomerPrincipalRelationshipKpi, BTRPD_CustomerPrincipalRelationship";

        public const string DeleteRelationshipSql =
            "DELETE FROM BTRPD_CustomerPrincipalRelationship";

        public const string MergeHeaderSql = @"
MERGE BTRPD_CustomerPrincipalRelationshipKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        KpiId = @KpiId,
        AsOfDate = @AsOfDate,
        HistoricalLimitation = @HistoricalLimitation,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, KpiId, AsOfDate, HistoricalLimitation, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @KpiId, @AsOfDate, @HistoricalLimitation, @GeneratedAt, @LastRefreshLogId);";

        public const string ListPairsForCustomersSql = @"
SELECT CustomerId, CustomerName, SupplierId, SupplierName,
    FirstTransactionDate, LastTransactionDate, RelationshipStatus, KpiId,
    SalesOutAmount, LineCount
FROM BTRPD_CustomerPrincipalRelationship
WHERE CustomerId IN @CustomerIds";

        public const string ListPairsForCustomerCodesSql = @"
SELECT r.CustomerId, c.CustomerCode AS CustomerCode, r.CustomerName, r.SupplierId, r.SupplierName,
    r.FirstTransactionDate, r.LastTransactionDate, r.RelationshipStatus, r.KpiId,
    r.SalesOutAmount, r.LineCount
FROM BTRPD_CustomerPrincipalRelationship r
INNER JOIN BTR_Customer c ON r.CustomerId = c.CustomerId
WHERE c.CustomerCode IN @CustomerCodes";

        public const string InsertRelationshipSql = @"
INSERT INTO BTRPD_CustomerPrincipalRelationship (
    CustomerPrincipalRelationshipId, CustomerId, CustomerName, SupplierId, SupplierName,
    FirstTransactionDate, LastTransactionDate, RelationshipStatus, KpiId, SalesOutAmount,
    LineCount, AsOfDate, GeneratedAt, LastRefreshLogId)
VALUES (
    @CustomerPrincipalRelationshipId, @CustomerId, @CustomerName, @SupplierId, @SupplierName,
    @FirstTransactionDate, @LastTransactionDate, @RelationshipStatus, @KpiId, @SalesOutAmount,
    @LineCount, @AsOfDate, @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public CustomerPrincipalRelationshipDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public CustomerPrincipalRelationshipResult GetProjection()
        {
            var header = ReadHeader();
            if (header is null)
                return null;

            const string pairSql = @"
SELECT CustomerId, CustomerName, SupplierId, SupplierName,
    FirstTransactionDate, LastTransactionDate, RelationshipStatus, KpiId,
    SalesOutAmount, LineCount
FROM BTRPD_CustomerPrincipalRelationship
ORDER BY CustomerId, SupplierId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var pairs = conn.Query<CustomerPrincipalRelationshipRow>(pairSql).ToList();
                return new CustomerPrincipalRelationshipResult
                {
                    KpiId = header.KpiId,
                    AsOfDate = header.AsOfDate,
                    HistoricalLimitation = header.HistoricalLimitation,
                    GeneratedAt = header.GeneratedAt,
                    Pairs = pairs
                };
            }
        }

        private HeaderRow ReadHeader()
        {
            const string headerSql = @"
SELECT SnapshotKey, KpiId, AsOfDate, HistoricalLimitation, GeneratedAt, LastRefreshLogId
FROM BTRPD_CustomerPrincipalRelationshipKpi
WHERE SnapshotKey = @SnapshotKey";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QueryFirstOrDefault<HeaderRow>(headerSql, new
                {
                    SnapshotKey = CustomerPrincipalRelationship.SnapshotKey
                });
            }
        }

        public CustomerPrincipalRelationshipResult ListPairsForCustomers(IEnumerable<string> customerIds)
        {
            var header = ReadHeader();
            if (header is null)
                return null;

            var ids = (customerIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pairs = new List<CustomerPrincipalRelationshipRow>();
            if (ids.Count > 0)
            {
                using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
                {
                    pairs = conn.Query<CustomerPrincipalRelationshipRow>(
                        ListPairsForCustomersSql,
                        new { CustomerIds = ids }).ToList();
                }
            }

            return new CustomerPrincipalRelationshipResult
            {
                KpiId = header.KpiId,
                AsOfDate = header.AsOfDate,
                HistoricalLimitation = header.HistoricalLimitation,
                GeneratedAt = header.GeneratedAt,
                Pairs = pairs
            };
        }

        public CustomerPrincipalRelationshipResult ListPairsForCustomerCodes(IEnumerable<string> customerCodes)
        {
            var header = ReadHeader();
            if (header is null)
                return null;

            var codes = (customerCodes ?? Enumerable.Empty<string>())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pairs = new List<CustomerPrincipalRelationshipRow>();
            if (codes.Count > 0)
            {
                using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
                {
                    pairs = conn.Query<CustomerPrincipalRelationshipRow>(
                        ListPairsForCustomerCodesSql,
                        new { CustomerCodes = codes }).ToList();
                }
            }

            return new CustomerPrincipalRelationshipResult
            {
                KpiId = header.KpiId,
                AsOfDate = header.AsOfDate,
                HistoricalLimitation = header.HistoricalLimitation,
                GeneratedAt = header.GeneratedAt,
                Pairs = pairs
            };
        }

        public void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var kpiId = PrincipalKpiCatalog.SalesOutId;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeleteRelationshipSql, transaction: transaction);
                    conn.Execute(MergeHeaderSql, new
                    {
                        SnapshotKey = CustomerPrincipalRelationship.SnapshotKey,
                        KpiId = kpiId,
                        result.AsOfDate,
                        HistoricalLimitation = result.HistoricalLimitation
                            ?? CustomerPrincipalRelationship.HistoricalLimitation,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Pairs ?? new List<CustomerPrincipalRelationshipRow>())
                    {
                        conn.Execute(InsertRelationshipSql, new
                        {
                            CustomerPrincipalRelationshipId = Ulid.NewUlid().ToString(),
                            CustomerId = row.CustomerId ?? string.Empty,
                            CustomerName = row.CustomerName ?? string.Empty,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.FirstTransactionDate,
                            row.LastTransactionDate,
                            RelationshipStatus = row.RelationshipStatus ?? string.Empty,
                            KpiId = kpiId,
                            row.SalesOutAmount,
                            row.LineCount,
                            result.AsOfDate,
                            result.GeneratedAt,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private sealed class HeaderRow
        {
            public string KpiId { get; set; }

            public DateTime AsOfDate { get; set; }

            public string HistoricalLimitation { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
