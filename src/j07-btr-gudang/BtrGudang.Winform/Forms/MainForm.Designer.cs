namespace BtrGudang.Winform
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.packingOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PT1DownloadPackingOrderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.pT2InfoPackingOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.PK1PrintPackingOrderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.packingOrderToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(939, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // packingOrderToolStripMenuItem
            // 
            this.packingOrderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PT1DownloadPackingOrderMenu,
            this.pT2InfoPackingOrderToolStripMenuItem,
            this.toolStripMenuItem1,
            this.PK1PrintPackingOrderMenu});
            this.packingOrderToolStripMenuItem.Name = "packingOrderToolStripMenuItem";
            this.packingOrderToolStripMenuItem.Size = new System.Drawing.Size(94, 20);
            this.packingOrderToolStripMenuItem.Text = "Packing Order";
            // 
            // PT1DownloadPackingOrderMenu
            // 
            this.PT1DownloadPackingOrderMenu.Name = "PT1DownloadPackingOrderMenu";
            this.PT1DownloadPackingOrderMenu.Size = new System.Drawing.Size(217, 22);
            this.PT1DownloadPackingOrderMenu.Text = "DL1 - Downloader...";
            this.PT1DownloadPackingOrderMenu.Click += new System.EventHandler(this.PT1DownloadPackingOrderMenu_Click);
            // 
            // pT2InfoPackingOrderToolStripMenuItem
            // 
            this.pT2InfoPackingOrderToolStripMenuItem.Name = "pT2InfoPackingOrderToolStripMenuItem";
            this.pT2InfoPackingOrderToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.pT2InfoPackingOrderToolStripMenuItem.Text = "DL2 - Info Download...";
            this.pT2InfoPackingOrderToolStripMenuItem.Click += new System.EventHandler(this.pT2InfoPackingOrderToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(214, 6);
            // 
            // PK1PrintPackingOrderMenu
            // 
            this.PK1PrintPackingOrderMenu.Name = "PK1PrintPackingOrderMenu";
            this.PK1PrintPackingOrderMenu.Size = new System.Drawing.Size(217, 22);
            this.PK1PrintPackingOrderMenu.Text = "PK1 - Print Packing Order...";
            this.PK1PrintPackingOrderMenu.Click += new System.EventHandler(this.PK1PrintPackingOrderMenu_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::BtrGudang.Winform.Properties.Resources.btr_gudang_background_v2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(939, 626);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "BTR Gudang App";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem packingOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PT1DownloadPackingOrderMenu;
        private System.Windows.Forms.ToolStripMenuItem pT2InfoPackingOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem PK1PrintPackingOrderMenu;
    }
}

