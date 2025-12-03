using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class AssemblyDialog : Form
    {
        private TextBox _txtGerekenMontajAdedi; // Formülden hesaplanan gereken montaj adedi
        private Label _lblMevcutKenetlenmisStok; // Mevcut kenetlenmiş stok bilgisi
        private Label _lblBilgilendirme; // Kullanıcı bilgilendirmesi
        private CheckedListBox _clbKenetlenmisStoklar; // Multi-select kenetlenmiş stoklar
        private TextBox _txtAssemblyCount; // Toplam montaj adedi (readonly)
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        // Seçilen kenetlenmiş stoklar için dictionary (ClampingId -> Seçilen adet)
        private Dictionary<Guid, int> _selectedClampings = new Dictionary<Guid, int>();
        
        private EmployeeRepository _employeeRepository;
        private AssemblyRepository _assemblyRepository;
        private OrderRepository _orderRepository;
        private ClampingRepository _clampingRepository;
        private Guid _orderId;

        public AssemblyDialog(EmployeeRepository employeeRepository, Guid orderId)
        {
            _employeeRepository = employeeRepository;
            _assemblyRepository = new AssemblyRepository();
            _orderRepository = new OrderRepository();
            _clampingRepository = new ClampingRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Montaj Yap";
            this.Width = 600;
            this.Height = 700;
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
            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 400;
            int spacing = 35;

            // Gereken Montaj Adedi (Formülden - Readonly)
            var lblGerekenMontajAdedi = new Label
            {
                Text = "Gereken Montaj Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtGerekenMontajAdedi = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 240, 248, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblGerekenMontajAdedi);
            this.Controls.Add(_txtGerekenMontajAdedi);
            yPos += spacing;

            // Mevcut Kenetlenmiş Stok Bilgisi
            var lblMevcutStokLabel = new Label
            {
                Text = "Mevcut Kenetlenmiş Stok:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblMevcutKenetlenmisStok = new Label
            {
                Location = new Point(180, yPos),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false
            };
            this.Controls.Add(lblMevcutStokLabel);
            this.Controls.Add(_lblMevcutKenetlenmisStok);
            yPos += spacing;

            // Bilgilendirme Mesajı
            _lblBilgilendirme = new Label
            {
                Location = new Point(20, yPos),
                Width = controlWidth + labelWidth + 20,
                Height = 40,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeColors.TextSecondary,
                Text = "",
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(_lblBilgilendirme);
            yPos += 45;

            // Kenetlenmiş Stoklar (Multi-select CheckedListBox)
            var lblKenetlenmisStoklar = new Label
            {
                Text = "Kenetlenmiş Stoklardan Seçiniz:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _clbKenetlenmisStoklar = new CheckedListBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 150,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            _clbKenetlenmisStoklar.ItemCheck += ClbKenetlenmisStoklar_ItemCheck;
            _clbKenetlenmisStoklar.MouseDoubleClick += ClbKenetlenmisStoklar_MouseDoubleClick;
            this.Controls.Add(lblKenetlenmisStoklar);
            this.Controls.Add(_clbKenetlenmisStoklar);
            yPos += 160;

            // Montaj Adedi
            var lblAssemblyCount = new Label
            {
                Text = "Toplam Montaj Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtAssemblyCount = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            this.Controls.Add(lblAssemblyCount);
            this.Controls.Add(_txtAssemblyCount);
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30
            };
            _cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = controlWidth - 120,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _btnAddEmployee = new Button
            {
                Text = "+ Ekle",
                Dock = DockStyle.Right,
                Width = 110,
                Height = 30,
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
            yPos += spacing + 10;

            // Butonlar
            _btnCancel = new Button
            {
                Text = "İptal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(380, yPos),
                Width = 100,
                Height = 35,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnCancel, 4);

            _btnSave = new Button
            {
                Text = "Kaydet",
                Location = new Point(270, yPos),
                Width = 100,
                Height = 35,
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

                // Gereken montaj adedini hesapla (formül sayfasından - aynı plaka adedi formülü)
                CalculateGerekenMontajAdedi(order);

                // Tüm kenetlenmiş stokları yükle (sadece bu sipariş için değil, tüm stoktan)
                LoadKenetlenmisStoklar(order);

                // Mevcut kenetlenmiş stok bilgisini göster
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

        private void CalculateGerekenMontajAdedi(Order order)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    _txtGerekenMontajAdedi.Text = "0";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 6)
                {
                    _txtGerekenMontajAdedi.Text = "0";
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

                // 10cm Plaka Adedi
                int plakaAdedi10cm = GetPlakaAdedi10cm(modelLetter);

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

                // Kapaksız yükseklik
                int kapaksizYukseklikMM = yukseklikMM - kapakDegeriMM;

                // Toplam Sipariş Adedi
                int boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                int plakaAdet = 1;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                    plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                int toplamSiparisAdedi = order.Quantity * boyAdet * plakaAdet;

                // Formül: plaka adedi = (Kapaksız Yükseklik (mm) / 100) * 10cm Plaka Adedi * Toplam Sipariş Adedi
                // Montaj adedi = Sipariş adedi (çünkü her ürün bir montaj işlemi gerektirir)
                decimal onCmDilimi = kapaksizYukseklikMM / 100m;
                decimal gerekenPlakaAdedi = onCmDilimi * plakaAdedi10cm * toplamSiparisAdedi;
                
                // Montaj adedi = Sipariş adedi
                _txtGerekenMontajAdedi.Text = order.Quantity.ToString();
            }
            catch (Exception ex)
            {
                _txtGerekenMontajAdedi.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Gereken montaj adedi hesaplanırken hata: {ex.Message}");
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

        private void LoadKenetlenmisStoklar(Order order)
        {
            try
            {
                _clbKenetlenmisStoklar.Items.Clear();
                _selectedClampings.Clear();

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
                decimal hatve = GetHtave(modelLetter);
                
                // Ölçü bilgisini al
                decimal size = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    size = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    size = size / 10; // cm'ye çevir
                }

                // Tüm kenetlenmiş stokları yükle (aynı hatve ve ölçü için)
                var allClampings = _clampingRepository.GetAll()
                    .Where(c => Math.Abs(c.Hatve - hatve) < 0.01m && 
                                Math.Abs(c.Size - size) < 0.1m && 
                                c.ClampCount > 0 && 
                                c.IsActive)
                    .OrderByDescending(c => c.ClampingDate)
                    .ToList();

                foreach (var clamping in allClampings)
                {
                    // Kullanılan kenet adedini hesapla (montaj işlemlerinde kullanılan)
                    var usedClampCount = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    int kalanKenetAdedi = clamping.ClampCount - usedClampCount;
                    
                    if (kalanKenetAdedi > 0)
                    {
                        var orderInfo = clamping.OrderId.HasValue ? _orderRepository.GetById(clamping.OrderId.Value) : null;
                        string orderNo = orderInfo?.TrexOrderNo ?? "-";
                        
                        string displayText = $"Kenet #{clamping.ClampingDate:dd.MM.yyyy} - Sipariş: {orderNo} - {kalanKenetAdedi} adet kalan";
                        var clampingItem = new ClampingItem 
                        { 
                            ClampingId = clamping.Id,
                            Clamping = clamping,
                            KalanAdet = kalanKenetAdedi,
                            DisplayText = displayText
                        };
                        _clbKenetlenmisStoklar.Items.Add(clampingItem, false);
                        
                        // Eğer daha önce seçilmişse, checkbox'ı işaretle
                        if (_selectedClampings.ContainsKey(clamping.Id))
                        {
                            int index = _clbKenetlenmisStoklar.Items.Count - 1;
                            _clbKenetlenmisStoklar.SetItemChecked(index, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş stoklar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMevcutStokBilgisi(Order order)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    _lblMevcutKenetlenmisStok.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 3)
                {
                    _lblMevcutKenetlenmisStok.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                string modelProfile = parts[2];
                if (modelProfile.Length == 0)
                {
                    _lblMevcutKenetlenmisStok.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                char modelLetter = modelProfile[0];
                decimal hatve = GetHtave(modelLetter);
                
                decimal size = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    size = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    size = size / 10;
                }

                // Toplam mevcut stok
                var mevcutKenetlenmisler = _clampingRepository.GetAll()
                    .Where(c => Math.Abs(c.Hatve - hatve) < 0.01m && 
                                Math.Abs(c.Size - size) < 0.1m && 
                                c.IsActive)
                    .ToList();

                int toplamMevcutStok = 0;
                foreach (var clamping in mevcutKenetlenmisler)
                {
                    var kullanilanKenetAdedi = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    int kalanKenetAdedi = clamping.ClampCount - kullanilanKenetAdedi;
                    if (kalanKenetAdedi > 0)
                        toplamMevcutStok += kalanKenetAdedi;
                }

                _lblMevcutKenetlenmisStok.Text = $"{toplamMevcutStok} adet (Hatve: {hatve:F2}, Ölçü: {size:F1}cm)";
            }
            catch (Exception ex)
            {
                _lblMevcutKenetlenmisStok.Text = "Stok bilgisi yüklenemedi";
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
                int.TryParse(_txtGerekenMontajAdedi.Text, out gereken);

                int mevcut = 0;
                string mevcutText = _lblMevcutKenetlenmisStok.Text;
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
                    _lblBilgilendirme.Text = "Formül bilgisi eksik, gereken montaj adedi hesaplanamadı.";
                }
            }
            catch (Exception ex)
            {
                _lblBilgilendirme.Text = "";
                System.Diagnostics.Debug.WriteLine($"Bilgilendirme güncellenirken hata: {ex.Message}");
            }
        }

        private void ClbKenetlenmisStoklar_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck event'i önce çalışır, bu yüzden async olarak güncelleme yapmalıyız
            this.BeginInvoke((MethodInvoker)delegate
            {
                var item = _clbKenetlenmisStoklar.Items[e.Index] as ClampingItem;
                if (item == null) return;

                if (e.NewValue == CheckState.Checked)
                {
                    // Item seçildiğinde, kullanılacak adet sor
                    // Eğer daha önce seçilmişse, önceki değeri göster
                    int oncekiAdet = _selectedClampings.ContainsKey(item.ClampingId) ? _selectedClampings[item.ClampingId] : item.KalanAdet;
                    int kullanilacakAdet = ShowKullanilacakAdetDialog(item, oncekiAdet);
                    if (kullanilacakAdet > 0)
                    {
                        _selectedClampings[item.ClampingId] = kullanilacakAdet;
                    }
                    else
                    {
                        // Kullanıcı iptal etti veya 0 girdi, seçimi geri al
                        _clbKenetlenmisStoklar.SetItemChecked(e.Index, false);
                        return;
                    }
                }
                else
                {
                    // Item seçimi kaldırıldığında, dictionary'den çıkar
                    _selectedClampings.Remove(item.ClampingId);
                }

                UpdateAssemblyCount();
                UpdateBilgilendirme(_orderRepository.GetById(_orderId));
            });
        }

        private void ClbKenetlenmisStoklar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Çift tıklama ile seçili item'ın kullanılacak adedini değiştir
            int index = _clbKenetlenmisStoklar.IndexFromPoint(e.Location);
            if (index >= 0 && _clbKenetlenmisStoklar.GetItemChecked(index))
            {
                var item = _clbKenetlenmisStoklar.Items[index] as ClampingItem;
                if (item != null)
                {
                    int mevcutAdet = _selectedClampings.ContainsKey(item.ClampingId) ? _selectedClampings[item.ClampingId] : item.KalanAdet;
                    int yeniAdet = ShowKullanilacakAdetDialog(item, mevcutAdet);
                    if (yeniAdet > 0)
                    {
                        _selectedClampings[item.ClampingId] = yeniAdet;
                        UpdateAssemblyCount();
                        UpdateBilgilendirme(_orderRepository.GetById(_orderId));
                    }
                }
            }
        }

        private int ShowKullanilacakAdetDialog(ClampingItem item, int oncekiAdet = 0)
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
                    Text = $"Kenet: {item.DisplayText}\n\nMaksimum kullanılabilir: {item.KalanAdet} adet",
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

        private int GetSelectedTotalCount()
        {
            return _selectedClampings.Values.Sum();
        }

        private void UpdateAssemblyCount()
        {
            int toplam = GetSelectedTotalCount();
            _txtAssemblyCount.Text = toplam.ToString();
        }

        private class ClampingItem
        {
            public Guid ClampingId { get; set; }
            public Clamping Clamping { get; set; }
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

                // Ürün kodundan bilgileri hesapla
                decimal plateThickness = 0;
                decimal hatve = 0;
                decimal size = 0;
                decimal length = 0;
                
                if (!string.IsNullOrEmpty(order.ProductCode))
                {
                    var parts = order.ProductCode.Split('-');
                    if (parts.Length >= 7)
                    {
                        decimal.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out plateThickness);
                    }
                    
                    // Hatve ve Size'ı ürün kodundan al
                    if (parts.Length >= 3)
                    {
                        string modelProfile = parts[2];
                        if (modelProfile.Length > 0)
                        {
                            char modelLetter = modelProfile[0];
                            hatve = GetHtave(modelLetter);
                        }
                    }
                    
                    if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                    {
                        size = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                        size = size / 10; // cm
                    }
                }

                // Seçilen kenetlenmiş stoklar için montaj kayıtları oluştur
                bool hasError = false;
                string errorMessage = "";

                foreach (var selectedClamping in _selectedClampings)
                {
                    var clamping = _clampingRepository.GetById(selectedClamping.Key);
                    if (clamping == null)
                        continue;

                    int kullanilacakAdet = selectedClamping.Value;

                    // Bu kenet için zaten kullanılan adeti kontrol et
                    var usedClampCount = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    int kalanKenetAdedi = clamping.ClampCount - usedClampCount;
                    
                    if (kullanilacakAdet > kalanKenetAdedi)
                    {
                        hasError = true;
                        errorMessage += $"Kenet #{clamping.ClampingDate:dd.MM.yyyy} için yeterli stok yok (Kalan: {kalanKenetAdedi}, İstenen: {kullanilacakAdet})\n";
                        continue;
                    }

                    // Montaj kaydı oluştur
                    var assembly = new Assembly
                    {
                        OrderId = _orderId,
                        PlateThickness = plateThickness > 0 ? plateThickness : clamping.PlateThickness,
                        Hatve = hatve > 0 ? hatve : clamping.Hatve,
                        Size = size > 0 ? size : clamping.Size,
                        Length = length > 0 ? length : clamping.Length,
                        SerialNoId = clamping.SerialNoId,
                        ClampingId = clamping.Id,
                        AssemblyCount = 1, // Her montaj kaydı bir ürün montajını temsil eder
                        UsedClampCount = kullanilacakAdet,
                        EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                        AssemblyDate = DateTime.Now
                    };

                    _assemblyRepository.Insert(assembly);
                }

                if (hasError)
                {
                    MessageBox.Show("Bazı montaj kayıtları oluşturulamadı:\n\n" + errorMessage, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Montaj kayıtları başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            // Kenetlenmiş stok seçimi kontrolü
            if (_selectedClampings.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir kenetlenmiş stok seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Toplam montaj adedi kontrolü
            int toplamMontajAdedi = GetSelectedTotalCount();
            if (toplamMontajAdedi <= 0)
            {
                MessageBox.Show("Seçilen kenetlenmiş stoklardan toplam montaj adedi 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Gereken montaj adedi kontrolü
            int gereken = 0;
            int.TryParse(_txtGerekenMontajAdedi.Text, out gereken);
            
            if (gereken > 0 && toplamMontajAdedi < gereken)
            {
                var result = MessageBox.Show(
                    $"Gereken montaj adedi: {gereken}, seçilen: {toplamMontajAdedi}.\nDevam etmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                    return false;
            }

            // Seçilen kenetlenmiş stoklar için adet kontrolü
            foreach (var selectedClamping in _selectedClampings)
            {
                var clamping = _clampingRepository.GetById(selectedClamping.Key);
                if (clamping != null)
                {
                    var usedClampCount = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    int kalanKenetAdedi = clamping.ClampCount - usedClampCount;
                    
                    if (selectedClamping.Value > kalanKenetAdedi)
                    {
                        MessageBox.Show($"Kenet #{clamping.ClampingDate:dd.MM.yyyy} için yeterli stok yok (Kalan: {kalanKenetAdedi}, Seçilen: {selectedClamping.Value})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

