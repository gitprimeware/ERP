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
    public partial class KenetlenmisStokTakipForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private ClampingRepository _clampingRepository;
        private AssemblyRepository _assemblyRepository;
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;
        
        // Filtreleme kontrolleri
        private ComboBox _cmbSiparisNo;
        private ComboBox _cmbHatve;
        private ComboBox _cmbOlcu;
        private ComboBox _cmbUzunluk;
        private ComboBox _cmbPlakaKalinligi;
        private ComboBox _cmbMusteri;
        private Button _btnFiltrele;
        private Button _btnFiltreleriTemizle;

        public KenetlenmisStokTakipForm()
        {
            _clampingRepository = new ClampingRepository();
            _assemblyRepository = new AssemblyRepository();
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            CreateMainPanel();
            LoadData();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "Kenetlenmiş Stok Takip",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Filtreleme paneli - Tek satır
            var filterPanel = new Panel
            {
                Location = new Point(20, 60),
                Width = _mainPanel.Width - 40,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            // Sipariş No filtresi
            var lblSiparisNo = new Label
            {
                Text = "Sipariş No:",
                Location = new Point(0, 15),
                Width = 80,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbSiparisNo = new ComboBox
            {
                Location = new Point(85, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Hatve filtresi
            var lblHatve = new Label
            {
                Text = "Hatve:",
                Location = new Point(215, 15),
                Width = 50,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbHatve = new ComboBox
            {
                Location = new Point(270, 12),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Ölçü filtresi
            var lblOlcu = new Label
            {
                Text = "Ölçü:",
                Location = new Point(380, 15),
                Width = 40,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbOlcu = new ComboBox
            {
                Location = new Point(425, 12),
                Width = 80,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Uzunluk filtresi
            var lblUzunluk = new Label
            {
                Text = "Uzunluk:",
                Location = new Point(515, 15),
                Width = 60,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbUzunluk = new ComboBox
            {
                Location = new Point(580, 12),
                Width = 80,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Plaka Kalınlığı filtresi
            var lblPlakaKalinligi = new Label
            {
                Text = "Plaka:",
                Location = new Point(670, 15),
                Width = 50,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbPlakaKalinligi = new ComboBox
            {
                Location = new Point(725, 12),
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Müşteri filtresi
            var lblMusteri = new Label
            {
                Text = "Müşteri:",
                Location = new Point(825, 15),
                Width = 60,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbMusteri = new ComboBox
            {
                Location = new Point(890, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Filtrele butonu
            _btnFiltrele = new Button
            {
                Text = "🔍 Filtrele",
                Location = new Point(1050, 10),
                Width = 90,
                Height = 30,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnFiltrele.FlatAppearance.BorderSize = 0;

            // Filtreleri Temizle butonu
            _btnFiltreleriTemizle = new Button
            {
                Text = "🗑️ Temizle",
                Location = new Point(1150, 10),
                Width = 90,
                Height = 30,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnFiltreleriTemizle.FlatAppearance.BorderSize = 0;

            filterPanel.Controls.Add(lblSiparisNo);
            filterPanel.Controls.Add(_cmbSiparisNo);
            filterPanel.Controls.Add(lblHatve);
            filterPanel.Controls.Add(_cmbHatve);
            filterPanel.Controls.Add(lblOlcu);
            filterPanel.Controls.Add(_cmbOlcu);
            filterPanel.Controls.Add(lblUzunluk);
            filterPanel.Controls.Add(_cmbUzunluk);
            filterPanel.Controls.Add(lblPlakaKalinligi);
            filterPanel.Controls.Add(_cmbPlakaKalinligi);
            filterPanel.Controls.Add(lblMusteri);
            filterPanel.Controls.Add(_cmbMusteri);
            filterPanel.Controls.Add(_btnFiltrele);
            filterPanel.Controls.Add(_btnFiltreleriTemizle);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(20, 120),
                Width = _mainPanel.Width - 40,
                Height = _mainPanel.Height - 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false
            };

            _mainPanel.Resize += (s, e) =>
            {
                filterPanel.Width = _mainPanel.Width - 40;
                _dataGridView.Width = _mainPanel.Width - 40;
                _dataGridView.Height = _mainPanel.Height - 200;
            };

            // Event handlers
            _btnFiltrele.Click += BtnFiltrele_Click;
            _btnFiltreleriTemizle.Click += BtnFiltreleriTemizle_Click;

            // Kolonlar - Resimdeki sıraya göre
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = "TARİH (DATE)",
                Name = "Date",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderNo",
                HeaderText = "SİPARİŞ NO (ORDER NUMBER)",
                Name = "OrderNo",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Hatve",
                HeaderText = "HATVE (PITCH)",
                Name = "Hatve",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Size",
                HeaderText = "ÖLÇÜ (SIZE)",
                Name = "Size",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "UZUNLUK (LENGTH)",
                Name = "Length",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ClampCount",
                HeaderText = "ADET (PCS.)",
                Name = "ClampCount",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "KalanAdet",
                HeaderText = "KALAN ADET (REMAINING PCS.)",
                Name = "KalanAdet",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UsedInAssembly",
                HeaderText = "MONTAJDA KULLANILAN (USED IN ASSEMBLY)",
                Name = "UsedInAssembly",
                Width = 200
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Customer",
                HeaderText = "MÜŞTERİ (CUSTOMER)",
                Name = "Customer",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UsedPlateCount",
                HeaderText = "KULLANILAN PLAKA ADEDİ (PCS OF LICENCE PLATES USED)",
                Name = "UsedPlateCount",
                Width = 200
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PlateThickness",
                HeaderText = "PLAKA KALINLIĞI (SHEET THICKNESS)",
                Name = "PlateThickness",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SerialNumber",
                HeaderText = "RULO SERİ NO (ROLL SERIAL NUMBER)",
                Name = "SerialNumber",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MachineName",
                HeaderText = "MAKİNA ADI (MACHINE NAME)",
                Name = "MachineName",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Operator",
                HeaderText = "OPERATÖR (OPERATOR)",
                Name = "Operator",
                Width = 150
            });

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(filterPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();

            // Stil ayarları
            _dataGridView.DefaultCellStyle.BackColor = Color.White;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;
        }

        private bool _filterControlsLoaded = false;

        private void LoadData()
        {
            try
            {
                var clampings = _clampingRepository.GetAll();
                var orders = _orderRepository.GetAll();

                // Filtreleme kontrollerini doldur
                if (!_filterControlsLoaded)
                {
                    LoadFilterControls(clampings, orders);
                    _filterControlsLoaded = true;
                }

                // Filtreleme uygula
                clampings = ApplyFilters(clampings, orders);

                // Montaj işlemlerinden kullanılan kenet adedini hesapla
                var allAssemblies = _assemblyRepository.GetAll();
                var usedClampCountByClampingId = allAssemblies
                    .Where(a => a.IsActive && a.ClampingId.HasValue)
                    .GroupBy(a => a.ClampingId.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(a => a.UsedClampCount));

                var data = clampings.Select(c =>
                {
                    var order = c.OrderId.HasValue ? orders.FirstOrDefault(o => o.Id == c.OrderId.Value) : null;
                    
                    // Bu kenet için montaj işlemlerinde kullanılan adeti hesapla
                    int usedInAssembly = 0;
                    if (usedClampCountByClampingId.ContainsKey(c.Id))
                    {
                        usedInAssembly = usedClampCountByClampingId[c.Id];
                    }
                    
                    // Kalan adet = Toplam kenet adedi - Montajda kullanılan adet
                    int kalanAdet = c.ClampCount - usedInAssembly;
                    
                    // Hatve değerini harf ile göster
                    string hatveDisplay = GetHatveDisplay(c.Hatve);
                    
                    return new
                    {
                        Date = c.ClampingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        OrderNo = order?.TrexOrderNo ?? "",
                        Hatve = hatveDisplay,
                        Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = c.Length.ToString("F2", CultureInfo.InvariantCulture),
                        ClampCount = c.ClampCount.ToString(),
                        KalanAdet = kalanAdet > 0 ? kalanAdet.ToString() : "0",
                        UsedInAssembly = usedInAssembly > 0 ? usedInAssembly.ToString() : "0",
                        Customer = order?.Company?.Name ?? "",
                        UsedPlateCount = c.UsedPlateCount.ToString(),
                        PlateThickness = c.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        SerialNumber = c.SerialNo?.SerialNumber ?? "",
                        MachineName = c.Machine?.Name ?? "",
                        Operator = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : ""
                    };
                }).ToList();

                _dataGridView.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetlenmiş stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilterControls(List<Clamping> clampings, List<Order> orders)
        {
            // Sipariş No listesini doldur
            var orderIds = clampings
                .Where(c => c.OrderId.HasValue)
                .Select(c => c.OrderId.Value)
                .Distinct()
                .ToList();
            
            var orderNos = orders
                .Where(o => orderIds.Contains(o.Id) && !string.IsNullOrEmpty(o.TrexOrderNo))
                .Select(o => o.TrexOrderNo)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            _cmbSiparisNo.Items.Clear();
            _cmbSiparisNo.Items.Add(""); // Tümü seçeneği
            foreach (var orderNo in orderNos)
            {
                _cmbSiparisNo.Items.Add(orderNo);
            }

            // Hatve listesini doldur (H, D, M, L değerleri ile)
            var hatveValues = clampings
                .Select(c => c.Hatve)
                .Distinct()
                .OrderBy(h => h)
                .ToList();
            
            _cmbHatve.Items.Clear();
            _cmbHatve.Items.Add(""); // Tümü seçeneği
            foreach (var hatve in hatveValues)
            {
                string hatveDisplay = GetHatveDisplay(hatve);
                _cmbHatve.Items.Add(hatveDisplay);
            }

            // Ölçü listesini doldur
            var sizes = clampings
                .Select(c => c.Size)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            _cmbOlcu.Items.Clear();
            _cmbOlcu.Items.Add(""); // Tümü seçeneği
            foreach (var size in sizes)
            {
                _cmbOlcu.Items.Add(size.ToString("F2", CultureInfo.InvariantCulture));
            }

            // Uzunluk listesini doldur
            var lengths = clampings
                .Select(c => c.Length)
                .Distinct()
                .OrderBy(l => l)
                .ToList();
            
            _cmbUzunluk.Items.Clear();
            _cmbUzunluk.Items.Add(""); // Tümü seçeneği
            foreach (var length in lengths)
            {
                _cmbUzunluk.Items.Add(length.ToString("F2", CultureInfo.InvariantCulture));
            }

            // Plaka Kalınlığı listesini doldur
            var thicknesses = clampings
                .Select(c => c.PlateThickness)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            
            _cmbPlakaKalinligi.Items.Clear();
            _cmbPlakaKalinligi.Items.Add(""); // Tümü seçeneği
            foreach (var thickness in thicknesses)
            {
                _cmbPlakaKalinligi.Items.Add(thickness.ToString("F3", CultureInfo.InvariantCulture));
            }

            // Müşteri listesini doldur
            var companyIds = orders
                .Where(o => orderIds.Contains(o.Id))
                .Select(o => o.CompanyId)
                .Distinct()
                .ToList();
            
            var companies = _companyRepository.GetAll()
                .Where(c => companyIds.Contains(c.Id))
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            
            _cmbMusteri.Items.Clear();
            _cmbMusteri.Items.Add(""); // Tümü seçeneği
            foreach (var company in companies)
            {
                _cmbMusteri.Items.Add(company);
            }
        }

        private List<Clamping> ApplyFilters(List<Clamping> clampings, List<Order> orders)
        {
            // Filtreleme kriterlerini al
            string filterSiparisNo = _cmbSiparisNo?.Text?.Trim() ?? "";
            string filterHatve = _cmbHatve?.Text?.Trim() ?? "";
            string filterOlcu = _cmbOlcu?.Text?.Trim() ?? "";
            string filterUzunluk = _cmbUzunluk?.Text?.Trim() ?? "";
            string filterPlakaKalinligi = _cmbPlakaKalinligi?.Text?.Trim() ?? "";
            string filterMusteri = _cmbMusteri?.Text?.Trim() ?? "";

            // Sipariş No filtresi
            if (!string.IsNullOrEmpty(filterSiparisNo))
            {
                var orderIds = orders
                    .Where(o => o.TrexOrderNo != null && o.TrexOrderNo.Contains(filterSiparisNo, StringComparison.OrdinalIgnoreCase))
                    .Select(o => o.Id)
                    .ToList();
                
                clampings = clampings.Where(c => c.OrderId.HasValue && orderIds.Contains(c.OrderId.Value)).ToList();
            }

            // Hatve filtresi (H, D, M, L veya sayısal değer)
            if (!string.IsNullOrEmpty(filterHatve))
            {
                // H, D, M, L harflerini kontrol et
                decimal? hatveValue = null;
                if (filterHatve.Length == 1)
                {
                    char hatveLetter = char.ToUpper(filterHatve[0]);
                    switch (hatveLetter)
                    {
                        case 'H': hatveValue = 3.25m; break;
                        case 'D': hatveValue = 4.5m; break;
                        case 'M': hatveValue = 6.5m; break;
                        case 'L': hatveValue = 9m; break;
                    }
                }
                
                // Eğer harf değilse, sayısal değer olarak parse et
                if (!hatveValue.HasValue)
                {
                    // "3.25 (H)" formatından sayıyı çıkar
                    string numericPart = filterHatve.Split('(')[0].Trim();
                    if (decimal.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedValue))
                    {
                        hatveValue = parsedValue;
                    }
                }

                if (hatveValue.HasValue)
                {
                    clampings = clampings.Where(c => Math.Abs(c.Hatve - hatveValue.Value) < 0.1m).ToList();
                }
            }

            // Ölçü filtresi
            if (!string.IsNullOrEmpty(filterOlcu))
            {
                if (decimal.TryParse(filterOlcu, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal sizeValue))
                {
                    clampings = clampings.Where(c => Math.Abs(c.Size - sizeValue) < 0.01m).ToList();
                }
            }

            // Uzunluk filtresi
            if (!string.IsNullOrEmpty(filterUzunluk))
            {
                if (decimal.TryParse(filterUzunluk, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lengthValue))
                {
                    clampings = clampings.Where(c => Math.Abs(c.Length - lengthValue) < 0.01m).ToList();
                }
            }

            // Plaka Kalınlığı filtresi
            if (!string.IsNullOrEmpty(filterPlakaKalinligi))
            {
                if (decimal.TryParse(filterPlakaKalinligi, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal thicknessValue))
                {
                    clampings = clampings.Where(c => Math.Abs(c.PlateThickness - thicknessValue) < 0.001m).ToList();
                }
            }

            // Müşteri filtresi
            if (!string.IsNullOrEmpty(filterMusteri))
            {
                var companyIds = _companyRepository.GetAll()
                    .Where(c => c.Name != null && c.Name.Contains(filterMusteri, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Id)
                    .ToList();
                
                var orderIds = orders
                    .Where(o => companyIds.Contains(o.CompanyId))
                    .Select(o => o.Id)
                    .ToList();
                
                clampings = clampings.Where(c => c.OrderId.HasValue && orderIds.Contains(c.OrderId.Value)).ToList();
            }

            return clampings;
        }

        private string GetHatveDisplay(decimal hatveValue)
        {
            string hatveLetter = GetHatveLetter(hatveValue);
            if (hatveLetter == "H" || hatveLetter == "D" || hatveLetter == "M" || hatveLetter == "L")
            {
                return $"{hatveValue.ToString("F2", CultureInfo.InvariantCulture)} ({hatveLetter})";
            }
            return hatveValue.ToString("F2", CultureInfo.InvariantCulture);
        }

        private string GetHatveLetter(decimal hatveValue)
        {
            const decimal tolerance = 0.1m;
            
            if (Math.Abs(hatveValue - 3.25m) < tolerance)
                return "H";
            else if (Math.Abs(hatveValue - 4.5m) < tolerance)
                return "D";
            else if (Math.Abs(hatveValue - 6.5m) < tolerance)
                return "M";
            else if (Math.Abs(hatveValue - 9m) < tolerance)
                return "L";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void BtnFiltrele_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnFiltreleriTemizle_Click(object sender, EventArgs e)
        {
            _cmbSiparisNo.SelectedIndex = -1;
            _cmbSiparisNo.Text = "";
            _cmbHatve.SelectedIndex = -1;
            _cmbHatve.Text = "";
            _cmbOlcu.SelectedIndex = -1;
            _cmbOlcu.Text = "";
            _cmbUzunluk.SelectedIndex = -1;
            _cmbUzunluk.Text = "";
            _cmbPlakaKalinligi.SelectedIndex = -1;
            _cmbPlakaKalinligi.Text = "";
            _cmbMusteri.SelectedIndex = -1;
            _cmbMusteri.Text = "";
            LoadData();
        }
    }
}

