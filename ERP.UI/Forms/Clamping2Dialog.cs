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
    public partial class Clamping2Dialog : Form
    {
        private ComboBox _cmbPlateThickness;
        private ComboBox _cmbHatve;
        private DataGridView _dgvClampings;
        private ComboBox _cmbFirstClamping;
        private ComboBox _cmbSecondClamping;
        private Label _lblResultedSize;
        private Label _lblResultedLength;
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

        public Clamping2Dialog(EmployeeRepository employeeRepository, 
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
            this.Text = "Kenetleme 2 - Birleştirme";
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

            // Lamel Kalınlığı (ComboBox - tablodaki değerlerden)
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
            _cmbHatve.SelectedIndexChanged += FilterClampings;
            this.Controls.Add(lblHatve);
            this.Controls.Add(_cmbHatve);
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

            // İlk Kenetlenmiş Ürün
            var lblFirstClamping = new Label
            {
                Text = "İlk Kenetlenmiş Ürün:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbFirstClamping = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            _cmbFirstClamping.SelectedIndexChanged += UpdateResultedValues;
            this.Controls.Add(lblFirstClamping);
            this.Controls.Add(_cmbFirstClamping);
            yPos += spacing;

            // İkinci Kenetlenmiş Ürün
            var lblSecondClamping = new Label
            {
                Text = "İkinci Kenetlenmiş Ürün:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSecondClamping = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            _cmbSecondClamping.SelectedIndexChanged += UpdateResultedValues;
            this.Controls.Add(lblSecondClamping);
            this.Controls.Add(_cmbSecondClamping);
            yPos += spacing;

            // Sonuç Ölçü
            var lblResultedSizeLabel = new Label
            {
                Text = "Sonuç Ölçü:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblResultedSize = new Label
            {
                Location = new Point(150, yPos),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblResultedSizeLabel);
            this.Controls.Add(_lblResultedSize);
            yPos += spacing;

            // Sonuç Uzunluk
            var lblResultedLengthLabel = new Label
            {
                Text = "Sonuç Uzunluk:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _lblResultedLength = new Label
            {
                Location = new Point(150, yPos),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary
            };
            this.Controls.Add(lblResultedLengthLabel);
            this.Controls.Add(_lblResultedLength);
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
                
                // Lamel Kalınlığı değerlerini al (distinct)
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

                // Hatve değerlerini al (distinct)
                var hatves = allClampings
                    .Select(c => c.Hatve)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                _cmbHatve.Items.Clear();
                foreach (var h in hatves)
                {
                    _cmbHatve.Items.Add(h.ToString("F2", CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Filtreleme verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterClampings(object sender, EventArgs e)
        {
            try
            {
                if (_cmbPlateThickness.SelectedItem == null || _cmbHatve.SelectedItem == null)
                {
                    _dgvClampings.DataSource = null;
                    _cmbFirstClamping.Items.Clear();
                    _cmbSecondClamping.Items.Clear();
                    _cmbFirstClamping.Enabled = false;
                    _cmbSecondClamping.Enabled = false;
                    return;
                }

                var selectedPlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                var selectedHatve = decimal.Parse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);

                // Tüm kenetlenmiş ürünleri getir
                var allClampings = _clampingRepository.GetAll();

                // Filtreleme: Aynı hatve ve lamel kalınlığına sahip kenetlenmiş ürünler
                var filteredList = allClampings
                    .Where(c => c.IsActive && 
                           Math.Abs(c.PlateThickness - selectedPlateThickness) < 0.001m &&
                           Math.Abs(c.Hatve - selectedHatve) < 0.01m)
                    .ToList();

                // DataGridView'e göster
                var displayData = filteredList.Select(c =>
                {
                    var order = c.OrderId.HasValue ? _orderRepository.GetById(c.OrderId.Value) : null;
                    string orderInfo = order != null ? order.TrexOrderNo : "Stok";
                    
                    // Kalan adet hesapla: Clamping2Request'lerden kullanılan miktarları çıkar (bekleyen talepler de dahil)
                    var usedAsFirst = _clamping2RequestRepository.GetAll()
                        .Where(cr2 => cr2.IsActive && cr2.FirstClampingId == c.Id)
                        .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                    
                    var usedAsSecond = _clamping2RequestRepository.GetAll()
                        .Where(cr2 => cr2.IsActive && cr2.SecondClampingId == c.Id)
                        .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                    
                    var totalUsedCount = usedAsFirst + usedAsSecond;
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

                // ComboBox'ları doldur (TÜM ürünleri ekle)
                _cmbFirstClamping.Items.Clear();
                _cmbSecondClamping.Items.Clear();
                
                // Tüm ürünleri her iki ComboBox'a da ekle
                foreach (var item in displayData)
                {
                    _cmbFirstClamping.Items.Add(new 
                    { 
                        Id = item.Id, 
                        DisplayText = $"Ölçü: {item.Size}, Uzunluk: {item.Length}, Kalan: {item.AvailableCount}",
                        Clamping = item.Clamping
                    });
                    _cmbSecondClamping.Items.Add(new 
                    { 
                        Id = item.Id, 
                        DisplayText = $"Ölçü: {item.Size}, Uzunluk: {item.Length}, Kalan: {item.AvailableCount}",
                        Clamping = item.Clamping
                    });
                }
                
                _cmbFirstClamping.DisplayMember = "DisplayText";
                _cmbFirstClamping.ValueMember = "Id";
                _cmbSecondClamping.DisplayMember = "DisplayText";
                _cmbSecondClamping.ValueMember = "Id";
                
                _cmbFirstClamping.Enabled = _cmbFirstClamping.Items.Count > 0;
                _cmbSecondClamping.Enabled = _cmbSecondClamping.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş ürünler filtrelenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateResultedValues(object sender, EventArgs e)
        {
            try
            {
                if (_cmbFirstClamping.SelectedItem == null || _cmbSecondClamping.SelectedItem == null)
                {
                    _lblResultedSize.Text = "";
                    _lblResultedLength.Text = "";
                    return;
                }

                var firstIdProperty = _cmbFirstClamping.SelectedItem.GetType().GetProperty("Clamping");
                var secondIdProperty = _cmbSecondClamping.SelectedItem.GetType().GetProperty("Clamping");
                
                if (firstIdProperty == null || secondIdProperty == null)
                    return;

                var firstClamping = firstIdProperty.GetValue(_cmbFirstClamping.SelectedItem) as Clamping;
                var secondClamping = secondIdProperty.GetValue(_cmbSecondClamping.SelectedItem) as Clamping;

                if (firstClamping == null || secondClamping == null)
                    return;

                // Aynı ölçüye sahip olmalı (farklı uzunlukta olabilir)
                if (Math.Abs(firstClamping.Size - secondClamping.Size) >= 0.01m)
                {
                    MessageBox.Show("Aynı ölçüye sahip ürünler seçilmelidir!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (sender == _cmbFirstClamping)
                        _cmbFirstClamping.SelectedItem = null;
                    else
                        _cmbSecondClamping.SelectedItem = null;
                    return;
                }

                // Sonuç ölçü: İki ürünün ölçüsü aynı olmalı
                _lblResultedSize.Text = firstClamping.Size.ToString("F2", CultureInfo.InvariantCulture);
                
                // Sonuç uzunluk: İki uzunluğun toplamı
                var resultedLength = firstClamping.Length + secondClamping.Length;
                _lblResultedLength.Text = resultedLength.ToString("F2", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sonuç değerleri hesaplanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var firstClampingId = GetSelectedId(_cmbFirstClamping);
                var firstClamping = _clampingRepository.GetById(firstClampingId);

                if (firstClamping == null)
                {
                    MessageBox.Show("Seçilen kenetlenmiş ürün bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var secondClampingId = GetSelectedId(_cmbSecondClamping);
                var secondClamping = _clampingRepository.GetById(secondClampingId);

                if (secondClamping == null)
                {
                    MessageBox.Show("Seçilen ikinci kenetlenmiş ürün bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Aynı ölçüye sahip olmalı (farklı uzunlukta olabilir)
                if (Math.Abs(firstClamping.Size - secondClamping.Size) >= 0.01m)
                {
                    MessageBox.Show("Aynı ölçüye sahip ürünler seçilmelidir!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // OrderId: Dialog hangi sipariş için açıldıysa o siparişe ait olmalı
                var orderId = _orderId != Guid.Empty ? _orderId : (Guid?)null;

                var clamping2Request = new Clamping2Request
                {
                    OrderId = orderId,
                    Hatve = decimal.Parse(_cmbHatve.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    FirstClampingId = firstClampingId,
                    SecondClampingId = secondClampingId,
                    ResultedSize = firstClamping.Size,
                    ResultedLength = firstClamping.Length + secondClamping.Length,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedCount = int.Parse(_txtRequestedCount.Text),
                    EmployeeId = _cmbEmployee.SelectedItem != null ? GetSelectedId(_cmbEmployee) : (Guid?)null,
                    Status = "Beklemede",
                    RequestDate = DateTime.Now
                };
                
                _clamping2RequestRepository.Insert(clamping2Request);
                MessageBox.Show("Kenetleme 2 talebi başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme 2 talebi oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (_cmbFirstClamping.SelectedItem == null)
            {
                MessageBox.Show("Lütfen ilk kenetlenmiş ürünü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbSecondClamping.SelectedItem == null)
            {
                MessageBox.Show("Lütfen ikinci kenetlenmiş ürünü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtRequestedCount.Text) || !int.TryParse(_txtRequestedCount.Text, out int requestedCount) || requestedCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir istenen adet girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Adet kontrolü: Seçilen ürünlerin kalan adetlerini kontrol et
            if (_cmbFirstClamping.SelectedItem != null)
            {
                var firstClampingId = GetSelectedId(_cmbFirstClamping);
                var firstClamping = _clampingRepository.GetById(firstClampingId);

                if (firstClamping != null)
                {
                    // İlk ürünün kalan adetini hesapla (tamamlanmış ve bekleyen taleplerin hepsi hesaba katılmalı)
                    var usedAsFirstFirst = _clamping2RequestRepository.GetAll()
                        .Where(cr2 => cr2.IsActive && cr2.FirstClampingId == firstClamping.Id)
                        .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                    
                    var usedAsFirstSecond = _clamping2RequestRepository.GetAll()
                        .Where(cr2 => cr2.IsActive && cr2.SecondClampingId == firstClamping.Id)
                        .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                    
                    var firstAvailableCount = firstClamping.ClampCount - usedAsFirstFirst - usedAsFirstSecond;

                    // İstenen adet ilk ürünün kalan adetinden fazla olamaz
                    if (requestedCount > firstAvailableCount)
                    {
                        MessageBox.Show($"İstenen adet, ürünün kalan adedinden fazla olamaz! (Kalan: {firstAvailableCount}, İstenen: {requestedCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // İkinci ürünün kalan adedini de kontrol et
                    var secondClampingId = GetSelectedId(_cmbSecondClamping);
                    var secondClamping = _clampingRepository.GetById(secondClampingId);

                    if (secondClamping != null)
                    {
                        var usedAsSecondFirst = _clamping2RequestRepository.GetAll()
                            .Where(cr2 => cr2.IsActive && cr2.FirstClampingId == secondClamping.Id)
                            .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                        
                        var usedAsSecondSecond = _clamping2RequestRepository.GetAll()
                            .Where(cr2 => cr2.IsActive && cr2.SecondClampingId == secondClamping.Id)
                            .Sum(cr2 => cr2.ActualCount ?? cr2.RequestedCount);
                        
                        var secondAvailableCount = secondClamping.ClampCount - usedAsSecondFirst - usedAsSecondSecond;

                        if (requestedCount > secondAvailableCount)
                        {
                            MessageBox.Show($"İstenen adet, ikinci ürünün kalan adedinden fazla olamaz! (Kalan: {secondAvailableCount}, İstenen: {requestedCount})", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
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

