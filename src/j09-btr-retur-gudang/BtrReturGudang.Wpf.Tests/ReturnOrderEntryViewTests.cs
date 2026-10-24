using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Wpf.Controls;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class ReturnOrderEntryViewTests
    {
        #region Test Fakes

        private class FakeBarcodeService : IBarcodeService
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
                decimal pengali = 1.0m)
            {
                RegisterBarcode(rawBarcode, brgId, $"Item-{brgId}", satuan, pengali);
                return new BarcodeRegistrationDto
                {
                    RequestId = "REQ-1",
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
                decimal pengali = 1.0m)
            {
                return Task.FromResult(QuickRegisterUnmappedBarcode(rawBarcode, brgId, satuan, warehouseCode, operatorName, pengali));
            }

            public IEnumerable<BarcodeDto> SearchBarcodes(string keyword) => Enumerable.Empty<BarcodeDto>();
            public Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword) => Task.FromResult(Enumerable.Empty<BarcodeDto>());
            public IEnumerable<BarcodeRegistrationDto> GetPendingRegistrations() => Enumerable.Empty<BarcodeRegistrationDto>();
            public Task<IEnumerable<BarcodeRegistrationDto>> GetPendingRegistrationsAsync() => Task.FromResult(Enumerable.Empty<BarcodeRegistrationDto>());

            public void DeleteBarcode(string rawBarcode)
            {
                var clean = BarcodeNormalization.Normalize(rawBarcode);
                _registry.Remove(clean);
            }

            public Task DeleteBarcodeAsync(string rawBarcode)
            {
                DeleteBarcode(rawBarcode);
                return Task.CompletedTask;
            }
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Driver> Drivers { get; } = new List<Driver>
            {
                new Driver("DRV-1", "Agus Supriyanto"),
                new Driver("DRV-2", "Bambang Pamungkas")
            };

            public List<SalesPerson> SalesPersons { get; } = new List<SalesPerson>
            {
                new SalesPerson("SLS-1", "Hendra Gunawan"),
                new SalesPerson("SLS-2", "Siti Aminah")
            };

            public List<Customer> Customers { get; } = new List<Customer>
            {
                new Customer("CUST-001", "TOKO REJEKI", "Jl. Merdeka 10", "C001", "SLAWI"),
                new Customer("CUST-002", "TOKO MAKMUR", "Jl. Sudirman 45", "C002", "TEGAL")
            };

            public Dictionary<string, Barang> Barangs { get; } = new Dictionary<string, Barang>
            {
                ["BRG-001"] = new Barang("BRG-001", "INDOMIE GORENG SPESIAL", "DUS", "PAK", "PCS", true),
                ["BRG-002941-KCP"] = new Barang("BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML", "DUS", "BTL", "PCS", true)
            };

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => Barangs.Values.Where(b => b.BrgName.Contains(keyword));
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(SearchBarangByNameOrCode(keyword));
            public Barang GetBarangById(string brgId) => Barangs.TryGetValue(brgId, out var b) ? b : null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> SearchCustomer(string keyword) => Customers.Where(c => c.CustomerName.Contains(keyword));
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
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "SRV-TEST";
            public string ApiBaseUrl { get; set; } = "http://localhost:5000";
            public string LastOperatorName { get; set; } = "Budi Hartono";
            public string LastOperatorEmail { get; set; } = "budi@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class FakeReturnOrderService : IReturnOrderService
        {
            public ReturnOrderDto CreatedOrder { get; private set; }
            public bool CommitUploadSuccess { get; set; } = true;

            public ReturnOrderDto Create(string custId, string custName, string warehouseCode, string operatorName,
                string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null,
                string keterangan = null, string customerCode = null, string wilayah = null,
                IEnumerable<ReturnOrderItemDto> items = null)
            {
                CreatedOrder = new ReturnOrderDto
                {
                    ReturnOrderId = "RO-20261022-001",
                    CustId = custId,
                    CustName = custName,
                    WarehouseCode = warehouseCode,
                    OperatorName = operatorName,
                    DriverId = driverId,
                    DriverName = driverName,
                    SalesPersonId = salesPersonId,
                    SalesPersonName = salesPersonName,
                    Keterangan = keterangan,
                    CustomerCode = customerCode,
                    Wilayah = wilayah,
                    Items = items?.ToList() ?? new List<ReturnOrderItemDto>()
                };
                return CreatedOrder;
            }

            public Task<ReturnOrderDto> CreateAsync(string custId, string custName, string warehouseCode, string operatorName,
                string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null,
                string keterangan = null, string customerCode = null, string wilayah = null,
                IEnumerable<ReturnOrderItemDto> items = null)
            {
                return Task.FromResult(Create(custId, custName, warehouseCode, operatorName, driverId, driverName, salesPersonId, salesPersonName, keterangan, customerCode, wilayah, items));
            }

            public ReturnOrderDto Update(string returnOrderId, string custId, string custName,
                string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null,
                string keterangan = null, string customerCode = null, string wilayah = null,
                IEnumerable<ReturnOrderItemDto> items = null)
            {
                CreatedOrder = new ReturnOrderDto
                {
                    ReturnOrderId = returnOrderId,
                    CustId = custId,
                    CustName = custName,
                    DriverId = driverId,
                    DriverName = driverName,
                    SalesPersonId = salesPersonId,
                    SalesPersonName = salesPersonName,
                    Keterangan = keterangan,
                    CustomerCode = customerCode,
                    Wilayah = wilayah,
                    Items = items?.ToList() ?? new List<ReturnOrderItemDto>()
                };
                return CreatedOrder;
            }

            public Task<ReturnOrderDto> UpdateAsync(string returnOrderId, string custId, string custName,
                string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null,
                string keterangan = null, string customerCode = null, string wilayah = null,
                IEnumerable<ReturnOrderItemDto> items = null)
            {
                return Task.FromResult(Update(returnOrderId, custId, custName, driverId, driverName, salesPersonId, salesPersonName, keterangan, customerCode, wilayah, items));
            }

            public ReturnOrderDto AddItem(string returnOrderId, string brgId, string brgName, string barcode, JenisRetur jenisRetur, decimal qty, string satId, string keterangan = null) => null;
            public ReturnOrderDto UpdateLine(string returnOrderId, int noUrut, decimal newQty, JenisRetur newJenisRetur, string newKeterangan = null) => null;
            public ReturnOrderDto UpdateLine(string returnOrderId, int noUrut, decimal newQty, string newJenisRetur, string newKeterangan = null) => null;
            public ReturnOrderDto RemoveLine(string returnOrderId, int noUrut) => null;
            public ReturnOrderDto CommitDraft(string returnOrderId) => null;
            public Task<ReturnOrderDto> CommitDraftAsync(string returnOrderId) => Task.FromResult<ReturnOrderDto>(null);

            public Task<SyncResultDto> CommitAndUploadAsync(string returnOrderId)
            {
                return Task.FromResult(new SyncResultDto
                {
                    Success = CommitUploadSuccess,
                    SyncType = "ReturnOrder",
                    ErrorMessage = CommitUploadSuccess ? null : "Simulated upload failure"
                });
            }

            public ReturnOrderDto GetById(string returnOrderId) => CreatedOrder;
            public Task<ReturnOrderDto> GetByIdAsync(string returnOrderId) => Task.FromResult(CreatedOrder);
            public IEnumerable<ReturnOrderDto> GetOrders(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Enumerable.Empty<ReturnOrderDto>();
            public Task<IEnumerable<ReturnOrderDto>> GetOrdersAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Task.FromResult(Enumerable.Empty<ReturnOrderDto>());
            public void DeleteDraft(string returnOrderId) { }
            public Task DeleteDraftAsync(string returnOrderId) => Task.CompletedTask;
        }

        #endregion

        private static readonly object _staAppLock = new object();

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    lock (_staAppLock)
                    {
                        if (System.Windows.Application.Current == null)
                        {
                            try
                            {
                                new System.Windows.Application();
                            }
                            catch (InvalidOperationException)
                            {
                                // Handled if concurrent thread initialized it
                            }
                        }
                        if (System.Windows.Application.Current != null && System.Windows.Application.Current.Resources.MergedDictionaries.Count == 0)
                        {
                            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Colors.xaml", UriKind.Absolute)
                            });
                            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Styles.xaml", UriKind.Absolute)
                            });
                            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/DataGrid.xaml", UriKind.Absolute)
                            });
                        }
                    }
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
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        [Fact]
        public void ProductBanner_ValidScan_DisplaysPureProductNameWithoutCodeNoise()
        {
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8992741982110", "BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML", "DUS", 24m);
            var audio = new FakeAudioHelper();
            var refRepo = new FakeReferenceRepo();

            var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, refRepo, audio, null);
            vm.OnBarcodeScanned("8992741982110");

            // Completion Criteria 1: Product Name displays pure BrgName in bold white text without internal code noise
            Assert.Equal("KECAP BANGO MANIS REFILL 550ML", vm.ProductName);
            Assert.DoesNotContain("BRG-002941-KCP", vm.ProductName);
            Assert.Contains("Barcode: 8992741982110", vm.BarcodeDetails);
            Assert.Contains("Satuan Default: DUS", vm.BarcodeDetails);
            Assert.Contains("Pengali: x24", vm.BarcodeDetails);
            Assert.Equal("BARCODE TERDETEKSI", vm.BannerStatus);
            Assert.True(vm.IsBannerActive);
            Assert.False(vm.IsBannerError);
            Assert.Equal(1, audio.SuccessBeepCount);
        }

        [Fact]
        public void BarcodeScanning_ReScanningSameBarcode_AutoIncrementsQtyBagus()
        {
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, null, audio, null);

            // First scan focuses Qty Bagus with text pre-selected; initial Qty = 1 (hands-free ergonomics)
            vm.OnBarcodeScanned("8998866200112");
            Assert.Equal(1, vm.QtyBagus);
            Assert.Equal(1, audio.SuccessBeepCount);

            // Second consecutive scan of identical barcode auto-increments to 2
            vm.OnBarcodeScanned("8998866200112");
            Assert.Equal(2, vm.QtyBagus);
            Assert.Equal("QTY BAGUS: 2", vm.BannerStatus);
            Assert.Equal(2, audio.SuccessBeepCount);

            // Third consecutive scan increments to 3
            vm.OnBarcodeScanned("8998866200112");
            Assert.Equal(3, vm.QtyBagus);
            Assert.Equal("QTY BAGUS: 3", vm.BannerStatus);
            Assert.Equal(3, audio.SuccessBeepCount);
        }

        [Fact]
        public void BarcodeScanning_UnknownBarcode_TriggersWarningAudioAndErrorBanner()
        {
            var barcodeSvc = new FakeBarcodeService();
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, null, audio, null);
            vm.OnBarcodeScanned("UNKNOWN_BARCODE_999");

            // Completion Criteria 5: Audio cue triggered on warning (400Hz)
            Assert.Equal(1, audio.WarningBeepCount);
            Assert.Null(vm.CurrentScanResult);
            Assert.Contains("⚠️ Barcode tidak dikenal: [UNKNOWN_BARCODE_999]", vm.ProductName);
            Assert.Equal("TIDAK DITEMUKAN", vm.BannerStatus);
            Assert.True(vm.IsBannerError);
            Assert.False(vm.IsBannerActive);
        }

        [Fact]
        public void QuantityStrip_Commit_AddsItemsAndUpdatesSummaryCountersInRealTime()
        {
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, null, audio, null);
            vm.SuppressMessageBoxes = true;

            // 1. Scan product
            vm.OnBarcodeScanned("8998866200112");

            // 2. Set Qty Bagus = 10, Qty Rusak = 3, Catatan = "Kardus penyok"
            vm.QtyBagus = 10;
            vm.QtyRusak = 3;
            vm.ItemKeterangan = "Kardus penyok";

            // 3. Commit item to grid (+ Tambah [Enter])
            // Completion Criteria 3 & 4
            bool committed = vm.CommitQuantityStripToGrid();
            Assert.True(committed);

            // Real-time verification of committed lines
            Assert.Equal(2, vm.Items.Count);

            var itemBagus = vm.Items[0];
            Assert.Equal(1, itemBagus.NoUrut);
            Assert.Equal("BAGUS", itemBagus.JenisRetur);
            Assert.Equal(10, itemBagus.Qty);
            Assert.Equal("INDOMIE GORENG SPESIAL", itemBagus.BrgName);
            Assert.Equal("Kardus penyok", itemBagus.Keterangan);

            var itemRusak = vm.Items[1];
            Assert.Equal(2, itemRusak.NoUrut);
            Assert.Equal("RUSAK", itemRusak.JenisRetur);
            Assert.Equal(3, itemRusak.Qty);
            Assert.Equal("INDOMIE GORENG SPESIAL", itemRusak.BrgName);
            Assert.Equal("Kardus penyok", itemRusak.Keterangan);

            // Summary counters updated in real time
            Assert.Equal(2, vm.TotalItemsCount);
            Assert.Equal(10m, vm.TotalQtyBagus);
            Assert.Equal(3m, vm.TotalQtyRusak);
            Assert.Equal(13m, vm.TotalKuantitas);
            Assert.Equal("Total Item: 2 baris", vm.TotalItemsSummaryText);
            Assert.Equal("Total Qty Bagus: 10", vm.TotalQtyBagusText);
            Assert.Equal("Total Qty Rusak: 3", vm.TotalQtyRusakText);
            Assert.Equal("Total Kuantitas: 13", vm.TotalKuantitasText);

            // Strip reset after commit
            Assert.Equal(0, vm.QtyBagus);
            Assert.Equal(0, vm.QtyRusak);
            Assert.Equal(string.Empty, vm.ItemKeterangan);
            Assert.Null(vm.CurrentScanResult);
            Assert.Equal("ITEM DITAMBAHKAN", vm.BannerStatus);
        }

        [Fact]
        public void DeleteItem_ReSequencesNoUrutAndUpdatesSummaryCounters()
        {
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);

            var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, null, new FakeAudioHelper(), null);
            vm.SuppressMessageBoxes = true;

            vm.OnBarcodeScanned("8998866200112");
            vm.QtyBagus = 5;
            vm.QtyRusak = 2;
            vm.CommitQuantityStripToGrid();

            Assert.Equal(2, vm.Items.Count);

            // Delete first item (Bagus)
            vm.DeleteItem(vm.Items[0]);

            Assert.Single(vm.Items);
            Assert.Equal(1, vm.Items[0].NoUrut);
            Assert.Equal("RUSAK", vm.Items[0].JenisRetur);
            Assert.Equal(1, vm.TotalItemsCount);
            Assert.Equal(0m, vm.TotalQtyBagus);
            Assert.Equal(2m, vm.TotalQtyRusak);
            Assert.Equal(2m, vm.TotalKuantitas);
        }

        [Fact]
        public async Task SaveDraftAsync_ValidInputs_PersistsDraftSuccessfully()
        {
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);
            var orderService = new FakeReturnOrderService();
            var registry = new FakeRegistryHelper();
            var refRepo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(orderService, barcodeSvc, refRepo, audio, registry);
            vm.SuppressMessageBoxes = true;

            // Header selection
            vm.SetSelectedCustomer(refRepo.Customers[0]);
            vm.SelectedDriver = vm.Drivers.First(d => d.Id == "DRV-1");
            vm.SelectedSalesPerson = vm.SalesPersons.First(s => s.Id == "SLS-1");
            vm.Keterangan = "Retur kelebihan kirim";

            // Add item
            vm.OnBarcodeScanned("8998866200112");
            vm.QtyBagus = 4;
            vm.CommitQuantityStripToGrid();

            // Save Draft
            var order = await vm.SaveDraftAsync();

            Assert.NotNull(order);
            Assert.Equal("RO-20261022-001", order.ReturnOrderId);
            Assert.Equal("CUST-001", order.CustId);
            Assert.Equal("TOKO REJEKI", order.CustName);
            Assert.Equal("DRV-1", order.DriverId);
            Assert.Equal("SLS-1", order.SalesPersonId);
            Assert.Equal("Retur kelebihan kirim", order.Keterangan);
            Assert.Single(order.Items);
            Assert.Equal(4, order.Items.First().Qty);
        }

        [Fact]
        public void OrderActionHandler_GlobalHotkeys_DelegatesToViewModelOperations()
        {
            var orderService = new FakeReturnOrderService();
            var barcodeSvc = new FakeBarcodeService();
            barcodeSvc.RegisterBarcode("8998866200112", "BRG-001", "INDOMIE GORENG SPESIAL", "PCS", 1m);
            var refRepo = new FakeReferenceRepo();
            var registry = new FakeRegistryHelper();

            var vm = new ReturnOrderEntryViewModel(orderService, barcodeSvc, refRepo, new FakeAudioHelper(), registry);
            vm.SuppressMessageBoxes = true;

            // Set customer and item
            vm.SetSelectedCustomer(refRepo.Customers[0]);
            vm.OnBarcodeScanned("8998866200112");
            vm.QtyBagus = 3;
            vm.CommitQuantityStripToGrid();

            // Test F9 Save Draft handler
            var handler = (IOrderActionHandler)vm;
            handler.HandleSaveDraft();
            Assert.NotNull(orderService.CreatedOrder);

            // Test Esc Reset Form handler
            handler.HandleCancelOrClear();
            Assert.Empty(vm.Items);
            Assert.Null(vm.SelectedCustomer);
            Assert.Equal(0, vm.TotalItemsCount);
        }

        [Fact]
        public void View_InstantiationInStaThread_RendersResponsiveIntakeLayout()
        {
            ExecuteInStaThread(() =>
            {
                var barcodeSvc = new FakeBarcodeService();
                barcodeSvc.RegisterBarcode("8992741982110", "BRG-002941-KCP", "KECAP BANGO MANIS REFILL 550ML JUMBO FMCG", "DUS", 24m);
                var refRepo = new FakeReferenceRepo();
                var registry = new FakeRegistryHelper();
                var audio = new FakeAudioHelper();

                var vm = new ReturnOrderEntryViewModel(null, barcodeSvc, refRepo, audio, registry);

                var view = new ReturnOrderEntryView
                {
                    DataContext = vm
                };

                var window = new Window
                {
                    Content = view,
                    Width = 1050,
                    Height = 650
                };
                window.Show();

                try
                {
                    // Verify header controls exist
                    Assert.NotNull(view.TxtCustomer);
                    Assert.NotNull(view.BtnSearchCustomer);
                    Assert.NotNull(view.CboDriver);
                    Assert.NotNull(view.CboSalesPerson);
                    Assert.NotNull(view.TxtKeterangan);

                    // Verify product banner controls exist
                    Assert.NotNull(view.PnlProductBanner);
                    Assert.NotNull(view.TxtProductName);
                    Assert.NotNull(view.TxtBannerStatus);
                    Assert.NotNull(view.TxtBarcodeDetails);
                    Assert.Equal(TextWrapping.Wrap, view.TxtProductName.TextWrapping);

                    // Verify quantity strip controls exist
                    Assert.NotNull(view.CboSatuan);
                    Assert.NotNull(view.TxtQtyBagus);
                    Assert.NotNull(view.TxtQtyRusak);
                    Assert.NotNull(view.TxtItemNotes);
                    Assert.NotNull(view.BtnAddItem);
                    Assert.Equal("+ Tambah [Enter]", view.BtnAddItem.Content);

                    // Verify live grid & footer controls exist
                    Assert.NotNull(view.DgvItems);
                    Assert.NotNull(view.LblSummaryItems);
                    Assert.NotNull(view.LblSummaryBagus);
                    Assert.NotNull(view.LblSummaryRusak);
                    Assert.NotNull(view.LblSummaryTotal);

                    // Simulate scan through ViewModel
                    vm.OnBarcodeScanned("8992741982110");
                    Assert.Equal("KECAP BANGO MANIS REFILL 550ML JUMBO FMCG", vm.ProductName);
                    Assert.Equal("BARCODE TERDETEKSI", vm.BannerStatus);

                    view.TxtProductName.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                    view.TxtBannerStatus.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                    Assert.Equal("KECAP BANGO MANIS REFILL 550ML JUMBO FMCG", view.TxtProductName.Text);
                    Assert.Equal("BARCODE TERDETEKSI", view.TxtBannerStatus.Text);
                }
                finally
                {
                    window.Close();
                }
            });
        }
    }
}
