using System;
using System.Collections.Generic;
using System.Threading;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class BarcodeRegistryViewTests
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
        public void BarcodeRegistryView_InitializesWithCorrectControls()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new BarcodeRegistryView())
                {
                    Assert.NotNull(view.BarcodesGrid);
                    Assert.NotNull(view.SearchTextBox);
                    Assert.NotNull(view.SearchButton);
                    Assert.NotNull(view.AddBarcodeButton);
                    Assert.NotNull(view.StatusFilterBox);

                    Assert.Equal(0, view.StatusFilterBox.SelectedIndex); // "Semua Status"
                }
            });
        }

        [Fact]
        public void BarcodeRegistryView_FiltersByKeyword_Correctly()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new BarcodeRegistryView())
                {
                    var items = new List<BarcodeDisplayItem>
                    {
                        new BarcodeDisplayItem { Barcode = "899123456", BrgId = "BRG01", BrgName = "Kopi Susu ABC", Satuan = "Dus", Pengali = 24, Status = "Aktif Pusat", IsPending = false },
                        new BarcodeDisplayItem { Barcode = "899987654", BrgId = "BRG02", BrgName = "Teh Melati Sosro", Satuan = "Pak", Pengali = 12, Status = "Aktif Pusat", IsPending = false },
                        new BarcodeDisplayItem { Barcode = "999000111", BrgId = "BRG03", BrgName = "Indomie Goreng Rasa Ayam", Satuan = "Dus", Pengali = 40, Status = "Pending Upload", IsPending = true }
                    };

                    view.SetBarcodes(items);
                    Assert.Equal(3, view.BarcodesGrid.Rows.Count);

                    // Search by keyword "Teh"
                    view.SearchTextBox.Text = "Teh";
                    Assert.Single(view.BarcodesGrid.Rows);
                    Assert.Equal("899987654", view.BarcodesGrid.Rows[0].Cells["colBarcode"].Value);

                    // Search by barcode "999000"
                    view.SearchTextBox.Text = "999000";
                    Assert.Single(view.BarcodesGrid.Rows);
                    Assert.Equal("999000111", view.BarcodesGrid.Rows[0].Cells["colBarcode"].Value);

                    // Clear search
                    view.SearchTextBox.Text = string.Empty;
                    Assert.Equal(3, view.BarcodesGrid.Rows.Count);
                }
            });
        }

        [Fact]
        public void BarcodeRegistryView_FiltersByStatus_Correctly()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new BarcodeRegistryView())
                {
                    var items = new List<BarcodeDisplayItem>
                    {
                        new BarcodeDisplayItem { Barcode = "BC-01", BrgId = "B1", BrgName = "Item 1", Satuan = "Dus", Pengali = 1, Status = "Aktif Pusat", IsPending = false },
                        new BarcodeDisplayItem { Barcode = "BC-02", BrgId = "B2", BrgName = "Item 2", Satuan = "Dus", Pengali = 1, Status = "Pending Upload", IsPending = true }
                    };

                    view.SetBarcodes(items);

                    // Filter "Aktif Pusat" (index 1)
                    view.StatusFilterBox.SelectedIndex = 1;
                    Assert.Single(view.BarcodesGrid.Rows);
                    Assert.Equal("BC-01", view.BarcodesGrid.Rows[0].Cells["colBarcode"].Value);
                    Assert.Equal("Aktif Pusat", view.BarcodesGrid.Rows[0].Cells["colStatus"].Value);

                    // Filter "Pending Upload" (index 2)
                    view.StatusFilterBox.SelectedIndex = 2;
                    Assert.Single(view.BarcodesGrid.Rows);
                    Assert.Equal("BC-02", view.BarcodesGrid.Rows[0].Cells["colBarcode"].Value);
                    Assert.Equal("Pending Upload", view.BarcodesGrid.Rows[0].Cells["colStatus"].Value);

                    // Filter "Semua Status" (index 0)
                    view.StatusFilterBox.SelectedIndex = 0;
                    Assert.Equal(2, view.BarcodesGrid.Rows.Count);
                }
            });
        }
    }
}
