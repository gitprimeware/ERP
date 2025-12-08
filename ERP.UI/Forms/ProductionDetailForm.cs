using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ProductionDetailForm : UserControl
    {
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
        
        private Button btnBack;
        private Button btnRapor;
        private Button btnMuhasebeyeGonder;

        private Guid _orderId = Guid.Empty;
        private OrderRepository _orderRepository;
        private CuttingRepository _cuttingRepository;
        private PressingRepository _pressingRepository;
        private ClampingRepository _clampingRepository;
        private AssemblyRepository _assemblyRepository;
        private MachineRepository _machineRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private Order _order;

        public event EventHandler BackRequested;
        public event EventHandler<Guid> ReportRequested;
        public event EventHandler<Guid> SendToAccountingRequested;

        public ProductionDetailForm(Guid orderId)
        {
            _orderId = orderId;
            _orderRepository = new OrderRepository();
            _cuttingRepository = new CuttingRepository();
            _pressingRepository = new PressingRepository();
            _clampingRepository = new ClampingRepository();
            _assemblyRepository = new AssemblyRepository();
            _machineRepository = new MachineRepository();
            _serialNoRepository = new SerialNoRepository();
            _employeeRepository = new EmployeeRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

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
            tabControl.DrawItem += (s, e) =>
            {
                var tabPage = tabControl.TabPages[e.Index];
                var tabRect = tabControl.GetTabRect(e.Index);
                
                // Arka planı tamamen beyaz yap
                e.Graphics.FillRectangle(new SolidBrush(Color.White), tabRect);
                tabControl.BackColor = Color.White;
                
                Color textColor;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    // Seçili tab için altında mavi çizgi
                    e.Graphics.FillRectangle(new SolidBrush(ThemeColors.Primary), new Rectangle(tabRect.X, tabRect.Y + tabRect.Height - 3, tabRect.Width, 3));
                    textColor = ThemeColors.Primary;
                }
                else
                {
                    // Seçili olmayan tab - tamamen beyaz arka plan
                    textColor = Color.FromArgb(150, 150, 150);
                }
                
                // Emoji'leri doğru şekilde render et - Segoe UI Emoji fontu kullan
                using (var emojiFont = new Font("Segoe UI Emoji", 10F))
                {
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, emojiFont, 
                        tabRect, textColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                
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

            // Geri butonu - sağ üste, tab'ların yanında
            btnBack = ButtonFactory.CreateActionButton("⬅️ Geri", ThemeColors.Secondary, Color.White, 90, 32);
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(mainPanel.Width - btnBack.Width - 20, 15);
            btnBack.Click += BtnBack_Click;
            btnBack.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // mainPanel resize olduğunda geri tuşunun konumunu güncelle
            mainPanel.Resize += (s, e) =>
            {
                if (btnBack != null)
                {
                    btnBack.Location = new Point(mainPanel.Width - btnBack.Width - 20, 15);
                }
            };

            mainPanel.Controls.Add(tabControl);
            mainPanel.Controls.Add(btnBack);
            btnBack.BringToFront();

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

            // Üretimdeyse sadece Muhasebeye Gönder butonu göster
            bool isInProduction = _order?.Status == "Üretimde";
            // Stok siparişleri için muhasebeye gönder butonunu gizle
            bool isStockOrder = _order?.IsStockOrder ?? false;

            if (!isInProduction)
            {
                btnRapor = ButtonFactory.CreateActionButton("📄 Rapor", ThemeColors.Info, Color.White, 150, 40);
                btnRapor.Location = new Point(0, 5);
                btnRapor.Click += BtnRapor_Click;
                panel.Controls.Add(btnRapor);
            }

            // Sadece üretimdeyse ve stok siparişi değilse muhasebeye gönder butonu göster
            if (isInProduction && !isStockOrder)
            {
                btnMuhasebeyeGonder = ButtonFactory.CreateActionButton("💰 Muhasebeye Gönder", ThemeColors.Success, Color.White, 180, 40);
                btnMuhasebeyeGonder.Location = new Point(btnRapor != null ? 160 : 0, 5);
                btnMuhasebeyeGonder.Click += BtnMuhasebeyeGonder_Click;
                panel.Controls.Add(btnMuhasebeyeGonder);
            }

            return panel;
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
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

            // Htave - Model satırından (formül sayfasındaki txtHtave'den) ve parantez içinde sözel olarak
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
                
                // Sözel karşılığını belirle
                string sozelKarsilik = "";
                switch (char.ToUpper(modelLetter))
                {
                    case 'H': sozelKarsilik = "Yüksek"; break;
                    case 'D': sozelKarsilik = "Düşük"; break;
                    case 'M': sozelKarsilik = "Orta"; break;
                    case 'L': sozelKarsilik = "Büyük"; break;
                    default: sozelKarsilik = ""; break;
                }
                
                // Parantez içinde sözel olarak göster
                if (!string.IsNullOrEmpty(sozelKarsilik))
                {
                    txtReportHtave.Text = $"{hatveText} ({sozelKarsilik})";
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

            // Yükseklik (mm) - Yükseklik 1800 üzerindeyse 2'ye böl, sonra kapak çıkar
            // Kapak tipi 30 veya 2 olsa da 16 olarak kabul edilir ((30+2)/2 = 16)
            int raporYukseklikMM = 0;
            if (txtReportYukseklikCM != null && txtYukseklikMM != null && txtKapakBoyuMM != null)
            {
                if (int.TryParse(txtYukseklikMM.Text, out int yukseklikMM))
                {
                    // Yükseklik 1800 üzerindeyse 2'ye böl
                    int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    
                    // Kapak değerini belirle - 30 veya 2 olsa da 16 kabul edilir
                    int cikarilacakKapakDegeri = 16; // Varsayılan olarak 16
                    
                    if (int.TryParse(txtKapakBoyuMM.Text, out int kapakBoyuMM))
                {
                        // Kapak tipi 30 veya 2 ise 16 kullan, diğer durumlarda direkt değeri kullan
                        if (kapakBoyuMM == 30 || kapakBoyuMM == 2)
                        {
                            cikarilacakKapakDegeri = 16; // (30+2)/2 = 16
                        }
                        else
                        {
                            cikarilacakKapakDegeri = kapakBoyuMM;
                        }
                    }
                    else if (_order != null && !string.IsNullOrEmpty(_order.ProductCode))
                    {
                        // Ürün kodundan kapak değerini çıkar
                        var productCodeParts = _order.ProductCode.Split('-');
                        if (productCodeParts.Length > 5)
                        {
                            string kapakDegeri = productCodeParts[5];
                            
                            // Ürün kodunda DisplayText formatı kullanılıyor: 030, 002, 016
                            if (kapakDegeri == "030" || kapakDegeri == "002")
                            {
                                cikarilacakKapakDegeri = 16; // (30+2)/2 = 16
                            }
                            else if (kapakDegeri == "016")
                            {
                                cikarilacakKapakDegeri = 16;
                            }
                            else if (int.TryParse(kapakDegeri, out int parsedKapak))
                            {
                                if (parsedKapak == 30 || parsedKapak == 2)
                                {
                                    cikarilacakKapakDegeri = 16;
                                }
                                else
                                {
                                    cikarilacakKapakDegeri = parsedKapak;
                            }
                            }
                        }
                    }
                    
                    // Rapor yükseklik = (Yükseklik com) - Kapak değeri
                    raporYukseklikMM = yukseklikCom - cikarilacakKapakDegeri;
                            txtReportYukseklikCM.Text = raporYukseklikMM.ToString();
                }
            }

            // Toplam Sipariş Adedi
            if (txtReportToplamSiparisAdedi != null && txtToplamAdet != null)
                txtReportToplamSiparisAdedi.Text = txtToplamAdet.Text;

            // Plaka Adedi - Formül: yükseklik mm/100 * 10cm plaka adedi * toplam sipariş adedi
            if (txtReportPlakaAdedi != null && txtYukseklikMM != null && txtToplamAdet != null && txtPlakaAdedi10cm != null)
            {
                // Yükseklik (mm) - Rapor için kapaksız yükseklik kullanılır
                int yukseklikMM = raporYukseklikMM > 0 ? raporYukseklikMM : 0;
                
                // Eğer raporYukseklikMM hesaplanamadıysa, direkt yükseklikMM'den al
                if (yukseklikMM == 0 && int.TryParse(txtYukseklikMM.Text, out int yukseklikMMFromText))
                {
                    yukseklikMM = yukseklikMMFromText;
                }
                
                if (yukseklikMM > 0 &&
                    int.TryParse(txtToplamAdet.Text, out int toplamSiparisAdedi) &&
                    int.TryParse(txtPlakaAdedi10cm.Text, out int plakaAdedi10cm))
                {
                    // Çarpılmamış: yükseklik mm/100 * 10cm plaka adedi
                    decimal carpilmamisPlakaAdedi = (decimal)yukseklikMM / 100m * plakaAdedi10cm;
                    decimal carpilmamisYuvarlanmis = Math.Round(carpilmamisPlakaAdedi, 0, MidpointRounding.AwayFromZero);
                    
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

            // Durum
            if (txtReportDurum != null)
                txtReportDurum.Text = _order.Status ?? "";

            // Buton panelini oluştur (üretimdeyse sadece muhasebeye gönder)
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
            var result = MessageBox.Show(
                $"Sipariş {_order?.TrexOrderNo} muhasebeye gönderilecek. Emin misiniz?",
                "Muhasebeye Gönder",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendToAccountingRequested?.Invoke(this, _orderId);
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

                // Model ve Profil: LG -> Model: L, Profil: G
                // veya HS -> Model: H, Profil: S
                string modelProfile = parts[2]; // LG veya HS
                if (modelProfile.Length >= 2)
                {
                    char modelLetter = modelProfile[0]; // L veya H
                    char profileLetter = modelProfile[1]; // G veya S

                    // Model
                    txtModel.Text = modelLetter.ToString().ToUpper();

                    // Profil Mode
                    txtProfilMode.Text = profileLetter.ToString().ToUpper();

                    // Htave: H=3.25, D=4.5, M=6.5, L=9
                    decimal htave = GetHtave(modelLetter);
                    txtHtave.Text = htave.ToString("F2", CultureInfo.InvariantCulture);

                    // 10cm Plaka Adedi: H=32, D=24, M=17, L=12
                    int plakaAdedi10cm = GetPlakaAdedi10cm(modelLetter);
                    txtPlakaAdedi10cm.Text = plakaAdedi10cm.ToString();

                    // Profil Mode Ağırlığı: G=0.5, S=0.3
                    decimal profilModeAgirligi = profileLetter == 'G' || profileLetter == 'g' ? 0.5m : 0.3m;
                    txtProfilModeAgirligi.Text = profilModeAgirligi.ToString("F1", CultureInfo.InvariantCulture);
                }

                // Plaka Ölçüsü (mm): 1422
                if (int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    txtPlakaOlcusuMM.Text = plakaOlcusuMM.ToString();

                    // Plaka Ölçüsü com (mm): 1422 <= 1150 ise 1422, > 1150 ise 1422/2
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    txtPlakaOlcusuComMM.Text = plakaOlcusuComMM.ToString();

                    // Plaka Ölçüsü (cm): Plaka ölçüsü com / 10
                    decimal plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                    txtPlakaOlcusuCM.Text = plakaOlcusuCM.ToString("F1", CultureInfo.InvariantCulture);

                    // Plaka Adet: Plaka ölçüsü <= 1150 ise 1, > 1150 ise 4
                    plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                    txtPlakaAdet.Text = plakaAdet.ToString();

                    // Plaka Ağırlığı ve Galvaniz Kapak Ağırlığı hesaplaması LoadOrderData sonunda yapılacak
                }

                // Yükseklik (mm): 1900
                if (int.TryParse(parts[4], out int yukseklikMM))
                {
                    txtYukseklikMM.Text = yukseklikMM.ToString();

                    // Yükseklik com: 1900 <= 1800 ise 1900, > 1800 ise 1900/2
                    int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    txtYukseklikCom.Text = yukseklikCom.ToString();

                    // Boy Adet: Yükseklik <= 1800 ise 1, > 1800 ise 2
                    boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                    txtBoyAdet.Text = boyAdet.ToString();
                }

                // Kapak Boyu (mm): 030 -> 30
                if (parts.Length > 5 && int.TryParse(parts[5], out int kapakBoyuMM))
                {
                    txtKapakBoyuMM.Text = kapakBoyuMM.ToString();
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

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerlerini harfe çevir: 3.25=H, 4.5=D, 6.5=M, 9=L
            const decimal tolerance = 0.1m;
            
            if (Math.Abs(hatveValue - 3.25m) < tolerance)
                return "H";
            else if (Math.Abs(hatveValue - 4.5m) < tolerance)
                return "D";
            else if (Math.Abs(hatveValue - 6.5m) < tolerance)
                return "M";
            else if (Math.Abs(hatveValue - 9m) < tolerance)
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
            cuttingTabControl.DrawItem += (s, e) =>
            {
                var tabPage = cuttingTabControl.TabPages[e.Index];
                var tabRect = cuttingTabControl.GetTabRect(e.Index);
                var textRect = new RectangleF(tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height);
                
                // Arka planı tamamen beyaz yap
                e.Graphics.FillRectangle(new SolidBrush(Color.White), tabRect);
                cuttingTabControl.BackColor = Color.White;
                
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    // Seçili tab için altında mavi çizgi
                    e.Graphics.FillRectangle(new SolidBrush(ThemeColors.Primary), new Rectangle(tabRect.X, tabRect.Y + tabRect.Height - 3, tabRect.Width, 3));
                    
                    // Emoji ve metni çiz - TextRenderer kullan
                    using (var font = new Font("Segoe UI Emoji", 10F))
                    {
                        TextRenderer.DrawText(e.Graphics, tabPage.Text, font, 
                            tabRect, ThemeColors.Primary, 
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
                else
                {
                    // Seçili olmayan tab - tamamen beyaz arka plan
                    using (var font = new Font("Segoe UI Emoji", 10F))
                    {
                        TextRenderer.DrawText(e.Graphics, tabPage.Text, font, 
                            tabRect, Color.FromArgb(150, 150, 150), 
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
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

            var tabMontaj = new TabPage("🔩 Montaj");
            tabMontaj.Padding = new Padding(20);
            tabMontaj.BackColor = Color.White;
            tabMontaj.UseVisualStyleBackColor = false;
            CreateAssemblyTab(tabMontaj);
            cuttingTabControl.TabPages.Add(tabMontaj);

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

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 120, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            AddKesimColumn(dataGridView, "Hatve", "Hatve", 80);
            AddKesimColumn(dataGridView, "Size", "Ölçü", 80);
            AddKesimColumn(dataGridView, "MachineName", "Makina No", 100);
            AddKesimColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
            AddKesimColumn(dataGridView, "TotalKg", "Toplam Kg", 100);
            AddKesimColumn(dataGridView, "CutKg", "Kesilen Kg", 100);
            AddKesimColumn(dataGridView, "CuttingCount", "Kesim Adedi", 100);
            AddKesimColumn(dataGridView, "PlakaAdedi", "Plaka Adedi", 100);
            AddKesimColumn(dataGridView, "WasteKg", "Hurda Kg", 100);
            AddKesimColumn(dataGridView, "RemainingKg", "Kalan Kg", 100);
            AddKesimColumn(dataGridView, "EmployeeName", "Operatör", 150);

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

            gridPanel.Controls.Add(dataGridView);
            
            // TableLayoutPanel'e ekle
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            
            tab.Controls.Add(mainPanel);

            // Event handler
            btnEkle.Click += (s, e) => BtnKesimEkle_Click(dataGridView);

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
                var cuttings = _cuttingRepository.GetByOrderId(_orderId);
                var data = cuttings.Select(c => new
                {
                    c.Id,
                    Hatve = GetHatveLetter(c.Hatve),
                    Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                    MachineName = c.Machine?.Name ?? "",
                    SerialNumber = c.SerialNo?.SerialNumber ?? "",
                    TotalKg = c.TotalKg.ToString("F3", CultureInfo.InvariantCulture),
                    CutKg = c.CutKg.ToString("F3", CultureInfo.InvariantCulture),
                    CuttingCount = c.CuttingCount.ToString(),
                    PlakaAdedi = c.PlakaAdedi.ToString(),
                    WasteKg = c.WasteKg.ToString("F3", CultureInfo.InvariantCulture),
                    RemainingKg = c.RemainingKg.ToString("F3", CultureInfo.InvariantCulture),
                    EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : ""
                }).ToList();

                // DataSource'u null yap (kolonlar kaybolmasın diye)
                dataGridView.DataSource = null;
                
                // Kolonların var olduğundan emin ol
                if (dataGridView.Columns.Count == 0)
                {
                    AddKesimColumn(dataGridView, "Hatve", "Hatve", 80);
                    AddKesimColumn(dataGridView, "Size", "Ölçü", 80);
                    AddKesimColumn(dataGridView, "MachineName", "Makina No", 100);
                    AddKesimColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
                    AddKesimColumn(dataGridView, "TotalKg", "Toplam Kg", 100);
                    AddKesimColumn(dataGridView, "CutKg", "Kesilen Kg", 100);
                    AddKesimColumn(dataGridView, "CuttingCount", "Kesim Adedi", 100);
                    AddKesimColumn(dataGridView, "PlakaAdedi", "Plaka Adedi", 100);
                    AddKesimColumn(dataGridView, "WasteKg", "Hurda Kg", 100);
                    AddKesimColumn(dataGridView, "RemainingKg", "Kalan Kg", 100);
                    AddKesimColumn(dataGridView, "EmployeeName", "Operatör", 150);
                }

                // Kolon başlıklarını kesinlikle göster
                dataGridView.ColumnHeadersVisible = true;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ColumnHeadersHeight = 40;
                
                // Veri kaynağını ayarla
                dataGridView.DataSource = data;
                
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
                        case "CuttingCount": column.HeaderText = "Kesim Adedi"; break;
                        case "PlakaAdedi": column.HeaderText = "Plaka Adedi"; break;
                        case "WasteKg": column.HeaderText = "Hurda Kg"; break;
                        case "RemainingKg": column.HeaderText = "Kalan Kg"; break;
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
                MessageBox.Show("Kesim verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 120, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            AddPresColumn(dataGridView, "Date", "Tarih", 120);
            AddPresColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
            AddPresColumn(dataGridView, "Hatve", "Hatve", 80);
            AddPresColumn(dataGridView, "Size", "Ölçü", 80);
            AddPresColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
            AddPresColumn(dataGridView, "PressNo", "Pres No", 100);
            AddPresColumn(dataGridView, "Pressure", "Basınç", 100);
            AddPresColumn(dataGridView, "PressCount", "Pres Adedi", 100);
            AddPresColumn(dataGridView, "WasteAmount", "Hurda Miktarı", 120);
            AddPresColumn(dataGridView, "EmployeeName", "Operatör", 150);

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
                var pressings = _pressingRepository.GetByOrderId(_orderId);
                var data = pressings.Select(p => new
                {
                    p.Id,
                    Date = p.PressingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    PlateThickness = p.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    Hatve = GetHatveLetter(p.Hatve),
                    Size = p.Size.ToString("F2", CultureInfo.InvariantCulture),
                    SerialNumber = p.SerialNo?.SerialNumber ?? "",
                    PressNo = p.PressNo ?? "",
                    Pressure = p.Pressure.ToString("F3", CultureInfo.InvariantCulture),
                    PressCount = p.PressCount.ToString(),
                    WasteAmount = p.WasteAmount.ToString("F3", CultureInfo.InvariantCulture),
                    EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : ""
                }).ToList();

                // DataSource'u null yap (kolonlar kaybolmasın diye)
                dataGridView.DataSource = null;
                
                // Kolonların var olduğundan emin ol
                if (dataGridView.Columns.Count == 0)
                {
                    AddPresColumn(dataGridView, "Date", "Tarih", 120);
                    AddPresColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
                    AddPresColumn(dataGridView, "Hatve", "Hatve", 80);
                    AddPresColumn(dataGridView, "Size", "Ölçü", 80);
                    AddPresColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
                    AddPresColumn(dataGridView, "PressNo", "Pres No", 100);
                    AddPresColumn(dataGridView, "Pressure", "Basınç", 100);
                    AddPresColumn(dataGridView, "PressCount", "Pres Adedi", 100);
                    AddPresColumn(dataGridView, "WasteAmount", "Hurda Miktarı", 120);
                    AddPresColumn(dataGridView, "EmployeeName", "Operatör", 150);
                }

                // Kolon başlıklarını kesinlikle göster
                dataGridView.ColumnHeadersVisible = true;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ColumnHeadersHeight = 40;
                
                // Veri kaynağını ayarla
                dataGridView.DataSource = data;
                
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

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 120, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            AddClampingColumn(dataGridView, "Date", "Tarih", 120);
            AddClampingColumn(dataGridView, "OrderNo", "Sipariş No", 100);
            AddClampingColumn(dataGridView, "Hatve", "Hatve", 80);
            AddClampingColumn(dataGridView, "Size", "Ölçü", 80);
            AddClampingColumn(dataGridView, "Length", "Uzunluk", 80);
            AddClampingColumn(dataGridView, "ClampCount", "Adet", 80);
            AddClampingColumn(dataGridView, "Customer", "Müşteri", 150);
            AddClampingColumn(dataGridView, "UsedPlateCount", "Kullanılan Plaka Adedi", 150);
            AddClampingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
            AddClampingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
            AddClampingColumn(dataGridView, "MachineName", "Makina Adı", 120);
            AddClampingColumn(dataGridView, "EmployeeName", "Operatör", 150);

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

            // Verileri yükle
            LoadClampingData(dataGridView);
        }

        private void AddClampingColumn(DataGridView dgv, string dataPropertyName, string headerText, int width)
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

        private void LoadClampingData(DataGridView dataGridView)
        {
            try
            {
                var clampings = _clampingRepository.GetByOrderId(_orderId);
                var order = _orderRepository.GetById(_orderId);
                
                var data = clampings.Select(c => new
                {
                    c.Id,
                    Date = c.ClampingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = order?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(c.Hatve),
                    Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = c.Length.ToString("F2", CultureInfo.InvariantCulture),
                    ClampCount = c.ClampCount.ToString(),
                    Customer = order?.Company?.Name ?? "",
                    UsedPlateCount = c.UsedPlateCount.ToString(),
                    PlateThickness = c.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = c.SerialNo?.SerialNumber ?? "",
                    MachineName = c.Machine?.Name ?? "",
                    EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : ""
                }).ToList();

                // DataSource'u null yap (kolonlar kaybolmasın diye)
                dataGridView.DataSource = null;
                
                // Kolonların var olduğundan emin ol
                if (dataGridView.Columns.Count == 0)
                {
                    AddClampingColumn(dataGridView, "Date", "Tarih", 120);
                    AddClampingColumn(dataGridView, "OrderNo", "Sipariş No", 100);
                    AddClampingColumn(dataGridView, "Hatve", "Hatve", 80);
                    AddClampingColumn(dataGridView, "Size", "Ölçü", 80);
                    AddClampingColumn(dataGridView, "Length", "Uzunluk", 80);
                    AddClampingColumn(dataGridView, "ClampCount", "Adet", 80);
                    AddClampingColumn(dataGridView, "Customer", "Müşteri", 150);
                    AddClampingColumn(dataGridView, "UsedPlateCount", "Kullanılan Plaka Adedi", 150);
                    AddClampingColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
                    AddClampingColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
                    AddClampingColumn(dataGridView, "MachineName", "Makina Adı", 120);
                    AddClampingColumn(dataGridView, "EmployeeName", "Operatör", 150);
                }

                // Kolon başlıklarını kesinlikle göster
                dataGridView.ColumnHeadersVisible = true;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ColumnHeadersHeight = 40;
                
                // Veri kaynağını ayarla
                dataGridView.DataSource = data;
                
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
                        case "Hatve": column.HeaderText = "Hatve"; break;
                        case "Size": column.HeaderText = "Ölçü"; break;
                        case "Length": column.HeaderText = "Uzunluk"; break;
                        case "ClampCount": column.HeaderText = "Adet"; break;
                        case "Customer": column.HeaderText = "Müşteri"; break;
                        case "UsedPlateCount": column.HeaderText = "Kullanılan Plaka Adedi"; break;
                        case "PlateThickness": column.HeaderText = "Plaka Kalınlığı"; break;
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

            // Ekle butonu
            var btnEkle = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Primary, Color.White, 120, 35);
            btnEkle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            AddAssemblyColumn(dataGridView, "Date", "Tarih", 120);
            AddAssemblyColumn(dataGridView, "OrderNo", "Sipariş No", 100);
            AddAssemblyColumn(dataGridView, "Hatve", "Hatve", 80);
            AddAssemblyColumn(dataGridView, "Size", "Ölçü", 80);
            AddAssemblyColumn(dataGridView, "Length", "Uzunluk", 80);
            AddAssemblyColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 100);
            AddAssemblyColumn(dataGridView, "Customer", "Müşteri", 150);
            AddAssemblyColumn(dataGridView, "UsedClampCount", "Kullanılan Kenet Adedi", 150);
            AddAssemblyColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
            AddAssemblyColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
            AddAssemblyColumn(dataGridView, "EmployeeName", "Operatör", 150);

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
                var assemblies = _assemblyRepository.GetByOrderId(_orderId);
                var order = _orderRepository.GetById(_orderId);
                
                var data = assemblies.Select(a => new
                {
                    a.Id,
                    Date = a.AssemblyDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    OrderNo = order?.TrexOrderNo ?? "",
                    Hatve = GetHatveLetter(a.Hatve),
                    Size = a.Size.ToString("F2", CultureInfo.InvariantCulture),
                    Length = a.Length.ToString("F2", CultureInfo.InvariantCulture),
                    AssemblyCount = a.AssemblyCount.ToString(),
                    Customer = order?.Company?.Name ?? "",
                    UsedClampCount = a.UsedClampCount.ToString(),
                    PlateThickness = a.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                    SerialNumber = a.SerialNo?.SerialNumber ?? "",
                    EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : ""
                }).ToList();

                // DataSource'u null yap (kolonlar kaybolmasın diye)
                dataGridView.DataSource = null;
                
                // Kolonların var olduğundan emin ol
                if (dataGridView.Columns.Count == 0)
                {
                    AddAssemblyColumn(dataGridView, "Date", "Tarih", 120);
                    AddAssemblyColumn(dataGridView, "OrderNo", "Sipariş No", 100);
                    AddAssemblyColumn(dataGridView, "Hatve", "Hatve", 80);
                    AddAssemblyColumn(dataGridView, "Size", "Ölçü", 80);
                    AddAssemblyColumn(dataGridView, "Length", "Uzunluk", 80);
                    AddAssemblyColumn(dataGridView, "AssemblyCount", "Montaj Adedi", 100);
                    AddAssemblyColumn(dataGridView, "Customer", "Müşteri", 150);
                    AddAssemblyColumn(dataGridView, "UsedClampCount", "Kullanılan Kenet Adedi", 150);
                    AddAssemblyColumn(dataGridView, "PlateThickness", "Plaka Kalınlığı", 120);
                    AddAssemblyColumn(dataGridView, "SerialNumber", "Rulo Seri No", 120);
                    AddAssemblyColumn(dataGridView, "EmployeeName", "Operatör", 150);
                }

                dataGridView.DataSource = data;
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
                using (var dialog = new AssemblyDialog(_employeeRepository, _orderId))
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

    }
}

