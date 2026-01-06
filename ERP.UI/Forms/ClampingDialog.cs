using System;
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
    public partial class ClampingDialog : Form
    {
        private ComboBox _cmbPlateThickness;
        private ComboBox _cmbHatve;
        private ComboBox _cmbSize;
        private TextBox _txtLength;
        private TextBox _txtResultedCount; // Kaç tane preslenmiş oluşacağı
        private ComboBox _cmbSerialNo;
        private ComboBox _cmbPressing;
        private ComboBox _cmbMachine;
        private TextBox _txtUsedPlateCount; // Kullanılacak Preslenmiş Adet (otomatik hesaplanacak)
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
            _cmbHatve.SelectedIndexChanged += (s, e) => CalculateUsedPlateCount(); // Hatve değiştiğinde hesaplamayı tetikle
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
            _cmbSize.SelectedIndexChanged += (s, e) => CalculateUsedPlateCount(); // Size değiştiğinde hesaplamayı tetikle
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
            _txtLength.TextChanged += TxtLength_TextChanged;
            yPos += spacing;

            // Adet (Kaç tane preslenmiş oluşacağı)
            var lblResultedCount = new Label
            {
                Text = "Adet:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtResultedCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            _txtResultedCount.TextChanged += TxtResultedCount_TextChanged;
            this.Controls.Add(lblResultedCount);
            this.Controls.Add(_txtResultedCount);
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

            // Kullanılacak Preslenmiş Adet (Otomatik hesaplanacak)
            var lblUsedPlateCount = new Label
            {
                Text = "Kullanılacak Preslenmiş Adet:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtUsedPlateCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 240, 248, 255)
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

                // Farklı Hatve değerlerini al ve H, D, M, L olarak göster
                var hatveValues = allPressings
                    .Select(p => p.Hatve)
                    .Distinct()
                    .OrderBy(h => h)
                    .ToList();
                _cmbHatve.Items.Clear();
                foreach (var hatve in hatveValues)
                {
                    // Hatve değerini harfe çevir (gösterim için)
                    string hatveDisplay = GetHatveLetter(hatve);
                    // Değer olarak sayısal hatve'yi sakla (obj olarak)
                    _cmbHatve.Items.Add(new { Display = hatveDisplay, Value = hatve });
                }
                _cmbHatve.DisplayMember = "Display";
                _cmbHatve.ValueMember = "Value";

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
                if (_cmbHatve.SelectedItem == null ||
                    _cmbSize.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbSize.SelectedItem.ToString()) ||
                    _cmbPlateThickness.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbPlateThickness.SelectedItem.ToString()))
                {
                    // Filtreleme kriterleri seçilmemiş, preslenmiş stokları gösterme
                    _cmbPressing.Enabled = false;
                    return;
                }

                decimal hatve = GetHatveValue(_cmbHatve);
                if (hatve == 0 ||
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
                    // Tamamlanmış kenetleme taleplerinden kullanılanları hesapla
                    var completedUsedPlateCount = _clampingRequestRepository.GetAll()
                        .Where(cr => cr.PressingId == pressing.Id && cr.IsActive && cr.Status == "Tamamlandı")
                        .Sum(cr => cr.ActualClampCount ?? cr.RequestedClampCount);
                    
                    // Beklemede olan kenetleme taleplerinden rezerve edilenleri hesapla (RequestedClampCount)
                    var pendingReservedPlateCount = _clampingRequestRepository.GetAll()
                        .Where(cr => cr.PressingId == pressing.Id && cr.IsActive && cr.Status != "Tamamlandı" && cr.Status != "İptal")
                        .Sum(cr => cr.RequestedClampCount);
                    
                    // NOT: Clamping kayıtlarını saymıyoruz çünkü onlar zaten ClampingRequest'lerden oluşturuluyor (çift sayım olur)
                    // Eğer eski sistemden kalma Clamping kayıtları varsa (PressingId olmayan), onlar farklı bir mantıkla işlenmeli
                    
                    var totalUsedCount = completedUsedPlateCount + pendingReservedPlateCount;
                    var availablePlateCount = pressing.PressCount - totalUsedCount;
                    
                    // Kalan stok varsa göster (kısmen kullanılmış olsa bile)
                    if (availablePlateCount > 0)
                    {
                        // Sipariş bilgisini al
                        var order = pressing.OrderId.HasValue ? _orderRepository.GetById(pressing.OrderId.Value) : null;
                        string orderInfo = order != null ? $" - {order.TrexOrderNo}" : "";
                        
                        _cmbPressing.Items.Add(new 
                        { 
                            Id = pressing.Id, 
                            DisplayText = $"Pres #{orderInfo} - {pressing.PressCount} adet (Kalan: {availablePlateCount})",
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
                Guid orderId = pressing?.OrderId ?? _orderId;
                if (orderId == Guid.Empty && _orderId != Guid.Empty)
                    orderId = _orderId;
                
                var clampingRequest = new ClampingRequest
                {
                    OrderId = orderId,
                    PressingId = pressingId,
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Hatve = GetHatveValue(_cmbHatve),
                    Size = decimal.Parse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Length = decimal.Parse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    SerialNoId = pressing?.SerialNoId,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedClampCount = int.Parse(_txtUsedPlateCount.Text), // Kullanılacak Preslenmiş Adet (otomatik hesaplanan)
                    ResultedClampCount = int.Parse(_txtResultedCount.Text), // Adet (kullanıcının girdiği)
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede",
                    RequestDate = DateTime.Now
                };

                var clampingRequestId = _clampingRequestRepository.Insert(clampingRequest);
                
                // Event feed kaydı ekle
                if (orderId != Guid.Empty)
                {
                    var orderRepository = new OrderRepository();
                    var orderForEvent = orderRepository.GetById(orderId);
                    if (orderForEvent != null)
                    {
                        EventFeedService.ClampingRequestCreated(clampingRequestId, orderId, orderForEvent.TrexOrderNo);
                    }
                }
                
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

            if (_cmbHatve.SelectedItem == null || GetHatveValue(_cmbHatve) == 0)
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

            if (string.IsNullOrWhiteSpace(_txtResultedCount.Text) || !int.TryParse(_txtResultedCount.Text, out int resultedCount) || resultedCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir adet giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtUsedPlateCount.Text) || !int.TryParse(_txtUsedPlateCount.Text, out int requestedClampCount) || requestedClampCount <= 0)
            {
                MessageBox.Show("Kullanılacak preslenmiş adet hesaplanamadı. Lütfen uzunluk, adet ve hatve değerlerini kontrol ediniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Preslenmiş stok kontrolü - kullanıcı zaten düşmüş halini yazıyor, bu yüzden sadece kontrol ediyoruz, düşme yapmıyoruz
            // Not: Stok düşmesi ProductionDetailForm'da kenetleme onaylanırken yapılıyor
            var pressingId = GetSelectedId(_cmbPressing);
            var pressing = _pressingRepository.GetAll().FirstOrDefault(p => p.Id == pressingId);
            if (pressing != null)
            {
                // Tamamlanmış kenetleme taleplerinden kullanılanları hesapla
                var completedUsedPlateCount = _clampingRequestRepository.GetAll()
                    .Where(cr => cr.PressingId == pressingId && cr.IsActive && cr.Status == "Tamamlandı")
                    .Sum(cr => cr.ActualClampCount ?? cr.RequestedClampCount);
                
                // Beklemede olan kenetleme taleplerinden rezerve edilenleri hesapla (RequestedClampCount)
                var pendingReservedPlateCount = _clampingRequestRepository.GetAll()
                    .Where(cr => cr.PressingId == pressingId && cr.IsActive && cr.Status != "Tamamlandı" && cr.Status != "İptal")
                    .Sum(cr => cr.RequestedClampCount);
                
                // NOT: Clamping kayıtlarını saymıyoruz çünkü onlar zaten ClampingRequest'lerden oluşturuluyor (çift sayım olur)
                
                var totalUsedCount = completedUsedPlateCount + pendingReservedPlateCount;
                var availablePlateCount = pressing.PressCount - totalUsedCount;
                
                // Kullanıcı zaten düşmüş halini yazıyor (RequestedClampCount), bu yüzden sadece kontrol ediyoruz
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

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerini "6.5(M)" formatında göster: sayısal değer + harf
            const decimal tolerance = 0.1m;
            string letter = "";
            
            if (Math.Abs(hatveValue - 3.25m) < tolerance || Math.Abs(hatveValue - 3.10m) < tolerance)
                letter = "H";
            else if (Math.Abs(hatveValue - 4.5m) < tolerance || Math.Abs(hatveValue - 4.3m) < tolerance)
                letter = "D";
            else if (Math.Abs(hatveValue - 6.5m) < tolerance || Math.Abs(hatveValue - 6.3m) < tolerance || Math.Abs(hatveValue - 6.4m) < tolerance)
                letter = "M";
            else if (Math.Abs(hatveValue - 9m) < tolerance || Math.Abs(hatveValue - 8.7m) < tolerance || Math.Abs(hatveValue - 8.65m) < tolerance)
                letter = "L";
            
            // Format: 6.5(M) veya sadece sayısal değer (harf bulunamazsa)
            if (!string.IsNullOrEmpty(letter))
                return $"{hatveValue.ToString("F2", CultureInfo.InvariantCulture)}({letter})";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture); // Eğer tanınmazsa sadece sayısal göster
        }

        private decimal GetHatveValue(ComboBox cmbHatve)
        {
            if (cmbHatve.SelectedItem == null)
                return 0;

            string selectedItemStr = cmbHatve.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(selectedItemStr) || selectedItemStr == "")
                return 0;

            // Eğer obje ise Value property'sinden al
            var valueProperty = cmbHatve.SelectedItem.GetType().GetProperty("Value");
            if (valueProperty != null)
            {
                return (decimal)valueProperty.GetValue(cmbHatve.SelectedItem);
            }

            // Eğer string ise parse et (geriye dönük uyumluluk için)
            if (decimal.TryParse(selectedItemStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve))
            {
                return hatve;
            }

            return 0;
        }

        private void TxtLength_TextChanged(object sender, EventArgs e)
        {
            CalculateUsedPlateCount();
        }

        private void TxtResultedCount_TextChanged(object sender, EventArgs e)
        {
            CalculateUsedPlateCount();
        }

        private void CalculateUsedPlateCount()
        {
            try
            {
                // Uzunluk ve Adet girilmiş mi kontrol et
                if (string.IsNullOrWhiteSpace(_txtLength.Text) || string.IsNullOrWhiteSpace(_txtResultedCount.Text))
                {
                    _txtUsedPlateCount.Text = "";
                    return;
                }

                if (!decimal.TryParse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
                {
                    _txtUsedPlateCount.Text = "";
                    return;
                }

                if (!int.TryParse(_txtResultedCount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int resultedCount) || resultedCount <= 0)
                {
                    _txtUsedPlateCount.Text = "";
                    return;
                }

                // Hatve değerini al
                decimal hatve = GetHatveValue(_cmbHatve);
                if (hatve == 0)
                {
                    _txtUsedPlateCount.Text = "";
                    return;
                }

                // Size (plaka ölçüsü) al - dinamik hatve hesaplama için
                decimal plakaOlcusuCM = 0;
                if (_cmbSize.SelectedItem != null && 
                    decimal.TryParse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out plakaOlcusuCM))
                {
                    // Hatve tipini belirle
                    string hatveLetter = GetHatveLetter(hatve);
                    if (!string.IsNullOrEmpty(hatveLetter) && (hatveLetter == "H" || hatveLetter == "D" || hatveLetter == "M" || hatveLetter == "L"))
                    {
                        char hatveTipi = hatveLetter[0];
                        // Dinamik hatve hesapla (rapor tarafındaki gibi)
                        var hatveOlcumu = GetHatveOlcumu(hatveTipi, plakaOlcusuCM);
                        if (hatveOlcumu.HasValue)
                        {
                            hatve = hatveOlcumu.Value;
                        }
                    }
                }

                // Hesaplama: (Uzunluk / Hatve) yukarı yuvarla * Adet
                decimal birTaneIcinDeger = length / hatve;
                int birTaneIcinYuvarlanmis = (int)Math.Ceiling(birTaneIcinDeger);
                int kullanilacakAdet = birTaneIcinYuvarlanmis * resultedCount;

                _txtUsedPlateCount.Text = kullanilacakAdet.ToString();
            }
            catch
            {
                _txtUsedPlateCount.Text = "";
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
    }
}

