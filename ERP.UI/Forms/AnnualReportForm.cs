using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class AnnualReportForm : UserControl
    {
        private Panel _mainPanel;
        private TableLayoutPanel _reportTable; // Tek tablo
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;
        private int _currentYear;
        private ComboBox _cmbYear;

        // Plaka ölçüleri ve modelleri
        private readonly Dictionary<int, List<char>> _columnModels = new Dictionary<int, List<char>>
        {
            { 20, new List<char> { 'H' } },
            { 30, new List<char> { 'H', 'D', 'M', 'L' } },
            { 40, new List<char> { 'H', 'D', 'M', 'L' } },
            { 50, new List<char> { 'H', 'D', 'M', 'L' } },
            { 60, new List<char> { 'D', 'M', 'L' } },
            { 70, new List<char> { 'M', 'L' } },
            { 80, new List<char> { 'M', 'L' } },
            { 100, new List<char> { 'M', 'L' } },
            { 120, new List<char> { 'M', 'L' } },
            { 140, new List<char> { 'M', 'L' } },
            { 160, new List<char> { 'M', 'L' } },
            { 200, new List<char> { 'M', 'L' } }
        };

        private readonly int[] _columnSizes = { 20, 30, 40, 50, 60, 70, 80, 100, 120, 140, 160, 200 };

        public AnnualReportForm()
        {
            _orderRepository = new OrderRepository();
            _companyRepository = new CompanyRepository();
            _currentYear = DateTime.Now.Year;
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadReportData();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30),
                AutoScroll = true
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "📊 Yıllık Rapor",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Yıl seçimi
            var yearPanel = new Panel
            {
                Location = new Point(30, 80),
                Width = 300,
                Height = 40
            };

            var lblYear = new Label
            {
                Text = "Yıl:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            _cmbYear = new ComboBox
            {
                Location = new Point(50, 7),
                Width = 100,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Son 5 yıl ve gelecek 2 yıl
            for (int year = DateTime.Now.Year - 5; year <= DateTime.Now.Year + 2; year++)
            {
                _cmbYear.Items.Add(year);
            }
            _cmbYear.SelectedItem = _currentYear;
            _cmbYear.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbYear.SelectedItem != null)
                {
                    _currentYear = (int)_cmbYear.SelectedItem;
                    LoadReportData();
                }
            };

            yearPanel.Controls.Add(lblYear);
            yearPanel.Controls.Add(_cmbYear);

            // Scroll edilebilir panel (yatay scroll için)
            var scrollPanel = new Panel
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoScroll = true,
                HorizontalScroll = { Enabled = true, Visible = true },
                VerticalScroll = { Enabled = true, Visible = true }
            };

            // Tek tablo
            _reportTable = new TableLayoutPanel
            {
                Location = new Point(0, 0),
                AutoSize = false,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Dock = DockStyle.None
            };

            scrollPanel.Controls.Add(_reportTable);

            // Panel resize event
            _mainPanel.Resize += (s, e) =>
            {
                if (scrollPanel != null)
                {
                    scrollPanel.Width = _mainPanel.Width - 60;
                    scrollPanel.Height = _mainPanel.Height - 180;
                }
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(yearPanel);
            _mainPanel.Controls.Add(scrollPanel);

            this.Controls.Add(_mainPanel);
        }

        private void LoadReportData()
        {
            // Tabloyu temizle
            _reportTable.Controls.Clear();
            _reportTable.RowStyles.Clear();
            _reportTable.ColumnStyles.Clear();

            // Tüm firmaları al
            var allCompanies = _companyRepository.GetAll();
            
            // Tüm siparişleri al
            var allOrders = _orderRepository.GetAll();
            
            // Yıla göre filtrele
            var yearOrders = allOrders.Where(o => o.OrderDate.Year == _currentYear).ToList();

            // Her firma için verileri hesapla
            Dictionary<string, Dictionary<string, int>> companyData = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> totalData = new Dictionary<string, int>();

            foreach (var company in allCompanies)
            {
                var companyOrders = yearOrders.Where(o => o.CompanyId == company.Id).ToList();
                companyData[company.Name] = CalculateCompanyData(companyOrders);
            }

            // Toplam Alım hesapla
            foreach (var companyDataItem in companyData.Values)
            {
                foreach (var kvp in companyDataItem)
                {
                    if (!totalData.ContainsKey(kvp.Key))
                        totalData[kvp.Key] = 0;
                    totalData[kvp.Key] += kvp.Value;
                }
            }

            // Tek tabloyu oluştur
            CreateReportTable(allCompanies, companyData, totalData);
        }

        private void CreateReportTable(List<Company> allCompanies, Dictionary<string, Dictionary<string, int>> companyData, Dictionary<string, int> totalData)
        {
            // Sütun sayısını hesapla
            int totalColumns = 2; // MÜŞTERİ + TOPLAM ALIM
            foreach (var size in _columnSizes)
            {
                totalColumns += _columnModels[size].Count;
            }

            _reportTable.ColumnCount = totalColumns;
            _reportTable.RowCount = allCompanies.Count + 3; // Firmalar + Toplam Alım + 2 başlık satırı

            // Sütun genişlikleri
            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // MÜŞTERİ
            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // TOPLAM ALIM
            foreach (var size in _columnSizes)
            {
                foreach (var model in _columnModels[size])
                {
                    _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
                }
            }

            // Satır yükseklikleri
            _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Başlık satırı
            _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Model harfleri satırı
            _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Toplam Alım satırı
            for (int i = 0; i < allCompanies.Count; i++)
            {
                _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            }

            int totalHeight = 35 + 30 + 30 + (allCompanies.Count * 28);
            int totalWidth = 120 + 100;
            foreach (var size in _columnSizes)
            {
                totalWidth += _columnModels[size].Count * 70;
            }
            _reportTable.Width = totalWidth;
            _reportTable.Height = totalHeight;
            _reportTable.AutoSize = false;
            _reportTable.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

            // Başlık satırı (0. satır) - MÜŞTERİ ve TOPLAM ALIM sadece bu satırda
            AddHeaderCell(_reportTable, "MÜŞTERİ", 0, 0, Color.FromArgb(68, 114, 196), Color.White);
            AddHeaderCell(_reportTable, "TOPLAM ALIM", 0, 1, Color.FromArgb(68, 114, 196), Color.White);
            
            // Plaka ölçüleri - TEK SATIRDA (0. satır)
            int colIndex = 2;
            foreach (var size in _columnSizes)
            {
                int colspan = _columnModels[size].Count;
                AddMergedHeaderCell(_reportTable, size.ToString(), 0, colIndex, colspan, Color.FromArgb(255, 192, 0), Color.Black);
                colIndex += colspan;
            }

            // Model harfleri satırı (1. satır) - MÜŞTERİ ve TOPLAM ALIM sütunları için görünmez hücreler
            // Böylece harfler sayıların tam altına geliyor ve boş kutular görünmüyor
            var invisibleLabel1 = new Label
            {
                Text = "",
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill
            };
            var invisibleLabel2 = new Label
            {
                Text = "",
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill
            };
            try
            {
                _reportTable.Controls.Add(invisibleLabel1, 0, 1);
                _reportTable.Controls.Add(invisibleLabel2, 1, 1);
            }
            catch (ArgumentException) { }
            
            // Harfler ekleniyor
            colIndex = 2;
            foreach (var size in _columnSizes)
            {
                foreach (var model in _columnModels[size])
                {
                    if (colIndex < _reportTable.ColumnCount)
                    {
                        AddHeaderCell(_reportTable, model.ToString(), 1, colIndex, Color.FromArgb(198, 224, 180), Color.Black);
                        colIndex++;
                    }
                }
            }

            // Toplam Alım satırı
            int rowIndex = 2;
            AddHeaderCell(_reportTable, "TOPLAM ALIM", rowIndex, 0, Color.FromArgb(255, 242, 204), Color.Black);
            int totalSum = 0;
            colIndex = 2;
            foreach (var size in _columnSizes)
            {
                foreach (var model in _columnModels[size])
                {
                    if (colIndex < _reportTable.ColumnCount)
                    {
                        string key = $"{size}-{model}";
                        int value = totalData.ContainsKey(key) ? totalData[key] : 0;
                        totalSum += value;
                        AddDataCell(_reportTable, value.ToString(), rowIndex, colIndex, Color.Black, Color.FromArgb(255, 242, 204));
                        colIndex++;
                    }
                }
            }
            AddDataCell(_reportTable, totalSum.ToString(), rowIndex, 1, Color.Black, Color.FromArgb(255, 242, 204));
            rowIndex++;

            // Firma satırları
            foreach (var company in allCompanies)
            {
                AddHeaderCell(_reportTable, company.Name, rowIndex, 0, Color.FromArgb(217, 217, 217), Color.Black);
                colIndex = 2;
                var data = companyData.ContainsKey(company.Name) ? companyData[company.Name] : new Dictionary<string, int>();
                int companySum = 0;
                foreach (var size in _columnSizes)
                {
                    foreach (var model in _columnModels[size])
                    {
                        if (colIndex < _reportTable.ColumnCount)
                        {
                            string key = $"{size}-{model}";
                            int value = data.ContainsKey(key) ? data[key] : 0;
                            companySum += value;
                            AddDataCell(_reportTable, value.ToString(), rowIndex, colIndex, Color.Black, Color.FromArgb(217, 217, 217));
                            colIndex++;
                        }
                    }
                }
                AddDataCell(_reportTable, companySum.ToString(), rowIndex, 1, Color.Black, Color.FromArgb(217, 217, 217));
                rowIndex++;
            }
        }

        private Dictionary<string, int> CalculateCompanyData(List<Order> orders)
        {
            var data = new Dictionary<string, int>();

            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.ProductCode))
                    continue;

                // Ürün kodundan model harfini çıkar (örn: TREX-CR-LG-1422-1900-030 -> L)
                char? modelLetter = ExtractModelLetter(order.ProductCode);
                if (!modelLetter.HasValue)
                    continue;

                // Ürün kodundan plaka ölçüsünü çıkar (örn: TREX-CR-LG-1422-1900-030 -> 1422)
                int? plateSize = ExtractPlateSize(order.ProductCode);
                if (!plateSize.HasValue)
                    continue;

                // Plaka ölçüsü hesaplama: <= 1150 ise aynen, > 1150 ise /2
                int calculatedPlateSize = plateSize.Value;
                if (calculatedPlateSize > 1150)
                {
                    calculatedPlateSize = calculatedPlateSize / 2;
                }

                // Hangi sütuna ait olduğunu bul
                int? columnSize = FindColumnSize(calculatedPlateSize);
                if (!columnSize.HasValue)
                    continue;

                // Model harfi bu sütun için geçerli mi kontrol et
                if (!_columnModels.ContainsKey(columnSize.Value) || !_columnModels[columnSize.Value].Contains(modelLetter.Value))
                    continue;

                // Key oluştur (örn: "30-H", "40-L")
                string key = $"{columnSize.Value}-{modelLetter.Value}";
                
                if (!data.ContainsKey(key))
                    data[key] = 0;
                
                data[key] += order.Quantity;
            }

            return data;
        }

        private char? ExtractModelLetter(string productCode)
        {
            // Ürün kodu formatı: TREX-CR-LG-1422-1900-030
            // Model harfi: LG'den L'yi al
            try
            {
                var parts = productCode.Split('-');
                if (parts.Length >= 3)
                {
                    string modelPart = parts[2]; // LG, MS, DS, vb.
                    if (modelPart.Length > 0)
                    {
                        return char.ToUpper(modelPart[0]); // İlk harfi al (L, M, D, S, G)
                    }
                }
            }
            catch { }
            return null;
        }

        private int? ExtractPlateSize(string productCode)
        {
            // Ürün kodu formatı: TREX-CR-LG-1422-1900-030
            // Plaka ölçüsü: 1422
            try
            {
                var parts = productCode.Split('-');
                if (parts.Length >= 4)
                {
                    if (int.TryParse(parts[3], out int plateSize))
                    {
                        return plateSize;
                    }
                }
            }
            catch { }
            return null;
        }

        private int? FindColumnSize(int calculatedPlateSize)
        {
            // Plaka ölçüsüne göre en yakın sütunu bul
            // Sıralı olarak kontrol et, ilk eşit veya büyük olanı döndür
            foreach (var size in _columnSizes)
            {
                if (calculatedPlateSize <= size)
                {
                    return size;
                }
            }
            // Eğer tüm sütunlardan büyükse, en büyük sütunu (200) döndür
            return _columnSizes.Length > 0 ? _columnSizes[_columnSizes.Length - 1] : 200;
        }

        private void AddHeaderCell(TableLayoutPanel table, string text, int row, int column, Color backColor, Color foreColor)
        {
            // Sınır kontrolü
            if (row < 0 || row >= table.RowCount || column < 0 || column >= table.ColumnCount)
                return;

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = foreColor,
                BackColor = backColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };

            try
            {
                table.Controls.Add(label, column, row);
            }
            catch (ArgumentException)
            {
                // GrowStyle FixedSize olduğu için hata oluşabilir, görmezden gel
            }
        }

        private void AddMergedHeaderCell(TableLayoutPanel table, string text, int row, int startColumn, int colspan, Color backColor, Color foreColor)
        {
            // Sınır kontrolü
            if (row < 0 || row >= table.RowCount || startColumn < 0 || startColumn >= table.ColumnCount)
                return;
            
            if (startColumn + colspan > table.ColumnCount)
                colspan = table.ColumnCount - startColumn; // Colspan'ı sınırla

            if (colspan <= 0)
                return;

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = foreColor,
                BackColor = backColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };

            try
            {
                table.Controls.Add(label, startColumn, row);
                if (colspan > 1)
                {
                    table.SetColumnSpan(label, colspan);
                }
            }
            catch (ArgumentException)
            {
                // GrowStyle FixedSize olduğu için hata oluşabilir, görmezden gel
            }
        }

        private void AddDataCell(TableLayoutPanel table, string text, int row, int column, Color foreColor, Color? backColor = null)
        {
            // Sınır kontrolü
            if (row < 0 || row >= table.RowCount || column < 0 || column >= table.ColumnCount)
                return;

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = foreColor,
                BackColor = backColor ?? ThemeColors.Surface,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };

            try
            {
                table.Controls.Add(label, column, row);
            }
            catch (ArgumentException)
            {
                // GrowStyle FixedSize olduğu için hata oluşabilir, görmezden gel
            }
        }

        private void AddMergedHeaderCellVertical(TableLayoutPanel table, string text, int startRow, int column, int rowspan, Color backColor, Color foreColor)
        {
            // Sınır kontrolü
            if (startRow < 0 || startRow >= table.RowCount || column < 0 || column >= table.ColumnCount)
                return;
            
            if (startRow + rowspan > table.RowCount)
                rowspan = table.RowCount - startRow; // Rowspan'ı sınırla

            if (rowspan <= 0)
                return;

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = foreColor,
                BackColor = backColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };

            try
            {
                table.Controls.Add(label, column, startRow);
                if (rowspan > 1)
                {
                    table.SetRowSpan(label, rowspan);
                }
            }
            catch (ArgumentException)
            {
                // GrowStyle FixedSize olduğu için hata oluşabilir, görmezden gel
            }
        }
    }
}

