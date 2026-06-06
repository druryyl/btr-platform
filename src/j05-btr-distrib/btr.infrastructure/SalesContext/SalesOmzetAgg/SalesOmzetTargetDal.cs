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

        private sealed class TargetRow
        {
            public decimal TargetAmount { get; set; }
        }
    }
}
