using btr.application.InventoryContext.DriverAgg;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.distrib.Browsers;
using btr.distrib.Helpers;
using btr.distrib.InventoryContext.ReturJualAgg;
using btr.distrib.SharedForm;
using btr.domain.InventoryContext.DriverAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    public partial class GenerateReturnOrderForm : Form
    {
        private const string SyncedStatus = "Synced";

        private readonly IMediator _mediator;
        private readonly IBrowser<CustomerBrowserView> _customerBrowser;
        private readonly IBrowser<SalesPersonBrowserView> _salesBrowser;
        private readonly IBrowser<DriverBrowserView> _driverBrowser;
        private readonly ICustomerDal _customerDal;
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly IDriverDal _driverDal;

        private readonly BindingList<GenerateReturnOrderFormOrderDto> _listOrder =
            new BindingList<GenerateReturnOrderFormOrderDto>();
        private readonly BindingList<GenerateReturnOrderFormItemDto> _listItem =
            new BindingList<GenerateReturnOrderFormItemDto>();
        private readonly BindingList<GenerateReturnOrderFormResultDto> _listResult =
            new BindingList<GenerateReturnOrderFormResultDto>();

        private bool _isLoading;
        private bool _suppressRowEnter;
        private ReturnOrderModel _loadedOrder;

        public GenerateReturnOrderForm(IMediator mediator,
            IBrowser<CustomerBrowserView> customerBrowser,
            IBrowser<SalesPersonBrowserView> salesBrowser,
            IBrowser<DriverBrowserView> driverBrowser,
            ICustomerDal customerDal,
            ISalesPersonDal salesPersonDal,
            IDriverDal driverDal)
        {
            InitializeComponent();
            RegisterEventHandler();

            _mediator = mediator;
            _customerBrowser = customerBrowser;
            _salesBrowser = salesBrowser;
            _driverBrowser = driverBrowser;
            _customerDal = customerDal;
            _salesPersonDal = salesPersonDal;
            _driverDal = driverDal;

            InitGrid();
        }

        private void RegisterEventHandler()
        {
            Load += GenerateReturnOrderForm_Load;

            RefreshButton.Click += RefreshButton_Click;
            GenerateButton.Click += GenerateButton_Click;

            Tgl1Date.ValueChanged += Filter_Changed;
            Tgl2Date.ValueChanged += Filter_Changed;
            CustomerButton.Click += CustomerButton_Click;
            CustomerIdText.Validated += CustomerIdText_Validated;

            WorklistGrid.RowEnter += WorklistGrid_RowEnter;
            WorklistGrid.CellClick += WorklistGrid_CellClick;

            SalesButton.Click += SalesButton_Click;
            SalesIdText.Validated += SalesIdText_Validated;
            DriverButton.Click += DriverButton_Click;
            DriverIdText.Validated += DriverIdText_Validated;
            CompleteButton.Click += CompleteButton_Click;

            ResultGrid.CellDoubleClick += ResultGrid_CellDoubleClick;
        }

        private void GenerateReturnOrderForm_Load(object sender, EventArgs e)
        {
            Tgl1Date.Value = DateTime.Today.AddDays(-30);
            Tgl2Date.Value = DateTime.Today;
            RefreshWorklist();
        }

        #region GRID-INIT

        private void InitGrid()
        {
            InitWorklistGrid();
            InitItemGrid();
            InitResultGrid();
        }

        private void InitWorklistGrid()
        {
            WorklistGrid.AutoGenerateColumns = false;
            WorklistGrid.DataSource = new BindingSource { DataSource = _listOrder };
            WorklistGrid.AllowUserToAddRows = false;
            WorklistGrid.AllowUserToDeleteRows = false;
            WorklistGrid.ReadOnly = true;
            WorklistGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            WorklistGrid.MultiSelect = false;

            WorklistGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Pilih",
                HeaderText = string.Empty,
                DataPropertyName = "Pilih",
                Width = 40
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturnOrderId",
                DataPropertyName = "ReturnOrderId",
                Visible = false
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturnOrderNo",
                HeaderText = "No. RO",
                DataPropertyName = "ReturnOrderNo",
                Width = 100
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturnOrderDate",
                HeaderText = "Tanggal",
                DataPropertyName = "ReturnOrderDate",
                Width = 90,
                DefaultCellStyle = { Format = "dd-MMM-yyyy" }
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerName",
                HeaderText = "Customer",
                DataPropertyName = "CustomerName",
                Width = 180
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WarehouseName",
                HeaderText = "Gudang",
                DataPropertyName = "WarehouseName",
                Width = 120
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                Width = 70
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SalesPersonName",
                HeaderText = "Salesman",
                DataPropertyName = "SalesPersonName",
                Width = 110
            });
            WorklistGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DriverName",
                HeaderText = "Driver",
                DataPropertyName = "DriverName",
                Width = 110
            });

            WorklistGrid.Columns.SetDefaultCellStyle(Color.Beige);
        }

        private void InitItemGrid()
        {
            ItemGrid.AutoGenerateColumns = false;
            ItemGrid.DataSource = new BindingSource { DataSource = _listItem };
            ItemGrid.AllowUserToAddRows = false;
            ItemGrid.AllowUserToDeleteRows = false;
            ItemGrid.ReadOnly = true;
            ItemGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ItemGrid.MultiSelect = false;

            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NoUrut",
                HeaderText = "No",
                DataPropertyName = "NoUrut",
                Width = 40
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrgId",
                DataPropertyName = "BrgId",
                Visible = false
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrgCode",
                HeaderText = "Kode",
                DataPropertyName = "BrgCode",
                Width = 80
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrgName",
                HeaderText = "Barang",
                DataPropertyName = "BrgName",
                Width = 300
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Qty",
                HeaderText = "Qty",
                DataPropertyName = "Qty",
                Width = 60
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SatId",
                HeaderText = "Sat",
                DataPropertyName = "SatId",
                Width = 50
            });
            ItemGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "JenisRetur",
                HeaderText = "Jenis",
                DataPropertyName = "JenisRetur",
                Width = 70
            });

            ItemGrid.Columns.SetDefaultCellStyle(Color.Beige);
        }

        private void InitResultGrid()
        {
            ResultGrid.AutoGenerateColumns = false;
            ResultGrid.DataSource = new BindingSource { DataSource = _listResult };
            ResultGrid.AllowUserToAddRows = false;
            ResultGrid.AllowUserToDeleteRows = false;
            ResultGrid.ReadOnly = true;
            ResultGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ResultGrid.MultiSelect = false;

            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturJualId",
                DataPropertyName = "ReturJualId",
                Visible = false
            });
            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturJualCode",
                HeaderText = "Retur Code",
                DataPropertyName = "ReturJualCode",
                Width = 120
            });
            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerId",
                HeaderText = "Customer",
                DataPropertyName = "CustomerId",
                Width = 100
            });
            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "JenisRetur",
                HeaderText = "Jenis",
                DataPropertyName = "JenisRetur",
                Width = 80
            });
            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SalesPersonId",
                HeaderText = "Salesman",
                DataPropertyName = "SalesPersonId",
                Width = 100
            });
            ResultGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DriverId",
                HeaderText = "Driver",
                DataPropertyName = "DriverId",
                Width = 100
            });

            ResultGrid.Columns.SetDefaultCellStyle(Color.Beige);
        }

        #endregion

        #region FILTER

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            RefreshWorklist();
        }

        private void CustomerButton_Click(object sender, EventArgs e)
        {
            _customerBrowser.Filter.UserKeyword = CustomerIdText.Text;
            CustomerIdText.Text = _customerBrowser.Browse(CustomerIdText.Text);
            CustomerIdText_Validated(CustomerIdText, null);
            RefreshWorklist();
        }

        private void CustomerIdText_Validated(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            var customerId = (CustomerIdText.Text ?? string.Empty).Trim();
            if (customerId.Length == 0)
            {
                CustomerNameText.Clear();
                return;
            }

            var customer = _customerDal.GetData(new CustomerModel(customerId));
            CustomerNameText.Text = customer?.CustomerName ?? string.Empty;
        }

        #endregion

        #region WORKLIST

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshWorklist();
        }

        private void RefreshWorklist()
        {
            _suppressRowEnter = true;
            try
            {
                var checkedIds = _listOrder
                    .Where(x => x.Pilih)
                    .Select(x => x.ReturnOrderId)
                    .ToList();

                var listOrder = _mediator
                                    .Send(new ListReturnOrderQuery(
                                        Tgl1Date.Value.ToString("yyyy-MM-dd"),
                                        Tgl2Date.Value.ToString("yyyy-MM-dd"),
                                        (CustomerIdText.Text ?? string.Empty).Trim()))
                                    .GetAwaiter().GetResult()
                                ?? Enumerable.Empty<ReturnOrderModel>();

                var result = listOrder
                    .Select(x => new GenerateReturnOrderFormOrderDto(x.ReturnOrderId,
                        x.ReturnOrderNo, x.ReturnOrderDate, x.CustomerId, x.CustomerName,
                        x.WarehouseCode, x.WarehouseName, x.SalesPersonId,
                        x.SalesPersonName, x.DriverId, x.DriverName, x.Status))
                    .ToList();
                foreach (var row in result)
                    row.Pilih = checkedIds.Contains(row.ReturnOrderId);

                _listOrder.RaiseListChangedEvents = false;
                try
                {
                    _listOrder.Clear();
                    foreach (var item in result)
                        _listOrder.Add(item);
                }
                finally
                {
                    _listOrder.RaiseListChangedEvents = true;
                    _listOrder.ResetBindings();
                }

                WorklistGrid.Refresh();

                if (WorklistGrid.Rows.Count > 0)
                    WorklistGrid.CurrentCell = WorklistGrid.Rows[0].Cells["ReturnOrderNo"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _suppressRowEnter = false;
            }

            if (_listOrder.Count == 0)
                ClearDetail();
            else
                LoadDetail(_listOrder[0].ReturnOrderId);

            ApplyUiState();
        }

        private void WorklistGrid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressRowEnter)
                return;
            if (e.RowIndex < 0)
                return;

            var returnOrderId = WorklistGrid.Rows[e.RowIndex]
                .Cells["ReturnOrderId"].Value?.ToString();
            LoadDetail(returnOrderId);
        }

        private void WorklistGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (WorklistGrid.Columns[e.ColumnIndex].Name != "Pilih")
                return;

            var row = _listOrder[e.RowIndex];
            row.Pilih = !row.Pilih;
            WorklistGrid.InvalidateRow(e.RowIndex);
            ApplyUiState();
        }

        #endregion

        #region DETAIL

        private void LoadDetail(string returnOrderId)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
            {
                ClearDetail();
                ApplyUiState();
                return;
            }

            try
            {
                var order = _mediator.Send(new GetReturnOrderQuery(returnOrderId))
                    .GetAwaiter().GetResult();
                if (order is null)
                {
                    ClearDetail();
                    ApplyUiState();
                    return;
                }

                _isLoading = true;
                try
                {
                    _loadedOrder = order;
                    SalesIdText.Text = order.SalesPersonId ?? string.Empty;
                    SalesNameText.Text = order.SalesPersonName ?? string.Empty;
                    DriverIdText.Text = order.DriverId ?? string.Empty;
                    DriverNameText.Text = order.DriverName ?? string.Empty;

                    _listItem.RaiseListChangedEvents = false;
                    try
                    {
                        _listItem.Clear();
                        foreach (var item in order.ListItem ??
                                             new List<ReturnOrderItemModel>())
                            _listItem.Add(new GenerateReturnOrderFormItemDto(
                                item.NoUrut, item.BrgId, item.BrgCode, item.BrgName,
                                item.Qty, item.SatId, item.JenisRetur));
                    }
                    finally
                    {
                        _listItem.RaiseListChangedEvents = true;
                        _listItem.ResetBindings();
                    }

                    ItemGrid.Refresh();
                }
                finally
                {
                    _isLoading = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearDetail();
            }

            ApplyUiState();
        }

        private void ClearDetail()
        {
            _isLoading = true;
            try
            {
                _loadedOrder = null;
                SalesIdText.Clear();
                SalesNameText.Clear();
                DriverIdText.Clear();
                DriverNameText.Clear();

                _listItem.RaiseListChangedEvents = false;
                try
                {
                    _listItem.Clear();
                }
                finally
                {
                    _listItem.RaiseListChangedEvents = true;
                    _listItem.ResetBindings();
                }

                ItemGrid.Refresh();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyUiState()
        {
            //  IR-D3 — an Imported (or any non-Synced) order is view-only and
            //  cannot be regenerated. The worklist carries Synced orders only,
            //  so this is a defensive guard.
            var canComplete = _loadedOrder != null &&
                              string.Equals(_loadedOrder.Status, SyncedStatus,
                                  StringComparison.Ordinal);

            SalesIdText.Enabled = canComplete;
            SalesButton.Enabled = canComplete;
            DriverIdText.Enabled = canComplete;
            DriverButton.Enabled = canComplete;
            CompleteButton.Enabled = canComplete;

            //  IR-D1 — Generate requires at least one selected order
            GenerateButton.Enabled = _listOrder.Any(x => x.Pilih);
        }

        private void SalesButton_Click(object sender, EventArgs e)
        {
            _salesBrowser.Filter.UserKeyword = SalesIdText.Text;
            SalesIdText.Text = _salesBrowser.Browse(SalesIdText.Text);
            ResolveSalesPersonName();
        }

        private void SalesIdText_Validated(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            ResolveSalesPersonName();
        }

        private void ResolveSalesPersonName()
        {
            var salesPersonId = (SalesIdText.Text ?? string.Empty).Trim();
            if (salesPersonId.Length == 0)
            {
                SalesNameText.Clear();
                return;
            }

            var salesPerson = _salesPersonDal.GetData(new SalesPersonModel(salesPersonId));
            SalesNameText.Text = salesPerson?.SalesPersonName ?? string.Empty;
        }

        private void DriverButton_Click(object sender, EventArgs e)
        {
            _driverBrowser.Filter.UserKeyword = DriverIdText.Text;
            DriverIdText.Text = _driverBrowser.Browse(DriverIdText.Text);
            ResolveDriverName();
        }

        private void DriverIdText_Validated(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            ResolveDriverName();
        }

        private void ResolveDriverName()
        {
            var driverId = (DriverIdText.Text ?? string.Empty).Trim();
            if (driverId.Length == 0)
            {
                DriverNameText.Clear();
                return;
            }

            var driver = _driverDal.GetData(new DriverModel(driverId));
            DriverNameText.Text = driver?.DriverName ?? string.Empty;
        }

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            if (_loadedOrder is null)
                return;

            var returnOrderId = _loadedOrder.ReturnOrderId;
            try
            {
                //  IR-D2 / ADR-RO-005 — Salesman/Driver are optional; a blank
                //  value is accepted and clears the optional field.
                _mediator.Send(new CompleteReturnOrderCommand(returnOrderId,
                        SalesIdText.Text, DriverIdText.Text, CurrentUserId))
                    .GetAwaiter().GetResult();

                RefreshWorklist();
                SelectOrderRow(returnOrderId);
                MessageBox.Show(@"Salesman/Driver berhasil disimpan.", @"Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectOrderRow(string returnOrderId)
        {
            foreach (DataGridViewRow row in WorklistGrid.Rows)
            {
                if (!string.Equals(row.Cells["ReturnOrderId"].Value?.ToString(),
                        returnOrderId, StringComparison.Ordinal))
                    continue;

                WorklistGrid.CurrentCell = row.Cells["ReturnOrderNo"];
                return;
            }
        }

        private string CurrentUserId
        {
            get
            {
                var mainForm = MdiParent as MainForm;
                return mainForm?.UserId?.UserId ?? string.Empty;
            }
        }

        #endregion

        #region GENERATE

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var selectedIds = _listOrder
                .Where(x => x.Pilih)
                .Select(x => x.ReturnOrderId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            //  IR-D1 — Generate requires selection
            if (selectedIds.Count == 0)
            {
                MessageBox.Show(@"Pilih minimal satu Return Order.", @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($@"Generate {selectedIds.Count} Return Order menjadi Retur Jual?",
                    @"Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            try
            {
                var response = _mediator
                    .Send(new GenerateSalesReturnFromReturnOrderCommand(
                        selectedIds, CurrentUserId))
                    .GetAwaiter().GetResult();

                _listResult.RaiseListChangedEvents = false;
                try
                {
                    _listResult.Clear();
                    foreach (var generated in response.ListGenerated)
                        _listResult.Add(new GenerateReturnOrderFormResultDto(
                            generated.ReturJualId, generated.ReturJualCode,
                            generated.CustomerId, generated.JenisRetur,
                            generated.SalesPersonId, generated.DriverId));
                }
                finally
                {
                    _listResult.RaiseListChangedEvents = true;
                    _listResult.ResetBindings();
                }

                ResultGrid.Refresh();
                RefreshWorklist();

                MessageBox.Show($@"{response.ListGenerated.Count} Retur Jual berhasil di-generate.",
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ResultGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var returJualId = ResultGrid.Rows[e.RowIndex]
                .Cells["ReturJualId"].Value?.ToString();
            OpenReturJual(returJualId);
        }

        private void OpenReturJual(string returJualId)
        {
            if (string.IsNullOrWhiteSpace(returJualId))
                return;

            var mainForm = MdiParent as MainForm;
            if (mainForm is null)
                return;

            //  §13.2 — the generated document opens in the existing
            //  RT1-Retur Jual form (the Faktur/ListOrder precedent).
            mainForm.RT1ReturJualButton_Click(null, null);
            var returJualForm = Application.OpenForms
                .OfType<ReturJualForm>().FirstOrDefault();
            returJualForm?.ShowReturJual(returJualId);
        }

        #endregion
    }
}
