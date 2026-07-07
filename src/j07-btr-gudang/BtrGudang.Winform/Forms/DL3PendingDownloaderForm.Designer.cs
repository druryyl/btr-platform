using System;
using System.Drawing;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    partial class DL3PendingDownloaderForm
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

        private void InitializeComponent()
        {
            this._headerPanel = new System.Windows.Forms.Panel();
            this._statusLabel = new System.Windows.Forms.Label();
            this._titleLabel = new System.Windows.Forms.Label();
            this._logTextBox = new System.Windows.Forms.RichTextBox();
            this._footerPanel = new System.Windows.Forms.Panel();
            this._downloadPendingButton = new System.Windows.Forms.Button();
            this._headerPanel.SuspendLayout();
            this._footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _headerPanel
            // 
            this._headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this._headerPanel.Controls.Add(this._statusLabel);
            this._headerPanel.Controls.Add(this._titleLabel);
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
            // _titleLabel
            // 
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._titleLabel.Location = new System.Drawing.Point(10, 10);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new System.Drawing.Size(364, 35);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "Pending Faktur Recovery";
            this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this._footerPanel.Controls.Add(this._downloadPendingButton);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(0, 431);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Padding = new System.Windows.Forms.Padding(10);
            this._footerPanel.Size = new System.Drawing.Size(384, 60);
            this._footerPanel.TabIndex = 2;
            // 
            // _downloadPendingButton
            // 
            this._downloadPendingButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._downloadPendingButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this._downloadPendingButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._downloadPendingButton.FlatAppearance.BorderSize = 0;
            this._downloadPendingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._downloadPendingButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._downloadPendingButton.ForeColor = System.Drawing.Color.White;
            this._downloadPendingButton.Location = new System.Drawing.Point(10, 10);
            this._downloadPendingButton.Name = "_downloadPendingButton";
            this._downloadPendingButton.Size = new System.Drawing.Size(364, 40);
            this._downloadPendingButton.TabIndex = 0;
            this._downloadPendingButton.Text = "Download Pending Faktur";
            this._downloadPendingButton.UseVisualStyleBackColor = false;
            this._downloadPendingButton.Click += new System.EventHandler(this.DownloadPendingButton_Click);
            // 
            // DL3PendingDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 491);
            this.Controls.Add(this._logTextBox);
            this.Controls.Add(this._footerPanel);
            this.Controls.Add(this._headerPanel);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "DL3PendingDownloaderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pending Faktur Recovery";
            this._headerPanel.ResumeLayout(false);
            this._footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel _headerPanel;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.RichTextBox _logTextBox;
        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.Button _downloadPendingButton;
    }
}
