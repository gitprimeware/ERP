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
        private CompanyRepository _companyRepository;
        private bool _isTableView = true; // Default tablo görünümü
        private ToolTip _actionToolTip;
        private string _currentToolTipText = "";
        private TextBox _txtSearch;
        private ComboBox _cmbCompanyFilter;
        private Button _btnSearch;
        private Button _btnRefresh;

        public event EventHandler<Guid> AccountingEntryRequested;
        public event EventHandler<Guid> OrderSendToShipmentRequested;

        public AccountingForm()
        {
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            _actionToolTip = new ToolTip();
            _actionToolTip.IsBalloon = false;
            _actionToolTip.ShowAlways = false;
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
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
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = false // Ana panel kaymasın, sadece tablo kayacak
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "Muhasebe",
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
                Location = new Point(30, 135),
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

            // DataGridView - ProductionListForm ile aynı yapı
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 170),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 210,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 40 },
                ScrollBars = ScrollBars.Vertical,
                Visible = _isTableView
            };
            _dataGridView.CellClick += DataGridView_CellClick;
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
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
                _cardsPanel.Height = _mainPanel.Height - 210;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 210;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(searchPanel);
            _mainPanel.Controls.Add(_chkTableView);
            _mainPanel.Controls.Add(_cardsPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadAccountingOrders()
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                string searchTerm = _txtSearch?.Text.Trim() ?? "";
                Guid? companyId = null;

                if (_cmbCompanyFilter?.SelectedItem != null)
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

                // Filtreleme ile siparişleri getir
                var orders = _orderRepository.GetAll(
                    string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    companyId
                ).ToList();

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
                var card = CreateAccountingCard(order);
                _cardsPanel.Controls.Add(card);
            }
        }

        private void LoadDataGridView(List<Order> orders)
        {
            _dataGridView.SuspendLayout();
            try
            {
                // Event handler'ları geçici olarak kaldır
                _dataGridView.DataBindingComplete -= DataGridView_DataBindingComplete;
                _dataGridView.RowsAdded -= DataGridView_RowsAdded;
                _dataGridView.RowPrePaint -= DataGridView_RowPrePaint;
                _dataGridView.CellPainting -= DataGridView_CellPainting;

            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();
                _dataGridView.Rows.Clear();
                _dataGridView.Tag = null;

                _dataGridView.Refresh();
                _dataGridView.Update();
                Application.DoEvents();

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
                    DataPropertyName = "CompanyName",
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
                    Width = 150,
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
                    CurrencyRate = o.CurrencyRate,
                IsInAccounting = o.Status == "Muhasebede"
            }).ToList();

            _dataGridView.DataSource = dataSource;
            _dataGridView.Tag = orders; // Orijinal order listesini sakla

                // Event handler'ları tekrar ekle
                _dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
                _dataGridView.RowsAdded += DataGridView_RowsAdded;
                _dataGridView.RowPrePaint += DataGridView_RowPrePaint;
                _dataGridView.CellPainting += DataGridView_CellPainting;

            // Stil ayarları
                _dataGridView.BackgroundColor = Color.White;
                _dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, ThemeColors.Primary.R, ThemeColors.Primary.G, ThemeColors.Primary.B);
                _dataGridView.GridColor = Color.FromArgb(230, 230, 230);
                _dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;
                _dataGridView.RowHeadersVisible = false;
                _dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                _dataGridView.BorderStyle = BorderStyle.None;

            // Buton kolonu stil
                if (_dataGridView.Columns["Actions"] != null)
                {
                    _dataGridView.Columns["Actions"].DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            _dataGridView.Columns["Actions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    _dataGridView.Columns["Actions"].DefaultCellStyle.Padding = new Padding(2, 2, 2, 2);
                }
                
                // Actions kolonundaki default tooltip'leri kapat
                _dataGridView.ShowCellToolTips = false;
                
                // Son bir güncelleme yap
                _dataGridView.Refresh();
            }
            finally
            {
                _dataGridView.ResumeLayout(true);
            }
        }

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Tüm satırlara renklendirme uygula
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                ApplyRowColorToRow(row);
            }
            UpdateActionButtons();
            _dataGridView.Invalidate();
        }

        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // Yeni eklenen satırlara renklendirme uygula
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i >= 0 && i < _dataGridView.Rows.Count)
                {
                    ApplyRowColorToRow(_dataGridView.Rows[i]);
                }
            }
            UpdateActionButtons();
            _dataGridView.Invalidate();
        }

        private void DataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            // Scroll sırasında görsel güncellemeyi zorla
            _dataGridView.Invalidate();
            _dataGridView.Update();
        }

        private void DataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // Satır renklendirmesi için RowPrePaint event'i
            if (e.RowIndex >= 0 && e.RowIndex < _dataGridView.Rows.Count)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                ApplyRowColorToRow(row);
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

                // Tag'dan Order listesini al
                List<Order> orders = null;
                Order order = null;
                if (_dataGridView.Tag is List<Order> tagOrders)
                {
                    orders = tagOrders;
                }

                // Order'ı bul
                if (row.DataBoundItem != null && orders != null)
                {
                    var rowData = row.DataBoundItem;
                    var idProperty = rowData.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var orderId = (Guid)idProperty.GetValue(rowData);
                        order = orders.FirstOrDefault(o => o.Id == orderId);
                    }
                }

                // Satır rengini al
                Color rowBgColor = order != null ? GetRowColor(order) : Color.White;

                // Actions kolonu için özel işlem
                if (isActionsColumn && order != null)
                        {
                            bool isInAccounting = order.Status == "Muhasebede";

                    // Actions kolonu için satır arka planını çiz
                    using (SolidBrush bgBrush = new SolidBrush(rowBgColor))
                    {
                        e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                    }

                    // Border'ı çiz
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

                    // Butonlar: 📋 Detay, 📝 İşle, 📦 Sevkıyata Gönder
                    string[] emojis = new[] { "📋", "📝", "📦" };
                    Color[] colors;
                                if (isInAccounting)
                                {
                        // Mavi, Turuncu, Yeşil (tümü aktif)
                        colors = new[] { Color.FromArgb(33, 150, 243), Color.FromArgb(255, 152, 0), Color.FromArgb(76, 175, 80) };
                                }
                                else
                                {
                        // Mavi aktif, diğerleri gri (pasif)
                        colors = new[] { Color.FromArgb(33, 150, 243), Color.FromArgb(200, 200, 200), Color.FromArgb(200, 200, 200) };
                    }

                    int emojiWidth = e.CellBounds.Width / emojis.Length;
                    Font emojiFont = new Font("Segoe UI Emoji", 12F);
                    int circleSize = 20;
                    int emojiSize = 14;

                    for (int i = 0; i < emojis.Length; i++)
                    {
                        int xCenter = e.CellBounds.X + (i * emojiWidth) + (emojiWidth / 2);
                        int yCenter = e.CellBounds.Y + (e.CellBounds.Height / 2);

                        int circleX = xCenter - (circleSize / 2);
                        int circleY = yCenter - (circleSize / 2);

                        // Renkli arka plan çemberi
                        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(70, colors[i])))
                        {
                            e.Graphics.FillEllipse(bgBrush, circleX, circleY, circleSize, circleSize);
                        }

                        // Renkli kenarlık
                        using (Pen borderPen = new Pen(colors[i], 1.5f))
                        {
                            e.Graphics.DrawEllipse(borderPen, circleX, circleY, circleSize, circleSize);
                        }

                        // Emoji'yi çiz
                        RectangleF emojiRect = new RectangleF(
                            xCenter - (emojiSize / 2f),
                            yCenter - (emojiSize / 2f),
                            emojiSize,
                            emojiSize
                        );

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

        private Color GetRowColor(Order order)
        {
            // Sadece muhasebede olan ürünler renklendirilir
            if (order.Status == "Muhasebede")
            {
                // Kuru girilmemiş (0 veya null) → Sarı
                if (order.CurrencyRate == null || order.CurrencyRate == 0)
                {
                    return Color.FromArgb(255, 255, 235, 59); // Sarı
                }
                
                // Kuru girilmiş → Yeşil
                if (order.CurrencyRate > 0)
                {
                    return Color.FromArgb(200, 76, 175, 80); // Yeşil
                }
            }
            
            // Diğerleri (muhasebede değilse veya diğer durumlarda) → Beyaz
            return Color.White;
        }

        private void ApplyRowColorToRow(DataGridViewRow row)
        {
            if (row == null || row.DataBoundItem == null) return;

            Order order = null;
            
            // Order'ı bul
            if (_dataGridView.Tag is List<Order> orders)
            {
                var rowData = row.DataBoundItem;
                var idProperty = rowData.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var orderId = (Guid)idProperty.GetValue(rowData);
                    order = orders.FirstOrDefault(o => o.Id == orderId);
                }
            }

            if (order == null) return;

            Color rowColor = GetRowColor(order);

            // Satır seviyesinde arka plan rengi uygula
            row.DefaultCellStyle.BackColor = rowColor;
            row.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;

            // Her hücreye ayrı ayrı uygula (Actions kolonu hariç)
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn != null && cell.OwningColumn.Name != "Actions")
                {
                    cell.Style.BackColor = rowColor;
                    cell.Style.ForeColor = ThemeColors.TextPrimary;
                    cell.Style.Padding = new Padding(0);
                }
            }
        }

        private void UpdateActionButtons()
        {
            // Butonlar CellPainting'de çizildiği için burada sadece placeholder değer veriyoruz
            if (_dataGridView.Columns["Actions"] == null) return;

            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                if (row.Cells["Actions"] != null)
                {
                    row.Cells["Actions"].Value = "📋 📝 📦"; // Placeholder
                }
            }
        }

        private Panel CreateSearchPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // TableLayoutPanel ile responsive yapı
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Kolon genişliklerini yüzdelik olarak ayarla
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // Arama kutusu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Firma:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // Firma combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Yenile butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Sağ boşluk

            // Ara
            var lblSearch = new Label
            {
                Text = "Ara:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(0, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblSearch, 0, 0);

            _txtSearch = new TextBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformSearch(); };
            tableLayout.Controls.Add(_txtSearch, 1, 0);

            // Firma
            var lblCompany = new Label
            {
                Text = "Firma:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblCompany, 2, 0);

            _cmbCompanyFilter = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            LoadCompaniesForFilter();
            tableLayout.Controls.Add(_cmbCompanyFilter, 3, 0);

            // Ara butonu
            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Primary, Color.White, 80, 30);
            _btnSearch.Click += (s, e) => PerformSearch();
            tableLayout.Controls.Add(_btnSearch, 4, 0);

            // Yenile butonu
            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Success, Color.White, 90, 30);
            _btnRefresh.Click += (s, e) =>
            {
                _txtSearch.Text = "";
                _cmbCompanyFilter.SelectedIndex = 0;
                PerformSearch();
            };
            tableLayout.Controls.Add(_btnRefresh, 5, 0);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private void LoadCompaniesForFilter()
        {
            try
            {
                _cmbCompanyFilter.Items.Clear();
                
                // Tüm Firmalar seçeneği
                _cmbCompanyFilter.Items.Add(new { Id = (Guid?)null, Name = "Tüm Firmalar" });
                
                // Firmaları yükle
                var companies = _companyRepository.GetAll().OrderBy(c => c.Name).ToList();
                foreach (var company in companies)
                {
                    _cmbCompanyFilter.Items.Add(new { Id = (Guid?)company.Id, Name = company.Name });
                }
                
                _cmbCompanyFilter.DisplayMember = "Name";
                _cmbCompanyFilter.ValueMember = "Id";
                _cmbCompanyFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChkTableView_CheckedChanged(object sender, EventArgs e)
        {
            _isTableView = _chkTableView.Checked;
            _cardsPanel.Visible = !_isTableView;
            _dataGridView.Visible = _isTableView;
            PerformSearch(); // Filtreleri koruyarak arama yap
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
                    bool isInAccounting = order.Status == "Muhasebede";

                    // Butonlar: 📋 Detay, 📝 İşle, 📦 Sevkıyata Gönder
                    string[] tooltips = new[] { "Detay", "İşle", "Sevkıyata Gönder" };

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
            _actionToolTip.Hide(_dataGridView);
            _currentToolTipText = "";
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
                    var buttonWidth = cellRect.Width / 3; // 3 buton var: 📋 📝 📦

                    int buttonIndex = (int)(clickX / buttonWidth);

                    switch (buttonIndex)
                    {
                        case 0: // 📋 Detay (her zaman aktif)
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
                        case 2: // 📦 Sevkıyata Gönder
                            if (!isInAccounting)
                            {
                                MessageBox.Show("Bu sipariş muhasebede değil, sevkıyata gönderilemez.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            var result = MessageBox.Show(
                                $"Sipariş {order.TrexOrderNo} sevkıyata gönderilecek. Emin misiniz?",
                                "Sevkıyata Gönder",
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

