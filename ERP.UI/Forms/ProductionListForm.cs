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
    public partial class ProductionListForm : UserControl
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
        private ComboBox _cmbModelFilter;
        private ComboBox _cmbLamelThicknessFilter;
        private Button _btnSearch;
        private Button _btnRefresh;

        public event EventHandler<Guid> ProductionDetailRequested;
        public event EventHandler<Guid> ProductionSendToAccountingRequested;
        public event EventHandler<Guid> ProductionReportRequested;

        public ProductionListForm()
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
            LoadProductionOrders();
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
                Text = "Üretim",
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
                ColumnCount = 12, // 12 kolon: Ara(2), Firma(3), Model(2), Lamel(2), Butonlar(3)
                RowCount = 1,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Kolon genişliklerini yüzdelik olarak ayarla
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F)); // Arama kutusu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Firma:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // Firma combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Model:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F)); // Model combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Lamel:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F)); // Lamel combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 3F)); // Boşluk
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Yenile butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 3F)); // Sağ boşluk

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

            // Model
            var lblModel = new Label
            {
                Text = "Model:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblModel, 4, 0);

            _cmbModelFilter = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            LoadModelFilter();
            tableLayout.Controls.Add(_cmbModelFilter, 5, 0);

            // Lamel
            var lblLamelThickness = new Label
            {
                Text = "Lamel:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblLamelThickness, 6, 0);

            _cmbLamelThicknessFilter = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            LoadLamelThicknessFilter();
            tableLayout.Controls.Add(_cmbLamelThicknessFilter, 7, 0);

            // Butonlar
            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Info, Color.White, 90, 30);
            _btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _btnSearch.Margin = new Padding(5, 12, 5, 8);
            _btnSearch.Click += (s, e) => PerformSearch();
            tableLayout.Controls.Add(_btnSearch, 9, 0);

            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Secondary, Color.White, 90, 30);
            _btnRefresh.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _btnRefresh.Margin = new Padding(5, 12, 5, 8);
            _btnRefresh.Click += (s, e) => {
                _txtSearch.Text = "";
                _cmbCompanyFilter.SelectedIndex = 0;
                _cmbModelFilter.SelectedIndex = 0;
                _cmbLamelThicknessFilter.SelectedIndex = 0;
                PerformSearch();
            };
            tableLayout.Controls.Add(_btnRefresh, 10, 0);

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

        private void LoadModelFilter()
        {
            _cmbModelFilter.Items.Clear();
            _cmbModelFilter.Items.Add(new { Value = (string)null, Name = "Tüm Modeller" });
            _cmbModelFilter.Items.Add(new { Value = "H", Name = "H (3.25)" });
            _cmbModelFilter.Items.Add(new { Value = "D", Name = "D (4.5)" });
            _cmbModelFilter.Items.Add(new { Value = "M", Name = "M (6.5)" });
            _cmbModelFilter.Items.Add(new { Value = "L", Name = "L (9)" });
            _cmbModelFilter.DisplayMember = "Name";
            _cmbModelFilter.ValueMember = "Value";
            _cmbModelFilter.SelectedIndex = 0;
        }

        private void LoadLamelThicknessFilter()
        {
            try
            {
                _cmbLamelThicknessFilter.Items.Clear();
                _cmbLamelThicknessFilter.Items.Add(new { Value = (decimal?)null, Name = "Tüm Kalınlıklar" });
                
                // Tüm siparişlerden unique lamel kalınlıklarını al
                var allOrders = _orderRepository.GetAll();
                var uniqueThicknesses = allOrders
                    .Where(o => o.LamelThickness.HasValue)
                    .Select(o => o.LamelThickness.Value)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                foreach (var thickness in uniqueThicknesses)
                {
                    _cmbLamelThicknessFilter.Items.Add(new { Value = (decimal?)thickness, Name = thickness.ToString("0.000") });
                }
                
                _cmbLamelThicknessFilter.DisplayMember = "Name";
                _cmbLamelThicknessFilter.ValueMember = "Value";
                _cmbLamelThicknessFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lamel kalınlıkları yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string modelFilter = null;
            decimal? lamelThicknessFilter = null;

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

            if (_cmbModelFilter.SelectedItem != null)
            {
                var selected = _cmbModelFilter.SelectedItem;
                var valueProperty = selected.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    modelFilter = valueProperty.GetValue(selected) as string;
                }
            }

            if (_cmbLamelThicknessFilter.SelectedItem != null)
            {
                var selected = _cmbLamelThicknessFilter.SelectedItem;
                var valueProperty = selected.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    lamelThicknessFilter = valueProperty.GetValue(selected) as decimal?;
                }
            }

            LoadProductionOrders(searchTerm, companyId, modelFilter, lamelThicknessFilter);
        }

        private void LoadProductionOrders(string searchTerm = null, Guid? companyId = null, string modelFilter = null, decimal? lamelThicknessFilter = null)
        {
            try
            {
                // Tüm siparişleri getir
                var allOrders = _orderRepository.GetAll(searchTerm, companyId).ToList();

                // Model filtresi uygula
                if (!string.IsNullOrEmpty(modelFilter))
                {
                    allOrders = allOrders.Where(o => GetModelLetterFromProductCode(o.ProductCode) == modelFilter.ToUpper()).ToList();
                }

                // Lamel kalınlığı filtresi uygula
                if (lamelThicknessFilter.HasValue)
                {
                    allOrders = allOrders.Where(o => o.LamelThickness.HasValue && Math.Abs(o.LamelThickness.Value - lamelThicknessFilter.Value) < 0.001m).ToList();
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

        private string GetModelLetterFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
                return "";

            try
            {
                var parts = productCode.Split('-');
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2]; // LG veya HS gibi
                    if (modelProfile.Length > 0)
                    {
                        return modelProfile[0].ToString().ToUpper(); // L veya H
                    }
                }
            }
            catch { }

            return "";
        }

        private string GetModelLetterWithSize(string productCode)
        {
            string modelLetter = GetModelLetterFromProductCode(productCode);
            if (string.IsNullOrEmpty(modelLetter))
                return "";

            char modelChar = modelLetter.Length > 0 ? modelLetter[0] : ' ';
            decimal htave = GetHtave(modelChar);
            
            if (htave > 0)
                return $"{modelLetter} ({htave.ToString("F2")})";
            else
                return modelLetter;
        }

        private decimal GetHtave(char modelLetter)
        {
            switch (char.ToUpper(modelLetter))
            {
                case 'H': return 3.25m;
                case 'D': return 4.5m;
                case 'M': return 6.5m;
                case 'L': return 9m;
                default: return 0m;
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
                var card = CreateProductionCard(order);
                _cardsPanel.Controls.Add(card);
            }
        }

        private void LoadDataGridView(List<Order> orders)
        {
            // Layout işlemlerini durdur - daha hızlı ve temiz yenileme için
            _dataGridView.SuspendLayout();
            
            try
            {
                // Event handler'ları geçici olarak kaldır (üst üste gelmeyi önlemek için)
                _dataGridView.DataBindingComplete -= DataGridView_DataBindingComplete;
                _dataGridView.RowsAdded -= DataGridView_RowsAdded;
                _dataGridView.RowPrePaint -= DataGridView_RowPrePaint;
                _dataGridView.CellPainting -= DataGridView_CellPainting;

                // DataSource'u sıfırla ve kolonları temizle
                _dataGridView.DataSource = null;
                _dataGridView.Columns.Clear();
                _dataGridView.Rows.Clear();
                _dataGridView.Tag = null;

                // Görsel güncellemeyi zorla
                _dataGridView.Refresh();
                _dataGridView.Update();
                
                // Application.DoEvents() çağırarak UI'ın güncellenmesini sağla
                Application.DoEvents();

                if (orders.Count == 0)
                {
                    return;
                }

                _dataGridView.AutoGenerateColumns = false;

                // Kolonları ekle - Trex Sipariş No, Firma, Ürün Kodu, Adet, Lamel Kalınlığı, Model
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "TrexOrderNo",
                    HeaderText = "Trex Sipariş No",
                    Name = "TrexOrderNo",
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
                    Width = 100
                });

                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "LamelThickness",
                    HeaderText = "Lamel Kalınlığı",
                    Name = "LamelThickness",
                    Width = 120
                });

                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ModelLetter",
                    HeaderText = "Model",
                    Name = "ModelLetter",
                    Width = 120
                });

                // İşlemler kolonu (emoji butonları)
                var actionsColumn = new DataGridViewButtonColumn
                {
                    HeaderText = "İşlemler",
                    Name = "Actions",
                    Width = 220,
                    Text = "",
                    UseColumnTextForButtonValue = false
                };
                _dataGridView.Columns.Add(actionsColumn);

                // DataSource için özel bir liste oluştur
                var dataSource = orders.Select(o => new
                {
                    o.Id,
                    o.TrexOrderNo,
                    CompanyName = o.Company?.Name ?? "",
                    o.ProductCode,
                    o.Quantity,
                    LamelThickness = o.LamelThickness.HasValue ? o.LamelThickness.Value.ToString("0.000") : "",
                    ModelLetter = GetModelLetterWithSize(o.ProductCode),
                    o.Status,
                    IsInProduction = o.Status == "Üretimde",
                    IsStockOrder = o.IsStockOrder
                }).ToList();

                _dataGridView.Tag = orders; // Orijinal order listesini sakla
                
                // DataSource'u ayarla
                _dataGridView.DataSource = dataSource;

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
                // Layout işlemlerini devam ettir
                _dataGridView.ResumeLayout(true);
            }
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
                            bool isInProduction = order.Status == "Üretimde";
                            bool isStockOrder = order.IsStockOrder;
                            var btnCell = row.Cells["Actions"] as DataGridViewButtonCell;
                            if (btnCell != null)
                            {
                                // Emoji değeri sadece placeholder - gerçek çizim CellPainting'de yapılacak
                                if (isInProduction && !isStockOrder)
                                {
                                    btnCell.Value = "💰 📋"; // Rapor, Ayrıntı, Muhasebeye Gönder
                                }
                                else
                                {
                                    btnCell.Value = "📄 📋"; // Rapor, Ayrıntı
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
            PerformSearch(); // Filtreleri koruyarak arama yap
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_dataGridView.Tag is List<Order> orders && e.RowIndex < orders.Count)
            {
                var order = orders[e.RowIndex];
                bool isInProduction = order.Status == "Üretimde";
                bool isStockOrder = order.IsStockOrder;

                // İşlemler kolonuna tıklandı
                if (_dataGridView.Columns[e.ColumnIndex].Name == "Actions")
                {
                    var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    var cellRect = _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var clickX = _dataGridView.PointToClient(Control.MousePosition).X - cellRect.X;
                    var showAccountingButton = isInProduction && !isStockOrder;
                    var emojiCount = showAccountingButton ? 3 : 2; // Rapor, Ayrıntı, (Muhasebe)
                    var emojiWidth = cellRect.Width / emojiCount;

                    int emojiIndex = (int)(clickX / emojiWidth);

                    if (showAccountingButton)
                    {
                        // 📄 📋 💰 - Rapor, Ayrıntı, Muhasebeye Gönder
                        switch (emojiIndex)
                        {
                            case 0: // 📄 Rapor
                                ProductionReportRequested?.Invoke(this, order.Id);
                                break;
                            case 1: // 📋 Ayrıntı
                                ProductionDetailRequested?.Invoke(this, order.Id);
                                break;
                            case 2: // 💰 Muhasebeye Gönder
                                var result = MessageBox.Show(
                                    $"Sipariş {order.TrexOrderNo} muhasebeye gönderilecek. Emin misiniz?",
                                    "Muhasebeye Gönder",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);
                                if (result == DialogResult.Yes)
                                {
                                    ProductionSendToAccountingRequested?.Invoke(this, order.Id);
                                }
                                break;
                        }
                    }
                    else
                    {
                        // 📄 📋 - Rapor, Ayrıntı
                        switch (emojiIndex)
                        {
                            case 0: // 📄 Rapor
                                ProductionReportRequested?.Invoke(this, order.Id);
                                break;
                            case 1: // 📋 Ayrıntı
                                ProductionDetailRequested?.Invoke(this, order.Id);
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
                    ProductionDetailRequested?.Invoke(this, order.Id);
                }
            }
        }

        private Panel CreateProductionCard(Order order)
        {
            var card = new Panel
            {
                Width = 350,
                Height = 420,
                BackColor = Color.White,
                Margin = new Padding(15),
                Padding = new Padding(20),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Üretimde olan siparişler için farklı arka plan rengi
            bool isInProduction = order.Status == "Üretimde";
            if (isInProduction)
            {
                card.BackColor = Color.FromArgb(255, 248, 249, 250); // Açık mavi-gri ton
                card.BorderStyle = BorderStyle.FixedSingle;
                card.Paint += (s, e) =>
                {
                    var rect = card.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(new Pen(ThemeColors.Info, 3), rect);
                };
            }

            // Yeni gelen sipariş için border rengi (Üretimde değilse)
            bool isNew = !isInProduction && (order.ModifiedDate == null || 
                        (DateTime.Now - order.ModifiedDate.Value).TotalHours < 24);
            
            if (isNew)
            {
                card.BorderStyle = BorderStyle.FixedSingle;
                card.Paint += (s, e) =>
                {
                    var rect = card.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(new Pen(ThemeColors.Warning, 2), rect);
                };
            }

            int yPos = 15;

            // Üretimde işareti
            if (isInProduction)
            {
                var lblProduction = new Label
                {
                    Text = "🏭 ÜRETİMDE",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ThemeColors.Info,
                    AutoSize = true,
                    Location = new Point(15, yPos)
                };
                card.Controls.Add(lblProduction);
                yPos += 25;
            }
            // Yeni işareti (Üretimde değilse)
            else if (isNew)
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

            // Durum
            var lblStatus = new Label
            {
                Text = $"Durum: {order.Status ?? "Yeni"}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = isInProduction ? ThemeColors.Info : ThemeColors.TextSecondary,
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

            // Adet
            var lblQuantity = new Label
            {
                Text = $"Adet: {order.Quantity}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(15, yPos)
            };
            yPos += 25;

            // Lamel Kalınlığı
            if (order.LamelThickness.HasValue)
            {
                var lblLamelThickness = new Label
                {
                    Text = $"Lamel Kalınlığı: {order.LamelThickness.Value.ToString("0.000")}",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(15, yPos)
                };
                card.Controls.Add(lblLamelThickness);
                yPos += 25;
            }
            yPos += 10;

            // Butonlar - Rapor, Ayrıntı, (Muhasebeye Gönder)
            var btnReport = ButtonFactory.CreateActionButton("📄 Rapor", ThemeColors.Info, Color.White, 100, 35);
            btnReport.Location = new Point(15, yPos);
            btnReport.Click += (s, e) => ProductionReportRequested?.Invoke(this, order.Id);
            card.Controls.Add(btnReport);

            var btnDetail = ButtonFactory.CreateActionButton("📋 Ayrıntı", ThemeColors.Primary, Color.White, 100, 35);
            btnDetail.Location = new Point(120, yPos);
            btnDetail.Click += (s, e) => ProductionDetailRequested?.Invoke(this, order.Id);
            card.Controls.Add(btnDetail);

            // Sadece üretimdeyse ve stok siparişi değilse muhasebeye gönder butonu (sarı)
            if (isInProduction && !order.IsStockOrder)
            {
                var btnSendToAccounting = ButtonFactory.CreateActionButton("💰 Muhasebe", ThemeColors.Warning, Color.White, 110, 35);
                btnSendToAccounting.Location = new Point(225, yPos);
                btnSendToAccounting.Click += (s, e) =>
                {
                    var result = MessageBox.Show(
                        $"Sipariş {order.TrexOrderNo} muhasebeye gönderilecek. Emin misiniz?",
                        "Muhasebeye Gönder",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        ProductionSendToAccountingRequested?.Invoke(this, order.Id);
                    }
                };
                card.Controls.Add(btnSendToAccounting);
            }

            card.Controls.Add(lblOrderNo);
            card.Controls.Add(lblCustomerOrderNo);
            card.Controls.Add(lblCompany);
            card.Controls.Add(lblQuantity);

            return card;
        }

        public void RefreshOrders()
        {
            LoadProductionOrders();
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

            // Sadece "Üretimde" durumu için mavi renklendirme (hafif saydam - Alpha değeri 120)
            if (status == "Üretimde")
            {
                rowColor = Color.FromArgb(120, 33, 150, 243); // Mavi
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
                        cell.Style.Padding = new Padding(0);
                    }
                    else
                    {
                        cell.Style.BackColor = rowColor;
                    }
                }
            }

            // Seçildiğinde de aynı rengi kullan
            row.DefaultCellStyle.SelectionBackColor = rowColor;
            row.DefaultCellStyle.SelectionForeColor = ThemeColors.TextPrimary;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn != null && cell.OwningColumn.Name != "Actions")
                {
                    cell.Style.SelectionBackColor = rowColor;
                    cell.Style.SelectionForeColor = ThemeColors.TextPrimary;
                }
            }
        }

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateActionButtons();
            
            // Satır renklendirmesi
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                if (row.DataBoundItem != null)
                {
                    ApplyRowColorToRow(row);
                }
            }
            
            _dataGridView.Invalidate();
        }

        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i >= 0 && i < _dataGridView.Rows.Count)
                {
                    ApplyRowColorToRow(_dataGridView.Rows[i]);
                }
            }
            _dataGridView.Invalidate();
        }

        private void DataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
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

                // Tag'dan Order listesini al
                List<Order> orders = null;
                if (_dataGridView.Tag is List<Order> tagOrders)
                {
                    orders = tagOrders;
                }

                // Status'u al
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

                if (string.IsNullOrEmpty(status) && orders != null && e.RowIndex < orders.Count)
                {
                    status = orders[e.RowIndex].Status ?? "";
                }

                // Satır rengini status'tan belirle - Sadece "Üretimde" durumu için mavi
                Color rowBgColor = Color.White;
                if (status == "Üretimde")
                {
                    rowBgColor = Color.FromArgb(120, 33, 150, 243); // Mavi
                }

                // Actions kolonu için özel işlem
                if (isActionsColumn && row.DataBoundItem != null && orders != null && e.RowIndex < orders.Count)
                {
                    var order = orders[e.RowIndex];
                    bool isInProduction = order.Status == "Üretimde";
                    bool isStockOrder = order.IsStockOrder;

                    // Actions kolonu için satır arka planını çiz
                    using (SolidBrush bgBrush = new SolidBrush(rowBgColor))
                    {
                        e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                    }

                    // Border'ı çiz
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

                    string[] emojis;
                    Color[] colors;
                    string[] tooltips;

                    if (isInProduction && !isStockOrder)
                    {
                        // 📄 📋 💰 - Rapor, Ayrıntı, Muhasebeye Gönder
                        emojis = new[] { "📄", "📋", "💰" };
                        colors = new[] { ThemeColors.Info, ThemeColors.Primary, ThemeColors.Warning };
                        tooltips = new[] { "Rapor", "Ayrıntı", "Muhasebeye Gönder" };
                    }
                    else
                    {
                        // 📄 📋 - Rapor, Ayrıntı
                        emojis = new[] { "📄", "📋" };
                        colors = new[] { ThemeColors.Info, ThemeColors.Primary };
                        tooltips = new[] { "Rapor", "Ayrıntı" };
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
                    bool isInProduction = order.Status == "Üretimde";
                    bool isStockOrder = order.IsStockOrder;

                    string[] tooltips;
                    if (isInProduction && !isStockOrder)
                    {
                        tooltips = new[] { "Rapor", "Ayrıntı", "Muhasebeye Gönder" };
                    }
                    else
                    {
                        tooltips = new[] { "Rapor", "Ayrıntı" };
                    }

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
    }
}

