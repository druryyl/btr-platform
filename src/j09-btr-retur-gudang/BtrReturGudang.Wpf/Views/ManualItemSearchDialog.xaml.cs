using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.ViewModels;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ManualItemSearchDialog.xaml.
    /// SCR-03: Keyboard-driven manual item search modal dialog (F2 fallback).
    /// Filters product master records in BTRG_Barang by item code or name,
    /// allows packaging unit selection, and passes selected product back to intake strip.
    /// Traceable to: ARCHITECTURE §5.2 SCR-03, TD-011, TD-013, P3-S07.
    /// </summary>
    public partial class ManualItemSearchDialog : Window
    {
        private readonly ManualItemSearchViewModel _viewModel;

        public ManualItemSearchViewModel ViewModel => _viewModel;

        public Barang SelectedBarang => _viewModel?.SelectedBarang;
        public string SelectedSatuan => _viewModel?.SelectedSatuan;
        public TextBox SearchBox => TxtSearch;
        public DataGrid GridView => DgvItems;
        public ComboBox SatuanComboBox => CboSatuan;
        public Button SelectButton => BtnSelect;
        public Button CancelButtonControl => BtnCancel;
        public IReadOnlyList<Barang> FilteredItems => _viewModel?.FilteredItems?.ToList() ?? new List<Barang>();

        public ManualItemSearchDialog()
            : this(App.ServiceProvider?.GetService(typeof(IReferenceRepo)) as IReferenceRepo,
                   App.ServiceProvider?.GetService(typeof(IAudioHelper)) as IAudioHelper)
        {
        }

        public ManualItemSearchDialog(IReferenceRepo referenceRepo, IAudioHelper audioHelper = null)
            : this(new ManualItemSearchViewModel(referenceRepo, audioHelper))
        {
        }

        public ManualItemSearchDialog(ManualItemSearchViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

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
                if (e.PropertyName == nameof(ManualItemSearchViewModel.SelectedBarang))
                {
                    UpdateSelectedItemText();
                    CboSatuan.ItemsSource = _viewModel.PackagingUnits;
                    CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
                }
                else if (e.PropertyName == nameof(ManualItemSearchViewModel.SelectedSatuan))
                {
                    CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
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
                UpdateSelectedItemText();
                if (_viewModel.FilteredItems.Count == 0)
                {
                    await _viewModel.PerformSearchAsync(string.Empty);
                }
                CboSatuan.ItemsSource = _viewModel.PackagingUnits;
                CboSatuan.SelectedItem = _viewModel.SelectedSatuan;
                TxtSearch.Focus();
                TxtSearch.SelectAll();
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

        public async Task PerformSearchAsync(string keyword)
        {
            if (_viewModel != null)
            {
                await _viewModel.PerformSearchAsync(keyword);
                UpdateSelectedItemText();
            }
        }

        public void ConfirmSelection()
        {
            if (_viewModel != null && _viewModel.ConfirmSelection())
            {
                try
                {
                    DialogResult = true;
                }
                catch (InvalidOperationException)
                {
                }
                Close();
            }
        }

        private void OnSelectClick(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            _viewModel?.Cancel();
        }

        private void OnSearchPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (DgvItems.Items.Count > 0)
                {
                    int next = Math.Min(DgvItems.SelectedIndex + 1, DgvItems.Items.Count - 1);
                    DgvItems.SelectedIndex = next;
                    DgvItems.ScrollIntoView(DgvItems.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (DgvItems.Items.Count > 0)
                {
                    int prev = Math.Max(DgvItems.SelectedIndex - 1, 0);
                    DgvItems.SelectedIndex = prev;
                    DgvItems.ScrollIntoView(DgvItems.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ConfirmSelection();
            }
        }

        private void OnGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ConfirmSelection();
            }
        }

        private void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
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
