using btr.application.PurchaseContext.SupplierAgg.Contracts;
using btr.distrib.Browsers;
using btr.distrib.Helpers;
using btr.domain.BrgContext.JenisBrgAgg;
using btr.domain.BrgContext.KategoriAgg;
using btr.domain.PurchaseContext.SupplierAgg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using btr.application.BrgContext.BrgAgg;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.application.BrgContext.HargaTypeAgg;
using btr.application.BrgContext.JenisBrgAgg;
using btr.application.InventoryContext.StokBalanceAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.domain.BrgContext.HargaTypeAgg;
using btr.domain.InventoryContext.StokBalanceAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using Polly;
using btr.nuna.Domain;
using System.Drawing;
using btr.application.BrgContext.KategoriAgg;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.SupportContext.UserAgg;
using btr.distrib.InventoryContext.BrgBarcodeAgg;
using btr.distrib.SharedForm;
using ClosedXML.Excel;
using MediatR;
using Syncfusion.DataSource.Extensions;

namespace btr.distrib.InventoryContext.BrgAgg
{
    public partial class BrgForm : Form
    {
        private readonly IBrowser<SupplierBrowserView> _supplierBrowser;
        private readonly IBrowser<KategoriBrowserView> _kategoriBrowser;
        private readonly IBrowser<BrgBrowserView> _brgBrowser;

        private readonly ISupplierDal _supplierDal;
        private readonly IJenisBrgDal _jenisBrgDal;
        private readonly IKategoriDal _kategoriDal;
        private readonly IHargaTypeDal _hargaTypeDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly IBrgDal _brgDal;
        private readonly IBrgSatuanDal _brgSatuanDal;
        private readonly IBrgHargaDal _brgHargaDal;

        private readonly IBrgBuilder _brgBuilder;
        private readonly IStokBalanceBuilder _stokBalanceBuilder;

        private readonly IBrgWriter _writer;
        private readonly IUserDal _userDal;

        private readonly BindingList<BrgFormSatuanDto> _listSatuan = new BindingList<BrgFormSatuanDto>();
        private readonly BindingList<BrgFormHargaDto> _listHarga = new BindingList<BrgFormHargaDto>();
        private readonly BindingList<BrgFormStokDto> _listStok = new BindingList<BrgFormStokDto>();
        private readonly BindingList<BrgFormBrgDto> _listBrg = new BindingList<BrgFormBrgDto>();
        private readonly BindingList<BrgBarcodeFormBarcodeDto> _listBarcode = new BindingList<BrgBarcodeFormBarcodeDto>();

        private readonly IMediator _mediator;
        private readonly Dictionary<string, string> _barcodeSnapshot = new Dictionary<string, string>();

        private const string SystemAdministratorRoleId = "SYSAD";
        private static readonly string[] LifecycleRoleNames =
        {
            "Office Admin",
            "System Administrator"
        };

        private bool _canManageLifecycle;

        public BrgForm(
            IBrowser<SupplierBrowserView> supplierBrowser,
            IBrowser<KategoriBrowserView> kategoriBrowser,
            IBrowser<BrgBrowserView> brgBrowser,
            ISupplierDal supplierDal,
            IJenisBrgDal jenisBrgDal, 
            IKategoriDal kategoriDal, 
            IHargaTypeDal hargaTypeDal, 
            IWarehouseDal warehouseDal, 
            IBrgDal brgDal, 
            IBrgBuilder brgBuilder, 
            IStokBalanceBuilder stokBalanceBuilder, 
            IBrgWriter writer, IBrgSatuanDal brgSatuanDal, IBrgHargaDal brgHargaDal,
            IUserDal userDal, IMediator mediator)
        {
            InitializeComponent();
            RegisterEventHandler();

            _supplierBrowser = supplierBrowser;
            _kategoriBrowser = kategoriBrowser;

            _supplierDal = supplierDal;
            _jenisBrgDal = jenisBrgDal;
            _kategoriDal = kategoriDal;
            _hargaTypeDal = hargaTypeDal;
            _warehouseDal = warehouseDal;
            _brgDal = brgDal;
            _brgBuilder = brgBuilder;
            _stokBalanceBuilder = stokBalanceBuilder;
            _writer = writer;
            _brgSatuanDal = brgSatuanDal;
            _brgHargaDal = brgHargaDal;
            _brgBrowser = brgBrowser;
            _mediator = mediator;
            _userDal = userDal;

            InitJenisBrg();
            InitGridSatuan();
            InitGridHarga();
            InitGridStok();
            InitGridBrg();
            InitGridBarcode();
        }

