using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.SalesContext.VisitPlanAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.VisitPlanAgg
{
    public class VisitPlanExceptionDal : IVisitPlanExceptionDal
    {
        private readonly DatabaseOptions _opt;

        public VisitPlanExceptionDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(VisitPlanExceptionModel model)
        {
            if (string.IsNullOrWhiteSpace(model.VisitPlanExceptionId))
                model.VisitPlanExceptionId = Ulid.NewUlid().ToString();

            const string sql = @"
                INSERT INTO BTR_VisitPlanException (
                    VisitPlanExceptionId, SalesPersonId, VisitDate, ExceptionType,
                    CustomerId, ReplacementCustomerId, CreatedAt, CreatedByUserId)
                VALUES (
                    @VisitPlanExceptionId, @SalesPersonId, @VisitDate, @ExceptionType,
                    @CustomerId, @ReplacementCustomerId, @CreatedAt, @CreatedByUserId)";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitPlanExceptionId", model.VisitPlanExceptionId, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@VisitDate", model.VisitDate.Date, SqlDbType.Date);
            dp.AddParam("@ExceptionType", model.ExceptionType, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@ReplacementCustomerId", model.ReplacementCustomerId ?? string.Empty, SqlDbType.VarChar);
            dp.AddParam("@CreatedAt", model.CreatedAt, SqlDbType.DateTime);
            dp.AddParam("@CreatedByUserId", model.CreatedByUserId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Delete(IVisitPlanExceptionKey key)
        {
            const string sql = @"
                DELETE FROM BTR_VisitPlanException
                WHERE VisitPlanExceptionId = @VisitPlanExceptionId";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitPlanExceptionId", key.VisitPlanExceptionId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public VisitPlanExceptionModel GetData(IVisitPlanExceptionKey key)
        {
            const string sql = @"
                SELECT
                    aa.VisitPlanExceptionId, aa.SalesPersonId, aa.VisitDate, aa.ExceptionType,
                    aa.CustomerId, aa.ReplacementCustomerId, aa.CreatedAt, aa.CreatedByUserId,
                    ISNULL(bb.CustomerName, '') CustomerName,
                    ISNULL(cc.CustomerName, '') ReplacementCustomerName
                FROM
                    BTR_VisitPlanException aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_Customer cc ON aa.ReplacementCustomerId = cc.CustomerId
                WHERE
                    aa.VisitPlanExceptionId = @VisitPlanExceptionId";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitPlanExceptionId", key.VisitPlanExceptionId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.ReadSingle<VisitPlanExceptionModel>(sql, dp);
            }
        }

        public IEnumerable<VisitPlanExceptionModel> ListData(IVisitPlanDateKey filter)
        {
            const string sql = @"
                SELECT
                    aa.VisitPlanExceptionId, aa.SalesPersonId, aa.VisitDate, aa.ExceptionType,
                    aa.CustomerId, aa.ReplacementCustomerId, aa.CreatedAt, aa.CreatedByUserId,
                    ISNULL(bb.CustomerName, '') CustomerName,
                    ISNULL(cc.CustomerName, '') ReplacementCustomerName
                FROM
                    BTR_VisitPlanException aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_Customer cc ON aa.ReplacementCustomerId = cc.CustomerId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                    AND aa.VisitDate = @VisitDate
                ORDER BY
                    aa.CreatedAt, aa.VisitPlanExceptionId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", filter.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@VisitDate", filter.VisitDate.Date, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<VisitPlanExceptionModel>(sql, dp);
            }
        }

        public IEnumerable<VisitPlanExceptionModel> ListData(VisitPlanDateRangeFilter filter)
        {
            const string sql = @"
                SELECT
                    aa.VisitPlanExceptionId, aa.SalesPersonId, aa.VisitDate, aa.ExceptionType,
                    aa.CustomerId, aa.ReplacementCustomerId, aa.CreatedAt, aa.CreatedByUserId,
                    ISNULL(bb.CustomerName, '') CustomerName,
                    ISNULL(cc.CustomerName, '') ReplacementCustomerName
                FROM
                    BTR_VisitPlanException aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                    LEFT JOIN BTR_Customer cc ON aa.ReplacementCustomerId = cc.CustomerId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                    AND aa.VisitDate >= @FromDate
                    AND aa.VisitDate <= @ToDate
                ORDER BY
                    aa.VisitDate, aa.CreatedAt, aa.VisitPlanExceptionId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", filter.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@FromDate", filter.FromDate.Date, SqlDbType.Date);
            dp.AddParam("@ToDate", filter.ToDate.Date, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<VisitPlanExceptionModel>(sql, dp);
            }
        }
    }
}
