using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.Controls;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for QuickBarcodeRegistrationDialog.xaml.
    /// SCR-04: On-the-spot modal dialog for quick barcode registration of unknown barcodes.
    /// Triggered automatically when an unmapped barcode is scanned.
    /// Prominently displays the unmapped barcode, provides product picker from BTRG_Barang,
    /// configures unit and conversion multiplier, persists local mapping and queues upload,
    /// and immediately transfers the resolved item into ReturnOrderEntryView intake.
    /// Traceable to: ARCHITECTURE §5.2 SCR-04, TD-006, TD-009, TD-013, P3-S07.
    /// </summary>
    public partial class QuickBarcodeRegistrationDialog : Window
    {
        private readonly QuickBarcodeRegistrationViewModel _viewModel;

        public QuickBarcodeRegistrationViewModel ViewModel => _viewModel;

        public BarcodeLookupResult RegisteredResult => _viewModel?.RegisteredResult;
        public string BarcodeValue => _viewModel?.BarcodeValue;
        public Barang SelectedBarang => _viewModel?.SelectedBarang;
        public string SelectedSatuan => _viewModel?.SelectedSatuan;
        public decimal SelectedPengali => _viewModel?.Pengali ?? 1m;
        public TextBox BarcodeTextBox => TxtBarcode;
        public TextBox SearchItemTextBox => TxtSearchItem;
        public ListBox ItemsListBox => LstItems;
        public ComboBox SatuanComboBox => CboSatuan;
        public NumericTextBox PengaliNumeric => TxtPengali;
        public Button RegisterButton => BtnRegisterAndUse;
        public Button CancelButtonControl => BtnCancel;

        public QuickBarcodeRegistrationDialog()
            : this(string.Empty, null, null, null, null, null)
        {
        }

        public QuickBarcodeRegistrationDialog(
            string unmappedBarcode = null,
            IBarcodeService barcodeService = null,
            IBarcodeRepo barcodeRepo = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
            : this(new QuickBarcodeRegistrationViewModel(
                unmappedBarcode,
                barcodeService ?? App.ServiceProvider?.GetService(typeof(IBarcodeService)) as IBarcodeService,
                barcodeRepo ?? App.ServiceProvider?.GetService(typeof(IBarcodeRepo)) as IBarcodeRepo,
                referenceRepo ?? App.ServiceProvider?.GetService(typeof(IReferenceRepo)) as IReferenceRepo,
                registryHelper ?? App.ServiceProvider?.GetService(typeof(IRegistryHelper)) as IRegistryHelper,
                audioHelper ?? App.ServiceProvider?.GetService(typeof(IAudioHelper)) as IAudioHelper))
        {
        }

        public QuickBarcodeRegistrationDialog(QuickBarcodeRegistrationViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            TxtBarcode.Text = _viewModel.BarcodeValue ?? string.Empty;
            CboSatuan.ItemsSource = _viewModel.PackagingUnits;
            CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
            TxtPengali.Value = (int)_viewModel.Pengali;

            _viewModel.RequestCloseOk += () =>
            {
                try
                {
                    DialogResult = true;
                }
                catch (InvalidOperationException)
                {
                }
                Close();
            };

            _viewModel.RequestCloseCancel += () =>
            {
                try
                {
                    DialogResult = false;
                }
                catch (InvalidOperationException)
                {
                }
                Close();
            };

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(QuickBarcodeRegistrationViewModel.SelectedBarang))
                {
                    UpdateSelectedItemText();
                    CboSatuan.ItemsSource = _viewModel.PackagingUnits;
                    CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
                }
                else if (e.PropertyName == nameof(QuickBarcodeRegistrationViewModel.SelectedSatuan))
                {
                    CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
                }
                else if (e.PropertyName == nameof(QuickBarcodeRegistrationViewModel.BarcodeValue))
                {
                    TxtBarcode.Text = _viewModel.BarcodeValue ?? string.Empty;
                }
                else if (e.PropertyName == nameof(QuickBarcodeRegistrationViewModel.Pengali))
                {
                    TxtPengali.Value = (int)_viewModel.Pengali;
                }
            };

            CboSatuan.SelectionChanged += (s, e) =>
            {
                if (CboSatuan.SelectedItem != null && _viewModel != null)
                {
                    _viewModel.SelectedSatuan = CboSatuan.SelectedItem.ToString();
                }
            };

            Loaded += async (s, e) =>
            {
                _viewModel.PlayInitialWarning();
                UpdateSelectedItemText();

                if (_viewModel.FilteredItems.Count == 0)
                {
                    await _viewModel.SearchItemsAsync(string.Empty);
                }

                CboSatuan.ItemsSource = _viewModel.PackagingUnits;
                CboSatuan.SelectedItem = _viewModel.SelectedSatuan;

                TxtSearchItem.Focus();
                TxtSearchItem.SelectAll();
            };

            PreviewKeyDown += OnWindowPreviewKeyDown;
        }

        private void UpdateSelectedItemText()
        {
            if (_viewModel.SelectedBarang != null)
            {
                TxtSelectedItem.Text = $"Barang Terpilih: [{_viewModel.SelectedBarang.BrgId}] {_viewModel.SelectedBarang.BrgName}";
            }
            else
            {
                TxtSelectedItem.Text = "Barang Terpilih: (Belum dipilih)";
            }
        }

        public void SetItems(IEnumerable<Barang> items)
        {
            _viewModel?.SetItems(items);
            UpdateSelectedItemText();
            if (_viewModel != null)
            {
                CboSatuan.ItemsSource = _viewModel.PackagingUnits;
                CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
            }
        }

        public async Task SearchItemsAsync(string keyword)
        {
            if (_viewModel != null)
            {
                await _viewModel.SearchItemsAsync(keyword);
                UpdateSelectedItemText();
            }
        }

        public async Task<bool> OnRegisterClickAsync()
        {
            if (_viewModel == null) return false;

            BtnRegisterAndUse.IsEnabled = false;
            try
            {
                bool success = await _viewModel.OnRegisterClickAsync();
                if (!success && !string.IsNullOrWhiteSpace(_viewModel.StatusMessage))
                {
                    MessageBox.Show(_viewModel.StatusMessage, "Peringatan Registrasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return success;
            }
            finally
            {
                BtnRegisterAndUse.IsEnabled = true;
            }
        }

        private async void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            await OnRegisterClickAsync();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            _viewModel?.Cancel();
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedItemText();
        }

        private void OnSearchItemPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (LstItems.Items.Count > 0)
                {
                    int next = Math.Min(LstItems.SelectedIndex + 1, LstItems.Items.Count - 1);
                    LstItems.SelectedIndex = next;
                    LstItems.ScrollIntoView(LstItems.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (LstItems.Items.Count > 0)
                {
                    int prev = Math.Max(LstItems.SelectedIndex - 1, 0);
                    LstItems.SelectedIndex = prev;
                    LstItems.ScrollIntoView(LstItems.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                CboSatuan.Focus();
            }
        }

        private void OnListPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                CboSatuan.Focus();
            }
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                _viewModel?.Cancel();
            }
        }
    }
}
