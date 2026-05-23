using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.UseCases;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.SalesPersonAgg
{
    public partial class SalesOmzetHealthWeeklyForm : Form
    {
        private readonly IGenerateSalesOmzetHealthWeeklyWorker _worker;
        private readonly IIsoWeekCalendar _isoWeekCalendar;

        public SalesOmzetHealthWeeklyForm(
            IGenerateSalesOmzetHealthWeeklyWorker worker,
            IIsoWeekCalendar isoWeekCalendar)
        {
            InitializeComponent();
            _worker = worker;
            _isoWeekCalendar = isoWeekCalendar;

            YearNumeric.Value = DateTime.Today.Year;
            WeekNumeric.Value = _isoWeekCalendar.GetIsoWeek(DateTime.Today).WeekNumber;

            YearNumeric.ValueChanged += (_, __) => UpdateWeekRangeLabel();
            WeekNumeric.ValueChanged += (_, __) => UpdateWeekRangeLabel();
            ProsesButton.Click += ProsesButton_Click;

            UpdateWeekRangeLabel();
        }

        private void UpdateWeekRangeLabel()
        {
            try
            {
                var year = (int)YearNumeric.Value;
                var week = (int)WeekNumeric.Value;
                var (start, end) = _isoWeekCalendar.GetWeekBounds(year, week);
                WeekRangeLabel.Text = $"{start:dd MMM yyyy} – {end:dd MMM yyyy}";
            }
            catch (Exception ex)
            {
                WeekRangeLabel.Text = ex.Message;
            }
        }

        private async void ProsesButton_Click(object sender, EventArgs e)
        {
            var year = (int)YearNumeric.Value;
            var week = (int)WeekNumeric.Value;

            if (week < 1 || week > 53)
            {
                MessageBox.Show("Nomor minggu ISO harus antara 1 dan 53.");
                return;
            }

            var request = new GenerateSalesOmzetHealthWeeklyRequest
            {
                YearNumber = year,
                WeekNumber = week
            };

            var previousCursor = Cursor;
            try
            {
                Cursor = Cursors.WaitCursor;
                ProsesButton.Enabled = false;
                StatusLabel.Text = "Menghitung indikator mingguan...";

                await Task.Run(() => _worker.Execute(request));

                var r = request.Result;
                if (r is null)
                {
                    StatusLabel.Text = "Tidak ada hasil.";
                    return;
                }

                StatusLabel.Text =
                    $"Minggu {week}/{year}: {r.HealthLevel} (skor {r.HealthScore}) • " +
                    $"order hilang {r.MissingOrdersCount}, faktur hilang {r.MissingFaktursCount}, " +
                    $"belum link {r.UnlinkedFaktursCount}, stale {r.StaleDataCount} • {r.CalculationDurationMs} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung indikator: " + ex.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                StatusLabel.Text = "Gagal.";
            }
            finally
            {
                ProsesButton.Enabled = true;
                Cursor = previousCursor;
            }
        }
    }
}
