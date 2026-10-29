using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.distrib.Browsers;
using btr.distrib.Helpers;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.distrib.SharedForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.VisitPlanAgg
{
    public partial class JadwalKunjunganForm : Form
    {
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly ICustomerDal _customerDal;
        private readonly IVisitPlanDal _visitPlanDal;
        private readonly IVisitPlanExceptionDal _exceptionDal;
        private readonly IEffectiveVisitPlanDal _effectiveVisitPlanDal;
        private readonly IVisitPlanExceptionWriter _exceptionWriter;
        private readonly IRuteCycleCalendar _calendar;
        private readonly IRegenerateVisitPlanWorker _regenerateWorker;

        private readonly BindingList<VisitPlanModel> _basePlanRows;
        private readonly BindingList<EffectiveVisitPlanEntry> _effectivePlanRows;
        private readonly BindingList<VisitPlanExceptionModel> _exceptionRows;
        private readonly BindingSource _basePlanBindingSource;
        private readonly BindingSource _effectivePlanBindingSource;
        private readonly BindingSource _exceptionBindingSource;

        public JadwalKunjunganForm(
            ISalesPersonDal salesPersonDal,
            ICustomerDal customerDal,
            IVisitPlanDal visitPlanDal,
            IVisitPlanExceptionDal exceptionDal,
            IEffectiveVisitPlanDal effectiveVisitPlanDal,
            IVisitPlanExceptionWriter exceptionWriter,
            IRuteCycleCalendar calendar,
            IRegenerateVisitPlanWorker regenerateWorker)
        {
            InitializeComponent();

            _salesPersonDal = salesPersonDal;
            _customerDal = customerDal;
            _visitPlanDal = visitPlanDal;
            _exceptionDal = exceptionDal;
            _effectiveVisitPlanDal = effectiveVisitPlanDal;
            _exceptionWriter = exceptionWriter;
            _calendar = calendar;
            _regenerateWorker = regenerateWorker;

            _basePlanRows = new BindingList<VisitPlanModel>();
            _effectivePlanRows = new BindingList<EffectiveVisitPlanEntry>();
            _exceptionRows = new BindingList<VisitPlanExceptionModel>();
            _basePlanBindingSource = new BindingSource(_basePlanRows, null);
            _effectivePlanBindingSource = new BindingSource(_effectivePlanRows, null);
            _exceptionBindingSource = new BindingSource(_exceptionRows, null);

            RegisterEventHandlers();
            InitSalesComboBox();
            InitGrids();

            VisitDatePicker.MinDate = DateTime.Today;
            VisitDatePicker.Value = DateTime.Today;
            RefreshPlans();
        }

        private void RegisterEventHandlers()
        {
            RefreshButton.Click += (_, __) => RefreshPlans();
            MaterializeButton.Click += MaterializeButton_Click;
            SalesComboBox.SelectedIndexChanged += (_, __) => RefreshPlans();
            VisitDatePicker.ValueChanged += (_, __) => RefreshPlans();

            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            ReplaceButton.Click += ReplaceButton_Click;
            DeleteExceptionButton.Click += DeleteExceptionButton_Click;

            ToggleDrawerButton.Click += (_, __) => SetDrawerVisibility(MainSplit.Panel2Collapsed);
            CloseDrawerButton.Click += (_, __) => SetDrawerVisibility(false);

            MenuTambahCustomer.Click += AddButton_Click;
            MenuGantiCustomer.Click += ReplaceButton_Click;
            MenuHapusCustomer.Click += RemoveButton_Click;

            EffectivePlanGrid.CellFormatting += EffectivePlanGrid_CellFormatting;
            EffectivePlanGrid.CellDoubleClick += (_, __) => ReplaceButton_Click(null, null);
            EffectivePlanGrid.KeyDown += EffectivePlanGrid_KeyDown;

            BasePlanGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
            EffectivePlanGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
            ExceptionGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
        }

        private void InitSalesComboBox()
        {
            var listSales = (_salesPersonDal.ListData() ?? Enumerable.Empty<SalesPersonModel>())
                .OrderBy(x => x.SalesPersonName)
                .ToList();
            SalesComboBox.DataSource = listSales;
            SalesComboBox.DisplayMember = "SalesPersonName";
            SalesComboBox.ValueMember = "SalesPersonId";
            SalesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void InitGrids()
        {
            BasePlanGrid.DataSource = _basePlanBindingSource;
            EffectivePlanGrid.DataSource = _effectivePlanBindingSource;
            ExceptionGrid.DataSource = _exceptionBindingSource;

            BasePlanGrid.AutoGenerateColumns = true;
            EffectivePlanGrid.AutoGenerateColumns = true;
            ExceptionGrid.AutoGenerateColumns = true;

            BasePlanGrid.DataBindingComplete += (_, __) => ConfigureBasePlanColumns();
            EffectivePlanGrid.DataBindingComplete += (_, __) => ConfigureEffectivePlanColumns();
            ExceptionGrid.DataBindingComplete += (_, __) => ConfigureExceptionColumns();
        }

        private void SetDrawerVisibility(bool visible)
        {
            MainSplit.Panel2Collapsed = !visible;
            if (visible)
            {
                MainSplit.SplitterDistance = Math.Max(200, MainSplit.Height - 220);
                ToggleDrawerButton.Text = "Tutup Riwayat ▲";
            }
            else
            {
                ToggleDrawerButton.Text = "Riwayat Exception ▼";
            }
        }

        private void ConfigureBasePlanColumns()
        {
            HideColumn(BasePlanGrid, "VisitPlanId");
            HideColumn(BasePlanGrid, "SalesPersonId");
            HideColumn(BasePlanGrid, "CustomerId");
            HideColumn(BasePlanGrid, "PlanSource");
            HideColumn(BasePlanGrid, "MaterializedAt");
            HideColumn(BasePlanGrid, "VisitDate");

            if (BasePlanGrid.Columns.Contains("NoUrut"))
            {
                BasePlanGrid.Columns["NoUrut"].HeaderText = "No";
                BasePlanGrid.Columns["NoUrut"].Width = 55;
                BasePlanGrid.Columns["NoUrut"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (BasePlanGrid.Columns.Contains("CustomerCode"))
            {
                BasePlanGrid.Columns["CustomerCode"].HeaderText = "Kode Cust";
                BasePlanGrid.Columns["CustomerCode"].Width = 110;
            }
            if (BasePlanGrid.Columns.Contains("CustomerName"))
            {
                BasePlanGrid.Columns["CustomerName"].HeaderText = "Nama Customer";
                BasePlanGrid.Columns["CustomerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            if (BasePlanGrid.Columns.Contains("HariRuteId"))
            {
                BasePlanGrid.Columns["HariRuteId"].HeaderText = "Hari Rute";
                BasePlanGrid.Columns["HariRuteId"].Width = 100;
            }
        }

        private void ConfigureEffectivePlanColumns()
        {
            HideColumn(EffectivePlanGrid, "CustomerId");

            if (EffectivePlanGrid.Columns.Contains("NoUrut"))
            {
                EffectivePlanGrid.Columns["NoUrut"].HeaderText = "No";
                EffectivePlanGrid.Columns["NoUrut"].Width = 55;
                EffectivePlanGrid.Columns["NoUrut"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (EffectivePlanGrid.Columns.Contains("CustomerCode"))
            {
                EffectivePlanGrid.Columns["CustomerCode"].HeaderText = "Kode Cust";
                EffectivePlanGrid.Columns["CustomerCode"].Width = 110;
            }
            if (EffectivePlanGrid.Columns.Contains("CustomerName"))
            {
                EffectivePlanGrid.Columns["CustomerName"].HeaderText = "Nama Customer";
                EffectivePlanGrid.Columns["CustomerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            if (EffectivePlanGrid.Columns.Contains("Origin"))
            {
                EffectivePlanGrid.Columns["Origin"].HeaderText = "Status / Asal";
                EffectivePlanGrid.Columns["Origin"].Width = 130;
                EffectivePlanGrid.Columns["Origin"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void ConfigureExceptionColumns()
        {
            HideColumn(ExceptionGrid, "VisitPlanExceptionId");
            HideColumn(ExceptionGrid, "SalesPersonId");
            HideColumn(ExceptionGrid, "CustomerId");
            HideColumn(ExceptionGrid, "ReplacementCustomerId");
            HideColumn(ExceptionGrid, "VisitDate");

            if (ExceptionGrid.Columns.Contains("ExceptionType"))
            {
                ExceptionGrid.Columns["ExceptionType"].HeaderText = "Tipe";
                ExceptionGrid.Columns["ExceptionType"].Width = 90;
            }
            if (ExceptionGrid.Columns.Contains("CustomerName"))
            {
                ExceptionGrid.Columns["CustomerName"].HeaderText = "Customer Terkait";
                ExceptionGrid.Columns["CustomerName"].Width = 220;
            }
            if (ExceptionGrid.Columns.Contains("ReplacementCustomerName"))
            {
                ExceptionGrid.Columns["ReplacementCustomerName"].HeaderText = "Customer Pengganti";
                ExceptionGrid.Columns["ReplacementCustomerName"].Width = 220;
            }
            if (ExceptionGrid.Columns.Contains("CreatedByUserId"))
            {
                ExceptionGrid.Columns["CreatedByUserId"].HeaderText = "Dibuat Oleh";
                ExceptionGrid.Columns["CreatedByUserId"].Width = 110;
            }
            if (ExceptionGrid.Columns.Contains("CreatedAt"))
            {
                ExceptionGrid.Columns["CreatedAt"].HeaderText = "Waktu";
                ExceptionGrid.Columns["CreatedAt"].Width = 140;
                ExceptionGrid.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }

        private void EffectivePlanGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= EffectivePlanGrid.Rows.Count)
                return;

            var row = EffectivePlanGrid.Rows[e.RowIndex].DataBoundItem as EffectiveVisitPlanEntry;
            if (row == null)
                return;

            if (string.Equals(row.Origin, "Added", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(235, 250, 235);
                e.CellStyle.SelectionBackColor = Color.FromArgb(170, 225, 170);
                e.CellStyle.SelectionForeColor = Color.Black;
                if (EffectivePlanGrid.Columns[e.ColumnIndex].Name == "Origin")
                {
                    e.Value = "+ Tambahan";
                    e.FormattingApplied = true;
                }
            }
            else if (string.Equals(row.Origin, "Replaced", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 250, 225);
                e.CellStyle.SelectionBackColor = Color.FromArgb(255, 225, 140);
                e.CellStyle.SelectionForeColor = Color.Black;
                if (EffectivePlanGrid.Columns[e.ColumnIndex].Name == "Origin")
                {
                    e.Value = "⇄ Pengganti";
                    e.FormattingApplied = true;
                }
            }
            else if (string.Equals(row.Origin, "Template", StringComparison.OrdinalIgnoreCase))
            {
                if (EffectivePlanGrid.Columns[e.ColumnIndex].Name == "Origin")
                {
                    e.Value = "Template";
                    e.FormattingApplied = true;
                }
            }
        }

        private void EffectivePlanGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert)
            {
                AddButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                RemoveButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                ReplaceButton_Click(null, null);
                e.Handled = true;
            }
        }

        private static void HideColumn(DataGridView grid, string columnName)
        {
            if (grid.Columns.Contains(columnName))
                grid.Columns[columnName].Visible = false;
        }

        private string SelectedSalesPersonId =>
            SalesComboBox.SelectedValue?.ToString() ?? string.Empty;

        private DateTime SelectedVisitDate => VisitDatePicker.Value.Date;

        private string CurrentUserId
        {
            get
            {
                if (MdiParent is MainForm mainForm && mainForm.UserId != null)
                    return mainForm.UserId.UserId;
                return Environment.UserName;
            }
        }

        private void RefreshPlans()
        {
            if (string.IsNullOrWhiteSpace(SelectedSalesPersonId))
                return;

            var hariRuteId = _calendar.ResolveHariRuteId(SelectedVisitDate);
            CycleLabel.Text = string.IsNullOrWhiteSpace(hariRuteId)
                ? "Minggu (tidak ada rute)"
                : $"{_calendar.GetCycleWeekLabel(SelectedVisitDate)} / {hariRuteId}";

            _basePlanRows.Clear();
            foreach (var row in _visitPlanDal.ListData(new VisitPlanDateFilter(SelectedSalesPersonId, SelectedVisitDate))
                         ?? Enumerable.Empty<VisitPlanModel>())
            {
                _basePlanRows.Add(row);
            }

            _effectivePlanRows.Clear();
            foreach (var row in _effectiveVisitPlanDal.ListEffectivePlan(SelectedSalesPersonId, SelectedVisitDate))
            {
                _effectivePlanRows.Add(row);
            }

            _exceptionRows.Clear();
            foreach (var row in _exceptionDal.ListData(new VisitPlanDateFilter(SelectedSalesPersonId, SelectedVisitDate))
                         ?? Enumerable.Empty<VisitPlanExceptionModel>())
            {
                _exceptionRows.Add(row);
            }

            SummaryCountLabel.Text = $"Total: {_effectivePlanRows.Count} Kunjungan";
            AuditTabPage.Text = $"Riwayat Exception ({_exceptionRows.Count})";
            BasePlanTabPage.Text = $"Master Rute Template ({_basePlanRows.Count})";
            PersistenceStatusLabel.Text = $"● Tersimpan di server ({DateTime.Now:HH:mm:ss})";
        }

        private void MaterializeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedSalesPersonId))
                return;

            if (SelectedVisitDate < DateTime.Today)
            {
                MessageBox.Show("Tanggal kunjungan harus hari ini atau ke depan.", "Materialisasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (SelectedVisitDate.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Hari Minggu tidak memiliki jadwal rute.", "Materialisasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                _regenerateWorker.Execute(new RegenerateVisitPlanRequest
                {
                    SalesPersonId = SelectedSalesPersonId,
                    FromDate = SelectedVisitDate,
                    ToDate = SelectedVisitDate,
                    AllowPast = false,
                    TriggeredBy = "SM7Manual"
                });

                RefreshPlans();

                if (_basePlanRows.Count > 0)
                {
                    MessageBox.Show($"Materialisasi berhasil: {_basePlanRows.Count} kunjungan dimuat.",
                        "Materialisasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Materialisasi selesai: template tidak memiliki kunjungan untuk rute ini.",
                        "Materialisasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Materialisasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!EnsureFutureDate())
                return;

            var customerBrowser = new CustomerBrowser(_customerDal);
            var customerId = customerBrowser.Browse(string.Empty);
            if (string.IsNullOrWhiteSpace(customerId))
                return;

            try
            {
                _exceptionWriter.Save(new VisitPlanExceptionModel
                {
                    SalesPersonId = SelectedSalesPersonId,
                    VisitDate = SelectedVisitDate,
                    ExceptionType = VisitPlanExceptionTypeEnum.Add.ToString(),
                    CustomerId = customerId,
                    CreatedByUserId = CurrentUserId
                });
                RefreshPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tambah Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (!EnsureFutureDate())
                return;

            var selected = GetSelectedEffectiveRow();
            if (selected == null)
            {
                MessageBox.Show("Pilih customer pada jadwal efektif.", "Hapus dari Jadwal",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Case 1: Customer ditambahkan secara manual melalui exception Add
            if (string.Equals(selected.Origin, "Added", StringComparison.OrdinalIgnoreCase))
            {
                var confirmAdd = MessageBox.Show(
                    $"Batalkan penambahan customer '{selected.CustomerName}' dari jadwal kunjungan?",
                    "Konfirmasi Batalkan Tambahan",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmAdd != DialogResult.Yes)
                    return;

                var matchingAddExc = _exceptionRows.FirstOrDefault(x =>
                    string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Add.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    x.CustomerId == selected.CustomerId);

                if (matchingAddExc != null)
                {
                    try
                    {
                        _exceptionWriter.Delete(matchingAddExc);
                        RefreshPlans();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Hapus Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // Case 2: Customer merupakan pengganti melalui exception Replace
            if (string.Equals(selected.Origin, "Replaced", StringComparison.OrdinalIgnoreCase))
            {
                var confirmRep = MessageBox.Show(
                    $"Batalkan customer pengganti '{selected.CustomerName}' dan kembalikan customer semula?",
                    "Konfirmasi Batalkan Pengganti",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmRep != DialogResult.Yes)
                    return;

                var matchingRepExc = _exceptionRows.FirstOrDefault(x =>
                    string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Replace.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    x.ReplacementCustomerId == selected.CustomerId);

                if (matchingRepExc != null)
                {
                    try
                    {
                        _exceptionWriter.Delete(matchingRepExc);
                        RefreshPlans();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Hapus Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // Case 3: Customer standar dari template rute
            var confirmRemove = MessageBox.Show(
                $"Apakah Anda yakin ingin melewati / menghapus '{selected.CustomerName}' dari jadwal kunjungan tanggal {SelectedVisitDate:dd/MM/yyyy}?",
                "Konfirmasi Hapus Kunjungan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmRemove != DialogResult.Yes)
                return;

            try
            {
                _exceptionWriter.Save(new VisitPlanExceptionModel
                {
                    SalesPersonId = SelectedSalesPersonId,
                    VisitDate = SelectedVisitDate,
                    ExceptionType = VisitPlanExceptionTypeEnum.Remove.ToString(),
                    CustomerId = selected.CustomerId,
                    CreatedByUserId = CurrentUserId
                });
                RefreshPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hapus Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (!EnsureFutureDate())
                return;

            var selected = GetSelectedEffectiveRow();
            if (selected == null)
            {
                MessageBox.Show("Pilih customer pada jadwal efektif yang ingin diganti.", "Ganti Customer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var customerBrowser = new CustomerBrowser(_customerDal);
            var replacementCustomerId = customerBrowser.Browse(string.Empty);
            if (string.IsNullOrWhiteSpace(replacementCustomerId))
                return;

            if (string.Equals(selected.CustomerId, replacementCustomerId, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Customer pengganti tidak boleh sama dengan customer semula.", "Ganti Customer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _exceptionWriter.Save(new VisitPlanExceptionModel
                {
                    SalesPersonId = SelectedSalesPersonId,
                    VisitDate = SelectedVisitDate,
                    ExceptionType = VisitPlanExceptionTypeEnum.Replace.ToString(),
                    CustomerId = selected.CustomerId,
                    ReplacementCustomerId = replacementCustomerId,
                    CreatedByUserId = CurrentUserId
                });
                RefreshPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ganti Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteExceptionButton_Click(object sender, EventArgs e)
        {
            if (ExceptionGrid.CurrentRow?.DataBoundItem is VisitPlanExceptionModel selected == false)
            {
                MessageBox.Show("Pilih baris exception yang ingin dihapus pada tabel riwayat exception.", "Hapus Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Hapus exception tipe '{selected.ExceptionType}' untuk customer '{selected.CustomerName}'?",
                "Konfirmasi Hapus Exception",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                _exceptionWriter.Delete(selected);
                RefreshPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hapus Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private EffectiveVisitPlanEntry GetSelectedEffectiveRow()
        {
            return EffectivePlanGrid.CurrentRow?.DataBoundItem as EffectiveVisitPlanEntry;
        }

        private bool EnsureFutureDate()
        {
            if (SelectedVisitDate < DateTime.Today)
            {
                MessageBox.Show("Tanggal kunjungan harus hari ini atau ke depan.", "Validasi Tanggal",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
