using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BtrReturGudang.Winform
{
    /// <summary>
    /// Centralized UI Theme and styling system for BTR Retur Gudang Windows Forms.
    /// Provides consistent enterprise color palettes, typography, button styles, card containers,
    /// and grid formatting following modern desktop UI design standards.
    /// </summary>
    public static class UiTheme
    {
        #region Color Palette (Tailwind-inspired enterprise palette)

        // Slate Neutrals
        public static readonly Color Slate900 = Color.FromArgb(15, 23, 42);   // Primary Dark Headers / Bars
        public static readonly Color Slate850 = Color.FromArgb(24, 33, 47);   // Sidebar background
        public static readonly Color Slate800 = Color.FromArgb(30, 41, 59);   // Dark cards / badges
        public static readonly Color Slate700 = Color.FromArgb(51, 65, 85);   // Dark borders / hover
        public static readonly Color Slate600 = Color.FromArgb(71, 85, 105);   // Secondary text
        public static readonly Color Slate500 = Color.FromArgb(100, 116, 139); // Subtitles / captions
        public static readonly Color Slate400 = Color.FromArgb(148, 163, 184); // Muted text / placeholders
        public static readonly Color Slate300 = Color.FromArgb(203, 213, 225); // Subtle borders
        public static readonly Color Slate200 = Color.FromArgb(226, 232, 240); // Card borders / dividers
        public static readonly Color Slate100 = Color.FromArgb(241, 245, 249); // Form & view background
        public static readonly Color Slate50  = Color.FromArgb(248, 250, 252); // Card surface / zebra row
        public static readonly Color White    = Color.White;

        // Primary Blue
        public static readonly Color Primary       = Color.FromArgb(37, 99, 235);  // Blue-600
        public static readonly Color PrimaryHover  = Color.FromArgb(29, 78, 216);  // Blue-700
        public static readonly Color PrimaryLight  = Color.FromArgb(239, 246, 255); // Blue-50
        public static readonly Color PrimaryBorder = Color.FromArgb(191, 219, 254); // Blue-200

        // Emerald / Success / Bagus / Synced
        public static readonly Color EmeraldDark   = Color.FromArgb(5, 150, 105);  // Emerald-600
        public static readonly Color Emerald       = Color.FromArgb(16, 185, 129); // Emerald-500
        public static readonly Color EmeraldLight  = Color.FromArgb(236, 253, 245); // Emerald-50
        public static readonly Color EmeraldBorder = Color.FromArgb(167, 243, 208); // Emerald-200
        public static readonly Color EmeraldText   = Color.FromArgb(6, 95, 70);    // Emerald-800

        // Rose / Danger / Rusak / Locked
        public static readonly Color RoseDark   = Color.FromArgb(190, 18, 60);  // Rose-700
        public static readonly Color Rose       = Color.FromArgb(225, 29, 72);  // Rose-600
        public static readonly Color RoseLight  = Color.FromArgb(255, 241, 242); // Rose-50
        public static readonly Color RoseBorder = Color.FromArgb(254, 205, 211); // Rose-200
        public static readonly Color RoseText   = Color.FromArgb(159, 18, 57);   // Rose-800

        // Amber / Warning / Draft / Fallback Search
        public static readonly Color AmberDark   = Color.FromArgb(180, 83, 9);   // Amber-700
        public static readonly Color Amber       = Color.FromArgb(217, 119, 6);  // Amber-600
        public static readonly Color AmberLight  = Color.FromArgb(254, 243, 199); // Amber-100
        public static readonly Color AmberBorder = Color.FromArgb(253, 230, 138); // Amber-200
        public static readonly Color AmberText   = Color.FromArgb(146, 64, 14);  // Amber-800

        #endregion

        #region Fonts

        public static readonly Font FontAppTitle      = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontPageTitle     = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontSectionTitle  = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontCardTitle     = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontSubtitle      = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontLabel         = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontBody          = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontBodyBold      = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontMonospace     = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontMonospaceBold = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontKpiValue      = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font FontBadge         = new Font("Segoe UI Bold", 8F, FontStyle.Bold, GraphicsUnit.Point);

        #endregion

        #region Card & Panel Styling

        /// <summary>
        /// Creates a modern card panel with crisp 1px borders and padding.
        /// </summary>
        public static Panel CreateCard(Padding? padding = null, Padding? margin = null)
        {
            var pnl = new Panel
            {
                BackColor = White,
                Padding = padding ?? new Padding(16),
                Margin = margin ?? new Padding(0, 0, 0, 12)
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Slate200, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };
            return pnl;
        }

        #endregion

        #region Button Styling

        public static void ApplyPrimaryButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Primary;
            btn.ForeColor = White;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplySuccessButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Emerald;
            btn.ForeColor = White;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyDangerButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Rose;
            btn.ForeColor = White;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyWarningButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Amber;
            btn.ForeColor = White;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplySecondaryButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Slate700;
            btn.ForeColor = White;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyOutlineButton(Button btn, int height = 34)
        {
            btn.Height = height;
            btn.BackColor = White;
            btn.ForeColor = Slate700;
            btn.Font = FontLabel;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Slate300;
            btn.FlatAppearance.BorderSize = 1;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyGhostButton(Button btn, int height = 36)
        {
            btn.Height = height;
            btn.BackColor = Slate800;
            btn.ForeColor = Slate300;
            btn.Font = FontBodyBold;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        #endregion

        #region DataGridView Styling

        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = Slate200;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Slate100;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Slate700;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontLabel;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.DefaultCellStyle.Font = FontBody;
            dgv.DefaultCellStyle.ForeColor = Slate900;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255); // Indigo-100
            dgv.DefaultCellStyle.SelectionForeColor = Slate900;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgv.RowTemplate.Height = 34;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Slate50;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Slate900;
        }

        #endregion
    }
}
