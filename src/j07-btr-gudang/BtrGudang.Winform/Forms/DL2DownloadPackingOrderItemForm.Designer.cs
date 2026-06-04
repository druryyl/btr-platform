namespace BtrGudang.Winform.Forms
{
    partial class DL2DownloadPackingOrderItemForm
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
            this.PackingOrderItemGrid = new System.Windows.Forms.DataGridView();
            this.FakturCodeLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.FakturDateLabel = new System.Windows.Forms.Label();
            this.CustomerNameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PackingOrderItemGrid)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PackingOrderItemGrid
            // 
            this.PackingOrderItemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackingOrderItemGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PackingOrderItemGrid.Location = new System.Drawing.Point(6, 40);
            this.PackingOrderItemGrid.Name = "PackingOrderItemGrid";
            this.PackingOrderItemGrid.Size = new System.Drawing.Size(640, 323);
            this.PackingOrderItemGrid.TabIndex = 0;
            // 
            // FakturCodeLabel
            // 
            this.FakturCodeLabel.AutoSize = true;
            this.FakturCodeLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FakturCodeLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FakturCodeLabel.Location = new System.Drawing.Point(3, 0);
            this.FakturCodeLabel.Name = "FakturCodeLabel";
            this.FakturCodeLabel.Size = new System.Drawing.Size(103, 21);
            this.FakturCodeLabel.TabIndex = 1;
            this.FakturCodeLabel.Text = "[Faktur Code]";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flowLayoutPanel1.Controls.Add(this.FakturCodeLabel);
            this.flowLayoutPanel1.Controls.Add(this.FakturDateLabel);
            this.flowLayoutPanel1.Controls.Add(this.CustomerNameLabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(628, 25);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // FakturDateLabel
            // 
            this.FakturDateLabel.AutoSize = true;
            this.FakturDateLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FakturDateLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FakturDateLabel.Location = new System.Drawing.Point(112, 0);
            this.FakturDateLabel.Name = "FakturDateLabel";
            this.FakturDateLabel.Size = new System.Drawing.Size(99, 21);
            this.FakturDateLabel.TabIndex = 2;
            this.FakturDateLabel.Text = "[Faktur Date]";
            // 
            // CustomerNameLabel
            // 
            this.CustomerNameLabel.AutoSize = true;
            this.CustomerNameLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CustomerNameLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CustomerNameLabel.Location = new System.Drawing.Point(217, 0);
            this.CustomerNameLabel.Name = "CustomerNameLabel";
            this.CustomerNameLabel.Size = new System.Drawing.Size(145, 21);
            this.CustomerNameLabel.TabIndex = 3;
            this.CustomerNameLabel.Text = "[Customer Name]";
            // 
            // DL2DownloadPackingOrderItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(652, 370);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.PackingOrderItemGrid);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DL2DownloadPackingOrderItemForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Faktur Detail Barang";
            ((System.ComponentModel.ISupportInitialize)(this.PackingOrderItemGrid)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView PackingOrderItemGrid;
        private System.Windows.Forms.Label FakturCodeLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label FakturDateLabel;
        private System.Windows.Forms.Label CustomerNameLabel;
    }
}