using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    /// <summary>
    /// Unit and UI tests for SCR-03 (ManualItemSearchDialog) and SCR-04 (QuickBarcodeRegistrationDialog),
    /// plus their integration with ReturnOrderEntryView.
    /// Traceable to: ARCHITECTURE §5.2 SCR-03, SCR-04, TD-009, TD-011, P3-S09.
    /// </summary>
    public class ModalDialogTests
    {
        private static void ExecuteInStaThread(Action action)
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
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        #region SCR-03: ManualItemSearchDialog Tests

        [Fact]
        public void ManualItemSearchDialog_InitializesWithDefaultState()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualItemSearchDialog())
                {
                    Assert.Contains("Manual", dlg.Text);
                    Assert.NotNull(dlg.SearchBox);
                    Assert.NotNull(dlg.GridView);
                    Assert.NotNull(dlg.SatuanComboBox);
                    Assert.NotNull(dlg.SelectButton);
                    Assert.NotNull(dlg.CancelButtonControl);
                    Assert.Null(dlg.SelectedBarang);
                }
            });
        }

        [Fact]
        public void ManualItemSearchDialog_SetItems_PopulatesGridAndSelectsFirstItem()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualItemSearchDialog())
                {
                    var items = new List<Barang>
                    {
                        new Barang("BRG01", "Indomie Goreng", "Dus", "Pak", "Pcs"),
                        new Barang("BRG02", "Sari Roti Tawar", "Pak", null, null)
                    };

                    dlg.SetItems(items);

                    Assert.Equal(2, dlg.GridView.Rows.Count);
                    Assert.NotNull(dlg.SelectedBarang);
                    Assert.Equal("BRG01", dlg.SelectedBarang.BrgId);
                    Assert.Equal("Dus", dlg.SelectedSatuan);
                }
            });
        }

        [Fact]
        public async Task ManualItemSearchDialog_PerformSearchAsync_QueriesReferenceRepo()
        {
            var fakeRepo = new FakeReferenceRepo();
            fakeRepo.BarangCatalog.Add(new Barang("B01", "Kecap Bango Manis", "Dus", "Btl", "Pcs"));
            fakeRepo.BarangCatalog.Add(new Barang("B02", "Minyak Goreng Bimoli", "Dus", "Pcs", null));

            ManualItemSearchDialog dialog = null;
            ExecuteInStaThread(() =>
            {
                dialog = new ManualItemSearchDialog(fakeRepo, new FakeAudioHelper());
            });

            using (dialog)
            {
                await dialog.PerformSearchAsync("Kecap");

                Assert.Single(dialog.FilteredItems);
                Assert.Equal("B01", dialog.FilteredItems[0].BrgId);
                Assert.Equal("Kecap Bango Manis", dialog.SelectedBarang.BrgName);
            }
        }

        [Fact]
        public void ManualItemSearchDialog_ConfirmSelection_SetsDialogResultOKAndPlaysSuccess()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var dlg = new ManualItemSearchDialog(null, audioHelper))
                {
                    dlg.SetItems(new[] { new Barang("BRG99", "Teh Botol Sosro", "Dus", "Btl", null) });
                    dlg.ConfirmSelection();

                    Assert.Equal(DialogResult.OK, dlg.DialogResult);
                    Assert.NotNull(dlg.SelectedBarang);
                    Assert.Equal("BRG99", dlg.SelectedBarang.BrgId);
                    Assert.True(audioHelper.SuccessCount > 0);
                }
            });
        }

        [Fact]
        public void ManualItemSearchDialog_UnitSelection_UpdatesSelectedSatuan()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualItemSearchDialog())
                {
                    dlg.SetItems(new[] { new Barang("BRG01", "Susu Ultra Milk", "Dus", "Pak", "Pcs") });

                    dlg.SatuanComboBox.SelectedItem = "Pcs";

                    Assert.Equal("Pcs", dlg.SelectedSatuan);
                }
            });
        }

        #endregion

        #region SCR-04: QuickBarcodeRegistrationDialog Tests

        [Fact]
        public void QuickBarcodeRegistrationDialog_InitializesWithProminentReadOnlyBarcode()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new QuickBarcodeRegistrationDialog("8991234567890"))
                {
                    Assert.Contains("Registrasi", dlg.Text);
                    Assert.Equal("8991234567890", dlg.BarcodeValue);
                    Assert.True(dlg.BarcodeTextBox.ReadOnly);
                    Assert.NotNull(dlg.SearchItemTextBox);
                    Assert.NotNull(dlg.ItemsListBox);
                    Assert.NotNull(dlg.SatuanComboBox);
                    Assert.NotNull(dlg.PengaliNumeric);
                    Assert.NotNull(dlg.RegisterButton);
                    Assert.Null(dlg.SelectedBarang);
                    Assert.Null(dlg.RegisteredResult);
                }
            });
        }

        [Fact]
        public void QuickBarcodeRegistrationDialog_SetItems_PopulatesListAndSelectsItem()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new QuickBarcodeRegistrationDialog("899888"))
                {
                    var items = new List<Barang>
                    {
                        new Barang("B10", "Kopi Kapal Api", "Dus", "Renteng", "Sachet")
                    };

                    dlg.SetItems(items);

                    Assert.Single(dlg.ItemsListBox.Items);
                    Assert.NotNull(dlg.SelectedBarang);
                    Assert.Equal("B10", dlg.SelectedBarang.BrgId);
                    Assert.Equal("Dus", dlg.SelectedSatuan);
                }
            });
        }

        [Fact]
        public async Task QuickBarcodeRegistrationDialog_OnRegisterClickAsync_SavesAndSetsRegisteredResult()
        {
            var fakeBarcodeService = new FakeBarcodeService();
            var audioHelper = new FakeAudioHelper();
            var fakeRegistry = new FakeRegistryHelper { WarehouseCode = "GDG-SBY", LastOperatorName = "Budi" };

            QuickBarcodeRegistrationDialog dlg = null;
            ExecuteInStaThread(() =>
            {
                dlg = new QuickBarcodeRegistrationDialog(
                    unmappedBarcode: "8999990001",
                    barcodeService: fakeBarcodeService,
                    barcodeRepo: null,
                    referenceRepo: null,
                    registryHelper: fakeRegistry,
                    audioHelper: audioHelper);

                dlg.SetItems(new[] { new Barang("BRG-KOPI", "Kopi ABC Susu", "Dus", "Pak", "Pcs") });
                dlg.SatuanComboBox.SelectedItem = "Dus";
                dlg.PengaliNumeric.Value = 20;
            });

            using (dlg)
            {
                var success = await dlg.OnRegisterClickAsync();

                Assert.True(success);
                Assert.Equal(DialogResult.OK, dlg.DialogResult);
                Assert.NotNull(dlg.RegisteredResult);
                Assert.True(dlg.RegisteredResult.Found);
                Assert.Equal("8999990001", dlg.RegisteredResult.NormalizedBarcode);
                Assert.Equal("BRG-KOPI", dlg.RegisteredResult.BrgId);
                Assert.Equal("Kopi ABC Susu", dlg.RegisteredResult.BrgName);
                Assert.Equal("Dus", dlg.RegisteredResult.Satuan);
                Assert.Equal(20m, dlg.RegisteredResult.Pengali);
                Assert.True(audioHelper.SuccessCount > 0);
            }
        }

        #endregion

        #region ReturnOrderEntryView Wiring Integration Tests

        [Fact]
        public void ReturnOrderEntryView_SetSelectedItem_AppliesProductAndPlaysSuccess()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    var barang = new Barang("BRG55", "Indomie Soto", "Dus", "Pak", "Pcs");
                    view.SetSelectedItem(barang, "Pak");

                    Assert.NotNull(view.CurrentScanResult);
                    Assert.True(view.CurrentScanResult.Found);
                    Assert.Equal("BRG55", view.CurrentScanResult.BrgId);
                    Assert.Contains("Indomie Soto", view.BannerTitleLabel.Text);
                    Assert.Equal("Pak", view.SatuanComboBox.SelectedItem.ToString());
                    Assert.True(audioHelper.SuccessCount > 0);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_TriggerManualItemSearch_InvokesLauncherAndSetsProduct()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    view.ManualSearchDialogLauncher = () => new Barang("BRG-F2", "Biskuit Khong Guan", "Dus", null, null);

                    view.TriggerManualItemSearch();

                    Assert.NotNull(view.CurrentScanResult);
                    Assert.Equal("BRG-F2", view.CurrentScanResult.BrgId);
                    Assert.Contains("Biskuit Khong Guan", view.BannerTitleLabel.Text);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_OnBarcodeScanned_UnknownBarcode_TriggersRegistrationLauncher()
        {
            ExecuteInStaThread(() =>
            {
                var barcodeService = new FakeBarcodeService();
                var audioHelper = new FakeAudioHelper();

                using (var view = new ReturnOrderEntryView(null, barcodeService, null, audioHelper, null))
                {
                    view.QuickRegistrationDialogLauncher = bc => BarcodeLookupResult.Hit(
                        rawInput: bc,
                        normalizedBarcode: bc,
                        brgId: "BRG-AUTO-REG",
                        brgName: "Sabun Nuvo",
                        satuan: "Dus",
                        pengali: 12m,
                        isActive: true);

                    view.OnBarcodeScanned("UNKNOWN_BARCODE_123");

                    Assert.NotNull(view.CurrentScanResult);
                    Assert.Equal("BRG-AUTO-REG", view.CurrentScanResult.BrgId);
                    Assert.Contains("Sabun Nuvo", view.BannerTitleLabel.Text);
                    Assert.Equal(12m, view.CurrentScanResult.Pengali);
                }
            });
        }

        #endregion

        #region Test Fakes

        private class FakeAudioHelper : IAudioHelper
        {
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }
            public bool EnableSoundFeedback { get; set; } = true;

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
            public void Beep(int frequency, int durationMs) { }
        }

        private class FakeBarcodeService : IBarcodeService
        {
            private readonly Dictionary<string, BarcodeLookupResult> _registry = new Dictionary<string, BarcodeLookupResult>();

            public void RegisterBarcode(string barcode, string brgId, string brgName, string satuan, decimal pengali)
            {
                _registry[barcode] = BarcodeLookupResult.Hit(barcode, barcode, brgId, brgName, satuan, pengali, true);
            }

            public BarcodeLookupResult LookupBarcode(string rawBarcode)
            {
                if (_registry.TryGetValue(rawBarcode, out var res))
                    return res;
                return BarcodeLookupResult.Miss(rawBarcode, rawBarcode);
            }

            public Task<BarcodeLookupResult> LookupBarcodeAsync(string rawBarcode) =>
                Task.FromResult(LookupBarcode(rawBarcode));

            public BarcodeRegistrationDto QuickRegisterUnmappedBarcode(string rawBarcode, string brgId, string satuan, string warehouseCode, string operatorName, decimal pengali = 1) =>
                new BarcodeRegistrationDto { Barcode = rawBarcode, BrgId = brgId, Satuan = satuan };

            public Task<BarcodeRegistrationDto> QuickRegisterUnmappedBarcodeAsync(string rawBarcode, string brgId, string satuan, string warehouseCode, string operatorName, decimal pengali = 1) =>
                Task.FromResult(QuickRegisterUnmappedBarcode(rawBarcode, brgId, satuan, warehouseCode, operatorName, pengali));

            public IEnumerable<BarcodeDto> SearchBarcodes(string keyword) => Enumerable.Empty<BarcodeDto>();
            public Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword) => Task.FromResult(Enumerable.Empty<BarcodeDto>());

            public IEnumerable<BarcodeRegistrationDto> GetPendingRegistrations() => Enumerable.Empty<BarcodeRegistrationDto>();
            public Task<IEnumerable<BarcodeRegistrationDto>> GetPendingRegistrationsAsync() => Task.FromResult(Enumerable.Empty<BarcodeRegistrationDto>());
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Barang> BarangCatalog { get; } = new List<Barang>();

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;

            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return BarangCatalog;
                return BarangCatalog.Where(b => (b.BrgId != null && b.BrgId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                                (b.BrgName != null && b.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) =>
                Task.FromResult(SearchBarangByNameOrCode(keyword));

            public Barang GetBarangById(string brgId) =>
                BarangCatalog.FirstOrDefault(b => b.BrgId == brgId);

            public Task<Barang> GetBarangByIdAsync(string brgId) =>
                Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> SearchCustomer(string keyword) => Enumerable.Empty<Customer>();
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult(Enumerable.Empty<Customer>());
            public Customer GetCustomerById(string custId) => null;
            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult<Customer>(null);
            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) => Task.CompletedTask;
            public IEnumerable<Driver> GetAllDrivers() => Enumerable.Empty<Driver>();
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(Enumerable.Empty<Driver>());
            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) => Task.CompletedTask;
            public IEnumerable<SalesPerson> GetAllSalesPersons() => Enumerable.Empty<SalesPerson>();
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(Enumerable.Empty<SalesPerson>());
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GD-TEST";
            public string ServerTargetId { get; set; } = "PKL";
            public string LocalDbServer { get; set; } = "(local)";
            public string LocalDbDatabase { get; set; } = "btrgd";
            public string LocalDbUser { get; set; } = "user";
            public string LocalDbPassword { get; set; } = "pass";
            public string CloudRelayBaseUrl { get; set; } = "http://localhost:5000";
            public string LastOperatorName { get; set; } = "OPERATOR";
            public bool EnableSoundFeedback { get; set; } = true;
            public string GetLocalDbConnectionString() => "Server=test";
            public void SaveWorkstationSettings(string warehouseCode, string serverTargetId, string localDbServer, string localDbDatabase, string localDbUser, string localDbPassword, string cloudRelayBaseUrl) { }
            public void SaveOperator(string operatorName) { }
            public void SaveAudioSetting(bool enableSoundFeedback) { }
        }

        #endregion
    }
}
