using System.Drawing;
using System.Windows.Forms;

namespace BtrReturGudang.Winform.Controls
{
    /// <summary>
    /// Double-buffered DataGridView implementation preventing screen flickering during rapid intake updates.
    /// Traceable to: ARCHITECTURE §10 constraint 7, TD-011.
    /// </summary>
    public class DoubleBufferedDataGridView : DataGridView
    {
        public new bool DoubleBuffered
        {
            get => base.DoubleBuffered;
            set => base.DoubleBuffered = value;
        }

        public DoubleBufferedDataGridView()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.EnableNotifyMessage, true);

            BackgroundColor = Color.White;
            BorderStyle = BorderStyle.None;
            GridColor = Color.FromArgb(226, 232, 240); // Slate-200
            RowHeadersVisible = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            AutoGenerateColumns = false;
            EnableHeadersVisualStyles = false;

            ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249); // Slate-100
            ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);   // Slate-700
            ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            ColumnHeadersHeight = 36;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255); // Indigo-100
            DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            RowTemplate.Height = 34;
        }
    }
}
