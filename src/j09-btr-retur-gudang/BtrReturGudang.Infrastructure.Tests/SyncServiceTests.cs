using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Infrastructure.Services;
using Xunit;

namespace BtrReturGudang.Infrastructure.Tests
{
    public class SyncServiceTests
    {
        #region Test Fakes

        private class FakeReturnOrderRepo : IReturnOrderRepo
        {
            public Dictionary<string, ReturnOrder> Orders { get; } = new Dictionary<string, ReturnOrder>();
            public List<string> SyncedMarkedOrders { get; } = new List<string>();

            public ReturnOrder GetById(string returnOrderId) => Orders.TryGetValue(returnOrderId, out var o) ? o : null;
            public Task<ReturnOrder> GetByIdAsync(string returnOrderId) => Task.FromResult(GetById(returnOrderId));

            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
            {
                var q = Orders.Values.Where(o => o.ReturnOrderDate >= startDate && o.ReturnOrderDate <= endDate);
                if (statusFilter.HasValue) q = q.Where(o => o.StatusSync == statusFilter.Value);
                return q.ToList();
            }

            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
                => Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));

            public IEnumerable<ReturnOrder> GetPendingUploadList() => Orders.Values.Where(o => o.StatusSync == StatusSync.Draft).ToList();
            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync() => Task.FromResult(GetPendingUploadList());

            public void Save(ReturnOrder returnOrder) => Orders[returnOrder.ReturnOrderId] = returnOrder;
            public Task SaveAsync(ReturnOrder returnOrder) { Save(returnOrder); return Task.CompletedTask; }

