using BtrGudang.Winform.BtrGudang.Winform.Services;
using BtrGudang.Winform.Forms;
using PackingOrderDownloader;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BtrGudang.Winform
{
    public partial class MainForm : Form
    {
        private readonly IFormFactory _formFactory;

        public MainForm(IFormFactory formFactory)
        {
            InitializeComponent();

            _formFactory = formFactory;
            SetMdiClientBackColor(Color.FromArgb(30,30,30));
        }
        private void SetMdiClientBackColor(Color color)
        {
            // Loop through controls to find the MDI client
            foreach (Control control in this.Controls)
            {
                if (control is MdiClient)
                {
                    control.BackColor = color;
                    break;
                }
            }
        }
        private bool BringMdiChildToFrontIfLoaded<T>() where T : Form
        {
            var loadedForm = this.MdiChildren.OfType<T>().FirstOrDefault();
            if (loadedForm != null)
            {
                loadedForm.BringToFront();
                return true;
            }
            return false;
        }
        private void PT1DownloadPackingOrderMenu_Click(object sender, EventArgs e)
        {
            if (BringMdiChildToFrontIfLoaded<DL1DownloaderForm>())
                return;

            var form = _formFactory.CreateForm<DL1DownloaderForm>();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MdiParent = this;
            form.Show();
        }


        private void pT2InfoPackingOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BringMdiChildToFrontIfLoaded<DL2DownloadPackingOrderInfoForm>())
                return;

            var form = _formFactory.CreateForm<DL2DownloadPackingOrderInfoForm>();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MdiParent = this;
            form.Show();
        }

        private void PK1PrintPackingOrderMenu_Click(object sender, EventArgs e)
        {
            if (BringMdiChildToFrontIfLoaded<PK1PrintPackingOrderForm>())
                return;

            var form = _formFactory.CreateForm<PK1PrintPackingOrderForm>();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MdiParent = this;
            form.Show();
        }
    }
}