        private void RegisterEventHandler()
        {
            Load += BrgForm_Load;

            SupplierButton.Click += SupplierButton_Click;
            SupplierIdText.Validated += SupplierIdText_Validated;

            KategoriButton.Click += KategoriButton_Click;
            KategoriIdText.Validated += KategoriIdText_Validated;

            BrgButton.Click += BrgButton_Click;
            BrgIdText.Validated += BrgIdText_Validated;
            
            SaveButton.Click += SaveButton_Click;
            NewButton.Click += NewButton_Click;
            SearchButton.Click += SearchButton_Click;
            SearchText.KeyDown += SearchText_KeyDown;
            BrgGrid.CellDoubleClick += BrgGrid_CellDoubleClick;
            
            ExcelButton.Click += ExcelButton_Click;

            AddBarcodeButton.Click += AddBarcodeButton_Click;
            EditBarcodeButton.Click += EditBarcodeButton_Click;
            ActivateBarcodeButton.Click += ActivateBarcodeButton_Click;
            DeactivateBarcodeButton.Click += DeactivateBarcodeButton_Click;
            BarcodeGrid.CellBeginEdit += BarcodeGrid_CellBeginEdit;
            BarcodeGrid.CellDoubleClick += BarcodeGrid_CellDoubleClick;
            BarcodeGrid.CellFormatting += BarcodeGrid_CellFormatting;
            BarcodeGrid.DataError += BarcodeGrid_DataError;
            BarcodeGrid.SelectionChanged += BarcodeGrid_SelectionChanged;

        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Export to Excel?", @"Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                return; 
            
            var listBrg = _brgDal.ListData()?.ToList() ?? new List<BrgModel>();
            var listBrgSat = _brgSatuanDal.ListData()?.ToList() ?? new List<BrgSatuanModel>();
            var listBrgHrg = _brgHargaDal.ListData()?.ToList() ?? new List<BrgHargaModel>();
            
            // projection listBrg, listBrgSat and listBrgHrg to BrgFormExcelDto using LINQ
            var listBrgExcel = listBrg
                .OrderBy(x => x.BrgName)
                .Select(x => new BrgFormExcelDto
                {
                    BrgId = x.BrgId,
                    BrgCode = x.BrgCode,
                    BrgName = x.BrgName,
                    Satuan1 = listBrgSat.FirstOrDefault(y => y.BrgId == x.BrgId && y.Conversion == 1)?.Satuan ?? string.Empty,
                    Hpp1 = x.Hpp,
                    HrgJual1Gt = listBrgHrg.FirstOrDefault(y => y.BrgId == x.BrgId && y.HargaTypeId == "GT")?.Harga ?? 0,
                    HrgJual1Mt = listBrgHrg.FirstOrDefault(y => y.BrgId == x.BrgId && y.HargaTypeId == "MT")?.Harga ?? 0,
                    HrgJual31 = listBrgHrg.FirstOrDefault(y => y.BrgId == x.BrgId && y.HargaTypeId == "H3")?.Harga ?? 0,
                    HrgJual41 = listBrgHrg.FirstOrDefault(y => y.BrgId == x.BrgId && y.HargaTypeId == "H4")?.Harga ?? 0,
                    Satuan2 = listBrgSat.FirstOrDefault(y => y.BrgId == x.BrgId && y.Conversion > 1)?.Satuan ?? string.Empty,
                    Conversion = listBrgSat.FirstOrDefault(y => y.BrgId == x.BrgId && y.Conversion > 1)?.Conversion ?? 0,
                    SupplierName = x.SupplierName,
                    KategoriName = x.KategoriName,
                    Aktif = x.IsAktif
                }).ToList();
            
            string filePath;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Excel Files|*.xlsx";
                saveFileDialog.Title = @"Save Excel File";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = $"brg-info-{DateTime.Now:yyyy-MM-dd-HHmm}";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                filePath = saveFileDialog.FileName;
            }

