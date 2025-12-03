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
        private TextBox _txtPlateThickness;
        private TextBox _txtHatve;
        private TextBox _txtSize;
        private TextBox _txtLength;
        private ComboBox _cmbSerialNo;
        private ComboBox _cmbPressing;
        private ComboBox _cmbMachine;
        private TextBox _txtClampCount;
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
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Kenetleme Yap";
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

            // Preslenmiş Plaka Seçimi
            var lblPressing = new Label
            {
                Text = "Preslenmiş Plaka:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPressing = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPressing.SelectedIndexChanged += CmbPressing_SelectedIndexChanged;
            this.Controls.Add(lblPressing);
            this.Controls.Add(_cmbPressing);
            yPos += spacing;

            // Plaka Kalınlığı (Readonly)
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
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            this.Controls.Add(lblPlateThickness);
            this.Controls.Add(_txtPlateThickness);
            yPos += spacing;

            // Hatve (Readonly)
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
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            this.Controls.Add(lblHatve);
            this.Controls.Add(_txtHatve);
            yPos += spacing;

            // Ölçü (Readonly)
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
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            this.Controls.Add(lblSize);
            this.Controls.Add(_txtSize);
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

            // Kenetleme Adedi
            var lblClampCount = new Label
            {
                Text = "Kenetleme Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtClampCount = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblClampCount);
            this.Controls.Add(_txtClampCount);
            yPos += spacing;

            // Kullanılan Plaka Adedi (Kullanıcı girebilir)
            var lblUsedPlateCount = new Label
            {
                Text = "Kullanılan Plaka Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtUsedPlateCount = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            _txtUsedPlateCount.TextChanged += TxtUsedPlateCount_TextChanged;
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
                // Preslenmiş plakaları yükle
                _cmbPressing.Items.Clear();
                List<Pressing> pressings;
                
                // Eğer orderId boşsa, tüm preslenmiş stokları göster
                if (_orderId == Guid.Empty)
                {
                    pressings = _pressingRepository.GetAll();
                }
                else
                {
                    pressings = _pressingRepository.GetByOrderId(_orderId);
                }
                
                foreach (var pressing in pressings.Where(p => p.PressCount > 0 && p.IsActive))
                {
                    // Daha önce kenetlenmiş plaka adedini hesapla
                    var usedPlateCount = _clampingRepository.GetAll()
                        .Where(c => c.PressingId == pressing.Id && c.IsActive)
                        .Sum(c => c.UsedPlateCount);
                    
                    var availablePlateCount = pressing.PressCount - usedPlateCount;
                    
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

                // Seri No'ları yükle (readonly için)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // Pres bilgilerini otomatik doldur
                    _txtPlateThickness.Text = pressing.PlateThickness.ToString("F3", CultureInfo.InvariantCulture);
                    _txtHatve.Text = pressing.Hatve.ToString("F2", CultureInfo.InvariantCulture);
                    _txtSize.Text = pressing.Size.ToString("F2", CultureInfo.InvariantCulture);
                    
                    // Mevcut kullanılabilir adeti göster (varsayılan olarak)
                    var usedPlateCount = _clampingRepository.GetAll()
                        .Where(c => c.PressingId == pressing.Id && c.IsActive)
                        .Sum(c => c.UsedPlateCount);
                    
                    var availablePlateCount = pressing.PressCount - usedPlateCount;
                    _txtUsedPlateCount.Text = availablePlateCount > 0 ? availablePlateCount.ToString() : "0";

                    // Rulo Seri No'yu doldur
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

                // OrderId'yi preslenmiş plakadan al (eğer yoksa null bırak)
                var orderId = pressing?.OrderId ?? _orderId;
                
                var clamping = new Clamping
                {
                    OrderId = orderId != Guid.Empty ? orderId : (Guid?)null,
                    PressingId = pressingId,
                    PlateThickness = pressing.PlateThickness,
                    Hatve = pressing.Hatve,
                    Size = pressing.Size,
                    Length = decimal.Parse(_txtLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    SerialNoId = pressing.SerialNoId,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    ClampCount = int.Parse(_txtClampCount.Text),
                    UsedPlateCount = int.Parse(_txtUsedPlateCount.Text),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    ClampingDate = DateTime.Now
                };

                _clampingRepository.Insert(clamping);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
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

            if (string.IsNullOrWhiteSpace(_txtClampCount.Text) || !int.TryParse(_txtClampCount.Text, out int clampCount) || clampCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir kenetleme adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtUsedPlateCount.Text) || !int.TryParse(_txtUsedPlateCount.Text, out int usedPlateCount) || usedPlateCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir kullanılan plaka adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbPressing.SelectedItem == null)
                return true;

            var pressingId = GetSelectedId(_cmbPressing);
            var pressing = _pressingRepository.GetAll().FirstOrDefault(p => p.Id == pressingId);
            if (pressing != null)
            {
                var alreadyUsedPlateCount = _clampingRepository.GetAll()
                    .Where(c => c.PressingId == pressingId && c.IsActive)
                    .Sum(c => c.UsedPlateCount);
                
                var availablePlateCount = pressing.PressCount - alreadyUsedPlateCount;
                
                if (usedPlateCount > availablePlateCount)
                {
                    MessageBox.Show($"Kullanılan plaka adedi kalan preslenmiş plaka adedinden fazla olamaz! (Kalan: {availablePlateCount}, İstenen: {usedPlateCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void TxtUsedPlateCount_TextChanged(object sender, EventArgs e)
        {
            // Kullanıcı kullanılacak adeti girdiğinde validasyon yapılabilir
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

