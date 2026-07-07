using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Winform.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    public partial class DL3PendingDownloaderForm : Form
    {
        private const int BatchSize = 100;

        private readonly RegistryHelper _registryHelper;
        private readonly IPackingOrderRepo _packingOrderRepo;
        private readonly PackingOrderDownloaderSvc _packingOrderDownloaderSvc;
        private readonly string _depoId;
        private int _isExecuting;

        public DL3PendingDownloaderForm(
            PackingOrderDownloaderSvc packingOrderDownloaderSvc,
            IPackingOrderRepo packingOrderRepo)
        {
            _registryHelper = new RegistryHelper();
            _depoId = _registryHelper.ReadString("DepoId");
            _packingOrderDownloaderSvc = packingOrderDownloaderSvc;
            _packingOrderRepo = packingOrderRepo;

            InitializeComponent();
            LogMessage("Pending Faktur recovery ready", LogLevel.Info);
        }

        private async void DownloadPendingButton_Click(object sender, EventArgs e)
        {
            await ExecuteRecoveryAsync();
        }

        private async Task ExecuteRecoveryAsync()
        {
            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 1)
            {
                LogMessage("Recovery already in progress, skipping execution", LogLevel.Warning);
                return;
            }

            var sw = Stopwatch.StartNew();
            var totalReceived = 0;
            var imported = 0;
            var skipped = 0;
            var failed = 0;

            try
            {
                UpdateUIState(isExecuting: true);
                LogMessage("Recovery started", LogLevel.Info);

                while (true)
                {
                    var (success, message, orders) = await _packingOrderDownloaderSvc.ExecutePending(_depoId, BatchSize);
                    if (!success)
                    {
                        LogMessage($"[ERROR] {message}", LogLevel.Error);
                        break;
                    }

                    var batch = orders?.ToList() ?? new System.Collections.Generic.List<PackingOrderModel>();
                    if (batch.Count == 0)
                    {
                        break;
                    }

                    totalReceived += batch.Count;
                    LogMessage($"Batch received: {batch.Count} pending faktur", LogLevel.Info);

                    foreach (var order in batch)
                    {
                        try
                        {
                            var exists = _packingOrderRepo.LoadEntity(order).HasValue;
                            _packingOrderRepo.SaveChanges(order);
                            if (exists)
                            {
                                skipped++;
                            }
                            else
                            {
                                imported++;
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            var fakturCode = order.Faktur.FakturCode;
                            LogMessage($"[ERROR] Failed {fakturCode}: {ex.Message}", LogLevel.Error);
                        }
                    }
                }

                sw.Stop();
                LogMessage(
                    $"Recovery completed in {sw.Elapsed.TotalSeconds:F1}s — received: {totalReceived}, imported: {imported}, skipped: {skipped}, failed: {failed}",
                    failed > 0 ? LogLevel.Warning : LogLevel.Success);
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogMessage($"[EXCEPTION] Unhandled error: {ex.Message}", LogLevel.Error);
                LogMessage($"Stack Trace: {ex.StackTrace}", LogLevel.Error);
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0);
                UpdateUIState(isExecuting: false);
            }
        }

        private void UpdateUIState(bool isExecuting)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(UpdateUIState), isExecuting);
                return;
            }

            _downloadPendingButton.Enabled = !isExecuting;
            _downloadPendingButton.Text = isExecuting ? "Downloading..." : "Download Pending Faktur";
            _statusLabel.Text = isExecuting ? "Recovering pending faktur..." : "Ready";
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

            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.SelectionLength = 0;
            _logTextBox.SelectionColor = color;
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            _logTextBox.SelectionColor = _logTextBox.ForeColor;
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.ScrollToCaret();
        }
    }
}
