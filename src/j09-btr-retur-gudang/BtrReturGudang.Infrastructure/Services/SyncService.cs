using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.Infrastructure.Services
{
    /// <summary>
    /// Dual-trigger background synchronization engine managing document uploads and master reference synchronization.
    /// Thread-safe via Interlocked.CompareExchange re-entrancy lock.
    /// Traceable to: ARCHITECTURE §4 TD-008, §5.1, §6, P2-S05.
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly IReturnOrderRepo _returnOrderRepo;
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;
        private readonly ISyncLogRepo _syncLogRepo;
        private readonly IBtradeApiClient _apiClient;
        private readonly IRegistryHelper _registryHelper;

        private int _syncLock = 0;

        /// <summary>
        /// Indicates if an active background sync cycle is currently in progress.
        /// </summary>
        public bool IsSyncInProgress => _syncLock != 0;

        public SyncService(
            IReturnOrderRepo returnOrderRepo,
            IBarcodeRepo barcodeRepo,
            IReferenceRepo referenceRepo,
            ISyncLogRepo syncLogRepo,
            IBtradeApiClient apiClient,
            IRegistryHelper registryHelper = null)
        {
            _returnOrderRepo = returnOrderRepo ?? throw new ArgumentNullException(nameof(returnOrderRepo));
            _barcodeRepo = barcodeRepo ?? throw new ArgumentNullException(nameof(barcodeRepo));
            _referenceRepo = referenceRepo ?? throw new ArgumentNullException(nameof(referenceRepo));
            _syncLogRepo = syncLogRepo ?? throw new ArgumentNullException(nameof(syncLogRepo));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _registryHelper = registryHelper;
        }

        /// <summary>
        /// Triggers immediate upload of a single Return Order document (e.g. on F10 Save & Upload).
        /// Transitions document from Draft to Synced upon successful HTTP response.
        /// </summary>
        public async Task<SyncResultDto> TriggerDocumentUploadAsync(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
            {
                return SyncResultDto.Fail("ReturnOrder", "ReturnOrderId cannot be null or empty.");
            }

            try
            {
                var order = await _returnOrderRepo.GetByIdAsync(returnOrderId).ConfigureAwait(false);
                if (order == null)
                {
                    return SyncResultDto.Fail("ReturnOrder", $"Return Order '{returnOrderId}' not found.");
                }

                if (order.StatusSync == StatusSync.Synced)
                {
                    return SyncResultDto.Ok("ReturnOrder", pushed: 0);
                }

                var dto = ReturnOrderDto.FromDomain(order);
                var success = await _apiClient.UploadReturnOrderAsync(dto).ConfigureAwait(false);

                if (success)
                {
                    var syncedDate = DateTime.Now;
                    await _returnOrderRepo.MarkSyncedAsync(returnOrderId, syncedDate).ConfigureAwait(false);

                    var log = new SyncLogEntry(
                        "ReturnOrder",
                        "SUCCESS",
                        1,
                        0,
                        $"Return Order '{returnOrderId}' successfully uploaded and marked Synced.");
                    await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                    return SyncResultDto.Ok("ReturnOrder", pushed: 1);
                }
                else
                {
                    var log = new SyncLogEntry(
                        "ReturnOrder",
                        "FAILED",
                        0,
                        0,
                        $"Cloud API rejected upload of Return Order '{returnOrderId}'.");
                    await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                    return SyncResultDto.Fail("ReturnOrder", $"Failed to upload Return Order '{returnOrderId}' to Cloud API.");
                }
            }
            catch (Exception ex)
            {
                var log = new SyncLogEntry(
                    "ReturnOrder",
                    "FAILED",
                    0,
                    0,
                    $"Exception uploading Return Order '{returnOrderId}': {ex.Message}");
                await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                return SyncResultDto.Fail("ReturnOrder", ex.Message);
            }
        }

        /// <summary>
        /// Queries the local queue of Return Orders that are still in Draft (un-synced) state.
        /// </summary>
        public async Task<IEnumerable<ReturnOrderDto>> QueryPendingQueueAsync()
        {
            var pendingOrders = await _returnOrderRepo.GetPendingUploadListAsync().ConfigureAwait(false);
            return pendingOrders.Select(ReturnOrderDto.FromDomain).ToList();
        }

        /// <summary>
        /// Queries the local queue of pending on-the-spot Barcode Registrations.
        /// </summary>
        public async Task<IEnumerable<BarcodeRegistrationDto>> QueryPendingRegistrationQueueAsync()
        {
            var pendingRegs = await _barcodeRepo.GetPendingRegistrationsAsync().ConfigureAwait(false);
            return pendingRegs.Select(r => BarcodeRegistrationDto.FromDomain(r)).ToList();
        }

        /// <summary>
        /// Pulls reference data deltas (Barang, Barcode, Customer, etc.) from the Cloud API and updates local caches.
        /// </summary>
        public async Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null)
        {
            var effectiveServerId = !string.IsNullOrWhiteSpace(serverId)
                ? serverId
                : _registryHelper?.ServerTargetId;

            try
            {
                var master = await _apiClient.DownloadMasterDataAsync(effectiveServerId).ConfigureAwait(false);
                if (master == null)
                {
                    return SyncResultDto.Fail("MasterData", "No response received from Cloud API.");
                }

                int pulledItems = 0;
                int pulledBarcodes = 0;
                int pulledCustomers = 0;
                int pulledDrivers = 0;
                int pulledSalesPersons = 0;

                // 1. Barang
                if (master.Items != null && master.Items.Any())
                {
                    var items = master.Items.Select(i => i.ToDomain()).ToList();
                    await _referenceRepo.BulkUpsertBarangAsync(items).ConfigureAwait(false);
                    pulledItems = items.Count;
                }

                // 2. Barcodes
                if (master.Barcodes != null && master.Barcodes.Any())
                {
                    var barcodes = master.Barcodes.Select(b => b.ToDomain()).ToList();
                    await _barcodeRepo.BulkUpsertBarcodesAsync(barcodes).ConfigureAwait(false);
                    pulledBarcodes = barcodes.Count;
                }

                // 3. Customers
                if (master.Customers != null && master.Customers.Any())
                {
                    var customers = master.Customers.Select(c => c.ToDomain()).ToList();
                    await _referenceRepo.BulkUpsertCustomerAsync(customers).ConfigureAwait(false);
                    pulledCustomers = customers.Count;
                }

                // 4. Drivers
                if (master.Drivers != null && master.Drivers.Any())
                {
                    var drivers = master.Drivers.Select(d => d.ToDomain()).ToList();
                    await _referenceRepo.BulkUpsertDriverAsync(drivers).ConfigureAwait(false);
                    pulledDrivers = drivers.Count;
                }

                // 5. SalesPersons
                if (master.SalesPersons != null && master.SalesPersons.Any())
                {
                    var sps = master.SalesPersons.Select(s => s.ToDomain()).ToList();
                    await _referenceRepo.BulkUpsertSalesPersonAsync(sps).ConfigureAwait(false);
                    pulledSalesPersons = sps.Count;
                }

                int totalPulled = pulledItems + pulledBarcodes + pulledCustomers + pulledDrivers + pulledSalesPersons;

                var log = new SyncLogEntry(
                    "MasterData",
                    "SUCCESS",
                    0,
                    totalPulled,
                    $"Reference sync complete: {pulledItems} items, {pulledBarcodes} barcodes, {pulledCustomers} customers, {pulledDrivers} drivers, {pulledSalesPersons} sales.");
                await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                return SyncResultDto.Ok("MasterData", pulled: totalPulled);
            }
            catch (Exception ex)
            {
                var log = new SyncLogEntry(
                    "MasterData",
                    "FAILED",
                    0,
                    0,
                    $"Reference sync failed: {ex.Message}");
                await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                return SyncResultDto.Fail("MasterData", ex.Message);
            }
        }

        /// <summary>
        /// Executes a complete background synchronization cycle (push pending barcodes, push pending orders, pull deltas).
        /// Guarded against overlapping executions via Interlocked.CompareExchange.
        /// </summary>
        public async Task<SyncResultDto> RunFullSyncPassAsync()
        {
            // Acquire re-entrancy lock
            if (Interlocked.CompareExchange(ref _syncLock, 1, 0) != 0)
            {
                return SyncResultDto.Fail("FullSync", "Synchronization pass already in progress.");
            }

            int pushedBarcodes = 0;
            int pushedOrders = 0;
            int pulledReferences = 0;

            try
            {
                // 1. Push pending barcode registrations
                var pendingRegs = await _barcodeRepo.GetPendingRegistrationsAsync().ConfigureAwait(false);
                foreach (var reg in pendingRegs)
                {
                    var regDto = BarcodeRegistrationDto.FromDomain(reg);
                    var ok = await _apiClient.UploadBarcodeRegistrationAsync(regDto).ConfigureAwait(false);
                    if (ok)
                    {
                        await _barcodeRepo.MarkRegistrationUploadedAsync(reg.RequestId).ConfigureAwait(false);
                        pushedBarcodes++;
                    }
                }

                // 2. Push pending Return Orders
                var pendingOrders = await _returnOrderRepo.GetPendingUploadListAsync().ConfigureAwait(false);
                foreach (var order in pendingOrders)
                {
                    var orderDto = ReturnOrderDto.FromDomain(order);
                    var ok = await _apiClient.UploadReturnOrderAsync(orderDto).ConfigureAwait(false);
                    if (ok)
                    {
                        await _returnOrderRepo.MarkSyncedAsync(order.ReturnOrderId, DateTime.Now).ConfigureAwait(false);
                        pushedOrders++;
                    }
                }

                // 3. Pull reference deltas
                var refResult = await PullReferenceDeltasAsync().ConfigureAwait(false);
                pulledReferences = refResult.RecordsPulled;

                int totalPushed = pushedBarcodes + pushedOrders;
                var log = new SyncLogEntry(
                    "FullSync",
                    "SUCCESS",
                    totalPushed,
                    pulledReferences,
                    $"Full sync pass completed: {pushedBarcodes} barcodes pushed, {pushedOrders} orders pushed, {pulledReferences} records pulled.");
                await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                return SyncResultDto.Ok("FullSync", pushed: totalPushed, pulled: pulledReferences);
            }
            catch (Exception ex)
            {
                int totalPushed = pushedBarcodes + pushedOrders;
                var log = new SyncLogEntry(
                    "FullSync",
                    "FAILED",
                    totalPushed,
                    pulledReferences,
                    $"Full sync pass failed: {ex.Message}");
                await _syncLogRepo.LogSyncAsync(log).ConfigureAwait(false);

                return SyncResultDto.Fail("FullSync", ex.Message, pushed: totalPushed, pulled: pulledReferences);
            }
            finally
            {
                // Release re-entrancy lock
                Interlocked.Exchange(ref _syncLock, 0);
            }
        }
    }
}
