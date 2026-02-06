using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;
using ERP.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ERP.UI.Forms
{
    public partial class ConsumptionListForm : UserControl
    {
        private Panel _mainPanel;
        private FlowLayoutPanel _cardsPanel;
        private DataGridView _dataGridView;
        private CheckBox _chkTableView;
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;
        private bool _isTableView = true; // Default tablo görünümü
        private TextBox _txtSearch;
        private ComboBox _cmbCompanyFilter;
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpEndDate;
        private Button _btnSearch;
        private Button _btnRefresh;
        private Button _btnExportExcel;

        public event EventHandler<Guid> ConsumptionDetailRequested;

        public ConsumptionListForm()
        {
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            InitializeCustomComponents();
            
            // Load event'inde boyutları güncelle
            this.Load += ConsumptionListForm_Load;
        }

        private void ConsumptionListForm_Load(object sender, EventArgs e)
        {
            // Form yüklendiğinde tablo boyutlarını güncelle
            if (_dataGridView != null && _mainPanel != null)
            {
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 210;
            }
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            PerformSearch();
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
                Text = "Sarfiyat",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Filtreleme paneli
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
                GridColor = Color.FromArgb(230, 230, 230),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersVisible = false,
                Visible = _isTableView
            };
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
            _dataGridView.CellPainting += DataGridView_CellPainting;
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
                ColumnCount = 12,
                RowCount = 1,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Kolon genişliklerini yüzdelik olarak ayarla
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F)); // Arama kutusu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Firma:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // Firma combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Başlangıç:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // Başlangıç tarihi
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Bitiş:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // Bitiş tarihi
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Yenile butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Excel butonu
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

            // Başlangıç Tarihi
            var lblStartDate = new Label
            {
                Text = "Başlangıç:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblStartDate, 4, 0);

            _dtpStartDate = new DateTimePicker
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            _dtpStartDate.Value = DateTime.Now.AddMonths(-1); // Son 1 ay
            tableLayout.Controls.Add(_dtpStartDate, 5, 0);

            // Bitiş Tarihi
            var lblEndDate = new Label
            {
                Text = "Bitiş:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblEndDate, 6, 0);

            _dtpEndDate = new DateTimePicker
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            _dtpEndDate.Value = DateTime.Now;
            tableLayout.Controls.Add(_dtpEndDate, 7, 0);

            // Butonlar
            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Info, Color.White, 90, 30);
            _btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _btnSearch.Margin = new Padding(5, 12, 5, 8);
            _btnSearch.Click += (s, e) => PerformSearch();
            tableLayout.Controls.Add(_btnSearch, 8, 0);

            // refresh
            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Secondary, Color.White, 90, 30);
            _btnRefresh.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _btnRefresh.Margin = new Padding(5, 12, 5, 8);
            _btnRefresh.Click += (s, e) => {
                _txtSearch.Text = "";
                _cmbCompanyFilter.SelectedIndex = 0;
                _dtpStartDate.Value = DateTime.Now.AddMonths(-1);
                _dtpEndDate.Value = DateTime.Now;
                PerformSearch();
            };
            tableLayout.Controls.Add(_btnRefresh, 9, 0);

            // excel
            _btnExportExcel = ButtonFactory.CreateActionButton("📊 Excel'e Aktar", ThemeColors.Success, Color.White, 140, 30);
            _btnExportExcel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _btnExportExcel.Margin = new Padding(5, 12, 5, 8);
            _btnExportExcel.Click += BtnExportExcel_Click;
            tableLayout.Controls.Add(_btnExportExcel, 10, 0);

            panel.Controls.Add(tableLayout);
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
            // Tablo görünümü aktifse, tabloyu görünür yap
            if (_isTableView && _dataGridView != null)
            {
                _dataGridView.Visible = true;
                _cardsPanel.Visible = false;
            }
            
            string searchTerm = _txtSearch.Text.Trim();
            Guid? companyId = null;
            DateTime? startDate = _dtpStartDate.Value.Date;
            DateTime? endDate = _dtpEndDate.Value.Date.AddDays(1).AddTicks(-1); // Günün sonuna kadar

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

            LoadConsumptionOrders(searchTerm, companyId, startDate, endDate);
        }

        private void LoadConsumptionOrders(string searchTerm = null, Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Tüm siparişleri getir
                var allOrders = _orderRepository.GetAll(searchTerm, companyId).ToList();

                // Tarih filtresi uygula
                if (startDate.HasValue)
                {
                    allOrders = allOrders.Where(o => o.CreatedDate >= startDate.Value).ToList();
                }
                if (endDate.HasValue)
                {
                    allOrders = allOrders.Where(o => o.CreatedDate <= endDate.Value).ToList();
                }

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
                var card = CreateConsumptionCard(order);
                _cardsPanel.Controls.Add(card);
            }
        }

        private void LoadDataGridView(List<Order> orders)
        {
            try
            {
                // Event handler'ları geçici olarak kaldır
                _dataGridView.DataBindingComplete -= DataGridView_DataBindingComplete;
                _dataGridView.RowsAdded -= DataGridView_RowsAdded;

                // Layout işlemlerini durdur
                _dataGridView.SuspendLayout();

                // DataSource'u sıfırla ve kolonları temizle
                _dataGridView.DataSource = null;
                _dataGridView.Columns.Clear();
                _dataGridView.Rows.Clear();
                _dataGridView.Tag = null;

                // Görsel güncellemeyi zorla
                _dataGridView.Refresh();
                _dataGridView.Update();
                Application.DoEvents();

                if (orders.Count == 0)
                {
                    _dataGridView.ResumeLayout(true);
                    return;
                }

                _dataGridView.AutoGenerateColumns = false;

                // Kolonları ekle - Tarih kolonu en başta
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "CreatedDate",
                    HeaderText = "Tarih",
                    Name = "CreatedDate",
                    Width = 100
                });

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
                    Width = 100
                });

                // Stil ayarları - DataSource'dan ÖNCE
                _dataGridView.BackgroundColor = Color.White;
                _dataGridView.DefaultCellStyle.BackColor = Color.White;
                _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
                _dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, ThemeColors.Primary.R, ThemeColors.Primary.G, ThemeColors.Primary.B);
                _dataGridView.DefaultCellStyle.SelectionForeColor = ThemeColors.TextSecondary;
                _dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
                _dataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
                _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                _dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                _dataGridView.ColumnHeadersHeight = 40;
                _dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                _dataGridView.EnableHeadersVisualStyles = false;
                _dataGridView.GridColor = Color.FromArgb(230, 230, 230);
                _dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                _dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                _dataGridView.BorderStyle = BorderStyle.None;
                _dataGridView.RowHeadersVisible = false;

                // DataSource için özel bir liste oluştur
                var dataSource = orders.Select(o => new
                {
                    o.Id,
                    CreatedDate = o.CreatedDate.ToString("dd.MM.yyyy"),
                    o.TrexOrderNo,
                    o.CustomerOrderNo,
                    CompanyName = o.Company?.Name ?? "",
                    o.ProductCode,
                    o.Quantity,
                    o.Status
                }).ToList();

                _dataGridView.DataSource = dataSource;
                _dataGridView.Tag = orders; // Orijinal order listesini sakla

                // Event handler'ları tekrar ekle
                _dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
                _dataGridView.RowsAdded += DataGridView_RowsAdded;

                // Son bir güncelleme yap
                _dataGridView.Refresh();
            }
            finally
            {
                // Layout işlemlerini devam ettir
                _dataGridView.ResumeLayout(true);
            }
        }

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Tüm satırlara beyaz arka plan uygula
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
                
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = Color.White;
                    cell.Style.ForeColor = ThemeColors.TextPrimary;
                }
            }
            _dataGridView.Invalidate();
        }

        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // Yeni eklenen satırlara beyaz arka plan uygula
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i >= 0 && i < _dataGridView.Rows.Count)
                {
                    var row = _dataGridView.Rows[i];
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
                    
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.White;
                        cell.Style.ForeColor = ThemeColors.TextPrimary;
                    }
                }
            }
            _dataGridView.Invalidate();
        }

        private void ChkTableView_CheckedChanged(object sender, EventArgs e)
        {
            _isTableView = _chkTableView.Checked;
            _cardsPanel.Visible = !_isTableView;
            _dataGridView.Visible = _isTableView;
            PerformSearch();
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Header'a tıklanmışsa işlem yapma
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
            {
                var order = orders[e.RowIndex];
                ConsumptionDetailRequested?.Invoke(this, order.Id);
            }
        }

        private Panel CreateConsumptionCard(Order order)
        {
            var card = new Panel
            {
                Width = 350,
                Height = 300,
                BackColor = ThemeColors.Surface,
                Margin = new Padding(15),
                Padding = new Padding(20)
            };

            UIHelper.ApplyCardStyle(card, 8);

            int yPos = 15;

            // Durum
            var lblStatus = new Label
            {
                Text = $"Durum: {order.Status ?? "Yeni"}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            card.Controls.Add(lblStatus);
            yPos += 25;

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

            // Adet
            var lblQuantity = new Label
            {
                Text = $"Adet: {order.Quantity}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 35;

            // Detay butonu
            var btnDetail = ButtonFactory.CreateActionButton("📋 Detay", ThemeColors.Info, Color.White, 150, 35);
            btnDetail.Location = new Point(15, yPos);
            btnDetail.Click += (s, e) => ConsumptionDetailRequested?.Invoke(this, order.Id);

            card.Controls.Add(lblOrderNo);
            card.Controls.Add(lblCustomerOrderNo);
            card.Controls.Add(lblCompany);
            card.Controls.Add(lblQuantity);
            card.Controls.Add(btnDetail);

            return card;
        }

        public void RefreshOrders()
        {
            PerformSearch();
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
                // Önce hücreyi tamamen beyaz arka planla temizle
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);
                
                // Hücre içeriğini çiz
                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
                
                e.Handled = true;
            }
        }
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (_dataGridView.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Aktarılacak sipariş bulunamadı.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ExcelExportHelper.ExportToExcel(
                _dataGridView,
                defaultFileName: "Sarf",
                sheetName: "Sarf Kayıtları",
                skippedColumnNames: new[] { "Actions", "IsSelected" },
                title: "Sarf Listesi");
        }
    }
}

