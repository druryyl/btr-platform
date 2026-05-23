using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.UseCases;
using btr.distrib.SharedForm;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using ClosedXML.Excel;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Grouping;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetInfoForm : Form
    {
        private readonly ISalesOmzetDal _salesOmzetDal;
        private readonly IReconcileSalesOmzetWorker _reconcileWorker;
        private List<SalesOmzetView> _dataSource;

        public SalesOmzetInfoForm(
            ISalesOmzetDal salesOmzetDal,
            IReconcileSalesOmzetWorker reconcileWorker)
        {
            InitializeComponent();
            _salesOmzetDal = salesOmzetDal;
            _reconcileWorker = reconcileWorker;

            var periodToolTip = new ToolTip();
            periodToolTip.SetToolTip(
                SalesPeriodCheckBox,
                "Tidak dicentang: Periode Omzet (hanya omzet yang sudah Kembali Faktur). " +
                "Dicentang: Periode Jual (filter Tanggal Jual, termasuk outstanding).");

            ProsesButton.Click += ProsesButton_Click;
            ExcelButton.Click += ExcelButton_Click;
            FullRebuildButton.Click += FullRebuildButton_Click;

            // Register the QueryCellStyleInfo event for conditional formatting
            InfoGrid.QueryCellStyleInfo += InfoGrid_QueryCellStyleInfo;

            InitGrid();
            _dataSource = new List<SalesOmzetView>();
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            // Export _dataSource to excel
            string filePath;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Excel Files|*.xlsx";
                saveFileDialog.Title = @"Save Excel File";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = $"sales-omzet-{DateTime.Now:yyyy-MM-dd-HHmm}";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                filePath = saveFileDialog.FileName;
            }

            var filtered = this.InfoGrid.Table.FilteredRecords;
            var listToExcel = new List<SalesOmzetView>();
            foreach (var item in filtered)
            {
                listToExcel.Add(item.GetData() as SalesOmzetView);
            }

            using (IXLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("sales-omzet");

                // Create header row
                ws.Cell("A1").Value = "No";
                ws.Cell("B1").Value = "Sales Name";
                ws.Cell("C1").Value = "Order ID";
                ws.Cell("D1").Value = "Order Date";
                ws.Cell("E1").Value = "Order Total";
                ws.Cell("F1").Value = "Faktur Code";
                ws.Cell("G1").Value = "Faktur Date";
                ws.Cell("H1").Value = "Customer Name";
                ws.Cell("I1").Value = "Cust.Code";
                ws.Cell("J1").Value = "Alamat";
                ws.Cell("K1").Value = "Faktur Total";
                ws.Cell("L1").Value = "Omzet Date";
                ws.Cell("M1").Value = "Status";

                // Fill data rows
                for (var i = 0; i < listToExcel.Count; i++)
                {
                    var omzet = listToExcel[i];
                    var row = i + 2;

                    string status = GetStatusDisplayText(omzet);

                    ws.Cell($"A{row}").Value = i + 1;
                    ws.Cell($"B{row}").Value = omzet.SalesPersonName;
                    ws.Cell($"C{row}").Value = omzet.OrderId;
                    ws.Cell($"D{row}").Value = FormatExportDate(omzet.OrderDate);
                    ws.Cell($"E{row}").Value = omzet.OrderTotal;
                    ws.Cell($"F{row}").Value = omzet.FakturCode;
                    ws.Cell($"G{row}").Value = FormatExportDate(omzet.FakturDate);
                    ws.Cell($"H{row}").Value = omzet.CustomerName;
                    ws.Cell($"I{row}").Value = omzet.Code;
                    ws.Cell($"J{row}").Value = omzet.Alamat;
                    ws.Cell($"K{row}").Value = omzet.FakturTotal;
                    ws.Cell($"L{row}").Value = FormatExportDate(omzet.OmzetDate);
                    ws.Cell($"M{row}").Value = status;

                    // Apply conditional formatting only to Status column (M)
                    Color bgColor = GetStatusColor(status);
                    ws.Cell($"M{row}").Style.Fill.BackgroundColor = XLColor.FromColor(bgColor);
                }

                var lastRow = listToExcel.Count + 1;

                // Ensure border covers all used columns (A..M)
                var fullRange = ws.Range(ws.Cell($"A1"), ws.Cell($"M{lastRow}"));
                fullRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                // Set fonts: default non-numeric to Segoe UI
                fullRange.Style.Font.FontName = "Segoe UI";
                fullRange.Style.Font.FontSize = 9;

                // Header styling
                var headerRange = ws.Range(ws.Cell($"A1"), ws.Cell($"M1"));
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontName = "Segoe UI";

                // Format numeric columns to monospace and numeric format
                var noRange = ws.Range(ws.Cell($"A2"), ws.Cell($"A{lastRow}"));
                var orderTotalRange = ws.Range(ws.Cell($"E2"), ws.Cell($"E{lastRow}"));
                var fakturTotalRange = ws.Range(ws.Cell($"K2"), ws.Cell($"K{lastRow}"));

                noRange.Style.Font.FontName = "Lucida Console";
                orderTotalRange.Style.Font.FontName = "Lucida Console";
                fakturTotalRange.Style.Font.FontName = "Lucida Console";

                orderTotalRange.Style.NumberFormat.Format = "#,##0";
                fakturTotalRange.Style.NumberFormat.Format = "#,##0";

                // Format date columns to dd-MM-yyyy
                var orderDateRange = ws.Range(ws.Cell($"D2"), ws.Cell($"D{lastRow}"));
                var fakturDateRange = ws.Range(ws.Cell($"G2"), ws.Cell($"G{lastRow}"));
                var omzetDateRange = ws.Range(ws.Cell($"L2"), ws.Cell($"L{lastRow}"));

                orderDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";
                fakturDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";
                omzetDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";

                // Auto-fit all columns
                ws.Columns().AdjustToContents();

                wb.SaveAs(filePath);
            }

            System.Diagnostics.Process.Start(filePath);
        }

        private void InitGrid()
        {
            InfoGrid.DataSource = new List<SalesOmzetView>();

            InfoGrid.TableDescriptor.AllowEdit = false;
            InfoGrid.TableDescriptor.AllowNew = false;
            InfoGrid.TableDescriptor.AllowRemove = false;
            InfoGrid.ShowGroupDropArea = true;

            InfoGrid.TopLevelGroupOptions.ShowFilterBar = true;
            foreach (GridColumnDescriptor column in InfoGrid.TableDescriptor.Columns)
            {
                column.AllowFilter = true;
            }

            // Configure column appearances
            InfoGrid.TableDescriptor.Columns["OrderTotal"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["OrderTotal"].Appearance.AnyRecordFieldCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            InfoGrid.TableDescriptor.Columns["FakturTotal"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["FakturTotal"].Appearance.AnyRecordFieldCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            InfoGrid.TableDescriptor.Columns["OrderDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";
            InfoGrid.TableDescriptor.Columns["FakturDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";
            InfoGrid.TableDescriptor.Columns["OmzetDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";

            // Set column widths for better readability
            InfoGrid.TableDescriptor.Columns["SalesPersonName"].Width = 150;
            InfoGrid.TableDescriptor.Columns["OrderTotal"].Width = 100;
            InfoGrid.TableDescriptor.Columns["FakturCode"].Width = 120;
            InfoGrid.TableDescriptor.Columns["FakturTotal"].Width = 100;
            InfoGrid.TableDescriptor.Columns["OrderId"].Width = 120;

            HideGridColumn("OmzetStatus");
            HideGridColumn("SaleKind");

            // Summary rows for totals
            var sumColOrderTotal = new GridSummaryColumnDescriptor("OrderTotal", SummaryType.DoubleAggregate, "OrderTotal", "{Sum}");
            sumColOrderTotal.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.LightYellow);
            sumColOrderTotal.Appearance.AnySummaryCell.Format = "N0";
            sumColOrderTotal.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumColFakturTotal = new GridSummaryColumnDescriptor("FakturTotal", SummaryType.DoubleAggregate, "FakturTotal", "{Sum}");
            sumColFakturTotal.Appearance.AnySummaryCell.Interior = new BrushInfo(Color.LightYellow);
            sumColFakturTotal.Appearance.AnySummaryCell.Format = "N0";
            sumColFakturTotal.Appearance.AnySummaryCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            var sumRowDescriptor = new GridSummaryRowDescriptor();
            sumRowDescriptor.SummaryColumns.AddRange(new GridSummaryColumnDescriptor[] { sumColOrderTotal, sumColFakturTotal });
            InfoGrid.TableDescriptor.SummaryRows.Add(sumRowDescriptor);

            //// Optional: Add grouping by SalesName
            //InfoGrid.TableDescriptor.Columns["SalesName"].AllowGroup = true;

            // For all records field cells
            InfoGrid.TableDescriptor.Appearance.AnyRecordFieldCell.AutoSize = true;
            InfoGrid.TableDescriptor.Appearance.AnyRecordFieldCell.WrapText = false;

            InfoGrid.Refresh();
            //Proses();
        }

        private void ProsesButton_Click(object sender, EventArgs e)
        {
            Proses();
        }

        private void FullRebuildButton_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Rebuild semua data omzet dari seluruh order dan faktur? " +
                "Proses ini dapat memakan waktu lama. Lanjutkan?",
                "Rebuild Omzet",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
                return;

            RunReconcile(ReconcileSalesOmzetScope.Full);
            MessageBox.Show(
                "Rebuild selesai. Jalankan Proses untuk periode laporan yang diinginkan.",
                "Rebuild Omzet",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void Proses()
        {
            var tgl1 = Tgl1Date.Value;
            var tgl2 = Tgl2Date.Value;
            var periode = new Periode(tgl1, tgl2);
            var timeSpan = tgl2 - tgl1;
            var dayCount = timeSpan.Days;

            if (dayCount > 122)
            {
                MessageBox.Show("Periode informasi maximal 3 bulan");
                return;
            }

            var mode = SalesPeriodCheckBox.Checked
                ? SalesOmzetPeriodFilterMode.SalesPeriod
                : SalesOmzetPeriodFilterMode.OmzetPeriod;

            RunReconcile(ReconcileSalesOmzetScope.PeriodeScoped, periode);

            var listOmzet = _salesOmzetDal.ListData(periode, mode)?.ToList() ?? new List<SalesOmzetView>();
            _dataSource = Filter(listOmzet, SearchText.Text);
            InfoGrid.DataSource = _dataSource;
        }

        private void RunReconcile(
            ReconcileSalesOmzetScope scope,
            Periode periode = null,
            string userId = null)
        {
            if (periode is null)
            {
                var end = DateTime.Today;
                periode = new Periode(end.AddYears(-20), end);
            }

            if (userId is null)
            {
                userId = string.Empty;
                if (Parent?.Parent is MainForm mainForm && mainForm.UserId != null)
                    userId = mainForm.UserId.UserId;
            }

            var request = new ReconcileSalesOmzetRequest
            {
                Periode = periode,
                Scope = scope,
                UserId = userId
            };

            _reconcileWorker.Execute(request);
            ShowReconcileStatus(request.Result);
        }

        private void ShowReconcileStatus(ReconcileSalesOmzetResult result)
        {
            if (result is null)
            {
                ReconcileStatusLabel.Text = string.Empty;
                return;
            }

            var seconds = result.Duration.TotalSeconds;
            ReconcileStatusLabel.Text =
                $"Reconcile ({result.Scope}): {result.OrdersProcessed} order, {result.FaktursProcessed} faktur, " +
                $"{result.RowsRefreshed} baris ({seconds:0.#} d)";
        }

        private static DateTime FormatExportDate(DateTime value) =>
            SalesOmzetDates.IsSentinel(value) || value == DateTime.MinValue
                ? default
                : value;

        private List<SalesOmzetView> Filter(List<SalesOmzetView> source, string keyword)
        {
            if (keyword.Trim().Length == 0)
                return source;

            var keywordLower = keyword.ToLower();
            var listFilteredSales = source.Where(x =>
                x.SalesPersonName.ToLower().ContainMultiWord(keywordLower)).ToList();

            var listFilteredFaktur = source.Where(x =>
                x.FakturCode.ToLower().Contains(keywordLower)).ToList();

            var listFilteredOrder = source.Where(x =>
                x.OrderId.ToLower().Contains(keywordLower)).ToList();

            var result = listFilteredSales
                .Union(listFilteredFaktur)
                .Union(listFilteredOrder);

            return result.ToList();
        }

        private void HideGridColumn(string columnName)
        {
            if (InfoGrid.TableDescriptor.Columns.Contains(columnName))
                InfoGrid.TableDescriptor.VisibleColumns.Remove(columnName);
        }

        private static string GetStatusDisplayText(SalesOmzetView omzet)
        {
            switch (omzet.OmzetStatus)
            {
                case SalesOmzetStatusEnum.Outstanding:
                    return "Outstanding Order";
                case SalesOmzetStatusEnum.PendingOmzet:
                    return "Pending Omzet";
                case SalesOmzetStatusEnum.Completed:
                    return omzet.SaleKind == SaleKindEnum.DirectSale || string.IsNullOrEmpty(omzet.OrderId)
                        ? "Direct Sales"
                        : "Completed Order";
                default:
                    return "Unknown";
            }
        }

        // Helper method to get color based on status
        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Outstanding Order":
                    return Color.MistyRose;  // Red-ish for outstanding
                case "Completed Order":
                    return Color.PaleGreen;  // Green for completed
                case "Direct Sales":
                    return Color.PowderBlue;  // Blue for direct sales
                case "Pending Omzet":
                    return Color.LightGoldenrodYellow;
                default:
                    return Color.White;
            }
        }

        // Conditional formatting for grid rows
        private void InfoGrid_QueryCellStyleInfo(object sender, GridTableCellStyleInfoEventArgs e)
        {
            if (e.TableCellIdentity.TableCellType == GridTableCellType.GroupCaptionCell)
            {
                e.Style.Themed = false;
                e.Style.BackColor = Color.PowderBlue;
            }
        }
    }
}
