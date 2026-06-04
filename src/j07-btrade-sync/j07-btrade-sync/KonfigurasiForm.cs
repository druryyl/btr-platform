using j07_btrade_sync.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace j07_btrade_sync
{
    public partial class KonfigurasiForm : Form
    {
        private readonly RegistryHelper _registryHelper;

        private string _serverTarget;
        private bool _downloadSalesOrder;
        private bool _syncCustomerLocation;
        private bool _downloadCheckIn;
        private bool _uploadPackingOrder;

        public KonfigurasiForm()
        {
            InitializeComponent();
            _registryHelper = new RegistryHelper();

            OKButton.Click += OKButton_Click;
            CancelButton.Click += (s, e) => this.Close();

            LoadConfig();
            DisplayConfig();
            ServerTargetIDText.Text = _registryHelper.ReadString("ServerTargetID", "JOG");
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            try
            {
                WriteConfig();
                //_registryHelper.WriteString("ServerTargetID", ServerTargetIDText.Text);
                MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadConfig()
        {
            _serverTarget = _registryHelper.ReadString("ServerTargetID", "JOG");
            _downloadSalesOrder = _registryHelper.ReadString("DownloadSalesOrder", "0") == "1";
            _syncCustomerLocation = _registryHelper.ReadString("SyncCustomerLocation", "0") == "1";
            _downloadCheckIn = _registryHelper.ReadString("DownloadCheckIn", "0") == "1";
            _uploadPackingOrder = _registryHelper.ReadString("UploadPackingOrder", "0") == "1";
        }

        private void DisplayConfig()
        {
            ServerTargetIDText.Text = _serverTarget;
            DownloadCheckInCheckBox.Checked = _downloadCheckIn;
            SyncCustomerLocCheckBox.Checked = _syncCustomerLocation;
            DownloadCheckInCheckBox.Checked = _downloadCheckIn;
            UploadPackingOrderCheckBox.Checked = _uploadPackingOrder;
        }
        private void WriteConfig()
        {
            _downloadSalesOrder = DownloadCheckInCheckBox.Checked;
            _syncCustomerLocation = SyncCustomerLocCheckBox.Checked;
            _downloadCheckIn = DownloadCheckInCheckBox.Checked;
            _uploadPackingOrder = UploadPackingOrderCheckBox.Checked;

            _registryHelper.WriteString("ServerTargetID", ServerTargetIDText.Text);
            _registryHelper.WriteString("DownloadSalesOrder", _downloadSalesOrder ? "1" : "0");
            _registryHelper.WriteString("SyncCustomerLocation", _syncCustomerLocation ? "1" : "0");
            _registryHelper.WriteString("DownloadCheckIn", _downloadCheckIn ? "1" : "0");
            _registryHelper.WriteString("UploadPackingOrder", _uploadPackingOrder ? "1" : "0");

        }
    }
}
