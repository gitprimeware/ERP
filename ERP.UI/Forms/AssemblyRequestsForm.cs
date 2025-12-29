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
        private ClampingRequestRepository _clampingRequestRepository;

        public AssemblyRequestsForm()
        {
            _assemblyRequestRepository = new AssemblyRequestRepository();
            _clampingRepository = new ClampingRepository();
            _orderRepository = new OrderRepository();
            _clampingRequestRepository = new ClampingRequestRepository();
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
            // Id kolonu kaldırıldı (görünür değil, sadece veri erişimi için LoadData'da anonymous object'te tutuluyor)
            
            AddAssemblyRequestColumn("TermDate", "Termin Tarihi", 120);
            AddAssemblyRequestColumn("TrexOrderNo", "Trex Kodu", 120);
            AddAssemblyRequestColumn("Hatve", "Hatve", 80);
            AddAssemblyRequestColumn("Size", "Ölçü", 80);
            AddAssemblyRequestColumn("Length", "Uzunluk", 100);
            AddAssemblyRequestColumn("Quantity", "Adet", 80);
            AddAssemblyRequestColumn("KapakTipi", "Kapak Tipi", 100);
            AddAssemblyRequestColumn("ProfilTipi", "Profil Tipi", 100);
            AddAssemblyRequestColumn("Customer", "Müşteri", 150);
            AddAssemblyRequestColumn("EmployeeName", "Operatör", 150);
            AddAssemblyRequestColumn("MontajlanacakKenet", "İstenen", 100);
            
            // Yapılan kolonu - editable TextBox
            var colYapilan = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Yapilan",
                HeaderText = "Yapılan",
                Name = "Yapilan",
                Width = 100,
                ReadOnly = false // Düzenlenebilir
            };
            colYapilan.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colYapilan);
            
            // Kalan kolonu - readonly
            AddAssemblyRequestColumn("Kalan", "Kalan", 100);
            
            // Montaj Tamamlandı checkbox kolonu
            var colMontajTamamlandi = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Montaj Tamamlandı",
                Name = "MontajTamamlandi",
                Width = 150,
                ReadOnly = false
            };
            colMontajTamamlandi.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridView.Columns.Add(colMontajTamamlandi);
            
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

            // CellValueChanged event'i - checkbox ve Yapılan değiştiğinde işlem yap
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            
            // CurrentCellDirtyStateChanged event'i - checkbox ve TextBox değişikliklerini hemen commit et
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
                    var order = r.OrderId.HasValue ? _orderRepository.GetById(r.OrderId.Value) : null;
                    
                    // Montajlanacak kenet sayısı (İstenen - RequestedAssemblyCount)
                    int istenen = r.RequestedAssemblyCount;
                    
                    // Yapılan (ResultedAssemblyCount - eğer null ise 0)
                    int yapilan = r.ResultedAssemblyCount ?? 0;
                    
                    // Kalan
                    int kalan = istenen - yapilan;
                    
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
                    
                    // Uzunluk MM cinsinden saklanıyor (artık CM'ye çevirmeye gerek yok)
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
                        MontajlanacakKenet = istenen.ToString(),
                        Yapilan = yapilan.ToString(),
                        Kalan = kalan.ToString(),
                        MontajTamamlandi = r.Status == "Tamamlandı" || kalan == 0,
                        Status = kalan == 0 ? "Tamamlandı" : r.Status
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
            
            // Yapılan kolonu değiştiğinde
            if (columnName == "Yapilan")
            {
                UpdateYapilanValue(e.RowIndex);
                return;
            }
            
            // Checkbox değiştiğinde
            if (columnName != "MontajTamamlandi")
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

                // Checkbox değerini kontrol et
                bool montajTamamlandi = false;
                if (row.Cells["MontajTamamlandi"].Value != null)
                {
                    montajTamamlandi = (bool)row.Cells["MontajTamamlandi"].Value;
                }

                if (montajTamamlandi)
                {
                    // Onaylama işlemi
                    OnaylaMontajTalebi(request);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Hata durumunda veriyi yeniden yükle
            }
        }

        private void UpdateYapilanValue(int rowIndex)
        {
            try
            {
                var row = _dataGridView.Rows[rowIndex];
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

                // Yapılan değerini al
                string yapilanStr = row.Cells["Yapilan"].Value?.ToString() ?? "0";
                if (!int.TryParse(yapilanStr, out int yapilan) || yapilan < 0)
                {
                    MessageBox.Show("Lütfen geçerli bir sayı giriniz (0 veya pozitif).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // Değeri geri almak için yeniden yükle
                    return;
                }

                int istenen = request.RequestedAssemblyCount;
                if (yapilan > istenen)
                {
                    MessageBox.Show($"Yapılan adet, istenen adetten ({istenen}) fazla olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // Değeri geri almak için yeniden yükle
                    return;
                }

                // ResultedAssemblyCount'u güncelle
                request.ResultedAssemblyCount = yapilan;
                
                // Kalan 0 ise status'u Tamamlandı yap, değilse Montajda yap
                int kalan = istenen - yapilan;
                if (kalan == 0)
                {
                    request.Status = "Tamamlandı";
                    request.CompletionDate = DateTime.Now;
                }
                else if (request.Status == "Tamamlandı")
                {
                    // Eğer daha önce tamamlanmışsa ama şimdi kalan varsa, durumu güncelle
                    request.Status = "Montajda";
                    request.CompletionDate = null;
                }
                else if (request.Status == "Beklemede")
                {
                    request.Status = "Montajda";
                }
                
                _assemblyRequestRepository.Update(request);

                // Verileri yeniden yükle (Kalan kolonu güncellenecek)
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yapılan adet güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Hata durumunda veriyi yeniden yükle
            }
        }

        private void OnaylaMontajTalebi(AssemblyRequest request)
        {
            try
            {
                // Montajlanacak kenet sayısı (RequestedAssemblyCount'tan gelecek)
                int montajlanacakKenet = request.RequestedAssemblyCount;

                // Oluşan montaj sipariş adedinden gelecek
                var order = request.OrderId.HasValue ? _orderRepository.GetById(request.OrderId.Value) : null;
                int olusanMontaj = order?.Quantity ?? 0;

                if (olusanMontaj == 0)
                {
                    MessageBox.Show("Sipariş adedi bulunamadı. Montaj talebi onaylanamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // Checkbox'ı geri al
                    return;
                }

                // ActualClampCount ve ResultedAssemblyCount değerlerini set et (sadece bilgi giriliyor, onay ProductionDetailForm'da yapılacak)
                request.ActualClampCount = montajlanacakKenet;
                request.ResultedAssemblyCount = olusanMontaj;
                
                // Durum değişmeyecek, "Beklemede" kalacak
                // Assembly kaydı oluşturulmayacak (ProductionDetailForm'da onaylandığında oluşturulacak)
                _assemblyRequestRepository.Update(request);

                MessageBox.Show($"Bilgiler kaydedildi. Onay için Üretim Ayrıntı sayfasındaki Montaj tab'ından 'Montaj Onayla' butonunu kullanın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Verileri yeniden yükle
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Montaj talebi onaylanırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Hata durumunda veriyi yeniden yükle
            }
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