            using (IXLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet("brg-info")
                    .Cell($"B1")
                    .InsertTable(listBrgExcel, false);
                var ws = wb.Worksheets.First();
                //  add row number at column A
                ws.Cell("A1").Value = "No";
                for (var i = 0; i < listBrgExcel.Count; i++)
                    ws.Cell($"A{i + 2}").Value = i + 1;

                //  border header
                ws.Range("A1:T1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                //  font bold header and background color light blue
                ws.Range("A1:T1").Style.Font.SetBold();
                ws.Range("A1:T1").Style.Fill.BackgroundColor = XLColor.LightBlue;
                //  freeze header
                ws.SheetView.FreezeRows(1);
                //  border table
                ws.Range($"A2:T{listBrgExcel.Count + 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range($"A2:T{listBrgExcel.Count + 1}").Style.Border.InsideBorder = XLBorderStyleValues.Hair;
                
                //  format number thousand separator and zero decimal place
                ws.Range($"F2:P{listBrgExcel.Count + 1}").Style.NumberFormat.Format = "#,##";
                ws.Range($"A2:A{listBrgExcel.Count + 1}").Style.NumberFormat.Format = "#,##";
                
                ws.Range($"A1:T{listBrgExcel.Count + 1}").Style.Font.SetFontName("Lucida Console");
                ws.Range($"A1:T{listBrgExcel.Count + 1}").Style.Font.SetFontSize(9f);
                
                //  set backcolor column E to H as light yellow
                ws.Range($"E2:J{listBrgExcel.Count + 1}").Style.Fill.BackgroundColor = XLColor.LightYellow;
                ws.Range($"K2:P{listBrgExcel.Count + 1}").Style.Fill.BackgroundColor = XLColor.LightGreen;

                //  auto fit column
                ws.Columns().AdjustToContents();
                wb.SaveAs(filePath);
            }
            System.Diagnostics.Process.Start(filePath);
        }

        private void ClearForm()
        {
            BrgIdText.Clear();
            BrgNameText.Clear();
            BrgCodeText.Clear();
            SupplierIdText.Clear();
            SupplierNameText.Clear();
            JenisBrgCombo.SelectedIndex = 0;
            KategoriIdText.Clear();
            KategoriNameText.Clear();
            HppText.Value = 0;
            HppTimestampText.Value = new DateTime(3000, 1, 1);
            
            _listSatuan.Clear();
            ResetGridHarga();
            ResetGridStok();

            _listBarcode.Clear();
            _barcodeSnapshot.Clear();
            BarcodeGrid.Refresh();
        }

        private void ShowData(string brgId)
        {
            var fallback = Policy<BrgModel>
                .Handle<KeyNotFoundException>()
                .Fallback(new BrgModel());
            var brg = fallback.Execute(()
                => _brgBuilder.Load(new BrgModel(brgId)).Build());

            BrgIdText.Text = brgId;
            BrgNameText.Text = brg.BrgName;
            BrgCodeText.Text = brg.BrgCode;
            HppText.Value = brg.Hpp;
            HppTimestampText.Value = brg.HppTimestamp;
            SupplierIdText.Text = brg.SupplierId;
            SupplierNameText.Text = brg.SupplierName;
            JenisBrgCombo.SelectedItem = brg.JenisBrgId;
            KategoriIdText.Text = brg.KategoriId;
            KategoriNameText.Text = brg.KategoriName;
            IsAktifCheckBox.Checked = brg.IsAktif;

            //  satuan
            _listSatuan.Clear();
            brg.ListSatuan
                .OrderBy(x => x.Conversion).ToList()
                .ForEach(x => _listSatuan
                    .Add(new BrgFormSatuanDto(x.Satuan, x.Conversion, x.SatuanPrint)));
            SatuanGrid.Refresh();

            //  harga
            foreach (var item in _listHarga)
            {
                item.Clear();
                var harga = brg.ListHarga.FirstOrDefault(x => x.HargaTypeId == item.TypeId);
                if (harga is null)
                    continue;

                item.SetHpp(brg.Hpp);
                var marginRp = harga.Harga - harga.Hpp;
                item.Harga = harga.Harga;
            }
            HargaGrid.Refresh();

            //  stok
            var fallbackStok = Policy<StokBalanceModel>
                .Handle<KeyNotFoundException>()
                .Fallback(new StokBalanceModel
                {
                    ListWarehouse = new List<StokBalanceWarehouseModel>()
                });
            var stokBalance = fallbackStok.Execute(()
                => _stokBalanceBuilder.Load(new BrgModel(brgId)).Build());
            foreach (var item in _listStok)
            {
                item.SetQty(0);
                var stok = stokBalance.ListWarehouse
                    .FirstOrDefault(x => x.WarehouseId == item.Id);
                if (stok is null)
                    continue;
                item.SetQty(stok.Qty);
            }
            StokGrid.Refresh();

            //  SCR-DESK-002: all mappings for the loaded Item (UC-004)
            LoadBarcodes(brgId);
        }

        #region SEARCH-TEXT
        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchBrg();
        }
        private void SearchBrg()
        {
            var keyword = SearchText.Text;
            if (keyword.Length == 0)
            {
                InitGridBrg();
                return;
            }
            
            var resultName = _listBrg.Where(x => x.BrgName.ContainMultiWord(keyword)).ToList();
            var resultId = _listBrg.Where(x => x.Id.ToLower().StartsWith(keyword.ToLower())).ToList();
            var resultCode = _listBrg.Where(x => x.Code.ToLower().StartsWith(keyword.ToLower())).ToList();
            var result = resultName.Concat(resultId).Concat(resultCode).ToList();

            BrgGrid.DataSource = result;
            BrgGrid.Refresh();
        }
        private void SearchButton_Click(object sender, EventArgs e)
        {
            SearchBrg();
        }
        #endregion

