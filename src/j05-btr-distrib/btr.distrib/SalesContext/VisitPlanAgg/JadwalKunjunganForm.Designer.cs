namespace btr.distrib.SalesContext.VisitPlanAgg
{
    partial class JadwalKunjunganForm
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
            this.TopPanel = new System.Windows.Forms.Panel();
            this.CycleLabel = new System.Windows.Forms.Label();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.VisitDatePicker = new System.Windows.Forms.DateTimePicker();
            this.VisitDateLabel = new System.Windows.Forms.Label();
            this.SalesComboBox = new System.Windows.Forms.ComboBox();
            this.SalesLabel = new System.Windows.Forms.Label();
            this.MainSplit = new System.Windows.Forms.SplitContainer();
            this.LeftSplit = new System.Windows.Forms.SplitContainer();
            this.BasePlanGrid = new System.Windows.Forms.DataGridView();
            this.EffectivePlanGrid = new System.Windows.Forms.DataGridView();
            this.RightPanel = new System.Windows.Forms.Panel();
            this.DeleteExceptionButton = new System.Windows.Forms.Button();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.ExceptionGrid = new System.Windows.Forms.DataGridView();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplit)).BeginInit();
            this.MainSplit.Panel1.SuspendLayout();
            this.MainSplit.Panel2.SuspendLayout();
            this.MainSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftSplit)).BeginInit();
            this.LeftSplit.Panel1.SuspendLayout();
            this.LeftSplit.Panel2.SuspendLayout();
            this.LeftSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BasePlanGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EffectivePlanGrid)).BeginInit();
            this.RightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExceptionGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            this.TopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TopPanel.Controls.Add(this.CycleLabel);
            this.TopPanel.Controls.Add(this.RefreshButton);
            this.TopPanel.Controls.Add(this.VisitDatePicker);
            this.TopPanel.Controls.Add(this.VisitDateLabel);
            this.TopPanel.Controls.Add(this.SalesComboBox);
            this.TopPanel.Controls.Add(this.SalesLabel);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(984, 48);
            this.TopPanel.TabIndex = 0;
            // 
            // CycleLabel
            // 
            this.CycleLabel.AutoSize = true;
            this.CycleLabel.Location = new System.Drawing.Point(560, 16);
            this.CycleLabel.Name = "CycleLabel";
            this.CycleLabel.Size = new System.Drawing.Size(12, 16);
            this.CycleLabel.TabIndex = 5;
            this.CycleLabel.Text = "-";
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(460, 10);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(90, 28);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            // 
            // VisitDatePicker
            // 
            this.VisitDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.VisitDatePicker.Location = new System.Drawing.Point(330, 12);
            this.VisitDatePicker.Name = "VisitDatePicker";
            this.VisitDatePicker.Size = new System.Drawing.Size(120, 22);
            this.VisitDatePicker.TabIndex = 3;
            // 
            // VisitDateLabel
            // 
            this.VisitDateLabel.AutoSize = true;
            this.VisitDateLabel.Location = new System.Drawing.Point(280, 16);
            this.VisitDateLabel.Name = "VisitDateLabel";
            this.VisitDateLabel.Size = new System.Drawing.Size(44, 16);
            this.VisitDateLabel.TabIndex = 2;
            this.VisitDateLabel.Text = "Tgl:";
            // 
            // SalesComboBox
            // 
            this.SalesComboBox.FormattingEnabled = true;
            this.SalesComboBox.Location = new System.Drawing.Point(70, 12);
            this.SalesComboBox.Name = "SalesComboBox";
            this.SalesComboBox.Size = new System.Drawing.Size(200, 24);
            this.SalesComboBox.TabIndex = 1;
            // 
            // SalesLabel
            // 
            this.SalesLabel.AutoSize = true;
            this.SalesLabel.Location = new System.Drawing.Point(12, 16);
            this.SalesLabel.Name = "SalesLabel";
            this.SalesLabel.Size = new System.Drawing.Size(52, 16);
            this.SalesLabel.TabIndex = 0;
            this.SalesLabel.Text = "Sales:";
            // 
            // MainSplit
            // 
            this.MainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplit.Location = new System.Drawing.Point(0, 48);
            this.MainSplit.Name = "MainSplit";
            // 
            // MainSplit.Panel1
            // 
            this.MainSplit.Panel1.Controls.Add(this.LeftSplit);
            // 
            // MainSplit.Panel2
            // 
            this.MainSplit.Panel2.Controls.Add(this.RightPanel);
            this.MainSplit.Size = new System.Drawing.Size(984, 513);
            this.MainSplit.SplitterDistance = 620;
            this.MainSplit.TabIndex = 1;
            // 
            // LeftSplit
            // 
            this.LeftSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftSplit.Location = new System.Drawing.Point(0, 0);
            this.LeftSplit.Name = "LeftSplit";
            this.LeftSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // LeftSplit.Panel1
            // 
            this.LeftSplit.Panel1.Controls.Add(this.BasePlanGrid);
            // 
            // LeftSplit.Panel2
            // 
            this.LeftSplit.Panel2.Controls.Add(this.EffectivePlanGrid);
            this.LeftSplit.Size = new System.Drawing.Size(620, 513);
            this.LeftSplit.SplitterDistance = 250;
            this.LeftSplit.TabIndex = 0;
            // 
            // BasePlanGrid
            // 
            this.BasePlanGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BasePlanGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BasePlanGrid.Location = new System.Drawing.Point(0, 0);
            this.BasePlanGrid.Name = "BasePlanGrid";
            this.BasePlanGrid.ReadOnly = true;
            this.BasePlanGrid.RowHeadersWidth = 51;
            this.BasePlanGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.BasePlanGrid.Size = new System.Drawing.Size(620, 250);
            this.BasePlanGrid.TabIndex = 0;
            // 
            // EffectivePlanGrid
            // 
            this.EffectivePlanGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EffectivePlanGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EffectivePlanGrid.Location = new System.Drawing.Point(0, 0);
            this.EffectivePlanGrid.Name = "EffectivePlanGrid";
            this.EffectivePlanGrid.ReadOnly = true;
            this.EffectivePlanGrid.RowHeadersWidth = 51;
            this.EffectivePlanGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.EffectivePlanGrid.Size = new System.Drawing.Size(620, 259);
            this.EffectivePlanGrid.TabIndex = 0;
            // 
            // RightPanel
            // 
            this.RightPanel.Controls.Add(this.DeleteExceptionButton);
            this.RightPanel.Controls.Add(this.ReplaceButton);
            this.RightPanel.Controls.Add(this.RemoveButton);
            this.RightPanel.Controls.Add(this.AddButton);
            this.RightPanel.Controls.Add(this.ExceptionGrid);
            this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightPanel.Location = new System.Drawing.Point(0, 0);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.Size = new System.Drawing.Size(360, 513);
            this.RightPanel.TabIndex = 0;
            // 
            // DeleteExceptionButton
            // 
            this.DeleteExceptionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteExceptionButton.Location = new System.Drawing.Point(250, 12);
            this.DeleteExceptionButton.Name = "DeleteExceptionButton";
            this.DeleteExceptionButton.Size = new System.Drawing.Size(100, 28);
            this.DeleteExceptionButton.TabIndex = 4;
            this.DeleteExceptionButton.Text = "Hapus Exc.";
            this.DeleteExceptionButton.UseVisualStyleBackColor = true;
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.Location = new System.Drawing.Point(170, 12);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(70, 28);
            this.ReplaceButton.TabIndex = 3;
            this.ReplaceButton.Text = "Ganti";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            // 
            // RemoveButton
            // 
            this.RemoveButton.Location = new System.Drawing.Point(90, 12);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(70, 28);
            this.RemoveButton.TabIndex = 2;
            this.RemoveButton.Text = "Hapus";
            this.RemoveButton.UseVisualStyleBackColor = true;
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(10, 12);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(70, 28);
            this.AddButton.TabIndex = 1;
            this.AddButton.Text = "Tambah";
            this.AddButton.UseVisualStyleBackColor = true;
            // 
            // ExceptionGrid
            // 
            this.ExceptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExceptionGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ExceptionGrid.Location = new System.Drawing.Point(10, 50);
            this.ExceptionGrid.Name = "ExceptionGrid";
            this.ExceptionGrid.ReadOnly = true;
            this.ExceptionGrid.RowHeadersWidth = 51;
            this.ExceptionGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ExceptionGrid.Size = new System.Drawing.Size(340, 450);
            this.ExceptionGrid.TabIndex = 0;
            // 
            // JadwalKunjunganForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.MainSplit);
            this.Controls.Add(this.TopPanel);
            this.Name = "JadwalKunjunganForm";
            this.Text = "SM7-Jadwal Kunjungan";
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.MainSplit.Panel1.ResumeLayout(false);
            this.MainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplit)).EndInit();
            this.MainSplit.ResumeLayout(false);
            this.LeftSplit.Panel1.ResumeLayout(false);
            this.LeftSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LeftSplit)).EndInit();
            this.LeftSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BasePlanGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EffectivePlanGrid)).EndInit();
            this.RightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ExceptionGrid)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Label CycleLabel;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.DateTimePicker VisitDatePicker;
        private System.Windows.Forms.Label VisitDateLabel;
        private System.Windows.Forms.ComboBox SalesComboBox;
        private System.Windows.Forms.Label SalesLabel;
        private System.Windows.Forms.SplitContainer MainSplit;
        private System.Windows.Forms.SplitContainer LeftSplit;
        private System.Windows.Forms.DataGridView BasePlanGrid;
        private System.Windows.Forms.DataGridView EffectivePlanGrid;
        private System.Windows.Forms.Panel RightPanel;
        private System.Windows.Forms.Button DeleteExceptionButton;
        private System.Windows.Forms.Button ReplaceButton;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.DataGridView ExceptionGrid;
    }
}
