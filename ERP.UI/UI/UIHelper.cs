using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ERP.UI.UI
{
    public static class UIHelper
    {
        public static void ApplyCardStyle(Panel panel, int borderRadius = 8)
        {
            panel.Paint += (sender, e) =>
            {
                var pnl = sender as Panel;
                if (pnl == null) return;

                using (var path = GetRoundedRectanglePath(pnl.ClientRectangle, borderRadius))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(pnl.BackColor), path);
                    
                    // Gölge efekti
                    using (var shadowPath = GetRoundedRectanglePath(
                        new Rectangle(pnl.ClientRectangle.X + 2, pnl.ClientRectangle.Y + 2, 
                                     pnl.ClientRectangle.Width, pnl.ClientRectangle.Height), 
                        borderRadius))
                    {
                        using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                        {
                            e.Graphics.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }
            };
        }

        public static void ApplyRoundedButton(Button button, int borderRadius = 6)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = button.BackColor;
            button.FlatAppearance.MouseDownBackColor = button.BackColor;
        }

        private static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        public static Panel CreateCardPanel(string title, string description, Color iconColor, int width = 280, int height = 150)
        {
            var card = new Panel
            {
                Width = width,
                Height = height,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(20)
            };

            ApplyCardStyle(card, 12);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var descLabel = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 50),
                MaximumSize = new Size(width - 40, 0)
            };

            card.Controls.Add(titleLabel);
            card.Controls.Add(descLabel);

            return card;
        }
    }
}

