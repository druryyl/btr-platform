using btr.application.PurchaseContext.SupplierAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.SalesPersonSupplierAgg;
using btr.application.SalesContext.SalesPersonSupplierAgg.Contracts;
using btr.distrib.Browsers;
using btr.distrib.Helpers;
using btr.domain.PurchaseContext.SupplierAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace btr.distrib.SalesContext.SalesPersonSupplierAgg
{
    public partial class SalesPersonSupplierForm : Form
    {
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly ISalesPersonSupplierDal _salesPersonSupplierDal;
        private readonly ISupplierDal _supplierDal;
        private readonly ISalesPersonSupplierWriter _salesPersonSupplierWriter;
        private readonly IBrowser<SupplierBrowserView> _supplierBrowser;

        private IEnumerable<SalesPersonListDto> _listSalesPerson;
        private readonly BindingList<SalesPersonSupplierViewDto> _listPrincipal;
        private readonly BindingSource _principalBindingSource;
        private string _selectedSalesPersonId;

        public SalesPersonSupplierForm(
            ISalesPersonDal salesPersonDal,
            ISalesPersonSupplierDal salesPersonSupplierDal,
            ISupplierDal supplierDal,
            ISalesPersonSupplierWriter salesPersonSupplierWriter,
            IBrowser<SupplierBrowserView> supplierBrowser)
        {
            InitializeComponent();

            _salesPersonDal = salesPersonDal;
            _salesPersonSupplierDal = salesPersonSupplierDal;
            _supplierDal = supplierDal;
            _salesPersonSupplierWriter = salesPersonSupplierWriter;
            _supplierBrowser = supplierBrowser;

            _listPrincipal = new BindingList<SalesPersonSupplierViewDto>();
            _principalBindingSource = new BindingSource(_listPrincipal, null);

            RegisterEventHandler();
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
            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
        }

        private void InitSalesPersonGrid()
        {
            var listSalesPerson = _salesPersonDal.ListData()?.ToList()
                ?? new List<SalesPersonModel>();

            _listSalesPerson = listSalesPerson
                .OrderBy(x => x.SalesPersonName)
                .Select(x => new SalesPersonListDto(x.SalesPersonId, x.SalesPersonCode, x.SalesPersonName))
                .ToList();

            SalesPersonGrid.DataSource = _listSalesPerson.ToList();
            SalesPersonGrid.Columns.SetDefaultCellStyle(Color.PowderBlue);
            SalesPersonGrid.Columns.GetCol("Id").Width = 50;
            SalesPersonGrid.Columns.GetCol("Code").Width = 60;
            SalesPersonGrid.Columns.GetCol("Name").Width = 180;
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
                PrincipalGrid.Columns["SupplierCode"].HeaderText = "Principal Code";

            if (PrincipalGrid.Columns.Contains("SupplierName"))
                PrincipalGrid.Columns["SupplierName"].HeaderText = "Principal Name";
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

            LoadAssignments(salesPersonId);
        }

        private void LoadAssignments(string salesPersonId)
        {
            _selectedSalesPersonId = salesPersonId;
            var salesPerson = _listSalesPerson.FirstOrDefault(x => x.Id == salesPersonId);
            SelectedLabel.Text = salesPerson is null
                ? $"Selected: {salesPersonId}"
                : $"Selected: {salesPerson.Name} ({salesPerson.Code})";

            _listPrincipal.Clear();
            var assignments = _salesPersonSupplierDal.ListData(new SalesPersonModel(salesPersonId))?.ToList()
                ?? new List<SalesPersonSupplierModel>();

            foreach (var item in assignments)
            {
                _listPrincipal.Add(new SalesPersonSupplierViewDto(
                    item.SupplierId,
                    item.SupplierCode,
                    item.SupplierName));
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSalesPersonId))
            {
                MessageBox.Show("Select a sales person first.", "Principal Assignment",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var supplierId = _supplierBrowser.Browse(string.Empty);
            if (string.IsNullOrWhiteSpace(supplierId))
                return;

            if (_listPrincipal.Any(x => x.SupplierId == supplierId))
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            var supplier = _supplierDal.GetData(new SupplierModel(supplierId));
            if (supplier is null)
                return;

            _listPrincipal.Add(new SalesPersonSupplierViewDto(
                supplier.SupplierId,
                supplier.SupplierCode,
                supplier.SupplierName));
            Save();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSalesPersonId))
                return;

            if (PrincipalGrid.CurrentRow is null || PrincipalGrid.CurrentRow.Index < 0)
                return;

            var index = PrincipalGrid.CurrentRow.Index;
            if (index >= _listPrincipal.Count)
                return;

            _listPrincipal.RemoveAt(index);
            Save();
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_selectedSalesPersonId))
                return;

            var assignments = _listPrincipal
                .Select(x => new SalesPersonSupplierModel
                {
                    SalesPersonId = _selectedSalesPersonId,
                    SupplierId = x.SupplierId
                })
                .ToList();

            _salesPersonSupplierWriter.Save(_selectedSalesPersonId, assignments);
        }

        private sealed class SalesPersonListDto
        {
            public SalesPersonListDto(string id, string code, string name)
            {
                Id = id;
                Code = code;
                Name = name;
            }

            public string Id { get; }
            public string Code { get; }
            public string Name { get; }
        }
    }
}
