using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    partial class DL2DownloadPackingOrderInfoForm
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

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.Periode1DatePicker = new System.Windows.Forms.DateTimePicker();
            this.LoadDataButton = new System.Windows.Forms.Button();
            this.ExportCsvButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.lblFilterInfo = new System.Windows.Forms.Label();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblRecordCount = new System.Windows.Forms.Label();
            this.PackingOrderGrid = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackingOrderGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelTop.Controls.Add(this.Periode1DatePicker);
            this.panelTop.Controls.Add(this.LoadDataButton);
            this.panelTop.Controls.Add(this.ExportCsvButton);
            this.panelTop.Controls.Add(this.CloseButton);
            this.panelTop.Controls.Add(this.lblFilterInfo);
            this.panelTop.Controls.Add(this.FilterTextBox);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(944, 66);
            this.panelTop.TabIndex = 0;
            // 
            // Periode1DatePicker
            // 
            this.Periode1DatePicker.CustomFormat = "ddd, dd-MMM-yyyy";
            this.Periode1DatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.Periode1DatePicker.Location = new System.Drawing.Point(13, 15);
            this.Periode1DatePicker.Name = "Periode1DatePicker";
            this.Periode1DatePicker.Size = new System.Drawing.Size(138, 23);
            this.Periode1DatePicker.TabIndex = 11;
            // 
            // LoadDataButton
            // 
            this.LoadDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadDataButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.LoadDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LoadDataButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadDataButton.ForeColor = System.Drawing.Color.White;
            this.LoadDataButton.Location = new System.Drawing.Point(655, 13);
            this.LoadDataButton.Name = "LoadDataButton";
            this.LoadDataButton.Size = new System.Drawing.Size(88, 24);
            this.LoadDataButton.TabIndex = 6;
            this.LoadDataButton.Text = "Load Data";
            this.LoadDataButton.UseVisualStyleBackColor = false;
            // 
            // ExportCsvButton
            // 
            this.ExportCsvButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportCsvButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.ExportCsvButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExportCsvButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportCsvButton.ForeColor = System.Drawing.Color.White;
            this.ExportCsvButton.Location = new System.Drawing.Point(749, 13);
            this.ExportCsvButton.Name = "ExportCsvButton";
            this.ExportCsvButton.Size = new System.Drawing.Size(88, 24);
            this.ExportCsvButton.TabIndex = 9;
            this.ExportCsvButton.Text = "Export to CSV";
            this.ExportCsvButton.UseVisualStyleBackColor = false;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseButton.ForeColor = System.Drawing.Color.White;
            this.CloseButton.Location = new System.Drawing.Point(843, 13);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(88, 24);
            this.CloseButton.TabIndex = 10;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = false;
            // 
            // lblFilterInfo
            // 
            this.lblFilterInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblFilterInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblFilterInfo.Location = new System.Drawing.Point(293, 41);
            this.lblFilterInfo.Name = "lblFilterInfo";
            this.lblFilterInfo.Size = new System.Drawing.Size(356, 15);
            this.lblFilterInfo.TabIndex = 5;
            this.lblFilterInfo.Text = "Search by Customer Name, Customer Code, Faktur Code, or Address";
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FilterTextBox.Location = new System.Drawing.Point(157, 13);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(492, 25);
            this.FilterTextBox.TabIndex = 4;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.lblStatus);
            this.panelBottom.Controls.Add(this.lblRecordCount);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 494);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(944, 30);
            this.panelBottom.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.lblStatus.Location = new System.Drawing.Point(644, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblStatus.Size = new System.Drawing.Size(298, 28);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRecordCount
            // 
            this.lblRecordCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblRecordCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecordCount.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblRecordCount.Location = new System.Drawing.Point(0, 0);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblRecordCount.Size = new System.Drawing.Size(300, 28);
            this.lblRecordCount.TabIndex = 0;
            this.lblRecordCount.Text = "Total Records: 0 | Filtered: 0";
            this.lblRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PackingOrderGrid
            // 
            this.PackingOrderGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PackingOrderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PackingOrderGrid.Location = new System.Drawing.Point(0, 66);
            this.PackingOrderGrid.Name = "PackingOrderGrid";
            this.PackingOrderGrid.Size = new System.Drawing.Size(944, 428);
            this.PackingOrderGrid.TabIndex = 3;
            // 
            // PT1DownloadPackingOrderInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(944, 524);
            this.Controls.Add(this.PackingOrderGrid);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "PT1DownloadPackingOrderInfoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PT1 Download Packing Order Info";
            this.Load += new System.EventHandler(this.PT1DownloadPackingOrderInfoForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PackingOrderGrid)).EndInit();
            this.ResumeLayout(false);

        }

        private Panel panelTop;
        private Panel panelBottom;
        private Label lblRecordCount;
        private Label lblStatus;

        #endregion
        private Button LoadDataButton;
        private Button ExportCsvButton;
        private Button CloseButton;
        private Label lblFilterInfo;
        private TextBox FilterTextBox;
        private DateTimePicker Periode1DatePicker;
        private DataGridView PackingOrderGrid;
    }
}