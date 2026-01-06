using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ClampingRequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private ClampingRequestRepository _clampingRequestRepository;
        private PressingRepository _pressingRepository;
        private OrderRepository _orderRepository;

        public ClampingRequestsForm()
        {
            _clampingRequestRepository = new ClampingRequestRepository();
            _pressingRepository = new PressingRepository();
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
                Text = "📋 Kenetleme Talepleri",
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
                ReadOnly = false,
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
            // Id kolonu kaldırıldı (görünür değil, sadece veri erişimi için LoadData'da anonymous object'te tutuluyor)
            
            AddClampingRequestColumn("Hatve", "Hatve", 80);
            AddClampingRequestColumn("Size", "Ölçü", 80);
            AddClampingRequestColumn("PlateThickness", "Plaka Kalınlığı", 120);
            AddClampingRequestColumn("Length", "Uzunluk", 100);
            AddClampingRequestColumn("SerialNumber", "Rulo Seri No", 120);
            AddClampingRequestColumn("RequestedClampCount", "Kullanılacak Pres Sayısı", 150);
            
            // Kullanılan Pres Adedi - buton kolonu
            var colActualClampCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kullanılan Pres Adedi",
                Name = "ActualClampCount",
                Width = 180,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colActualClampCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colActualClampCount);
            
            // Adet - readonly TextBox kolonu (ClampingDialog'dan RequestedClampCount gelir)
            var colAdet = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Adet",
                HeaderText = "Adet",
                Name = "Adet",
                Width = 150,
                ReadOnly = true // Sabit - ClampingDialog'dan gelir
            };
            colAdet.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colAdet);
            
            AddClampingRequestColumn("Status", "Durum", 100);

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

            // Event handler
            btnYenile.Click += (s, e) => LoadData();

            _mainPanel.Resize += (s, e) =>
            {
                buttonPanel.Width = _mainPanel.Width - 60;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 180;
                btnYenile.Location = new Point(buttonPanel.Width - 120, 5);
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(buttonPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void AddClampingRequestColumn(string dataPropertyName, string headerText, int width)
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
                var requests = _clampingRequestRepository.GetPendingRequests();
                
                var data = requests.Select(r =>
                {
                    return new
                    {
                        Id = r.Id,
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        Length = r.Length.ToString("F2", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "",
                        RequestedClampCount = r.RequestedClampCount.ToString(),
                        ActualClampCount = r.ActualClampCount.HasValue ? r.ActualClampCount.Value.ToString() : "",
                        Adet = r.ResultedClampCount?.ToString() ?? "", // ClampingDialog'dan gelen Adet (_txtResultedCount)
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
                MessageBox.Show("Kenetleme talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
                var row = _dataGridView.Rows[e.RowIndex];
                
                if (row.DataBoundItem != null)
                {
                    var item = row.DataBoundItem;
                    
                    // ActualClampCount buton kolonu için
                    if (columnName == "ActualClampCount")
                    {
                        var actualClampCountProperty = item.GetType().GetProperty("ActualClampCount");
                        if (actualClampCountProperty != null)
                        {
                            var actualClampCountValue = actualClampCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(actualClampCountValue))
                            {
                                e.Value = $"Girildi ({actualClampCountValue})";
                                e.FormattingApplied = true;
                            }
                            else
                            {
                                e.Value = "Gir";
                                e.FormattingApplied = true;
                            }
                        }
                    }
                    // ResultedClampCount artık buton değil, TextBox - formatting'e gerek yok
                }
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ActualClampCount")
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

                var request = _clampingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Dialog aç
                if (columnName == "ActualClampCount")
                {
                    int? actualClampCount = ShowActualClampCountDialog(request);
                    if (actualClampCount.HasValue)
                    {
                        request.ActualClampCount = actualClampCount.Value;
                        request.Status = "Kenetmede";
                        _clampingRequestRepository.Update(request);
                        
                        // Event feed kaydı ekle - Kenetleme tamamlandı, onay bekliyor
                        if (request.OrderId != Guid.Empty)
                        {
                            var orderRepository = new OrderRepository();
                            var order = orderRepository.GetById(request.OrderId);
                            if (order != null)
                            {
                                EventFeedService.ClampingCompleted(request.Id, request.OrderId, order.TrexOrderNo, actualClampCount.Value);
                            }
                        }
                        
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kenetleme adedi girilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowActualClampCountDialog(ClampingRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Preslenmiş Kenetleneceği",
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
                    Text = $"İstenen Kenetleme: {request.RequestedClampCount} adet\n\nKaç tane preslenmiş kenetlenecek?",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Kenetlenecek Adet:",
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
                    Value = request.ActualClampCount ?? request.RequestedClampCount,
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

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerlerini harfe çevir (yeni format): 3.10, 3.25=H | 4.3, 4.5=D | 6.3, 6.4, 6.5=M | 9.0=L
            // Tolerance'ı biraz artırdık (0.1'den 0.15'e) - 6.4 ve benzeri değerleri daha iyi yakalamak için
            const decimal tolerance = 0.15m;
            
            // H: 3.10, 3.25 (±0.15 = 2.95-3.40 arası)
            if (hatveValue >= 2.95m && hatveValue <= 3.40m)
                return "H";
            // D: 4.3, 4.5 (±0.15 = 4.15-4.65 arası)
            else if (hatveValue >= 4.15m && hatveValue <= 4.65m)
                return "D";
            // M: 6.3, 6.4, 6.5 (±0.15 = 6.15-6.65 arası)
            else if (hatveValue >= 6.15m && hatveValue <= 6.65m)
                return "M";
            // L: 8.65, 8.7, 9.0 (±0.15 = 8.50-9.15 arası)
            else if (hatveValue >= 8.50m && hatveValue <= 9.15m)
                return "L";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture); // Eğer tanınmazsa sayısal göster
        }
    }
}

