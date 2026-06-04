namespace btr.distrib.SalesContext.SalesPersonAgg

{

    partial class SalesOmzetInfoForm

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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ExcelButton = new System.Windows.Forms.Button();
            this.ChartButton = new System.Windows.Forms.Button();
            this.ProsesButton = new System.Windows.Forms.Button();
            this.MaterializeButton = new System.Windows.Forms.Button();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.SalesPeriodCheckBox = new System.Windows.Forms.CheckBox();
            this.Tgl2Date = new System.Windows.Forms.DateTimePicker();
            this.Tgl1Date = new System.Windows.Forms.DateTimePicker();
            this.InfoGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
            this.summaryPanel = new System.Windows.Forms.Panel();
            this.SummaryLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InfoGrid)).BeginInit();
            this.summaryPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.PowderBlue;
            this.panel1.Controls.Add(this.ExcelButton);
            this.panel1.Controls.Add(this.ChartButton);
            this.panel1.Controls.Add(this.ProsesButton);
            this.panel1.Controls.Add(this.MaterializeButton);
            this.panel1.Controls.Add(this.SearchText);
            this.panel1.Controls.Add(this.SalesPeriodCheckBox);
            this.panel1.Controls.Add(this.Tgl2Date);
            this.panel1.Controls.Add(this.Tgl1Date);
            this.panel1.Location = new System.Drawing.Point(7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(823, 34);
            this.panel1.TabIndex = 0;
            // 
            // ExcelButton
            // 
            this.ExcelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExcelButton.Image = global::btr.distrib.Properties.Resources.icons8_microsoft_excel_32;
            this.ExcelButton.Location = new System.Drawing.Point(790, 3);
            this.ExcelButton.Name = "ExcelButton";
            this.ExcelButton.Size = new System.Drawing.Size(28, 28);
            this.ExcelButton.TabIndex = 7;
            this.ExcelButton.UseVisualStyleBackColor = true;
            // 
            // ChartButton
            // 
            this.ChartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChartButton.Image = global::btr.distrib.Properties.Resources.icons8_graph_32;
            this.ChartButton.Location = new System.Drawing.Point(758, 3);
            this.ChartButton.Name = "ChartButton";
            this.ChartButton.Size = new System.Drawing.Size(28, 28);
            this.ChartButton.TabIndex = 6;
            this.ChartButton.UseVisualStyleBackColor = true;
            // 
            // ProsesButton
            // 
            this.ProsesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProsesButton.Location = new System.Drawing.Point(645, 3);
            this.ProsesButton.Name = "ProsesButton";
            this.ProsesButton.Size = new System.Drawing.Size(77, 28);
            this.ProsesButton.TabIndex = 4;
            this.ProsesButton.Text = "Proses";
            this.ProsesButton.UseVisualStyleBackColor = true;
            // 
            // MaterializeButton
            // 
            this.MaterializeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MaterializeButton.Image = global::btr.distrib.Properties.Resources.icons8_stack_overflow_32;
            this.MaterializeButton.Location = new System.Drawing.Point(726, 3);
            this.MaterializeButton.Name = "MaterializeButton";
            this.MaterializeButton.Size = new System.Drawing.Size(28, 28);
            this.MaterializeButton.TabIndex = 5;
            this.MaterializeButton.UseVisualStyleBackColor = true;
            // 
            // SearchText
            // 
            this.SearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchText.Location = new System.Drawing.Point(378, 6);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(261, 22);
            this.SearchText.TabIndex = 3;
            // 
            // SalesPeriodCheckBox
            // 
            this.SalesPeriodCheckBox.AutoSize = true;
            this.SalesPeriodCheckBox.Location = new System.Drawing.Point(284, 11);
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
            this.Tgl2Date.Location = new System.Drawing.Point(145, 6);
            this.Tgl2Date.Name = "Tgl2Date";
            this.Tgl2Date.Size = new System.Drawing.Size(133, 22);
            this.Tgl2Date.TabIndex = 1;
            // 
            // Tgl1Date
            // 
            this.Tgl1Date.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Tgl1Date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl1Date.Location = new System.Drawing.Point(6, 6);
            this.Tgl1Date.Name = "Tgl1Date";
            this.Tgl1Date.Size = new System.Drawing.Size(133, 22);
            this.Tgl1Date.TabIndex = 0;
            // 
            // InfoGrid
            // 
            this.InfoGrid.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.InfoGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoGrid.BackColor = System.Drawing.SystemColors.Window;
            this.InfoGrid.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoGrid.Location = new System.Drawing.Point(7, 47);
            this.InfoGrid.Name = "InfoGrid";
            this.InfoGrid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.GrayWhenLostFocus;
            this.InfoGrid.Size = new System.Drawing.Size(823, 363);
            this.InfoGrid.TabIndex = 1;
            this.InfoGrid.Text = "gridGroupingControl1";
            this.InfoGrid.UseRightToLeftCompatibleTextBox = true;
            this.InfoGrid.VersionInfo = "22.1460.34";
            // 
            // summaryPanel
            // 
            this.summaryPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.summaryPanel.BackColor = System.Drawing.Color.LightYellow;
            this.summaryPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.summaryPanel.Controls.Add(this.SummaryLabel);
            this.summaryPanel.Location = new System.Drawing.Point(7, 416);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Size = new System.Drawing.Size(823, 27);
            this.summaryPanel.TabIndex = 2;
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SummaryLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SummaryLabel.Location = new System.Drawing.Point(0, 0);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.SummaryLabel.Size = new System.Drawing.Size(821, 25);
            this.SummaryLabel.TabIndex = 0;
            this.SummaryLabel.Text = "Order Count: 0   Faktur Count: 0   Total Order: 0   Total Faktur: 0";
            this.SummaryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SalesOmzetInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CadetBlue;
            this.ClientSize = new System.Drawing.Size(836, 450);
            this.Controls.Add(this.summaryPanel);
            this.Controls.Add(this.InfoGrid);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SalesOmzetInfoForm";
            this.Text = "RO2 - Sales Omzet";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InfoGrid)).EndInit();
            this.summaryPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }



        #endregion



        private System.Windows.Forms.Panel panel1;

        private System.Windows.Forms.Button ExcelButton;

        private System.Windows.Forms.Button ChartButton;

        private System.Windows.Forms.Button ProsesButton;

        private System.Windows.Forms.Button MaterializeButton;

        private System.Windows.Forms.TextBox SearchText;

        private System.Windows.Forms.CheckBox SalesPeriodCheckBox;

        private System.Windows.Forms.DateTimePicker Tgl2Date;

        private System.Windows.Forms.DateTimePicker Tgl1Date;

        private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl InfoGrid;

        private System.Windows.Forms.Panel summaryPanel;

        private System.Windows.Forms.Label SummaryLabel;

    }

}

