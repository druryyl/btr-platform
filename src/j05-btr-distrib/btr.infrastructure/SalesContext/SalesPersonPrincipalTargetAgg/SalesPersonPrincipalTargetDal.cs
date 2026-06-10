using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesPersonPrincipalTargetAgg
{
    public class SalesPersonPrincipalTargetDal : ISalesPersonPrincipalTargetDal
    {
        private readonly DatabaseOptions _opt;

        public SalesPersonPrincipalTargetDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<SalesPersonPrincipalTargetModel> ListBySalesPersonPeriod(
            string salesPersonId, int year, int month)
        {
            const string sql = @"
                SELECT
                    aa.SalesPersonId,
                    aa.SupplierId,
                    aa.TargetYear,
                    aa.TargetMonth,
                    aa.TargetAmount,
                    aa.UpdatedDate,
                    ISNULL(bb.SupplierCode, '') SupplierCode,
                    ISNULL(bb.SupplierName, '') SupplierName
                FROM
                    BTR_SalesPersonPrincipalTarget aa
                    LEFT JOIN BTR_Supplier bb ON aa.SupplierId = bb.SupplierId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                    AND aa.TargetYear = @TargetYear
                    AND aa.TargetMonth = @TargetMonth
                ORDER BY
                    bb.SupplierName";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", salesPersonId, SqlDbType.VarChar);
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesPersonPrincipalTargetModel>(sql, dp);
            }
        }

        public IEnumerable<SalesPersonPrincipalTargetModel> ListByPeriod(int year, int month)
        {
            const string sql = @"
                SELECT
                    aa.SalesPersonId,
                    aa.SupplierId,
                    aa.TargetYear,
                    aa.TargetMonth,
                    aa.TargetAmount,
                    aa.UpdatedDate,
                    ISNULL(bb.SupplierCode, '') SupplierCode,
                    ISNULL(bb.SupplierName, '') SupplierName
                FROM
                    BTR_SalesPersonPrincipalTarget aa
                    LEFT JOIN BTR_Supplier bb ON aa.SupplierId = bb.SupplierId
                WHERE
                    aa.TargetYear = @TargetYear
                    AND aa.TargetMonth = @TargetMonth
                ORDER BY
                    aa.SalesPersonId, bb.SupplierName";

            var dp = new DynamicParameters();
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesPersonPrincipalTargetModel>(sql, dp);
            }
        }

        public void Upsert(IEnumerable<SalesPersonPrincipalTargetModel> rows)
        {
            const string sql = @"
                IF EXISTS (
                    SELECT 1
                    FROM BTR_SalesPersonPrincipalTarget
                    WHERE SalesPersonId = @SalesPersonId
                      AND SupplierId = @SupplierId
                      AND TargetYear = @TargetYear
                      AND TargetMonth = @TargetMonth)
                BEGIN
                    UPDATE BTR_SalesPersonPrincipalTarget
                    SET TargetAmount = @TargetAmount,
                        UpdatedDate = GETDATE()
                    WHERE SalesPersonId = @SalesPersonId
                      AND SupplierId = @SupplierId
                      AND TargetYear = @TargetYear
                      AND TargetMonth = @TargetMonth
                END
                ELSE
                BEGIN
                    INSERT INTO BTR_SalesPersonPrincipalTarget
                        (SalesPersonId, SupplierId, TargetYear, TargetMonth, TargetAmount, UpdatedDate)
                    VALUES
                        (@SalesPersonId, @SupplierId, @TargetYear, @TargetMonth, @TargetAmount, GETDATE())
                END";

            var list = rows?.ToList() ?? new List<SalesPersonPrincipalTargetModel>();
            if (list.Count == 0)
                return;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                foreach (var row in list)
                {
                    var dp = new DynamicParameters();
                    dp.AddParam("@SalesPersonId", row.SalesPersonId, SqlDbType.VarChar);
                    dp.AddParam("@SupplierId", row.SupplierId, SqlDbType.VarChar);
                    dp.AddParam("@TargetYear", row.TargetYear, SqlDbType.Int);
                    dp.AddParam("@TargetMonth", row.TargetMonth, SqlDbType.Int);
                    dp.AddParam("@TargetAmount", row.TargetAmount, SqlDbType.Decimal);
                    conn.Execute(sql, dp);
                }
            }
        }

        public decimal SumBySalesPersonPeriod(string salesPersonId, int year, int month)
        {
            const string sql = @"
                SELECT ISNULL(SUM(TargetAmount), 0)
                FROM BTR_SalesPersonPrincipalTarget
                WHERE SalesPersonId = @SalesPersonId
                  AND TargetYear = @TargetYear
                  AND TargetMonth = @TargetMonth";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", salesPersonId, SqlDbType.VarChar);
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ExecuteScalar<decimal>(sql, dp);
            }
        }

        public IReadOnlyDictionary<string, decimal> SumByPeriod(int year, int month)
        {
            const string sql = @"
                SELECT SalesPersonId, ISNULL(SUM(TargetAmount), 0) TargetAmount
                FROM BTR_SalesPersonPrincipalTarget
                WHERE TargetYear = @TargetYear
                  AND TargetMonth = @TargetMonth
                GROUP BY SalesPersonId";

            var dp = new DynamicParameters();
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<SumRow>(sql, dp)?.ToList()
                    ?? new List<SumRow>();
                var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.SalesPersonId))
                        continue;

                    dict[row.SalesPersonId.Trim()] = row.TargetAmount;
                }

                return dict;
            }
        }

        private sealed class SumRow
        {
            public string SalesPersonId { get; set; }
            public decimal TargetAmount { get; set; }
        }
    }
}
