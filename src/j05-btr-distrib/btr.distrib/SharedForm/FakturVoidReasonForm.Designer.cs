using System.ComponentModel;

namespace btr.distrib.SharedForm
{
    partial class FakturVoidReasonForm
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.AlasanLabel = new System.Windows.Forms.Label();
            this.SalahInputRadio = new System.Windows.Forms.RadioButton();
            this.RevisiRadio = new System.Windows.Forms.RadioButton();
            this.CustomerRejectRadio = new System.Windows.Forms.RadioButton();
            this.NoteLabel = new System.Windows.Forms.Label();
            this.NoteText = new System.Windows.Forms.TextBox();
            this.OkButton = new System.Windows.Forms.Button();
            this.BatalButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AlasanLabel
            // 
            this.AlasanLabel.AutoSize = true;
            this.AlasanLabel.Location = new System.Drawing.Point(6, 6);
            this.AlasanLabel.Name = "AlasanLabel";
            this.AlasanLabel.Size = new System.Drawing.Size(67, 13);
            this.AlasanLabel.TabIndex = 0;
            this.AlasanLabel.Text = "Alasan Void";
            // 
            // SalahInputRadio
            // 
            this.SalahInputRadio.AutoSize = true;
            this.SalahInputRadio.Location = new System.Drawing.Point(9, 25);
            this.SalahInputRadio.Name = "SalahInputRadio";
            this.SalahInputRadio.Size = new System.Drawing.Size(82, 17);
            this.SalahInputRadio.TabIndex = 1;
            this.SalahInputRadio.TabStop = true;
            this.SalahInputRadio.Text = "Salah Input";
            this.SalahInputRadio.UseVisualStyleBackColor = true;
            // 
            // RevisiRadio
            // 
            this.RevisiRadio.AutoSize = true;
            this.RevisiRadio.Location = new System.Drawing.Point(9, 48);
            this.RevisiRadio.Name = "RevisiRadio";
            this.RevisiRadio.Size = new System.Drawing.Size(53, 17);
            this.RevisiRadio.TabIndex = 2;
            this.RevisiRadio.TabStop = true;
            this.RevisiRadio.Text = "Revisi";
            this.RevisiRadio.UseVisualStyleBackColor = true;
            // 
            // CustomerRejectRadio
            // 
            this.CustomerRejectRadio.AutoSize = true;
            this.CustomerRejectRadio.Location = new System.Drawing.Point(9, 71);
            this.CustomerRejectRadio.Name = "CustomerRejectRadio";
            this.CustomerRejectRadio.Size = new System.Drawing.Size(104, 17);
            this.CustomerRejectRadio.TabIndex = 3;
            this.CustomerRejectRadio.TabStop = true;
            this.CustomerRejectRadio.Text = "Customer Reject";
            this.CustomerRejectRadio.UseVisualStyleBackColor = true;
            // 
            // NoteLabel
            // 
            this.NoteLabel.AutoSize = true;
            this.NoteLabel.Location = new System.Drawing.Point(6, 96);
            this.NoteLabel.Name = "NoteLabel";
            this.NoteLabel.Size = new System.Drawing.Size(90, 13);
            this.NoteLabel.TabIndex = 4;
            this.NoteLabel.Text = "Catatan (opsional)";
            // 
            // NoteText
            // 
            this.NoteText.Location = new System.Drawing.Point(6, 112);
            this.NoteText.MaxLength = 200;
            this.NoteText.Name = "NoteText";
            this.NoteText.Size = new System.Drawing.Size(254, 22);
            this.NoteText.TabIndex = 5;
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(57, 145);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 6;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            // 
            // BatalButton
            // 
            this.BatalButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BatalButton.Location = new System.Drawing.Point(138, 145);
            this.BatalButton.Name = "BatalButton";
            this.BatalButton.Size = new System.Drawing.Size(75, 23);
            this.BatalButton.TabIndex = 7;
            this.BatalButton.Text = "Batal";
            this.BatalButton.UseVisualStyleBackColor = true;
            // 
            // FakturVoidReasonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.RosyBrown;
            this.ClientSize = new System.Drawing.Size(266, 176);
            this.ControlBox = false;
            this.Controls.Add(this.BatalButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.NoteText);
            this.Controls.Add(this.NoteLabel);
            this.Controls.Add(this.CustomerRejectRadio);
            this.Controls.Add(this.RevisiRadio);
            this.Controls.Add(this.SalahInputRadio);
            this.Controls.Add(this.AlasanLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FakturVoidReasonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Batal Faktur";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label AlasanLabel;
        private System.Windows.Forms.RadioButton SalahInputRadio;
        private System.Windows.Forms.RadioButton RevisiRadio;
        private System.Windows.Forms.RadioButton CustomerRejectRadio;
        private System.Windows.Forms.Label NoteLabel;
        private System.Windows.Forms.TextBox NoteText;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button BatalButton;
    }
}
