namespace BtrGudang.Winform.Forms
{
    partial class PK1PrintPackingOrderForm
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
            this.Tgl1DatePicker = new System.Windows.Forms.DateTimePicker();
            this.Tgl2DatePicker = new System.Windows.Forms.DateTimePicker();
            this.ListFakturButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.AllFakturPage = new System.Windows.Forms.TabPage();
            this.PisahHalamanCheckBox = new System.Windows.Forms.CheckBox();
            this.PrintPerSupplierButton = new System.Windows.Forms.Button();
            this.PrintPerFakturButton = new System.Windows.Forms.Button();
            this.AllFakturGrid = new System.Windows.Forms.DataGridView();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.PerSupplierPage = new System.Windows.Forms.TabPage();
            this.PerSupplierFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.PerSupplierGrid = new System.Windows.Forms.DataGridView();
            this.PerFakturPage = new System.Windows.Forms.TabPage();
            this.PrintFakturListButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.AllFakturPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AllFakturGrid)).BeginInit();
            this.PerSupplierPage.SuspendLayout();
            this.PerSupplierFlowPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PerSupplierGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // Tgl1DatePicker
            // 
            this.Tgl1DatePicker.CustomFormat = "ddd, dd-MM-yyyy";
            this.Tgl1DatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl1DatePicker.Location = new System.Drawing.Point(12, 12);
            this.Tgl1DatePicker.Name = "Tgl1DatePicker";
            this.Tgl1DatePicker.Size = new System.Drawing.Size(123, 22);
            this.Tgl1DatePicker.TabIndex = 1;
            // 
            // Tgl2DatePicker
            // 
            this.Tgl2DatePicker.CustomFormat = "ddd, dd-MM-yyyy";
            this.Tgl2DatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Tgl2DatePicker.Location = new System.Drawing.Point(141, 12);
            this.Tgl2DatePicker.Name = "Tgl2DatePicker";
            this.Tgl2DatePicker.Size = new System.Drawing.Size(123, 22);
            this.Tgl2DatePicker.TabIndex = 2;
            // 
            // ListFakturButton
            // 
            this.ListFakturButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.ListFakturButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ListFakturButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ListFakturButton.Location = new System.Drawing.Point(270, 12);
            this.ListFakturButton.Name = "ListFakturButton";
            this.ListFakturButton.Size = new System.Drawing.Size(75, 23);
            this.ListFakturButton.TabIndex = 3;
            this.ListFakturButton.Text = "List Faktur";
            this.ListFakturButton.UseVisualStyleBackColor = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.AllFakturPage);
            this.tabControl1.Controls.Add(this.PerSupplierPage);
            this.tabControl1.Controls.Add(this.PerFakturPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(888, 454);
            this.tabControl1.TabIndex = 5;
            // 
            // AllFakturPage
            // 
            this.AllFakturPage.Controls.Add(this.PrintFakturListButton);
            this.AllFakturPage.Controls.Add(this.PisahHalamanCheckBox);
            this.AllFakturPage.Controls.Add(this.PrintPerSupplierButton);
            this.AllFakturPage.Controls.Add(this.PrintPerFakturButton);
            this.AllFakturPage.Controls.Add(this.AllFakturGrid);
            this.AllFakturPage.Controls.Add(this.FilterTextBox);
            this.AllFakturPage.Location = new System.Drawing.Point(4, 25);
            this.AllFakturPage.Name = "AllFakturPage";
            this.AllFakturPage.Padding = new System.Windows.Forms.Padding(3);
            this.AllFakturPage.Size = new System.Drawing.Size(880, 425);
            this.AllFakturPage.TabIndex = 0;
            this.AllFakturPage.Text = "All Faktur";
            // 
            // PisahHalamanCheckBox
            // 
            this.PisahHalamanCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PisahHalamanCheckBox.AutoSize = true;
            this.PisahHalamanCheckBox.Location = new System.Drawing.Point(773, 406);
            this.PisahHalamanCheckBox.Name = "PisahHalamanCheckBox";
            this.PisahHalamanCheckBox.Size = new System.Drawing.Size(101, 17);
            this.PisahHalamanCheckBox.TabIndex = 7;
            this.PisahHalamanCheckBox.Text = "Pisah Halaman";
            this.PisahHalamanCheckBox.UseVisualStyleBackColor = true;
            // 
            // PrintPerSupplierButton
            // 
            this.PrintPerSupplierButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintPerSupplierButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.PrintPerSupplierButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PrintPerSupplierButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PrintPerSupplierButton.Location = new System.Drawing.Point(645, 377);
            this.PrintPerSupplierButton.Name = "PrintPerSupplierButton";
            this.PrintPerSupplierButton.Size = new System.Drawing.Size(113, 23);
            this.PrintPerSupplierButton.TabIndex = 6;
            this.PrintPerSupplierButton.Text = "Print Per-Supplier";
            this.PrintPerSupplierButton.UseVisualStyleBackColor = false;
            // 
            // PrintPerFakturButton
            // 
            this.PrintPerFakturButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintPerFakturButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.PrintPerFakturButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PrintPerFakturButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PrintPerFakturButton.Location = new System.Drawing.Point(761, 377);
            this.PrintPerFakturButton.Name = "PrintPerFakturButton";
            this.PrintPerFakturButton.Size = new System.Drawing.Size(113, 23);
            this.PrintPerFakturButton.TabIndex = 5;
            this.PrintPerFakturButton.Text = "Print Per-Faktur";
            this.PrintPerFakturButton.UseVisualStyleBackColor = false;
            // 
            // AllFakturGrid
            // 
            this.AllFakturGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AllFakturGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AllFakturGrid.Location = new System.Drawing.Point(8, 6);
            this.AllFakturGrid.Name = "AllFakturGrid";
            this.AllFakturGrid.Size = new System.Drawing.Size(866, 365);
            this.AllFakturGrid.TabIndex = 1;
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FilterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterTextBox.Location = new System.Drawing.Point(6, 400);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(323, 22);
            this.FilterTextBox.TabIndex = 0;
            // 
            // PerSupplierPage
            // 
            this.PerSupplierPage.Controls.Add(this.PerSupplierFlowPanel);
            this.PerSupplierPage.Location = new System.Drawing.Point(4, 25);
            this.PerSupplierPage.Name = "PerSupplierPage";
            this.PerSupplierPage.Padding = new System.Windows.Forms.Padding(3);
            this.PerSupplierPage.Size = new System.Drawing.Size(880, 425);
            this.PerSupplierPage.TabIndex = 1;
            this.PerSupplierPage.Text = "Per Supplier";
            this.PerSupplierPage.UseVisualStyleBackColor = true;
            // 
            // PerSupplierFlowPanel
            // 
            this.PerSupplierFlowPanel.Controls.Add(this.PerSupplierGrid);
            this.PerSupplierFlowPanel.Location = new System.Drawing.Point(6, 6);
            this.PerSupplierFlowPanel.Name = "PerSupplierFlowPanel";
            this.PerSupplierFlowPanel.Size = new System.Drawing.Size(756, 359);
            this.PerSupplierFlowPanel.TabIndex = 0;
            // 
            // PerSupplierGrid
            // 
            this.PerSupplierGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PerSupplierGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PerSupplierGrid.Location = new System.Drawing.Point(3, 3);
            this.PerSupplierGrid.Name = "PerSupplierGrid";
            this.PerSupplierGrid.Size = new System.Drawing.Size(754, 0);
            this.PerSupplierGrid.TabIndex = 2;
            // 
            // PerFakturPage
            // 
            this.PerFakturPage.Location = new System.Drawing.Point(4, 25);
            this.PerFakturPage.Name = "PerFakturPage";
            this.PerFakturPage.Size = new System.Drawing.Size(880, 425);
            this.PerFakturPage.TabIndex = 2;
            this.PerFakturPage.Text = "Per Faktur";
            this.PerFakturPage.UseVisualStyleBackColor = true;
            // 
            // PrintFakturListButton
            // 
            this.PrintFakturListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintFakturListButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.PrintFakturListButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PrintFakturListButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PrintFakturListButton.Location = new System.Drawing.Point(526, 377);
            this.PrintFakturListButton.Name = "PrintFakturListButton";
            this.PrintFakturListButton.Size = new System.Drawing.Size(113, 23);
            this.PrintFakturListButton.TabIndex = 8;
            this.PrintFakturListButton.Text = "Print Faktur List";
            this.PrintFakturListButton.UseVisualStyleBackColor = false;
            // 
            // PK1PrintPackingOrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(912, 507);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.ListFakturButton);
            this.Controls.Add(this.Tgl2DatePicker);
            this.Controls.Add(this.Tgl1DatePicker);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PK1PrintPackingOrderForm";
            this.Text = "Print Packing Order";
            this.tabControl1.ResumeLayout(false);
            this.AllFakturPage.ResumeLayout(false);
            this.AllFakturPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AllFakturGrid)).EndInit();
            this.PerSupplierPage.ResumeLayout(false);
            this.PerSupplierFlowPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PerSupplierGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DateTimePicker Tgl1DatePicker;
        private System.Windows.Forms.DateTimePicker Tgl2DatePicker;
        private System.Windows.Forms.Button ListFakturButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage AllFakturPage;
        private System.Windows.Forms.DataGridView AllFakturGrid;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.TabPage PerSupplierPage;
        private System.Windows.Forms.TabPage PerFakturPage;
        private System.Windows.Forms.FlowLayoutPanel PerSupplierFlowPanel;
        private System.Windows.Forms.DataGridView PerSupplierGrid;
        private System.Windows.Forms.Button PrintPerFakturButton;
        private System.Windows.Forms.Button PrintPerSupplierButton;
        private System.Windows.Forms.CheckBox PisahHalamanCheckBox;
        private System.Windows.Forms.Button PrintFakturListButton;
    }
}