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
    public partial class DivideDialog : Form
    {
        private ComboBox _cmbPlateThickness;
        private ComboBox _cmbHatve;
        private ComboBox _cmbSize;
        private ComboBox _cmbLength;
        private DataGridView _dgvClampings;
        private ComboBox _cmbClamping;
        private TextBox _txtFirstLength;
        private TextBox _txtSecondLength;
        private Label _lblOriginalLength;
        private TextBox _txtRequestedCount;
        private ComboBox _cmbMachine;
        private ComboBox _cmbEmployee;
        private Button _btnAddEmployee;
        private Button _btnSave;
        private Button _btnCancel;
        
        private EmployeeRepository _employeeRepository;
        private MachineRepository _machineRepository;
        private ClampingRepository _clampingRepository;
        private Clamping2RequestRepository _clamping2RequestRepository;
        private OrderRepository _orderRepository;
        private Guid _orderId;

        public DivideDialog(EmployeeRepository employeeRepository, 
            MachineRepository machineRepository, Guid orderId)
        {
            _employeeRepository = employeeRepository;
            _machineRepository = machineRepository;
            _clampingRepository = new ClampingRepository();
            _clamping2RequestRepository = new Clamping2RequestRepository();
            _orderRepository = new OrderRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Bölme İşlemi";
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

            // Lamel Kalınlığı
            var lblPlateThickness = new Label
            {
                Text = "Lamel Kalınlığı:",
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
            _cmbPlateThickness.SelectedIndexChanged += FilterClampings;
            this.Controls.Add(lblPlateThickness);
            this.Controls.Add(_cmbPlateThickness);
            yPos += spacing;

            // Hatve
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
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize.SelectedIndexChanged += FilterClampings;
            this.Controls.Add(lblSize);
            this.Controls.Add(_cmbSize);
            yPos += spacing;

            // Uzunluk (ComboBox - tablodaki değerlerden)
            var lblLength = new Label
            {
                Text = "Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbLength = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbLength.SelectedIndexChanged += FilterClampings;
            this.Controls.Add(lblLength);
            this.Controls.Add(_cmbLength);
            yPos += spacing;

            // Kenetlenmiş Ürünler Listesi
            var lblClampings = new Label
            {
                Text = "Kenetlenmiş Ürünler:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblClampings);
            yPos += 25;

            _dgvClampings = new DataGridView
            {
                Location = new Point(20, yPos),
                Width = 480,
                Height = 180,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(_dgvClampings);
            yPos += 190;

            // Kenetlenmiş Ürün
            var lblClamping = new Label
            {
                Text = "Kenetlenmiş Ürün:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbClamping = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            _cmbClamping.SelectedIndexChanged += CmbClamping_SelectedIndexChanged;
            this.Controls.Add(lblClamping);
            this.Controls.Add(_cmbClamping);
            yPos += spacing;

            // Orijinal Uzunluk (ReadOnly)
            var lblOriginalLength = new Label
            {
                Text = "Orijinal Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblOriginalLength = new Label
            {
                Location = new Point(150, yPos),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblOriginalLength);
            this.Controls.Add(_lblOriginalLength);
            yPos += spacing;

            // İlk Uzunluk
            var lblFirstLength = new Label
            {
                Text = "İlk Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtFirstLength = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            _txtFirstLength.TextChanged += UpdateSecondLength;
            this.Controls.Add(lblFirstLength);
            this.Controls.Add(_txtFirstLength);
            yPos += spacing;

            // İkinci Uzunluk (ReadOnly - Otomatik hesaplanır)
            var lblSecondLength = new Label
            {
                Text = "İkinci Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtSecondLength = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };
            this.Controls.Add(lblSecondLength);
            this.Controls.Add(_txtSecondLength);
            yPos += spacing;

            // İstenen Adet
            var lblRequestedCount = new Label
            {
                Text = "İstenen Adet:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtRequestedCount = new TextBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblRequestedCount);
            this.Controls.Add(_txtRequestedCount);
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

            // Butonlar (ortada)
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
                var allClampings = _clampingRepository.GetAll();
                
                // Lamel Kalınlığı değerlerini al
                var plateThicknesses = allClampings
                    .Select(c => c.PlateThickness)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                _cmbPlateThickness.Items.Clear();
                foreach (var pt in plateThicknesses)
                {
                    _cmbPlateThickness.Items.Add(pt.ToString("F3", CultureInfo.InvariantCulture));
                }

                // Hatve değerlerini al
                var hatves = allClampings
                    .Select(c => c.Hatve)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                _cmbHatve.Items.Clear();
                foreach (var h in hatves)
                {
                    // Hatve değerini "6.5(M)" formatında göster
                    string hatveDisplay = GetHatveLetter(h);
                    _cmbHatve.Items.Add(hatveDisplay);
                }

                // Ölçü değerlerini al (distinct)
                var sizes = allClampings
                    .Select(c => c.Size)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                _cmbSize.Items.Clear();
                foreach (var s in sizes)
                {
                    _cmbSize.Items.Add(s.ToString("F2", CultureInfo.InvariantCulture));
                }

                // Uzunluk değerlerini al (distinct)
                var lengths = allClampings
                    .Select(c => c.Length)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                _cmbLength.Items.Clear();
                foreach (var l in lengths)
                {
                    _cmbLength.Items.Add(l.ToString("F2", CultureInfo.InvariantCulture));
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
                if (_cmbPlateThickness.SelectedItem == null || _cmbHatve.SelectedItem == null)
                {
                    _dgvClampings.DataSource = null;
                    _cmbClamping.Items.Clear();
                    _cmbClamping.Enabled = false;
                    return;
                }

                var selectedPlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                var selectedHatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString());

                var allClampings = _clampingRepository.GetAll();

                // Filtreleme: Lamel kalınlığı, hatve, ölçü ve uzunluk ile filtrele
                var filteredList = allClampings
                    .Where(c => c.IsActive && 
                           Math.Abs(c.PlateThickness - selectedPlateThickness) < 0.001m &&
                           Math.Abs(c.Hatve - selectedHatve) < 0.01m &&
                           (_cmbSize.SelectedItem == null || Math.Abs(c.Size - decimal.Parse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture)) < 0.1m) &&
                           (_cmbLength.SelectedItem == null || Math.Abs(c.Length - decimal.Parse(_cmbLength.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture)) < 1.0m))
                    .ToList();

                var displayData = filteredList.Select(c =>
                {
                    var order = c.OrderId.HasValue ? _orderRepository.GetById(c.OrderId.Value) : null;
                    string orderInfo = order != null ? order.TrexOrderNo : "Stok";
                    
                    // Kalan adet hesapla
                    var allRequests = _clamping2RequestRepository.GetAll().Where(cr2 => cr2.IsActive).ToList();
                    var totalUsedCount = 0;
                    
                    foreach (var cr2 in allRequests)
                    {
                        // Items listesi varsa onu kullan
                        if (cr2.Items != null && cr2.Items.Count > 0)
                        {
                            // Bu ürün Items listesinde kaç kere geçiyor?
                            var itemCount = cr2.Items.Count(item => item.ClampingId == c.Id);
                            if (itemCount > 0)
                            {
                                totalUsedCount += (cr2.ActualCount ?? cr2.RequestedCount) * itemCount;
                            }
                        }
                        // Geriye dönük uyumluluk için FirstClampingId/SecondClampingId kullan
                        else
                        {
                            if (cr2.FirstClampingId == c.Id)
                                totalUsedCount += cr2.ActualCount ?? cr2.RequestedCount;
                            if (cr2.SecondClampingId == c.Id)
                                totalUsedCount += cr2.ActualCount ?? cr2.RequestedCount;
                        }
                    }
                    
                    var availableCount = c.ClampCount - totalUsedCount;
                    
                    return new
                    {
                        Id = c.Id,
                        Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = c.Length.ToString("F2", CultureInfo.InvariantCulture),
                        OrderNo = orderInfo,
                        AvailableCount = availableCount > 0 ? availableCount : 0,
                        Clamping = c
                    };
                }).Where(x => x.AvailableCount > 0).ToList();

                _dgvClampings.DataSource = displayData;
                _dgvClampings.Columns["Id"].Visible = false;
                _dgvClampings.Columns["Clamping"].Visible = false;
                if (_dgvClampings.Columns["Size"] != null)
                {
                    _dgvClampings.Columns["Size"].HeaderText = "Ölçü";
                    _dgvClampings.Columns["Size"].Width = 80;
                }
                if (_dgvClampings.Columns["Length"] != null)
                {
                    _dgvClampings.Columns["Length"].HeaderText = "Uzunluk";
                    _dgvClampings.Columns["Length"].Width = 80;
                }
                if (_dgvClampings.Columns["OrderNo"] != null)
                {
                    _dgvClampings.Columns["OrderNo"].HeaderText = "Sipariş No";
                    _dgvClampings.Columns["OrderNo"].Width = 140;
                }
                if (_dgvClampings.Columns["AvailableCount"] != null)
                {
                    _dgvClampings.Columns["AvailableCount"].HeaderText = "Kalan Adet";
                    _dgvClampings.Columns["AvailableCount"].Width = 100;
                }

                _cmbClamping.Items.Clear();
                foreach (var item in displayData)
                {
                    _cmbClamping.Items.Add(new 
                    { 
                        Id = item.Id, 
                        DisplayText = $"Ölçü: {item.Size}, Uzunluk: {item.Length}, Kalan: {item.AvailableCount}",
                        Clamping = item.Clamping
                    });
                }
                
                _cmbClamping.DisplayMember = "DisplayText";
                _cmbClamping.ValueMember = "Id";
                _cmbClamping.Enabled = _cmbClamping.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş ürünler filtrelenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbClamping_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbClamping.SelectedItem == null)
            {
                _lblOriginalLength.Text = "";
                _txtFirstLength.Text = "";
                _txtSecondLength.Text = "";
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

                _lblOriginalLength.Text = clamping.Length.ToString("F2", CultureInfo.InvariantCulture);
                _txtFirstLength.Text = "";
                _txtSecondLength.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün bilgileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSecondLength(object sender, EventArgs e)
        {
            try
            {
                if (_cmbClamping.SelectedItem == null || string.IsNullOrWhiteSpace(_lblOriginalLength.Text))
                {
                    _txtSecondLength.Text = "";
                    return;
                }

                if (!decimal.TryParse(_lblOriginalLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal originalLength))
                {
                    _txtSecondLength.Text = "";
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtFirstLength.Text))
                {
                    _txtSecondLength.Text = "";
                    return;
                }

                if (!decimal.TryParse(_txtFirstLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal firstLength))
                {
                    _txtSecondLength.Text = "";
                    return;
                }

                if (firstLength >= originalLength || firstLength <= 0)
                {
                    _txtSecondLength.Text = "";
                    return;
                }

                var secondLength = originalLength - firstLength;
                _txtSecondLength.Text = secondLength.ToString("F2", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uzunluk hesaplanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    _cmbEmployee.Items.Add(new { Id = employee.Id, Name = $"{employee.FirstName} {employee.LastName}" });
                }
                _cmbEmployee.DisplayMember = "Name";
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
                Height = 200,
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
                var clamping = _clampingRepository.GetById(clampingId);

                if (clamping == null)
                {
                    MessageBox.Show("Seçilen kenetlenmiş ürün bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!decimal.TryParse(_txtFirstLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal firstLength) || firstLength <= 0)
                {
                    MessageBox.Show("Geçerli bir ilk uzunluk giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (firstLength >= clamping.Length)
                {
                    MessageBox.Show("İlk uzunluk, orijinal uzunluktan küçük olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var secondLength = clamping.Length - firstLength;

                // OrderId: Dialog hangi sipariş için açıldıysa o siparişe ait olmalı (stok için null)
                var orderId = _orderId != Guid.Empty ? _orderId : (Guid?)null;

                // Bölme işlemi için Clamping2Request kullanıyoruz (SecondClampingId null olacak, ResultedLength ilk uzunluk)
                var divideRequest = new Clamping2Request
                {
                    OrderId = orderId,
                    Hatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString()),
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    FirstClampingId = clampingId,
                    SecondClampingId = null, // Bölme işleminde ikinci ürün yok
                    ResultedSize = clamping.Size,
                    ResultedLength = firstLength, // İlk uzunluk stoğa eklenecek
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedCount = int.Parse(_txtRequestedCount.Text),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede",
                    RequestDate = DateTime.Now
                };
                
                _clamping2RequestRepository.Insert(divideRequest);
                MessageBox.Show("Bölme talebi başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bölme talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (_cmbPlateThickness.SelectedItem == null)
            {
                MessageBox.Show("Lütfen lamel kalınlığı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbHatve.SelectedItem == null)
            {
                MessageBox.Show("Lütfen hatve seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbClamping.SelectedItem == null)
            {
                MessageBox.Show("Lütfen kenetlenmiş ürünü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtFirstLength.Text) || !decimal.TryParse(_txtFirstLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal firstLength) || firstLength <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir ilk uzunluk girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtRequestedCount.Text) || !int.TryParse(_txtRequestedCount.Text, out int requestedCount) || requestedCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir istenen adet girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Adet kontrolü
            var clampingId = GetSelectedId(_cmbClamping);
            var clamping = _clampingRepository.GetById(clampingId);

            if (clamping != null)
            {
                // Stok kontrolü: Items listesini de dikkate al
                var allRequests = _clamping2RequestRepository.GetAll().Where(cr2 => cr2.IsActive).ToList();
                var totalUsedCount = 0;
                
                foreach (var cr2 in allRequests)
                {
                    // Items listesi varsa onu kullan
                    if (cr2.Items != null && cr2.Items.Count > 0)
                    {
                        // Bu ürün Items listesinde kaç kere geçiyor?
                        var itemCount = cr2.Items.Count(item => item.ClampingId == clamping.Id);
                        if (itemCount > 0)
                        {
                            totalUsedCount += (cr2.ActualCount ?? cr2.RequestedCount) * itemCount;
                        }
                    }
                    // Geriye dönük uyumluluk için FirstClampingId/SecondClampingId kullan
                    else
                    {
                        if (cr2.FirstClampingId == clamping.Id)
                            totalUsedCount += cr2.ActualCount ?? cr2.RequestedCount;
                        if (cr2.SecondClampingId == clamping.Id)
                            totalUsedCount += cr2.ActualCount ?? cr2.RequestedCount;
                    }
                }
                
                var availableCount = clamping.ClampCount - totalUsedCount;

                if (requestedCount > availableCount)
                {
                    MessageBox.Show($"İstenen adet, ürünün kalan adedinden fazla olamaz! (Kalan: {availableCount}, İstenen: {requestedCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // İlk uzunluk kontrolü
                if (firstLength >= clamping.Length)
                {
                    MessageBox.Show("İlk uzunluk, orijinal uzunluktan küçük olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private decimal ParseHatveValue(string hatveText)
        {
            // "6.5(M)" formatından sayısal değeri çıkar
            if (string.IsNullOrWhiteSpace(hatveText))
                return 0;

            // Parantez varsa parantez öncesini al
            if (hatveText.Contains("("))
            {
                string hatveNumberPart = hatveText.Split('(')[0].Trim();
                if (decimal.TryParse(hatveNumberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatveValue))
                    return hatveValue;
            }

            // Direkt parse et
            if (decimal.TryParse(hatveText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hatve))
                return hatve;

            return 0;
        }
    }
}

