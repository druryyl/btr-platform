using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Wpf.Controls;
using BtrReturGudang.Wpf.Services;
using BtrReturGudang.Wpf.ViewModels;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    /// <summary>
    /// Comprehensive automated unit and integration test suite verifying:
    /// 1. Scan barcode -> auto-populates product name without internal code.
    /// 2. Zero prevention: typing digit when initial value is 0 sets value directly without appending 0.
    /// 3. Re-scan consecutive identical barcode increments QtyBagus.
    /// 4. Commit line item adds line and resets inputs.
    /// 5. Post-sync document locking enforcement.
    /// 6. End-to-end full workflow simulation from intake to save draft to cloud sync upload.
    /// Traceable to: IMPLEMENTATION-PLAN P5-S11, ARCHITECTURE §5, §6.
    /// </summary>
    public class WorkflowIntegrationTests
    {
        #region Test Fakes & In-Memory Services

        private class InMemoryBarcodeService : IBarcodeService
        {
            private readonly Dictionary<string, BarcodeLookupResult> _registry =
                new Dictionary<string, BarcodeLookupResult>(StringComparer.OrdinalIgnoreCase);

            public void RegisterBarcode(string barcode, string brgId, string brgName, string satuan, decimal pengali = 1m)
            {
                var clean = BarcodeNormalization.Normalize(barcode);
                _registry[clean] = BarcodeLookupResult.Hit(
                    rawInput: barcode,
                    normalizedBarcode: clean,
                    brgId: brgId,
                    brgName: brgName,
                    satuan: satuan,
                    pengali: pengali,
                    isActive: true);
            }

            public BarcodeLookupResult LookupBarcode(string barcode)
            {
                var clean = BarcodeNormalization.Normalize(barcode);
                if (_registry.TryGetValue(clean, out var hit))
                    return hit;

                return BarcodeLookupResult.Miss(barcode, clean);
            }

            public Task<BarcodeLookupResult> LookupBarcodeAsync(string barcode)
            {
                return Task.FromResult(LookupBarcode(barcode));
            }

            public BarcodeRegistrationDto QuickRegisterUnmappedBarcode(
                string rawBarcode,
                string brgId,
                string satuan,
                string warehouseCode,
                string operatorName,
                decimal pengali = 1m)
            {
                RegisterBarcode(rawBarcode, brgId, $"Item-{brgId}", satuan, pengali);
                return new BarcodeRegistrationDto
                {
                    RequestId = "REQ-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Barcode = rawBarcode,
                    BrgId = brgId,
                    Satuan = satuan
                };
            }

            public Task<BarcodeRegistrationDto> QuickRegisterUnmappedBarcodeAsync(
                string rawBarcode,
                string brgId,
                string satuan,
                string warehouseCode,
                string operatorName,
                decimal pengali = 1m)
            {
                return Task.FromResult(QuickRegisterUnmappedBarcode(rawBarcode, brgId, satuan, warehouseCode, operatorName, pengali));
            }

            public IEnumerable<BarcodeDto> SearchBarcodes(string keyword) => Enumerable.Empty<BarcodeDto>();
            public Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword) => Task.FromResult(Enumerable.Empty<BarcodeDto>());
            public IEnumerable<BarcodeRegistrationDto> GetPendingRegistrations() => Enumerable.Empty<BarcodeRegistrationDto>();
            public Task<IEnumerable<BarcodeRegistrationDto>> GetPendingRegistrationsAsync() => Task.FromResult(Enumerable.Empty<BarcodeRegistrationDto>());
            public void DeleteBarcode(string rawBarcode) => _registry.Remove(BarcodeNormalization.Normalize(rawBarcode));
            public Task DeleteBarcodeAsync(string rawBarcode)
            {
                DeleteBarcode(rawBarcode);
                return Task.CompletedTask;
            }
        }

        private class InMemoryReturnOrderRepo : IReturnOrderRepo
        {
            private readonly Dictionary<string, ReturnOrder> _orders = new Dictionary<string, ReturnOrder>(StringComparer.OrdinalIgnoreCase);

            public void Save(ReturnOrder order)
            {
                _orders[order.ReturnOrderId] = order;
            }

            public Task SaveAsync(ReturnOrder order)
            {
                Save(order);
                return Task.CompletedTask;
            }

            public ReturnOrder GetById(string returnOrderId)
            {
                return _orders.TryGetValue(returnOrderId, out var order) ? order : null;
            }

            public Task<ReturnOrder> GetByIdAsync(string returnOrderId)
            {
                return Task.FromResult(GetById(returnOrderId));
            }

            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
            {
                var query = _orders.Values.Where(o => o.ReturnOrderDate >= startDate.Date && o.ReturnOrderDate <= endDate.Date);
                if (statusFilter.HasValue)
                {
                    query = query.Where(o => o.StatusSync == statusFilter.Value);
                }
                return query.ToList();
            }

            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
            {
                return Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));
            }

            public IEnumerable<ReturnOrder> GetPendingUploadList()
            {
                return _orders.Values.Where(o => o.StatusSync == StatusSync.Draft).ToList();
            }

            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync()
            {
                return Task.FromResult(GetPendingUploadList());
            }

            public void MarkSynced(string returnOrderId, DateTime syncedDate)
            {
                if (_orders.TryGetValue(returnOrderId, out var order))
                {
                    order.MarkSynced(syncedDate);
                }
            }

            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate)
            {
                MarkSynced(returnOrderId, syncedDate);
                return Task.CompletedTask;
            }

            public void DeleteDraft(string returnOrderId)
            {
                if (_orders.TryGetValue(returnOrderId, out var order) && order.StatusSync == StatusSync.Draft)
                {
                    _orders.Remove(returnOrderId);
                }
            }

            public Task DeleteDraftAsync(string returnOrderId)
            {
                DeleteDraft(returnOrderId);
                return Task.CompletedTask;
            }
        }

        private class InMemorySyncService : ISyncService
        {
            private readonly InMemoryReturnOrderRepo _repo;
            public int UploadCount { get; private set; }
            public bool IsSyncInProgress { get; set; } = false;

            public InMemorySyncService(InMemoryReturnOrderRepo repo)
            {
                _repo = repo;
            }

            public Task<SyncResultDto> TriggerDocumentUploadAsync(string returnOrderId)
            {
                var order = _repo.GetById(returnOrderId);
                if (order == null)
                {
                    return Task.FromResult(SyncResultDto.Fail("ReturnOrder", "Document not found"));
                }

                // Simulate cloud synchronization transition
                order.MarkSynced(DateTime.Now);
                _repo.Save(order);
                UploadCount++;

                return Task.FromResult(SyncResultDto.Ok("ReturnOrder", pushed: 1));
            }

            public Task<IEnumerable<ReturnOrderDto>> QueryPendingQueueAsync()
            {
                var drafts = _repo.GetPendingUploadList().Select(ReturnOrderDto.FromDomain);
                return Task.FromResult(drafts);
            }

            public Task<IEnumerable<BarcodeRegistrationDto>> QueryPendingRegistrationQueueAsync()
            {
                return Task.FromResult(Enumerable.Empty<BarcodeRegistrationDto>());
            }

            public Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null)
            {
                return Task.FromResult(SyncResultDto.Ok("MasterData", pulled: 0));
            }

            public Task<SyncResultDto> RunFullSyncPassAsync()
            {
                return Task.FromResult(SyncResultDto.Ok("FullSyncPass", pushed: UploadCount));
            }
        }

        private class FakeAudioHelper : IAudioHelper
        {
            public bool EnableSoundFeedback { get; set; } = true;
            public int SuccessBeepCount { get; private set; }
            public int WarningBeepCount { get; private set; }

            public void PlaySuccess() => SuccessBeepCount++;
            public void PlayWarning() => WarningBeepCount++;
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "DEP-JKT01";
            public string ServerTargetId { get; set; } = "SRV-CENTRAL-01";
            public string ApiBaseUrl { get; set; } = "https://relay.btrade.example.com/api";
            public string LastOperatorName { get; set; } = "Ahmad Warehouse";
            public string LastOperatorEmail { get; set; } = "ahmad@example.com";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Customer> Customers { get; set; } = new List<Customer>
            {
                new Customer("CUST-001", "TOKO REJEKI UTAMA", "JL. RAYA BOGOR KM 28", "CUST-001", "JAKARTA TIMUR"),
                new Customer("CUST-002", "SUPERMARKET MAKMUR", "JL. SUDIRMAN KAV 10", "CUST-002", "JAKARTA PUSAT")
            };

            public List<Driver> Drivers { get; set; } = new List<Driver>
            {
                new Driver("DRV-01", "Budi Santoso")
            };

            public List<SalesPerson> SalesPersons { get; set; } = new List<SalesPerson>
            {
                new SalesPerson("SLS-01", "Siti Aminah")
            };

            public Dictionary<string, Barang> Barangs { get; } = new Dictionary<string, Barang>
            {
                ["BRG-001234-IDM"] = new Barang("BRG-001234-IDM", "INDOMIE GORENG SPESIAL 85G", "KRT", "PAK", "PCS", true),
                ["BRG-002941-KCP"] = new Barang("BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML", "DUS", "BTL", "PCS", true)
            };

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => Barangs.Values.Where(b => b.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(SearchBarangByNameOrCode(keyword));
            public Barang GetBarangById(string brgId) => Barangs.TryGetValue(brgId, out var b) ? b : null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> SearchCustomer(string keyword) => Customers.Where(c => c.CustomerName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult(SearchCustomer(keyword));
            public Customer GetCustomerById(string custId) => Customers.FirstOrDefault(c => c.CustomerId == custId);
            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult(GetCustomerById(custId));

            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) => Task.CompletedTask;
            public IEnumerable<Driver> GetAllDrivers() => Drivers;
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult<IEnumerable<Driver>>(Drivers);

            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) => Task.CompletedTask;
            public IEnumerable<SalesPerson> GetAllSalesPersons() => SalesPersons;
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult<IEnumerable<SalesPerson>>(SalesPersons);
        }

        private class FakeApiClient : IBtradeApiClient
        {
            public Task<SessionResolveResultDto> ResolveSessionAsync(string operatorEmail)
            {
                return Task.FromResult(SessionResolveResultDto.Ok(200, userName: "Ahmad Warehouse", roleId: "WarehouseStaff"));
            }

            public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto order)
            {
                return Task.FromResult(new ApiUploadResultDto { Success = true });
            }

            public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto registration)
            {
                return Task.FromResult(new ApiUploadResultDto { Success = true });
            }

            public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
            {
                return Task.FromResult(new MasterSyncDto());
            }
        }

        #endregion

        #region STA Test Runner Helper

        private static void RunInSta(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        #endregion

        #region Criterion 2.1: Scan Barcode -> Auto-populates Product Name Without Internal Code

        [Fact]
        public void Criterion_2_1_ScanBarcode_AutoPopulatesProductNameWithoutInternalCode()
        {
            // Arrange
            var barcodeService = new InMemoryBarcodeService();
            barcodeService.RegisterBarcode(
                barcode: "8992741982110",
                brgId: "BRG-002941-KCP",
                brgName: "KECAP BANGO MANIS REFILL 550ML",
                satuan: "DUS",
                pengali: 24m);

            var audioHelper = new FakeAudioHelper();
            var referenceRepo = new FakeReferenceRepo();
            var registryHelper = new FakeRegistryHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: barcodeService,
                referenceRepo: referenceRepo,
                audioHelper: audioHelper,
                registryHelper: registryHelper);

            // Act - Simulate scanning barcode via hardware scanner wedge
            vm.OnBarcodeScanned("8992741982110");

            // Assert
            // 1. Pure product name is populated prominently
            Assert.Equal("KECAP BANGO MANIS REFILL 550ML", vm.ProductName);
            // 2. Zero internal code noise in the product name
            Assert.DoesNotContain("BRG-002941-KCP", vm.ProductName);
            // 3. Technical metadata is separated cleanly into details strip
            Assert.Contains("Barcode: 8992741982110", vm.BarcodeDetails);
            Assert.Contains("Satuan Default: DUS", vm.BarcodeDetails);
            Assert.Contains("Pengali: x24", vm.BarcodeDetails);
            // 4. Banner status and audio
            Assert.Equal("BARCODE TERDETEKSI", vm.BannerStatus);
            Assert.True(vm.IsBannerActive);
            Assert.False(vm.IsBannerError);
            Assert.Equal(1, audioHelper.SuccessBeepCount);
        }

        #endregion

        #region Criterion 2.2: Zero Prevention (Anti-Zero Behavior)

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("5", 5)]
        [InlineData("9", 9)]
        public void Criterion_2_2_ZeroPrevention_TypingDigitWhenInitialValueIsZeroSetsValueDirectlyWithoutAppendingZero(string inputDigit, int expectedValue)
        {
            RunInSta(() =>
            {
                // Arrange - Control starts with value 0 / placeholder "0"
                var numericControl = new NumericTextBox
                {
                    Text = "0"
                };

                Assert.Equal(0, numericControl.Value);
                Assert.True(numericControl.IsZero);

                // Act - User presses a digit key on physical numpad/keyboard
                bool handled = !numericControl.ProcessTextInput(inputDigit);

                // Assert
                // Typing '1' directly replaces '0' without producing '10' or '01'
                Assert.True(handled);
                Assert.Equal(inputDigit, numericControl.Text);
                Assert.Equal(expectedValue, numericControl.Value);
                Assert.NotEqual("10", numericControl.Text);
                Assert.NotEqual("0" + inputDigit, numericControl.Text);
            });
        }

        [Fact]
        public void Criterion_2_2_ZeroPrevention_TypingZeroWhenAlreadyZero_SuppressesDuplicateZero()
        {
            RunInSta(() =>
            {
                var numericControl = new NumericTextBox
                {
                    Text = "0"
                };

                // Typing '0' when already '0' does not append into "00"
                bool handled = !numericControl.ProcessTextInput("0");
                Assert.True(handled);
                Assert.Equal("0", numericControl.Text);
                Assert.Equal(0, numericControl.Value);
            });
        }

        #endregion

        #region Criterion 2.3: Re-scan Consecutive Identical Barcode Increments QtyBagus

        [Fact]
        public void Criterion_2_3_RescanConsecutiveIdenticalBarcode_IncrementsQtyBagus()
        {
            // Arrange
            var barcodeService = new InMemoryBarcodeService();
            barcodeService.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);
            var audioHelper = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: barcodeService,
                referenceRepo: null,
                audioHelper: audioHelper,
                registryHelper: null);

            // Act 1: Initial scan of barcode
            vm.OnBarcodeScanned("8998866200112");

            // Assert 1: Product banner is populated, initial Qty is 0 (ready for manual entry or consecutive scan)
            Assert.Equal("INDOMIE GORENG SPESIAL", vm.ProductName);
            Assert.Equal(0, vm.QtyBagus);
            Assert.Equal(1, audioHelper.SuccessBeepCount);

            // Act 2: Second consecutive scan of identical barcode
            vm.OnBarcodeScanned("8998866200112");

            // Assert 2: QtyBagus auto-increments to 2 (initial 0 -> 2 consecutive items)
            Assert.Equal(2, vm.QtyBagus);
            Assert.Equal("QTY BAGUS: 2", vm.BannerStatus);
            Assert.Equal(2, audioHelper.SuccessBeepCount);

            // Act 3: Third consecutive scan of identical barcode
            vm.OnBarcodeScanned("8998866200112");

            // Assert 3: QtyBagus increments to 3
            Assert.Equal(3, vm.QtyBagus);
            Assert.Equal("QTY BAGUS: 3", vm.BannerStatus);
            Assert.Equal(3, audioHelper.SuccessBeepCount);

            // Act 4: Scanning a different barcode breaks consecutive sequence
            barcodeService.RegisterBarcode("8991234567890", "BRG-002", "TEH BOTOL SOSRO 250ML", "KRT", 24m);
            vm.OnBarcodeScanned("8991234567890");

            // Assert 4: New item loaded, QtyBagus resets to 0
            Assert.Equal("TEH BOTOL SOSRO 250ML", vm.ProductName);
            Assert.Equal(0, vm.QtyBagus);
        }

        #endregion

        #region Criterion 2.4: Commit Line Item Adds Line and Resets Inputs

        [Fact]
        public void Criterion_2_4_CommitLineItem_AddsLineAndResetsInputs()
        {
            // Arrange
            var barcodeService = new InMemoryBarcodeService();
            barcodeService.RegisterBarcode("8992741982110", "BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML", "DUS", 24m);
            var audioHelper = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: barcodeService,
                referenceRepo: null,
                audioHelper: audioHelper,
                registryHelper: null);
            vm.SuppressMessageBoxes = true;

            // Step 1: Scan barcode
            vm.OnBarcodeScanned("8992741982110");
            Assert.True(vm.IsBannerActive);

            // Step 2: Input quantities and remark
            vm.QtyBagus = 10;
            vm.QtyRusak = 2;
            vm.ItemKeterangan = "2 dus penyok di sudut";

            // Step 3: Commit line item via AddItemCommand (simulating "+ Tambah [Enter]")
            Assert.True(vm.AddItemCommand.CanExecute(null));
            vm.AddItemCommand.Execute(null);

            // Assert: Line items added to collection
            // In accordance with domain rules: 1 BAGUS line and 1 RUSAK line created when both quantities > 0
            Assert.Equal(2, vm.Items.Count);

            var bagusItem = vm.Items.FirstOrDefault(i => i.JenisRetur == "BAGUS");
            Assert.NotNull(bagusItem);
            Assert.Equal(10, bagusItem.Qty);
            Assert.Equal("DUS", bagusItem.Satuan);
            Assert.Equal("KECAP BANGO MANIS REFILL 550ML", bagusItem.BrgName);

            var rusakItem = vm.Items.FirstOrDefault(i => i.JenisRetur == "RUSAK");
            Assert.NotNull(rusakItem);
            Assert.Equal(2, rusakItem.Qty);
            Assert.Equal("2 dus penyok di sudut", rusakItem.Keterangan);

            // Assert: Inputs reset back to clean state for next intake scan
            Assert.Equal(0, vm.QtyBagus);
            Assert.Equal(0, vm.QtyRusak);
            Assert.Equal(string.Empty, vm.ItemKeterangan);
            Assert.Equal("ITEM DITAMBAHKAN", vm.BannerStatus);
            Assert.False(vm.IsBannerActive);

            // Assert: Summary counters updated
            Assert.Equal(2, vm.TotalItemsCount);
            Assert.Equal(10, vm.TotalQtyBagus);
            Assert.Equal(2, vm.TotalQtyRusak);
            Assert.Equal(12, vm.TotalKuantitas);
            Assert.Equal("Total Item: 2 baris", vm.TotalItemsSummaryText);
            Assert.Equal("Total Qty Bagus: 10", vm.TotalQtyBagusText);
            Assert.Equal("Total Qty Rusak: 2", vm.TotalQtyRusakText);
            Assert.Equal("Total Kuantitas: 12", vm.TotalKuantitasText);
        }

        #endregion

        #region Criterion 2.5: Post-Sync Document Locking Enforcement

        [Fact]
        public async Task Criterion_2_5_PostSyncDocumentLockingEnforcement()
        {
            // Arrange
            var repo = new InMemoryReturnOrderRepo();
            var syncService = new InMemorySyncService(repo);
            var returnOrderService = new ReturnOrderService(repo, syncService);
            var audioHelper = new FakeAudioHelper();
            var registryHelper = new FakeRegistryHelper();
            var sessionValidator = new OperatorSessionValidator(new FakeApiClient(), registryHelper);
            await sessionValidator.ValidateAsync();

            var listVm = new ReturnOrderListViewModel(returnOrderService, repo, syncService, audioHelper, sessionValidator);

            // Create a Synced order in the repository
            var order = new ReturnOrder(
                returnOrderId: "RO-SYNCED-001",
                returnOrderDate: DateTime.Today,
                custId: "CUST-001",
                custName: "TOKO REJEKI UTAMA",
                warehouseCode: "DEP-JKT01",
                operatorName: "Ahmad Warehouse",
                driverId: null,
                driverName: null,
                salesPersonId: null,
                salesPersonName: null,
                keterangan: "Order that was already synchronized upstream",
                statusSync: StatusSync.Draft);
            order.AddItem("BRG-01", "KECAP BANGO 550ML", "8992741982110", JenisRetur.BAGUS, 5, "DUS");
            order.MarkSynced(DateTime.Now);
            repo.Save(order);

            // Act - Load orders and select the Synced order
            await listVm.LoadOrdersAsync();
            listVm.SelectedOrder = listVm.Orders.First(o => o.ReturnOrderId == "RO-SYNCED-001");

            // Assert - Post-sync document locking enforcement
            // 1. IsSelectedOrderSynced is true -> triggers prominent red locked banner in UI
            Assert.True(listVm.IsSelectedOrderSynced);

            // 2. Commands are permanently disabled
            Assert.False(listVm.CanEditOrder);
            Assert.False(listVm.EditDraftCommand.CanExecute(null));

            Assert.False(listVm.CanDeleteOrder);
            Assert.False(listVm.DeleteDraftCommand.CanExecute(null));

            Assert.False(listVm.CanUploadOrder);
            Assert.False(listVm.UploadOrderCommand.CanExecute(null));

            // 3. Immutability: verifying deletion is blocked
            var initialCount = listVm.Orders.Count;
            listVm.DeleteDraftCommand.Execute(null);
            Assert.Equal(initialCount, listVm.Orders.Count);
            Assert.NotNull(repo.GetById("RO-SYNCED-001"));
        }

        #endregion

        #region Criterion 2.6: End-to-End Full Workflow Simulation (Intake -> Draft -> Cloud Sync)

        [Fact]
        public async Task Criterion_2_6_EndToEndFullWorkflowSimulation_IntakeToSaveDraftToCloudSyncUpload()
        {
            // === INFRASTRUCTURE & REPOSITORIES SETUP ===
            var repo = new InMemoryReturnOrderRepo();
            var syncService = new InMemorySyncService(repo);
            var returnOrderService = new ReturnOrderService(repo, syncService);
            var barcodeService = new InMemoryBarcodeService();
            var referenceRepo = new FakeReferenceRepo();
            var audioHelper = new FakeAudioHelper();
            var registryHelper = new FakeRegistryHelper();
            var apiClient = new FakeApiClient();
            var sessionValidator = new OperatorSessionValidator(apiClient, registryHelper);

            // Pre-seed barcode catalog
            barcodeService.RegisterBarcode("8992741982110", "BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML", "DUS", 24m);
            barcodeService.RegisterBarcode("8998866200112", "BRG-001234-IDM", "INDOMIE GORENG SPESIAL 85G", "KRT", 40m);

            // Verify operator identity gate
            await sessionValidator.ValidateAsync();
            Assert.False(sessionValidator.IsUnresolved);

            // === STAGE 1: INTAKE VIEW MODEL INITIALIZATION ===
            var intakeVm = new ReturnOrderEntryViewModel(
                returnOrderService: returnOrderService,
                barcodeService: barcodeService,
                referenceRepo: referenceRepo,
                audioHelper: audioHelper,
                registryHelper: registryHelper,
                sessionValidator: sessionValidator);
            intakeVm.SuppressMessageBoxes = true;

            // 1. Select Customer and Driver
            var customer = referenceRepo.Customers.First();
            intakeVm.SetSelectedCustomer(customer);
            Assert.Equal(customer, intakeVm.SelectedCustomer);
            Assert.Contains(customer.CustomerName, intakeVm.CustomerDisplayText);

            var driver = referenceRepo.Drivers.First();
            intakeVm.SelectedDriver = new KeyValueItem(driver.DriverId, driver.DriverName);

            // === STAGE 2: SCAN ITEM 1 (KECAP BANGO) & INPUT QUANTITIES ===
            // Simulate barcode hardware scan
            intakeVm.OnBarcodeScanned("8992741982110");
            Assert.Equal("KECAP BANGO MANIS REFILL 550ML", intakeVm.ProductName);
            Assert.DoesNotContain("BRG-002941-KCP", intakeVm.ProductName);

            // Set quantity (verifying anti-zero direct assignment)
            intakeVm.QtyBagus = 12;
            intakeVm.QtyRusak = 0;

            // Commit Item 1
            Assert.True(intakeVm.AddItemCommand.CanExecute(null));
            intakeVm.AddItemCommand.Execute(null);
            Assert.Single(intakeVm.Items);
            Assert.Equal(12, intakeVm.TotalQtyBagus);
            Assert.Equal(0, intakeVm.QtyBagus); // Reset verified

            // === STAGE 3: SCAN ITEM 2 (INDOMIE) WITH CONSECUTIVE RE-SCAN INCREMENT ===
            // First scan
            intakeVm.OnBarcodeScanned("8998866200112");
            Assert.Equal("INDOMIE GORENG SPESIAL 85G", intakeVm.ProductName);
            Assert.Equal(0, intakeVm.QtyBagus);

            // Consecutive re-scans increment count
            intakeVm.OnBarcodeScanned("8998866200112"); // Count becomes 2
            intakeVm.OnBarcodeScanned("8998866200112"); // Count becomes 3
            Assert.Equal(3, intakeVm.QtyBagus);

            // Add rusak quantity and notes
            intakeVm.QtyRusak = 1;
            intakeVm.ItemKeterangan = "1 karton basah terkena hujan";

            // Commit Item 2
            intakeVm.AddItemCommand.Execute(null);

            // === STAGE 4: VERIFY ACCUMULATED LIVE SUMMARY ===
            // Total 3 lines: Bango BAGUS 12, Indomie BAGUS 3, Indomie RUSAK 1
            Assert.Equal(3, intakeVm.Items.Count);
            Assert.Equal(15, intakeVm.TotalQtyBagus);
            Assert.Equal(1, intakeVm.TotalQtyRusak);
            Assert.Equal(16, intakeVm.TotalKuantitas);

            // === STAGE 5: SAVE DRAFT (F9 ACCELERATOR WORKFLOW) ===
            var createdDraft = await intakeVm.SaveDraftAsync();
            Assert.NotNull(createdDraft);
            Assert.False(string.IsNullOrWhiteSpace(createdDraft.ReturnOrderId));
            Assert.Equal("Draft", createdDraft.StatusSync);

            var savedOrderInRepo = repo.GetById(createdDraft.ReturnOrderId);
            Assert.NotNull(savedOrderInRepo);
            Assert.Equal(StatusSync.Draft, savedOrderInRepo.StatusSync);
            Assert.Equal(3, savedOrderInRepo.Items.Count);

            // === STAGE 6: INSPECT ORDER IN RETURN ORDER LIST VIEW ===
            var listVm = new ReturnOrderListViewModel(returnOrderService, repo, syncService, audioHelper, sessionValidator);
            await listVm.LoadOrdersAsync();

            var listDraftOrder = listVm.Orders.FirstOrDefault(o => o.ReturnOrderId == createdDraft.ReturnOrderId);
            Assert.NotNull(listDraftOrder);
            Assert.Equal("Draft", listDraftOrder.StatusSync);

            // Select draft in inspector
            listVm.SelectedOrder = listDraftOrder;
            Assert.False(listVm.IsSelectedOrderSynced);
            Assert.True(listVm.CanEditOrder);
            Assert.True(listVm.CanDeleteOrder);
            Assert.True(listVm.CanUploadOrder);

            // === STAGE 7: TRIGGER CLOUD SYNC UPLOAD (F10 ACCELERATOR WORKFLOW) ===
            Assert.True(listVm.UploadOrderCommand.CanExecute(null));
            await listVm.UploadOrderAsync();

            // Verify upload succeeded upstream
            Assert.Equal(1, syncService.UploadCount);

            // Verify order status updated locally
            var syncedOrderInRepo = repo.GetById(createdDraft.ReturnOrderId);
            Assert.Equal(StatusSync.Synced, syncedOrderInRepo.StatusSync);
            Assert.Equal("Synced", listVm.SelectedOrder.StatusSync);

            // === STAGE 8: POST-SYNC IMMUTABILITY VERIFICATION ===
            // Document is now locked
            Assert.True(listVm.IsSelectedOrderSynced);
            Assert.False(listVm.CanEditOrder);
            Assert.False(listVm.CanDeleteOrder);
            Assert.False(listVm.CanUploadOrder);
            Assert.False(listVm.EditDraftCommand.CanExecute(null));
            Assert.False(listVm.DeleteDraftCommand.CanExecute(null));
            Assert.False(listVm.UploadOrderCommand.CanExecute(null));
        }

        #endregion

        #region Hardware Scanner Wedge High-Speed Burst Integration Test

        [Fact]
        public void ScannerWedge_HighSpeedBurst_DispatchesDirectlyToViewModel()
        {
            // Verify that hardware scanner bursts routed via WpfScannerWedgeListener
            // arrive at the active ReturnOrderEntryViewModel seamlessly
            var barcodeService = new InMemoryBarcodeService();
            barcodeService.RegisterBarcode("8992741982110", "BRG-01", "KECAP BANGO 550ML", "DUS");
            var audioHelper = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: barcodeService,
                referenceRepo: null,
                audioHelper: audioHelper,
                registryHelper: null);

            var listener = new WpfScannerWedgeListener();
            listener.ActiveConsumer = vm;

            // Simulate hardware scanner sending high-speed character burst
            listener.SimulateScan("8992741982110");

            // Verify product name auto-populated in ViewModel
            Assert.Equal("KECAP BANGO 550ML", vm.ProductName);
            Assert.Equal(1, audioHelper.SuccessBeepCount);
        }

        #endregion

        #region High-Load Intake: Sustained Scan -> Commit Cycles

        [Fact]
        public void HighLoadIntake_SustainedScanCommitCycles_KeepsGridCountersAndResetsConsistent()
        {
            // Arrange - one warehouse SKU scanned/committed repeatedly during a busy shift
            const int intakeCycles = 200;
            const string barcode = "8992741982110";

            var barcodeService = new InMemoryBarcodeService();
            barcodeService.RegisterBarcode(barcode, "BRG-01", "KECAP BANGO 550ML", "DUS");
            var audioHelper = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: barcodeService,
                referenceRepo: null,
                audioHelper: audioHelper,
                registryHelper: null);
            vm.SuppressMessageBoxes = true;

            // Act - repeat the full ergonomic intake loop under sustained load
            for (int i = 0; i < intakeCycles; i++)
            {
                vm.OnBarcodeScanned(barcode);

                Assert.Equal("KECAP BANGO 550ML", vm.ProductName);
                Assert.Equal(0, vm.QtyBagus);

                vm.QtyBagus = 1;
                Assert.True(vm.AddItemCommand.CanExecute(null));
                vm.AddItemCommand.Execute(null);

                // Every cycle must reset the quantity strip for the next scan
                Assert.Equal(0, vm.QtyBagus);
                Assert.Equal(0, vm.QtyRusak);
            }

            // Assert - no lost, duplicated, or stale intake state after high-load processing
            Assert.Equal(intakeCycles, vm.Items.Count);
            Assert.Equal(intakeCycles, vm.TotalItemsCount);
            Assert.Equal(intakeCycles, vm.TotalQtyBagus);
            Assert.Equal(0, vm.TotalQtyRusak);
            Assert.Equal(intakeCycles, vm.TotalKuantitas);
            Assert.Equal($"Total Item: {intakeCycles} baris", vm.TotalItemsSummaryText);
            Assert.Equal($"Total Qty Bagus: {intakeCycles:N0}", vm.TotalQtyBagusText);
            // Success cue fires twice per cycle: successful lookup + successful line commit
            Assert.Equal(intakeCycles * 2, audioHelper.SuccessBeepCount);
            Assert.Equal(Enumerable.Range(1, intakeCycles), vm.Items.Select(item => item.NoUrut));
        }

        #endregion
    }
}
