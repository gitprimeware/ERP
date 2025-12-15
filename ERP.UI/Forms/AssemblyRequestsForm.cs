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
    public partial class AssemblyRequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private AssemblyRequestRepository _assemblyRequestRepository;
        private ClampingRepository _clampingRepository;
        private OrderRepository _orderRepository;

        public AssemblyRequestsForm()
        {
            _assemblyRequestRepository = new AssemblyRequestRepository();
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
                Text = "📋 Montaj Talepleri",
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
            // Id kolonu (gizli)
            var colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                Name = "Id",
                Width = 0,
                Visible = false
            };
            _dataGridView.Columns.Add(colId);
            
            AddAssemblyRequestColumn("Hatve", "Hatve", 80);
            AddAssemblyRequestColumn("Size", "Ölçü", 80);
            AddAssemblyRequestColumn("PlateThickness", "Plaka Kalınlığı", 120);
            AddAssemblyRequestColumn("Length", "Uzunluk", 100);
            AddAssemblyRequestColumn("SerialNumber", "Rulo Seri No", 120);
            AddAssemblyRequestColumn("RequestedAssemblyCount", "İstenen Montaj", 150);
            
            // Kaç Tane Kenet Kullanıldı - buton kolonu
            var colActualClampCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kaç Tane Kenet Kullanıldı",
                Name = "ActualClampCount",
                Width = 180,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colActualClampCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colActualClampCount);
            
            // Kaç Tane Montaj Oluştu - buton kolonu
            var colResultedAssemblyCount = new DataGridViewButtonColumn
            {
                HeaderText = "Kaç Tane Montaj Oluştu",
                Name = "ResultedAssemblyCount",
                Width = 180,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
            };
            colResultedAssemblyCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colResultedAssemblyCount);
            
            AddAssemblyRequestColumn("Status", "Durum", 100);

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

        private void AddAssemblyRequestColumn(string dataPropertyName, string headerText, int width)
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
                var requests = _assemblyRequestRepository.GetPendingRequests();
                
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
                        RequestedAssemblyCount = r.RequestedAssemblyCount.ToString(),
                        ActualClampCount = r.ActualClampCount.HasValue ? r.ActualClampCount.Value.ToString() : "",
                        ResultedAssemblyCount = r.ResultedAssemblyCount.HasValue ? r.ResultedAssemblyCount.Value.ToString() : "",
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
                MessageBox.Show("Montaj talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // ResultedAssemblyCount buton kolonu için
                    else if (columnName == "ResultedAssemblyCount")
                    {
                        var resultedAssemblyCountProperty = item.GetType().GetProperty("ResultedAssemblyCount");
                        if (resultedAssemblyCountProperty != null)
                        {
                            var resultedAssemblyCountValue = resultedAssemblyCountProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(resultedAssemblyCountValue))
                            {
                                e.Value = $"Girildi ({resultedAssemblyCountValue})";
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
            if (columnName != "ActualClampCount" && columnName != "ResultedAssemblyCount")
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

                var request = _assemblyRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Dialog aç
                if (columnName == "ActualClampCount")
                {
                    int? actualClampCount = ShowActualClampCountDialog(request);
                    if (actualClampCount.HasValue)
                    {
                        request.ActualClampCount = actualClampCount.Value;
                        request.Status = "Montajda";
                        _assemblyRequestRepository.Update(request);
                        LoadData();
                    }
                }
                else if (columnName == "ResultedAssemblyCount")
                {
                    int? resultedAssemblyCount = ShowResultedAssemblyCountDialog(request);
                    if (resultedAssemblyCount.HasValue)
                    {
                        request.ResultedAssemblyCount = resultedAssemblyCount.Value;
                        request.Status = "Montajda";
                        _assemblyRequestRepository.Update(request);
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj adedi girilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowActualClampCountDialog(AssemblyRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Kenet Kullanıldı",
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
                    Text = $"İstenen Montaj: {request.RequestedAssemblyCount} adet\n\nKaç tane kenet kullanıldı?",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Kullanılan Kenet Adedi:",
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
                    Value = request.ActualClampCount ?? request.RequestedAssemblyCount,
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

        private int? ShowResultedAssemblyCountDialog(AssemblyRequest request)
        {
            using (var dialog = new Form
            {
                Text = "Kaç Tane Montaj Oluştu",
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
                    Text = $"İstenen Montaj: {request.RequestedAssemblyCount} adet\n\nKaç tane montaj oluştu?",
                    Location = new Point(20, 20),
                    Width = 350,
                    Height = 60,
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblAdet = new Label
                {
                    Text = "Oluşan Montaj Adedi:",
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
                    Value = request.ResultedAssemblyCount ?? request.RequestedAssemblyCount,
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
            decimal tolerance = 0.1m;
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

