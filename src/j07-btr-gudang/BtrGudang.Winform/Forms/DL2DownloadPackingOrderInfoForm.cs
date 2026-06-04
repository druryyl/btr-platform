using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Winform.Helpers;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    public partial class DL2DownloadPackingOrderInfoForm : Form
    {
        private readonly IPackingOrderRepo _packingOrderRepo;
        private BindingList<PackingOrderView> _allData;
        private BindingSource _bindingSource;

        public DL2DownloadPackingOrderInfoForm(IPackingOrderRepo packingOrderRepo)
        {
            _packingOrderRepo = packingOrderRepo;
            InitializeComponent();
            InitializeCustomComponents();
            RegisterControlEventHandler();
        }

        private void RegisterControlEventHandler()
        {
            LoadDataButton.Click += LoadDataButton_Click;
            FilterTextBox.TextChanged += FilterText_TextChanged;
            ExportCsvButton.Click += ExcelButton_Click;
            CloseButton.Click += CloseButton_Click;
            PackingOrderGrid.RowPostPaint += DataGridViewExtension.DataGridView_RowPostPaint;
            PackingOrderGrid.CellDoubleClick += PackingOrderGrid_CellDoubleClick;
        }

        private void PackingOrderGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < PackingOrderGrid.Rows.Count)
            {
                DataGridViewRow row = PackingOrderGrid.Rows[e.RowIndex];

                // Check if it's not the new row placeholder
                if (!row.IsNewRow)
                {
                    var currentOrder = row.DataBoundItem as PackingOrderView;
                    var packingOrder = _packingOrderRepo.LoadEntity(PackingOrderModel.Key(currentOrder.PackingOrderId));
                    if (packingOrder.HasValue)
                        ViewDetilItem(packingOrder.Value);
                }
            }
        }

        private void InitializeCustomComponents()
        {
            // Initialize BindingSource
            _bindingSource = new BindingSource();
            _allData = new BindingList<PackingOrderView>();
            _bindingSource.DataSource = _allData;

            // Setup DataGridView columns
            SetupDataGridViewColumns();
        }

        private void SetupDataGridViewColumns()
        {
            PackingOrderGrid.AutoGenerateColumns = true;
            PackingOrderGrid.DataSource = _bindingSource;
            PackingOrderGrid.Columns.SetDefaultCellStyle(System.Drawing.Color.Azure);
            var col = PackingOrderGrid.Columns;
            col["PackingOrderId"].Width = 200;
            col["FakturCode"].Width = 100;
            
            col["FakturDate"].Width = 100;
            col["FakturDate"].DefaultCellStyle.Format = "dd-MM-yyyy";
            //col["Driver"].Width = 100;
            col["CustomerCode"].Width = 80;
            col["CustomerName"].Width = 225;
            col["Alamat"].Width = 180;
            col["DownloadTimestamp"].DefaultCellStyle.Format = "dd-MM-yyyy HH:mm:ss";
            col["DownloadTimestamp"].Width = 120;

        }

        private void PT1DownloadPackingOrderInfoForm_Load(object sender, EventArgs e)
        {
            UpdateRecordCount();
            SetStatus("Ready to load data", System.Drawing.Color.FromArgb(76, 175, 80));
        }

        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                SetStatus("Loading data...", System.Drawing.Color.FromArgb(255, 152, 0));
                var downloadTimestamp = Periode1DatePicker.Value;
                var data = _packingOrderRepo.ListByDownloadTimestamp(downloadTimestamp);

                // Update data source
                _allData = new BindingList<PackingOrderView>(data.ToList());
                _bindingSource.DataSource = _allData;

                UpdateRecordCount();
                SetStatus($"Data loaded successfully - {_allData.Count} records", System.Drawing.Color.FromArgb(76, 175, 80));

                // Clear filter
                FilterTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                SetStatus("Error loading data", System.Drawing.Color.FromArgb(211, 47, 47));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void FilterText_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filterText = FilterTextBox.Text.Trim();

            if (string.IsNullOrEmpty(filterText))
            {
                _bindingSource.DataSource = _allData;
            }
            else
            {
                var filtered = _allData.Where(x =>
                    (!string.IsNullOrEmpty(x.CustomerName) && x.CustomerName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.FakturCode) && x.FakturCode.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.Alamat) && x.Alamat.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.DriverName) && x.DriverName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                _bindingSource.DataSource = new BindingList<PackingOrderView>(filtered);
            }

            UpdateRecordCount();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateRecordCount()
        {
            var totalRecords = _allData?.Count ?? 0;
            var filteredRecords = _bindingSource?.Count ?? 0;

            lblRecordCount.Text = $"Total Records: {totalRecords} | Filtered: {filteredRecords}";
        }

        private void SetStatus(string message, System.Drawing.Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            //  export _dataSource to excel
            string filePath;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Excel Files|*.xlsx";
                saveFileDialog.Title = @"Save Excel File";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = $"download-packing-order-info-{DateTime.Now:yyyy-MM-dd-HHmm}";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                filePath = saveFileDialog.FileName;
            }

            var listToExcel = _bindingSource.DataSource as BindingList<PackingOrderView>;

            using (IXLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet("download-packing-order-info")
                    .Cell($"B1")
                    .InsertTable(listToExcel, false);
                var ws = wb.Worksheets.First();
                //  set border and font
                ws.Range(ws.Cell($"A{1}"), ws.Cell($"I{listToExcel.Count + 1}")).Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair);
                ws.Range(ws.Cell($"A{1}"), ws.Cell($"I{listToExcel.Count + 1}")).Style
                    .Font.SetFontName("Lucida Console")
                    .Font.SetFontSize(9);

                //  set format number for columnto N0
                ws.Range(ws.Cell($"D{2}"), ws.Cell($"I{listToExcel.Count + 1}"))
                    .Style.NumberFormat.Format = "dd-MM-yyyy";
                ws.Range(ws.Cell($"H{2}"), ws.Cell($"I{listToExcel.Count + 1}"))
                    .Style.NumberFormat.Format = "dd-MM-yyyy HH:mm:ss";

                //  add rownumbering
                ws.Cell($"A1").Value = "No";
                for (var i = 0; i < listToExcel.Count; i++)
                    ws.Cell($"A{i + 2}").Value = i + 1;
                ws.Columns().AdjustToContents();
                wb.SaveAs(filePath);
            }
            System.Diagnostics.Process.Start(filePath);
        }

        private void ViewDetilItem(PackingOrderModel packingOrder)
        {
            var form = new DL2DownloadPackingOrderItemForm(packingOrder);
            form.ShowDialog();
        }
    }
}