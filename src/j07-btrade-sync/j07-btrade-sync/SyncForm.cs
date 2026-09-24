using j07_btrade_sync.Model;
using j07_btrade_sync.Repository;
using j07_btrade_sync.Service;
using j07_btrade_sync.Shared;
using Nuna.Lib.ValidationHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace j07_btrade_sync
{
    public partial class SyncForm : Form
    {
        private readonly BrgSyncService _brgSyncService;
        private readonly CustomerUploadService _customerSyncService;
        private readonly SalesPersonSyncService _salesPersonSyncService;
        private readonly KategoriSyncService _kategoriSyncService;
        private readonly WilayahSyncService _wilayahSyncService;
        private readonly OrderIncrementalDownloadService _orderDownloadService;
        private readonly CustomerDownloadUpdatedService _customerDownloadUpdatedService;
        private readonly CustomerClearUpdateFlagService _customerClearUpdateFlagService;
        private readonly CheckInIncrementalDownloadService _checkInDownloadService;
        private readonly ReturnOrderIncrementalDownloadService _returnOrderDownloadService;
        private readonly PackingOrderUploadSvc _packingOrderUploadSvc;
        private readonly BarcodeSyncService _barcodeSyncService;
        private readonly BarcodeRegistrationRelayService _barcodeRegistrationRelayService;
        private readonly UserSyncService _userSyncService;
        private readonly DriverSyncService _driverSyncService;
        private readonly MainOfficeCommandExecutor _mainOfficeCommand;

        private readonly BrgDal _brgDal;
        private readonly CustomerDal _customerDal;
        private readonly SalesPersonDal _salesPersonDal;
        private readonly KategoriDal _kategoriDal;
        private readonly WilayahDal _wilayahDal;
        private readonly OrderDal _orderDal;
        private readonly OrderItemDal _orderItemDal;
        private readonly CheckInDal _checkInDal;
        private readonly ReturnOrderDal _returnOrderDal;
        private readonly ReturnOrderItemDal _returnOrderItemDal;
        private readonly DriverDal _driverDal;
        private readonly PackingOrderDal _packingOrderDal;
        private readonly PackingOrderItemDal _packingOrderItemDal;
        private readonly UserDal _userDal;

        private Timer timerClock = new Timer();
        private System.Timers.Timer processingTimer;
        private int processingIntervalMinutes = 5;
        private const int RANGE_PERIODE = -3;
        private readonly RegistryHelper _registryHelper;

        private bool _downloadSalesOrder;
        private bool _syncCustomerLocation;
        private bool _downloadCheckIn;
        private bool _downloadReturnOrder;
        private bool _uploadPackingOrder;

        public SyncForm()
        {
            InitializeComponent();

            _brgSyncService = new BrgSyncService();
            _customerSyncService = new CustomerUploadService();
            _salesPersonSyncService = new SalesPersonSyncService();
            _kategoriSyncService = new KategoriSyncService();
            _wilayahSyncService = new WilayahSyncService();
            _orderDownloadService = new OrderIncrementalDownloadService();
            _customerDownloadUpdatedService = new CustomerDownloadUpdatedService();
            _customerClearUpdateFlagService = new CustomerClearUpdateFlagService();
            _checkInDownloadService = new CheckInIncrementalDownloadService();
            _returnOrderDownloadService = new ReturnOrderIncrementalDownloadService();
            _packingOrderUploadSvc = new PackingOrderUploadSvc();
            _barcodeSyncService = new BarcodeSyncService();
            _barcodeRegistrationRelayService = new BarcodeRegistrationRelayService();
            _userSyncService = new UserSyncService();
            _driverSyncService = new DriverSyncService();
            _mainOfficeCommand = new MainOfficeCommandExecutor();

            _brgDal = new BrgDal();
            _customerDal = new CustomerDal();
            _salesPersonDal = new SalesPersonDal();
            _kategoriDal = new KategoriDal();
            _wilayahDal = new WilayahDal();
            _orderDal = new OrderDal();
            _orderItemDal = new OrderItemDal();
            _checkInDal = new CheckInDal();
            _returnOrderDal = new ReturnOrderDal();
            _returnOrderItemDal = new ReturnOrderItemDal();
            _driverDal = new DriverDal();
            _registryHelper = new RegistryHelper();
            _packingOrderDal = new PackingOrderDal();
            _packingOrderItemDal = new PackingOrderItemDal();
            _userDal = new UserDal();
            LoadConfig();

            InitializeTimer();
            InitializeClock();
            RegisterEventHandler();
            LogMessage("BTrade Sync started.");
            ProcessOrder(RANGE_PERIODE);
            ProcessCheckIn(RANGE_PERIODE);
            ProcessReturnOrder(RANGE_PERIODE);
            ProcessPackingOrder(RANGE_PERIODE);
            
            ShowServerTarget();

            var nextAuto = DateTime.Now.AddMinutes(processingIntervalMinutes);
            LogMessage($"Next auto download start at {nextAuto:HH:mm:ss}");

        }
        private void LoadConfig()
        {
            _downloadSalesOrder = _registryHelper.ReadString("DownloadSalesOrder", "0") == "1";
            _syncCustomerLocation = _registryHelper.ReadString("SyncCustomerLocation", "0") == "1";
            _downloadCheckIn = _registryHelper.ReadString("DownloadCheckIn", "0") == "1";
            _downloadReturnOrder = _registryHelper.ReadString("DownloadReturnOrder", "0") == "1";
            _uploadPackingOrder = _registryHelper.ReadString("UploadPackingOrder", "0") == "1";
        }
        private void ShowServerTarget()
        {
            var server = _registryHelper.ReadString("ServerTargetID");
            this.Text = $"BTrade Sync - Server ID: {server}";
        }

        private void RegisterEventHandler()
        {
            SyncBrgButton.Click += SyncBrgButton_Click;
            SyncCustomerButton.Click += SyncCustomerButton_Click;
            SyncSalesPersonBotton.Click += SyncSalesPersonButton_Click;
            SyncBarcodeButton.Click += SyncBarcodeButton_Click;

            // Create context menu
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Quick (H-3)", null, (s, e) => QuickDownloadOrderButton_Click(s,e));
            menu.Items.Add("Extended  (H-6)", null, (s, e) => ExtenderdDownloadOrderButton_Click(s, e));
            menu.Items.Add("-", null);
            menu.Items.Add("Set Server ID", null, (s, e) => SetServerId_Click(s, e));


            // Attach menu to button
            IncrementalDownloadOrderButton.ContextMenuStrip = menu;

            // Optional: Show menu on click
            IncrementalDownloadOrderButton.Click += (s, e) => {
                menu.Show(IncrementalDownloadOrderButton, new Point(0, IncrementalDownloadOrderButton.Height));
            };
        }

        private void SetServerId_Click(object s, EventArgs e)
        {
            var dialogResult = MessageBox.Show("Set Server ID akan mempengaruhi transfer data antar cabang. Lanjutkan?", "X", MessageBoxButtons.YesNoCancel);
            if ( dialogResult != DialogResult.Yes)
                return;
            var formKonfig = new KonfigurasiForm();
            formKonfig.ShowDialog();
            LoadConfig();
        }

        public void LogMessage(string message, Color? color = null)
        {
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            color = color ?? Color.Black;

            LogTextBox.SelectionStart = LogTextBox.TextLength;
            LogTextBox.SelectionLength = 0;
            LogTextBox.SelectionColor = color.Value;
            LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            LogTextBox.SelectionColor = LogTextBox.ForeColor;
            LogTextBox.ScrollToCaret();
        }

        private async void ProcessOrder(int periodeLength)
        {
            if (!_downloadSalesOrder)
                return;

            if (periodeLength > 0)
                periodeLength = periodeLength * -1;
            try
            {
                LogMessage("Starting processing cycle...");
                var today = DateTime.Now.Date;
                var startDate = today.AddDays(periodeLength);
                var periode = new Periode(startDate, today);
                var result = await _orderDownloadService.Execute(periode);
                
                if (result.Item1)
                {
                    var listOrder = result.Item3;
                    if (listOrder != null && listOrder.Any())
                    {
                        foreach (var order in listOrder)
                        {
                            var orderDb = _orderDal.GetData(order);
                            if (orderDb != null)
                                continue;

                            _orderDal.Insert(order);
                            _orderItemDal.Delete(order);
                            _orderItemDal.Insert(order.ListItems);
                            LogMessage($"Download order {order.SalesName} - {order.CustomerName} ...", Color.Blue);
                        }
                        LogMessage($"Download done");
                    }
                    else
                    {
                        LogMessage("No orders found");
                    }
                }
                else
                {
                    LogMessage($"Download failed: {result.Item2}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex.Message}", Color.Red);
            }
        }

        private async void ProcessCheckIn(int periodeLength)
        {
            if (!_downloadCheckIn)
                return;

            if (periodeLength > 0)
                periodeLength = periodeLength * -1;
            try
            {
                LogMessage("Starting check-in processing cycle...");
                var today = DateTime.Now.Date;
                var startDate = today.AddDays(periodeLength);
                var periode = new Periode(startDate, today);
                var result = await _checkInDownloadService.Execute(periode);

                if (result.Item1)
                {
                    var listCheckIn = result.Item3;
                    if (listCheckIn != null && listCheckIn.Any())
                    {
                        foreach (var checkIn in listCheckIn)
                        {
                            var checkInDb = _checkInDal.GetData(checkIn);
                            if (checkInDb != null)
                            {
                                _checkInDal.Update(checkIn);
                                LogMessage($"Updated check-in {checkIn.UserEmail} - {checkIn.CustomerName} ...", Color.Blue);
                            }
                            else
                            {
                                _checkInDal.Insert(checkIn);
                                LogMessage($"Download check-in {checkIn.UserEmail} - {checkIn.CustomerName} ...", Color.Blue);
                            }
                        }
                        LogMessage($"Check-in download done");
                    }
                    else
                    {
                        LogMessage("No check-ins found");
                    }
                }
                else
                {
                    LogMessage($"Check-in download failed: {result.Item2}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"CHECK-IN ERROR: {ex.Message}", Color.Red);
            }
        }

        private void InitializeTimer()
        {
            processingTimer = new System.Timers.Timer(processingIntervalMinutes * 60 * 1000);
            processingTimer.Elapsed += ProcessingTimer_Elapsed;
            processingTimer.AutoReset = false; // We'll manually restart after processing
            processingTimer.Start();
        }

        //  S3.7 — Return Order sync run (Arch §4.3, §8.2, §20): the Driver
        //  projection upload (S3.5 / I-RO-06), then download (S3.3 / I-RO-02)
        //  → stage (S3.2 / I-RO-07) → in-process import (S3.4 / I-RO-08),
        //  mirroring ProcessOrder/ProcessCheckIn. Gated by the
        //  DownloadReturnOrder registry flag.
        private async void ProcessReturnOrder(int periodeLength)
        {
            if (!_downloadReturnOrder)
                return;

            if (periodeLength > 0)
                periodeLength = periodeLength * -1;

            //  DRIVER PROJECTION UPLOAD (S3.5 / I-RO-06). Uploaded before the
            //  download so the device Driver reference cache (I-RO-05) is
            //  refreshed by the same run; a failure is logged and never
            //  blocks the Return Order flow.
            await ProcessDriverUpload();

            try
            {
                LogMessage("Starting return-order processing cycle...");
                var today = DateTime.Now.Date;
                var startDate = today.AddDays(periodeLength);
                var periode = new Periode(startDate, today);
                var result = await _returnOrderDownloadService.Execute(periode);

                if (!result.Item1)
                {
                    LogMessage($"Return order download failed: {result.Item2}", Color.Red);
                    return;
                }

                var listReturnOrder = result.Item3;
                if (listReturnOrder == null || !listReturnOrder.Any())
                {
                    LogMessage("No return orders found");
                    return;
                }

                //  DOWNLOAD → STAGE → IMPORT run sequentially per order. The
                //  stage is an idempotent upsert by ReturnOrderId (I-RO-07) and
                //  the import is dispatched in-process (I-RO-08): numbering
                //  (ReturnOrderNo) stays office-side (ADR-RO-002/008). A row
                //  failure is logged and the remaining rows still process; a
                //  failed import leaves the order staged, never falsely
                //  imported (INV-11).
                var userId = _registryHelper.ReadString("SyncUserId");
                foreach (var returnOrder in listReturnOrder)
                {
                    try
                    {
                        //  TD-15 — the Cloud-resolved operator UserId (SubmittedBy)
                        //  is relayed to the existing Main Office import as the
                        //  office audit identity. A legacy row without SubmittedBy
                        //  keeps the existing sync service-account identity
                        //  (registry SyncUserId) — the pre-relay behavior.
                        var auditUserId = string.IsNullOrWhiteSpace(returnOrder.SubmittedBy)
                            ? userId
                            : returnOrder.SubmittedBy;

                        var returnOrderDb = _returnOrderDal.GetData(returnOrder);
                        if (returnOrderDb != null)
                        {
                            _returnOrderDal.Update(returnOrder);
                            _returnOrderItemDal.Delete(returnOrder);
                            _returnOrderItemDal.Insert(returnOrder.ListItems);
                            LogMessage($"Updated return order {returnOrder.CustomerName} ...", Color.Blue);
                        }
                        else
                        {
                            _returnOrderDal.Insert(returnOrder, auditUserId);
                            _returnOrderItemDal.Delete(returnOrder);
                            _returnOrderItemDal.Insert(returnOrder.ListItems);
                            LogMessage($"Staged return order {returnOrder.CustomerName} ...", Color.Blue);
                        }

                        var importResult = await _mainOfficeCommand.ImportReturnOrder(returnOrder, auditUserId);
                        LogMessage($"Imported return order {importResult.ReturnOrderNo} ...", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"RETURN ORDER ERROR [{returnOrder.ReturnOrderId}]: {ex.Message}", Color.Red);
                    }
                }
                LogMessage("Return order download done");
            }
            catch (Exception ex)
            {
                LogMessage($"RETURN ORDER ERROR: {ex.Message}", Color.Red);
            }
        }

        //  S3.5 / I-RO-06 — Driver projection upload, mirroring the
        //  SalesPerson uploader shape (list from the Main Office master, POST
        //  to the Cloud; the Cloud upserts by DriverId + ServerId, so a
        //  re-upload is idempotent).
        private async Task ProcessDriverUpload()
        {
            try
            {
                LogMessage("Upload Driver started...", Color.Green);
                var listDriver = _driverDal.ListData().ToList();
                var result = await _driverSyncService.SyncDriver(listDriver);
                if (result.Item1)
                    LogMessage("Driver upload done", Color.Green);
                else
                    LogMessage($"Driver upload failed: {result.Item2}", Color.Red);
            }
            catch (Exception ex)
            {
                LogMessage($"DRIVER ERROR: {ex.Message}", Color.Red);
            }
        }

        private void InitializeClock()
        {
            var timeLabel = new ToolStripStatusLabel();
            timeLabel.BackColor = Color.WhiteSmoke;
            timeLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            timeLabel.ForeColor = Color.Gray;
            statusStrip1.Items.Add(timeLabel);

            timerClock.Interval = 1000;
            timerClock.Tick += (s, e) => {
                timeLabel.Text = $"Running Timer {DateTime.Now.ToString("HH:mm:ss")}";
            };
            timerClock.Start();
        }
        private async void ProcessingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                //LogMessage($"Timer triggered - Starting processing at {DateTime.Now}");
            }));

            try
            {
                await Task.Run(() => ProcessOrder(RANGE_PERIODE)); // Run processing on background thread
                await Task.Run(() => ProcessCustomer());
                await Task.Run(() => ProcessCheckIn(RANGE_PERIODE));
                await Task.Run(() => ProcessReturnOrder(RANGE_PERIODE));
                await Task.Run(() => ProcessPackingOrder(RANGE_PERIODE));
            }
            catch (Exception ex)
            {
                this.BeginInvoke((Action)(() =>
                {
                    LogMessage($"Processing error: {ex.Message}", Color.Red);
                }));
            }
            finally
            {
                processingTimer.Start(); // Restart the timer
                var nextAuto = DateTime.Now.AddMinutes(processingIntervalMinutes);
                LogMessage($"Next auto download start at {nextAuto:HH:mm:ss}");
            }
        }

        private void QuickDownloadOrderButton_Click(object sender, EventArgs e)
        {
            LogMessage($"Quick Download triggered", Color.Green);
            ProcessOrder(-3);
            ProcessCustomer();
            ProcessCheckIn(-3);
            ProcessReturnOrder(-3);
            ProcessPackingOrder(-3);
        }
        private void ExtenderdDownloadOrderButton_Click(object sender, EventArgs e)
        {
            LogMessage($"Extended Download triggered", Color.Green);
            ProcessOrder(-6);
            ProcessCustomer();
            ProcessCheckIn(-6);
            ProcessReturnOrder(-6);
            ProcessPackingOrder(-6);
        }

        private async void ProcessCustomer()
        {
            if (!_syncCustomerLocation)
                return;
            try
            {
                LogMessage("Start processing customer location...");
                var result = await _customerDownloadUpdatedService.Execute();

                if (result.Item1)
                {
                    var listCustomer = result.Item3;
                    if (listCustomer != null && listCustomer.Any())
                    {
                        foreach (var cust in listCustomer)
                        {
                            var custDb = _customerDal.GetData(cust.CustomerId);
                            if (custDb is null)
                                continue;
                            _customerDal.UpdateLocation(cust);
                            LogMessage($"Download location {cust.CustomerName} [{cust.Latitude:N5}, {cust.Longitude:N5}] ...", Color.Blue);
                        }
                        LogMessage($"Download done");
                        await _customerClearUpdateFlagService.Execute();
                    }
                    else
                    {
                        LogMessage("No new location found");
                    }
                }
                else
                {
                    LogMessage($"Download failed: {result.Item2}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex.Message}", Color.Red);
            }
        }
        private void SyncCustomerButton_Click(object sender, EventArgs e)
        {
            if (!_syncCustomerLocation)
                return;

            ProcessCustomer();
            LogMessage("Upload Customer started...", Color.Green);
            var listCustomer = _customerDal.ListData().ToList();
            var result = _customerSyncService.UploadCustomer(listCustomer);
            result.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    LogMessage("Done");
                }
                else
                {
                    LogMessage($"Upload Customer failed: {task.Result.Item2}", Color.Red);
                }
            });
        }

        private void SyncBrgButton_Click(object sender, EventArgs e)
        {
            LogMessage("Sync Barang started...", Color.Green);
            var listBrg = _brgDal.ListData().ToList();
            var result = _brgSyncService.SyncBrg(listBrg);
            result.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    LogMessage("Done");
                }
                else
                {
                    LogMessage($"Sync failed: {task.Result.Item2}", Color.Red);
                    return;
                }
            });

            var listKategori = _kategoriDal.ListData().ToList();
            var kategoriResult = _kategoriSyncService.SyncKategori(listKategori);
            kategoriResult.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    //MessageBox.Show("Brg-Kategori sync successful!");
                }
                else
                {
                    //MessageBox.Show($"Kategori sync failed: {task.Result.Item2}");
                }
            });
        }
        private void SyncSalesPersonButton_Click(object sender, EventArgs e)
        {
            LogMessage("Sync Sales started...", Color.Green);
            var listSalesPerson = _salesPersonDal.ListData().ToList();
            var result = _salesPersonSyncService.SyncSalesPerson(listSalesPerson);
            result.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    LogMessage("Done");
                }
                else
                {
                    LogMessage($"Sync failed: {task.Result.Item2}", Color.Red);
                }
            });

            var listWilayah = _wilayahDal.ListData().ToList();
            var wilayahResult = _wilayahSyncService.SyncWilayah(listWilayah);
            wilayahResult.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    //MessageBox.Show("SalesPerson-Wilayah sync successful!");
                }
                else
                {
                    //MessageBox.Show($"Wilayah sync failed: {task.Result.Item2}");
                }
            });
        }
        private async void SyncBarcodeButton_Click(object sender, EventArgs e)
        {
            LogMessage("Sync Barcode Registry started...", Color.Green);

            //  ONE SYNCHRONIZATION RUN (8.1 / 8.2), executed in a fixed order:
            //  user projection (IR-05) -> registration relay (8.2) ->
            //  barcode publish (8.1). Relay runs before publish so that newly
            //  accepted registrations are published by the following publish.
            //  A failing step is surfaced and does not abort the run; watermark
            //  and ack state only advance inside the step that committed.

            //  1. USER PROJECTION (IR-05 / I-08)
            var userResult = await _userSyncService.SyncUser(_userDal.ListData().ToList());
            if (userResult.Item1)
                LogMessage("User projection done", Color.Green);
            else
                LogMessage($"User projection failed: {userResult.Item2}", Color.Red);

            //  2. REGISTRATION RELAY (8.2 / I-02, I-03)
            var relayResult = await _barcodeRegistrationRelayService.SyncRegistration();
            if (relayResult.Item1)
                LogMessage("Registration relay done", Color.Green);
            else
                LogMessage($"Registration relay failed: {relayResult.Item2}", Color.Red);

            //  3. BARCODE PUBLISH (8.1 / I-01)
            var publishResult = await _barcodeSyncService.SyncBarcode();
            if (publishResult.Item1)
                LogMessage("Barcode publish done", Color.Green);
            else
                LogMessage($"Barcode publish failed: {publishResult.Item2}", Color.Red);
        }

        private void ProcessPackingOrder(int periodeLength)
        {
            if (!_uploadPackingOrder)
                return;
            
            LogMessage("Upload PackingOrder started...", Color.Green);
            var listPacking = CreateListPackingOrder(periodeLength);
            var result = _packingOrderUploadSvc.UploadPackingOrder(listPacking);
            result.ContinueWith(task =>
            {
                if (task.Result.Item1)
                {
                    if (task.Result.Item2 != "")
                        LogMessage($"Uploading Packing Order...\r{task.Result.Item2}", Color.Green);
                    LogMessage("Done", Color.Green);
                }
                else
                {
                    LogMessage($"Sync failed: {task.Result.Item2}", Color.Red);
                }
            });
        }

        private IEnumerable<PackingOrderModel> CreateListPackingOrder(int periodeLength)
        {
            var startDate = DateTime.Now.Date.AddDays(periodeLength);
            var endDate = DateTime.Now;
            var periode = new Periode(startDate, endDate);
            var listHdr = _packingOrderDal.ListData(periode)?.ToList() ?? new List<PackingOrderModel>();
            listHdr.RemoveAll(x => x.UploadTimestamp != new DateTime(3000, 1, 1));
            foreach(var hdr in listHdr)
            {
                var listItem = _packingOrderItemDal.ListData(hdr)?.ToList();
                hdr.SetListItem(listItem.Select(x => x.ToModel()));
            }
            return listHdr;
        }
    }
}
