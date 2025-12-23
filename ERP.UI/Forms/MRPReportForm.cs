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
    public partial class MRPReportForm : UserControl
    {
        private Panel _mainPanel;
        private TableLayoutPanel _reportTable;
        private OrderRepository _orderRepository;
        private int _currentYear;

        public MRPReportForm()
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
                Text = "📊 Üretim Raporu",
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

            var cmbYear = new ComboBox
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
                cmbYear.Items.Add(year);
            }
            cmbYear.SelectedItem = _currentYear;
            cmbYear.SelectedIndexChanged += (s, e) =>
            {
                if (cmbYear.SelectedItem != null)
                {
                    _currentYear = (int)cmbYear.SelectedItem;
                    LoadReportData();
                }
            };

            yearPanel.Controls.Add(lblYear);
            yearPanel.Controls.Add(cmbYear);

            // Rapor tablosu
            _reportTable = new TableLayoutPanel
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 14, // 12 ay + başlık + toplam
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Ay sütunu küçültüldü
            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Sevk Edilmemiş
            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Sevk Edilmiş
            _reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Toplam

            // Satır yükseklikleri - daha kompakt
            _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Başlık küçültüldü
            for (int i = 0; i < 12; i++)
            {
                _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // Aylar küçültüldü
            }
            _reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); // Toplam satırı

            // Panel resize event
            _mainPanel.Resize += (s, e) =>
            {
                if (_reportTable != null)
                {
                    _reportTable.Width = _mainPanel.Width - 60;
                }
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(yearPanel);
            _mainPanel.Controls.Add(_reportTable);

            this.Controls.Add(_mainPanel);
        }

        private void LoadReportData()
        {
            _reportTable.Controls.Clear();

            // Başlık satırı
            AddHeaderCell("Ay", 0, 0, ThemeColors.Primary, Color.White);
            AddHeaderCell("Sevk Edilmemiş", 0, 1, ThemeColors.Warning, Color.White);
            AddHeaderCell("Sevk Edilmiş", 0, 2, ThemeColors.Success, Color.White);
            AddHeaderCell("Toplam", 0, 3, ThemeColors.Info, Color.White);

            // Tüm siparişleri al
            var allOrders = _orderRepository.GetAll();

            // Yıla göre filtrele
            var yearOrders = allOrders.Where(o => o.OrderDate.Year == _currentYear).ToList();

            decimal totalNotShipped = 0;
            decimal totalShipped = 0;
            decimal grandTotal = 0;

            // 12 ay için verileri hesapla
            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = yearOrders.Where(o => o.OrderDate.Month == month).ToList();

                // Sevk edilmemiş (Status != "Sevk Edildi")
                var notShippedOrders = monthOrders.Where(o => o.Status != "Sevk Edildi").ToList();
                decimal notShippedUSD = notShippedOrders.Sum(o => (o.SalesPrice ?? 0) * o.Quantity);

                // Sevk edilmiş (Status == "Sevk Edildi")
                var shippedOrders = monthOrders.Where(o => o.Status == "Sevk Edildi").ToList();
                decimal shippedUSD = shippedOrders.Sum(o => (o.SalesPrice ?? 0) * o.Quantity);

                // Toplam
                decimal monthTotal = notShippedUSD + shippedUSD;

                totalNotShipped += notShippedUSD;
                totalShipped += shippedUSD;
                grandTotal += monthTotal;

                // Ay adı
                var monthName = new DateTime(_currentYear, month, 1).ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
                AddDataCell(monthName, month, 0, ThemeColors.TextPrimary);

                // Sevk edilmemiş
                AddDataCell(notShippedUSD.ToString("N2"), month, 1, ThemeColors.TextPrimary);

                // Sevk edilmiş
                AddDataCell(shippedUSD.ToString("N2"), month, 2, ThemeColors.TextPrimary);

                // Toplam
                AddDataCell(monthTotal.ToString("N2"), month, 3, ThemeColors.TextPrimary);
            }

            // Toplam satırı - daha belirgin
            AddHeaderCell("TOPLAM", 13, 0, Color.FromArgb(60, 60, 80), Color.White);
            AddDataCell(totalNotShipped.ToString("N2"), 13, 1, Color.FromArgb(60, 60, 80));
            AddDataCell(totalShipped.ToString("N2"), 13, 2, Color.FromArgb(60, 60, 80));
            AddDataCell(grandTotal.ToString("N2"), 13, 3, Color.FromArgb(60, 60, 80));
        }

        private void AddHeaderCell(string text, int row, int column, Color backColor, Color foreColor)
        {
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

            _reportTable.Controls.Add(label, column, row);
        }

        private void AddDataCell(string text, int row, int column, Color foreColor)
        {
            bool isTotalRow = row == 13;
            var label = new Label
            {
                Text = text,
                Font = isTotalRow ? new Font("Segoe UI", 10F, FontStyle.Bold) : new Font("Segoe UI", 9F),
                ForeColor = isTotalRow ? Color.White : foreColor,
                BackColor = isTotalRow ? Color.FromArgb(60, 60, 80) : ThemeColors.Surface,
                Dock = DockStyle.Fill,
                TextAlign = column == 0 ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight,
                Padding = new Padding(column == 0 ? 10 : 3, 0, column == 0 ? 3 : 10, 0)
            };

            _reportTable.Controls.Add(label, column, row);
        }
    }
}

