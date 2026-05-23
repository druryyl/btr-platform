using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services;
using btr.application.SupportContext.UserAgg;
using btr.distrib.SharedForm;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Drawing;
using Syncfusion.Grouping;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetInfoForm : Form, ISalesOmzetChartHost
    {
        private readonly ISalesOmzetDal _salesOmzetDal;
        private readonly ISalesOmzetHealthWeeklyDal _healthWeeklyDal;
        private readonly IIsoWeekCalendar _isoWeekCalendar;
        private readonly ISalesOmzetReportHealthResolver _reportHealthResolver;
        private readonly IUserDal _userDal;
        private List<SalesOmzetView> _dataSource;
        private List<SalesOmzetView> _fullDataSource;
        private Periode _reportPeriode;
        private SalesOmzetReportHealthResult _lastReportHealth;
        private int _healthLoadGeneration;
        private bool _suppressSearchFilterRefresh;

        public bool IsManagerView { get; private set; }

        IReadOnlyList<SalesOmzetView> ISalesOmzetChartHost.FilteredRows => _dataSource;
        IReadOnlyList<SalesOmzetView> ISalesOmzetChartHost.FullRows => _fullDataSource;
        Periode ISalesOmzetChartHost.ReportPeriode => _reportPeriode ?? new Periode(Tgl1Date.Value, Tgl2Date.Value);
        SalesOmzetPeriodFilterMode ISalesOmzetChartHost.PeriodMode => CurrentPeriodMode();
        string ISalesOmzetChartHost.SearchKeyword => SearchText.Text;

        public SalesOmzetInfoForm(
            ISalesOmzetDal salesOmzetDal,
            ISalesOmzetHealthWeeklyDal healthWeeklyDal,
            IIsoWeekCalendar isoWeekCalendar,
            ISalesOmzetReportHealthResolver reportHealthResolver,
            IUserDal userDal)
        {
            InitializeComponent();

            _salesOmzetDal = salesOmzetDal;
            _healthWeeklyDal = healthWeeklyDal;
            _isoWeekCalendar = isoWeekCalendar;
            _reportHealthResolver = reportHealthResolver;
            _userDal = userDal;

            var periodToolTip = new ToolTip();
            periodToolTip.SetToolTip(
                SalesPeriodCheckBox,
                "Tidak dicentang: Periode Omzet (hanya omzet yang sudah Kembali Faktur). " +
                "Dicentang: Periode Jual (filter Tanggal Jual, termasuk outstanding).");

            var materializeToolTip = new ToolTip();
            materializeToolTip.SetToolTip(
                MaterializeButton,
                "Perbarui data agregat BTR_SalesOmzet untuk periode ini");

            var healthToolTip = new ToolTip();
            healthToolTip.SetToolTip(
                HealthWeeklyButton,
                "Hitung dan simpan indikator kesehatan materialisasi untuk satu minggu ISO.");
            healthToolTip.SetToolTip(
                MaterializeHealthLabel,
                "Indikator gabungan periode laporan (worst-bucket dari minggu ISO yang bersinggungan).");
            healthToolTip.SetToolTip(
                HealthDetailsLabel,
                "Rincian per minggu ISO; minggu belum dihitung dianggap Buruk.");

            ProsesButton.Click += ProsesButton_Click;
            ExcelButton.Click += ExcelButton_Click;
            ChartButton.Click += ChartButton_Click;
            MaterializeButton.Click += MaterializeButton_Click;
            HealthWeeklyButton.Click += HealthWeeklyButton_Click;
            Tgl1Date.ValueChanged += ReportPeriodDates_ValueChanged;
            Tgl2Date.ValueChanged += ReportPeriodDates_ValueChanged;
            Shown += SalesOmzetInfoForm_Shown;

            InfoGrid.QueryCellStyleInfo += InfoGrid_QueryCellStyleInfo;
            SearchText.TextChanged += SearchText_TextChanged;

            InitGrid();
            _dataSource = new List<SalesOmzetView>();
            _fullDataSource = new List<SalesOmzetView>();
            _reportPeriode = new Periode(Tgl1Date.Value, Tgl2Date.Value);
        }

        /// <summary>Manager / pusat view: no default rep search.</summary>
        public void ConfigureAsManagerView()
        {
            IsManagerView = true;
            Text = "RO2 - Sales Omzet (Pusat)";
            SearchText.Clear();
        }

        void ISalesOmzetChartHost.ApplySalesPersonFilter(string salesPersonName)
        {
            _suppressSearchFilterRefresh = true;
            try
            {
                SearchText.Text = salesPersonName ?? string.Empty;
            }
            finally
            {
                _suppressSearchFilterRefresh = false;
            }

            ApplySearchFilter();
        }

        string ISalesOmzetChartHost.GetCurrentUserDisplayName() => GetCurrentUserDisplayName();

        private void ChartButton_Click(object sender, EventArgs e)
        {
            if (!(MdiParent is MainForm mainForm))
                return;

            var chartForm = mainForm.ThisServicesProvider.GetRequiredService<SalesOmzetChartForm>();
            chartForm.BindHost(this);
            if (IsManagerView)
                chartForm.ConfigureAsManagerView();
            chartForm.ShowDialog(this);
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSearchFilterRefresh || _fullDataSource is null)
                return;
            ApplySearchFilter();
        }

        private void SalesOmzetInfoForm_Shown(object sender, EventArgs e)
        {
            if (!IsManagerView)
                TrySetDefaultSalesPersonSearch();
            _ = RefreshHealthAsync();
        }

        private void TrySetDefaultSalesPersonSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchText.Text))
                return;

            var userName = GetCurrentUserDisplayName();
            if (!string.IsNullOrWhiteSpace(userName))
                SearchText.Text = userName;
        }

        private string GetCurrentUserDisplayName()
        {
            if (!(MdiParent is MainForm mainForm))
                return null;

            try
            {
                var user = _userDal.GetData(mainForm.UserId);
                return user?.UserName;
            }
            catch
            {
                return null;
            }
        }

        private void ReportPeriodDates_ValueChanged(object sender, EventArgs e)
        {
            _ = RefreshHealthAsync();
        }

        private void HealthWeeklyButton_Click(object sender, EventArgs e)
        {
            if (MdiParent is MainForm mainForm)
            {
                var form = mainForm.ThisServicesProvider.GetRequiredService<SalesOmzetHealthWeeklyForm>();
                form.ShowDialog(this);
                _ = RefreshHealthAsync();
            }
        }

        private async Task RefreshHealthAsync()
        {
            var generation = ++_healthLoadGeneration;
            MaterializeHealthLabel.Text = "Memuat indikator kesehatan...";
            MaterializeHealthLabel.BackColor = SystemColors.Control;
            HealthDetailsLabel.Text = string.Empty;

            var reportPeriode = new Periode(Tgl1Date.Value, Tgl2Date.Value);

            SalesOmzetReportHealthResult resolved;
            try
            {
                resolved = await Task.Run(() =>
                {
                    var weeks = _isoWeekCalendar.ListWeeksIntersecting(reportPeriode)?.ToList() ?? new List<IsoWeekIdentifier>();
                    var rows = _healthWeeklyDal.ListByYearWeeks(weeks)?.ToList() ?? new List<SalesOmzetHealthWeeklyModel>();
                    return _reportHealthResolver.Resolve(reportPeriode, weeks, rows);
                });
            }
            catch (Exception ex)
            {
                if (generation != _healthLoadGeneration)
                    return;
                MaterializeHealthLabel.Text = "Indikator gagal dimuat: " + ex.Message;
                MaterializeHealthLabel.BackColor = Color.MistyRose;
                return;
            }

            if (generation != _healthLoadGeneration)
                return;

            _lastReportHealth = resolved;
            ApplyHealthLabel(resolved);
        }

        private void ApplyHealthLabel(SalesOmzetReportHealthResult health)
        {
            MaterializeHealthLabel.Text = SalesOmzetHealthLevelDisplay.FormatReportSummary(health);
            HealthDetailsLabel.Text = SalesOmzetHealthLevelDisplay.FormatWeekDetails(health.WeekDetails);

            Color backColor;
            switch (health.FinalLevel)
            {
                case SalesOmzetHealthLevelEnum.Good:
                    backColor = Color.PaleGreen;
                    break;
                case SalesOmzetHealthLevelEnum.Warning:
                    backColor = Color.LightGoldenrodYellow;
                    break;
                default:
                    backColor = Color.MistyRose;
                    break;
            }

            MaterializeHealthLabel.BackColor = backColor;
            healthPanel.BackColor = backColor;
        }

        private async void MaterializeButton_Click(object sender, EventArgs e)
        {
            if (MdiParent is MainForm mainForm)
            {
                var matForm = mainForm.ThisServicesProvider.GetRequiredService<SalesOmzetMaterializeForm>();
                matForm.SetInitialPeriode(Tgl1Date.Value, Tgl2Date.Value);
                matForm.ShowDialog(this);
                await RefreshHealthAsync();
            }
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
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

                    Color bgColor = GetStatusColor(status);
                    ws.Cell($"M{row}").Style.Fill.BackgroundColor = XLColor.FromColor(bgColor);
                }

                var lastRow = listToExcel.Count + 1;
                var fullRange = ws.Range(ws.Cell($"A1"), ws.Cell($"M{lastRow}"));
                fullRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                fullRange.Style.Font.FontName = "Segoe UI";
                fullRange.Style.Font.FontSize = 9;

                var headerRange = ws.Range(ws.Cell($"A1"), ws.Cell($"M1"));
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontName = "Segoe UI";

                var noRange = ws.Range(ws.Cell($"A2"), ws.Cell($"A{lastRow}"));
                var orderTotalRange = ws.Range(ws.Cell($"E2"), ws.Cell($"E{lastRow}"));
                var fakturTotalRange = ws.Range(ws.Cell($"K2"), ws.Cell($"K{lastRow}"));

                noRange.Style.Font.FontName = "Lucida Console";
                orderTotalRange.Style.Font.FontName = "Lucida Console";
                fakturTotalRange.Style.Font.FontName = "Lucida Console";

                orderTotalRange.Style.NumberFormat.Format = "#,##0";
                fakturTotalRange.Style.NumberFormat.Format = "#,##0";

                var orderDateRange = ws.Range(ws.Cell($"D2"), ws.Cell($"D{lastRow}"));
                var fakturDateRange = ws.Range(ws.Cell($"G2"), ws.Cell($"G{lastRow}"));
                var omzetDateRange = ws.Range(ws.Cell($"L2"), ws.Cell($"L{lastRow}"));

                orderDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";
                fakturDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";
                omzetDateRange.Style.NumberFormat.Format = "dd-MM-yyyy";

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

            InfoGrid.TableDescriptor.Columns["OrderTotal"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["OrderTotal"].Appearance.AnyRecordFieldCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            InfoGrid.TableDescriptor.Columns["FakturTotal"].Appearance.AnyRecordFieldCell.Format = "N0";
            InfoGrid.TableDescriptor.Columns["FakturTotal"].Appearance.AnyRecordFieldCell.HorizontalAlignment = GridHorizontalAlignment.Right;

            InfoGrid.TableDescriptor.Columns["OrderDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";
            InfoGrid.TableDescriptor.Columns["FakturDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";
            InfoGrid.TableDescriptor.Columns["OmzetDate"].Appearance.AnyRecordFieldCell.Format = "dd-MMM-yyyy";

            InfoGrid.TableDescriptor.Columns["SalesPersonName"].Width = 150;
            InfoGrid.TableDescriptor.Columns["OrderTotal"].Width = 100;
            InfoGrid.TableDescriptor.Columns["FakturCode"].Width = 120;
            InfoGrid.TableDescriptor.Columns["FakturTotal"].Width = 100;
            InfoGrid.TableDescriptor.Columns["OrderId"].Width = 120;

            HideGridColumn("OmzetStatus");
            HideGridColumn("SaleKind");

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

            InfoGrid.TableDescriptor.Appearance.AnyRecordFieldCell.AutoSize = true;
            InfoGrid.TableDescriptor.Appearance.AnyRecordFieldCell.WrapText = false;
            InfoGrid.Refresh();
        }

        private void ProsesButton_Click(object sender, EventArgs e)
        {
            Proses();
        }

        private void Proses()
        {
            var tgl1 = Tgl1Date.Value;
            var tgl2 = Tgl2Date.Value;
            var periode = new Periode(tgl1, tgl2);
            var dayCount = (tgl2 - tgl1).Days;

            if (dayCount > 122)
            {
                MessageBox.Show("Periode informasi maximal 3 bulan");
                return;
            }

            var mode = CurrentPeriodMode();
            var listOmzet = _salesOmzetDal.ListData(periode, mode)?.ToList() ?? new List<SalesOmzetView>();
            _fullDataSource = listOmzet;
            _dataSource = Filter(_fullDataSource, SearchText.Text);
            _reportPeriode = periode;
            InfoGrid.DataSource = _dataSource;
            _ = RefreshHealthAsync();
        }

        private void ApplySearchFilter()
        {
            if (_fullDataSource is null)
                return;
            _dataSource = Filter(_fullDataSource, SearchText.Text);
            InfoGrid.DataSource = _dataSource;
        }

        private SalesOmzetPeriodFilterMode CurrentPeriodMode() =>
            SalesPeriodCheckBox.Checked
                ? SalesOmzetPeriodFilterMode.SalesPeriod
                : SalesOmzetPeriodFilterMode.OmzetPeriod;

        private static DateTime FormatExportDate(DateTime value) =>
            SalesOmzetDates.IsSentinel(value) || value == DateTime.MinValue
                ? default
                : value;

        private List<SalesOmzetView> Filter(List<SalesOmzetView> source, string keyword)
        {
            if (keyword.Trim().Length == 0)
                return source;

            var keywordLower = keyword.ToLower();
            var listFilteredSales = source.Where(x => x.SalesPersonName.ToLower().ContainMultiWord(keywordLower)).ToList();
            var listFilteredFaktur = source.Where(x => x.FakturCode.ToLower().Contains(keywordLower)).ToList();
            var listFilteredOrder = source.Where(x => x.OrderId.ToLower().Contains(keywordLower)).ToList();

            var result = listFilteredSales.Union(listFilteredFaktur).Union(listFilteredOrder);
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

        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Outstanding Order":
                    return Color.MistyRose;
                case "Completed Order":
                    return Color.PaleGreen;
                case "Direct Sales":
                    return Color.PowderBlue;
                case "Pending Omzet":
                    return Color.LightGoldenrodYellow;
                default:
                    return Color.White;
            }
        }

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

