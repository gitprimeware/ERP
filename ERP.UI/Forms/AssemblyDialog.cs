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
    public partial class AssemblyDialog : Form
    {
        private ComboBox _cmbPlateThickness;
        private ComboBox _cmbHatve;
        private ComboBox _cmbSize;
        private TextBox _txtLength;
        private ComboBox _cmbSerialNo;
        private ComboBox _cmbClamping;
        private ComboBox _cmbMachine;
        private TextBox _txtRequestedAssemblyCount;
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private MachineRepository _machineRepository;
        private ClampingRepository _clampingRepository;
        private AssemblyRepository _assemblyRepository;
        private AssemblyRequestRepository _assemblyRequestRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;

        public AssemblyDialog(SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, 
            MachineRepository machineRepository, Guid orderId)
        {
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _machineRepository = machineRepository;
            _clampingRepository = new ClampingRepository();
            _assemblyRepository = new AssemblyRepository();
            _assemblyRequestRepository = new AssemblyRequestRepository();
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Montaj Yap";
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

            // Kenetlenmiş Plaka Seçimi (Önce seçilecek, sonra değerler otomatik gelecek)
            var lblClamping = new Label
            {
                Text = "Kenetlenmiş Plaka:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbClamping = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbClamping.SelectedIndexChanged += CmbClamping_SelectedIndexChanged;
            this.Controls.Add(lblClamping);
            this.Controls.Add(_cmbClamping);
            yPos += spacing;

            // Plaka Kalınlığı (ReadOnly - kenetlenmiş plakadan otomatik gelecek)
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
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPlateThickness);
            this.Controls.Add(_cmbPlateThickness);
            yPos += spacing;

            // Hatve (ReadOnly - kenetlenmiş plakadan otomatik gelecek)
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
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblHatve);
            this.Controls.Add(_cmbHatve);
            yPos += spacing;

            // Ölçü (ReadOnly - kenetlenmiş plakadan otomatik gelecek)
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
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblSize);
            this.Controls.Add(_cmbSize);
            yPos += spacing;

            // Uzunluk (ReadOnly - kenetlenmiş plakadan otomatik gelecek)
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

            // Montajlanacak Kenet Sayısı (Mühendis tarafından girilecek)
            var lblRequestedAssemblyCount = new Label
            {
                Text = "Montajlanacak Kenet Sayısı:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtRequestedAssemblyCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblRequestedAssemblyCount);
            this.Controls.Add(_txtRequestedAssemblyCount);
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
                
                // Siparişten bilgileri al ve form alanlarını doldur
                LoadOrderDataToForm();
                
                // Sipariş bilgilerine göre kenetlenmiş plakaları yükle
                LoadClampingsBasedOnOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderDataToForm()
        {
            try
            {
                // Siparişten bilgileri al
                var order = _orderRepository.GetById(_orderId);
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                {
                    MessageBox.Show("Sipariş bulunamadı veya ürün kodu eksik!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var parts = order.ProductCode.Split('-');
                
                // Size (Ölçü) bilgisini siparişten al
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    // Plaka ölçüsü com (mm): <= 1150 ise aynı, > 1150 ise /2
                    int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                    decimal sizeCM = plakaOlcusuComMM / 10.0m;
                    
                    // ComboBox'a tüm mevcut size değerlerini ekle ve default'u seç
                    var allSizes = _clampingRepository.GetAll()
                        .Select(c => c.Size)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();
                    _cmbSize.Items.Clear();
                    foreach (var s in allSizes)
                    {
                        _cmbSize.Items.Add(s.ToString("F2", CultureInfo.InvariantCulture));
                    }
                    
                    var sizeText = sizeCM.ToString("F2", CultureInfo.InvariantCulture);
                    if (_cmbSize.Items.Contains(sizeText))
                        _cmbSize.SelectedItem = sizeText;
                    else
                        _cmbSize.Text = sizeText;
                }
                
                // Hatve bilgisini siparişten al
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2];
                    if (modelProfile.Length > 0)
                    {
                        char modelLetter = modelProfile[0];
                        decimal hatve = GetHtave(modelLetter);
                        
                        // ComboBox'a tüm mevcut hatve değerlerini ekle ve default'u seç
                        var allHatves = _clampingRepository.GetAll()
                            .Select(c => c.Hatve)
                            .Distinct()
                            .OrderBy(h => h)
                            .ToList();
                        _cmbHatve.Items.Clear();
                        foreach (var h in allHatves)
                        {
                            _cmbHatve.Items.Add(h.ToString("F2", CultureInfo.InvariantCulture));
                        }
                        
                        var hatveText = hatve.ToString("F2", CultureInfo.InvariantCulture);
                        if (_cmbHatve.Items.Contains(hatveText))
                            _cmbHatve.SelectedItem = hatveText;
                        else
                            _cmbHatve.Text = hatveText;
                    }
                }
                
                // Plaka Kalınlığı (Lamel Kalınlığı)
                if (order.LamelThickness.HasValue)
                {
                    // ComboBox'a tüm mevcut plaka kalınlığı değerlerini ekle ve default'u seç
                    var allPlateThicknesses = _clampingRepository.GetAll()
                        .Select(c => c.PlateThickness)
                        .Distinct()
                        .OrderBy(pt => pt)
                        .ToList();
                    _cmbPlateThickness.Items.Clear();
                    foreach (var pt in allPlateThicknesses)
                    {
                        _cmbPlateThickness.Items.Add(pt.ToString("F3", CultureInfo.InvariantCulture));
                    }
                    
                    var plateThicknessText = order.LamelThickness.Value.ToString("F3", CultureInfo.InvariantCulture);
                    if (_cmbPlateThickness.Items.Contains(plateThicknessText))
                        _cmbPlateThickness.SelectedItem = plateThicknessText;
                    else
                        _cmbPlateThickness.Text = plateThicknessText;
                }
                
                // Uzunluk bilgisini siparişten al (MM olarak göster)
                if (parts.Length >= 5 && int.TryParse(parts[4], out int yukseklikMM))
                {
                    // Yükseklik com: <= 1800 ise aynı, > 1800 ise /2
                    int yukseklikComMM = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    // MM olarak göster (CM'ye çevirmeden direkt MM)
                    _txtLength.Text = yukseklikComMM.ToString("F2", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş bilgileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private int GetKapakBoyuFromOrder()
        {
            var order = _orderRepository.GetById(_orderId);
            if (order == null || string.IsNullOrEmpty(order.ProductCode))
                return 0;

            var parts = order.ProductCode.Split('-');
            if (parts.Length > 5)
            {
                string kapakDegeri = parts[5];
                
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
            
            return 0;
        }

        private void LoadClampingsBasedOnOrder()
        {
            try
            {
                _cmbClamping.Items.Clear();
                
                // Form alanlarından filtreleme değerlerini al
                if (string.IsNullOrWhiteSpace(_cmbHatve.Text) || 
                    string.IsNullOrWhiteSpace(_cmbSize.Text) || 
                    string.IsNullOrWhiteSpace(_cmbPlateThickness.Text) ||
                    string.IsNullOrWhiteSpace(_txtLength.Text))
                {
                    _cmbClamping.Enabled = false;
                    return;
                }

                if (!decimal.TryParse(_cmbHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve) ||
                    !decimal.TryParse(_cmbSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) ||
                    !decimal.TryParse(_cmbPlateThickness.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness) ||
                    !decimal.TryParse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lengthMM))
                {
                    _cmbClamping.Enabled = false;
                    return;
                }

                // Uzunluk filtresi: Ürün uzunluğundan kapak boyu çıkarılacak
                int kapakBoyuMM = GetKapakBoyuFromOrder();
                decimal kapaksizUzunlukMM = lengthMM - kapakBoyuMM;
                decimal kapaksizUzunlukCM = kapaksizUzunlukMM / 10.0m; // MM'yi CM'ye çevir

                // TÜM kenetlenmiş stokları yükle (sadece belirli bir siparişe ait değil, stoktan da kullanılabilir)
                var allClampings = _clampingRepository.GetAll();
                
                // Filtreleme: Hatve, Ölçü, Plaka Kalınlığı, Uzunluk (kapaksız uzunluk ile karşılaştırılacak)
                var filteredClampings = allClampings.Where(c => 
                    c.ClampCount > 0 && 
                    c.IsActive &&
                    Math.Abs(c.Hatve - hatve) < 0.01m &&
                    Math.Abs(c.Size - size) < 0.1m &&
                    Math.Abs(c.PlateThickness - plateThickness) < 0.001m &&
                    Math.Abs(c.Length - kapaksizUzunlukMM) < 0.1m); // CM cinsinden tolerance (0.1cm = 1mm)
                
                var filteredList = filteredClampings.OrderByDescending(c => c.ClampingDate).ToList();
                
                if (!filteredList.Any())
                {
                    _cmbClamping.Enabled = false;
                    return;
                }
                
                foreach (var clamping in filteredList)
                {
                    // Daha önce montajda kullanılan kenet adedini hesapla
                    // Hem eski Assembly kayıtlarından hem de tamamlanmış AssemblyRequest'lerden
                    var usedClampCountFromAssembly = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    var usedClampCountFromRequests = _assemblyRequestRepository.GetAll()
                        .Where(ar => ar.ClampingId == clamping.Id && ar.IsActive && ar.Status == "Tamamlandı")
                        .Sum(ar => ar.ActualClampCount ?? ar.RequestedAssemblyCount);
                    
                    var totalUsedCount = usedClampCountFromAssembly + usedClampCountFromRequests;
                    var availableClampCount = clamping.ClampCount - totalUsedCount;
                    
                    if (availableClampCount > 0)
                    {
                        var order = clamping.OrderId.HasValue ? _orderRepository.GetById(clamping.OrderId.Value) : null;
                        string orderInfo = order != null ? $" - {order.TrexOrderNo}" : " - Stok";
                        
                        _cmbClamping.Items.Add(new 
                        { 
                            Id = clamping.Id, 
                            DisplayText = $"Kenet #{orderInfo} - {clamping.ClampCount} adet (Kalan: {availableClampCount})",
                            Clamping = clamping
                        });
                    }
                }
                _cmbClamping.DisplayMember = "DisplayText";
                _cmbClamping.ValueMember = "Id";
                
                // Combo box'u aktif et (en az bir öğe varsa)
                _cmbClamping.Enabled = _cmbClamping.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş stoklar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // FilterClampings metodu kaldırıldı - artık kenetlenmiş plaka seçilince değerler otomatik geliyor
        private void FilterClampings_Removed(object sender, EventArgs e)
        {
            try
            {
                _cmbClamping.Items.Clear();
                
                // Hatve, Ölçü, Plaka Kalınlığı seçildiyse filtreleme yap
                if (_cmbHatve.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbHatve.SelectedItem.ToString()) ||
                    _cmbSize.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbSize.SelectedItem.ToString()) ||
                    _cmbPlateThickness.SelectedItem == null || string.IsNullOrWhiteSpace(_cmbPlateThickness.SelectedItem.ToString()))
                {
                    // Filtreleme kriterleri seçilmemiş, kenetlenmiş stokları gösterme
                    _cmbClamping.Enabled = false;
                    return;
                }

                if (!decimal.TryParse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve) ||
                    !decimal.TryParse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) ||
                    !decimal.TryParse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness))
                {
                    _cmbClamping.Enabled = false;
                    return;
                }

                // TÜM kenetlenmiş stokları yükle (sadece belirli bir siparişe ait değil, stoktan da kullanılabilir)
                var allClampings = _clampingRepository.GetAll();
                
                // Filtreleme: Hatve, Ölçü, Plaka Kalınlığı - tam eşleşme (combo box'dan seçildiği için)
                var filteredClampings = allClampings.Where(c => 
                    c.ClampCount > 0 && 
                    c.IsActive &&
                    Math.Abs(c.Hatve - hatve) < 0.01m &&
                    Math.Abs(c.Size - size) < 0.1m &&
                    Math.Abs(c.PlateThickness - plateThickness) < 0.001m);
                
                var filteredList = filteredClampings.OrderByDescending(c => c.ClampingDate).ToList();
                
                if (!filteredList.Any())
                {
                    _cmbClamping.Enabled = false;
                    return;
                }
                
                foreach (var clamping in filteredList)
                {
                    // Daha önce montajda kullanılan kenet adedini hesapla
                    // Hem eski Assembly kayıtlarından hem de tamamlanmış AssemblyRequest'lerden
                    var usedClampCountFromAssembly = _assemblyRepository.GetAll()
                        .Where(a => a.ClampingId == clamping.Id && a.IsActive)
                        .Sum(a => a.UsedClampCount);
                    
                    var usedClampCountFromRequests = _assemblyRequestRepository.GetAll()
                        .Where(ar => ar.ClampingId == clamping.Id && ar.IsActive && ar.Status == "Tamamlandı")
                        .Sum(ar => ar.ActualClampCount ?? ar.RequestedAssemblyCount);
                    
                    var totalUsedCount = usedClampCountFromAssembly + usedClampCountFromRequests;
                    var availableClampCount = clamping.ClampCount - totalUsedCount;
                    
                    if (availableClampCount > 0)
                    {
                        var order = clamping.OrderId.HasValue ? _orderRepository.GetById(clamping.OrderId.Value) : null;
                        string orderInfo = order != null ? $" - {order.TrexOrderNo}" : " - Stok";
                        
                        _cmbClamping.Items.Add(new 
                        { 
                            Id = clamping.Id, 
                            DisplayText = $"Kenet #{orderInfo} - {clamping.ClampCount} adet (Kalan: {availableClampCount})",
                            Clamping = clamping
                        });
                    }
                }
                _cmbClamping.DisplayMember = "DisplayText";
                _cmbClamping.ValueMember = "Id";
                
                // Combo box'u aktif et (en az bir öğe varsa)
                _cmbClamping.Enabled = _cmbClamping.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş stoklar filtrelenirken hata oluştu: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Kenetlenmiş plaka seçimi artık sadece seçim yapıyor, değerleri değiştirmiyor
        private void CmbClamping_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kenetlenmiş plaka seçildiğinde sadece Rulo Seri No'yu güncelle
            // Diğer değerler (Hatve, Ölçü, Plaka Kalınlığı, Uzunluk) Order'dan geliyor
            if (_cmbClamping.SelectedItem == null)
            {
                _cmbSerialNo.SelectedItem = null;
                return;
            }

            try
            {
                var clampingProperty = _cmbClamping.SelectedItem.GetType().GetProperty("Clamping");
                if (clampingProperty == null)
                    return;

                var clamping = clampingProperty.GetValue(_cmbClamping.SelectedItem) as Clamping;
                if (clamping == null)
                    return;

                // Rulo Seri No'yu doldur (kenetlenmiş plakadan)
                if (clamping.SerialNoId.HasValue)
                {
                    foreach (var item in _cmbSerialNo.Items)
                    {
                        var itemIdProperty = item.GetType().GetProperty("Id");
                        if (itemIdProperty != null && itemIdProperty.GetValue(item).Equals(clamping.SerialNoId.Value))
                        {
                            _cmbSerialNo.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    _cmbSerialNo.SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenet bilgileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var clampingId = GetSelectedId(_cmbClamping);
                var clamping = _clampingRepository.GetAll().FirstOrDefault(c => c.Id == clampingId);

                // OrderId: Dialog hangi sipariş için açıldıysa o siparişe ait olmalı
                // Stoktan kenetlenmiş plaka kullanılsa bile, montaj talebi açıldığı siparişe bağlı olmalı
                var orderId = _orderId != Guid.Empty ? _orderId : (Guid?)null;
                
                var assemblyRequest = new AssemblyRequest
                {
                    OrderId = orderId, // Dialog hangi sipariş için açıldıysa o siparişe ait
                    ClampingId = clampingId,
                    PlateThickness = decimal.Parse(_cmbPlateThickness.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    Hatve = decimal.Parse(_cmbHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(_cmbSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    // Uzunluk MM olarak giriliyor, CM'ye çevirip kaydet (veritabanında CM olarak saklanıyor)
                    Length = decimal.Parse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture) / 10.0m,
                    SerialNoId = clamping?.SerialNoId,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedAssemblyCount = int.Parse(_txtRequestedAssemblyCount.Text),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede",
                    RequestDate = DateTime.Now
                };
                _assemblyRequestRepository.Insert(assemblyRequest);
                MessageBox.Show("Montaj talebi başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            // Kenetlenmiş plaka seçimi zorunlu (diğer değerler bundan otomatik gelecek)
            if (_cmbClamping.SelectedItem == null)
            {
                MessageBox.Show("Lütfen kenetlenmiş plaka seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Plaka Kalınlığı kontrolü (Text veya SelectedItem olabilir - DropDown style)
            if (string.IsNullOrWhiteSpace(_cmbPlateThickness.Text))
            {
                MessageBox.Show("Lütfen plaka kalınlığı giriniz veya seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_cmbPlateThickness.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness) || plateThickness <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir plaka kalınlığı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Hatve kontrolü (Text veya SelectedItem olabilir - DropDown style)
            if (string.IsNullOrWhiteSpace(_cmbHatve.Text))
            {
                MessageBox.Show("Lütfen hatve giriniz veya seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_cmbHatve.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve) || hatve <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir hatve giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Ölçü kontrolü (Text veya SelectedItem olabilir - DropDown style)
            if (string.IsNullOrWhiteSpace(_cmbSize.Text))
            {
                MessageBox.Show("Lütfen ölçü giriniz veya seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_cmbSize.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) || size <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir ölçü giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtLength.Text) || !decimal.TryParse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
            {
                MessageBox.Show("Uzunluk bilgisi yüklenemedi. Lütfen tekrar deneyiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtRequestedAssemblyCount.Text) || !int.TryParse(_txtRequestedAssemblyCount.Text, out int requestedAssemblyCount) || requestedAssemblyCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir montajlanacak kenet sayısı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Kenetlenmiş stok kontrolü
            var clampingId = GetSelectedId(_cmbClamping);
            var clamping = _clampingRepository.GetAll().FirstOrDefault(c => c.Id == clampingId);
            if (clamping != null)
            {
                // Tamamlanmış montaj taleplerinden kullanılanları hesapla
                var usedClampCountFromRequests = _assemblyRequestRepository.GetAll()
                    .Where(ar => ar.ClampingId == clampingId && ar.IsActive && ar.Status == "Tamamlandı")
                    .Sum(ar => ar.ActualClampCount ?? ar.RequestedAssemblyCount);
                
                // Eski Assembly kayıtlarından da kullanılanları hesapla
                var oldUsedClampCount = _assemblyRepository.GetAll()
                    .Where(a => a.ClampingId == clampingId && a.IsActive)
                    .Sum(a => a.UsedClampCount);
                
                var totalUsedCount = usedClampCountFromRequests + oldUsedClampCount;
                var availableClampCount = clamping.ClampCount - totalUsedCount;
                
                if (requestedAssemblyCount > availableClampCount)
                {
                    MessageBox.Show($"Montajlanacak kenet sayısı kalan kenetlenmiş plaka adedinden fazla olamaz! (Kalan: {availableClampCount}, İstenen: {requestedAssemblyCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
