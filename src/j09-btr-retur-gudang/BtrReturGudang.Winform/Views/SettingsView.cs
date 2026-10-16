using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Infrastructure.Helpers;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// SCR-07: Workstation Settings &amp; Hardware Configuration view.
    /// Manages Depo/Warehouse code assignment, PC machine identification, shift operator switcher,
    /// local SQL Server connection tester, cloud API relay ping test, and audio feedback test/toggle.
    /// Traceable to: ARCHITECTURE §5.2 SCR-07, TD-009, TD-011, P3-S10.
    /// </summary>
    public class SettingsView : UserControl
    {
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;
        private readonly IBtradeApiClient _apiClient;

        // Header controls
        private Panel _pnlHeader;
        private Label _lblTitle;
        private Label _lblStatus;
        private Button _btnSaveAll;

        // Workstation identity controls
        private TextBox _txtWarehouseCode;
        private TextBox _txtServerTargetId;
        private TextBox _txtPcName;
        private Label _lblOperatorDisplay;
        private Button _btnSwitchOperator;

        // Database connection controls
        private TextBox _txtConnString;
        private Button _btnTestDb;
        private Button _btnResetDb;
        private Label _lblDbStatus;

        // Cloud API relay controls
        private TextBox _txtApiUrl;
        private Button _btnTestApi;
        private Label _lblApiStatus;

        // Audio controls
        private CheckBox _chkEnableAudio;
        private Button _btnTestSuccessBeep;
        private Button _btnTestWarningBeep;

        // Public test accessors
        public TextBox WarehouseCodeBox => _txtWarehouseCode;
        public TextBox ServerTargetIdBox => _txtServerTargetId;
        public TextBox PcNameBox => _txtPcName;
        public Label OperatorLabel => _lblOperatorDisplay;
        public Button SwitchOperatorButton => _btnSwitchOperator;
        public TextBox ConnStringBox => _txtConnString;
        public Button TestDbButton => _btnTestDb;
        public Label DbStatusLabel => _lblDbStatus;
        public TextBox ApiUrlBox => _txtApiUrl;
        public Button TestApiButton => _btnTestApi;
        public Label ApiStatusLabel => _lblApiStatus;
        public CheckBox EnableAudioCheckBox => _chkEnableAudio;
        public Button TestSuccessBeepButton => _btnTestSuccessBeep;
        public Button TestWarningBeepButton => _btnTestWarningBeep;
        public Button SaveAllButton => _btnSaveAll;

        public SettingsView()
            : this(null, null, null)
        {
        }

        public SettingsView(
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null,
            IBtradeApiClient apiClient = null)
        {
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;
            _apiClient = apiClient;

            InitializeComponent();
            LoadCurrentConfiguration();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;

            // 1. Header
            BuildHeader();

            // 2. Scrollable Body
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24, 16, 24, 24)
            };

            // Container for cards
            var pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Width = 980
            };

            // Card 1: Identitas Workstation
            pnlCards.Controls.Add(BuildIdentityCard());

            // Card 2: Koneksi Database Lokal
            pnlCards.Controls.Add(BuildDatabaseCard());

            // Card 3: Cloud API Relay
            pnlCards.Controls.Add(BuildCloudApiCard());

            // Card 4: Audio Feedback
            pnlCards.Controls.Add(BuildAudioCard());

            pnlBody.Controls.Add(pnlCards);

            Controls.Add(pnlBody);
            Controls.Add(_pnlHeader);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = UiTheme.White,
                Padding = new Padding(24, 12, 24, 12)
            };

            _lblTitle = new Label
            {
                Text = "Pengaturan Workstation [F5]",
                Font = UiTheme.FontPageTitle,
                ForeColor = UiTheme.Slate900,
                AutoSize = true,
                Location = new Point(24, 10)
            };

            _lblStatus = new Label
            {
                Text = "Konfigurasi Depo/Warehouse, koneksi database btrgd, pengujian relay API, dan umpan balik audio scanner.",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate500,
                AutoSize = true,
                Location = new Point(26, 38)
            };

            _btnSaveAll = new Button
            {
                Text = "💾 Simpan Pengaturan",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 36,
                Location = new Point(Width - 205, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            _btnSaveAll.FlatAppearance.BorderSize = 0;
            _btnSaveAll.Click += OnSaveAllClick;

            _pnlHeader.Controls.Add(_lblTitle);
            _pnlHeader.Controls.Add(_lblStatus);
            _pnlHeader.Controls.Add(_btnSaveAll);
        }

        private Control BuildIdentityCard()
        {
            var pnl = CreateCardPanel("🏢 Identitas Workstation & Shift Operator", 185);

            // Depo / Warehouse Code
            var lblWh = new Label { Text = "Kode Depo / Warehouse:", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(24, 48), AutoSize = true };
            _txtWarehouseCode = new TextBox { Location = new Point(210, 45), Width = 170, Font = UiTheme.FontBody };

            // Server Target ID
            var lblServer = new Label { Text = "Server Target ID:", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(410, 48), AutoSize = true };
            _txtServerTargetId = new TextBox { Location = new Point(540, 45), Width = 170, Font = UiTheme.FontBody };

            // PC Machine Name
            var lblPc = new Label { Text = "Nama Komputer (PC):", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(24, 88), AutoSize = true };
            _txtPcName = new TextBox
            {
                Text = Environment.MachineName,
                ReadOnly = true,
                BackColor = UiTheme.Slate100,
                Font = UiTheme.FontMonospace,
                Location = new Point(210, 85),
                Width = 260
            };

            // Operator
            var lblOp = new Label { Text = "Operator Shift Aktif:", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(24, 128), AutoSize = true };
            _lblOperatorDisplay = new Label
            {
                Text = "Operator-1",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Slate900,
                Location = new Point(210, 128),
                AutoSize = true
            };

            _btnSwitchOperator = new Button
            {
                Text = "Ganti Operator...",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Slate100,
                ForeColor = UiTheme.Slate800,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(370, 123),
                Width = 130,
                Height = 30,
                Cursor = Cursors.Hand
            };
            _btnSwitchOperator.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnSwitchOperator.Click += OnSwitchOperatorClick;

            pnl.Controls.Add(lblWh);
            pnl.Controls.Add(_txtWarehouseCode);
            pnl.Controls.Add(lblServer);
            pnl.Controls.Add(_txtServerTargetId);
            pnl.Controls.Add(lblPc);
            pnl.Controls.Add(_txtPcName);
            pnl.Controls.Add(lblOp);
            pnl.Controls.Add(_lblOperatorDisplay);
            pnl.Controls.Add(_btnSwitchOperator);

            return pnl;
        }

        private Control BuildDatabaseCard()
        {
            var pnl = CreateCardPanel("🗄️ Koneksi Database Lokal (SQL Server - btrgd)", 185);

            var lblConn = new Label { Text = "Connection String btrgd:", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(24, 48), AutoSize = true };
            _txtConnString = new TextBox
            {
                Location = new Point(24, 72),
                Width = 780,
                Font = UiTheme.FontMonospace
            };

            _btnTestDb = new Button
            {
                Text = "🔌 Tes Koneksi Local DB",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 116),
                Width = 185,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnTestDb.FlatAppearance.BorderSize = 0;
            _btnTestDb.Click += async (s, e) => await OnTestDbClickAsync();

            _btnResetDb = new Button
            {
                Text = "Reset Default",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Slate100,
                ForeColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(218, 116),
                Width = 115,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnResetDb.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnResetDb.Click += (s, e) =>
            {
                _txtConnString.Text = ConnStringHelper.DefaultConnectionString;
                _lblDbStatus.Text = "Status: Dikembalikan ke default (belum diuji).";
                _lblDbStatus.ForeColor = UiTheme.Slate500;
            };

            _lblDbStatus = new Label
            {
                Text = "Status: Belum diuji.",
                Font = UiTheme.FontLabel,
                Location = new Point(345, 124),
                AutoSize = true,
                ForeColor = UiTheme.Slate500
            };

            pnl.Controls.Add(lblConn);
            pnl.Controls.Add(_txtConnString);
            pnl.Controls.Add(_btnTestDb);
            pnl.Controls.Add(_btnResetDb);
            pnl.Controls.Add(_lblDbStatus);

            return pnl;
        }

        private Control BuildCloudApiCard()
        {
            var pnl = CreateCardPanel("☁️ Konfigurasi Cloud API Relay (j06-pkl-btrade-api)", 150);

            var lblUrl = new Label { Text = "Base URL Staging Relay:", Font = UiTheme.FontLabel, ForeColor = UiTheme.Slate700, Location = new Point(24, 48), AutoSize = true };
            _txtApiUrl = new TextBox
            {
                Text = ConfigurationManager.AppSettings["BtradeApiBaseUrl"] ?? "http://localhost:5000",
                Font = UiTheme.FontBody,
                Location = new Point(210, 45),
                Width = 380
            };

            _btnTestApi = new Button
            {
                Text = "🌐 Ping / Tes API Relay",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 90),
                Width = 185,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnTestApi.FlatAppearance.BorderSize = 0;
            _btnTestApi.Click += async (s, e) => await OnTestApiClickAsync();

            _lblApiStatus = new Label
            {
                Text = "Status: Belum diuji.",
                Font = UiTheme.FontLabel,
                Location = new Point(220, 98),
                AutoSize = true,
                ForeColor = UiTheme.Slate500
            };

            pnl.Controls.Add(lblUrl);
            pnl.Controls.Add(_txtApiUrl);
            pnl.Controls.Add(_btnTestApi);
            pnl.Controls.Add(_lblApiStatus);

            return pnl;
        }

        private Control BuildAudioCard()
        {
            var pnl = CreateCardPanel("🔊 Umpan Balik Audio (Scanner Sound Feedback)", 150);

            _chkEnableAudio = new CheckBox
            {
                Text = "Aktifkan Umpan Balik Suara Scanner & Notifikasi (EnableSoundFeedback)",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Slate900,
                Checked = true,
                Location = new Point(24, 46),
                AutoSize = true
            };
            _chkEnableAudio.CheckedChanged += OnEnableAudioChanged;

            _btnTestSuccessBeep = new Button
            {
                Text = "🔔 Tes Beep Sukses (2000Hz, 80ms)",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.EmeraldLight,
                ForeColor = UiTheme.EmeraldText,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 88),
                Width = 250,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnTestSuccessBeep.FlatAppearance.BorderColor = UiTheme.EmeraldBorder;
            _btnTestSuccessBeep.Click += (s, e) => _audioHelper?.PlaySuccess();

            _btnTestWarningBeep = new Button
            {
                Text = "⚠️ Tes Beep Peringatan (400Hz, 200ms)",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.AmberLight,
                ForeColor = UiTheme.AmberText,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(284, 88),
                Width = 260,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnTestWarningBeep.FlatAppearance.BorderColor = UiTheme.AmberBorder;
            _btnTestWarningBeep.Click += (s, e) => _audioHelper?.PlayWarning();

            pnl.Controls.Add(_chkEnableAudio);
            pnl.Controls.Add(_btnTestSuccessBeep);
            pnl.Controls.Add(_btnTestWarningBeep);

            return pnl;
        }

        private Panel CreateCardPanel(string title, int height)
        {
            var pnl = new Panel
            {
                Width = 830,
                Height = height,
                BackColor = UiTheme.White,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(20)
            };

            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate200, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

            var lblCardTitle = new Label
            {
                Text = title,
                Font = UiTheme.FontSectionTitle,
                ForeColor = UiTheme.Slate900,
                Location = new Point(20, 14),
                AutoSize = true
            };

            pnl.Controls.Add(lblCardTitle);
            return pnl;
        }

        private void LoadCurrentConfiguration()
        {
            if (_registryHelper != null)
            {
                _txtWarehouseCode.Text = _registryHelper.WarehouseCode ?? "GDG-PST";
                _txtServerTargetId.Text = _registryHelper.ServerTargetId ?? string.Empty;
                _lblOperatorDisplay.Text = !string.IsNullOrWhiteSpace(_registryHelper.LastOperatorName)
                    ? _registryHelper.LastOperatorName
                    : "Operator-1";
                _chkEnableAudio.Checked = _registryHelper.EnableSoundFeedback;
            }
            else
            {
                _txtWarehouseCode.Text = "GDG-PST";
                _txtServerTargetId.Text = string.Empty;
                _lblOperatorDisplay.Text = "Operator-1";
                _chkEnableAudio.Checked = true;
            }

            _txtConnString.Text = ConnStringHelper.GetConnectionString();
        }

        private void OnSwitchOperatorClick(object sender, EventArgs e)
        {
            var helper = _registryHelper ?? new RegistryHelper();
            using (var dlg = new OperatorSwitcherDialog(helper))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _lblOperatorDisplay.Text = dlg.SelectedOperatorName;
                }
            }
        }

        private async Task OnTestDbClickAsync()
        {
            string connStr = _txtConnString.Text?.Trim();
            if (string.IsNullOrWhiteSpace(connStr))
            {
                _lblDbStatus.Text = "❌ Connection string tidak boleh kosong.";
                _lblDbStatus.ForeColor = Color.FromArgb(185, 28, 28);
                return;
            }

            _btnTestDb.Enabled = false;
            _lblDbStatus.Text = "Menguji koneksi database...";
            _lblDbStatus.ForeColor = Color.FromArgb(100, 116, 139);

            try
            {
                bool success = await Task.Run(() =>
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT 1";
                            cmd.ExecuteScalar();
                        }
                    }
                    return true;
                });

                if (success)
                {
                    _lblDbStatus.Text = "✅ Koneksi database lokal BERHASIL! (btrgd terhubung)";
                    _lblDbStatus.ForeColor = Color.FromArgb(4, 120, 87);
                    _audioHelper?.PlaySuccess();
                }
            }
            catch (Exception ex)
            {
                _lblDbStatus.Text = $"❌ Gagal: {ex.Message}";
                _lblDbStatus.ForeColor = Color.FromArgb(185, 28, 28);
                _audioHelper?.PlayWarning();
            }
            finally
            {
                _btnTestDb.Enabled = true;
            }
        }

        private async Task OnTestApiClickAsync()
        {
            string url = _txtApiUrl.Text?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                _lblApiStatus.Text = "❌ Base URL relay tidak boleh kosong.";
                _lblApiStatus.ForeColor = Color.FromArgb(185, 28, 28);
                return;
            }

            _btnTestApi.Enabled = false;
            _lblApiStatus.Text = "Menguji route HTTP API...";
            _lblApiStatus.ForeColor = Color.FromArgb(100, 116, 139);

            try
            {
                bool connected = await Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        http.Timeout = TimeSpan.FromSeconds(5);
                        var response = await http.GetAsync(url).ConfigureAwait(false);
                        return response.IsSuccessStatusCode || ((int)response.StatusCode < 500);
                    }
                });

                if (connected)
                {
                    _lblApiStatus.Text = "✅ Cloud API relay terhubung!";
                    _lblApiStatus.ForeColor = Color.FromArgb(4, 120, 87);
                    _audioHelper?.PlaySuccess();
                }
                else
                {
                    _lblApiStatus.Text = "⚠️ Respons relay non-OK dari server.";
                    _lblApiStatus.ForeColor = Color.FromArgb(180, 83, 9);
                    _audioHelper?.PlayWarning();
                }
            }
            catch (Exception ex)
            {
                _lblApiStatus.Text = $"❌ Gagal koneksi relay: {ex.Message}";
                _lblApiStatus.ForeColor = Color.FromArgb(185, 28, 28);
                _audioHelper?.PlayWarning();
            }
            finally
            {
                _btnTestApi.Enabled = true;
            }
        }

        private void OnEnableAudioChanged(object sender, EventArgs e)
        {
            bool enabled = _chkEnableAudio.Checked;
            if (_registryHelper != null)
            {
                _registryHelper.EnableSoundFeedback = enabled;
            }
            if (_audioHelper != null)
            {
                _audioHelper.EnableSoundFeedback = enabled;
            }
        }

        public bool SuppressMessageBox { get; set; } = false;
        public string LastMessage { get; private set; }

        private void OnSaveAllClick(object sender, EventArgs e)
        {
            try
            {
                string wh = _txtWarehouseCode.Text?.Trim() ?? string.Empty;
                string serverId = _txtServerTargetId.Text?.Trim() ?? string.Empty;
                string connStr = _txtConnString.Text?.Trim() ?? string.Empty;
                bool audio = _chkEnableAudio.Checked;

                if (_registryHelper != null)
                {
                    _registryHelper.WarehouseCode = wh;
                    _registryHelper.ServerTargetId = serverId;
                    _registryHelper.EnableSoundFeedback = audio;
                }

                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    ConnStringHelper.SetConnectionString(connStr);
                }

                _audioHelper?.PlaySuccess();
                LastMessage = "Semua konfigurasi workstation berhasil disimpan.";

                if (!SuppressMessageBox)
                {
                    MessageBox.Show(
                        LastMessage,
                        "Pengaturan Tersimpan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                LastMessage = $"Gagal menyimpan pengaturan: {ex.Message}";

                if (!SuppressMessageBox)
                {
                    MessageBox.Show(
                        LastMessage,
                        "Error Simpan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
