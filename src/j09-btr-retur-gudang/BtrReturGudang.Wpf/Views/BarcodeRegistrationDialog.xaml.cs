using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for BarcodeRegistrationDialog.xaml.
    /// Modal dialog for registering a new barcode or linking an unmapped barcode to a master product.
    /// Traceable to: ARCHITECTURE §4 TD-013 (580x480 low-res viewport), §5.2 SCR-04/SCR-06, P4-S10.
    /// </summary>
    public partial class BarcodeRegistrationDialog : Window
    {
        private readonly IBarcodeService _barcodeService;
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        private List<Barang> _filteredItems = new List<Barang>();
        private Barang _selectedBarang;

        public string RegisteredBarcode => TxtBarcode.Text?.Trim();
        public string RegisteredBrgId => _selectedBarang?.BrgId;
        public string RegisteredBrgName => _selectedBarang?.BrgName;
        public string RegisteredSatuan =>
            (CboSatuan?.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? CboSatuan?.SelectedItem?.ToString()
            ?? CboSatuan?.Text;
        public decimal RegisteredPengali => decimal.TryParse(TxtPengali.Text, out var p) ? p : 1m;

        public BarcodeRegistrationDialog()
            : this(null, null, null, null, null, null)
        {
        }

        public BarcodeRegistrationDialog(
            string initialBarcode = null,
            IBarcodeService barcodeService = null,
            IBarcodeRepo barcodeRepo = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
        {
            InitializeComponent();

            _barcodeService = barcodeService;
            _barcodeRepo = barcodeRepo;
            _referenceRepo = referenceRepo;
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;

            if (!string.IsNullOrWhiteSpace(initialBarcode))
            {
                TxtBarcode.Text = initialBarcode;
            }

            Loaded += async (s, e) => await SearchItemsAsync(string.Empty);
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            await SearchItemsAsync(TxtSearchItem.Text);
        }

        private async Task SearchItemsAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            try
            {
                var items = await _referenceRepo.SearchBarangByNameOrCodeAsync(keyword ?? string.Empty);
                _filteredItems = (items ?? Enumerable.Empty<Barang>()).Take(50).ToList();

                LstItems.ItemsSource = null;
                LstItems.ItemsSource = _filteredItems;

                // Clear error on successful search
                TxtSearchStatus.Text = "";
                TxtSearchStatus.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Show error to user instead of silently suppressing
                TxtSearchStatus.Text = $"Gagal mencari barang: {ex.Message}";
                TxtSearchStatus.Visibility = Visibility.Visible;

                // Clear the list on error
                _filteredItems.Clear();
                LstItems.ItemsSource = null;
                LstItems.ItemsSource = _filteredItems;
            }
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (LstItems.SelectedItem is Barang selected)
            {
                _selectedBarang = selected;
                TxtSelectedItem.Text = $"Barang Terpilih: [{selected.BrgId}] {selected.BrgName}";

                var units = selected.GetAvailableUnits().ToList();
                if (CboSatuan.ItemsSource != null)
                {
                    CboSatuan.ItemsSource = null;
                }
                else
                {
                    CboSatuan.Items.Clear();
                }

                CboSatuan.ItemsSource = units;
                CboSatuan.IsEnabled = units.Count > 0;

                if (!string.IsNullOrWhiteSpace(selected.SatBesar) && units.Contains(selected.SatBesar))
                {
                    CboSatuan.SelectedItem = selected.SatBesar;
                }
                else if (!string.IsNullOrWhiteSpace(selected.SatKecil) && units.Contains(selected.SatKecil))
                {
                    CboSatuan.SelectedItem = selected.SatKecil;
                }
                else
                {
                    CboSatuan.SelectedItem = units.FirstOrDefault();
                }

                UpdatePengali();
            }
            else
            {
                _selectedBarang = null;
                TxtSelectedItem.Text = "Barang Terpilih: (Belum dipilih)";
                CboSatuan.ItemsSource = null;
                CboSatuan.IsEnabled = false;
                UpdatePengali();
            }
        }

        private void OnSatuanSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePengali();
        }

        private void UpdatePengali()
        {
            if (TxtPengali == null) return;
            var satuan = RegisteredSatuan;
            var multiplier = _selectedBarang?.GetConversionForUnit(satuan) ?? 1;
            TxtPengali.Text = multiplier.ToString();
        }

        public void ResetForm()
        {
            _selectedBarang = null;
            TxtBarcode.Text = string.Empty;
            TxtSearchItem.Text = string.Empty;
            TxtSelectedItem.Text = "Barang Terpilih: (Belum dipilih)";
            if (CboSatuan.ItemsSource != null)
                CboSatuan.ItemsSource = null;
            else
                CboSatuan.Items.Clear();
            CboSatuan.IsEnabled = false;
            TxtPengali.Text = "1";
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            string rawBarcode = TxtBarcode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(rawBarcode))
            {
                MessageBox.Show("Barcode tidak boleh kosong.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedBarang == null)
            {
                MessageBox.Show("Silakan pilih barang master yang akan ditautkan.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string satuan = RegisteredSatuan;
            if (string.IsNullOrWhiteSpace(satuan))
            {
                MessageBox.Show("Silakan pilih satuan kemasan yang valid.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPengali.Text, out var pengali) || pengali <= 0)
            {
                pengali = 1m;
            }

            string warehouseCode = _registryHelper?.WarehouseCode ?? "GDG-PST";
            string operatorName = _registryHelper?.LastOperatorName ?? "Operator-1";

            BtnSave.IsEnabled = false;

            try
            {
                if (_barcodeService != null)
                {
                    await _barcodeService.QuickRegisterUnmappedBarcodeAsync(
                        rawBarcode,
                        _selectedBarang.BrgId,
                        satuan,
                        warehouseCode,
                        operatorName,
                        pengali);
                }
                else if (_barcodeRepo != null)
                {
                    var barcodeObj = new Barcode(rawBarcode, _selectedBarang.BrgId, _selectedBarang.BrgName, satuan, pengali, true);
                    await _barcodeRepo.InsertBarcodeAsync(barcodeObj);

                    var reg = BarcodeRegistration.Create(rawBarcode, _selectedBarang.BrgId, satuan, warehouseCode, operatorName);
                    await _barcodeRepo.QueueRegistrationAsync(reg);
                }

                _audioHelper?.PlaySuccess();
                try
                {
                    DialogResult = true;
                }
                catch (InvalidOperationException)
                {
                }
                Close();
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show($"Gagal mendaftarkan barcode: {ex.Message}", "Error Registrasi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSave.IsEnabled = true;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch (InvalidOperationException)
            {
            }
            Close();
        }
    }
}
