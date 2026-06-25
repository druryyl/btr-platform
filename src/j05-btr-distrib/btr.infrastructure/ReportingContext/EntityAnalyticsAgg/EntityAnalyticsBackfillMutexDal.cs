using System;
using System.Data.SqlClient;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.EntityAnalyticsAgg
{
    public class EntityAnalyticsBackfillMutexDal : IEntityAnalyticsBackfillMutex
    {
        private readonly DatabaseOptions _opt;

        public EntityAnalyticsBackfillMutexDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Acquire(string entityType, string jobId, bool skipLiveMutexCheck)
        {
            if (!skipLiveMutexCheck)
                EnsureLiveRefreshNotRunning(entityType);

            const string selectSql = @"
SELECT BackfillJobId
FROM BTRPD_EntityAnalytics_BackfillLock
WHERE EntityType = @EntityType";

            const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_BackfillLock (EntityType, BackfillJobId, AcquiredAt)
VALUES (@EntityType, @JobId, @AcquiredAt)";

            const string updateSql = @"
UPDATE BTRPD_EntityAnalytics_BackfillLock
SET BackfillJobId = @JobId,
    AcquiredAt = @AcquiredAt
WHERE EntityType = @EntityType";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var holderJobId = conn.QuerySingleOrDefault<string>(selectSql, new { EntityType = entityType });
                if (!string.IsNullOrWhiteSpace(holderJobId)
                    && !string.Equals(holderJobId, jobId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Entity analytics backfill lock is already held for entity type '{entityType}' " +
                        $"(BackfillJobId={holderJobId}).");
                }

                if (string.IsNullOrWhiteSpace(holderJobId))
                {
                    conn.Execute(insertSql, new
                    {
                        EntityType = entityType,
                        JobId = jobId,
                        AcquiredAt = DateTime.Now
                    });
                }
                else
                {
                    conn.Execute(updateSql, new
                    {
                        EntityType = entityType,
                        JobId = jobId,
                        AcquiredAt = DateTime.Now
                    });
                }
            }
        }

        public void Release(string entityType, string jobId)
        {
            const string sql = @"
DELETE FROM BTRPD_EntityAnalytics_BackfillLock
WHERE EntityType = @EntityType
  AND BackfillJobId = @JobId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, new { EntityType = entityType, JobId = jobId });
            }
        }

        private void EnsureLiveRefreshNotRunning(string entityType)
        {
            if (!EntityAnalyticsBackfillExecutionOrder.EntityTypeToWorkerDomain
                .TryGetValue(entityType, out var workerDomain))
            {
                return;
            }

            const string sql = @"
SELECT TOP 1 RefreshLogId
FROM BTRPD_RefreshLog
WHERE Domain = @Domain
  AND Status = 'Running'";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var runningId = conn.QuerySingleOrDefault<string>(sql, new { Domain = workerDomain });
                if (!string.IsNullOrWhiteSpace(runningId))
                {
                    throw new InvalidOperationException(
                        $"Live dashboard refresh is running for domain '{workerDomain}' (RefreshLogId={runningId}). " +
                        "Pause live refresh before starting backfill.");
                }
            }
        }
    }
}
