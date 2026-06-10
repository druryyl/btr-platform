namespace btr.distrib.SalesContext.SalesPersonPrincipalTargetAgg
{
    partial class SalesPersonPrincipalTargetForm
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
            this.periodPanel = new System.Windows.Forms.Panel();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CopyAllRepsButton = new System.Windows.Forms.Button();
            this.CopyPrevMonthButton = new System.Windows.Forms.Button();
            this.MonthCombo = new System.Windows.Forms.ComboBox();
            this.MonthLabel = new System.Windows.Forms.Label();
            this.YearCombo = new System.Windows.Forms.ComboBox();
            this.YearLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.topPanel = new System.Windows.Forms.Panel();
            this.SearchButton = new System.Windows.Forms.Button();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.SalesPersonGrid = new System.Windows.Forms.DataGridView();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.CompletenessLabel = new System.Windows.Forms.Label();
            this.SelectedLabel = new System.Windows.Forms.Label();
            this.PrincipalGrid = new System.Windows.Forms.DataGridView();
            this.periodPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SalesPersonGrid)).BeginInit();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // periodPanel
            // 
            this.periodPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            this.periodPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.periodPanel.Controls.Add(this.RefreshButton);
            this.periodPanel.Controls.Add(this.SaveButton);
            this.periodPanel.Controls.Add(this.CopyAllRepsButton);
            this.periodPanel.Controls.Add(this.CopyPrevMonthButton);
            this.periodPanel.Controls.Add(this.MonthCombo);
            this.periodPanel.Controls.Add(this.MonthLabel);
            this.periodPanel.Controls.Add(this.YearCombo);
            this.periodPanel.Controls.Add(this.YearLabel);
            this.periodPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.periodPanel.Location = new System.Drawing.Point(0, 0);
            this.periodPanel.Name = "periodPanel";
            this.periodPanel.Size = new System.Drawing.Size(884, 40);
            this.periodPanel.TabIndex = 0;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.RefreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RefreshButton.Location = new System.Drawing.Point(797, 8);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(75, 23);
            this.RefreshButton.TabIndex = 7;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = false;
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveButton.Location = new System.Drawing.Point(716, 8);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = false;
            // 
            // CopyAllRepsButton
            // 
            this.CopyAllRepsButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.CopyAllRepsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CopyAllRepsButton.Location = new System.Drawing.Point(430, 8);
            this.CopyAllRepsButton.Name = "CopyAllRepsButton";
            this.CopyAllRepsButton.Size = new System.Drawing.Size(120, 23);
            this.CopyAllRepsButton.TabIndex = 5;
            this.CopyAllRepsButton.Text = "Copy All Reps";
            this.CopyAllRepsButton.UseVisualStyleBackColor = false;
            // 
            // CopyPrevMonthButton
            // 
            this.CopyPrevMonthButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.CopyPrevMonthButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CopyPrevMonthButton.Location = new System.Drawing.Point(300, 8);
            this.CopyPrevMonthButton.Name = "CopyPrevMonthButton";
            this.CopyPrevMonthButton.Size = new System.Drawing.Size(124, 23);
            this.CopyPrevMonthButton.TabIndex = 4;
            this.CopyPrevMonthButton.Text = "Copy Prev Month";
            this.CopyPrevMonthButton.UseVisualStyleBackColor = false;
            // 
            // MonthCombo
            // 
            this.MonthCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MonthCombo.FormattingEnabled = true;
            this.MonthCombo.Location = new System.Drawing.Point(200, 9);
            this.MonthCombo.Name = "MonthCombo";
            this.MonthCombo.Size = new System.Drawing.Size(90, 21);
            this.MonthCombo.TabIndex = 3;
            // 
            // MonthLabel
            // 
            this.MonthLabel.AutoSize = true;
            this.MonthLabel.Location = new System.Drawing.Point(155, 12);
            this.MonthLabel.Name = "MonthLabel";
            this.MonthLabel.Size = new System.Drawing.Size(40, 13);
            this.MonthLabel.TabIndex = 2;
            this.MonthLabel.Text = "Month:";
            // 
            // YearCombo
            // 
            this.YearCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.YearCombo.FormattingEnabled = true;
            this.YearCombo.Location = new System.Drawing.Point(48, 9);
            this.YearCombo.Name = "YearCombo";
            this.YearCombo.Size = new System.Drawing.Size(90, 21);
            this.YearCombo.TabIndex = 1;
            // 
            // YearLabel
            // 
            this.YearLabel.AutoSize = true;
            this.YearLabel.Location = new System.Drawing.Point(8, 12);
            this.YearLabel.Name = "YearLabel";
            this.YearLabel.Size = new System.Drawing.Size(32, 13);
            this.YearLabel.TabIndex = 0;
            this.YearLabel.Text = "Year:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 40);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.topPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.bottomPanel);
            this.splitContainer1.Size = new System.Drawing.Size(884, 581);
            this.splitContainer1.SplitterDistance = 260;
            this.splitContainer1.TabIndex = 1;
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topPanel.Controls.Add(this.SearchButton);
            this.topPanel.Controls.Add(this.SearchText);
            this.topPanel.Controls.Add(this.SalesPersonGrid);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(884, 260);
            this.topPanel.TabIndex = 0;
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SearchButton.Location = new System.Drawing.Point(797, 8);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(75, 23);
            this.SearchButton.TabIndex = 2;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = false;
            // 
            // SearchText
            // 
            this.SearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchText.Location = new System.Drawing.Point(8, 8);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(783, 22);
            this.SearchText.TabIndex = 1;
            // 
            // SalesPersonGrid
            // 
            this.SalesPersonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SalesPersonGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SalesPersonGrid.Location = new System.Drawing.Point(8, 37);
            this.SalesPersonGrid.Name = "SalesPersonGrid";
            this.SalesPersonGrid.ReadOnly = true;
            this.SalesPersonGrid.Size = new System.Drawing.Size(864, 213);
            this.SalesPersonGrid.TabIndex = 0;
            // 
            // bottomPanel
            // 
            this.bottomPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            this.bottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bottomPanel.Controls.Add(this.TotalLabel);
            this.bottomPanel.Controls.Add(this.CompletenessLabel);
            this.bottomPanel.Controls.Add(this.SelectedLabel);
            this.bottomPanel.Controls.Add(this.PrincipalGrid);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(0, 0);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(884, 317);
            this.bottomPanel.TabIndex = 0;
            // 
            // TotalLabel
            // 
            this.TotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TotalLabel.AutoSize = true;
            this.TotalLabel.Location = new System.Drawing.Point(8, 295);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(37, 13);
            this.TotalLabel.TabIndex = 3;
            this.TotalLabel.Text = "Total:";
            // 
            // CompletenessLabel
            // 
            this.CompletenessLabel.AutoSize = true;
            this.CompletenessLabel.Location = new System.Drawing.Point(8, 28);
            this.CompletenessLabel.Name = "CompletenessLabel";
            this.CompletenessLabel.Size = new System.Drawing.Size(78, 13);
            this.CompletenessLabel.TabIndex = 2;
            this.CompletenessLabel.Text = "Completeness:";
            // 
            // SelectedLabel
            // 
            this.SelectedLabel.AutoSize = true;
            this.SelectedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.SelectedLabel.Location = new System.Drawing.Point(8, 8);
            this.SelectedLabel.Name = "SelectedLabel";
            this.SelectedLabel.Size = new System.Drawing.Size(61, 13);
            this.SelectedLabel.TabIndex = 1;
            this.SelectedLabel.Text = "Selected:";
            // 
            // PrincipalGrid
            // 
            this.PrincipalGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PrincipalGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PrincipalGrid.Location = new System.Drawing.Point(8, 48);
            this.PrincipalGrid.Name = "PrincipalGrid";
            this.PrincipalGrid.Size = new System.Drawing.Size(864, 240);
            this.PrincipalGrid.TabIndex = 0;
            // 
            // SalesPersonPrincipalTargetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 621);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.periodPanel);
            this.Name = "SalesPersonPrincipalTargetForm";
            this.Text = "SM6-Principal Target";
            this.periodPanel.ResumeLayout(false);
            this.periodPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SalesPersonGrid)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel periodPanel;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CopyAllRepsButton;
        private System.Windows.Forms.Button CopyPrevMonthButton;
        private System.Windows.Forms.ComboBox MonthCombo;
        private System.Windows.Forms.Label MonthLabel;
        private System.Windows.Forms.ComboBox YearCombo;
        private System.Windows.Forms.Label YearLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TextBox SearchText;
        private System.Windows.Forms.DataGridView SalesPersonGrid;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label TotalLabel;
        private System.Windows.Forms.Label CompletenessLabel;
        private System.Windows.Forms.Label SelectedLabel;
        private System.Windows.Forms.DataGridView PrincipalGrid;
    }
}
