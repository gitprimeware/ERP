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
    public partial class KesilmisStokTakipForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private CuttingRepository _cuttingRepository;
        private PressingRepository _pressingRepository;
        private OrderRepository _orderRepository;
        
        // Filtreleme kontrolleri
        private ComboBox _cmbSiparisNo;
        private ComboBox _cmbHatve;
        private ComboBox _cmbOlcu;
        private ComboBox _cmbRuloSeriNo;
        private Button _btnFiltrele;
        private Button _btnFiltreleriTemizle;

        public KesilmisStokTakipForm()
        {
            _cuttingRepository = new CuttingRepository();
            _pressingRepository = new PressingRepository();
            _orderRepository = new OrderRepository();
            
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadData();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "Kesilmiş Stok Takip",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Filtreleme paneli
            var filterPanel = new Panel
            {
                Location = new Point(30, 70),
                Width = _mainPanel.Width - 60,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            // Sipariş No filtresi
            var lblSiparisNo = new Label
            {
                Text = "Sipariş No:",
                Location = new Point(0, 15),
                Width = 90,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbSiparisNo = new ComboBox
            {
                Location = new Point(95, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Hatve filtresi
            var lblHatve = new Label
            {
                Text = "Hatve:",
                Location = new Point(255, 15),
                Width = 60,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbHatve = new ComboBox
            {
                Location = new Point(320, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Ölçü filtresi
            var lblOlcu = new Label
            {
                Text = "Ölçü:",
                Location = new Point(450, 15),
                Width = 50,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbOlcu = new ComboBox
            {
                Location = new Point(505, 12),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Rulo Seri No filtresi
            var lblRuloSeriNo = new Label
            {
                Text = "Rulo Seri No:",
                Location = new Point(615, 15),
                Width = 100,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbRuloSeriNo = new ComboBox
            {
                Location = new Point(720, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Filtrele butonu
            _btnFiltrele = new Button
            {
                Text = "🔍 Filtrele",
                Location = new Point(880, 10),
                Width = 100,
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
                Location = new Point(990, 10),
                Width = 100,
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
            filterPanel.Controls.Add(lblRuloSeriNo);
            filterPanel.Controls.Add(_cmbRuloSeriNo);
            filterPanel.Controls.Add(_btnFiltrele);
            filterPanel.Controls.Add(_btnFiltreleriTemizle);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 130),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            _mainPanel.Resize += (s, e) =>
            {
                filterPanel.Width = _mainPanel.Width - 60;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 180;
            };

            // Event handlers
            _btnFiltrele.Click += BtnFiltrele_Click;
            _btnFiltreleriTemizle.Click += BtnFiltreleriTemizle_Click;

            // Kolonlar
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SiparisNo",
                HeaderText = "Sipariş No",
                Name = "SiparisNo",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "KesimTarihi",
                HeaderText = "Kesim Tarihi",
                Name = "KesimTarihi",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Hatve",
                HeaderText = "Hatve",
                Name = "Hatve",
                Width = 80
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Olcu",
                HeaderText = "Ölçü",
                Name = "Olcu",
                Width = 80
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RuloSeriNo",
                HeaderText = "Rulo Seri No",
                Name = "RuloSeriNo",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ToplamPlakaAdedi",
                HeaderText = "Toplam Plaka Adedi",
                Name = "ToplamPlakaAdedi",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "KullanilanPlakaAdedi",
                HeaderText = "Kullanılan Plaka Adedi",
                Name = "KullanilanPlakaAdedi",
                Width = 180
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "KalanPlakaAdedi",
                HeaderText = "Kalan Plaka Adedi",
                Name = "KalanPlakaAdedi",
                Width = 150
            });

            // Stil ayarları
            _dataGridView.DefaultCellStyle.BackColor = ThemeColors.Surface;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(filterPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private bool _filterControlsLoaded = false;

        private void LoadData()
        {
            try
            {
                var cuttings = _cuttingRepository.GetAll()
                    .Where(c => c.PlakaAdedi > 0 && c.IsActive)
                    .OrderByDescending(c => c.CuttingDate)
                    .ToList();

                // Filtreleme kontrollerini doldur
                if (!_filterControlsLoaded)
                {
                    LoadFilterControls(cuttings);
                    _filterControlsLoaded = true;
                }

                // Filtreleme uygula
                cuttings = ApplyFilters(cuttings);

                var stockData = new List<StockRowData>();

                foreach (var cutting in cuttings)
                {
                    // Sipariş bilgisi
                    var order = cutting.OrderId.HasValue ? _orderRepository.GetById(cutting.OrderId.Value) : null;
                    string siparisNo = order?.TrexOrderNo ?? "-";

                    // Kullanılan plaka adedi (pres işlemlerinde kullanılan)
                    var usedPlakaAdedi = _pressingRepository.GetAll()
                        .Where(p => p.CuttingId == cutting.Id && p.IsActive)
                        .Sum(p => p.PressCount);

                    // Kalan plaka adedi
                    int kalanPlakaAdedi = cutting.PlakaAdedi - usedPlakaAdedi;

                    // Hatve değerini harf ile göster
                    string hatveDisplay = GetHatveDisplay(cutting.Hatve);

                    stockData.Add(new StockRowData
                    {
                        SiparisNo = siparisNo,
                        KesimTarihi = cutting.CuttingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        Hatve = hatveDisplay,
                        Olcu = cutting.Size.ToString("F2", CultureInfo.InvariantCulture),
                        RuloSeriNo = cutting.SerialNo?.SerialNumber ?? "-",
                        ToplamPlakaAdedi = cutting.PlakaAdedi.ToString(),
                        KullanilanPlakaAdedi = usedPlakaAdedi.ToString(),
                        KalanPlakaAdedi = kalanPlakaAdedi.ToString()
                    });
                }

                _dataGridView.DataSource = stockData;

                // Kalan plaka adedi kolonunu renklendir
                if (_dataGridView.Columns["KalanPlakaAdedi"] != null)
                {
                    _dataGridView.Columns["KalanPlakaAdedi"].DefaultCellStyle.ForeColor = ThemeColors.Success;
                    _dataGridView.Columns["KalanPlakaAdedi"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshData()
        {
            LoadData();
        }

        private void LoadFilterControls(List<Cutting> cuttings)
        {
            // Sipariş No listesini doldur
            var orderIds = cuttings
                .Where(c => c.OrderId.HasValue)
                .Select(c => c.OrderId.Value)
                .Distinct()
                .ToList();
            
            var orders = _orderRepository.GetAll()
                .Where(o => orderIds.Contains(o.Id) && !string.IsNullOrEmpty(o.TrexOrderNo))
                .Select(o => o.TrexOrderNo)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            _cmbSiparisNo.Items.Clear();
            _cmbSiparisNo.Items.Add(""); // Tümü seçeneği
            foreach (var orderNo in orders)
            {
                _cmbSiparisNo.Items.Add(orderNo);
            }

            // Hatve listesini doldur (H, D, M, L değerleri ile)
            var hatveValues = cuttings
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
            var sizes = cuttings
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

            // Rulo Seri No listesini doldur
            var serialNumbers = cuttings
                .Where(c => c.SerialNo != null && !string.IsNullOrEmpty(c.SerialNo.SerialNumber))
                .Select(c => c.SerialNo.SerialNumber)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            _cmbRuloSeriNo.Items.Clear();
            _cmbRuloSeriNo.Items.Add(""); // Tümü seçeneği
            foreach (var serialNo in serialNumbers)
            {
                _cmbRuloSeriNo.Items.Add(serialNo);
            }
        }

        private List<Cutting> ApplyFilters(List<Cutting> cuttings)
        {
            // Filtreleme kriterlerini al
            string filterSiparisNo = _cmbSiparisNo?.Text?.Trim() ?? "";
            string filterHatve = _cmbHatve?.Text?.Trim() ?? "";
            string filterOlcu = _cmbOlcu?.Text?.Trim() ?? "";
            string filterRuloSeriNo = _cmbRuloSeriNo?.Text?.Trim() ?? "";

            // Sipariş No filtresi
            if (!string.IsNullOrEmpty(filterSiparisNo))
            {
                var orderIds = _orderRepository.GetAll()
                    .Where(o => o.TrexOrderNo != null && o.TrexOrderNo.Contains(filterSiparisNo, StringComparison.OrdinalIgnoreCase))
                    .Select(o => o.Id)
                    .ToList();
                
                cuttings = cuttings.Where(c => c.OrderId.HasValue && orderIds.Contains(c.OrderId.Value)).ToList();
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
                    cuttings = cuttings.Where(c => Math.Abs(c.Hatve - hatveValue.Value) < 0.1m).ToList();
                }
            }

            // Ölçü filtresi
            if (!string.IsNullOrEmpty(filterOlcu))
            {
                if (decimal.TryParse(filterOlcu, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal sizeValue))
                {
                    cuttings = cuttings.Where(c => Math.Abs(c.Size - sizeValue) < 0.01m).ToList();
                }
            }

            // Rulo Seri No filtresi
            if (!string.IsNullOrEmpty(filterRuloSeriNo))
            {
                cuttings = cuttings.Where(c => c.SerialNo != null && 
                    c.SerialNo.SerialNumber != null && 
                    c.SerialNo.SerialNumber.Contains(filterRuloSeriNo, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return cuttings;
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
            _cmbRuloSeriNo.SelectedIndex = -1;
            _cmbRuloSeriNo.Text = "";
            LoadData();
        }

        // DataGridView için veri modeli
        private class StockRowData
        {
            public string SiparisNo { get; set; }
            public string KesimTarihi { get; set; }
            public string Hatve { get; set; }
            public string Olcu { get; set; }
            public string RuloSeriNo { get; set; }
            public string ToplamPlakaAdedi { get; set; }
            public string KullanilanPlakaAdedi { get; set; }
            public string KalanPlakaAdedi { get; set; }
        }
    }
}

