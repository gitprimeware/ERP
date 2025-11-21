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
    public partial class GeneralReportForm : UserControl
    {
        private Panel _mainPanel;
        private TabControl _tabControl;
        private OrderRepository _orderRepository;
        private int _currentYear;
        private ComboBox _cmbYear;

        // Plaka ölçüleri ve modeller
        private readonly int[] _plateSizes = { 20, 30, 40, 50, 60, 70, 80, 100, 120, 140, 160, 200 };
        private readonly char[] _models = { 'H', 'D', 'M', 'L' };
        
        // Kombinasyonlar
        private readonly string[] _combinations = {
            "H20", "H30", "H40", "D30", "D40", "D50", "D60",
            "M30", "M40", "M50", "M60", "M70", "M80", "M100", "M120", "M140", "M160", "M200",
            "L50", "L60", "L70", "L80", "L100", "L120", "L140", "L160", "L200"
        };

        public GeneralReportForm()
        {
            _orderRepository = new OrderRepository();
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
                Text = "📊 Genel Rapor",
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

            // TabControl
            _tabControl = new TabControl
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 10F)
            };

            // Tab sayfaları
            var tab1 = new TabPage("Tablo");
            var tab2 = new TabPage("Daire Grafik");
            var tab3 = new TabPage("Kombinasyonlar");
            var tab4 = new TabPage("Kombinasyon Grafik");

            _tabControl.TabPages.Add(tab1);
            _tabControl.TabPages.Add(tab2);
            _tabControl.TabPages.Add(tab3);
            _tabControl.TabPages.Add(tab4);

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(yearPanel);
            _mainPanel.Controls.Add(_tabControl);

            _mainPanel.Resize += (s, e) =>
            {
                if (_tabControl != null)
                {
                    _tabControl.Width = _mainPanel.Width - 60;
                    _tabControl.Height = _mainPanel.Height - 180;
                }
            };

            this.Controls.Add(_mainPanel);
        }

        private void LoadReportData()
        {
            var allOrders = _orderRepository.GetAll();
            var yearOrders = allOrders.Where(o => o.OrderDate.Year == _currentYear).ToList();

            // Verileri hesapla
            var tableData = CalculateTableData(yearOrders);
            var combinationData = CalculateCombinationData(yearOrders);

            // Tab 1: Tablo
            CreateTableTab(tableData);

            // Tab 2: Daire Grafik
            CreatePieChartTab(tableData);

            // Tab 3: Kombinasyonlar Tablosu
            CreateCombinationTableTab(combinationData);

            // Tab 4: Kombinasyon Grafik
            CreateCombinationChartTab(combinationData);
        }

        private Dictionary<string, int> CalculateTableData(List<Order> orders)
        {
            var data = new Dictionary<string, int>();

            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.ProductCode))
                    continue;

                char? modelLetter = ExtractModelLetter(order.ProductCode);
                int? plateSize = ExtractPlateSize(order.ProductCode);
                
                if (!modelLetter.HasValue || !plateSize.HasValue)
                    continue;

                int calculatedPlateSize = plateSize.Value;
                if (calculatedPlateSize > 1150)
                    calculatedPlateSize /= 2;

                int? columnSize = FindColumnSize(calculatedPlateSize);
                if (!columnSize.HasValue)
                    continue;

                string key = $"{columnSize.Value}-{modelLetter.Value}";
                if (!data.ContainsKey(key))
                    data[key] = 0;
                data[key] += order.Quantity;
            }

            return data;
        }

        private Dictionary<string, int> CalculateCombinationData(List<Order> orders)
        {
            var data = new Dictionary<string, int>();

            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.ProductCode))
                    continue;

                char? modelLetter = ExtractModelLetter(order.ProductCode);
                int? plateSize = ExtractPlateSize(order.ProductCode);
                
                if (!modelLetter.HasValue || !plateSize.HasValue)
                    continue;

                int calculatedPlateSize = plateSize.Value;
                if (calculatedPlateSize > 1150)
                    calculatedPlateSize /= 2;

                int? columnSize = FindColumnSize(calculatedPlateSize);
                if (!columnSize.HasValue)
                    continue;

                string combination = $"{modelLetter.Value}{columnSize.Value}";
                if (!data.ContainsKey(combination))
                    data[combination] = 0;
                data[combination] += order.Quantity;
            }

            return data;
        }

        private void CreateTableTab(Dictionary<string, int> data)
        {
            var tab = _tabControl.TabPages[0];
            tab.Controls.Clear();

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var table = new TableLayoutPanel
            {
                Location = new Point(20, 20),
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                ColumnCount = 5, // Plaka ölçüsü + H + D + M + L
                RowCount = _plateSizes.Length + 1
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            for (int i = 0; i < 4; i++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            // Satır yükseklikleri - tüm satırlar için sabit yükseklik
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Başlık
            for (int i = 0; i < _plateSizes.Length; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Tüm veri satırları aynı yükseklik
            }

            // Başlık satırı
            AddTableHeaderCell(table, "Plaka Ölçüsü", 0, 0);
            AddTableHeaderCell(table, "H", 0, 1);
            AddTableHeaderCell(table, "D", 0, 2);
            AddTableHeaderCell(table, "M", 0, 3);
            AddTableHeaderCell(table, "L", 0, 4);

            // Veri satırları
            for (int i = 0; i < _plateSizes.Length; i++)
            {
                int size = _plateSizes[i];
                int row = i + 1;
                
                AddTableDataCell(table, size.ToString(), row, 0);
                
                foreach (var model in _models)
                {
                    string key = $"{size}-{model}";
                    int value = data.ContainsKey(key) ? data[key] : 0;
                    AddTableDataCell(table, value.ToString(), row, Array.IndexOf(_models, model) + 1);
                }
            }

            scrollPanel.Controls.Add(table);
            tab.Controls.Add(scrollPanel);
        }

        private void CreatePieChartTab(Dictionary<string, int> data)
        {
            var tab = _tabControl.TabPages[1];
            tab.Controls.Clear();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Plaka Ölçüleri Dağılımı",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Toplam hesapla
            int grandTotal = 0;
            var sizeTotals = new Dictionary<int, int>();
            foreach (var size in _plateSizes)
            {
                int total = 0;
                foreach (var model in _models)
                {
                    string key = $"{size}-{model}";
                    if (data.ContainsKey(key))
                        total += data[key];
                }
                sizeTotals[size] = total;
                grandTotal += total;
            }

            if (grandTotal == 0)
            {
                var noDataLabel = new Label
                {
                    Text = "Gösterilecek veri bulunamadı.",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(20, 100)
                };
                panel.Controls.Add(titleLabel);
                panel.Controls.Add(noDataLabel);
                tab.Controls.Add(panel);
                return;
            }

            // Daire grafik paneli
            var pieChartPanel = new Panel
            {
                Location = new Point(20, 60),
                Width = 500,
                Height = 500,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pieChartPanel.Paint += (s, e) =>
            {
                DrawPieChart(e.Graphics, sizeTotals, grandTotal, pieChartPanel.ClientRectangle);
            };

            // Legend paneli
            var legendPanel = new FlowLayoutPanel
            {
                Location = new Point(540, 60),
                Width = panel.Width - 560,
                Height = panel.Height - 80,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown
            };

            foreach (var size in _plateSizes)
            {
                int total = sizeTotals[size];
                if (total > 0)
                {
                    double percentage = (double)total / grandTotal * 100;
                    
                    var itemPanel = new Panel
                    {
                        Width = 250,
                        Height = 30,
                        BackColor = ThemeColors.Surface,
                        Margin = new Padding(0, 0, 0, 5)
                    };

                    var colorBox = new Panel
                    {
                        Location = new Point(5, 5),
                        Width = 20,
                        Height = 20,
                        BackColor = GetColorForSize(size)
                    };

                    var sizeLabel = new Label
                    {
                        Text = $"Plaka {size}: {total} adet ({percentage:F1}%)",
                        Font = new Font("Segoe UI", 10F),
                        ForeColor = ThemeColors.TextPrimary,
                        AutoSize = true,
                        Location = new Point(30, 7)
                    };

                    itemPanel.Controls.Add(colorBox);
                    itemPanel.Controls.Add(sizeLabel);
                    legendPanel.Controls.Add(itemPanel);
                }
            }

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(pieChartPanel);
            panel.Controls.Add(legendPanel);
            tab.Controls.Add(panel);
        }

        private void DrawPieChart(Graphics g, Dictionary<int, int> sizeTotals, int grandTotal, Rectangle bounds)
        {
            if (grandTotal == 0) return;

            int centerX = bounds.Width / 2;
            int centerY = bounds.Height / 2;
            int radius = Math.Min(bounds.Width, bounds.Height) / 2 - 40;

            float startAngle = -90; // Üstten başla
            float totalAngle = 360;

            foreach (var size in _plateSizes)
            {
                int total = sizeTotals[size];
                if (total > 0)
                {
                    float sweepAngle = (float)total / grandTotal * totalAngle;
                    
                    using (var brush = new SolidBrush(GetColorForSize(size)))
                    {
                        g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sweepAngle);
                        
                        // Kenar çizgisi
                        using (var pen = new Pen(Color.White, 2))
                        {
                            g.DrawPie(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sweepAngle);
                        }
                    }

                    // Yüzde etiketi (büyük dilimler için)
                    if (sweepAngle > 5)
                    {
                        float labelAngle = startAngle + sweepAngle / 2;
                        float labelX = centerX + (float)(radius * 0.7 * Math.Cos(labelAngle * Math.PI / 180));
                        float labelY = centerY + (float)(radius * 0.7 * Math.Sin(labelAngle * Math.PI / 180));
                        
                        double percentage = (double)total / grandTotal * 100;
                        string labelText = $"{percentage:F1}%";
                        
                        var textSize = g.MeasureString(labelText, new Font("Segoe UI", 9F, FontStyle.Bold));
                        g.DrawString(labelText, new Font("Segoe UI", 9F, FontStyle.Bold), 
                            Brushes.Black, labelX - textSize.Width / 2, labelY - textSize.Height / 2);
                    }

                    startAngle += sweepAngle;
                }
            }
        }

        private Color GetColorForSize(int size)
        {
            var colors = new[] {
                Color.FromArgb(255, 99, 132), Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 206, 86), Color.FromArgb(75, 192, 192),
                Color.FromArgb(153, 102, 255), Color.FromArgb(255, 159, 64),
                Color.FromArgb(199, 199, 199), Color.FromArgb(83, 102, 255),
                Color.FromArgb(255, 99, 255), Color.FromArgb(99, 255, 132),
                Color.FromArgb(255, 205, 86), Color.FromArgb(255, 159, 64)
            };
            int index = Array.IndexOf(_plateSizes, size);
            return index >= 0 && index < colors.Length ? colors[index] : ThemeColors.Primary;
        }

        private void CreateCombinationTableTab(Dictionary<string, int> data)
        {
            var tab = _tabControl.TabPages[2];
            tab.Controls.Clear();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var table = new TableLayoutPanel
            {
                Location = new Point(20, 20),
                Width = 400,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                ColumnCount = 2,
                RowCount = _combinations.Length + 1
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Başlık
            AddTableHeaderCell(table, "Kombinasyon", 0, 0);
            AddTableHeaderCell(table, "Adet", 0, 1);

            // Veriler
            for (int i = 0; i < _combinations.Length; i++)
            {
                string combo = _combinations[i];
                int value = data.ContainsKey(combo) ? data[combo] : 0;
                AddTableDataCell(table, combo, i + 1, 0);
                AddTableDataCell(table, value.ToString(), i + 1, 1);
            }

            panel.Controls.Add(table);
            tab.Controls.Add(panel);
        }

        private void CreateCombinationChartTab(Dictionary<string, int> data)
        {
            var tab = _tabControl.TabPages[3];
            tab.Controls.Clear();

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Kombinasyon Dağılımı",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Maksimum değeri bul (Y ekseni için)
            int maxValue = data.Values.Any() ? data.Values.Max() : 10;
            if (maxValue == 0) maxValue = 10; // En az 10 olsun
            
            // Grafik boyutları - tab genişliğine göre ayarla
            int chartHeight = 450;
            int availableWidth = mainPanel.Width - 100; // Padding ve margin için
            int minBarWidth = 35;
            int chartWidth = Math.Max(availableWidth, _combinations.Length * minBarWidth);
            int barWidth = Math.Max(minBarWidth, chartWidth / _combinations.Length);
            
            // Scroll paneli
            var scrollPanel = new Panel
            {
                Location = new Point(20, 60),
                Width = mainPanel.Width - 40,
                Height = mainPanel.Height - 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoScroll = true,
                HorizontalScroll = { Enabled = true, Visible = true }
            };

            var chartPanel = new Panel
            {
                Location = new Point(0, 0),
                Width = chartWidth + 80, // Y ekseni için alan
                Height = chartHeight + 80, // X ekseni için alan
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = false
            };

            chartPanel.Paint += (s, e) =>
            {
                DrawCombinationChart(e.Graphics, data, maxValue, chartPanel.ClientRectangle);
            };

            scrollPanel.Controls.Add(chartPanel);
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(scrollPanel);
            tab.Controls.Add(mainPanel);

            // Panel resize event
            mainPanel.Resize += (s, e) =>
            {
                if (scrollPanel != null)
                {
                    scrollPanel.Width = mainPanel.Width - 40;
                    scrollPanel.Height = mainPanel.Height - 80;
                }
            };
        }

        private void DrawCombinationChart(Graphics g, Dictionary<string, int> data, int maxValue, Rectangle bounds)
        {
            int chartAreaX = 60; // Y ekseni için alan
            int chartAreaY = 20; // Üst margin
            int chartAreaWidth = bounds.Width - chartAreaX - 20; // Sağ margin
            int chartAreaHeight = bounds.Height - chartAreaY - 60; // Alt margin (X ekseni etiketleri için)
            
            int barWidth = Math.Max(30, chartAreaWidth / _combinations.Length);
            int xStart = chartAreaX;
            int baseY = chartAreaY + chartAreaHeight;

            // Y ekseni maksimum değerini hesapla (yukarı yuvarla)
            int yAxisMax = CalculateYAxisMax(maxValue);

            // Y ekseni çizgisi
            using (var pen = new Pen(Color.Gray, 1))
            {
                g.DrawLine(pen, chartAreaX, chartAreaY, chartAreaX, baseY);
                g.DrawLine(pen, chartAreaX, baseY, bounds.Width - 20, baseY); // X ekseni çizgisi
            }

            // Y ekseni etiketleri ve grid çizgileri (10 adım)
            int stepCount = 10;
            for (int i = 0; i <= stepCount; i++)
            {
                int value = (int)((double)i / stepCount * yAxisMax);
                int yPos = baseY - (int)((double)i / stepCount * chartAreaHeight);
                
                // Grid çizgisi
                using (var pen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawLine(pen, chartAreaX, yPos, bounds.Width - 20, yPos);
                }
                
                // Y ekseni etiketi
                string labelText = value.ToString();
                var textSize = g.MeasureString(labelText, new Font("Segoe UI", 8F));
                g.DrawString(labelText, new Font("Segoe UI", 8F), Brushes.Black, 
                    chartAreaX - textSize.Width - 5, yPos - textSize.Height / 2);
            }

            // Çubuklar ve X ekseni etiketleri
            for (int i = 0; i < _combinations.Length; i++)
            {
                string combo = _combinations[i];
                int value = data.ContainsKey(combo) ? data[combo] : 0;
                int barHeight = yAxisMax > 0 ? (int)((double)value / yAxisMax * chartAreaHeight) : 0;
                int xPos = xStart + i * barWidth;

                // Çubuk
                if (barHeight > 0)
                {
                    using (var brush = new SolidBrush(GetColorForSize(int.Parse(combo.Substring(1)))))
                    {
                        g.FillRectangle(brush, xPos + 2, baseY - barHeight, barWidth - 4, barHeight);
                    }
                    
                    // Kenar çizgisi
                    using (var pen = new Pen(Color.DarkGray, 1))
                    {
                        g.DrawRectangle(pen, xPos + 2, baseY - barHeight, barWidth - 4, barHeight);
                    }

                    // Değer etiketi (çubuk üzerinde)
                    string valueText = value.ToString();
                    var valueTextSize = g.MeasureString(valueText, new Font("Segoe UI", 8F, FontStyle.Bold));
                    float labelX = xPos + (barWidth - valueTextSize.Width) / 2;
                    float labelY = baseY - barHeight - valueTextSize.Height - 3;
                    
                    if (labelY < chartAreaY) labelY = baseY - barHeight + 3; // Çubuk içinde göster
                    
                    g.DrawString(valueText, new Font("Segoe UI", 8F, FontStyle.Bold), 
                        Brushes.Black, labelX, labelY);
                }

                // X ekseni etiketi
                var comboTextSize = g.MeasureString(combo, new Font("Segoe UI", 7F));
                g.DrawString(combo, new Font("Segoe UI", 7F), Brushes.Black, 
                    xPos + (barWidth - comboTextSize.Width) / 2, baseY + 5);
            }
        }

        private char? ExtractModelLetter(string productCode)
        {
            try
            {
                var parts = productCode.Split('-');
                if (parts.Length >= 3)
                {
                    string modelPart = parts[2];
                    if (modelPart.Length > 0)
                        return char.ToUpper(modelPart[0]);
                }
            }
            catch { }
            return null;
        }

        private int? ExtractPlateSize(string productCode)
        {
            try
            {
                var parts = productCode.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int plateSize))
                    return plateSize;
            }
            catch { }
            return null;
        }

        private int? FindColumnSize(int calculatedPlateSize)
        {
            foreach (var size in _plateSizes)
            {
                if (calculatedPlateSize <= size)
                    return size;
            }
            return 200;
        }

        private int CalculateYAxisMax(int maxValue)
        {
            if (maxValue == 0) return 10;
            
            // Maksimum değeri yukarı yuvarla (örneğin: 47 -> 50, 123 -> 130, 456 -> 500)
            int magnitude = (int)Math.Pow(10, Math.Floor(Math.Log10(maxValue)));
            int firstDigit = maxValue / magnitude;
            
            // İlk rakamı yukarı yuvarla
            int roundedFirst = firstDigit + 1;
            
            // Eğer 10 olduysa, bir sonraki büyüklüğe geç
            if (roundedFirst >= 10)
            {
                magnitude *= 10;
                roundedFirst = 1;
            }
            
            return roundedFirst * magnitude;
        }

        private void AddTableHeaderCell(TableLayoutPanel table, string text, int row, int column)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ThemeColors.Primary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };
            table.Controls.Add(label, column, row);
        }

        private void AddTableDataCell(TableLayoutPanel table, string text, int row, int column)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextPrimary,
                BackColor = ThemeColors.Surface,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };
            table.Controls.Add(label, column, row);
        }
    }
}

