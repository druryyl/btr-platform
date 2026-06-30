using System.Windows.Forms;
using btr.domain.SalesContext.FakturAgg;

namespace btr.distrib.SharedForm
{
    public partial class FakturVoidReasonForm : Form
    {
        private int _voidReasonCode;

        public int VoidReasonCode => _voidReasonCode;
        public string VoidReasonNote => NoteText.Text;

        public FakturVoidReasonForm()
        {
            InitializeComponent();
            OkButton.Enabled = false;
            OkButton.Click += OkButton_Click;
            BatalButton.Click += CancelButton_Click;
            SalahInputRadio.CheckedChanged += ReasonRadio_CheckedChanged;
            RevisiRadio.CheckedChanged += ReasonRadio_CheckedChanged;
            CustomerRejectRadio.CheckedChanged += ReasonRadio_CheckedChanged;
        }

        private void ReasonRadio_CheckedChanged(object sender, System.EventArgs e)
        {
            OkButton.Enabled = SalahInputRadio.Checked
                               || RevisiRadio.Checked
                               || CustomerRejectRadio.Checked;
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            if (SalahInputRadio.Checked)
                _voidReasonCode = (int)FakturVoidReasonEnum.SalahInput;
            else if (RevisiRadio.Checked)
                _voidReasonCode = (int)FakturVoidReasonEnum.Revisi;
            else if (CustomerRejectRadio.Checked)
                _voidReasonCode = (int)FakturVoidReasonEnum.CustomerReject;
            else
                return;

            DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
