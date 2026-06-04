namespace j07_btrade_sync
{
    partial class KonfigurasiForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ServerTargetIDText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.DownloadOrderCheckBox = new System.Windows.Forms.CheckBox();
            this.SyncCustomerLocCheckBox = new System.Windows.Forms.CheckBox();
            this.DownloadCheckInCheckBox = new System.Windows.Forms.CheckBox();
            this.UploadPackingOrderCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Beige;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.UploadPackingOrderCheckBox);
            this.panel1.Controls.Add(this.DownloadCheckInCheckBox);
            this.panel1.Controls.Add(this.SyncCustomerLocCheckBox);
            this.panel1.Controls.Add(this.DownloadOrderCheckBox);
            this.panel1.Controls.Add(this.ServerTargetIDText);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(6, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(323, 169);
            this.panel1.TabIndex = 0;
            // 
            // ServerTargetIDText
            // 
            this.ServerTargetIDText.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ServerTargetIDText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ServerTargetIDText.Location = new System.Drawing.Point(16, 38);
            this.ServerTargetIDText.Name = "ServerTargetIDText";
            this.ServerTargetIDText.Size = new System.Drawing.Size(128, 22);
            this.ServerTargetIDText.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Target ID";
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(81)))), ((int)(((byte)(133)))));
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.OKButton.Location = new System.Drawing.Point(173, 182);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = false;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(81)))), ((int)(((byte)(133)))));
            this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CancelButton.Location = new System.Drawing.Point(254, 182);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = false;
            // 
            // DownloadOrderCheckBox
            // 
            this.DownloadOrderCheckBox.AutoSize = true;
            this.DownloadOrderCheckBox.Location = new System.Drawing.Point(16, 70);
            this.DownloadOrderCheckBox.Name = "DownloadOrderCheckBox";
            this.DownloadOrderCheckBox.Size = new System.Drawing.Size(142, 17);
            this.DownloadOrderCheckBox.TabIndex = 2;
            this.DownloadOrderCheckBox.Text = "Download Sales Order";
            this.DownloadOrderCheckBox.UseVisualStyleBackColor = true;
            // 
            // SyncCustomerLocCheckBox
            // 
            this.SyncCustomerLocCheckBox.AutoSize = true;
            this.SyncCustomerLocCheckBox.Location = new System.Drawing.Point(16, 93);
            this.SyncCustomerLocCheckBox.Name = "SyncCustomerLocCheckBox";
            this.SyncCustomerLocCheckBox.Size = new System.Drawing.Size(148, 17);
            this.SyncCustomerLocCheckBox.TabIndex = 3;
            this.SyncCustomerLocCheckBox.Text = "Sync Customer Location";
            this.SyncCustomerLocCheckBox.UseVisualStyleBackColor = true;
            // 
            // DownloadCheckInCheckBox
            // 
            this.DownloadCheckInCheckBox.AutoSize = true;
            this.DownloadCheckInCheckBox.Location = new System.Drawing.Point(164, 70);
            this.DownloadCheckInCheckBox.Name = "DownloadCheckInCheckBox";
            this.DownloadCheckInCheckBox.Size = new System.Drawing.Size(128, 17);
            this.DownloadCheckInCheckBox.TabIndex = 4;
            this.DownloadCheckInCheckBox.Text = "Download Check-In";
            this.DownloadCheckInCheckBox.UseVisualStyleBackColor = true;
            // 
            // UploadPackingOrderCheckBox
            // 
            this.UploadPackingOrderCheckBox.AutoSize = true;
            this.UploadPackingOrderCheckBox.Location = new System.Drawing.Point(164, 93);
            this.UploadPackingOrderCheckBox.Name = "UploadPackingOrderCheckBox";
            this.UploadPackingOrderCheckBox.Size = new System.Drawing.Size(140, 17);
            this.UploadPackingOrderCheckBox.TabIndex = 5;
            this.UploadPackingOrderCheckBox.Text = "Upload Packing Order";
            this.UploadPackingOrderCheckBox.UseVisualStyleBackColor = true;
            // 
            // KonfigurasiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(79)))), ((int)(((byte)(107)))));
            this.ClientSize = new System.Drawing.Size(339, 215);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "KonfigurasiForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KonfigurasiForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox ServerTargetIDText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.CheckBox DownloadOrderCheckBox;
        private System.Windows.Forms.CheckBox SyncCustomerLocCheckBox;
        private System.Windows.Forms.CheckBox DownloadCheckInCheckBox;
        private System.Windows.Forms.CheckBox UploadPackingOrderCheckBox;
    }
}