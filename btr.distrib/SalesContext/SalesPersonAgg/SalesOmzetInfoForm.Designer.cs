namespace btr.distrib.SalesContext.SalesPersonAgg
{
    partial class SalesOmzetInfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ExcelButton = new System.Windows.Forms.Button();
            this.ProsesButton = new System.Windows.Forms.Button();
            this.MaterializeButton = new System.Windows.Forms.Button();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.SalesPeriodCheckBox = new System.Windows.Forms.CheckBox();
            this.Tgl2Date = new System.Windows.Forms.DateTimePicker();
            this.Tgl1Date = new System.Windows.Forms.DateTimePicker();
            this.healthPanel = new System.Windows.Forms.Panel();
            this.MaterializeHealthLabel = new System.Windows.Forms.Label();
            this.HealthWindowEndDate = new System.Windows.Forms.DateTimePicker();
            this.HealthWindowEndLabel = new System.Windows.Forms.Label();
            this.kpiPanel = new System.Windows.Forms.Panel();
            this.AchievementValueLabel = new System.Windows.Forms.Label();
            this.AchievementCaptionLabel = new System.Windows.Forms.Label();
            this.TargetValueLabel = new System.Windows.Forms.Label();
            this.TargetCaptionLabel = new System.Windows.Forms.Label();
            this.TransactionCountValueLabel = new System.Windows.Forms.Label();
            this.TransactionCountCaptionLabel = new System.Windows.Forms.Label();
            this.PipelineValueLabel = new System.Windows.Forms.Label();
            this.PipelineCaptionLabel = new System.Windows.Forms.Label();
            this.RecognizedOmzetValueLabel = new System.Windows.Forms.Label();
            this.RecognizedOmzetCaptionLabel = new System.Windows.Forms.Label();
            this.contentSplitContainer = new System.Windows.Forms.SplitContainer();
            this.chartPanel = new System.Windows.Forms.Panel();
            this.ChartModeCombo = new System.Windows.Forms.ComboBox();
            this.OmzetChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.InfoGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
            this.panel1.SuspendLayout();
            this.healthPanel.SuspendLayout();
            this.kpiPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contentSplitContainer)).BeginInit();
            this.chartPanel.SuspendLayout();
            this.contentSplitContainer.Panel1.SuspendLayout();
            this.contentSplitContainer.Panel2.SuspendLayout();
            this.contentSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OmzetChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InfoGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.PowderBlue;
            this.panel1.Controls.Add(this.ExcelButton);
            this.panel1.Controls.Add(this.ProsesButton);
            this.panel1.Controls.Add(this.MaterializeButton);
            this.panel1.Controls.Add(this.SearchText);
            this.panel1.Controls.Add(this.SalesPeriodCheckBox);
            this.panel1.Controls.Add(this.Tgl2Date);
            this.panel1.Controls.Add(this.Tgl1Date);
            this.panel1.Location = new System.Drawing.Point(7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(787, 34);
            this.panel1.TabIndex = 0;
            // 
            // ExcelButton
            // 
            this.ExcelButton.Location = new System.Drawing.Point(703, 6);
            this.ExcelButton.Name = "ExcelButton";
            this.ExcelButton.Size = new System.Drawing.Size(80, 23);
            this.ExcelButton.TabIndex = 5;
            this.ExcelButton.Text = "Excel";
            this.ExcelButton.UseVisualStyleBackColor = true;
            // 
            // ProsesButton
            // 
            this.ProsesButton.Location = new System.Drawing.Point(627, 6);
            this.ProsesButton.Name = "ProsesButton";
            this.ProsesButton.Size = new System.Drawing.Size(70, 23);
            this.ProsesButton.TabIndex = 4;
            this.ProsesButton.Text = "Proses";
            this.ProsesButton.UseVisualStyleBackColor = true;
            // 
            // MaterializeButton
            // 
            this.MaterializeButton.Location = new System.Drawing.Point(531, 6);
            this.MaterializeButton.Name = "MaterializeButton";
            this.MaterializeButton.Size = new System.Drawing.Size(90, 23);
            this.MaterializeButton.TabIndex = 6;
            this.MaterializeButton.Text = "Materialisasi";
            this.MaterializeButton.UseVisualStyleBackColor = true;
            // 
            // SearchText
            // 
            this.SearchText.Location = new System.Drawing.Point(424, 6);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(101, 22);
            this.SearchText.TabIndex = 3;
            // 
            // SalesPeriodCheckBox
            // 
            this.SalesPeriodCheckBox.AutoSize = true;
            this.SalesPeriodCheckBox.Location = new System.Drawing.Point(330, 10);
            this.SalesPeriodCheckBox.Name = "SalesPeriodCheckBox";
            this.SalesPeriodCheckBox.Size = new System.Drawing.Size(88, 17);
            this.SalesPeriodCheckBox.TabIndex = 2;
            this.SalesPeriodCheckBox.Text = "Periode Jual";
            this.SalesPeriodCheckBox.UseVisualStyleBackColor = true;
            // 
            // Tgl2Date
            // 
            this.Tgl2Date.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Tgl2Date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl2Date.Location = new System.Drawing.Point(168, 6);
            this.Tgl2Date.Name = "Tgl2Date";
            this.Tgl2Date.Size = new System.Drawing.Size(156, 22);
            this.Tgl2Date.TabIndex = 1;
            // 
            // Tgl1Date
            // 
            this.Tgl1Date.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Tgl1Date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl1Date.Location = new System.Drawing.Point(6, 6);
            this.Tgl1Date.Name = "Tgl1Date";
            this.Tgl1Date.Size = new System.Drawing.Size(156, 22);
            this.Tgl1Date.TabIndex = 0;
            // 
            // healthPanel
            // 
            this.healthPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.healthPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.healthPanel.Controls.Add(this.MaterializeHealthLabel);
            this.healthPanel.Controls.Add(this.HealthWindowEndDate);
            this.healthPanel.Controls.Add(this.HealthWindowEndLabel);
            this.healthPanel.Location = new System.Drawing.Point(7, 47);
            this.healthPanel.Name = "healthPanel";
            this.healthPanel.Size = new System.Drawing.Size(787, 28);
            this.healthPanel.TabIndex = 1;
            // 
            // MaterializeHealthLabel
            // 
            this.MaterializeHealthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MaterializeHealthLabel.AutoEllipsis = true;
            this.MaterializeHealthLabel.Location = new System.Drawing.Point(268, 6);
            this.MaterializeHealthLabel.Name = "MaterializeHealthLabel";
            this.MaterializeHealthLabel.Size = new System.Drawing.Size(515, 16);
            this.MaterializeHealthLabel.TabIndex = 2;
            this.MaterializeHealthLabel.Text = "Memuat indikator materialisasi...";
            this.MaterializeHealthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // HealthWindowEndDate
            // 
            this.HealthWindowEndDate.CustomFormat = "ddd, dd-MMM-yyyy";
            this.HealthWindowEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.HealthWindowEndDate.Location = new System.Drawing.Point(106, 4);
            this.HealthWindowEndDate.Name = "HealthWindowEndDate";
            this.HealthWindowEndDate.Size = new System.Drawing.Size(156, 22);
            this.HealthWindowEndDate.TabIndex = 1;
            // 
            // HealthWindowEndLabel
            // 
            this.HealthWindowEndLabel.AutoSize = true;
            this.HealthWindowEndLabel.Location = new System.Drawing.Point(6, 8);
            this.HealthWindowEndLabel.Name = "HealthWindowEndLabel";
            this.HealthWindowEndLabel.Size = new System.Drawing.Size(94, 13);
            this.HealthWindowEndLabel.TabIndex = 0;
            this.HealthWindowEndLabel.Text = "Indikator sampai:";
            // 
            // kpiPanel
            // 
            this.kpiPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.kpiPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.kpiPanel.Controls.Add(this.AchievementValueLabel);
            this.kpiPanel.Controls.Add(this.AchievementCaptionLabel);
            this.kpiPanel.Controls.Add(this.TargetValueLabel);
            this.kpiPanel.Controls.Add(this.TargetCaptionLabel);
            this.kpiPanel.Controls.Add(this.TransactionCountValueLabel);
            this.kpiPanel.Controls.Add(this.TransactionCountCaptionLabel);
            this.kpiPanel.Controls.Add(this.PipelineValueLabel);
            this.kpiPanel.Controls.Add(this.PipelineCaptionLabel);
            this.kpiPanel.Controls.Add(this.RecognizedOmzetValueLabel);
            this.kpiPanel.Controls.Add(this.RecognizedOmzetCaptionLabel);
            this.kpiPanel.Location = new System.Drawing.Point(7, 81);
            this.kpiPanel.Name = "kpiPanel";
            this.kpiPanel.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.kpiPanel.Size = new System.Drawing.Size(787, 32);
            this.kpiPanel.TabIndex = 2;
            // 
            // AchievementValueLabel
            // 
            this.AchievementValueLabel.AutoSize = true;
            this.AchievementValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.AchievementValueLabel.Location = new System.Drawing.Point(738, 8);
            this.AchievementValueLabel.Name = "AchievementValueLabel";
            this.AchievementValueLabel.Size = new System.Drawing.Size(14, 13);
            this.AchievementValueLabel.TabIndex = 9;
            this.AchievementValueLabel.Text = "—";
            // 
            // AchievementCaptionLabel
            // 
            this.AchievementCaptionLabel.AutoSize = true;
            this.AchievementCaptionLabel.Location = new System.Drawing.Point(678, 8);
            this.AchievementCaptionLabel.Name = "AchievementCaptionLabel";
            this.AchievementCaptionLabel.Size = new System.Drawing.Size(54, 13);
            this.AchievementCaptionLabel.TabIndex = 8;
            this.AchievementCaptionLabel.Text = "Tercapai:";
            // 
            // TargetValueLabel
            // 
            this.TargetValueLabel.AutoSize = true;
            this.TargetValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.TargetValueLabel.Location = new System.Drawing.Point(620, 8);
            this.TargetValueLabel.Name = "TargetValueLabel";
            this.TargetValueLabel.Size = new System.Drawing.Size(14, 13);
            this.TargetValueLabel.TabIndex = 7;
            this.TargetValueLabel.Text = "—";
            // 
            // TargetCaptionLabel
            // 
            this.TargetCaptionLabel.AutoSize = true;
            this.TargetCaptionLabel.Location = new System.Drawing.Point(548, 8);
            this.TargetCaptionLabel.Name = "TargetCaptionLabel";
            this.TargetCaptionLabel.Size = new System.Drawing.Size(66, 13);
            this.TargetCaptionLabel.TabIndex = 6;
            this.TargetCaptionLabel.Text = "Target (Rp):";
            // 
            // TransactionCountValueLabel
            // 
            this.TransactionCountValueLabel.AutoSize = true;
            this.TransactionCountValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.TransactionCountValueLabel.Location = new System.Drawing.Point(500, 8);
            this.TransactionCountValueLabel.Name = "TransactionCountValueLabel";
            this.TransactionCountValueLabel.Size = new System.Drawing.Size(13, 13);
            this.TransactionCountValueLabel.TabIndex = 5;
            this.TransactionCountValueLabel.Text = "0";
            // 
            // TransactionCountCaptionLabel
            // 
            this.TransactionCountCaptionLabel.AutoSize = true;
            this.TransactionCountCaptionLabel.Location = new System.Drawing.Point(432, 8);
            this.TransactionCountCaptionLabel.Name = "TransactionCountCaptionLabel";
            this.TransactionCountCaptionLabel.Size = new System.Drawing.Size(62, 13);
            this.TransactionCountCaptionLabel.TabIndex = 4;
            this.TransactionCountCaptionLabel.Text = "Transaksi:";
            // 
            // PipelineValueLabel
            // 
            this.PipelineValueLabel.AutoSize = true;
            this.PipelineValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.PipelineValueLabel.Location = new System.Drawing.Point(360, 8);
            this.PipelineValueLabel.Name = "PipelineValueLabel";
            this.PipelineValueLabel.Size = new System.Drawing.Size(13, 13);
            this.PipelineValueLabel.TabIndex = 3;
            this.PipelineValueLabel.Text = "0";
            // 
            // PipelineCaptionLabel
            // 
            this.PipelineCaptionLabel.AutoSize = true;
            this.PipelineCaptionLabel.Location = new System.Drawing.Point(300, 8);
            this.PipelineCaptionLabel.Name = "PipelineCaptionLabel";
            this.PipelineCaptionLabel.Size = new System.Drawing.Size(54, 13);
            this.PipelineCaptionLabel.TabIndex = 2;
            this.PipelineCaptionLabel.Text = "Pipeline:";
            // 
            // RecognizedOmzetValueLabel
            // 
            this.RecognizedOmzetValueLabel.AutoSize = true;
            this.RecognizedOmzetValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.RecognizedOmzetValueLabel.Location = new System.Drawing.Point(108, 8);
            this.RecognizedOmzetValueLabel.Name = "RecognizedOmzetValueLabel";
            this.RecognizedOmzetValueLabel.Size = new System.Drawing.Size(13, 13);
            this.RecognizedOmzetValueLabel.TabIndex = 1;
            this.RecognizedOmzetValueLabel.Text = "0";
            // 
            // RecognizedOmzetCaptionLabel
            // 
            this.RecognizedOmzetCaptionLabel.AutoSize = true;
            this.RecognizedOmzetCaptionLabel.Location = new System.Drawing.Point(9, 8);
            this.RecognizedOmzetCaptionLabel.Name = "RecognizedOmzetCaptionLabel";
            this.RecognizedOmzetCaptionLabel.Size = new System.Drawing.Size(93, 13);
            this.RecognizedOmzetCaptionLabel.TabIndex = 0;
            this.RecognizedOmzetCaptionLabel.Text = "Omzet diakui (Rp):";
            // 
            // contentSplitContainer
            // 
            this.contentSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentSplitContainer.Location = new System.Drawing.Point(7, 119);
            this.contentSplitContainer.Name = "contentSplitContainer";
            // 
            // contentSplitContainer.Panel1
            // 
            this.contentSplitContainer.Panel1.Controls.Add(this.OmzetChart);
            this.contentSplitContainer.Panel1.Controls.Add(this.chartPanel);
            // 
            // chartPanel
            // 
            this.chartPanel.Controls.Add(this.ChartModeCombo);
            this.chartPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartPanel.Location = new System.Drawing.Point(0, 0);
            this.chartPanel.Name = "chartPanel";
            this.chartPanel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.chartPanel.Size = new System.Drawing.Size(275, 30);
            this.chartPanel.TabIndex = 1;
            // 
            // ChartModeCombo
            // 
            this.ChartModeCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChartModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChartModeCombo.FormattingEnabled = true;
            this.ChartModeCombo.Items.AddRange(new object[] {
            "Status",
            "Mingguan",
            "Perbandingan"});
            this.ChartModeCombo.Location = new System.Drawing.Point(4, 4);
            this.ChartModeCombo.Name = "ChartModeCombo";
            this.ChartModeCombo.Size = new System.Drawing.Size(267, 21);
            this.ChartModeCombo.TabIndex = 0;
            // 
            // contentSplitContainer.Panel2
            // 
            this.contentSplitContainer.Panel2.Controls.Add(this.InfoGrid);
            this.contentSplitContainer.Size = new System.Drawing.Size(787, 325);
            this.contentSplitContainer.SplitterDistance = 275;
            this.contentSplitContainer.TabIndex = 3;
            // 
            // OmzetChart
            // 
            this.OmzetChart.BackColor = System.Drawing.Color.White;
            chartArea1.Name = "MainArea";
            this.OmzetChart.ChartAreas.Add(chartArea1);
            this.OmzetChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.OmzetChart.Legends.Add(legend1);
            this.OmzetChart.Location = new System.Drawing.Point(0, 0);
            this.OmzetChart.Name = "OmzetChart";
            this.OmzetChart.Size = new System.Drawing.Size(275, 325);
            this.OmzetChart.TabIndex = 0;
            this.OmzetChart.Text = "OmzetChart";
            // 
            // InfoGrid
            // 
            this.InfoGrid.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.InfoGrid.BackColor = System.Drawing.SystemColors.Window;
            this.InfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoGrid.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoGrid.Location = new System.Drawing.Point(0, 0);
            this.InfoGrid.Name = "InfoGrid";
            this.InfoGrid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.GrayWhenLostFocus;
            this.InfoGrid.Size = new System.Drawing.Size(508, 325);
            this.InfoGrid.TabIndex = 0;
            this.InfoGrid.Text = "gridGroupingControl1";
            this.InfoGrid.UseRightToLeftCompatibleTextBox = true;
            this.InfoGrid.VersionInfo = "22.1460.34";
            // 
            // SalesOmzetInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CadetBlue;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.contentSplitContainer);
            this.Controls.Add(this.kpiPanel);
            this.Controls.Add(this.healthPanel);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SalesOmzetInfoForm";
            this.Text = "RO2 - Sales Omzet";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.healthPanel.ResumeLayout(false);
            this.healthPanel.PerformLayout();
            this.kpiPanel.ResumeLayout(false);
            this.kpiPanel.PerformLayout();
            this.chartPanel.ResumeLayout(false);
            this.contentSplitContainer.Panel1.ResumeLayout(false);
            this.contentSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contentSplitContainer)).EndInit();
            this.contentSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OmzetChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InfoGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ExcelButton;
        private System.Windows.Forms.Button ProsesButton;
        private System.Windows.Forms.Button MaterializeButton;
        private System.Windows.Forms.TextBox SearchText;
        private System.Windows.Forms.CheckBox SalesPeriodCheckBox;
        private System.Windows.Forms.DateTimePicker Tgl2Date;
        private System.Windows.Forms.DateTimePicker Tgl1Date;
        private System.Windows.Forms.Panel healthPanel;
        private System.Windows.Forms.Label MaterializeHealthLabel;
        private System.Windows.Forms.DateTimePicker HealthWindowEndDate;
        private System.Windows.Forms.Label HealthWindowEndLabel;
        private System.Windows.Forms.Panel kpiPanel;
        private System.Windows.Forms.Label RecognizedOmzetCaptionLabel;
        private System.Windows.Forms.Label RecognizedOmzetValueLabel;
        private System.Windows.Forms.Label PipelineCaptionLabel;
        private System.Windows.Forms.Label PipelineValueLabel;
        private System.Windows.Forms.Label TransactionCountCaptionLabel;
        private System.Windows.Forms.Label TransactionCountValueLabel;
        private System.Windows.Forms.Label TargetCaptionLabel;
        private System.Windows.Forms.Label TargetValueLabel;
        private System.Windows.Forms.Label AchievementCaptionLabel;
        private System.Windows.Forms.Label AchievementValueLabel;
        private System.Windows.Forms.SplitContainer contentSplitContainer;
        private System.Windows.Forms.Panel chartPanel;
        private System.Windows.Forms.ComboBox ChartModeCombo;
        private System.Windows.Forms.DataVisualization.Charting.Chart OmzetChart;
        private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl InfoGrid;
    }
}
