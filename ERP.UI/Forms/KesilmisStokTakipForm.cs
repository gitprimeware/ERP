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

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 80),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 130,
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
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 130;
            };

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
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                var cuttings = _cuttingRepository.GetAll()
                    .Where(c => c.PlakaAdedi > 0 && c.IsActive)
                    .OrderByDescending(c => c.CuttingDate)
                    .ToList();

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

                    stockData.Add(new StockRowData
                    {
                        SiparisNo = siparisNo,
                        KesimTarihi = cutting.CuttingDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        Hatve = cutting.Hatve.ToString("F2", CultureInfo.InvariantCulture),
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

