using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.AppTier.Repositories
{
    /// <summary>
    /// Repository contract for BTRG_SyncLog persistence and retrieval.
    /// Traceable to: ARCHITECTURE §5.1, P1-S03, P2-S04, P2-S05.
    /// </summary>
    public interface ISyncLogRepo
    {
        void LogSync(SyncLogEntry entry);
        Task LogSyncAsync(SyncLogEntry entry);

        IEnumerable<SyncLogEntry> GetRecentLogs(int count = 50);
        Task<IEnumerable<SyncLogEntry>> GetRecentLogsAsync(int count = 50);

        DateTime? GetLastSuccessfulSyncTimestamp(string syncType);
        Task<DateTime?> GetLastSuccessfulSyncTimestampAsync(string syncType);
    }
}
