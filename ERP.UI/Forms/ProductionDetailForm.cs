using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ProductionDetailForm : UserControl
    {
        // Performans için cache'lenmiş font ve brush'lar
        private readonly Font _tabFont = new Font("Segoe UI Emoji", 10F);
        private readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);
        private readonly SolidBrush _primaryBrush;
        private readonly Color _inactiveTabColor = Color.FromArgb(150, 150, 150);
        
        private Panel mainPanel;
        private TabControl tabControl;
        
        // İlk tab (Formül sayfası)
        private TableLayoutPanel tableLayout;
        private Label lblTitle;
        
        // Sipariş bilgileri (Readonly)
        private TextBox txtTrexOrderNo;
        private TextBox txtModel;
        private TextBox txtHtave;
        private TextBox txtPlakaAdedi10cm;
        private TextBox txtPlakaOlcusuMM;
        private TextBox txtPlakaOlcusuComMM;
        private TextBox txtPlakaOlcusuCM;
        private TextBox txtPlakaAgirligi;
        private TextBox txtGalvanizKapakAgirligi;
        private TextBox txtYukseklikMM;
        private TextBox txtYukseklikCom;
        private TextBox txtKapakBoyuMM;
        private TextBox txtProfilMode;
        private TextBox txtProfilModeAgirligi;
        private TextBox txtBypassOlcusu;
        private TextBox txtUrunTuru;
        private TextBox txtAluminyumKalinligi;
        private TextBox txtSiparisAdedi;
        private TextBox txtBoyAdet;
        private TextBox txtPlakaAdet;
        private TextBox txtToplamAdet;
        
        // İkinci tab (Rapor sayfası)
        private TableLayoutPanel reportTableLayout;
        private TextBox txtReportTrexOrderNo;
        private TextBox txtReportProductCode;
        private TextBox txtReportHtave;
        private TextBox txtReportPlakaOlcusuCM;
        private TextBox txtReportYukseklikCM;
        private TextBox txtReportToplamSiparisAdedi;
        private TextBox txtReportKapak;
        private TextBox txtReportProfil;
        private TextBox txtReportTerminTarihi;
        private TextBox txtReportFirma;
        private TextBox txtReportLamelKalinligi;
        private TextBox txtReportUrunTuru;
        private TextBox txtReportDurum;
        private TextBox txtReportPlakaAdedi;
        
        private Button btnRapor;
        private Button btnMuhasebeyeGonder;

        private Guid _orderId = Guid.Empty;
        private OrderRepository _orderRepository;
        private CuttingRepository _cuttingRepository;
        private CuttingRequestRepository _cuttingRequestRepository;
        private PressingRequestRepository _pressingRequestRepository;
        private ClampingRequestRepository _clampingRequestRepository;
        private AssemblyRequestRepository _assemblyRequestRepository;
        private Clamping2RequestRepository _clamping2RequestRepository;
        private MaterialEntryRepository _materialEntryRepository;
        private PressingRepository _pressingRepository;
        private ClampingRepository _clampingRepository;
        private AssemblyRepository _assemblyRepository;
        private IsolationRepository _isolationRepository;
        private PackagingRepository _packagingRepository;
        private PackagingRequestRepository _packagingRequestRepository;
        private MachineRepository _machineRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private CoverStockRepository _coverStockRepository;
        private SideProfileStockRepository _sideProfileStockRepository;
        private SideProfileRemnantRepository _sideProfileRemnantRepository;
        private IsolationStockRepository _isolationStockRepository;
        private Order _order;
        
        // Tab DataGridView referansları (otomatik refresh için)
        private DataGridView _isolationDataGridView;
        private DataGridView _packagingDataGridView;

        public event EventHandler BackRequested;
        public event EventHandler<Guid> ReportRequested;
        public event EventHandler<Guid> ReturnToOrderRequested;

        public ProductionDetailForm(Guid orderId)
        {
            _orderId = orderId;
            _primaryBrush = new SolidBrush(ThemeColors.Primary); // Constructor'da initialize et
            _orderRepository = new OrderRepository();
            _cuttingRepository = new CuttingRepository();
            _cuttingRequestRepository = new CuttingRequestRepository();
            _pressingRequestRepository = new PressingRequestRepository();
            _clampingRequestRepository = new ClampingRequestRepository();
            _assemblyRequestRepository = new AssemblyRequestRepository();
            _clamping2RequestRepository = new Clamping2RequestRepository();
            _materialEntryRepository = new MaterialEntryRepository();
            _pressingRepository = new PressingRepository();
            _clampingRepository = new ClampingRepository();
            _assemblyRepository = new AssemblyRepository();
            _isolationRepository = new IsolationRepository();
            _packagingRepository = new PackagingRepository();
            _packagingRequestRepository = new PackagingRequestRepository();
            _machineRepository = new MachineRepository();
            _serialNoRepository = new SerialNoRepository();
            _employeeRepository = new EmployeeRepository();
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
            this.Padding = new Padding(20);
            
            // DoubleBuffered özelliğini aç - performans için kritik
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);

            CreateMainPanel();
            LoadOrderData();
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };
            
            // Panel için de DoubleBuffered aç
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, mainPanel, new object[] { true });

            // TabControl oluştur
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(10, 5),
                BackColor = Color.White,
                Appearance = TabAppearance.FlatButtons
            };
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.BackColor = Color.White; // Sadece bir kez ayarla
            
            tabControl.DrawItem += (s, e) =>
            {
                var tabPage = tabControl.TabPages[e.Index];
                var tabRect = tabControl.GetTabRect(e.Index);
                
                // Arka planı tamamen beyaz yap - cache'lenmiş brush kullan
                e.Graphics.FillRectangle(_whiteBrush, tabRect);
                
                Color textColor;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    // Seçili tab için altında mavi çizgi - cache'lenmiş brush kullan
                    e.Graphics.FillRectangle(_primaryBrush, new Rectangle(tabRect.X, tabRect.Y + tabRect.Height - 3, tabRect.Width, 3));
                    textColor = ThemeColors.Primary;
                }
                else
                {
                    textColor = _inactiveTabColor;
                }
                
                // Cache'lenmiş font kullan
                TextRenderer.DrawText(e.Graphics, tabPage.Text, _tabFont, 
                    tabRect, textColor, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                
                e.DrawFocusRectangle();
            };

            // İlk tab: Formül
            var tabFormul = new TabPage("📐 Formül");
            tabFormul.Padding = new Padding(20);
            tabFormul.BackColor = Color.White;
            tabFormul.UseVisualStyleBackColor = false;
            CreateFormulTab(tabFormul);
            tabControl.TabPages.Add(tabFormul);

            // İkinci tab: Rapor
            var tabRapor = new TabPage("📄 Rapor");
            tabRapor.Padding = new Padding(20);
            tabRapor.BackColor = Color.White;
            tabRapor.UseVisualStyleBackColor = false;
            CreateRaporTab(tabRapor);
            tabControl.TabPages.Add(tabRapor);

            // Üçüncü tab: Üretim Ayrıntı
            var tabUretimAyrinti = new TabPage("⚙️ Üretim Ayrıntı");
            tabUretimAyrinti.Padding = new Padding(20);
            tabUretimAyrinti.BackColor = Color.White;
            tabUretimAyrinti.UseVisualStyleBackColor = false;
            CreateUretimAyrintiTab(tabUretimAyrinti);
            tabControl.TabPages.Add(tabUretimAyrinti);

            mainPanel.Controls.Add(tabControl);

            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();
        }

        private void CreateFormulTab(TabPage tab)
        {
            // Başlık
            lblTitle = new Label
            {
                Text = "Üretim Formül Detayları",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // TableLayoutPanel oluştur
            CreateTableLayout();

            tableLayout.Location = new Point(10, 50);
            tableLayout.Width = tab.Width - 40;
            tableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            tab.Controls.Add(lblTitle);
            tab.Controls.Add(tableLayout);

            // Tab boyutu değiştiğinde tableLayout'u güncelle
            tab.Resize += (s, e) =>
            {
                if (tableLayout != null)
                {
                    tableLayout.Width = tab.Width - 40;
                }
            };
        }

        private void CreateRaporTab(TabPage tab)
        {
            // Başlık
            var lblReportTitle = new Label
            {
                Text = "Üretim Rapor Detayları",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // TableLayoutPanel oluştur
            CreateReportTableLayout();

            reportTableLayout.Location = new Point(10, 50);
            reportTableLayout.Width = tab.Width - 40;
            reportTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            tab.Controls.Add(lblReportTitle);
            tab.Controls.Add(reportTableLayout);

            // Tab boyutu değiştiğinde reportTableLayout'u güncelle
            tab.Resize += (s, e) =>
            {
                if (reportTableLayout != null)
                {
                    reportTableLayout.Width = tab.Width - 40;
                }
            };
        }

        private void CreateTableLayout()
        {
            tableLayout = new TableLayoutPanel
            {
                Location = new Point(30, 80),
                Width = mainPanel.Width - 60,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            int row = 0;

            // Trex Sipariş No
            AddTableRow("Trex Sipariş No:", CreateReadOnlyTextBox(txtTrexOrderNo = new TextBox()),
                       "Model:", CreateReadOnlyTextBox(txtModel = new TextBox()), row++);

            // Hatve
            AddTableRow("Hatve:", CreateReadOnlyTextBox(txtHtave = new TextBox()),
                       "10cm Plaka Adedi:", CreateReadOnlyTextBox(txtPlakaAdedi10cm = new TextBox()), row++);

            // Plaka Ölçüsü (mm)
            AddTableRow("Plaka Ölçüsü (mm):", CreateReadOnlyTextBox(txtPlakaOlcusuMM = new TextBox()),
                       "Plaka Ölçüsü com (mm):", CreateReadOnlyTextBox(txtPlakaOlcusuComMM = new TextBox()), row++);

            // Plaka Ölçüsü (cm)
            AddTableRow("Plaka Ölçüsü (cm):", CreateReadOnlyTextBox(txtPlakaOlcusuCM = new TextBox()),
                       "Plaka Ağırlığı:", CreateReadOnlyTextBox(txtPlakaAgirligi = new TextBox()), row++);

            // Yükseklik (mm)
            AddTableRow("Yükseklik (mm):", CreateReadOnlyTextBox(txtYukseklikMM = new TextBox()),
                       "Yükseklik com:", CreateReadOnlyTextBox(txtYukseklikCom = new TextBox()), row++);

            // Kapak Boyu (mm)
            AddTableRow("Kapak Boyu (mm):", CreateReadOnlyTextBox(txtKapakBoyuMM = new TextBox()),
                       "Profil Mode:", CreateReadOnlyTextBox(txtProfilMode = new TextBox()), row++);

            // Profil Mode Ağırlığı
            AddTableRow("Profil Mode Ağırlığı:", CreateReadOnlyTextBox(txtProfilModeAgirligi = new TextBox()),
                       "Bypass Ölçüsü:", CreateReadOnlyTextBox(txtBypassOlcusu = new TextBox()), row++);

            // Galvaniz Kapak Ağırlığı
            AddTableRow("Galvaniz Kapak Ağırlığı:", CreateReadOnlyTextBox(txtGalvanizKapakAgirligi = new TextBox()),
                       "", new Label { Text = "", Dock = DockStyle.Fill }, row++);

            // Ürün Türü
            AddTableRow("Ürün Türü:", CreateReadOnlyTextBox(txtUrunTuru = new TextBox()),
                       "Alüminyum Kalınlığı:", CreateReadOnlyTextBox(txtAluminyumKalinligi = new TextBox()), row++);

            // Sipariş Adedi
            AddTableRow("Sipariş Adedi:", CreateReadOnlyTextBox(txtSiparisAdedi = new TextBox()),
                       "Boy Adet:", CreateReadOnlyTextBox(txtBoyAdet = new TextBox()), row++);

            // Plaka Adet
            AddTableRow("Plaka Adet:", CreateReadOnlyTextBox(txtPlakaAdet = new TextBox()),
                       "Toplam Adet:", CreateReadOnlyTextBox(txtToplamAdet = new TextBox()), row++);
        }

        private void AddTableRow(string label1Text, Control control1, string label2Text, Control control2, int row)
        {
            tableLayout.RowCount = row + 1;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            var label1 = new Label
            {
                Text = label1Text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 0, 0, 0)
            };

            var label2 = new Label
            {
                Text = label2Text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 0, 0, 0)
            };

            control1.Dock = DockStyle.Fill;
            control1.Margin = new Padding(2);
            control2.Dock = DockStyle.Fill;
            control2.Margin = new Padding(2);

            tableLayout.Controls.Add(label1, 0, row);
            tableLayout.Controls.Add(control1, 1, row);
            tableLayout.Controls.Add(label2, 2, row);
            tableLayout.Controls.Add(control2, 3, row);
        }

        private TextBox CreateReadOnlyTextBox(TextBox txt)
        {
            txt.ReadOnly = true;
            txt.BackColor = Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Segoe UI", 9F);
            txt.Padding = new Padding(3);
            return txt;
        }

        private void CreateReportTableLayout()
        {
            reportTableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 0
            };

            reportTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            reportTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            reportTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            reportTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            int row = 0;

            // Trex Sipariş No
            AddReportTableRow("Trex Sipariş No:", CreateReadOnlyTextBox(txtReportTrexOrderNo = new TextBox()),
                       "Ürün Kodu:", CreateReadOnlyTextBox(txtReportProductCode = new TextBox()), row++);

            // Plaka Ölçüsü (mm)
            AddReportTableRow("Plaka Ölçüsü (mm):", CreateReadOnlyTextBox(txtReportPlakaOlcusuCM = new TextBox()),
                       "Hatve:", CreateReadOnlyTextBox(txtReportHtave = new TextBox()), row++);

            // Toplam Sipariş Adedi
            AddReportTableRow("Toplam Sipariş Adedi:", CreateReadOnlyTextBox(txtReportToplamSiparisAdedi = new TextBox()),
                       "Yükseklik (mm):", CreateReadOnlyTextBox(txtReportYukseklikCM = new TextBox()), row++);

            // Plaka Adedi
            AddReportTableRow("Plaka Adedi:", CreateReadOnlyTextBox(txtReportPlakaAdedi = new TextBox()),
                       "Kapak:", CreateReadOnlyTextBox(txtReportKapak = new TextBox()), row++);

            // Termin Tarihi
            AddReportTableRow("Termin Tarihi:", CreateReadOnlyTextBox(txtReportTerminTarihi = new TextBox()),
                       "Profil:", CreateReadOnlyTextBox(txtReportProfil = new TextBox()), row++);

            // Lamel Kalınlığı
            AddReportTableRow("Lamel Kalınlığı:", CreateReadOnlyTextBox(txtReportLamelKalinligi = new TextBox()),
                       "Firma:", CreateReadOnlyTextBox(txtReportFirma = new TextBox()), row++);

            // Durum
            AddReportTableRow("Durum:", CreateReadOnlyTextBox(txtReportDurum = new TextBox()),
                       "Ürün Türü:", CreateReadOnlyTextBox(txtReportUrunTuru = new TextBox()), row++);
        }

        private void AddReportTableRow(string label1Text, Control control1, string label2Text, Control control2, int row)
        {
            reportTableLayout.RowCount = row + 1;
            reportTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            var label1 = new Label
            {
                Text = label1Text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 0, 0, 0)
            };

            var label2 = new Label
            {
                Text = label2Text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 0, 0, 0)
            };

            control1.Dock = DockStyle.Fill;
            control1.Margin = new Padding(2);
            control2.Dock = DockStyle.Fill;
            control2.Margin = new Padding(2);

            reportTableLayout.Controls.Add(label1, 0, row);
            reportTableLayout.Controls.Add(control1, 1, row);
            reportTableLayout.Controls.Add(label2, 2, row);
            reportTableLayout.Controls.Add(control2, 3, row);
        }

        private Panel CreateReportButtonPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                Width = 400
            };

            // Üretimdeyse sadece Siparişe Dön butonu göster
            bool isInProduction = _order?.Status == "Üretimde";
            // Stok siparişleri için siparişe dön butonunu gizle
            bool isStockOrder = _order?.IsStockOrder ?? false;

            if (!isInProduction)
            {
                btnRapor = ButtonFactory.CreateActionButton("📄 Rapor", ThemeColors.Info, Color.White, 150, 40);
                btnRapor.Location = new Point(0, 5);
                btnRapor.Click += BtnRapor_Click;
                panel.Controls.Add(btnRapor);
            }

            // Sadece üretimdeyse ve stok siparişi değilse siparişe dön butonu göster
            if (isInProduction && !isStockOrder)
            {
                btnMuhasebeyeGonder = ButtonFactory.CreateActionButton("📦 Siparişe Dön", ThemeColors.Info, Color.White, 180, 40);
                btnMuhasebeyeGonder.Location = new Point(btnRapor != null ? 160 : 0, 5);
                btnMuhasebeyeGonder.Click += BtnMuhasebeyeGonder_Click;
                panel.Controls.Add(btnMuhasebeyeGonder);
            }

            return panel;
        }

        private void LoadOrderData()
        {
            try
            {
                _order = _orderRepository.GetById(_orderId);
                if (_order == null)
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Bu ekran sadece "Üretimde" olan siparişler için kullanılmalı
                if (_order.Status != "Üretimde")
                {
                    MessageBox.Show(
                        $"Bu ekran sadece 'Üretimde' durumundaki siparişler için kullanılabilir.\nMevcut durum: {_order.Status}",
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Sipariş bilgilerini doldur (Formül sayfası)
                txtTrexOrderNo.Text = _order.TrexOrderNo ?? "";
                txtBypassOlcusu.Text = _order.BypassSize ?? "";
                txtUrunTuru.Text = _order.ProductType ?? "";
                // Alüminyum Kalınlığı (Lamel Kalınlığı) - siparişteki lamel kalınlığından al
                LoadAluminyumKalinligi();
                txtSiparisAdedi.Text = _order.Quantity.ToString();

                // Ürün kodundan bilgileri çıkar
                if (!string.IsNullOrEmpty(_order.ProductCode))
                {
                    ParseProductCode(_order.ProductCode);
                }
                
                // Alüminyum Kalınlığı tekrar yükle (ParseProductCode sonrası, üzerine yazılmış olabilir)
                LoadAluminyumKalinligi();

                // Plaka ağırlığını hesapla (alüminyum kalınlığı yüklendikten sonra)
                if (txtPlakaOlcusuCM != null && txtPlakaAgirligi != null && _order != null && _order.LamelThickness.HasValue)
                {
                    if (decimal.TryParse(txtPlakaOlcusuCM.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM))
                    {
                        decimal aluminyumKalinligi = _order.LamelThickness.Value;
                        decimal plakaAgirligi = CalculatePlakaAgirligi(plakaOlcusuCM, aluminyumKalinligi);
                        if (plakaAgirligi > 0)
                            txtPlakaAgirligi.Text = plakaAgirligi.ToString("F3", CultureInfo.InvariantCulture);
                    }
                }

                // Galvaniz Kapak Ağırlığı hesapla
                if (txtPlakaOlcusuCM != null && txtGalvanizKapakAgirligi != null)
                {
                    if (decimal.TryParse(txtPlakaOlcusuCM.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM))
                    {
                        decimal galvanizKapakAgirligi = CalculateGalvanizKapakAgirligi(plakaOlcusuCM);
                        if (galvanizKapakAgirligi > 0)
                            txtGalvanizKapakAgirligi.Text = galvanizKapakAgirligi.ToString("F4", CultureInfo.InvariantCulture);
                    }
                }

                // Rapor sayfası bilgilerini doldur
                LoadReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReportData()
        {
            if (_order == null) return;

            // Kontroller: TextBox'lar null olabilir
            if (txtReportTrexOrderNo != null)
                txtReportTrexOrderNo.Text = _order.TrexOrderNo ?? "";

            // Ürün Kodu
            if (txtReportProductCode != null)
                txtReportProductCode.Text = _order.ProductCode ?? "";

            // Htave - Model satırından (formül sayfasındaki txtHtave'den), hatve ölçümü ve parantez içinde hatve tipi
            if (txtReportHtave != null && txtHtave != null)
            {
                string hatveText = txtHtave.Text;
                
                // Model harfini ürün kodundan al
                char modelLetter = ' ';
                if (_order != null && !string.IsNullOrEmpty(_order.ProductCode))
                {
                    var parts = _order.ProductCode.Split('-');
                    if (parts.Length >= 3 && parts[2].Length > 0)
                    {
                        modelLetter = parts[2][0];
                    }
                }
                
                // Hatve ölçümünü hesapla
                decimal? hatveOlcumu = null;
                if (txtPlakaOlcusuCM != null)
                {
                    if (decimal.TryParse(txtPlakaOlcusuCM.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM))
                    {
                        hatveOlcumu = GetHatveOlcumu(modelLetter, plakaOlcusuCM);
                    }
                }
                
                // Hatve tipi harfini belirle
                string hatveTipiHarf = "";
                switch (char.ToUpper(modelLetter))
                {
                    case 'H': hatveTipiHarf = "H"; break;
                    case 'D': hatveTipiHarf = "D"; break;
                    case 'M': hatveTipiHarf = "M"; break;
                    case 'L': hatveTipiHarf = "L"; break;
                }
                
                // Format: 3.10(H) gibi göster
                if (hatveOlcumu.HasValue && !string.IsNullOrEmpty(hatveTipiHarf))
                {
                    txtReportHtave.Text = $"{hatveOlcumu.Value:F2}({hatveTipiHarf})";
                }
                else if (!string.IsNullOrEmpty(hatveTipiHarf))
                {
                    // Hatve ölçümü bulunamadıysa sadece hatve tipi göster
                    txtReportHtave.Text = $"({hatveTipiHarf})";
                }
                else
                {
                    txtReportHtave.Text = hatveText;
                }
            }

            // Plaka Ölçüsü (mm) - Formül sayfasındaki plaka ölçüsü cm'yi mm'ye çevir ve 100'ün katlarına yuvarla
            if (txtReportPlakaOlcusuCM != null && txtPlakaOlcusuCM != null)
            {
                if (decimal.TryParse(txtPlakaOlcusuCM.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM))
                {
                    // cm'yi mm'ye çevir (10 ile çarp)
                    int plakaOlcusuMM = (int)Math.Round(plakaOlcusuCM * 10m);
                    
                    // 100'ün katlarına yuvarla (711 -> 700)
                    int roundedPlakaOlcusuMM = (plakaOlcusuMM / 100) * 100;
                    txtReportPlakaOlcusuCM.Text = roundedPlakaOlcusuMM.ToString();
                }
                else
                {
                    txtReportPlakaOlcusuCM.Text = txtPlakaOlcusuCM.Text;
                }
            }

            // Yükseklik (mm) - SP ürünleri için kapak boyunu çıkar, YM ürünleri için çıkarma
            int raporYukseklikMM = 0;
            if (txtReportYukseklikCM != null && txtYukseklikMM != null && txtKapakBoyuMM != null)
            {
                if (int.TryParse(txtYukseklikMM.Text, out int yukseklikMM))
                {
                    // YM (stok) ürünleri kontrolü
                    bool isYM = _order?.IsStockOrder ?? false;
                    
                    // Yükseklik 1800 üzerindeyse 2'ye böl
                    int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    
                    // YM ürünleri için kapağı çıkarma, SP ürünleri için çıkar
                    if (isYM)
                    {
                        // YM ürünleri için kapağı çıkarma
                        raporYukseklikMM = yukseklikCom;
                    }
                    else
                    {
                        // SP ürünleri için kapak boyunu çıkar
                        int kapakBoyuMM = 0;
                        if (int.TryParse(txtKapakBoyuMM.Text, out kapakBoyuMM))
                        {
                            // Kapak boyunu çıkar
                            raporYukseklikMM = yukseklikCom - kapakBoyuMM;
                        }
                        else if (_order != null && !string.IsNullOrEmpty(_order.ProductCode))
                        {
                            // Ürün kodundan kapak değerini çıkar
                            var productCodeParts = _order.ProductCode.Split('-');
                            if (productCodeParts.Length > 5)
                            {
                                string kapakDegeri = productCodeParts[5];
                                
                                // Ürün kodunda DisplayText formatı kullanılıyor: 030, 002, 016
                                if (kapakDegeri == "030")
                                    kapakBoyuMM = 30;
                                else if (kapakDegeri == "002")
                                    kapakBoyuMM = 2;
                                else if (kapakDegeri == "016")
                                    kapakBoyuMM = 16;
                                else if (int.TryParse(kapakDegeri, out int parsedKapak))
                                    kapakBoyuMM = parsedKapak;
                                
                                // Kapak boyunu çıkar
                                raporYukseklikMM = yukseklikCom - kapakBoyuMM;
                            }
                        }
                        
                        // Eğer yükseklik com belli bir değerin üstündeyse (örneğin 1800), ek olarak 16 çıkar
                        // Not: Bu mantık kullanıcıya göre değişebilir, şimdilik yukseklikCom > 1800 kontrolü yapıyoruz
                        if (yukseklikCom > 1800)
                        {
                            raporYukseklikMM = raporYukseklikMM - 16;
                        }
                    }
                    
                    txtReportYukseklikCM.Text = raporYukseklikMM.ToString();
                }
            }

            // Toplam Sipariş Adedi
            if (txtReportToplamSiparisAdedi != null && txtToplamAdet != null)
                txtReportToplamSiparisAdedi.Text = txtToplamAdet.Text;

            // Plaka Adedi - Formül: yükseklik mm/100 * 10cm plaka adedi * toplam sipariş adedi
            if (txtReportPlakaAdedi != null && txtYukseklikMM != null && txtToplamAdet != null && txtPlakaAdedi10cm != null)
            {
                // YM (stok) ürünleri kontrolü
                bool isYM = _order?.IsStockOrder ?? false;
                
                // Yükseklik (mm) - SP ürünleri için kapaksız yükseklik, YM ürünleri için kapaklı yükseklik kullanılır
                int yukseklikMM = raporYukseklikMM > 0 ? raporYukseklikMM : 0;
                
                // Eğer raporYukseklikMM hesaplanamadıysa
                if (yukseklikMM == 0 && int.TryParse(txtYukseklikMM.Text, out int yukseklikMMFromText))
                {
                    // Yükseklik 1800 üzerindeyse 2'ye böl
                    int yukseklikCom = yukseklikMMFromText <= 1800 ? yukseklikMMFromText : yukseklikMMFromText / 2;
                    
                    // YM ürünleri için kapağı çıkarma, SP ürünleri için çıkar
                    if (isYM)
                    {
                        yukseklikMM = yukseklikCom;
                    }
                    else
                    {
                        // SP ürünleri için kapak boyunu çıkar
                        if (txtKapakBoyuMM != null && int.TryParse(txtKapakBoyuMM.Text, out int kapakBoyuMM))
                        {
                            yukseklikMM = yukseklikCom - kapakBoyuMM;
                        }
                        else if (txtYukseklikCom != null && int.TryParse(txtYukseklikCom.Text, out int yukseklikComFromText))
                        {
                            yukseklikMM = yukseklikComFromText;
                        }
                        else
                        {
                            yukseklikMM = yukseklikCom;
                        }
                    }
                }
                
                // Hatve değerini al
                decimal hatve = 0;
                if (txtHtave != null)
                {
                    // Hatve text'inden sayısal değeri çıkar (örn: "3.10(H)" -> 3.10)
                    string hatveText = txtHtave.Text;
                    var hatveMatch = System.Text.RegularExpressions.Regex.Match(hatveText, @"(\d+\.?\d*)");
                    if (hatveMatch.Success && decimal.TryParse(hatveMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedHatve))
                    {
                        hatve = parsedHatve;
                    }
                }
                
                if (yukseklikMM > 0 && hatve > 0 &&
                    int.TryParse(txtToplamAdet.Text, out int toplamSiparisAdedi))
                {
                    // Yeni formül: çarpılmamış = Math.Ceiling(yükseklik mm / hatve)
                    decimal birimPlakaAdedi = (decimal)yukseklikMM / hatve;
                    decimal carpilmamisYuvarlanmis = Math.Ceiling(birimPlakaAdedi);
                    
                    // Çarpılmış: çarpılmamış * toplam sipariş adedi
                    decimal carpilmisPlakaAdedi = carpilmamisYuvarlanmis * toplamSiparisAdedi;
                    
                    // Gösterim: "çarpılmamış - çarpılmış"
                    txtReportPlakaAdedi.Text = $"{carpilmamisYuvarlanmis} - {carpilmisPlakaAdedi}";
                }
                else if (int.TryParse(txtPlakaAdet.Text, out int plakaAdetFallback) && int.TryParse(txtToplamAdet?.Text, out int toplamAdetFallback))
                {
                    // Fallback: Eski mantığa geri dön
                    int carpilmis = plakaAdetFallback * toplamAdetFallback;
                    txtReportPlakaAdedi.Text = $"{plakaAdetFallback} - {carpilmis}";
                }
                else
                {
                    txtReportPlakaAdedi.Text = "0";
                }
            }

            // Kapak - Kapak boyu 030 ise "Normal Kapak", 002 ise "Düz Kapak"
            if (txtReportKapak != null && txtKapakBoyuMM != null && int.TryParse(txtKapakBoyuMM.Text, out int kapakBoyu))
            {
                if(kapakBoyu == 30)
                {
                    txtReportKapak.Text = "Normal Kapak";
                }
                else if(kapakBoyu == 2)
                {
                    txtReportKapak.Text = "Düz Kapak";
                }
                else if(kapakBoyu == 16)
                {
                    txtReportKapak.Text = "Normal ve Düz Kapak";
                }
                else
                {
                    txtReportKapak.Text = "-";
                }
            }

            // Profil - S ve G ise "Standart", G ise "Geniş Profil"
            if (txtReportProfil != null && txtProfilMode != null)
            {
                string profilMode = txtProfilMode.Text.ToUpper();
                if (profilMode == "S")
                {
                    txtReportProfil.Text = "Standart";
                }
                else if (profilMode == "G")
                {
                    txtReportProfil.Text = "Geniş Profil";
                }
            }

            // Termin Tarihi
            if (txtReportTerminTarihi != null)
                txtReportTerminTarihi.Text = _order.TermDate.ToString("dd.MM.yyyy");

            // Firma
            if (txtReportFirma != null)
                txtReportFirma.Text = _order.Company?.Name ?? "";

            // Lamel Kalınlığı
            if (txtReportLamelKalinligi != null)
                txtReportLamelKalinligi.Text = _order.LamelThickness?.ToString("F3", CultureInfo.InvariantCulture) ?? "";

            // Ürün Türü
            if (txtReportUrunTuru != null)
                txtReportUrunTuru.Text = _order.ProductType ?? "";

            // Durum - üretim durumunu kontrol et
            if (txtReportDurum != null)
            {
                string statusText = GetProductionStatusText(_order);
                txtReportDurum.Text = statusText;
            }

            // Buton panelini oluştur (üretimdeyse sadece siparişe dön)
            var tabRapor = tabControl?.TabPages["📄 Rapor"];
            if (tabRapor != null)
            {
                // Mevcut buton panelini kaldır
                foreach (Control control in tabRapor.Controls.OfType<Panel>().ToList())
                {
                    if (control.Controls.OfType<Button>().Any())
                    {
                        tabRapor.Controls.Remove(control);
                        control.Dispose();
                    }
                }

                var buttonPanel = CreateReportButtonPanel();
                buttonPanel.Location = new Point(10, reportTableLayout.Bottom + 30);
                buttonPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                tabRapor.Controls.Add(buttonPanel);
            }
        }

        private void BtnRapor_Click(object sender, EventArgs e)
        {
            ReportRequested?.Invoke(this, _orderId);
        }

        private void BtnMuhasebeyeGonder_Click(object sender, EventArgs e)
        {
            // Paketleme kontrolü
            var packagings = _packagingRepository.GetByOrderId(_orderId);
            bool hasCompletedPackaging = packagings.Any(p => p.IsActive);
            
            if (!hasCompletedPackaging)
            {
                MessageBox.Show(
                    "Bu siparişi siparişe döndürmek için önce paketleme işleminin tamamlanmış olması gerekir.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            var result = MessageBox.Show(
                $"Sipariş {_order?.TrexOrderNo} siparişe döndürülecek. Emin misiniz?",
                "Siparişe Dön",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ReturnToOrderRequested?.Invoke(this, _orderId);
            }
        }


        private void ParseProductCode(string productCode)
        {
            try
            {
                // Format: TREX-CR-LG-1422-1900-030
                // veya: TREX-CR-HS-1422-1900-030
                var parts = productCode.Split('-');
                if (parts.Length < 6)
                {
                    MessageBox.Show("Ürün kodu formatı geçersiz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int plakaAdet = 1; // Varsayılan değer
                int boyAdet = 1; // Varsayılan değer
                char modelLetter = ' '; // Varsayılan değer

                // Model ve Profil: LG -> Model: L, Profil: G
                // veya HS -> Model: H, Profil: S
                string modelProfile = parts[2]; // LG veya HS
                if (modelProfile.Length >= 2)
                {
                    modelLetter = modelProfile[0]; // L veya H
                    char profileLetter = modelProfile[1]; // G veya S

                    // Model
                    txtModel.Text = modelLetter.ToString().ToUpper();

                    // Profil Mode
                    txtProfilMode.Text = profileLetter.ToString().ToUpper();

                    // Profil Mode Ağırlığı: G=0.5, S=0.3
                    decimal profilModeAgirligi = profileLetter == 'G' || profileLetter == 'g' ? 0.5m : 0.3m;
                    txtProfilModeAgirligi.Text = profilModeAgirligi.ToString("F1", CultureInfo.InvariantCulture);
                }

                // Plaka Ölçüsü (mm): 1422
                decimal plakaOlcusuCM = 0;
                if (int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    txtPlakaOlcusuMM.Text = plakaOlcusuMM.ToString();

                    // Plaka Ölçüsü com (mm): 1422 <= 1150 ise 1422, > 1150 ise 1422/2
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    txtPlakaOlcusuComMM.Text = plakaOlcusuComMM.ToString();

                    // Plaka Ölçüsü (cm): Plaka ölçüsü com / 10
                    plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                    txtPlakaOlcusuCM.Text = plakaOlcusuCM.ToString("F1", CultureInfo.InvariantCulture);
                    
                    // Hatve: Plaka ölçüsüne göre hesaplanır
                    decimal htave = 0;
                    var hatveOlcumu = GetHatveOlcumu(modelLetter, plakaOlcusuCM);
                    if (hatveOlcumu.HasValue)
                    {
                        htave = hatveOlcumu.Value;
                    }
                    else
                    {
                        // Fallback: Eski metod
                        htave = GetHtave(modelLetter);
                    }
                    txtHtave.Text = htave.ToString("F2", CultureInfo.InvariantCulture);

                    // 10cm Plaka Adedi: 100 / hatve (tam bölünmüyorsa 1 ekle)
                    decimal plakaAdedi10cmDecimal = htave > 0 ? 100m / htave : 0;
                    int plakaAdedi10cm = 0;
                    if (plakaAdedi10cmDecimal > 0)
                    {
                        int tamKisim = (int)Math.Floor(plakaAdedi10cmDecimal);
                        // Eğer tam bölünmüyorsa (ondalık kısmı varsa) 1 ekle
                        if (plakaAdedi10cmDecimal % 1 != 0)
                            plakaAdedi10cm = tamKisim + 1;
                        else
                            plakaAdedi10cm = tamKisim;
                    }
                    txtPlakaAdedi10cm.Text = plakaAdedi10cm.ToString();

                    // Plaka Adet: Plaka ölçüsü <= 1150 ise 1, > 1150 ise 4
                    plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                    txtPlakaAdet.Text = plakaAdet.ToString();

                    // Plaka Ağırlığı ve Galvaniz Kapak Ağırlığı hesaplaması LoadOrderData sonunda yapılacak
                }

                // Yükseklik (mm): 1900
                int kapakBoyuMM = 0;
                if (int.TryParse(parts[4], out int yukseklikMM))
                {
                    txtYukseklikMM.Text = yukseklikMM.ToString();

                    // Yükseklik com: 1900 <= 1800 ise 1900, > 1800 ise 1900/2
                    int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;

                    // Boy Adet: Yükseklik <= 1800 ise 1, > 1800 ise 2
                    boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                    txtBoyAdet.Text = boyAdet.ToString();
                }

                // Kapak Boyu (mm): 030 -> 30
                if (parts.Length > 5 && int.TryParse(parts[5], out kapakBoyuMM))
                {
                    txtKapakBoyuMM.Text = kapakBoyuMM.ToString();
                }
                
                // Yükseklik com: Yükseklik 1800 üzerindeyse 2'ye böl, sonra kapak boyunu çıkar
                if (int.TryParse(txtYukseklikMM.Text, out int yukseklikMMForCom))
                {
                    int yukseklikCom = yukseklikMMForCom <= 1800 ? yukseklikMMForCom : yukseklikMMForCom / 2;
                    // Kapak boyunu çıkar
                    if (kapakBoyuMM > 0)
                    {
                        yukseklikCom = yukseklikCom - kapakBoyuMM;
                    }
                    txtYukseklikCom.Text = yukseklikCom.ToString();
                }

                // Toplam Adet: Sipariş adedi * Boy adet * Plaka adet
                if (int.TryParse(txtSiparisAdedi.Text, out int siparisAdedi))
                {
                    int toplamAdet = siparisAdedi * boyAdet * plakaAdet;
                    txtToplamAdet.Text = toplamAdet.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün kodu parse edilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private int GetPlakaAdedi10cm(char modelLetter)
        {
            switch (char.ToUpper(modelLetter))
            {
                case 'H': return 32;
                case 'D': return 24;
                case 'M': return 17;
                case 'L': return 12;
                default: return 0;
            }
        }

        private string GetShortStatus(string status)
        {
            // Durum metinlerini kısalt
            switch (status)
            {
                case "Tamamlandı":
                    return "Tamam";
                case "Beklemede":
                    return "Bekliyor";
                case "Kesimde":
                    return "Kesim";
                case "Presde":
                    return "Pres";
                case "Montajda":
                    return "Montaj";
                case "Kenetmede":
                    return "Kenet";
                default:
                    return status;
            }
        }

        private string GetProductionStatusText(Order order)
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
                return "Paketli";
            }
            
            // Montaj işlemi yapılmış mı kontrol et
            var assemblyRequests = _assemblyRequestRepository.GetByOrderId(order.Id);
            bool hasAssembly = assemblyRequests.Any(ar => ar.IsActive);
            
            if (hasAssembly)
            {
                return "Montajlı";
            }
            
            // Kenetleme işlemi yapılmış mı kontrol et
            var clampingRequests = _clampingRequestRepository.GetByOrderId(order.Id);
            bool hasClamping = clampingRequests.Any(cr => cr.IsActive);
            
            if (hasClamping)
            {
                return "Kenetli";
            }
            
            // Hiç işlem yapılmamışsa
            return "Bekliyor";
        }

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerlerini harfe çevir (yeni format): 3.10, 3.25=H | 4.3, 4.5=D | 6.3, 6.4, 6.5=M | 9.0=L
            // Tolerance'ı biraz artırdık (0.1'den 0.15'e) - 6.4 ve benzeri değerleri daha iyi yakalamak için
            const decimal tolerance = 0.15m;
            
            // H: 3.10, 3.25 (±0.15 = 2.95-3.40 arası)
            if (hatveValue >= 2.95m && hatveValue <= 3.40m)
                return "H";
            // D: 4.3, 4.5 (±0.15 = 4.15-4.65 arası)
            else if (hatveValue >= 4.15m && hatveValue <= 4.65m)
                return "D";
            // M: 6.3, 6.4, 6.5 (±0.15 = 6.15-6.65 arası)
            else if (hatveValue >= 6.15m && hatveValue <= 6.65m)
                return "M";
            // L: 8.65, 8.7, 9.0 (±0.15 = 8.50-9.15 arası)
            else if (hatveValue >= 8.50m && hatveValue <= 9.15m)
                return "L";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture); // Eğer tanınmazsa sayısal göster
        }

        private decimal CalculatePlakaAgirligi(decimal plakaOlcusuCM, decimal aluminyumKalinligi)
        {
            // Plaka ölçüsü (cm) = x, Alüminyum kalınlığı = y
            // Değerleri yakın eşleştirme için tolerance kullanıyoruz
            const decimal tolerance = 0.001m;

            // x 18-24 arası
            if (plakaOlcusuCM >= 18 && plakaOlcusuCM <= 24)
            {
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.019m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.014m;
            }

            // x 28-34 arası
            if (plakaOlcusuCM >= 28 && plakaOlcusuCM <= 34)
            {
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.042m;
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.380m; // Excel formülünde 0,38 olarak belirtilmiş
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.031m;
            }

            // x 38-44 arası
            if (plakaOlcusuCM >= 38 && plakaOlcusuCM <= 44)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.068m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.074m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.054m;
            }

            // x 48-54 arası
            if (plakaOlcusuCM >= 48 && plakaOlcusuCM <= 54)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.105m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.115m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.085m;
            }

            // x 58-64 arası
            if (plakaOlcusuCM >= 58 && plakaOlcusuCM <= 64)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.150m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.164m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.120m;
            }

            // x 68-74 arası
            if (plakaOlcusuCM >= 68 && plakaOlcusuCM <= 74)
            {
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.162m;
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.203m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.223m;
            }

            // x 78-84 arası
            if (plakaOlcusuCM >= 78 && plakaOlcusuCM <= 84)
            {
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.212m;
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.265m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.291m;
            }

            // x 98-104 arası
            if (plakaOlcusuCM >= 98 && plakaOlcusuCM <= 104)
            {
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.360m;
                if (Math.Abs(aluminyumKalinligi - 0.18m) < tolerance)
                    return 0.494m;
            }

            // Eşleşme bulunamazsa 0 döndür
            return 0m;
        }

        private void ConsumeCoverStock(Order order, int yapilanAdet)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                    return;

                var parts = order.ProductCode.Split('-');
                
                // Profil tipi (S=Standart, G=Geniş)
                string profileType = "";
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2];
                    if (modelProfile.Length >= 2)
                    {
                        char profileLetter = modelProfile[1];
                        profileType = profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
                    }
                }

                // Plaka ölçüsü
                int plateSizeMM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    plateSizeMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                }

                // Kapak boyu
                int coverLengthMM = GetKapakBoyuFromOrder(order);

                if (!string.IsNullOrEmpty(profileType) && plateSizeMM > 0 && coverLengthMM > 0)
                {
                    // CoverStock'tan bul
                    var coverStock = _coverStockRepository.GetByProperties(profileType, plateSizeMM, coverLengthMM);
                    if (coverStock != null)
                    {
                        // Her adet için 2 tane kapak kullanılacak
                        int neededCoverCount = yapilanAdet * 2;
                        
                        if (coverStock.Quantity >= neededCoverCount)
                        {
                            coverStock.Quantity -= neededCoverCount;
                            _coverStockRepository.Update(coverStock);
                        }
                        else
                        {
                            MessageBox.Show($"Yetersiz kapak stoku! Gereken: {neededCoverCount}, Mevcut: {coverStock.Quantity}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kapak stoku tüketilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConsumeIsolationStock(string isolationMethod, decimal isolationLiquidAmount, int izosiyanatRatio = 1, int poliolRatio = 1)
        {
            try
            {
                if (isolationMethod == "MS Silikon")
                {
                    // MS Silikon tüketimi (kg cinsinden)
                    // isolationLiquidAmount zaten kg cinsinden geliyor (IsolationDialog'dan)
                    // MS Silikon için 1 metre = 2 kg MS Silikon tüketimi
                    // isolationLiquidAmount = totalLengthM * 2m (1 metre = 2 kg MS Silikon)
                    decimal msSilikonNeededKg = isolationLiquidAmount; // isolationLiquidAmount zaten 1 metre = 2 kg MS Silikon olarak hesaplanmış

                    var msSilikonStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "MS Silikon" && s.Kilogram > 0)
                        .OrderBy(s => s.EntryDate)
                        .ToList();

                    decimal remainingNeeded = msSilikonNeededKg;
                    foreach (var stock in msSilikonStocks)
                    {
                        if (remainingNeeded <= 0)
                            break;

                        decimal useKilogram = Math.Min(stock.Kilogram, remainingNeeded);
                        stock.Kilogram -= useKilogram;

                        if (stock.Kilogram <= 0)
                        {
                            _isolationStockRepository.Delete(stock.Id);
                        }
                        else
                        {
                            _isolationStockRepository.Update(stock);
                        }

                        remainingNeeded -= useKilogram;
                    }

                    if (remainingNeeded > 0)
                    {
                        MessageBox.Show($"Yetersiz MS Silikon stoku!\nGereken: {msSilikonNeededKg:F3} kg\nEksik: {remainingNeeded:F3} kg", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else // İzosiyanat+Poliol
                {
                    // İzolasyon sıvısı miktarı kg cinsinden
                    // İzosiyanat ve Poliol'ü belirlenen oranlara göre kullan
                    int totalRatio = izosiyanatRatio + poliolRatio;
                    decimal izosiyanatKg = (isolationLiquidAmount * izosiyanatRatio) / totalRatio;
                    decimal poliolKg = (isolationLiquidAmount * poliolRatio) / totalRatio;
                    
                    // İzosiyanat stoklarından kullan
                    var isosiyanatStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "İzosiyanat" && s.Kilogram > 0)
                        .OrderBy(s => s.EntryDate)
                        .ToList();
                    
                    decimal remainingIsosiyanat = izosiyanatKg;
                    foreach (var stock in isosiyanatStocks)
                    {
                        if (remainingIsosiyanat <= 0)
                            break;
                        
                        decimal useKilogram = Math.Min(stock.Kilogram, remainingIsosiyanat);
                        stock.Kilogram -= useKilogram;
                        
                        if (stock.Kilogram <= 0)
                        {
                            _isolationStockRepository.Delete(stock.Id);
                        }
                        else
                        {
                            _isolationStockRepository.Update(stock);
                        }
                        
                        remainingIsosiyanat -= useKilogram;
                    }
                    
                    // Poliol stoklarından kullan
                    var poliolStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "Poliol" && s.Kilogram > 0)
                        .OrderBy(s => s.EntryDate)
                        .ToList();
                    
                    decimal remainingPoliol = poliolKg;
                    foreach (var stock in poliolStocks)
                    {
                        if (remainingPoliol <= 0)
                            break;
                        
                        decimal useKilogram = Math.Min(stock.Kilogram, remainingPoliol);
                        stock.Kilogram -= useKilogram;
                        
                        if (stock.Kilogram <= 0)
                        {
                            _isolationStockRepository.Delete(stock.Id);
                        }
                        else
                        {
                            _isolationStockRepository.Update(stock);
                        }
                        
                        remainingPoliol -= useKilogram;
                    }
                    
                    // Eğer yeterli stok yoksa uyarı ver
                    if (remainingIsosiyanat > 0 || remainingPoliol > 0)
                    {
                        MessageBox.Show(
                            $"Yetersiz izolasyon sıvısı stoku!\nGereken: {isolationLiquidAmount:F2} kg\n" +
                            $"Eksik İzosiyanat: {Math.Max(0, remainingIsosiyanat):F3} kg\n" +
                            $"Eksik Poliol: {Math.Max(0, remainingPoliol):F3} kg",
                            "Uyarı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzolasyon sıvısı stoku tüketilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetProfileTypeFromOrder(Order order)
        {
            if (order == null || string.IsNullOrEmpty(order.ProductCode))
                return "Standart";
            
            // Ürün kodundan profil tipini çıkar (örnek: TREX-CR-LG-500-730-002)
            // parts[2] = "LG" -> ikinci karakter 'G' = Geniş, 'S' = Standart
            var productCodeParts = order.ProductCode.Split('-');
            if (productCodeParts.Length >= 3 && productCodeParts[2].Length >= 2)
            {
                char profileLetter = productCodeParts[2][1];
                return profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
            }
            
            return "Standart";
        }

        private void ConsumeSideProfileStock(Order order, Clamping clamping, int yapilanAdet)
        {
            try
            {
                // Profil tipini Order'dan al (ürün kodundan)
                string profileType = GetProfileTypeFromOrder(order);
                
                // Yan profil uzunluğu = clamping.Length (MM cinsinden)
                decimal sideProfileLengthMM = clamping.Length;
                decimal sideProfileLengthM = sideProfileLengthMM / 1000.0m; // MM'den metreye çevir
                
                // Her adet için 4 tane yan profil gerekiyor
                int neededProfileCount = yapilanAdet * 4;

                // Önce kalanlardan (remnants) kullan - aynı profil tipindekilerden
                var usableRemnants = _sideProfileRemnantRepository.GetAll(includeWaste: false)
                    .Where(r => r.ProfileType == profileType && r.Length >= sideProfileLengthM && r.Quantity > 0)
                    .OrderBy(r => r.Length)
                    .ToList();

                int remainingNeeded = neededProfileCount;

                foreach (var remnant in usableRemnants)
                {
                    if (remainingNeeded <= 0)
                        break;

                    int useCount = Math.Min(remnant.Quantity, remainingNeeded);
                    remnant.Quantity -= useCount;
                    
                    if (remnant.Quantity == 0)
                    {
                        // Eğer remnant tamamen kullanıldıysa sil (IsActive = false)
                        _sideProfileRemnantRepository.Delete(remnant.Id);
                    }
                    else
                    {
                        _sideProfileRemnantRepository.Update(remnant);
                    }

                    remainingNeeded -= useCount;
                }

                // Hala ihtiyaç varsa 6 metrelik stoklardan kullan - aynı profil tipindekilerden
                if (remainingNeeded > 0)
                {
                    var sixMeterStock = _sideProfileStockRepository.GetByLengthAndProfileType(6.0m, profileType);
                    if (sixMeterStock != null && sixMeterStock.RemainingLength > 0)
                    {
                        // Her bir 6 metrelik profilden kaç tane yan profil çıkar
                        int profilesPerSixMeter = (int)Math.Floor(6.0m / sideProfileLengthM);
                        
                        if (profilesPerSixMeter > 0)
                        {
                            // Kaç tane 6 metrelik profil gerekiyor
                            int neededSixMeterProfiles = (int)Math.Ceiling((decimal)remainingNeeded / profilesPerSixMeter);
                            
                            // Mevcut 6 metrelik stoktan kaç tane kullanılabilir
                            int availableSixMeterProfiles = (int)Math.Floor(sixMeterStock.RemainingLength / 6.0m);
                            int useFromStock = Math.Min(neededSixMeterProfiles, availableSixMeterProfiles);

                            if (useFromStock > 0)
                            {
                                decimal usedLengthM = useFromStock * 6.0m;
                                sixMeterStock.UsedLength += usedLengthM;
                                _sideProfileStockRepository.Update(sixMeterStock);

                                // Kalan parçaları hesapla ve remnant'a ekle
                                // Her 6 metrelik profilden kesilen parçadan kalan = 6m - (profilesPerSixMeter * sideProfileLengthM)
                                decimal remnantLength = 6.0m - (profilesPerSixMeter * sideProfileLengthM);
                                if (remnantLength > 0)
                                {
                                    var remnant = new SideProfileRemnant
                                    {
                                        ProfileType = profileType,
                                        Length = remnantLength,
                                        Quantity = useFromStock,
                                        IsWaste = false
                                    };
                                    _sideProfileRemnantRepository.InsertOrMerge(remnant);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yan profil stoku tüketilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetKapakBoyuFromOrder(Order order)
        {
            // Önce txtKapakBoyuMM'den al
            if (txtKapakBoyuMM != null && int.TryParse(txtKapakBoyuMM.Text, out int kapakBoyuMM))
            {
                return kapakBoyuMM;
            }
            
            // Ürün kodundan kapak değerini çıkar
            if (order != null && !string.IsNullOrEmpty(order.ProductCode))
            {
                var productCodeParts = order.ProductCode.Split('-');
                if (productCodeParts.Length > 5)
                {
                    string kapakDegeri = productCodeParts[5];
                    
                    // Ürün kodunda DisplayText formatı kullanılıyor: 030, 002, 016
                    if (kapakDegeri == "030")
                        return 30;
                    else if (kapakDegeri == "002")
                        return 2;
                    else if (kapakDegeri == "016")
                        return 16;
                    else if (int.TryParse(kapakDegeri, out int parsedKapak))
                        return parsedKapak;
                }
            }
            
            return 0;
        }

        private decimal? GetHatveOlcumu(char hatveTipi, decimal plakaOlcusuCM)
        {
            // Plaka ölçüsünü cm cinsinden al (20, 30, 40, 50, 60, 70, 80, 100 gibi)
            // En yakın 10'a yuvarla (örn: 21-29 -> 20, 31-39 -> 30)
            int plakaOlcusuYuvarla = (int)Math.Round(plakaOlcusuCM / 10.0m, MidpointRounding.AwayFromZero) * 10;
            
            char hatveTipiUpper = char.ToUpper(hatveTipi);
            
            // Hatve tipi ve plaka ölçüsüne göre hatve değerini döndür
            switch (hatveTipiUpper)
            {
                case 'H':
                    // H20, H30, H40, H50: 3.10
                    if (plakaOlcusuYuvarla == 20 || plakaOlcusuYuvarla == 30 || plakaOlcusuYuvarla == 40 || plakaOlcusuYuvarla == 50)
                        return 3.10m;
                    break;
                case 'M':
                    // M30: 6.4, M40: 6.3, M50: 6.4, M60: 6.3, M70: 6.5, M80: 6.5, M100: 6.5
                    if (plakaOlcusuYuvarla == 30 || plakaOlcusuYuvarla == 50) return 6.4m;
                    if (plakaOlcusuYuvarla == 40 || plakaOlcusuYuvarla == 60) return 6.3m;
                    if (plakaOlcusuYuvarla == 70 || plakaOlcusuYuvarla == 80 || plakaOlcusuYuvarla == 100) return 6.5m;
                    break;
                case 'D':
                    // D30: 4.5, D40: 4.5, D50: 4.5, D60: 4.3
                    if (plakaOlcusuYuvarla == 30 || plakaOlcusuYuvarla == 40 || plakaOlcusuYuvarla == 50) return 4.5m;
                    if (plakaOlcusuYuvarla == 60) return 4.3m;
                    break;
                case 'L':
                    // L50: 8.7, L40: 8.7, L30: 8.7, L60: 8.65, L70: 8.65, L80: 8.65, L100: 8.65
                    if (plakaOlcusuYuvarla == 30 || plakaOlcusuYuvarla == 40 || plakaOlcusuYuvarla == 50) return 8.7m;
                    if (plakaOlcusuYuvarla == 60 || plakaOlcusuYuvarla == 70 || plakaOlcusuYuvarla == 80 || plakaOlcusuYuvarla == 100) return 8.65m;
                    break;
            }
            
            return null; // Bulunamadıysa null döndür
        }

        private decimal CalculateGalvanizKapakAgirligi(decimal plakaOlcusuCM)
        {
            // Galvaniz kapak ağırlığı - plaka ölçüsü cm'ye göre
            if (Math.Abs(plakaOlcusuCM - 20m) < 0.1m)
                return 0.421m;
            if (Math.Abs(plakaOlcusuCM - 30m) < 0.1m)
                return 0.663m;
            if (Math.Abs(plakaOlcusuCM - 40m) < 0.1m)
                return 1.358m;
            if (Math.Abs(plakaOlcusuCM - 50m) < 0.1m)
                return 2.026m;
            if (Math.Abs(plakaOlcusuCM - 60m) < 0.1m)
                return 2.828m;
            if (Math.Abs(plakaOlcusuCM - 70m) < 0.1m)
                return 3.764m;
            if (Math.Abs(plakaOlcusuCM - 80m) < 0.1m)
                return 5.5685m;
            if (Math.Abs(plakaOlcusuCM - 100m) < 0.1m)
                return 8.672m;

            // Eşleşme bulunamazsa 0 döndür
            return 0m;
        }

        private void LoadAluminyumKalinligi()
        {
            // Alüminyum Kalınlığı (Lamel Kalınlığı) - siparişteki lamel kalınlığından al
            // TextBox oluşturulmuş olmalı çünkü CreateFormulTab önce çağrılıyor
            if (txtAluminyumKalinligi == null && tableLayout != null)
            {
                // TextBox'ı tableLayout'tan bul - Alüminyum Kalınlığı satırındaki TextBox
                // Satır 8'de (index 8) Alüminyum Kalınlığı var, 3. sütunda (index 3) TextBox
                try
                {
                    // Tüm satırları kontrol et
                    for (int row = 0; row < tableLayout.RowCount; row++)
                    {
                        var labelControl = tableLayout.GetControlFromPosition(2, row);
                        if (labelControl is Label && labelControl.Text.Contains("Alüminyum"))
                        {
                            var textBoxControl = tableLayout.GetControlFromPosition(3, row);
                            if (textBoxControl is TextBox)
                            {
                                txtAluminyumKalinligi = textBoxControl as TextBox;
                                break;
                            }
                        }
                    }
                }
                catch { }
            }

            if (txtAluminyumKalinligi != null && _order != null && _order.LamelThickness.HasValue)
            {
                decimal lamelKalinligi = _order.LamelThickness.Value;
                txtAluminyumKalinligi.Text = lamelKalinligi.ToString("0.000", CultureInfo.InvariantCulture);
            }
            else if (txtAluminyumKalinligi != null)
            {
                txtAluminyumKalinligi.Text = "";
            }
        }

        private void CreateUretimAyrintiTab(TabPage tab)
        {
            // Başlık
            var lblUretimAyrintiTitle = new Label
            {
                Text = "Üretim Ayrıntı",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // İçerik paneli (alt sekmeler buraya eklenecek)
            var contentPanel = new Panel
            {
                Location = new Point(10, 50),
                Width = tab.Width - 40,
                Height = tab.Height - 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.White
            };

            tab.Controls.Add(lblUretimAyrintiTitle);
            tab.Controls.Add(contentPanel);

            // Tab boyutu değiştiğinde contentPanel'i güncelle
            tab.Resize += (s, e) =>
            {
                if (contentPanel != null)
                {
                    contentPanel.Width = tab.Width - 40;
                    contentPanel.Height = tab.Height - 100;
                }
            };

            // Alt sekmeler: Kesim
            var cuttingTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(10, 5),
                BackColor = Color.White,
                Appearance = TabAppearance.FlatButtons
            };
            cuttingTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            cuttingTabControl.BackColor = Color.White; // Sadece bir kez ayarla
            
            cuttingTabControl.DrawItem += (s, e) =>
            {
                var tabPage = cuttingTabControl.TabPages[e.Index];
                var tabRect = cuttingTabControl.GetTabRect(e.Index);
                
                // Arka planı tamamen beyaz yap - cache'lenmiş brush kullan
                e.Graphics.FillRectangle(_whiteBrush, tabRect);
                
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    // Seçili tab için altında mavi çizgi - cache'lenmiş brush kullan
                    e.Graphics.FillRectangle(_primaryBrush, new Rectangle(tabRect.X, tabRect.Y + tabRect.Height - 3, tabRect.Width, 3));
                    
                    // Cache'lenmiş font kullan
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, _tabFont, 
                        tabRect, ThemeColors.Primary, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                else
                {
                    // Cache'lenmiş font kullan
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, _tabFont, 
                        tabRect, _inactiveTabColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                
                e.DrawFocusRectangle();
            };

            var tabKesim = new TabPage("✂️ Kesim");
            tabKesim.Padding = new Padding(20);
            tabKesim.BackColor = Color.White;
            tabKesim.UseVisualStyleBackColor = false;
            CreateKesimTab(tabKesim);
            cuttingTabControl.TabPages.Add(tabKesim);

            var tabPres = new TabPage("🔧 Pres");
            tabPres.Padding = new Padding(20);
            tabPres.BackColor = Color.White;
            tabPres.UseVisualStyleBackColor = false;
            CreatePresTab(tabPres);
            cuttingTabControl.TabPages.Add(tabPres);

            var tabKenetleme = new TabPage("🔗 Kenetleme");
            tabKenetleme.Padding = new Padding(20);
            tabKenetleme.BackColor = Color.White;
            tabKenetleme.UseVisualStyleBackColor = false;
            CreateClampingTab(tabKenetleme);
            cuttingTabControl.TabPages.Add(tabKenetleme);

            var tabKenetleme2 = new TabPage("🔗 Kenetleme 2");
            tabKenetleme2.Padding = new Padding(20);
            tabKenetleme2.BackColor = Color.White;
            tabKenetleme2.UseVisualStyleBackColor = false;
            CreateClamping2Tab(tabKenetleme2);
            cuttingTabControl.TabPages.Add(tabKenetleme2);

            var tabMontaj = new TabPage("🔩 Montaj");
            tabMontaj.Padding = new Padding(20);
            tabMontaj.BackColor = Color.White;
            tabMontaj.UseVisualStyleBackColor = false;
            CreateAssemblyTab(tabMontaj);
            cuttingTabControl.TabPages.Add(tabMontaj);

            var tabIzolasyon = new TabPage("🛡️ İzolasyon");
            tabIzolasyon.Padding = new Padding(20);
            tabIzolasyon.BackColor = Color.White;
            tabIzolasyon.UseVisualStyleBackColor = false;
            CreateIsolationTab(tabIzolasyon);
            cuttingTabControl.TabPages.Add(tabIzolasyon);

            var tabPaketleme = new TabPage("📦 Paketleme");
            tabPaketleme.Padding = new Padding(20);
            tabPaketleme.BackColor = Color.White;
            tabPaketleme.UseVisualStyleBackColor = false;
            CreatePackagingTab(tabPaketleme);
            cuttingTabControl.TabPages.Add(tabPaketleme);

            contentPanel.Controls.Add(cuttingTabControl);
        }


        private void CreateKesimTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu (Kesim taleplerini onaylamak için)
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Kesim Onayla", ThemeColors.Success, Color.White, 130, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 130, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 80, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEkle.Location = new Point(buttonPanel.Width - 130 - 90, 5);
            buttonPanel.Controls.Add(btnEkle);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            AddKesimColumn(dataGridView, "Hatve", "Hatve (mm)", 80);
            AddKesimColumn(dataGridView, "Size", "Ölçü (cm)", 80);
            AddKesimColumn(dataGridView, "MachineName", "Makina No", 80);
            AddKesimColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddKesimColumn(dataGridView, "TotalKg", "Toplam Kg", 85);
            AddKesimColumn(dataGridView, "CutKg", "Kesilen Kg", 85);
            AddKesimColumn(dataGridView, "CuttingCount", "Kesilen Plaka Adedi", 120);
            AddKesimColumn(dataGridView, "WasteCount", "Hurda Plaka Adedi", 120);
            AddKesimColumn(dataGridView, "WasteKg", "Hurda Kg", 80);
            AddKesimColumn(dataGridView, "RemainingKg", "Kalan Kg", 80);
            AddKesimColumn(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları - ÖNCE bu ayarları yap
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            
            // DoubleBuffered özelliğini aç - scroll performansı için kritik
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dataGridView, new object[] { true });
            
            // Scroll event'ini optimize et - OrderListForm ile aynı
            dataGridView.Scroll += (s, e) =>
            {
                if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
                {
                    dataGridView.Invalidate();
                    dataGridView.Update();
                }
            };

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnEkle.Click += (s, e) => BtnKesimEkle_Click(dataGridView);
            btnOnayla.Click += (s, e) => BtnKesimTalebiOnayla_Click(dataGridView);

            // Verileri yükle - Kolonlar zaten eklendi
            LoadKesimData(dataGridView);
        }

        private void AddKesimColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadKesimData(DataGridView dataGridView)
        {
            try
            {
                // Onaylanmış kesim kayıtları
                var cuttings = _cuttingRepository.GetByOrderId(_orderId);
                var completedData = cuttings.Select(c => new
                {
                    Id = c.Id,
                    Hatve = GetHatveLetter(c.Hatve),
                    Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                    MachineName = c.Machine?.Name ?? "",
                    SerialNumber = c.SerialNo?.SerialNumber ?? "",
                    TotalKg = c.TotalKg.ToString("F3", CultureInfo.InvariantCulture),
                    CutKg = c.CutKg.ToString("F3", CultureInfo.InvariantCulture),
                    CuttingCount = c.CuttingCount.ToString(),
                    WasteCount = c.WasteCount.HasValue ? c.WasteCount.Value.ToString() : "-",
                    WasteKg = c.WasteKg.ToString("F3", CultureInfo.InvariantCulture),
                    RemainingKg = c.RemainingKg.ToString("F3", CultureInfo.InvariantCulture),
                    EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : "",
                    Status = GetShortStatus("Tamamlandı")
                }).ToList();

                // Bekleyen kesim talepleri
                var requests = _cuttingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status != "Tamamlandı" && r.Status != "İptal")
                    .Select(r => new
                    {
                        Id = r.Id,
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F2", CultureInfo.InvariantCulture),
                        MachineName = r.Machine?.Name ?? "-",
                        SerialNumber = r.SerialNo?.SerialNumber ?? "-",
                        TotalKg = "-",
                        CutKg = "-",
                        CuttingCount = r.ActualCutCount?.ToString() ?? "-",
                        WasteCount = r.WasteCount?.ToString() ?? "-",
                        WasteKg = r.WasteCount.HasValue && r.OnePlateWeight > 0 
                            ? (r.WasteCount.Value * r.OnePlateWeight).ToString("F3", CultureInfo.InvariantCulture) 
                            : "-",
                        RemainingKg = r.RemainingKg.ToString("F3", CultureInfo.InvariantCulture),
                        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "-",
                        Status = GetShortStatus(r.Status)
                    }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(requests.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddKesimColumn(dataGridView, "Hatve", "Hatve", 60);
                        AddKesimColumn(dataGridView, "Size", "Ölçü", 70);
                        AddKesimColumn(dataGridView, "MachineName", "Makina No", 80);
                        AddKesimColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                        AddKesimColumn(dataGridView, "TotalKg", "Toplam Kg", 85);
                        AddKesimColumn(dataGridView, "CutKg", "Kesilen Kg", 85);
                        AddKesimColumn(dataGridView, "CuttingCount", "Kesilen Plaka Adedi", 120);
                        AddKesimColumn(dataGridView, "WasteCount", "Hurda Plaka Adedi", 120);
                        AddKesimColumn(dataGridView, "WasteKg", "Hurda Kg", 80);
                        AddKesimColumn(dataGridView, "RemainingKg", "Kalan Kg", 80);
                        AddKesimColumn(dataGridView, "EmployeeName", "Operatör", 120);
                        AddKesimColumn(dataGridView, "Status", "Durum", 80);
                    }

                    // Kolon başlıklarını kesinlikle göster
                    dataGridView.ColumnHeadersVisible = true;
                    dataGridView.RowHeadersVisible = false;
                    dataGridView.ColumnHeadersHeight = 40;
                    
                    // Veri kaynağını ayarla
                    dataGridView.DataSource = data;
                }
                finally
                {
                    // Layout işlemlerini devam ettir
                    dataGridView.ResumeLayout();
                }
                
                // DataSource ayarlandıktan SONRA HeaderText'leri tekrar ayarla
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    column.Visible = true;
                    column.ReadOnly = true;
                    // HeaderText'i tekrar ayarla
                    switch (column.Name)
                    {
                        case "Hatve": column.HeaderText = "Hatve"; break;
                        case "Size": column.HeaderText = "Ölçü"; break;
                        case "MachineName": column.HeaderText = "Makina No"; break;
                        case "SerialNumber": column.HeaderText = "Rulo Seri No"; break;
                        case "TotalKg": column.HeaderText = "Toplam Kg"; break;
                        case "CutKg": column.HeaderText = "Kesilen Kg"; break;
                        case "CuttingCount": column.HeaderText = "Kesilen Plaka Adedi"; break;
                        case "WasteCount": column.HeaderText = "Hurda Plaka Adedi"; break;
                        case "WasteKg": column.HeaderText = "Hurda Kg"; break;
                        case "RemainingKg": column.HeaderText = "Kalan Kg"; break;
                        case "EmployeeName": column.HeaderText = "Operatör"; break;
                        case "Status": column.HeaderText = "Durum"; break;
                    }
                }
                
                // Yeniden çiz
                dataGridView.Invalidate();
                dataGridView.Update();
                dataGridView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKesimTalebiOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                // Bu siparişe ait bekleyen kesim taleplerini getir
                var pendingRequests = _cuttingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status == "Kesimde" || r.Status == "Beklemede").ToList();

                if (pendingRequests.Count == 0)
                {
                    MessageBox.Show("Bu sipariş için onaylanacak kesim talebi bulunmamaktadır.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Eğer birden fazla talep varsa, kullanıcıdan seçmesini iste
                CuttingRequest selectedRequest = null;
                if (pendingRequests.Count == 1)
                {
                    selectedRequest = pendingRequests[0];
                }
                else
                {
                    // Dialog ile seçim yap
                    using (var selectDialog = new Form
                    {
                        Text = "Kesim Talebi Seç",
                        Width = 500,
                        Height = 400,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    })
                    {
                        var dgv = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            AllowUserToAddRows = false,
                            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                            MultiSelect = false
                        };

                        dgv.AutoGenerateColumns = false;
                        
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "Id", Visible = false });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hatve", DataPropertyName = "Hatve", HeaderText = "Hatve" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", DataPropertyName = "Size", HeaderText = "Ölçü" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestedPlateCount", DataPropertyName = "RequestedPlateCount", HeaderText = "İstenen" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActualCutCount", DataPropertyName = "ActualCutCount", HeaderText = "Kesilen" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Durum" });

                        dgv.DataSource = pendingRequests.Select(r => new
                        {
                            Id = r.Id,
                            Hatve = GetHatveLetter(r.Hatve),
                            Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                            RequestedPlateCount = r.RequestedPlateCount,
                            ActualCutCount = r.ActualCutCount?.ToString() ?? "-",
                            Status = GetShortStatus(r.Status)
                        }).ToList();

                        var btnSelect = new Button
                        {
                            Text = "Seç",
                            DialogResult = DialogResult.OK,
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        selectDialog.Controls.Add(dgv);
                        selectDialog.Controls.Add(btnSelect);
                        selectDialog.AcceptButton = btnSelect;

                        if (selectDialog.ShowDialog() == DialogResult.OK && dgv.SelectedRows.Count > 0)
                        {
                            var selectedRow = dgv.SelectedRows[0];
                            if (selectedRow != null && selectedRow.Cells["Id"] != null && selectedRow.Cells["Id"].Value != null)
                            {
                                var selectedId = (Guid)selectedRow.Cells["Id"].Value;
                                selectedRequest = pendingRequests.FirstOrDefault(r => r.Id == selectedId);
                            }
                        }
                    }
                }

                if (selectedRequest == null)
                    return;

                // Kesim adedi girilmiş mi kontrol et
                if (!selectedRequest.ActualCutCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce kesim adedini giriniz (Kesim Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Onaylama işlemi
                var result = MessageBox.Show(
                    $"Kesim talebi onaylanacak:\n\n" +
                    $"İstenen: {selectedRequest.RequestedPlateCount} adet\n" +
                    $"Kesilen: {selectedRequest.ActualCutCount.Value} adet\n\n" +
                    $"Onaylamak istediğinize emin misiniz?",
                    "Kesim Talebi Onayla",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Durumu "Tamamlandı" yap
                selectedRequest.Status = "Tamamlandı";
                selectedRequest.CompletionDate = DateTime.Now;
                _cuttingRequestRepository.Update(selectedRequest);

                // Kesim kaydı oluştur (Cutting)
                int actualCutCountValue = selectedRequest.ActualCutCount.Value;
                decimal actualCutKg = selectedRequest.OnePlateWeight * actualCutCountValue;

                // Rulodan gerçek kesilen adede göre düşülecek kg'ı hesapla
                // Önce mevcut rulo stokunu al
                var materialEntries = _materialEntryRepository.GetAll()
                    .Where(me => me.SerialNoId == selectedRequest.SerialNoId && me.IsActive)
                    .ToList();
                
                decimal totalEntryKg = materialEntries.Sum(me => me.Quantity);
                
                // Hurda kg hesapla: hurda adedi * plaka ağırlığı
                decimal wasteKg = selectedRequest.WasteCount.HasValue 
                    ? selectedRequest.WasteCount.Value * selectedRequest.OnePlateWeight 
                    : 0;
                
                // Bu seri no için daha önce kesilen kg'ları hesapla (sadece tamamlananlar, gerçek kesilen adede göre + hurda)
                var previousCutKg = _cuttingRequestRepository.GetAll()
                    .Where(cr => cr.SerialNoId == selectedRequest.SerialNoId && cr.IsActive && cr.Status == "Tamamlandı" && cr.Id != selectedRequest.Id)
                    .Sum(cr => 
                    {
                        int actualCount = cr.ActualCutCount ?? cr.RequestedPlateCount;
                        decimal prevWasteKg = cr.WasteCount.HasValue ? cr.WasteCount.Value * cr.OnePlateWeight : 0;
                        return cr.OnePlateWeight * actualCount + prevWasteKg;
                    });
                
                // Mevcut stok = Toplam giriş - Daha önce kesilenler (kesilen kg + hurda kg)
                decimal currentStockKg = totalEntryKg - previousCutKg;
                
                // Kalan kg = Mevcut stok - Bu kesimde kesilen kg - Bu kesimde hurda kg
                decimal remainingKg = currentStockKg - actualCutKg - wasteKg;

                var cutting = new Cutting
                {
                    OrderId = selectedRequest.OrderId,
                    Hatve = selectedRequest.Hatve,
                    Size = selectedRequest.Size,
                    MachineId = selectedRequest.MachineId,
                    SerialNoId = selectedRequest.SerialNoId,
                    TotalKg = currentStockKg, // Mevcut stok
                    CutKg = actualCutKg, // Gerçek kesilen kg
                    CuttingCount = actualCutCountValue, // Kesim adedi (gerçek kesilen adet)
                    PlakaAdedi = actualCutCountValue,
                    WasteCount = selectedRequest.WasteCount, // Hurda plaka adedi
                    WasteKg = wasteKg, // Hurda kg: hurda adedi * plaka ağırlığı
                    RemainingKg = remainingKg, // Gerçek kesilen adede göre kalan (hurda dahil)
                    EmployeeId = selectedRequest.EmployeeId,
                    CuttingDate = DateTime.Now
                };
                var cuttingId = _cuttingRepository.Insert(cutting);
                
                // Event feed kaydı ekle - Kesim onaylandı
                if (selectedRequest.OrderId != Guid.Empty)
                {
                    var orderForCutting = _orderRepository.GetById(selectedRequest.OrderId);
                    if (orderForCutting != null)
                    {
                        EventFeedService.CuttingApproved(selectedRequest.Id, selectedRequest.OrderId, orderForCutting.TrexOrderNo);
                    }
                }
                
                MessageBox.Show("Kesim talebi onaylandı ve kesim kaydı oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Verileri yeniden yükle
                LoadKesimData(dataGridView);
                
                // Rulo Stok Takip sayfasını yenile
                RuloStokTakipForm.NotifyCuttingSaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKesimEkle_Click(DataGridView dataGridView)
        {
            try
            {
                using (var dialog = new CuttingDialog(_machineRepository, _serialNoRepository, _employeeRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadKesimData(dataGridView);
                        
                        // Rulo Stok Takip sayfasını yenile
                        RuloStokTakipForm.NotifyCuttingSaved();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreatePresTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu (Pres taleplerini onaylamak için)
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Pres Onayla", ThemeColors.Success, Color.White, 130, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 130, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 80, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEkle.Location = new Point(buttonPanel.Width - 130 - 90, 5);
            buttonPanel.Controls.Add(btnEkle);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            AddPresColumn(dataGridView, "Date", "Tarih", 100);
            AddPresColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı (mm)", 130);
            AddPresColumn(dataGridView, "Hatve", "Hatve (mm)", 80);
            AddPresColumn(dataGridView, "Size", "Ölçü (cm)", 80);
            AddPresColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddPresColumn(dataGridView, "PressNo", "Pres No", 80);
            AddPresColumn(dataGridView, "Pressure", "Basınç", 80);
            AddPresColumn(dataGridView, "PressCount", "Pres Adedi", 85);
            AddPresColumn(dataGridView, "WasteAmount", "Hurda Miktarı", 100);
            AddPresColumn(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnEkle.Click += (s, e) => BtnPresEkle_Click(dataGridView);
            btnOnayla.Click += (s, e) => BtnPresTalebiOnayla_Click(dataGridView);

            // Verileri yükle
            LoadPresData(dataGridView);
        }

        private void AddPresColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadPresData(DataGridView dataGridView)
        {
            try
            {
                // Onaylanmış pres kayıtları
                var pressings = _pressingRepository.GetByOrderId(_orderId);
                // Tamamlanmış PressingRequest'leri al (WasteCount için)
                var completedRequests = _pressingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status == "Tamamlandı")
                    .ToList();
                
                var completedData = pressings.Select(p =>
                {
                    // Bu pressing için ilgili PressingRequest'i bul (CuttingId ve OrderId üzerinden, en son tamamlananı al)
                    var relatedRequest = completedRequests
                        .Where(r => r.CuttingId == p.CuttingId && r.OrderId == p.OrderId)
                        .OrderByDescending(r => r.CompletionDate ?? r.ModifiedDate ?? r.CreatedDate)
                        .FirstOrDefault();
                    
                    return new
                    {
                        Id = p.Id,
                        Date = p.PressingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        PlateThickness = p.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        Hatve = GetHatveLetter(p.Hatve),
                        Size = p.Size.ToString("F2", CultureInfo.InvariantCulture),
                        SerialNumber = p.SerialNo?.SerialNumber ?? "",
                        PressNo = p.PressNo ?? "",
                        Pressure = p.Pressure.ToString("F3", CultureInfo.InvariantCulture),
                        PressCount = p.PressCount.ToString(),
                        WasteAmount = relatedRequest?.WasteCount.HasValue == true ? relatedRequest.WasteCount.Value.ToString() : "-",
                        EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : "",
                        Status = GetShortStatus("Tamamlandı")
                    };
                }).ToList();

                // Bekleyen pres talepleri
                var requests = _pressingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status != "Tamamlandı" && r.Status != "İptal")
                    .Select(r => new
                    {
                        Id = r.Id,
                        Date = r.RequestDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F2", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "-",
                        PressNo = r.PressNo ?? "-",
                        Pressure = r.Pressure.ToString("F3", CultureInfo.InvariantCulture),
                        PressCount = r.ResultedPressCount?.ToString() ?? "-",
                        WasteAmount = r.WasteCount.HasValue ? r.WasteCount.Value.ToString() : "-",
                        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "-",
                        Status = GetShortStatus(r.Status)
                    }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(requests.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddPresColumn(dataGridView, "Date", "Tarih", 100);
                        AddPresColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
                        AddPresColumn(dataGridView, "Hatve", "Hatve", 60);
                        AddPresColumn(dataGridView, "Size", "Ölçü", 70);
                        AddPresColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                        AddPresColumn(dataGridView, "PressNo", "Pres No", 80);
                        AddPresColumn(dataGridView, "Pressure", "Basınç", 80);
                        AddPresColumn(dataGridView, "PressCount", "Pres Adedi", 85);
                        AddPresColumn(dataGridView, "WasteAmount", "Hurda Miktarı", 100);
                        AddPresColumn(dataGridView, "EmployeeName", "Operatör", 120);
                        AddPresColumn(dataGridView, "Status", "Durum", 80);
                    }

                    // Kolon başlıklarını kesinlikle göster
                    dataGridView.ColumnHeadersVisible = true;
                    dataGridView.RowHeadersVisible = false;
                    dataGridView.ColumnHeadersHeight = 40;
                    
                    // Veri kaynağını ayarla
                    dataGridView.DataSource = data;
                }
                finally
                {
                    // Layout işlemlerini devam ettir
                    dataGridView.ResumeLayout();
                }
                
                // DataSource ayarlandıktan SONRA HeaderText'leri tekrar ayarla
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    column.Visible = true;
                    column.ReadOnly = true;
                    // HeaderText'i tekrar ayarla
                    switch (column.Name)
                    {
                        case "Date": column.HeaderText = "Tarih"; break;
                        case "PlateThickness": column.HeaderText = "Plaka Kalınlığı"; break;
                        case "Hatve": column.HeaderText = "Hatve"; break;
                        case "Size": column.HeaderText = "Ölçü"; break;
                        case "SerialNumber": column.HeaderText = "Rulo Seri No"; break;
                        case "PressNo": column.HeaderText = "Pres No"; break;
                        case "Pressure": column.HeaderText = "Basınç"; break;
                        case "PressCount": column.HeaderText = "Pres Adedi"; break;
                        case "WasteAmount": column.HeaderText = "Hurda Miktarı"; break;
                        case "EmployeeName": column.HeaderText = "Operatör"; break;
                        case "Status": column.HeaderText = "Durum"; break;
                    }
                }
                
                // Yeniden çiz
                dataGridView.Invalidate();
                dataGridView.Update();
                dataGridView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPresTalebiOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                // Bu siparişe ait bekleyen pres taleplerini getir
                var pendingRequests = _pressingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status == "Presde" || r.Status == "Beklemede").ToList();

                if (pendingRequests.Count == 0)
                {
                    MessageBox.Show("Bu sipariş için onaylanacak pres talebi bulunmamaktadır.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Eğer birden fazla talep varsa, kullanıcıdan seçmesini iste
                PressingRequest selectedRequest = null;
                if (pendingRequests.Count == 1)
                {
                    selectedRequest = pendingRequests[0];
                }
                else
                {
                    // Dialog ile seçim yap
                    using (var selectDialog = new Form
                    {
                        Text = "Pres Talebi Seç",
                        Width = 500,
                        Height = 400,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    })
                    {
                        var dgv = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            AllowUserToAddRows = false,
                            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                            MultiSelect = false
                        };

                        dgv.AutoGenerateColumns = false;
                        
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "Id", Visible = false });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hatve", DataPropertyName = "Hatve", HeaderText = "Hatve" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", DataPropertyName = "Size", HeaderText = "Ölçü" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestedPressCount", DataPropertyName = "RequestedPressCount", HeaderText = "İstenen" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActualPressCount", DataPropertyName = "ActualPressCount", HeaderText = "Preslenen (Kullanılan)" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ResultedPressCount", DataPropertyName = "ResultedPressCount", HeaderText = "Oluşan" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Durum" });

                        dgv.DataSource = pendingRequests.Select(r => new
                        {
                            Id = r.Id,
                            Hatve = GetHatveLetter(r.Hatve),
                            Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                            RequestedPressCount = r.RequestedPressCount,
                            ActualPressCount = r.ActualPressCount?.ToString() ?? "-",
                            ResultedPressCount = r.ResultedPressCount?.ToString() ?? "-",
                            Status = GetShortStatus(r.Status)
                        }).ToList();

                        var btnSelect = new Button
                        {
                            Text = "Seç",
                            DialogResult = DialogResult.OK,
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        selectDialog.Controls.Add(dgv);
                        selectDialog.Controls.Add(btnSelect);
                        selectDialog.AcceptButton = btnSelect;

                        if (selectDialog.ShowDialog() == DialogResult.OK && dgv.SelectedRows.Count > 0)
                        {
                            var selectedRow = dgv.SelectedRows[0];
                            if (selectedRow != null && selectedRow.Cells["Id"] != null && selectedRow.Cells["Id"].Value != null)
                            {
                                var selectedId = (Guid)selectedRow.Cells["Id"].Value;
                                selectedRequest = pendingRequests.FirstOrDefault(r => r.Id == selectedId);
                            }
                        }
                    }
                }

                if (selectedRequest == null)
                    return;

                // Preslenmiş adet girilmiş mi kontrol et
                if (!selectedRequest.ResultedPressCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce preslenmiş adedi giriniz (Pres Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Hurda adedi girilmiş mi kontrol et
                if (!selectedRequest.WasteCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce hurda adedini giriniz (Pres Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kontrol: İstenen Pres = Preslenmiş + Hurda (1:1 oran)
                int toplamCikis = selectedRequest.ResultedPressCount.Value + selectedRequest.WasteCount.Value;
                if (toplamCikis != selectedRequest.RequestedPressCount)
                {
                    MessageBox.Show(
                        $"Hata: İstenen Pres ({selectedRequest.RequestedPressCount}) ile çıktılar eşleşmiyor!\n\n" +
                        $"Preslenmiş adet: {selectedRequest.ResultedPressCount.Value}\n" +
                        $"Hurda adedi: {selectedRequest.WasteCount.Value}\n" +
                        $"Toplam: {toplamCikis}\n\n" +
                        $"İstenen Pres = Preslenmiş Adet + Hurda Adedi olmalıdır!",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Onaylama işlemi
                // ActualPressCount = ResultedPressCount + WasteCount
                int calculatedActualPressCount = selectedRequest.ResultedPressCount.Value + selectedRequest.WasteCount.Value;
                
                var result = MessageBox.Show(
                    $"Pres talebi onaylanacak:\n\n" +
                    $"İstenen: {selectedRequest.RequestedPressCount} adet\n" +
                    $"Preslenmiş adet: {selectedRequest.ResultedPressCount.Value} adet\n" +
                    $"Hurda adedi: {selectedRequest.WasteCount.Value} adet\n" +
                    $"Kullanılan (otomatik hesaplanan): {calculatedActualPressCount} adet\n\n" +
                    $"Onaylamak istediğinize emin misiniz?",
                    "Pres Talebi Onayla",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Kesilmiş stoktan düş (ActualPressCount kadar)
                if (selectedRequest.CuttingId.HasValue)
                {
                    var cutting = _cuttingRepository.GetById(selectedRequest.CuttingId.Value);
                    if (cutting != null)
                    {
                        // Kesilmiş stoktan kullanılan adeti düş
                        // Not: Kesilmiş stok zaten Pressing kayıtlarından düşülmüş olabilir, 
                        // bu yüzden sadece kenetleme işlemlerinden düşülmemiş olanları kontrol ediyoruz
                        // Burada sadece kontrol yapıyoruz, asıl düşme işlemi Preslenmiş stoktakip formunda yapılıyor
                        // Ama yine de cutting'in PlakaAdedi'sini güncelleyebiliriz (eğer gerekirse)
                    }
                }

                // ActualPressCount'u güncelle (calculatedActualPressCount zaten yukarıda hesaplanmıştı)
                selectedRequest.ActualPressCount = calculatedActualPressCount;
                
                // Durumu "Tamamlandı" yap
                selectedRequest.Status = "Tamamlandı";
                selectedRequest.CompletionDate = DateTime.Now;
                _pressingRequestRepository.Update(selectedRequest);

                // Pres kaydı oluştur (Pressing) - ResultedPressCount preslenmiş stoğa eklenecek
                var pressing = new Pressing
                {
                    OrderId = selectedRequest.OrderId,
                    PlateThickness = selectedRequest.PlateThickness,
                    Hatve = selectedRequest.Hatve,
                    Size = selectedRequest.Size,
                    SerialNoId = selectedRequest.SerialNoId,
                    CuttingId = selectedRequest.CuttingId,
                    PressNo = selectedRequest.PressNo,
                    Pressure = selectedRequest.Pressure,
                    PressCount = selectedRequest.ResultedPressCount.Value, // Oluşan preslenmiş adet
                    WasteAmount = 0, // Artık WasteCount kullanılıyor, WasteAmount deprecated
                    EmployeeId = selectedRequest.EmployeeId,
                    PressingDate = DateTime.Now
                };
                var pressingId = _pressingRepository.Insert(pressing);
                
                // Event feed kaydı ekle - Pres onaylandı
                if (selectedRequest.OrderId != Guid.Empty)
                {
                    var orderForPressing = _orderRepository.GetById(selectedRequest.OrderId);
                    if (orderForPressing != null)
                    {
                        EventFeedService.PressingApproved(selectedRequest.Id, selectedRequest.OrderId, orderForPressing.TrexOrderNo);
                    }
                }
                
                MessageBox.Show("Pres talebi onaylandı ve pres kaydı oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Verileri yeniden yükle
                LoadPresData(dataGridView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPresEkle_Click(DataGridView dataGridView)
        {
            try
            {
                using (var dialog = new PressingDialog(_serialNoRepository, _employeeRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadPresData(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateClampingTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu (Kenetleme taleplerini onaylamak için)
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Kenetleme Onayla", ThemeColors.Success, Color.White, 150, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 150, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 80, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEkle.Location = new Point(buttonPanel.Width - 150 - 90, 5);
            buttonPanel.Controls.Add(btnEkle);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            AddClampingColumn(dataGridView, "Date", "Tarih", 100);
            AddClampingColumn(dataGridView, "OrderNo", "Sipariş No", 90);
            AddClampingColumn(dataGridView, "Hatve", "Hatve (mm)", 80);
            AddClampingColumn(dataGridView, "Size", "Ölçü (cm)", 80);
            AddClampingColumn(dataGridView, "Length", "Uzunluk (mm)", 100);
            AddClampingColumn(dataGridView, "ClampCount", "Adet", 70, readOnly: false); // Editable - sadece bekleyen talepler için
            AddClampingColumn(dataGridView, "Customer", "Müşteri", 130);
            AddClampingColumn(dataGridView, "UsedPlateCount", "Kullanılan Plaka Adedi", 140);
            AddClampingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı (mm)", 130);
            AddClampingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddClampingColumn(dataGridView, "MachineName", "Makina Adı", 100);
            AddClampingColumn(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnEkle.Click += (s, e) => BtnClampingEkle_Click(dataGridView);
            btnOnayla.Click += (s, e) => BtnClampingRequestOnayla_Click(dataGridView);
            
            // CellValueChanged event'i - Adet değiştiğinde kaydet (sadece bekleyen talepler için)
            dataGridView.CellValueChanged += (s, e) => ClampingDataGridView_CellValueChanged(s, e, dataGridView);
            
            // CellBeginEdit event'i - Sadece bekleyen talepler için editable yap
            dataGridView.CellBeginEdit += (s, e) => ClampingDataGridView_CellBeginEdit(s, e, dataGridView);

            // Verileri yükle
            LoadClampingData(dataGridView);
        }

        private void AddClampingColumn(DataGridView dgv, string dataPropertyName, string headerText, int width, bool readOnly = true)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = readOnly
            };
            dgv.Columns.Add(column);
        }

        private void LoadClampingData(DataGridView dataGridView)
        {
            try
            {
                var orderForClamping = _orderRepository.GetById(_orderId);
                
                // Onaylanmış kenetleme kayıtları
                // NOT: Kenetlemede kapaksız üretim yapıldığı için uzunluktan kapak boyu çıkarılmıyor
                var clampings = _clampingRepository.GetByOrderId(_orderId);
                var completedData = clampings.Select(c => new
                {
                    Id = c.Id,
                    Date = c.ClampingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForClamping?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(c.Hatve),
                    Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = c.Length.ToString("F2", CultureInfo.InvariantCulture), // Kapaksız - doğrudan uzunluk
                    ClampCount = c.ClampCount.ToString(),
                    Customer = orderForClamping?.Company?.Name ?? "",
                    UsedPlateCount = c.UsedPlateCount.ToString(),
                    PlateThickness = c.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = c.SerialNo?.SerialNumber ?? "",
                    MachineName = c.Machine?.Name ?? "",
                    EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : "",
                    Status = GetShortStatus("Tamamlandı")
                }).ToList();

                // Bekleyen kenetleme talepleri
                // NOT: Kenetlemede kapaksız üretim yapıldığı için uzunluktan kapak boyu çıkarılmıyor
                var requests = _clampingRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status != "Tamamlandı" && r.Status != "İptal")
                    .Select(r => new
                    {
                        Id = r.Id,
                        Date = r.RequestDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        OrderNo = orderForClamping?.TrexOrderNo ?? "",
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = r.Length.ToString("F2", CultureInfo.InvariantCulture), // Kapaksız - doğrudan uzunluk
                        ClampCount = r.ResultedClampCount?.ToString() ?? "-",
                        Customer = orderForClamping?.Company?.Name ?? "",
                        UsedPlateCount = r.ActualClampCount?.ToString() ?? "-",
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "-",
                        MachineName = r.Machine?.Name ?? "-",
                        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "-",
                        Status = GetShortStatus(r.Status)
                    }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(requests.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddClampingColumn(dataGridView, "Date", "Tarih", 100);
                        AddClampingColumn(dataGridView, "OrderNo", "Sipariş No", 90);
                    AddClampingColumn(dataGridView, "Hatve", "Hatve", 60);
                    AddClampingColumn(dataGridView, "Size", "Ölçü", 70);
                    AddClampingColumn(dataGridView, "Length", "Uzunluk", 80);
                    AddClampingColumn(dataGridView, "ClampCount", "Adet", 70, readOnly: false); // Editable - sadece bekleyen talepler için
                    AddClampingColumn(dataGridView, "Customer", "Müşteri", 130);
                    AddClampingColumn(dataGridView, "UsedPlateCount", "Kullanılan Plaka Adedi", 140);
                    AddClampingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
                    AddClampingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                    AddClampingColumn(dataGridView, "MachineName", "Makina Adı", 100);
                    AddClampingColumn(dataGridView, "EmployeeName", "Operatör", 120);
                    AddClampingColumn(dataGridView, "Status", "Durum", 80);
                }

                // Kolon başlıklarını kesinlikle göster
                dataGridView.ColumnHeadersVisible = true;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ColumnHeadersHeight = 40;
                
                // Veri kaynağını ayarla
                dataGridView.DataSource = data;
                    }
                    finally
                    {
                        // Layout işlemlerini devam ettir
                        dataGridView.ResumeLayout();
                    }
                
                // DataSource ayarlandıktan SONRA HeaderText'leri tekrar ayarla
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    column.Visible = true;
                    column.ReadOnly = true;
                    // HeaderText'i tekrar ayarla
                    switch (column.Name)
                    {
                        case "Date": column.HeaderText = "Tarih"; break;
                        case "OrderNo": column.HeaderText = "Sipariş No"; break;
                        case "Hatve": column.HeaderText = "Hatve (mm)"; break;
                        case "Size": column.HeaderText = "Ölçü (cm)"; break;
                        case "Length": column.HeaderText = "Uzunluk (mm)"; break;
                        case "ClampCount": column.HeaderText = "Adet"; break;
                        case "Customer": column.HeaderText = "Müşteri"; break;
                        case "UsedPlateCount": column.HeaderText = "Kullanılan Plaka Adedi"; break;
                        case "PlateThickness": column.HeaderText = "Plaka Kalınlığı (mm)"; break;
                        case "SerialNumber": column.HeaderText = "Rulo Seri No"; break;
                        case "MachineName": column.HeaderText = "Makina Adı"; break;
                        case "EmployeeName": column.HeaderText = "Operatör"; break;
                    }
                }
                
                // Yeniden çiz
                dataGridView.Invalidate();
                dataGridView.Update();
                dataGridView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClampingEkle_Click(DataGridView dataGridView)
        {
            try
            {
                using (var dialog = new ClampingDialog(_serialNoRepository, _employeeRepository, _machineRepository, _pressingRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadClampingData(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClampingRequestOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                var pendingRequests = _clampingRequestRepository.GetPendingRequests()
                    .Where(r => r.OrderId == _orderId)
                    .ToList();

                if (!pendingRequests.Any())
                {
                    MessageBox.Show("Bu sipariş için bekleyen kenetleme talebi bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ClampingRequest selectedRequest = null;

                // Eğer tek bir talep varsa direkt seç
                if (pendingRequests.Count == 1)
                {
                    selectedRequest = pendingRequests.First();
                }
                else
                {
                    // Birden fazla talep varsa seçim dialogu göster
                    using (var selectDialog = new Form
                    {
                        Text = "Kenetleme Talebi Seç",
                        Width = 800,
                        Height = 500,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false
                    })
                    {
                        var dgv = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            AllowUserToAddRows = false,
                            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                            MultiSelect = false
                        };

                        dgv.AutoGenerateColumns = false;
                        
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "Id", Visible = false });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hatve", DataPropertyName = "Hatve", HeaderText = "Hatve" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", DataPropertyName = "Size", HeaderText = "Ölçü" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestedClampCount", DataPropertyName = "RequestedClampCount", HeaderText = "İstenen" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActualClampCount", DataPropertyName = "ActualClampCount", HeaderText = "Kenetlenecek (Kullanılan)" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ResultedClampCount", DataPropertyName = "ResultedClampCount", HeaderText = "Oluşan" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Durum" });

                        dgv.DataSource = pendingRequests.Select(r => new
                        {
                            Id = r.Id,
                            Hatve = GetHatveLetter(r.Hatve),
                            Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                            RequestedClampCount = r.RequestedClampCount,
                            ActualClampCount = r.ActualClampCount?.ToString() ?? "-",
                            ResultedClampCount = r.ResultedClampCount?.ToString() ?? "-",
                            Status = GetShortStatus(r.Status)
                        }).ToList();

                        var btnSelect = new Button
                        {
                            Text = "Seç",
                            DialogResult = DialogResult.OK,
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        selectDialog.Controls.Add(dgv);
                        selectDialog.Controls.Add(btnSelect);
                        selectDialog.AcceptButton = btnSelect;

                        if (selectDialog.ShowDialog() == DialogResult.OK && dgv.SelectedRows.Count > 0)
                        {
                            var selectedRow = dgv.SelectedRows[0];
                            if (selectedRow != null && selectedRow.Cells["Id"] != null && selectedRow.Cells["Id"].Value != null)
                            {
                                var selectedId = (Guid)selectedRow.Cells["Id"].Value;
                                selectedRequest = pendingRequests.FirstOrDefault(r => r.Id == selectedId);
                            }
                        }
                    }
                }

                if (selectedRequest == null)
                    return;

                // Kenetleme adedi girilmiş mi kontrol et
                if (!selectedRequest.ActualClampCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce kaç tane preslenmiş kenetleneceğini giriniz (Kenetleme Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Oluşan adet girilmiş mi kontrol et
                if (!selectedRequest.ResultedClampCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce kaç tane oluştuğunu giriniz (Kenetleme Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Onaylama işlemi
                var result = MessageBox.Show(
                    $"Kenetleme talebi onaylanacak:\n\n" +
                    $"İstenen: {selectedRequest.RequestedClampCount} adet\n" +
                    $"Kenetlenecek (preslenmiş stoktan kullanılan): {selectedRequest.ActualClampCount.Value} adet\n" +
                    $"Oluşan (kenetlenmiş stoğa eklenecek): {selectedRequest.ResultedClampCount.Value} adet\n\n" +
                    $"Onaylamak istediğinize emin misiniz?",
                    "Kenetleme Talebi Onayla",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Preslenmiş stoktan düş (ActualClampCount kadar)
                // Not: Preslenmiş stok takibi başka bir formda yapılıyor

                // Durumu "Tamamlandı" yap
                selectedRequest.Status = "Tamamlandı";
                selectedRequest.CompletionDate = DateTime.Now;
                _clampingRequestRepository.Update(selectedRequest);

                // Kenetleme kaydı oluştur (Clamping) - ResultedClampCount kenetlenmiş stoğa eklenecek
                var clamping = new Clamping
                {
                    OrderId = selectedRequest.OrderId,
                    PressingId = selectedRequest.PressingId,
                    PlateThickness = selectedRequest.PlateThickness,
                    Hatve = selectedRequest.Hatve,
                    Size = selectedRequest.Size,
                    Length = selectedRequest.Length,
                    SerialNoId = selectedRequest.SerialNoId,
                    MachineId = selectedRequest.MachineId,
                    ClampCount = selectedRequest.ResultedClampCount.Value, // Oluşan kenetlenmiş adet
                    UsedPlateCount = selectedRequest.ActualClampCount.Value, // Kullanılan preslenmiş adet
                    EmployeeId = selectedRequest.EmployeeId,
                    ClampingDate = DateTime.Now
                };
                var clampingId = _clampingRepository.Insert(clamping);
                
                // Event feed kaydı ekle - Kenetleme onaylandı
                if (selectedRequest.OrderId != Guid.Empty)
                {
                    var orderForClamping = _orderRepository.GetById(selectedRequest.OrderId);
                    if (orderForClamping != null)
                    {
                        EventFeedService.ClampingApproved(selectedRequest.Id, selectedRequest.OrderId, orderForClamping.TrexOrderNo);
                    }
                }
                
                MessageBox.Show("Kenetleme talebi onaylandı ve kenetleme kaydı oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Verileri yeniden yükle
                LoadClampingData(dataGridView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClampingDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e, DataGridView dataGridView)
        {
            // Sadece ClampCount kolonu için ve sadece bekleyen talepler için editable yap
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            var columnName = dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ClampCount")
            {
                e.Cancel = true;
                return;
            }

            // Tamamlanmış kayıtlar için düzenlemeyi engelle
            var row = dataGridView.Rows[e.RowIndex];
            if (row.DataBoundItem != null)
            {
                var item = row.DataBoundItem;
                var statusProperty = item.GetType().GetProperty("Status");
                if (statusProperty != null)
                {
                    var status = statusProperty.GetValue(item)?.ToString();
                    if (status == "Tamam" || status == "Tamamlandı")
                    {
                        e.Cancel = true; // Tamamlanmış kayıtlar düzenlenemez
                        return;
                    }
                }
            }
        }

        private void ClampingDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e, DataGridView dataGridView)
        {
            // Sadece ClampCount kolonu için kaydet
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ClampCount")
                return;

            try
            {
                var row = dataGridView.Rows[e.RowIndex];
                if (row.DataBoundItem == null)
                    return;

                // Id'yi al
                Guid requestId = Guid.Empty;
                var item = row.DataBoundItem;
                var idProperty = item.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    requestId = (Guid)idProperty.GetValue(item);
                }

                if (requestId == Guid.Empty)
                    return;

                // Status kontrolü - sadece bekleyen talepler için kaydet
                var statusProperty = item.GetType().GetProperty("Status");
                if (statusProperty != null)
                {
                    var status = statusProperty.GetValue(item)?.ToString();
                    if (status == "Tamam" || status == "Tamamlandı")
                    {
                        LoadClampingData(dataGridView); // Veriyi yeniden yükle
                        return; // Tamamlanmış kayıtlar güncellenemez
                    }
                }

                var request = _clampingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Yeni değeri al
                var newValueStr = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(newValueStr) || newValueStr == "-")
                {
                    request.ResultedClampCount = null;
                }
                else if (int.TryParse(newValueStr, out int newValue))
                {
                    request.ResultedClampCount = newValue;
                }
                else
                {
                    MessageBox.Show("Lütfen geçerli bir sayı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadClampingData(dataGridView); // Veriyi yeniden yükle
                    return;
                }

                _clampingRequestRepository.Update(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Adet kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadClampingData(dataGridView); // Hata durumunda veriyi yeniden yükle
            }
        }

        private void CreateAssemblyTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu (Montaj taleplerini onaylamak için)
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Montaj Onayla", ThemeColors.Success, Color.White, 140, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 140, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 80, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEkle.Location = new Point(buttonPanel.Width - 140 - 90, 5);
            buttonPanel.Controls.Add(btnEkle);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            AddAssemblyColumn(dataGridView, "Date", "Tarih", 100);
            AddAssemblyColumn(dataGridView, "OrderNo", "Sipariş No", 90);
            AddAssemblyColumn(dataGridView, "Hatve", "Hatve (mm)", 80);
            AddAssemblyColumn(dataGridView, "Size", "Ölçü (cm)", 80);
            AddAssemblyColumn(dataGridView, "Length", "Uzunluk (mm)", 100);
            AddAssemblyColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 90);
            AddAssemblyColumn(dataGridView, "Customer", "Müşteri", 130);
            AddAssemblyColumn(dataGridView, "UsedClampCount", "Kullanılan Kenet Adedi", 140);
            AddAssemblyColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı (mm)", 130);
            AddAssemblyColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddAssemblyColumn(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnEkle.Click += (s, e) => BtnAssemblyEkle_Click(dataGridView);
            btnOnayla.Click += (s, e) => BtnAssemblyRequestOnayla_Click(dataGridView);

            // Verileri yükle
            LoadAssemblyData(dataGridView);
        }

        private void AddAssemblyColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadAssemblyData(DataGridView dataGridView)
        {
            try
            {
                var orderForAssembly = _orderRepository.GetById(_orderId);
                int kapakBoyuMM = GetKapakBoyuFromOrder(orderForAssembly);
                
                // Onaylanmış montaj kayıtları
                var assemblies = _assemblyRepository.GetByOrderId(_orderId);
                var completedData = assemblies.Select(a => new
                {
                    Id = a.Id,
                    Date = a.AssemblyDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForAssembly?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(a.Hatve),
                    Size = a.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = a.Length.ToString("F2", CultureInfo.InvariantCulture), // Length MM cinsinden saklanıyor
                    AssemblyCount = a.AssemblyCount.ToString(),
                    Customer = orderForAssembly?.Company?.Name ?? "",
                    UsedClampCount = a.UsedClampCount.ToString(),
                    PlateThickness = a.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = a.SerialNo?.SerialNumber ?? "",
                    EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "",
                    Status = GetShortStatus("Tamamlandı")
                }).ToList();

                // Bekleyen montaj talepleri
                var requests = _assemblyRequestRepository.GetByOrderId(_orderId)
                    .Where(r => r.Status != "Tamamlandı" && r.Status != "İptal")
                    .Select(r => new
                    {
                        Id = r.Id,
                        Date = r.RequestDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        OrderNo = orderForAssembly?.TrexOrderNo ?? "",
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = r.Length.ToString("F2", CultureInfo.InvariantCulture), // Length MM cinsinden saklanıyor
                        AssemblyCount = r.ResultedAssemblyCount?.ToString() ?? "-",
                        Customer = orderForAssembly?.Company?.Name ?? "",
                        UsedClampCount = r.ActualClampCount?.ToString() ?? "-",
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "-",
                        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "-",
                        Status = GetShortStatus(r.Status)
                    }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(requests.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddAssemblyColumn(dataGridView, "Date", "Tarih", 100);
                        AddAssemblyColumn(dataGridView, "OrderNo", "Sipariş No", 90);
                        AddAssemblyColumn(dataGridView, "Hatve", "Hatve (mm)", 80);
                        AddAssemblyColumn(dataGridView, "Size", "Ölçü (cm)", 80);
                        AddAssemblyColumn(dataGridView, "Length", "Uzunluk (mm)", 100);
                        AddAssemblyColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 90);
                        AddAssemblyColumn(dataGridView, "Customer", "Müşteri", 130);
                        AddAssemblyColumn(dataGridView, "UsedClampCount", "Kullanılan Kenet Adedi", 140);
                        AddAssemblyColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı (mm)", 130);
                        AddAssemblyColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                        AddAssemblyColumn(dataGridView, "EmployeeName", "Operatör", 120);
                        AddAssemblyColumn(dataGridView, "Status", "Durum", 80);
                    }

                    // Veri kaynağını ayarla
                    dataGridView.DataSource = data;
                }
                finally
                {
                    dataGridView.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAssemblyEkle_Click(DataGridView dataGridView)
        {
            try
            {
                // YM (stok) ürünleri için montaj işlemi yapılamaz
                var orderForAssemblyCheck = _orderRepository.GetById(_orderId);
                if (orderForAssemblyCheck != null && orderForAssemblyCheck.IsStockOrder)
                {
                    MessageBox.Show("Stok (YM) ürünleri için montaj işlemi yapılamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var dialog = new AssemblyDialog(_serialNoRepository, _employeeRepository, _machineRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadAssemblyData(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAssemblyRequestOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                // Bu siparişe ait bekleyen ve montajda olan montaj taleplerini getir
                // "Tamamlandı" statusündeki talepler için daha önce bir Assembly kaydı oluşturulmuş mu kontrol et
                // Eğer oluşturulmuşsa, bu talep artık gösterilmemeli (zaten onaylanmış)
                var allRequests = _assemblyRequestRepository.GetAll()
                    .Where(r => r.OrderId == _orderId && r.Status != "İptal" && r.IsActive).ToList();
                
                // Bu siparişe ait tüm Assembly kayıtlarını al (bir kere al, tekrar tekrar sorgu atmamak için)
                var allAssemblies = _assemblyRepository.GetByOrderId(_orderId);
                
                // Bekleyen talepleri filtrele
                var pendingRequests = new List<AssemblyRequest>();
                foreach (var request in allRequests)
                {
                    if (request.Status == "Montajda" || request.Status == "Beklemede")
                    {
                        // Bekleyen ve montajda olan talepler her zaman gösterilmeli
                        pendingRequests.Add(request);
                    }
                    else if (request.Status == "Tamamlandı")
                    {
                        // "Tamamlandı" statusündeki talepler için Assembly kaydı var mı kontrol et
                        // Eğer varsa, bu talep zaten onaylanmış demektir ve tekrar gösterilmemeli
                        bool hasAssemblyRecord = allAssemblies.Any(a => 
                            a.ClampingId == request.ClampingId && 
                            a.OrderId == request.OrderId &&
                            Math.Abs(a.Hatve - request.Hatve) < 0.01m &&
                            Math.Abs(a.Size - request.Size) < 0.1m &&
                            Math.Abs(a.PlateThickness - request.PlateThickness) < 0.001m &&
                            Math.Abs(a.Length - request.Length) < 0.1m &&
                            a.AssemblyCount == request.ResultedAssemblyCount &&
                            a.UsedClampCount == request.ActualClampCount);
                        
                        // Eğer Assembly kaydı yoksa, bu talep henüz onaylanmamış demektir (stok tüketimi için bekliyor)
                        if (!hasAssemblyRecord)
                        {
                            pendingRequests.Add(request);
                        }
                    }
                }

                if (pendingRequests.Count == 0)
                {
                    MessageBox.Show("Bu sipariş için onaylanacak montaj talebi bulunmamaktadır.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Eğer birden fazla talep varsa, kullanıcıdan seçmesini iste
                AssemblyRequest selectedRequest = null;
                if (pendingRequests.Count == 1)
                {
                    selectedRequest = pendingRequests.First();
                }
                else
                {
                    // Dialog ile seçim yap
                    using (var selectDialog = new Form
                    {
                        Text = "Montaj Talebi Seç",
                        Width = 800,
                        Height = 500,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false
                    })
                    {
                        var dgv = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            AllowUserToAddRows = false,
                            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                            MultiSelect = false
                        };

                        dgv.AutoGenerateColumns = false;
                        
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "Id", Visible = false });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hatve", DataPropertyName = "Hatve", HeaderText = "Hatve" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", DataPropertyName = "Size", HeaderText = "Ölçü" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestedAssemblyCount", DataPropertyName = "RequestedAssemblyCount", HeaderText = "İstenen" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActualClampCount", DataPropertyName = "ActualClampCount", HeaderText = "Kullanılan Kenet" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ResultedAssemblyCount", DataPropertyName = "ResultedAssemblyCount", HeaderText = "Oluşan Montaj" });
                        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Durum" });

                        dgv.DataSource = pendingRequests.Select(r => new
                        {
                            Id = r.Id,
                            Hatve = GetHatveLetter(r.Hatve),
                            Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                            RequestedAssemblyCount = r.RequestedAssemblyCount,
                            ActualClampCount = r.ActualClampCount?.ToString() ?? "-",
                            ResultedAssemblyCount = r.ResultedAssemblyCount?.ToString() ?? "-",
                            Status = GetShortStatus(r.Status)
                        }).ToList();

                        var btnSelect = new Button
                        {
                            Text = "Seç",
                            DialogResult = DialogResult.OK,
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        selectDialog.Controls.Add(dgv);
                        selectDialog.Controls.Add(btnSelect);
                        selectDialog.AcceptButton = btnSelect;

                        if (selectDialog.ShowDialog() == DialogResult.OK && dgv.SelectedRows.Count > 0)
                        {
                            var selectedRow = dgv.SelectedRows[0];
                            if (selectedRow != null && selectedRow.Cells["Id"] != null && selectedRow.Cells["Id"].Value != null)
                            {
                                var selectedId = (Guid)selectedRow.Cells["Id"].Value;
                                selectedRequest = pendingRequests.FirstOrDefault(r => r.Id == selectedId);
                            }
                        }
                    }
                }

                if (selectedRequest == null)
                    return;

                // Montajlanan kenet adedi girilmiş mi kontrol et
                // Montajlanan kenet sayısı = Oluşan montaj sayısı (1:1 oran)
                if (!selectedRequest.ActualClampCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce kaç tane kenet montajlandığını giriniz (Montaj Talepleri sayfasından).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Montajlanan kenet sayısı = Oluşan montaj sayısı (otomatik eşitle)
                int montajlananKenetSayisi = selectedRequest.ActualClampCount.Value;
                int olusanMontajSayisi = montajlananKenetSayisi; // 1:1 oran

                // Eğer ResultedAssemblyCount girilmişse ve farklıysa uyarı ver
                if (selectedRequest.ResultedAssemblyCount.HasValue && selectedRequest.ResultedAssemblyCount.Value != montajlananKenetSayisi)
                {
                    MessageBox.Show(
                        $"Uyarı: Montajlanan kenet sayısı ({montajlananKenetSayisi}) ile oluşan montaj sayısı ({selectedRequest.ResultedAssemblyCount.Value}) eşleşmiyor!\n\n" +
                        $"Montajlanan kenet sayısı = Oluşan montaj sayısı olmalıdır (1:1 oran).\n" +
                        $"Oluşan montaj sayısı {montajlananKenetSayisi} olarak ayarlanacak.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                // ResultedAssemblyCount'u montajlanan kenet sayısına eşitle
                selectedRequest.ResultedAssemblyCount = montajlananKenetSayisi;
                _assemblyRequestRepository.Update(selectedRequest);

                // Onaylama işlemi
                var result = MessageBox.Show(
                    $"Montaj talebi onaylanacak:\n\n" +
                    $"İstenen: {selectedRequest.RequestedAssemblyCount} adet\n" +
                    $"Montajlanan Kenet (kenetlenmiş stoktan): {montajlananKenetSayisi} adet\n" +
                    $"Oluşan Montaj (montajlanmış stoğa): {olusanMontajSayisi} adet\n\n" +
                    $"Onaylamak istediğinize emin misiniz?",
                    "Montaj Talebi Onayla",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Durumu "Tamamlandı" yap
                selectedRequest.Status = "Tamamlandı";
                selectedRequest.CompletionDate = DateTime.Now;
                _assemblyRequestRepository.Update(selectedRequest);

                // Montaj kaydı oluştur (Assembly) - montajlanan kenet sayısı = oluşan montaj sayısı
                var assembly = new Assembly
                {
                    OrderId = selectedRequest.OrderId,
                    ClampingId = selectedRequest.ClampingId,
                    PlateThickness = selectedRequest.PlateThickness,
                    Hatve = selectedRequest.Hatve,
                    Size = selectedRequest.Size,
                    Length = selectedRequest.Length,
                    SerialNoId = selectedRequest.SerialNoId,
                    MachineId = selectedRequest.MachineId,
                    AssemblyCount = olusanMontajSayisi, // Oluşan montaj adedi = Montajlanan kenet adedi
                    UsedClampCount = montajlananKenetSayisi, // Montajlanan kenet adedi
                    EmployeeId = selectedRequest.EmployeeId,
                    AssemblyDate = DateTime.Now
                };
                var assemblyId = _assemblyRepository.Insert(assembly);
                
                // Event feed kaydı ekle ve stok tüketimleri için order'ı al
                Order orderForStock = null;
                if (selectedRequest.OrderId.HasValue)
                {
                    var orderForEvent = _orderRepository.GetById(selectedRequest.OrderId.Value);
                    if (orderForEvent != null)
                    {
                        EventFeedService.AssemblyApproved(selectedRequest.Id, selectedRequest.OrderId.Value, orderForEvent.TrexOrderNo);
                        orderForStock = orderForEvent;
                    }
                }
                
                // Stok tüketimleri
                int yapilanAdet = olusanMontajSayisi;
                
                if (orderForStock != null)
                {
                    // 1. Kapak stokundan tüketim (her adet için 2 tane)
                    ConsumeCoverStock(orderForStock, yapilanAdet);
                    
                    // 2. Yan profil stokundan tüketim (her adet için 4 tane)
                    if (selectedRequest.ClampingId.HasValue)
                    {
                        var clamping = _clampingRepository.GetById(selectedRequest.ClampingId.Value);
                        if (clamping != null)
                        {
                            ConsumeSideProfileStock(orderForStock, clamping, yapilanAdet);
                        }
                    }
                }

                MessageBox.Show("Montaj talebi onaylandı ve montaj kaydı oluşturuldu!\nStok tüketimleri yapıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Verileri yeniden yükle
                LoadAssemblyData(dataGridView);
                
                // İzolasyon tab'ını otomatik yenile (montaj onaylandıktan sonra izolasyon tab'ına ürün düşer)
                if (_isolationDataGridView != null)
                {
                    LoadIsolationData(_isolationDataGridView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateIsolationTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // İzole Et butonu
            var btnIzoleEt = ButtonFactory.CreateActionButton("🛡️ İzole Et", ThemeColors.Success, Color.White, 120, 35);
            btnIzoleEt.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnIzoleEt.Location = new Point(buttonPanel.Width - 120, 5);
            buttonPanel.Controls.Add(btnIzoleEt);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };
            
            // DataGridView referansını sakla (otomatik refresh için)
            _isolationDataGridView = dataGridView;

            // Kolonları ekle
            AddIsolationColumn(dataGridView, "Date", "Tarih", 100);
            AddIsolationColumn(dataGridView, "OrderNo", "Sipariş No", 90);
            AddIsolationColumn(dataGridView, "Hatve", "Hatve", 60);
            AddIsolationColumn(dataGridView, "Size", "Ölçü", 70);
            AddIsolationColumn(dataGridView, "Length", "Uzunluk (m)", 90);
            AddIsolationColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 100);
            AddIsolationColumn(dataGridView, "Customer", "Müşteri", 130);
            AddIsolationColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
            AddIsolationColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddIsolationColumn(dataGridView, "EmployeeName", "Operatör", 120);
            AddIsolationColumn(dataGridView, "IsolationLiquidAmount", "İzolasyon Sıvısı (kg)", 150);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnIzoleEt.Click += (s, e) => BtnIzoleEt_Click(dataGridView);

            // Verileri yükle
            LoadIsolationData(dataGridView);
        }

        private void AddIsolationColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadIsolationData(DataGridView dataGridView)
        {
            try
            {
                var orderForIsolation = _orderRepository.GetById(_orderId);
                
                // Onaylanmış izolasyon kayıtları
                var isolations = _isolationRepository.GetByOrderId(_orderId);
                var completedData = isolations.Select(i => new
                {
                    Id = i.Id,
                    Date = i.IsolationDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForIsolation?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(i.Hatve),
                    Size = i.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = (i.Length / 1000m).ToString("F2", CultureInfo.InvariantCulture), // MM'den metre'ye çevir
                    AssemblyCount = i.IsolationCount.ToString(),
                    Customer = orderForIsolation?.Company?.Name ?? "",
                    PlateThickness = i.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = i.SerialNo?.SerialNumber ?? "",
                    EmployeeName = i.Employee != null ? $"{i.Employee.FirstName} {i.Employee.LastName}" : "",
                    IsolationLiquidAmount = i.IsolationLiquidAmount.ToString("F2", CultureInfo.InvariantCulture)
                }).ToList();

                // Tamamlanmış montaj kayıtları (henüz izole edilmemiş olanlar)
                var assemblies = _assemblyRepository.GetByOrderId(_orderId);
                var isolatedAssemblyIds = isolations.Where(i => i.AssemblyId.HasValue).Select(i => i.AssemblyId.Value).ToList();
                var unisolatedAssemblies = assemblies.Where(a => !isolatedAssemblyIds.Contains(a.Id)).ToList();
                
                var pendingData = unisolatedAssemblies.Select(a => new
                {
                    Id = a.Id,
                    Date = a.AssemblyDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForIsolation?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(a.Hatve),
                    Size = a.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = (a.Length / 1000m).ToString("F2", CultureInfo.InvariantCulture), // MM'den metre'ye çevir
                    AssemblyCount = a.AssemblyCount.ToString(),
                    Customer = orderForIsolation?.Company?.Name ?? "",
                    PlateThickness = a.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = a.SerialNo?.SerialNumber ?? "",
                    EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "",
                    IsolationLiquidAmount = "-"
                }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(pendingData.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddIsolationColumn(dataGridView, "Date", "Tarih", 100);
                        AddIsolationColumn(dataGridView, "OrderNo", "Sipariş No", 90);
                        AddIsolationColumn(dataGridView, "Hatve", "Hatve", 60);
                        AddIsolationColumn(dataGridView, "Size", "Ölçü", 70);
                        AddIsolationColumn(dataGridView, "Length", "Uzunluk (m)", 90);
                        AddIsolationColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 100);
                        AddIsolationColumn(dataGridView, "Customer", "Müşteri", 130);
                        AddIsolationColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
                        AddIsolationColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                        AddIsolationColumn(dataGridView, "EmployeeName", "Operatör", 120);
                        AddIsolationColumn(dataGridView, "IsolationLiquidAmount", "İzolasyon Sıvısı (kg)", 150);
                    }

                    // Veri kaynağını ayarla
                    dataGridView.DataSource = data;
                }
                finally
                {
                    dataGridView.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzolasyon verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIzoleEt_Click(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen izole edilecek montaj kaydını seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedRow = dataGridView.SelectedRows[0];
                var dataItem = selectedRow.DataBoundItem;
                if (dataItem == null)
                {
                    MessageBox.Show("Geçersiz satır seçildi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Id'yi al
                Guid assemblyId = Guid.Empty;
                var idProperty = dataItem.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    assemblyId = (Guid)idProperty.GetValue(dataItem);
                }

                if (assemblyId == Guid.Empty)
                {
                    MessageBox.Show("Montaj kaydı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var assembly = _assemblyRepository.GetById(assemblyId);
                if (assembly == null)
                {
                    MessageBox.Show("Montaj kaydı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // İzolasyon yöntemi seçim dialog'unu aç
                using (var dialog = new IsolationDialog(assembly, _isolationStockRepository))
                {
                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return; // Kullanıcı iptal etti
                    }

                    string selectedMethod = dialog.SelectedMethod;
                    decimal isolationLiquidAmount = dialog.IsolationLiquidAmount;
                    int isolationCount = dialog.IsolationCount;
                    int izosiyanatRatio = dialog.IzosiyanatRatio;
                    int poliolRatio = dialog.PoliolRatio;

                    // İzolasyon kaydı oluştur
                    var isolation = new Isolation
                    {
                        OrderId = assembly.OrderId,
                        AssemblyId = assembly.Id,
                        PlateThickness = assembly.PlateThickness,
                        Hatve = assembly.Hatve,
                        Size = assembly.Size,
                        Length = assembly.Length, // MM cinsinden sakla
                        SerialNoId = assembly.SerialNoId,
                        MachineId = assembly.MachineId,
                        IsolationCount = isolationCount, // İzolasyon adedi (montaj adedi ile aynı)
                        UsedAssemblyCount = assembly.AssemblyCount, // Kullanılan montaj adedi
                        IsolationLiquidAmount = isolationLiquidAmount, // İzolasyon sıvısı miktarı (kg veya ml)
                        IsolationMethod = selectedMethod, // "MS Silikon" veya "İzosiyanat+Poliol"
                        EmployeeId = assembly.EmployeeId,
                        IsolationDate = DateTime.Now
                    };
                    var isolationId = _isolationRepository.Insert(isolation);
                    
                    // Event feed kaydı ekle
                    if (assembly.OrderId.HasValue)
                    {
                        var orderForEvent = _orderRepository.GetById(assembly.OrderId.Value);
                        if (orderForEvent != null)
                        {
                            EventFeedService.IsolationCompleted(isolationId, assembly.OrderId.Value, orderForEvent.TrexOrderNo, isolationCount);
                        }
                    }
                    
                    // İzolasyon sıvısı stoğundan tüketim
                    ConsumeIsolationStock(selectedMethod, isolationLiquidAmount, izosiyanatRatio, poliolRatio);
                    
                    string amountUnit = "kg"; // Hem MS Silikon hem de İzosiyanat+Poliol için kg cinsinden
                    MessageBox.Show($"İzolasyon kaydı oluşturuldu!\nKullanılan İzolasyon Sıvısı: {isolationLiquidAmount:F3} {amountUnit}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Verileri yeniden yükle
                    LoadIsolationData(dataGridView);
                    
                    // Paketleme tab'ını otomatik yenile (izolasyon yapıldıktan sonra paketleme tab'ına ürün düşer)
                    if (_packagingDataGridView != null)
                    {
                        LoadPackagingData(_packagingDataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzolasyon yapılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreatePackagingTab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Onayla Paketle", ThemeColors.Success, Color.White, 150, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 150, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Paketlemeye Gönder butonu
            var btnPaketlemeyeGonder = ButtonFactory.CreateActionButton("📦 Paketlemeye Gönder", ThemeColors.Primary, Color.White, 180, 35);
            btnPaketlemeyeGonder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPaketlemeyeGonder.Location = new Point(buttonPanel.Width - 150 - 190, 5);
            buttonPanel.Controls.Add(btnPaketlemeyeGonder);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };
            
            // DataGridView referansını sakla (otomatik refresh için)
            _packagingDataGridView = dataGridView;

            // Kolonları ekle
            AddPackagingColumn(dataGridView, "Date", "Tarih", 100);
            AddPackagingColumn(dataGridView, "OrderNo", "Sipariş No", 90);
            AddPackagingColumn(dataGridView, "Hatve", "Hatve", 60);
            AddPackagingColumn(dataGridView, "Size", "Ölçü", 70);
            AddPackagingColumn(dataGridView, "Length", "Uzunluk", 80);
            AddPackagingColumn(dataGridView, "ProductType", "Ürün Türü", 100);
            AddPackagingColumn(dataGridView, "Profil", "Profil", 80);
            AddPackagingColumn(dataGridView, "KapakTipi", "Kapak Tipi", 120);
            AddPackagingColumn(dataGridView, "PackagingCount", "Paketleme Adedi", 120);
            AddPackagingColumn(dataGridView, "Customer", "Müşteri", 130);
            AddPackagingColumn(dataGridView, "UsedAssemblyCount", "Kullanılan Montaj Adedi", 160);
            AddPackagingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
            AddPackagingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
            AddPackagingColumn(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnOnayla.Click += (s, e) => BtnPackagingOnayla_Click(dataGridView);
            btnPaketlemeyeGonder.Click += (s, e) => BtnPaketlemeyeGonder_Click(dataGridView);

            // Verileri yükle
            LoadPackagingData(dataGridView);
        }

        private void AddPackagingColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadPackagingData(DataGridView dataGridView)
        {
            try
            {
                var orderForPackaging = _orderRepository.GetById(_orderId);
                int kapakBoyuMM = GetKapakBoyuFromOrder(orderForPackaging);
                
                // Ürün türü ve profil bilgilerini al
                string productType = orderForPackaging?.ProductType ?? "";
                string profil = "";
                string kapakTipi = "";
                if (orderForPackaging != null && !string.IsNullOrEmpty(orderForPackaging.ProductCode))
                {
                    var parts = orderForPackaging.ProductCode.Split('-');
                    if (parts.Length >= 3 && parts[2].Length >= 2)
                    {
                        profil = parts[2].Substring(1, 1).ToUpper(); // Profil harfi (örn: LG -> G)
                    }
                    // Kapak tipini al (5. index: 002, 030, vb.)
                    if (parts.Length > 5)
                    {
                        string kapakKodu = parts[5];
                        if (kapakKodu == "002")
                            kapakTipi = "2mm-düz kapak";
                        else if (kapakKodu == "030")
                            kapakTipi = "30mm-normal kapak";
                        else
                            kapakTipi = kapakKodu;
                    }
                }

                // Onaylanmış paketleme kayıtları
                var packagings = _packagingRepository.GetByOrderId(_orderId);
                var completedData = packagings.Select(p => new
                {
                    Id = p.Id,
                    Date = p.PackagingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForPackaging?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(p.Hatve),
                    Size = p.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = (p.Length + kapakBoyuMM).ToString("F2", CultureInfo.InvariantCulture), // Uzunluk (MM) + kapak boyu (MM)
                    ProductType = productType,
                    Profil = profil,
                    KapakTipi = kapakTipi,
                    PackagingCount = p.PackagingCount.ToString(),
                    Customer = orderForPackaging?.Company?.Name ?? "",
                    UsedAssemblyCount = p.UsedAssemblyCount.ToString(),
                    PlateThickness = p.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = p.SerialNo?.SerialNumber ?? "",
                    EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : ""
                }).ToList();

                // Paketleme talepleri oluşturulmuş izolasyon ID'lerini al
                var packagingRequests = _packagingRequestRepository.GetByOrderId(_orderId);
                var requestedIsolationIds = packagingRequests.Where(pr => pr.IsolationId.HasValue).Select(pr => pr.IsolationId.Value).ToList();
                
                // Tamamlanmış izolasyon kayıtları (henüz paketlenmemiş ve paketleme talebi oluşturulmamış olanlar)
                var isolations = _isolationRepository.GetByOrderId(_orderId);
                var packagedIsolationIds = packagings.Where(p => p.IsolationId.HasValue).Select(p => p.IsolationId.Value).ToList();
                var unpackagedIsolations = isolations.Where(i => !packagedIsolationIds.Contains(i.Id) && !requestedIsolationIds.Contains(i.Id)).ToList();
                
                var pendingData = unpackagedIsolations.Select(i => new
                {
                    Id = i.Id,
                    Date = i.IsolationDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = orderForPackaging?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(i.Hatve),
                    Size = i.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = (i.Length + kapakBoyuMM).ToString("F2", CultureInfo.InvariantCulture), // Uzunluk (MM) + kapak boyu (MM)
                    ProductType = productType,
                    Profil = profil,
                    KapakTipi = kapakTipi,
                    PackagingCount = "-",
                    Customer = orderForPackaging?.Company?.Name ?? "",
                    UsedAssemblyCount = i.UsedAssemblyCount.ToString(),
                    PlateThickness = i.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = i.SerialNo?.SerialNumber ?? "",
                    EmployeeName = i.Employee != null ? $"{i.Employee.FirstName} {i.Employee.LastName}" : ""
                }).ToList();

                // Birleştir
                var data = completedData.Cast<object>().Concat(pendingData.Cast<object>()).ToList();

                // Layout işlemlerini durdur - performans için kritik
                dataGridView.SuspendLayout();
                
                try
                {
                    // DataSource'u null yap (kolonlar kaybolmasın diye)
                    dataGridView.DataSource = null;
                    
                    // Kolonların var olduğundan emin ol
                    if (dataGridView.Columns.Count == 0)
                    {
                        AddPackagingColumn(dataGridView, "Date", "Tarih", 100);
                        AddPackagingColumn(dataGridView, "OrderNo", "Sipariş No", 90);
                        AddPackagingColumn(dataGridView, "Hatve", "Hatve", 60);
                        AddPackagingColumn(dataGridView, "Size", "Ölçü", 70);
                        AddPackagingColumn(dataGridView, "Length", "Uzunluk", 80);
                        AddPackagingColumn(dataGridView, "ProductType", "Ürün Türü", 100);
                        AddPackagingColumn(dataGridView, "Profil", "Profil", 80);
                        AddPackagingColumn(dataGridView, "KapakTipi", "Kapak Tipi", 120);
                        AddPackagingColumn(dataGridView, "PackagingCount", "Paketleme Adedi", 120);
                        AddPackagingColumn(dataGridView, "Customer", "Müşteri", 130);
                        AddPackagingColumn(dataGridView, "UsedAssemblyCount", "Kullanılan Montaj Adedi", 160);
                        AddPackagingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 110);
                        AddPackagingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 100);
                        AddPackagingColumn(dataGridView, "EmployeeName", "Operatör", 120);
                    }

                    // Veri kaynağını ayarla
                    dataGridView.DataSource = data;
                }
                finally
                {
                    dataGridView.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paketleme verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPaketlemeyeGonder_Click(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen paketlemeye gönderilecek izolasyon kaydını seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedRow = dataGridView.SelectedRows[0];
                var dataItem = selectedRow.DataBoundItem;
                if (dataItem == null)
                {
                    MessageBox.Show("Geçersiz satır seçildi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Id'yi al
                Guid isolationId = Guid.Empty;
                var idProperty = dataItem.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    isolationId = (Guid)idProperty.GetValue(dataItem);
                }

                if (isolationId == Guid.Empty)
                {
                    MessageBox.Show("İzolasyon kaydı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Izolasyon kaydını al
                var isolation = _isolationRepository.GetById(isolationId);
                if (isolation == null)
                {
                    MessageBox.Show("İzolasyon kaydı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Bu izolasyon için zaten bir paketleme talebi var mı kontrol et
                var existingRequests = _packagingRequestRepository.GetByOrderId(_orderId);
                if (existingRequests.Any(r => r.IsolationId == isolationId && r.Status != "İptal"))
                {
                    MessageBox.Show("Bu izolasyon için zaten bir paketleme talebi mevcut.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Paketleme adedini sor
                using (var inputDialog = new Form
                {
                    Text = "Paketleme Adedi",
                    Width = 350,
                    Height = 200,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var lblPrompt = new Label
                    {
                        Text = $"Kullanılacak İzolasyon Adedi: {isolation.IsolationCount}\n\nPaketleme Adedi:",
                        Location = new Point(20, 20),
                        Width = 300,
                        Height = 60,
                        Font = new Font("Segoe UI", 10F)
                    };

                    var txtPackagingCount = new NumericUpDown
                    {
                        Location = new Point(20, 80),
                        Width = 290,
                        Minimum = 1,
                        Maximum = isolation.IsolationCount,
                        Value = isolation.IsolationCount,
                        Font = new Font("Segoe UI", 10F)
                    };

                    var btnOK = new Button
                    {
                        Text = "Tamam",
                        Location = new Point(150, 120),
                        Width = 80,
                        Height = 35,
                        DialogResult = DialogResult.OK,
                        BackColor = ThemeColors.Primary,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                    };
                    btnOK.FlatAppearance.BorderSize = 0;

                    var btnCancel = new Button
                    {
                        Text = "İptal",
                        Location = new Point(240, 120),
                        Width = 80,
                        Height = 35,
                        DialogResult = DialogResult.Cancel,
                        BackColor = Color.Gray,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 10F)
                    };
                    btnCancel.FlatAppearance.BorderSize = 0;

                    inputDialog.Controls.Add(lblPrompt);
                    inputDialog.Controls.Add(txtPackagingCount);
                    inputDialog.Controls.Add(btnOK);
                    inputDialog.Controls.Add(btnCancel);
                    inputDialog.AcceptButton = btnOK;
                    inputDialog.CancelButton = btnCancel;

                    if (inputDialog.ShowDialog() == DialogResult.OK)
                    {
                        int packagingCount = (int)txtPackagingCount.Value;
                        int usedIsolationCount = isolation.IsolationCount;

                        // Paketleme talebi oluştur
                        var packagingRequest = new PackagingRequest
                        {
                            OrderId = isolation.OrderId,
                            IsolationId = isolation.Id,
                            PlateThickness = isolation.PlateThickness,
                            Hatve = isolation.Hatve,
                            Size = isolation.Size,
                            Length = isolation.Length,
                            SerialNoId = isolation.SerialNoId,
                            MachineId = isolation.MachineId,
                            RequestedPackagingCount = packagingCount,
                            UsedIsolationCount = usedIsolationCount,
                            EmployeeId = isolation.EmployeeId, // İzolasyon işlemini yapan operatör
                            Status = "Beklemede",
                            RequestDate = DateTime.Now
                        };
                        var requestId = _packagingRequestRepository.Insert(packagingRequest);
                        
                        // Event feed kaydı ekle
                        if (isolation.OrderId.HasValue)
                        {
                            var orderForRequest = _orderRepository.GetById(isolation.OrderId.Value);
                            if (orderForRequest != null)
                            {
                                EventFeedService.PackagingRequestCreated(requestId, isolation.OrderId.Value, orderForRequest.TrexOrderNo);
                            }
                        }
                        
                        MessageBox.Show("Paketleme talebi oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Verileri yeniden yükle
                        LoadPackagingData(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paketleme talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPackagingOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                // Bu siparişe ait "Tamamlandı" statusündeki paketleme taleplerini getir
                var allRequests = _packagingRequestRepository.GetAll()
                    .Where(r => r.OrderId == _orderId && r.Status == "Tamamlandı" && r.IsActive).ToList();
                
                // Bu siparişe ait tüm Packaging kayıtlarını al
                var allPackagings = _packagingRepository.GetByOrderId(_orderId);
                
                // Henüz onaylanmamış talepleri filtrele
                var pendingRequests = new List<PackagingRequest>();
                foreach (var request in allRequests)
                {
                    // Bu talep için zaten bir Packaging kaydı var mı kontrol et
                    bool alreadyApproved = allPackagings.Any(p => p.IsolationId == request.IsolationId);
                    if (!alreadyApproved)
                    {
                        pendingRequests.Add(request);
                    }
                }

                if (pendingRequests.Count == 0)
                {
                    MessageBox.Show("Onaylanacak paketleme talebi bulunmuyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // İlk talebi seç (veya kullanıcıdan seçtirebilirsiniz)
                var selectedRequest = pendingRequests.First();

                // Onaylama işlemi
                var result = MessageBox.Show(
                    $"Paketleme talebi onaylanacak:\n\n" +
                    $"İstenen Paketleme Adedi: {selectedRequest.RequestedPackagingCount} adet\n" +
                    $"Yapılan Paketleme Adedi: {selectedRequest.ActualPackagingCount ?? selectedRequest.RequestedPackagingCount} adet\n" +
                    $"Kullanılan İzolasyon Adedi: {selectedRequest.UsedIsolationCount ?? 0} adet\n\n" +
                    $"Onaylamak istediğinize emin misiniz?",
                    "Paketleme Talebi Onayla",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Paketleme kaydı oluştur
                var packaging = new Packaging
                {
                    OrderId = selectedRequest.OrderId,
                    IsolationId = selectedRequest.IsolationId,
                    PlateThickness = selectedRequest.PlateThickness,
                    Hatve = selectedRequest.Hatve,
                    Size = selectedRequest.Size,
                    Length = selectedRequest.Length,
                    SerialNoId = selectedRequest.SerialNoId,
                    MachineId = selectedRequest.MachineId,
                    PackagingCount = selectedRequest.ActualPackagingCount ?? selectedRequest.RequestedPackagingCount,
                    UsedAssemblyCount = selectedRequest.UsedIsolationCount ?? 0,
                    EmployeeId = selectedRequest.EmployeeId,
                    PackagingDate = DateTime.Now
                };
                var packagingId = _packagingRepository.Insert(packaging);
                
                // Event feed kaydı ekle
                if (selectedRequest.OrderId.HasValue)
                {
                    var orderForPackaging = _orderRepository.GetById(selectedRequest.OrderId.Value);
                    if (orderForPackaging != null)
                    {
                        EventFeedService.PackagingApproved(selectedRequest.Id, selectedRequest.OrderId.Value, orderForPackaging.TrexOrderNo);
                    }
                }
                
                MessageBox.Show("Paketleme talebi onaylandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Verileri yeniden yükle
                LoadPackagingData(dataGridView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paketleme onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateClamping2Tab(TabPage tab)
        {
            // Ana panel - TableLayoutPanel kullan
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Buton paneli için sabit yükseklik
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid paneli için kalan alan

            // Buton paneli - Üstte
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 5, 20, 5),
                BackColor = Color.White
            };

            // Onayla butonu (Kenetleme 2 taleplerini onaylamak için)
            var btnOnayla = ButtonFactory.CreateActionButton("✅ Kenetleme 2 Onayla", ThemeColors.Success, Color.White, 160, 35);
            btnOnayla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnayla.Location = new Point(buttonPanel.Width - 160, 5);
            buttonPanel.Controls.Add(btnOnayla);

            // Kenetle butonu (Birleştirme)
            var btnKenetle = ButtonFactory.CreateActionButton("🔗 Kenetle", ThemeColors.Primary, Color.White, 90, 35);
            btnKenetle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnKenetle.Location = new Point(buttonPanel.Width - 160 - 100, 5);
            buttonPanel.Controls.Add(btnKenetle);

            // Bölme butonu
            var btnBolme = ButtonFactory.CreateActionButton("✂️ Bölme", ThemeColors.Info, Color.White, 90, 35);
            btnBolme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBolme.Location = new Point(buttonPanel.Width - 160 - 100 - 100, 5);
            buttonPanel.Controls.Add(btnBolme);

            // DataGridView paneli
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // DataGridView
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            AddClamping2Column(dataGridView, "Date", "Tarih", 100);
            AddClamping2Column(dataGridView, "OrderNo", "Sipariş No", 90);
            AddClamping2Column(dataGridView, "Hatve", "Hatve (mm)", 80);
            AddClamping2Column(dataGridView, "PlateThickness", "Lamel Kalınlığı (mm)", 130);
            AddClamping2Column(dataGridView, "ResultedSize", "Sonuç Ölçü (cm)", 100);
            AddClamping2Column(dataGridView, "ResultedLength", "Sonuç Uzunluk (mm)", 120);
            AddClamping2Column(dataGridView, "ClampingsList", "Kullanılacak Ürünler", 250);
            AddClamping2Column(dataGridView, "Count", "Adet", 70);
            AddClamping2Column(dataGridView, "EmployeeName", "Operatör", 120);

            // Stil ayarları
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dataGridView.DefaultCellStyle.BackColor = Color.White;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnKenetle.Click += (s, e) => BtnClamping2Kenetle_Click(dataGridView);
            btnBolme.Click += (s, e) => BtnClamping2Bolme_Click(dataGridView);
            btnOnayla.Click += (s, e) => BtnClamping2RequestOnayla_Click(dataGridView);

            // Verileri yükle
            LoadClamping2Data(dataGridView);
        }

        private void AddClamping2Column(DataGridView dgv, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            dgv.Columns.Add(column);
        }

        private void LoadClamping2Data(DataGridView dataGridView)
        {
            try
            {
                var clamping2Requests = _clamping2RequestRepository.GetByOrderId(_orderId);
                var orderForClamping2 = _orderRepository.GetById(_orderId);
                int kapakBoyuMM = GetKapakBoyuFromOrder(orderForClamping2);
                
                var data = clamping2Requests.Select(cr2 =>
                {
                    // Items listesi varsa onu kullan, yoksa FirstClampingId/SecondClampingId kullan (geriye dönük uyumluluk)
                    string clampingsList = "";
                    
                    if (cr2.Items != null && cr2.Items.Count > 0)
                    {
                        var clampingInfos = cr2.Items
                            .OrderBy(item => item.Sequence)
                            .Select(item =>
                            {
                                var clamping = _clampingRepository.GetById(item.ClampingId);
                                return clamping != null ? $"{clamping.Size:F2} x {clamping.Length:F2}" : "";
                            })
                            .Where(info => !string.IsNullOrEmpty(info))
                            .ToList();
                        
                        clampingsList = string.Join(" + ", clampingInfos);
                    }
                    else
                    {
                        // Geriye dönük uyumluluk için FirstClampingId/SecondClampingId kullan
                        var firstClamping = cr2.FirstClampingId.HasValue ? _clampingRepository.GetById(cr2.FirstClampingId.Value) : null;
                        var secondClamping = cr2.SecondClampingId.HasValue ? _clampingRepository.GetById(cr2.SecondClampingId.Value) : null;
                        
                        var clampingInfos = new List<string>();
                        if (firstClamping != null)
                            clampingInfos.Add($"{firstClamping.Size:F2} x {firstClamping.Length:F2}");
                        if (secondClamping != null)
                            clampingInfos.Add($"{secondClamping.Size:F2} x {secondClamping.Length:F2}");
                        
                        clampingsList = string.Join(" + ", clampingInfos);
                    }
                    
                    return new
                    {
                        cr2.Id,
                        Date = cr2.RequestDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        OrderNo = orderForClamping2?.TrexOrderNo ?? "",
                        Hatve = GetHatveLetter(cr2.Hatve),
                        PlateThickness = cr2.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        ResultedSize = cr2.ResultedSize.ToString("F2", CultureInfo.InvariantCulture),
                        ResultedLength = cr2.ResultedLength.ToString("F2", CultureInfo.InvariantCulture),
                        ClampingsList = clampingsList,
                        Count = cr2.ResultedCount?.ToString() ?? cr2.ActualCount?.ToString() ?? cr2.RequestedCount.ToString(),
                        EmployeeName = cr2.Employee != null ? $"{cr2.Employee.FirstName} {cr2.Employee.LastName}" : ""
                    };
                }).ToList();

                // DataSource'u null yap (kolonlar kaybolmasın diye)
                dataGridView.DataSource = null;
                
                // Kolonların var olduğundan emin ol
                if (dataGridView.Columns.Count == 0)
                {
                    AddClamping2Column(dataGridView, "Date", "Tarih", 100);
                    AddClamping2Column(dataGridView, "OrderNo", "Sipariş No", 90);
                    AddClamping2Column(dataGridView, "Hatve", "Hatve (mm)", 80);
                    AddClamping2Column(dataGridView, "PlateThickness", "Lamel Kalınlığı (mm)", 130);
                    AddClamping2Column(dataGridView, "ResultedSize", "Sonuç Ölçü (cm)", 100);
                    AddClamping2Column(dataGridView, "ResultedLength", "Sonuç Uzunluk (mm)", 120);
                    AddClamping2Column(dataGridView, "ClampingsList", "Kullanılacak Ürünler", 250);
                    AddClamping2Column(dataGridView, "Count", "Adet", 70);
                    AddClamping2Column(dataGridView, "EmployeeName", "Operatör", 120);
                }

                dataGridView.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme 2 verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClamping2Kenetle_Click(DataGridView dataGridView)
        {
            try
            {
                using (var dialog = new Clamping2Dialog(_employeeRepository, _machineRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadClamping2Data(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme 2 eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClamping2Bolme_Click(DataGridView dataGridView)
        {
            try
            {
                using (var dialog = new DivideDialog(_employeeRepository, _machineRepository, _orderId))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadClamping2Data(dataGridView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bölme işlemi eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClamping2RequestOnayla_Click(DataGridView dataGridView)
        {
            try
            {
                // Bu siparişe ait bekleyen kenetleme 2 taleplerini getir
                var pendingRequests = _clamping2RequestRepository.GetAll()
                    .Where(r => r.OrderId == _orderId && (r.Status == "Kenetmede" || r.Status == "Beklemede")).ToList();

                if (pendingRequests.Count == 0)
                {
                    MessageBox.Show("Bu sipariş için onaylanacak kenetleme 2 talebi bulunmamaktadır.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Eğer birden fazla talep varsa, kullanıcıdan seçmesini iste
                Clamping2Request selectedRequest = null;
                if (pendingRequests.Count == 1)
                {
                    // Veritabanından güncel halini çek
                    selectedRequest = _clamping2RequestRepository.GetById(pendingRequests.First().Id);
                }
                else
                {
                    // Çoklu seçim dialogu (basit bir form)
                    using (var selectDialog = new Form
                    {
                        Text = "Kenetleme 2 Talebi Seç",
                        Width = 500,
                        Height = 400,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    })
                    {
                        var listBox = new ListBox
                        {
                            Dock = DockStyle.Fill,
                            Font = new Font("Segoe UI", 10F)
                        };

                        foreach (var req in pendingRequests)
                        {
                            string clampingsList = "";
                            
                            // Items listesi varsa onu kullan, yoksa FirstClampingId/SecondClampingId kullan (geriye dönük uyumluluk)
                            if (req.Items != null && req.Items.Count > 0)
                            {
                                var clampingInfos = req.Items
                                    .OrderBy(item => item.Sequence)
                                    .Select(item =>
                                    {
                                        var clamping = _clampingRepository.GetById(item.ClampingId);
                                        return clamping != null ? $"{clamping.Size:F2} x {clamping.Length:F2}" : "";
                                    })
                                    .Where(info => !string.IsNullOrEmpty(info))
                                    .ToList();
                                
                                clampingsList = string.Join(" + ", clampingInfos);
                            }
                            else
                            {
                                // Geriye dönük uyumluluk için FirstClampingId/SecondClampingId kullan
                                var firstClampItem = req.FirstClampingId.HasValue ? _clampingRepository.GetById(req.FirstClampingId.Value) : null;
                                var secondClampItem = req.SecondClampingId.HasValue ? _clampingRepository.GetById(req.SecondClampingId.Value) : null;
                                
                                var clampingInfos = new List<string>();
                                if (firstClampItem != null)
                                    clampingInfos.Add($"{firstClampItem.Size:F2} x {firstClampItem.Length:F2}");
                                if (secondClampItem != null)
                                    clampingInfos.Add($"{secondClampItem.Size:F2} x {secondClampItem.Length:F2}");
                                
                                clampingsList = string.Join(" + ", clampingInfos);
                            }
                            
                            string hatveLetter = GetHatveLetter(req.Hatve);
                            listBox.Items.Add(new { Request = req, Display = $"Hatve: {hatveLetter} | Sonuç: {req.ResultedSize:F2} x {req.ResultedLength:F2} (Ürünler: {clampingsList})" });
                        }
                        listBox.DisplayMember = "Display";
                        listBox.ValueMember = "Request";

                        var btnSelect = new Button
                        {
                            Text = "Seç",
                            DialogResult = DialogResult.OK,
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        selectDialog.Controls.Add(listBox);
                        selectDialog.Controls.Add(btnSelect);
                        selectDialog.AcceptButton = btnSelect;

                        if (selectDialog.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null)
                        {
                            var selectedItem = listBox.SelectedItem.GetType().GetProperty("Request").GetValue(listBox.SelectedItem);
                            var tempRequest = selectedItem as Clamping2Request;
                            if (tempRequest != null)
                            {
                                // Veritabanından güncel halini çek
                                selectedRequest = _clamping2RequestRepository.GetById(tempRequest.Id);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (selectedRequest == null)
                    return;

                // ActualCount ve ResultedCount kontrolü
                if (!selectedRequest.ActualCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce 'Kaç Tane Kullanıldı' değerini girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!selectedRequest.ResultedCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce 'Kaç Tane Oluştu' değerini girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kenetleme 2 kaydı oluştur (Clamping tablosuna ekle)
                var firstClamping = selectedRequest.FirstClampingId.HasValue ? _clampingRepository.GetById(selectedRequest.FirstClampingId.Value) : null;

                if (firstClamping == null)
                {
                    MessageBox.Show("Seçilen kenetlenmiş ürün bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Bölme işlemi mi kontrol et (SecondClampingId null ise bölme)
                bool isDivideOperation = !selectedRequest.SecondClampingId.HasValue;
                Clamping secondClamping = null;
                Clamping firstClampingResult = null;
                Clamping newClamping = null;
                
                if (!isDivideOperation)
                {
                    // Birleştirme işlemi - ikinci ürün zorunlu
                    secondClamping = _clampingRepository.GetById(selectedRequest.SecondClampingId.Value);
                    if (secondClamping == null)
                    {
                        MessageBox.Show("Seçilen ikinci kenetlenmiş ürün bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (isDivideOperation)
                {
                    // Bölme işlemi: Hem ilk hem ikinci uzunluk stoğa eklenecek
                    var originalLength = firstClamping.Length;
                    var firstLength = selectedRequest.ResultedLength; // İlk uzunluk
                    var secondLength = originalLength - firstLength; // İkinci uzunluk

                    // İlk uzunluk stoğa ekle
                    firstClampingResult = new Clamping
                    {
                        OrderId = _orderId,
                        PlateThickness = selectedRequest.PlateThickness,
                        Hatve = selectedRequest.Hatve,
                        Size = selectedRequest.ResultedSize,
                        Length = firstLength,
                        ClampCount = selectedRequest.ResultedCount.Value,
                        UsedPlateCount = selectedRequest.ActualCount.Value, // Bir kenetlenmiş ürün kullanıldı
                        MachineId = selectedRequest.MachineId,
                        EmployeeId = selectedRequest.EmployeeId,
                        ClampingDate = DateTime.Now
                    };

                    _clampingRepository.Insert(firstClampingResult);

                    // İkinci uzunluk stoğa ekle
                    var secondClampingResult = new Clamping
                    {
                        OrderId = _orderId,
                        PlateThickness = selectedRequest.PlateThickness,
                        Hatve = selectedRequest.Hatve,
                        Size = selectedRequest.ResultedSize,
                        Length = secondLength,
                        ClampCount = selectedRequest.ResultedCount.Value,
                        UsedPlateCount = selectedRequest.ActualCount.Value, // Bir kenetlenmiş ürün kullanıldı
                        MachineId = selectedRequest.MachineId,
                        EmployeeId = selectedRequest.EmployeeId,
                        ClampingDate = DateTime.Now
                    };

                    _clampingRepository.Insert(secondClampingResult);

                    // Orijinal ürünün stoktan düşürülmesi: ActualCount kadar kullanıldı
                    // Bu mantık zaten Clamping2Request'lerden hesaplanıyor, burada ekstra bir işlem gerekmez
                }
                else
                {
                    // Birleştirme işlemi: Tek bir kenetlenmiş ürün oluşur
                    newClamping = new Clamping
                    {
                        OrderId = _orderId,
                        PlateThickness = selectedRequest.PlateThickness,
                        Hatve = selectedRequest.Hatve,
                        Size = selectedRequest.ResultedSize,
                        Length = selectedRequest.ResultedLength,
                        ClampCount = selectedRequest.ResultedCount.Value,
                        UsedPlateCount = selectedRequest.ActualCount.Value * (selectedRequest.Items != null && selectedRequest.Items.Count > 0 ? selectedRequest.Items.Count : 2), // Kullanılan kenetlenmiş ürün sayısı
                        MachineId = selectedRequest.MachineId,
                        EmployeeId = selectedRequest.EmployeeId,
                        ClampingDate = DateTime.Now
                    };

                    _clampingRepository.Insert(newClamping);

                    // İlk ve ikinci kenetlenmiş ürünlerden stok düşürme: ActualCount kadar kullanıldı
                    // Bu mantık zaten Clamping2Request'lerden hesaplanıyor, burada ekstra bir işlem gerekmez
                }

                // Talebi tamamlandı olarak işaretle
                selectedRequest.Status = "Tamamlandı";
                selectedRequest.CompletionDate = DateTime.Now;
                _clamping2RequestRepository.Update(selectedRequest);
                
                // Event feed kaydı ekle
                if (selectedRequest.OrderId.HasValue && selectedRequest.OrderId.Value != Guid.Empty)
                {
                    var orderForClamping2 = _orderRepository.GetById(selectedRequest.OrderId.Value);
                    if (orderForClamping2 != null)
                    {
                        // Clamping2 için oluşturulan clamping ID'sini al
                        Guid? clampingId = null;
                        if (isDivideOperation)
                        {
                            // Bölme işlemi: İlk clamping ID'sini kullan
                            clampingId = firstClampingResult?.Id;
                        }
                        else
                        {
                            // Birleştirme işlemi: Yeni oluşturulan clamping ID'sini kullan
                            clampingId = newClamping?.Id;
                        }
                        // Event feed kaydı ekle - Kenetleme 2 onaylandı
                        EventFeedService.Clamping2Approved(selectedRequest.Id, selectedRequest.OrderId.Value, orderForClamping2.TrexOrderNo);
                    }
                }
                
                MessageBox.Show("Kenetleme 2 talebi başarıyla onaylandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Verileri yeniden yükle
                LoadClamping2Data(dataGridView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme 2 talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Brush'ları temizle
                _whiteBrush?.Dispose();
                _primaryBrush?.Dispose();
                _tabFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


