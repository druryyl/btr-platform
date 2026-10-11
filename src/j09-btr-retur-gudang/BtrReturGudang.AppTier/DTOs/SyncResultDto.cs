using System;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Encapsulates the execution result of a synchronization pass.
    /// </summary>
    public class SyncResultDto
    {
        public bool Success { get; set; }
        public string SyncType { get; set; }
        public int RecordsPushed { get; set; }
        public int RecordsPulled { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;

        public static SyncResultDto Ok(string syncType, int pushed = 0, int pulled = 0)
        {
            return new SyncResultDto
            {
                Success = true,
                SyncType = syncType,
                RecordsPushed = pushed,
                RecordsPulled = pulled,
                ErrorMessage = null,
                CompletedAt = DateTime.Now
            };
        }

        public static SyncResultDto Fail(string syncType, string errorMessage, int pushed = 0, int pulled = 0)
        {
            return new SyncResultDto
            {
                Success = false,
                SyncType = syncType,
                RecordsPushed = pushed,
                RecordsPulled = pulled,
                ErrorMessage = errorMessage,
                CompletedAt = DateTime.Now
            };
        }
    }
}
