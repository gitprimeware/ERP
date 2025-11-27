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
    public partial class PreslenmisStokTakipForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private PressingRepository _pressingRepository;

        // Model kodları
        private readonly string[] _modelCodes = new string[]
        {
            "H20", "H30", "H40", "H50",
            "M30", "M40", "M50", "M60", "M70", "M80", "M100",
            "D40", "D50", "D60",
            "L50", "L60", "L70", "L80", "L100",
            "Diğer"
        };

        public PreslenmisStokTakipForm()
        {
            _pressingRepository = new PressingRepository();
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
                Text = "Preslenmiş Stok Takip",
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
                DataPropertyName = "ModelOlcu",
                HeaderText = "Model/Ölçü",
                Name = "ModelOlcu",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StokUrunler",
                HeaderText = "Stok Ürünler",
                Name = "StokUrunler",
                Width = 200
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PlakaAdedi",
                HeaderText = "Plaka Adedi",
                Name = "PlakaAdedi",
                Width = 150
            });

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                var pressings = _pressingRepository.GetAll();
                var stockData = new List<StockRowData>();

                foreach (var modelCode in _modelCodes)
                {
                    // Model koduna göre pres işlemlerini filtrele
                    var modelPressings = GetPressingsForModel(pressings, modelCode);
                    
                    // Plaka adedi toplamını hesapla
                    int totalPlakaAdedi = modelPressings.Sum(p => p.PressCount);

                    // Stok ürünler bilgisini oluştur (örnek: "H20 - 50cm x 0.165")
                    string stokUrunler = GetStokUrunlerInfo(modelPressings, modelCode);

                    stockData.Add(new StockRowData
                    {
                        ModelOlcu = modelCode,
                        StokUrunler = stokUrunler,
                        PlakaAdedi = totalPlakaAdedi > 0 ? totalPlakaAdedi.ToString() : "0"
                    });
                }

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
            catch (Exception ex)
            {
                MessageBox.Show("Preslenmiş stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Pressing> GetPressingsForModel(List<Pressing> pressings, string modelCode)
        {
            if (modelCode == "Diğer")
            {
                // Diğer modeller için, tanımlı modeller dışındakileri al
                var definedModels = _modelCodes.Where(m => m != "Diğer").ToList();
                return pressings.Where(p => 
                {
                    string pressingModel = GetModelFromPressing(p);
                    return !definedModels.Contains(pressingModel);
                }).ToList();
            }

            return pressings.Where(p => GetModelFromPressing(p) == modelCode).ToList();
        }

        private string GetModelFromPressing(Pressing pressing)
        {
            // Hatve ve Size'a göre model kodu belirle
            // H: 3.25, D: 4.5, M: 6.5, L: 9
            decimal hatve = pressing.Hatve;
            decimal size = pressing.Size;

            string modelLetter = "";
            if (Math.Abs(hatve - 3.25m) < 0.1m) modelLetter = "H";
            else if (Math.Abs(hatve - 4.5m) < 0.1m) modelLetter = "D";
            else if (Math.Abs(hatve - 6.5m) < 0.1m) modelLetter = "M";
            else if (Math.Abs(hatve - 9m) < 0.1m) modelLetter = "L";

            // Size'a göre sayıyı belirle (örn: 20, 30, 40, 50, 60, 70, 80, 100)
            int sizeNumber = (int)Math.Round(size / 10) * 10;

            return $"{modelLetter}{sizeNumber}";
        }

        private string GetStokUrunlerInfo(List<Pressing> pressings, string modelCode)
        {
            if (pressings.Count == 0)
                return "";

            // Pres işlemlerinden örnek bilgileri al
            var firstPressing = pressings.First();
            return $"{modelCode} - {firstPressing.Size}cm x {firstPressing.PlateThickness}";
        }

        private class StockRowData
        {
            public string ModelOlcu { get; set; }
            public string StokUrunler { get; set; }
            public string PlakaAdedi { get; set; }
        }
    }
}

