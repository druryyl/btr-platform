using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Wpf.Mvvm;
using BtrReturGudang.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the primary Return Order Intake view (ReturnOrderEntryView SCR-02).
    /// Orchestrates customer/driver header, full-width high-contrast product banner,
    /// spacious quantity strip, live items grid, summary counters, and order persistence.
    /// Traceable to: ARCHITECTURE §4 TD-011, TD-013, §5.2 SCR-02, P2-S06.
    /// </summary>
    public class ReturnOrderEntryViewModel : ViewModelBase, IScannerConsumer, IOrderActionHandler
    {
        private readonly IReturnOrderService _returnOrderService;
        private readonly IBarcodeService _barcodeService;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;
        private readonly IRegistryHelper _registryHelper;
        private readonly OperatorSessionValidator _sessionValidator;

        // Header State
        private string _currentReturnOrderId;
        private Customer _selectedCustomer;
        private string _customerDisplayText;
        private string _customerInputText;
        private KeyValueItem _selectedDriver;
        private KeyValueItem _selectedSalesPerson;
        private string _keterangan;

        // Product Banner State
        private BarcodeLookupResult _currentScanResult;
        private string _productName = "Menunggu scan barcode scanner USB atau tekan F2 untuk pencarian produk...";
        private string _barcodeDetails = "Barcode: -  |  Satuan Default: -  |  Pengali: -";
        private string _bannerStatus = "STATUS: STANDBY";
        private bool _isBannerActive;
        private bool _isBannerError;

        // Quantity Strip State
        private string _selectedSatuan;
        private int _qtyBagus;
        private int _qtyRusak;
        private string _itemKeterangan;

        // Summary Counters
        private int _totalItemsCount;
        private string _totalItemsSummaryText = "Total Item: 0 baris";
        private decimal _totalQtyBagus;
        private string _totalQtyBagusText = "Total Qty Bagus: 0";
        private decimal _totalQtyRusak;
        private string _totalQtyRusakText = "Total Qty Rusak: 0";
        private decimal _totalKuantitas;
        private string _totalKuantitasText = "Total Kuantitas: 0";

        // Collections
        public ObservableCollection<KeyValueItem> Drivers { get; } = new ObservableCollection<KeyValueItem>();
        public ObservableCollection<KeyValueItem> SalesPersons { get; } = new ObservableCollection<KeyValueItem>();
        public ObservableCollection<string> PackagingUnits { get; } = new ObservableCollection<string>();
        public ObservableCollection<ReturnOrderItemDto> Items { get; } = new ObservableCollection<ReturnOrderItemDto>();

        // Selected grid item
        private ReturnOrderItemDto _selectedItem;
        public ReturnOrderItemDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        // Dialog hooks and automation configuration
        public bool SuppressMessageBoxes { get; set; } = false;
        public Func<string, BarcodeLookupResult> QuickRegistrationDialogLauncher { get; set; }
        public Func<Barang> ManualSearchDialogLauncher { get; set; }
        public Func<string, Customer> CustomerSearchDialogLauncher { get; set; }
        public Func<string, string, bool> ConfirmDialogHandler { get; set; }
        public Action<string, string, string> MessageDialogHandler { get; set; }

        // View Notification Events
        public event Action RequestFocusQtyBagus;
        public event Action RequestFocusGrid;

        /// <summary>
        /// DI Constructor resolving required service and repository contracts.
        /// </summary>
        public ReturnOrderEntryViewModel(
            IReturnOrderService returnOrderService,
            IBarcodeService barcodeService,
            IReferenceRepo referenceRepo,
            IAudioHelper audioHelper,
            IRegistryHelper registryHelper,
            OperatorSessionValidator sessionValidator = null)
        {
            _returnOrderService = returnOrderService;
            _barcodeService = barcodeService;
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;
            _registryHelper = registryHelper;
            _sessionValidator = sessionValidator;

            // Commands
            AddItemCommand = new RelayCommand(ExecuteAddItem, CanAddItem);
            DeleteItemCommand = new RelayCommand<ReturnOrderItemDto>(DeleteItem);
            SaveDraftCommand = new AsyncRelayCommand(async () => await SaveDraftAsync());
            SaveAndUploadCommand = new AsyncRelayCommand(async () => await SaveAndUploadAsync());
            ResetFormCommand = new RelayCommand(() => ResetForm(promptUser: true));
            ManualSearchCommand = new RelayCommand(TriggerManualItemSearch);
            CustomerSearchCommand = new RelayCommand(() => TriggerCustomerSearch(CustomerInputText));

            LoadReferenceData();
            UpdateSummaryFooter();
        }

        /// <summary>
        /// Parameterless constructor for designer and DI resolution fallback.
        /// </summary>
        public ReturnOrderEntryViewModel() : this(
            App.ServiceProvider?.GetService<IReturnOrderService>(),
            App.ServiceProvider?.GetService<IBarcodeService>(),
            App.ServiceProvider?.GetService<IReferenceRepo>(),
            App.ServiceProvider?.GetService<IAudioHelper>(),
            App.ServiceProvider?.GetService<IRegistryHelper>(),
            App.ServiceProvider?.GetService<OperatorSessionValidator>())
        {
        }

        #region Commands

        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand SaveDraftCommand { get; }
        public ICommand SaveAndUploadCommand { get; }
        public ICommand ResetFormCommand { get; }
        public ICommand ManualSearchCommand { get; }
        public ICommand CustomerSearchCommand { get; }

        #endregion

        #region Header Properties

        public string CurrentReturnOrderId
        {
            get => _currentReturnOrderId;
            set => SetProperty(ref _currentReturnOrderId, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (value != null)
                    {
                        var codePart = !string.IsNullOrWhiteSpace(value.CustomerCode)
                            ? $"[{value.CustomerCode}] "
                            : (!string.IsNullOrWhiteSpace(value.CustomerId) ? $"[{value.CustomerId}] " : string.Empty);
                        var regionPart = !string.IsNullOrWhiteSpace(value.Wilayah) ? $" - {value.Wilayah}" : string.Empty;
                        CustomerDisplayText = $"{codePart}{value.CustomerName}{regionPart}";
                        CustomerInputText = CustomerDisplayText;
                    }
                }
            }
        }

        public string CustomerDisplayText
        {
            get => _customerDisplayText;
            set => SetProperty(ref _customerDisplayText, value);
        }

        public string CustomerInputText
        {
            get => _customerInputText;
            set => SetProperty(ref _customerInputText, value);
        }

        public KeyValueItem SelectedDriver
        {
            get => _selectedDriver;
            set => SetProperty(ref _selectedDriver, value);
        }

        public KeyValueItem SelectedSalesPerson
        {
            get => _selectedSalesPerson;
            set => SetProperty(ref _selectedSalesPerson, value);
        }

        public string Keterangan
        {
            get => _keterangan;
            set => SetProperty(ref _keterangan, value);
        }

        #endregion

        #region Product Banner Properties

        public BarcodeLookupResult CurrentScanResult => _currentScanResult;

        public string ProductName
        {
            get => _productName;
            private set => SetProperty(ref _productName, value);
        }

        public string BarcodeDetails
        {
            get => _barcodeDetails;
            private set => SetProperty(ref _barcodeDetails, value);
        }

        public string BannerStatus
        {
            get => _bannerStatus;
            private set => SetProperty(ref _bannerStatus, value);
        }

        public bool IsBannerActive
        {
            get => _isBannerActive;
            private set => SetProperty(ref _isBannerActive, value);
        }

        public bool IsBannerError
        {
            get => _isBannerError;
            private set => SetProperty(ref _isBannerError, value);
        }

        #endregion

        #region Quantity Strip Properties

        public string SelectedSatuan
        {
            get => _selectedSatuan;
            set => SetProperty(ref _selectedSatuan, value);
        }

        public int QtyBagus
        {
            get => _qtyBagus;
            set
            {
                if (SetProperty(ref _qtyBagus, value))
                {
                    (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int QtyRusak
        {
            get => _qtyRusak;
            set
            {
                if (SetProperty(ref _qtyRusak, value))
                {
                    (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ItemKeterangan
        {
            get => _itemKeterangan;
            set => SetProperty(ref _itemKeterangan, value);
        }

        #endregion

        #region Summary Properties

        public int TotalItemsCount
        {
            get => _totalItemsCount;
            private set => SetProperty(ref _totalItemsCount, value);
        }

        public string TotalItemsSummaryText
        {
            get => _totalItemsSummaryText;
            private set => SetProperty(ref _totalItemsSummaryText, value);
        }

        public decimal TotalQtyBagus
        {
            get => _totalQtyBagus;
            private set => SetProperty(ref _totalQtyBagus, value);
        }

        public string TotalQtyBagusText
        {
            get => _totalQtyBagusText;
            private set => SetProperty(ref _totalQtyBagusText, value);
        }

        public decimal TotalQtyRusak
        {
            get => _totalQtyRusak;
            private set => SetProperty(ref _totalQtyRusak, value);
        }

        public string TotalQtyRusakText
        {
            get => _totalQtyRusakText;
            private set => SetProperty(ref _totalQtyRusakText, value);
        }

        public decimal TotalKuantitas
        {
            get => _totalKuantitas;
            private set => SetProperty(ref _totalKuantitas, value);
        }

        public string TotalKuantitasText
        {
            get => _totalKuantitasText;
            private set => SetProperty(ref _totalKuantitasText, value);
        }

        #endregion

        #region Reference Data Loading

        public void LoadReferenceData()
        {
            if (_referenceRepo == null) return;

            try
            {
                // 1. Drivers
                Drivers.Clear();
                Drivers.Add(new KeyValueItem(null, null, "-- Pilih Driver (Opsional) --"));
                var drivers = _referenceRepo.GetAllDrivers() ?? Enumerable.Empty<Driver>();
                foreach (var drv in drivers)
                {
                    Drivers.Add(new KeyValueItem(drv.DriverId, drv.DriverName));
                }
                SelectedDriver = Drivers.FirstOrDefault();

                // 2. Salespersons
                SalesPersons.Clear();
                SalesPersons.Add(new KeyValueItem(null, null, "-- Pilih Salesman (Opsional) --"));
                var salesPersons = _referenceRepo.GetAllSalesPersons() ?? Enumerable.Empty<SalesPerson>();
                foreach (var sp in salesPersons)
                {
                    SalesPersons.Add(new KeyValueItem(sp.SalesPersonId, sp.SalesPersonName));
                }
                SelectedSalesPerson = SalesPersons.FirstOrDefault();
            }
            catch
            {
                // Fallback for offline/disconnected mode
            }
        }

        #endregion

        #region IScannerConsumer Implementation

        public void OnBarcodeScanned(string normalizedBarcode)
        {
            if (string.IsNullOrWhiteSpace(normalizedBarcode))
                return;

            var clean = BarcodeNormalization.Normalize(normalizedBarcode);

            // High-load warehouse ergonomics: If scanning the same product consecutively before commit, increment Qty Bagus
            if (_currentScanResult != null &&
                ((!string.IsNullOrWhiteSpace(_currentScanResult.NormalizedBarcode) && _currentScanResult.NormalizedBarcode == clean) ||
                 (!string.IsNullOrWhiteSpace(_currentScanResult.BarcodeValue) && _currentScanResult.BarcodeValue == clean)))
            {
                if (QtyBagus == 0)
                    QtyBagus = 2;
                else
                    QtyBagus += 1;

                BannerStatus = $"QTY BAGUS: {QtyBagus}";
                IsBannerActive = true;
                IsBannerError = false;
                _audioHelper?.PlaySuccess();
                RequestFocusQtyBagus?.Invoke();
                return;
            }

            var lookup = _barcodeService?.LookupBarcode(clean);
            if (lookup != null && lookup.Found)
            {
                ApplyActiveProduct(lookup);
                _audioHelper?.PlaySuccess();
            }
            else
            {
                _currentScanResult = null;
                ProductName = $"⚠️ Barcode tidak dikenal: [{clean}]";
                BarcodeDetails = "Item tidak terdaftar di katalog lokal. Tekan F2 untuk cari barang manual.";
                BannerStatus = "TIDAK DITEMUKAN";
                IsBannerActive = false;
                IsBannerError = true;
                _audioHelper?.PlayWarning();

                // Trigger QuickBarcodeRegistrationDialog launcher if provided
                if (QuickRegistrationDialogLauncher != null)
                {
                    var registered = QuickRegistrationDialogLauncher(clean);
                    if (registered != null)
                    {
                        ApplyActiveProduct(registered);
                        _audioHelper?.PlaySuccess();
                    }
                }
            }
        }

        public void ApplyActiveProduct(BarcodeLookupResult result)
        {
            if (result == null) return;
            _currentScanResult = result;

            // Pure BrgName without internal code noise in 14pt bold text
            ProductName = result.BrgName;

            var barcodeDisplay = !string.IsNullOrWhiteSpace(result.NormalizedBarcode)
                ? result.NormalizedBarcode
                : (!string.IsNullOrWhiteSpace(result.BarcodeValue) ? result.BarcodeValue : "(Manual / Tanpa Barcode)");

            BarcodeDetails = $"Barcode: {barcodeDisplay}  |  Satuan Default: {result.Satuan ?? "-"}  |  Pengali: x{result.Pengali:N0}";
            BannerStatus = "BARCODE TERDETEKSI";
            IsBannerActive = true;
            IsBannerError = false;

            PopulatePackagingUnits(result);

            QtyBagus = 0;
            QtyRusak = 0;
            ItemKeterangan = string.Empty;

            RequestFocusQtyBagus?.Invoke();
        }

        private void PopulatePackagingUnits(BarcodeLookupResult lookup)
        {
            PackagingUnits.Clear();
            var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(lookup.Satuan))
            {
                units.Add(lookup.Satuan);
            }

            // Query product definition for alternative packaging units
            if (_referenceRepo != null && !string.IsNullOrWhiteSpace(lookup.BrgId))
            {
                try
                {
                    var barang = _referenceRepo.GetBarangById(lookup.BrgId);
                    if (barang != null)
                    {
                        if (!string.IsNullOrWhiteSpace(barang.Satuan1)) units.Add(barang.Satuan1);
                        if (!string.IsNullOrWhiteSpace(barang.Satuan2)) units.Add(barang.Satuan2);
                        if (!string.IsNullOrWhiteSpace(barang.Satuan3)) units.Add(barang.Satuan3);
                    }
                }
                catch
                {
                    // Fallback
                }
            }

            if (units.Count == 0)
            {
                units.Add("Pcs");
            }

            foreach (var u in units)
            {
                PackagingUnits.Add(u);
            }

            if (!string.IsNullOrWhiteSpace(lookup.Satuan) && PackagingUnits.Contains(lookup.Satuan))
            {
                SelectedSatuan = lookup.Satuan;
            }
            else
            {
                SelectedSatuan = PackagingUnits.FirstOrDefault();
            }
        }

        #endregion

        #region Item Intake & Live Grid Commits

        private bool CanAddItem()
        {
            return _currentScanResult != null && (QtyBagus > 0 || QtyRusak > 0);
        }

        private void ExecuteAddItem()
        {
            CommitQuantityStripToGrid();
        }

        public bool CommitQuantityStripToGrid()
        {
            if (_currentScanResult == null)
            {
                _audioHelper?.PlayWarning();
                ShowMessage("Silakan scan barcode terlebih dahulu atau pilih item produk.", "Informasi", "Warning");
                return false;
            }

            if (QtyBagus <= 0 && QtyRusak <= 0)
            {
                _audioHelper?.PlayWarning();
                ShowMessage("Masukkan kuantitas Qty Bagus atau Qty Rusak minimal 1.", "Peringatan", "Warning");
                return false;
            }

            var satuan = !string.IsNullOrWhiteSpace(SelectedSatuan)
                ? SelectedSatuan
                : (_currentScanResult.Satuan ?? "Pcs");

            var itemNotes = ItemKeterangan?.Trim() ?? string.Empty;

            // Add Qty Bagus line if specified
            if (QtyBagus > 0)
            {
                var rowBagus = new ReturnOrderItemDto
                {
                    ReturnOrderId = CurrentReturnOrderId,
                    NoUrut = Items.Count + 1,
                    BrgId = _currentScanResult.BrgId,
                    BrgCode = _currentScanResult.BrgId,
                    BrgName = _currentScanResult.BrgName,
                    Barcode = _currentScanResult.BarcodeValue ?? _currentScanResult.NormalizedBarcode,
                    JenisRetur = "BAGUS",
                    Qty = QtyBagus,
                    SatId = satuan,
                    Keterangan = itemNotes
                };
                Items.Add(rowBagus);
            }

            // Add Qty Rusak line if specified
            if (QtyRusak > 0)
            {
                var rowRusak = new ReturnOrderItemDto
                {
                    ReturnOrderId = CurrentReturnOrderId,
                    NoUrut = Items.Count + 1,
                    BrgId = _currentScanResult.BrgId,
                    BrgCode = _currentScanResult.BrgId,
                    BrgName = _currentScanResult.BrgName,
                    Barcode = _currentScanResult.BarcodeValue ?? _currentScanResult.NormalizedBarcode,
                    JenisRetur = "RUSAK",
                    Qty = QtyRusak,
                    SatId = satuan,
                    Keterangan = itemNotes
                };
                Items.Add(rowRusak);
            }

            // Trigger success audio cue
            _audioHelper?.PlaySuccess();

            // Reset quantity strip and active scanned item
            _currentScanResult = null;
            QtyBagus = 0;
            QtyRusak = 0;
            ItemKeterangan = string.Empty;

            // Update banner & summaries
            UpdateSummaryFooter();
            BannerStatus = "ITEM DITAMBAHKAN";
            IsBannerActive = false;
            IsBannerError = false;

            RequestFocusGrid?.Invoke();
            return true;
        }

        public void DeleteItem(ReturnOrderItemDto item)
        {
            if (item == null || !Items.Contains(item)) return;

            Items.Remove(item);

            // Re-sequence line numbers
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].NoUrut = i + 1;
            }

            UpdateSummaryFooter();
        }

        public void DeleteItemAt(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            DeleteItem(Items[index]);
        }

        public void UpdateSummaryFooter()
        {
            TotalQtyBagus = Items
                .Where(i => string.Equals(i.JenisRetur, "BAGUS", StringComparison.OrdinalIgnoreCase))
                .Sum(i => i.Qty);

            TotalQtyRusak = Items
                .Where(i => string.Equals(i.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase))
                .Sum(i => i.Qty);

            TotalKuantitas = TotalQtyBagus + TotalQtyRusak;
            TotalItemsCount = Items.Count;

            TotalItemsSummaryText = $"Total Item: {TotalItemsCount} baris";
            TotalQtyBagusText = $"Total Qty Bagus: {TotalQtyBagus:N0}";
            TotalQtyRusakText = $"Total Qty Rusak: {TotalQtyRusak:N0}";
            TotalKuantitasText = $"Total Kuantitas: {TotalKuantitas:N0}";
        }

        #endregion

        #region Actions (Save Draft, Save & Upload, Reset)

        public async Task<ReturnOrderDto> SaveDraftAsync()
        {
            var (custId, custName, customerCode, wilayah) = GetSelectedCustomer();
            if (string.IsNullOrWhiteSpace(custName))
            {
                _audioHelper?.PlayWarning();
                ShowMessage("Silakan pilih atau masukkan Customer terlebih dahulu (wajib).", "Peringatan", "Warning");
                return null;
            }

            if (Items.Count == 0)
            {
                _audioHelper?.PlayWarning();
                ShowMessage("Belum ada item retur yang dimasukkan ke dalam daftar.", "Peringatan", "Warning");
                return null;
            }

            if (_returnOrderService == null)
            {
                ShowMessage("Layanan ReturnOrderService belum terhubung.", "Peringatan", "Warning");
                return null;
            }

            var driver = SelectedDriver;
            var salesman = SelectedSalesPerson;
            var notes = Keterangan?.Trim();

            var warehouseCode = _registryHelper?.WarehouseCode;
            var operatorName = _registryHelper?.LastOperatorName;

            if (string.IsNullOrWhiteSpace(warehouseCode) || string.IsNullOrWhiteSpace(operatorName))
            {
                _audioHelper?.PlayWarning();
                ShowMessage(
                    "Identitas workstation belum dikonfigurasi. Lengkapi 'Kode Depo / Warehouse' dan 'Operator Shift Aktif' pada menu [F5] Pengaturan sebelum menyimpan Return Order.",
                    "Workstation Belum Dikonfigurasi",
                    "Warning");
                return null;
            }

            ReturnOrderDto order;
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentReturnOrderId))
                {
                    order = await _returnOrderService.CreateAsync(
                        custId,
                        custName,
                        warehouseCode,
                        operatorName,
                        driver?.Id,
                        driver?.Name,
                        salesman?.Id,
                        salesman?.Name,
                        notes,
                        customerCode,
                        wilayah,
                        Items);

                    CurrentReturnOrderId = order.ReturnOrderId;
                }
                else
                {
                    order = await _returnOrderService.UpdateAsync(
                        CurrentReturnOrderId,
                        custId,
                        custName,
                        driver?.Id,
                        driver?.Name,
                        salesman?.Id,
                        salesman?.Name,
                        notes,
                        customerCode,
                        wilayah,
                        Items);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                ShowMessage($"Gagal menyimpan draft Return Order: {ex.Message}", "Kesalahan", "Error");
                return null;
            }

            _audioHelper?.PlaySuccess();
            ShowMessage($"Draft Return Order '{CurrentReturnOrderId}' berhasil disimpan secara lokal.", "Sukses", "Information");
            return order;
        }

        public async Task<SyncResultDto> SaveAndUploadAsync()
        {
            var draftOrder = await SaveDraftAsync();
            if (draftOrder == null)
                return SyncResultDto.Fail("ReturnOrder", "Gagal menyimpan draft.");

            if (_returnOrderService == null)
                return SyncResultDto.Fail("ReturnOrder", "Service tidak tersedia.");

            // Pre-empt Cloud upload when the configured operator identity is unresolved
            if (_sessionValidator != null && _sessionValidator.IsUnresolved)
            {
                _audioHelper?.PlayWarning();
                var refusal = _sessionValidator.RefusalMessage;
                ShowMessage(
                    $"Return Order '{CurrentReturnOrderId}' tersimpan sebagai Draft LOKAL.\r\nUpload Cloud dibatalkan: {refusal}",
                    "Operator Belum Terverifikasi",
                    "Warning");
                return SyncResultDto.Fail("ReturnOrder", refusal);
            }

            var syncResult = await _returnOrderService.CommitAndUploadAsync(CurrentReturnOrderId);
            if (syncResult != null && syncResult.Success)
            {
                _audioHelper?.PlaySuccess();
                ShowMessage($"Return Order '{CurrentReturnOrderId}' berhasil disimpan dan di-upload ke server Cloud!", "Upload Berhasil", "Information");
                ResetForm(promptUser: false);
            }
            else
            {
                _audioHelper?.PlayWarning();
                var err = syncResult?.ErrorMessage ?? "Koneksi offline atau server relay tidak merespon.";
                ShowMessage(
                    $"Return Order '{CurrentReturnOrderId}' tersimpan sebagai Draft LOKAL.\r\nUpload Cloud belum selesai: {err}\r\nDokumen akan di-upload otomatis saat online.",
                    "Tersimpan Lokal",
                    "Warning");
                ResetForm(promptUser: false);
            }

            return syncResult;
        }

        public void ResetForm(bool promptUser = true)
        {
            if (promptUser && !SuppressMessageBoxes && Items.Count > 0)
            {
                bool confirmed = PromptConfirm(
                    "Apakah Anda yakin ingin membatalkan transaksi ini dan mengosongkan form?",
                    "Konfirmasi Reset Form");

                if (!confirmed)
                    return;
            }

            CurrentReturnOrderId = null;
            SelectedCustomer = null;
            CustomerDisplayText = string.Empty;
            CustomerInputText = string.Empty;

            if (Drivers.Count > 0) SelectedDriver = Drivers[0];
            if (SalesPersons.Count > 0) SelectedSalesPerson = SalesPersons[0];

            Keterangan = string.Empty;

            _currentScanResult = null;
            ProductName = "Menunggu scan barcode scanner USB atau tekan F2 untuk pencarian produk...";
            BarcodeDetails = "Barcode: -  |  Satuan Default: -  |  Pengali: -";
            BannerStatus = "STATUS: STANDBY";
            IsBannerActive = false;
            IsBannerError = false;

            PackagingUnits.Clear();
            QtyBagus = 0;
            QtyRusak = 0;
            ItemKeterangan = string.Empty;

            Items.Clear();
            UpdateSummaryFooter();
        }

        public void LoadOrder(ReturnOrderDto order)
        {
            if (order == null) return;

            CurrentReturnOrderId = order.ReturnOrderId;
            Keterangan = order.Keterangan ?? string.Empty;

            // Set customer
            if (!string.IsNullOrWhiteSpace(order.CustId))
            {
                Customer customer = null;
                try
                {
                    customer = _referenceRepo?.GetCustomerById(order.CustId);
                }
                catch { }

                if (customer == null)
                {
                    customer = new Customer(
                        order.CustId,
                        order.CustName,
                        null,
                        order.CustomerCode,
                        order.Wilayah);
                }
                SetSelectedCustomer(customer);
            }
            else if (!string.IsNullOrWhiteSpace(order.CustName))
            {
                _selectedCustomer = null;
                CustomerDisplayText = order.CustName;
                CustomerInputText = order.CustName;
            }
            else
            {
                _selectedCustomer = null;
                CustomerDisplayText = string.Empty;
                CustomerInputText = string.Empty;
            }

            // Set driver
            if (!string.IsNullOrWhiteSpace(order.DriverId))
            {
                SelectedDriver = Drivers.FirstOrDefault(d => d.Id == order.DriverId) ?? Drivers.FirstOrDefault();
            }

            // Set salesman
            if (!string.IsNullOrWhiteSpace(order.SalesPersonId))
            {
                SelectedSalesPerson = SalesPersons.FirstOrDefault(s => s.Id == order.SalesPersonId) ?? SalesPersons.FirstOrDefault();
            }

            // Load items
            Items.Clear();
            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    Items.Add(item);
                }
            }

            UpdateSummaryFooter();
        }

        public void SetSelectedItem(Barang item, string selectedSatuan = null)
        {
            if (item == null) return;

            string effectiveSatuan = selectedSatuan;
            if (string.IsNullOrWhiteSpace(effectiveSatuan))
            {
                effectiveSatuan = item.Satuan1 ?? "Pcs";
            }

            var lookup = BarcodeLookupResult.Hit(
                rawInput: null,
                normalizedBarcode: null,
                brgId: item.BrgId,
                brgName: item.BrgName,
                satuan: effectiveSatuan,
                pengali: 1m,
                isActive: true);

            ApplyActiveProduct(lookup);
            _audioHelper?.PlaySuccess();
        }

        public void TriggerManualItemSearch()
        {
            if (ManualSearchDialogLauncher != null)
            {
                var item = ManualSearchDialogLauncher();
                if (item != null)
                {
                    SetSelectedItem(item);
                }
            }
        }

        public void SetSelectedCustomer(Customer customer)
        {
            SelectedCustomer = customer;
        }

        public void TriggerCustomerSearch(string initialKeyword = null)
        {
            if (CustomerSearchDialogLauncher != null)
            {
                var customer = CustomerSearchDialogLauncher(initialKeyword);
                if (customer != null)
                {
                    SetSelectedCustomer(customer);
                }
            }
        }

        public (string CustId, string CustName, string CustomerCode, string Wilayah) GetSelectedCustomer()
        {
            if (_selectedCustomer != null)
            {
                return (_selectedCustomer.CustomerId, _selectedCustomer.CustomerName, _selectedCustomer.CustomerCode, _selectedCustomer.Wilayah);
            }

            var text = !string.IsNullOrWhiteSpace(CustomerInputText)
                ? CustomerInputText.Trim()
                : (!string.IsNullOrWhiteSpace(CustomerDisplayText) ? CustomerDisplayText.Trim() : null);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return ("CUST-MANUAL", text, null, null);
            }

            return (null, null, null, null);
        }

        #endregion

        #region IOrderActionHandler Implementation

        public void HandleSaveDraft()
        {
            _ = SaveDraftAsync();
        }

        public void HandleSaveAndUpload()
        {
            _ = SaveAndUploadAsync();
        }

        public void HandleCancelOrClear()
        {
            ResetForm(promptUser: true);
        }

        #endregion

        #region Private Dialog Helpers

        private void ShowMessage(string message, string title, string iconType)
        {
            if (SuppressMessageBoxes) return;

            if (MessageDialogHandler != null)
            {
                MessageDialogHandler(message, title, iconType);
                return;
            }

            MessageBoxImage img = MessageBoxImage.Information;
            if (iconType == "Warning") img = MessageBoxImage.Warning;
            else if (iconType == "Error") img = MessageBoxImage.Error;

            MessageBox.Show(message, title, MessageBoxButton.OK, img);
        }

        private bool PromptConfirm(string message, string title)
        {
            if (ConfirmDialogHandler != null)
            {
                return ConfirmDialogHandler(message, title);
            }

            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        #endregion
    }
}
