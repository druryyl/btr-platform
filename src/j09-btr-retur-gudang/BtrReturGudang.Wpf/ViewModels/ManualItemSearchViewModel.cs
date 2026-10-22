using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.Mvvm;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the manual item search dialog (ManualItemSearchDialog SCR-03).
    /// Provides real-time filtering over master items by code or name, packaging unit selection,
    /// and keyboard accelerator orchestration for warehouse intake fallback.
    /// Traceable to: ARCHITECTURE §5.2 SCR-03, TD-011, TD-013, P3-S07.
    /// </summary>
    public class ManualItemSearchViewModel : ViewModelBase
    {
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;

        private string _searchKeyword = string.Empty;
        private Barang _selectedBarang;
        private string _selectedSatuan;
        private string _statusText = "0 barang ditemukan";
        private bool _isLoading;

        public ObservableCollection<Barang> FilteredItems { get; } = new ObservableCollection<Barang>();
        public ObservableCollection<string> PackagingUnits { get; } = new ObservableCollection<string>();

        public event Action RequestCloseOk;
        public event Action RequestCloseCancel;

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    _ = PerformSearchAsync(value);
                }
            }
        }

        public Barang SelectedBarang
        {
            get => _selectedBarang;
            set
            {
                if (SetProperty(ref _selectedBarang, value))
                {
                    UpdatePackagingUnits(value);
                }
            }
        }

        public string SelectedSatuan
        {
            get => _selectedSatuan;
            set => SetProperty(ref _selectedSatuan, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand SelectCommand { get; }
        public ICommand CancelCommand { get; }

        public ManualItemSearchViewModel(IReferenceRepo referenceRepo = null, IAudioHelper audioHelper = null)
        {
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;

            SearchCommand = new AsyncRelayCommand(async () => await PerformSearchAsync(_searchKeyword));
            SelectCommand = new RelayCommand(() => ConfirmSelection(), () => _selectedBarang != null);
            CancelCommand = new RelayCommand(Cancel);
        }

        public async Task PerformSearchAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            IsLoading = true;
            try
            {
                var results = await _referenceRepo.SearchBarangByNameOrCodeAsync(keyword ?? string.Empty);
                var itemsList = (results ?? Enumerable.Empty<Barang>()).Take(100).ToList();

                FilteredItems.Clear();
                foreach (var b in itemsList)
                {
                    FilteredItems.Add(b);
                }

                StatusText = $"{FilteredItems.Count} barang ditemukan";

                if (FilteredItems.Count > 0)
                {
                    if (_selectedBarang == null || !FilteredItems.Any(i => i.BrgId == _selectedBarang.BrgId))
                    {
                        SelectedBarang = FilteredItems[0];
                    }
                }
                else
                {
                    SelectedBarang = null;
                }
            }
            catch
            {
                StatusText = "Gagal memuat barang.";
            }
            finally
            {
                IsLoading = false;
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

            StatusText = $"{FilteredItems.Count} barang ditemukan";

            if (FilteredItems.Count > 0)
            {
                SelectedBarang = FilteredItems[0];
            }
            else
            {
                SelectedBarang = null;
            }
        }

        private void UpdatePackagingUnits(Barang barang)
        {
            PackagingUnits.Clear();

            if (barang == null)
            {
                SelectedSatuan = null;
                return;
            }

            var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(barang.Satuan1)) units.Add(barang.Satuan1);
            if (!string.IsNullOrWhiteSpace(barang.Satuan2)) units.Add(barang.Satuan2);
            if (!string.IsNullOrWhiteSpace(barang.Satuan3)) units.Add(barang.Satuan3);

            if (units.Count == 0)
            {
                units.Add("Dus");
                units.Add("Pak");
                units.Add("Pcs");
            }

            foreach (var u in units)
            {
                PackagingUnits.Add(u);
            }

            SelectedSatuan = PackagingUnits.FirstOrDefault();
        }

        public bool ConfirmSelection()
        {
            if (_selectedBarang == null) return false;

            _audioHelper?.PlaySuccess();
            RequestCloseOk?.Invoke();
            return true;
        }

        public void Cancel()
        {
            RequestCloseCancel?.Invoke();
        }
    }
}
