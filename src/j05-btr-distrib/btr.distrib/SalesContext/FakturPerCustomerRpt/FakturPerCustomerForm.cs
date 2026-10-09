using btr.application.SalesContext.FakturPerCustomerRpt;
using btr.nuna.Domain;
using ClosedXML.Excel;
using Syncfusion.Grouping;
using Syncfusion.Windows.Forms.Grid.Grouping;
using Syncfusion.Windows.Forms.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.Drawing;

namespace btr.distrib.SalesContext.FakturPerCustomerRpt
{
    public partial class FakturPerCustomerForm : Form
    {
        private readonly IFakturPerCustomerDal _fakturPerCustomerDal;
        private List<FakturPerCustomerView> _dataSource = new List<FakturPerCustomerView>();
        private List<FakturPerCustomerView> _rawLoadedList = new List<FakturPerCustomerView>();
        private CancellationTokenSource _cts;
        private bool _isLoading;

        public FakturPerCustomerForm(IFakturPerCustomerDal fakturPerCustomerDal)
        {
            InitializeComponent();
            _fakturPerCustomerDal = fakturPerCustomerDal;
            InfoGrid.QueryCellStyleInfo += InfoGrid_QueryCellStyleInfo;
            ExcelButton.Click += ExcelButton_Click;
            ProsesButton.Click += ProsesButton_Click;
            InitGrid();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await ProsesAsync();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isLoading)
            {
                _cts?.Cancel();
            }
            base.OnFormClosing(e);
        }

        private void InfoGrid_QueryCellStyleInfo(object sender, GridTableCellStyleInfoEventArgs e)
        {
            if (e.TableCellIdentity.TableCellType == GridTableCellType.GroupCaptionCell)
            {
                e.Style.Themed = false;
                e.Style.BackColor = Color.PowderBlue;
            }
        }

