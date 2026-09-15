using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using btr.application.BrgContext.BrgAgg;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.application.SupportContext.UserAgg;
using btr.distrib.Browsers;
using btr.distrib.Helpers;
using btr.distrib.SharedForm;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using MediatR;

namespace btr.distrib.InventoryContext.BrgBarcodeAgg
{
    public partial class BrgBarcodeForm : Form
    {
        private enum EntryState
        {
            Browse,
            NewEntry,
            ValueCaptured,
            ReadyToSave,
            ExistingLoaded,
            Dirty,
            DuplicateDetected
        }

        private const string SystemAdministratorRoleId = "SYSAD";
        private static readonly string[] LifecycleRoleNames =
        {
            "Office Admin",
            "System Administrator"
        };

        private readonly IBrowser<BrgBrowserView> _brgBrowser;
        private readonly IBrgDal _brgDal;
        private readonly IBrgSatuanDal _brgSatuanDal;
        private readonly IBrgBarcodeDal _brgBarcodeDal;
        private readonly IUserDal _userDal;
        private readonly IMediator _mediator;

        private readonly BindingList<BrgBarcodeFormBarcodeDto> _listBarcode =
            new BindingList<BrgBarcodeFormBarcodeDto>();

        private EntryState _state = EntryState.Browse;
        private bool _editing;
        private bool _canManageLifecycle;
        private bool _isLoading;
        private BrgBarcodeModel _loadedMapping;
        private BrgBarcodeModel _duplicateMapping;

        public BrgBarcodeForm(IBrowser<BrgBrowserView> brgBrowser,
            IBrgDal brgDal,
            IBrgSatuanDal brgSatuanDal,
            IBrgBarcodeDal brgBarcodeDal,
            IUserDal userDal,
            IMediator mediator)
        {
            InitializeComponent();
            RegisterEventHandler();

            _brgBrowser = brgBrowser;
            _brgDal = brgDal;
            _brgSatuanDal = brgSatuanDal;
            _brgBarcodeDal = brgBarcodeDal;
            _userDal = userDal;
            _mediator = mediator;

            InitGrid();
        }

        private void RegisterEventHandler()
        {
            Load += BrgBarcodeForm_Load;

            NewButton.Click += NewButton_Click;
            SaveButton.Click += SaveButton_Click;
            ActivateButton.Click += ActivateButton_Click;
            DeactivateButton.Click += DeactivateButton_Click;
            ViewExistingButton.Click += ViewExistingButton_Click;
            RefreshButton.Click += RefreshButton_Click;
            CancelActionButton.Click += CancelActionButton_Click;

            BrgButton.Click += BrgButton_Click;
            BrgIdText.Validated += BrgIdText_Validated;
            BrgIdText.TextChanged += Field_TextChanged;

            BarcodeValueText.KeyDown += BarcodeValueText_KeyDown;
            BarcodeValueText.TextChanged += BarcodeValueText_TextChanged;
            SatuanCombo.SelectedIndexChanged += SatuanCombo_SelectedIndexChanged;

            SearchButton.Click += SearchButton_Click;
            SearchText.KeyDown += SearchText_KeyDown;
            BarcodeGrid.CellDoubleClick += BarcodeGrid_CellDoubleClick;
        }

        private void BrgBarcodeForm_Load(object sender, EventArgs e)
        {
            _canManageLifecycle = ResolveCanManageLifecycle();
            RefreshWorklist();
            _editing = false;
            ClearForm();
            ApplyUiState();
        }

        #region STATE

        private void RecalculateState()
        {
            if (_loadedMapping != null)
            {
                _state = IsDirty() ? EntryState.Dirty : EntryState.ExistingLoaded;
                ApplyUiState();
                return;
            }

            if (_state == EntryState.DuplicateDetected)
            {
                ApplyUiState();
                return;
            }

            var hasBarcode = HasBarcodeValue();
            var hasItem = HasItem();
            if (!hasBarcode)
                _state = EntryState.NewEntry;
            else if (!hasItem)
                _state = EntryState.ValueCaptured;
            else
                _state = EntryState.ReadyToSave;

            ApplyUiState();
        }

