using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.Models;
using ERP.UI.UI;

namespace ERP.UI.Components
{
    public class SubMenuPanel : Panel
    {
        private Panel _subMenuContainer;
        private bool _isExpanded = false;

        public event EventHandler<string> SubMenuItemClicked;

        public SubMenuPanel(MenuItem parentItem)
        {
            InitializeComponent();
            CreateSubMenu(parentItem);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Top;
            this.BackColor = ThemeColors.MenuBackground;
            this.AutoSize = false;
            this.Height = 0;
        }

        private void CreateSubMenu(MenuItem parentItem)
        {
            if (!parentItem.HasSubMenu)
                return;

            _subMenuContainer = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(45, 58, 66),
                Padding = new Padding(0)
            };

            foreach (var subItem in parentItem.SubMenuItems.OrderBy(x => x.Order))
            {
                var subButton = CreateSubMenuButton(subItem);
                _subMenuContainer.Controls.Add(subButton);
            }

            this.Controls.Add(_subMenuContainer);
            this.Height = 0; // Başlangıçta kapalı
        }

        private Button CreateSubMenuButton(MenuItem menuItem)
        {
            var button = new Button
            {
                Text = "  " + menuItem.Text,
                Tag = menuItem.Tag,
                Dock = DockStyle.Top,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(40, 0, 0, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeColors.MenuText,
                BackColor = Color.FromArgb(45, 58, 66),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.MenuHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.MenuSelected;

            button.Click += (s, e) => SubMenuItemClicked?.Invoke(this, menuItem.Tag);

            return button;
        }

        public void Toggle()
        {
            _isExpanded = !_isExpanded;
            this.Height = _isExpanded ? _subMenuContainer?.Height ?? 0 : 0;
        }

        public void Expand()
        {
            if (!_isExpanded)
            {
                _isExpanded = true;
                this.Height = _subMenuContainer?.Height ?? 0;
            }
        }

        public void Collapse()
        {
            if (_isExpanded)
            {
                _isExpanded = false;
                this.Height = 0;
            }
        }
    }
}

