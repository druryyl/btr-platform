using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    /// <summary>
    /// Automated unit and STA UI integration tests for SCR-03 (ManualItemSearchDialog)
    /// and SCR-04 (QuickBarcodeRegistrationDialog).
    /// Traceable to: ARCHITECTURE §4 TD-006, TD-009, TD-011, TD-013, §5.2 SCR-03, SCR-04, P3-S07.
    /// </summary>
    public class ModalDialogTests
    {
        #region Test Fakes

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Barang> BarangCatalog { get; } = new List<Barang>();
            public List<Customer> Customers { get; } = new List<Customer>();
            public List<Driver> Drivers { get; } = new List<Driver>();
            public List<SalesPerson> SalesPersons { get; } = new List<SalesPerson>();

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;

            public IEnumerable<Barang> GetAllBarang() => BarangCatalog;
            public Task<IEnumerable<Barang>> GetAllBarangAsync() => Task.FromResult<IEnumerable<Barang>>(BarangCatalog);
            public Barang GetBarangById(string brgId) => BarangCatalog.FirstOrDefault(b => b.BrgId == brgId);
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult(GetBarangById(brgId));

            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return BarangCatalog;
                return BarangCatalog.Where(b =>
                    (b.BrgId != null && b.BrgId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (b.BrgName != null && b.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) =>
                Task.FromResult(SearchBarangByNameOrCode(keyword));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> GetAllCustomers() => Customers;
            public Task<IEnumerable<Customer>> GetAllCustomersAsync() => Task.FromResult<IEnumerable<Customer>>(Customers);
            public Customer GetCustomerById(string customerId) => Customers.FirstOrDefault(c => c.CustomerId == customerId);
            public Task<Customer> GetCustomerByIdAsync(string customerId) => Task.FromResult(GetCustomerById(customerId));
            public IEnumerable<Customer> SearchCustomer(string keyword) => Customers;
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult<IEnumerable<Customer>>(Customers);
            public IEnumerable<Customer> SearchCustomers(string keyword) => Customers;
            public Task<IEnumerable<Customer>> SearchCustomersAsync(string keyword) => Task.FromResult<IEnumerable<Customer>>(Customers);

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
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "SRV-PST";
            public string ApiBaseUrl { get; set; } = "http://localhost:5000/";
            public string LastOperatorName { get; set; } = "Budi Hartono";
            public string LastOperatorEmail { get; set; } = "budi@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

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

            public Task<BarcodeLookupResult> LookupBarcodeAsync(string barcode) => Task.FromResult(LookupBarcode(barcode));

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
            public void DeleteBarcode(string rawBarcode) => _registry.Remove(BarcodeNormalization.Normalize(rawBarcode));
            public Task DeleteBarcodeAsync(string rawBarcode)
            {
                DeleteBarcode(rawBarcode);
                return Task.CompletedTask;
            }
        }

        #endregion

        #region STA Test Runner

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                    {
                        try
                        {
                            new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.Resources.MergedDictionaries.Count == 0)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Colors.xaml", UriKind.Absolute)
                        });
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Styles.xaml", UriKind.Absolute)
                        });
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

        #endregion

        #region SCR-03: ManualItemSearchViewModel & ManualItemSearchDialog Tests

        [Fact]
        public void ManualItemSearchViewModel_InitializesWithDefaultState()
        {
            var vm = new ManualItemSearchViewModel();

            Assert.NotNull(vm.FilteredItems);
            Assert.Empty(vm.FilteredItems);
            Assert.Null(vm.SelectedBarang);
            Assert.NotNull(vm.SearchCommand);
            Assert.NotNull(vm.SelectCommand);
            Assert.NotNull(vm.CancelCommand);
            Assert.Contains("0 barang", vm.StatusText);
        }

        [Fact]
        public async Task ManualItemSearchViewModel_PerformSearchAsync_QueriesRepoAndSelectsFirstItem()
        {
            var fakeRepo = new FakeReferenceRepo();
            fakeRepo.BarangCatalog.Add(new Barang("BRG-01", "Indomie Goreng Spesial", "Dus", "Pak", "Pcs"));
            fakeRepo.BarangCatalog.Add(new Barang("BRG-02", "Sari Roti Tawar Gandum", "Pak", "Pcs", null));
            var audio = new FakeAudioHelper();

            var vm = new ManualItemSearchViewModel(fakeRepo, audio);

            await vm.PerformSearchAsync("Indomie");

            Assert.Single(vm.FilteredItems);
            Assert.NotNull(vm.SelectedBarang);
            Assert.Equal("BRG-01", vm.SelectedBarang.BrgId);
            Assert.Equal("Indomie Goreng Spesial", vm.SelectedBarang.BrgName);
            Assert.Equal("Dus", vm.SelectedSatuan);
            Assert.Contains("1 barang", vm.StatusText);
        }

        [Fact]
        public void ManualItemSearchViewModel_ConfirmSelection_PlaysSuccessAndInvokesClose()
        {
            var audio = new FakeAudioHelper();
            var vm = new ManualItemSearchViewModel(null, audio);

            bool closeOkTriggered = false;
            vm.RequestCloseOk += () => closeOkTriggered = true;

            vm.SetItems(new[] { new Barang("B-99", "Teh Botol Sosro", "Dus", "Btl", "Pcs") });

            Assert.NotNull(vm.SelectedBarang);
            var success = vm.ConfirmSelection();

            Assert.True(success);
            Assert.True(closeOkTriggered);
            Assert.True(audio.SuccessCount > 0);
        }

        [Fact]
        public void ManualItemSearchDialog_WindowGeometry_CompliesWith720pViewport()
        {
            ExecuteInStaThread(() =>
            {
                var dlg = new ManualItemSearchDialog(new FakeReferenceRepo(), new FakeAudioHelper());

                Assert.Equal(740, dlg.Width);
                Assert.Equal(480, dlg.Height);
                Assert.True(dlg.Height <= 480, "SCR-03 dialog height must be <= 480px to fit 720p screens with margin");
                Assert.Equal(WindowStartupLocation.CenterOwner, dlg.WindowStartupLocation);
                Assert.Equal(ResizeMode.NoResize, dlg.ResizeMode);
            });
        }

        [Fact]
        public void ManualItemSearchDialog_SetItems_PopulatesGridAndSelectsItem()
        {
            ExecuteInStaThread(() =>
            {
                var dlg = new ManualItemSearchDialog(new FakeReferenceRepo(), new FakeAudioHelper());
                var items = new List<Barang>
                {
                    new Barang("BRG-01", "Kopi Kapal Api Special", "Dus", "Renteng", "Sachet"),
                    new Barang("BRG-02", "Susu Dancow Coklat", "Dus", "Box", null)
                };

                dlg.SetItems(items);

                Assert.Equal(2, dlg.FilteredItems.Count);
                Assert.NotNull(dlg.SelectedBarang);
                Assert.Equal("BRG-01", dlg.SelectedBarang.BrgId);
                Assert.Equal("Dus", dlg.SelectedSatuan);
                Assert.Equal("Dus", dlg.SatuanComboBox.SelectedItem?.ToString());
            });
        }

        [Fact]
        public void ManualItemSearchDialog_UnitDropdownChange_UpdatesSelectedSatuan()
        {
            ExecuteInStaThread(() =>
            {
                var dlg = new ManualItemSearchDialog(new FakeReferenceRepo(), new FakeAudioHelper());
                dlg.SetItems(new[] { new Barang("BRG-01", "Susu Ultra Milk", "Dus", "Pak", "Pcs") });

                dlg.SatuanComboBox.SelectedItem = "Pcs";

                Assert.Equal("Pcs", dlg.SelectedSatuan);
            });
        }

        #endregion

        #region SCR-04: QuickBarcodeRegistrationViewModel & Dialog Tests

        [Fact]
        public void QuickBarcodeRegistrationViewModel_InitializesWithProminentBarcode()
        {
            var vm = new QuickBarcodeRegistrationViewModel("8991234567890");

            Assert.Equal("8991234567890", vm.BarcodeValue);
            Assert.Null(vm.SelectedBarang);
            Assert.Null(vm.RegisteredResult);
            Assert.NotNull(vm.RegisterCommand);
            Assert.NotNull(vm.CancelCommand);
            Assert.Equal(1m, vm.Pengali);
        }

        [Fact]
        public void QuickBarcodeRegistrationViewModel_PlayInitialWarning_Emits400HzBuzz()
        {
            var audio = new FakeAudioHelper();
            var vm = new QuickBarcodeRegistrationViewModel("899999", audioHelper: audio);

            vm.PlayInitialWarning();

            Assert.True(audio.WarningCount > 0, "Initial prompt must play warning tone for unknown barcode");
        }

        [Fact]
        public async Task QuickBarcodeRegistrationViewModel_OnRegisterClickAsync_SavesAndSetsResult()
        {
            var fakeBarcodeService = new FakeBarcodeService();
            var audio = new FakeAudioHelper();
            var reg = new FakeRegistryHelper { WarehouseCode = "GDG-SBY", LastOperatorName = "Andi" };

            var vm = new QuickBarcodeRegistrationViewModel(
                barcode: "899777111222",
                barcodeService: fakeBarcodeService,
                barcodeRepo: null,
                referenceRepo: null,
                registryHelper: reg,
                audioHelper: audio);

            vm.SetItems(new[] { new Barang("BRG-ABC", "Kecap Manis ABC", "Dus", "Btl", "Pcs") });
            vm.SelectedSatuan = "Dus";
            vm.Pengali = 12m;

            bool closeOkTriggered = false;
            vm.RequestCloseOk += () => closeOkTriggered = true;

            var success = await vm.OnRegisterClickAsync();

            Assert.True(success);
            Assert.True(closeOkTriggered);
            Assert.NotNull(vm.RegisteredResult);
            Assert.True(vm.RegisteredResult.Found);
            Assert.Equal("899777111222", vm.RegisteredResult.NormalizedBarcode);
            Assert.Equal("BRG-ABC", vm.RegisteredResult.BrgId);
            Assert.Equal("Kecap Manis ABC", vm.RegisteredResult.BrgName);
            Assert.Equal("Dus", vm.RegisteredResult.Satuan);
            Assert.Equal(12m, vm.RegisteredResult.Pengali);
            Assert.True(audio.SuccessCount > 0);
        }

        [Fact]
        public void QuickBarcodeRegistrationDialog_WindowGeometry_CompliesWith720pViewport()
        {
            ExecuteInStaThread(() =>
            {
                var dlg = new QuickBarcodeRegistrationDialog("8991234567890");

                Assert.Equal(580, dlg.Width);
                Assert.Equal(480, dlg.Height);
                Assert.True(dlg.Height <= 480, "SCR-04 dialog height must be <= 480px to fit 720p screens with margin");
                Assert.Equal(WindowStartupLocation.CenterOwner, dlg.WindowStartupLocation);
                Assert.Equal(ResizeMode.NoResize, dlg.ResizeMode);
            });
        }

        [Fact]
        public void QuickBarcodeRegistrationDialog_BarcodeTextBox_IsReadOnlyWithProminentStyling()
        {
            ExecuteInStaThread(() =>
            {
                var dlg = new QuickBarcodeRegistrationDialog("899555666777");

                Assert.Equal("899555666777", dlg.BarcodeTextBox.Text);
                Assert.True(dlg.BarcodeTextBox.IsReadOnly);
            });
        }

        #endregion

        #region Intake Strip Integration Tests (SCR-02 with SCR-03 and SCR-04)

        [Fact]
        public void ReturnOrderEntryViewModel_TriggerManualItemSearch_SetsActiveProductBanner()
        {
            var fakeBarcodeService = new FakeBarcodeService();
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: fakeBarcodeService,
                referenceRepo: null,
                audioHelper: audio,
                registryHelper: null,
                sessionValidator: null);

            // Configure manual search launcher returning Indomie
            vm.ManualSearchDialogLauncher = () => new Barang("BRG-MANUAL", "Indomie Kari Ayam", "Dus", "Pak", "Pcs");

            vm.TriggerManualItemSearch();

            Assert.NotNull(vm.CurrentScanResult);
            Assert.True(vm.CurrentScanResult.Found);
            Assert.Equal("BRG-MANUAL", vm.CurrentScanResult.BrgId);
            Assert.Equal("Indomie Kari Ayam", vm.ProductName);
            Assert.True(vm.IsBannerActive);
            Assert.False(vm.IsBannerError);
            Assert.True(audio.SuccessCount > 0);
        }

        [Fact]
        public void ReturnOrderEntryViewModel_ScanUnknownBarcode_AutoTriggersQuickRegistrationAndResumesIntake()
        {
            var fakeBarcodeService = new FakeBarcodeService();
            var audio = new FakeAudioHelper();

            var vm = new ReturnOrderEntryViewModel(
                returnOrderService: null,
                barcodeService: fakeBarcodeService,
                referenceRepo: null,
                audioHelper: audio,
                registryHelper: null,
                sessionValidator: null);

            // Configure launcher simulating user completing quick registration
            vm.QuickRegistrationDialogLauncher = (unmappedBarcode) =>
            {
                return BarcodeLookupResult.Hit(
                    rawInput: unmappedBarcode,
                    normalizedBarcode: unmappedBarcode,
                    brgId: "BRG-NEW",
                    brgName: "Biskuit Oreo Vanilla",
                    satuan: "Dus",
                    pengali: 24m,
                    isActive: true);
            };

            // Operator scans unknown barcode
            vm.OnBarcodeScanned("899333444555");

            // Verify intake was automatically resumed with newly mapped product in banner
            Assert.NotNull(vm.CurrentScanResult);
            Assert.True(vm.CurrentScanResult.Found);
            Assert.Equal("BRG-NEW", vm.CurrentScanResult.BrgId);
            Assert.Equal("Biskuit Oreo Vanilla", vm.ProductName);
            Assert.True(vm.IsBannerActive);
            Assert.False(vm.IsBannerError);
            Assert.True(audio.SuccessCount > 0);
        }

        [Fact]
        public void ReturnOrderEntryView_PreviewKeyDown_F2_TriggersManualItemSearch()
        {
            ExecuteInStaThread(() =>
            {
                var audio = new FakeAudioHelper();
                var vm = new ReturnOrderEntryViewModel(
                    returnOrderService: null,
                    barcodeService: null,
                    referenceRepo: null,
                    audioHelper: audio,
                    registryHelper: null,
                    sessionValidator: null);

                bool searchTriggered = false;
                vm.ManualSearchDialogLauncher = () =>
                {
                    searchTriggered = true;
                    return new Barang("BRG-F2", "Wafer Tango Cokelat", "Dus", null, null);
                };

                var view = new ReturnOrderEntryView { DataContext = vm };

                // Simulate F2 key down
                var eventArgs = new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero),
                    0,
                    Key.F2)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };

                view.RaiseEvent(eventArgs);

                Assert.True(searchTriggered, "Pressing F2 must invoke ManualSearchDialogLauncher");
                Assert.Equal("Wafer Tango Cokelat", vm.ProductName);
            });
        }

        #endregion
    }
}
