using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.FakturAgg
{
    public partial class RdlcViewerForm : Form
    {
        private string _reportName;
        private List<ReportDataSource> _listDatasource;
        private bool _isLandscape;
        private PaperSize _currentPaperSize;
        private ToolStripButton _paperSizeButton;

        // Define standard paper sizes
        private static readonly PaperSize _letterSize = new PaperSize("Letter", 850, 1100);
        private static readonly PaperSize _halfLetterSize = new PaperSize("Half Letter", 850, 550);

        //  Last paper choice shared across preview dialogs and silent
        //  SAVE & PRINT, so direct print honors the viewer's Paper toggle.
        private static PaperSize _lastPaperSize = _letterSize;

        public RdlcViewerForm()
        {
            InitializeComponent();
            TheViewer.Print += TheViewer_Print;
            _currentPaperSize = _lastPaperSize;
            AddCustomToolbarButton();
        }

        //  Shared paper instances so other preview dialogs (Faktur save
        //  confirm) can reuse the same objects and comparisons.
        internal static PaperSize LetterPaperSize => _letterSize;
        internal static PaperSize HalfLetterPaperSize => _halfLetterSize;

        /// <summary>
        /// Last paper size chosen in any preview dialog. Shared so silent
        /// direct print (SAVE &amp; PRINT) honors the operator's selection.
        /// </summary>
        internal static PaperSize GetLastPaperSize()
        {
            return _lastPaperSize ?? _letterSize;
        }

        internal static void SetLastPaperSize(PaperSize paperSize)
        {
            _lastPaperSize = paperSize ?? _letterSize;
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
            _lastPaperSize = _letterSize;
            LoadReport();
        }

        public void SwitchToHalfLetter()
        {
            _currentPaperSize = _halfLetterSize;
            _lastPaperSize = _halfLetterSize;
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

        /// <summary>
        /// Silent direct print to the default printer with no preview window.
        /// Uses the last Paper toggle choice (Letter / Half Letter) and the
        /// same margins/landscape conventions as the viewer.
        /// </summary>
        public static void PrintDirect(string reportName, List<ReportDataSource> listDatasource, bool isLandscape = false)
        {
            var reportFileName = $"{reportName}.rdlc";
            var reportFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportFileName);
            if (!File.Exists(reportFileFullPath))
                throw new FileNotFoundException($"Report template tidak ditemukan: {reportFileFullPath}");

            var paperSize = _lastPaperSize ?? _letterSize;
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
                    if (!printDoc.PrinterSettings.IsValid)
                        throw new InvalidOperationException("Default printer tidak valid.");

                    printDoc.DefaultPageSettings.PaperSize = paperSize;
                    printDoc.DefaultPageSettings.Margins = margins;
                    printDoc.DefaultPageSettings.Landscape = isLandscape;

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
