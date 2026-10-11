namespace BtrReturGudang.Domain.Enums
{
    /// <summary>
    /// Synchronization status of a ReturnOrder document: Draft (local only) or Synced (uploaded upstream).
    /// </summary>
    public enum StatusSync
    {
        Draft,
        Synced
    }
}
