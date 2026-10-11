using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.AppTier.Services
{
    /// <summary>
    /// Application service contract for background synchronization and immediate document relay.
    /// Traceable to: ARCHITECTURE §5.1, §6, P1-S03, P2-S05.
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Indicates if an active synchronization pass is currently in flight.
        /// </summary>
        bool IsSyncInProgress { get; }

        /// <summary>
        /// Triggers immediate upload of a single Return Order document (e.g. on F10 Save & Upload).
        /// </summary>
        Task<SyncResultDto> TriggerDocumentUploadAsync(string returnOrderId);

        /// <summary>
        /// Queries the local queue of Return Orders that are still in Draft (un-synced) state.
        /// </summary>
        Task<IEnumerable<ReturnOrderDto>> QueryPendingQueueAsync();

        /// <summary>
        /// Queries the local queue of pending on-the-spot Barcode Registrations.
        /// </summary>
        Task<IEnumerable<BarcodeRegistrationDto>> QueryPendingRegistrationQueueAsync();

        /// <summary>
        /// Pulls reference data deltas (Barang, Barcode, Customer, etc.) from the Cloud API and updates local caches.
        /// </summary>
        Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null);

        /// <summary>
        /// Executes a complete background synchronization cycle (push pending orders, push pending barcodes, pull deltas).
        /// </summary>
        Task<SyncResultDto> RunFullSyncPassAsync();
    }
}
