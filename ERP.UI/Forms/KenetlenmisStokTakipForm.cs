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
        private OrderRepository _orderRepository;

        public KenetlenmisStokTakipForm()
        {
            _clampingRepository = new ClampingRepository();
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
                Text = "Kenetlenmiş Stok Takip",
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
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false
            };

            _mainPanel.Resize += (s, e) =>
            {
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 130;
            };

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
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();

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

        private void LoadData()
        {
            try
            {
                var clampings = _clampingRepository.GetAll();
                var orders = _orderRepository.GetAll();

                var data = clampings.Select(c =>
                {
                    var order = orders.FirstOrDefault(o => o.Id == c.OrderId);
                    return new
                    {
                        Date = c.ClampingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        OrderNo = order?.TrexOrderNo ?? "",
                        Hatve = c.Hatve.ToString("F2", CultureInfo.InvariantCulture),
                        Size = c.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = c.Length.ToString("F2", CultureInfo.InvariantCulture),
                        ClampCount = c.ClampCount.ToString(),
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
    }
}

