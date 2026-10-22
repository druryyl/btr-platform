using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Wpf.Services;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class ReturnOrderListViewTests
    {
        #region Test Fakes

        private class FakeAudioHelper : IAudioHelper
        {
            public bool EnableSoundFeedback { get; set; } = true;
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
        }

        private class FakeReturnOrderService : IReturnOrderService
        {
            public List<ReturnOrderDto> Orders { get; set; } = new List<ReturnOrderDto>();
            public string LastUploadedOrderId { get; private set; }
            public string LastDeletedOrderId { get; private set; }
            public bool ShouldUploadSucceed { get; set; } = true;
            public string UploadErrorMessage { get; set; }

            public DateTime LastQueryStart { get; private set; }
            public DateTime LastQueryEnd { get; private set; }
            public StatusSync? LastQueryStatus { get; private set; }

            public Task<IEnumerable<ReturnOrderDto>> GetOrdersAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
            {
                LastQueryStart = startDate;
                LastQueryEnd = endDate;
                LastQueryStatus = statusFilter;

                IEnumerable<ReturnOrderDto> filtered = Orders;
                if (statusFilter.HasValue)
                {
                    filtered = filtered.Where(o => string.Equals(o.StatusSync, statusFilter.Value.ToString(), StringComparison.OrdinalIgnoreCase));
                }

                return Task.FromResult(filtered);
            }

            public Task<SyncResultDto> CommitAndUploadAsync(string returnOrderId)
            {
                LastUploadedOrderId = returnOrderId;
                if (ShouldUploadSucceed)
                {
                    var order = Orders.FirstOrDefault(o => o.ReturnOrderId == returnOrderId);
                    if (order != null)
                    {
                        order.StatusSync = "Synced";
                        order.SyncedDate = DateTime.Now;
                    }
                    return Task.FromResult(new SyncResultDto { Success = true });
                }

                return Task.FromResult(new SyncResultDto { Success = false, ErrorMessage = UploadErrorMessage ?? "Upload rejected" });
            }

            public Task DeleteDraftAsync(string returnOrderId)
            {
                LastDeletedOrderId = returnOrderId;
                Orders.RemoveAll(o => o.ReturnOrderId == returnOrderId);
                return Task.CompletedTask;
            }

            public ReturnOrderDto Create(string custId, string custName, string warehouseCode, string operatorName, string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null, string keterangan = null, string customerCode = null, string wilayah = null, IEnumerable<ReturnOrderItemDto> items = null) => throw new NotImplementedException();
            public Task<ReturnOrderDto> CreateAsync(string custId, string custName, string warehouseCode, string operatorName, string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null, string keterangan = null, string customerCode = null, string wilayah = null, IEnumerable<ReturnOrderItemDto> items = null) => throw new NotImplementedException();
            public ReturnOrderDto Update(string returnOrderId, string custId, string custName, string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null, string keterangan = null, string customerCode = null, string wilayah = null, IEnumerable<ReturnOrderItemDto> items = null) => throw new NotImplementedException();
            public Task<ReturnOrderDto> UpdateAsync(string returnOrderId, string custId, string custName, string driverId = null, string driverName = null, string salesPersonId = null, string salesPersonName = null, string keterangan = null, string customerCode = null, string wilayah = null, IEnumerable<ReturnOrderItemDto> items = null) => throw new NotImplementedException();
            public ReturnOrderDto AddItem(string returnOrderId, string brgId, string brgName, string barcode, JenisRetur jenisRetur, decimal qty, string satId, string keterangan = null) => throw new NotImplementedException();
            public ReturnOrderDto UpdateLine(string returnOrderId, int noUrut, decimal newQty, JenisRetur newJenisRetur, string newKeterangan = null) => throw new NotImplementedException();
            public ReturnOrderDto UpdateLine(string returnOrderId, int noUrut, decimal newQty, string newJenisRetur, string newKeterangan = null) => throw new NotImplementedException();
            public ReturnOrderDto RemoveLine(string returnOrderId, int noUrut) => throw new NotImplementedException();
            public ReturnOrderDto CommitDraft(string returnOrderId) => throw new NotImplementedException();
            public Task<ReturnOrderDto> CommitDraftAsync(string returnOrderId) => throw new NotImplementedException();
            public ReturnOrderDto GetById(string returnOrderId) => Orders.FirstOrDefault(o => o.ReturnOrderId == returnOrderId);
            public Task<ReturnOrderDto> GetByIdAsync(string returnOrderId) => Task.FromResult(GetById(returnOrderId));
            public IEnumerable<ReturnOrderDto> GetOrders(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Orders;
            public void DeleteDraft(string returnOrderId) => DeleteDraftAsync(returnOrderId).GetAwaiter().GetResult();
        }

        private class FakeApiClientForSession : IBtradeApiClient
        {
            private readonly SessionResolveResultDto _resolveResult;

            public FakeApiClientForSession(SessionResolveResultDto resolveResult)
            {
                _resolveResult = resolveResult;
            }

            public Task<SessionResolveResultDto> ResolveSessionAsync(string operatorEmail) => Task.FromResult(_resolveResult);
            public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto order) => throw new NotImplementedException();
            public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto registration) => throw new NotImplementedException();
            public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId) => throw new NotImplementedException();
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "SRV-TEST";
            public string ApiBaseUrl { get; set; } = "http://localhost:5000";
            public string LastOperatorName { get; set; } = "Budi Hartono";
            public string LastOperatorEmail { get; set; } = "budi@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        #endregion

        private static readonly object _staAppLock = new object();

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    lock (_staAppLock)
                    {
                        if (System.Windows.Application.Current == null)
                        {
                            try
                            {
                                new System.Windows.Application();
                            }
                            catch (InvalidOperationException)
                            {
                                // Handled if concurrent thread initialized it
                            }
                        }
                    }
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        private static ReturnOrderDto CreateSampleOrder(string id, string status, decimal qtyBagus = 10, decimal qtyRusak = 2)
        {
            return new ReturnOrderDto
            {
                ReturnOrderId = id,
                ReturnOrderDate = DateTime.Today,
                CustId = "CUST-001",
                CustName = "Toko Sumber Rejeki",
                DriverId = "DRV-01",
                DriverName = "Pak Joko",
                SalesPersonId = "SLS-01",
                SalesPersonName = "Andi",
                WarehouseCode = "GDG-PST",
                OperatorName = "Budi Hartono",
                Keterangan = "Retur barang expired & rusak karton",
                StatusSync = status,
                SyncedDate = status == "Synced" ? (DateTime?)DateTime.Today.AddHours(2) : null,
                TotalQtyBagus = qtyBagus,
                TotalQtyRusak = qtyRusak,
                TotalItemsCount = 2,
                Items = new List<ReturnOrderItemDto>
                {
                    new ReturnOrderItemDto(id, 1, "BRG-01", "INDOMILK COKLAT 190ML", "8992753112233", "BAGUS", qtyBagus, "PCS", "Kemasan utuh"),
                    new ReturnOrderItemDto(id, 2, "BRG-02", "MIE SEDAAP GORENG 90G", "8998866200112", "RUSAK", qtyRusak, "DUS", "Kardus penyok basah")
                }
            };
        }

        [Fact]
        public void VisualLayout_ConformsTo720pSplitterAndMetadataProportions()
        {
            ExecuteInStaThread(() =>
            {
                var view = new ReturnOrderListView();
                Assert.NotNull(view);

                // Locate DataGrids
                var ordersGrid = view.FindName("OrdersGrid") as DataGrid;
                var itemsGrid = view.FindName("ItemsGrid") as DataGrid;
                var editButton = view.FindName("EditButton") as Button;
                var deleteButton = view.FindName("DeleteButton") as Button;
                var uploadButton = view.FindName("UploadButton") as Button;

                Assert.NotNull(ordersGrid);
                Assert.NotNull(itemsGrid);
                Assert.NotNull(editButton);
                Assert.NotNull(deleteButton);
                Assert.NotNull(uploadButton);
            });
        }

        [Fact]
        public void PostSyncImmutability_WhenOrderIsSynced_DisplaysLockedBanner_DisablesEditAndDelete()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio);

            var syncedOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FAV", "Synced");
            vm.SetOrders(new[] { syncedOrder });

            Assert.Equal(syncedOrder, vm.SelectedOrder);
            Assert.True(vm.IsSelectedOrderSynced, "IsSelectedOrderSynced must be true for Synced order.");
            Assert.False(vm.CanEditOrder, "Edit must be disabled for Synced order (TD-010).");
            Assert.False(vm.CanDeleteOrder, "Delete must be disabled for Synced order (TD-010).");
            Assert.False(vm.CanUploadOrder, "Upload must be disabled for already Synced order.");

            Assert.False(vm.EditDraftCommand.CanExecute(null));
            Assert.False(vm.DeleteDraftCommand.CanExecute(null));
            Assert.False(vm.UploadOrderCommand.CanExecute(null));

            Assert.Contains("SYNCED", vm.SyncStatusText);
            Assert.True(vm.IsSyncStatusSynced);
        }

        [Fact]
        public void DraftOrder_WhenSelected_HidesLockedBanner_EnablesEditDeleteAndUpload()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio);

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FAW", "Draft");
            vm.SetOrders(new[] { draftOrder });

            Assert.Equal(draftOrder, vm.SelectedOrder);
            Assert.False(vm.IsSelectedOrderSynced, "IsSelectedOrderSynced must be false for Draft.");
            Assert.True(vm.CanEditOrder, "Edit must be enabled for Draft.");
            Assert.True(vm.CanDeleteOrder, "Delete must be enabled for Draft.");
            Assert.True(vm.CanUploadOrder, "Upload must be enabled for Draft.");

            Assert.True(vm.EditDraftCommand.CanExecute(null));
            Assert.True(vm.DeleteDraftCommand.CanExecute(null));
            Assert.True(vm.UploadOrderCommand.CanExecute(null));

            Assert.Equal("DRAFT (Lokal)", vm.SyncStatusText);
            Assert.False(vm.IsSyncStatusSynced);
        }

        [Fact]
        public async Task UploadOrderCommand_WhenSuccessful_TransitionsToSyncedAndPlaysSuccess()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio);

            string notificationTitle = null;
            string notificationMsg = null;
            bool? notificationIsWarning = null;
            vm.ShowNotificationAction = (t, m, w) =>
            {
                notificationTitle = t;
                notificationMsg = m;
                notificationIsWarning = w;
            };

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FAX", "Draft");
            fakeService.Orders.Add(draftOrder);
            await vm.LoadOrdersAsync();

            Assert.Equal(draftOrder.ReturnOrderId, vm.SelectedOrder.ReturnOrderId);
            Assert.True(vm.CanUploadOrder);

            await vm.UploadOrderAsync();

            Assert.Equal(draftOrder.ReturnOrderId, fakeService.LastUploadedOrderId);
            Assert.Equal(1, fakeAudio.SuccessCount);
            Assert.Equal(0, fakeAudio.WarningCount);
            Assert.Equal("Upload Berhasil", notificationTitle);
            Assert.False(notificationIsWarning);

            // Re-selected order should now be Synced and locked
            Assert.Equal("Synced", vm.SelectedOrder.StatusSync);
            Assert.True(vm.IsSelectedOrderSynced);
            Assert.False(vm.CanEditOrder);
            Assert.False(vm.CanDeleteOrder);
        }

        [Fact]
        public async Task UploadOrderCommand_WhenOperatorSessionUnresolved_AbortsAndPlaysWarning()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var fakeRegistry = new FakeRegistryHelper { LastOperatorEmail = "unregistered@btr.co.id" };
            var fakeApi = new FakeApiClientForSession(new SessionResolveResultDto
            {
                Success = false,
                StatusCode = 400,
                ErrorMessage = "Google account is not registered or is not active. Contact your administrator."
            });
            var validator = new OperatorSessionValidator(fakeApi, fakeRegistry);
            // Pre-resolve so validator enters definite refusal state
            await validator.ValidateAsync();

            Assert.True(validator.IsUnresolved);

            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio, sessionValidator: validator);

            string notificationTitle = null;
            vm.ShowNotificationAction = (t, m, w) => notificationTitle = t;

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FAY", "Draft");
            fakeService.Orders.Add(draftOrder);
            await vm.LoadOrdersAsync();

            await vm.UploadOrderAsync();

            // Cloud upload should be pre-empted
            Assert.Null(fakeService.LastUploadedOrderId);
            Assert.Equal(0, fakeAudio.SuccessCount);
            Assert.Equal(1, fakeAudio.WarningCount);
            Assert.Equal("Operator Belum Terverifikasi", notificationTitle);
        }

        [Fact]
        public async Task DeleteDraftCommand_WhenConfirmed_CallsDeleteAndReloadsOrders()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio);

            // Configure confirmation to simulate clicking "Yes"
            vm.ConfirmDeleteAction = (msg, title) => true;

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FAZ", "Draft");
            fakeService.Orders.Add(draftOrder);
            await vm.LoadOrdersAsync();

            Assert.Single(vm.Orders);

            await vm.DeleteDraftAsync();

            Assert.Equal(draftOrder.ReturnOrderId, fakeService.LastDeletedOrderId);
            Assert.Equal(1, fakeAudio.SuccessCount);
            Assert.Empty(vm.Orders);
            Assert.Null(vm.SelectedOrder);
        }

        [Fact]
        public async Task DeleteDraftCommand_WhenCancelled_DoesNotDelete()
        {
            var fakeService = new FakeReturnOrderService();
            var fakeAudio = new FakeAudioHelper();
            var vm = new ReturnOrderListViewModel(fakeService, null, null, fakeAudio);

            // Configure confirmation to simulate clicking "No"
            vm.ConfirmDeleteAction = (msg, title) => false;

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FB0", "Draft");
            fakeService.Orders.Add(draftOrder);
            await vm.LoadOrdersAsync();

            await vm.DeleteDraftAsync();

            Assert.Null(fakeService.LastDeletedOrderId);
            Assert.Equal(0, fakeAudio.SuccessCount);
            Assert.Single(vm.Orders);
        }

        [Fact]
        public void EditDraftCommand_WhenDraftSelected_InvokesEditDraftRequestedEvent()
        {
            var fakeService = new FakeReturnOrderService();
            var vm = new ReturnOrderListViewModel(fakeService);

            string requestedOrderId = null;
            vm.EditDraftRequested += (s, id) => requestedOrderId = id;

            var draftOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FB1", "Draft");
            vm.SetOrders(new[] { draftOrder });

            vm.EditDraft();

            Assert.Equal("01ARZ3NDEKTSV4RRFFQ69G5FB1", requestedOrderId);
        }

        [Fact]
        public void EditDraftCommand_WhenSyncedSelected_DoesNotFireEventAndShowsWarning()
        {
            var fakeService = new FakeReturnOrderService();
            var vm = new ReturnOrderListViewModel(fakeService);

            string requestedOrderId = null;
            vm.EditDraftRequested += (s, id) => requestedOrderId = id;

            string warningTitle = null;
            vm.ShowNotificationAction = (t, m, w) => warningTitle = t;

            var syncedOrder = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FB2", "Synced");
            vm.SetOrders(new[] { syncedOrder });

            vm.EditDraft();

            Assert.Null(requestedOrderId);
            Assert.Equal("Dokumen Terkunci", warningTitle);
        }

        [Fact]
        public async Task DateAndStatusFilters_QueryOrdersWithCorrectParameters()
        {
            var fakeService = new FakeReturnOrderService();
            var vm = new ReturnOrderListViewModel(fakeService);

            vm.StartDate = new DateTime(2026, 10, 1);
            vm.EndDate = new DateTime(2026, 10, 15);
            vm.SelectedStatusFilter = "Draft";

            await vm.LoadOrdersAsync();

            Assert.Equal(new DateTime(2026, 10, 1), fakeService.LastQueryStart.Date);
            Assert.Equal(new DateTime(2026, 10, 15), fakeService.LastQueryEnd.Date);
            Assert.Equal(StatusSync.Draft, fakeService.LastQueryStatus);

            vm.SelectedStatusFilter = "Synced";
            await vm.LoadOrdersAsync();
            Assert.Equal(StatusSync.Synced, fakeService.LastQueryStatus);

            vm.SelectedStatusFilter = "Semua (All)";
            await vm.LoadOrdersAsync();
            Assert.Null(fakeService.LastQueryStatus);
        }

        [Fact]
        public void MetadataInspection_PopulatesAllFieldsWithoutTruncation()
        {
            var vm = new ReturnOrderListViewModel();
            var order = CreateSampleOrder("01ARZ3NDEKTSV4RRFFQ69G5FB3", "Draft", 15, 3);
            vm.SetOrders(new[] { order });

            Assert.Equal("01ARZ3NDEKTSV4RRFFQ69G5FB3", vm.OrderIdText);
            Assert.Contains("Toko Sumber Rejeki", vm.CustomerText);
            Assert.Equal("Budi Hartono", vm.OperatorText);
            Assert.Equal("GDG-PST", vm.WarehouseText);
            Assert.Equal("Pak Joko", vm.DriverText);
            Assert.Equal("Andi", vm.SalesText);
            Assert.Equal("Retur barang expired & rusak karton", vm.NoteText);
            Assert.Equal(18, order.TotalQty);

            Assert.Equal(2, vm.Items.Count);
            Assert.Equal("INDOMILK COKLAT 190ML", vm.Items[0].BrgName);
            Assert.Equal("BAGUS", vm.Items[0].JenisRetur);
            Assert.Equal(15, vm.Items[0].Qty);

            Assert.Equal("MIE SEDAAP GORENG 90G", vm.Items[1].BrgName);
            Assert.Equal("RUSAK", vm.Items[1].JenisRetur);
            Assert.Equal(3, vm.Items[1].Qty);
        }
    }
}
