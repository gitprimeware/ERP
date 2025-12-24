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
        private AssemblyRepository _assemblyRepository;
        private PackagingRepository _packagingRepository;
        private ClampingRequestRepository _clampingRequestRepository;
        private bool _isTableView = true; // Default tablo görünümü
        private ToolTip _actionToolTip;
        private string _currentToolTipText = "";
        
        // Sütun filtreleri
        private Panel _columnFilterPanel;
        private Dictionary<string, Control> _columnFilters = new Dictionary<string, Control>();
        private string _currentSortColumn = "";
        private enum SortDirection
        {
            None = 0,
            Ascending = 1,
            Descending = 2
        }
        private SortDirection _sortDirection = SortDirection.None;

        public event EventHandler<Guid> ProductionDetailRequested;
        public event EventHandler<Guid> ProductionSendToAccountingRequested;
        public event EventHandler<Guid> ProductionReportRequested;

        public ProductionListForm()
        {
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            _assemblyRepository = new AssemblyRepository();
            _packagingRepository = new PackagingRepository();
            _clampingRequestRepository = new ClampingRequestRepository();
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

            // Başlık ve görünüm switch'i - tek satır
            var titlePanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.None,
                Location = new Point(30, 30),
                Width = _mainPanel.Width - 60,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var titleLabel = new Label
            {
                Text = "Üretim",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            // Renk açıklamaları - basit Label'lar ile (performanslı)
            var colorLegendPanel = new Panel
            {
                Location = new Point(120, 12),
                Width = 400,
                Height = 25,
                BackColor = Color.Transparent
            };
            
            var legendItems = new[]
            {
                new { Color = Color.FromArgb(244, 67, 54), Text = "İşlem Yok" },
                new { Color = Color.FromArgb(255, 193, 7), Text = "Kenetleme" },
                new { Color = Color.FromArgb(33, 150, 243), Text = "Paketleme" },
                new { Color = Color.FromArgb(76, 175, 80), Text = "Gönderildi" }
            };
            
            int xPos = 0;
            var font = new Font("Segoe UI", 9F, FontStyle.Regular);
            var textColor = ThemeColors.TextPrimary;
            
            foreach (var item in legendItems)
            {
                // Renkli daire - Unicode karakter ile (●)
                var circleLabel = new Label
                {
                    Text = "●",
                    Font = font,
                    ForeColor = item.Color,
                    AutoSize = true,
                    Location = new Point(xPos, 4),
                    BackColor = Color.Transparent
                };
                colorLegendPanel.Controls.Add(circleLabel);
                xPos += circleLabel.Width + 3;
                
                // Metin
                var textLabel = new Label
                {
                    Text = item.Text,
                    Font = font,
                    ForeColor = textColor,
                    AutoSize = true,
                    Location = new Point(xPos, 4),
                    BackColor = Color.Transparent
                };
                colorLegendPanel.Controls.Add(textLabel);
                xPos += textLabel.Width + 12;
                
                // Ayırıcı (son değilse)
                if (item != legendItems.Last())
                {
                    var separatorLabel = new Label
                    {
                        Text = "|",
                        Font = font,
                        ForeColor = Color.LightGray,
                        AutoSize = true,
                        Location = new Point(xPos, 4),
                        BackColor = Color.Transparent
                    };
                    colorLegendPanel.Controls.Add(separatorLabel);
                    xPos += separatorLabel.Width + 5;
                }
            }

            _chkTableView = new CheckBox
            {
                Text = "📊 Tablo Görünümü",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(titlePanel.Width - 200, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Checked = _isTableView
            };
            _chkTableView.CheckedChanged += ChkTableView_CheckedChanged;

            titlePanel.Controls.Add(titleLabel);
            titlePanel.Controls.Add(colorLegendPanel);
            titlePanel.Controls.Add(_chkTableView);
            
            // Başlık panelinin yüksekliğini artır
            titlePanel.Height = 70;

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 120),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 160,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = !_isTableView
            };

            // DataGridView - Filtre panelinin altında, header'lar görünür olacak şekilde
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 160), // Filtre panelinin altında (120px + 40px = 160px)
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 200, // Filtre paneli için yükseklik azaltıldı
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
            _dataGridView.ColumnHeaderMouseClick += DataGridView_ColumnHeaderMouseClick;
            
            // DoubleBuffered özelliğini aç - scroll sırasında üst üste binmeyi önler
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, _dataGridView, new object[] { true });

            _mainPanel.Resize += (s, e) =>
            {
                if (titlePanel != null)
                    titlePanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Width = _mainPanel.Width - 60;
                _cardsPanel.Height = _mainPanel.Height - 160;
                _columnFilterPanel.Width = _mainPanel.Width - 60;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 200;
                UpdateColumnFilterPanel();
            };

            // Sütun filtre paneli (DataGridView header'larının üstünde)
            _columnFilterPanel = new Panel
            {
                Location = new Point(30, 120), // Başlığın hemen altında (70px titlePanel + 50px boşluk)
                Width = _mainPanel.Width - 60,
                Height = 40,
                BackColor = Color.FromArgb(245, 245, 245),
                Visible = _isTableView,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            _mainPanel.Controls.Add(titlePanel);
            _mainPanel.Controls.Add(_columnFilterPanel); // Önce filtre paneli (header'ların üstünde)
            _mainPanel.Controls.Add(_dataGridView); // Sonra DataGridView (filtre panelinin altında, header'lar görünür)
            _mainPanel.Controls.Add(_cardsPanel);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        // CreateSearchPanel ve ilgili metodlar kaldırıldı - artık sütun filtreleri kullanılıyor

        private void LoadProductionOrders()
        {
            try
            {
                // Tüm siparişleri getir
                var allOrders = _orderRepository.GetAll().ToList();

                if (_isTableView)
                {
                    LoadDataGridView(allOrders);
                    // Sütun filtrelerini uygula
                    ApplyColumnFilters();
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

        private (string Hatve, string PlakaOlcusu, string Yukseklik, string Kapak, string Profil) ParseProductCodeForTable(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
                return ("", "", "", "", "");

            try
            {
                // Format: TREX-CR-LG-1422-1900-030
                var parts = productCode.Split('-');
                if (parts.Length < 6)
                    return ("", "", "", "", "");

                string hatve = "";
                string plakaOlcusu = "";
                string yukseklik = "";
                string kapak = "";
                string profil = "";

                // Model ve Profil: LG -> Model: L, Profil: G
                string modelProfile = parts[2];
                if (modelProfile.Length >= 2)
                {
                    char modelLetter = modelProfile[0];
                    char profileLetter = modelProfile[1];

                    // Hatve: H, D, M, L olarak göster
                    hatve = modelLetter.ToString().ToUpper();

                    // Profil
                    profil = profileLetter.ToString().ToUpper();
                }

                // Plaka Ölçüsü (mm): Rapor formülüne göre hesapla ve yuvarla
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    // Plaka Ölçüsü com (mm): 1422 <= 1150 ise 1422, > 1150 ise 1422/2 = 711
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    
                    // 100'ün katlarına yuvarla: 711 -> 700
                    int roundedPlakaOlcusu = (plakaOlcusuComMM / 100) * 100;
                    plakaOlcusu = roundedPlakaOlcusu.ToString(); // MM cinsinden göster
                }

                // Kapak: 030 -> 30 (önce kapak değerini al ki yükseklikten çıkarabilelim)
                int kapakBoyuMM = 0;
                if (parts.Length >= 6)
                {
                    string kapakStr = parts[5];
                    if (int.TryParse(kapakStr, out int kapakValue))
                    {
                        kapak = kapakValue.ToString();
                        kapakBoyuMM = kapakValue;
                    }
                    else if (kapakStr == "030")
                    {
                        kapak = "30";
                        kapakBoyuMM = 30;
                    }
                    else if (kapakStr == "002")
                    {
                        kapak = "2";
                        kapakBoyuMM = 2;
                    }
                    else if (kapakStr == "016")
                    {
                        kapak = "16";
                        kapakBoyuMM = 16;
                    }
                }

                // Yükseklik: Yükseklik com'dan kapak boyunu çıkar
                // Yükseklik <= 1800 ise Yükseklik, > 1800 ise Yükseklik/2, sonra kapak boyunu çıkar
                if (parts.Length >= 5 && int.TryParse(parts[4], out int yukseklikMM))
                {
                    // Yükseklik com hesaplama
                    int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    
                    // Kapak boyunu çıkar
                    int yukseklikSon = yukseklikCom - kapakBoyuMM;
                    yukseklik = yukseklikSon.ToString();
                }

                return (hatve, plakaOlcusu, yukseklik, kapak, profil);
            }
            catch
            {
                return ("", "", "", "", "");
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

                // Kolonları ekle - İstenen sıraya göre
            // 1. Trex Sipariş No
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrexOrderNo",
                HeaderText = "Trex Sipariş No",
                Name = "TrexOrderNo",
                Width = 150,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 2. Üretim Kodu
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductCode",
                HeaderText = "Üretim Kodu",
                Name = "ProductCode",
                Width = 180,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 3. Hatve (küçük)
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Hatve",
                HeaderText = "Hatve",
                Name = "Hatve",
                Width = 70,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 4. Plaka Ölçüsü
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PlakaOlcusu",
                HeaderText = "Plaka Ölçüsü",
                Name = "PlakaOlcusu",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 5. Yükseklik
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Yukseklik",
                HeaderText = "Yükseklik",
                Name = "Yukseklik",
                Width = 100,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 6. Kapak (küçük)
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Kapak",
                HeaderText = "Kapak",
                Name = "Kapak",
                Width = 60,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 7. Profil (küçük)
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Profil",
                HeaderText = "Profil",
                Name = "Profil",
                Width = 60,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 8. Adet (küçük)
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 70,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 9. Lamel Kalınlığı
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LamelThickness",
                HeaderText = "Lamel Kalınlığı",
                Name = "LamelThickness",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 10. Firma
            var companyColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CompanyName",
                HeaderText = "Firma",
                Name = "CompanyName",
                Width = 200,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };
            _dataGridView.Columns.Add(companyColumn);

            // 11. Termin Tarihi
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TermDate",
                HeaderText = "Termin Tarihi",
                Name = "TermDate",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 12. Durum
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StatusText",
                HeaderText = "Durum",
                Name = "StatusText",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });

            // 13. İşlemler kolonu (emoji butonları)
            var actionsColumn = new DataGridViewButtonColumn
            {
                HeaderText = "İşlemler",
                Name = "Actions",
                Width = 220,
                Text = "",
                UseColumnTextForButtonValue = false
            };
            _dataGridView.Columns.Add(actionsColumn);

                // DataSource için özel bir liste oluştur - Ürün kodundan parse edilen değerlerle
                var dataSource = orders.Select(o => 
                {
                    var parsedData = ParseProductCodeForTable(o.ProductCode);
                    return new
            {
                o.Id,
                o.TrexOrderNo,
                o.ProductCode,
                        Hatve = parsedData.Hatve,
                        PlakaOlcusu = parsedData.PlakaOlcusu,
                        Yukseklik = parsedData.Yukseklik,
                        Kapak = parsedData.Kapak,
                        Profil = parsedData.Profil,
                o.Quantity,
                        LamelThickness = o.LamelThickness.HasValue ? o.LamelThickness.Value.ToString("0.000", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) : "",
                        CompanyName = o.Company?.Name ?? "",
                        TermDate = o.TermDate.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),
                        StatusText = GetStatusText(o),
                o.Status,
                        IsInProduction = o.Status == "Üretimde",
                        IsStockOrder = o.IsStockOrder
                    };
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
                _dataGridView.ColumnHeadersVisible = true; // Başlıklar görünür olmalı
                _dataGridView.ColumnHeadersHeight = 40;
                _dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                _dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.EnableHeadersVisualStyles = false;
                _dataGridView.RowHeadersVisible = false;
                _dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                _dataGridView.BorderStyle = BorderStyle.None;
                
                // Sütun filtre panelini güncelle
                UpdateColumnFilterPanel();

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
            _columnFilterPanel.Visible = _isTableView;
            if (_isTableView)
            {
                UpdateColumnFilterPanel();
            }
            LoadProductionOrders(); // Filtreleri koruyarak arama yap
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
                                // Paketleme kontrolü
                                var packagings = _packagingRepository.GetByOrderId(order.Id);
                                bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
                                
                                if (!hasCompletedPackaging)
                                {
                                    MessageBox.Show(
                                        "Bu siparişi muhasebeye göndermek için önce paketleme işleminin tamamlanmış olması gerekir.",
                                        "Uyarı",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                    break;
                                }
                                
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
                    // Paketleme kontrolü
                    var packagings = _packagingRepository.GetByOrderId(order.Id);
                    bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
                    
                    if (!hasCompletedPackaging)
                    {
                        MessageBox.Show(
                            "Bu siparişi muhasebeye göndermek için önce paketleme işleminin tamamlanmış olması gerekir.",
                            "Uyarı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    
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
            Guid orderId = Guid.Empty;

            // Status'u ve OrderId'yi al - önce DataBoundItem'dan
            if (row.DataBoundItem != null)
            {
                var rowData = row.DataBoundItem;
                var statusProperty = rowData.GetType().GetProperty("Status");
                if (statusProperty != null)
                {
                    status = statusProperty.GetValue(rowData)?.ToString() ?? "";
                }

                var idProperty = rowData.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    orderId = (Guid)idProperty.GetValue(rowData);
                }

                // Tag'dan da deneyelim (Order listesi)
                if (string.IsNullOrEmpty(status) && _dataGridView.Tag is List<Order> orders)
                {
                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        status = order.Status ?? "";
                    }
                }
            }

            Color rowColor = Color.White;

            // Order bilgisini al
            Order currentOrder = null;
            if (_dataGridView.Tag is List<Order> ordersList)
            {
                currentOrder = ordersList.FirstOrDefault(o => o.Id == orderId);
            }
            
            // Üretim geçtiyse (Muhasebede, Tamamlandı, Sevkiyata Hazır) veya Üretimde durumu için renklendirme yap
            if (orderId != Guid.Empty && currentOrder != null)
            {
                // Üretimden geçmiş mi kontrol et (Muhasebede, Tamamlandı, Sevkiyata Hazır veya ShipmentDate dolu ise)
                bool isProductionPassed = currentOrder.Status == "Muhasebede" || 
                                         currentOrder.Status == "Tamamlandı" || 
                                         currentOrder.Status == "Sevkiyata Hazır" ||
                                         currentOrder.ShipmentDate.HasValue;
                
                // Üretimden geçmişse yeşil
                if (isProductionPassed)
                {
                    rowColor = Color.FromArgb(120, 76, 175, 80); // Yeşil - Gönderildi
                }
                else if (status == "Üretimde")
                {
                    // Paketleme işlemi yapılmış mı kontrol et
                    var packagings = _packagingRepository.GetByOrderId(orderId);
                    bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
                    
                    // Paketleme yapılmışsa mavi
                    if (hasCompletedPackaging)
                    {
                        rowColor = Color.FromArgb(120, 33, 150, 243); // Mavi - Paketleme
                    }
                    else
                    {
                        // Kenetleme işlemi yapılmış mı kontrol et
                        var clampingRequests = _clampingRequestRepository.GetByOrderId(orderId);
                        bool hasClamping = clampingRequests.Any(cr => cr.IsActive);
                        
                        // Kenetleme yapılmışsa sarı
                        if (hasClamping)
                        {
                            rowColor = Color.FromArgb(120, 255, 193, 7); // Sarı - Kenetleme
                        }
                        else
                        {
                            // Hiç işlem yapılmamışsa kırmızı
                            rowColor = Color.FromArgb(120, 244, 67, 54); // Kırmızı - İşlem Yok
                        }
                    }
                }
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
                Guid orderId = Guid.Empty;
                if (row.DataBoundItem != null)
                {
                    var rowData = row.DataBoundItem;
                    var statusProperty = rowData.GetType().GetProperty("Status");
                    if (statusProperty != null)
                    {
                        status = statusProperty.GetValue(rowData)?.ToString() ?? "";
                    }
                    
                    var idProperty = rowData.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        orderId = (Guid)idProperty.GetValue(rowData);
                    }
                }

                if (string.IsNullOrEmpty(status) && orders != null && e.RowIndex < orders.Count)
                {
                    status = orders[e.RowIndex].Status ?? "";
                    if (orderId == Guid.Empty)
                    {
                        orderId = orders[e.RowIndex].Id;
                    }
                }

                // Satır rengini belirle
                Color rowBgColor = Color.White;
                
                // Order bilgisini al
                Order order = null;
                if (orders != null && e.RowIndex < orders.Count)
                {
                    order = orders[e.RowIndex];
                }
                else if (orders != null)
                {
                    order = orders.FirstOrDefault(o => o.Id == orderId);
                }
                else if (_dataGridView.Tag is List<Order> orderList)
                {
                    order = orderList.FirstOrDefault(o => o.Id == orderId);
                }
                
                // Üretim geçtiyse (Muhasebede, Tamamlandı, Sevkiyata Hazır) veya Üretimde durumu için renklendirme yap
                if (orderId != Guid.Empty && order != null)
                {
                    // Üretimden geçmiş mi kontrol et (Muhasebede, Tamamlandı, Sevkiyata Hazır veya ShipmentDate dolu ise)
                    bool isProductionPassed = order.Status == "Muhasebede" || 
                                             order.Status == "Tamamlandı" || 
                                             order.Status == "Sevkiyata Hazır" ||
                                             order.ShipmentDate.HasValue;
                    
                    // Üretimden geçmişse yeşil
                    if (isProductionPassed)
                    {
                        rowBgColor = Color.FromArgb(120, 76, 175, 80); // Yeşil - Gönderildi
                    }
                    else if (status == "Üretimde")
                    {
                        // Paketleme işlemi yapılmış mı kontrol et
                        var packagings = _packagingRepository.GetByOrderId(orderId);
                        bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
                        
                        // Paketleme yapılmışsa mavi
                        if (hasCompletedPackaging)
                        {
                            rowBgColor = Color.FromArgb(120, 33, 150, 243); // Mavi - Paketleme
                        }
                        else
                        {
                            // Kenetleme işlemi yapılmış mı kontrol et
                            var clampingRequests = _clampingRequestRepository.GetByOrderId(orderId);
                            bool hasClamping = clampingRequests.Any(cr => cr.IsActive);
                            
                            // Kenetleme yapılmışsa sarı
                            if (hasClamping)
                            {
                                rowBgColor = Color.FromArgb(120, 255, 193, 7); // Sarı - Kenetleme
                            }
                            else
                            {
                                // Hiç işlem yapılmamışsa kırmızı
                                rowBgColor = Color.FromArgb(120, 244, 67, 54); // Kırmızı - İşlem Yok
                            }
                        }
                    }
                }

                // Actions kolonu için özel işlem
                if (isActionsColumn && row.DataBoundItem != null && orders != null && e.RowIndex < orders.Count)
                {
                    var actionOrder = orders[e.RowIndex];
                    bool isInProduction = actionOrder.Status == "Üretimde";
                    bool isStockOrder = actionOrder.IsStockOrder;

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

        private string GetStatusText(Order order)
        {
            if (order == null) return "";
            
            // Üretimden geçmiş mi kontrol et (Muhasebede, Tamamlandı, Sevkiyata Hazır veya ShipmentDate dolu ise)
            bool isProductionPassed = order.Status == "Muhasebede" || 
                                     order.Status == "Tamamlandı" || 
                                     order.Status == "Sevkiyata Hazır" ||
                                     order.ShipmentDate.HasValue;
            
            if (isProductionPassed)
            {
                return "Gönderildi";
            }
            
            // Üretimde değilse durumu direkt döndür
            if (order.Status != "Üretimde")
            {
                return order.Status;
            }
            
            // Paketleme işlemi yapılmış mı kontrol et
            var packagings = _packagingRepository.GetByOrderId(order.Id);
            bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
            
            if (hasCompletedPackaging)
            {
                return "Paketleme";
            }
            
            // Kenetleme işlemi yapılmış mı kontrol et
            var clampingRequests = _clampingRequestRepository.GetByOrderId(order.Id);
            bool hasClamping = clampingRequests.Any(cr => cr.IsActive);
            
            if (hasClamping)
            {
                return "Kenetleme";
            }
            
            // Hiç işlem yapılmamışsa
            return "İşlem Yok";
        }

        private string GetPlaceholderText(string columnName)
        {
            switch (columnName)
            {
                case "TrexOrderNo":
                    return "Sipariş No Girin";
                case "ProductCode":
                    return "Üretim Kodu Girin";
                case "PlakaOlcusu":
                    return "Plaka Ölçüsü Girin";
                case "Yukseklik":
                    return "Yükseklik Girin";
                case "Quantity":
                    return "Adet Girin";
                case "TermDate":
                    return "Termin Tarihi Girin";
                default:
                    return "Ara...";
            }
        }

        private void UpdateColumnFilterPanel()
        {
            if (_columnFilterPanel == null || _dataGridView == null || _dataGridView.Columns.Count == 0)
                return;

            _columnFilterPanel.Controls.Clear();
            _columnFilters.Clear();

            int xPos = 0;
            int filterHeight = 35;

            foreach (DataGridViewColumn column in _dataGridView.Columns)
            {
                if (column.Name == "Actions")
                {
                    // İşlemler kolonu için filtre yok
                    xPos += column.Width;
                    continue;
                }

                Control filterControl = null;

                // Sütun tipine göre filtre kontrolü oluştur
                if (column.Name == "CompanyName")
                {
                    // Firma için ComboBox
                    var cmb = new ComboBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        DropDownStyle = ComboBoxStyle.DropDown,
                        Location = new Point(xPos, 2),
                        BackColor = Color.White
                    };
                    cmb.Items.Add("Tüm Firmalar");
                    var companies = _companyRepository.GetAll().OrderBy(c => c.Name).ToList();
                    foreach (var company in companies)
                    {
                        cmb.Items.Add(company.Name);
                    }
                    cmb.SelectedIndex = 0;
                    cmb.TextChanged += (s, e) => ApplyColumnFilters();
                    filterControl = cmb;
                }
                else if (column.Name == "Hatve")
                {
                    // Hatve için ComboBox
                    var cmb = new ComboBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(xPos, 2),
                        BackColor = Color.White
                    };
                    cmb.Items.Add("Tüm Hatveler");
                    cmb.Items.Add("H");
                    cmb.Items.Add("D");
                    cmb.Items.Add("M");
                    cmb.Items.Add("L");
                    cmb.SelectedIndex = 0;
                    cmb.SelectedIndexChanged += (s, e) => ApplyColumnFilters();
                    filterControl = cmb;
                }
                else if (column.Name == "Kapak")
                {
                    // Kapak için ComboBox (30, 2)
                    var cmb = new ComboBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(xPos, 2),
                        BackColor = Color.White
                    };
                    cmb.Items.Add("Tüm Kapaklar");
                    cmb.Items.Add("30");
                    cmb.Items.Add("2");
                    cmb.SelectedIndex = 0;
                    cmb.SelectedIndexChanged += (s, e) => ApplyColumnFilters();
                    filterControl = cmb;
                }
                else if (column.Name == "Profil")
                {
                    // Profil için ComboBox (S, G)
                    var cmb = new ComboBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(xPos, 2),
                        BackColor = Color.White
                    };
                    cmb.Items.Add("Tüm Profiller");
                    cmb.Items.Add("S");
                    cmb.Items.Add("G");
                    cmb.SelectedIndex = 0;
                    cmb.SelectedIndexChanged += (s, e) => ApplyColumnFilters();
                    filterControl = cmb;
                }
                else if (column.Name == "LamelThickness")
                {
                    // Lamel Kalınlığı için ComboBox (0,100, 0,120, 0,150, 0,165, 0,180) - virgülle
                    var cmb = new ComboBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(xPos, 2),
                        BackColor = Color.White
                    };
                    cmb.Items.Add("Tüm Kalınlıklar");
                    cmb.Items.Add("0,100");
                    cmb.Items.Add("0,120");
                    cmb.Items.Add("0,150");
                    cmb.Items.Add("0,165");
                    cmb.Items.Add("0,180");
                    cmb.SelectedIndex = 0;
                    cmb.SelectedIndexChanged += (s, e) => ApplyColumnFilters();
                    filterControl = cmb;
                }
                else
                {
                    // Diğer sütunlar için TextBox (placeholder ile)
                    var txt = new TextBox
                    {
                        Width = column.Width - 2,
                        Height = filterHeight,
                        Font = new Font("Segoe UI", 9F),
                        Location = new Point(xPos, 2),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    
                    // Placeholder metni belirle
                    string placeholder = GetPlaceholderText(column.Name);
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                    txt.Tag = placeholder; // Orijinal placeholder'ı sakla
                    
                    // Focus olayları ile placeholder işlevi
                    txt.Enter += (s, e) =>
                    {
                        if (txt.Text == placeholder)
                        {
                            txt.Text = "";
                            txt.ForeColor = Color.Black;
                        }
                    };
                    txt.Leave += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(txt.Text))
                        {
                            txt.Text = placeholder;
                            txt.ForeColor = Color.Gray;
                            ApplyColumnFilters();
                        }
                    };
                    
                    txt.TextChanged += (s, e) =>
                    {
                        // Placeholder text ise filtreleme yapma
                        if (txt.Text != placeholder && txt.ForeColor != Color.Gray)
                        {
                            ApplyColumnFilters();
                        }
                    };
                    filterControl = txt;
                }

                if (filterControl != null)
                {
                    _columnFilterPanel.Controls.Add(filterControl);
                    _columnFilters[column.Name] = filterControl;
                }

                xPos += column.Width;
            }
        }

        private void ApplyColumnFilters()
        {
            if (!_isTableView || _dataGridView.Tag == null)
                return;

            try
            {
                // Orijinal veriyi Tag'den al (filtrelenmemiş)
                if (!(_dataGridView.Tag is List<Order> originalOrders))
                    return;

                var filteredData = originalOrders.Select(o => 
                {
                    var parsedData = ParseProductCodeForTable(o.ProductCode);
                    return new
                    {
                        o.Id,
                        o.TrexOrderNo,
                        o.ProductCode,
                        Hatve = parsedData.Hatve,
                        PlakaOlcusu = parsedData.PlakaOlcusu,
                        Yukseklik = parsedData.Yukseklik,
                        Kapak = parsedData.Kapak,
                        Profil = parsedData.Profil,
                        o.Quantity,
                        LamelThickness = o.LamelThickness.HasValue ? o.LamelThickness.Value.ToString("0.000", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) : "",
                        CompanyName = o.Company?.Name ?? "",
                        TermDate = o.TermDate.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),
                        StatusText = GetStatusText(o),
                        o.Status,
                        IsInProduction = o.Status == "Üretimde",
                        IsStockOrder = o.IsStockOrder
                    };
                }).Cast<object>().ToList();

                // Her sütun filtresini uygula
                foreach (var kvp in _columnFilters)
                {
                    string columnName = kvp.Key;
                    Control filterControl = kvp.Value;

                    if (filterControl is TextBox txt && !string.IsNullOrWhiteSpace(txt.Text))
                    {
                        // Placeholder text ise filtreleme yapma
                        string placeholder = txt.Tag?.ToString() ?? "";
                        if (txt.Text == placeholder || txt.ForeColor == Color.Gray)
                            continue;
                            
                        string filterText = txt.Text.ToLower();
                        filteredData = filteredData.Where(item =>
                        {
                            var prop = item.GetType().GetProperty(columnName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(item);
                                return value != null && value.ToString().ToLower().Contains(filterText);
                            }
                            return true;
                        }).ToList();
                    }
                    else if (filterControl is ComboBox cmb)
                    {
                        // "Tümü" ile başlayan seçenekler için filtreleme yapma (SelectedIndex == 0)
                        if (cmb.SelectedIndex > 0 && !cmb.SelectedItem.ToString().StartsWith("Tüm"))
                        {
                            string filterValue = cmb.SelectedItem.ToString();
                            
                            // LamelThickness için özel kontrol - virgül/nokta dönüşümü
                            if (columnName == "LamelThickness")
                            {
                                // ComboBox'tan gelen değer: "0,100" şeklinde
                                // Tablodaki değer: "0,100" şeklinde (Türkçe format)
                                // Her iki tarafı da normalize et
                                string normalizedFilter = filterValue.Replace(".", ",").Trim();
                                
                                filteredData = filteredData.Where(item =>
                                {
                                    var prop = item.GetType().GetProperty(columnName);
                                    if (prop != null)
                                    {
                                        var value = prop.GetValue(item)?.ToString() ?? "";
                                        // Değeri normalize et (virgül/nokta)
                                        string normalizedValue = value.Replace(".", ",").Trim();
                                        // Tam eşitlik kontrolü
                                        return normalizedValue == normalizedFilter;
                                    }
                                    return true;
                                }).ToList();
                            }
                            else
                            {
                                // Diğer ComboBox'lar için normal eşitlik kontrolü
                                filteredData = filteredData.Where(item =>
                                {
                                    var prop = item.GetType().GetProperty(columnName);
                                    if (prop != null)
                                    {
                                        var value = prop.GetValue(item);
                                        return value != null && value.ToString().Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                                    }
                                    return true;
                                }).ToList();
                            }
                        }
                        // SelectedIndex == 0 ise (Tümü) filtreleme yapma, tüm veriler kalsın
                    }
                }

                // Sıralama uygula
                if (!string.IsNullOrEmpty(_currentSortColumn) && _sortDirection != SortDirection.None)
                {
                    var prop = filteredData.FirstOrDefault()?.GetType().GetProperty(_currentSortColumn);
                    if (prop != null)
                    {
                        if (_sortDirection == SortDirection.Ascending)
                        {
                            filteredData = filteredData.OrderBy(item => prop.GetValue(item)).ToList();
                        }
                        else if (_sortDirection == SortDirection.Descending)
                        {
                            filteredData = filteredData.OrderByDescending(item => prop.GetValue(item)).ToList();
                        }
                    }
                }

                // DataSource'u güncelle - Tag'i koru (orijinal order listesi)
                var bindingSource = new BindingSource();
                foreach (var item in filteredData)
                {
                    bindingSource.Add(item);
                }
                _dataGridView.DataSource = bindingSource;
                // Tag'i koru - orijinal order listesi kalmalı
                _dataGridView.Tag = originalOrders;
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et
                System.Diagnostics.Debug.WriteLine($"Filtre uygulanırken hata: {ex.Message}");
            }
        }

        private void DataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || _dataGridView.Columns[e.ColumnIndex].Name == "Actions")
                return;

            string columnName = _dataGridView.Columns[e.ColumnIndex].Name;

            // Sıralama yönünü değiştir
            if (_currentSortColumn == columnName)
            {
                if (_sortDirection == SortDirection.None)
                    _sortDirection = SortDirection.Ascending;
                else if (_sortDirection == SortDirection.Ascending)
                    _sortDirection = SortDirection.Descending;
                else
                    _sortDirection = SortDirection.None;
            }
            else
            {
                _currentSortColumn = columnName;
                _sortDirection = SortDirection.Ascending;
            }

            // Header'da sıralama işaretini göster
            UpdateSortIndicators();

            // Filtreleri uygula (sıralama dahil)
            ApplyColumnFilters();
        }

        private void UpdateSortIndicators()
        {
            foreach (DataGridViewColumn column in _dataGridView.Columns)
            {
                string originalHeader = column.HeaderText.Replace(" ▲", "").Replace(" ▼", "");

                if (column.Name == _currentSortColumn)
                {
                    if (_sortDirection == SortDirection.Ascending)
                        column.HeaderText = originalHeader + " ▲";
                    else if (_sortDirection == SortDirection.Descending)
                        column.HeaderText = originalHeader + " ▼";
                    else
                        column.HeaderText = originalHeader;
                }
                else
                {
                    column.HeaderText = originalHeader;
                }
            }
        }
    }
}

