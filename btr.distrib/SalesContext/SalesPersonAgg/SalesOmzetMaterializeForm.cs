using btr.application.SalesContext.SalesOmzetAgg.UseCases;
using btr.distrib.SharedForm;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetMaterializeForm : Form
    {
        private readonly IReconcileSalesOmzetWorker _reconcileWorker;

        public SalesOmzetMaterializeForm(IReconcileSalesOmzetWorker reconcileWorker)
        {
            InitializeComponent();
            _reconcileWorker = reconcileWorker;

            FullRebuildButton.Click += FullRebuildButton_Click;
        }

        private async void FullRebuildButton_Click(object sender, EventArgs e)
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

            await RunReconcileAsync(ReconcileSalesOmzetScope.Full);
            MessageBox.Show(
                "Rebuild selesai. Tutup dialog ini lalu jalankan Proses di laporan untuk periode yang diinginkan.",
                "Rebuild Omzet",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async Task RunReconcileAsync(ReconcileSalesOmzetScope scope, Periode periode = null)
        {
            if (periode is null)
            {
                var end = DateTime.Today;
                periode = new Periode(end.AddYears(-20), end);
            }

            var userId = string.Empty;
            var ownerForm = Owner as Form;
            if (ownerForm?.MdiParent is MainForm mainForm && mainForm.UserId != null)
                userId = mainForm.UserId.UserId;

            var request = new ReconcileSalesOmzetRequest
            {
                Periode = periode,
                Scope = scope,
                UserId = userId,
                Progress = new Progress<ReconcileSalesOmzetProgress>(UpdateProgress)
            };

            var previousCursor = Cursor;
            try
            {
                Cursor = Cursors.WaitCursor;
                SetReconcileUiBusy(true);
                BeginProgress();

                await Task.Run(() => _reconcileWorker.Execute(request));

                ShowReconcileStatus(request.Result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Rebuild Omzet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                EndProgress();
                SetReconcileUiBusy(false);
                Cursor = previousCursor;
            }
        }

        private void SetReconcileUiBusy(bool busy)
        {
            FullRebuildButton.Enabled = !busy;
        }

        private void BeginProgress()
        {
            ProgressBar.Visible = true;
            ProgressBar.Style = ProgressBarStyle.Continuous;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = 100;
            ProgressBar.Value = 0;
            StatusLabel.Text = "Memulai rebuild...";
            ProgressBar.Refresh();
        }

        private void EndProgress()
        {
            ProgressBar.Visible = false;
            ProgressBar.Value = 0;
        }

        private void UpdateProgress(ReconcileSalesOmzetProgress progress)
        {
            if (progress == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgress(progress)));
                return;
            }

            if (!string.IsNullOrEmpty(progress.Phase))
                StatusLabel.Text = progress.Phase;

            if (progress.Total <= 0)
            {
                ProgressBar.Style = ProgressBarStyle.Marquee;
                ProgressBar.Refresh();
                return;
            }

            ProgressBar.Style = ProgressBarStyle.Continuous;
            if (ProgressBar.Maximum != progress.Total)
                ProgressBar.Maximum = progress.Total;

            var value = Math.Min(Math.Max(progress.Current, ProgressBar.Minimum), ProgressBar.Maximum);
            if (ProgressBar.Value != value)
                ProgressBar.Value = value;

            ProgressBar.Refresh();
        }

        private void ShowReconcileStatus(ReconcileSalesOmzetResult result)
        {
            if (result is null)
            {
                StatusLabel.Text = string.Empty;
                return;
            }

            var seconds = result.Duration.TotalSeconds;
            StatusLabel.Text =
                $"Rebuild ({result.Scope}): {result.OrdersProcessed} order, {result.FaktursProcessed} faktur, " +
                $"{result.RowsRefreshed} baris ({seconds:0.#} d)";
        }
    }
}
