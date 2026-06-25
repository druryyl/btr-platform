using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.EntityAnalyticsAgg
{
    public class EntityAnalyticsBackfillCheckpointStoreDal : IEntityAnalyticsBackfillCheckpointStore
    {
        private readonly DatabaseOptions _opt;

        public EntityAnalyticsBackfillCheckpointStoreDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public string CreateJob(EntityAnalyticsBackfillJobModel job)
        {
            const string sql = @"
INSERT INTO BTRPD_EntityAnalytics_BackfillJob (
    BackfillJobId, EntityTypeScope, FromPeriodYear, FromPeriodMonth,
    ToPeriodYear, ToPeriodMonth, Layers, OptionsJson, Status,
    StartedAt, CompletedAt, TriggeredBy, MachineName, LastError)
VALUES (
    @BackfillJobId, @EntityTypeScope, @FromPeriodYear, @FromPeriodMonth,
    @ToPeriodYear, @ToPeriodMonth, @Layers, @OptionsJson, @Status,
    @StartedAt, NULL, @TriggeredBy, @MachineName, '')";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, job);
            }

            return job.BackfillJobId;
        }

        public EntityAnalyticsBackfillJobModel GetJob(string jobId)
        {
            const string sql = @"
SELECT BackfillJobId, EntityTypeScope, FromPeriodYear, FromPeriodMonth,
       ToPeriodYear, ToPeriodMonth, Layers, OptionsJson, Status,
       StartedAt, CompletedAt, TriggeredBy, MachineName, LastError
FROM BTRPD_EntityAnalytics_BackfillJob
WHERE BackfillJobId = @JobId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QuerySingleOrDefault<EntityAnalyticsBackfillJobModel>(sql, new { JobId = jobId });
            }
        }

        public void UpdateJob(string jobId, Action<EntityAnalyticsBackfillJobModel> mutate)
        {
            var job = GetJob(jobId);
            if (job == null)
                throw new InvalidOperationException($"Backfill job '{jobId}' was not found.");

            mutate(job);

            const string sql = @"
UPDATE BTRPD_EntityAnalytics_BackfillJob
SET Status = @Status,
    CompletedAt = @CompletedAt,
    LastError = @LastError
WHERE BackfillJobId = @BackfillJobId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, job);
            }
        }

        public EntityAnalyticsBackfillCheckpointModel GetCheckpoint(
            string jobId,
            string entityType,
            int year,
            int month)
        {
            const string sql = @"
SELECT BackfillCheckpointId, BackfillJobId, EntityType, PeriodYear, PeriodMonth,
       Status, LayersCompleted, EntityCount, RowCountsJson,
       StartedAt, CompletedAt, LastError, LastRefreshLogId
FROM BTRPD_EntityAnalytics_BackfillCheckpoint
WHERE BackfillJobId = @JobId
  AND EntityType = @EntityType
  AND PeriodYear = @Year
  AND PeriodMonth = @Month";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QuerySingleOrDefault<EntityAnalyticsBackfillCheckpointModel>(sql, new
                {
                    JobId = jobId,
                    EntityType = entityType,
                    Year = year,
                    Month = month
                });
            }
        }

        public EntityAnalyticsBackfillCheckpointModel GetLatestCheckpoint(
            string entityType,
            int year,
            int month)
        {
            const string sql = @"
SELECT TOP 1 BackfillCheckpointId, BackfillJobId, EntityType, PeriodYear, PeriodMonth,
       Status, LayersCompleted, EntityCount, RowCountsJson,
       StartedAt, CompletedAt, LastError, LastRefreshLogId
FROM BTRPD_EntityAnalytics_BackfillCheckpoint
WHERE EntityType = @EntityType
  AND PeriodYear = @Year
  AND PeriodMonth = @Month
ORDER BY StartedAt DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QuerySingleOrDefault<EntityAnalyticsBackfillCheckpointModel>(sql, new
                {
                    EntityType = entityType,
                    Year = year,
                    Month = month
                });
            }
        }

        public IReadOnlyList<EntityAnalyticsBackfillCheckpointModel> GetCheckpointsForJob(
            string jobId,
            string entityType)
        {
            const string sql = @"
SELECT BackfillCheckpointId, BackfillJobId, EntityType, PeriodYear, PeriodMonth,
       Status, LayersCompleted, EntityCount, RowCountsJson,
       StartedAt, CompletedAt, LastError, LastRefreshLogId
FROM BTRPD_EntityAnalytics_BackfillCheckpoint
WHERE BackfillJobId = @JobId
  AND EntityType = @EntityType
ORDER BY PeriodYear, PeriodMonth";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsBackfillCheckpointModel>(sql, new
                {
                    JobId = jobId,
                    EntityType = entityType
                }).ToList();
            }
        }

        public void UpsertCheckpoint(EntityAnalyticsBackfillCheckpointModel checkpoint)
        {
            const string sql = @"
IF EXISTS (
    SELECT 1 FROM BTRPD_EntityAnalytics_BackfillCheckpoint
    WHERE BackfillJobId = @BackfillJobId
      AND EntityType = @EntityType
      AND PeriodYear = @PeriodYear
      AND PeriodMonth = @PeriodMonth)
BEGIN
    UPDATE BTRPD_EntityAnalytics_BackfillCheckpoint
    SET Status = @Status,
        LayersCompleted = @LayersCompleted,
        EntityCount = @EntityCount,
        RowCountsJson = @RowCountsJson,
        StartedAt = @StartedAt,
        CompletedAt = @CompletedAt,
        LastError = @LastError,
        LastRefreshLogId = @LastRefreshLogId
    WHERE BackfillJobId = @BackfillJobId
      AND EntityType = @EntityType
      AND PeriodYear = @PeriodYear
      AND PeriodMonth = @PeriodMonth
END
ELSE
BEGIN
    INSERT INTO BTRPD_EntityAnalytics_BackfillCheckpoint (
        BackfillCheckpointId, BackfillJobId, EntityType, PeriodYear, PeriodMonth,
        Status, LayersCompleted, EntityCount, RowCountsJson,
        StartedAt, CompletedAt, LastError, LastRefreshLogId)
    VALUES (
        @BackfillCheckpointId, @BackfillJobId, @EntityType, @PeriodYear, @PeriodMonth,
        @Status, @LayersCompleted, @EntityCount, @RowCountsJson,
        @StartedAt, @CompletedAt, @LastError, @LastRefreshLogId)
END";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, checkpoint);
            }
        }

        public void DeleteCheckpointsForScope(
            string jobId,
            string entityType,
            int fromYear,
            int fromMonth,
            int toYear,
            int toMonth)
        {
            const string sql = @"
DELETE FROM BTRPD_EntityAnalytics_BackfillCheckpoint
WHERE (@JobId IS NULL OR BackfillJobId = @JobId)
  AND (@EntityType IS NULL OR EntityType = @EntityType)
  AND (PeriodYear > @FromYear OR (PeriodYear = @FromYear AND PeriodMonth >= @FromMonth))
  AND (PeriodYear < @ToYear OR (PeriodYear = @ToYear AND PeriodMonth <= @ToMonth))";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, new
                {
                    JobId = string.IsNullOrWhiteSpace(jobId) ? null : jobId,
                    EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType,
                    FromYear = fromYear,
                    FromMonth = fromMonth,
                    ToYear = toYear,
                    ToMonth = toMonth
                });
            }
        }
    }
}