        private void ApplyUiState()
        {
            var isExisting = _loadedMapping != null;
            var isDuplicate = _state == EntryState.DuplicateDetected;
            var editable = _editing && !isDuplicate;
            var hasBarcode = HasBarcodeValue();
            var hasItem = HasItem();

            //  IR-D1 browse mode: no field editable; IR-D6 duplicate: no mutation
            BarcodeValueText.ReadOnly = !editable || isExisting;
            BrgIdText.ReadOnly = !editable;
            BrgButton.Enabled = editable;
            SatuanCombo.Enabled = editable && hasItem;

            //  §14.1: Save enabled only in Ready To Save and Dirty
            if (isDuplicate)
                SaveButton.Enabled = false;
            else if (isExisting)
                SaveButton.Enabled = IsDirty();
            else
                SaveButton.Enabled = editable && hasBarcode && hasItem;

            //  IR-D3 / IR-D4 / IR-D5 lifecycle legality and role gate (BQ-6)
            ActivateButton.Enabled = _editing && isExisting
                                     && _canManageLifecycle && !_loadedMapping.IsAktif;
            DeactivateButton.Enabled = _editing && isExisting
                                       && _canManageLifecycle && _loadedMapping.IsAktif;

            //  IR-D6: duplicate surfaces View Existing Mapping, never Save
            ViewExistingButton.Enabled = isDuplicate;

            //  IR-D1 / IR-D2 / §15.1: Cancel only while in Edit Mode
            CancelActionButton.Enabled = _editing;
        }

        private bool IsDirty()
        {
            if (_loadedMapping is null)
                return false;

            if (!string.Equals(BrgIdText.Text ?? string.Empty,
                    _loadedMapping.BrgId ?? string.Empty, StringComparison.Ordinal))
                return true;

            return !string.Equals(GetSelectedSatuan(),
                _loadedMapping.Satuan ?? string.Empty, StringComparison.Ordinal);
        }

        private bool HasBarcodeValue()
            => !string.IsNullOrWhiteSpace(BrgBarcodeModel.BarcodeValueKey(BarcodeValueText.Text));

        private bool HasItem()
            => !string.IsNullOrWhiteSpace(BrgIdText.Text);

