namespace btr.distrib.SalesContext.SalesPersonAgg
{
    partial class SalesOmzetMaterializeForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.Tgl1Date = new System.Windows.Forms.DateTimePicker();
            this.Tgl2Date = new System.Windows.Forms.DateTimePicker();
            this.ProsesButton = new System.Windows.Forms.Button();
            this.FullRebuildButton = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // Tgl1Date
            // 
            this.Tgl1Date.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Tgl1Date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl1Date.Location = new System.Drawing.Point(12, 32);
            this.Tgl1Date.Name = "Tgl1Date";
            this.Tgl1Date.Size = new System.Drawing.Size(156, 22);
            this.Tgl1Date.TabIndex = 0;
            // 
            // Tgl2Date
            // 
            this.Tgl2Date.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Tgl2Date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl2Date.Location = new System.Drawing.Point(174, 32);
            this.Tgl2Date.Name = "Tgl2Date";
            this.Tgl2Date.Size = new System.Drawing.Size(156, 22);
            this.Tgl2Date.TabIndex = 1;
            // 
            // ProsesButton
            // 
            this.ProsesButton.Location = new System.Drawing.Point(336, 31);
            this.ProsesButton.Name = "ProsesButton";
            this.ProsesButton.Size = new System.Drawing.Size(90, 23);
            this.ProsesButton.TabIndex = 2;
            this.ProsesButton.Text = "Materialisasi";
            this.ProsesButton.UseVisualStyleBackColor = true;
            // 
            // FullRebuildButton
            // 
            this.FullRebuildButton.Location = new System.Drawing.Point(432, 31);
            this.FullRebuildButton.Name = "FullRebuildButton";
            this.FullRebuildButton.Size = new System.Drawing.Size(100, 23);
            this.FullRebuildButton.TabIndex = 3;
            this.FullRebuildButton.Text = "Rebuild Semua";
            this.FullRebuildButton.UseVisualStyleBackColor = true;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.AutoEllipsis = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 68);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(520, 16);
            this.StatusLabel.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(285, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Perbarui agregat BTR_SalesOmzet untuk periode terpilih";
            // 
            // ProgressBar
            // 
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(12, 90);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(520, 18);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.TabIndex = 6;
            this.ProgressBar.Visible = false;
            // 
            // SalesOmzetMaterializeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 120);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.FullRebuildButton);
            this.Controls.Add(this.ProsesButton);
            this.Controls.Add(this.Tgl2Date);
            this.Controls.Add(this.Tgl1Date);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SalesOmzetMaterializeForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Materialisasi Omzet Sales";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DateTimePicker Tgl1Date;
        private System.Windows.Forms.DateTimePicker Tgl2Date;
        private System.Windows.Forms.Button ProsesButton;
        private System.Windows.Forms.Button FullRebuildButton;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar ProgressBar;
    }
}
