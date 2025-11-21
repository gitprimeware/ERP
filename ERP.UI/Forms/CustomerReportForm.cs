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
    public partial class CustomerReportForm : UserControl
    {
        private Panel _mainPanel;
        private FlowLayoutPanel _companiesPanel;
        private OrderRepository _orderRepository;
        private CompanyRepository _companyRepository;
        private int _currentYear;
        private ComboBox _cmbYear;

        public CustomerReportForm()
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
                Text = "📊 Cari Raporu",
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

            // Firmalar paneli (scroll edilebilir)
            _companiesPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(yearPanel);
            _mainPanel.Controls.Add(_companiesPanel);

            // Panel resize event
            _mainPanel.Resize += (s, e) =>
            {
                if (_companiesPanel != null)
                {
                    _companiesPanel.Width = _mainPanel.Width - 60;
                    _companiesPanel.Height = _mainPanel.Height - 180;
                }
            };

            this.Controls.Add(_mainPanel);
        }

        private void LoadReportData()
        {
            _companiesPanel.Controls.Clear();

            // Tüm firmaları al
            var allCompanies = _companyRepository.GetAll();
            
            // Tüm siparişleri al
            var allOrders = _orderRepository.GetAll();
            
            // Yıla göre filtrele
            var yearOrders = allOrders.Where(o => o.OrderDate.Year == _currentYear).ToList();

            // Her firma için rapor oluştur
            foreach (var company in allCompanies)
            {
                var companyOrders = yearOrders.Where(o => o.CompanyId == company.Id).ToList();
                var companyPanel = CreateCompanyReportPanel(company, companyOrders);
                _companiesPanel.Controls.Add(companyPanel);
            }
        }

        private Panel CreateCompanyReportPanel(Company company, List<Order> companyOrders)
        {
            var panel = new Panel
            {
                Width = _companiesPanel.Width - 40,
                Height = 480, // Yükseklik azaltıldı
                BackColor = ThemeColors.Surface,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(15) // Padding azaltıldı
            };

            UIHelper.ApplyCardStyle(panel, 8);

            // Firma başlığı
            var companyTitle = new Label
            {
                Text = $"🏢 {company.Name}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(15, 15)
            };

            // Rapor tablosu
            var reportTable = new TableLayoutPanel
            {
                Location = new Point(15, 50),
                Width = panel.Width - 30,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 14, // 12 ay + başlık + toplam
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Ay sütunu küçültüldü
            reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Bekleyen
            reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Sevk Edilmiş
            reportTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); // Toplam

            // Satır yükseklikleri - daha kompakt
            reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Başlık küçültüldü
            for (int i = 0; i < 12; i++)
            {
                reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // Aylar küçültüldü
            }
            reportTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); // Toplam satırı

            // Başlık satırı
            AddHeaderCellToTable(reportTable, "Ay", 0, 0, ThemeColors.Primary, Color.White);
            AddHeaderCellToTable(reportTable, "Bekleyen", 0, 1, ThemeColors.Warning, Color.White);
            AddHeaderCellToTable(reportTable, "Sevk Edilmiş", 0, 2, ThemeColors.Success, Color.White);
            AddHeaderCellToTable(reportTable, "Toplam", 0, 3, ThemeColors.Info, Color.White);

            decimal totalNotShipped = 0;
            decimal totalShipped = 0;
            decimal grandTotal = 0;

            // 12 ay için verileri hesapla
            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = companyOrders.Where(o => o.OrderDate.Month == month).ToList();

                // Bekleyen (Status != "Sevk Edildi")
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
                AddDataCellToTable(reportTable, monthName, month, 0, ThemeColors.TextPrimary);

                // Bekleyen
                AddDataCellToTable(reportTable, notShippedUSD.ToString("N2"), month, 1, ThemeColors.TextPrimary);

                // Sevk edilmiş
                AddDataCellToTable(reportTable, shippedUSD.ToString("N2"), month, 2, ThemeColors.TextPrimary);

                // Toplam
                AddDataCellToTable(reportTable, monthTotal.ToString("N2"), month, 3, ThemeColors.TextPrimary);
            }

            // Toplam satırı - daha belirgin
            AddHeaderCellToTable(reportTable, "TOPLAM", 13, 0, Color.FromArgb(60, 60, 80), Color.White);
            AddDataCellToTable(reportTable, totalNotShipped.ToString("N2"), 13, 1, Color.FromArgb(60, 60, 80));
            AddDataCellToTable(reportTable, totalShipped.ToString("N2"), 13, 2, Color.FromArgb(60, 60, 80));
            AddDataCellToTable(reportTable, grandTotal.ToString("N2"), 13, 3, Color.FromArgb(60, 60, 80));

            panel.Controls.Add(companyTitle);
            panel.Controls.Add(reportTable);

            // Panel resize event
            panel.Resize += (s, e) =>
            {
                if (reportTable != null)
                {
                    reportTable.Width = panel.Width - 40;
                }
            };

            return panel;
        }

        private void AddHeaderCellToTable(TableLayoutPanel table, string text, int row, int column, Color backColor, Color foreColor)
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

            table.Controls.Add(label, column, row);
        }

        private void AddDataCellToTable(TableLayoutPanel table, string text, int row, int column, Color foreColor)
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

            table.Controls.Add(label, column, row);
        }
    }
}

