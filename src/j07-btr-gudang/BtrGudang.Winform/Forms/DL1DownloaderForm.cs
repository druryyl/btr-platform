using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Infrastructure.PackingOrderFeature;
using BtrGudang.Winform.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PackingOrderDownloader
{
    public partial class DL1DownloaderForm : Form
    {
        // Timers
        private System.Windows.Forms.Timer _downloadTimer;
        private System.Windows.Forms.Timer _clockTimer;
        private RegistryHelper _registryHelper;
        private IPackingOrderRepo _packingOrderRepo;

        // Service and state management
        private DateTime _nextScheduledExecution;
        private int _isExecuting = 0; // Using int for Interlocked operations
        private const int DOWNLOAD_INTERVAL_MILLISECONDS = 5 * 60 * 1000; // 5 minutes
        private readonly string _depoId;
        private DateTime _lastTimestamp;

        private readonly PackingOrderDownloaderSvc _packingOrderDownloaderSvc;


        public DL1DownloaderForm(PackingOrderDownloaderSvc packingOrderDownloaderSvc, 
            IPackingOrderRepo packingOrderRepo)
        {
            _registryHelper = new RegistryHelper();
            _depoId = _registryHelper.ReadString("DepoId");

            _packingOrderDownloaderSvc = packingOrderDownloaderSvc;
            _packingOrderRepo = packingOrderRepo;

            InitializeComponent();
            InitializeTimers();
            LoadLastTimestamp();
            ScheduleNextExecution();


            // Log initial message
            LogMessage("Application started successfully", LogLevel.Info);
            LogMessage($"Automatic download interval: {DOWNLOAD_INTERVAL_MILLISECONDS / 1000 / 60} minutes", LogLevel.Info);
            _packingOrderRepo = packingOrderRepo;
        }

        private void LoadLastTimestamp()
        {
            var defaultLast = DateTime.Now.Date.AddDays(-3);
            var formatDt = "yyyy-MM-dd HH:mm:ss";
            var lastTimestampStr = _registryHelper.ReadString("LastDownloadPackingOrderTimestamp", defaultLast.ToString(formatDt));
            var isValidDate = DateTime.TryParseExact(lastTimestampStr, formatDt,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastTimestampDt);
            _lastTimestamp = isValidDate ? lastTimestampDt : defaultLast;
        }
        private void RememberLastTimestamp(DateTime lastTimestamp)
        {
            var lastTimestampStr = lastTimestamp.ToString("yyyy-MM-dd HH:mm:ss");
            _registryHelper.WriteString("LastDownloadPackingOrderTimestamp", lastTimestampStr);
        }

        private void InitializeTimers()
        {
            // Clock timer - updates every second
            _clockTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1 second
            };
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();

            // Download timer - triggers every 5 minutes
            _downloadTimer = new System.Windows.Forms.Timer
            {
                Interval = DOWNLOAD_INTERVAL_MILLISECONDS
            };
            _downloadTimer.Tick += DownloadTimer_Tick;
            _downloadTimer.Start();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            _clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async void DownloadTimer_Tick(object sender, EventArgs e)
        {
            await ExecuteDownloadAsync(isManual: false);
        }

        private async void ManualDownloadButton_Click(object sender, EventArgs e)
        {
            await ExecuteDownloadAsync(isManual: true);
        }

        private async Task ExecuteDownloadAsync(bool isManual)
        {
            // Prevent overlapping executions using thread-safe approach
            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 1)
            {
                LogMessage("Download already in progress, skipping execution", LogLevel.Warning);
                return;
            }

            try
            {
                // Update UI state
                UpdateUIState(isExecuting: true);

                string executionType = isManual ? "Manual" : "Scheduled";
                LogMessage($"Download Started...", LogLevel.Info);

                // Execute the service call
                var (success, message, lastTimestamp, orders) = await _packingOrderDownloaderSvc.Execute(_lastTimestamp, _depoId, 100);
                if (lastTimestamp.Date.ToString("yyyy-MM-dd") == "3000-01-01")
                {
                    var packingOrd = _packingOrderRepo.GetLastDownloadByTimestamp();
                    if (packingOrd != null)
                    {
                        lastTimestamp = packingOrd.DownloadTimestamp;
                    }
                    else
                    {
                        lastTimestamp = DateTime.Now.Date.AddDays(-3);
                    }
                }
                _lastTimestamp = GetNextSecond(lastTimestamp);

                // Log results
                if (success)
                {
                    SavePackingOrder(orders);
                    LogMessage($"SUCCESS {message}", LogLevel.Success);
                    LogMessage($"Records retrieved: {orders.Count()}", LogLevel.Info);
                }
                else
                {
                    LogMessage($"[ERROR] {message}", LogLevel.Error);
                }

                // Schedule next execution (only for non-manual)
                if (!isManual)
                {
                    ScheduleNextExecution();
                }
                
                LogMessage($"Next download: {_nextScheduledExecution:HH:mm:ss}", LogLevel.Info);
                RememberLastTimestamp(_lastTimestamp);
            }
            catch (Exception ex)
            {
                LogMessage($"[EXCEPTION] Unhandled error: {ex.Message}", LogLevel.Error);
                LogMessage($"Stack Trace: {ex.StackTrace}", LogLevel.Error);
            }
            finally
            {
                // Release execution lock
                Interlocked.Exchange(ref _isExecuting, 0);
                UpdateUIState(isExecuting: false);
            }
        }

        private void SavePackingOrder(IEnumerable<PackingOrderModel> listPackingOrder)
        {
            foreach(var packingOrder in listPackingOrder)
                _packingOrderRepo.SaveChanges(packingOrder);
        }

        static DateTime GetNextSecond(DateTime dt)
        {
            return dt.AddSeconds(1).AddTicks(-(dt.AddSeconds(1).Ticks % TimeSpan.TicksPerSecond));
        }
        private void ScheduleNextExecution()
        {
            _nextScheduledExecution = DateTime.Now.AddMilliseconds(DOWNLOAD_INTERVAL_MILLISECONDS);
        }

        private void UpdateUIState(bool isExecuting)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(UpdateUIState), isExecuting);
                return;
            }

            _manualDownloadButton.Enabled = !isExecuting;
            _manualDownloadButton.Text = isExecuting ? "Downloading..." : "Download Now";
            _statusLabel.Text = isExecuting ? "Downloading data..." : "Ready";
        }

        private enum LogLevel
        {
            Info,
            Success,
            Warning,
            Error
        }

        private void LogMessage(string message, LogLevel level)
        {
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action<string, LogLevel>(LogMessage), message, level);
                return;
            }

            // Determine color based on log level
            Color color;
            switch (level)
            {
                case LogLevel.Success:
                    color = Color.LimeGreen;
                    break;
                case LogLevel.Warning:
                    color = Color.Orange;
                    break;
                case LogLevel.Error:
                    color = Color.Red;
                    break;
                case LogLevel.Info:
                default:
                    color = Color.FromArgb(220, 220, 220);
                    break;
            }

            // Append message with color
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.SelectionLength = 0;
            _logTextBox.SelectionColor = color;
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            _logTextBox.SelectionColor = _logTextBox.ForeColor;

            // Auto-scroll to bottom
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _downloadTimer?.Stop();
            _downloadTimer?.Dispose();
            _clockTimer?.Stop();
            _clockTimer?.Dispose();

            LogMessage("Application shutting down...", LogLevel.Info);
            
            base.OnFormClosing(e);
        }
    }
}
