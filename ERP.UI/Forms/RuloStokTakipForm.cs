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
                Text = "Rulo Stok Takip",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 80),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None
            };

            _mainPanel.Resize += (s, e) =>
            {
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 130;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                var entries = _materialEntryRepository.GetAll();
                var serialNos = _serialNoRepository.GetAll();
                var cuttings = _cuttingRepository.GetAll();

                LoadDataGridView(entries, serialNos, cuttings);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rulo stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataGridView(List<MaterialEntry> entries, List<SerialNo> serialNos, List<Cutting> cuttings)
        {
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
            _dataGridView.DefaultCellStyle.BackColor = ThemeColors.Surface;
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

