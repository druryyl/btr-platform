using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetChartForm : Form
    {
        private readonly ISalesOmzetChartSummaryBuilder _chartSummaryBuilder;
        private readonly ISalesOmzetTargetResolver _targetResolver;
        private ISalesOmzetChartHost _host;

        private enum OmzetChartDisplayMode
        {
            Status,
            Weekly,
            ManagerComparison
        }

        public SalesOmzetChartForm(
            ISalesOmzetChartSummaryBuilder chartSummaryBuilder,
            ISalesOmzetTargetResolver targetResolver)
        {
            InitializeComponent();
            _chartSummaryBuilder = chartSummaryBuilder;
            _targetResolver = targetResolver;

            ChartModeCombo.SelectedIndex = 0;
            ChartModeCombo.SelectedIndexChanged += ChartModeCombo_SelectedIndexChanged;
            OmzetChart.MouseClick += OmzetChart_MouseClick;

            InitOmzetChart();
        }

        public void BindHost(ISalesOmzetChartHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            RefreshChartAndKpi();
        }

        /// <summary>Manager / pusat view: comparison chart by default.</summary>
        public void ConfigureAsManagerView()
        {
            Text = "RO2 - Sales Omzet Grafik (Pusat)";
            SelectChartMode(OmzetChartDisplayMode.ManagerComparison);
        }

        private void ChartModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshChartAndKpi();
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
            else
                RefreshChartAndKpi();
        }

        private bool IsSearchEmpty() => string.IsNullOrWhiteSpace(_host?.SearchKeyword);

        private IEnumerable<SalesOmzetView> RowsForManagerComparison()
        {
            var full = _host?.FullRows;
            if (full != null && full.Count > 0)
                return full;
            return _host?.FilteredRows ?? Array.Empty<SalesOmzetView>();
        }

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
                Text = "",
                Docking = Docking.Top,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.DimGray
            });
        }

        private void RefreshChartAndKpi()
        {
            if (_host == null)
                return;

            var mode = _host.PeriodMode;
            var periode = _host.ReportPeriode;
            var chartMode = CurrentChartDisplayMode();
            var filteredRows = _host.FilteredRows ?? Array.Empty<SalesOmzetView>();
            var kpiRows = chartMode == OmzetChartDisplayMode.ManagerComparison && IsSearchEmpty()
                ? RowsForManagerComparison().ToList()
                : filteredRows.ToList();

            var targetAmount = _targetResolver.ResolveTarget(
                kpiRows,
                _host.SearchKeyword,
                periode,
                _host.GetCurrentUserDisplayName());

            var summary = _chartSummaryBuilder.Build(
                kpiRows,
                periode,
                mode,
                targetAmount);

            var managerComparison = chartMode == OmzetChartDisplayMode.ManagerComparison
                ? _chartSummaryBuilder.BuildManagerComparison(RowsForManagerComparison().ToList())
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
                OmzetChart.Titles[0].Text = GetChartTitle(chartMode);

            if (OmzetChart.Titles.Count > 1)
                OmzetChart.Titles[1].Text = GetChartSubtitle(mode, chartMode);

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

            decimal cumulative = 0;
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
            if (_host == null ||
                CurrentChartDisplayMode() != OmzetChartDisplayMode.ManagerComparison ||
                !IsSearchEmpty())
                return;

            var result = OmzetChart.HitTest(e.X, e.Y);
            if (result.ChartElementType != ChartElementType.DataPoint)
                return;

            var salesPersonName = result.Series.Points[result.PointIndex].AxisLabel;
            if (string.IsNullOrWhiteSpace(salesPersonName))
                return;

            _host.ApplySalesPersonFilter(salesPersonName);
            DialogResult = DialogResult.OK;
            Close();
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
    }
}
