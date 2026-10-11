using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Infrastructure.Helpers;
using Dapper;

namespace BtrReturGudang.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper persistence implementation for ISyncLogRepo managing BTRG_SyncLog entries.
    /// Traceable to: ARCHITECTURE §5.1, §8, P2-S04.
    /// </summary>
    public class SyncLogDal : ISyncLogRepo
    {
        private readonly string _connectionString;

        public SyncLogDal(string connectionString = null)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => ConnStringHelper.CreateConnection(_connectionString);

        public void LogSync(SyncLogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
INSERT INTO BTRG_SyncLog (
    SyncLogId, SyncTimestamp, SyncType, Status, RecordsPushed, RecordsPulled, ErrorMessage
) VALUES (
    @SyncLogId, @SyncTimestamp, @SyncType, @Status, @RecordsPushed, @RecordsPulled, @ErrorMessage
);";

                conn.Execute(sql, new
                {
                    entry.SyncLogId,
                    entry.SyncTimestamp,
                    entry.SyncType,
                    entry.Status,
                    entry.RecordsPushed,
                    entry.RecordsPulled,
                    entry.ErrorMessage
                });
            }
        }

        public async Task LogSyncAsync(SyncLogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
INSERT INTO BTRG_SyncLog (
    SyncLogId, SyncTimestamp, SyncType, Status, RecordsPushed, RecordsPulled, ErrorMessage
) VALUES (
    @SyncLogId, @SyncTimestamp, @SyncType, @Status, @RecordsPushed, @RecordsPulled, @ErrorMessage
);";

                await conn.ExecuteAsync(sql, new
                {
                    entry.SyncLogId,
                    entry.SyncTimestamp,
                    entry.SyncType,
                    entry.Status,
                    entry.RecordsPushed,
                    entry.RecordsPulled,
                    entry.ErrorMessage
                }).ConfigureAwait(false);
            }
        }

        public IEnumerable<SyncLogEntry> GetRecentLogs(int count = 50)
        {
            if (count <= 0) count = 50;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT TOP (@Count) SyncLogId, SyncTimestamp, SyncType, Status, RecordsPushed, RecordsPulled, ErrorMessage
FROM BTRG_SyncLog
ORDER BY SyncTimestamp DESC;";

                return conn.Query<SyncLogEntry>(sql, new { Count = count }).ToList();
            }
        }

        public async Task<IEnumerable<SyncLogEntry>> GetRecentLogsAsync(int count = 50)
        {
            if (count <= 0) count = 50;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT TOP (@Count) SyncLogId, SyncTimestamp, SyncType, Status, RecordsPushed, RecordsPulled, ErrorMessage
FROM BTRG_SyncLog
ORDER BY SyncTimestamp DESC;";

                var rows = await conn.QueryAsync<SyncLogEntry>(sql, new { Count = count }).ConfigureAwait(false);
                return rows.ToList();
            }
        }

        public DateTime? GetLastSuccessfulSyncTimestamp(string syncType)
        {
            if (string.IsNullOrWhiteSpace(syncType))
                return null;

            using (var conn = CreateConnection())
            {
                conn.Open();
                const string sql = @"
SELECT MAX(SyncTimestamp)
FROM BTRG_SyncLog
WHERE SyncType = @SyncType AND Status = 'SUCCESS';";

                return conn.ExecuteScalar<DateTime?>(sql, new { SyncType = syncType });
            }
        }

        public async Task<DateTime?> GetLastSuccessfulSyncTimestampAsync(string syncType)
        {
            if (string.IsNullOrWhiteSpace(syncType))
                return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"
SELECT MAX(SyncTimestamp)
FROM BTRG_SyncLog
WHERE SyncType = @SyncType AND Status = 'SUCCESS';";

                return await conn.ExecuteScalarAsync<DateTime?>(sql, new { SyncType = syncType }).ConfigureAwait(false);
            }
        }
    }
}