        private void InitGrid()
        {
            _dataSource = new List<FakturPerCustomerView>();
            InfoGrid.DataSource = _dataSource;

            InfoGrid.TableDescriptor.AllowEdit = false;
            InfoGrid.TableDescriptor.AllowNew = false;
            InfoGrid.TableDescriptor.AllowRemove = false;
            InfoGrid.ShowGroupDropArea = true;

            InfoGrid.TopLevelGroupOptions.ShowFilterBar = true;
            foreach (GridColumnDescriptor column in InfoGrid.TableDescriptor.Columns)
            {
                column.AllowFilter = true;
            }
            InfoGrid.TableDescriptor.VisibleColumns.Remove("StatusFaktur");

            var sumColSubTotal = new GridSummaryColumnDescriptor("SubTotal", SummaryType.DoubleAggregate, "SubTotal", "{Sum}");
            sumColSubTotal.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.Yellow);
            sumColSubTotal.Appearance.AnySummaryCell.Format = "N0";
            sumColSubTotal.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumColTotalDisc = new GridSummaryColumnDescriptor("TotalDisc", SummaryType.DoubleAggregate, "TotalDisc", "{Sum}");
            sumColTotalDisc.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.Yellow);
            sumColTotalDisc.Appearance.AnySummaryCell.Format = "N0";
            sumColTotalDisc.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumColTotalSebelumTax = new GridSummaryColumnDescriptor("TotalSebelumTax", SummaryType.DoubleAggregate, "TotalSebelumTax", "{Sum}");
            sumColTotalSebelumTax.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.Yellow);
            sumColTotalSebelumTax.Appearance.AnySummaryCell.Format = "N0";
            sumColTotalSebelumTax.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumColPpnRp = new GridSummaryColumnDescriptor("PpnRp", SummaryType.DoubleAggregate, "PpnRp", "{Sum}");
            sumColPpnRp.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.Yellow);
            sumColPpnRp.Appearance.AnySummaryCell.Format = "N0";
            sumColPpnRp.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumColTotal = new GridSummaryColumnDescriptor("Total", SummaryType.DoubleAggregate, "Total", "{Sum}");
            sumColTotal.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.Yellow);
            sumColTotal.Appearance.AnySummaryCell.Format = "N0";
            sumColTotal.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumRowDescriptor = new GridSummaryRowDescriptor();
            sumRowDescriptor.SummaryColumns.AddRange(new GridSummaryColumnDescriptor[] { sumColSubTotal, sumColTotalDisc, sumColTotalSebelumTax, sumColPpnRp, sumColTotal });
            InfoGrid.TableDescriptor.SummaryRows.Add(sumRowDescriptor);

            InfoGrid.TableDescriptor.Columns["QtyBesar"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["HrgSatBesar"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["QtyKecil"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["HrgSatKecil"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["QtyPotStok"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["SubTotal"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["DiscProsen1"].Appearance.AnyRecordFieldCell.Format = "N2";
            InfoGrid.TableDescriptor.Columns["DiscProsen2"].Appearance.AnyRecordFieldCell.Format = "N2";
            InfoGrid.TableDescriptor.Columns["DiscProsen3"].Appearance.AnyRecordFieldCell.Format = "N2";
            InfoGrid.TableDescriptor.Columns["DiscProsen4"].Appearance.AnyRecordFieldCell.Format = "N2";
            InfoGrid.TableDescriptor.Columns["TotalDisc"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["TotalSebelumTax"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["PpnRp"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["Total"].Appearance.AnyRecordFieldCell.Format = "N0";
            
            //  format column FaturDate and DueDate to dd MMM yyyy
            InfoGrid.TableDescriptor.Columns["FakturDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";
            InfoGrid.TableDescriptor.Columns["DueDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";

            InfoGrid.TableDescriptor.Columns["QtyBesar"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["SatBesar"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["HrgSatBesar"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["QtyKecil"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["SatKecil"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["HrgSatKecil"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["QtyPotStok"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["QtyBonus"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;
            InfoGrid.TableDescriptor.Columns["SubTotal"].Appearance.AnyRecordFieldCell.BackColor = Color.LightGreen;

            InfoGrid.TableDescriptor.Columns["DiscProsen1"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;
            InfoGrid.TableDescriptor.Columns["DiscProsen2"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;
            InfoGrid.TableDescriptor.Columns["DiscProsen3"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;
            InfoGrid.TableDescriptor.Columns["DiscProsen4"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;
            InfoGrid.TableDescriptor.Columns["TotalDisc"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;
            InfoGrid.TableDescriptor.Columns["TotalSebelumTax"].Appearance.AnyRecordFieldCell.BackColor = Color.LightYellow;

            InfoGrid.TableDescriptor.Columns["PpnRp"].Appearance.AnyRecordFieldCell.BackColor = Color.LightBlue;
            InfoGrid.TableDescriptor.Columns["Total"].Appearance.AnyRecordFieldCell.BackColor = Color.LightBlue;

            InfoGrid.Refresh();
        }

        private async void ProsesButton_Click(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                _cts?.Cancel();
                StatusLabel.Text = "Membatalkan proses...";
                return;
            }

            await ProsesAsync();
        }

        private static List<Periode> CreateSlices(DateTime start, DateTime end, int sliceDays = 14)
        {
            var slices = new List<Periode>();
            var currentStart = start.Date;
            var endDate = end.Date;

            if (currentStart > endDate)
                return slices;

            while (currentStart <= endDate)
            {
                var currentEnd = currentStart.AddDays(sliceDays - 1);
                if (currentEnd > endDate)
                    currentEnd = endDate;

                slices.Add(new Periode(currentStart, currentEnd));
                currentStart = currentEnd.AddDays(1);
            }
            return slices;
        }

        private async Task ProsesAsync()
        {
            if (Tgl1Date.Value.Date > Tgl2Date.Value.Date)
            {
                MessageBox.Show("Tanggal awal tidak boleh melebihi tanggal akhir.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var slices = CreateSlices(Tgl1Date.Value, Tgl2Date.Value, sliceDays: 14);
            if (!slices.Any())
                return;

            _isLoading = true;
            _cts = new CancellationTokenSource();

            Tgl1Date.Enabled = false;
            Tgl2Date.Enabled = false;
            SearchText.Enabled = false;
            ExcelButton.Enabled = false;
            ProsesButton.Text = "Batal";
            ProsesButton.BackColor = Color.IndianRed;

            ProgressBar.Visible = true;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = slices.Count;
            ProgressBar.Value = 0;

            var accumulated = new List<FakturPerCustomerView>();
            bool wasCancelled = false;

            try
            {
                for (int i = 0; i < slices.Count; i++)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    var currentSlice = slices[i];
                    StatusLabel.Text = $"Memuat periode: {currentSlice.Tgl1:dd-MMM-yyyy} s/d {currentSlice.Tgl2:dd-MMM-yyyy} (Bagian {i + 1}/{slices.Count})... {accumulated.Count:N0} baris terkumpul";
                    ProgressBar.Value = i;

                    var sliceData = await _fakturPerCustomerDal.ListDataAsync(currentSlice, _cts.Token);
                    if (sliceData != null)
                    {
                        accumulated.AddRange(sliceData);
                    }
                }
                ProgressBar.Value = slices.Count;
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan saat memuat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Tgl1Date.Enabled = true;
                Tgl2Date.Enabled = true;
                SearchText.Enabled = true;
                ExcelButton.Enabled = true;
                ProsesButton.Text = "Proses";
                ProsesButton.BackColor = Color.LightSeaGreen;
                ProgressBar.Visible = false;
            }

            _rawLoadedList = accumulated;

            // Memory guard check
            if (accumulated.Count > 50000)
            {
                var msg = $"Data yang dimuat berjumlah {accumulated.Count:N0} baris.\n\n" +
                          $"Memuat lebih dari 50.000 baris ke dalam grid dapat membebani memori dan menyebabkan keterlambatan respon aplikasi.\n\n" +
                          $"Apakah Anda tetap ingin menampilkan seluruh data ke dalam grid?\n" +
                          $"(Pilih 'No' untuk menyimpan data di memori agar dapat langsung diekspor ke Excel).";
                var result = MessageBox.Show(msg, "Peringatan Jumlah Data Besar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    StatusLabel.Text = $"Data siap diekspor ke Excel ({accumulated.Count:N0} baris). Tidak dimuat ke grid.";
                    return;
                }
            }

            // Bind to grid safely
            var filtered = Filter(accumulated, SearchText.Text).ToList();
            filtered.ForEach(x => x.FakturDate = x.FakturDate.Date);
            _dataSource = filtered;

            InfoGrid.BeginUpdate();
            try
            {
                InfoGrid.DataSource = _dataSource;
            }
            finally
            {
                InfoGrid.EndUpdate(true);
            }

            if (wasCancelled)
            {
                StatusLabel.Text = $"Dibatalkan pengguna. {_dataSource.Count:N0} baris data berhasil dimuat.";
            }
            else
            {
                StatusLabel.Text = $"Selesai. Total {_dataSource.Count:N0} baris data ditampilkan.";
            }
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            var filtered = this.InfoGrid.Table?.FilteredRecords;
            var listToExcel = new List<FakturPerCustomerView>();
            if (filtered != null && filtered.Count > 0)
            {
                foreach (var item in filtered)
                {
                    if (item.GetData() is FakturPerCustomerView row)
                        listToExcel.Add(row);
                }
            }
            else if (_rawLoadedList != null && _rawLoadedList.Count > 0)
            {
                listToExcel = Filter(_rawLoadedList, SearchText.Text).ToList();
            }

            if (listToExcel.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diekspor ke Excel.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listToExcel.Count > 100000)
            {
                var confirm = MessageBox.Show(
                    $"Jumlah data sangat besar ({listToExcel.Count:N0} baris).\n" +
                    "Proses export ke Excel membutuhkan alokasi memori besar dan beberapa menit.\n\n" +
                    "Lanjutkan proses export?",
                    "Konfirmasi Export Besar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;
            }

            string filePath;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Excel Files|*.xlsx";
                saveFileDialog.Title = @"Save Excel File";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = $"faktur-per-customer-info-{DateTime.Now:yyyy-MM-dd-HHmm}";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                filePath = saveFileDialog.FileName;
            }

            Cursor = Cursors.WaitCursor;
            StatusLabel.Text = $"Mengekspor {listToExcel.Count:N0} baris ke Excel...";
            try
            {
                using (IXLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet("faktur-per-customer-info")
                    .Cell($"B1")
                        .InsertTable(listToExcel, false);
                    var ws = wb.Worksheets.First();

                    //  set format row header: font bold, background lightblue, border medium
                    ws.Range(ws.Cell("A1"), ws.Cell($"AG1")).Style
                        .Font.SetFontName("Lucida Console")
                        .Font.SetFontSize(9)
                        .Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.LightBlue)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                    //  set format row data: font Lucida Console 9, border medium, border inside hair
                    ws.Range(ws.Cell("A2"), ws.Cell($"AG{listToExcel.Count + 1}")).Style
                        .Font.SetFontName("Lucida Console")
                        .Font.SetFontSize(9)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                    //  add row numbering
                    ws.Cell($"A1").Value = "No";
                    for (var i = 0; i < listToExcel.Count; i++)
                        ws.Cell($"A{i + 2}").Value = i + 1;
                    ws.Range(ws.Cell("A2"), ws.Cell($"AG{listToExcel.Count + 1}"))
                        .Style.NumberFormat.Format = "#,##";

                    //  format numeric column  
                    ws.Range(ws.Cell("Q2"), ws.Cell($"AG{listToExcel.Count + 1}"))
                        .Style.NumberFormat.Format = "#,##";

                    //  format date column to dd MMM yyyy
                    ws.Range(ws.Cell("C2"), ws.Cell($"C{listToExcel.Count + 1}"))
                        .Style.NumberFormat.Format = "dd-MMM-yyyy";
                    ws.Range(ws.Cell("D2"), ws.Cell($"D{listToExcel.Count + 1}"))
                        .Style.NumberFormat.Format = "dd-MMM-yyyy";
                    
                    //  format numeric column DiscTotal with 2 decimal places but hide zero
                    ws.Range(ws.Cell("Z2"), ws.Cell($"AC{listToExcel.Count + 1}"))
                        .Style.NumberFormat.Format = "#,##0.00_);(#,##0.00);-";

                    //  set backcolor numeric column
                    ws.Range(ws.Cell("Q2"), ws.Cell($"Y{listToExcel.Count + 1}"))
                        .Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                    ws.Range(ws.Cell("Z2"), ws.Cell($"AD{listToExcel.Count + 1}"))
                        .Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                    ws.Range(ws.Cell("AE2"), ws.Cell($"AG{listToExcel.Count + 1}"))
                        .Style.Fill.SetBackgroundColor(XLColor.LightMauve);

                    //  add row footer: sum column NilaiStokBesar, NilaiStokKecil, NilaiInPcs
                    ws.Cell($"Y{listToExcel.Count + 2}").FormulaA1 = $"=SUM(Y2:Y{listToExcel.Count + 1})";
                    ws.Cell($"AD{listToExcel.Count + 2}").FormulaA1 = $"=SUM(AD2:AD{listToExcel.Count + 1})";
                    ws.Cell($"AE{listToExcel.Count + 2}").FormulaA1 = $"=SUM(AE2:AE{listToExcel.Count + 1})";
                    ws.Cell($"AF{listToExcel.Count + 2}").FormulaA1 = $"=SUM(AF2:AF{listToExcel.Count + 1})";
                    ws.Cell($"AG{listToExcel.Count + 2}").FormulaA1 = $"=SUM(AG2:AG{listToExcel.Count + 1})";

                    //  format row footer font bold, background yellow, border medium
                    ws.Range(ws.Cell($"S{listToExcel.Count + 2}"), ws.Cell($"AG{listToExcel.Count + 2}")).Style
                        .Font.SetFontName("Lucida Console")
                        .Font.SetBold()
                        .NumberFormat.SetFormat("#,##")
                        .Fill.SetBackgroundColor(XLColor.Yellow)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(filePath);
                }
                StatusLabel.Text = $"Export Excel selesai: {filePath}";
                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengekspor file Excel: {ex.Message}", "Error Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StatusLabel.Text = "Export Excel gagal.";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static IEnumerable<FakturPerCustomerView> Filter(IReadOnlyCollection<FakturPerCustomerView> source, string keyword)
        {
            if (source == null || !source.Any())
                return new List<FakturPerCustomerView>();

            var kw = keyword?.Trim() ?? string.Empty;
            if (kw.Length == 0)
                return source;

            var kwLower = kw.ToLower();
            return source.Where(x =>
                (x.BrgName != null && x.BrgName.ToLower().ContainMultiWord(kw)) ||
                (x.BrgCode != null && x.BrgCode.ToLower().StartsWith(kwLower)) ||
                (x.SupplierName != null && x.SupplierName.ToLower().ContainMultiWord(kw)) ||
                (x.CustomerName != null && x.CustomerName.ToLower().ContainMultiWord(kw)) ||
                (x.FakturCode != null && x.FakturCode.ToLower().StartsWith(kwLower))
            ).ToList();
        }
    }
}
