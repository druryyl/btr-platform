using System;
using BtrReturGudang.Domain.Common;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Represents an execution record in the BTRG_SyncLog table.
    /// </summary>
    public class SyncLogEntry
    {
        public string SyncLogId { get; set; }
        public DateTime SyncTimestamp { get; set; } = DateTime.Now;
        public string SyncType { get; set; } // 'ReturnOrder', 'Barcode', 'MasterData'
        public string Status { get; set; }   // 'SUCCESS', 'FAILED'
        public int RecordsPushed { get; set; }
        public int RecordsPulled { get; set; }
        public string ErrorMessage { get; set; }

        public SyncLogEntry()
        {
            SyncLogId = UlidGenerator.NewUlid();
        }

        public SyncLogEntry(
            string syncType,
            string status,
            int recordsPushed,
            int recordsPulled,
            string errorMessage = null,
            string syncLogId = null,
            DateTime? syncTimestamp = null)
        {
            SyncLogId = syncLogId ?? UlidGenerator.NewUlid();
            SyncTimestamp = syncTimestamp ?? DateTime.Now;
            SyncType = syncType;
            Status = status;
            RecordsPushed = recordsPushed;
            RecordsPulled = recordsPulled;
            ErrorMessage = errorMessage;
        }

        public static SyncLogEntry FromResult(SyncResultDto result)
        {
            if (result == null) return null;
            return new SyncLogEntry(
                result.SyncType,
                result.Success ? "SUCCESS" : "FAILED",
                result.RecordsPushed,
                result.RecordsPulled,
                result.ErrorMessage);
        }
    }
}
