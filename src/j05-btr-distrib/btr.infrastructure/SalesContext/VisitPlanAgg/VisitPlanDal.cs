using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.VisitPlanAgg
{
    public class VisitPlanDal : IVisitPlanDal
    {
        private readonly DatabaseOptions _opt;
        private readonly ITglJamDal _tglJamDal;

        public VisitPlanDal(IOptions<DatabaseOptions> opt, ITglJamDal tglJamDal)
        {
            _opt = opt.Value;
            _tglJamDal = tglJamDal;
        }

        public IEnumerable<VisitPlanModel> ListData(IVisitPlanDateKey filter)
        {
            const string sql = @"
                SELECT
                    aa.VisitPlanId, aa.SalesPersonId, aa.VisitDate, aa.CustomerId,
                    aa.NoUrut, aa.HariRuteId, aa.PlanSource, aa.MaterializedAt,
                    ISNULL(bb.CustomerName, '') CustomerName,
                    ISNULL(bb.CustomerCode, '') CustomerCode
                FROM
                    BTR_VisitPlan aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                    AND aa.VisitDate = @VisitDate
                ORDER BY
                    aa.NoUrut, aa.CustomerId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", filter.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@VisitDate", filter.VisitDate.Date, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<VisitPlanModel>(sql, dp);
            }
        }

        public IEnumerable<VisitPlanModel> ListData(VisitPlanDateRangeFilter filter)
        {
            const string sql = @"
                SELECT
                    aa.VisitPlanId, aa.SalesPersonId, aa.VisitDate, aa.CustomerId,
                    aa.NoUrut, aa.HariRuteId, aa.PlanSource, aa.MaterializedAt,
                    ISNULL(bb.CustomerName, '') CustomerName,
                    ISNULL(bb.CustomerCode, '') CustomerCode
                FROM
                    BTR_VisitPlan aa
                    LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                    AND aa.VisitDate >= @FromDate
                    AND aa.VisitDate <= @ToDate
                ORDER BY
                    aa.VisitDate, aa.NoUrut, aa.CustomerId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", filter.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@FromDate", filter.FromDate.Date, SqlDbType.Date);
            dp.AddParam("@ToDate", filter.ToDate.Date, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<VisitPlanModel>(sql, dp);
            }
        }

        public IEnumerable<string> ListSalesPersonIdsWithRoutes()
        {
            const string sql = @"
                SELECT DISTINCT SalesPersonId
                FROM BTR_SalesRute
                WHERE SalesPersonId <> ''
                ORDER BY SalesPersonId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<string>(sql).ToList();
            }
        }

        public void DeleteFuture(string salesPersonId, DateTime fromDate, DateTime toDate)
        {
            var today = _tglJamDal.Now.Date;
            if (fromDate.Date < today)
                fromDate = today;

            if (fromDate > toDate)
                return;

            const string sql = @"
                DELETE FROM BTR_VisitPlan
                WHERE SalesPersonId = @SalesPersonId
                    AND VisitDate >= @FromDate
                    AND VisitDate <= @ToDate
                    AND VisitDate >= @Today";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", salesPersonId, SqlDbType.VarChar);
            dp.AddParam("@FromDate", fromDate.Date, SqlDbType.Date);
            dp.AddParam("@ToDate", toDate.Date, SqlDbType.Date);
            dp.AddParam("@Today", today, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void BulkInsert(IEnumerable<VisitPlanModel> rows)
        {
            var fetched = rows?.ToList() ?? new List<VisitPlanModel>();
            if (fetched.Count == 0)
                return;

            foreach (var row in fetched)
            {
                if (string.IsNullOrWhiteSpace(row.VisitPlanId))
                    row.VisitPlanId = Ulid.NewUlid().ToString();
            }

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            using (var bcp = new SqlBulkCopy(conn))
            {
                bcp.AddMap("VisitPlanId", "VisitPlanId");
                bcp.AddMap("SalesPersonId", "SalesPersonId");
                bcp.AddMap("VisitDate", "VisitDate");
                bcp.AddMap("CustomerId", "CustomerId");
                bcp.AddMap("NoUrut", "NoUrut");
                bcp.AddMap("HariRuteId", "HariRuteId");
                bcp.AddMap("PlanSource", "PlanSource");
                bcp.AddMap("MaterializedAt", "MaterializedAt");

                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "dbo.BTR_VisitPlan";
                conn.Open();
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }
    }
}
