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
    public partial class ClampingDialog : Form
    {
        private ComboBox _cmbPlateThickness;
        private ComboBox _cmbHatve;
        private ComboBox _cmbSize;
        private TextBox _txtLength;
        private ComboBox _cmbSerialNo;
        private ComboBox _cmbPressing;
        private ComboBox _cmbMachine;
        private TextBox _txtUsedPlateCount;
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private MachineRepository _machineRepository;
        private PressingRepository _pressingRepository;
        private ClampingRepository _clampingRepository;
        private ClampingRequestRepository _clampingRequestRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;

        public ClampingDialog(SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, 
            MachineRepository machineRepository, PressingRepository pressingRepository, Guid orderId)
        {
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _machineRepository = machineRepository;
            _pressingRepository = pressingRepository;
            _clampingRepository = new ClampingRepository();
            _clampingRequestRepository = new ClampingRequestRepository();
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Kenetleme Yap";
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

            // Preslenmiş Plaka Seçimi (Filtrelenmiş)
            var lblPressing = new Label
            {
                Text = "Preslenmiş Plaka:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPressing = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false  // Başlangıçta devre dışı, filtreleme kriterleri girildikten sonra aktif olacak
            };
            _cmbPressing.SelectedIndexChanged += CmbPressing_SelectedIndexChanged;
            this.Controls.Add(lblPressing);
            this.Controls.Add(_cmbPressing);
            yPos += spacing;

            // Plaka Kalınlığı (ComboBox - tablodaki değerlerden)
            var lblPlateThickness = new Label
            {
                Text = "Plaka Kalınlığı:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPlateThickness = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPlateThickness.SelectedIndexChanged += FilterPressings;
            this.Controls.Add(lblPlateThickness);
            this.Controls.Add(_cmbPlateThickness);
            yPos += spacing;

            // Hatve (ComboBox - tablodaki değerlerden)
            var lblHatve = new Label
            {
                Text = "Hatve:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbHatve = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbHatve.SelectedIndexChanged += FilterPressings;
            this.Controls.Add(lblHatve);
            this.Controls.Add(_cmbHatve);
            yPos += spacing;

            // Ölçü (ComboBox - tablodaki değerlerden)
            var lblSize = new Label
            {
                Text = "Ölçü:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize.SelectedIndexChanged += FilterPressings;
            this.Controls.Add(lblSize);
            this.Controls.Add(_cmbSize);
            yPos += spacing;

            // Uzunluk
            var lblLength = new Label
            {
                Text = "Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtLength = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblLength);
            this.Controls.Add(_txtLength);
            yPos += spacing;

            // Rulo Seri No (Readonly)
            var lblSerialNo = new Label
            {
                Text = "Rulo Seri No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSerialNo = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            this.Controls.Add(lblSerialNo);
            this.Controls.Add(_cmbSerialNo);
            yPos += spacing;

            // Makina
            var lblMachine = new Label
            {
                Text = "Makina:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbMachine = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblMachine);
            this.Controls.Add(_cmbMachine);
            yPos += spacing;

            // İstenen Kenetleme Adedi (Mühendis tarafından girilecek)
            var lblUsedPlateCount = new Label
            {
                Text = "İstenen Kenetleme Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtUsedPlateCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblUsedPlateCount);
            this.Controls.Add(_txtUsedPlateCount);
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
                // Seri No'ları yükle
                _cmbSerialNo.Items.Clear();
                var serialNos = _serialNoRepository.GetAll();
                foreach (var serialNo in serialNos)
                {
                    _cmbSerialNo.Items.Add(new { Id = serialNo.Id, SerialNumber = serialNo.SerialNumber });
                }
                _cmbSerialNo.DisplayMember = "SerialNumber";
                _cmbSerialNo.ValueMember = "Id";

                // Makinaları yükle
                _cmbMachine.Items.Clear();
                var machines = _machineRepository.GetAll();
                foreach (var machine in machines)
                {
                    _cmbMachine.Items.Add(new { Id = machine.Id, Name = machine.Name });
                }
                _cmbMachine.DisplayMember = "Name";
                _cmbMachine.ValueMember = "Id";

                // Operatörleri yükle
                LoadEmployees();
                
                // Combo box'ları preslenmiş stok tablosundan doldur
                LoadFilterComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilterComboBoxes()
        {
            try
            {
                // Tüm preslenmiş stokları al
                var allPressings = _pressingRepository.GetAll()
                    .Where(p => p.PressCount > 0 && p.IsActive)
                    .ToList();

                // Farklı Hatve değerlerini al
                var hatveValues = allPressings
                    .Select(p => p.Hatve)
                    .Distinct()
                    .OrderBy(h => h)
                    .ToList();
                _cmbHatve.Items.Clear();
                _cmbHatve.Items.Add(""); // Boş seçenek
                foreach (var hatve in hatveValues)
                {
                    _cmbHatve.Items.Add(hatve.ToString("F2", CultureInfo.InvariantCulture));
                }

                // Farklı Size değerlerini al
                var sizeValues = allPressings
                    .Select(p => p.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                _cmbSize.Items.Clear();
                _cmbSize.Items.Add(""); // Boş seçenek
                foreach (var size in sizeValues)
                {
                    _cmbSize.Items.Add(size.ToString("F1", CultureInfo.InvariantCulture));
                }

                // Farklı PlateThickness değerlerini al
                var plateThicknessValues = allPressings
                    .Select(p => p.PlateThickness)
                    .Distinct()
                    .OrderBy(pt => pt)
                    .ToList();
                _cmbPlateThickness.Items.Clear();
                _cmbPlateThickness.Items.Add(""); // Boş seçenek
                foreach (var pt in plateThicknessValues)
                {
                    _cmbPlateThickness.Items.Add(pt.ToString("F3", CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Filtre combo box'ları yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterPressings(object sender, EventArgs e)
        {
            try
            {
                _cmbPressing.Items.Clear();
                
                // Hatve, Ölçü, Plaka Kalınlığı seçildiyse filtreleme yap
                if (_cmbHatve.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbHatve.SelectedItem.ToString()) ||
                    _cmbSize.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbSize.SelectedItem.ToString()) ||
                    _cmbPlateThickness.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbPlateThickness.SelectedItem.ToString()))
                {
                    // Filtreleme kriterleri seçilmemiş, preslenmiş stokları gösterme
                    _cmbPressing.Enabled = false;
                    return;
                }

                if (!decimal.TryParse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve) ||
                    !decimal.TryParse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) ||
                    !decimal.TryParse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness))
                {
                    _cmbPressing.Enabled = false;
                    return;
                }

                // TÜM preslenmiş stokları yükle (sadece belirli bir siparişe ait değil)
                var allPressings = _pressingRepository.GetAll();
                
                // Filtreleme: Hatve, Ölçü, Plaka Kalınlığı - tam eşleşme (combo box'dan seçildiği için)
                var filteredPressings = allPressings.Where(p => 
                    p.PressCount > 0 && 
                    p.IsActive &&
                    Math.Abs(p.Hatve - hatve) < 0.01m &&  // Hatve için küçük tolerance
                    Math.Abs(p.Size - size) < 0.1m &&    // Size için küçük tolerance
                    Math.Abs(p.PlateThickness - plateThickness) < 0.001m);  // PlateThickness için küçük tolerance
                
                var filteredList = filteredPressings.OrderByDescending(p => p.PressingDate).ToList();
                
                if (!filteredList.Any())
                {
                    // Filtre kriterlerine uygun preslenmiş stok bulunamadı
                    // Debug için: tüm preslenmiş stokları listele
                    System.Diagnostics.Debug.WriteLine($"Filtreleme: Hatve={hatve}, Size={size}, PlateThickness={plateThickness}");
                    System.Diagnostics.Debug.WriteLine($"Toplam preslenmiş stok sayısı: {allPressings.Count}");
                    System.Diagnostics.Debug.WriteLine($"Filtreli stok sayısı: {filteredList.Count}");
                    
                    // Kullanıcıya bilgi ver (opsiyonel - sadece debug için)
                    // MessageBox.Show($"Bu kriterlere uygun preslenmiş stok bulunamadı.\n\nHatve: {hatve}\nÖlçü: {size}\nPlaka Kalınlığı: {plateThickness}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                foreach (var pressing in filteredList)
                {
                    // Daha önce kenetlenmiş plaka adedini hesapla (tamamlanmış kenetleme taleplerinden)
                    var usedPlateCount = _clampingRequestRepository.GetAll()
                        .Where(cr => cr.PressingId == pressing.Id && cr.IsActive && cr.Status == "Tamamlandı")
                        .Sum(cr => cr.ActualClampCount ?? cr.RequestedClampCount);
                    
                    // Ayrıca eski Clamping kayıtlarından da kullanılanları hesapla (geriye dönük uyumluluk için)
                    var oldUsedPlateCount = _clampingRepository.GetAll()
                        .Where(c => c.PressingId == pressing.Id && c.IsActive)
                        .Sum(c => c.UsedPlateCount);
                    
                    var totalUsedCount = usedPlateCount + oldUsedPlateCount;
                    var availablePlateCount = pressing.PressCount - totalUsedCount;
                    
                    if (availablePlateCount > 0)
                    {
                        // Sipariş bilgisini al
                        var order = pressing.OrderId.HasValue ? _orderRepository.GetById(pressing.OrderId.Value) : null;
                        string orderInfo = order != null ? $" - {order.TrexOrderNo}" : "";
                        
                        _cmbPressing.Items.Add(new 
                        { 
                            Id = pressing.Id, 
                            DisplayText = $"Pres #{pressing.PressingDate:dd.MM.yyyy}{orderInfo} - {pressing.PressCount} adet (Kalan: {availablePlateCount})",
                            Pressing = pressing
                        });
                    }
                }
                _cmbPressing.DisplayMember = "DisplayText";
                _cmbPressing.ValueMember = "Id";
                
                // Combo box'u aktif et (en az bir öğe varsa)
                _cmbPressing.Enabled = _cmbPressing.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Preslenmiş stoklar filtrelenirken hata oluştu: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPressing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbPressing.SelectedItem == null)
                return;

            try
            {
                var idProperty = _cmbPressing.SelectedItem.GetType().GetProperty("Id");
                if (idProperty == null)
                    return;

                var pressingId = (Guid)idProperty.GetValue(_cmbPressing.SelectedItem);
                var pressing = _pressingRepository.GetAll().FirstOrDefault(p => p.Id == pressingId);

                if (pressing != null)
                {
                    // Rulo Seri No'yu doldur (preslenmiş plakadan)
                    if (pressing.SerialNoId.HasValue)
                    {
                        foreach (var item in _cmbSerialNo.Items)
                        {
                            var itemIdProperty = item.GetType().GetProperty("Id");
                            if (itemIdProperty != null && itemIdProperty.GetValue(item).Equals(pressing.SerialNoId.Value))
                            {
                                _cmbSerialNo.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres bilgileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var pressingId = GetSelectedId(_cmbPressing);
                var pressing = _pressingRepository.GetAll().FirstOrDefault(p => p.Id == pressingId);

                // OrderId'yi preslenmiş plakadan al (eğer yoksa mevcut orderId'yi kullan)
                var orderId = pressing?.OrderId ?? _orderId;
                if (orderId == Guid.Empty && _orderId != Guid.Empty)
                    orderId = _orderId;
                
                var clampingRequest = new ClampingRequest
                {
                    OrderId = orderId,
                    PressingId = pressingId,
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Hatve = decimal.Parse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Length = decimal.Parse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    SerialNoId = pressing?.SerialNoId,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedClampCount = int.Parse(_txtUsedPlateCount.Text), // İstenen Kenetleme Adedi
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede",
                    RequestDate = DateTime.Now
                };

                _clampingRequestRepository.Insert(clampingRequest);
                MessageBox.Show("Kenetleme talebi başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (_cmbPlateThickness.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbPlateThickness.SelectedItem.ToString()))
            {
                MessageBox.Show("Lütfen plaka kalınlığı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbHatve.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbHatve.SelectedItem.ToString()))
            {
                MessageBox.Show("Lütfen hatve seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbSize.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbSize.SelectedItem.ToString()))
            {
                MessageBox.Show("Lütfen ölçü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbPressing.SelectedItem == null)
            {
                MessageBox.Show("Lütfen preslenmiş plaka seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtLength.Text) || !decimal.TryParse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir uzunluk giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtUsedPlateCount.Text) || !int.TryParse(_txtUsedPlateCount.Text, out int requestedClampCount) || requestedClampCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir istenen kenetleme adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Preslenmiş stok kontrolü
            var pressingId = GetSelectedId(_cmbPressing);
            var pressing = _pressingRepository.GetAll().FirstOrDefault(p => p.Id == pressingId);
            if (pressing != null)
            {
                // Tamamlanmış kenetleme taleplerinden kullanılanları hesapla
                var usedPlateCount = _clampingRequestRepository.GetAll()
                    .Where(cr => cr.PressingId == pressingId && cr.IsActive && cr.Status == "Tamamlandı")
                    .Sum(cr => cr.ActualClampCount ?? cr.RequestedClampCount);
                
                // Eski Clamping kayıtlarından da kullanılanları hesapla
                var oldUsedPlateCount = _clampingRepository.GetAll()
                    .Where(c => c.PressingId == pressingId && c.IsActive)
                    .Sum(c => c.UsedPlateCount);
                
                var totalUsedCount = usedPlateCount + oldUsedPlateCount;
                var availablePlateCount = pressing.PressCount - totalUsedCount;
                
                if (requestedClampCount > availablePlateCount)
                {
                    MessageBox.Show($"İstenen kenetleme adedi kalan preslenmiş plaka adedinden fazla olamaz! (Kalan: {availablePlateCount}, İstenen: {requestedClampCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
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

