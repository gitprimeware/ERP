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
    public partial class CuttingDialog : Form
    {
        private TextBox _txtHatve;
        private TextBox _txtSize;
        private ComboBox _cmbMachine;
        private Button _btnAddMachine;
        private ComboBox _cmbSerialNo;
        private TextBox _txtTotalKg;
        private TextBox _txtCutKg;
        private TextBox _txtCuttingCount;
        private TextBox _txtWasteKg;
        private TextBox _txtRemainingKg;
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private MachineRepository _machineRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private CuttingRepository _cuttingRepository;
        private MaterialEntryRepository _materialEntryRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;

        public CuttingDialog(MachineRepository machineRepository, SerialNoRepository serialNoRepository, EmployeeRepository employeeRepository, Guid orderId)
        {
            _machineRepository = machineRepository;
            _serialNoRepository = serialNoRepository;
            _employeeRepository = employeeRepository;
            _cuttingRepository = new CuttingRepository();
            _materialEntryRepository = new MaterialEntryRepository();
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Kesim Yap";
            this.Width = 500;
            this.Height = 600;
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

            // Kesilen Kg
            var lblCutKg = new Label
            {
                Text = "Kesilen Kg:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtCutKg = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            _txtCutKg.TextChanged += (s, e) => CalculateRemainingKg();
            this.Controls.Add(lblCutKg);
            this.Controls.Add(_txtCutKg);
            yPos += spacing;

            // Kesim Adedi
            var lblCuttingCount = new Label
            {
                Text = "Kesim Adedi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtCuttingCount = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblCuttingCount);
            this.Controls.Add(_txtCuttingCount);
            yPos += spacing;

            // Hurda Kg
            var lblWasteKg = new Label
            {
                Text = "Hurda Kg:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtWasteKg = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            _txtWasteKg.TextChanged += (s, e) => CalculateRemainingKg();
            this.Controls.Add(lblWasteKg);
            this.Controls.Add(_txtWasteKg);
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

                // Siparişten hatve bilgisini al
                var order = _orderRepository.GetById(_orderId);
                if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                {
                    var parts = order.ProductCode.Split('-');
                    if (parts.Length >= 3)
                    {
                        string modelProfile = parts[2];
                        if (modelProfile.Length > 0)
                        {
                            char modelLetter = modelProfile[0];
                            decimal hatve = GetHtave(modelLetter);
                            _txtHatve.Text = hatve.ToString("F2", CultureInfo.InvariantCulture);
                        }
                    }
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

        private void CalculateRemainingKg()
        {
            if (decimal.TryParse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalKg))
            {
                decimal cutKg = 0;
                decimal.TryParse(_txtCutKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out cutKg);
                
                decimal wasteKg = 0;
                decimal.TryParse(_txtWasteKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out wasteKg);
                
                decimal remainingKg = totalKg - cutKg - wasteKg;
                _txtRemainingKg.Text = remainingKg.ToString("F3", CultureInfo.InvariantCulture);
            }
            else
            {
                _txtRemainingKg.Text = "0";
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
                    CuttingCount = int.Parse(_txtCuttingCount.Text),
                    WasteKg = decimal.Parse(_txtWasteKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
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

            if (string.IsNullOrWhiteSpace(_txtCutKg.Text) || !decimal.TryParse(_txtCutKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cutKg) || cutKg <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir kesilen kg giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtCuttingCount.Text) || !int.TryParse(_txtCuttingCount.Text, out int cuttingCount) || cuttingCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir kesim adedi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_txtWasteKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal wasteKg))
            {
                wasteKg = 0;
            }

            if (decimal.TryParse(_txtTotalKg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalKg) && (cutKg + wasteKg) > totalKg)
            {
                MessageBox.Show("Kesilen kg + Hurda kg, toplam kg'dan fazla olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
