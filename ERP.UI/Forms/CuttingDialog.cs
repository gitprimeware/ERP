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
    public partial class CuttingDialog : Form
    {
        private TextBox _txtHatve; // Readonly - gösterim için
        private TextBox _txtSize; // Readonly - gösterim için
        private TextBox _txtPlateThickness; // Readonly - gösterim için (Plaka Kalınlığı)
        private ComboBox _cmbMachine;
        private Button _btnAddMachine;
        private ComboBox _cmbSerialNo;
        private TextBox _txtTotalKg; // Readonly - bilgilendirme için
        private TextBox _txtBirPlakaAgirligi; // Readonly - bilgilendirme için
        private TextBox _txtTotalRequiredPlateWeight; // Readonly - bilgilendirme için (Toplam Gereken Plaka)
        private TextBox _txtRemainingKg; // Readonly - Kalan
        private TextBox _txtGerekenPlakaAdedi; // Readonly - Formülden hesaplanan gereken plaka adedi (bilgilendirme)
        private Label _lblBilgilendirme; // Kullanıcı bilgilendirmesi
        private TextBox _txtIstenenPlakaAdedi; // İstenen Plaka Adedi (kullanıcı girer)
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private MachineRepository _machineRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private CuttingRequestRepository _cuttingRequestRepository;
        private MaterialEntryRepository _materialEntryRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;

        public CuttingDialog(MachineRepository machineRepository, SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, Guid orderId)
        {
            _machineRepository = machineRepository;
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _cuttingRequestRepository = new CuttingRequestRepository();
            _materialEntryRepository = new MaterialEntryRepository();
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Kesim Yap";
            this.Width = 600;
            this.Height = 850;
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
            int controlWidth = 300;
            int spacing = 35;

            // Hatve
            var lblHatve = new Label
            {
                Text = "Hatve:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtHatve = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblHatve);
            this.Controls.Add(_txtHatve);
            yPos += spacing;

            // Ölçü
            var lblSize = new Label
            {
                Text = "Ölçü:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtSize = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblSize);
            this.Controls.Add(_txtSize);
            yPos += spacing;

            // Plaka Kalınlığı
            var lblPlateThickness = new Label
            {
                Text = "Plaka Kalınlığı:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtPlateThickness = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPlateThickness);
            this.Controls.Add(_txtPlateThickness);
            yPos += spacing;

            // Makina No
            var lblMachine = new Label
            {
                Text = "Makina No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            var machinePanel = new Panel
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30
            };
            _cmbMachine = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = controlWidth - 120,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _btnAddMachine = new Button
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
            UIHelper.ApplyRoundedButton(_btnAddMachine, 4);
            _btnAddMachine.Click += BtnAddMachine_Click;
            machinePanel.Controls.Add(_cmbMachine);
            machinePanel.Controls.Add(_btnAddMachine);
            this.Controls.Add(lblMachine);
            this.Controls.Add(machinePanel);
            yPos += spacing;

            // Rulo Seri No
            var lblSerialNo = new Label
            {
                Text = "Rulo Seri No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSerialNo = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSerialNo.SelectedIndexChanged += (s, e) => 
            {
                CalculateTotalKg();
                LoadMaterialEntryData();
            };
            this.Controls.Add(lblSerialNo);
            this.Controls.Add(_cmbSerialNo);
            yPos += spacing;

            // Toplam Kg (Readonly)
            var lblTotalKg = new Label
            {
                Text = "Toplam Kg:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtTotalKg = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblTotalKg);
            this.Controls.Add(_txtTotalKg);
            yPos += spacing;

            // İstenen Plaka Adedi (Kullanıcı girer)
            var lblIstenenPlakaAdedi = new Label
            {
                Text = "İstenen Plaka Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtIstenenPlakaAdedi = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtIstenenPlakaAdedi.TextChanged += (s, e) => 
            {
                // Toplam gereken plaka ağırlığını hesapla
                CalculateTotalRequiredPlateWeight();
                CalculateRemainingKg();
                var order = _orderRepository.GetById(_orderId);
                if (order != null)
                {
                    UpdateBilgilendirme(order);
                }
            };
            this.Controls.Add(lblIstenenPlakaAdedi);
            this.Controls.Add(_txtIstenenPlakaAdedi);
            yPos += spacing;

            // Bir Plaka Ağırlığı (Readonly - hesaplanır)
            var lblBirPlakaAgirligi = new Label
            {
                Text = "Bir Plaka Ağırlığı (kg):",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtBirPlakaAgirligi = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F)
            };
            // Bir plaka ağırlığı güncellendiğinde toplam kg'yi yeniden hesapla
            // Readonly TextBox'larda TextChanged bazen tetiklenmeyebilir, bu yüzden CalculateBirPlakaAgirligi içinde de çağrıyoruz
            _txtBirPlakaAgirligi.TextChanged += (s, e) => 
            {
                if (_txtIstenenPlakaAdedi != null && !string.IsNullOrWhiteSpace(_txtIstenenPlakaAdedi.Text))
                {
                    CalculateTotalRequiredPlateWeight();
                    CalculateRemainingKg();
                }
            };
            this.Controls.Add(lblBirPlakaAgirligi);
            this.Controls.Add(_txtBirPlakaAgirligi);
            yPos += spacing;

            // Toplam Gereken Plaka (Readonly - bilgilendirme için)
            var lblTotalRequiredPlateWeight = new Label
            {
                Text = "Toplam Gereken Plaka (kg):",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtTotalRequiredPlateWeight = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblTotalRequiredPlateWeight);
            this.Controls.Add(_txtTotalRequiredPlateWeight);
            yPos += spacing;

            // Gereken Plaka Adedi (Formülden - Readonly)
            var lblGerekenPlakaAdedi = new Label
            {
                Text = "Gereken Plaka Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtGerekenPlakaAdedi = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 240, 248, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblGerekenPlakaAdedi);
            this.Controls.Add(_txtGerekenPlakaAdedi);
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


            // Kalan Kg (Readonly)
            var lblRemainingKg = new Label
            {
                Text = "Kalan Kg:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtRemainingKg = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblRemainingKg);
            this.Controls.Add(_txtRemainingKg);
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
                // Makinaları yükle
                _cmbMachine.Items.Clear();
                var machines = _machineRepository.GetAll();
                foreach (var machine in machines)
                {
                    _cmbMachine.Items.Add(new { Id = machine.Id, Name = machine.Name });
                }
                _cmbMachine.DisplayMember = "Name";
                _cmbMachine.ValueMember = "Id";

                // Seri No'ları yükle
                _cmbSerialNo.Items.Clear();
                var serialNos = _serialNoRepository.GetAll();
                foreach (var serialNo in serialNos)
                {
                    _cmbSerialNo.Items.Add(new { Id = serialNo.Id, SerialNumber = serialNo.SerialNumber });
                }
                _cmbSerialNo.DisplayMember = "SerialNumber";
                _cmbSerialNo.ValueMember = "Id";

                // Operatörleri yükle
                _cmbEmployee.Items.Clear();
                var employees = _employeeRepository.GetAll();
                foreach (var employee in employees)
                {
                    _cmbEmployee.Items.Add(new { Id = employee.Id, FullName = employee.FullName });
                }
                _cmbEmployee.DisplayMember = "FullName";
                _cmbEmployee.ValueMember = "Id";

                // Siparişten hatve, size ve plaka kalınlığı bilgisini al
                var order = _orderRepository.GetById(_orderId);
                if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                {
                    var parts = order.ProductCode.Split('-');
                    
                    // Size bilgisini siparişten al
                    if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                    {
                        // Plaka ölçüsü com (mm): <= 1150 ise aynı, > 1150 ise /2
                        int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                        decimal sizeCM = plakaOlcusuComMM / 10.0m;
                        _txtSize.Text = sizeCM.ToString("F1", CultureInfo.InvariantCulture);
                    }
                    
                    if (parts.Length >= 3)
                    {
                        string modelProfile = parts[2];
                        if (modelProfile.Length > 0)
                        {
                            char modelLetter = modelProfile[0];
                            decimal hatve = GetHtave(modelLetter);
                            _txtHatve.Text = hatve.ToString("F2", CultureInfo.InvariantCulture);

                            // Gereken plaka adedini hesapla (formül sayfasından)
                            CalculateGerekenPlakaAdedi(order, modelLetter, parts);
                        }
                    }
                    
                    // Plaka Kalınlığı (Lamel Kalınlığı)
                    if (order.LamelThickness.HasValue)
                    {
                        _txtPlateThickness.Text = order.LamelThickness.Value.ToString("F3", CultureInfo.InvariantCulture);
                    }
                }

                // Bir plaka ağırlığını hesapla ve göster
                CalculateBirPlakaAgirligi(order);
                
                // Eğer istenen plaka adedi girilmişse, toplam gereken plaka ağırlığını hesapla
                if (!string.IsNullOrWhiteSpace(_txtIstenenPlakaAdedi.Text))
                {
                    CalculateTotalRequiredPlateWeight();
                }
                
                // Kullanıcı bilgilendirmesini güncelle (order ve hesaplanan değerlerle)
                var orderForInfo = _orderRepository.GetById(_orderId);
                if (orderForInfo != null)
                {
                    UpdateBilgilendirme(orderForInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void CalculateTotalKg()
        {
            if (_cmbSerialNo.SelectedItem == null)
            {
                _txtTotalKg.Text = "0";
                _txtRemainingKg.Text = "0";
                return;
            }

            try
            {
                var serialNoId = GetSelectedId(_cmbSerialNo);
                
                // Seri no'ya ait malzeme girişlerinden toplam kg'ı bul
                var materialEntries = _materialEntryRepository.GetAll()
                    .Where(me => me.SerialNoId == serialNoId && me.IsActive)
                    .ToList();

                if (materialEntries.Count > 0)
                {
                    // Toplam kg = Tüm malzeme girişlerinin toplamı - Daha önce kesilen kg'lar
                    decimal totalEntryKg = materialEntries.Sum(me => me.Quantity);
                    
                    // Bu seri no için daha önce kesilen kg'ları hesapla (CuttingRequest'lerden - sadece tamamlananlar)
                    // ÖNEMLİ: Gerçek kesilen adede göre hesapla (ActualCutCount varsa onu kullan, yoksa RequestedPlateCount)
                    decimal previousCutKg = _cuttingRequestRepository.GetAll()
                        .Where(cr => cr.SerialNoId == serialNoId && cr.IsActive && cr.Status == "Tamamlandı")
                        .Sum(cr => 
                        {
                            int actualCount = cr.ActualCutCount ?? cr.RequestedPlateCount;
                            return cr.OnePlateWeight * actualCount;
                        });
                    
                    decimal availableKg = totalEntryKg - previousCutKg;
                    _txtTotalKg.Text = availableKg.ToString("F3", CultureInfo.InvariantCulture);
                }
                else
                {
                    _txtTotalKg.Text = "0";
                }
                
                CalculateRemainingKg();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Toplam kg hesaplanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtTotalKg.Text = "0";
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

        private void CalculateBirPlakaAgirligi(Order order)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode) || !order.LamelThickness.HasValue)
                {
                    _txtBirPlakaAgirligi.Text = "0";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 4)
                {
                    _txtBirPlakaAgirligi.Text = "0";
                    return;
                }

                // Plaka ölçüsü (mm)
                int plakaOlcusuMM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusu))
                {
                    plakaOlcusuMM = plakaOlcusu;
                }

                // Plaka ölçüsü com (mm): <= 1150 ise aynı, > 1150 ise /2
                int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                decimal plakaOlcusuCM = plakaOlcusuComMM / 10.0m;

                // Lamel kalınlığı (plaka kalınlığı)
                decimal lamelKalinligi = order.LamelThickness.Value;

                // Bir plaka ağırlığını hesapla
                decimal birPlakaAgirligi = CalculatePlakaAgirligi(plakaOlcusuCM, lamelKalinligi);
                
                _txtBirPlakaAgirligi.Text = birPlakaAgirligi.ToString("F3", CultureInfo.InvariantCulture);
                
                // Bir plaka ağırlığı hesaplandıktan sonra, eğer yeni kesilecek adet girilmişse toplam kg'yi hesapla
                // Readonly TextBox'larda TextChanged bazen tetiklenmeyebilir, bu yüzden burada da çağrıyoruz
                if (_txtIstenenPlakaAdedi != null && !string.IsNullOrWhiteSpace(_txtIstenenPlakaAdedi.Text))
                {
                    // Kısa bir gecikme ekleyerek TextChanged event'inin tamamlanmasını bekleyelim
                    this.BeginInvoke(new Action(() =>
                    {
                        CalculateTotalRequiredPlateWeight();
                        CalculateRemainingKg();
                    }));
                }
            }
            catch (Exception ex)
            {
                _txtBirPlakaAgirligi.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Bir plaka ağırlığı hesaplanırken hata: {ex.Message}");
            }
        }

        private void CalculateTotalRequiredPlateWeight()
        {
            try
            {
                // İstenen plaka adedi kontrolü
                if (_txtIstenenPlakaAdedi == null || string.IsNullOrWhiteSpace(_txtIstenenPlakaAdedi.Text))
                {
                    if (_txtTotalRequiredPlateWeight != null) _txtTotalRequiredPlateWeight.Text = "0";
                    return;
                }

                if (!int.TryParse(_txtIstenenPlakaAdedi.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int istenenPlakaAdedi) || istenenPlakaAdedi <= 0)
                {
                    if (_txtTotalRequiredPlateWeight != null) _txtTotalRequiredPlateWeight.Text = "0";
                    return;
                }

                // Bir plaka ağırlığı kontrolü
                if (_txtBirPlakaAgirligi == null || string.IsNullOrWhiteSpace(_txtBirPlakaAgirligi.Text))
                {
                    if (_txtTotalRequiredPlateWeight != null) _txtTotalRequiredPlateWeight.Text = "0";
                    return;
                }

                if (!decimal.TryParse(_txtBirPlakaAgirligi.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal birPlakaAgirligi) || birPlakaAgirligi <= 0)
                {
                    if (_txtTotalRequiredPlateWeight != null) _txtTotalRequiredPlateWeight.Text = "0";
                    return;
                }

                // Toplam Gereken Plaka Ağırlığı = İstenen Plaka Adedi × Bir Plaka Ağırlığı
                decimal toplamGerekenPlakaAgirligi = istenenPlakaAdedi * birPlakaAgirligi;
                
                // Toplam gereken plaka ağırlığını güncelle
                if (_txtTotalRequiredPlateWeight != null)
                {
                    _txtTotalRequiredPlateWeight.Text = toplamGerekenPlakaAgirligi.ToString("F3", CultureInfo.InvariantCulture);
                }
                
                // Kalan kg'yi hesapla (Toplam Kg - Toplam Gereken Plaka Ağırlığı)
                CalculateRemainingKg();
            }
            catch (Exception ex)
            {
                if (_txtTotalRequiredPlateWeight != null) _txtTotalRequiredPlateWeight.Text = "0";
                if (_txtRemainingKg != null) _txtRemainingKg.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Kg hesaplanırken hata: {ex.Message}");
            }
        }

        private decimal CalculatePlakaAgirligi(decimal plakaOlcusuCM, decimal aluminyumKalinligi)
        {
            // Üretim tablolarında ölçüler 1 ondalık, kalınlıklar 3 ondalık hassasiyetinde tutuluyor.
            var normalizedSize = Math.Round(plakaOlcusuCM, 1, MidpointRounding.AwayFromZero);
            var normalizedThickness = Math.Round(aluminyumKalinligi, 3, MidpointRounding.AwayFromZero);

            if (normalizedSize >= 18 && normalizedSize <= 24)
            {
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.019m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.014m;
            }

            if (normalizedSize >= 28 && normalizedSize <= 34)
            {
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.042m;
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.038m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.031m;
            }

            if (normalizedSize >= 38 && normalizedSize <= 44)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.068m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.074m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.054m;
            }

            if (normalizedSize >= 48 && normalizedSize <= 54)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.105m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.115m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.084m;
            }

            if (normalizedSize >= 58 && normalizedSize <= 64)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.153m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.168m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.122m;
            }

            if (normalizedSize >= 68 && normalizedSize <= 74)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.210m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.231m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.168m;
            }

            if (normalizedSize >= 78 && normalizedSize <= 84)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.278m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.306m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.222m;
            }

            if (normalizedSize >= 88 && normalizedSize <= 94)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.358m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.394m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.286m;
            }

            if (normalizedSize >= 98 && normalizedSize <= 104)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m))
                    return 0.450m;
                if (ThicknessMatches(normalizedThickness, 0.165m))
                    return 0.495m;
                if (ThicknessMatches(normalizedThickness, 0.12m))
                    return 0.360m;
            }

            return 0m;
        }

        private bool ThicknessMatches(decimal actual, decimal expected)
        {
            return Math.Abs(actual - expected) < 0.001m;
        }

        private void CalculateGerekenPlakaAdedi(Order order, char modelLetter, string[] productCodeParts)
        {
            try
            {
                if (order == null || productCodeParts == null || productCodeParts.Length < 6)
                {
                    _txtGerekenPlakaAdedi.Text = "0";
                    return;
                }

                // 10cm Plaka Adedi
                int plakaAdedi10cm = GetPlakaAdedi10cm(modelLetter);

                // Yükseklik (mm)
                int yukseklikMM = 0;
                if (productCodeParts.Length >= 5 && int.TryParse(productCodeParts[4], out int yukseklik))
                {
                    yukseklikMM = yukseklik;
                }

                // Kapak değeri (mm)
                int kapakDegeriMM = 0;
                if (productCodeParts.Length > 5)
                {
                    string kapakDegeriStr = productCodeParts[5];
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

                // Formül: plaka adedi = (Kapaksız Yükseklik (mm) / 100) * 10cm Plaka Adedi * Toplam Sipariş Adedi
                // Toplam Sipariş Adedi = Sipariş Adedi * Boy Adet * Plaka Adet
                int boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                int plakaAdet = 1; // Varsayılan
                if (productCodeParts.Length >= 4 && int.TryParse(productCodeParts[3], out int plakaOlcusuMM))
                {
                    plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                }
                int toplamSiparisAdedi = order.Quantity * boyAdet * plakaAdet;

                decimal onCmDilimi = kapaksizYukseklikMM / 100m;
                decimal gerekenPlakaAdedi = onCmDilimi * plakaAdedi10cm * toplamSiparisAdedi;
                
                _txtGerekenPlakaAdedi.Text = Math.Round(gerekenPlakaAdedi, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _txtGerekenPlakaAdedi.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Gereken plaka adedi hesaplanırken hata: {ex.Message}");
            }
        }

        // LoadMevcutStokBilgisi metodu kaldırıldı - artık mevcut stok kullanılmıyor

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
                int.TryParse(_txtGerekenPlakaAdedi.Text, out gereken);

                int istenen = 0;
                int.TryParse(_txtIstenenPlakaAdedi.Text, out istenen);

                if (gereken > 0)
                {
                    string bilgi = $"📊 Gereken: {gereken} adet | ";
                    bilgi += $"🆕 İstenen: {istenen} adet";
                    
                    if (istenen < gereken)
                    {
                        int eksik = gereken - istenen;
                        bilgi += $" | ⚠️ {eksik} adet eksik!";
                    }
                    else if (istenen > gereken)
                    {
                        int fazla = istenen - gereken;
                        bilgi += $" | ℹ️ {fazla} adet fazla (kenara konacak)";
                    }
                    
                    _lblBilgilendirme.Text = bilgi;
                }
                else
                {
                    _lblBilgilendirme.Text = "Formül bilgisi eksik, gereken plaka adedi hesaplanamadı.";
                }
            }
            catch (Exception ex)
            {
                _lblBilgilendirme.Text = "";
                System.Diagnostics.Debug.WriteLine($"Bilgilendirme güncellenirken hata: {ex.Message}");
            }
        }

        // Mevcut stok ile ilgili metodlar kaldırıldı - artık kullanılmıyor

        private void CalculateRemainingKg()
        {
            try
            {
                if (_txtTotalKg == null || _txtTotalRequiredPlateWeight == null || _txtRemainingKg == null)
                    return;

                if (decimal.TryParse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalKg))
                {
                    decimal totalRequiredWeight = 0;
                    decimal.TryParse(_txtTotalRequiredPlateWeight.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out totalRequiredWeight);
                    
                    // Kalan Kg = Toplam Kg - Toplam Gereken Plaka Ağırlığı
                    decimal remainingKg = totalKg - totalRequiredWeight;
                    _txtRemainingKg.Text = remainingKg.ToString("F3", CultureInfo.InvariantCulture);
                }
                else
                {
                    _txtRemainingKg.Text = "0";
                }
            }
            catch (Exception ex)
            {
                if (_txtRemainingKg != null)
                    _txtRemainingKg.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Kalan kg hesaplanırken hata: {ex.Message}");
            }
        }

        private void LoadMaterialEntryData()
        {
            if (_cmbSerialNo.SelectedItem == null)
                return;

            try
            {
                var serialNoId = GetSelectedId(_cmbSerialNo);
                
                // Seri no'ya ait ilk malzeme girişini bul
                var materialEntry = _materialEntryRepository.GetAll()
                    .Where(me => me.SerialNoId == serialNoId && me.IsActive)
                    .FirstOrDefault();

                if (materialEntry != null)
                {
                    // MaterialEntry'den bilgileri al ve form alanlarına doldur
                    // Ölçü (Size) zaten var, Hatve'yi siparişten alabiliriz
                    if (materialEntry.Size > 0 && string.IsNullOrEmpty(_txtSize.Text))
                    {
                        _txtSize.Text = materialEntry.Size.ToString();
                    }

                    // Eğer Hatve boşsa ve sipariş varsa, siparişten Hatve'yi al
                    if (string.IsNullOrEmpty(_txtHatve.Text) && _orderId != Guid.Empty)
                    {
                        var order = _orderRepository.GetById(_orderId);
                        if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                        {
                            // Ürün kodundan model harfini al (L, M, D, H)
                            var parts = order.ProductCode.Split('-');
                            if (parts.Length > 0 && parts[0].Length > 0)
                            {
                                char modelLetter = parts[0][parts[0].Length - 1];
                                decimal hatve = GetHtave(modelLetter);
                                if (hatve > 0)
                                {
                                    _txtHatve.Text = hatve.ToString("F2", CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata olursa sessizce devam et
                System.Diagnostics.Debug.WriteLine("MaterialEntry verileri yüklenirken hata: " + ex.Message);
            }
        }

        private void BtnAddMachine_Click(object sender, EventArgs e)
        {
            using (var dialog = new Form
            {
                Text = "Yeni Makina Ekle",
                Width = 400,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblName = new Label
                {
                    Text = "Makina Adı:",
                    Location = new Point(20, 30),
                    AutoSize = true
                };

                var txtName = new TextBox
                {
                    Location = new Point(120, 27),
                    Width = 250,
                    Height = 25
                };

                var lblCode = new Label
                {
                    Text = "Makina Kodu:",
                    Location = new Point(20, 70),
                    AutoSize = true
                };

                var txtCode = new TextBox
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

                dialog.Controls.AddRange(new Control[] { lblName, txtName, lblCode, txtCode, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    try
                    {
                        var newMachine = new Machine 
                        { 
                            Name = txtName.Text,
                            Code = string.IsNullOrWhiteSpace(txtCode.Text) ? null : txtCode.Text
                        };
                        var machineId = _machineRepository.Insert(newMachine);
                        
                        LoadMachines();
                        
                        foreach (var item in _cmbMachine.Items)
                        {
                            var idProperty = item.GetType().GetProperty("Id");
                            if (idProperty != null && idProperty.GetValue(item).Equals(machineId))
                            {
                                _cmbMachine.SelectedItem = item;
                                break;
                            }
                        }
                        
                        MessageBox.Show("Makina başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Makina eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

        private void LoadMachines()
        {
            try
            {
                _cmbMachine.Items.Clear();
                var machines = _machineRepository.GetAll();
                foreach (var machine in machines)
                {
                    _cmbMachine.Items.Add(new { Id = machine.Id, Name = machine.Name });
                }
                _cmbMachine.DisplayMember = "Name";
                _cmbMachine.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Makinalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                var cuttingRequest = new CuttingRequest
                {
                    OrderId = _orderId,
                    Hatve = decimal.Parse(_txtHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(_txtSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    PlateThickness = order.LamelThickness ?? 0m,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    SerialNoId = _cmbSerialNo.SelectedItem != null ? GetSelectedId(_cmbSerialNo) : (Guid?)null,
                    RequestedPlateCount = int.Parse(_txtIstenenPlakaAdedi.Text),
                    OnePlateWeight = decimal.Parse(_txtBirPlakaAgirligi.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    TotalRequiredPlateWeight = decimal.Parse(_txtTotalRequiredPlateWeight.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    RemainingKg = decimal.Parse(_txtRemainingKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede"
                };

                _cuttingRequestRepository.Insert(cuttingRequest);
                MessageBox.Show("Kesim talebi başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_txtHatve.Text) || !decimal.TryParse(_txtHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve) || hatve <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir hatve giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtSize.Text) || !decimal.TryParse(_txtSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) || size <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir ölçü giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbSerialNo.SelectedItem == null)
            {
                MessageBox.Show("Lütfen rulo seri no seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // İstenen plaka adedi kontrolü (kullanıcı girer)
            if (string.IsNullOrWhiteSpace(_txtIstenenPlakaAdedi.Text) || !int.TryParse(_txtIstenenPlakaAdedi.Text, out int istenenPlakaAdedi) || istenenPlakaAdedi <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir istenen plaka adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Toplam gereken plaka ağırlığı kontrolü
            if (string.IsNullOrWhiteSpace(_txtTotalRequiredPlateWeight.Text) || !decimal.TryParse(_txtTotalRequiredPlateWeight.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalRequiredWeight) || totalRequiredWeight <= 0)
            {
                MessageBox.Show("Toplam gereken plaka ağırlığı hesaplanamadı. Lütfen bir plaka ağırlığının doğru hesaplandığından emin olun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
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
