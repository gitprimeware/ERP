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
        private TextBox _txtHatve;
        private TextBox _txtSize;
        private ComboBox _cmbMachine;
        private Button _btnAddMachine;
        private ComboBox _cmbSerialNo;
        private TextBox _txtTotalKg;
        private TextBox _txtCutKg; // Toplam Kesilen Kg (readonly - adet * bir plaka ağırlığı)
        private TextBox _txtBirPlakaAgirligi; // Bir Plaka Ağırlığı (readonly - hesaplanır)
        private TextBox _txtPlakaAdedi; // Bu Kesimde Oluşan Plaka Adedi (readonly - yeni kesilecek adet ile aynı)
        private TextBox _txtWasteKg;
        private TextBox _txtRemainingKg;
        private TextBox _txtGerekenPlakaAdedi; // Formülden hesaplanan gereken plaka adedi
        private Label _lblMevcutStokBilgisi; // Mevcut kesilmiş stok bilgisi
        private Label _lblBilgilendirme; // Kullanıcı bilgilendirmesi
        private CheckedListBox _clbMevcutKesilmisStok; // Mevcut kesilmiş stoklardan seçim
        private TextBox _txtKullanilacakMevcutStok; // Mevcut stoktan kullanılacak adet (readonly)
        private TextBox _txtYeniKesilecekAdet; // Yeni kesilecek adet (gereken - mevcut stoktan kullanılan)
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private MachineRepository _machineRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private CuttingRepository _cuttingRepository;
        private PressingRepository _pressingRepository;
        private MaterialEntryRepository _materialEntryRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;
        
        // Seçilen mevcut kesilmiş stoklar için dictionary (CuttingId -> Seçilen adet)
        private Dictionary<Guid, int> _selectedMevcutStoklar = new Dictionary<Guid, int>();

        public CuttingDialog(MachineRepository machineRepository, SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, Guid orderId)
        {
            _machineRepository = machineRepository;
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _cuttingRepository = new CuttingRepository();
            _pressingRepository = new PressingRepository();
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
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblSize);
            this.Controls.Add(_txtSize);
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

            // Yeni Kesilecek Plaka Adedi (Kullanıcı girer)
            var lblYeniKesilecekPlakaAdedi = new Label
            {
                Text = "Yeni Kesilecek Plaka Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtYeniKesilecekAdet = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            _txtYeniKesilecekAdet.TextChanged += (s, e) => 
            {
                // Oluşan plaka adedi = Yeni kesilecek adet
                if (int.TryParse(_txtYeniKesilecekAdet.Text, out int adet))
                {
                    _txtPlakaAdedi.Text = adet.ToString();
                }
                else
                {
                    _txtPlakaAdedi.Text = "0";
                }
                
                // Toplam kesilen kg'yi hesapla
                CalculateKgFromAdet();
                CalculateRemainingKg();
                var order = _orderRepository.GetById(_orderId);
                if (order != null)
                {
                    UpdateBilgilendirme(order);
                }
            };
            this.Controls.Add(lblYeniKesilecekPlakaAdedi);
            this.Controls.Add(_txtYeniKesilecekAdet);
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
                if (_txtYeniKesilecekAdet != null && !string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text))
                {
                    CalculateKgFromAdet();
                    CalculateRemainingKg();
                }
            };
            this.Controls.Add(lblBirPlakaAgirligi);
            this.Controls.Add(_txtBirPlakaAgirligi);
            yPos += spacing;

            // Toplam Kesilen Kg (Readonly - otomatik hesaplanır: Adet × Bir Plaka Ağırlığı)
            var lblCutKg = new Label
            {
                Text = "Toplam Kesilen Kg:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtCutKg = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblCutKg);
            this.Controls.Add(_txtCutKg);
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

            // Mevcut Stok Bilgisi (Readonly)
            var lblMevcutStokLabel = new Label
            {
                Text = "Mevcut Kesilmiş Stok:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblMevcutStokBilgisi = new Label
            {
                Location = new Point(180, yPos),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false
            };
            this.Controls.Add(lblMevcutStokLabel);
            this.Controls.Add(_lblMevcutStokBilgisi);
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

            // Mevcut Kesilmiş Stoklardan Seçim (Multi-select)
            var lblMevcutStokSecim = new Label
            {
                Text = "Mevcut Kesilmiş Stoklardan Seçiniz:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _clbMevcutKesilmisStok = new CheckedListBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 120,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            _clbMevcutKesilmisStok.ItemCheck += ClbMevcutKesilmisStok_ItemCheck;
            this.Controls.Add(lblMevcutStokSecim);
            this.Controls.Add(_clbMevcutKesilmisStok);
            yPos += 130;

            // Mevcut Stoktan Kullanılacak Adet (Readonly)
            var lblKullanilacakMevcutStok = new Label
            {
                Text = "Mevcut Stoktan Kullanılacak:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtKullanilacakMevcutStok = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblKullanilacakMevcutStok);
            this.Controls.Add(_txtKullanilacakMevcutStok);
            yPos += spacing;


            // Oluşan Plaka Adedi (Readonly - otomatik hesaplanır)
            var lblPlakaAdedi = new Label
            {
                Text = "Bu Kesimde Oluşan Plaka Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtPlakaAdedi = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblPlakaAdedi);
            this.Controls.Add(_txtPlakaAdedi);
            yPos += spacing;


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

                // Siparişten hatve ve size bilgisini al
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
                        
                        if (string.IsNullOrEmpty(_txtSize.Text))
                        {
                            _txtSize.Text = sizeCM.ToString("F1", CultureInfo.InvariantCulture);
                        }
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
                    
                    // Mevcut kesilmiş stok bilgisini yükle
                    LoadMevcutStokBilgisi(order);
                    
                    // Mevcut kesilmiş stokları listele (seçim için)
                    LoadMevcutKesilmisStoklar(order);
                }

                // Bir plaka ağırlığını hesapla ve göster
                CalculateBirPlakaAgirligi(order);
                
                // Eğer yeni kesilecek adet girilmişse, toplam kg'yi hesapla
                if (!string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text))
                {
                    CalculateKgFromAdet();
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
                    
                    // Bu seri no için daha önce kesilen kg'ları hesapla
                    var previousCuttings = _cuttingRepository.GetAll()
                        .Where(c => c.SerialNoId == serialNoId && c.IsActive)
                        .Sum(c => c.CutKg);
                    
                    decimal availableKg = totalEntryKg - previousCuttings;
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
                if (_txtYeniKesilecekAdet != null && !string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text))
                {
                    // Kısa bir gecikme ekleyerek TextChanged event'inin tamamlanmasını bekleyelim
                    this.BeginInvoke(new Action(() =>
                    {
                        CalculateKgFromAdet();
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

        private void CalculateKgFromAdet()
        {
            try
            {
                // Yeni kesilecek adet kontrolü
                if (_txtYeniKesilecekAdet == null || string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text))
                {
                    if (_txtCutKg != null) _txtCutKg.Text = "0";
                    if (_txtRemainingKg != null) _txtRemainingKg.Text = "0";
                    return;
                }

                if (!int.TryParse(_txtYeniKesilecekAdet.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int yeniKesilecekAdet) || yeniKesilecekAdet <= 0)
                {
                    if (_txtCutKg != null) _txtCutKg.Text = "0";
                    if (_txtRemainingKg != null) _txtRemainingKg.Text = "0";
                    return;
                }

                // Bir plaka ağırlığı kontrolü
                if (_txtBirPlakaAgirligi == null || string.IsNullOrWhiteSpace(_txtBirPlakaAgirligi.Text))
                {
                    if (_txtCutKg != null) _txtCutKg.Text = "0";
                    if (_txtRemainingKg != null) _txtRemainingKg.Text = "0";
                    return;
                }

                if (!decimal.TryParse(_txtBirPlakaAgirligi.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal birPlakaAgirligi) || birPlakaAgirligi <= 0)
                {
                    if (_txtCutKg != null) _txtCutKg.Text = "0";
                    if (_txtRemainingKg != null) _txtRemainingKg.Text = "0";
                    return;
                }

                // Toplam Kesilen Kg = Yeni Kesilecek Adet × Bir Plaka Ağırlığı
                decimal toplamKesilenKg = yeniKesilecekAdet * birPlakaAgirligi;
                
                // Toplam kesilen kg'yi güncelle
                if (_txtCutKg != null)
                {
                    _txtCutKg.Text = toplamKesilenKg.ToString("F3", CultureInfo.InvariantCulture);
                }
                
                // Kalan kg'yi hesapla (Toplam Kg - Toplam Kesilen Kg)
                CalculateRemainingKg();
            }
            catch (Exception ex)
            {
                if (_txtCutKg != null) _txtCutKg.Text = "0";
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

        private void LoadMevcutStokBilgisi(Order order)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    _lblMevcutStokBilgisi.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                if (parts.Length < 3)
                {
                    _lblMevcutStokBilgisi.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                string modelProfile = parts[2];
                if (modelProfile.Length == 0)
                {
                    _lblMevcutStokBilgisi.Text = "Stok bilgisi bulunamadı";
                    return;
                }

                char modelLetter = modelProfile[0];
                decimal hatve = GetHtave(modelLetter);
                
                // Ölçü bilgisini al
                decimal size = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    size = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    size = size / 10; // cm'ye çevir
                }

                // Aynı hatve ve ölçüdeki kesilmiş stokları bul
                var mevcutKesilmisler = _cuttingRepository.GetAll()
                    .Where(c => Math.Abs(c.Hatve - hatve) < 0.01m && 
                                Math.Abs(c.Size - size) < 0.1m && 
                                c.IsActive)
                    .ToList();

                // Her kesim işleminden kalan plaka adedi
                int toplamMevcutStok = 0;
                foreach (var cutting in mevcutKesilmisler)
                {
                    var kullanilanPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                        .Sum(p => p.PressCount);
                    
                    int kalanPlakaAdedi = cutting.PlakaAdedi - kullanilanPlakaAdedi;
                    if (kalanPlakaAdedi > 0)
                    {
                        toplamMevcutStok += kalanPlakaAdedi;
                    }
                }

                _lblMevcutStokBilgisi.Text = $"{toplamMevcutStok} adet (Hatve: {hatve:F2}, Ölçü: {size:F1}cm)";
            }
            catch (Exception ex)
            {
                _lblMevcutStokBilgisi.Text = "Stok bilgisi yüklenemedi";
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
                int.TryParse(_txtGerekenPlakaAdedi.Text, out gereken);

                int mevcut = 0;
                string mevcutText = _lblMevcutStokBilgisi.Text;
                if (!string.IsNullOrEmpty(mevcutText))
                {
                    var mevcutParts = mevcutText.Split(' ');
                    if (mevcutParts.Length > 0)
                    {
                        int.TryParse(mevcutParts[0], out mevcut);
                    }
                }

                int mevcutStoktanKullanilacak = GetSelectedMevcutStokTotalCount();
                int yeniKesilecek = 0;
                int.TryParse(_txtYeniKesilecekAdet.Text, out yeniKesilecek);

                if (gereken > 0)
                {
                    string bilgi = $"📊 Gereken: {gereken} adet | ";
                    bilgi += $"📦 Stokta var: {mevcut} adet | ";
                    bilgi += $"✅ Stoktan seçilen: {mevcutStoktanKullanilacak} adet | ";
                    bilgi += $"🆕 Yeni kesilecek: {yeniKesilecek} adet";
                    
                    int toplam = mevcutStoktanKullanilacak + yeniKesilecek;
                    if (toplam < gereken)
                    {
                        int eksik = gereken - toplam;
                        bilgi += $" | ⚠️ {eksik} adet eksik!";
                    }
                    else if (toplam > gereken)
                    {
                        int fazla = toplam - gereken;
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

        private int GetSelectedMevcutStokTotalCount()
        {
            return _selectedMevcutStoklar.Values.Sum();
        }

        private void UpdateSelectedMevcutStoklar()
        {
            _selectedMevcutStoklar.Clear();
            
            for (int i = 0; i < _clbMevcutKesilmisStok.Items.Count; i++)
            {
                if (_clbMevcutKesilmisStok.GetItemChecked(i))
                {
                    var item = _clbMevcutKesilmisStok.Items[i] as CuttingStockItem;
                    if (item != null)
                    {
                        // Seçilen tüm adedi kullan
                        _selectedMevcutStoklar[item.CuttingId] = item.KalanAdet;
                    }
                }
            }
        }

        private void UpdateKullanilacakMevcutStok()
        {
            int toplam = _selectedMevcutStoklar.Values.Sum();
            _txtKullanilacakMevcutStok.Text = toplam.ToString();
        }

        private void UpdateYeniKesilecekAdet(Order order)
        {
            // Artık kullanıcı adeti manuel giriyor, kg otomatik hesaplanıyor
            // Bu metod artık sadece adet değiştiğinde kg'yi güncellemek için kullanılıyor
            try
            {
                if (!string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text))
                {
                    CalculateKgFromAdet();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Yeni kesilecek adet güncellenirken hata: {ex.Message}");
            }
        }

        private void ClbMevcutKesilmisStok_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck event'i önce çalışır, bu yüzden async olarak güncelleme yapmalıyız
            this.BeginInvoke((MethodInvoker)delegate
            {
                UpdateSelectedMevcutStoklar();
                UpdateKullanilacakMevcutStok();
                UpdateYeniKesilecekAdet(_orderRepository.GetById(_orderId));
                UpdateBilgilendirme(_orderRepository.GetById(_orderId));
            });
        }

        private void LoadMevcutKesilmisStoklar(Order order)
        {
            try
            {
                _clbMevcutKesilmisStok.Items.Clear();
                _selectedMevcutStoklar.Clear();

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
                    // Kullanılan plaka adedini hesapla (pres işlemlerinde kullanılan)
                    var usedPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                        .Sum(p => p.PressCount);
                    
                    int kalanPlakaAdedi = cutting.PlakaAdedi - usedPlakaAdedi;
                    
                    if (kalanPlakaAdedi > 0)
                    {
                        var orderInfo = cutting.OrderId.HasValue ? _orderRepository.GetById(cutting.OrderId.Value) : null;
                        string orderNo = orderInfo?.TrexOrderNo ?? "-";
                        
                        string displayText = $"Kesim #{cutting.CuttingDate:dd.MM.yyyy} - Sipariş: {orderNo} - {kalanPlakaAdedi} adet kalan";
                        _clbMevcutKesilmisStok.Items.Add(new CuttingStockItem 
                        { 
                            CuttingId = cutting.Id,
                            Cutting = cutting,
                            KalanAdet = kalanPlakaAdedi,
                            DisplayText = displayText
                        }, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mevcut kesilmiş stoklar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class CuttingStockItem
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

        private void CalculateRemainingKg()
        {
            try
            {
                if (_txtTotalKg == null || _txtCutKg == null || _txtRemainingKg == null)
                    return;

                if (decimal.TryParse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalKg))
                {
                    decimal cutKg = 0;
                    decimal.TryParse(_txtCutKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out cutKg);
                    
                    // Kalan Kg = Toplam Kg - Toplam Kesilen Kg
                    decimal remainingKg = totalKg - cutKg;
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
                var cutting = new Cutting
                {
                    OrderId = _orderId,
                    Hatve = decimal.Parse(_txtHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(_txtSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    SerialNoId = _cmbSerialNo.SelectedItem != null ? GetSelectedId(_cmbSerialNo) : (Guid?)null,
                    TotalKg = decimal.Parse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    CutKg = decimal.Parse(_txtCutKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    CuttingCount = 1, // Artık kesim adedi kullanılmıyor, her kayıt bir kesim işlemi
                    PlakaAdedi = int.TryParse(_txtYeniKesilecekAdet.Text, out int plakaAdedi) ? plakaAdedi : 0,
                    WasteKg = decimal.Parse("0"),
                    RemainingKg = decimal.Parse(_txtRemainingKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    CuttingDate = DateTime.Now
                };

                _cuttingRepository.Insert(cutting);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Yeni kesilecek adet kontrolü (kullanıcı girer)
            if (string.IsNullOrWhiteSpace(_txtYeniKesilecekAdet.Text) || !int.TryParse(_txtYeniKesilecekAdet.Text, out int yeniKesilecekAdet) || yeniKesilecekAdet <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir yeni kesilecek plaka adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Toplam kesilen kg kontrolü (otomatik hesaplanır ama kontrol edelim)
            if (string.IsNullOrWhiteSpace(_txtCutKg.Text) || !decimal.TryParse(_txtCutKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cutKg) || cutKg <= 0)
            {
                MessageBox.Show("Kesilen kg hesaplanamadı. Lütfen bir plaka ağırlığının doğru hesaplandığından emin olun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (decimal.TryParse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalKg) && cutKg > totalKg)
            {
                MessageBox.Show("Kesilen kg, toplam kg'dan fazla olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
