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
        private ComboBox _cmbLength;
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
        private CoverStockRepository _coverStockRepository;
        private SideProfileStockRepository _sideProfileStockRepository;
        private SideProfileRemnantRepository _sideProfileRemnantRepository;
        private Guid _orderId;
        
        // Yeni kontroller
        private Label _lblSizeDisplay; // Ölçü (cm) gösterimi
        private Label _lblLengthDisplay; // Uzunluk (mm) gösterimi
        private Label _lblCoverInfo; // Kapak bilgisi
        private Label _lblCoverCount; // Kapak adedi (her adet için 2 tane)
        private Label _lblSideProfileInfo; // Yan profil bilgisi
        private DataGridView _dgvSideProfiles; // Yan profil seçim listesi

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
            _coverStockRepository = new CoverStockRepository();
            _sideProfileStockRepository = new SideProfileStockRepository();
            _sideProfileRemnantRepository = new SideProfileRemnantRepository();
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Montaj Yap";
            this.Width = 850; // Daha geniş yapıyoruz
            this.Height = 800; // Daha yüksek yapıyoruz
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
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbPlateThickness.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
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
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbHatve.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
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
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
            this.Controls.Add(lblSize);
            this.Controls.Add(_cmbSize);
            yPos += spacing;

            // Uzunluk (Seçilebilir - sipariş uzunluğundan kapak boyu çıkarılmış, MM cinsinden)
            var lblLength = new Label
            {
                Text = "Uzunluk (mm):",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbLength = new ComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbLength.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLength.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
            this.Controls.Add(lblLength);
            this.Controls.Add(_cmbLength);
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
            _txtRequestedAssemblyCount.TextChanged += TxtRequestedAssemblyCount_TextChanged;
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

            // Bilgi paneli (Kenetlenmiş seçildiğinde gösterilecek)
            var infoPanel = new Panel
            {
                Location = new Point(20, yPos),
                Width = 500,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            int infoYPos = 10;
            
            // Ölçü gösterimi (cm parantez içinde)
            var lblSizeLabel = new Label
            {
                Text = "Ölçü:",
                Location = new Point(10, infoYPos),
                Width = 80,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _lblSizeDisplay = new Label
            {
                Text = "-",
                Location = new Point(90, infoYPos),
                Width = 150,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary
            };
            infoPanel.Controls.Add(lblSizeLabel);
            infoPanel.Controls.Add(_lblSizeDisplay);
            infoYPos += 25;

            // Uzunluk gösterimi (mm parantez içinde)
            var lblLengthLabel = new Label
            {
                Text = "Uzunluk:",
                Location = new Point(10, infoYPos),
                Width = 80,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _lblLengthDisplay = new Label
            {
                Text = "-",
                Location = new Point(90, infoYPos),
                Width = 150,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary
            };
            infoPanel.Controls.Add(lblLengthLabel);
            infoPanel.Controls.Add(_lblLengthDisplay);
            infoYPos += 25;

            // Kapak bilgisi
            var lblCoverLabel = new Label
            {
                Text = "Kullanılacak Kapak:",
                Location = new Point(10, infoYPos),
                Width = 150,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _lblCoverInfo = new Label
            {
                Text = "-",
                Location = new Point(160, infoYPos),
                Width = 300,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary
            };
            infoPanel.Controls.Add(lblCoverLabel);
            infoPanel.Controls.Add(_lblCoverInfo);
            infoYPos += 25;

            // Kapak adedi (Her adet için 2 tane)
            _lblCoverCount = new Label
            {
                Text = "Her adet için 2 tane kullanılacak",
                Location = new Point(160, infoYPos),
                Width = 300,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeColors.TextSecondary
            };
            infoPanel.Controls.Add(_lblCoverCount);
            infoYPos += 25;

            // Yan profil başlığı
            var lblSideProfileLabel = new Label
            {
                Text = "Yan Profiller:",
                Location = new Point(10, infoYPos),
                Width = 150,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _lblSideProfileInfo = new Label
            {
                Text = "-",
                Location = new Point(160, infoYPos),
                Width = 300,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary
            };
            infoPanel.Controls.Add(lblSideProfileLabel);
            infoPanel.Controls.Add(_lblSideProfileInfo);

            this.Controls.Add(infoPanel);
            yPos += 210;

            // Yan profil seçim tablosu
            var lblSideProfileGrid = new Label
            {
                Text = "Kullanılacak Yan Profiller:",
                Location = new Point(20, yPos),
                Width = 250,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblSideProfileGrid);
            yPos += 25;

            _dgvSideProfiles = new DataGridView
            {
                Location = new Point(20, yPos),
                Width = 500,
                Height = 150,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoGenerateColumns = false
            };

            _dgvSideProfiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Source",
                HeaderText = "Kaynak",
                Name = "Source",
                Width = 100
            });

            _dgvSideProfiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "Uzunluk (m)",
                Name = "Length",
                Width = 120
            });

            _dgvSideProfiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 80
            });

            _dgvSideProfiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalLength",
                HeaderText = "Toplam (m)",
                Name = "TotalLength",
                Width = 120
            });

            // Stil ayarları
            _dgvSideProfiles.DefaultCellStyle.BackColor = Color.White;
            _dgvSideProfiles.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvSideProfiles.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvSideProfiles.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvSideProfiles.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvSideProfiles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvSideProfiles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvSideProfiles.EnableHeadersVisualStyles = false;

            this.Controls.Add(_dgvSideProfiles);
            yPos += 160;

            // Butonlar (ortalanmış)
            int buttonWidth = 90;
            int buttonSpacing = 10;
            int totalButtonWidth = (buttonWidth * 2) + buttonSpacing;
            int startX = (this.Width - totalButtonWidth) / 2;
            
            _btnSave = new Button
            {
                Text = "Kaydet",
                Location = new Point(startX, yPos),
                Width = buttonWidth,
                Height = 32,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnSave, 4);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "İptal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(startX + buttonWidth + buttonSpacing, yPos),
                Width = buttonWidth,
                Height = 32,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnCancel, 4);

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
                            // Hatve değerini "6.5(M)" formatında göster
                            string hatveDisplay = GetHatveLetter(h);
                            _cmbHatve.Items.Add(hatveDisplay);
                        }
                        
                        var hatveText = GetHatveLetter(hatve);
                        if (_cmbHatve.Items.Contains(hatveText))
                            _cmbHatve.SelectedItem = hatveText;
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
                }
                
                // Uzunluk bilgisini siparişten al (kapak boyu çıkarılmış, MM olarak göster)
                if (parts.Length >= 5 && int.TryParse(parts[4], out int yukseklikMM))
                {
                    // Yükseklik com: <= 1800 ise aynı, > 1800 ise /2
                    int yukseklikComMM = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                    
                    // Kapak boyu çıkarılmış uzunluk (montajlanmamış kenet ararken kapaksız uzunluğu arıyoruz)
                    int kapakBoyuMM = GetKapakBoyuFromOrder();
                    decimal kapaksizUzunlukMM = yukseklikComMM - kapakBoyuMM;
                    
                    // ComboBox'a tüm mevcut uzunluk değerlerini ekle (MM cinsinden)
                    // NOT: Clamping.Length veritabanında CM cinsinden saklanıyor, ama biz MM olarak gösteriyoruz
                    // Eğer değerler yanlış geliyorsa (10 katı fazla), Length'in nasıl saklandığını kontrol et
                    var allLengths = _clampingRepository.GetAll()
                        .Where(c => c.ClampCount > 0 && c.IsActive)
                        .Select(c => c.Length) // Direkt Length'i al - eğer MM cinsindense direkt kullan
                        .Distinct()
                        .OrderBy(l => l)
                        .ToList();
                    _cmbLength.Items.Clear();
                    foreach (var l in allLengths)
                    {
                        _cmbLength.Items.Add(l.ToString("F2", CultureInfo.InvariantCulture));
                    }
                    
                    // Sipariş uzunluğundan (kapak boyu çıkarılmış) değeri seç
                    var lengthText = kapaksizUzunlukMM.ToString("F2", CultureInfo.InvariantCulture);
                    if (_cmbLength.Items.Contains(lengthText))
                        _cmbLength.SelectedItem = lengthText;
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
                if (_cmbHatve.SelectedItem == null || 
                    _cmbSize.SelectedItem == null || 
                    _cmbPlateThickness.SelectedItem == null ||
                    _cmbLength.SelectedItem == null)
                {
                    _cmbClamping.Enabled = false;
                    return;
                }

                decimal hatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString());
                if (hatve == 0 ||
                    !decimal.TryParse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) ||
                    !decimal.TryParse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness) ||
                    !decimal.TryParse(_cmbLength.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lengthMM))
                {
                    _cmbClamping.Enabled = false;
                    return;
                }

                // Uzunluk filtresi: ComboBox'tan seçilen uzunluk MM cinsinden
                // NOT: Eğer Length MM cinsinden saklanıyorsa direkt karşılaştır, CM ise /10 yap
                // Şimdilik direkt karşılaştırıyoruz (Length'in MM cinsinden olduğunu varsayıyoruz)
                decimal uzunlukDegeri = lengthMM; // MM cinsinden

                // TÜM kenetlenmiş stokları yükle (sadece belirli bir siparişe ait değil, stoktan da kullanılabilir)
                var allClampings = _clampingRepository.GetAll();
                
                // Filtreleme: Hatve, Ölçü, Plaka Kalınlığı, Uzunluk
                var filteredClampings = allClampings.Where(c => 
                    c.ClampCount > 0 && 
                    c.IsActive &&
                    Math.Abs(c.Hatve - hatve) < 0.01m &&
                    Math.Abs(c.Size - size) < 0.1m &&
                    Math.Abs(c.PlateThickness - plateThickness) < 0.001m &&
                    Math.Abs(c.Length - uzunlukDegeri) < 0.1m); // Direkt karşılaştırma (tolerance: 0.1mm)
                
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

                decimal hatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString());
                if (hatve == 0 ||
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
            // Kenetlenmiş plaka seçildiğinde Rulo Seri No'yu güncelle ve bilgileri göster
            if (_cmbClamping.SelectedItem == null)
            {
                _cmbSerialNo.SelectedItem = null;
                _lblSizeDisplay.Text = "-";
                _lblLengthDisplay.Text = "-";
                _lblCoverInfo.Text = "-";
                _lblSideProfileInfo.Text = "-";
                _dgvSideProfiles.DataSource = null;
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

                // Ölçü ve Uzunluk bilgilerini göster
                _lblSizeDisplay.Text = $"{clamping.Size.ToString("F2", CultureInfo.InvariantCulture)} (cm)";
                _lblLengthDisplay.Text = $"{clamping.Length.ToString("F2", CultureInfo.InvariantCulture)} (mm)";

                // Sipariş bilgilerini al
                var order = _orderRepository.GetById(_orderId);
                if (order != null)
                {
                    // Kapak bilgisini göster
                    LoadCoverInfo(order);
                    
                    // Yan profil bilgilerini hesapla ve göster
                    LoadSideProfileInfo(order, clamping);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenet bilgileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCoverInfo(Order order)
        {
            try
            {
                if (string.IsNullOrEmpty(order.ProductCode))
                {
                    _lblCoverInfo.Text = "-";
                    return;
                }

                var parts = order.ProductCode.Split('-');
                
                // Profil tipi (2. parça, 2. karakter: S=Standart, G=Geniş)
                string profileType = "";
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2];
                    if (modelProfile.Length >= 2)
                    {
                        char profileLetter = modelProfile[1];
                        profileType = profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
                    }
                }

                // Plaka ölçüsü
                int plateSizeMM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    plateSizeMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                }

                // Kapak boyu
                int coverLengthMM = GetKapakBoyuFromOrder();

                if (!string.IsNullOrEmpty(profileType) && plateSizeMM > 0 && coverLengthMM > 0)
                {
                    // CoverStock'tan kontrol et
                    var coverStock = _coverStockRepository.GetByProperties(profileType, plateSizeMM, coverLengthMM);
                    string stockInfo = coverStock != null ? $" (Stok: {coverStock.Quantity} adet)" : " (Stokta yok)";
                    _lblCoverInfo.Text = $"{profileType} - {plateSizeMM}mm - {coverLengthMM}mm{stockInfo}";
                }
                else
                {
                    _lblCoverInfo.Text = "-";
                }
            }
            catch (Exception ex)
            {
                _lblCoverInfo.Text = "Hata: " + ex.Message;
            }
        }

        private void LoadSideProfileInfo(Order order, Clamping clamping)
        {
            try
            {
                if (string.IsNullOrEmpty(_txtRequestedAssemblyCount.Text) || 
                    !int.TryParse(_txtRequestedAssemblyCount.Text, out int assemblyCount) || 
                    assemblyCount <= 0)
                {
                    _lblSideProfileInfo.Text = "Lütfen montajlanacak adet giriniz";
                    _dgvSideProfiles.DataSource = null;
                    return;
                }

                // Clamping.Length zaten kapaksız uzunluk (MM cinsinden), bu yan profil uzunluğu
                decimal sideProfileLengthMM = clamping.Length; // MM cinsinden
                decimal sideProfileLengthM = clamping.Length / 1000.0m; // MM'den metreye çevir (1000 ile böl)

                // Her adet için 4 tane yan profil gerekiyor
                int requiredSideProfileCount = assemblyCount * 4;
                decimal requiredTotalLengthM = sideProfileLengthM * requiredSideProfileCount;

                _lblSideProfileInfo.Text = $"Her adet için 4 tane - Toplam {requiredSideProfileCount} tane ({requiredTotalLengthM.ToString("F2", CultureInfo.InvariantCulture)} m) gerekiyor";

                // Kullanılabilir yan profilleri hesapla ve göster
                CalculateAndDisplaySideProfiles(order, sideProfileLengthM, requiredSideProfileCount, requiredTotalLengthM);
            }
            catch (Exception ex)
            {
                _lblSideProfileInfo.Text = "Hata: " + ex.Message;
                _dgvSideProfiles.DataSource = null;
            }
        }

        private void CmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hatve, Ölçü, Plaka Kalınlığı veya Uzunluk değiştiğinde kenetlenmiş stokları yeniden yükle
            LoadClampingsBasedOnOrder();
        }

        private void TxtRequestedAssemblyCount_TextChanged(object sender, EventArgs e)
        {
            // Montajlanacak adet değiştiğinde yan profil bilgilerini yeniden hesapla
            if (_cmbClamping.SelectedItem != null)
            {
                var clampingProperty = _cmbClamping.SelectedItem.GetType().GetProperty("Clamping");
                if (clampingProperty != null)
                {
                    var clamping = clampingProperty.GetValue(_cmbClamping.SelectedItem) as Clamping;
                    if (clamping != null)
                    {
                        var order = _orderRepository.GetById(_orderId);
                        if (order != null)
                        {
                            LoadSideProfileInfo(order, clamping);
                        }
                    }
                }
            }
        }

        private void CalculateAndDisplaySideProfiles(Order order, decimal requiredLengthM, int requiredCount, decimal requiredTotalLengthM)
        {
            try
            {
                // Profil tipini Order'dan al (ürün kodundan)
                string profileType = "";
                if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                {
                    var parts = order.ProductCode.Split('-');
                    if (parts.Length >= 3)
                    {
                        string modelProfile = parts[2];
                        if (modelProfile.Length >= 2)
                        {
                            char profileLetter = modelProfile[1];
                            profileType = profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
                        }
                    }
                }

                if (string.IsNullOrEmpty(profileType))
                {
                    profileType = "Standart"; // Varsayılan
                }

                var profiles = new List<object>();
                int remainingCount = requiredCount;

                // Önce kalanlardan (remnants) kontrol et - profil tipine göre - eşit veya daha uzun olanları öncelikli kullan
                var usableRemnants = _sideProfileRemnantRepository.GetAll(includeWaste: false)
                    .Where(r => r.ProfileType == profileType && r.Length >= requiredLengthM && r.Quantity > 0)
                    .OrderBy(r => r.Length) // En kısa olanlardan başla (öncelik)
                    .ToList();

                foreach (var remnant in usableRemnants)
                {
                    if (remainingCount <= 0)
                        break;

                    // Bu remnant'tan kaç tane kullanabiliriz
                    int useCount = Math.Min(remnant.Quantity, remainingCount);
                    
                    profiles.Add(new
                    {
                        Source = "Kalan",
                        Length = remnant.Length.ToString("F2", CultureInfo.InvariantCulture),
                        Quantity = useCount.ToString(),
                        TotalLength = (requiredLengthM * useCount).ToString("F2", CultureInfo.InvariantCulture)
                    });

                    remainingCount -= useCount;
                }

                // Hala ihtiyaç varsa 6 metrelik stoklardan kullan - profil tipine göre
                if (remainingCount > 0)
                {
                    var sixMeterStock = _sideProfileStockRepository.GetByLengthAndProfileType(6.0m, profileType);
                    if (sixMeterStock != null && sixMeterStock.RemainingLength > 0)
                    {
                        // Her bir 6 metrelik profilden kaç tane yan profil çıkar (6m / requiredLengthM)
                        decimal profilesPerSixMeter = 6.0m / requiredLengthM;
                        int profilesPerSixMeterInt = (int)Math.Floor(profilesPerSixMeter);
                        
                        if (profilesPerSixMeterInt > 0)
                        {
                            // Kaç tane 6 metrelik profil gerekiyor
                            int neededSixMeterProfiles = (int)Math.Ceiling((decimal)remainingCount / profilesPerSixMeterInt);
                            
                            // Mevcut 6 metrelik stoktan kaç tane kullanılabilir
                            int availableSixMeterProfiles = (int)Math.Floor(sixMeterStock.RemainingLength / 6.0m);
                            int useFromStock = Math.Min(neededSixMeterProfiles, availableSixMeterProfiles);

                            if (useFromStock > 0)
                            {
                                profiles.Add(new
                                {
                                    Source = "Stok (6m)",
                                    Length = "6.00",
                                    Quantity = useFromStock.ToString(),
                                    TotalLength = (useFromStock * 6.0m).ToString("F2", CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }
                }

                _dgvSideProfiles.DataSource = profiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yan profil hesaplaması sırasında hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var order = _orderRepository.GetById(_orderId);
                
                int requestedCount = int.Parse(_txtRequestedAssemblyCount.Text);
                
                // Stok kontrolleri - montaj talebi oluşturulmadan önce kontrol et
                if (order != null)
                {
                    // Kapak stok kontrolü
                    if (!CheckCoverStock(order, requestedCount))
                    {
                        return; // Hata mesajı zaten gösterildi
                    }

                    // Yan profil stok kontrolü
                    if (clamping != null)
                    {
                        if (!CheckSideProfileStock(order, clamping, requestedCount))
                        {
                            return; // Hata mesajı zaten gösterildi
                        }
                    }
                }
                
                var assemblyRequest = new AssemblyRequest
                {
                    OrderId = orderId, // Dialog hangi sipariş için açıldıysa o siparişe ait
                    ClampingId = clampingId,
                    PlateThickness = decimal.Parse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    Hatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString()),
                    Size = decimal.Parse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    // Uzunluk MM olarak giriliyor, direkt kaydet (Length MM cinsinden saklanıyor)
                    Length = decimal.Parse(_cmbLength.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture),
                    SerialNoId = clamping?.SerialNoId,
                    MachineId = _cmbMachine.SelectedItem != null ? GetSelectedId(_cmbMachine) : (Guid?)null,
                    RequestedAssemblyCount = requestedCount,
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

            // Plaka Kalınlığı kontrolü
            if (_cmbPlateThickness.SelectedItem == null)
            {
                MessageBox.Show("Lütfen plaka kalınlığı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_cmbPlateThickness.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plateThickness) || plateThickness <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir plaka kalınlığı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Hatve kontrolü
            if (_cmbHatve.SelectedItem == null)
            {
                MessageBox.Show("Lütfen hatve seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            decimal hatve = ParseHatveValue(_cmbHatve.SelectedItem.ToString());
            if (hatve <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir hatve seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Ölçü kontrolü
            if (_cmbSize.SelectedItem == null)
            {
                MessageBox.Show("Lütfen ölçü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_cmbSize.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal size) || size <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir ölçü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbLength.SelectedItem == null || !decimal.TryParse(_cmbLength.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
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

        private bool CheckCoverStock(Order order, int requestedAdet)
        {
            try
            {
                if (order == null || string.IsNullOrEmpty(order.ProductCode))
                    return true; // Kontrol edilemiyorsa geç

                var parts = order.ProductCode.Split('-');
                
                // Profil tipi (S=Standart, G=Geniş)
                string profileType = "";
                if (parts.Length >= 3)
                {
                    string modelProfile = parts[2];
                    if (modelProfile.Length >= 2)
                    {
                        char profileLetter = modelProfile[1];
                        profileType = profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
                    }
                }

                // Plaka ölçüsü
                int plateSizeMM = 0;
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plakaOlcusuMM))
                {
                    plateSizeMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                }

                // Kapak boyu
                int coverLengthMM = GetKapakBoyuFromOrder();

                if (!string.IsNullOrEmpty(profileType) && plateSizeMM > 0 && coverLengthMM > 0)
                {
                    // CoverStock'tan bul
                    var coverStock = _coverStockRepository.GetByProperties(profileType, plateSizeMM, coverLengthMM);
                    if (coverStock == null)
                    {
                        MessageBox.Show($"Uygun ölçüde kapak stoku bulunamadı! (Profil Tipi: {profileType}, Ölçü: {plateSizeMM}mm, Uzunluk: {coverLengthMM}mm)\n\nMontaj talebi oluşturulamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Her adet için 2 tane kapak kullanılacak
                    int neededCoverCount = requestedAdet * 2;
                    
                    if (coverStock.Quantity < neededCoverCount)
                    {
                        MessageBox.Show($"Yetersiz kapak stoku! Gereken: {neededCoverCount} adet, Mevcut: {coverStock.Quantity} adet\n\nMontaj talebi oluşturulamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kapak stok kontrolü yapılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool CheckSideProfileStock(Order order, Clamping clamping, int requestedAdet)
        {
            try
            {
                // Profil tipini Order'dan al (ürün kodundan)
                string profileType = "";
                if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                {
                    var parts = order.ProductCode.Split('-');
                    if (parts.Length >= 3)
                    {
                        string modelProfile = parts[2];
                        if (modelProfile.Length >= 2)
                        {
                            char profileLetter = modelProfile[1];
                            profileType = profileLetter == 'S' || profileLetter == 's' ? "Standart" : "Geniş";
                        }
                    }
                }

                if (string.IsNullOrEmpty(profileType))
                {
                    profileType = "Standart"; // Varsayılan
                }

                // Yan profil uzunluğu = clamping.Length (MM cinsinden)
                decimal sideProfileLengthMM = clamping.Length;
                decimal sideProfileLengthM = sideProfileLengthMM / 1000.0m; // MM'den metreye çevir
                
                // Her adet için 4 tane yan profil gerekiyor
                int neededProfileCount = requestedAdet * 4;

                // Önce kalanlardan (remnants) kontrol et - profil tipine göre
                var usableRemnants = _sideProfileRemnantRepository.GetAll(includeWaste: false)
                    .Where(r => r.ProfileType == profileType && r.Length >= sideProfileLengthM && r.Quantity > 0)
                    .OrderBy(r => r.Length)
                    .ToList();

                int remainingNeeded = neededProfileCount;
                foreach (var remnant in usableRemnants)
                {
                    if (remainingNeeded <= 0)
                        break;
                    remainingNeeded -= remnant.Quantity;
                }

                // Hala ihtiyaç varsa 6 metrelik stoklardan kontrol et - profil tipine göre
                if (remainingNeeded > 0)
                {
                    var sixMeterStock = _sideProfileStockRepository.GetByLengthAndProfileType(6.0m, profileType);
                    if (sixMeterStock == null || sixMeterStock.RemainingLength <= 0)
                    {
                        MessageBox.Show($"Yetersiz yan profil stoku! Gereken: {neededProfileCount} adet ({sideProfileLengthMM}mm uzunluğunda), ancak yeterli stok bulunamadı.\n\nMontaj talebi oluşturulamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Her bir 6 metrelik profilden kaç tane yan profil çıkar
                    int profilesPerSixMeter = (int)Math.Floor(6.0m / sideProfileLengthM);
                    
                    if (profilesPerSixMeter > 0)
                    {
                        // Kaç tane 6 metrelik profil gerekiyor
                        int neededSixMeterProfiles = (int)Math.Ceiling((decimal)remainingNeeded / profilesPerSixMeter);
                        
                        // Mevcut 6 metrelik stoktan kaç tane kullanılabilir
                        int availableSixMeterProfiles = (int)Math.Floor(sixMeterStock.RemainingLength / 6.0m);
                        
                        if (availableSixMeterProfiles < neededSixMeterProfiles)
                        {
                            MessageBox.Show($"Yetersiz yan profil stoku! Gereken: {neededProfileCount} adet ({sideProfileLengthMM}mm uzunluğunda), ancak yeterli stok bulunamadı.\n\nMontaj talebi oluşturulamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Yan profil uzunluğu ({sideProfileLengthMM}mm) çok uzun! 6 metrelik stoktan kesilemez.\n\nMontaj talebi oluşturulamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yan profil stok kontrolü yapılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
