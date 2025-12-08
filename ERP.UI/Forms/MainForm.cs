using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.UI.Components;
using ERP.UI.Managers;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class MainForm : Form
    {
        private Panel _menuPanel;
        private Panel _contentPanel;
        private HeaderPanel _headerPanel;
        private MenuManager _menuManager;
        private ContentManager _contentManager;
        private FormResolverService _formResolver;
        private MenuService _menuService;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _menuService = new MenuService();
            _formResolver = new FormResolverService();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Text = "ERP/MRP Sistemi";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 700);

            CreateHeaderPanel();
            CreateMenuPanel();
            CreateContentPanel();
            
            // ContentPanel'i en öne getir (menünün üstünde görünsün)
            _contentPanel.BringToFront();
        }

        private void CreateHeaderPanel()
        {
            _headerPanel = new HeaderPanel
            {
                Title = "ERP/MRP YÖNETİM SİSTEMİ",
                UserName = "Kullanıcı: Admin"
            };

            this.Controls.Add(_headerPanel);
        }

        private void CreateMenuPanel()
        {
            _menuPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250
            };

            _menuManager = new MenuManager(_menuPanel, _menuService);
            _menuManager.MenuItemClicked += MenuManager_MenuItemClicked;

            this.Controls.Add(_menuPanel);
        }

        private void CreateContentPanel()
        {
            _contentPanel = new Panel();
            _contentManager = new ContentManager(_contentPanel, _formResolver);
            _contentManager.ShowWelcomePanel();

            this.Controls.Add(_contentPanel);
        }

        private void MenuManager_MenuItemClicked(object sender, string formTag)
        {
            _menuManager.ActivateButton(formTag);

            if (formTag == "Home")
            {
                _contentManager.ShowWelcomePanel();
            }
            else if (formTag == "OrderList")
            {
                _contentManager.ShowForm("OrderList");
            }
            else if (formTag == "OrderCreate")
            {
                _contentManager.ShowForm("OrderCreate");
            }
            else if (formTag == "Accounting")
            {
                _contentManager.ShowForm("Accounting");
            }
            else if (formTag == "Production" || formTag == "ProductionFormul" || formTag == "ProductionReport")
            {
                // Üretim menüsü ProductionListForm'u açsın
                _contentManager.ShowForm("Production");
            }
            else if (formTag == "CuttingRequests")
            {
                _contentManager.ShowForm("CuttingRequests");
            }
            else if (formTag == "RuloStokTakip")
            {
                _contentManager.ShowForm("RuloStokTakip");
            }
            else if (formTag == "PreslenmisStokTakip")
            {
                _contentManager.ShowForm("PreslenmisStokTakip");
            }
            else if (formTag == "KenetlenmisStokTakip")
            {
                _contentManager.ShowForm("KenetlenmisStokTakip");
            }
            else if (formTag == "Consumption")
            {
                _contentManager.ShowForm("Consumption");
            }
            else if (formTag == "MaterialEntry" || formTag == "MaterialExit" || formTag == "StockDetail")
            {
                _contentManager.ShowForm(formTag);
            }
            else
            {
                _contentManager.ShowForm(formTag);
            }
        }
    }
}
