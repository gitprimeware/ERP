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
    public partial class PressingRequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private PressingRequestRepository _pressingRequestRepository;
        private PressingRepository _pressingRepository;
        private OrderRepository _orderRepository;

        public PressingRequestsForm()
        {
            _pressingRequestRepository = new PressingRequestRepository();
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
                Text = "📋 Pres Talepleri",
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
            // Id kolonu kaldırıldı (görünür değil, sadece veri erişimi için LoadData'da tutuluyor)
            
            AddPressingRequestColumn("Hatve", "Hatve", 80);
            AddPressingRequestColumn("Size", "Ölçü", 80);
            AddPressingRequestColumn("PlateThickness", "Plaka Kalınlığı", 120);
            AddPressingRequestColumn("SerialNumber", "Rulo Seri No", 120);
            AddPressingRequestColumn("PressNo", "Pres No", 100);
            AddPressingRequestColumn("Pressure", "Basınç", 100);
            AddPressingRequestColumn("RequestedPressCount", "İstenen Pres", 120);
            
            // Preslenmiş Adet - buton kolonu (eski: Kaç Tane Preslenmiş Oluştu)
            var colResultedPressCount = new DataGridViewButtonColumn
            {
                HeaderText = "Preslenmiş Adet",
                Name = "ResultedPressCount",
                Width = 150,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colResultedPressCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colResultedPressCount);
            
            // Hurda Adedi - buton kolonu
            var colWasteCount = new DataGridViewButtonColumn
            {
                HeaderText = "Hurda Adedi",
                Name = "WasteCount",
                Width = 120,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colWasteCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colWasteCount);
            
            AddPressingRequestColumn("Status", "Durum", 100);

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

        private void AddPressingRequestColumn(string dataPropertyName, string headerText, int width)
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
                var requests = _pressingRequestRepository.GetPendingRequests();
                var orders = _orderRepository.GetAll();
                
                var data = requests.Select(r =>
                {
                    return new
                    {
                        Id = r.Id,
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F1", CultureInfo.InvariantCulture),
                        PlateThickness = r.PlateThickness.ToString("F3", CultureInfo.InvariantCulture),
                        SerialNumber = r.SerialNo?.SerialNumber ?? "",
                        PressNo = r.PressNo ?? "",
                        Pressure = r.Pressure.ToString("F2", CultureInfo.InvariantCulture),
                        RequestedPressCount = r.RequestedPressCount.ToString(),
                        ResultedPressCount = r.ResultedPressCount.HasValue ? r.ResultedPressCount.Value.ToString() : "",
                        WasteCount = r.WasteCount.HasValue ? r.WasteCount.Value.ToString() : "",
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
                MessageBox.Show("Pres talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    
                    // ResultedPressCount buton kolonu için
                    if (columnName == "ResultedPressCount")
                    {
                        var resultedPressCountProperty = item.GetType().GetProperty("ResultedPressCount");
                        if (resultedPressCountProperty != null)
                        {
                            var resultedPressCountValue = resultedPressCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(resultedPressCountValue))
                            {
                                e.Value = $"Girildi ({resultedPressCountValue})";
                                e.FormattingApplied = true;
                            }
                            else
                            {
                                e.Value = "Gir";
                                e.FormattingApplied = true;
                            }
                        }
                    }
                    // WasteCount buton kolonu için
                    else if (columnName == "WasteCount")
                    {
                        var wasteCountProperty = item.GetType().GetProperty("WasteCount");
                        if (wasteCountProperty != null)
                        {
                            var wasteCountValue = wasteCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(wasteCountValue) && wasteCountValue != "0")
                            {
                                e.Value = $"Girildi ({wasteCountValue})";
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
            // Sadece "Kaç Tane Preslendiği" buton kolonuna tıklandığında dialog aç
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName != "ResultedPressCount" && columnName != "WasteCount")
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

                var request = _pressingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Dialog aç
                if (columnName == "ResultedPressCount")
                {
                    int? resultedPressCount = ShowResultedPressCountDialog(request);
                    if (resultedPressCount.HasValue)
                    {
                        request.ResultedPressCount = resultedPressCount.Value;
                        // ActualPressCount = ResultedPressCount + WasteCount (otomatik hesapla)
                        request.ActualPressCount = resultedPressCount.Value + (request.WasteCount ?? 0);
                        request.Status = "Presde";
                        _pressingRequestRepository.Update(request);
                        LoadData(); // Verileri yeniden yükle
                    }
                }
                else if (columnName == "WasteCount")
                {
                    int? wasteCount = ShowWasteCountDialog(request);
                    if (wasteCount.HasValue)
                    {
                        request.WasteCount = wasteCount.Value;
                        // ActualPressCount = ResultedPressCount + WasteCount (otomatik hesapla)
                        request.ActualPressCount = (request.ResultedPressCount ?? 0) + wasteCount.Value;
                        _pressingRequestRepository.Update(request);
                        LoadData(); // Verileri yeniden yükle
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pres adedi girilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowActualPressCountDialog(PressingRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Preslendiği",
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
                    Text = $"İstenen Pres: {request.RequestedPressCount} adet\n\nKaç tane preslendi?",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Preslenen Adet:",
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
                    Value = request.ActualPressCount ?? request.RequestedPressCount,
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

        private int? ShowResultedPressCountDialog(PressingRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Preslenmiş Adet",
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
                    Text = $"İstenen Pres: {request.RequestedPressCount} adet\n\nPreslenmiş adet girin:",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Preslenmiş Adet:",
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
                    Value = request.ResultedPressCount ?? 0,
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

        private int? ShowWasteCountDialog(PressingRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Hurda Adedi",
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
                    Text = "Hurda adedi girin:",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 40,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Hurda Adedi:",
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
                    Value = request.WasteCount ?? 0,
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
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}

