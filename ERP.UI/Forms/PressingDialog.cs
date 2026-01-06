using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class PressingDialog : Form
    {
        private TextBox _txtGerekenPresAdedi; // Formülden hesaplanan gereken pres adedi
        private Label _lblMevcutKesilmisStok; // Mevcut kesilmiş stok bilgisi
        private Label _lblBilgilendirme; // Kullanıcı bilgilendirmesi
        private CheckedListBox _clbKesilmisStoklar; // Multi-select kesilmiş stoklar
        private TextBox _txtPressCount;
        private TextBox _txtPressNo;
        private TextBox _txtPressure;
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        // Seçilen kesilmiş stoklar için dictionary (CuttingId -> Seçilen adet)
        private Dictionary<Guid, int> _selectedCuttings = new Dictionary<Guid, int>();
        
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private PressingRequestRepository _pressingRequestRepository;
        private PressingRepository _pressingRepository;
        private OrderRepository _orderRepository;
        private CuttingRepository _cuttingRepository;
        private Guid _orderId;

        public PressingDialog(SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, Guid orderId)
        {
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _pressingRequestRepository = new PressingRequestRepository();
            _pressingRepository = new PressingRepository();
            _orderRepository = new OrderRepository();
            _cuttingRepository = new CuttingRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Pres Yap";
            this.Width = 550;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;

            CreateControls();
            LoadData();
        }

        private void CreateControls()
        {
            int yPos = 30;
            int labelWidth = 130;
            int controlWidth = 300;
            int controlHeight = 32;
            int spacing = 32;

            // Gereken Pres Adedi (Formülden - Readonly)
            var lblGerekenPresAdedi = new Label
            {
                Text = "Gereken Pres Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtGerekenPresAdedi = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 240, 248, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblGerekenPresAdedi);
            this.Controls.Add(_txtGerekenPresAdedi);
            yPos += spacing;

            // Mevcut Kesilmiş Stok Bilgisi
            var lblMevcutStokLabel = new Label
            {
                Text = "Mevcut Kesilmiş Stok:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblMevcutKesilmisStok = new Label
            {
                Location = new Point(150, yPos),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false
            };
            this.Controls.Add(lblMevcutStokLabel);
            this.Controls.Add(_lblMevcutKesilmisStok);
            yPos += spacing;

            // Bilgilendirme Mesajı
            _lblBilgilendirme = new Label
            {
                Location = new Point(20, yPos),
                Width = controlWidth + labelWidth + 20,
                Height = 30,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeColors.TextSecondary,
                Text = "",
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(_lblBilgilendirme);
            yPos += 45;

            // Kesilmiş Stoklar (Multi-select CheckedListBox)
            var lblKesilmisStoklar = new Label
            {
                Text = "Kesilmişler:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _clbKesilmisStoklar = new CheckedListBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = 150,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            _clbKesilmisStoklar.ItemCheck += ClbKesilmisStoklar_ItemCheck;
            _clbKesilmisStoklar.MouseDoubleClick += ClbKesilmisStoklar_MouseDoubleClick;
            this.Controls.Add(lblKesilmisStoklar);
            this.Controls.Add(_clbKesilmisStoklar);
            yPos += 160;

            // Pres Adedi
            var lblPressCount = new Label
            {
                Text = "Toplam Pres Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtPressCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            _txtPressCount.TextChanged += TxtPressCount_TextChanged;
            this.Controls.Add(lblPressCount);
            this.Controls.Add(_txtPressCount);
            yPos += spacing;

            // Pres No
            var lblPressNo = new Label
            {
                Text = "Pres No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtPressNo = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPressNo);
            this.Controls.Add(_txtPressNo);
            yPos += spacing;

            // Basınç
            var lblPressure = new Label
            {
                Text = "Basınç:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtPressure = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPressure);
            this.Controls.Add(_txtPressure);
            yPos += spacing;

            // Operatör
            var lblEmployee = new Label
            {
                Text = "Operatör:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            var employeePanel = new Panel
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight
            };
            _cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = controlWidth - 75,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _btnAddEmployee = new Button
            {
                Text = "+ Ekle",
                Dock = DockStyle.Right,
                Width = 70,
                Height = controlHeight,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnAddEmployee, 4);
            _btnAddEmployee.Click += BtnAddEmployee_Click;
            employeePanel.Controls.Add(_cmbEmployee);
            employeePanel.Controls.Add(_btnAddEmployee);
            this.Controls.Add(lblEmployee);
            this.Controls.Add(employeePanel);
            yPos += spacing + 12;

            // Butonlar
            _btnCancel = new Button
            {
                Text = "İptal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(370, yPos),
                Width = 90,
                Height = 32,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnCancel, 4);

            _btnSave = new Button
            {
                Text = "Kaydet",
                Location = new Point(275, yPos),
                Width = 90,
                Height = 32,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnSave, 4);
            _btnSave.Click += BtnSave_Click;

            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
            this.AcceptButton = _btnSave;
            this.CancelButton = _btnCancel;
            
            // Dialog yüksekliğini butonların altına göre ayarla (biraz boşluk ile)
            this.Height = yPos + _btnSave.Height + 45;
        }

        private void LoadData()
        {
            try
            {
                var order = _orderRepository.GetById(_orderId);
                if (order == null)
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Gereken pres adedini hesapla (formül sayfasından - aynı plaka adedi formülü)
                CalculateGerekenPresAdedi(order);

                // Tüm kesilmiş stokları yükle (sadece bu sipariş için değil, tüm stoktan)
                LoadKesilmisStoklar(order);

                // Mevcut kesilmiş stok bilgisini göster
                LoadMevcutStokBilgisi(order);

                // Kullanıcı bilgilendirmesini güncelle
                UpdateBilgilendirme(order);

                // Operatörleri yükle
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateGerekenPresAdedi(Order order)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    _txtGerekenPresAdedi.Text = "0";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 6)
                {
                    _txtGerekenPresAdedi.Text = "0";
                    return;
                }

                // Model harfi
                char modelLetter = 'H';
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2];
                    if (modelProfile.Length > 0)
                        modelLetter = modelProfile[0];
                }

                // Ölçüyü al (CM cinsinden) - plakaOlcusuCM hesaplama için
                decimal plakaOlcusuCM = 0;
                int plakaOlcusuMMValue = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out plakaOlcusuMMValue))
                {
                    int plakaOlcusuComMM = plakaOlcusuMMValue <= 1150 ? plakaOlcusuMMValue : plakaOlcusuMMValue / 2;
                    plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                }

                // Hatve ölçümünü hesapla (yeni formata göre)
                decimal? hatveOlcumu = GetHatveOlcumu(modelLetter, plakaOlcusuCM);
                decimal hatve = 0;
                if (hatveOlcumu.HasValue)
                {
                    hatve = hatveOlcumu.Value;
                }
                else
                {
                    hatve = GetHtave(modelLetter);
                }

                // Yükseklik (mm)
                int yukseklikMM = 0;
                if (parts.Length >= 5 && int.TryParse(parts[4], out int yukseklik))
                    yukseklikMM = yukseklik;

                // Kapak değeri (mm)
                int kapakDegeriMM = 0;
                if (parts.Length > 5)
                {
                    string kapakDegeriStr = parts[5];
                    if (kapakDegeriStr == "030")
                        kapakDegeriMM = 30;
                    else if (kapakDegeriStr == "002")
                        kapakDegeriMM = 2;
                    else if (kapakDegeriStr == "016")
                        kapakDegeriMM = 16;
                    else if (int.TryParse(kapakDegeriStr, out int parsedKapak))
                        kapakDegeriMM = parsedKapak;
                }

                // Kapaksız yükseklik - YM ürünleri için kapağı çıkarma, SP ürünleri için çıkar
                bool isYM = order.IsStockOrder;
                int kapaksizYukseklikMM = isYM ? yukseklikMM : (yukseklikMM - kapakDegeriMM);

                // Toplam Sipariş Adedi
                int boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                int plakaAdet = 1;
                // plakaOlcusuMMValue zaten yukarıda tanımlanmış, onu kullan
                if (plakaOlcusuMMValue > 0)
                    plakaAdet = plakaOlcusuMMValue <= 1150 ? 1 : 4;
                int toplamSiparisAdedi = order.Quantity * boyAdet * plakaAdet;

                // Yeni formül: plaka adedi = Math.Ceiling(Kapaksız Yükseklik (mm) / hatve) * Toplam Sipariş Adedi
                // Pres adedi = Plaka adedi (çünkü her plaka bir pres işlemi gerektirir)
                decimal birimPlakaAdedi = hatve > 0 ? (decimal)kapaksizYukseklikMM / hatve : 0;
                decimal birimPlakaAdediYuvarlanmis = Math.Ceiling(birimPlakaAdedi);
                decimal gerekenPresAdedi = birimPlakaAdediYuvarlanmis * toplamSiparisAdedi;
                
                _txtGerekenPresAdedi.Text = Math.Round(gerekenPresAdedi, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _txtGerekenPresAdedi.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Gereken pres adedi hesaplanırken hata: {ex.Message}");
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

        private void LoadKesilmisStoklar(Order order)
        {
            try
            {
                _clbKesilmisStoklar.Items.Clear();
                _selectedCuttings.Clear();

                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                    return;

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 3)
                    return;

                // Model bilgisini al
                string modelProfile = parts[2];
                if (modelProfile.Length == 0)
                    return;

                char modelLetter = modelProfile[0];
                
                // Ölçü bilgisini al (CM cinsinden)
                decimal size = 0;
                decimal plakaOlcusuCM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    size = plakaOlcusuComMM / 10.0m; // cm'ye çevir
                    plakaOlcusuCM = size;
                }

                // Hatve ölçümünü hesapla (yeni formata göre)
                decimal? hatveOlcumu = GetHatveOlcumu(modelLetter, plakaOlcusuCM);
                decimal hatve = 0;
                if (hatveOlcumu.HasValue)
                {
                    hatve = hatveOlcumu.Value;
                }
                else
                {
                    hatve = GetHtave(modelLetter);
                }

                // Tüm kesilmiş stokları yükle (aynı hatve ve ölçü için)
                var allCuttings = _cuttingRepository.GetAll()
                    .Where(c => Math.Abs(c.Hatve - hatve) < 0.01m && 
                                Math.Abs(c.Size - size) < 0.1m && 
                                c.PlakaAdedi > 0 && 
                                c.IsActive)
                    .OrderByDescending(c => c.CuttingDate)
                    .ToList();

                foreach (var cutting in allCuttings)
                {
                    // Kullanılan plaka adedini hesapla
                    var usedPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                        .Sum(p => p.PressCount);
                    
                    int kalanPlakaAdedi = cutting.PlakaAdedi - usedPlakaAdedi;
                    
                    if (kalanPlakaAdedi > 0)
                    {
                        var orderInfo = cutting.OrderId.HasValue ? _orderRepository.GetById(cutting.OrderId.Value) : null;
                        string orderNo = orderInfo?.TrexOrderNo ?? "-";
                        
                        string displayText = $"Kesim #{cutting.CuttingDate:dd.MM.yyyy} - Sipariş: {orderNo} - {kalanPlakaAdedi} adet kalan";
                        var cuttingItem = new CuttingItem 
                        { 
                            CuttingId = cutting.Id,
                            Cutting = cutting,
                            KalanAdet = kalanPlakaAdedi,
                            DisplayText = displayText
                        };
                        _clbKesilmisStoklar.Items.Add(cuttingItem, false);
                        
                        // Eğer daha önce seçilmişse, checkbox'ı işaretle
                        if (_selectedCuttings.ContainsKey(cutting.Id))
                        {
                            int index = _clbKesilmisStoklar.Items.Count - 1;
                            _clbKesilmisStoklar.SetItemChecked(index, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesilmiş stoklar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMevcutStokBilgisi(Order order)
        {
            try
        {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    _lblMevcutKesilmisStok.Text = "Stok bilgisi bulunamadı";
                return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 3)
            {
                    _lblMevcutKesilmisStok.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                string modelProfile = parts[2];
                if (modelProfile.Length == 0)
                {
                    _lblMevcutKesilmisStok.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                char modelLetter = modelProfile[0];
                
                decimal size = 0;
                decimal plakaOlcusuCM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    size = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    size = size / 10; // cm'ye çevir
                    plakaOlcusuCM = size; // Plaka ölçüsü cm olarak
                }

                // Dinamik hatve hesaplaması (rapordaki gibi)
                decimal hatve = GetHtave(modelLetter); // Fallback için eski metod
                var hatveOlcumu = GetHatveOlcumu(modelLetter, plakaOlcusuCM);
                if (hatveOlcumu.HasValue)
                {
                    hatve = hatveOlcumu.Value;
                }

                // Hatve tipi harfini belirle
                string hatveTipiHarf = char.ToUpper(modelLetter).ToString();

                // Toplam mevcut stok
                var mevcutKesilmisler = _cuttingRepository.GetAll()
                    .Where(c => Math.Abs(c.Hatve - hatve) < 0.01m && 
                                Math.Abs(c.Size - size) < 0.1m && 
                                c.IsActive)
                    .ToList();

                int toplamMevcutStok = 0;
                foreach (var cutting in mevcutKesilmisler)
                {
                    var kullanilanPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                        .Sum(p => p.PressCount);
                    
                    int kalanPlakaAdedi = cutting.PlakaAdedi - kullanilanPlakaAdedi;
                    if (kalanPlakaAdedi > 0)
                        toplamMevcutStok += kalanPlakaAdedi;
                }

                // Format: 3.10(H) gibi göster
                _lblMevcutKesilmisStok.Text = $"{toplamMevcutStok} adet (Hatve: {hatve:F2}({hatveTipiHarf}), Ölçü: {size:F1}cm)";
            }
            catch (Exception ex)
            {
                _lblMevcutKesilmisStok.Text = "Stok bilgisi yüklenemedi";
                System.Diagnostics.Debug.WriteLine($"Mevcut stok bilgisi yüklenirken hata: {ex.Message}");
            }
        }

        private void UpdateBilgilendirme(Order order)
        {
            try
            {
                if (order == null)
                {
                    _lblBilgilendirme.Text = "";
                    return;
                }

                int gereken = 0;
                int.TryParse(_txtGerekenPresAdedi.Text, out gereken);

                int mevcut = 0;
                string mevcutText = _lblMevcutKesilmisStok.Text;
                if (!string.IsNullOrEmpty(mevcutText))
                    {
                    var mevcutParts = mevcutText.Split(' ');
                    if (mevcutParts.Length > 0)
                        int.TryParse(mevcutParts[0], out mevcut);
                }

                int secilen = GetSelectedTotalCount();

                if (gereken > 0)
                        {
                    string bilgi = $"📊 Gereken: {gereken} adet | ";
                    bilgi += $"📦 Stokta var: {mevcut} adet | ";
                    bilgi += $"✅ Seçilen: {secilen} adet";
                    
                    if (secilen < gereken && mevcut >= gereken)
                        bilgi += $" | ⚠️ {gereken - secilen} adet daha seçmeniz gerekiyor";
                    else if (mevcut < gereken)
                        bilgi += $" | ⚠️ Stok yetersiz! {gereken - mevcut} adet eksik";
                    
                    _lblBilgilendirme.Text = bilgi;
                }
                else
                            {
                    _lblBilgilendirme.Text = "Formül bilgisi eksik, gereken pres adedi hesaplanamadı.";
                }
            }
            catch (Exception ex)
            {
                _lblBilgilendirme.Text = "";
                System.Diagnostics.Debug.WriteLine($"Bilgilendirme güncellenirken hata: {ex.Message}");
            }
        }

        private void ClbKesilmisStoklar_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck event'i önce çalışır, bu yüzden async olarak güncelleme yapmalıyız
            this.BeginInvoke((MethodInvoker)delegate
            {
                var item = _clbKesilmisStoklar.Items[e.Index] as CuttingItem;
                if (item == null) return;

                if (e.NewValue == CheckState.Checked)
                {
                    // Item seçildiğinde, kullanılacak adet sor
                    // Eğer daha önce seçilmişse, önceki değeri göster
                    int oncekiAdet = _selectedCuttings.ContainsKey(item.CuttingId) ? _selectedCuttings[item.CuttingId] : item.KalanAdet;
                    int kullanilacakAdet = ShowKullanilacakAdetDialog(item, oncekiAdet);
                    if (kullanilacakAdet > 0)
                    {
                        _selectedCuttings[item.CuttingId] = kullanilacakAdet;
                    }
                    else
                    {
                        // Kullanıcı iptal etti veya 0 girdi, seçimi geri al
                        _clbKesilmisStoklar.SetItemChecked(e.Index, false);
                        return;
                    }
                }
                else
                {
                    // Item seçimi kaldırıldığında, dictionary'den çıkar
                    _selectedCuttings.Remove(item.CuttingId);
                }

                UpdatePressCount();
                UpdateBilgilendirme(_orderRepository.GetById(_orderId));
            });
        }

        private void ClbKesilmisStoklar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Çift tıklama ile seçili item'ın kullanılacak adedini değiştir
            int index = _clbKesilmisStoklar.IndexFromPoint(e.Location);
            if (index >= 0 && _clbKesilmisStoklar.GetItemChecked(index))
            {
                var item = _clbKesilmisStoklar.Items[index] as CuttingItem;
                if (item != null)
                {
                    int mevcutAdet = _selectedCuttings.ContainsKey(item.CuttingId) ? _selectedCuttings[item.CuttingId] : item.KalanAdet;
                    int yeniAdet = ShowKullanilacakAdetDialog(item, mevcutAdet);
                    if (yeniAdet > 0)
                    {
                        _selectedCuttings[item.CuttingId] = yeniAdet;
                        UpdatePressCount();
                        UpdateBilgilendirme(_orderRepository.GetById(_orderId));
                    }
                }
            }
        }

        private int ShowKullanilacakAdetDialog(CuttingItem item, int oncekiAdet = 0)
        {
            using (var dialog = new Form
            {
                Text = "Kullanılacak Adet Belirle",
                Width = 400,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblInfo = new Label
                {
                    Text = $"Kesim: {item.DisplayText}\n\nMaksimum kullanılabilir: {item.KalanAdet} adet",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false
                };

                var lblAdet = new Label
                {
                    Text = "Kullanılacak Adet:",
                    Location = new Point(20, 90),
                    AutoSize = true
                };

                var txtAdet = new NumericUpDown
                {
                    Location = new Point(150, 87),
                    Width = 200,
                    Minimum = 1,
                    Maximum = item.KalanAdet,
                    Value = oncekiAdet > 0 ? oncekiAdet : item.KalanAdet, // Önceki değer varsa onu, yoksa tüm adet
                    DecimalPlaces = 0
                };

                var btnOk = new Button
                {
                    Text = "Tamam",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 130),
                    Width = 80
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 130),
                    Width = 80
                };

                dialog.Controls.AddRange(new Control[] { lblInfo, lblAdet, txtAdet, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return (int)txtAdet.Value;
                }
            }

            return 0;
        }

        private void UpdateSelectedCuttings()
        {
            // Bu metod artık kullanılmıyor, ClbKesilmisStoklar_ItemCheck içinde direkt yapılıyor
            // Ama geriye dönük uyumluluk için bırakıyoruz
        }

        private int GetSelectedTotalCount()
        {
            return _selectedCuttings.Values.Sum();
        }

        private void UpdatePressCount()
        {
            int toplam = GetSelectedTotalCount();
            _txtPressCount.Text = toplam.ToString();
        }

        private void TxtPressCount_TextChanged(object sender, EventArgs e)
        {
            // Readonly olduğu için bu event tetiklenmeyecek
        }

        private class CuttingItem
        {
            public Guid CuttingId { get; set; }
            public Cutting Cutting { get; set; }
            public int KalanAdet { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
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
                    // L30: 8.7, L40: 8.7, L50: 8.7, L60: 8.65, L70: 8.65, L80: 8.65, L100: 8.65
                    if (plakaOlcusuYuvarla == 30 || plakaOlcusuYuvarla == 40 || plakaOlcusuYuvarla == 50) return 8.7m;
                    if (plakaOlcusuYuvarla == 60 || plakaOlcusuYuvarla == 70 || plakaOlcusuYuvarla == 80 || plakaOlcusuYuvarla == 100) return 8.65m;
                    break;
            }
            
            return null; // Eğer eşleşme bulunamazsa null döndür
        }

        private void LoadEmployees()
        {
            try
            {
                _cmbEmployee.Items.Clear();
                var employees = _employeeRepository.GetAll();
                foreach (var employee in employees)
                {
                    _cmbEmployee.Items.Add(new { Id = employee.Id, FullName = employee.FullName });
                }
                _cmbEmployee.DisplayMember = "FullName";
                _cmbEmployee.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Operatörler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddEmployee_Click(object sender, EventArgs e)
        {
            using (var dialog = new Form
            {
                Text = "Yeni Operatör Ekle",
                Width = 400,
                Height = 250,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblFirstName = new Label
                {
                    Text = "Ad:",
                    Location = new Point(20, 30),
                    AutoSize = true
                };

                var txtFirstName = new TextBox
                {
                    Location = new Point(120, 27),
                    Width = 250,
                    Height = 25
                };

                var lblLastName = new Label
                {
                    Text = "Soyad:",
                    Location = new Point(20, 70),
                    AutoSize = true
                };

                var txtLastName = new TextBox
                {
                    Location = new Point(120, 67),
                    Width = 250,
                    Height = 25
                };

                var btnOk = new Button
                {
                    Text = "Kaydet",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 120),
                    Width = 80
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 120),
                    Width = 80
                };

                dialog.Controls.AddRange(new Control[] { lblFirstName, txtFirstName, lblLastName, txtLastName, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtFirstName.Text) && !string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    try
                    {
                        var newEmployee = new Employee 
                        { 
                            FirstName = txtFirstName.Text,
                            LastName = txtLastName.Text
                        };
                        var employeeId = _employeeRepository.Insert(newEmployee);
                        
                        LoadEmployees();
                        
                        foreach (var item in _cmbEmployee.Items)
                        {
                            var idProperty = item.GetType().GetProperty("Id");
                            if (idProperty != null && idProperty.GetValue(item).Equals(employeeId))
                            {
                                _cmbEmployee.SelectedItem = item;
                                break;
                            }
                        }
                        
                        MessageBox.Show("Operatör başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Operatör eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var order = _orderRepository.GetById(_orderId);
                if (order == null)
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Seçilen kesilmiş stoklar için pres kayıtları oluştur
                bool hasError = false;
                string errorMessage = "";

                foreach (var selectedCutting in _selectedCuttings)
                {
                    var cutting = _cuttingRepository.GetById(selectedCutting.Key);
                    if (cutting == null)
                        continue;

                    int kullanilacakAdet = selectedCutting.Value;

                    // Bu kesim için zaten kullanılan adeti kontrol et (tamamlanmış pres taleplerinden)
                    // ActualPressCount: kaç tane kesilmiş plaka kullanıldı (kesilmiş stoktan düşecek)
                    var usedPlakaAdedi = _pressingRequestRepository.GetAll()
                        .Where(pr => pr.CuttingId == cutting.Id && pr.IsActive && pr.Status == "Tamamlandı")
                        .Sum(pr => pr.ActualPressCount ?? pr.RequestedPressCount);
                    
                    int kalanPlakaAdedi = cutting.PlakaAdedi - usedPlakaAdedi;
                    
                    if (kullanilacakAdet > kalanPlakaAdedi)
                    {
                        hasError = true;
                        errorMessage += $"Kesim #{cutting.CuttingDate:dd.MM.yyyy} için yeterli stok yok (Kalan: {kalanPlakaAdedi}, İstenen: {kullanilacakAdet})\n";
                        continue;
                    }

                    // Kesilmiş stoktan bilgileri al
                    decimal hatve = cutting.Hatve;
                    decimal size = cutting.Size;
                    
                    // PlateThickness'i kesilmiş stokun siparişinden al
                    decimal plateThickness = 0;
                    if (cutting.OrderId.HasValue)
                    {
                        var cuttingOrder = _orderRepository.GetById(cutting.OrderId.Value);
                        if (cuttingOrder != null && cuttingOrder.LamelThickness.HasValue)
                        {
                            plateThickness = cuttingOrder.LamelThickness.Value;
                        }
                    }
                    

                    // Eğer plateThickness hala 0 ise, mevcut siparişten al
                    if (plateThickness == 0 && order.LamelThickness.HasValue)
                    {
                        plateThickness = order.LamelThickness.Value;
                    }
                    
                    // Eğer hala 0 ise, ürün kodundan al
                    if (plateThickness == 0 && !string.IsNullOrEmpty(order.ProductCode))
                    {
                        var parts = order.ProductCode.Split('-');
                        if (parts.Length >= 7)
                        {
                            decimal.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out plateThickness);
                        }
                    }
                    
                    // Son kontrol: eğer hala 0 ise hata ver
                    if (plateThickness == 0)
                    {
                        hasError = true;
                        errorMessage += $"Kesim #{cutting.CuttingDate:dd.MM.yyyy} için plaka kalınlığı bulunamadı.\n";
                        continue;
                    }

                    // Pres talebi oluştur
                    var pressingRequest = new PressingRequest
                {
                    OrderId = _orderId,
                        PlateThickness = plateThickness,
                        Hatve = hatve,
                        Size = size,
                        SerialNoId = cutting.SerialNoId,
                        CuttingId = cutting.Id,
                        RequestedPressCount = kullanilacakAdet,
                    PressNo = _txtPressNo.Text,
                    Pressure = decimal.Parse(_txtPressure.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    WasteAmount = 0, // Artık WasteCount kullanılıyor, WasteAmount deprecated
                    WasteCount = null, // İlk oluşturulurken null, sonra girilecek
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                        Status = "Beklemede",
                        RequestDate = DateTime.Now
                };

                    var pressingRequestId = _pressingRequestRepository.Insert(pressingRequest);
                    
                    // Event feed kaydı ekle
                    if (order != null)
                    {
                        EventFeedService.PressingRequestCreated(pressingRequestId, _orderId, order.TrexOrderNo);
                    }
                }

                if (hasError)
                {
                    MessageBox.Show("Bazı pres talepleri oluşturulamadı:\n\n" + errorMessage, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Pres talepleri başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres kaydedilirken hata oluştu: " + ex.Message + "\n\nDetay: " + (ex.InnerException?.Message ?? ""), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            // Kesilmiş stok seçimi kontrolü
            if (_selectedCuttings.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir kesilmiş stok seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Toplam pres adedi kontrolü
            int toplamPresAdedi = GetSelectedTotalCount();
            if (toplamPresAdedi <= 0)
            {
                MessageBox.Show("Seçilen kesilmiş stoklardan toplam pres adedi 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Gereken pres adedi kontrolü
            int gereken = 0;
            int.TryParse(_txtGerekenPresAdedi.Text, out gereken);
            
            if (gereken > 0 && toplamPresAdedi < gereken)
            {
                var result = MessageBox.Show(
                    $"Gereken pres adedi: {gereken}, seçilen: {toplamPresAdedi}.\nDevam etmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtPressNo.Text))
            {
                MessageBox.Show("Lütfen pres no giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtPressure.Text) || !decimal.TryParse(_txtPressure.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pressure) || pressure <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir basınç giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Seçilen kesilmiş stoklar için adet kontrolü
            foreach (var selectedCutting in _selectedCuttings)
            {
                var cutting = _cuttingRepository.GetById(selectedCutting.Key);
            if (cutting != null)
            {
                var usedPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                    .Sum(p => p.PressCount);
                
                    int kalanPlakaAdedi = cutting.PlakaAdedi - usedPlakaAdedi;
                
                    if (selectedCutting.Value > kalanPlakaAdedi)
                {
                        MessageBox.Show($"Kesim #{cutting.CuttingDate:dd.MM.yyyy} için yeterli stok yok (Kalan: {kalanPlakaAdedi}, Seçilen: {selectedCutting.Value})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                    }
                }
            }

            return true;
        }

        private Guid GetSelectedId(ComboBox comboBox)
        {
            if (comboBox.SelectedItem == null)
                return Guid.Empty;

            var idProperty = comboBox.SelectedItem.GetType().GetProperty("Id");
            return (Guid)idProperty.GetValue(comboBox.SelectedItem);
        }
    }
}

