using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardFieldActivityOverviewAgg
{
    public class FieldActivityBatchVisitPlanDal : IFieldActivityBatchVisitPlanDal
    {
        private readonly DatabaseOptions _opt;

        public FieldActivityBatchVisitPlanDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public FieldActivityVisitPlanBatchResult ListByDate(DateTime visitDate)
        {
            const string planSql = @"
SELECT
    aa.VisitPlanId, aa.SalesPersonId, aa.VisitDate, aa.CustomerId,
    aa.NoUrut, aa.HariRuteId, aa.PlanSource, aa.MaterializedAt,
    ISNULL(bb.CustomerName, '') CustomerName,
    ISNULL(bb.CustomerCode, '') CustomerCode
FROM BTR_VisitPlan aa
LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
WHERE aa.VisitDate = @VisitDate
ORDER BY aa.SalesPersonId, aa.NoUrut, aa.CustomerId";

            const string exceptionSql = @"
SELECT
    aa.VisitPlanExceptionId, aa.SalesPersonId, aa.VisitDate, aa.ExceptionType,
    aa.CustomerId, aa.ReplacementCustomerId, aa.CreatedAt, aa.CreatedByUserId,
    ISNULL(bb.CustomerName, '') CustomerName,
    ISNULL(cc.CustomerName, '') ReplacementCustomerName
FROM BTR_VisitPlanException aa
LEFT JOIN BTR_Customer bb ON aa.CustomerId = bb.CustomerId
LEFT JOIN BTR_Customer cc ON aa.ReplacementCustomerId = cc.CustomerId
WHERE aa.VisitDate = @VisitDate
ORDER BY aa.SalesPersonId, aa.CreatedAt, aa.VisitPlanExceptionId";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitDate", visitDate.Date, SqlDbType.Date);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var plans = conn.Read<VisitPlanModel>(planSql, dp)?.ToList()
                    ?? new List<VisitPlanModel>();
                var exceptions = conn.Read<VisitPlanExceptionModel>(exceptionSql, dp)?.ToList()
                    ?? new List<VisitPlanExceptionModel>();

                return new FieldActivityVisitPlanBatchResult
                {
                    BasePlans = plans,
                    Exceptions = exceptions
                };
            }
        }
    }
}
