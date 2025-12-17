using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class CuttingRequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private CuttingRequestRepository _cuttingRequestRepository;
        private CuttingRepository _cuttingRepository;
        private OrderRepository _orderRepository;

        public CuttingRequestsForm()
        {
            _cuttingRequestRepository = new CuttingRequestRepository();
            _cuttingRepository = new CuttingRepository();
            _orderRepository = new OrderRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
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
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 Kesim Talepleri",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Buton paneli
            var buttonPanel = new Panel
            {
                Location = new Point(30, 80),
                Width = _mainPanel.Width - 60,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            // Yenile butonu
            var btnYenile = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Secondary, Color.White, 120, 35);
            btnYenile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnYenile.Location = new Point(buttonPanel.Width - 120, 5);
            buttonPanel.Controls.Add(btnYenile);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 140),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false, // İşçi kesim adedini girebilmeli
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                GridColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };

            // Kolonları ekle
            // Id kolonu kaldırıldı (görünür değil, sadece veri erişimi için LoadData'da tutuluyor)
            
            AddCuttingRequestColumn("Hatve", "Hatve", 80);
            AddCuttingRequestColumn("Size", "Ölçü", 80);
            AddCuttingRequestColumn("Length", "Uzunluk", 80);
            AddCuttingRequestColumn("KapakTipi", "Kapak Tipi", 100);
            AddCuttingRequestColumn("ProfilTipi", "Profil Tipi", 100);
            AddCuttingRequestColumn("PlateThickness", "Plaka Kalınlığı", 120);
            AddCuttingRequestColumn("SerialNumber", "Rulo Seri No", 120);
            AddCuttingRequestColumn("RequestedPlateCount", "İstenen Kesim", 120);
            
            // Kaç Tane Kesildiği - buton kolonu (dialog açmak için)
            var colActualCutCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kaç Tane Kesildiği",
                Name = "ActualCutCount",
                Width = 150,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colActualCutCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colActualCutCount);
            
            // Rulo Bitti - checkbox
            var colIsRollFinished = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsRollFinished",
                HeaderText = "Rulo Bitti",
                Name = "IsRollFinished",
                Width = 100,
                ReadOnly = false
            };
            colIsRollFinished.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colIsRollFinished);
            
            AddCuttingRequestColumn("Status", "Durum", 100);

            // Stil ayarları
            _dataGridView.ColumnHeadersVisible = true;
            _dataGridView.RowHeadersVisible = false;
            _dataGridView.EnableHeadersVisualStyles = false;
            _dataGridView.ColumnHeadersHeight = 40;
            _dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            _dataGridView.DefaultCellStyle.BackColor = Color.White;
            _dataGridView.BackgroundColor = Color.White;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            _dataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // CellClick event'i - buton kolonuna tıklandığında dialog aç
            _dataGridView.CellClick += DataGridView_CellClick;
            
            // CellFormatting event'i - buton metnini dinamik olarak ayarla
            _dataGridView.CellFormatting += DataGridView_CellFormatting;
            
            // CellValueChanged event'i - rulo bitti durumunu değiştirdiğinde kaydet
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;

            // Event handler
            btnYenile.Click += (s, e) => LoadData();

            _mainPanel.Resize += (s, e) =>
            {
                buttonPanel.Width = _mainPanel.Width - 60;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 180;
                btnYenile.Location = new Point(buttonPanel.Width - 120 - 130, 5);
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(buttonPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void AddCuttingRequestColumn(string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Name = dataPropertyName,
                Width = width,
                Visible = true,
                ReadOnly = true
            };
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(column);
        }

        private void LoadData()
        {
            try
            {
                var requests = _cuttingRequestRepository.GetPendingRequests(); // Sadece beklemede ve kesimde olanlar
                var orders = _orderRepository.GetAll();
                
                var data = requests.Select(r =>
                {
                    var order = orders.FirstOrDefault(o => o.Id == r.OrderId);
                    
                    // Ürün kodundan bilgileri parse et
                    string kapakTipi = "";
                    string profilTipi = "";
                    decimal uzunluk = 0;
                    
                    if (order != null && !string.IsNullOrEmpty(order.ProductCode))
                    {
                        var parts = order.ProductCode.Split('-');
                        if (parts.Length >= 3)
                        {
                            string modelProfile = parts[2];
                            if (modelProfile.Length >= 2)
                            {
                                profilTipi = modelProfile[1].ToString().ToUpper();
                            }
                        }
                        
                        // Kapak tipi: 5. parça (030 -> 30)
                        if (parts.Length >= 6 && int.TryParse(parts[5], out int kapakBoyuMM))
                        {
                            kapakTipi = kapakBoyuMM.ToString();
                        }
                        
                        // Uzunluk: Yükseklik (4. parça)
                        if (parts.Length >= 5 && int.TryParse(parts[4], out int yukseklikMM))
                        {
                            // Yükseklik <= 1800 ise aynı, > 1800 ise /2
                            int yukseklikCom = yukseklikMM <= 1800 ? yukseklikMM : yukseklikMM / 2;
                            // Kapak değeri çıkar
                            if (parts.Length >= 6 && int.TryParse(parts[5], out int kapakMM))
                            {
                                uzunluk = (yukseklikCom - kapakMM) / 10.0m;
                            }
                            else
                            {
                                uzunluk = yukseklikCom / 10.0m;
                            }
                        }
                    }
                    
                    return new
                    {
                        Id = r.Id,
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                        Length = uzunluk.ToString("F1", CultureInfo.InvariantCulture),
                        KapakTipi = kapakTipi,
                        ProfilTipi = profilTipi,
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "",
                        RequestedPlateCount = r.RequestedPlateCount.ToString(),
                        ActualCutCount = r.ActualCutCount.HasValue ? r.ActualCutCount.Value.ToString() : "",
                        IsRollFinished = r.IsRollFinished,
                        Status = r.Status
                    };
                }).ToList();

                _dataGridView.DataSource = data;
                
                // DataSource ayarlandıktan SONRA HeaderText'leri tekrar ayarla
                foreach (DataGridViewColumn column in _dataGridView.Columns)
                {
                    column.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Sadece ActualCutCount buton kolonu için
            if (_dataGridView.Columns[e.ColumnIndex].Name == "ActualCutCount" && e.RowIndex >= 0)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                if (row.DataBoundItem != null)
                {
                    var item = row.DataBoundItem;
                    var actualCutCountProperty = item.GetType().GetProperty("ActualCutCount");
                    if (actualCutCountProperty != null)
                    {
                        var actualCutCountValue = actualCutCountProperty.GetValue(item)?.ToString();
                        
                        // Eğer değer varsa "Girildi (X)" göster, yoksa "Gir" göster
                        if (!string.IsNullOrWhiteSpace(actualCutCountValue))
                        {
                            e.Value = $"Girildi ({actualCutCountValue})";
                            e.FormattingApplied = true;
                        }
                        else
                        {
                            e.Value = "Gir";
                            e.FormattingApplied = true;
                        }
                    }
                }
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Sadece "Kaç Tane Kesildiği" buton kolonuna tıklandığında dialog aç
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ActualCutCount")
                return;

            try
            {
                var row = _dataGridView.Rows[e.RowIndex];
                if (row.DataBoundItem == null)
                    return;

                // Id'yi al
                Guid requestId = Guid.Empty;
                var item = row.DataBoundItem;
                var idProperty = item.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    requestId = (Guid)idProperty.GetValue(item);
                }

                if (requestId == Guid.Empty)
                    return;

                var request = _cuttingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Dialog aç
                int? actualCutCount = ShowActualCutCountDialog(request);
                if (actualCutCount.HasValue)
                {
                    request.ActualCutCount = actualCutCount.Value;
                    request.Status = "Kesimde"; // İşçi kesim adedini girdiğinde durum "Kesimde" olur
                    _cuttingRequestRepository.Update(request);
                    LoadData(); // Verileri yeniden yükle
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kesim adedi girilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowActualCutCountDialog(CuttingRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Kesildiği",
                Width = 400,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeColors.Background
            })
            {
                var lblInfo = new Label
                {
                    Text = $"İstenen Kesim: {request.RequestedPlateCount} adet\n\nKaç tane kesildi?",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Kesilen Adet:",
                    Location = new Point(20, 90),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var txtAdet = new NumericUpDown
                {
                    Location = new Point(150, 87),
                    Width = 200,
                    Minimum = 0,
                    Maximum = 999999,
                    Value = request.ActualCutCount ?? request.RequestedPlateCount,
                    DecimalPlaces = 0,
                    Font = new Font("Segoe UI", 10F)
                };

                var btnOk = new Button
                {
                    Text = "Tamam",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 130),
                    Width = 80,
                    BackColor = ThemeColors.Success,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 130),
                    Width = 80,
                    BackColor = ThemeColors.Secondary,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                dialog.Controls.AddRange(new Control[] { lblInfo, lblAdet, txtAdet, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return (int)txtAdet.Value;
                }
            }

            return null;
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Sadece IsRollFinished kolonu değiştiğinde kaydet
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "IsRollFinished")
                return;

            try
            {
                var row = _dataGridView.Rows[e.RowIndex];
                if (row.DataBoundItem == null)
                    return;

                // Id'yi al
                Guid requestId = Guid.Empty;
                var item = row.DataBoundItem;
                var idProperty = item.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    requestId = (Guid)idProperty.GetValue(item);
                }

                if (requestId == Guid.Empty)
                    return;

                var request = _cuttingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Rulo bitti durumunu kaydet
                if (row.Cells["IsRollFinished"] != null)
                {
                    request.IsRollFinished = (bool)(row.Cells["IsRollFinished"].Value ?? false);
                }

                _cuttingRequestRepository.Update(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Değişiklik kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerlerini harfe çevir: 3.25=H, 4.5=D, 6.5=M, 9=L
            const decimal tolerance = 0.1m;
            
            if (Math.Abs(hatveValue - 3.25m) < tolerance)
                return "H";
            else if (Math.Abs(hatveValue - 4.5m) < tolerance)
                return "D";
            else if (Math.Abs(hatveValue - 6.5m) < tolerance)
                return "M";
            else if (Math.Abs(hatveValue - 9m) < tolerance)
                return "L";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture); // Eğer tanınmazsa sayısal göster
        }
    }
}

