namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    partial class GenerateReturnOrderForm
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
            this.RefreshButton = new System.Windows.Forms.Button();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.FilterPanel = new System.Windows.Forms.Panel();
            this.labelPeriode = new System.Windows.Forms.Label();
            this.Tgl1Date = new System.Windows.Forms.DateTimePicker();
            this.labelSd = new System.Windows.Forms.Label();
            this.Tgl2Date = new System.Windows.Forms.DateTimePicker();
            this.labelCustomer = new System.Windows.Forms.Label();
            this.CustomerIdText = new System.Windows.Forms.TextBox();
            this.CustomerButton = new System.Windows.Forms.Button();
            this.CustomerNameText = new System.Windows.Forms.TextBox();
            this.labelWorklist = new System.Windows.Forms.Label();
            this.WorklistGrid = new System.Windows.Forms.DataGridView();
            this.DetailPanel = new System.Windows.Forms.Panel();
            this.labelItem = new System.Windows.Forms.Label();
            this.ItemGrid = new System.Windows.Forms.DataGridView();
            this.labelSales = new System.Windows.Forms.Label();
            this.SalesIdText = new System.Windows.Forms.TextBox();
            this.SalesButton = new System.Windows.Forms.Button();
            this.SalesNameText = new System.Windows.Forms.TextBox();
            this.labelDriver = new System.Windows.Forms.Label();
            this.DriverIdText = new System.Windows.Forms.TextBox();
            this.DriverButton = new System.Windows.Forms.Button();
            this.DriverNameText = new System.Windows.Forms.TextBox();
            this.CompleteButton = new System.Windows.Forms.Button();
            this.labelResult = new System.Windows.Forms.Label();
            this.ResultGrid = new System.Windows.Forms.DataGridView();
            this.FilterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WorklistGrid)).BeginInit();
            this.DetailPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(6, 6);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(80, 26);
            this.RefreshButton.TabIndex = 0;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            // 
            // GenerateButton
            // 
            this.GenerateButton.Location = new System.Drawing.Point(92, 6);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(90, 26);
            this.GenerateButton.TabIndex = 1;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            // 
            // FilterPanel
            // 
            this.FilterPanel.BackColor = System.Drawing.Color.Cornsilk;
            this.FilterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterPanel.Controls.Add(this.labelPeriode);
            this.FilterPanel.Controls.Add(this.Tgl1Date);
            this.FilterPanel.Controls.Add(this.labelSd);
            this.FilterPanel.Controls.Add(this.Tgl2Date);
            this.FilterPanel.Controls.Add(this.labelCustomer);
            this.FilterPanel.Controls.Add(this.CustomerIdText);
            this.FilterPanel.Controls.Add(this.CustomerButton);
            this.FilterPanel.Controls.Add(this.CustomerNameText);
            this.FilterPanel.Location = new System.Drawing.Point(6, 38);
            this.FilterPanel.Name = "FilterPanel";
            this.FilterPanel.Size = new System.Drawing.Size(988, 48);
            this.FilterPanel.TabIndex = 2;
            // 
            // labelPeriode
            // 
            this.labelPeriode.AutoSize = true;
            this.labelPeriode.Location = new System.Drawing.Point(8, 6);
            this.labelPeriode.Name = "labelPeriode";
            this.labelPeriode.Size = new System.Drawing.Size(45, 13);
            this.labelPeriode.TabIndex = 0;
            this.labelPeriode.Text = "Periode";
            // 
            // Tgl1Date
            // 
            this.Tgl1Date.Location = new System.Drawing.Point(8, 23);
            this.Tgl1Date.Name = "Tgl1Date";
            this.Tgl1Date.Size = new System.Drawing.Size(110, 22);
            this.Tgl1Date.TabIndex = 1;
            // 
            // labelSd
            // 
            this.labelSd.AutoSize = true;
            this.labelSd.Location = new System.Drawing.Point(124, 26);
            this.labelSd.Name = "labelSd";
            this.labelSd.Size = new System.Drawing.Size(22, 13);
            this.labelSd.TabIndex = 2;
            this.labelSd.Text = "s/d";
            // 
            // Tgl2Date
            // 
            this.Tgl2Date.Location = new System.Drawing.Point(150, 23);
            this.Tgl2Date.Name = "Tgl2Date";
            this.Tgl2Date.Size = new System.Drawing.Size(110, 22);
            this.Tgl2Date.TabIndex = 3;
            // 
            // labelCustomer
            // 
            this.labelCustomer.AutoSize = true;
            this.labelCustomer.Location = new System.Drawing.Point(280, 6);
            this.labelCustomer.Name = "labelCustomer";
            this.labelCustomer.Size = new System.Drawing.Size(57, 13);
            this.labelCustomer.TabIndex = 4;
            this.labelCustomer.Text = "Customer";
            // 
            // CustomerIdText
            // 
            this.CustomerIdText.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CustomerIdText.Location = new System.Drawing.Point(280, 23);
            this.CustomerIdText.Name = "CustomerIdText";
            this.CustomerIdText.Size = new System.Drawing.Size(80, 22);
            this.CustomerIdText.TabIndex = 5;
            // 
            // CustomerButton
            // 
            this.CustomerButton.Location = new System.Drawing.Point(364, 23);
            this.CustomerButton.Name = "CustomerButton";
            this.CustomerButton.Size = new System.Drawing.Size(30, 22);
            this.CustomerButton.TabIndex = 6;
            this.CustomerButton.Text = "...";
            this.CustomerButton.UseVisualStyleBackColor = true;
            // 
            // CustomerNameText
            // 
            this.CustomerNameText.Location = new System.Drawing.Point(400, 23);
            this.CustomerNameText.Name = "CustomerNameText";
            this.CustomerNameText.ReadOnly = true;
            this.CustomerNameText.Size = new System.Drawing.Size(320, 22);
            this.CustomerNameText.TabIndex = 7;
            // 
            // labelWorklist
            // 
            this.labelWorklist.AutoSize = true;
            this.labelWorklist.Location = new System.Drawing.Point(6, 92);
            this.labelWorklist.Name = "labelWorklist";
            this.labelWorklist.Size = new System.Drawing.Size(140, 13);
            this.labelWorklist.TabIndex = 3;
            this.labelWorklist.Text = "Return Order (Synced)";
            // 
            // WorklistGrid
            // 
            this.WorklistGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorklistGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.WorklistGrid.Location = new System.Drawing.Point(6, 110);
            this.WorklistGrid.Name = "WorklistGrid";
            this.WorklistGrid.Size = new System.Drawing.Size(988, 200);
            this.WorklistGrid.TabIndex = 4;
            // 
            // DetailPanel
            // 
            this.DetailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailPanel.BackColor = System.Drawing.Color.Cornsilk;
            this.DetailPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DetailPanel.Controls.Add(this.labelItem);
            this.DetailPanel.Controls.Add(this.ItemGrid);
            this.DetailPanel.Controls.Add(this.labelSales);
            this.DetailPanel.Controls.Add(this.SalesIdText);
            this.DetailPanel.Controls.Add(this.SalesButton);
            this.DetailPanel.Controls.Add(this.SalesNameText);
            this.DetailPanel.Controls.Add(this.labelDriver);
            this.DetailPanel.Controls.Add(this.DriverIdText);
            this.DetailPanel.Controls.Add(this.DriverButton);
            this.DetailPanel.Controls.Add(this.DriverNameText);
            this.DetailPanel.Controls.Add(this.CompleteButton);
            this.DetailPanel.Location = new System.Drawing.Point(6, 318);
            this.DetailPanel.Name = "DetailPanel";
            this.DetailPanel.Size = new System.Drawing.Size(988, 190);
            this.DetailPanel.TabIndex = 5;
            // 
            // labelItem
            // 
            this.labelItem.AutoSize = true;
            this.labelItem.Location = new System.Drawing.Point(8, 8);
            this.labelItem.Name = "labelItem";
            this.labelItem.Size = new System.Drawing.Size(30, 13);
            this.labelItem.TabIndex = 0;
            this.labelItem.Text = "Item";
            // 
            // ItemGrid
            // 
            this.ItemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ItemGrid.Location = new System.Drawing.Point(8, 26);
            this.ItemGrid.Name = "ItemGrid";
            this.ItemGrid.Size = new System.Drawing.Size(670, 156);
            this.ItemGrid.TabIndex = 1;
            // 
            // labelSales
            // 
            this.labelSales.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSales.AutoSize = true;
            this.labelSales.Location = new System.Drawing.Point(690, 8);
            this.labelSales.Name = "labelSales";
            this.labelSales.Size = new System.Drawing.Size(54, 13);
            this.labelSales.TabIndex = 2;
            this.labelSales.Text = "Salesman";
            // 
            // SalesIdText
            // 
            this.SalesIdText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SalesIdText.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SalesIdText.Location = new System.Drawing.Point(690, 26);
            this.SalesIdText.Name = "SalesIdText";
            this.SalesIdText.Size = new System.Drawing.Size(205, 22);
            this.SalesIdText.TabIndex = 3;
            // 
            // SalesButton
            // 
            this.SalesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SalesButton.Location = new System.Drawing.Point(899, 26);
            this.SalesButton.Name = "SalesButton";
            this.SalesButton.Size = new System.Drawing.Size(34, 22);
            this.SalesButton.TabIndex = 4;
            this.SalesButton.Text = "...";
            this.SalesButton.UseVisualStyleBackColor = true;
            // 
            // SalesNameText
            // 
            this.SalesNameText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SalesNameText.Location = new System.Drawing.Point(690, 52);
            this.SalesNameText.Name = "SalesNameText";
            this.SalesNameText.ReadOnly = true;
            this.SalesNameText.Size = new System.Drawing.Size(243, 22);
            this.SalesNameText.TabIndex = 5;
            // 
            // labelDriver
            // 
            this.labelDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDriver.AutoSize = true;
            this.labelDriver.Location = new System.Drawing.Point(690, 80);
            this.labelDriver.Name = "labelDriver";
            this.labelDriver.Size = new System.Drawing.Size(38, 13);
            this.labelDriver.TabIndex = 6;
            this.labelDriver.Text = "Driver";
            // 
            // DriverIdText
            // 
            this.DriverIdText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DriverIdText.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DriverIdText.Location = new System.Drawing.Point(690, 98);
            this.DriverIdText.Name = "DriverIdText";
            this.DriverIdText.Size = new System.Drawing.Size(205, 22);
            this.DriverIdText.TabIndex = 7;
            // 
            // DriverButton
            // 
            this.DriverButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DriverButton.Location = new System.Drawing.Point(899, 98);
            this.DriverButton.Name = "DriverButton";
            this.DriverButton.Size = new System.Drawing.Size(34, 22);
            this.DriverButton.TabIndex = 8;
            this.DriverButton.Text = "...";
            this.DriverButton.UseVisualStyleBackColor = true;
            // 
            // DriverNameText
            // 
            this.DriverNameText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DriverNameText.Location = new System.Drawing.Point(690, 124);
            this.DriverNameText.Name = "DriverNameText";
            this.DriverNameText.ReadOnly = true;
            this.DriverNameText.Size = new System.Drawing.Size(243, 22);
            this.DriverNameText.TabIndex = 9;
            // 
            // CompleteButton
            // 
            this.CompleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CompleteButton.Location = new System.Drawing.Point(690, 152);
            this.CompleteButton.Name = "CompleteButton";
            this.CompleteButton.Size = new System.Drawing.Size(150, 26);
            this.CompleteButton.TabIndex = 10;
            this.CompleteButton.Text = "Simpan Sales/Driver";
            this.CompleteButton.UseVisualStyleBackColor = true;
            // 
            // labelResult
            // 
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(6, 514);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(160, 13);
            this.labelResult.TabIndex = 6;
            this.labelResult.Text = "Hasil Generate (Retur Jual)";
            // 
            // ResultGrid
            // 
            this.ResultGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ResultGrid.Location = new System.Drawing.Point(6, 532);
            this.ResultGrid.Name = "ResultGrid";
            this.ResultGrid.Size = new System.Drawing.Size(988, 110);
            this.ResultGrid.TabIndex = 7;
            // 
            // GenerateReturnOrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Khaki;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.ResultGrid);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.DetailPanel);
            this.Controls.Add(this.WorklistGrid);
            this.Controls.Add(this.labelWorklist);
            this.Controls.Add(this.FilterPanel);
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.RefreshButton);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "GenerateReturnOrderForm";
            this.Text = "Generate Return Order";
            this.FilterPanel.ResumeLayout(false);
            this.FilterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WorklistGrid)).EndInit();
            this.DetailPanel.ResumeLayout(false);
            this.DetailPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.Panel FilterPanel;
        private System.Windows.Forms.Label labelPeriode;
        private System.Windows.Forms.DateTimePicker Tgl1Date;
        private System.Windows.Forms.Label labelSd;
        private System.Windows.Forms.DateTimePicker Tgl2Date;
        private System.Windows.Forms.Label labelCustomer;
        private System.Windows.Forms.TextBox CustomerIdText;
        private System.Windows.Forms.Button CustomerButton;
        private System.Windows.Forms.TextBox CustomerNameText;
        private System.Windows.Forms.Label labelWorklist;
        private System.Windows.Forms.DataGridView WorklistGrid;
        private System.Windows.Forms.Panel DetailPanel;
        private System.Windows.Forms.Label labelItem;
        private System.Windows.Forms.DataGridView ItemGrid;
        private System.Windows.Forms.Label labelSales;
        private System.Windows.Forms.TextBox SalesIdText;
        private System.Windows.Forms.Button SalesButton;
        private System.Windows.Forms.TextBox SalesNameText;
        private System.Windows.Forms.Label labelDriver;
        private System.Windows.Forms.TextBox DriverIdText;
        private System.Windows.Forms.Button DriverButton;
        private System.Windows.Forms.TextBox DriverNameText;
        private System.Windows.Forms.Button CompleteButton;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.DataGridView ResultGrid;
    }
}
