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
    public partial class RuloStokTakipForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private MaterialEntryRepository _materialEntryRepository;
        private SerialNoRepository _serialNoRepository;
        private CuttingRepository _cuttingRepository;
        
        // Filtreleme kontrolleri
        private ComboBox _cmbRuloSeriNo;
        private ComboBox _cmbMalzeme;
        private ComboBox _cmbKalinlik;
        private ComboBox _cmbOlcu;
        private Button _btnFiltrele;
        private Button _btnFiltreleriTemizle;

        // Static event - kesim yapıldığında tetiklenecek
        public static event EventHandler CuttingSaved;

        public RuloStokTakipForm()
        {
            _materialEntryRepository = new MaterialEntryRepository();
            _serialNoRepository = new SerialNoRepository();
            _cuttingRepository = new CuttingRepository();
            
            // Event'i dinle
            CuttingSaved += RuloStokTakipForm_CuttingSaved;
            
            InitializeCustomComponents();
        }

        private void RuloStokTakipForm_CuttingSaved(object sender, EventArgs e)
        {
            // Kesim yapıldığında verileri yenile
            try
            {
                if (this.IsHandleCreated)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            LoadData();
                        });
                    }
                    else
                    {
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et
                System.Diagnostics.Debug.WriteLine($"RuloStokTakipForm yenileme hatası: {ex.Message}");
            }
        }

        public void RefreshData()
        {
            LoadData();
        }

        public static void NotifyCuttingSaved()
        {
            // Kesim yapıldığında bu metod çağrılacak
            CuttingSaved?.Invoke(null, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Event handler'ı kaldır
                CuttingSaved -= RuloStokTakipForm_CuttingSaved;
            }
            base.Dispose(disposing);
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
                Text = "Rulo Stok Takip",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Filtreleme paneli
            var filterPanel = new Panel
            {
                Location = new Point(20, 60),
                Width = _mainPanel.Width - 40,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            // Rulo Seri No filtresi
            var lblRuloSeriNo = new Label
            {
                Text = "Rulo Seri No:",
                Location = new Point(0, 15),
                Width = 100,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbRuloSeriNo = new ComboBox
            {
                Location = new Point(105, 12),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Malzeme filtresi
            var lblMalzeme = new Label
            {
                Text = "Malzeme:",
                Location = new Point(295, 15),
                Width = 80,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbMalzeme = new ComboBox
            {
                Location = new Point(380, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Kalınlık filtresi
            var lblKalinlik = new Label
            {
                Text = "Kalınlık:",
                Location = new Point(540, 15),
                Width = 70,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbKalinlik = new ComboBox
            {
                Location = new Point(615, 12),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Ölçü filtresi
            var lblOlcu = new Label
            {
                Text = "Ölçü:",
                Location = new Point(725, 15),
                Width = 50,
                ForeColor = ThemeColors.TextPrimary
            };
            _cmbOlcu = new ComboBox
            {
                Location = new Point(780, 12),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            // Filtrele butonu
            _btnFiltrele = new Button
            {
                Text = "🔍 Filtrele",
                Location = new Point(890, 10),
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
                Location = new Point(1000, 10),
                Width = 100,
                Height = 30,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnFiltreleriTemizle.FlatAppearance.BorderSize = 0;

            filterPanel.Controls.Add(lblRuloSeriNo);
            filterPanel.Controls.Add(_cmbRuloSeriNo);
            filterPanel.Controls.Add(lblMalzeme);
            filterPanel.Controls.Add(_cmbMalzeme);
            filterPanel.Controls.Add(lblKalinlik);
            filterPanel.Controls.Add(_cmbKalinlik);
            filterPanel.Controls.Add(lblOlcu);
            filterPanel.Controls.Add(_cmbOlcu);
            filterPanel.Controls.Add(_btnFiltrele);
            filterPanel.Controls.Add(_btnFiltreleriTemizle);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(20, 120),
                Width = _mainPanel.Width - 40,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            _mainPanel.Resize += (s, e) =>
            {
                filterPanel.Width = _mainPanel.Width - 40;
                _dataGridView.Width = _mainPanel.Width - 40;
                _dataGridView.Height = _mainPanel.Height - 180;
            };

            // Event handlers
            _btnFiltrele.Click += BtnFiltrele_Click;
            _btnFiltreleriTemizle.Click += BtnFiltreleriTemizle_Click;

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
                var entries = _materialEntryRepository.GetAll();
                var serialNos = _serialNoRepository.GetAll();
                var cuttings = _cuttingRepository.GetAll();

                // Filtreleme kontrollerini sadece ilk yüklemede doldur
                if (!_filterControlsLoaded)
                {
                    LoadFilterControls(entries, serialNos);
                    _filterControlsLoaded = true;
                }

                LoadDataGridView(entries, serialNos, cuttings);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rulo stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilterControls(List<MaterialEntry> entries, List<SerialNo> serialNos)
        {
            // Rulo Seri No listesini doldur
            var serialNumbers = entries
                .Where(e => e.SerialNoId.HasValue && e.SerialNo != null)
                .Select(e => e.SerialNo.SerialNumber)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            _cmbRuloSeriNo.Items.Clear();
            _cmbRuloSeriNo.Items.Add(""); // Tümü seçeneği
            foreach (var sn in serialNumbers)
            {
                _cmbRuloSeriNo.Items.Add(sn);
            }

            // Malzeme listesini doldur
            var materials = entries
                .Where(e => !string.IsNullOrEmpty(e.MaterialSize))
                .Select(e => e.MaterialSize)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            
            _cmbMalzeme.Items.Clear();
            _cmbMalzeme.Items.Add(""); // Tümü seçeneği
            foreach (var material in materials)
            {
                _cmbMalzeme.Items.Add(material);
            }

            // Kalınlık listesini doldur
            var thicknesses = entries
                .Where(e => e.Thickness > 0)
                .Select(e => e.Thickness.ToString("F3", CultureInfo.InvariantCulture))
                .Distinct()
                .OrderBy(t => decimal.Parse(t, CultureInfo.InvariantCulture))
                .ToList();
            
            _cmbKalinlik.Items.Clear();
            _cmbKalinlik.Items.Add(""); // Tümü seçeneği
            foreach (var thickness in thicknesses)
            {
                _cmbKalinlik.Items.Add(thickness);
            }

            // Ölçü listesini doldur
            var sizes = entries
                .Where(e => e.Size > 0)
                .Select(e => e.Size.ToString())
                .Distinct()
                .OrderBy(s => int.Parse(s))
                .ToList();
            
            _cmbOlcu.Items.Clear();
            _cmbOlcu.Items.Add(""); // Tümü seçeneği
            foreach (var size in sizes)
            {
                _cmbOlcu.Items.Add(size);
            }
        }

        private void BtnFiltrele_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnFiltreleriTemizle_Click(object sender, EventArgs e)
        {
            _cmbRuloSeriNo.SelectedIndex = -1;
            _cmbRuloSeriNo.Text = "";
            _cmbMalzeme.SelectedIndex = -1;
            _cmbMalzeme.Text = "";
            _cmbKalinlik.SelectedIndex = -1;
            _cmbKalinlik.Text = "";
            _cmbOlcu.SelectedIndex = -1;
            _cmbOlcu.Text = "";
            LoadData();
        }

        private void LoadDataGridView(List<MaterialEntry> entries, List<SerialNo> serialNos, List<Cutting> cuttings)
        {
            // Filtreleme kriterlerini al
            string filterRuloSeriNo = _cmbRuloSeriNo?.Text?.Trim() ?? "";
            string filterMalzeme = _cmbMalzeme?.Text?.Trim() ?? "";
            string filterKalinlik = _cmbKalinlik?.Text?.Trim() ?? "";
            string filterOlcu = _cmbOlcu?.Text?.Trim() ?? "";

            // Önce hangi SerialNo'ların gösterilmesi gerektiğini belirle
            HashSet<Guid> allowedSerialNoIds = null;

            // Rulo Seri No filtresi
            if (!string.IsNullOrEmpty(filterRuloSeriNo))
            {
                var filteredSerialNos = serialNos
                    .Where(sn => sn.SerialNumber != null && sn.SerialNumber.Contains(filterRuloSeriNo, StringComparison.OrdinalIgnoreCase))
                    .Select(sn => sn.Id)
                    .ToHashSet();
                
                if (allowedSerialNoIds == null)
                    allowedSerialNoIds = filteredSerialNos;
                else
                    allowedSerialNoIds.IntersectWith(filteredSerialNos);
            }

            // Malzeme, Kalınlık ve Ölçü filtreleri için entries'i filtrele ve SerialNo'ları bul
            var filteredEntries = entries.Where(e => e.SerialNoId.HasValue && e.IsActive).ToList();

            if (!string.IsNullOrEmpty(filterMalzeme))
            {
                filteredEntries = filteredEntries.Where(e => e.MaterialSize != null && e.MaterialSize.Contains(filterMalzeme, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(filterKalinlik))
            {
                if (decimal.TryParse(filterKalinlik, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal thicknessValue))
                {
                    filteredEntries = filteredEntries.Where(e => Math.Abs(e.Thickness - thicknessValue) < 0.001m).ToList();
                }
            }

            if (!string.IsNullOrEmpty(filterOlcu))
            {
                if (int.TryParse(filterOlcu, out int sizeValue))
                {
                    filteredEntries = filteredEntries.Where(e => e.Size == sizeValue).ToList();
                }
            }

            // Filtrelenmiş entries'lerden SerialNo'ları al
            var serialNosFromEntries = filteredEntries
                .Where(e => e.SerialNoId.HasValue)
                .Select(e => e.SerialNoId.Value)
                .Distinct()
                .ToHashSet();

            // SerialNo filtrelerini birleştir
            if (allowedSerialNoIds == null)
                allowedSerialNoIds = serialNosFromEntries;
            else
                allowedSerialNoIds.IntersectWith(serialNosFromEntries);

            // Eğer hiçbir filtre yoksa, tüm SerialNo'ları göster
            if (string.IsNullOrEmpty(filterRuloSeriNo) && string.IsNullOrEmpty(filterMalzeme) && 
                string.IsNullOrEmpty(filterKalinlik) && string.IsNullOrEmpty(filterOlcu))
            {
                allowedSerialNoIds = null; // null = tüm SerialNo'lar
            }

            // Şimdi entries ve cuttings'i filtrele (sadece izin verilen SerialNo'lara ait olanlar)
            // ÖNEMLİ: Eğer bir SerialNo filtrelenmiş entries'lerde varsa, o SerialNo'ya ait TÜM entries ve cuttings gösterilmeli
            if (allowedSerialNoIds != null && allowedSerialNoIds.Count > 0)
            {
                // Orijinal entries ve cuttings listelerinden filtrele (filtrelenmiş entries'den değil!)
                var originalEntries = _materialEntryRepository.GetAll();
                var originalCuttings = _cuttingRepository.GetAll();
                
                entries = originalEntries.Where(e => e.SerialNoId.HasValue && allowedSerialNoIds.Contains(e.SerialNoId.Value) && e.IsActive).ToList();
                cuttings = originalCuttings.Where(c => c.SerialNoId.HasValue && allowedSerialNoIds.Contains(c.SerialNoId.Value) && c.IsActive).ToList();
            }
            else if (allowedSerialNoIds != null && allowedSerialNoIds.Count == 0)
            {
                // Hiçbir SerialNo eşleşmediyse boş liste göster
                entries = new List<MaterialEntry>();
                cuttings = new List<Cutting>();
            }

            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();

            _dataGridView.AutoGenerateColumns = false;

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SerialNumber",
                HeaderText = "Rulo Seri No",
                Name = "SerialNumber",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaterialSize",
                HeaderText = "Malzeme",
                Name = "MaterialSize",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = "Tarih",
                Name = "Date",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Company",
                HeaderText = "Firma",
                Name = "Company",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Thickness",
                HeaderText = "Kalınlık",
                Name = "Thickness",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Size",
                HeaderText = "Ölçü",
                Name = "Size",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransactionType",
                HeaderText = "İşlem Tipi",
                Name = "TransactionType",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Giriş Kg",
                Name = "Quantity",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CutKg",
                HeaderText = "Kesilen Kg",
                Name = "CutKg",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WasteKg",
                HeaderText = "Hurda Kg",
                Name = "WasteKg",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RemainingKg",
                HeaderText = "Kalan Kg",
                Name = "RemainingKg",
                Width = 100
            });

            var stockData = new List<StockRowData>();

            // Tüm işlemleri (giriş ve kesim) birleştir ve tarih sırasına göre sırala
            var allTransactions = new List<(DateTime Date, bool IsEntry, object Data)>();

            // Giriş işlemlerini ekle
            foreach (var entry in entries.Where(e => e.SerialNoId.HasValue && e.IsActive))
            {
                allTransactions.Add((entry.EntryDate, true, entry));
            }

            // Kesim işlemlerini ekle
            foreach (var cutting in cuttings.Where(c => c.SerialNoId.HasValue && c.IsActive))
            {
                allTransactions.Add((cutting.CuttingDate, false, cutting));
            }

            // Tarihe göre sırala (en eski önce)
            allTransactions = allTransactions.OrderBy(t => t.Date).ToList();

            // SerialNo'ya göre grupla
            var serialNoGroups = allTransactions.GroupBy(t =>
            {
                if (t.IsEntry)
                    return ((MaterialEntry)t.Data).SerialNoId.Value;
                else
                    return ((Cutting)t.Data).SerialNoId.Value;
            });

            foreach (var serialNoGroup in serialNoGroups)
            {
                var serialNoId = serialNoGroup.Key;
                var serialNo = serialNos.FirstOrDefault(sn => sn.Id == serialNoId);
                string serialNumber = serialNo?.SerialNumber ?? "";

                if (string.IsNullOrEmpty(serialNumber))
                {
                    var firstEntry = entries.FirstOrDefault(e => e.SerialNoId == serialNoId);
                    if (firstEntry?.SerialNo != null)
                        serialNumber = firstEntry.SerialNo.SerialNumber;
                    else
                    {
                        var firstCutting = cuttings.FirstOrDefault(c => c.SerialNoId == serialNoId);
                        if (firstCutting?.SerialNo != null)
                            serialNumber = firstCutting.SerialNo.SerialNumber;
                        else
                            serialNumber = serialNoId.ToString().Substring(0, 8) + "...";
                    }
                }

                // Bu SerialNo için MaterialEntry bilgilerini bul
                var firstEntryForInfo = entries
                    .Where(e => e.SerialNoId == serialNoId && e.IsActive)
                    .FirstOrDefault();

                string materialSize = firstEntryForInfo?.MaterialSize ?? "";
                string company = firstEntryForInfo?.Supplier?.Name ?? "";
                decimal thickness = firstEntryForInfo?.Thickness ?? 0m;
                int defaultSize = firstEntryForInfo?.Size ?? 0;

                // Kümülatif toplamları takip et
                decimal cumulativeEntryKg = 0;
                decimal cumulativeCutKg = 0;
                decimal cumulativeWasteKg = 0;

                // Her işlemi sırayla işle (tarihe göre sıralı - en eski önce)
                var sortedTransactions = serialNoGroup.OrderBy(t => t.Date).ToList();
                foreach (var transaction in sortedTransactions)
                {
                    if (transaction.IsEntry)
                    {
                        var entry = (MaterialEntry)transaction.Data;
                        
                        // Bu girişten önceki toplam giriş kg
                        decimal entryKgBefore = cumulativeEntryKg;
                        
                        // Bu girişi ekle
                        cumulativeEntryKg += entry.Quantity;
                        
                        // Bu girişten sonraki kalan kg (kesilen + hurda düşülür)
                        decimal remainingKgAfter = cumulativeEntryKg - cumulativeCutKg - cumulativeWasteKg;

                        stockData.Add(new StockRowData
                        {
                            SerialNumber = serialNumber,
                            MaterialSize = entry.MaterialSize ?? materialSize,
                            Date = entry.EntryDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                            Company = entry.Supplier?.Name ?? company,
                            Thickness = entry.Thickness > 0 ? entry.Thickness.ToString("F3", CultureInfo.InvariantCulture) : (thickness > 0 ? thickness.ToString("F3", CultureInfo.InvariantCulture) : ""),
                            Size = entry.Size > 0 ? entry.Size.ToString() : (defaultSize > 0 ? defaultSize.ToString() : ""),
                            TransactionType = "Giriş",
                            Quantity = entry.Quantity.ToString("F3", CultureInfo.InvariantCulture),
                            CutKg = "",
                            WasteKg = "",
                            RemainingKg = remainingKgAfter.ToString("F3", CultureInfo.InvariantCulture)
                        });
                    }
                    else
                    {
                        var cutting = (Cutting)transaction.Data;
                        
                        // Bu kesimi ekle (kesilen + hurda)
                        cumulativeCutKg += cutting.CutKg;
                        cumulativeWasteKg += cutting.WasteKg;
                        
                        // Bu kesimden sonraki kalan kg (kesilen + hurda düşülür)
                        decimal remainingKgAfter = cumulativeEntryKg - cumulativeCutKg - cumulativeWasteKg;
                        
                        // Giriş Kg = Kesilen Kg + Hurda Kg + Kalan Kg
                        decimal entryKg = cutting.CutKg + cutting.WasteKg + remainingKgAfter;

                        stockData.Add(new StockRowData
                        {
                            SerialNumber = serialNumber,
                            MaterialSize = materialSize,
                            Date = cutting.CuttingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                            Company = company,
                            Thickness = thickness > 0 ? thickness.ToString("F3", CultureInfo.InvariantCulture) : "",
                            Size = ((int)cutting.Size).ToString(),
                            TransactionType = "Kesim",
                            Quantity = entryKg > 0 ? entryKg.ToString("F3", CultureInfo.InvariantCulture) : "", // Kesilen + Hurda + Kalan = Giriş
                            CutKg = cutting.CutKg.ToString("F3", CultureInfo.InvariantCulture),
                            WasteKg = cutting.WasteKg > 0 ? cutting.WasteKg.ToString("F3", CultureInfo.InvariantCulture) : "",
                            RemainingKg = remainingKgAfter.ToString("F3", CultureInfo.InvariantCulture)
                        });
                    }
                }
            }

            // Tarihe göre sırala (en yeni önce)
            stockData = stockData.OrderByDescending(s => 
            {
                // Türkçe tarih formatını parse et (dd.MM.yyyy)
                if (DateTime.TryParseExact(s.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;
                return DateTime.MinValue;
            }).ToList();

            _dataGridView.DataSource = stockData;

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

        private class StockRowData
        {
            public string SerialNumber { get; set; }
            public string MaterialSize { get; set; }
            public string Date { get; set; }
            public string Company { get; set; }
            public string Thickness { get; set; }
            public string Size { get; set; }
            public string TransactionType { get; set; }
            public string Quantity { get; set; }
            public string CutKg { get; set; }
            public string WasteKg { get; set; }
            public string RemainingKg { get; set; }
        }
    }
}

