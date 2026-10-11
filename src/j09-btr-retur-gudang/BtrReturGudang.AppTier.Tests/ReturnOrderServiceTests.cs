using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using Xunit;

namespace BtrReturGudang.AppTier.Tests
{
    public class ReturnOrderServiceTests
    {
        private class FakeReturnOrderRepo : IReturnOrderRepo
        {
            public readonly Dictionary<string, ReturnOrder> Store = new Dictionary<string, ReturnOrder>();

            public ReturnOrder GetById(string returnOrderId) =>
                Store.TryGetValue(returnOrderId, out var ro) ? ro : null;

            public Task<ReturnOrder> GetByIdAsync(string returnOrderId) =>
                Task.FromResult(GetById(returnOrderId));

            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) =>
                Store.Values.Where(o => o.ReturnOrderDate >= startDate && o.ReturnOrderDate <= endDate && (!statusFilter.HasValue || o.StatusSync == statusFilter.Value));

            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) =>
                Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));

            public IEnumerable<ReturnOrder> GetPendingUploadList() =>
                Store.Values.Where(o => o.StatusSync == StatusSync.Draft);

            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync() =>
                Task.FromResult(GetPendingUploadList());

            public void Save(ReturnOrder returnOrder) =>
                Store[returnOrder.ReturnOrderId] = returnOrder;

            public Task SaveAsync(ReturnOrder returnOrder)
            {
                Save(returnOrder);
                return Task.CompletedTask;
            }

            public void MarkSynced(string returnOrderId, DateTime syncedDate)
            {
                if (Store.TryGetValue(returnOrderId, out var ro))
                {
                    ro.MarkSynced(syncedDate);
                }
            }

            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate)
            {
                MarkSynced(returnOrderId, syncedDate);
                return Task.CompletedTask;
            }

            public void DeleteDraft(string returnOrderId) =>
                Store.Remove(returnOrderId);

            public Task DeleteDraftAsync(string returnOrderId)
            {
                DeleteDraft(returnOrderId);
                return Task.CompletedTask;
            }
        }

        private class FakeSyncService : ISyncService
        {
            public bool IsSyncInProgress => false;
            public bool UploadTriggered { get; private set; }
            public string LastUploadedId { get; private set; }

            public Task<SyncResultDto> TriggerDocumentUploadAsync(string returnOrderId)
            {
                UploadTriggered = true;
                LastUploadedId = returnOrderId;
                return Task.FromResult(SyncResultDto.Ok("ReturnOrder", pushed: 1));
            }

            public Task<IEnumerable<ReturnOrderDto>> QueryPendingQueueAsync() =>
                Task.FromResult(Enumerable.Empty<ReturnOrderDto>());

            public Task<IEnumerable<BarcodeRegistrationDto>> QueryPendingRegistrationQueueAsync() =>
                Task.FromResult(Enumerable.Empty<BarcodeRegistrationDto>());

            public Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null) =>
                Task.FromResult(SyncResultDto.Ok("MasterData"));

            public Task<SyncResultDto> RunFullSyncPassAsync() =>
                Task.FromResult(SyncResultDto.Ok("Full"));
        }

        [Fact]
        public void Create_And_AddItem_PersistsAndReturnsDto()
        {
            var repo = new FakeReturnOrderRepo();
            var service = new ReturnOrderService(repo);

            var created = service.Create(
                "CUST-001",
                "Toko Berkah",
                "WHS-01",
                "Admin-1",
                keterangan: "Retur nota 123");

            Assert.NotNull(created);
            Assert.False(string.IsNullOrEmpty(created.ReturnOrderId));
            Assert.Equal("CUST-001", created.CustId);

            var updated = service.AddItem(
                created.ReturnOrderId,
                "BRG-01",
                "Indomie Goreng",
                "8991234567890",
                JenisRetur.BAGUS,
                5,
                "PCS");

            Assert.Single(updated.Items);
            Assert.Equal(5m, updated.TotalQtyBagus);
            Assert.Single(repo.Store[created.ReturnOrderId].Items);
        }

        [Fact]
        public void UpdateLine_And_RemoveLine_ModifiesOrderLines()
        {
            var repo = new FakeReturnOrderRepo();
            var service = new ReturnOrderService(repo);

            var created = service.Create("CUST-001", "Toko Berkah", "WHS-01", "Admin-1");
            service.AddItem(created.ReturnOrderId, "BRG-01", "Indomie", "8991", JenisRetur.BAGUS, 5, "PCS");
            service.AddItem(created.ReturnOrderId, "BRG-02", "Sedaap", "8992", JenisRetur.RUSAK, 2, "PCS");

            // Update item 1
            var updated = service.UpdateLine(created.ReturnOrderId, 1, 10, JenisRetur.BAGUS, "Karton utuh");
            Assert.Equal(10m, updated.Items.First(i => i.NoUrut == 1).Qty);

            // Remove item 2
            var afterRemove = service.RemoveLine(created.ReturnOrderId, 2);
            Assert.Single(afterRemove.Items);
        }

        [Fact]
        public async Task CommitAndUploadAsync_InvokesSyncService()
        {
            var repo = new FakeReturnOrderRepo();
            var sync = new FakeSyncService();
            var service = new ReturnOrderService(repo, sync);

            var created = service.Create("CUST-001", "Toko Berkah", "WHS-01", "Admin-1");
            service.AddItem(created.ReturnOrderId, "BRG-01", "Indomie", "8991", JenisRetur.BAGUS, 5, "PCS");

            var result = await service.CommitAndUploadAsync(created.ReturnOrderId);

            Assert.True(result.Success);
            Assert.True(sync.UploadTriggered);
            Assert.Equal(created.ReturnOrderId, sync.LastUploadedId);
        }
    }
}
