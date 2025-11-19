using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class OrderListForm : UserControl
    {
        private Panel _mainPanel;
        private FlowLayoutPanel _cardsPanel;
        private TextBox _txtSearch;
        private ComboBox _cmbCompanyFilter;
        private Button _btnSearch;
        private Button _btnRefresh;
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;

        public event EventHandler<Guid> OrderSelected;
        public event EventHandler<Guid> OrderUpdateRequested;
        public event EventHandler<Guid> OrderDeleteRequested;

        public OrderListForm()
        {
            InitializeComponent();
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadOrders();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "Siparişleri Görüntüle",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Arama paneli
            var searchPanel = CreateSearchPanel();
            searchPanel.Location = new Point(30, 80);

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _mainPanel.Resize += (s, e) =>
            {
                searchPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Height = _mainPanel.Height - 180;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(searchPanel);
            _mainPanel.Controls.Add(_cardsPanel);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private Panel CreateSearchPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblSearch = new Label
            {
                Text = "Ara:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 15)
            };

            _txtSearch = new TextBox
            {
                Width = 300,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(50, 12),
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformSearch(); };

            var lblCompany = new Label
            {
                Text = "Firma:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(370, 15)
            };

            _cmbCompanyFilter = new ComboBox
            {
                Width = 250,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(430, 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            LoadCompaniesForFilter();

            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Info, Color.White, 100, 30);
            _btnSearch.Location = new Point(700, 12);
            _btnSearch.Click += (s, e) => PerformSearch();

            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Secondary, Color.White, 100, 30);
            _btnRefresh.Location = new Point(810, 12);
            _btnRefresh.Click += (s, e) => LoadOrders();

            panel.Controls.Add(lblSearch);
            panel.Controls.Add(_txtSearch);
            panel.Controls.Add(lblCompany);
            panel.Controls.Add(_cmbCompanyFilter);
            panel.Controls.Add(_btnSearch);
            panel.Controls.Add(_btnRefresh);

            return panel;
        }

        private void LoadCompaniesForFilter()
        {
            try
            {
                _cmbCompanyFilter.Items.Clear();
                _cmbCompanyFilter.Items.Add(new { Id = (Guid?)null, Name = "Tüm Firmalar" });
                _cmbCompanyFilter.DisplayMember = "Name";
                _cmbCompanyFilter.ValueMember = "Id";
                _cmbCompanyFilter.SelectedIndex = 0;

                var companies = _companyRepository.GetAll();
                foreach (var company in companies)
                {
                    _cmbCompanyFilter.Items.Add(new { Id = (Guid?)company.Id, Name = company.Name });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformSearch()
        {
            string searchTerm = _txtSearch.Text.Trim();
            Guid? companyId = null;

            if (_cmbCompanyFilter.SelectedItem != null)
            {
                var selected = _cmbCompanyFilter.SelectedItem;
                var idProperty = selected.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(selected);
                    if (idValue != null && idValue != DBNull.Value)
                    {
                        companyId = (Guid?)idValue;
                    }
                }
            }

            LoadOrders(searchTerm, companyId);
        }

        private void LoadOrders(string searchTerm = null, Guid? companyId = null)
        {
            try
            {
                _cardsPanel.Controls.Clear();

                var orders = _orderRepository.GetAll(searchTerm, companyId);

                if (orders.Count == 0)
                {
                    var noDataLabel = new Label
                    {
                        Text = "Sipariş bulunamadı.",
                        Font = new Font("Segoe UI", 12F),
                        ForeColor = ThemeColors.TextSecondary,
                        AutoSize = true,
                        Location = new Point(20, 20)
                    };
                    _cardsPanel.Controls.Add(noDataLabel);
                    return;
                }

                foreach (var order in orders)
                {
                    var card = CreateOrderCard(order);
                    _cardsPanel.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateOrderCard(Order order)
        {
            var card = new Panel
            {
                Width = 350,
                Height = 280,
                BackColor = ThemeColors.Surface,
                Margin = new Padding(15),
                Padding = new Padding(20)
            };

            UIHelper.ApplyCardStyle(card, 8);

            int yPos = 15;

            // Sipariş No
            var lblOrderNo = new Label
            {
                Text = $"Sipariş No: {order.TrexOrderNo}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 30;

            // Müşteri Sipariş No
            var lblCustomerOrderNo = new Label
            {
                Text = $"Müşteri Sipariş: {order.CustomerOrderNo}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 25;

            // Firma
            var lblCompany = new Label
            {
                Text = $"Firma: {order.Company?.Name ?? "Bilinmiyor"}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos),
                MaximumSize = new Size(310, 0)
            };
            yPos += 25;

            // Cihaz Adı
            if (!string.IsNullOrEmpty(order.DeviceName))
            {
                var lblDevice = new Label
                {
                    Text = $"Cihaz: {order.DeviceName}",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(15, yPos),
                    MaximumSize = new Size(310, 0)
                };
                card.Controls.Add(lblDevice);
                yPos += 25;
            }

            // Tarih
            var lblDate = new Label
            {
                Text = $"Tarih: {order.OrderDate:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 25;

            // Termin Tarihi
            var lblTermDate = new Label
            {
                Text = $"Termin: {order.TermDate:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 25;

            // Toplam Fiyat
            var lblTotal = new Label
            {
                Text = $"Toplam: {order.TotalPrice:N2} ₺",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeColors.Success,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 35;

            // Butonlar
            var btnDetail = ButtonFactory.CreateActionButton("📋 Detay", ThemeColors.Info, Color.White, 140, 35);
            btnDetail.Location = new Point(15, yPos);
            btnDetail.Click += (s, e) => OrderUpdateRequested?.Invoke(this, order.Id);

            var btnDelete = ButtonFactory.CreateActionButton("🗑️ Sil", ThemeColors.Error, Color.White, 140, 35);
            btnDelete.Location = new Point(165, yPos);
            btnDelete.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    $"Sipariş {order.TrexOrderNo} silinecek. Emin misiniz?",
                    "Sipariş Sil",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    OrderDeleteRequested?.Invoke(this, order.Id);
                }
            };

            card.Controls.Add(lblOrderNo);
            card.Controls.Add(lblCustomerOrderNo);
            card.Controls.Add(lblCompany);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblTermDate);
            card.Controls.Add(lblTotal);
            card.Controls.Add(btnDetail);
            card.Controls.Add(btnDelete);

            return card;
        }
    }
}
