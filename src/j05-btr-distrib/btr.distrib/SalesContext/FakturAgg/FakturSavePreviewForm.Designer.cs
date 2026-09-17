namespace btr.distrib.SalesContext.FakturAgg
{
    partial class FakturSavePreviewForm
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
            this.TheViewer = new Microsoft.Reporting.WinForms.ReportViewer();
            this.PanelBottom = new System.Windows.Forms.Panel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.SavePrintButton = new System.Windows.Forms.Button();
            this.CancelPreviewButton = new System.Windows.Forms.Button();
            this.PanelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // TheViewer
            //
            this.TheViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TheViewer.Location = new System.Drawing.Point(0, 0);
            this.TheViewer.Name = "TheViewer";
            this.TheViewer.ServerReport.BearerToken = null;
            this.TheViewer.Size = new System.Drawing.Size(800, 405);
            this.TheViewer.TabIndex = 0;
            //
            // PanelBottom
            //
            this.PanelBottom.Controls.Add(this.SaveButton);
            this.PanelBottom.Controls.Add(this.SavePrintButton);
            this.PanelBottom.Controls.Add(this.CancelPreviewButton);
            this.PanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelBottom.Location = new System.Drawing.Point(0, 405);
            this.PanelBottom.Name = "PanelBottom";
            this.PanelBottom.Size = new System.Drawing.Size(800, 45);
            this.PanelBottom.TabIndex = 1;
            //
            // SaveButton
            //
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(536, 11);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "SAVE";
            this.SaveButton.UseVisualStyleBackColor = true;
            //
            // SavePrintButton
            //
            this.SavePrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SavePrintButton.Location = new System.Drawing.Point(617, 11);
            this.SavePrintButton.Name = "SavePrintButton";
            this.SavePrintButton.Size = new System.Drawing.Size(95, 23);
            this.SavePrintButton.TabIndex = 1;
            this.SavePrintButton.Text = "SAVE && PRINT";
            this.SavePrintButton.UseVisualStyleBackColor = true;
            //
            // CancelPreviewButton
            //
            this.CancelPreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelPreviewButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelPreviewButton.Location = new System.Drawing.Point(718, 11);
            this.CancelPreviewButton.Name = "CancelPreviewButton";
            this.CancelPreviewButton.Size = new System.Drawing.Size(70, 23);
            this.CancelPreviewButton.TabIndex = 2;
            this.CancelPreviewButton.Text = "Cancel";
            this.CancelPreviewButton.UseVisualStyleBackColor = true;
            //
            // FakturSavePreviewForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelPreviewButton;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TheViewer);
            this.Controls.Add(this.PanelBottom);
            this.Name = "FakturSavePreviewForm";
            this.Text = "FakturSavePreviewForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.PanelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer TheViewer;
        private System.Windows.Forms.Panel PanelBottom;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button SavePrintButton;
        private System.Windows.Forms.Button CancelPreviewButton;
    }
}
