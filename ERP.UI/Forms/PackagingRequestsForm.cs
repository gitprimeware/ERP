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
    public partial class PackagingRequestsForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private PackagingRequestRepository _packagingRequestRepository;
        private IsolationRepository _isolationRepository;
        private OrderRepository _orderRepository;

        public PackagingRequestsForm()
        {
            _packagingRequestRepository = new PackagingRequestRepository();
            _isolationRepository = new IsolationRepository();
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
                Text = "📦 Paketleme Talepleri",
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
            AddPackagingRequestColumn("TermDate", "Termin Tarihi", 120);
            AddPackagingRequestColumn("TrexOrderNo", "Trex Kodu", 120);
            AddPackagingRequestColumn("Hatve", "Hatve", 80);
            AddPackagingRequestColumn("Size", "Ölçü", 80);
            AddPackagingRequestColumn("Length", "Uzunluk", 100);
            AddPackagingRequestColumn("Quantity", "Adet", 80);
            AddPackagingRequestColumn("KapakTipi", "Kapak Tipi", 100);
            AddPackagingRequestColumn("ProfilTipi", "Profil Tipi", 100);
            AddPackagingRequestColumn("Customer", "Müşteri", 150);
            AddPackagingRequestColumn("EmployeeName", "Operatör", 150);
            AddPackagingRequestColumn("IstenenPaketleme", "İstenen", 100);
            
            // Paketleme Tamamlandı checkbox kolonu
            var colPaketlemeTamamlandi = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Paketleme Tamamlandı",
                Name = "PaketlemeTamamlandi",
                Width = 180,
                ReadOnly = false
            };
            colPaketlemeTamamlandi.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colPaketlemeTamamlandi);
            
            AddPackagingRequestColumn("Status", "Durum", 100);

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

            // CellValueChanged event'i - checkbox değiştiğinde işlem yap
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            
            // CurrentCellDirtyStateChanged event'i - checkbox değişikliklerini hemen commit et
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;

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

        private void AddPackagingRequestColumn(string dataPropertyName, string headerText, int width)
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
                // Sadece bekleyen ve paketlemede olan talepleri göster
                var requests = _packagingRequestRepository.GetPendingRequests();
                
                var data = requests.Select(r =>
                {
                    var order = r.OrderId.HasValue ? _orderRepository.GetById(r.OrderId.Value) : null;
                    
                    // İstenen paketleme adedi
                    int istenen = r.RequestedPackagingCount;
                    
                    // Kapak Tipi ve Profil Tipi parse et
                    string kapakTipi = "";
                    string profilTipi = "";
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
                        if (parts.Length >= 6)
                        {
                            string kapakStr = parts[5];
                            if (kapakStr == "030")
                                kapakTipi = "30";
                            else if (kapakStr == "002")
                                kapakTipi = "2";
                            else if (kapakStr == "016")
                                kapakTipi = "16";
                            else if (int.TryParse(kapakStr, out int kapakValue))
                                kapakTipi = kapakValue.ToString();
                            else
                                kapakTipi = kapakStr;
                        }
                    }
                    
                    // Uzunluk MM cinsinden saklanıyor
                    decimal lengthMM = r.Length;
                    
                    return new
                    {
                        Id = r.Id,
                        TermDate = order?.TermDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "",
                        TrexOrderNo = order?.TrexOrderNo ?? "",
                        Hatve = GetHatveLetter(r.Hatve),
                        Size = r.Size.ToString("F2", CultureInfo.InvariantCulture),
                        Length = lengthMM.ToString("F2", CultureInfo.InvariantCulture),
                        Quantity = order?.Quantity.ToString() ?? "",
                        KapakTipi = kapakTipi,
                        ProfilTipi = profilTipi,
                        Customer = order?.Company?.Name ?? "",
                        EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "",
                        IstenenPaketleme = istenen.ToString(),
                        PaketlemeTamamlandi = r.Status == "Tamamlandı",
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
                MessageBox.Show("Paketleme talepleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_dataGridView.IsCurrentCellDirty)
            {
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            
            // Checkbox değiştiğinde
            if (columnName != "PaketlemeTamamlandi")
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

                var request = _packagingRequestRepository.GetById(requestId);
                if (request == null)
                    return;

                // Checkbox değerini kontrol et
                bool paketlemeTamamlandi = false;
                if (row.Cells["PaketlemeTamamlandi"].Value != null)
                {
                    paketlemeTamamlandi = (bool)row.Cells["PaketlemeTamamlandi"].Value;
                }

                if (paketlemeTamamlandi)
                {
                    // Onaylama işlemi
                    OnaylaPaketlemeTalebi(request);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paketleme onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Hata durumunda veriyi yeniden yükle
            }
        }

        private void OnaylaPaketlemeTalebi(PackagingRequest request)
        {
            try
            {
                int istenen = request.RequestedPackagingCount;
                
                // ActualPackagingCount yoksa, RequestedPackagingCount'u kullan
                if (!request.ActualPackagingCount.HasValue)
                {
                    request.ActualPackagingCount = request.RequestedPackagingCount;
                }

                int yapilanPaketleme = request.ActualPackagingCount.Value;

                // Status'u Tamamlandı yap
                request.Status = "Tamamlandı";
                request.CompletionDate = DateTime.Now;
                _packagingRequestRepository.Update(request);

                // Event feed kaydı ekle - Paketleme tamamlandı, onay bekliyor
                if (request.OrderId.HasValue)
                {
                    var order = _orderRepository.GetById(request.OrderId.Value);
                    if (order != null)
                    {
                        EventFeedService.PackagingCompleted(request.Id, request.OrderId.Value, order.TrexOrderNo, yapilanPaketleme);
                    }
                }

                MessageBox.Show("Paketleme talebi tamamlandı olarak işaretlendi. Stok tüketimi için Üretim Ayrıntı sayfasındaki Paketleme tab'ından 'Onayla Paketle' butonunu kullanın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Verileri yeniden yükle
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paketleme talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Hata durumunda veriyi yeniden yükle
            }
        }

        private string GetHatveLetter(decimal hatveValue)
        {
            // Hatve değerini "6.5(M)" formatında göster: sayısal değer + harf
            const decimal tolerance = 0.1m;
            string letter = "";
            
            if (Math.Abs(hatveValue - 3.25m) < tolerance || Math.Abs(hatveValue - 3.10m) < tolerance)
                letter = "H";
            else if (Math.Abs(hatveValue - 4.5m) < tolerance || Math.Abs(hatveValue - 4.3m) < tolerance)
                letter = "D";
            else if (Math.Abs(hatveValue - 6.5m) < tolerance || Math.Abs(hatveValue - 6.3m) < tolerance || Math.Abs(hatveValue - 6.4m) < tolerance)
                letter = "M";
            else if (Math.Abs(hatveValue - 9m) < tolerance || Math.Abs(hatveValue - 8.7m) < tolerance || Math.Abs(hatveValue - 8.65m) < tolerance)
                letter = "L";
            
            // Format: 6.5(M) veya sadece sayısal değer (harf bulunamazsa)
            if (!string.IsNullOrEmpty(letter))
                return $"{hatveValue.ToString("F2", CultureInfo.InvariantCulture)}({letter})";
            else
                return hatveValue.ToString("F2", CultureInfo.InvariantCulture); // Eğer tanınmazsa sadece sayısal göster
        }
    }
}

