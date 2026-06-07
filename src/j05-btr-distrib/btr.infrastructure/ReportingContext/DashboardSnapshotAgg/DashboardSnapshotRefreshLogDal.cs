using System;
using System.Data.SqlClient;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardSnapshotRefreshLogDal : IDashboardSnapshotRefreshLogDal
    {
        private readonly DatabaseOptions _opt;

        public DashboardSnapshotRefreshLogDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void InsertRunning(DashboardSnapshotRefreshLogModel model)
        {
            const string sql = @"
INSERT INTO BTR_PortalDashboardRefreshLog (
    RefreshLogId, Domain, StartedAt, CompletedAt, Status, DurationMs, ErrorMessage, TriggeredBy)
VALUES (
    @RefreshLogId, @Domain, @StartedAt, NULL, @Status, 0, '', @TriggeredBy)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, model);
            }
        }

        public void MarkSuccess(string refreshLogId, int durationMs)
        {
            const string sql = @"
UPDATE BTR_PortalDashboardRefreshLog
SET CompletedAt = @CompletedAt,
    Status = 'Success',
    DurationMs = @DurationMs,
    ErrorMessage = ''
WHERE RefreshLogId = @RefreshLogId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, new
                {
                    RefreshLogId = refreshLogId,
                    CompletedAt = DateTime.Now,
                    DurationMs = durationMs
                });
            }
        }

        public void MarkFailed(string refreshLogId, int durationMs, string errorMessage)
        {
            const string sql = @"
UPDATE BTR_PortalDashboardRefreshLog
SET CompletedAt = @CompletedAt,
    Status = 'Failed',
    DurationMs = @DurationMs,
    ErrorMessage = @ErrorMessage
WHERE RefreshLogId = @RefreshLogId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, new
                {
                    RefreshLogId = refreshLogId,
                    CompletedAt = DateTime.Now,
                    DurationMs = durationMs,
                    ErrorMessage = errorMessage ?? string.Empty
                });
            }
        }
    }
}
