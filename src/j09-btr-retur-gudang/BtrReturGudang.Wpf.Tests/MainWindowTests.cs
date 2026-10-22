using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.ValueObjects;
using BtrReturGudang.Wpf.Mvvm;
using BtrReturGudang.Wpf.Services;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class MainWindowTests
    {
        #region Test Fakes

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "SRV-TEST";
            public string ApiBaseUrl { get; set; } = "http://localhost:5000";
            public string LastOperatorName { get; set; } = "Budi Hartono";
            public string LastOperatorEmail { get; set; } = "budi@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class FakeSyncService : ISyncService
        {
            public bool IsSyncInProgress { get; set; } = false;
            public bool ShouldSucceed { get; set; } = true;
            public int RunCount { get; private set; } = 0;

            public Task<SyncResultDto> TriggerDocumentUploadAsync(string returnOrderId)
            {
                return Task.FromResult(new SyncResultDto { Success = ShouldSucceed });
            }

            public Task<IEnumerable<ReturnOrderDto>> QueryPendingQueueAsync()
            {
                return Task.FromResult<IEnumerable<ReturnOrderDto>>(new List<ReturnOrderDto>());
            }

            public Task<IEnumerable<BarcodeRegistrationDto>> QueryPendingRegistrationQueueAsync()
            {
                return Task.FromResult<IEnumerable<BarcodeRegistrationDto>>(new List<BarcodeRegistrationDto>());
            }

            public Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null)
            {
                return Task.FromResult(new SyncResultDto { Success = ShouldSucceed });
            }

            public Task<SyncResultDto> RunFullSyncPassAsync()
            {
                RunCount++;
                return Task.FromResult(new SyncResultDto
                {
                    Success = ShouldSucceed,
                    ErrorMessage = ShouldSucceed ? null : "Network unreachable"
                });
            }
        }

        private class FakeBtradeApiClient : IBtradeApiClient
        {
            public bool ShouldResolve { get; set; } = true;
            public int StatusCode { get; set; } = 200;
            public string RefusalMessage { get; set; } = null;

            public Task<SessionResolveResultDto> ResolveSessionAsync(string operatorEmail)
            {
                return Task.FromResult(new SessionResolveResultDto
                {
                    Success = ShouldResolve,
                    StatusCode = StatusCode,
                    UserName = ShouldResolve ? "Budi Hartono" : null,
                    UserId = ShouldResolve ? "10" : null,
                    RoleId = ShouldResolve ? "GUDANG" : null,
                    ErrorMessage = RefusalMessage
                });
            }

            public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto) =>
                Task.FromResult(new ApiUploadResultDto { Success = true });

            public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto) =>
                Task.FromResult(new ApiUploadResultDto { Success = true });

            public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId) =>
                Task.FromResult(new MasterSyncDto());
        }

        private class TestOrderActionHandler : ViewModelBase, IOrderActionHandler, IScannerConsumer
        {
            public bool SaveDraftCalled { get; private set; }
            public bool SaveAndUploadCalled { get; private set; }
            public bool CancelOrClearCalled { get; private set; }
            public string LastScannedBarcode { get; private set; }

            public void HandleSaveDraft() => SaveDraftCalled = true;
            public void HandleSaveAndUpload() => SaveAndUploadCalled = true;
            public void HandleCancelOrClear() => CancelOrClearCalled = true;
            public void OnBarcodeScanned(string barcode) => LastScannedBarcode = barcode;
        }

        #endregion

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
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

        [Fact]
        public void DependencyInjection_WiresAllRequiredWpfServicesSuccessfully()
        {
            var services = new ServiceCollection();
            App.ConfigureServices(services);
            using (var provider = services.BuildServiceProvider())
            {
                Assert.NotNull(provider.GetService<IRegistryHelper>());
                Assert.NotNull(provider.GetService<ISyncService>());
                Assert.NotNull(provider.GetService<WpfScannerWedgeListener>());
                Assert.NotNull(provider.GetService<OperatorSessionValidator>());
                Assert.NotNull(provider.GetService<MainViewModel>());
                Assert.NotNull(provider.GetService<ReturnOrderEntryViewModel>());
                Assert.NotNull(provider.GetService<ReturnOrderListViewModel>());
                Assert.NotNull(provider.GetService<BarcodeRegistryViewModel>());
                Assert.NotNull(provider.GetService<SettingsViewModel>());
            }
        }

        [Fact]
        public void MainWindow_WindowBounds_ConformTo1280x720Specifications()
        {
            ExecuteInStaThread(() =>
            {
                var fakeRegistry = new FakeRegistryHelper();
                var fakeSync = new FakeSyncService();
                var scanner = new WpfScannerWedgeListener(() => 0);
                var vm = new MainViewModel(null, fakeSync, fakeRegistry, scanner);

                var window = new MainWindow(vm);
                try
                {
                    Assert.Equal(1280, window.Width);
                    Assert.Equal(720, window.Height);
                    Assert.Equal(1024, window.MinWidth);
                    Assert.Equal(600, window.MinHeight);
                }
                finally
                {
                    vm.Dispose();
                }
            });
        }

        [Fact]
        public void MainWindow_GlobalHotkeys_BoundViaInputBindings()
        {
            ExecuteInStaThread(() =>
            {
                var fakeRegistry = new FakeRegistryHelper();
                var fakeSync = new FakeSyncService();
                var vm = new MainViewModel(null, fakeSync, fakeRegistry);

                var window = new MainWindow(vm);
                try
                {
                    var bindings = window.InputBindings;
                    Assert.NotNull(bindings);
                    Assert.True(bindings.Count >= 7);

                    var mappedKeys = new HashSet<Key>();
                    foreach (InputBinding b in bindings)
                    {
                        if (b is KeyBinding kb)
                        {
                            mappedKeys.Add(kb.Key);
                        }
                    }

                    Assert.Contains(Key.F1, mappedKeys);
                    Assert.Contains(Key.F3, mappedKeys);
                    Assert.Contains(Key.F4, mappedKeys);
                    Assert.Contains(Key.F5, mappedKeys);
                    Assert.Contains(Key.F9, mappedKeys);
                    Assert.Contains(Key.F10, mappedKeys);
                    Assert.Contains(Key.Escape, mappedKeys);
                }
                finally
                {
                    vm.Dispose();
                }
            });
        }

        [Fact]
        public void MainViewModel_NavigationSwitchesModuleAndUpdatesActiveHighlight()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService();
            var vm = new MainViewModel(null, fakeSync, fakeRegistry);
            try
            {
                // Default: Intake
                Assert.Equal(NavigationModule.Intake, vm.CurrentModule);
                Assert.True(vm.IsIntakeActive);
                Assert.False(vm.IsDaftarReturActive);
                Assert.False(vm.IsBarcodeRegistryActive);
                Assert.False(vm.IsPengaturanActive);
                Assert.IsType<ReturnOrderEntryViewModel>(vm.CurrentViewModel);

                // Switch to Daftar Retur (F3)
                vm.NavigateToDaftarReturCommand.Execute(null);
                Assert.Equal(NavigationModule.DaftarRetur, vm.CurrentModule);
                Assert.False(vm.IsIntakeActive);
                Assert.True(vm.IsDaftarReturActive);
                Assert.IsType<ReturnOrderListViewModel>(vm.CurrentViewModel);

                // Switch to Barcode Registry (F4)
                vm.NavigateToBarcodeRegistryCommand.Execute(null);
                Assert.Equal(NavigationModule.BarcodeRegistry, vm.CurrentModule);
                Assert.False(vm.IsDaftarReturActive);
                Assert.True(vm.IsBarcodeRegistryActive);
                Assert.IsType<BarcodeRegistryViewModel>(vm.CurrentViewModel);

                // Switch to Pengaturan (F5)
                vm.NavigateToSettingsCommand.Execute(null);
                Assert.Equal(NavigationModule.Pengaturan, vm.CurrentModule);
                Assert.False(vm.IsBarcodeRegistryActive);
                Assert.True(vm.IsPengaturanActive);
                Assert.IsType<SettingsViewModel>(vm.CurrentViewModel);

                // Switch back to Intake (F1)
                vm.NavigateToIntakeCommand.Execute(null);
                Assert.Equal(NavigationModule.Intake, vm.CurrentModule);
                Assert.True(vm.IsIntakeActive);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public void MainViewModel_HeaderAndWorkstationMetadata_ReflectsRegistry()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-SBY",
                LastOperatorName = "Agus Santoso",
                LastOperatorEmail = "agus@btr.co.id"
            };
            var fakeSync = new FakeSyncService();
            var vm = new MainViewModel(null, fakeSync, fakeRegistry);
            try
            {
                Assert.Equal("GDG-SBY", vm.DepoCode);
                Assert.Equal("🏢 DEPO: GDG-SBY", vm.DepoBadgeText);
                Assert.Equal("Agus Santoso", vm.OperatorName);
                Assert.Equal("👤 Agus Santoso", vm.OperatorBadgeText);
                Assert.Equal("agus@btr.co.id", vm.OperatorEmail);
                Assert.Equal("Google Email Operator: agus@btr.co.id", vm.OperatorEmailTooltip);
                Assert.True(vm.IsOnline);
                Assert.Equal("ONLINE", vm.OnlineStatusText);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public async Task MainViewModel_ManualSync_ExecutesSuccessfullyAndUpdatesOnlineStatus()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService { ShouldSucceed = true };
            var vm = new MainViewModel(null, fakeSync, fakeRegistry);
            try
            {
                Assert.Equal(0, fakeSync.RunCount);
                await vm.TriggerManualSyncForTestingAsync();

                Assert.Equal(1, fakeSync.RunCount);
                Assert.True(vm.LastSyncSuccess);
                Assert.True(vm.IsOnline);
                Assert.Equal("ONLINE", vm.OnlineStatusText);
                Assert.Contains("(Sukses)", vm.LastSyncStatusText);
                Assert.False(vm.IsSyncRunning);
                Assert.Equal("🔄 Sync Sekarang", vm.ManualSyncButtonText);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public async Task MainViewModel_SyncFailure_UpdatesOfflineStatus()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService { ShouldSucceed = false };
            var vm = new MainViewModel(null, fakeSync, fakeRegistry);
            try
            {
                await vm.TriggerManualSyncForTestingAsync();

                Assert.False(vm.LastSyncSuccess);
                Assert.False(vm.IsOnline);
                Assert.Equal("OFFLINE", vm.OnlineStatusText);
                Assert.Contains("(Offline/Pending)", vm.LastSyncStatusText);
                Assert.False(vm.IsSyncRunning);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public async Task MainViewModel_UnresolvedOperator_PreemptsSyncWithRefusalMessage()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService();
            var fakeApi = new FakeBtradeApiClient
            {
                ShouldResolve = false,
                StatusCode = 400,
                RefusalMessage = "Google account not recognized."
            };
            var validator = new OperatorSessionValidator(fakeApi, fakeRegistry);
            var vm = new MainViewModel(null, fakeSync, fakeRegistry, sessionValidator: validator);
            vm.SuppressSessionWarnings = true;
            try
            {
                // Validate to make validator unresolved
                await validator.ValidateAsync();
                Assert.True(validator.IsUnresolved);

                // Attempt sync
                await vm.TriggerManualSyncForTestingAsync();

                // Should be pre-empted without calling sync service
                Assert.Equal(0, fakeSync.RunCount);
                Assert.Contains("Sinkronisasi dihentikan", vm.StatusInfoText);
                Assert.False(vm.IsOnline);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public void MainViewModel_GlobalHotkeys_DelegatesToOrderActionHandler()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService();
            var vm = new MainViewModel(null, fakeSync, fakeRegistry);
            try
            {
                var actionHandler = new TestOrderActionHandler();
                // Replace current view model with test handler
                typeof(MainViewModel).GetProperty(nameof(MainViewModel.CurrentViewModel))
                    ?.SetValue(vm, actionHandler, null);

                vm.SaveDraftCommand.Execute(null);
                Assert.True(actionHandler.SaveDraftCalled);

                vm.SaveAndUploadCommand.Execute(null);
                Assert.True(actionHandler.SaveAndUploadCalled);

                vm.CancelOrClearCommand.Execute(null);
                Assert.True(actionHandler.CancelOrClearCalled);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public void MainViewModel_ScannerEvent_DispatchesToActiveConsumer()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService();
            var scanner = new WpfScannerWedgeListener(() => 0);
            var vm = new MainViewModel(null, fakeSync, fakeRegistry, scannerListener: scanner);
            try
            {
                var actionHandler = new TestOrderActionHandler();
                typeof(MainViewModel).GetProperty(nameof(MainViewModel.CurrentViewModel))
                    ?.SetValue(vm, actionHandler, null);

                // Simulate scanner wedge scan
                scanner.SimulateScan("8991234567890");

                Assert.Equal("8991234567890", actionHandler.LastScannedBarcode);
                Assert.Contains("8991234567890", vm.StatusInfoText);
            }
            finally
            {
                vm.Dispose();
            }
        }

        [Fact]
        public async Task MainViewModel_UpdateOperator_UpdatesRegistryAndTriggersValidation()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeSync = new FakeSyncService();
            var fakeApi = new FakeBtradeApiClient { ShouldResolve = true };
            var validator = new OperatorSessionValidator(fakeApi, fakeRegistry);
            var vm = new MainViewModel(null, fakeSync, fakeRegistry, sessionValidator: validator);
            vm.SuppressSessionWarnings = true;
            try
            {
                await vm.UpdateOperatorAsync("Siti Aminah", "siti@btr.co.id");

                Assert.Equal("Siti Aminah", vm.OperatorName);
                Assert.Equal("siti@btr.co.id", vm.OperatorEmail);
                Assert.Equal("Siti Aminah", fakeRegistry.LastOperatorName);
                Assert.Equal("siti@btr.co.id", fakeRegistry.LastOperatorEmail);
                Assert.Contains("Siti Aminah", vm.StatusInfoText);
            }
            finally
            {
                vm.Dispose();
            }
        }
    }
}
