using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.FakturAgg
{
    public enum FakturPreviewChoice
    {
        Cancel,
        Save,
        SaveAndPrint
    }

    public partial class RdlcViewerForm : Form
    {
        private string _reportName;
        private List<ReportDataSource> _listDatasource;
        private bool _isLandscape;
        private PaperSize _currentPaperSize;
        private ToolStripButton _paperSizeButton;

        /// <summary>
        /// Preview dialog outcome (SL-04, D-003/D-005).
        /// Defaults to Cancel so closing the dialog persists nothing.
        /// </summary>
        public FakturPreviewChoice PreviewChoice { get; private set; } = FakturPreviewChoice.Cancel;

        // Define standard paper sizes
        private readonly PaperSize _letterSize = new PaperSize("Letter", 850, 1100);
        private readonly PaperSize _halfLetterSize = new PaperSize("Half Letter", 850, 550);

        public RdlcViewerForm()
        {
            InitializeComponent();
            TheViewer.Print += TheViewer_Print;
            _currentPaperSize = _letterSize;
            AddCustomToolbarButton();
            SaveButton.Click += SaveButton_Click;
            SavePrintButton.Click += SavePrintButton_Click;
            CancelPreviewButton.Click += CancelPreviewButton_Click;
        }

        /// <summary>
        /// Opt-in SAVE / SAVE &amp; PRINT / Cancel actions for the Faktur
        /// preview-before-save dialog (SL-04, D-003/D-005).
        /// Other RdlcViewerForm callers are unaffected (panel stays hidden).
        /// </summary>
        public void EnableSaveConfirm()
        {
            PanelBottom.Visible = true;
            PreviewChoice = FakturPreviewChoice.Cancel;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            PreviewChoice = FakturPreviewChoice.Save;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SavePrintButton_Click(object sender, EventArgs e)
        {
            PreviewChoice = FakturPreviewChoice.SaveAndPrint;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelPreviewButton_Click(object sender, EventArgs e)
        {
            PreviewChoice = FakturPreviewChoice.Cancel;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddCustomToolbarButton()
        {
            // Access the built-in toolbar
            if (TheViewer.Controls.Find("ToolStrip1", true).Length > 0)
            {
                ToolStrip toolStrip = (ToolStrip)TheViewer.Controls.Find("ToolStrip1", true)[0];

                // Add separator
                toolStrip.Items.Add(new ToolStripSeparator());

                // Add custom paper size button
                _paperSizeButton = new ToolStripButton("Paper: Letter");
                _paperSizeButton.ToolTipText = "Click to switch paper size";
                _paperSizeButton.Click += PaperSizeButton_Click;
                toolStrip.Items.Add(_paperSizeButton);
            }
        }

        private void PaperSizeButton_Click(object sender, EventArgs e)
        {
            TogglePaperSize();
        }

        private void TheViewer_Print(object sender, ReportPrintEventArgs e)
        {
            //this.Close();
        }

        public void SetReportData(string reportName, List<ReportDataSource> listDatasource, bool isLandscape = false)
        {
            _reportName = reportName;
            _listDatasource = new List<ReportDataSource>(listDatasource);
            _isLandscape = isLandscape;

            LoadReport();
        }

        private void LoadReport()
        {
            var reportFileName = $"{_reportName}.rdlc";
            var reportFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportFileName);

            TheViewer.LocalReport.DataSources.Clear();
            _listDatasource.ForEach(x => TheViewer.LocalReport.DataSources.Add(x));
            TheViewer.LocalReport.ReportPath = reportFileFullPath;

            TheViewer.ProcessingMode = ProcessingMode.Local;
            TheViewer.SetDisplayMode(DisplayMode.PrintLayout);
            TheViewer.ZoomMode = ZoomMode.PageWidth;
            TheViewer.ZoomMode = ZoomMode.Percent;
            TheViewer.ZoomPercent = 100;

            PageSettings pageSettings = new PageSettings
            {
                Margins = new Margins(25, 25, 25, 25),
                PaperSize = _currentPaperSize
            };
            pageSettings.Landscape = _isLandscape;
            TheViewer.SetPageSettings(pageSettings);

            TheViewer.RefreshReport();

            // Update button text
            UpdatePaperSizeButtonText();
        }

        public void SwitchToLetter()
        {
            _currentPaperSize = _letterSize;
            LoadReport();
        }

        public void SwitchToHalfLetter()
        {
            _currentPaperSize = _halfLetterSize;
            LoadReport();
        }

        public void TogglePaperSize()
        {
            if (_currentPaperSize == _letterSize)
            {
                SwitchToHalfLetter();
            }
            else
            {
                SwitchToLetter();
            }
        }

        private void UpdatePaperSizeButtonText()
        {
            if (_paperSizeButton != null)
            {
                _paperSizeButton.Text = $"Paper: {GetCurrentPaperSizeName()}";
            }
        }

        public string GetCurrentPaperSizeName()
        {
            return _currentPaperSize == _letterSize ? "Letter" : "Half Letter";
        }
    }
}
