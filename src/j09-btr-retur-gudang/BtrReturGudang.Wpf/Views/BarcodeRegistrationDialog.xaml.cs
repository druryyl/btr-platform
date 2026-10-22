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
        public string RegisteredSatuan => (CboSatuan.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? CboSatuan.Text;
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
            }
            catch
            {
                // Suppress search failure in UI typing
            }
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (LstItems.SelectedItem is Barang selected)
            {
                _selectedBarang = selected;
                TxtSelectedItem.Text = $"Barang Terpilih: [{selected.BrgId}] {selected.BrgName}";

                // Auto-set satuan if master specifies it
                if (!string.IsNullOrWhiteSpace(selected.Satuan1))
                {
                    foreach (ComboBoxItem item in CboSatuan.Items)
                    {
                        if (string.Equals(item.Content?.ToString(), selected.Satuan1, StringComparison.OrdinalIgnoreCase))
                        {
                            CboSatuan.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
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

            if (!decimal.TryParse(TxtPengali.Text, out var pengali) || pengali <= 0)
            {
                pengali = 1m;
            }

            string warehouseCode = _registryHelper?.WarehouseCode ?? "GDG-PST";
            string operatorName = _registryHelper?.LastOperatorName ?? "Operator-1";
            string satuan = RegisteredSatuan;

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
