namespace btr.distrib.SalesContext.VisitPlanAgg
{
    partial class JadwalKunjunganForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.PersistenceStatusLabel = new System.Windows.Forms.Label();
            this.CycleLabel = new System.Windows.Forms.Label();
            this.MaterializeButton = new System.Windows.Forms.Button();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.VisitDatePicker = new System.Windows.Forms.DateTimePicker();
            this.VisitDateLabel = new System.Windows.Forms.Label();
            this.SalesComboBox = new System.Windows.Forms.ComboBox();
            this.SalesLabel = new System.Windows.Forms.Label();
            this.MainSplit = new System.Windows.Forms.SplitContainer();
            this.EffectivePlanGrid = new System.Windows.Forms.DataGridView();
            this.EffectivePlanHeaderPanel = new System.Windows.Forms.Panel();
            this.ToggleDrawerButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.SummaryCountLabel = new System.Windows.Forms.Label();
            this.EffectivePlanTitleLabel = new System.Windows.Forms.Label();
            this.AuditTabControl = new System.Windows.Forms.TabControl();
            this.AuditTabPage = new System.Windows.Forms.TabPage();
            this.ExceptionGrid = new System.Windows.Forms.DataGridView();
            this.BasePlanTabPage = new System.Windows.Forms.TabPage();
            this.BasePlanGrid = new System.Windows.Forms.DataGridView();
            this.DrawerHeaderPanel = new System.Windows.Forms.Panel();
            this.CloseDrawerButton = new System.Windows.Forms.Button();
            this.DeleteExceptionButton = new System.Windows.Forms.Button();
            this.DrawerTitleLabel = new System.Windows.Forms.Label();
            this.EffectiveContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuTambahCustomer = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuGantiCustomer = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuHapusCustomer = new System.Windows.Forms.ToolStripMenuItem();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplit)).BeginInit();
            this.MainSplit.Panel1.SuspendLayout();
            this.MainSplit.Panel2.SuspendLayout();
            this.MainSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EffectivePlanGrid)).BeginInit();
            this.EffectivePlanHeaderPanel.SuspendLayout();
            this.AuditTabControl.SuspendLayout();
            this.AuditTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExceptionGrid)).BeginInit();
            this.BasePlanTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BasePlanGrid)).BeginInit();
            this.DrawerHeaderPanel.SuspendLayout();
            this.EffectiveContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            this.TopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TopPanel.Controls.Add(this.PersistenceStatusLabel);
            this.TopPanel.Controls.Add(this.CycleLabel);
            this.TopPanel.Controls.Add(this.MaterializeButton);
            this.TopPanel.Controls.Add(this.RefreshButton);
            this.TopPanel.Controls.Add(this.VisitDatePicker);
            this.TopPanel.Controls.Add(this.VisitDateLabel);
            this.TopPanel.Controls.Add(this.SalesComboBox);
            this.TopPanel.Controls.Add(this.SalesLabel);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(1084, 48);
            this.TopPanel.TabIndex = 0;
            // 
            // PersistenceStatusLabel
            // 
            this.PersistenceStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PersistenceStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PersistenceStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.PersistenceStatusLabel.Location = new System.Drawing.Point(820, 14);
            this.PersistenceStatusLabel.Name = "PersistenceStatusLabel";
            this.PersistenceStatusLabel.Size = new System.Drawing.Size(250, 20);
            this.PersistenceStatusLabel.TabIndex = 7;
            this.PersistenceStatusLabel.Text = "● Tersimpan di server";
            this.PersistenceStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CycleLabel
            // 
            this.CycleLabel.AutoSize = true;
            this.CycleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CycleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(101)))), ((int)(((byte)(192)))));
            this.CycleLabel.Location = new System.Drawing.Point(670, 16);
            this.CycleLabel.Name = "CycleLabel";
            this.CycleLabel.Size = new System.Drawing.Size(15, 20);
            this.CycleLabel.TabIndex = 6;
            this.CycleLabel.Text = "-";
            // 
            // MaterializeButton
            // 
            this.MaterializeButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MaterializeButton.Location = new System.Drawing.Point(555, 9);
            this.MaterializeButton.Name = "MaterializeButton";
            this.MaterializeButton.Size = new System.Drawing.Size(105, 30);
            this.MaterializeButton.TabIndex = 5;
            this.MaterializeButton.Text = "Materialisasi";
            this.MaterializeButton.UseVisualStyleBackColor = true;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.RefreshButton.Location = new System.Drawing.Point(460, 9);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(88, 30);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            // 
            // VisitDatePicker
            // 
            this.VisitDatePicker.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.VisitDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.VisitDatePicker.Location = new System.Drawing.Point(325, 11);
            this.VisitDatePicker.Name = "VisitDatePicker";
            this.VisitDatePicker.Size = new System.Drawing.Size(125, 27);
            this.VisitDatePicker.TabIndex = 3;
            // 
            // VisitDateLabel
            // 
            this.VisitDateLabel.AutoSize = true;
            this.VisitDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.VisitDateLabel.Location = new System.Drawing.Point(285, 14);
            this.VisitDateLabel.Name = "VisitDateLabel";
            this.VisitDateLabel.Size = new System.Drawing.Size(32, 20);
            this.VisitDateLabel.TabIndex = 2;
            this.VisitDateLabel.Text = "Tgl:";
            // 
            // SalesComboBox
            // 
            this.SalesComboBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SalesComboBox.FormattingEnabled = true;
            this.SalesComboBox.Location = new System.Drawing.Point(60, 11);
            this.SalesComboBox.Name = "SalesComboBox";
            this.SalesComboBox.Size = new System.Drawing.Size(215, 28);
            this.SalesComboBox.TabIndex = 1;
            // 
            // SalesLabel
            // 
            this.SalesLabel.AutoSize = true;
            this.SalesLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SalesLabel.Location = new System.Drawing.Point(12, 14);
            this.SalesLabel.Name = "SalesLabel";
            this.SalesLabel.Size = new System.Drawing.Size(46, 20);
            this.SalesLabel.TabIndex = 0;
            this.SalesLabel.Text = "Sales:";
            // 
            // MainSplit
            // 
            this.MainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.MainSplit.Location = new System.Drawing.Point(0, 48);
            this.MainSplit.Name = "MainSplit";
            this.MainSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainSplit.Panel1
            // 
            this.MainSplit.Panel1.Controls.Add(this.EffectivePlanGrid);
            this.MainSplit.Panel1.Controls.Add(this.EffectivePlanHeaderPanel);
            // 
            // MainSplit.Panel2
            // 
            this.MainSplit.Panel2.Controls.Add(this.AuditTabControl);
            this.MainSplit.Panel2.Controls.Add(this.DrawerHeaderPanel);
            this.MainSplit.Panel2Collapsed = true;
            this.MainSplit.Size = new System.Drawing.Size(1084, 613);
            this.MainSplit.SplitterDistance = 410;
            this.MainSplit.SplitterWidth = 6;
            this.MainSplit.TabIndex = 1;
            // 
            // EffectivePlanGrid
            // 
            this.EffectivePlanGrid.AllowUserToAddRows = false;
            this.EffectivePlanGrid.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.EffectivePlanGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.EffectivePlanGrid.BackgroundColor = System.Drawing.Color.White;
            this.EffectivePlanGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.EffectivePlanGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EffectivePlanGrid.ContextMenuStrip = this.EffectiveContextMenu;
            this.EffectivePlanGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EffectivePlanGrid.Location = new System.Drawing.Point(0, 42);
            this.EffectivePlanGrid.MultiSelect = false;
            this.EffectivePlanGrid.Name = "EffectivePlanGrid";
            this.EffectivePlanGrid.ReadOnly = true;
            this.EffectivePlanGrid.RowHeadersWidth = 50;
            this.EffectivePlanGrid.RowTemplate.Height = 26;
            this.EffectivePlanGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.EffectivePlanGrid.Size = new System.Drawing.Size(1084, 571);
            this.EffectivePlanGrid.TabIndex = 1;
            // 
            // EffectivePlanHeaderPanel
            // 
            this.EffectivePlanHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.EffectivePlanHeaderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EffectivePlanHeaderPanel.Controls.Add(this.ToggleDrawerButton);
            this.EffectivePlanHeaderPanel.Controls.Add(this.RemoveButton);
            this.EffectivePlanHeaderPanel.Controls.Add(this.ReplaceButton);
            this.EffectivePlanHeaderPanel.Controls.Add(this.AddButton);
            this.EffectivePlanHeaderPanel.Controls.Add(this.SummaryCountLabel);
            this.EffectivePlanHeaderPanel.Controls.Add(this.EffectivePlanTitleLabel);
            this.EffectivePlanHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EffectivePlanHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.EffectivePlanHeaderPanel.Name = "EffectivePlanHeaderPanel";
            this.EffectivePlanHeaderPanel.Size = new System.Drawing.Size(1084, 42);
            this.EffectivePlanHeaderPanel.TabIndex = 0;
            // 
            // ToggleDrawerButton
            // 
            this.ToggleDrawerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToggleDrawerButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ToggleDrawerButton.Location = new System.Drawing.Point(920, 6);
            this.ToggleDrawerButton.Name = "ToggleDrawerButton";
            this.ToggleDrawerButton.Size = new System.Drawing.Size(150, 28);
            this.ToggleDrawerButton.TabIndex = 5;
            this.ToggleDrawerButton.Text = "Riwayat Exception ▼";
            this.ToggleDrawerButton.UseVisualStyleBackColor = true;
            // 
            // RemoveButton
            // 
            this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.RemoveButton.Location = new System.Drawing.Point(795, 6);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(118, 28);
            this.RemoveButton.TabIndex = 4;
            this.RemoveButton.Text = "Hapus / Lewati";
            this.RemoveButton.UseVisualStyleBackColor = true;
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReplaceButton.Location = new System.Drawing.Point(675, 6);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(114, 28);
            this.ReplaceButton.TabIndex = 3;
            this.ReplaceButton.Text = "Ganti Customer";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            // 
            // AddButton
            // 
            this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(101)))), ((int)(((byte)(192)))));
            this.AddButton.Location = new System.Drawing.Point(525, 6);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(144, 28);
            this.AddButton.TabIndex = 2;
            this.AddButton.Text = "+ Tambah Customer";
            this.AddButton.UseVisualStyleBackColor = true;
            // 
            // SummaryCountLabel
            // 
            this.SummaryCountLabel.AutoSize = true;
            this.SummaryCountLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SummaryCountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
            this.SummaryCountLabel.Location = new System.Drawing.Point(230, 9);
            this.SummaryCountLabel.Name = "SummaryCountLabel";
            this.SummaryCountLabel.Size = new System.Drawing.Size(155, 21);
            this.SummaryCountLabel.TabIndex = 1;
            this.SummaryCountLabel.Text = "Total: 0 Kunjungan";
            // 
            // EffectivePlanTitleLabel
            // 
            this.EffectivePlanTitleLabel.AutoSize = true;
            this.EffectivePlanTitleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EffectivePlanTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.EffectivePlanTitleLabel.Location = new System.Drawing.Point(10, 8);
            this.EffectivePlanTitleLabel.Name = "EffectivePlanTitleLabel";
            this.EffectivePlanTitleLabel.Size = new System.Drawing.Size(206, 23);
            this.EffectivePlanTitleLabel.TabIndex = 0;
            this.EffectivePlanTitleLabel.Text = "Jadwal Kunjungan Efektif";
            // 
            // AuditTabControl
            // 
            this.AuditTabControl.Controls.Add(this.AuditTabPage);
            this.AuditTabControl.Controls.Add(this.BasePlanTabPage);
            this.AuditTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuditTabControl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AuditTabControl.Location = new System.Drawing.Point(0, 36);
            this.AuditTabControl.Name = "AuditTabControl";
            this.AuditTabControl.SelectedIndex = 0;
            this.AuditTabControl.Size = new System.Drawing.Size(1084, 161);
            this.AuditTabControl.TabIndex = 1;
            // 
            // AuditTabPage
            // 
            this.AuditTabPage.Controls.Add(this.ExceptionGrid);
            this.AuditTabPage.Location = new System.Drawing.Point(4, 29);
            this.AuditTabPage.Name = "AuditTabPage";
            this.AuditTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AuditTabPage.Size = new System.Drawing.Size(1076, 128);
            this.AuditTabPage.TabIndex = 0;
            this.AuditTabPage.Text = "Riwayat Exception";
            this.AuditTabPage.UseVisualStyleBackColor = true;
            // 
            // ExceptionGrid
            // 
            this.ExceptionGrid.AllowUserToAddRows = false;
            this.ExceptionGrid.AllowUserToDeleteRows = false;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(249)))), ((int)(((byte)(231)))));
            this.ExceptionGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            this.ExceptionGrid.BackgroundColor = System.Drawing.Color.White;
            this.ExceptionGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ExceptionGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ExceptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExceptionGrid.Location = new System.Drawing.Point(3, 3);
            this.ExceptionGrid.MultiSelect = false;
            this.ExceptionGrid.Name = "ExceptionGrid";
            this.ExceptionGrid.ReadOnly = true;
            this.ExceptionGrid.RowHeadersWidth = 50;
            this.ExceptionGrid.RowTemplate.Height = 24;
            this.ExceptionGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ExceptionGrid.Size = new System.Drawing.Size(1070, 122);
            this.ExceptionGrid.TabIndex = 0;
            // 
            // BasePlanTabPage
            // 
            this.BasePlanTabPage.Controls.Add(this.BasePlanGrid);
            this.BasePlanTabPage.Location = new System.Drawing.Point(4, 29);
            this.BasePlanTabPage.Name = "BasePlanTabPage";
            this.BasePlanTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.BasePlanTabPage.Size = new System.Drawing.Size(1076, 128);
            this.BasePlanTabPage.TabIndex = 1;
            this.BasePlanTabPage.Text = "Master Rute Template";
            this.BasePlanTabPage.UseVisualStyleBackColor = true;
            // 
            // BasePlanGrid
            // 
            this.BasePlanGrid.AllowUserToAddRows = false;
            this.BasePlanGrid.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.BasePlanGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.BasePlanGrid.BackgroundColor = System.Drawing.Color.White;
            this.BasePlanGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BasePlanGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BasePlanGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BasePlanGrid.Location = new System.Drawing.Point(3, 3);
            this.BasePlanGrid.MultiSelect = false;
            this.BasePlanGrid.Name = "BasePlanGrid";
            this.BasePlanGrid.ReadOnly = true;
            this.BasePlanGrid.RowHeadersWidth = 50;
            this.BasePlanGrid.RowTemplate.Height = 24;
            this.BasePlanGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.BasePlanGrid.Size = new System.Drawing.Size(1070, 122);
            this.BasePlanGrid.TabIndex = 0;
            // 
            // DrawerHeaderPanel
            // 
            this.DrawerHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(241)))));
            this.DrawerHeaderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DrawerHeaderPanel.Controls.Add(this.CloseDrawerButton);
            this.DrawerHeaderPanel.Controls.Add(this.DeleteExceptionButton);
            this.DrawerHeaderPanel.Controls.Add(this.DrawerTitleLabel);
            this.DrawerHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.DrawerHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.DrawerHeaderPanel.Name = "DrawerHeaderPanel";
            this.DrawerHeaderPanel.Size = new System.Drawing.Size(1084, 36);
            this.DrawerHeaderPanel.TabIndex = 0;
            // 
            // CloseDrawerButton
            // 
            this.CloseDrawerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseDrawerButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.CloseDrawerButton.Location = new System.Drawing.Point(995, 3);
            this.CloseDrawerButton.Name = "CloseDrawerButton";
            this.CloseDrawerButton.Size = new System.Drawing.Size(75, 28);
            this.CloseDrawerButton.TabIndex = 2;
            this.CloseDrawerButton.Text = "✕ Tutup";
            this.CloseDrawerButton.UseVisualStyleBackColor = true;
            // 
            // DeleteExceptionButton
            // 
            this.DeleteExceptionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteExceptionButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.DeleteExceptionButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.DeleteExceptionButton.Location = new System.Drawing.Point(825, 3);
            this.DeleteExceptionButton.Name = "DeleteExceptionButton";
            this.DeleteExceptionButton.Size = new System.Drawing.Size(164, 28);
            this.DeleteExceptionButton.TabIndex = 1;
            this.DeleteExceptionButton.Text = "Hapus Exception Terpilih";
            this.DeleteExceptionButton.UseVisualStyleBackColor = true;
            // 
            // DrawerTitleLabel
            // 
            this.DrawerTitleLabel.AutoSize = true;
            this.DrawerTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DrawerTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(71)))), ((int)(((byte)(79)))));
            this.DrawerTitleLabel.Location = new System.Drawing.Point(10, 8);
            this.DrawerTitleLabel.Name = "DrawerTitleLabel";
            this.DrawerTitleLabel.Size = new System.Drawing.Size(328, 20);
            this.DrawerTitleLabel.TabIndex = 0;
            this.DrawerTitleLabel.Text = "Detail Pengecualian (Exception) & Master Rute";
            // 
            // EffectiveContextMenu
            // 
            this.EffectiveContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.EffectiveContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuTambahCustomer,
            this.MenuGantiCustomer,
            this.MenuHapusCustomer});
            this.EffectiveContextMenu.Name = "EffectiveContextMenu";
            this.EffectiveContextMenu.Size = new System.Drawing.Size(251, 82);
            // 
            // MenuTambahCustomer
            // 
            this.MenuTambahCustomer.Name = "MenuTambahCustomer";
            this.MenuTambahCustomer.Size = new System.Drawing.Size(250, 26);
            this.MenuTambahCustomer.Text = "+ Tambah Customer...";
            // 
            // MenuGantiCustomer
            // 
            this.MenuGantiCustomer.Name = "MenuGantiCustomer";
            this.MenuGantiCustomer.Size = new System.Drawing.Size(250, 26);
            this.MenuGantiCustomer.Text = "Ganti Customer Ini...";
            // 
            // MenuHapusCustomer
            // 
            this.MenuHapusCustomer.Name = "MenuHapusCustomer";
            this.MenuHapusCustomer.Size = new System.Drawing.Size(250, 26);
            this.MenuHapusCustomer.Text = "Hapus / Lewati Kunjungan Ini";
            // 
            // JadwalKunjunganForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 661);
            this.Controls.Add(this.MainSplit);
            this.Controls.Add(this.TopPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "JadwalKunjunganForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SM7 - Jadwal Kunjungan";
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.MainSplit.Panel1.ResumeLayout(false);
            this.MainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplit)).EndInit();
            this.MainSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EffectivePlanGrid)).EndInit();
            this.EffectivePlanHeaderPanel.ResumeLayout(false);
            this.EffectivePlanHeaderPanel.PerformLayout();
            this.AuditTabControl.ResumeLayout(false);
            this.AuditTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ExceptionGrid)).EndInit();
            this.BasePlanTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BasePlanGrid)).EndInit();
            this.DrawerHeaderPanel.ResumeLayout(false);
            this.DrawerHeaderPanel.PerformLayout();
            this.EffectiveContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Label CycleLabel;
        private System.Windows.Forms.Label PersistenceStatusLabel;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Button MaterializeButton;
        private System.Windows.Forms.DateTimePicker VisitDatePicker;
        private System.Windows.Forms.Label VisitDateLabel;
        private System.Windows.Forms.ComboBox SalesComboBox;
        private System.Windows.Forms.Label SalesLabel;
        private System.Windows.Forms.SplitContainer MainSplit;
        private System.Windows.Forms.Panel EffectivePlanHeaderPanel;
        private System.Windows.Forms.Label EffectivePlanTitleLabel;
        private System.Windows.Forms.Label SummaryCountLabel;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button ReplaceButton;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button ToggleDrawerButton;
        private System.Windows.Forms.DataGridView EffectivePlanGrid;
        private System.Windows.Forms.Panel DrawerHeaderPanel;
        private System.Windows.Forms.Label DrawerTitleLabel;
        private System.Windows.Forms.Button DeleteExceptionButton;
        private System.Windows.Forms.Button CloseDrawerButton;
        private System.Windows.Forms.TabControl AuditTabControl;
        private System.Windows.Forms.TabPage AuditTabPage;
        private System.Windows.Forms.DataGridView ExceptionGrid;
        private System.Windows.Forms.TabPage BasePlanTabPage;
        private System.Windows.Forms.DataGridView BasePlanGrid;
        private System.Windows.Forms.ContextMenuStrip EffectiveContextMenu;
        private System.Windows.Forms.ToolStripMenuItem MenuTambahCustomer;
        private System.Windows.Forms.ToolStripMenuItem MenuGantiCustomer;
        private System.Windows.Forms.ToolStripMenuItem MenuHapusCustomer;
    }
}
