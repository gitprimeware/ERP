using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;
using ERP.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ERP.UI.Forms
{
    public partial class PreslenmisStokTakipForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private PressingRepository _pressingRepository;
        private ClampingRepository _clampingRepository;
        private SerialNoRepository _serialNoRepository;
        private EmployeeRepository _employeeRepository;
        private MachineRepository _machineRepository;

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
            _clampingRepository = new ClampingRepository();
            _serialNoRepository = new SerialNoRepository();
            _employeeRepository = new EmployeeRepository();
            _machineRepository = new MachineRepository();
            InitializeCustomComponents();
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

            // Başlık ve buton paneli
            var headerPanel = new Panel
            {
                Location = new Point(20, 20),
                Width = _mainPanel.Width - 40,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = "Preslenmiş Stok Takip",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            // Excel'e aktar butonu
            //var btnExportExcel = ButtonFactory.CreateActionButton("📊 Excel'e Aktar", ThemeColors.Success, Color.White, 140, 35);
            var btnExportExcel = new Button
            {
                Text = "📊 Excel'e Aktar",
                Location = new Point(headerPanel.Width - 140, 5),
                Width = 140,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnExportExcel.Location = new Point(headerPanel.Width - 10, 5); // Adjust position as needed
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.Click += BtnExportExcel_Click;

            // Yeni Kenetleme Ekle butonu
            var btnKenetlemeEkle = new Button
            {
                Text = "➕ Yeni Kenetleme Ekle",
                Location = new Point(headerPanel.Width - 350, 5),
                Width = 180,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnKenetlemeEkle.FlatAppearance.BorderSize = 0;
            btnKenetlemeEkle.Click += BtnKenetlemeEkle_Click;



            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(btnExportExcel);
            headerPanel.Controls.Add(btnKenetlemeEkle);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(20, 80),
                Width = _mainPanel.Width - 40,
                Height = _mainPanel.Height - 140,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            _mainPanel.Resize += (s, e) =>
            {
                _dataGridView.Width = _mainPanel.Width - 40;
                _dataGridView.Height = _mainPanel.Height - 140;
                headerPanel.Width = _mainPanel.Width - 40;
                btnExportExcel.Location = new Point(headerPanel.Width - 140, 5); // Adjust position as needed
                btnKenetlemeEkle.Location = new Point(headerPanel.Width - 330, 5);
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
                DataPropertyName = "PlakaAdedi",
                HeaderText = "Plaka Adedi",
                Name = "PlakaAdedi",
                Width = 150
            });

            _mainPanel.Controls.Add(headerPanel);
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

                // Eğer hiç press verisi yoksa, boş liste göster
                if (pressings == null || pressings.Count == 0)
                {
                    // Boş durum için tüm modelleri 0 ile göster
                    foreach (var modelCode in _modelCodes)
                    {
                        stockData.Add(new StockRowData
                        {
                            ModelOlcu = modelCode,
                            PlakaAdedi = "0"
                        });
                    }
                }
                else
                {
                    // Kenetleme işlemlerinden kullanılan plaka adedini hesapla
                    var allClampings = _clampingRepository.GetAll();
                    var usedPlateCountByPressingId = allClampings
                        .Where(c => c.PressingId.HasValue)
                        .GroupBy(c => c.PressingId.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(c => c.UsedPlateCount));

                    foreach (var modelCode in _modelCodes)
                    {
                        // Model koduna göre pres işlemlerini filtrele
                        var modelPressings = GetPressingsForModel(pressings, modelCode);
                        
                        // Plaka adedi toplamını hesapla (kenetleme işlemlerinden kullanılanları çıkar)
                        int totalPlakaAdedi = modelPressings.Sum(p =>
                        {
                            int usedPlateCount = 0;
                            if (usedPlateCountByPressingId.ContainsKey(p.Id))
                            {
                                usedPlateCount = usedPlateCountByPressingId[p.Id];
                            }
                            return p.PressCount - usedPlateCount;
                        });

                        stockData.Add(new StockRowData
                        {
                            ModelOlcu = modelCode,
                            PlakaAdedi = totalPlakaAdedi > 0 ? totalPlakaAdedi.ToString() : "0"
                        });
                    }
                }

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
            // H: 3.10, 3.25 | D: 4.3, 4.5 | M: 6.3, 6.4, 6.5 | L: 8.65, 8.7, 9.0
            decimal hatve = pressing.Hatve;
            decimal size = pressing.Size;

            string modelLetter = "";
            const decimal tolerance = 0.1m;
            if (Math.Abs(hatve - 3.25m) < tolerance || Math.Abs(hatve - 3.10m) < tolerance) modelLetter = "H";
            else if (Math.Abs(hatve - 4.5m) < tolerance || Math.Abs(hatve - 4.3m) < tolerance) modelLetter = "D";
            else if (Math.Abs(hatve - 6.5m) < tolerance || Math.Abs(hatve - 6.3m) < tolerance || Math.Abs(hatve - 6.4m) < tolerance) modelLetter = "M";
            else if (Math.Abs(hatve - 9m) < tolerance || Math.Abs(hatve - 8.7m) < tolerance || Math.Abs(hatve - 8.65m) < tolerance) modelLetter = "L";

            // Size'a göre sayıyı belirle (örn: 20, 30, 40, 50, 60, 70, 80, 100)
            int sizeNumber = (int)Math.Round(size / 10) * 10;

            return $"{modelLetter}{sizeNumber}";
        }
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (_dataGridView.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Aktarılacak veri bulunamadı.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ExcelExportHelper.ExportToExcel(
                _dataGridView,
                defaultFileName: "PerslenmisStok",
                sheetName: "Perslenmiş Stok",
                skippedColumnNames: new[] { "Actions", "IsSelected" },
                title: "Perslenmiş Stok Takip");
        }
        private void BtnKenetlemeEkle_Click(object sender, EventArgs e)
        {
            try
            {
                // OrderId null olarak gönder - tüm preslenmiş stoktan seçim yapılabilir
                using (var dialog = new ClampingDialog(_serialNoRepository, _employeeRepository, _machineRepository, _pressingRepository, Guid.Empty))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Verileri yeniden yükle
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class StockRowData
        {
            public string ModelOlcu { get; set; }
            public string PlakaAdedi { get; set; }
        }
    }
}

