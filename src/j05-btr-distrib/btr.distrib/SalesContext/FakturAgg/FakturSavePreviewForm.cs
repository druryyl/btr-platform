using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.FakturAgg
{
    public enum FakturPreviewChoice
    {
        Cancel,
        Save,
        SaveAndPrint
    }

    /// <summary>
    /// Faktur-specific preview-before-save dialog. Displays the draft Faktur
    /// report and lets the operator choose SAVE, SAVE &amp; PRINT, or Cancel.
    /// It only returns the choice; FakturForm owns all persistence and printing.
    /// </summary>
    public partial class FakturSavePreviewForm : Form
    {
        private string _reportName;
        private List<ReportDataSource> _listDatasource;
        private bool _isLandscape;
        private PaperSize _currentPaperSize;
        private ToolStripButton _paperSizeButton;

        /// <summary>
        /// Preview dialog outcome. Defaults to Cancel so closing the dialog
        /// persists nothing.
        /// </summary>
        public FakturPreviewChoice PreviewChoice { get; private set; } = FakturPreviewChoice.Cancel;

        public FakturSavePreviewForm()
        {
            InitializeComponent();
            TheViewer.ShowPrintButton = false;
            _currentPaperSize = RdlcViewerForm.GetLastPaperSize();
            AddCustomToolbarButton();
            SaveButton.Click += SaveButton_Click;
            SavePrintButton.Click += SavePrintButton_Click;
            CancelPreviewButton.Click += CancelPreviewButton_Click;
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
            _currentPaperSize = RdlcViewerForm.LetterPaperSize;
            RdlcViewerForm.SetLastPaperSize(_currentPaperSize);
            LoadReport();
        }

        public void SwitchToHalfLetter()
        {
            _currentPaperSize = RdlcViewerForm.HalfLetterPaperSize;
            RdlcViewerForm.SetLastPaperSize(_currentPaperSize);
            LoadReport();
        }

        public void TogglePaperSize()
        {
            if (_currentPaperSize == RdlcViewerForm.LetterPaperSize)
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
            return _currentPaperSize == RdlcViewerForm.LetterPaperSize ? "Letter" : "Half Letter";
        }

        /// <summary>
        /// Prints the finalized Faktur after SAVE &amp; PRINT with a printer settings
        /// dialog shown first, allowing the operator to select printer destination,
        /// copies, and properties before printing. Cancelling the dialog aborts
        /// printing without affecting the already-saved Faktur.
        /// </summary>
        public static void PrintWithDialog(string reportName, List<ReportDataSource> listDatasource, bool isLandscape = false)
        {
            var reportFileName = $"{reportName}.rdlc";
            var reportFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportFileName);
            if (!File.Exists(reportFileFullPath))
                throw new FileNotFoundException($"Report template tidak ditemukan: {reportFileFullPath}");

            var paperSize = RdlcViewerForm.GetLastPaperSize();
            var margins = new Margins(25, 25, 25, 25);

            var pageWidthInch = paperSize.Width / 100.0;
            var pageHeightInch = paperSize.Height / 100.0;
            if (isLandscape)
            {
                var tmp = pageWidthInch;
                pageWidthInch = pageHeightInch;
                pageHeightInch = tmp;
            }

            var deviceInfo = new StringBuilder();
            deviceInfo.Append("<DeviceInfo>");
            deviceInfo.Append("<OutputFormat>EMF</OutputFormat>");
            deviceInfo.Append($"<PageWidth>{pageWidthInch.ToString(System.Globalization.CultureInfo.InvariantCulture)}in</PageWidth>");
            deviceInfo.Append($"<PageHeight>{pageHeightInch.ToString(System.Globalization.CultureInfo.InvariantCulture)}in</PageHeight>");
            deviceInfo.Append("<MarginTop>0.25in</MarginTop>");
            deviceInfo.Append("<MarginLeft>0.25in</MarginLeft>");
            deviceInfo.Append("<MarginRight>0.25in</MarginRight>");
            deviceInfo.Append("<MarginBottom>0.25in</MarginBottom>");
            deviceInfo.Append("</DeviceInfo>");

            var streams = new List<Stream>();
            using (var report = new LocalReport())
            {
                report.ReportPath = reportFileFullPath;
                foreach (var ds in listDatasource)
                    report.DataSources.Add(new ReportDataSource(ds.Name, ds.Value));

                Warning[] warnings;
                report.Render("Image", deviceInfo.ToString(),
                    (name, fileNameExtension, encoding, mimeType, willSeek) =>
                    {
                        var stream = new MemoryStream();
                        streams.Add(stream);
                        return stream;
                    },
                    out warnings);

                foreach (var stream in streams)
                    stream.Position = 0;
            }

            if (streams.Count == 0)
                return;

            try
            {
                using (var printDoc = new PrintDocument())
                {
                    printDoc.DefaultPageSettings.PaperSize = paperSize;
                    printDoc.DefaultPageSettings.Margins = margins;
                    printDoc.DefaultPageSettings.Landscape = isLandscape;

                    using (var printDialog = new PrintDialog())
                    {
                        printDialog.Document = printDoc;
                        printDialog.UseEXDialog = true;
                        if (printDialog.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }

                    if (!printDoc.PrinterSettings.IsValid)
                        throw new InvalidOperationException("Printer yang dipilih tidak valid.");

                    var pageIndex = 0;
                    printDoc.PrintPage += (sender, e) =>
                    {
                        using (var pageImage = new Metafile(streams[pageIndex]))
                        {
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        }
                        pageIndex++;
                        e.HasMorePages = pageIndex < streams.Count;
                    };
                    printDoc.Print();
                }
            }
            finally
            {
                foreach (var stream in streams)
                    stream.Dispose();
            }
        }
    }
}
