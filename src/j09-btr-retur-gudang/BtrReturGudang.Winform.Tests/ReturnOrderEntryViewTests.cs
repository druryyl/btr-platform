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
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class ReturnOrderEntryViewTests
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

        [Fact]
        public void ReturnOrderEntryView_InitializesWithCorrectDefaultProperties()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    Assert.Equal(DockStyle.Fill, view.Dock);
                    Assert.IsAssignableFrom<IScannerConsumer>(view);
                    Assert.NotNull(view.ItemsGridView);
                    Assert.True(view.ItemsGridView.DoubleBuffered);
                    Assert.Equal(0, view.TotalItemsCount);
                    Assert.Equal(0m, view.TotalQtyBagus);
                    Assert.Equal(0m, view.TotalQtyRusak);
                    Assert.Null(view.CurrentScanResult);
                    Assert.Null(view.CurrentReturnOrderId);
                }
            });
        }

        [Fact]
        public void OnBarcodeScanned_RecognizedBarcode_UpdatesBannerAndFocusesQuantityStrip()
        {
            ExecuteInStaThread(() =>
            {
                var barcodeService = new FakeBarcodeService();
                barcodeService.RegisterBarcode("8991234567890", "BRG001", "Indomie Goreng Spesial", "Dus", 24m);

                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, barcodeService, null, audioHelper, null))
                {
                    view.OnBarcodeScanned("8991234567890");

                    Assert.NotNull(view.CurrentScanResult);
                    Assert.True(view.CurrentScanResult.Found);
                    Assert.Equal("BRG001", view.CurrentScanResult.BrgId);
                    Assert.Contains("Indomie Goreng", view.BannerTitleLabel.Text);
                    Assert.Contains("8991234567890", view.BannerDetailsLabel.Text);
                    Assert.Equal("Dus", view.SatuanComboBox.SelectedItem.ToString());
                    Assert.Equal(0, view.QtyBagusNumeric.Value);
                    Assert.Equal(0, view.QtyRusakNumeric.Value);
                }
            });
        }

        [Fact]
        public void OnBarcodeScanned_UnknownBarcode_PlaysWarningAndDisplaysMissBanner()
        {
            ExecuteInStaThread(() =>
            {
                var barcodeService = new FakeBarcodeService();
                var audioHelper = new FakeAudioHelper();

                using (var view = new ReturnOrderEntryView(null, barcodeService, null, audioHelper, null))
                {
                    view.OnBarcodeScanned("UNKNOWN_BARCODE");

                    Assert.Null(view.CurrentScanResult);
                    Assert.Contains("tidak dikenal", view.BannerTitleLabel.Text);
                    Assert.True(audioHelper.WarningCount > 0);
                }
            });
        }

        [Fact]
        public void CommitQuantityStrip_WithQtyBagus_AddsBagusLineAndPlaysSuccess()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Minyak Goreng", "Pcs", 1m, true);
                    view.ApplyActiveProduct(lookup);

                    view.QtyBagusNumeric.Value = 5;
                    view.QtyRusakNumeric.Value = 0;

                    var success = view.CommitQuantityStripToGrid();

                    Assert.True(success);
                    Assert.Equal(1, view.TotalItemsCount);
                    Assert.Equal(5m, view.TotalQtyBagus);
                    Assert.Equal(0m, view.TotalQtyRusak);
                    Assert.Equal("BAGUS", view.ItemsList[0].JenisRetur);
                    Assert.Equal(5m, view.ItemsList[0].Qty);
                    Assert.Equal(1, view.ItemsList[0].NoUrut);
                    Assert.True(audioHelper.SuccessCount > 0);
                    Assert.Equal(0, view.QtyBagusNumeric.Value);
                }
            });
        }

        [Fact]
        public void CommitQuantityStrip_WithQtyRusak_AddsRusakLineAndPlaysSuccess()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Minyak Goreng", "Pcs", 1m, true);
                    view.ApplyActiveProduct(lookup);

                    view.QtyBagusNumeric.Value = 0;
                    view.QtyRusakNumeric.Value = 4;

                    var success = view.CommitQuantityStripToGrid();

                    Assert.True(success);
                    Assert.Equal(1, view.TotalItemsCount);
                    Assert.Equal(0m, view.TotalQtyBagus);
                    Assert.Equal(4m, view.TotalQtyRusak);
                    Assert.Equal("RUSAK", view.ItemsList[0].JenisRetur);
                    Assert.Equal(4m, view.ItemsList[0].Qty);
                    Assert.True(audioHelper.SuccessCount > 0);
                }
            });
        }

        [Fact]
        public void CommitQuantityStrip_WithBothBagusAndRusak_AddsBothRowsSequentially()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Minyak Goreng", "Pcs", 1m, true);
                    view.ApplyActiveProduct(lookup);

                    view.QtyBagusNumeric.Value = 10;
                    view.QtyRusakNumeric.Value = 2;

                    var success = view.CommitQuantityStripToGrid();

                    Assert.True(success);
                    Assert.Equal(2, view.TotalItemsCount);
                    Assert.Equal(10m, view.TotalQtyBagus);
                    Assert.Equal(2m, view.TotalQtyRusak);
                    Assert.Equal("BAGUS", view.ItemsList[0].JenisRetur);
                    Assert.Equal("RUSAK", view.ItemsList[1].JenisRetur);
                    Assert.Equal(1, view.ItemsList[0].NoUrut);
                    Assert.Equal(2, view.ItemsList[1].NoUrut);
                }
            });
        }

        [Fact]
        public void CommitQuantityStrip_ZeroQuantity_DoesNotAddRowAndEmitsWarning()
        {
            ExecuteInStaThread(() =>
            {
                var audioHelper = new FakeAudioHelper();
                using (var view = new ReturnOrderEntryView(null, null, null, audioHelper, null))
                {
                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Minyak Goreng", "Pcs", 1m, true);
                    view.ApplyActiveProduct(lookup);

                    view.QtyBagusNumeric.Value = 0;
                    view.QtyRusakNumeric.Value = 0;

                    var success = view.CommitQuantityStripToGrid();

                    Assert.False(success);
                    Assert.Equal(0, view.TotalItemsCount);
                    Assert.True(audioHelper.WarningCount > 0);
                }
            });
        }

        [Fact]
        public void DeleteItemAt_RemovesLineAndResequencesNoUrutAndUpdatesSummary()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Item A", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    var lookup2 = BarcodeLookupResult.Hit("899222", "899222", "BRG02", "Item B", "Pak", 1m, true);
                    view.ApplyActiveProduct(lookup2);
                    view.QtyRusakNumeric.Value = 3;
                    view.CommitQuantityStripToGrid();

                    Assert.Equal(2, view.TotalItemsCount);

                    // Delete first row
                    view.DeleteItemAt(0);

                    Assert.Equal(1, view.TotalItemsCount);
                    Assert.Equal(1, view.ItemsList[0].NoUrut);
                    Assert.Equal("BRG02", view.ItemsList[0].BrgId);
                    Assert.Equal(0m, view.TotalQtyBagus);
                    Assert.Equal(3m, view.TotalQtyRusak);
                }
            });
        }

        [Fact]
        public void ResetForm_ClearsAllInputsAndItems()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    view.CustomerComboBox.Text = "Toko Sukses";
                    view.NotesTextBox.Text = "Catatan retur";

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Item A", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    Assert.Equal(1, view.TotalItemsCount);

                    view.ResetForm(promptUser: false);

                    Assert.Equal(string.Empty, view.CustomerComboBox.Text);
                    Assert.Equal(string.Empty, view.NotesTextBox.Text);
                    Assert.Equal(0, view.TotalItemsCount);
                    Assert.Null(view.CurrentScanResult);
                    Assert.Null(view.CurrentReturnOrderId);
                }
            });
        }

        [Fact]
        public void LoadOrder_PopulatesHeaderAndGridItems()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    var order = new ReturnOrderDto
                    {
                        ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                        CustId = "C001",
                        CustName = "Toko Makmur",
                        Keterangan = "Retur kemasan rusak",
                        Items = new List<ReturnOrderItemDto>
                        {
                            new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAV", 1, "BRG01", "Indomie Goreng", "8991", "BAGUS", 10m, "Dus"),
                            new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAV", 2, "BRG02", "Sari Roti", "8992", "RUSAK", 3m, "Pak")
                        }
                    };

                    view.LoadOrder(order);

                    Assert.Equal("01ARZ3NDEKTSV4RRFFQ69G5FAV", view.CurrentReturnOrderId);
                    Assert.Equal("Retur kemasan rusak", view.NotesTextBox.Text);
                    Assert.Equal(2, view.TotalItemsCount);
                    Assert.Equal(10m, view.TotalQtyBagus);
                    Assert.Equal(3m, view.TotalQtyRusak);
                }
            });
        }

        [Fact]
        public void SaveDraftAsync_WithItems_SucceedsWithoutThrowingBR012()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var audioHelper = new FakeAudioHelper();

                using (var view = new ReturnOrderEntryView(orderService, null, null, audioHelper, new FakeRegistryHelper()))
                {
                    view.SuppressMessageBoxes = true;
                    view.CustomerComboBox.Text = "Toko Makmur";
                    view.NotesTextBox.Text = "Retur barang";

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Indomie Goreng", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    var saved = view.SaveDraftAsync().GetAwaiter().GetResult();

                    Assert.NotNull(saved);
                    Assert.False(string.IsNullOrWhiteSpace(saved.ReturnOrderId));
                    Assert.Single(saved.Items);
                    Assert.Equal(5m, saved.TotalQtyBagus);
                    Assert.Single(repo.Store[saved.ReturnOrderId].Items);
                }
            });
        }

        [Fact]
        public void SaveDraftAsync_WithPlaceholderDriverAndSalesman_PersistsBlankOptionalNames()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var referenceRepo = new FakeReferenceRepo();
                referenceRepo.Drivers.Add(new Driver("DRV-01", "Bambang"));
                referenceRepo.SalesPersons.Add(new SalesPerson("SLS-01", "Agus"));

                using (var view = new ReturnOrderEntryView(orderService, null, referenceRepo, new FakeAudioHelper(), new FakeRegistryHelper()))
                {
                    view.SuppressMessageBoxes = true;

                    // The prompt items are selected by default; they must render the prompt but carry no value.
                    Assert.Equal("-- Pilih Driver (Opsional) --", view.DriverComboBox.SelectedItem.ToString());
                    Assert.Equal("-- Pilih Salesman (Opsional) --", view.SalesPersonComboBox.SelectedItem.ToString());

                    view.CustomerComboBox.Text = "Toko Makmur";

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Indomie Goreng", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    var saved = view.SaveDraftAsync().GetAwaiter().GetResult();

                    Assert.NotNull(saved);
                    Assert.True(string.IsNullOrEmpty(saved.DriverId));
                    Assert.True(string.IsNullOrEmpty(saved.DriverName));
                    Assert.True(string.IsNullOrEmpty(saved.SalesPersonId));
                    Assert.True(string.IsNullOrEmpty(saved.SalesPersonName));

                    var stored = repo.Store[saved.ReturnOrderId];
                    Assert.True(string.IsNullOrEmpty(stored.DriverName));
                    Assert.True(string.IsNullOrEmpty(stored.SalesPersonName));
                }
            });
        }

        [Fact]
        public void SaveDraftAsync_WithSelectedDriverAndSalesman_PersistsTheirNames()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var referenceRepo = new FakeReferenceRepo();
                referenceRepo.Drivers.Add(new Driver("DRV-01", "Bambang"));
                referenceRepo.SalesPersons.Add(new SalesPerson("SLS-01", "Agus"));

                using (var view = new ReturnOrderEntryView(orderService, null, referenceRepo, new FakeAudioHelper(), new FakeRegistryHelper()))
                {
                    view.SuppressMessageBoxes = true;
                    view.CustomerComboBox.Text = "Toko Makmur";

                    view.DriverComboBox.SelectedIndex = 1;
                    view.SalesPersonComboBox.SelectedIndex = 1;

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Indomie Goreng", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    var saved = view.SaveDraftAsync().GetAwaiter().GetResult();

                    Assert.NotNull(saved);
                    Assert.Equal("DRV-01", saved.DriverId);
                    Assert.Equal("Bambang", saved.DriverName);
                    Assert.Equal("SLS-01", saved.SalesPersonId);
                    Assert.Equal("Agus", saved.SalesPersonName);
                }
            });
        }

        [Fact]
        public void SaveDraftAsync_WithBlankWorkstationIdentity_RefusesAndDoesNotPersist()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var audioHelper = new FakeAudioHelper();
                var registry = new FakeRegistryHelper { WarehouseCode = string.Empty, LastOperatorName = string.Empty };

                using (var view = new ReturnOrderEntryView(orderService, null, null, audioHelper, registry))
                {
                    view.SuppressMessageBoxes = true;
                    view.CustomerComboBox.Text = "Toko Makmur";

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Indomie Goreng", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 5;
                    view.CommitQuantityStripToGrid();

                    var saved = view.SaveDraftAsync().GetAwaiter().GetResult();

                    Assert.Null(saved);
                    Assert.Empty(repo.Store);
                    Assert.True(audioHelper.WarningCount > 0);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_LayoutDimensions_ConformTo720pCompactSpecifications()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    // Root TableLayoutPanel check
                    var tblMain = view.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                    Assert.NotNull(tblMain);
                    Assert.True(tblMain.AutoScroll);
                    Assert.Equal(new Padding(12, 6, 12, 6), tblMain.Padding);

                    // Action Toolbar check
                    var toolbar = view.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                    Assert.NotNull(toolbar);
                    Assert.Equal(40, toolbar.Height);
                    Assert.Equal(new Padding(12, 4, 12, 4), toolbar.Padding);

                    // Header Area Card check
                    var pnlHeader = tblMain.GetControlFromPosition(0, 0) as Panel;
                    Assert.NotNull(pnlHeader);
                    Assert.Equal(new Padding(12, 6, 12, 8), pnlHeader.Padding);
                    Assert.Equal(new Padding(0, 0, 0, 6), pnlHeader.Margin);

                    var tblHeader = pnlHeader.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                    Assert.NotNull(tblHeader);
                    Assert.Equal(4, tblHeader.ColumnCount);
                    Assert.Equal(2, tblHeader.RowCount);
                    Assert.Equal(85F, tblHeader.ColumnStyles[0].Width);
                    Assert.Equal(100F, tblHeader.ColumnStyles[2].Width);

                    // Intake Card check
                    var pnlIntake = tblMain.GetControlFromPosition(0, 1) as Panel;
                    Assert.NotNull(pnlIntake);
                    Assert.Equal(new Padding(10, 6, 10, 6), pnlIntake.Padding);
                    Assert.Equal(new Padding(0, 0, 0, 6), pnlIntake.Margin);

                    var tblIntake = pnlIntake.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                    Assert.NotNull(tblIntake);
                    Assert.Equal(2, tblIntake.ColumnCount);
                    Assert.Equal(42F, tblIntake.ColumnStyles[0].Width);
                    Assert.Equal(58F, tblIntake.ColumnStyles[1].Width);

                    // Grid Container & Footer check
                    var pnlGrid = tblMain.GetControlFromPosition(0, 2) as Panel;
                    Assert.NotNull(pnlGrid);
                    Assert.Equal(new Padding(8, 6, 8, 6), pnlGrid.Padding);

                    var pnlFooter = pnlGrid.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                    Assert.NotNull(pnlFooter);
                    Assert.Equal(32, pnlFooter.Height);
                    Assert.Equal(new Padding(12, 6, 12, 6), pnlFooter.Padding);

                    // In 1280x720 maximized mode (content host ~1080x602), verify compact dimensions and grid row capacity
                    view.Size = new System.Drawing.Size(1080, 602);
                    view.PerformLayout();
                    tblMain.PerformLayout();
                    pnlGrid.PerformLayout();

                    Assert.InRange(pnlHeader.Height, 56, 62); // ~58-60px
                    Assert.InRange(pnlIntake.Height, 54, 58); // ~54-58px
                    Assert.True(view.ItemsGridView.Height >= 350,
                        $"Expected grid height >= 350px for 10-12 rows visibility, but was {view.ItemsGridView.Height}px");

                    // 10 data rows require 28px header + (10 * 26px) = 288px; verify grid displays at least 10 rows
                    int minHeightFor10Rows = 28 + (10 * 26);
                    Assert.True(view.ItemsGridView.Height >= minHeightFor10Rows,
                        $"Grid height {view.ItemsGridView.Height}px must accommodate at least 10 data rows ({minHeightFor10Rows}px)");
                }
            });
        }

        #region Fake Test Doubles

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

        private class ValidatingReturnOrderRepo : IReturnOrderRepo
        {
            public readonly Dictionary<string, ReturnOrder> Store = new Dictionary<string, ReturnOrder>();

            public ReturnOrder GetById(string returnOrderId) => Store.TryGetValue(returnOrderId, out var o) ? o : null;
            public Task<ReturnOrder> GetByIdAsync(string returnOrderId) => Task.FromResult(GetById(returnOrderId));
            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Store.Values;
            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));
            public IEnumerable<ReturnOrder> GetPendingUploadList() => Store.Values;
            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync() => Task.FromResult(GetPendingUploadList());

            public void Save(ReturnOrder returnOrder)
            {
                if (returnOrder == null) throw new ArgumentNullException(nameof(returnOrder));
                returnOrder.Validate(); // Strictly enforces BR-012!
                Store[returnOrder.ReturnOrderId] = returnOrder;
            }

            public Task SaveAsync(ReturnOrder returnOrder)
            {
                Save(returnOrder);
                return Task.CompletedTask;
            }

            public void MarkSynced(string returnOrderId, DateTime syncedDate) { }
            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate) => Task.CompletedTask;
            public void DeleteDraft(string returnOrderId) => Store.Remove(returnOrderId);
            public Task DeleteDraftAsync(string returnOrderId) => Task.CompletedTask;
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public readonly List<Driver> Drivers = new List<Driver>();
            public readonly List<SalesPerson> SalesPersons = new List<SalesPerson>();

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => Enumerable.Empty<Barang>();
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(Enumerable.Empty<Barang>());
            public Barang GetBarangById(string brgId) => null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult<Barang>(null);

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> SearchCustomer(string keyword) => Enumerable.Empty<Customer>();
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult(Enumerable.Empty<Customer>());
            public Customer GetCustomerById(string custId) => null;
            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult<Customer>(null);

            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) => Task.CompletedTask;
            public IEnumerable<Driver> GetAllDrivers() => Drivers;
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(GetAllDrivers());

            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) => Task.CompletedTask;
            public IEnumerable<SalesPerson> GetAllSalesPersons() => SalesPersons;
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(GetAllSalesPersons());
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "JOG";
            public string ApiBaseUrl { get; set; } = "http://localhost/";
            public string LastOperatorName { get; set; } = "Operator-1";
            public string LastOperatorEmail { get; set; } = "operator@example.com";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        #endregion
    }
}
