using System;
using System.Drawing;
using System.Windows.Forms;

namespace PackingOrderDownloader
{
    partial class DL1DownloaderForm
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
            this._headerPanel = new System.Windows.Forms.Panel();
            this._statusLabel = new System.Windows.Forms.Label();
            this._clockLabel = new System.Windows.Forms.Label();
            this._logTextBox = new System.Windows.Forms.RichTextBox();
            this._footerPanel = new System.Windows.Forms.Panel();
            this._manualDownloadButton = new System.Windows.Forms.Button();
            this._headerPanel.SuspendLayout();
            this._footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _headerPanel
            // 
            this._headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this._headerPanel.Controls.Add(this._statusLabel);
            this._headerPanel.Controls.Add(this._clockLabel);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Padding = new System.Windows.Forms.Padding(10);
            this._headerPanel.Size = new System.Drawing.Size(384, 80);
            this._headerPanel.TabIndex = 0;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._statusLabel.ForeColor = System.Drawing.Color.White;
            this._statusLabel.Location = new System.Drawing.Point(10, 45);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(364, 25);
            this._statusLabel.TabIndex = 1;
            this._statusLabel.Text = "Ready";
            this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _clockLabel
            // 
            this._clockLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._clockLabel.Font = new System.Drawing.Font("Courier New", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._clockLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._clockLabel.Location = new System.Drawing.Point(10, 10);
            this._clockLabel.Name = "_clockLabel";
            this._clockLabel.Size = new System.Drawing.Size(364, 35);
            this._clockLabel.TabIndex = 0;
            this._clockLabel.Text = "00:00:00";
            this._clockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _logTextBox
            // 
            this._logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._logTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._logTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._logTextBox.Location = new System.Drawing.Point(0, 80);
            this._logTextBox.Margin = new System.Windows.Forms.Padding(12);
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ReadOnly = true;
            this._logTextBox.Size = new System.Drawing.Size(384, 351);
            this._logTextBox.TabIndex = 1;
            this._logTextBox.Text = "";
            this._logTextBox.WordWrap = false;
            // 
            // _footerPanel
            // 
            this._footerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this._footerPanel.Controls.Add(this._manualDownloadButton);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(0, 431);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Padding = new System.Windows.Forms.Padding(10);
            this._footerPanel.Size = new System.Drawing.Size(384, 60);
            this._footerPanel.TabIndex = 2;
            // 
            // _manualDownloadButton
            // 
            this._manualDownloadButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._manualDownloadButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this._manualDownloadButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._manualDownloadButton.FlatAppearance.BorderSize = 0;
            this._manualDownloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._manualDownloadButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._manualDownloadButton.ForeColor = System.Drawing.Color.White;
            this._manualDownloadButton.Location = new System.Drawing.Point(10, 10);
            this._manualDownloadButton.Name = "_manualDownloadButton";
            this._manualDownloadButton.Size = new System.Drawing.Size(364, 40);
            this._manualDownloadButton.TabIndex = 0;
            this._manualDownloadButton.Text = "Download Now";
            this._manualDownloadButton.UseVisualStyleBackColor = false;
            this._manualDownloadButton.Click += new System.EventHandler(this.ManualDownloadButton_Click);
            // 
            // DownloadPackingOrder2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 491);
            this.Controls.Add(this._logTextBox);
            this.Controls.Add(this._footerPanel);
            this.Controls.Add(this._headerPanel);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "DownloadPackingOrder2Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Packing Order Downloader";
            this._headerPanel.ResumeLayout(false);
            this._footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _headerPanel;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.Label _clockLabel;
        private System.Windows.Forms.RichTextBox _logTextBox;
        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.Button _manualDownloadButton;
    }
}
