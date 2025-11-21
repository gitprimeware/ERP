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
    public partial class AccountingForm : UserControl
    {
        private Panel _mainPanel;
        private FlowLayoutPanel _cardsPanel;
        private DataGridView _dataGridView;
        private CheckBox _chkTableView;
        private OrderRepository _orderRepository;
        private bool _isTableView = true; // Default tablo görünümü

        public event EventHandler<Guid> AccountingEntryRequested;
        public event EventHandler<Guid> OrderSendToShipmentRequested;

        public AccountingForm()
        {
            _orderRepository = new OrderRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadAccountingOrders();
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
                Text = "Muhasebe",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Görünüm switch'i
            _chkTableView = new CheckBox
            {
                Text = "📊 Tablo Görünümü",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(30, 80),
                Checked = _isTableView
            };
            _chkTableView.CheckedChanged += ChkTableView_CheckedChanged;

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 110),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 150,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = !_isTableView
            };

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 110),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 150,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None,
                Visible = _isTableView
            };
            _dataGridView.CellClick += DataGridView_CellClick;
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            _mainPanel.Resize += (s, e) =>
            {
                _cardsPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Height = _mainPanel.Height - 150;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 150;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_chkTableView);
            _mainPanel.Controls.Add(_cardsPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadAccountingOrders()
        {
            try
            {
                // Tüm siparişleri getir (filtreleme yok)
                var allOrders = _orderRepository.GetAll().ToList();

                if (_isTableView)
                {
                    LoadDataGridView(allOrders);
                }
                else
                {
                    LoadCardsView(allOrders);
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
                var card = CreateAccountingCard(order);
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

            // Kolonları ekle
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrexOrderNo",
                HeaderText = "Trex Sipariş No",
                Name = "TrexOrderNo",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CustomerOrderNo",
                HeaderText = "Müşteri Sipariş No",
                Name = "CustomerOrderNo",
                Width = 130
            });

            var companyColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Firma",
                Name = "CompanyName",
                Width = 150
            };
            _dataGridView.Columns.Add(companyColumn);

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductCode",
                HeaderText = "Ürün Kodu",
                Name = "ProductCode",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalPrice",
                HeaderText = "Toplam Fiyat (₺)",
                Name = "TotalPrice",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Durum",
                Name = "Status",
                Width = 100
            });

            // İşlemler kolonu (sadece emoji)
            var actionsColumn = new DataGridViewButtonColumn
            {
                HeaderText = "İşlemler",
                Name = "Actions",
                Width = 120,
                Text = "",
                UseColumnTextForButtonValue = false
            };
            _dataGridView.Columns.Add(actionsColumn);

            // DataSource için özel bir liste oluştur
            var dataSource = orders.Select(o => new
            {
                o.Id,
                o.TrexOrderNo,
                o.CustomerOrderNo,
                CompanyName = o.Company?.Name ?? "",
                o.ProductCode,
                TotalPrice = o.TotalPrice.ToString("N2") + " ₺",
                o.Status,
                IsInAccounting = o.Status == "Muhasebede"
            }).ToList();

            _dataGridView.DataSource = dataSource;
            _dataGridView.Tag = orders; // Orijinal order listesini sakla

            // DataBindingComplete event'inde butonları doldur
            _dataGridView.DataBindingComplete += (s, e) =>
            {
                UpdateActionButtons();
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
                            bool isInAccounting = order.Status == "Muhasebede";
                            var btnCell = row.Cells["Actions"] as DataGridViewButtonCell;
                            if (btnCell != null)
                            {
                                // Sadece emoji'ler - Detay, İşle, Siparişe Gönder
                                if (isInAccounting)
                                {
                                    btnCell.Value = "📋 📝 📦"; // Detay, İşle ve Siparişe Gönder
                                    btnCell.Style.ForeColor = ThemeColors.Info;
                                }
                                else
                                {
                                    btnCell.Value = "📋 📝 📦"; // Detay, İşle, Siparişe Gönder (gri)
                                    btnCell.Style.ForeColor = Color.Gray;
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
            LoadAccountingOrders();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
            {
                var order = orders[e.RowIndex];
                bool isInAccounting = order.Status == "Muhasebede";

                // İşlemler kolonuna tıklandı
                if (_dataGridView.Columns[e.ColumnIndex].Name == "Actions")
                {
                    // Emoji pozisyonuna göre işlem seç
                    var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    var cellRect = _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var clickX = _dataGridView.PointToClient(Control.MousePosition).X - cellRect.X;
                    var emojiWidth = cellRect.Width / 3; // 3 emoji var: 📋 📝 📦

                    int emojiIndex = (int)(clickX / emojiWidth);

                    switch (emojiIndex)
                    {
                        case 0: // 📋 Detay
                            AccountingEntryRequested?.Invoke(this, order.Id);
                            break;
                        case 1: // 📝 İşle
                            if (!isInAccounting)
                            {
                                MessageBox.Show("Bu sipariş muhasebede değil, işlem yapılamaz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            AccountingEntryRequested?.Invoke(this, order.Id);
                            break;
                        case 2: // 📦 Siparişe Gönder
                            if (!isInAccounting)
                            {
                                MessageBox.Show("Bu sipariş muhasebede değil, işlem yapılamaz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            var result = MessageBox.Show(
                                $"Sipariş {order.TrexOrderNo} siparişe gönderilecek. Emin misiniz?",
                                "Siparişe Gönder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                OrderSendToShipmentRequested?.Invoke(this, order.Id);
                            }
                            break;
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
                    AccountingEntryRequested?.Invoke(this, order.Id);
                }
            }
        }

        private Panel CreateAccountingCard(Order order)
        {
            var card = new Panel
            {
                Width = 350,
                Height = 380,
                BackColor = ThemeColors.Surface,
                Margin = new Padding(15),
                Padding = new Padding(20)
            };

            UIHelper.ApplyCardStyle(card, 8);

            // Yeni gelen sipariş için border rengi
            bool isNew = order.ModifiedDate == null || 
                        (DateTime.Now - order.ModifiedDate.Value).TotalHours < 24;
            
            if (isNew)
            {
                card.BorderStyle = BorderStyle.FixedSingle;
                // Border rengini değiştirmek için Paint event'i kullanabiliriz
                card.Paint += (s, e) =>
                {
                    var rect = card.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(new Pen(ThemeColors.Warning, 3), rect);
                };
            }

            int yPos = 15;

            // Yeni işareti
            if (isNew)
            {
                var lblNew = new Label
                {
                    Text = "🆕 YENİ",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ThemeColors.Warning,
                    AutoSize = true,
                    Location = new Point(15, yPos)
                };
                card.Controls.Add(lblNew);
                yPos += 25;
            }

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

            // Ürün Kodu
            if (!string.IsNullOrEmpty(order.ProductCode))
            {
                var lblProductCode = new Label
                {
                    Text = $"Ürün Kodu: {order.ProductCode}",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(15, yPos),
                    MaximumSize = new Size(310, 0)
                };
                card.Controls.Add(lblProductCode);
                yPos += 25;
            }

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

            // Butonlar - Sadece "Muhasebede" statüsündeki siparişler için aktif
            bool isInAccounting = order.Status == "Muhasebede";
            
            // Detay butonu (her zaman aktif)
            var btnDetail = ButtonFactory.CreateActionButton("📋 Detay", ThemeColors.Info, Color.White, 150, 35);
            btnDetail.Location = new Point(15, yPos);
            btnDetail.Click += (s, e) => AccountingEntryRequested?.Invoke(this, order.Id);
            yPos += 45;
            
            var btnProcess = ButtonFactory.CreateActionButton("📝 İşle", ThemeColors.Info, Color.White, 150, 35);
            btnProcess.Location = new Point(15, yPos);
            
            if (isInAccounting)
            {
                btnProcess.Click += (s, e) => AccountingEntryRequested?.Invoke(this, order.Id);
            }
            else
            {
                btnProcess.Enabled = false;
                btnProcess.BackColor = Color.Gray;
                btnProcess.Cursor = Cursors.No;
            }
            yPos += 45;

            var btnSendToShipment = ButtonFactory.CreateActionButton("📦 Siparişe Gönder", ThemeColors.Success, Color.White, 150, 35);
            btnSendToShipment.Location = new Point(15, yPos);
            
            if (isInAccounting)
            {
                btnSendToShipment.Click += (s, e) =>
                {
                    var result = MessageBox.Show(
                        $"Sipariş {order.TrexOrderNo} siparişe gönderilecek. Emin misiniz?",
                        "Siparişe Gönder",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        OrderSendToShipmentRequested?.Invoke(this, order.Id);
                    }
                };
            }
            else
            {
                btnSendToShipment.Enabled = false;
                btnSendToShipment.BackColor = Color.Gray;
                btnSendToShipment.Cursor = Cursors.No;
            }

            card.Controls.Add(lblOrderNo);
            card.Controls.Add(lblCustomerOrderNo);
            card.Controls.Add(lblCompany);
            card.Controls.Add(lblTotal);
            card.Controls.Add(btnDetail);
            card.Controls.Add(btnProcess);
            card.Controls.Add(btnSendToShipment);

            return card;
        }

        public void RefreshOrders()
        {
            LoadAccountingOrders();
        }
    }
}

