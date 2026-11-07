using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.Mvvm;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the on-the-spot barcode registration dialog (QuickBarcodeRegistrationDialog SCR-04).
    /// Auto-triggered when an unknown barcode is scanned to map the barcode to master product catalog,
    /// configure packaging unit and conversion multiplier, queue registration, and return resolved product.
    /// Traceable to: ARCHITECTURE §5.2 SCR-04, TD-006, TD-009, TD-013, P3-S07.
    /// </summary>
    public class QuickBarcodeRegistrationViewModel : ViewModelBase
    {
        private readonly IBarcodeService _barcodeService;
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        private string _barcodeValue = string.Empty;
        private string _searchKeyword = string.Empty;
        private Barang _selectedBarang;
        private string _selectedSatuan;
        private decimal _pengali = 1m;
        private string _statusMessage = string.Empty;
        private bool _isRegistering;
        private BarcodeLookupResult _registeredResult;

        public ObservableCollection<Barang> FilteredItems { get; } = new ObservableCollection<Barang>();
        public ObservableCollection<string> PackagingUnits { get; } = new ObservableCollection<string>();

        public event Action RequestCloseOk;
        public event Action RequestCloseCancel;

        public string BarcodeValue
        {
            get => _barcodeValue;
            set => SetProperty(ref _barcodeValue, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    _ = SearchItemsAsync(value);
                }
            }
        }

        public bool HasPackagingUnits => PackagingUnits.Count > 0 && SelectedBarang != null;

        public Barang SelectedBarang
        {
            get => _selectedBarang;
            set
            {
                if (SetProperty(ref _selectedBarang, value))
                {
                    UpdatePackagingUnits(value);
                    Pengali = _selectedBarang?.GetConversionForUnit(_selectedSatuan) ?? 1m;
                    OnPropertyChanged(nameof(HasPackagingUnits));
                }
            }
        }

        public string SelectedSatuan
        {
            get => _selectedSatuan;
            set
            {
                if (SetProperty(ref _selectedSatuan, value))
                {
                    Pengali = _selectedBarang?.GetConversionForUnit(value) ?? 1m;
                }
            }
        }

        public decimal Pengali
        {
            get => _pengali;
            set => SetProperty(ref _pengali, value <= 0 ? 1m : value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsRegistering
        {
            get => _isRegistering;
            private set => SetProperty(ref _isRegistering, value);
        }

        public BarcodeLookupResult RegisteredResult
        {
            get => _registeredResult;
            private set => SetProperty(ref _registeredResult, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public QuickBarcodeRegistrationViewModel(
            string barcode = null,
            IBarcodeService barcodeService = null,
            IBarcodeRepo barcodeRepo = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
        {
            _barcodeValue = barcode ?? string.Empty;
            _barcodeService = barcodeService;
            _barcodeRepo = barcodeRepo;
            _referenceRepo = referenceRepo;
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;

            SearchCommand = new AsyncRelayCommand(async () => await SearchItemsAsync(_searchKeyword));
            RegisterCommand = new AsyncRelayCommand(async () => await OnRegisterClickAsync());
            CancelCommand = new RelayCommand(Cancel);
        }

        public void PlayInitialWarning()
        {
            _audioHelper?.PlayWarning();
        }

        public async Task SearchItemsAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            try
            {
                var results = await _referenceRepo.SearchBarangByNameOrCodeAsync(keyword ?? string.Empty);
                var itemsList = (results ?? Enumerable.Empty<Barang>()).Take(50).ToList();

                FilteredItems.Clear();
                foreach (var b in itemsList)
                {
                    FilteredItems.Add(b);
                }

                if (FilteredItems.Count > 0)
                {
                    if (_selectedBarang == null || !FilteredItems.Any(i => i.BrgId == _selectedBarang.BrgId))
                    {
                        SelectedBarang = FilteredItems[0];
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gagal mencari barang: {ex.Message}";
            }
        }

        public void SetItems(IEnumerable<Barang> items)
        {
            var list = (items ?? Enumerable.Empty<Barang>()).ToList();
            FilteredItems.Clear();
            foreach (var b in list)
            {
                FilteredItems.Add(b);
            }

            if (FilteredItems.Count > 0)
            {
                SelectedBarang = FilteredItems[0];
            }
            else
            {
                SelectedBarang = null;
            }
        }

        public void OnItemSelected(Barang item)
        {
            SelectedBarang = item;
            Pengali = _selectedBarang?.GetConversionForUnit(_selectedSatuan) ?? 1m;
        }

        public void ResetForm()
        {
            BarcodeValue = string.Empty;
            SearchKeyword = string.Empty;
            FilteredItems.Clear();
            SelectedBarang = null;
            SelectedSatuan = null;
            Pengali = 1m;
            StatusMessage = string.Empty;
            RegisteredResult = null;
        }

        private void UpdatePackagingUnits(Barang barang)
        {
            PackagingUnits.Clear();
            if (barang != null)
            {
                foreach (var unit in barang.GetAvailableUnits())
                {
                    PackagingUnits.Add(unit);
                }

                SelectedSatuan = !string.IsNullOrWhiteSpace(barang.SatBesar) && PackagingUnits.Contains(barang.SatBesar)
                    ? barang.SatBesar
                    : (!string.IsNullOrWhiteSpace(barang.SatKecil) && PackagingUnits.Contains(barang.SatKecil)
                        ? barang.SatKecil
                        : PackagingUnits.FirstOrDefault());
            }
            else
            {
                SelectedSatuan = null;
            }

            OnPropertyChanged(nameof(HasPackagingUnits));
        }

        public async Task<bool> OnRegisterClickAsync()
        {
            string barcode = _barcodeValue?.Trim();
            if (string.IsNullOrWhiteSpace(barcode))
            {
                _audioHelper?.PlayWarning();
                StatusMessage = "Barcode tidak boleh kosong.";
                return false;
            }

            if (_selectedBarang == null)
            {
                _audioHelper?.PlayWarning();
                StatusMessage = "Silakan pilih barang master yang akan ditautkan.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedSatuan))
            {
                _audioHelper?.PlayWarning();
                StatusMessage = "Barang yang dipilih tidak memiliki satuan kemasan terdaftar.";
                return false;
            }

            string warehouseCode = _registryHelper?.WarehouseCode ?? "GDG-PST";
            string operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";
            string satuan = SelectedSatuan;
            decimal pengali = _pengali <= 0 ? 1m : _pengali;

            IsRegistering = true;
            try
            {
                if (_barcodeService != null)
                {
                    await _barcodeService.QuickRegisterUnmappedBarcodeAsync(
                        barcode,
                        _selectedBarang.BrgId,
                        satuan,
                        warehouseCode,
                        operatorName,
                        pengali);
                }
                else if (_barcodeRepo != null)
                {
                    var barcodeObj = new Barcode(barcode, _selectedBarang.BrgId, _selectedBarang.BrgName, satuan, pengali, true);
                    await _barcodeRepo.InsertBarcodeAsync(barcodeObj);

                    var reg = BarcodeRegistration.Create(barcode, _selectedBarang.BrgId, satuan, warehouseCode, operatorName);
                    await _barcodeRepo.QueueRegistrationAsync(reg);
                }

                RegisteredResult = BarcodeLookupResult.Hit(
                    rawInput: barcode,
                    normalizedBarcode: barcode,
                    brgId: _selectedBarang.BrgId,
                    brgName: _selectedBarang.BrgName,
                    satuan: satuan,
                    pengali: pengali,
                    isActive: true);

                _audioHelper?.PlaySuccess();
                RequestCloseOk?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                StatusMessage = $"Gagal mendaftarkan barcode: {ex.Message}";
                return false;
            }
            finally
            {
                IsRegistering = false;
            }
        }

        public void Cancel()
        {
            RequestCloseCancel?.Invoke();
        }
    }
}
