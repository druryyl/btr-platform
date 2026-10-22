using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.Mvvm;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the keyboard-driven customer lookup dialog (SCR-08).
    /// Provides fast real-time search over large master customer datasets (> 2,000 records),
    /// arrow key grid navigation, and walk-in customer fallback (CUST-MANUAL).
    /// Traceable to: ARCHITECTURE §5.2 SCR-08, TD-011, TD-013, P3-S08.
    /// </summary>
    public class ManualCustomerSearchViewModel : ViewModelBase
    {
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;

        private string _searchKeyword = string.Empty;
        private Customer _selectedCustomer;
        private int _selectedIndex = -1;
        private string _statusText = "Siap mencari customer";
        private string _selectedCustomerDisplayText = "Customer Terpilih: (Belum dipilih)";
        private bool _isSearching;
        private CancellationTokenSource _searchCts;

        public bool SuppressAudio { get; set; } = false;

        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        public event Action<bool?> RequestClose;
        public event Action RequestFocusGrid;
        public event Action RequestFocusSearchBox;

        public ManualCustomerSearchViewModel(
            IReferenceRepo referenceRepo,
            IAudioHelper audioHelper = null,
            string initialKeyword = null)
        {
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;

            SearchCommand = new AsyncRelayCommand(async () => await SearchCustomersAsync(SearchKeyword, immediate: true));
            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection);
            SelectWalkInCustomerCommand = new RelayCommand(SelectWalkInCustomer);
            NavigateDownCommand = new RelayCommand(NavigateDown);
            NavigateUpCommand = new RelayCommand(NavigateUp);
            CancelCommand = new RelayCommand(Cancel);

            if (!string.IsNullOrWhiteSpace(initialKeyword))
            {
                _searchKeyword = initialKeyword;
                _ = SearchCustomersAsync(initialKeyword, immediate: true);
            }
            else
            {
                _ = SearchCustomersAsync(string.Empty, immediate: true);
            }
        }

        public ManualCustomerSearchViewModel() : this(
            App.ServiceProvider?.GetService<IReferenceRepo>(),
            App.ServiceProvider?.GetService<IAudioHelper>(),
            null)
        {
        }

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand ConfirmSelectionCommand { get; }
        public ICommand SelectWalkInCustomerCommand { get; }
        public ICommand NavigateDownCommand { get; }
        public ICommand NavigateUpCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Properties

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    _ = SearchCustomersAsync(value, immediate: false);
                }
            }
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    UpdateSelectedCustomerDisplay();
                }
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public string SelectedCustomerDisplayText
        {
            get => _selectedCustomerDisplayText;
            private set => SetProperty(ref _selectedCustomerDisplayText, value);
        }

        public bool IsSearching
        {
            get => _isSearching;
            private set => SetProperty(ref _isSearching, value);
        }

        #endregion

        #region Search and Selection Logic

        public async Task SearchCustomersAsync(string keyword, bool immediate = false)
        {
            _searchCts?.Cancel();
            var cts = new CancellationTokenSource();
            _searchCts = cts;

            IsSearching = true;
            StatusText = "Mencari customer...";

            try
            {
                if (!immediate)
                {
                    await Task.Delay(120, cts.Token);
                }

                if (_referenceRepo != null)
                {
                    var results = await _referenceRepo.SearchCustomerAsync(keyword ?? string.Empty);
                    if (cts.IsCancellationRequested) return;

                    var list = results?.ToList() ?? new List<Customer>();

                    Customers.Clear();
                    foreach (var c in list)
                    {
                        Customers.Add(c);
                    }

                    StatusText = $"{Customers.Count:N0} customer ditemukan";
                    if (Customers.Count > 0)
                    {
                        SelectedIndex = 0;
                        SelectedCustomer = Customers[0];
                    }
                    else
                    {
                        SelectedIndex = -1;
                        SelectedCustomer = null;
                        StatusText = "0 customer ditemukan. Tekan [F6] atau Enter untuk Walk-in Customer.";
                    }
                }
                else
                {
                    Customers.Clear();
                    SelectedIndex = -1;
                    SelectedCustomer = null;
                    StatusText = "0 customer ditemukan (Repository tidak terhubung)";
                }
            }
            catch (OperationCanceledException)
            {
                // Keystroke debounce superseded
            }
            catch (Exception ex)
            {
                if (!cts.IsCancellationRequested)
                {
                    StatusText = $"Gagal memuat customer: {ex.Message}";
                }
            }
            finally
            {
                if (!cts.IsCancellationRequested)
                {
                    IsSearching = false;
                }
            }
        }

        public void NavigateDown()
        {
            if (Customers.Count == 0) return;
            int next = SelectedIndex + 1;
            if (next < Customers.Count)
            {
                SelectedIndex = next;
                SelectedCustomer = Customers[next];
            }
            RequestFocusGrid?.Invoke();
        }

        public void NavigateUp()
        {
            if (Customers.Count == 0) return;
            int prev = SelectedIndex - 1;
            if (prev >= 0)
            {
                SelectedIndex = prev;
                SelectedCustomer = Customers[prev];
            }
            RequestFocusGrid?.Invoke();
        }

        public void FocusSearchBox()
        {
            RequestFocusSearchBox?.Invoke();
        }

        public void ConfirmSelection()
        {
            if (SelectedCustomer == null && Customers.Count > 0 && SelectedIndex >= 0 && SelectedIndex < Customers.Count)
            {
                SelectedCustomer = Customers[SelectedIndex];
            }

            if (SelectedCustomer != null)
            {
                if (!SuppressAudio) _audioHelper?.PlaySuccess();
                RequestClose?.Invoke(true);
                return;
            }

            // Fallback: If no record matched from catalog, treat keyword as walk-in customer (CUST-MANUAL)
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                SelectWalkInCustomer();
                return;
            }

            if (!SuppressAudio) _audioHelper?.PlayWarning();
            StatusText = "Peringatan: Silakan pilih customer atau gunakan Walk-in.";
        }

        public void SelectWalkInCustomer()
        {
            var walkInName = !string.IsNullOrWhiteSpace(SearchKeyword)
                ? SearchKeyword.Trim()
                : "Pelanggan Langsung (Walk-in)";

            var walkIn = new Customer(
                customerId: "CUST-MANUAL",
                customerName: walkInName,
                alamat: "-",
                customerCode: "CUST-MANUAL",
                wilayah: "Walk-in");

            SelectedCustomer = walkIn;
            if (!SuppressAudio) _audioHelper?.PlaySuccess();
            RequestClose?.Invoke(true);
        }

        public void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        private void UpdateSelectedCustomerDisplay()
        {
            if (_selectedCustomer == null)
            {
                SelectedCustomerDisplayText = "Customer Terpilih: (Belum dipilih)";
                return;
            }

            var displayCode = !string.IsNullOrWhiteSpace(_selectedCustomer.CustomerCode)
                ? _selectedCustomer.CustomerCode
                : (!string.IsNullOrWhiteSpace(_selectedCustomer.CustomerId) ? _selectedCustomer.CustomerId : "-");
            var wilayahText = !string.IsNullOrWhiteSpace(_selectedCustomer.Wilayah) ? $" - {_selectedCustomer.Wilayah}" : string.Empty;
            SelectedCustomerDisplayText = $"Customer Terpilih: [{displayCode}] {_selectedCustomer.CustomerName}{wilayahText}";
        }

        #endregion
    }
}
