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
    public partial class ConsumptionDetailForm : UserControl
    {
        private Panel mainPanel;
        private TableLayoutPanel tableLayout;
        private Label lblTitle;
        
        // Sarfiyat bilgileri (Readonly)
        private TextBox txtFirma;
        private TextBox txtTrexSiparisNo;
        private TextBox txtUrunKodu;
        private TextBox txtAdet;
        private TextBox txtHatve;
        private TextBox txtUretimPlakaOlcusu;
        private TextBox txtPlakaAdedi;
        private TextBox txtPlakaKalinligi;
        private TextBox txtPlakaAgirligi;
        private TextBox txtToplamAluminyumAgirligi;
        private TextBox txtProfil;
        private TextBox txtProfilAgirligi;
        private TextBox txtGalvanizKapakAgirligi;
        private TextBox txtKapakAdedi;
        private TextBox txtToplamGalvanizAgirligi;
        private TextBox txtDurum;
        
        private Button btnBack;

        private Guid _orderId = Guid.Empty;
        private OrderRepository _orderRepository;
        private Order _order;

        // Hesaplama için gerekli değerler
        private decimal _plakaAgirligi = 0;
        private int _plakaAdet = 0;
        private decimal _profilModeAgirligi = 0;
        private int _toplamAdet = 0;
        private int _yukseklikMM = 0;
        private decimal _bypassOlcusu = 0;
        private decimal _galvanizKapakAgirligi = 0;
        private int _kapakAdedi = 0;
        private char _modelLetter = ' '; // Hatve harf simgesi için

        public event EventHandler BackRequested;

        public ConsumptionDetailForm(Guid orderId)
        {
            _orderId = orderId;
            _orderRepository = new OrderRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
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
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30),
                AutoScroll = true
            };

            UIHelper.ApplyCardStyle(mainPanel, 12);

            // Başlık
            lblTitle = new Label
            {
                Text = "Sarfiyat Detayları",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // TableLayoutPanel oluştur
            CreateTableLayout();

            tableLayout.Location = new Point(30, 80);
            tableLayout.Width = mainPanel.Width - 60;
            tableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Geri butonu - sağ üste
            btnBack = ButtonFactory.CreateActionButton("⬅️ Geri", ThemeColors.Secondary, Color.White, 120, 40);
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(mainPanel.Width - btnBack.Width - 30, 10);
            btnBack.Click += BtnBack_Click;

            // mainPanel resize olduğunda geri tuşunun ve tableLayout'un konumunu güncelle
            mainPanel.Resize += (s, e) =>
            {
                if (btnBack != null)
                {
                    btnBack.Location = new Point(mainPanel.Width - btnBack.Width - 30, 10);
                }
                if (tableLayout != null)
                {
                    tableLayout.Width = mainPanel.Width - 60;
                }
            };

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(tableLayout);
            mainPanel.Controls.Add(btnBack);
            btnBack.BringToFront();

            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();
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
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            int row = 0;

            // Firma
            AddTableRow("Firma:", CreateReadOnlyTextBox(txtFirma = new TextBox()),
                       "Trex Sipariş No:", CreateReadOnlyTextBox(txtTrexSiparisNo = new TextBox()), row++);

            // Ürün Kodu
            AddTableRow("Ürün Kodu:", CreateReadOnlyTextBox(txtUrunKodu = new TextBox()),
                       "Adet:", CreateReadOnlyTextBox(txtAdet = new TextBox()), row++);

            // Hatve
            AddTableRow("Hatve:", CreateReadOnlyTextBox(txtHatve = new TextBox()),
                       "Üretim Plaka Ölçüsü:", CreateReadOnlyTextBox(txtUretimPlakaOlcusu = new TextBox()), row++);

            // Plaka Adedi
            AddTableRow("Plaka Adedi:", CreateReadOnlyTextBox(txtPlakaAdedi = new TextBox()),
                       "Plaka Kalınlığı:", CreateReadOnlyTextBox(txtPlakaKalinligi = new TextBox()), row++);

            // Plaka Ağırlığı
            AddTableRow("Plaka Ağırlığı:", CreateReadOnlyTextBox(txtPlakaAgirligi = new TextBox()),
                       "Toplam Alüminyum Ağırlığı:", CreateReadOnlyTextBox(txtToplamAluminyumAgirligi = new TextBox()), row++);

            // Profil
            AddTableRow("Profil:", CreateReadOnlyTextBox(txtProfil = new TextBox()),
                       "Profil Ağırlığı:", CreateReadOnlyTextBox(txtProfilAgirligi = new TextBox()), row++);

            // Galvaniz Kapak Ağırlığı
            AddTableRow("Galvaniz Kapak Ağırlığı:", CreateReadOnlyTextBox(txtGalvanizKapakAgirligi = new TextBox()),
                       "Kapak Adedi:", CreateReadOnlyTextBox(txtKapakAdedi = new TextBox()), row++);

            // Toplam Galvaniz Ağırlığı
            AddTableRow("Toplam Galvaniz Ağırlığı:", CreateReadOnlyTextBox(txtToplamGalvanizAgirligi = new TextBox()),
                       "Durum:", CreateReadOnlyTextBox(txtDurum = new TextBox()), row++);
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
            txt.BackColor = ThemeColors.Background;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Segoe UI", 9F);
            txt.Padding = new Padding(3);
            return txt;
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

                // Üst bilgi alanlarını doldur
                txtFirma.Text = _order.Company?.Name ?? "Bilinmiyor";
                txtTrexSiparisNo.Text = _order.TrexOrderNo ?? "";
                txtUrunKodu.Text = _order.ProductCode ?? "";
                txtAdet.Text = _order.Quantity.ToString();

                // Durum
                txtDurum.Text = _order.Status ?? "";

                // Ürün kodundan bilgileri çıkar
                if (!string.IsNullOrEmpty(_order.ProductCode))
                {
                    ParseProductCode(_order.ProductCode);
                }

                // Plaka ağırlığını hesapla
                if (_order.LamelThickness.HasValue)
                {
                    decimal aluminyumKalinligi = _order.LamelThickness.Value;
                    txtPlakaKalinligi.Text = aluminyumKalinligi.ToString("0.000", CultureInfo.InvariantCulture);
                    
                    if (decimal.TryParse(txtUretimPlakaOlcusu.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM))
                    {
                        _plakaAgirligi = CalculatePlakaAgirligi(plakaOlcusuCM, aluminyumKalinligi);
                        if (_plakaAgirligi > 0)
                        {
                            txtPlakaAgirligi.Text = _plakaAgirligi.ToString("F3", CultureInfo.InvariantCulture);
                        }
                    }
                }

                // Galvaniz Kapak Ağırlığı hesapla
                if (decimal.TryParse(txtUretimPlakaOlcusu.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plakaOlcusuCM2))
                {
                    _galvanizKapakAgirligi = CalculateGalvanizKapakAgirligi(plakaOlcusuCM2);
                    if (_galvanizKapakAgirligi > 0)
                    {
                        txtGalvanizKapakAgirligi.Text = _galvanizKapakAgirligi.ToString("F4", CultureInfo.InvariantCulture);
                    }
                }

                // Bypass ölçüsü
                if (!string.IsNullOrEmpty(_order.BypassSize))
                {
                    if (decimal.TryParse(_order.BypassSize.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal bypass))
                    {
                        _bypassOlcusu = bypass;
                    }
                }

                // Toplam adet
                if (int.TryParse(_order.Quantity.ToString(), out int siparisAdedi))
                {
                    _toplamAdet = siparisAdedi * _plakaAdet; // Boy adet ve plaka adet çarpımı
                    // ProductionDetailForm'dan toplam adet hesaplamasını kullan
                    // Toplam Adet = Sipariş adedi * Boy adet * Plaka adet
                }

                // Hesaplamaları yap
                CalculateConsumption();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseProductCode(string productCode)
        {
            try
            {
                // Format: TREX-CR-LG-1422-1900-030
                var parts = productCode.Split('-');
                if (parts.Length < 6)
                {
                    return;
                }

                // Model ve Profil: LG -> Model: L, Profil: G
                string modelProfile = parts[2]; // LG veya HS
                if (modelProfile.Length >= 2)
                {
                    _modelLetter = modelProfile[0]; // L veya H
                    char profileLetter = modelProfile[1]; // G veya S

                    // Hatve: H=3.25, D=4.5, M=6.5, L=9
                    decimal htave = GetHtave(_modelLetter);
                    // Hatve yanında harf simgesi de göster
                    txtHatve.Text = $"{htave.ToString("F2", CultureInfo.InvariantCulture)} ({_modelLetter.ToString().ToUpper()})";

                    // Profil
                    txtProfil.Text = profileLetter.ToString().ToUpper();

                    // Profil Mode Ağırlığı: G=0.5, S=0.3
                    _profilModeAgirligi = profileLetter == 'G' || profileLetter == 'g' ? 0.5m : 0.3m;
                }

                // Plaka Ölçüsü (mm): 1422
                if (int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    // Plaka Ölçüsü com (mm): 1422 <= 1150 ise 1422, > 1150 ise 1422/2
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;

                    // Plaka Ölçüsü (cm): Plaka ölçüsü com / 10
                    decimal plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                    txtUretimPlakaOlcusu.Text = plakaOlcusuCM.ToString("F1", CultureInfo.InvariantCulture);

                    // Plaka Adet: Plaka ölçüsü <= 1150 ise 1, > 1150 ise 4
                    _plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                    txtPlakaAdedi.Text = _plakaAdet.ToString();
                }

                // Yükseklik (mm): 1900
                if (int.TryParse(parts[4], out int yukseklikMM))
                {
                    _yukseklikMM = yukseklikMM;
                    
                    // Boy Adet: Yükseklik <= 1800 ise 1, > 1800 ise 2
                    int boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                    
                    // Toplam Adet: Sipariş adedi * Boy adet * Plaka adet
                    if (int.TryParse(_order.Quantity.ToString(), out int siparisAdedi))
                    {
                        _toplamAdet = siparisAdedi * boyAdet * _plakaAdet;
                    }
                }

                // Kapak Adedi
                if (parts.Length > 5)
                {
                    // Kapak boyu parse edilebilirse
                    if (int.TryParse(parts[5], out int kapakBoyu))
                    {
                        // Kapak adedi hesaplama mantığı buraya eklenecek
                        // Şimdilik sipariş adedi kullanıyoruz
                        _kapakAdedi = _order.Quantity;
                        txtKapakAdedi.Text = _kapakAdedi.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün kodu parse edilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateConsumption()
        {
            // Toplam Alüminyum Ağırlığı = Plaka Ağırlığı * Plaka Adedi
            if (_plakaAgirligi > 0 && _plakaAdet > 0)
            {
                decimal toplamAluminyumAgirligi = _plakaAgirligi * _plakaAdet;
                txtToplamAluminyumAgirligi.Text = toplamAluminyumAgirligi.ToString("F3", CultureInfo.InvariantCulture);
            }

            // Profil Ağırlığı = Profil Mode Ağırlığı * 4 * Toplam Adet * (Yükseklik (mm) + Bypass / 1000)
            if (_profilModeAgirligi > 0 && _toplamAdet > 0)
            {
                decimal profilAgirligi = _profilModeAgirligi * 4 * _toplamAdet * ((_yukseklikMM + _bypassOlcusu) / 1000m);
                txtProfilAgirligi.Text = profilAgirligi.ToString("F3", CultureInfo.InvariantCulture);
            }

            // Toplam Galvaniz Ağırlığı = Galvaniz Kapak Ağırlığı * Kapak Adedi
            if (_galvanizKapakAgirligi > 0 && _kapakAdedi > 0)
            {
                decimal toplamGalvanizAgirligi = _galvanizKapakAgirligi * _kapakAdedi;
                txtToplamGalvanizAgirligi.Text = toplamGalvanizAgirligi.ToString("F4", CultureInfo.InvariantCulture);
            }
            else if (_galvanizKapakAgirligi > 0)
            {
                // Kapak adedi yoksa sadece galvaniz kapak ağırlığını göster
                txtToplamGalvanizAgirligi.Text = _galvanizKapakAgirligi.ToString("F4", CultureInfo.InvariantCulture);
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

        private decimal CalculatePlakaAgirligi(decimal plakaOlcusuCM, decimal aluminyumKalinligi)
        {
            // ProductionDetailForm'dan alınan hesaplama mantığı
            const decimal tolerance = 0.001m;

            if (plakaOlcusuCM >= 18 && plakaOlcusuCM <= 24)
            {
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.019m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.014m;
            }

            if (plakaOlcusuCM >= 28 && plakaOlcusuCM <= 34)
            {
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.042m;
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.038m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.031m;
            }

            if (plakaOlcusuCM >= 38 && plakaOlcusuCM <= 44)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.068m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.074m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.054m;
            }

            if (plakaOlcusuCM >= 48 && plakaOlcusuCM <= 54)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.105m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.115m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.085m;
            }

            if (plakaOlcusuCM >= 58 && plakaOlcusuCM <= 64)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.150m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.164m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.120m;
            }

            if (plakaOlcusuCM >= 68 && plakaOlcusuCM <= 74)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.205m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.224m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.165m;
            }

            if (plakaOlcusuCM >= 78 && plakaOlcusuCM <= 84)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.270m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.295m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.218m;
            }

            if (plakaOlcusuCM >= 88 && plakaOlcusuCM <= 94)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.345m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.377m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.279m;
            }

            if (plakaOlcusuCM >= 98 && plakaOlcusuCM <= 104)
            {
                if (Math.Abs(aluminyumKalinligi - 0.15m) < tolerance)
                    return 0.430m;
                if (Math.Abs(aluminyumKalinligi - 0.165m) < tolerance)
                    return 0.470m;
                if (Math.Abs(aluminyumKalinligi - 0.12m) < tolerance)
                    return 0.348m;
            }

            return 0m;
        }

        private decimal CalculateGalvanizKapakAgirligi(decimal plakaOlcusuCM)
        {
            // ProductionDetailForm'dan alınan hesaplama mantığı
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

            return 0m;
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

