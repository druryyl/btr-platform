namespace btr.distrib.SalesContext.SalesPersonAgg
{
    partial class SalesOmzetHealthWeeklyForm
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
            this.YearLabel = new System.Windows.Forms.Label();
            this.YearNumeric = new System.Windows.Forms.NumericUpDown();
            this.WeekLabel = new System.Windows.Forms.Label();
            this.WeekNumeric = new System.Windows.Forms.NumericUpDown();
            this.WeekRangeLabel = new System.Windows.Forms.Label();
            this.ProsesButton = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.YearNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WeekNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // YearLabel
            // 
            this.YearLabel.AutoSize = true;
            this.YearLabel.Location = new System.Drawing.Point(12, 15);
            this.YearLabel.Name = "YearLabel";
            this.YearLabel.Size = new System.Drawing.Size(31, 13);
            this.YearLabel.TabIndex = 0;
            this.YearLabel.Text = "Tahun";
            // 
            // YearNumeric
            // 
            this.YearNumeric.Location = new System.Drawing.Point(56, 12);
            this.YearNumeric.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            this.YearNumeric.Minimum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.YearNumeric.Name = "YearNumeric";
            this.YearNumeric.Size = new System.Drawing.Size(70, 22);
            this.YearNumeric.TabIndex = 1;
            this.YearNumeric.Value = new decimal(new int[] { 2026, 0, 0, 0 });
            // 
            // WeekLabel
            // 
            this.WeekLabel.AutoSize = true;
            this.WeekLabel.Location = new System.Drawing.Point(140, 15);
            this.WeekLabel.Name = "WeekLabel";
            this.WeekLabel.Size = new System.Drawing.Size(68, 13);
            this.WeekLabel.TabIndex = 2;
            this.WeekLabel.Text = "Minggu (ISO)";
            // 
            // WeekNumeric
            // 
            this.WeekNumeric.Location = new System.Drawing.Point(214, 12);
            this.WeekNumeric.Maximum = new decimal(new int[] { 53, 0, 0, 0 });
            this.WeekNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.WeekNumeric.Name = "WeekNumeric";
            this.WeekNumeric.Size = new System.Drawing.Size(50, 22);
            this.WeekNumeric.TabIndex = 3;
            this.WeekNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // WeekRangeLabel
            // 
            this.WeekRangeLabel.AutoSize = true;
            this.WeekRangeLabel.Location = new System.Drawing.Point(12, 42);
            this.WeekRangeLabel.Name = "WeekRangeLabel";
            this.WeekRangeLabel.Size = new System.Drawing.Size(10, 13);
            this.WeekRangeLabel.TabIndex = 4;
            this.WeekRangeLabel.Text = "-";
            // 
            // ProsesButton
            // 
            this.ProsesButton.Location = new System.Drawing.Point(280, 10);
            this.ProsesButton.Name = "ProsesButton";
            this.ProsesButton.Size = new System.Drawing.Size(75, 23);
            this.ProsesButton.TabIndex = 5;
            this.ProsesButton.Text = "Proses";
            this.ProsesButton.UseVisualStyleBackColor = true;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.AutoEllipsis = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 64);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(343, 32);
            this.StatusLabel.TabIndex = 6;
            // 
            // SalesOmzetHealthWeeklyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 108);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.ProsesButton);
            this.Controls.Add(this.WeekRangeLabel);
            this.Controls.Add(this.WeekNumeric);
            this.Controls.Add(this.WeekLabel);
            this.Controls.Add(this.YearNumeric);
            this.Controls.Add(this.YearLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SalesOmzetHealthWeeklyForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hitung Indikator Mingguan";
            ((System.ComponentModel.ISupportInitialize)(this.YearNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WeekNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label YearLabel;
        private System.Windows.Forms.NumericUpDown YearNumeric;
        private System.Windows.Forms.Label WeekLabel;
        private System.Windows.Forms.NumericUpDown WeekNumeric;
        private System.Windows.Forms.Label WeekRangeLabel;
        private System.Windows.Forms.Button ProsesButton;
        private System.Windows.Forms.Label StatusLabel;
    }
}
