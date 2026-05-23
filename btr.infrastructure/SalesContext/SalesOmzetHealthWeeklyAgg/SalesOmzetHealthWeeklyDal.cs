using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetHealthWeeklyAgg
{
    public class SalesOmzetHealthWeeklyDal : ISalesOmzetHealthWeeklyDal
    {
        private const string SelectColumns = @"
                HealthWeeklyId, YearNumber, WeekNumber,
                PeriodStartDate, PeriodEndDate,
                HealthLevel, HealthScore,
                MissingOrdersCount, MissingFaktursCount, UnlinkedFaktursCount, StaleDataCount,
                LastCalculatedAt, CalculationDurationMs, CreatedAt, UpdatedAt";

        private readonly DatabaseOptions _opt;

        public SalesOmzetHealthWeeklyDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(SalesOmzetHealthWeeklyModel model)
        {
            const string sql = @"
            INSERT INTO BTR_SalesOmzetHealthWeekly(
                HealthWeeklyId, YearNumber, WeekNumber,
                PeriodStartDate, PeriodEndDate,
                HealthLevel, HealthScore,
                MissingOrdersCount, MissingFaktursCount, UnlinkedFaktursCount, StaleDataCount,
                LastCalculatedAt, CalculationDurationMs, CreatedAt, UpdatedAt)
            VALUES (
                @HealthWeeklyId, @YearNumber, @WeekNumber,
                @PeriodStartDate, @PeriodEndDate,
                @HealthLevel, @HealthScore,
                @MissingOrdersCount, @MissingFaktursCount, @UnlinkedFaktursCount, @StaleDataCount,
                @LastCalculatedAt, @CalculationDurationMs, @CreatedAt, @UpdatedAt)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, CreateParameters(model));
            }
        }

        public void Update(SalesOmzetHealthWeeklyModel model)
        {
            const string sql = @"
            UPDATE BTR_SalesOmzetHealthWeekly
            SET
                PeriodStartDate = @PeriodStartDate,
                PeriodEndDate = @PeriodEndDate,
                HealthLevel = @HealthLevel,
                HealthScore = @HealthScore,
                MissingOrdersCount = @MissingOrdersCount,
                MissingFaktursCount = @MissingFaktursCount,
                UnlinkedFaktursCount = @UnlinkedFaktursCount,
                StaleDataCount = @StaleDataCount,
                LastCalculatedAt = @LastCalculatedAt,
                CalculationDurationMs = @CalculationDurationMs,
                UpdatedAt = @UpdatedAt
            WHERE
                HealthWeeklyId = @HealthWeeklyId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, CreateParameters(model));
            }
        }

        public SalesOmzetHealthWeeklyModel GetData(ISalesOmzetHealthWeeklyKey key)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzetHealthWeekly
            WHERE HealthWeeklyId = @HealthWeeklyId";

            var dp = new DynamicParameters();
            dp.AddParam("@HealthWeeklyId", key.HealthWeeklyId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<SalesOmzetHealthWeeklyModel>(sql, dp);
            }
        }

        public SalesOmzetHealthWeeklyModel GetByYearWeek(int yearNumber, int weekNumber)
        {
            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzetHealthWeekly
            WHERE YearNumber = @YearNumber AND WeekNumber = @WeekNumber";

            var dp = new DynamicParameters();
            dp.AddParam("@YearNumber", yearNumber, SqlDbType.Int);
            dp.AddParam("@WeekNumber", weekNumber, SqlDbType.Int);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<SalesOmzetHealthWeeklyModel>(sql, dp);
            }
        }

        public IReadOnlyList<SalesOmzetHealthWeeklyModel> ListByYearWeeks(IEnumerable<IsoWeekIdentifier> weeks)
        {
            var weekList = weeks?.ToList() ?? new List<IsoWeekIdentifier>();
            if (weekList.Count == 0)
                return new List<SalesOmzetHealthWeeklyModel>();

            var conditions = new List<string>();
            var dp = new DynamicParameters();
            for (var i = 0; i < weekList.Count; i++)
            {
                conditions.Add($"(YearNumber = @Year{i} AND WeekNumber = @Week{i})");
                dp.Add($"Year{i}", weekList[i].YearNumber);
                dp.Add($"Week{i}", weekList[i].WeekNumber);
            }

            var sql = $@"
            SELECT {SelectColumns}
            FROM BTR_SalesOmzetHealthWeekly
            WHERE {string.Join(" OR ", conditions)}";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesOmzetHealthWeeklyModel>(sql, dp).ToList();
            }
        }

        private static DynamicParameters CreateParameters(SalesOmzetHealthWeeklyModel model)
        {
            var dp = new DynamicParameters();
            dp.AddParam("@HealthWeeklyId", model.HealthWeeklyId, SqlDbType.VarChar);
            dp.AddParam("@YearNumber", model.YearNumber, SqlDbType.Int);
            dp.AddParam("@WeekNumber", model.WeekNumber, SqlDbType.Int);
            dp.AddParam("@PeriodStartDate", model.PeriodStartDate, SqlDbType.DateTime);
            dp.AddParam("@PeriodEndDate", model.PeriodEndDate, SqlDbType.DateTime);
            dp.AddParam("@HealthLevel", model.HealthLevel.ToString(), SqlDbType.VarChar);
            dp.AddParam("@HealthScore", model.HealthScore, SqlDbType.Int);
            dp.AddParam("@MissingOrdersCount", model.MissingOrdersCount, SqlDbType.Int);
            dp.AddParam("@MissingFaktursCount", model.MissingFaktursCount, SqlDbType.Int);
            dp.AddParam("@UnlinkedFaktursCount", model.UnlinkedFaktursCount, SqlDbType.Int);
            dp.AddParam("@StaleDataCount", model.StaleDataCount, SqlDbType.Int);
            dp.AddParam("@LastCalculatedAt", model.LastCalculatedAt, SqlDbType.DateTime);
            dp.AddParam("@CalculationDurationMs", model.CalculationDurationMs, SqlDbType.Int);
            dp.AddParam("@CreatedAt", model.CreatedAt, SqlDbType.DateTime);
            dp.AddParam("@UpdatedAt", model.UpdatedAt, SqlDbType.DateTime);
            return dp;
        }
    }
}