        private bool ResolveCanManageLifecycle()
        {
            var mainForm = MdiParent as MainForm;
            if (mainForm?.UserId is null)
                return false;

            var user = _userDal.GetData(mainForm.UserId);
            if (user is null)
                return false;

            if (string.Equals(user.RoleId, SystemAdministratorRoleId,
                    StringComparison.OrdinalIgnoreCase))
                return true;

            return !string.IsNullOrWhiteSpace(user.RoleName)
                   && LifecycleRoleNames.Any(role => string.Equals(user.RoleName,
                       role, StringComparison.OrdinalIgnoreCase));
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

        #region FORM-ACTIONS

        private void ClearForm()
        {
            _isLoading = true;
            try
            {
                _loadedMapping = null;
                _duplicateMapping = null;
                _state = _editing ? EntryState.NewEntry : EntryState.Browse;

                BrgBarcodeIdText.Clear();
                BarcodeValueText.Clear();
                BrgIdText.Clear();
                BrgCodeText.Clear();
                BrgNameText.Clear();
                SatuanCombo.Items.Clear();
                IsAktifLabel.Text = string.Empty;
                CreatedLabel.Text = string.Empty;
                ModifiedLabel.Text = string.Empty;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            _editing = true;
            ClearForm();
            ApplyUiState();
            BarcodeValueText.Focus();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var userId = CurrentUserId;
                var satuan = GetSelectedSatuan();
                string brgBarcodeId;

                if (_loadedMapping is null)
                {
                    //  UC-002 register (new)
                    var response = _mediator
                        .Send(new RegisterBrgBarcodeCommand(BarcodeValueText.Text,
                            BrgIdText.Text, satuan, userId))
                        .GetAwaiter().GetResult();
                    brgBarcodeId = response.BrgBarcodeId;
                }
                else
                {
                    //  UC-003 correct (existing); activation state is preserved (INV-07)
                    var response = _mediator
                        .Send(new CorrectBrgBarcodeCommand(_loadedMapping.BrgBarcodeId,
                            BrgIdText.Text, satuan, userId))
                        .GetAwaiter().GetResult();
                    brgBarcodeId = response.BrgBarcodeId;
                }

                //  verify the committed mapping is readable before leaving the entry
                _mediator.Send(new GetBrgBarcodeQuery(brgBarcodeId))
                    .GetAwaiter().GetResult();

                //  §14.1: Saved returns to Idle after the grid is refreshed
                _editing = false;
                ClearForm();
                RefreshWorklist();
                ApplyUiState();
                MessageBox.Show(@"Mapping berhasil disimpan.", @"Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ActivateButton_Click(object sender, EventArgs e)
        {
            if (_loadedMapping is null)
                return;

            try
            {
                _mediator.Send(new ActivateBrgBarcodeCommand(
                        _loadedMapping.BrgBarcodeId, CurrentUserId))
                    .GetAwaiter().GetResult();
                ReloadLoadedMapping();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeactivateButton_Click(object sender, EventArgs e)
        {
            if (_loadedMapping is null)
                return;

            try
            {
                _mediator.Send(new DeactivateBrgBarcodeCommand(
                        _loadedMapping.BrgBarcodeId, CurrentUserId))
                    .GetAwaiter().GetResult();
                ReloadLoadedMapping();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ReloadLoadedMapping()
        {
            var mappingId = _loadedMapping.BrgBarcodeId;
            var mapping = _mediator.Send(new GetBrgBarcodeQuery(mappingId))
                .GetAwaiter().GetResult();
            LoadExistingMapping(mapping);
            RefreshWorklist();
        }

        private void ViewExistingButton_Click(object sender, EventArgs e)
        {
            if (_duplicateMapping is null)
                return;

            //  §14.1 / IR-D6: navigate to the existing mapping, never mutate
            var existing = _mediator
                .Send(new GetBrgBarcodeQuery(_duplicateMapping.BrgBarcodeId))
                .GetAwaiter().GetResult();
            LoadExistingMapping(existing);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshWorklist();
        }

        private void CancelActionButton_Click(object sender, EventArgs e)
        {
            _editing = false;
            ClearForm();
            ApplyUiState();
        }

        #endregion

        #region BARCODE-INPUT

        private void BarcodeValueText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            LookupBarcode();
        }

        private void BarcodeValueText_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading || _loadedMapping != null)
                return;

            _duplicateMapping = null;
            if (_state == EntryState.DuplicateDetected)
                _state = EntryState.NewEntry;
            RecalculateState();
        }

        private void LookupBarcode()
        {
            var raw = BarcodeValueText.Text;
            var key = BrgBarcodeModel.BarcodeValueKey(raw);
            if (string.IsNullOrWhiteSpace(key))
            {
                _editing = true;
                RecalculateState();
                return;
            }

            _isLoading = true;
            BarcodeValueText.Text = BrgBarcodeModel.NormalizeBarcodeValue(raw);
            _isLoading = false;

            //  UC-001 resolution via the query surface; Active mappings only (BR-007)
            var active = _mediator.Send(new GetBrgBarcodeByValueQuery(raw))
                .GetAwaiter().GetResult();
            if (active != null)
            {
                LoadExistingMapping(active);
                return;
            }

            //  INV-02 / IR-D6 duplicate detection across all lifecycle states,
            //  never mutating state
            var existing = _brgBarcodeDal.GetByValue(key);
            if (existing != null)
            {
                _duplicateMapping = existing;
                _loadedMapping = null;
                _editing = true;
                _state = EntryState.DuplicateDetected;
                IsAktifLabel.Text = existing.IsAktif ? "Aktif" : "Non-Aktif";
                ApplyUiState();
                return;
            }

            _duplicateMapping = null;
            _loadedMapping = null;
            _editing = true;
            RecalculateState();
        }

        private void LoadExistingMapping(BrgBarcodeModel mapping)
        {
            _isLoading = true;
            try
            {
                _editing = true;
                _duplicateMapping = null;
                _loadedMapping = mapping;

                BrgBarcodeIdText.Text = mapping.BrgBarcodeId;
                BarcodeValueText.Text = mapping.BarcodeValue;
                BrgIdText.Text = mapping.BrgId;
                BrgCodeText.Text = mapping.BrgCode;
                BrgNameText.Text = mapping.BrgName;
                LoadSatuanCombo(mapping.BrgId, mapping.Satuan);
                IsAktifLabel.Text = mapping.IsAktif ? "Aktif" : "Non-Aktif";
                CreatedLabel.Text = FormatAudit(mapping.CreatedBy, mapping.CreatedDate);
                ModifiedLabel.Text = FormatAudit(mapping.ModifiedBy, mapping.ModifiedDate);
                _state = EntryState.ExistingLoaded;
            }
            finally
            {
                _isLoading = false;
            }

            ApplyUiState();
        }

        private static string FormatAudit(string user, DateTime date)
            => date == default
                ? string.Empty
                : $"{user} / {date:ddd, dd-MMM-yyyy HH:mm}";

        #endregion

        #region ITEM-SELECTION

        private void BrgButton_Click(object sender, EventArgs e)
        {
            BrgIdText.Text = _brgBrowser.Browse(BrgIdText.Text);
            BrgIdText_Validated(BrgIdText, null);
            RecalculateState();
        }

        private void BrgIdText_Validated(object sender, EventArgs e)
        {
            var brgId = (BrgIdText.Text ?? string.Empty).Trim();
            var previousItem = _loadedMapping?.BrgId;
            var previousSatuan = GetSelectedSatuan();

            if (string.IsNullOrWhiteSpace(brgId))
            {
                BrgCodeText.Clear();
                BrgNameText.Clear();
                LoadSatuanCombo(string.Empty);
                RecalculateState();
                return;
            }

            var brg = _brgDal.GetData(new BrgModel(brgId));
            if (brg is null)
            {
                BrgCodeText.Clear();
                BrgNameText.Clear();
                LoadSatuanCombo(string.Empty);
                MessageBox.Show($@"Item tidak ditemukan ({brgId}).", @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RecalculateState();
                return;
            }

            BrgCodeText.Text = brg.BrgCode;
            BrgNameText.Text = brg.BrgName;
            var satuan = string.Equals(brg.BrgId, previousItem, StringComparison.Ordinal)
                ? previousSatuan
                : string.Empty;
            LoadSatuanCombo(brg.BrgId, satuan);
            RecalculateState();
        }

        private void LoadSatuanCombo(string brgId, string selectedSatuan = "")
        {
            _isLoading = true;
            try
            {
                SatuanCombo.Items.Clear();
                SatuanCombo.Items.Add(string.Empty);
                if (!string.IsNullOrWhiteSpace(brgId))
                {
                    var listSatuan = _brgSatuanDal
                                         .ListData((IBrgKey)new BrgModel(brgId))?.ToList()
                                     ?? new List<BrgSatuanModel>();
                    foreach (var satuan in listSatuan.OrderBy(x => x.Conversion))
                        SatuanCombo.Items.Add(satuan.Satuan);
                }

                var index = SatuanCombo.Items.IndexOf(selectedSatuan ?? string.Empty);
                SatuanCombo.SelectedIndex = index >= 0 ? index : 0;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private string GetSelectedSatuan()
            => SatuanCombo.SelectedItem?.ToString() ?? string.Empty;

        private void SatuanCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            RecalculateState();
        }

        private void Field_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
            RecalculateState();
        }

        #endregion

        #region WORKLIST

        private void InitGrid()
        {
            var binding = new BindingSource();
            binding.DataSource = _listBarcode;
            BarcodeGrid.DataSource = binding;
            BarcodeGrid.AllowUserToAddRows = false;
            BarcodeGrid.AllowUserToDeleteRows = false;
            BarcodeGrid.ReadOnly = true;
            BarcodeGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            BarcodeGrid.MultiSelect = false;
            BarcodeGrid.Columns.SetDefaultCellStyle(Color.Beige);

            BarcodeGrid.Columns.GetCol("BrgBarcodeId").Visible = false;
            BarcodeGrid.Columns.GetCol("BrgId").Visible = false;
            BarcodeGrid.Columns.GetCol("ModifiedBy").Visible = false;
            BarcodeGrid.Columns.GetCol("BarcodeValue").Width = 180;
            BarcodeGrid.Columns.GetCol("BrgCode").Width = 100;
            BarcodeGrid.Columns.GetCol("BrgName").Width = 280;
            BarcodeGrid.Columns.GetCol("Satuan").Width = 80;
            BarcodeGrid.Columns.GetCol("IsAktif").HeaderText = "Status";
            BarcodeGrid.Columns.GetCol("IsAktif").Width = 90;
            BarcodeGrid.Columns.GetCol("ModifiedDate").Width = 120;
        }

        private void RefreshWorklist()
        {
            try
            {
                var keyword = SearchText.Text ?? string.Empty;
                var listBarcode = _mediator
                                      .Send(new ListBrgBarcodeQuery(keyword))
                                      .GetAwaiter().GetResult()
                                  ?? Enumerable.Empty<BrgBarcodeModel>();

                //  §20: worklist default load 200 rows
                var result = listBarcode
                    .OrderBy(x => x.BarcodeValue)
                    .Take(200)
                    .Select(x => new BrgBarcodeFormBarcodeDto(x.BrgBarcodeId,
                        x.BarcodeValue, x.BrgId, x.BrgCode, x.BrgName, x.Satuan,
                        x.IsAktif, x.ModifiedBy, x.ModifiedDate))
                    .ToList();

                _listBarcode.RaiseListChangedEvents = false;
                try
                {
                    _listBarcode.Clear();
                    foreach (var item in result)
                        _listBarcode.Add(item);
                }
                finally
                {
                    _listBarcode.RaiseListChangedEvents = true;
                    _listBarcode.ResetBindings();
                }

                BarcodeGrid.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            RefreshWorklist();
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            RefreshWorklist();
        }

        private void BarcodeGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var brgBarcodeId = BarcodeGrid.Rows[e.RowIndex].Cells["BrgBarcodeId"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(brgBarcodeId))
                return;

            var mapping = _mediator.Send(new GetBrgBarcodeQuery(brgBarcodeId))
                .GetAwaiter().GetResult();
            LoadExistingMapping(mapping);
        }

        #endregion
    }
}
