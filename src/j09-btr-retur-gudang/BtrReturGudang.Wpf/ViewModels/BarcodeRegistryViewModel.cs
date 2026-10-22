using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Wpf.Mvvm;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// Display model for Barcode Registry rows in the WPF catalog.
    /// Combines active catalog barcodes and pending registration queue records.
    /// </summary>
    public class BarcodeDisplayItem
    {
        public string Barcode { get; set; }
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public decimal Pengali { get; set; }
        public string Status { get; set; } // "Aktif Pusat" or "Pending Upload"
        public bool IsPending { get; set; }
    }

    /// <summary>
    /// ViewModel for the Barcode Registry &amp; Queue view (SCR-06).
    /// Provides search filtering across local catalog and registration queues,
    /// colored status badges, pagination, barcode deletion, and modal registration dialog.
    /// Traceable to: ARCHITECTURE §5.2 SCR-06, TD-006, TD-011, P4-S10.
    /// </summary>
    public class BarcodeRegistryViewModel : ViewModelBase
    {
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IBarcodeService _barcodeService;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        private string _searchKeyword = string.Empty;
        private string _selectedStatusFilter = "Semua Status";
        private int _currentPage = 1;
        private int _pageSize = 25;
        private int _totalPages = 1;
        private int _totalItemsCount = 0;
        private string _pageSummary = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isLoading;
        private BarcodeDisplayItem _selectedBarcode;

        private List<BarcodeDisplayItem> _allItems = new List<BarcodeDisplayItem>();
        private List<BarcodeDisplayItem> _filteredItems = new List<BarcodeDisplayItem>();

        public ObservableCollection<BarcodeDisplayItem> PagedItems { get; } = new ObservableCollection<BarcodeDisplayItem>();

        public List<string> StatusFilterOptions { get; } = new List<string>
        {
            "Semua Status",
            "Aktif Pusat",
            "Pending Upload"
        };

        public List<int> PageSizeOptions { get; } = new List<int> { 10, 25, 50, 100 };

        /// <summary>
        /// Delegate for confirming deletion in UI or unit tests.
        /// </summary>
        public Func<string, bool> ConfirmDeleteFunc { get; set; }

        /// <summary>
        /// Delegate for showing the modal barcode registration dialog in UI or unit tests.
        /// </summary>
        public Func<Task<bool>> ShowAddBarcodeDialogFunc { get; set; }

        public BarcodeRegistryViewModel()
            : this(null, null, null, null, null)
        {
        }

        public BarcodeRegistryViewModel(
            IBarcodeRepo barcodeRepo = null,
            IBarcodeService barcodeService = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
        {
            _barcodeRepo = barcodeRepo;
            _barcodeService = barcodeService;
            _referenceRepo = referenceRepo;
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;

            // Default delete confirmation prompt
            ConfirmDeleteFunc = barcode =>
            {
                var result = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus barcode \"{barcode}\" dari database lokal?",
                    "Konfirmasi Hapus Barcode",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            };

            RefreshCommand = new RelayCommand(async () => await LoadBarcodesAsync(), () => !IsLoading);
            SearchCommand = new RelayCommand(ApplyFilter);
            ClearSearchCommand = new RelayCommand(() =>
            {
                SearchKeyword = string.Empty;
                ApplyFilter();
            });
            NextPageCommand = new RelayCommand(NextPage, () => CanNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => CanPreviousPage);
            AddBarcodeCommand = new RelayCommand(async () => await OnAddBarcodeAsync());
            DeleteBarcodeCommand = new RelayCommand<BarcodeDisplayItem>(async item => await DeleteBarcodeAsync(item));
        }

        #region Properties

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    ApplyFilter();
                }
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    UpdatePage();
                }
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value) && value > 0)
                {
                    _currentPage = 1;
                    UpdatePage();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            private set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    OnPropertyChanged(nameof(CanNextPage));
                    OnPropertyChanged(nameof(CanPreviousPage));
                }
            }
        }

        public int TotalItemsCount
        {
            get => _totalItemsCount;
            private set => SetProperty(ref _totalItemsCount, value);
        }

        public string PageSummary
        {
            get => _pageSummary;
            private set => SetProperty(ref _pageSummary, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public BarcodeDisplayItem SelectedBarcode
        {
            get => _selectedBarcode;
            set => SetProperty(ref _selectedBarcode, value);
        }

        public bool CanNextPage => CurrentPage < TotalPages;
        public bool CanPreviousPage => CurrentPage > 1;

        public IReadOnlyList<BarcodeDisplayItem> AllItems => _allItems;
        public IReadOnlyList<BarcodeDisplayItem> FilteredItems => _filteredItems;

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand AddBarcodeCommand { get; }
        public ICommand DeleteBarcodeCommand { get; }

        #endregion

        #region Methods

        public async Task LoadBarcodesAsync()
        {
            IsLoading = true;
            StatusMessage = "Memuat katalog barcode...";

            try
            {
                var list = new List<BarcodeDisplayItem>();

                if (_barcodeRepo != null)
                {
                    // 1. Fetch active cached barcodes
                    var barcodes = await _barcodeRepo.GetAllBarcodesAsync();
                    if (barcodes != null)
                    {
                        foreach (var b in barcodes)
                        {
                            list.Add(new BarcodeDisplayItem
                            {
                                Barcode = b.BarcodeValue,
                                BrgId = b.BrgId,
                                BrgName = b.BrgName,
                                Satuan = b.Satuan,
                                Pengali = b.Pengali,
                                Status = "Aktif Pusat",
                                IsPending = false
                            });
                        }
                    }

                    // 2. Fetch pending registration requests
                    var pending = await _barcodeRepo.GetPendingRegistrationsAsync();
                    if (pending != null)
                    {
                        foreach (var p in pending)
                        {
                            var existing = list.FirstOrDefault(x => string.Equals(x.Barcode, p.Barcode, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.Status = "Pending Upload";
                                existing.IsPending = true;
                            }
                            else
                            {
                                list.Insert(0, new BarcodeDisplayItem
                                {
                                    Barcode = p.Barcode,
                                    BrgId = p.BrgId,
                                    BrgName = $"[Pending] {p.BrgId}",
                                    Satuan = p.Satuan,
                                    Pengali = 1m,
                                    Status = "Pending Upload",
                                    IsPending = true
                                });
                            }
                        }
                    }
                }

                _allItems = list;
                _currentPage = 1;
                ApplyFilter();
                StatusMessage = $"Katalog berhasil dimuat ({_allItems.Count} barcode terdaftar).";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gagal memuat katalog barcode: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SetBarcodes(IEnumerable<BarcodeDisplayItem> items)
        {
            _allItems = items != null ? items.ToList() : new List<BarcodeDisplayItem>();
            _currentPage = 1;
            ApplyFilter();
        }

        public void ApplyFilter()
        {
            string keyword = _searchKeyword?.Trim() ?? string.Empty;
            string status = _selectedStatusFilter ?? "Semua Status";

            var query = _allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    (x.Barcode != null && x.Barcode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.BrgId != null && x.BrgId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.BrgName != null && x.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (status.Equals("Aktif Pusat", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => !x.IsPending);
            }
            else if (status.Equals("Pending Upload", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.IsPending);
            }

            _filteredItems = query.ToList();
            TotalItemsCount = _filteredItems.Count;
            _currentPage = 1;

            UpdatePage();
        }

        private void UpdatePage()
        {
            if (_pageSize <= 0) _pageSize = 25;

            TotalPages = Math.Max(1, (int)Math.Ceiling((double)_filteredItems.Count / _pageSize));
            if (_currentPage > TotalPages) _currentPage = TotalPages;
            if (_currentPage < 1) _currentPage = 1;

            int skip = (_currentPage - 1) * _pageSize;
            var pageData = _filteredItems.Skip(skip).Take(_pageSize).ToList();

            PagedItems.Clear();
            foreach (var item in pageData)
            {
                PagedItems.Add(item);
            }

            PageSummary = $"Halaman {_currentPage} dari {TotalPages} (Total {TotalItemsCount} barcode)";
            OnPropertyChanged(nameof(CanNextPage));
            OnPropertyChanged(nameof(CanPreviousPage));
        }

        public void NextPage()
        {
            if (CanNextPage)
            {
                CurrentPage++;
            }
        }

        public void PreviousPage()
        {
            if (CanPreviousPage)
            {
                CurrentPage--;
            }
        }

        public async Task DeleteBarcodeAsync(BarcodeDisplayItem item)
        {
            var target = item ?? SelectedBarcode;
            if (target == null || string.IsNullOrWhiteSpace(target.Barcode))
                return;

            if (ConfirmDeleteFunc != null && !ConfirmDeleteFunc(target.Barcode))
                return;

            try
            {
                if (_barcodeRepo != null)
                {
                    await _barcodeRepo.DeleteBarcodeAsync(target.Barcode);
                }
                else if (_barcodeService != null)
                {
                    await _barcodeService.DeleteBarcodeAsync(target.Barcode);
                }

                _allItems.RemoveAll(x => string.Equals(x.Barcode, target.Barcode, StringComparison.OrdinalIgnoreCase));
                _filteredItems.RemoveAll(x => string.Equals(x.Barcode, target.Barcode, StringComparison.OrdinalIgnoreCase));

                _audioHelper?.PlaySuccess();
                StatusMessage = $"Barcode \"{target.Barcode}\" berhasil dihapus.";
                UpdatePage();
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                StatusMessage = $"Gagal menghapus barcode: {ex.Message}";
            }
        }

        private async Task OnAddBarcodeAsync()
        {
            if (ShowAddBarcodeDialogFunc != null)
            {
                bool registered = await ShowAddBarcodeDialogFunc();
                if (registered)
                {
                    await LoadBarcodesAsync();
                }
            }
        }

        #endregion
    }
}
