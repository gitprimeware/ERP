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
            this.Width = 500;
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
            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 300;
            int spacing = 35;

            // Kenetlenmiş Plaka Seçimi (Filtrelenmiş)
            var lblClamping = new Label
            {
                Text = "Kenetlenmiş Plaka:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbClamping = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false  // Başlangıçta devre dışı, filtreleme kriterleri girildikten sonra aktif olacak
            };
            _cmbClamping.SelectedIndexChanged += CmbClamping_SelectedIndexChanged;
            this.Controls.Add(lblClamping);
            this.Controls.Add(_cmbClamping);
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPlateThickness.SelectedIndexChanged += FilterClampings;
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbHatve.SelectedIndexChanged += FilterClampings;
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize.SelectedIndexChanged += FilterClampings;
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
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
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblMachine);
            this.Controls.Add(_cmbMachine);
            yPos += spacing;

            // İstenen Montaj Adedi (Mühendis tarafından girilecek)
            var lblRequestedAssemblyCount = new Label
            {
                Text = "İstenen Montaj Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtRequestedAssemblyCount = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
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
                
                // Combo box'ları kenetlenmiş stok tablosundan doldur
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
                // Tüm kenetlenmiş stokları al
                var allClampings = _clampingRepository.GetAll()
                    .Where(c => c.ClampCount > 0 && c.IsActive)
                    .ToList();

                // Farklı Hatve değerlerini al
                var hatveValues = allClampings
                    .Select(c => c.Hatve)
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
                var sizeValues = allClampings
                    .Select(c => c.Size)
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
                var plateThicknessValues = allClampings
                    .Select(c => c.PlateThickness)
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

        private void FilterClampings(object sender, EventArgs e)
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
                            DisplayText = $"Kenet #{clamping.ClampingDate:dd.MM.yyyy}{orderInfo} - {clamping.ClampCount} adet (Kalan: {availableClampCount})",
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

        private void CmbClamping_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbClamping.SelectedItem == null)
                return;

            try
            {
                var idProperty = _cmbClamping.SelectedItem.GetType().GetProperty("Id");
                if (idProperty == null)
                    return;

                var clampingId = (Guid)idProperty.GetValue(_cmbClamping.SelectedItem);
                var clamping = _clampingRepository.GetAll().FirstOrDefault(c => c.Id == clampingId);

                if (clamping != null)
                {
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
                    
                    // Uzunluk bilgisini doldur
                    _txtLength.Text = clamping.Length.ToString("F2", CultureInfo.InvariantCulture);
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
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Hatve = decimal.Parse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Length = decimal.Parse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
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

            if (_cmbClamping.SelectedItem == null)
            {
                MessageBox.Show("Lütfen kenetlenmiş plaka seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtLength.Text) || !decimal.TryParse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir uzunluk giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtRequestedAssemblyCount.Text) || !int.TryParse(_txtRequestedAssemblyCount.Text, out int requestedAssemblyCount) || requestedAssemblyCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir istenen montaj adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show($"İstenen montaj adedi kalan kenetlenmiş plaka adedinden fazla olamaz! (Kalan: {availableClampCount}, İstenen: {requestedAssemblyCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