            public void MarkSynced(string returnOrderId, DateTime syncedDate)
            {
                if (Orders.TryGetValue(returnOrderId, out var order))
                {
                    order.MarkSynced(syncedDate);
                    SyncedMarkedOrders.Add(returnOrderId);
                }
            }

            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate)
            {
                MarkSynced(returnOrderId, syncedDate);
                return Task.CompletedTask;
            }

            public void DeleteDraft(string returnOrderId) => Orders.Remove(returnOrderId);
            public Task DeleteDraftAsync(string returnOrderId) { DeleteDraft(returnOrderId); return Task.CompletedTask; }
        }

        private class FakeBarcodeRepo : IBarcodeRepo
        {
            public Dictionary<string, Barcode> Barcodes { get; } = new Dictionary<string, Barcode>();
            public List<BarcodeRegistration> Registrations { get; } = new List<BarcodeRegistration>();
            public List<string> UploadedRegistrations { get; } = new List<string>();

            public Barcode LookupByBarcode(string barcode) => Barcodes.TryGetValue(barcode, out var b) ? b : null;
            public Task<Barcode> LookupByBarcodeAsync(string barcode) => Task.FromResult(LookupByBarcode(barcode));

            public void InsertBarcode(Barcode barcode) => Barcodes[barcode.BarcodeValue] = barcode;
            public Task InsertBarcodeAsync(Barcode barcode) { InsertBarcode(barcode); return Task.CompletedTask; }

            public void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes)
            {
                foreach (var b in barcodes) Barcodes[b.BarcodeValue] = b;
            }
            public Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes) { BulkUpsertBarcodes(barcodes); return Task.CompletedTask; }

            public IEnumerable<Barcode> GetAllBarcodes() => Barcodes.Values.ToList();
            public Task<IEnumerable<Barcode>> GetAllBarcodesAsync() => Task.FromResult(GetAllBarcodes());

            public IEnumerable<Barcode> SearchBarcodes(string keyword) => Barcodes.Values.Where(b => b.BarcodeValue.Contains(keyword) || b.BrgName.Contains(keyword)).ToList();
            public Task<IEnumerable<Barcode>> SearchBarcodesAsync(string keyword) => Task.FromResult(SearchBarcodes(keyword));

            public void QueueRegistration(BarcodeRegistration registration) => Registrations.Add(registration);
            public Task QueueRegistrationAsync(BarcodeRegistration registration) { QueueRegistration(registration); return Task.CompletedTask; }

            public IEnumerable<BarcodeRegistration> GetPendingRegistrations() => Registrations.Where(r => r.StatusSync == BarcodeRegistrationStatus.Pending).ToList();
            public Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync() => Task.FromResult(GetPendingRegistrations());

            public void MarkRegistrationUploaded(string requestId)
            {
                var reg = Registrations.FirstOrDefault(r => r.RequestId == requestId);
                if (reg != null)
                {
                    reg.MarkUploaded();
                    UploadedRegistrations.Add(requestId);
                }
            }
            public Task MarkRegistrationUploadedAsync(string requestId) { MarkRegistrationUploaded(requestId); return Task.CompletedTask; }
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public Dictionary<string, Barang> BarangList { get; } = new Dictionary<string, Barang>();
            public Dictionary<string, Customer> CustomerList { get; } = new Dictionary<string, Customer>();
            public Dictionary<string, Driver> DriverList { get; } = new Dictionary<string, Driver>();
            public Dictionary<string, SalesPerson> SalesPersonList { get; } = new Dictionary<string, SalesPerson>();

            public void BulkUpsertBarang(IEnumerable<Barang> items) { foreach (var i in items) BarangList[i.BrgId] = i; }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) { BulkUpsertBarang(items); return Task.CompletedTask; }
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => BarangList.Values.Where(b => b.BrgId.Contains(keyword) || b.BrgName.Contains(keyword)).ToList();
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(SearchBarangByNameOrCode(keyword));
            public Barang GetBarangById(string brgId) => BarangList.TryGetValue(brgId, out var b) ? b : null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { foreach (var c in customers) CustomerList[c.CustId] = c; }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) { BulkUpsertCustomer(customers); return Task.CompletedTask; }
            public IEnumerable<Customer> SearchCustomer(string keyword) => CustomerList.Values.Where(c => c.CustId.Contains(keyword) || c.CustName.Contains(keyword)).ToList();
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult(SearchCustomer(keyword));
            public Customer GetCustomerById(string custId) => CustomerList.TryGetValue(custId, out var c) ? c : null;
            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult(GetCustomerById(custId));

            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { foreach (var d in drivers) DriverList[d.DriverId] = d; }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) { BulkUpsertDriver(drivers); return Task.CompletedTask; }
            public IEnumerable<Driver> GetAllDrivers() => DriverList.Values.ToList();
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(GetAllDrivers());

            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { foreach (var s in salesPersons) SalesPersonList[s.SalesPersonId] = s; }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) { BulkUpsertSalesPerson(salesPersons); return Task.CompletedTask; }
            public IEnumerable<SalesPerson> GetAllSalesPersons() => SalesPersonList.Values.ToList();
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(GetAllSalesPersons());
        }

        private class FakeSyncLogRepo : ISyncLogRepo
        {
            public List<SyncLogEntry> Logs { get; } = new List<SyncLogEntry>();

            public void LogSync(SyncLogEntry entry) => Logs.Add(entry);
            public Task LogSyncAsync(SyncLogEntry entry) { LogSync(entry); return Task.CompletedTask; }

            public IEnumerable<SyncLogEntry> GetRecentLogs(int count = 50) => Logs.Take(count).ToList();
            public Task<IEnumerable<SyncLogEntry>> GetRecentLogsAsync(int count = 50) => Task.FromResult(GetRecentLogs(count));

            public DateTime? GetLastSuccessfulSyncTimestamp(string syncType)
                => Logs.Where(l => l.SyncType == syncType && l.Status == "SUCCESS").OrderByDescending(l => l.SyncTimestamp).FirstOrDefault()?.SyncTimestamp;
            public Task<DateTime?> GetLastSuccessfulSyncTimestampAsync(string syncType) => Task.FromResult(GetLastSuccessfulSyncTimestamp(syncType));
        }

        private class FakeApiClient : IBtradeApiClient
        {
            public ApiUploadResultDto ReturnOrderUploadResult { get; set; } = ApiUploadResultDto.Ok();
            public ApiUploadResultDto BarcodeRegistrationUploadResult { get; set; } = ApiUploadResultDto.Ok();
            public MasterSyncDto DownloadMasterDataResult { get; set; } = new MasterSyncDto();

            public List<ReturnOrderDto> UploadedOrders { get; } = new List<ReturnOrderDto>();
            public List<BarcodeRegistrationDto> UploadedBarcodes { get; } = new List<BarcodeRegistrationDto>();

            public SessionResolveResultDto SessionResolveResult { get; set; } =
                SessionResolveResultDto.Ok(200, null, "USR-001", "Operator1", "ROLE-1", null);

            public List<string> ResolvedEmails { get; } = new List<string>();

            public int DelayMs { get; set; } = 0;

            public async Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto)
            {
                if (DelayMs > 0) await Task.Delay(DelayMs);
                UploadedOrders.Add(dto);
                return ReturnOrderUploadResult;
            }

            public async Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)
            {
                if (DelayMs > 0) await Task.Delay(DelayMs);
                UploadedBarcodes.Add(dto);
                return BarcodeRegistrationUploadResult;
            }

            public string LastRequestedServerId { get; set; }

            public async Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
            {
                if (DelayMs > 0) await Task.Delay(DelayMs);
                LastRequestedServerId = serverId;
                return DownloadMasterDataResult;
            }

            public Task<SessionResolveResultDto> ResolveSessionAsync(string email)
            {
                ResolvedEmails.Add(email);
                return Task.FromResult(SessionResolveResult);
            }
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "";
            public string ApiBaseUrl { get; set; } = BtrReturGudang.Infrastructure.Helpers.RegistryHelper.DefaultApiBaseUrl;
            public string LastOperatorName { get; set; } = "Operator1";
            public string LastOperatorEmail { get; set; } = "operator1@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        #endregion

        [Fact]
        public async Task TriggerDocumentUploadAsync_SuccessfulUpload_TransitionsDraftToSynced()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient();

            var order = new ReturnOrder("ORD-001", DateTime.Today, "C01", "Toko Subur", "GDG-PST", "Operator1");
            order.AddItem("B01", "Indomie", "899123", JenisRetur.BAGUS, 5, "Dus");
            returnRepo.Save(order);

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            var result = await syncService.TriggerDocumentUploadAsync("ORD-001");

            Assert.True(result.Success);
            Assert.Equal(1, result.RecordsPushed);
            Assert.Equal(StatusSync.Synced, order.StatusSync);
            Assert.Contains("ORD-001", returnRepo.SyncedMarkedOrders);

            var log = Assert.Single(logRepo.Logs);
            Assert.Equal("ReturnOrder", log.SyncType);
            Assert.Equal("SUCCESS", log.Status);
            Assert.Equal(1, log.RecordsPushed);
        }

        [Fact]
        public async Task TriggerDocumentUploadAsync_FailedUpload_RemainsDraftAndLogsFailure()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient { ReturnOrderUploadResult = ApiUploadResultDto.Fail(500, "Internal Server Error") };

            var order = new ReturnOrder("ORD-002", DateTime.Today, "C01", "Toko Subur", "GDG-PST", "Operator1");
            returnRepo.Save(order);

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            var result = await syncService.TriggerDocumentUploadAsync("ORD-002");

            Assert.False(result.Success);
            Assert.Equal(0, result.RecordsPushed);
            Assert.Equal(StatusSync.Draft, order.StatusSync);
            Assert.DoesNotContain("ORD-002", returnRepo.SyncedMarkedOrders);

            var log = Assert.Single(logRepo.Logs);
            Assert.Equal("ReturnOrder", log.SyncType);
            Assert.Equal("FAILED", log.Status);
        }

        [Fact]
        public async Task TriggerDocumentUploadAsync_FailedUpload_PropagatesHttpStatusAndErrorIntoResultAndLog()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();

            var errorMessage = "Return order endpoint not found";
            var apiClient = new FakeApiClient
            {
                ReturnOrderUploadResult = ApiUploadResultDto.Fail(404, errorMessage, "{\"status\":\"error\"}")
            };

            var order = new ReturnOrder("ORD-003", DateTime.Today, "C01", "Toko Subur", "GDG-PST", "Operator1");
            returnRepo.Save(order);

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            var result = await syncService.TriggerDocumentUploadAsync("ORD-003");

            Assert.False(result.Success);
            Assert.Contains("HTTP 404", result.ErrorMessage);
            Assert.Contains(errorMessage, result.ErrorMessage);
            Assert.Equal(StatusSync.Draft, order.StatusSync);

            var log = Assert.Single(logRepo.Logs);
            Assert.Equal("ReturnOrder", log.SyncType);
            Assert.Equal("FAILED", log.Status);
            Assert.Contains("HTTP 404", log.ErrorMessage);
            Assert.Contains(errorMessage, log.ErrorMessage);
        }

        [Fact]
        public async Task PullReferenceDeltasAsync_BulkUpsertsAllReferenceEntities()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient();

            apiClient.DownloadMasterDataResult = new MasterSyncDto
            {
                ServerId = "SERVER-01",
                Barcodes = new List<BarcodeDto>
                {
                    new BarcodeDto { BarcodeValue = "899001", BrgId = "BRG-01", BrgName = "Kopi", Satuan = "Pcs" }
                },
                Items = new List<BarangDto>
                {
                    new BarangDto { BrgId = "BRG-01", BrgName = "Kopi", Satuan1 = "Dus", Satuan2 = "Pcs" }
                },
                Customers = new List<CustomerDto>
                {
                    new CustomerDto { CustId = "CUST-01", CustName = "Toko Makmur", Alamat = "Jl. Solo" }
                },
                Drivers = new List<DriverDto>
                {
                    new DriverDto { DriverId = "DRV-01", DriverName = "Bambang" }
                },
                SalesPersons = new List<SalesPersonDto>
                {
                    new SalesPersonDto { SalesPersonId = "SLS-01", SalesPersonName = "Dewi" }
                }
            };

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            var result = await syncService.PullReferenceDeltasAsync("SERVER-01");

            Assert.True(result.Success);
            Assert.Equal(5, result.RecordsPulled);

            Assert.Single(barcodeRepo.Barcodes);
            Assert.Single(refRepo.BarangList);
            Assert.Single(refRepo.CustomerList);
            Assert.Single(refRepo.DriverList);
            Assert.Single(refRepo.SalesPersonList);

            var log = Assert.Single(logRepo.Logs);
            Assert.Equal("MasterData", log.SyncType);
            Assert.Equal("SUCCESS", log.Status);
            Assert.Equal(5, log.RecordsPulled);
        }

        [Fact]
        public async Task RunFullSyncPassAsync_PushesBarcodesAndOrders_AndPullsDeltas()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient();

            // Seed pending barcode registration
            var reg = new BarcodeRegistration("REG-01", "899999", "BRG-01", "Pcs", "GDG-PST", "Operator1");
            barcodeRepo.QueueRegistration(reg);

            // Seed pending return order
            var order = new ReturnOrder("ORD-01", DateTime.Today, "C01", "Customer", "GDG-PST", "Operator1");
            order.AddItem("BRG-01", "Kopi", "899999", JenisRetur.BAGUS, 2, "Dus");
            returnRepo.Save(order);

            // Seed master data
            apiClient.DownloadMasterDataResult = new MasterSyncDto
            {
                Barcodes = new List<BarcodeDto> { new BarcodeDto { BarcodeValue = "899888", BrgId = "BRG-02", BrgName = "Teh", Satuan = "Pcs" } }
            };

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            var result = await syncService.RunFullSyncPassAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.RecordsPushed); // 1 reg + 1 order
            Assert.Equal(1, result.RecordsPulled); // 1 barcode

            Assert.Contains("REG-01", barcodeRepo.UploadedRegistrations);
            Assert.Contains("ORD-01", returnRepo.SyncedMarkedOrders);
            Assert.False(syncService.IsSyncInProgress);
        }

        [Fact]
        public async Task RunFullSyncPassAsync_ReentrancyLock_PreventsOverlappingRuns()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient { DelayMs = 200 }; // Delay to simulate in-flight execution

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient);

            // Start long running sync task
            var task1 = Task.Run(() => syncService.RunFullSyncPassAsync());

            // Wait brief moment to guarantee task1 has acquired lock
            await Task.Delay(50);

            Assert.True(syncService.IsSyncInProgress);

            // Attempt overlapping sync pass
            var overlappingResult = await syncService.RunFullSyncPassAsync();

            Assert.False(overlappingResult.Success);
            Assert.Contains("already in progress", overlappingResult.ErrorMessage);

            // Wait for first task to finish
            var result1 = await task1;
            Assert.True(result1.Success);

            // After completion, lock is released
            Assert.False(syncService.IsSyncInProgress);
        }

        [Fact]
        public async Task PullReferenceDeltasAsync_ResolvesJOG_WhenServerIdAndRegistryAreEmpty()
        {
            var returnRepo = new FakeReturnOrderRepo();
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            var logRepo = new FakeSyncLogRepo();
            var apiClient = new FakeApiClient();
            var registry = new FakeRegistryHelper { ServerTargetId = "" };

            var syncService = new SyncService(returnRepo, barcodeRepo, refRepo, logRepo, apiClient, registry);

            await syncService.PullReferenceDeltasAsync("");

            Assert.Equal("JOG", apiClient.LastRequestedServerId);
        }
    }
}