        #region BRG
        private void BrgButton_Click(object sender, EventArgs e)
        {
            BrgIdText.Text = _brgBrowser.Browse(SupplierIdText.Text);
            BrgIdText_Validated(BrgIdText, null);
        }

        private void BrgIdText_Validated(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            if (textbox.Text.Length == 0)
            {
                ClearForm();
                return;
            }
            ShowData(textbox.Text);
        }

        #endregion
        
        #region SUPPLIER
        private void SupplierButton_Click(object sender, EventArgs e)
        {
            SupplierIdText.Text = _supplierBrowser.Browse(SupplierIdText.Text);
            SupplierIdText_Validated(SupplierIdText, null);
        }

        private void SupplierIdText_Validated(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            if (textbox.Text.Length == 0)
                return;

            var supplier = _supplierDal.GetData(new SupplierModel(textbox.Text));
            SupplierNameText.Text = supplier?.SupplierName ?? string.Empty;
        }

        #endregion

        #region JENIS-BRG
        private void InitJenisBrg()
        {
            var listJenisBrg = _jenisBrgDal.ListData()?.ToList() ?? new List<JenisBrgModel>();
            JenisBrgCombo.DataSource = listJenisBrg;
            JenisBrgCombo.DisplayMember = "JenisBrgName";
            JenisBrgCombo.ValueMember = "JenisBrgId";
        }
        #endregion

        #region KATEGORI-BRG
        private void KategoriButton_Click(object sender, EventArgs e)
        {
            KategoriIdText.Text = _kategoriBrowser.Browse(KategoriIdText.Text);
            KategoriIdText_Validated(KategoriIdText, null);
        }

        private void KategoriIdText_Validated(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            if (textbox.Text.Length == 0)
                return;

            var kategori = _kategoriDal.GetData(new KategoriModel(textbox.Text));
            KategoriNameText.Text = kategori?.KategoriName ?? string.Empty;
        }
        #endregion

        #region GRID-SATUAN
        private void InitGridSatuan()
        {
            var binding = new BindingSource();
            binding.DataSource = _listSatuan;
            SatuanGrid.DataSource = binding;
            SatuanGrid.Refresh();
            SatuanGrid.Columns.SetDefaultCellStyle(Color.Beige);
            SatuanGrid.Columns.GetCol("Satuan").Width = 80;
            SatuanGrid.Columns.GetCol("Conversion").Width = 80;
        }
        #endregion

        #region GRID-HARGA
        private void InitGridHarga()
        {
            ResetGridHarga();
            HargaGrid.AllowUserToAddRows = false;
            HargaGrid.AllowUserToDeleteRows = false;
            
            var binding = new BindingSource();
            binding.DataSource = _listHarga;
            HargaGrid.DataSource = binding;
            HargaGrid.Refresh();
            
            HargaGrid.Columns.SetDefaultCellStyle(Color.Beige);
            HargaGrid.Columns.GetCol("TypeId").Width = 50;
            HargaGrid.Columns.GetCol("Name").Width = 100;
            HargaGrid.Columns.GetCol("Margin").Width = 60;
            HargaGrid.Columns.GetCol("Harga").Width = 60;
            HargaGrid.Columns.GetCol("Keterangan").Width = 120;
            HargaGrid.Columns.GetCol("Hpp").Visible= false;
        }

        private void ResetGridHarga()
        {
            _listHarga.Clear();
            var listHargaType = _hargaTypeDal.ListData()?.ToList() ?? new List<HargaTypeModel>();
            var prio = listHargaType.Where(x => x.HargaTypeName.EndsWith("Trade")).ToList();
            var non = listHargaType.Where(x => !x.HargaTypeName.EndsWith("Trade")).ToList();
            foreach (var item in prio.Concat(non))
            {
                var harga = new BrgFormHargaDto();
                harga.TypeId = item.HargaTypeId;
                harga.SetName(item.HargaTypeName);
                _listHarga.Add(harga);
            }
            HargaGrid.Refresh();
        }
        #endregion
        
