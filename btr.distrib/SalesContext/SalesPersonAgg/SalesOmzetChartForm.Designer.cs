namespace btr.distrib.SalesContext.SalesPersonAgg
{
    partial class SalesOmzetChartForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
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
            this.chartPanel = new System.Windows.Forms.Panel();
            this.ChartModeCombo = new System.Windows.Forms.ComboBox();
            this.OmzetChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.kpiPanel.SuspendLayout();
            this.chartPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OmzetChart)).BeginInit();
            this.SuspendLayout();
            // 
            // kpiPanel
            // 
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
            this.kpiPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.kpiPanel.Location = new System.Drawing.Point(0, 0);
            this.kpiPanel.Name = "kpiPanel";
            this.kpiPanel.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.kpiPanel.Size = new System.Drawing.Size(784, 32);
            this.kpiPanel.TabIndex = 0;
            // 
            // AchievementValueLabel
            // 
            this.AchievementValueLabel.AutoSize = true;
            this.AchievementValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.AchievementValueLabel.Location = new System.Drawing.Point(738, 8);
            this.AchievementValueLabel.Name = "AchievementValueLabel";
            this.AchievementValueLabel.Size = new System.Drawing.Size(18, 13);
            this.AchievementValueLabel.TabIndex = 9;
            this.AchievementValueLabel.Text = "—";
            // 
            // AchievementCaptionLabel
            // 
            this.AchievementCaptionLabel.AutoSize = true;
            this.AchievementCaptionLabel.Location = new System.Drawing.Point(678, 8);
            this.AchievementCaptionLabel.Name = "AchievementCaptionLabel";
            this.AchievementCaptionLabel.Size = new System.Drawing.Size(51, 13);
            this.AchievementCaptionLabel.TabIndex = 8;
            this.AchievementCaptionLabel.Text = "Tercapai:";
            // 
            // TargetValueLabel
            // 
            this.TargetValueLabel.AutoSize = true;
            this.TargetValueLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.TargetValueLabel.Location = new System.Drawing.Point(620, 8);
            this.TargetValueLabel.Name = "TargetValueLabel";
            this.TargetValueLabel.Size = new System.Drawing.Size(18, 13);
            this.TargetValueLabel.TabIndex = 7;
            this.TargetValueLabel.Text = "—";
            // 
            // TargetCaptionLabel
            // 
            this.TargetCaptionLabel.AutoSize = true;
            this.TargetCaptionLabel.Location = new System.Drawing.Point(548, 8);
            this.TargetCaptionLabel.Name = "TargetCaptionLabel";
            this.TargetCaptionLabel.Size = new System.Drawing.Size(64, 13);
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
            this.TransactionCountCaptionLabel.Size = new System.Drawing.Size(56, 13);
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
            this.PipelineCaptionLabel.Size = new System.Drawing.Size(51, 13);
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
            this.RecognizedOmzetCaptionLabel.Size = new System.Drawing.Size(101, 13);
            this.RecognizedOmzetCaptionLabel.TabIndex = 0;
            this.RecognizedOmzetCaptionLabel.Text = "Omzet diakui (Rp):";
            // 
            // chartPanel
            // 
            this.chartPanel.Controls.Add(this.ChartModeCombo);
            this.chartPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartPanel.Location = new System.Drawing.Point(0, 32);
            this.chartPanel.Name = "chartPanel";
            this.chartPanel.Padding = new System.Windows.Forms.Padding(6, 4, 6, 0);
            this.chartPanel.Size = new System.Drawing.Size(784, 29);
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
            this.ChartModeCombo.Location = new System.Drawing.Point(6, 4);
            this.ChartModeCombo.Name = "ChartModeCombo";
            this.ChartModeCombo.Size = new System.Drawing.Size(772, 21);
            this.ChartModeCombo.TabIndex = 0;
            // 
            // OmzetChart
            // 
            chartArea1.Name = "MainArea";
            this.OmzetChart.ChartAreas.Add(chartArea1);
            this.OmzetChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.OmzetChart.Legends.Add(legend1);
            this.OmzetChart.Location = new System.Drawing.Point(0, 61);
            this.OmzetChart.Name = "OmzetChart";
            this.OmzetChart.Size = new System.Drawing.Size(784, 389);
            this.OmzetChart.TabIndex = 2;
            this.OmzetChart.Text = "OmzetChart";
            // 
            // SalesOmzetChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CadetBlue;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.OmzetChart);
            this.Controls.Add(this.chartPanel);
            this.Controls.Add(this.kpiPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "SalesOmzetChartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RO2 - Sales Omzet Grafik";
            this.kpiPanel.ResumeLayout(false);
            this.kpiPanel.PerformLayout();
            this.chartPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OmzetChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

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
        private System.Windows.Forms.Panel chartPanel;
        private System.Windows.Forms.ComboBox ChartModeCombo;
        private System.Windows.Forms.DataVisualization.Charting.Chart OmzetChart;
    }
}
