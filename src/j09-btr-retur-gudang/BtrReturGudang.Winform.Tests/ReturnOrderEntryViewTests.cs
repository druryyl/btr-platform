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

        #endregion
    }
}