        #region GRID-STOK
        private void InitGridStok()
        {
            ResetGridStok();
            
            StokGrid.AllowUserToAddRows = false;
            StokGrid.AllowUserToDeleteRows = false;
            
            var binding = new BindingSource();
            binding.DataSource = _listStok;
            StokGrid.DataSource = binding;
            StokGrid.Refresh();
            
            StokGrid.Columns.SetDefaultCellStyle(Color.Beige);
            StokGrid.Columns.GetCol("Id").Width = 50;
            StokGrid.Columns.GetCol("WarehouseName").Width = 200;
            StokGrid.Columns.GetCol("Qty").Width = 60;
        }

        private void ResetGridStok()
        {
            _listStok.Clear();
            var listWarehouse = _warehouseDal.ListData()?.ToList() ?? new List<WarehouseModel>();
            foreach (var item in listWarehouse)
            {
                var brgStok = new BrgFormStokDto(
                    item.WarehouseId, item.WarehouseName, 0);
                _listStok.Add(brgStok);
            }
            StokGrid.Refresh();
        }
        #endregion

        #region GRID-BRG
        private void BrgGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (e.RowIndex < 0)
                ClearForm();
            else
            {
                var brgId = grid.Rows[e.RowIndex].Cells[0].Value.ToString();
                ShowData(brgId);
            }
        }

        private void InitGridBrg()
        {
            _listBrg.Clear();
            var listBrg = _brgDal.ListData()?.ToList() ?? new List<BrgModel>();
            var listHarga = _brgHargaDal.ListData();
            var listSatuan = _brgSatuanDal.ListData();
            foreach (var item in listBrg)
            {
                var harga = listHarga.Where(x => x.BrgId == item.BrgId).ToList();
                var konversi = listSatuan.Where(x => x.BrgId == item.BrgId).FirstOrDefault(y => y.Conversion != 1) ?? new BrgSatuanModel { Conversion = 1 };
                var hpp1 = item.Hpp;
                var gt = harga.FirstOrDefault(x => x.HargaTypeId == "GT") ?? new BrgHargaModel { Harga = 0 };
                var mt = harga.FirstOrDefault(x => x.HargaTypeId == "MT") ?? new BrgHargaModel { Harga = 0 };
                var h3 = harga.FirstOrDefault(x => x.HargaTypeId == "H3") ?? new BrgHargaModel { Harga = 0 };
                var h4 = harga.FirstOrDefault(x => x.HargaTypeId == "H4") ?? new BrgHargaModel { Harga = 0 };

                var hargaGt1 = gt.Harga;
                var hargaMt1 = mt.Harga;
                var hargaH31 = h3.Harga;
                var hargaH41 = h4.Harga;

                var hpp2 = konversi.Conversion != 1 ? hpp1 * konversi.Conversion : 0;
                var hargaGt2 = konversi.Conversion != 1 ?  hargaGt1 * konversi.Conversion : 0;
                var hargaMt2 = konversi.Conversion != 1 ? hargaMt1 * konversi.Conversion : 0;
                var harga32 = konversi.Conversion != 1 ? hargaH31 * konversi.Conversion : 0;
                var harga42 = konversi.Conversion != 1 ? hargaH41 * konversi.Conversion : 0;

                var brg = new BrgFormBrgDto(
                    item.BrgId, item.BrgCode, item.BrgName, item.KategoriName, item.SupplierName,
                    hpp1, hargaGt1, hargaMt1, hargaH31, hargaH41,
                    hpp2, hargaGt2, hargaMt2, harga32, harga42);
                _listBrg.Add(brg);
            }

            BrgGrid.AllowUserToAddRows = false;
            BrgGrid.AllowUserToDeleteRows = false;
            
            var binding = new BindingSource();
            binding.DataSource = _listBrg;
            BrgGrid.DataSource = binding;
            BrgGrid.Refresh();
            
            BrgGrid.Columns.SetDefaultCellStyle(Color.Beige);
            BrgGrid.Columns.GetCol("Id").Width = 50;
            BrgGrid.Columns.GetCol("Code").Width = 80;
            BrgGrid.Columns.GetCol("BrgName").Width = 150;
            BrgGrid.Columns.GetCol("Kategori").Width = 100;

            BrgGrid.Columns.GetCol("Hpp1").DefaultCellStyle.BackColor = Color.LightYellow;
            BrgGrid.Columns.GetCol("HargaGt1").DefaultCellStyle.BackColor = Color.LightYellow;
            BrgGrid.Columns.GetCol("HargaMt1").DefaultCellStyle.BackColor = Color.LightYellow;
            BrgGrid.Columns.GetCol("Harga31").DefaultCellStyle.BackColor = Color.LightYellow;
            BrgGrid.Columns.GetCol("Harga41").DefaultCellStyle.BackColor = Color.LightYellow;

            BrgGrid.Columns.GetCol("Hpp2").DefaultCellStyle.BackColor = Color.LightGreen;
            BrgGrid.Columns.GetCol("HargaGt2").DefaultCellStyle.BackColor = Color.LightGreen;
            BrgGrid.Columns.GetCol("HargaMt2").DefaultCellStyle.BackColor = Color.LightGreen;
            BrgGrid.Columns.GetCol("Harga32").DefaultCellStyle.BackColor = Color.LightYellow;
            BrgGrid.Columns.GetCol("Harga42").DefaultCellStyle.BackColor = Color.LightYellow;

            if (BrgGrid.Rows.Count == 0)
                return;
            BrgGrid.FirstDisplayedScrollingRowIndex = BrgGrid.Rows.Count - 1;
            BrgGrid.SetAlternatingRowColors();
            BrgGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
        }
        #endregion
        
