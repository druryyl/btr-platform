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
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Wpf.Mvvm;
using BtrReturGudang.Wpf.Services;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the Return Order History &amp; Inspector view (ReturnOrderListView SCR-05).
    /// Provides date filtering, status sync filtering, responsive master-detail inspector,
    /// and strict post-sync read-only document locking (blocking Edit/Delete on Synced documents).
    /// Traceable to: ARCHITECTURE §4 TD-010, TD-011, TD-012, TD-013, §5.2 SCR-05, P4-S09.
    /// </summary>
    public class ReturnOrderListViewModel : ViewModelBase
    {
        private readonly IReturnOrderService _orderService;
        private readonly IReturnOrderRepo _orderRepo;
        private readonly ISyncService _syncService;
        private readonly IAudioHelper _audioHelper;
        private readonly OperatorSessionValidator _sessionValidator;

        // Filter state
        private DateTime _startDate = DateTime.Today.AddDays(-14);
        private DateTime _endDate = DateTime.Today;
        private string _selectedStatusFilter = "Semua (All)";
        private string _summaryCountText = "0 dokumen ditemukan";

        // Selection & data collections
        private ReturnOrderDto _selectedOrder;
        private bool _isLoading = false;
        private bool _isUploading = false;
        private string _uploadButtonText = "☁️ Upload Sekarang";
        private string _statusMessage = string.Empty;

        /// <summary>
        /// Event fired when user requests to open a draft order for editing in intake view.
        /// </summary>
        public event EventHandler<string> EditDraftRequested;

        /// <summary>
        /// Hook for confirming deletion (defaults to WPF MessageBox.Show, customizable in tests).
        /// </summary>
        public Func<string, string, bool> ConfirmDeleteAction { get; set; }

        /// <summary>
        /// Hook for displaying notification messages (defaults to WPF MessageBox.Show, customizable in tests).
        /// </summary>
        public Action<string, string, bool> ShowNotificationAction { get; set; }

        public ObservableCollection<ReturnOrderDto> Orders { get; } = new ObservableCollection<ReturnOrderDto>();
        public ObservableCollection<ReturnOrderItemDto> Items { get; } = new ObservableCollection<ReturnOrderItemDto>();

        public IReadOnlyList<string> StatusFilterOptions { get; } = new[]
        {
            "Semua (All)",
            "Draft",
            "Synced"
        };

        public ReturnOrderListViewModel()
            : this(null, null, null, null, null)
        {
        }

        public ReturnOrderListViewModel(
            IReturnOrderService orderService = null,
            IReturnOrderRepo orderRepo = null,
            ISyncService syncService = null,
            IAudioHelper audioHelper = null,
            OperatorSessionValidator sessionValidator = null)
        {
            _orderService = orderService;
            _orderRepo = orderRepo;
            _syncService = syncService;
            _audioHelper = audioHelper;
            _sessionValidator = sessionValidator;

            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsLoading);
            ApplyFilterCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsLoading);
            EditDraftCommand = new RelayCommand(EditDraft, () => CanEditOrder);
            DeleteDraftCommand = new AsyncRelayCommand(DeleteDraftAsync, () => CanDeleteOrder && !IsLoading);
            UploadOrderCommand = new AsyncRelayCommand(UploadOrderAsync, () => CanUploadOrder && !IsUploading);
        }

        #region Filter Properties

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set => SetProperty(ref _selectedStatusFilter, value);
        }

        public string SummaryCountText
        {
            get => _summaryCountText;
            private set => SetProperty(ref _summaryCountText, value);
        }

        #endregion

        #region State & Selection Properties

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(CanDeleteOrder));
                }
            }
        }

        public bool IsUploading
        {
            get => _isUploading;
            private set
            {
                if (SetProperty(ref _isUploading, value))
                {
                    OnPropertyChanged(nameof(CanUploadOrder));
                }
            }
        }

        public string UploadButtonText
        {
            get => _uploadButtonText;
            private set => SetProperty(ref _uploadButtonText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (SetProperty(ref _statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public ReturnOrderDto SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    InspectOrder(_selectedOrder);
                }
            }
        }

        #endregion

        #region Inspector Metadata Properties (Traceable to SCR-05)

        public bool HasSelectedOrder => SelectedOrder != null;

        /// <summary>
        /// True when the inspected document has been synchronized upstream.
        /// When true, strict Post-Sync Immutability displays the locked banner and disables edits/deletes.
        /// </summary>
        public bool IsSelectedOrderSynced => SelectedOrder != null && SelectedOrder.IsSynced;

        public bool CanEditOrder => SelectedOrder != null && !SelectedOrder.IsSynced;
        public bool CanDeleteOrder => SelectedOrder != null && !SelectedOrder.IsSynced && !IsLoading;
        public bool CanUploadOrder => SelectedOrder != null && !SelectedOrder.IsSynced && !IsUploading;

        public string OrderIdText => SelectedOrder?.ReturnOrderId ?? "-";
        public string DateText => SelectedOrder != null ? SelectedOrder.ReturnOrderDate.ToString("yyyy-MM-dd") : "-";
        public string CustomerText => SelectedOrder != null ? $"{SelectedOrder.CustName} ({SelectedOrder.CustId ?? SelectedOrder.CustomerId})" : "-";
        public string OperatorText => SelectedOrder?.OperatorName ?? "-";
        public string WarehouseText => SelectedOrder?.WarehouseCode ?? "-";
        public string DriverText => !string.IsNullOrWhiteSpace(SelectedOrder?.DriverName) ? SelectedOrder.DriverName : "-";
        public string SalesText => !string.IsNullOrWhiteSpace(SelectedOrder?.SalesPersonName) ? SelectedOrder.SalesPersonName : "-";
        public string NoteText => !string.IsNullOrWhiteSpace(SelectedOrder?.Keterangan) ? SelectedOrder.Keterangan : "-";

        public string SyncStatusText
        {
            get
            {
                if (SelectedOrder == null) return "-";
                return SelectedOrder.IsSynced
                    ? $"SYNCED ({SelectedOrder.SyncedDate:yyyy-MM-dd HH:mm})"
                    : "DRAFT (Lokal)";
            }
        }

        public bool IsSyncStatusSynced => SelectedOrder != null && SelectedOrder.IsSynced;

        #endregion

        #region Commands

        public ICommand LoadOrdersCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand EditDraftCommand { get; }
        public ICommand DeleteDraftCommand { get; }
        public ICommand UploadOrderCommand { get; }

        #endregion

        #region Actions & Business Operations

        /// <summary>
        /// Queries return orders matching current date range and status filters.
        /// </summary>
        public async Task LoadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                DateTime start = StartDate.Date;
                DateTime end = EndDate.Date.AddDays(1).AddSeconds(-1);

                StatusSync? statusFilter = null;
                if (SelectedStatusFilter == "Draft") statusFilter = StatusSync.Draft;
                else if (SelectedStatusFilter == "Synced") statusFilter = StatusSync.Synced;

                IEnumerable<ReturnOrderDto> orders = null;
                if (_orderService != null)
                {
                    orders = await _orderService.GetOrdersAsync(start, end, statusFilter);
                }
                else if (_orderRepo != null)
                {
                    var domainOrders = await _orderRepo.GetFilteredListAsync(start, end, statusFilter);
                    orders = domainOrders?.Select(ReturnOrderDto.FromDomain);
                }

                var list = orders != null ? orders.ToList() : new List<ReturnOrderDto>();
                var previousSelectedId = SelectedOrder?.ReturnOrderId;

                Orders.Clear();
                foreach (var order in list)
                {
                    Orders.Add(order);
                }

                SummaryCountText = $"{Orders.Count} dokumen ditemukan";

                // Preserve selection or select first
                var matching = !string.IsNullOrEmpty(previousSelectedId)
                    ? Orders.FirstOrDefault(o => o.ReturnOrderId == previousSelectedId)
                    : null;

                SelectedOrder = matching ?? Orders.FirstOrDefault();
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gagal memuat daftar retur: {ex.Message}";
                ShowNotification("Error Database", StatusMessage, isWarning: true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Sets orders directly (useful for testing or in-memory view presentation).
        /// </summary>
        public void SetOrders(IEnumerable<ReturnOrderDto> orders)
        {
            Orders.Clear();
            if (orders != null)
            {
                foreach (var o in orders)
                {
                    Orders.Add(o);
                }
            }
            SummaryCountText = $"{Orders.Count} dokumen ditemukan";
            SelectedOrder = Orders.FirstOrDefault();
        }

        /// <summary>
        /// Inspects the given order and updates all metadata and detail items.
        /// </summary>
        public void InspectOrder(ReturnOrderDto order)
        {
            OnPropertyChanged(nameof(HasSelectedOrder));
            OnPropertyChanged(nameof(IsSelectedOrderSynced));
            OnPropertyChanged(nameof(CanEditOrder));
            OnPropertyChanged(nameof(CanDeleteOrder));
            OnPropertyChanged(nameof(CanUploadOrder));
            OnPropertyChanged(nameof(OrderIdText));
            OnPropertyChanged(nameof(DateText));
            OnPropertyChanged(nameof(CustomerText));
            OnPropertyChanged(nameof(OperatorText));
            OnPropertyChanged(nameof(WarehouseText));
            OnPropertyChanged(nameof(DriverText));
            OnPropertyChanged(nameof(SalesText));
            OnPropertyChanged(nameof(NoteText));
            OnPropertyChanged(nameof(SyncStatusText));
            OnPropertyChanged(nameof(IsSyncStatusSynced));

            Items.Clear();
            if (order?.Items != null)
            {
                foreach (var item in order.Items)
                {
                    Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Dispatches draft edit request to navigation coordinator.
        /// </summary>
        public void EditDraft()
        {
            if (SelectedOrder == null) return;

            if (SelectedOrder.IsSynced)
            {
                ShowNotification(
                    "Dokumen Terkunci",
                    "Dokumen telah terkunci karena sudah disinkronkan ke pusat (Status = Synced). Perubahan tidak diizinkan.",
                    isWarning: true);
                return;
            }

            EditDraftRequested?.Invoke(this, SelectedOrder.ReturnOrderId);
        }

        /// <summary>
        /// Deletes the currently selected draft document from local database.
        /// </summary>
        public async Task DeleteDraftAsync()
        {
            if (SelectedOrder == null) return;

            if (SelectedOrder.IsSynced)
            {
                ShowNotification(
                    "Dokumen Terkunci",
                    "Dokumen telah terkunci karena sudah disinkronkan ke pusat (Status = Synced). Penghapusan tidak diizinkan.",
                    isWarning: true);
                return;
            }

            string orderId = SelectedOrder.ReturnOrderId;
            string custName = SelectedOrder.CustName;

            bool confirmed;
            if (ConfirmDeleteAction != null)
            {
                confirmed = ConfirmDeleteAction(
                    $"Apakah Anda yakin ingin menghapus dokumen draft retur [{orderId}] untuk customer '{custName}'?",
                    "Konfirmasi Hapus Draft");
            }
            else
            {
                var result = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus dokumen draft retur [{orderId}] untuk customer '{custName}'?",
                    "Konfirmasi Hapus Draft",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                confirmed = (result == MessageBoxResult.Yes);
            }

            if (!confirmed) return;

            try
            {
                if (_orderService != null)
                {
                    await _orderService.DeleteDraftAsync(orderId);
                }
                else if (_orderRepo != null)
                {
                    await _orderRepo.DeleteDraftAsync(orderId);
                }

                _audioHelper?.PlaySuccess();
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                ShowNotification("Error Hapus Draft", $"Gagal menghapus draft: {ex.Message}", isWarning: true);
            }
        }

        /// <summary>
        /// Pushes the local draft return order upstream to Cloud API relay.
        /// Traceable to: ARCHITECTURE §4 TD-007, TD-008, TD-012.
        /// </summary>
        public async Task UploadOrderAsync()
        {
            if (SelectedOrder == null) return;

            if (SelectedOrder.IsSynced)
            {
                ShowNotification("Info Sinkronisasi", "Dokumen ini sudah tersinkronkan sebelumnya.", isWarning: false);
                return;
            }

            // Pre-empt Cloud upload when operator session is unverified
            if (_sessionValidator != null && _sessionValidator.IsUnresolved)
            {
                _audioHelper?.PlayWarning();
                ShowNotification(
                    "Operator Belum Terverifikasi",
                    $"Upload dokumen retur [{SelectedOrder.ReturnOrderId}] ke Cloud dibatalkan: {_sessionValidator.RefusalMessage}",
                    isWarning: true);
                return;
            }

            string orderId = SelectedOrder.ReturnOrderId;

            try
            {
                IsUploading = true;
                UploadButtonText = "Mengupload...";

                bool success = false;
                string failureReason = null;

                if (_orderService != null)
                {
                    var result = await _orderService.CommitAndUploadAsync(orderId);
                    success = result != null && result.Success;
                    failureReason = result?.ErrorMessage;
                }
                else if (_syncService != null)
                {
                    var syncResult = await _syncService.TriggerDocumentUploadAsync(orderId);
                    success = syncResult != null && syncResult.Success;
                    failureReason = syncResult?.ErrorMessage;
                }

                if (success)
                {
                    _audioHelper?.PlaySuccess();
                    ShowNotification(
                        "Upload Berhasil",
                        $"Dokumen retur [{orderId}] berhasil disinkronkan ke pusat!",
                        isWarning: false);
                    await LoadOrdersAsync();
                }
                else
                {
                    _audioHelper?.PlayWarning();
                    var msg = !string.IsNullOrWhiteSpace(failureReason)
                        ? $"Gagal mengupload dokumen retur [{orderId}] ke Cloud API relay:\r\n{failureReason}"
                        : $"Gagal mengupload dokumen retur [{orderId}] ke Cloud API relay.";
                    ShowNotification("Upload Gagal", msg, isWarning: true);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                ShowNotification("Error Upload", $"Kesalahan saat upload: {ex.Message}", isWarning: true);
            }
            finally
            {
                IsUploading = false;
                UploadButtonText = "☁️ Upload Sekarang";
                OnPropertyChanged(nameof(CanUploadOrder));
            }
        }

        private void ShowNotification(string title, string message, bool isWarning)
        {
            if (ShowNotificationAction != null)
            {
                ShowNotificationAction(title, message, isWarning);
            }
            else
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    isWarning ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
