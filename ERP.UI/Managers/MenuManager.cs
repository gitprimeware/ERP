using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.Interfaces;
using ERP.UI.Models;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Managers
{
    public class MenuManager
    {
        private readonly Panel _menuPanel;
        private readonly IMenuProvider _menuProvider;
        private Button _currentButton;
        private Dictionary<string, Components.SubMenuPanel> _subMenuPanels;

        public event EventHandler<string> MenuItemClicked;

        public MenuManager(Panel menuPanel, IMenuProvider menuProvider)
        {
            _menuPanel = menuPanel ?? throw new ArgumentNullException(nameof(menuPanel));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _subMenuPanels = new Dictionary<string, Components.SubMenuPanel>();
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            _menuPanel.BackColor = ThemeColors.MenuBackground;
            _menuPanel.Padding = new Padding(0, 10, 0, 10);

            var menuItems = _menuProvider.GetMenuItems().ToList();
            
            // DockStyle.Top kullandığımız için ters sırada ekliyoruz
            for (int i = menuItems.Count - 1; i >= 0; i--)
            {
                var menuItem = menuItems[i];
                var button = CreateMenuButton(menuItem);
                _menuPanel.Controls.Add(button);

                // SubMenu varsa ekle
                if (menuItem.HasSubMenu)
                {
                    var subMenuPanel = new Components.SubMenuPanel(menuItem);
                    subMenuPanel.SubMenuItemClicked += (s, tag) => OnMenuItemClicked(tag);
                    _subMenuPanels[menuItem.Tag] = subMenuPanel;
                    _menuPanel.Controls.Add(subMenuPanel);
                }
            }
        }

        private Button CreateMenuButton(MenuItem menuItem)
        {
            var buttonText = menuItem.Text;
            if (menuItem.HasSubMenu)
            {
                buttonText += " ▼";
            }

            var button = new Button
            {
                Text = buttonText,
                Tag = menuItem.Tag,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = ThemeColors.MenuText,
                BackColor = ThemeColors.MenuBackground,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.MenuHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.MenuSelected;

            if (menuItem.HasSubMenu)
            {
                button.Click += (s, e) => ToggleSubMenu(menuItem.Tag);
            }
            else
            {
                button.Click += (s, e) => OnMenuItemClicked(menuItem.Tag);
            }

            button.MouseEnter += MenuButton_MouseEnter;
            button.MouseLeave += MenuButton_MouseLeave;

            return button;
        }

        private void ToggleSubMenu(string parentTag)
        {
            if (_subMenuPanels.ContainsKey(parentTag))
            {
                _subMenuPanels[parentTag].Toggle();
            }
        }

        private void OnMenuItemClicked(string tag)
        {
            MenuItemClicked?.Invoke(this, tag);
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null && button != _currentButton)
            {
                button.BackColor = ThemeColors.MenuHover;
            }
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null && button != _currentButton)
            {
                button.BackColor = ThemeColors.MenuBackground;
            }
        }

        public void ActivateButton(string tag)
        {
            // Önceki butonu deaktif et
            if (_currentButton != null)
            {
                _currentButton.BackColor = ThemeColors.MenuBackground;
                _currentButton.ForeColor = ThemeColors.MenuText;
            }

            // Yeni butonu bul ve aktif et
            _currentButton = _menuPanel.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Tag?.ToString() == tag);

            if (_currentButton != null)
            {
                _currentButton.BackColor = ThemeColors.MenuSelected;
                _currentButton.ForeColor = ThemeColors.TextOnPrimary;
            }
        }
    }
}

