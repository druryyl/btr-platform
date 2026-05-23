using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SupportContext.UserAgg;
using btr.distrib.SharedForm;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Syncfusion.Drawing;
using Syncfusion.Grouping;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetInfoForm : Form
    {
        private readonly ISalesOmzetDal _salesOmzetDal;
        private readonly ISalesOmzetMaterializeHealthDal _materializeHealthDal;
        private readonly ISalesOmzetChartSummaryBuilder _chartSummaryBuilder;
        private readonly ISalesOmzetTargetResolver _targetResolver;
        private readonly IUserDal _userDal;
        private readonly SalesOmzetHealthOptions _healthOptions;
        private List<SalesOmzetView> _dataSource;
        private List<SalesOmzetView> _fullDataSource;
        private Periode _reportPeriode;
        private SalesOmzetMaterializeHealth _lastHealth;
        private int _healthLoadGeneration;
        private bool _suppressSearchFilterRefresh;

        public bool IsManagerView { get; private set; }

        private enum OmzetChartDisplayMode
        {
            Status,
            Weekly,
            ManagerComparison
        }

        public SalesOmzetInfoForm(
            ISalesOmzetDal salesOmzetDal,
            ISalesOmzetMaterializeHealthDal materializeHealthDal,
            ISalesOmzetChartSummaryBuilder chartSummaryBuilder,
            ISalesOmzetTargetResolver targetResolver,
            IUserDal userDal,
            IOptions<SalesOmzetHealthOptions> healthOptions)
        {
            InitializeComponent();
            _salesOmzetDal = salesOmzetDal;
            _materializeHealthDal = materializeHealthDal;
            _chartSummaryBuilder = chartSummaryBuilder;
            _targetResolver = targetResolver;
            _userDal = userDal;
            _healthOptions = healthOptions?.Value ?? new SalesOmzetHealthOptions();

            var defaultEnd = _healthOptions.WindowEndDate?.Date ?? DateTime.Today;
            HealthWindowEndDate.Value = defaultEnd;

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
                HealthWindowEndLabel,
                "Akhir periode 60 hari untuk indikator materialisasi (perkiraan). " +
                "Atur ke tanggal data di DB dev jika snapshot tertinggal.");
            healthToolTip.SetToolTip(
                HealthWindowEndDate,
                "Ubah tanggal akhir bucket 60 hari; indikator dimuat ulang.");
            healthToolTip.SetToolTip(
                MaterializeHealthLabel,
                "Perkiraan data belum tersinkron ke BTR_SalesOmzet. " +
                "Tidak terikat filter Periode Omzet/Jual pada laporan.");

            ProsesButton.Click += ProsesButton_Click;
            ExcelButton.Click += ExcelButton_Click;
            MaterializeButton.Click += MaterializeButton_Click;
            HealthWindowEndDate.ValueChanged += HealthWindowEndDate_ValueChanged;
            Shown += SalesOmzetInfoForm_Shown;

            InfoGrid.QueryCellStyleInfo += InfoGrid_QueryCellStyleInfo;

            ChartModeCombo.SelectedIndex = 0;
            ChartModeCombo.SelectedIndexChanged += ChartModeCombo_SelectedIndexChanged;
            SearchText.TextChanged += SearchText_TextChanged;
            OmzetChart.MouseClick += OmzetChart_MouseClick;

            InitGrid();
            InitOmzetChart();
            _dataSource = new List<SalesOmzetView>();
            _fullDataSource = new List<SalesOmzetView>();
            _reportPeriode = new Periode(Tgl1Date.Value, Tgl2Date.Value);
            RefreshChartAndKpi();
        }

        /// <summary>Manager / pusat view: no default rep search; opens on comparison chart.</summary>
        public void ConfigureAsManagerView()
        {
            IsManagerView = true;
            Text = "RO2 - Sales Omzet (Pusat)";
            SearchText.Clear();
            SelectChartMode(OmzetChartDisplayMode.ManagerComparison);
        }

        private void ChartModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshChartAndKpi();
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSearchFilterRefresh || _fullDataSource is null)
                return;

            ApplySearchFilter();
        }

        private OmzetChartDisplayMode CurrentChartDisplayMode()
        {
            switch (ChartModeCombo.SelectedIndex)
            {
                case 1:
                    return OmzetChartDisplayMode.Weekly;
                case 2:
                    return OmzetChartDisplayMode.ManagerComparison;
                default:
                    return OmzetChartDisplayMode.Status;
            }
        }

        private void SelectChartMode(OmzetChartDisplayMode mode)
        {
            var index = 0;
            switch (mode)
            {
                case OmzetChartDisplayMode.Weekly:
                    index = 1;
                    break;
                case OmzetChartDisplayMode.ManagerComparison:
                    index = 2;
                    break;
            }

            if (ChartModeCombo.SelectedIndex != index)
                ChartModeCombo.SelectedIndex = index;
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

        private void HealthWindowEndDate_ValueChanged(object sender, EventArgs e)
        {
            _ = RefreshHealthAsync();
        }

        private async Task RefreshHealthAsync()
        {
            var generation = ++_healthLoadGeneration;
            MaterializeHealthLabel.Text = "Memuat indikator materialisasi...";
            MaterializeHealthLabel.BackColor = SystemColors.Control;

            var window = SalesOmzetMaterializeHealthWindow.Resolve(HealthWindowEndDate.Value);

            SalesOmzetMaterializeHealth health;
            try
            {
                health = await Task.Run(() => _materializeHealthDal.GetHealth(window));
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

            _lastHealth = health;
            ApplyHealthLabel(health);
        }

        private void ApplyHealthLabel(SalesOmzetMaterializeHealth health)
        {
            var metrics = new SalesOmzetMaterializeHealthMetrics
            {
                MissingOrders = health.MissingOrders,
                MissingDirectFakturs = health.MissingDirectFakturs,
                UnlinkedFakturs = health.UnlinkedFakturs,
                AggregateRowsInScope = health.AggregateRowsInScope,
                LastReconciledMax = health.LastReconciledMax,
                StaleFakturEstimate = health.StaleFakturEstimate
            };

            MaterializeHealthLabel.Text = SalesOmzetMaterializeHealthPolicy.FormatDisplayText(
                metrics, health.Level, health.Window);

            Color backColor;
            switch (health.Level)
            {
                case SalesOmzetMaterializeHealthLevel.Good:
                    backColor = Color.PaleGreen;
                    break;
                case SalesOmzetMaterializeHealthLevel.Warning:
                    backColor = Color.LightGoldenrodYellow;
                    break;
                default:
                    backColor = Color.MistyRose;
                    break;
            }
            MaterializeHealthLabel.BackColor = backColor;
        }

        private async void MaterializeButton_Click(object sender, EventArgs e)
        {
            if (MdiParent is MainForm mainForm)
            {
                var matForm = mainForm.ThisServicesProvider.GetRequiredService<SalesOmzetMaterializeForm>();

                if (_lastHealth != null && _lastHealth.Level != SalesOmzetMaterializeHealthLevel.Good)
                {
                    matForm.SetInitialPeriode(_lastHealth.Window.Tgl1, _lastHealth.Window.Tgl2);
                }
                else
                {
                    matForm.SetInitialPeriode(Tgl1Date.Value, Tgl2Date.Value);
                }

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

            var mode = SalesPeriodCheckBox.Checked
                ? SalesOmzetPeriodFilterMode.SalesPeriod
                : SalesOmzetPeriodFilterMode.OmzetPeriod;

            var listOmzet = _salesOmzetDal.ListData(periode, mode)?.ToList() ?? new List<SalesOmzetView>();
            _fullDataSource = listOmzet;
            _dataSource = Filter(_fullDataSource, SearchText.Text);
            _reportPeriode = periode;
            InfoGrid.DataSource = _dataSource;
            RefreshChartAndKpi();
        }

        private void ApplySearchFilter()
        {
            if (_fullDataSource is null)
                return;

            _dataSource = Filter(_fullDataSource, SearchText.Text);
            InfoGrid.DataSource = _dataSource;
            RefreshChartAndKpi();
        }

        private bool IsSearchEmpty() => string.IsNullOrWhiteSpace(SearchText.Text);

        private IEnumerable<SalesOmzetView> RowsForManagerComparison() =>
            _fullDataSource ?? _dataSource ?? new List<SalesOmzetView>();

        private SalesOmzetPeriodFilterMode CurrentPeriodMode() =>
            SalesPeriodCheckBox.Checked
                ? SalesOmzetPeriodFilterMode.SalesPeriod
                : SalesOmzetPeriodFilterMode.OmzetPeriod;

        private void InitOmzetChart()
        {
            OmzetChart.ChartAreas.Clear();
            OmzetChart.Series.Clear();
            OmzetChart.Legends.Clear();
            OmzetChart.Titles.Clear();
            OmzetChart.Annotations.Clear();

            var chartArea = new ChartArea("MainArea")
            {
                BackColor = Color.White
            };
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisX.LabelStyle.Enabled = false;
            chartArea.AxisY.LabelStyle.Format = "N0";
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            OmzetChart.ChartAreas.Add(chartArea);

            var legend = new Legend("Legend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 8F)
            };
            OmzetChart.Legends.Add(legend);

            OmzetChart.Titles.Add(new Title("Omzet menurut status", Docking.Top, new Font("Segoe UI", 9F, FontStyle.Bold), Color.Black));
            OmzetChart.Titles.Add(new Title
            {
                Text = GetChartSubtitle(CurrentPeriodMode(), OmzetChartDisplayMode.Status),
                Docking = Docking.Top,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.DimGray
            });
        }

        private void RefreshChartAndKpi()
        {
            var mode = CurrentPeriodMode();
            var periode = _reportPeriode ?? new Periode(Tgl1Date.Value, Tgl2Date.Value);
            var chartMode = CurrentChartDisplayMode();
            var kpiRows = chartMode == OmzetChartDisplayMode.ManagerComparison && IsSearchEmpty()
                ? RowsForManagerComparison()
                : _dataSource ?? new List<SalesOmzetView>();

            var targetAmount = _targetResolver.ResolveTarget(
                kpiRows,
                SearchText.Text,
                periode,
                GetCurrentUserDisplayName());

            var summary = _chartSummaryBuilder.Build(
                kpiRows,
                periode,
                mode,
                targetAmount);

            var managerComparison = chartMode == OmzetChartDisplayMode.ManagerComparison
                ? _chartSummaryBuilder.BuildManagerComparison(RowsForManagerComparison())
                : null;

            RecognizedOmzetValueLabel.Text = summary.RecognizedOmzet.ToString("N0");
            TransactionCountValueLabel.Text = summary.RecognizedTransactionCount.ToString("N0");
            TargetValueLabel.Text = summary.Target.HasValue
                ? summary.Target.Value.ToString("N0")
                : "—";
            AchievementValueLabel.Text = SalesOmzetChartAchievementPolicy.FormatPercentDisplay(
                summary.AchievementPercent);

            var salesPeriod = mode == SalesOmzetPeriodFilterMode.SalesPeriod;
            PipelineCaptionLabel.Visible = salesPeriod;
            PipelineValueLabel.Visible = salesPeriod;
            if (salesPeriod)
                PipelineValueLabel.Text = summary.PipelineOmzet.ToString("N0");

            if (OmzetChart.Titles.Count > 0)
                OmzetChart.Titles[0].Text = GetChartTitle(CurrentChartDisplayMode());

            if (OmzetChart.Titles.Count > 1)
                OmzetChart.Titles[1].Text = GetChartSubtitle(mode, CurrentChartDisplayMode());

            switch (chartMode)
            {
                case OmzetChartDisplayMode.Weekly:
                    BindWeeklyChart(summary);
                    break;
                case OmzetChartDisplayMode.ManagerComparison:
                    BindManagerComparisonChart(managerComparison);
                    break;
                default:
                    BindStatusChart(summary);
                    break;
            }
        }

        private static string GetChartTitle(OmzetChartDisplayMode displayMode)
        {
            switch (displayMode)
            {
                case OmzetChartDisplayMode.Weekly:
                    return "Omzet mingguan (diakui)";
                case OmzetChartDisplayMode.ManagerComparison:
                    return "Perbandingan omzet diakui per sales";
                default:
                    return "Omzet menurut status";
            }
        }

        private string GetChartSubtitle(
            SalesOmzetPeriodFilterMode mode,
            OmzetChartDisplayMode displayMode)
        {
            if (displayMode == OmzetChartDisplayMode.ManagerComparison)
            {
                if (!IsSearchEmpty())
                    return "Kosongkan pencarian untuk membandingkan semua sales (top 15)";

                return $"Top {SalesOmzetChartSummaryBuilder.ManagerComparisonTopCount} sales — omzet diakui, klik bar untuk filter";
            }

            if (displayMode == OmzetChartDisplayMode.Weekly)
                return "Hanya omzet diakui (selesai) per minggu dalam periode";

            return mode == SalesOmzetPeriodFilterMode.SalesPeriod
                ? "Periode Jual — termasuk order outstanding dan pending omzet"
                : "Periode Omzet — hanya omzet yang sudah Kembali Faktur";
        }

        private void ConfigureChartAreaForMode(OmzetChartDisplayMode displayMode)
        {
            var chartArea = OmzetChart.ChartAreas["MainArea"];
            var manager = displayMode == OmzetChartDisplayMode.ManagerComparison;
            var weekly = displayMode == OmzetChartDisplayMode.Weekly;

            chartArea.AxisX.LabelStyle.Enabled = weekly || manager;
            chartArea.AxisX.LabelStyle.Format = manager ? "N0" : null;
            chartArea.AxisX.LabelStyle.Angle = weekly ? -45 : 0;
            chartArea.AxisX.MajorGrid.Enabled = weekly || manager;

            chartArea.AxisY.LabelStyle.Enabled = manager || !weekly;
            chartArea.AxisY.LabelStyle.Angle = manager ? 0 : 0;
            chartArea.AxisY.MajorGrid.Enabled = !manager;
        }

        private void BindStatusChart(SalesOmzetChartSummary summary)
        {
            OmzetChart.Series.Clear();
            OmzetChart.Annotations.Clear();
            OmzetChart.Legends["Legend"].Enabled = true;
            ConfigureChartAreaForMode(OmzetChartDisplayMode.Status);

            var chartArea = OmzetChart.ChartAreas["MainArea"];
            chartArea.AxisX.CustomLabels.Clear();

            if (summary.ByStatus == null || summary.ByStatus.Count == 0)
            {
                ApplyTargetStripLine(chartArea, null);
                ShowChartNoDataMessage();
                return;
            }

            const string category = "Omzet";
            foreach (var slice in summary.ByStatus)
            {
                var series = new Series(slice.Label)
                {
                    ChartType = SeriesChartType.StackedColumn,
                    ChartArea = chartArea.Name,
                    Legend = "Legend",
                    Color = GetChartSliceColor(slice.Label)
                };
                series.Points.AddXY(category, (double)slice.Amount);
                series.ToolTip = $"{slice.Label}: {slice.Amount:N0}";
                OmzetChart.Series.Add(series);
            }

            ApplyTargetStripLine(chartArea, summary.Target);
        }

        private void BindWeeklyChart(SalesOmzetChartSummary summary)
        {
            OmzetChart.Series.Clear();
            OmzetChart.Annotations.Clear();
            OmzetChart.Legends["Legend"].Enabled = false;
            ConfigureChartAreaForMode(OmzetChartDisplayMode.Weekly);

            var chartArea = OmzetChart.ChartAreas["MainArea"];
            chartArea.AxisX.CustomLabels.Clear();

            if (summary.ByWeek == null || !summary.ByWeek.Any(w => w.RecognizedAmount > 0))
            {
                ApplyTargetStripLine(chartArea, null);
                ShowChartNoDataMessage();
                return;
            }

            var series = new Series("Omzet diakui")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = chartArea.Name,
                Color = Color.PaleGreen,
                BorderWidth = 1
            };

            decimal cumulative = 0;
            foreach (var week in summary.ByWeek)
            {
                series.Points.AddXY(week.WeekLabel, (double)week.RecognizedAmount);
                var pointIndex = series.Points.Count - 1;
                series.Points[pointIndex].ToolTip =
                    $"{week.WeekLabel}: {week.RecognizedAmount:N0}";
            }

            OmzetChart.Series.Add(series);

            var cumulativeSeries = new Series("Kumulatif")
            {
                ChartType = SeriesChartType.Line,
                ChartArea = chartArea.Name,
                Color = Color.SteelBlue,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6
            };

            foreach (var week in summary.ByWeek)
            {
                cumulative += week.RecognizedAmount;
                cumulativeSeries.Points.AddXY(week.WeekLabel, (double)cumulative);
                var pointIndex = cumulativeSeries.Points.Count - 1;
                cumulativeSeries.Points[pointIndex].ToolTip =
                    $"{week.WeekLabel} kumulatif: {cumulative:N0}";
            }

            OmzetChart.Series.Add(cumulativeSeries);
            ApplyTargetStripLine(chartArea, summary.Target);
        }

        private void BindManagerComparisonChart(IReadOnlyList<SalesOmzetSalesPersonSlice> slices)
        {
            OmzetChart.Series.Clear();
            OmzetChart.Annotations.Clear();
            OmzetChart.Legends["Legend"].Enabled = false;
            ConfigureChartAreaForMode(OmzetChartDisplayMode.ManagerComparison);

            var chartArea = OmzetChart.ChartAreas["MainArea"];
            chartArea.AxisX.CustomLabels.Clear();
            ApplyTargetStripLine(chartArea, null);

            if (!IsSearchEmpty())
            {
                ShowChartMessage("Kosongkan pencarian untuk perbandingan sales");
                return;
            }

            if (slices == null || slices.Count == 0)
            {
                ShowChartNoDataMessage();
                return;
            }

            var series = new Series("Omzet diakui")
            {
                ChartType = SeriesChartType.Bar,
                ChartArea = chartArea.Name,
                Color = Color.PaleGreen,
                BorderWidth = 1
            };

            foreach (var slice in slices.OrderBy(s => s.RecognizedOmzet))
            {
                series.Points.AddXY(slice.SalesPersonName, (double)slice.RecognizedOmzet);
                var pointIndex = series.Points.Count - 1;
                series.Points[pointIndex].ToolTip =
                    $"{slice.SalesPersonName}: {slice.RecognizedOmzet:N0}";
            }

            OmzetChart.Series.Add(series);
        }

        private void OmzetChart_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentChartDisplayMode() != OmzetChartDisplayMode.ManagerComparison || !IsSearchEmpty())
                return;

            var result = OmzetChart.HitTest(e.X, e.Y);
            if (result.ChartElementType != ChartElementType.DataPoint)
                return;

            var salesPersonName = result.Series.Points[result.PointIndex].AxisLabel;
            if (string.IsNullOrWhiteSpace(salesPersonName))
                return;

            _suppressSearchFilterRefresh = true;
            try
            {
                SearchText.Text = salesPersonName;
            }
            finally
            {
                _suppressSearchFilterRefresh = false;
            }

            ApplySearchFilter();
            SelectChartMode(OmzetChartDisplayMode.Status);
        }

        private static void ApplyTargetStripLine(ChartArea chartArea, decimal? target)
        {
            chartArea.AxisY.StripLines.Clear();
            if (!target.HasValue || target.Value <= 0)
                return;

            var stripLine = new StripLine
            {
                Interval = 0,
                IntervalOffset = (double)target.Value,
                StripWidth = 0,
                BorderColor = Color.OrangeRed,
                BorderWidth = 2
            };
            chartArea.AxisY.StripLines.Add(stripLine);
        }

        private void ShowChartNoDataMessage() => ShowChartMessage("Tidak ada data");

        private void ShowChartMessage(string message)
        {
            var annotation = new TextAnnotation
            {
                Text = message,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Gray,
                Alignment = ContentAlignment.MiddleCenter
            };
            annotation.AnchorX = 50;
            annotation.AnchorY = 50;
            annotation.AnchorAlignment = ContentAlignment.MiddleCenter;
            annotation.AxisX = OmzetChart.ChartAreas["MainArea"].AxisX;
            annotation.AxisY = OmzetChart.ChartAreas["MainArea"].AxisY;
            OmzetChart.Annotations.Add(annotation);
        }

        private static Color GetChartSliceColor(string label)
        {
            if (label == SalesOmzetChartSummaryBuilder.LabelOutstanding)
                return Color.MistyRose;
            if (label == SalesOmzetChartSummaryBuilder.LabelPendingOmzet)
                return Color.LightGoldenrodYellow;
            if (label == SalesOmzetChartSummaryBuilder.LabelDirectSale)
                return Color.PowderBlue;
            return Color.PaleGreen;
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
