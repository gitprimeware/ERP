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
        private DataGridView _dataGridView;
        private TextBox _txtSearch;
        private ComboBox _cmbCompanyFilter;
        private Button _btnSearch;
        private Button _btnRefresh;
        private CheckBox _chkTableView;
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;
        private bool _isTableView = true; // Default tablo görünümü
        private ComboBox _cmbSortBy;

        public event EventHandler<Guid> OrderSelected;
        public event EventHandler<Guid> OrderUpdateRequested;
        public event EventHandler<Guid> OrderDeleteRequested;
        public event EventHandler<Guid> OrderSendToProductionRequested;
        public event EventHandler<Guid> OrderGetWorkOrderRequested;
        public event EventHandler<List<Guid>> OrderGetBulkWorkOrderRequested; // Toplu iş emri için

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

            // Görünüm switch'i
            _chkTableView = new CheckBox
            {
                Text = "📊 Tablo Görünümü",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(30, 140),
                Checked = _isTableView
            };
            _chkTableView.CheckedChanged += ChkTableView_CheckedChanged;

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 170),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 210,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = !_isTableView
            };

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 170),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 210,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false, // Checkbox'ların çalışması için false olmalı
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true, // Çoklu seçim için true yapıldı
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None,
                Visible = _isTableView
            };
            _dataGridView.CellClick += DataGridView_CellClick;
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;

            _mainPanel.Resize += (s, e) =>
            {
                searchPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Height = _mainPanel.Height - 210;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 210;
            };

            // Toplu iş emri butonu
            var btnBulkWorkOrder = new Button
            {
                Text = "📄 Toplu İş Emri Al",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ThemeColors.Success,
                Size = new Size(180, 35),
                Location = new Point(_mainPanel.Width - 210, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnBulkWorkOrder.FlatAppearance.BorderSize = 0;
            btnBulkWorkOrder.Click += BtnBulkWorkOrder_Click;
            
            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(searchPanel);
            _mainPanel.Controls.Add(_chkTableView);
            _mainPanel.Controls.Add(btnBulkWorkOrder);
            _mainPanel.Controls.Add(_cardsPanel);
            _mainPanel.Controls.Add(_dataGridView);

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
            _btnRefresh.Click += (s, e) => PerformSearch();

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
                var orders = _orderRepository.GetAll(searchTerm, companyId).ToList();

                if (_isTableView)
                {
                    LoadDataGridView(orders);
                }
                else
                {
                    LoadCardsView(orders);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCardsView(List<Order> orders)
        {
            _cardsPanel.Controls.Clear();

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

        private void LoadDataGridView(List<Order> orders)
        {
            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();

            if (orders.Count == 0)
            {
                return;
            }

            _dataGridView.AutoGenerateColumns = false;
            
            // Checkbox kolonu (seçim için)
            var checkboxColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Seç",
                Name = "IsSelected",
                DataPropertyName = "IsSelected", // DataSource'daki property ile bağla
                Width = 50,
                ReadOnly = false
            };
            _dataGridView.Columns.Add(checkboxColumn);
            
            // Kolonları ekle
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrexOrderNo",
                HeaderText = "Trex Sipariş No",
                Name = "TrexOrderNo",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CustomerOrderNo",
                HeaderText = "Müşteri Sipariş No",
                Name = "CustomerOrderNo",
                Width = 150
            });

            var companyColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CompanyName",
                HeaderText = "Firma",
                Name = "CompanyName",
                Width = 200
            };
            _dataGridView.Columns.Add(companyColumn);

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeviceName",
                HeaderText = "Cihaz Adı",
                Name = "DeviceName",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductCode",
                HeaderText = "Ürün Kodu",
                Name = "ProductCode",
                Width = 200
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 80
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Durum",
                Name = "Status",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDate",
                HeaderText = "Sipariş Tarihi",
                Name = "OrderDate",
                Width = 120
            });

            // İşlemler kolonu (sadece emoji)
            var actionsColumn = new DataGridViewButtonColumn
            {
                HeaderText = "İşlemler",
                Name = "Actions",
                Width = 180,
                Text = "",
                UseColumnTextForButtonValue = false
            };
            _dataGridView.Columns.Add(actionsColumn);

            // DataSource için özel bir liste oluştur (Company.Name için)
            // Checkbox'ların çalışması için class kullanıyoruz
            var dataSource = orders.Select(o => new OrderRowData
            {
                Id = o.Id,
                IsSelected = false, // Checkbox için başlangıç değeri
                TrexOrderNo = o.TrexOrderNo,
                CustomerOrderNo = o.CustomerOrderNo,
                CompanyName = o.Company?.Name ?? "",
                DeviceName = o.DeviceName,
                ProductCode = o.ProductCode,
                Quantity = o.Quantity,
                Status = o.Status,
                OrderDate = o.OrderDate.ToString("dd.MM.yyyy"),
                IsReadyForShipment = o.Status == "Sevkiyata Hazır"
            }).ToList();

            _dataGridView.DataSource = dataSource;
            _dataGridView.Tag = orders; // Orijinal order listesini sakla

            // DataBindingComplete event'inde butonları doldur ve checkbox kolonunu ayarla
            _dataGridView.DataBindingComplete += (s, e) =>
            {
                UpdateActionButtons();
                
                // Checkbox kolonu dışındaki tüm kolonları ReadOnly yap
                foreach (DataGridViewColumn column in _dataGridView.Columns)
                {
                    if (column.Name != "IsSelected")
                    {
                        column.ReadOnly = true;
                    }
                }
            };

            // İlk yükleme için butonları güncelle
            UpdateActionButtons();

            // Stil ayarları
            _dataGridView.DefaultCellStyle.BackColor = ThemeColors.Surface;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;

            // Buton kolonu stil
            _dataGridView.Columns["Actions"].DefaultCellStyle.Font = new Font("Segoe UI", 14F);
            _dataGridView.Columns["Actions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void UpdateActionButtons()
        {
            if (_dataGridView.Columns["Actions"] == null) return;

            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                if (row.DataBoundItem != null && _dataGridView.Tag is List<Order> orders)
                {
                    var dataItem = row.DataBoundItem;
                    var idProperty = dataItem.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var orderId = (Guid)idProperty.GetValue(dataItem);
                        var order = orders.FirstOrDefault(o => o.Id == orderId);
                        if (order != null)
                        {
                            bool isReadyForShipment = order.Status == "Sevkiyata Hazır";
                            var btnCell = row.Cells["Actions"] as DataGridViewButtonCell;
                            if (btnCell != null)
                            {
                                // Sadece emoji'ler - Detay, Sil, Üretime Gönder, İş Emri Al
                                if (isReadyForShipment)
                                {
                                    btnCell.Value = "📋 🗑️ 📄"; // Detay, Sil, İş Emri Al (Üretime gönder disabled)
                                    btnCell.Style.ForeColor = Color.Gray;
                                }
                                else
                                {
                                    btnCell.Value = "📋 🗑️ 🏭 📄"; // Detay, Sil, Üretime Gönder, İş Emri Al
                                    btnCell.Style.ForeColor = ThemeColors.Info;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ChkTableView_CheckedChanged(object sender, EventArgs e)
        {
            _isTableView = _chkTableView.Checked;
            _cardsPanel.Visible = !_isTableView;
            _dataGridView.Visible = _isTableView;
            PerformSearch(); // Mevcut filtrelerle yeniden yükle
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
            {
                var order = orders[e.RowIndex];
                bool isReadyForShipment = order.Status == "Sevkiyata Hazır";

                // İşlemler kolonuna tıklandı
                if (_dataGridView.Columns[e.ColumnIndex].Name == "Actions")
                {
                    var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    var cellRect = _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var clickX = _dataGridView.PointToClient(Control.MousePosition).X - cellRect.X;
                    var emojiWidth = cellRect.Width / (isReadyForShipment ? 3 : 4); // Emoji sayısına göre böl

                    int emojiIndex = (int)(clickX / emojiWidth);

                    if (isReadyForShipment)
                    {
                        // 📋 🗑️ 📄
                        switch (emojiIndex)
                        {
                            case 0: // 📋 Detay
                                OrderUpdateRequested?.Invoke(this, order.Id);
                                break;
                            case 1: // 🗑️ Sil
                                var resultDelete = MessageBox.Show(
                                    $"Sipariş {order.TrexOrderNo} silinecek. Emin misiniz?",
                                    "Sipariş Sil",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);
                                if (resultDelete == DialogResult.Yes)
                                {
                                    OrderDeleteRequested?.Invoke(this, order.Id);
                                }
                                break;
                            case 2: // 📄 İş Emri Al
                                OrderGetWorkOrderRequested?.Invoke(this, order.Id);
                                break;
                        }
                    }
                    else
                    {
                    // 📋 🗑️ 🏭 📄
                    switch (emojiIndex)
                    {
                        case 0: // 📋 Detay
                            OrderUpdateRequested?.Invoke(this, order.Id);
                            break;
                        case 1: // 🗑️ Sil
                            var resultDelete = MessageBox.Show(
                                $"Sipariş {order.TrexOrderNo} silinecek. Emin misiniz?",
                                "Sipariş Sil",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (resultDelete == DialogResult.Yes)
                            {
                                OrderDeleteRequested?.Invoke(this, order.Id);
                            }
                            break;
                        case 2: // 🏭 Üretime Gönder
                            var resultProduction = MessageBox.Show(
                                $"Sipariş {order.TrexOrderNo} üretime gönderilecek. Emin misiniz?",
                                "Üretime Gönder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (resultProduction == DialogResult.Yes)
                            {
                                OrderSendToProductionRequested?.Invoke(this, order.Id);
                            }
                            break;
                        case 3: // 📄 İş Emri Al
                            OrderGetWorkOrderRequested?.Invoke(this, order.Id);
                            break;
                    }
                    }
                }
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Header'a tıklanmışsa işlem yapma
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
            {
                var order = orders[e.RowIndex];
                // Çift tıklama ile detay aç (Actions kolonuna değilse)
                if (e.ColumnIndex < _dataGridView.Columns.Count && _dataGridView.Columns[e.ColumnIndex].Name != "Actions")
                {
                    OrderUpdateRequested?.Invoke(this, order.Id);
                }
            }
        }

        private Panel CreateOrderCard(Order order)
        {
            var card = new Panel
            {
                Width = 350,
                Height = 420, // Yükseklik artırıldı (yeni butonlar için)
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

            // Durum
            var lblStatus = new Label
            {
                Text = $"Durum: {order.Status ?? "Yeni"}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = GetStatusColor(order.Status),
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            card.Controls.Add(lblStatus);
            yPos += 35;

            // Butonlar - İlk satır
            var btnDetail = ButtonFactory.CreateActionButton("📋 Detay", ThemeColors.Info, Color.White, 105, 30);
            btnDetail.Location = new Point(15, yPos);
            btnDetail.Click += (s, e) => OrderUpdateRequested?.Invoke(this, order.Id);

            var btnDelete = ButtonFactory.CreateActionButton("🗑️ Sil", ThemeColors.Error, Color.White, 105, 30);
            btnDelete.Location = new Point(125, yPos);
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

            var btnSendToProduction = ButtonFactory.CreateActionButton("🏭 Üretime Gönder", ThemeColors.Warning, Color.White, 120, 30);
            btnSendToProduction.Location = new Point(235, yPos);
            
            // Sevkiyata Hazır durumunda butonları disable et
            bool isReadyForShipment = order.Status == "Sevkiyata Hazır";
            if (isReadyForShipment)
            {
                btnSendToProduction.Enabled = false;
                btnSendToProduction.BackColor = Color.Gray;
                btnSendToProduction.Cursor = Cursors.No;
            }
            else
            {
                btnSendToProduction.Click += (s, e) =>
                {
                    var result = MessageBox.Show(
                        $"Sipariş {order.TrexOrderNo} üretime gönderilecek. Emin misiniz?",
                        "Üretime Gönder",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        OrderSendToProductionRequested?.Invoke(this, order.Id);
                    }
                };
            }
            yPos += 40;

            // Butonlar - İkinci satır
            var btnGetWorkOrder = ButtonFactory.CreateActionButton("📄 İş Emri Al", ThemeColors.Info, Color.White, 160, 30);
            btnGetWorkOrder.Location = new Point(15, yPos);
            btnGetWorkOrder.Click += (s, e) => OrderGetWorkOrderRequested?.Invoke(this, order.Id);

            card.Controls.Add(lblOrderNo);
            card.Controls.Add(lblCustomerOrderNo);
            card.Controls.Add(lblCompany);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblTermDate);
            card.Controls.Add(lblTotal);
            card.Controls.Add(btnDetail);
            card.Controls.Add(btnDelete);
            card.Controls.Add(btnSendToProduction);
            card.Controls.Add(btnGetWorkOrder);

            return card;
        }

        private void BtnBulkWorkOrder_Click(object sender, EventArgs e)
        {
            if (!_isTableView)
            {
                MessageBox.Show("Toplu iş emri almak için tablo görünümünde olmalısınız.", 
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Seçili satırları al - Checkbox kolonundan oku
            var selectedOrderIds = new List<Guid>();
            
            if (_dataGridView.Columns["IsSelected"] == null)
            {
                MessageBox.Show("Checkbox kolonu bulunamadı. Lütfen sayfayı yenileyin.", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                if (row.DataBoundItem is OrderRowData rowData && rowData.IsSelected)
                {
                    selectedOrderIds.Add(rowData.Id);
                }
            }

            if (selectedOrderIds.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir sipariş seçin.", 
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Toplu iş emri event'ini tetikle
            OrderGetBulkWorkOrderRequested?.Invoke(this, selectedOrderIds);
        }

        private Color GetStatusColor(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return ThemeColors.TextSecondary;

            return status switch
            {
                "Yeni" => ThemeColors.Info,
                "Üretimde" => ThemeColors.Warning,
                "Muhasebede" => ThemeColors.Accent,
                "Sevkiyata Hazır" => ThemeColors.Secondary,
                "Sevk Edildi" => ThemeColors.Success,
                "Tamamlandı" => ThemeColors.Success,
                "İptal" => ThemeColors.Error,
                _ => ThemeColors.TextSecondary
            };
        }

        private void DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Checkbox değiştiğinde commit et
            if (_dataGridView.IsCurrentCellDirty && 
                _dataGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Checkbox kolonu değiştiğinde
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && 
                _dataGridView.Columns[e.ColumnIndex].Name == "IsSelected")
            {
                // Görsel güncelleme için refresh
                _dataGridView.InvalidateRow(e.RowIndex);
            }
        }
    }

    // Checkbox'ların çalışması için wrapper class
    public class OrderRowData
    {
        public Guid Id { get; set; }
        public bool IsSelected { get; set; }
        public string TrexOrderNo { get; set; }
        public string CustomerOrderNo { get; set; }
        public string CompanyName { get; set; }
        public string DeviceName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string OrderDate { get; set; }
        public bool IsReadyForShipment { get; set; }
    }
}
