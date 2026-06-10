using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.application.SalesContext.SalesPersonSupplierAgg.Contracts;
using btr.distrib.Helpers;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.SalesPersonPrincipalTargetAgg
{
    public partial class SalesPersonPrincipalTargetForm : Form
    {
        private static readonly string[] MonthNames =
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        private readonly ISalesPersonDal _salesPersonDal;
        private readonly ISalesPersonSupplierDal _salesPersonSupplierDal;
        private readonly ISalesPersonPrincipalTargetDal _targetDal;
        private readonly ISalesPersonPrincipalTargetWriter _targetWriter;

        private List<SalesPersonListDto> _listSalesPerson;
        private readonly BindingList<SalesPersonPrincipalTargetViewDto> _listPrincipal;
        private readonly BindingSource _principalBindingSource;
        private string _selectedSalesPersonId;
        private int _selectedYear;
        private int _selectedMonth;

        public SalesPersonPrincipalTargetForm(
            ISalesPersonDal salesPersonDal,
            ISalesPersonSupplierDal salesPersonSupplierDal,
            ISalesPersonPrincipalTargetDal targetDal,
            ISalesPersonPrincipalTargetWriter targetWriter)
        {
            InitializeComponent();

            _salesPersonDal = salesPersonDal;
            _salesPersonSupplierDal = salesPersonSupplierDal;
            _targetDal = targetDal;
            _targetWriter = targetWriter;

            _listPrincipal = new BindingList<SalesPersonPrincipalTargetViewDto>();
            _principalBindingSource = new BindingSource(_listPrincipal, null);

            RegisterEventHandler();
            InitPeriodSelectors();
            InitSalesPersonGrid();
            InitPrincipalGrid();
        }

        private void RegisterEventHandler()
        {
            SearchButton.Click += SearchButton_Click;
            SearchText.KeyDown += SearchText_KeyDown;
            SalesPersonGrid.CellClick += SalesPersonGrid_CellClick;
            SalesPersonGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
            PrincipalGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
            PrincipalGrid.CellValueChanged += PrincipalGrid_CellValueChanged;
            PrincipalGrid.CurrentCellDirtyStateChanged += PrincipalGrid_CurrentCellDirtyStateChanged;
            YearCombo.SelectedIndexChanged += PeriodSelector_Changed;
            MonthCombo.SelectedIndexChanged += PeriodSelector_Changed;
            SaveButton.Click += SaveButton_Click;
            RefreshButton.Click += RefreshButton_Click;
            CopyPrevMonthButton.Click += CopyPrevMonthButton_Click;
            CopyAllRepsButton.Click += CopyAllRepsButton_Click;
        }

        private void InitPeriodSelectors()
        {
            var currentYear = DateTime.Today.Year;
            for (var year = currentYear - 2; year <= currentYear + 2; year++)
                YearCombo.Items.Add(year);

            for (var i = 0; i < MonthNames.Length; i++)
                MonthCombo.Items.Add(MonthNames[i]);

            _selectedYear = currentYear;
            _selectedMonth = DateTime.Today.Month;
            YearCombo.SelectedItem = _selectedYear;
            MonthCombo.SelectedIndex = _selectedMonth - 1;
        }

        private void InitSalesPersonGrid()
        {
            ReloadSalesPersonList();
            SalesPersonGrid.Columns.SetDefaultCellStyle(Color.PowderBlue);
            SalesPersonGrid.Columns.GetCol("Id").Width = 50;
            SalesPersonGrid.Columns.GetCol("Code").Width = 60;
            SalesPersonGrid.Columns.GetCol("Name").Width = 180;
            SalesPersonGrid.Columns.GetCol("Done").Width = 40;
            SalesPersonGrid.SetAlternatingRowColors();
        }

        private void InitPrincipalGrid()
        {
            PrincipalGrid.DataSource = _principalBindingSource;
            PrincipalGrid.AutoGenerateColumns = true;
            PrincipalGrid.DataBindingComplete += PrincipalGrid_DataBindingComplete;
        }

        private void PrincipalGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (PrincipalGrid.Columns.Contains("SupplierId"))
                PrincipalGrid.Columns["SupplierId"].Visible = false;

            if (PrincipalGrid.Columns.Contains("SupplierCode"))
            {
                PrincipalGrid.Columns["SupplierCode"].HeaderText = "Principal Code";
                PrincipalGrid.Columns["SupplierCode"].ReadOnly = true;
            }

            if (PrincipalGrid.Columns.Contains("SupplierName"))
            {
                PrincipalGrid.Columns["SupplierName"].HeaderText = "Principal Name";
                PrincipalGrid.Columns["SupplierName"].ReadOnly = true;
            }

            if (PrincipalGrid.Columns.Contains("TargetAmount"))
            {
                PrincipalGrid.Columns["TargetAmount"].HeaderText = "Target Amount";
                PrincipalGrid.Columns["TargetAmount"].DefaultCellStyle.Format = "#,##0.00";
                PrincipalGrid.Columns["TargetAmount"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void PrincipalGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (PrincipalGrid.IsCurrentCellDirty)
                PrincipalGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void PrincipalGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateTotalLabel();
        }

        private void PeriodSelector_Changed(object sender, EventArgs e)
        {
            if (YearCombo.SelectedItem is int year)
                _selectedYear = year;

            if (MonthCombo.SelectedIndex >= 0)
                _selectedMonth = MonthCombo.SelectedIndex + 1;

            ReloadSalesPersonList();

            if (!string.IsNullOrWhiteSpace(_selectedSalesPersonId))
                LoadTargets(_selectedSalesPersonId);
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                FilterSalesPersonGrid(SearchText.Text);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            FilterSalesPersonGrid(SearchText.Text);
        }

        private void FilterSalesPersonGrid(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                SalesPersonGrid.DataSource = _listSalesPerson.ToList();
                return;
            }

            var listFilter = _listSalesPerson
                .Where(x => x.Name.ContainMultiWord(keyword)
                    || x.Code.ContainMultiWord(keyword)
                    || x.Id.ContainMultiWord(keyword))
                .ToList();
            SalesPersonGrid.DataSource = listFilter;
        }

        private void SalesPersonGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var grid = (DataGridView)sender;
            var salesPersonId = grid.Rows[e.RowIndex].Cells[0].Value?.ToString();
            if (string.IsNullOrWhiteSpace(salesPersonId))
                return;

            LoadTargets(salesPersonId);
        }

        private void ReloadSalesPersonList()
        {
            var listSalesPerson = _salesPersonDal.ListData()?.ToList()
                ?? new List<SalesPersonModel>();

            _listSalesPerson = listSalesPerson
                .OrderBy(x => x.SalesPersonName)
                .Select(x => new SalesPersonListDto(
                    x.SalesPersonId,
                    x.SalesPersonCode,
                    x.SalesPersonName,
                    ComputeDoneStatus(x.SalesPersonId)))
                .ToList();

            SalesPersonGrid.DataSource = _listSalesPerson.ToList();
        }

        private string ComputeDoneStatus(string salesPersonId)
        {
            var assignments = _salesPersonSupplierDal.ListData(new SalesPersonModel(salesPersonId))?.ToList()
                ?? new List<SalesPersonSupplierModel>();

            if (assignments.Count == 0)
                return string.Empty;

            var targetSupplierIds = new HashSet<string>(
                _targetDal
                    .ListBySalesPersonPeriod(salesPersonId, _selectedYear, _selectedMonth)
                    ?.Select(x => x.SupplierId)
                    ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            var allHaveTarget = assignments.All(x => targetSupplierIds.Contains(x.SupplierId));
            return allHaveTarget ? "✓" : "○";
        }

        private void LoadTargets(string salesPersonId)
        {
            _selectedSalesPersonId = salesPersonId;
            var salesPerson = _listSalesPerson.FirstOrDefault(x => x.Id == salesPersonId);
            var monthName = MonthNames[_selectedMonth - 1];

            SelectedLabel.Text = salesPerson is null
                ? $"Principal Targets for: {salesPersonId} — {monthName} {_selectedYear}"
                : $"Principal Targets for: {salesPerson.Name} ({salesPerson.Code}) — {monthName} {_selectedYear}";

            var assignments = _salesPersonSupplierDal.ListData(new SalesPersonModel(salesPersonId))?.ToList()
                ?? new List<SalesPersonSupplierModel>();

            var existingTargets = _targetDal
                .ListBySalesPersonPeriod(salesPersonId, _selectedYear, _selectedMonth)
                ?.ToDictionary(x => x.SupplierId, x => x.TargetAmount, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            _listPrincipal.Clear();
            foreach (var assignment in assignments)
            {
                var amount = existingTargets.TryGetValue(assignment.SupplierId, out var targetAmount)
                    ? targetAmount
                    : 0m;

                _listPrincipal.Add(new SalesPersonPrincipalTargetViewDto(
                    assignment.SupplierId,
                    assignment.SupplierCode,
                    assignment.SupplierName,
                    amount));
            }

            var withTarget = assignments.Count(x => existingTargets.ContainsKey(x.SupplierId));
            CompletenessLabel.Text = $"Completeness: {withTarget}/{assignments.Count} assigned principals have target";
            UpdateTotalLabel();
        }

        private void UpdateTotalLabel()
        {
            var total = _listPrincipal.Sum(x => x.TargetAmount);
            TotalLabel.Text = $"Total: {total.ToString("#,##0.00", CultureInfo.CurrentCulture)}";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSalesPersonId))
            {
                MessageBox.Show("Select a sales person first.", "Principal Target",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _targetWriter.Save(
                    _selectedSalesPersonId,
                    _selectedYear,
                    _selectedMonth,
                    _listPrincipal.ToList());

                ReloadSalesPersonList();
                LoadTargets(_selectedSalesPersonId);
                MessageBox.Show("Targets saved.", "Principal Target",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Principal Target",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ReloadSalesPersonList();
            if (!string.IsNullOrWhiteSpace(_selectedSalesPersonId))
                LoadTargets(_selectedSalesPersonId);
        }

        private void CopyPrevMonthButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSalesPersonId))
            {
                MessageBox.Show("Select a sales person first.", "Principal Target",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (sourceYear, sourceMonth) = GetPreviousMonth(_selectedYear, _selectedMonth);
            var monthName = MonthNames[_selectedMonth - 1];
            var sourceMonthName = MonthNames[sourceMonth - 1];

            var confirm = MessageBox.Show(
                $"Copy targets from {sourceMonthName} {sourceYear} to {monthName} {_selectedYear} for the selected sales person?",
                "Copy Previous Month",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var copied = _targetWriter.CopyPreviousMonthForSalesPerson(
                    _selectedSalesPersonId,
                    sourceYear,
                    sourceMonth,
                    _selectedYear,
                    _selectedMonth);

                ReloadSalesPersonList();
                LoadTargets(_selectedSalesPersonId);
                MessageBox.Show($"Copied {copied} principal target(s).", "Copy Previous Month",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Copy Previous Month",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopyAllRepsButton_Click(object sender, EventArgs e)
        {
            var (sourceYear, sourceMonth) = GetPreviousMonth(_selectedYear, _selectedMonth);
            var monthName = MonthNames[_selectedMonth - 1];
            var sourceMonthName = MonthNames[sourceMonth - 1];
            var repCount = _listSalesPerson?.Count ?? 0;

            var confirm = MessageBox.Show(
                $"Copy targets from {sourceMonthName} {sourceYear} to {monthName} {_selectedYear} for all {repCount} sales person(s) with current principal assignments?",
                "Copy All Reps",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var affected = _targetWriter.CopyPreviousMonthForAllSalesPersons(
                    sourceYear,
                    sourceMonth,
                    _selectedYear,
                    _selectedMonth);

                ReloadSalesPersonList();
                if (!string.IsNullOrWhiteSpace(_selectedSalesPersonId))
                    LoadTargets(_selectedSalesPersonId);

                MessageBox.Show($"Copied targets for {affected} sales person(s).", "Copy All Reps",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Copy All Reps",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static (int Year, int Month) GetPreviousMonth(int year, int month)
        {
            var date = new DateTime(year, month, 1).AddMonths(-1);
            return (date.Year, date.Month);
        }

        private sealed class SalesPersonListDto
        {
            public SalesPersonListDto(string id, string code, string name, string done)
            {
                Id = id;
                Code = code;
                Name = name;
                Done = done;
            }

            public string Id { get; }
            public string Code { get; }
            public string Name { get; }
            public string Done { get; }
        }
    }
}
