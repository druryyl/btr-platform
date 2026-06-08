using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetTargetDal : ISalesOmzetTargetDal
    {
        private readonly DatabaseOptions _opt;

        public SalesOmzetTargetDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public decimal SumTargetAmountForMonth(int year, int month)
        {
            const string sql = @"
                SELECT ISNULL(SUM(TargetAmount), 0)
                FROM BTR_SalesOmzetTarget
                WHERE TargetYear = @TargetYear
                  AND TargetMonth = @TargetMonth";

            var dp = new DynamicParameters();
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ExecuteScalar<decimal>(sql, dp);
            }
        }

        public decimal? GetTargetAmount(string salesPersonId, int year, int month)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
                return null;

            const string sql = @"
                SELECT TargetAmount
                FROM BTR_SalesOmzetTarget
                WHERE SalesPersonId = @SalesPersonId
                  AND TargetYear = @TargetYear
                  AND TargetMonth = @TargetMonth";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", salesPersonId, SqlDbType.VarChar);
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var row = conn.Read<TargetRow>(sql, dp).FirstOrDefault();
                return row?.TargetAmount;
            }
        }

        public IReadOnlyDictionary<string, decimal?> ListTargetsForMonth(int year, int month)
        {
            const string sql = @"
                SELECT SalesPersonId, TargetAmount
                FROM BTR_SalesOmzetTarget
                WHERE TargetYear = @TargetYear
                  AND TargetMonth = @TargetMonth";

            var dp = new DynamicParameters();
            dp.AddParam("@TargetYear", year, SqlDbType.Int);
            dp.AddParam("@TargetMonth", month, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<TargetRowWithId>(sql, dp).ToList();
                var dict = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.SalesPersonId))
                        continue;

                    dict[row.SalesPersonId.Trim()] = row.TargetAmount;
                }

                return dict;
            }
        }

        private sealed class TargetRow
        {
            public decimal TargetAmount { get; set; }
        }

        private sealed class TargetRowWithId
        {
            public string SalesPersonId { get; set; }

            public decimal TargetAmount { get; set; }
        }
    }
}
