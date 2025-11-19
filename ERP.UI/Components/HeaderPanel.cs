using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.UI.UI;

namespace ERP.UI.Components
{
    public class HeaderPanel : Panel
    {
        private Label _titleLabel;
        private Label _userLabel;

        public string Title
        {
            get => _titleLabel?.Text ?? string.Empty;
            set
            {
                if (_titleLabel != null)
                    _titleLabel.Text = value;
            }
        }

        public string UserName
        {
            get => _userLabel?.Text ?? string.Empty;
            set
            {
                if (_userLabel != null)
                    _userLabel.Text = value;
            }
        }

        public HeaderPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Top;
            this.Height = 60;
            this.BackColor = ThemeColors.Primary;
            this.Padding = new Padding(20, 0, 20, 0);

            _titleLabel = new Label
            {
                Text = "ERP/MRP YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.TextOnPrimary,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            _userLabel = new Label
            {
                Text = "Kullanıcı: Admin",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextOnPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            this.Resize += HeaderPanel_Resize;
            this.Controls.Add(_titleLabel);
            this.Controls.Add(_userLabel);
            
            // İlk konumlandırma
            this.PerformLayout();
        }

        private void HeaderPanel_Resize(object sender, EventArgs e)
        {
            if (_userLabel != null)
            {
                _userLabel.Location = new Point(this.Width - _userLabel.Width - 20, 20);
            }
        }
    }
}

