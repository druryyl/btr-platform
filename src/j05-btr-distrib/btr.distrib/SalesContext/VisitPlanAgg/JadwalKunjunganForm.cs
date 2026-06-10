using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
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
            IRuteCycleCalendar calendar)
        {
            InitializeComponent();

            _salesPersonDal = salesPersonDal;
            _customerDal = customerDal;
            _visitPlanDal = visitPlanDal;
            _exceptionDal = exceptionDal;
            _effectiveVisitPlanDal = effectiveVisitPlanDal;
            _exceptionWriter = exceptionWriter;
            _calendar = calendar;

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
            SalesComboBox.SelectedIndexChanged += (_, __) => RefreshPlans();
            VisitDatePicker.ValueChanged += (_, __) => RefreshPlans();
            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            ReplaceButton.Click += ReplaceButton_Click;
            DeleteExceptionButton.Click += DeleteExceptionButton_Click;
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

        private void ConfigureBasePlanColumns()
        {
            HideColumn(BasePlanGrid, "VisitPlanId");
            HideColumn(BasePlanGrid, "SalesPersonId");
            HideColumn(BasePlanGrid, "PlanSource");
            HideColumn(BasePlanGrid, "MaterializedAt");
        }

        private void ConfigureEffectivePlanColumns()
        {
            HideColumn(EffectivePlanGrid, "CustomerId");
        }

        private void ConfigureExceptionColumns()
        {
            HideColumn(ExceptionGrid, "VisitPlanExceptionId");
            HideColumn(ExceptionGrid, "SalesPersonId");
            HideColumn(ExceptionGrid, "CreatedByUserId");
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
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!EnsureFutureDate())
                return;

            var customerId = PromptCustomerId("Tambah customer ke jadwal");
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
                MessageBox.Show("Pilih customer pada effective plan.", "Hapus dari Jadwal",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
                MessageBox.Show("Pilih customer pada effective plan.", "Ganti Customer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var replacementCustomerId = PromptCustomerId("Customer pengganti");
            if (string.IsNullOrWhiteSpace(replacementCustomerId))
                return;

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

        private string PromptCustomerId(string caption)
        {
            using (var dialog = new Form())
            {
                dialog.Text = caption;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(420, 120);

                var searchText = new TextBox { Location = new Point(12, 12), Width = 280 };
                var searchButton = new Button { Text = "Cari", Location = new Point(300, 10), Width = 100 };
                var resultLabel = new Label { Location = new Point(12, 44), Width = 390, Height = 40 };
                var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(220, 82), Width = 80 };
                var cancelButton = new Button { Text = "Batal", DialogResult = DialogResult.Cancel, Location = new Point(310, 82), Width = 80 };

                string selectedCustomerId = null;

                searchButton.Click += (_, __) =>
                {
                    var keyword = searchText.Text.Trim();
                    var matches = (_customerDal.ListData() ?? Enumerable.Empty<CustomerModel>())
                        .Where(x =>
                            x.CustomerCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            x.CustomerName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Take(5)
                        .ToList();

                    if (matches.Count == 0)
                    {
                        resultLabel.Text = "Customer tidak ditemukan.";
                        selectedCustomerId = null;
                        return;
                    }

                    var first = matches[0];
                    selectedCustomerId = first.CustomerId;
                    resultLabel.Text = $"{first.CustomerCode} - {first.CustomerName}";
                };

                dialog.Controls.AddRange(new Control[]
                {
                    searchText, searchButton, resultLabel, okButton, cancelButton
                });
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                return dialog.ShowDialog(this) == DialogResult.OK ? selectedCustomerId : null;
            }
        }
    }
}
