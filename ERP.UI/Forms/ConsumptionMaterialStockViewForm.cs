using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ConsumptionMaterialStockViewForm : UserControl
    {
        private TabControl _tabControl;
        private CoverStockRepository _coverStockRepository;
        private SideProfileStockRepository _sideProfileStockRepository;
        private SideProfileRemnantRepository _sideProfileRemnantRepository;
        private IsolationStockRepository _isolationStockRepository;
        
        // Kapak tab kontrolleri
        private DataGridView _dgvKapak;

        // Yan Profil tab kontrolleri
        private DataGridView _dgvYanProfilStock;
        private DataGridView _dgvYanProfilDetail;

        // İzolasyon Sıvısı tab kontrolleri
        private DataGridView _dgvIzolasyon;

        public ConsumptionMaterialStockViewForm()
        {
            _coverStockRepository = new CoverStockRepository();
            _sideProfileStockRepository = new SideProfileStockRepository();
            _sideProfileRemnantRepository = new SideProfileRemnantRepository();
            _isolationStockRepository = new IsolationStockRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            CreateTabControl();
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(15, 5),
                Appearance = TabAppearance.FlatButtons
            };

            // Tab çizim modu
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.DrawItem += (s, e) =>
            {
                var tabPage = _tabControl.TabPages[e.Index];
                var tabRect = _tabControl.GetTabRect(e.Index);
                var textColor = e.Index == _tabControl.SelectedIndex ? ThemeColors.Primary : ThemeColors.TextSecondary;
                var backColor = e.Index == _tabControl.SelectedIndex ? Color.White : Color.FromArgb(245, 245, 245);

                // Arka plan
                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, tabRect);
                }

                // Metin
                using (var emojiFont = new Font("Segoe UI Emoji", 10F))
                {
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, emojiFont, 
                        tabRect, textColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                
                e.DrawFocusRectangle();
            };

            // Kapak Tab
            var tabKapak = new TabPage("🪟 Kapak");
            tabKapak.Padding = new Padding(0);
            tabKapak.BackColor = Color.White;
            tabKapak.UseVisualStyleBackColor = false;
            CreateTabContent(tabKapak, "Kapak");
            _tabControl.TabPages.Add(tabKapak);

            // Yan Profil Tab
            var tabYanProfil = new TabPage("📐 Yan Profil");
            tabYanProfil.Padding = new Padding(0);
            tabYanProfil.BackColor = Color.White;
            tabYanProfil.UseVisualStyleBackColor = false;
            CreateTabContent(tabYanProfil, "Yan Profil");
            _tabControl.TabPages.Add(tabYanProfil);

            // İzolasyon Sıvısı Tab
            var tabIzolasyon = new TabPage("💧 İzolasyon Sıvısı");
            tabIzolasyon.Padding = new Padding(0);
            tabIzolasyon.BackColor = Color.White;
            tabIzolasyon.UseVisualStyleBackColor = false;
            CreateTabContent(tabIzolasyon, "İzolasyon Sıvısı");
            _tabControl.TabPages.Add(tabIzolasyon);

            this.Controls.Add(_tabControl);
        }

        private void CreateTabContent(TabPage tab, string tabName)
        {
            if (tabName == "Kapak")
            {
                CreateKapakViewTabContent(tab);
            }
            else if (tabName == "Yan Profil")
            {
                CreateYanProfilViewTabContent(tab);
            }
            else if (tabName == "İzolasyon Sıvısı")
            {
                CreateIzolasyonViewTabContent(tab);
            }
            else
            {
                // Diğer tab'lar için placeholder
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(20)
                };

                var titleLabel = new Label
                {
                    Text = $"📋 {tabName} Stok Görüntüle",
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = ThemeColors.Primary,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };

                var infoLabel = new Label
                {
                    Text = "Bu sayfa yakında eklenecek...",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(20, 60)
                };

                panel.Controls.Add(titleLabel);
                panel.Controls.Add(infoLabel);
                tab.Controls.Add(panel);
            }
        }

        private void CreateYanProfilViewTabContent(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // İki kolonlu TableLayoutPanel
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Sol Panel - Ana Stok Girişleri
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            CreateYanProfilViewLeftPanel(leftPanel);
            tableLayout.Controls.Add(leftPanel, 0, 0);

            // Sağ Panel - Detaylı Görünüm
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            CreateYanProfilViewRightPanel(rightPanel);
            tableLayout.Controls.Add(rightPanel, 1, 0);

            mainPanel.Controls.Add(tableLayout);
            tab.Controls.Add(mainPanel);
        }

        private void CreateYanProfilViewLeftPanel(Panel panel)
        {
            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 Stok Girişleri",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 30)
            };
            panel.Controls.Add(titleLabel);

            // DataGridView
            _dgvYanProfilStock = new DataGridView
            {
                Location = new Point(20, 80),
                Width = panel.Width - 40,
                Height = panel.Height - 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            // Kolonlar
            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileType",
                HeaderText = "Profil Tipi",
                Name = "ProfileType",
                Width = 120
            });

            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "Uzunluk (m)",
                Name = "Length",
                Width = 120
            });

            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InitialQuantity",
                HeaderText = "Girilen Adet",
                Name = "InitialQuantity",
                Width = 120
            });

            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UsedLength",
                HeaderText = "Kullanılan (m)",
                Name = "UsedLength",
                Width = 120
            });

            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WastedLength",
                HeaderText = "Hurdaya Giden (m)",
                Name = "WastedLength",
                Width = 140
            });

            _dgvYanProfilStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RemainingLength",
                HeaderText = "Kalan (m)",
                Name = "RemainingLength",
                Width = 120
            });

            // Stil ayarları
            _dgvYanProfilStock.DefaultCellStyle.BackColor = Color.White;
            _dgvYanProfilStock.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvYanProfilStock.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvYanProfilStock.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvYanProfilStock.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvYanProfilStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvYanProfilStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvYanProfilStock.EnableHeadersVisualStyles = false;

            panel.Controls.Add(_dgvYanProfilStock);

            // Verileri yükle
            LoadYanProfilStock();
        }

        private void CreateYanProfilViewRightPanel(Panel panel)
        {
            // Başlık
            var titleLabel = new Label
            {
                Text = "📊 Detaylı Görünüm (Kalan Parçalar)",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 30)
            };
            panel.Controls.Add(titleLabel);

            // DataGridView
            _dgvYanProfilDetail = new DataGridView
            {
                Location = new Point(20, 80),
                Width = panel.Width - 40,
                Height = panel.Height - 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            // Kolonlar
            _dgvYanProfilDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileType",
                HeaderText = "Profil Tipi",
                Name = "ProfileType",
                Width = 120
            });

            _dgvYanProfilDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "Uzunluk (m)",
                Name = "Length",
                Width = 150
            });

            _dgvYanProfilDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 100
            });

            // Stil ayarları
            _dgvYanProfilDetail.DefaultCellStyle.BackColor = Color.White;
            _dgvYanProfilDetail.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvYanProfilDetail.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvYanProfilDetail.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvYanProfilDetail.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvYanProfilDetail.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvYanProfilDetail.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvYanProfilDetail.EnableHeadersVisualStyles = false;

            panel.Controls.Add(_dgvYanProfilDetail);

            // Verileri yükle
            LoadYanProfilDetail();
        }

        private void LoadYanProfilStock()
        {
            try
            {
                var stocks = _sideProfileStockRepository.GetAll();
                
                var data = stocks.Select(s => new
                {
                    ProfileType = s.ProfileType,
                    Length = $"{s.Length.ToString("F2", CultureInfo.InvariantCulture)} m",
                    InitialQuantity = s.InitialQuantity.ToString(),
                    UsedLength = $"{s.UsedLength.ToString("F2", CultureInfo.InvariantCulture)} m",
                    WastedLength = $"{s.WastedLength.ToString("F2", CultureInfo.InvariantCulture)} m",
                    RemainingLength = $"{s.RemainingLength.ToString("F2", CultureInfo.InvariantCulture)} m"
                }).ToList();

                _dgvYanProfilStock.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yan profil stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadYanProfilDetail()
        {
            try
            {
                var remnants = _sideProfileRemnantRepository.GetAll(includeWaste: false); // Sadece hurda olmayanları göster
                
                var data = remnants.Select(r => new
                {
                    ProfileType = r.ProfileType,
                    Length = $"{r.Length.ToString("F2", CultureInfo.InvariantCulture)} m",
                    Quantity = r.Quantity.ToString()
                }).ToList();

                _dgvYanProfilDetail.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Detaylı görünüm verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateIzolasyonViewTabContent(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 İzolasyon Sıvısı Stok Görüntüle",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            mainPanel.Controls.Add(titleLabel);

            // Toplam Stok Özeti Paneli
            var summaryPanel = new Panel
            {
                Location = new Point(30, 80),
                Width = mainPanel.Width - 60,
                Height = 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(20)
            };

            var summaryTitleLabel = new Label
            {
                Text = "📊 Toplam Stok Özeti",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            summaryPanel.Controls.Add(summaryTitleLabel);

            // Toplam stok bilgileri için label'lar (dinamik olarak yüklenecek)
            var summaryInfoLabel = new Label
            {
                Text = "Yükleniyor...",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 45),
                Name = "SummaryInfoLabel"
            };
            summaryPanel.Controls.Add(summaryInfoLabel);

            mainPanel.Controls.Add(summaryPanel);

            // DataGridView
            _dgvIzolasyon = new DataGridView
            {
                Location = new Point(30, 190),
                Width = mainPanel.Width - 60,
                Height = mainPanel.Height - 230,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            // Kolonlar
            _dgvIzolasyon.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EntryDate",
                HeaderText = "Tarih",
                Name = "EntryDate",
                Width = 120
            });

            _dgvIzolasyon.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LiquidType",
                HeaderText = "Ürün Türü",
                Name = "LiquidType",
                Width = 150
            });

            _dgvIzolasyon.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Kilogram",
                HeaderText = "Kilogram (kg)",
                Name = "Kilogram",
                Width = 150
            });

            // Stil ayarları
            _dgvIzolasyon.DefaultCellStyle.BackColor = Color.White;
            _dgvIzolasyon.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvIzolasyon.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvIzolasyon.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvIzolasyon.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvIzolasyon.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvIzolasyon.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvIzolasyon.EnableHeadersVisualStyles = false;

            mainPanel.Controls.Add(_dgvIzolasyon);

            tab.Controls.Add(mainPanel);

            // Verileri yükle
            LoadIzolasyonData();
            LoadIzolasyonSummary(summaryInfoLabel);
        }

        private void LoadIzolasyonData()
        {
            try
            {
                var stocks = _isolationStockRepository.GetAll();
                
                // Tarih sırasına göre sırala (en eski önce)
                var sortedStocks = stocks.OrderBy(s => s.EntryDate).ToList();

                var data = new List<object>();

                foreach (var stock in sortedStocks)
                {
                    data.Add(new
                    {
                        EntryDate = stock.EntryDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        LiquidType = stock.LiquidType,
                        Kilogram = stock.Kilogram.ToString("F3", CultureInfo.InvariantCulture)
                    });
                }

                _dgvIzolasyon.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzolasyon sıvısı stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadIzolasyonSummary(Label summaryLabel)
        {
            try
            {
                var stocks = _isolationStockRepository.GetAll();
                
                // Ürün türüne göre grupla ve topla
                var summary = stocks
                    .GroupBy(s => s.LiquidType)
                    .Select(g => new
                    {
                        LiquidType = g.Key,
                        TotalKilogram = g.Sum(s => s.Kilogram)
                    })
                    .ToList();

                if (summary.Count == 0)
                {
                    summaryLabel.Text = "Stok bulunmamaktadır.";
                    return;
                }

                var summaryText = string.Join(" | ", summary.Select(s => 
                    $"{s.LiquidType}: {s.TotalKilogram.ToString("F3", CultureInfo.InvariantCulture)} kg"));

                summaryLabel.Text = summaryText;
            }
            catch (Exception ex)
            {
                summaryLabel.Text = "Toplam stok bilgisi yüklenirken hata oluştu: " + ex.Message;
            }
        }

        private void CreateKapakViewTabContent(TabPage tab)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 Kapak Stok Görüntüle",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            panel.Controls.Add(titleLabel);

            // DataGridView
            _dgvKapak = new DataGridView
            {
                Location = new Point(30, 80),
                Width = panel.Width - 60,
                Height = panel.Height - 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            // Kolonlar
            _dgvKapak.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileType",
                HeaderText = "Profil Tipi",
                Name = "ProfileType",
                Width = 150
            });

            _dgvKapak.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Size",
                HeaderText = "Kapak Ölçüsü",
                Name = "Size",
                Width = 200
            });

            _dgvKapak.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CoverLength",
                HeaderText = "Kapak Boyu",
                Name = "CoverLength",
                Width = 150
            });

            _dgvKapak.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 100
            });

            // Stil ayarları
            _dgvKapak.DefaultCellStyle.BackColor = Color.White;
            _dgvKapak.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvKapak.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvKapak.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvKapak.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvKapak.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvKapak.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvKapak.EnableHeadersVisualStyles = false;

            panel.Controls.Add(_dgvKapak);
            tab.Controls.Add(panel);

            // Verileri yükle
            LoadKapakData();
        }

        private void LoadKapakData()
        {
            try
            {
                var stocks = _coverStockRepository.GetAll();
                
                var data = stocks.Select(s => new
                {
                    ProfileType = s.ProfileType,
                    Size = $"{s.Size} mm",
                    CoverLength = s.CoverLength == 2 ? "002 (2 mm)" : "030 (30 mm)",
                    Quantity = s.Quantity.ToString()
                }).ToList();

                _dgvKapak.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kapak stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

