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
    public partial class Clamping2RequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private Clamping2RequestRepository _clamping2RequestRepository;
        private ClampingRepository _clampingRepository;
        private OrderRepository _orderRepository;

        public Clamping2RequestsForm()
        {
            _clamping2RequestRepository = new Clamping2RequestRepository();
            _clampingRepository = new ClampingRepository();
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
                Text = "📋 Kenetleme 2 Talepleri",
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
            
            AddClamping2RequestColumn("Hatve", "Hatve", 80);
            AddClamping2RequestColumn("PlateThickness", "Lamel Kalınlığı", 120);
            AddClamping2RequestColumn("ResultedSize", "Sonuç Ölçü", 100);
            AddClamping2RequestColumn("ResultedLength", "Sonuç Uzunluk", 120);
            AddClamping2RequestColumn("ClampingsList", "Kullanılacak Ürünler", 250);
            AddClamping2RequestColumn("RequestedCount", "İstenen Adet", 120);
            
            // Kaç Tane Kullanıldı - buton kolonu
            var colActualCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kaç Tane Kullanıldı",
                Name = "ActualCount",
                Width = 180,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colActualCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colActualCount);
            
            // Kaç Tane Oluştu - buton kolonu
            var colResultedCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kaç Tane Oluştu",
                Name = "ResultedCount",
                Width = 150,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colResultedCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colResultedCount);
            
            AddClamping2RequestColumn("Status", "Durum", 100);

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

        private void AddClamping2RequestColumn(string dataPropertyName, string headerText, int width)
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
                var requests = _clamping2RequestRepository.GetPendingRequests();
                
                var data = requests.Select(r =>
                {
                    string clampingsList = "";
                    
                    // Items listesi varsa onu kullan, yoksa FirstClampingId/SecondClampingId kullan (geriye dönük uyumluluk)
                    if (r.Items != null && r.Items.Count > 0)
                    {
                        var clampingInfos = r.Items
                            .OrderBy(item => item.Sequence)
                            .Select(item =>
                            {
                                var clamping = _clampingRepository.GetById(item.ClampingId);
                                return clamping != null ? $"{clamping.Size:F2} x {clamping.Length:F2}" : "";
                            })
                            .Where(info => !string.IsNullOrEmpty(info))
                            .ToList();
                        
                        clampingsList = string.Join(" + ", clampingInfos);
                    }
                    else
                    {
                        // Geriye dönük uyumluluk için FirstClampingId/SecondClampingId kullan
                        var firstClamping = r.FirstClampingId.HasValue ? _clampingRepository.GetById(r.FirstClampingId.Value) : null;
                        var secondClamping = r.SecondClampingId.HasValue ? _clampingRepository.GetById(r.SecondClampingId.Value) : null;
                        
                        var clampingInfos = new List<string>();
                        if (firstClamping != null)
                            clampingInfos.Add($"{firstClamping.Size:F2} x {firstClamping.Length:F2}");
                        if (secondClamping != null)
                            clampingInfos.Add($"{secondClamping.Size:F2} x {secondClamping.Length:F2}");
                        
                        clampingsList = string.Join(" + ", clampingInfos);
                    }
                    
                    return new
                    {
                        Id = r.Id,
                        Hatve = GetHatveLetter(r.Hatve),
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        ResultedSize = r.ResultedSize.ToString("F2", CultureInfo.InvariantCulture),
                        ResultedLength = r.ResultedLength.ToString("F2", CultureInfo.InvariantCulture),
                        ClampingsList = clampingsList,
                        RequestedCount = r.RequestedCount.ToString(),
                        ActualCount = r.ActualCount.HasValue ? r.ActualCount.Value.ToString() : "",
                        ResultedCount = r.ResultedCount.HasValue ? r.ResultedCount.Value.ToString() : "",
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
                MessageBox.Show("Kenetleme 2 talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    
                    // ActualCount buton kolonu için
                    if (columnName == "ActualCount")
                    {
                        var actualCountProperty = item.GetType().GetProperty("ActualCount");
                        if (actualCountProperty != null)
                        {
                            var actualCountValue = actualCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(actualCountValue))
                            {
                                e.Value = $"Girildi ({actualCountValue})";
                                e.FormattingApplied = true;
                            }
                            else
                            {
                                e.Value = "Gir";
                                e.FormattingApplied = true;
                            }
                        }
                    }
                    // ResultedCount buton kolonu için
                    else if (columnName == "ResultedCount")
                    {
                        var resultedCountProperty = item.GetType().GetProperty("ResultedCount");
                        if (resultedCountProperty != null)
                        {
                            var resultedCountValue = resultedCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(resultedCountValue))
                            {
                                e.Value = $"Girildi ({resultedCountValue})";
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
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ActualCount" && columnName != "ResultedCount")
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

                var request = _clamping2RequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Dialog aç
                if (columnName == "ActualCount")
                {
                    int? actualCount = ShowActualCountDialog(request);
                    if (actualCount.HasValue)
                    {
                        request.ActualCount = actualCount.Value;
                        request.Status = "Kenetmede";
                        _clamping2RequestRepository.Update(request);
                        LoadData();
                    }
                }
                else if (columnName == "ResultedCount")
                {
                    int? resultedCount = ShowResultedCountDialog(request);
                    if (resultedCount.HasValue)
                    {
                        request.ResultedCount = resultedCount.Value;
                        request.Status = "Kenetmede";
                        _clamping2RequestRepository.Update(request);
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Değer girilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowActualCountDialog(Clamping2Request request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Kullanıldı",
                Width = 350,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblInfo = new Label
                {
                    Text = $"İstenen Adet: {request.RequestedCount}",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblCount = new Label
                {
                    Text = "Kullanılan Adet:",
                    Location = new Point(20, 60),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var txtCount = new NumericUpDown
                {
                    Location = new Point(150, 57),
                    Width = 150,
                    Minimum = 0,
                    Maximum = 999999,
                    Value = request.ActualCount ?? request.RequestedCount
                };

                var btnOk = new Button
                {
                    Text = "Kaydet",
                    DialogResult = DialogResult.OK,
                    Location = new Point(150, 110),
                    Width = 80
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(240, 110),
                    Width = 80
                };

                dialog.Controls.AddRange(new Control[] { lblInfo, lblCount, txtCount, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return (int)txtCount.Value;
                }
            }
            return null;
        }

        private int? ShowResultedCountDialog(Clamping2Request request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Oluştu",
                Width = 350,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblInfo = new Label
                {
                    Text = $"Kullanılan Adet: {request.ActualCount ?? request.RequestedCount}",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblCount = new Label
                {
                    Text = "Oluşan Adet:",
                    Location = new Point(20, 60),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var txtCount = new NumericUpDown
                {
                    Location = new Point(150, 57),
                    Width = 150,
                    Minimum = 0,
                    Maximum = 999999,
                    Value = request.ResultedCount ?? 0
                };

                var btnOk = new Button
                {
                    Text = "Kaydet",
                    DialogResult = DialogResult.OK,
                    Location = new Point(150, 110),
                    Width = 80
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(240, 110),
                    Width = 80
                };

                dialog.Controls.AddRange(new Control[] { lblInfo, lblCount, txtCount, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return (int)txtCount.Value;
                }
            }
            return null;
        }

        private string GetHatveLetter(decimal hatve)
        {
            // Hatve değerine göre harf döndür
            if (hatve == 2.5m) return "A";
            if (hatve == 3.0m) return "B";
            if (hatve == 3.5m) return "C";
            if (hatve == 4.0m) return "D";
            if (hatve == 4.5m) return "E";
            if (hatve == 5.0m) return "F";
            return hatve.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}

