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
        private ToolTip _actionToolTip;
        private string _currentToolTipText = "";

        public event EventHandler<Guid> OrderSelected;
        public event EventHandler<Guid> OrderUpdateRequested;
        public event EventHandler<Guid> OrderDeleteRequested;
        public event EventHandler<Guid> OrderSendToProductionRequested;
        public event EventHandler<Guid> OrderSendToAccountingRequested; // Siparişten muhasebeye gönder
        public event EventHandler<Guid> OrderGetWorkOrderRequested;
        public event EventHandler<List<Guid>> OrderGetBulkWorkOrderRequested; // Toplu iş emri için

        public OrderListForm()
        {
            InitializeComponent();
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            _actionToolTip = new ToolTip();
            _actionToolTip.IsBalloon = false;
            _actionToolTip.ShowAlways = false;
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
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = false // Ana panel kaymasın, sadece tablo kayacak
            };

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

            // Toplu iş emri butonu - Checkbox ile aynı hizada
            var btnBulkWorkOrder = new Button
            {
                Text = "📄 Toplu İş Emri Al",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ThemeColors.Success,
                Size = new Size(180, 35),
                Location = new Point(_mainPanel.Width - 210, 135),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnBulkWorkOrder.FlatAppearance.BorderSize = 0;
            btnBulkWorkOrder.Click += BtnBulkWorkOrder_Click;

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 180),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 220,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = !_isTableView
            };

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 180),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 220,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false, // Checkbox'ların çalışması için false olmalı
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true, // Çoklu seçim için true yapıldı
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 40 }, // Satır yüksekliği
                ScrollBars = ScrollBars.Vertical, // Sadece dikey scroll
                Visible = _isTableView
            };
            _dataGridView.CellClick += DataGridView_CellClick;
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;
            _dataGridView.RowPrePaint += DataGridView_RowPrePaint;
            _dataGridView.CellPainting += DataGridView_CellPainting;
            _dataGridView.CellMouseEnter += DataGridView_CellMouseEnter;
            _dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;
            _dataGridView.Scroll += DataGridView_Scroll;
            
            // DoubleBuffered özelliğini aç - scroll sırasında üst üste binmeyi önler
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, _dataGridView, new object[] { true });

            _mainPanel.Resize += (s, e) =>
            {
                searchPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Height = _mainPanel.Height - 220;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 220;
            };
            
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
                // Sadece SP (normal) siparişleri göster, YM (stok) siparişlerini filtrele
                var orders = _orderRepository.GetAll(searchTerm, companyId)
                    .Where(o => !o.IsStockOrder) // IsStockOrder == false olanları al (SP siparişleri)
                    .ToList();

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
                Width = 35,
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
                Width = 220,
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

                // Satır renklendirmesi - ilk yüklemede - DataBindingComplete'ten SONRA
                foreach (DataGridViewRow row in _dataGridView.Rows)
                {
                    if (row.DataBoundItem != null)
                    {
                        ApplyRowColorToRow(row);
                    }
                }

            // İlk yükleme için butonları güncelle
            UpdateActionButtons();

                // Tüm satırları yeniden çiz (renklendirmenin görünmesi için)
                _dataGridView.Invalidate();
                
                // Refresh'i de çağır (hemen görünmesi için)
                _dataGridView.Refresh();
            };

            // Satırlar eklendiğinde renklendirmeyi uygula
            _dataGridView.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    if (i >= 0 && i < _dataGridView.Rows.Count)
                    {
                        ApplyRowColorToRow(_dataGridView.Rows[i]);
                    }
                }
                _dataGridView.Invalidate(); // Tüm satırları yeniden çiz
            };

            // Stil ayarları - ÖNCE stil ayarları yapılsın
            _dataGridView.BackgroundColor = Color.White;
            // DefaultCellStyle.BackColor'u burada ayarlamayalım - satır renklendirmesi override edecek
            _dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, ThemeColors.Primary.R, ThemeColors.Primary.G, ThemeColors.Primary.B);
            _dataGridView.GridColor = Color.FromArgb(230, 230, 230); // Açık gri border
            _dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; // Sadece yatay çizgiler
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;
            _dataGridView.RowHeadersVisible = false; // Sol taraftaki row header'ı kaldır
            _dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; // Header border yok
            _dataGridView.BorderStyle = BorderStyle.None; // Dış border yok

            // Buton kolonu stil - tooltip'i kapat
            _dataGridView.Columns["Actions"].DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            _dataGridView.Columns["Actions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns["Actions"].DefaultCellStyle.Padding = new Padding(2, 2, 2, 2);

            // Actions kolonundaki default tooltip'leri kapat (biz kendi tooltip'imizi gösteriyoruz)
            _dataGridView.ShowCellToolTips = false;

            // İlk yükleme için butonları güncelle - DataBindingComplete'ten SONRA
        }

        private void UpdateActionButtons()
        {
            if (_dataGridView.Columns["Actions"] == null) return;

            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                // Actions kolonundaki tooltip'i boşalt
                if (row.Cells["Actions"] != null)
                {
                    row.Cells["Actions"].ToolTipText = "";
                }

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
                            bool isNew = order.Status == "Yeni";
                            var btnCell = row.Cells["Actions"] as DataGridViewButtonCell;
                            if (btnCell != null)
                            {
                                // Sadece emoji'ler - Soldan sağa: Ayrıntılar, İş Emri, Üretim, Silme
                                if (isReadyForShipment)
                                {
                                    btnCell.Value = "📋 📄 🗑️"; // Detay, İş Emri, Sil (Üretime gönder yok)
                                }
                                else if (isNew)
                                {
                                    btnCell.Value = "📋 📄 🏭 🗑️"; // Detay, İş Emri, Üretim, Sil
                                }
                                else
                                {
                                    // Üretimde, Sevk Edildi vs. durumlarında Üretime Gönder yok
                                    btnCell.Value = "📋 📄 🗑️"; // Detay, İş Emri, Sil
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
                bool isNew = order.Status == "Yeni";
                
                // Emoji sayısını belirle - "Yeni" durumunda 5 buton (Detay, İş Emri, Üretime Gönder, Muhasebeye Gönder, Sil)
                int emojiCount = isNew ? 5 : 3;

                // İşlemler kolonuna tıklandı
                if (_dataGridView.Columns[e.ColumnIndex].Name == "Actions")
                {
                    var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    var cellRect = _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var clickX = _dataGridView.PointToClient(Control.MousePosition).X - cellRect.X;
                    var emojiWidth = cellRect.Width / emojiCount; // Emoji sayısına göre böl

                    int emojiIndex = (int)(clickX / emojiWidth);

                    if (isNew)
                    {
                        // 📋 📄 🏭 💰 🗑️ - "Yeni" durumunda 5 buton (Detay, İş Emri, Üretime Gönder, Muhasebeye Gönder, Sil)
                        // emojiCount zaten üstte 5 olarak hesaplanmış, emojiIndex de doğru hesaplanmış
                        switch (emojiIndex)
                        {
                            case 0: // 📋 Detay
                                OrderUpdateRequested?.Invoke(this, order.Id);
                                break;
                            case 1: // 📄 İş Emri Al
                                OrderGetWorkOrderRequested?.Invoke(this, order.Id);
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
                            case 3: // 💰 Muhasebeye Gönder
                                var resultAccounting = MessageBox.Show(
                                    $"Sipariş {order.TrexOrderNo} muhasebeye gönderilecek. Emin misiniz?",
                                    "Muhasebeye Gönder",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);
                                if (resultAccounting == DialogResult.Yes)
                                {
                                    OrderSendToAccountingRequested?.Invoke(this, order.Id);
                                }
                                break;
                            case 4: // 🗑️ Sil
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
                        }
                    }
                    else
                    {
                        // 📋 📄 🗑️ - Diğer durumlarda 3 buton (Üretime Gönder yok)
                    switch (emojiIndex)
                    {
                        case 0: // 📋 Detay
                            OrderUpdateRequested?.Invoke(this, order.Id);
                            break;
                            case 1: // 📄 İş Emri Al
                                OrderGetWorkOrderRequested?.Invoke(this, order.Id);
                                break;
                            case 2: // 🗑️ Sil
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

            // Butonlar - Tek satır halinde (soldan sağa: Ayrıntılar, İş Emri, Üretim, Silme)
            var btnDetail = ButtonFactory.CreateActionButton("📋", ThemeColors.Info, Color.White, 70, 30);
            btnDetail.Location = new Point(15, yPos);
            btnDetail.Click += (s, e) => OrderUpdateRequested?.Invoke(this, order.Id);

            var btnGetWorkOrder = ButtonFactory.CreateActionButton("📄", ThemeColors.Primary, Color.White, 70, 30);
            btnGetWorkOrder.Location = new Point(90, yPos);
            btnGetWorkOrder.Click += (s, e) => OrderGetWorkOrderRequested?.Invoke(this, order.Id);

            // Sadece "Yeni" durumunda Üretime Gönder butonu göster
            bool isNew = order.Status == "Yeni";
            var btnSendToProduction = ButtonFactory.CreateActionButton("🏭", ThemeColors.Warning, Color.White, 70, 30);
            btnSendToProduction.Location = new Point(165, yPos);
            if (!isNew)
            {
                // "Yeni" değilse butonu gizle
                btnSendToProduction.Visible = false;
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

            var btnDelete = ButtonFactory.CreateActionButton("🗑️", ThemeColors.Error, Color.White, 70, 30);
            btnDelete.Location = new Point(240, yPos);
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
            yPos += 40;

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

        private void ApplyRowColorToRow(DataGridViewRow row)
        {
            if (row == null) return;

            string status = "";

            // Status'u al - önce DataBoundItem'dan
            if (row.DataBoundItem != null)
            {
                var rowData = row.DataBoundItem;
                var statusProperty = rowData.GetType().GetProperty("Status");
                if (statusProperty != null)
                {
                    status = statusProperty.GetValue(rowData)?.ToString() ?? "";
                }

                // Tag'dan da deneyelim (Order listesi)
                if (string.IsNullOrEmpty(status) && _dataGridView.Tag is List<Order> orders)
                {
                    var idProperty = rowData.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var orderId = (Guid)idProperty.GetValue(rowData);
                        var order = orders.FirstOrDefault(o => o.Id == orderId);
                        if (order != null)
                        {
                            status = order.Status ?? "";
                        }
                    }
                }
            }

            Color rowColor = Color.White;

            // Durum renklendirmesi (daha belirgin - Alpha değeri 120)
            if (status == "Yeni")
            {
                rowColor = Color.FromArgb(120, 33, 150, 243); // Mavi, hafif saydam
            }
            else if (status == "Sevkiyata Hazır")
            {
                rowColor = Color.FromArgb(120, 255, 193, 7); // Sarı, hafif saydam
            }
            else if (status == "Sevk Edildi")
            {
                rowColor = Color.FromArgb(120, 76, 175, 80); // Yeşil, hafif saydam
            }

            // Satır seviyesinde arka plan rengi uygula
            row.DefaultCellStyle.BackColor = rowColor;
            row.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;

            // Her hücreye ayrı ayrı uygula (Actions kolonu dahil - arka plan için)
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn != null)
                {
                    if (cell.OwningColumn.Name != "Actions")
                    {
                        cell.Style.BackColor = rowColor;
                        cell.Style.ForeColor = ThemeColors.TextPrimary;
                        cell.Style.Padding = new Padding(0); // Padding'i kaldır
                    }
                    else
                    {
                        // Actions kolonu için de arka plan rengini ayarla (emoji'ler üzerine çizilecek)
                        cell.Style.BackColor = rowColor;
                    }
                }
            }

            // Seçildiğinde de aynı rengi kullan - renk değişimi yok
            row.DefaultCellStyle.SelectionBackColor = rowColor;
            row.DefaultCellStyle.SelectionForeColor = ThemeColors.TextPrimary;

            // Her hücreye de uygula
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn != null && cell.OwningColumn.Name != "Actions")
                {
                    cell.Style.SelectionBackColor = rowColor;
                    cell.Style.SelectionForeColor = ThemeColors.TextPrimary;
                }
            }
        }

        private void DataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // Satır renklendirmesi - sadece stilleri uygula, custom painting yapma
            if (e.RowIndex >= 0 && e.RowIndex < _dataGridView.Rows.Count)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                ApplyRowColorToRow(row);
            }
        }

        private void DataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            // Scroll sırasında tüm görünür satırları yeniden çiz
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                _dataGridView.Invalidate();
                _dataGridView.Update();
            }
        }

        private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Header satırlarını atla
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex >= 0 && e.RowIndex < _dataGridView.Rows.Count)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                bool isActionsColumn = _dataGridView.Columns[e.ColumnIndex].Name == "Actions";
                
                // Önce hücreyi tamamen temizle (üst üste binmeyi önlemek için)
                e.Graphics.FillRectangle(new SolidBrush(_dataGridView.BackgroundColor), e.CellBounds);

                // Status'u al - önce DataBoundItem'dan, sonra Tag'dan
                string status = "";
                if (row.DataBoundItem != null)
                {
                    var rowData = row.DataBoundItem;
                    var statusProperty = rowData.GetType().GetProperty("Status");
                    if (statusProperty != null)
                    {
                        status = statusProperty.GetValue(rowData)?.ToString() ?? "";
                    }
                }
                
                // Tag'dan Order listesini al
                List<Order> orders = null;
                if (_dataGridView.Tag is List<Order> tagOrders)
                {
                    orders = tagOrders;
                }
                
                // Tag'dan da deneyelim (Order listesi) - eğer status boşsa
                if (string.IsNullOrEmpty(status) && orders != null && e.RowIndex < orders.Count)
                {
                    status = orders[e.RowIndex].Status ?? "";
                }

                // Satır rengini status'tan belirle
                Color rowBgColor = Color.White;
                if (status == "Yeni")
                {
                    rowBgColor = Color.FromArgb(120, 33, 150, 243);
                }
                else if (status == "Sevkiyata Hazır")
                {
                    rowBgColor = Color.FromArgb(120, 255, 193, 7);
                }
                else if (status == "Sevk Edildi")
                {
                    rowBgColor = Color.FromArgb(120, 76, 175, 80);
                }

                // Seçili durumda da aynı rengi kullan (renk değişimi yok)

                // Actions kolonu için özel işlem
                if (isActionsColumn && row.DataBoundItem != null)
                {
                    // Önce hücreyi tamamen temizle
                    e.Graphics.FillRectangle(new SolidBrush(rowBgColor), e.CellBounds);
                    
                    // Border'ı çiz
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

                    if (orders != null && e.RowIndex < orders.Count)
                    {
                        var order = orders[e.RowIndex];
                        bool isReadyForShipment = order.Status == "Sevkiyata Hazır";
                        bool isNew = order.Status == "Yeni";

                        string[] emojis;
                        Color[] colors;

                        if (isReadyForShipment)
                        {
                            emojis = new[] { "📋", "📄", "🗑️" };
                            colors = new[] { ThemeColors.Info, ThemeColors.Primary, ThemeColors.Error };
                        }
                        else if (isNew)
                        {
                            emojis = new[] { "📋", "📄", "🏭", "💰", "🗑️" };
                            colors = new[] { ThemeColors.Info, ThemeColors.Primary, ThemeColors.Warning, ThemeColors.Success, ThemeColors.Error };
                        }
                        else
                        {
                            // Üretimde, Sevk Edildi vs. durumlarında Üretime Gönder yok
                            emojis = new[] { "📋", "📄", "🗑️" };
                            colors = new[] { ThemeColors.Info, ThemeColors.Primary, ThemeColors.Error };
                        }

                        int emojiWidth = e.CellBounds.Width / emojis.Length;
                        Font emojiFont = new Font("Segoe UI Emoji", 12F);
                        int circleSize = 20;
                        int emojiSize = 14;

                        for (int i = 0; i < emojis.Length; i++)
                        {
                            // Her emoji için merkez noktası
                            int xCenter = e.CellBounds.X + (i * emojiWidth) + (emojiWidth / 2);
                            // Emoji'leri hücrenin ortasına dikey olarak hizala
                            int yCenter = e.CellBounds.Y + (e.CellBounds.Height / 2);

                            // Renkli arka plan çemberi (tam yuvarlak)
                            int circleX = xCenter - (circleSize / 2);
                            int circleY = yCenter - (circleSize / 2);

                            // Renkli arka plan çemberi - daha belirgin renkler (Alpha değeri 70)
                            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(70, colors[i])))
                            {
                                e.Graphics.FillEllipse(bgBrush, circleX, circleY, circleSize, circleSize);
                            }

                            // Renkli kenarlık da ekle
                            using (Pen borderPen = new Pen(colors[i], 1.5f))
                            {
                                e.Graphics.DrawEllipse(borderPen, circleX, circleY, circleSize, circleSize);
                            }

                            // Emoji'yi çemberin tam ortasına çiz
                            RectangleF emojiRect = new RectangleF(
                                xCenter - (emojiSize / 2f),
                                yCenter - (emojiSize / 2f),
                                emojiSize,
                                emojiSize
                            );

                            // Emoji'yi çiz - düzgün hizalı ve ortalanmış
                            using (StringFormat sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;
                                sf.FormatFlags = StringFormatFlags.NoWrap;
                                e.Graphics.DrawString(emojis[i], emojiFont, Brushes.Black, emojiRect, sf);
                            }
                        }

                        emojiFont.Dispose();
                        e.Handled = true;
                        return;
                    }
                }
                // Actions kolonu değilse - arka planı çiz
                else if (!isActionsColumn)
                {
                    if (rowBgColor != Color.White)
                    {
                        // Önce arka planı tamamen temizle ve yeni rengi uygula
                        e.Graphics.FillRectangle(new SolidBrush(rowBgColor), e.CellBounds);
                        // İçeriği ve border'ı çiz
                        e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.Border);
                        e.Handled = true;
                    }
                    else
                    {
                        // Beyaz arka plan için de temizle
                        e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);
                        e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                        e.Handled = true;
                    }
                }
            }
        }
    

        private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Actions kolonundaki emoji'lerin üzerine gelindiğinde tooltip göster
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && 
                _dataGridView.Columns[e.ColumnIndex].Name == "Actions" &&
                _dataGridView.Rows[e.RowIndex].DataBoundItem != null)
            {
                if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
                {
                    var order = orders[e.RowIndex];
                    bool isReadyForShipment = order.Status == "Sevkiyata Hazır";
                    bool isNew = order.Status == "Yeni";
                    
                    string[] tooltips;
                    if (isNew)
                    {
                        // Sadece "Yeni" durumunda Üretime Gönder butonu var
                        tooltips = new[] { "Ayrıntılar", "İş Emri Al", "Üretime Gönder", "Sil" };
                    }
                    else
                    {
                        // Diğer durumlarda Üretime Gönder butonu yok
                        tooltips = new[] { "Ayrıntılar", "İş Emri Al", "Sil" };
                    }
                    
                    // Mouse pozisyonunu kontrol et
                    var cellRect = _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var mousePos = _dataGridView.PointToClient(Control.MousePosition);
                    var clickX = mousePos.X - cellRect.X;
                    var emojiWidth = cellRect.Width / tooltips.Length;
                    
                    if (emojiWidth > 0)
                    {
                        int emojiIndex = Math.Max(0, Math.Min(tooltips.Length - 1, (int)(clickX / emojiWidth)));
                        
                        if (emojiIndex >= 0 && emojiIndex < tooltips.Length)
                        {
                            _currentToolTipText = tooltips[emojiIndex];
                            _actionToolTip.Show(tooltips[emojiIndex], _dataGridView, 
                                mousePos.X + 10, mousePos.Y + 20, 3000);
                        }
                    }
                }
            }
        }

        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // Tooltip'i gizle
            _actionToolTip.Hide(_dataGridView);
            _currentToolTipText = "";
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