        #region SAVE
        private void SaveButton_Click(object sender, EventArgs e)
        {
            BrgModel brg;
            if (BrgIdText.Text.IsNullOrEmpty())
                brg = _brgBuilder.Create().Build();
            else
                brg = _brgBuilder.Load(new BrgModel(BrgIdText.Text)).Build();

            var fallback = Policy<BrgModel>
                .Handle<KeyNotFoundException>()
                .Fallback(null as BrgModel, (result, context) => MessageBox.Show(result.Exception.Message));

            
            brg = fallback.Execute(() => _brgBuilder
                .Attach(brg)
                .BrgId(BrgIdText.Text)
                .Name(BrgNameText.Text)
                .Code(BrgCodeText.Text)
                .Hpp(HppText.Value)
                .Supplier(new SupplierModel(SupplierIdText.Text))
                .JenisBrg(new JenisBrgModel(JenisBrgCombo.SelectedValue.ToString()))
                .Kategori(new KategoriModel(KategoriIdText.Text))
                .IsAktif(IsAktifCheckBox.Checked)
                .Build());
            if (brg is null)
                return;

            brg.ListSatuan.Clear();
            foreach (var item in _listSatuan)
                brg = _brgBuilder
                    .Attach(brg)
                    .AddSatuan(item.Satuan, item.Conversion, item.SatuanPrint)
                    .Build();

            brg.ListHarga.Clear();
            foreach (var item in _listHarga)
                brg = _brgBuilder
                    .Attach(brg)
                    .AddHarga(new HargaTypeModel(item.TypeId), item.Harga)
                    .Build();

            _writer.Save(ref brg);

            //  SCR-DESK-002: barcode changes participate in the form's save;
            //  persistence goes through BrgBarcodeWriter (P-03)
            try
            {
                SaveBarcodeChanges(brg.BrgId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearForm();
            InitGridBrg();
        }
        #endregion

        #region GRID-BARCODE
        private void InitGridBarcode()
        {
            BarcodeGrid.AllowUserToAddRows = false;
            BarcodeGrid.AllowUserToDeleteRows = false;
            BarcodeGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            BarcodeGrid.MultiSelect = false;

            var binding = new BindingSource();
            binding.DataSource = _listBarcode;
            BarcodeGrid.DataSource = binding;
            BarcodeGrid.Refresh();

            BarcodeGrid.Columns.SetDefaultCellStyle(Color.Beige);
            BarcodeGrid.Columns.GetCol("BrgBarcodeId").Visible = false;
            BarcodeGrid.Columns.GetCol("BrgId").Visible = false;
            BarcodeGrid.Columns.GetCol("BrgCode").Visible = false;
            BarcodeGrid.Columns.GetCol("BrgName").Visible = false;
            BarcodeGrid.Columns.GetCol("ModifiedBy").Visible = false;

            BarcodeGrid.Columns.GetCol("BarcodeValue").HeaderText = "Barcode";
            BarcodeGrid.Columns.GetCol("BarcodeValue").Width = 170;
            BarcodeGrid.Columns.GetCol("IsAktif").HeaderText = "Status";
            BarcodeGrid.Columns.GetCol("IsAktif").Width = 80;
            BarcodeGrid.Columns.GetCol("IsAktif").ReadOnly = true;
            BarcodeGrid.Columns.GetCol("ModifiedDate").HeaderText = "Modified";
            BarcodeGrid.Columns.GetCol("ModifiedDate").Width = 130;
            BarcodeGrid.Columns.GetCol("ModifiedDate").ReadOnly = true;
            BarcodeGrid.Columns.GetCol("ModifiedDate").DefaultCellStyle.Format = "ddd, dd-MMM-yyyy HH:mm";

            //  Unit is restricted to the Item's own units (IR-01)
            var unitColumn = BarcodeGrid.Columns.GetCol("Satuan");
            var unitIndex = unitColumn.Index;
            BarcodeGrid.Columns.Remove(unitColumn);
            BarcodeGrid.Columns.Insert(unitIndex, new DataGridViewComboBoxColumn
            {
                Name = "Satuan",
                HeaderText = "Unit",
                DataPropertyName = "Satuan",
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            });
        }

        private void LoadBarcodes(string brgId)
        {
            _listBarcode.RaiseListChangedEvents = false;
            try
            {
                _listBarcode.Clear();
                _barcodeSnapshot.Clear();

                var listBarcode = _mediator
                                      .Send(new ListBrgBarcodeByBrgQuery(brgId))
                                      .GetAwaiter().GetResult()
                                  ?? Enumerable.Empty<BrgBarcodeModel>();
                foreach (var item in listBarcode.OrderBy(x => x.BarcodeValue))
                {
                    _listBarcode.Add(new BrgBarcodeFormBarcodeDto(item.BrgBarcodeId,
                        item.BarcodeValue, item.BrgId, item.BrgCode, item.BrgName,
                        item.Satuan, item.IsAktif, item.ModifiedBy, item.ModifiedDate));
                    _barcodeSnapshot[item.BrgBarcodeId] = item.Satuan ?? string.Empty;
                }
            }
            finally
            {
                _listBarcode.RaiseListChangedEvents = true;
                _listBarcode.ResetBindings();
            }

            LoadBarcodeUnitColumn(brgId);
            BarcodeGrid.Refresh();
            UpdateBarcodeButtons();
        }

        private void LoadBarcodeUnitColumn(string brgId)
        {
            if (!(BarcodeGrid.Columns.GetCol("Satuan") is DataGridViewComboBoxColumn unitColumn))
                return;

            var items = new List<string> { string.Empty };
            if (!string.IsNullOrWhiteSpace(brgId))
            {
                var listSatuan = _brgSatuanDal
                                     .ListData((IBrgKey)new BrgModel(brgId))?.ToList()
                                 ?? new List<BrgSatuanModel>();
                items.AddRange(listSatuan
                    .OrderBy(x => x.Conversion)
                    .Select(x => x.Satuan)
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            unitColumn.DataSource = items;
        }

        private void SaveBarcodeChanges(string brgId)
        {
            if (BarcodeGrid.IsCurrentCellInEditMode)
                BarcodeGrid.EndEdit();

            var userId = CurrentUserId;

            //  UC-002: new mappings are registered Active (INV-06)
            var newRows = _listBarcode
                .Where(x => string.IsNullOrWhiteSpace(x.BrgBarcodeId))
                .Where(x => !string.IsNullOrWhiteSpace(
                    BrgBarcodeModel.BarcodeValueKey(x.BarcodeValue)))
                .ToList();
            foreach (var row in newRows)
                _mediator.Send(new RegisterBrgBarcodeCommand(row.BarcodeValue, brgId,
                    row.Satuan ?? string.Empty, userId)).GetAwaiter().GetResult();

            //  UC-003: correction changes Unit only; identity is preserved (INV-07)
            var correctedRows = _listBarcode
                .Where(x => !string.IsNullOrWhiteSpace(x.BrgBarcodeId))
                .Where(x => _barcodeSnapshot.TryGetValue(x.BrgBarcodeId, out var satuan)
                            && !string.Equals(satuan, x.Satuan ?? string.Empty,
                                StringComparison.Ordinal))
                .ToList();
            foreach (var row in correctedRows)
                _mediator.Send(new CorrectBrgBarcodeCommand(row.BrgBarcodeId, brgId,
                    row.Satuan ?? string.Empty, userId)).GetAwaiter().GetResult();
        }

        private void AddBarcodeButton_Click(object sender, EventArgs e)
        {
            var brgId = (BrgIdText.Text ?? string.Empty).Trim();
            if (brgId.Length == 0)
            {
                MessageBox.Show(@"Pilih atau simpan Item terlebih dahulu.",
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _listBarcode.Add(new BrgBarcodeFormBarcodeDto(string.Empty, string.Empty,
                brgId, BrgCodeText.Text, BrgNameText.Text, string.Empty, true,
                string.Empty, default(DateTime)));
            BarcodeGrid.Refresh();

            var rowIndex = BarcodeGrid.Rows.Count - 1;
            BarcodeGrid.CurrentCell = BarcodeGrid.Rows[rowIndex].Cells["BarcodeValue"];
            BarcodeGrid.BeginEdit(true);
            UpdateBarcodeButtons();
        }

        private void EditBarcodeButton_Click(object sender, EventArgs e)
        {
            if (BarcodeGrid.CurrentRow is null)
                return;
            BeginEditBarcodeRow(BarcodeGrid.CurrentRow.Index);
        }

        private void BeginEditBarcodeRow(int rowIndex)
        {
            var row = BarcodeGrid.Rows[rowIndex];
            var columnName = string.IsNullOrWhiteSpace(GetRowBarcodeId(row))
                ? "BarcodeValue"
                : "Satuan";
            BarcodeGrid.CurrentCell = row.Cells[columnName];
            BarcodeGrid.BeginEdit(true);
        }

        private void ActivateBarcodeButton_Click(object sender, EventArgs e)
        {
            var row = SelectedBarcodeRow();
            if (row is null || string.IsNullOrWhiteSpace(row.BrgBarcodeId))
                return;
            if (!_canManageLifecycle)
                return;

            try
            {
                _mediator.Send(new ActivateBrgBarcodeCommand(row.BrgBarcodeId, CurrentUserId))
                    .GetAwaiter().GetResult();
                LoadBarcodes((BrgIdText.Text ?? string.Empty).Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeactivateBarcodeButton_Click(object sender, EventArgs e)
        {
            var row = SelectedBarcodeRow();
            if (row is null || string.IsNullOrWhiteSpace(row.BrgBarcodeId))
                return;
            if (!_canManageLifecycle)
                return;

            try
            {
                _mediator.Send(new DeactivateBrgBarcodeCommand(row.BrgBarcodeId, CurrentUserId))
                    .GetAwaiter().GetResult();
                LoadBarcodes((BrgIdText.Text ?? string.Empty).Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Validation Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BarcodeGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = BarcodeGrid.Rows[e.RowIndex];
            var columnName = BarcodeGrid.Columns[e.ColumnIndex].Name;

            //  the barcode value is never editable for a persisted mapping (INV-07)
            if (columnName == "BarcodeValue" && !string.IsNullOrWhiteSpace(GetRowBarcodeId(row)))
                e.Cancel = true;
        }

        private void BarcodeGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            BeginEditBarcodeRow(e.RowIndex);
        }

        private void BarcodeGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (BarcodeGrid.Columns[e.ColumnIndex].Name != "IsAktif")
                return;

            e.Value = e.Value is bool isAktif && isAktif ? "Aktif" : "Non-Aktif";
            e.FormattingApplied = true;
        }

        private void BarcodeGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //  unit values outside the Item's list are suppressed here; the
            //  authoritative validation happens in the Main Office (INV-05)
            e.ThrowException = false;
        }

        private void BarcodeGrid_SelectionChanged(object sender, EventArgs e)
            => UpdateBarcodeButtons();

        private void BrgForm_Load(object sender, EventArgs e)
        {
            //  BQ-6 / IR-D5: only Office Admin and System Administrator may
            //  manage the barcode lifecycle
            _canManageLifecycle = ResolveCanManageLifecycle();
            UpdateBarcodeButtons();
        }

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

        private void UpdateBarcodeButtons()
        {
            var row = SelectedBarcodeRow();
            var persisted = row != null && !string.IsNullOrWhiteSpace(row.BrgBarcodeId);

            EditBarcodeButton.Enabled = row != null;
            ActivateBarcodeButton.Enabled = persisted && !row.IsAktif && _canManageLifecycle;
            DeactivateBarcodeButton.Enabled = persisted && row.IsAktif && _canManageLifecycle;
        }

        private BrgBarcodeFormBarcodeDto SelectedBarcodeRow()
            => BarcodeGrid.CurrentRow?.DataBoundItem as BrgBarcodeFormBarcodeDto;

        private static string GetRowBarcodeId(DataGridViewRow row)
            => row.Cells["BrgBarcodeId"].Value?.ToString() ?? string.Empty;

        private string CurrentUserId
        {
            get
            {
                var mainForm = MdiParent as MainForm;
                return mainForm?.UserId?.UserId ?? string.Empty;
            }
        }
        #endregion

        #region NEW
        private void NewButton_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        #endregion
    }
}
    