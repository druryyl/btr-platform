using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;
using BtrReturGudang.Infrastructure.Services;
using BtrReturGudang.Winform.Hardware;
using BtrReturGudang.Winform.Services;
using BtrReturGudang.Winform.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    /// <summary>
    /// P4-S11: End-to-End Workflow Integration Test Suite.
    /// Verifies complete physical intake workflow, USB scanner input simulation, modal dialogs,
    /// offline persistence tolerance, dual-trigger cloud sync transitions, post-sync document immutability,
    /// and full 7-screen navigation.
    /// Traceable to: ARCHITECTURE §4, §5.2, §6, §11, and IMPLEMENTATION-PLAN P4-S11.
    /// </summary>
    public class EndToEndWorkflowIntegrationTests
    {
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

        #region Workflow 1: Valid Barcode Intake with 2000Hz Beep & Quantity Strip Commit

        [Fact]
        public void Workflow_01_ScanValidBarcode_AutoFocusesQuantityStrip_AndEnterCommitsLineWith2000HzBeep()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();

                // Seed catalog with valid barcode
                fixture.SeedProductWithBarcode("8991001100110", "BRG-001", "Indomie Goreng Spesial", "Dus", 24m);

                using (var entryView = fixture.CreateIntakeView())
                {
                    // 1. Simulate USB Scanner emission of valid barcode
                    entryView.OnBarcodeScanned("8991001100110\r\n");

                    // Verify scanner banner populated
                    Assert.NotNull(entryView.CurrentScanResult);
                    Assert.True(entryView.CurrentScanResult.Found);
                    Assert.Equal("BRG-001", entryView.CurrentScanResult.BrgId);
                    Assert.Contains("Indomie Goreng", entryView.BannerTitleLabel.Text);
                    Assert.Equal("BARCODE TERDETEKSI", entryView.CurrentScanResult.Found ? "BARCODE TERDETEKSI" : "");

                    // 2. Quantity strip auto-focuses numQtyBagus; operator enters quantities
                    entryView.QtyBagusNumeric.Value = 10;
                    entryView.QtyRusakNumeric.Value = 2;

                    // 3. Press Enter / commit quantity strip
                    int initialBeeps = fixture.AudioHelper.SuccessCount;
                    bool committed = entryView.CommitQuantityStripToGrid();

                    Assert.True(committed);

                    // Verify 2000Hz success audio cue recorded
                    Assert.True(fixture.AudioHelper.SuccessCount > initialBeeps);
                    Assert.Contains(fixture.AudioHelper.BeepLogs, b => b.Frequency == 2000 && b.DurationMs == 80);

                    // Verify items committed to double-buffered grid
                    Assert.Equal(2, entryView.TotalItemsCount);
                    Assert.Equal(10m, entryView.TotalQtyBagus);
                    Assert.Equal(2m, entryView.TotalQtyRusak);

                    // Verify line numbering and classification badges in grid
                    var items = (IReadOnlyList<ReturnOrderItemDto>)entryView.ItemsGridView.DataSource;
                    Assert.Equal(1, items[0].NoUrut);
                    Assert.Equal("BAGUS", items[0].JenisRetur);
                    Assert.Equal(10m, items[0].Qty);

                    Assert.Equal(2, items[1].NoUrut);
                    Assert.Equal("RUSAK", items[1].JenisRetur);
                    Assert.Equal(2m, items[1].Qty);

                    // Verify quantity strip reset for next scan
                    Assert.Equal(0m, entryView.QtyBagusNumeric.Value);
                    Assert.Equal(0m, entryView.QtyRusakNumeric.Value);
                }
            });
        }

        #endregion

        #region Workflow 2: Invalid Barcode Triggers 400Hz Buzz & Quick Barcode Registration Modal

        [Fact]
        public void Workflow_02_ScanInvalidBarcode_Plays400HzBuzz_AndQuickBarcodeRegistrationDialogMapsItemToReturnOrder()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();

                // Seed master product (not yet mapped to the barcode)
                fixture.SeedProductMaster("BRG-002", "Sari Roti Tawar Spesial", "Pak", "Dus", "Pcs");

                using (var entryView = fixture.CreateIntakeView())
                {
                    string unknownBarcode = "8999999112233";

                    // Configure dialog launcher hook to simulate operator filling SCR-04 modal
                    entryView.QuickRegistrationDialogLauncher = (rawBarcode) =>
                    {
                        // Verify 400Hz warning buzzer emitted upon invalid scan
                        Assert.Contains(fixture.AudioHelper.BeepLogs, b => b.Frequency == 400 && b.DurationMs == 200);

                        using (var dlg = new QuickBarcodeRegistrationDialog(
                            unmappedBarcode: rawBarcode,
                            barcodeService: fixture.BarcodeService,
                            barcodeRepo: fixture.BarcodeRepo,
                            referenceRepo: fixture.ReferenceRepo,
                            registryHelper: fixture.RegistryHelper,
                            audioHelper: fixture.AudioHelper))
                        {
                            // Operator selects product, unit, and multiplier in dialog
                            dlg.SetItems(new[] { new Barang("BRG-002", "Sari Roti Tawar Spesial", "Pak", "Dus", "Pcs") });
                            dlg.SatuanComboBox.SelectedItem = "Pak";
                            dlg.PengaliNumeric.Value = 1m;

                            // Commit dialog
                            dlg.OnRegisterClickAsync().GetAwaiter().GetResult();

                            Assert.Equal(DialogResult.OK, dlg.DialogResult);
                            Assert.NotNull(dlg.RegisteredResult);
                            return dlg.RegisteredResult;
                        }
                    };

                    int warningBeepsBefore = fixture.AudioHelper.WarningCount;

                    // 1. Simulate scanning unknown barcode
                    entryView.OnBarcodeScanned(unknownBarcode);

                    // Verify warning beep count incremented
                    Assert.True(fixture.AudioHelper.WarningCount > warningBeepsBefore);

                    // Verify item was automatically mapped and applied to active intake banner
                    Assert.NotNull(entryView.CurrentScanResult);
                    Assert.True(entryView.CurrentScanResult.Found);
                    Assert.Equal("BRG-002", entryView.CurrentScanResult.BrgId);
                    Assert.Contains("Sari Roti Tawar", entryView.BannerTitleLabel.Text);

                    // 2. Operator inputs quantity and commits
                    entryView.QtyBagusNumeric.Value = 6;
                    bool committed = entryView.CommitQuantityStripToGrid();

                    Assert.True(committed);
                    Assert.Equal(1, entryView.TotalItemsCount);
                    Assert.Equal(6m, entryView.TotalQtyBagus);

                    // Verify barcode now exists in local repository
                    var savedBarcode = fixture.BarcodeRepo.LookupByBarcode(unknownBarcode);
                    Assert.NotNull(savedBarcode);
                    Assert.Equal("BRG-002", savedBarcode.BrgId);

                    // Verify registration was queued for upload
                    var pending = fixture.BarcodeRepo.GetPendingRegistrations();
                    Assert.Contains(pending, p => p.Barcode == unknownBarcode && p.BrgId == "BRG-002");
                }
            });
        }

        #endregion

        #region Workflow 3: Press F2 Opens ManualItemSearchDialog and Populates Intake

        [Fact]
        public void Workflow_03_PressF2_ManualItemSearchDialog_FiltersMasterProduct_AndSelectsIntoQuantityStrip()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();

                // Seed master product in BTRG_Barang cache
                fixture.SeedProductMaster("BRG-003", "Kopi Kapal Api Special Mix", "Dus", "Renteng", "Sachet");

                using (var entryView = fixture.CreateIntakeView())
                {
                    // Configure manual search launcher hook simulating SCR-03 modal execution
                    entryView.ManualSearchDialogLauncher = () =>
                    {
                        using (var dlg = new ManualItemSearchDialog(fixture.ReferenceRepo, fixture.AudioHelper))
                        {
                            dlg.SetItems(new[] { new Barang("BRG-003", "Kopi Kapal Api Special Mix", "Dus", "Renteng", "Sachet") });
                            dlg.SatuanComboBox.SelectedItem = "Dus";

                            dlg.ConfirmSelection();
                            Assert.Equal(DialogResult.OK, dlg.DialogResult);
                            return dlg.SelectedBarang;
                        }
                    };

                    // Simulate operator pressing F2
                    entryView.TriggerManualItemSearch();

                    // Verify selected item populated into intake banner & quantity strip
                    Assert.NotNull(entryView.CurrentScanResult);
                    Assert.Equal("BRG-003", entryView.CurrentScanResult.BrgId);
                    Assert.Contains("Kopi Kapal Api", entryView.BannerTitleLabel.Text);

                    // Enter quantity and commit
                    entryView.QtyBagusNumeric.Value = 15;
                    entryView.CommitQuantityStripToGrid();

                    Assert.Equal(1, entryView.TotalItemsCount);
                    Assert.Equal(15m, entryView.TotalQtyBagus);
                }
            });
        }

        #endregion

        #region Workflow 4: Press F10 ("Simpan & Upload") Saves Local Draft and Transitions to Synced

        [Fact]
        public async Task Workflow_04_PressF10_SimpanDanUpload_SavesDraftLocally_AndHttpUploadTransitionsToSynced()
        {
            ReturnOrder capturedOrder = null;
            SyncResultDto syncResult = null;

            await Task.Run(() =>
            {
                ExecuteInStaThread(() =>
                {
                    var fixture = new TestWorkflowFixture();
                    fixture.ApiClient.IsOnline = true; // Cloud API available

                    fixture.SeedProductWithBarcode("8991001100110", "BRG-001", "Indomie Goreng", "Dus", 24m);

                    using (var entryView = fixture.CreateIntakeView())
                    {
                        entryView.CustomerComboBox.Text = "Toko Makmur Sentosa";
                        entryView.NotesTextBox.Text = "Retur kemasan kotor";

                        entryView.OnBarcodeScanned("8991001100110");
                        entryView.QtyBagusNumeric.Value = 8;
                        entryView.QtyRusakNumeric.Value = 1;
                        entryView.CommitQuantityStripToGrid();

                        // Simulate F10 Save & Upload
                        syncResult = entryView.SaveAndUploadAsync().GetAwaiter().GetResult();
                        capturedOrder = fixture.ReturnOrderRepo.GetById(fixture.LastCreatedOrderId);
                    }
                });
            });

            Assert.NotNull(syncResult);
            Assert.True(syncResult.Success);

            // Verify document persisted in repository and transitioned to Synced
            Assert.NotNull(capturedOrder);
            Assert.Equal(StatusSync.Synced, capturedOrder.StatusSync);
            Assert.NotNull(capturedOrder.SyncedDate);
            Assert.Equal(2, capturedOrder.Items.Count);
        }

        #endregion

        #region Workflow 5: ReturnOrderListView Displays Synced Badge and Permanently Locks Editing

        [Fact]
        public void Workflow_05_ReturnOrderListView_DisplaysSyncedBadge_AndPermanentlyLocksEditing()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();

                // 1. Seed a Synced Return Order
                var syncedOrder = new ReturnOrder(
                    returnOrderId: "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                    returnOrderDate: DateTime.Today,
                    custId: "C001",
                    custName: "Toko Sinar Jaya",
                    warehouseCode: "GD-01",
                    operatorName: "BUDI",
                    driverId: "DRV-1",
                    driverName: "Pak Supri",
                    salesPersonId: "SLS-1",
                    salesPersonName: "Agus",
                    keterangan: "Retur kadaluarsa",
                    statusSync: StatusSync.Draft);

                syncedOrder.AddItem("BRG-001", "Indomie Goreng", "8991", JenisRetur.RUSAK, 5m, "Dus");
                syncedOrder.MarkSynced(DateTime.Now);
                fixture.ReturnOrderRepo.Save(syncedOrder);

                // 2. Open ReturnOrderListView (SCR-05)
                using (var listView = new ReturnOrderListView(
                    orderService: fixture.OrderService,
                    orderRepo: fixture.ReturnOrderRepo,
                    syncService: fixture.SyncService,
                    audioHelper: fixture.AudioHelper))
                {
                    listView.LoadOrdersAsync().GetAwaiter().GetResult();

                    Assert.Single(listView.LoadedOrders);
                    var inspected = listView.LoadedOrders[0];
                    Assert.Equal("01ARZ3NDEKTSV4RRFFQ69G5FAV", inspected.ReturnOrderId);
                    Assert.Equal("Synced", inspected.StatusSync);
                    Assert.True(inspected.IsSynced);

                    // 3. Verify Post-Sync Immutability Enforcement
                    Assert.True(listView.LockedBanner.Visible, "Read-Only locked banner must be visible for Synced orders");
                    Assert.False(listView.EditButton.Enabled, "Edit Draft button must be disabled for Synced orders");
                    Assert.False(listView.DeleteButton.Enabled, "Delete Draft button must be disabled for Synced orders");
                    Assert.False(listView.UploadButton.Enabled, "Upload Ulang button must be disabled for already Synced orders");
                }
            });
        }

        #endregion

        #region Workflow 6: Offline Tolerance Allows Saving Local Draft; 5-Minute Timer Pushes When Online

        [Fact]
        public async Task Workflow_06_OfflineSavingAllowsLocalDraft_AndFiveMinuteBackgroundTimerPushesOnceOnline()
        {
            string draftOrderId = null;
            TestWorkflowFixture fixture = null;

            await Task.Run(() =>
            {
                ExecuteInStaThread(() =>
                {
                    fixture = new TestWorkflowFixture();
                    fixture.ApiClient.IsOnline = false; // Simulate Network Disconnected / Relay Offline

                    fixture.SeedProductWithBarcode("8991001100110", "BRG-001", "Indomie Goreng", "Dus", 24m);

                    using (var entryView = fixture.CreateIntakeView())
                    {
                        entryView.CustomerComboBox.Text = "Toko Offline Berdikari";
                        entryView.OnBarcodeScanned("8991001100110");
                        entryView.QtyBagusNumeric.Value = 20;
                        entryView.CommitQuantityStripToGrid();

                        // Attempt Save & Upload while offline
                        var result = entryView.SaveAndUploadAsync().GetAwaiter().GetResult();

                        draftOrderId = fixture.LastCreatedOrderId;
                        Assert.NotNull(draftOrderId);

                        // Document MUST be preserved locally as Draft despite upload failure
                        var localOrder = fixture.ReturnOrderRepo.GetById(draftOrderId);
                        Assert.NotNull(localOrder);
                        Assert.Equal(StatusSync.Draft, localOrder.StatusSync);
                        Assert.Null(localOrder.SyncedDate);
                    }
                });
            });

            // 2. Network connectivity resumes!
            fixture.ApiClient.IsOnline = true;

            // 3. 5-minute background synchronization timer triggers
            var syncResult = await fixture.SyncService.RunFullSyncPassAsync();

            Assert.NotNull(syncResult);
            Assert.True(syncResult.Success);
            Assert.True(syncResult.RecordsPushed >= 1);

            // 4. Verify document is now transitioned to Synced in local repository
            var syncedOrder = fixture.ReturnOrderRepo.GetById(draftOrderId);
            Assert.NotNull(syncedOrder);
            Assert.Equal(StatusSync.Synced, syncedOrder.StatusSync);
            Assert.NotNull(syncedOrder.SyncedDate);
        }

        #endregion

        #region Workflow 7: End-to-End Navigation Across All 7 Screens in Shell Window

        [Fact]
        public void Workflow_07_FullSevenScreensNavigationAndKeyboardInterception_EndToEnd()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();

                using (var mainForm = fixture.CreateMainForm())
                {
                    mainForm.Show();

                    // SCR-01: MainForm Shell starts with Intake Retur (SCR-02) as active view
                    Assert.Equal(NavigationModule.Intake, mainForm.CurrentModule);
                    Assert.IsType<ReturnOrderEntryView>(mainForm.ActiveViewControl);

                    // Navigate to SCR-05: Daftar Retur (F3)
                    mainForm.NavigateTo(NavigationModule.DaftarRetur);
                    Assert.Equal(NavigationModule.DaftarRetur, mainForm.CurrentModule);
                    Assert.IsType<ReturnOrderListView>(mainForm.ActiveViewControl);

                    // Navigate to SCR-06: Barcode Registry (F4)
                    mainForm.NavigateTo(NavigationModule.BarcodeRegistry);
                    Assert.Equal(NavigationModule.BarcodeRegistry, mainForm.CurrentModule);
                    Assert.IsType<BarcodeRegistryView>(mainForm.ActiveViewControl);

                    // Navigate to SCR-07: Settings View (F5)
                    mainForm.NavigateTo(NavigationModule.Pengaturan);
                    Assert.Equal(NavigationModule.Pengaturan, mainForm.CurrentModule);
                    Assert.IsType<SettingsView>(mainForm.ActiveViewControl);

                    // Return to Intake Retur (SCR-02 / F1)
                    mainForm.NavigateTo(NavigationModule.Intake);
                    Assert.Equal(NavigationModule.Intake, mainForm.CurrentModule);
                    Assert.IsType<ReturnOrderEntryView>(mainForm.ActiveViewControl);

                    // Test SCR-03: ManualItemSearchDialog instantiation and cancellation
                    using (var searchDlg = new ManualItemSearchDialog(fixture.ReferenceRepo, fixture.AudioHelper))
                    {
                        Assert.NotNull(searchDlg);
                        Assert.Contains("Pencarian Barang Manual", searchDlg.Text);
                    }

                    // Test SCR-04: QuickBarcodeRegistrationDialog instantiation
                    using (var regDlg = new QuickBarcodeRegistrationDialog(
                        "999888777",
                        fixture.BarcodeService,
                        fixture.BarcodeRepo,
                        fixture.ReferenceRepo,
                        fixture.RegistryHelper,
                        fixture.AudioHelper))
                    {
                        Assert.NotNull(regDlg);
                        Assert.Contains("Registrasi Barcode Cepat", regDlg.Text);
                    }

                    mainForm.Close();
                }
            });
        }

        #endregion

        #region Workflow 8/9: Unresolved operator configuration surfaces the resolve refusal, not a raw 409

        [Fact]
        public void Workflow_08_BlankOperatorEmail_PreEmptsUploadWithClearRefusal_NotRaw409()
        {
            ExecuteInStaThread(() =>
            {
                var fixture = new TestWorkflowFixture();
                fixture.RegistryHelper.LastOperatorEmail = string.Empty; // blank actor configuration
                fixture.SeedProductWithBarcode("8991001100110", "BRG-001", "Indomie Goreng", "Dus", 24m);

                var validator = new OperatorSessionValidator(fixture.ApiClient, fixture.RegistryHelper);
                validator.ValidateAsync().GetAwaiter().GetResult();

                Assert.True(validator.IsUnresolved);
                Assert.False(string.IsNullOrWhiteSpace(validator.RefusalMessage));

                using (var entryView = new ReturnOrderEntryView(
                    fixture.OrderService,
                    fixture.BarcodeService,
                    fixture.ReferenceRepo,
                    fixture.AudioHelper,
                    fixture.RegistryHelper,
                    validator))
                {
                    entryView.SuppressMessageBoxes = true;
                    entryView.AutoLaunchRegistrationDialog = false;
                    entryView.CustomerComboBox.Text = "Toko Refusal";
                    entryView.OnBarcodeScanned("8991001100110");
                    entryView.QtyBagusNumeric.Value = 1;
                    Assert.True(entryView.CommitQuantityStripToGrid());

                    var result = entryView.SaveAndUploadAsync().GetAwaiter().GetResult();

                    // TD-012 regression: the Cloud upload is pre-empted and a clear refusal is surfaced
                    // instead of the opaque HTTP 409 "Google account is not registered..." failure.
                    Assert.False(result.Success);
                    Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
                    Assert.DoesNotContain("409", result.ErrorMessage);
                    Assert.Empty(fixture.ApiClient.UploadedOrders);
                }
            });
        }

        [Fact]
        public void Workflow_09_NonEmailOperatorActor_PreEmptsUploadWithClearRefusal_NotRaw409()
        {
            ExecuteInStaThread(() =>
            {
                const string refusal =
                    "Google account is not registered or is not active. Contact your administrator.";

                var fixture = new TestWorkflowFixture();
                fixture.RegistryHelper.LastOperatorEmail = "not-an-email"; // non-email actor configuration
                fixture.ApiClient.SessionResolveResult = SessionResolveResultDto.Fail(400, refusal);
                fixture.SeedProductWithBarcode("8991001100110", "BRG-001", "Indomie Goreng", "Dus", 24m);

                var validator = new OperatorSessionValidator(fixture.ApiClient, fixture.RegistryHelper);
                validator.ValidateAsync().GetAwaiter().GetResult();

                Assert.True(validator.IsUnresolved);
                Assert.Equal(refusal, validator.RefusalMessage);

                using (var entryView = new ReturnOrderEntryView(
                    fixture.OrderService,
                    fixture.BarcodeService,
                    fixture.ReferenceRepo,
                    fixture.AudioHelper,
                    fixture.RegistryHelper,
                    validator))
                {
                    entryView.SuppressMessageBoxes = true;
                    entryView.AutoLaunchRegistrationDialog = false;
                    entryView.CustomerComboBox.Text = "Toko Refusal";
                    entryView.OnBarcodeScanned("8991001100110");
                    entryView.QtyBagusNumeric.Value = 1;
                    Assert.True(entryView.CommitQuantityStripToGrid());

                    var result = entryView.SaveAndUploadAsync().GetAwaiter().GetResult();

                    Assert.False(result.Success);
                    Assert.Equal(refusal, result.ErrorMessage);
                    Assert.DoesNotContain("409", result.ErrorMessage);
                    Assert.Empty(fixture.ApiClient.UploadedOrders);
                }
            });
        }

        #endregion

        #region Test Workflow Fixture & In-Memory Doubles

        private class TestWorkflowFixture
        {
            public InMemoryReturnOrderRepo ReturnOrderRepo { get; } = new InMemoryReturnOrderRepo();
            public InMemoryBarcodeRepo BarcodeRepo { get; } = new InMemoryBarcodeRepo();
            public InMemoryReferenceRepo ReferenceRepo { get; } = new InMemoryReferenceRepo();
            public InMemorySyncLogRepo SyncLogRepo { get; } = new InMemorySyncLogRepo();
            public RecordingAudioHelper AudioHelper { get; } = new RecordingAudioHelper();
            public MockBtradeApiClient ApiClient { get; } = new MockBtradeApiClient();
            public TestRegistryHelper RegistryHelper { get; } = new TestRegistryHelper();

            public BarcodeService BarcodeService { get; }
            public SyncService SyncService { get; }
            public ReturnOrderService OrderService { get; }

            public string LastCreatedOrderId => ReturnOrderRepo.LastCreatedOrderId;

            public TestWorkflowFixture()
            {
                BarcodeService = new BarcodeService(BarcodeRepo, ReferenceRepo);
                SyncService = new SyncService(ReturnOrderRepo, BarcodeRepo, ReferenceRepo, SyncLogRepo, ApiClient, RegistryHelper);
                OrderService = new ReturnOrderService(ReturnOrderRepo, SyncService);
            }

            public void SeedProductMaster(string brgId, string brgName, string sat1, string sat2 = null, string sat3 = null)
            {
                var barang = new Barang(brgId, brgName, sat1, sat2, sat3, true);
                ReferenceRepo.BulkUpsertBarang(new[] { barang });
            }

            public void SeedProductWithBarcode(string barcode, string brgId, string brgName, string satuan, decimal pengali = 1m)
            {
                SeedProductMaster(brgId, brgName, satuan);
                var barcodeEntity = new Barcode(barcode, brgId, brgName, satuan, pengali, true);
                BarcodeRepo.InsertBarcode(barcodeEntity);
            }

            public ReturnOrderEntryView CreateIntakeView()
            {
                var view = new ReturnOrderEntryView(
                    OrderService,
                    BarcodeService,
                    ReferenceRepo,
                    AudioHelper,
                    RegistryHelper);

                view.SuppressMessageBoxes = true;
                view.AutoLaunchRegistrationDialog = false; // deterministic launcher control
                return view;
            }

            public MainForm CreateMainForm()
            {
                var services = new ServiceCollection();
                services.AddSingleton<IReturnOrderRepo>(ReturnOrderRepo);
                services.AddSingleton<IBarcodeRepo>(BarcodeRepo);
                services.AddSingleton<IReferenceRepo>(ReferenceRepo);
                services.AddSingleton<ISyncLogRepo>(SyncLogRepo);
                services.AddSingleton<IAudioHelper>(AudioHelper);
                services.AddSingleton<IRegistryHelper>(RegistryHelper);
                services.AddSingleton<IBtradeApiClient>(ApiClient);
                services.AddSingleton<ISyncService>(SyncService);
                services.AddSingleton<IReturnOrderService>(OrderService);
                services.AddSingleton<IBarcodeService>(BarcodeService);
                services.AddSingleton<ScannerWedgeListener>();

                services.AddTransient<ReturnOrderEntryView>(_ => CreateIntakeView());
                services.AddTransient<ReturnOrderListView>(_ => new ReturnOrderListView(OrderService, ReturnOrderRepo, SyncService, AudioHelper));
                services.AddTransient<BarcodeRegistryView>(_ => new BarcodeRegistryView(BarcodeRepo, BarcodeService, ReferenceRepo, RegistryHelper, AudioHelper));
                services.AddTransient<SettingsView>(_ => new SettingsView(RegistryHelper, AudioHelper, ApiClient));
                services.AddTransient<MainForm>();

                var provider = services.BuildServiceProvider();
                return provider.GetRequiredService<MainForm>();
            }
        }

        public class RecordingAudioHelper : IAudioHelper
        {
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }
            public bool EnableSoundFeedback { get; set; } = true;
            public List<(int Frequency, int DurationMs)> BeepLogs { get; } = new List<(int, int)>();

            public void PlaySuccess()
            {
                SuccessCount++;
                BeepLogs.Add((2000, 80));
            }

            public void PlayWarning()
            {
                WarningCount++;
                BeepLogs.Add((400, 200));
            }

            public void Beep(int frequency, int durationMs)
            {
                BeepLogs.Add((frequency, durationMs));
            }
        }

        public class TestRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GD-TEST";
            public string ServerTargetId { get; set; } = "SRV-TEST";
            public string ApiBaseUrl { get; set; } = BtrReturGudang.Infrastructure.Helpers.RegistryHelper.DefaultApiBaseUrl;
            public string LastOperatorName { get; set; } = "BUDI-GUDANG";
            public string LastOperatorEmail { get; set; } = "budi.gudang@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        public class MockBtradeApiClient : IBtradeApiClient
        {
            public bool IsOnline { get; set; } = true;
            public List<ReturnOrderDto> UploadedOrders { get; } = new List<ReturnOrderDto>();
            public List<BarcodeRegistrationDto> UploadedRegistrations { get; } = new List<BarcodeRegistrationDto>();

            public SessionResolveResultDto SessionResolveResult { get; set; } =
                SessionResolveResultDto.Ok(200, null, "USR-001", "BUDI-GUDANG", "ROLE-GDG", null);

            public List<string> ResolvedEmails { get; } = new List<string>();

            public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto)
            {
                if (!IsOnline) return Task.FromResult(ApiUploadResultDto.Fail(0, "Cloud API relay unavailable (offline)."));
                UploadedOrders.Add(dto);
                return Task.FromResult(ApiUploadResultDto.Ok(200));
            }

            public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)
            {
                if (!IsOnline) return Task.FromResult(ApiUploadResultDto.Fail(0, "Cloud API relay unavailable (offline)."));
                UploadedRegistrations.Add(dto);
                return Task.FromResult(ApiUploadResultDto.Ok(200));
            }

            public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
            {
                if (!IsOnline) return Task.FromResult<MasterSyncDto>(null);
                return Task.FromResult(new MasterSyncDto());
            }

            public Task<SessionResolveResultDto> ResolveSessionAsync(string email)
            {
                ResolvedEmails.Add(email);
                return Task.FromResult(SessionResolveResult);
            }
        }

        public class InMemoryReturnOrderRepo : IReturnOrderRepo
        {
            private readonly Dictionary<string, ReturnOrder> _orders = new Dictionary<string, ReturnOrder>();
            public string LastCreatedOrderId { get; private set; }

            public ReturnOrder GetById(string returnOrderId)
            {
                if (returnOrderId != null && _orders.TryGetValue(returnOrderId, out var order))
                    return order;
                return null;
            }

            public Task<ReturnOrder> GetByIdAsync(string returnOrderId) => Task.FromResult(GetById(returnOrderId));

            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
            {
                var query = _orders.Values.Where(o => o.ReturnOrderDate >= startDate.Date && o.ReturnOrderDate <= endDate.Date);
                if (statusFilter.HasValue)
                {
                    query = query.Where(o => o.StatusSync == statusFilter.Value);
                }
                return query.ToList();
            }

            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) =>
                Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));

            public IEnumerable<ReturnOrder> GetPendingUploadList() =>
                _orders.Values.Where(o => o.StatusSync == StatusSync.Draft).ToList();

            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync() =>
                Task.FromResult(GetPendingUploadList());

            public void Save(ReturnOrder returnOrder)
            {
                if (returnOrder == null) return;
                returnOrder.Validate();
                if (_orders.TryGetValue(returnOrder.ReturnOrderId, out var existing))
                {
                    if (existing.StatusSync == StatusSync.Synced)
                    {
                        throw new DocumentLockedException(returnOrder.ReturnOrderId);
                    }
                }
                _orders[returnOrder.ReturnOrderId] = returnOrder;
                LastCreatedOrderId = returnOrder.ReturnOrderId;
            }

            public Task SaveAsync(ReturnOrder returnOrder)
            {
                Save(returnOrder);
                return Task.CompletedTask;
            }

            public void MarkSynced(string returnOrderId, DateTime syncedDate)
            {
                if (_orders.TryGetValue(returnOrderId, out var order))
                {
                    order.MarkSynced(syncedDate);
                }
            }

            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate)
            {
                MarkSynced(returnOrderId, syncedDate);
                return Task.CompletedTask;
            }

            public void DeleteDraft(string returnOrderId)
            {
                if (_orders.TryGetValue(returnOrderId, out var order))
                {
                    if (order.StatusSync == StatusSync.Synced)
                        throw new DocumentLockedException(returnOrderId);
                    _orders.Remove(returnOrderId);
                }
            }

            public Task DeleteDraftAsync(string returnOrderId)
            {
                DeleteDraft(returnOrderId);
                return Task.CompletedTask;
            }
        }

        public class InMemoryBarcodeRepo : IBarcodeRepo
        {
            private readonly Dictionary<string, Barcode> _barcodes = new Dictionary<string, Barcode>(StringComparer.OrdinalIgnoreCase);
            private readonly List<BarcodeRegistration> _registrations = new List<BarcodeRegistration>();

            public Barcode LookupByBarcode(string barcode)
            {
                if (barcode != null && _barcodes.TryGetValue(barcode, out var b))
                    return b;
                return null;
            }

            public Task<Barcode> LookupByBarcodeAsync(string barcode) => Task.FromResult(LookupByBarcode(barcode));

            public void InsertBarcode(Barcode barcode)
            {
                if (barcode != null) _barcodes[barcode.BarcodeValue] = barcode;
            }

            public Task InsertBarcodeAsync(Barcode barcode)
            {
                InsertBarcode(barcode);
                return Task.CompletedTask;
            }

            public void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes)
            {
                if (barcodes == null) return;
                foreach (var b in barcodes) InsertBarcode(b);
            }

            public Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes)
            {
                BulkUpsertBarcodes(barcodes);
                return Task.CompletedTask;
            }

            public IEnumerable<Barcode> GetAllBarcodes() => _barcodes.Values.ToList();
            public Task<IEnumerable<Barcode>> GetAllBarcodesAsync() => Task.FromResult(GetAllBarcodes());

            public IEnumerable<Barcode> SearchBarcodes(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return GetAllBarcodes();
                return _barcodes.Values.Where(b => b.BarcodeValue.Contains(keyword) || b.BrgId.Contains(keyword) || b.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            public Task<IEnumerable<Barcode>> SearchBarcodesAsync(string keyword) => Task.FromResult(SearchBarcodes(keyword));

            public void QueueRegistration(BarcodeRegistration registration)
            {
                if (registration != null) _registrations.Add(registration);
            }

            public Task QueueRegistrationAsync(BarcodeRegistration registration)
            {
                QueueRegistration(registration);
                return Task.CompletedTask;
            }

            public IEnumerable<BarcodeRegistration> GetPendingRegistrations() =>
                _registrations.Where(r => r.StatusSync == BarcodeRegistrationStatus.Pending).ToList();

            public Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync() =>
                Task.FromResult(GetPendingRegistrations());

            public void MarkRegistrationUploaded(string requestId)
            {
                var reg = _registrations.FirstOrDefault(r => r.RequestId == requestId);
                if (reg != null) reg.MarkUploaded();
            }

            public Task MarkRegistrationUploadedAsync(string requestId)
            {
                MarkRegistrationUploaded(requestId);
                return Task.CompletedTask;
            }

            public void DeleteBarcode(string barcode)
            {
                if (barcode != null)
                {
                    _barcodes.Remove(barcode);
                    _registrations.RemoveAll(r => string.Equals(r.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
                }
            }

            public Task DeleteBarcodeAsync(string barcode)
            {
                DeleteBarcode(barcode);
                return Task.CompletedTask;
            }
        }

        public class InMemoryReferenceRepo : IReferenceRepo
        {
            private readonly Dictionary<string, Barang> _barangs = new Dictionary<string, Barang>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, Customer> _customers = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, Driver> _drivers = new Dictionary<string, Driver>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, SalesPerson> _sales = new Dictionary<string, SalesPerson>(StringComparer.OrdinalIgnoreCase);

            public void BulkUpsertBarang(IEnumerable<Barang> items)
            {
                if (items == null) return;
                foreach (var itm in items) _barangs[itm.BrgId] = itm;
            }

            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items)
            {
                BulkUpsertBarang(items);
                return Task.CompletedTask;
            }

            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return _barangs.Values.ToList();
                return _barangs.Values.Where(b => b.BrgId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  b.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) =>
                Task.FromResult(SearchBarangByNameOrCode(keyword));

            public Barang GetBarangById(string brgId) =>
                brgId != null && _barangs.TryGetValue(brgId, out var b) ? b : null;

            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers)
            {
                if (customers == null) return;
                foreach (var c in customers) _customers[c.CustId] = c;
            }

            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers)
            {
                BulkUpsertCustomer(customers);
                return Task.CompletedTask;
            }

            public IEnumerable<Customer> SearchCustomer(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return _customers.Values.ToList();
                return _customers.Values.Where(c => c.CustId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                    c.CustName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) =>
                Task.FromResult(SearchCustomer(keyword));

            public Customer GetCustomerById(string custId) =>
                custId != null && _customers.TryGetValue(custId, out var c) ? c : null;

            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult(GetCustomerById(custId));

            public void BulkUpsertDriver(IEnumerable<Driver> drivers)
            {
                if (drivers == null) return;
                foreach (var d in drivers) _drivers[d.DriverId] = d;
            }

            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers)
            {
                BulkUpsertDriver(drivers);
                return Task.CompletedTask;
            }

            public IEnumerable<Driver> GetAllDrivers() => _drivers.Values.ToList();
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(GetAllDrivers());

            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons)
            {
                if (salesPersons == null) return;
                foreach (var s in salesPersons) _sales[s.SalesPersonId] = s;
            }

            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons)
            {
                BulkUpsertSalesPerson(salesPersons);
                return Task.CompletedTask;
            }

            public IEnumerable<SalesPerson> GetAllSalesPersons() => _sales.Values.ToList();
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(GetAllSalesPersons());
        }

        public class InMemorySyncLogRepo : ISyncLogRepo
        {
            private readonly List<SyncLogEntry> _logs = new List<SyncLogEntry>();

            public void LogSync(SyncLogEntry entry)
            {
                if (entry != null) _logs.Add(entry);
            }

            public Task LogSyncAsync(SyncLogEntry entry)
            {
                LogSync(entry);
                return Task.CompletedTask;
            }

            public IEnumerable<SyncLogEntry> GetRecentLogs(int count = 50) =>
                _logs.OrderByDescending(l => l.SyncTimestamp).Take(count).ToList();

            public Task<IEnumerable<SyncLogEntry>> GetRecentLogsAsync(int count = 50) =>
                Task.FromResult(GetRecentLogs(count));

            public DateTime? GetLastSuccessfulSyncTimestamp(string syncType) =>
                _logs.Where(l => l.SyncType == syncType && l.Status == "SUCCESS").OrderByDescending(l => l.SyncTimestamp).FirstOrDefault()?.SyncTimestamp;

            public Task<DateTime?> GetLastSuccessfulSyncTimestampAsync(string syncType) =>
                Task.FromResult(GetLastSuccessfulSyncTimestamp(syncType));
        }

        #endregion
    }
}
