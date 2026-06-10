using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetTargetDal : ISalesOmzetTargetDal
    {
        private readonly DatabaseOptions _opt;
        private readonly ISalesPersonPrincipalTargetDal _principalTargetDal;

        public SalesOmzetTargetDal(
            IOptions<DatabaseOptions> opt,
            ISalesPersonPrincipalTargetDal principalTargetDal)
        {
            _opt = opt.Value;
            _principalTargetDal = principalTargetDal;
        }

        public decimal SumTargetAmountForMonth(int year, int month)
        {
            var principalSums = _principalTargetDal.SumByPeriod(year, month);
            var legacyRows = ListLegacyTargetsForMonth(year, month);

            var salesPersonIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in principalSums.Keys)
                salesPersonIds.Add(id);
            foreach (var id in legacyRows.Keys)
                salesPersonIds.Add(id);

            decimal total = 0m;
            foreach (var salesPersonId in salesPersonIds)
            {
                var resolved = ResolveRepTarget(salesPersonId, year, month, principalSums, legacyRows);
                if (resolved.HasValue)
                    total += resolved.Value;
            }

            return total;
        }

        public decimal? GetTargetAmount(string salesPersonId, int year, int month)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
                return null;

            var principalSum = _principalTargetDal.SumBySalesPersonPeriod(salesPersonId, year, month);
            if (principalSum > 0)
                return principalSum;

            return GetLegacyTargetAmount(salesPersonId, year, month);
        }

        public IReadOnlyDictionary<string, decimal?> ListTargetsForMonth(int year, int month)
        {
            var principalSums = _principalTargetDal.SumByPeriod(year, month);
            var legacyRows = ListLegacyTargetsForMonth(year, month);

            var salesPersonIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in principalSums.Keys)
                salesPersonIds.Add(id);
            foreach (var id in legacyRows.Keys)
                salesPersonIds.Add(id);

            var dict = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
            foreach (var salesPersonId in salesPersonIds)
            {
                dict[salesPersonId] = ResolveRepTarget(
                    salesPersonId, year, month, principalSums, legacyRows);
            }

            return dict;
        }

        private decimal? ResolveRepTarget(
            string salesPersonId,
            int year,
            int month,
            IReadOnlyDictionary<string, decimal> principalSums,
            IReadOnlyDictionary<string, decimal> legacyRows)
        {
            if (principalSums.TryGetValue(salesPersonId, out var principalSum) && principalSum > 0)
                return principalSum;

            if (legacyRows.TryGetValue(salesPersonId, out var legacyAmount))
                return legacyAmount;

            return null;
        }

        private decimal? GetLegacyTargetAmount(string salesPersonId, int year, int month)
        {
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
                var row = conn.Read<TargetRow>(sql, dp)?.FirstOrDefault();
                return row?.TargetAmount;
            }
        }

        private IReadOnlyDictionary<string, decimal> ListLegacyTargetsForMonth(int year, int month)
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
                var rows = conn.Read<TargetRowWithId>(sql, dp)?.ToList()
                    ?? new List<TargetRowWithId>();
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
