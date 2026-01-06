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
            
            // Yapılan kolonu - buton kolonu
            var colYapilan = new DataGridViewButtonColumn
            {
                HeaderText = "Yapılan",
                Name = "Yapilan",
                Width = 120,
                Text = "Gir",
                UseColumnTextForButtonValue = false // Dinamik buton metni için false
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

            // CellValueChanged event'i - checkbox değiştiğinde işlem yap
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            
            // CurrentCellDirtyStateChanged event'i - checkbox değişikliklerini hemen commit et
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;
            
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
                // Sadece bekleyen ve montajda olan talepleri göster - "Tamamlandı" ve "İptal" durumundaki talepler listede görünmemeli
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
                        MontajTamamlandi = r.Status == "Tamamlandı",
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

        private void DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_dataGridView.IsCurrentCellDirty)
            {
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
                
                // Yapılan buton kolonu için
                if (columnName == "Yapilan")
                {
                    var row = _dataGridView.Rows[e.RowIndex];
                    if (row.DataBoundItem != null)
                    {
                        var item = row.DataBoundItem;
                        var yapilanProperty = item.GetType().GetProperty("Yapilan");
                        if (yapilanProperty != null)
                        {
                            var yapilanValue = yapilanProperty.GetValue(item)?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(yapilanValue) && yapilanValue != "0")
                            {
                                e.Value = $"Girildi ({yapilanValue})";
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
            
            // Yapılan buton kolonuna tıklandığında
            if (columnName == "Yapilan")
            {
                UpdateYapilanValue(e.RowIndex);
                return;
            }
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            
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

                int istenen = request.RequestedAssemblyCount;
                // Mevcut montajlanan kenet sayısını al (ActualClampCount varsa onu kullan, yoksa ResultedAssemblyCount'u kullan)
                int mevcutMontajlananKenet = request.ActualClampCount ?? request.ResultedAssemblyCount ?? 0;

                // Dialog göster
                int? yapilan = ShowYapilanDialog(istenen, mevcutMontajlananKenet);
                if (!yapilan.HasValue)
                    return;

                if (yapilan.Value > istenen)
                {
                    MessageBox.Show($"Yapılan adet, istenen adetten ({istenen}) fazla olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Montajlanan kenet sayısı = Oluşan montaj sayısı (1:1 oran)
                // Girilen değer hem ActualClampCount hem de ResultedAssemblyCount'a atanır
                request.ActualClampCount = yapilan.Value; // Montajlanan kenet sayısı
                request.ResultedAssemblyCount = yapilan.Value; // Oluşan montaj sayısı (aynı değer)
                
                // Status güncellemesi - otomatik tamamlandı yapmıyoruz
                if (request.Status == "Beklemede")
                {
                    request.Status = "Montajda";
                }
                // Eğer daha önce tamamlanmışsa ve şimdi değiştirildiyse, durumu Montajda yap
                else if (request.Status == "Tamamlandı" && istenen != yapilan.Value)
                {
                    request.Status = "Montajda";
                    request.CompletionDate = null;
                }
                
                // Event feed kaydı ekle - Montaj tamamlandı, onay bekliyor
                if (request.OrderId.HasValue)
                {
                    var orderRepository = new OrderRepository();
                    var order = orderRepository.GetById(request.OrderId.Value);
                    if (order != null)
                    {
                        EventFeedService.AssemblyCompleted(request.Id, request.OrderId.Value, order.TrexOrderNo, yapilan.Value);
                    }
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

        private int? ShowYapilanDialog(int istenen, int mevcutMontajlananKenet)
        {
            using (var dialog = new Form
            {
                Text = "Montajlanan Kenet Sayısı",
                Width = 380,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblInfo = new Label
                {
                    Text = $"İstenen Adet: {istenen}",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblCount = new Label
                {
                    Text = "Montajlanan Kenet Sayısı:",
                    Location = new Point(20, 60),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                };

                var txtCount = new NumericUpDown
                {
                    Location = new Point(200, 57),
                    Width = 150,
                    Minimum = 0,
                    Maximum = 999999,
                    Value = mevcutMontajlananKenet
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

        private void OnaylaMontajTalebi(AssemblyRequest request)
        {
            try
            {
                int istenen = request.RequestedAssemblyCount;
                
                // Montajlanan kenet sayısı = Oluşan montaj sayısı (1:1 oran)
                // ActualClampCount (kullanılan/montajlanan kenet) girilmiş olmalı
                if (!request.ActualClampCount.HasValue)
                {
                    MessageBox.Show("Lütfen önce kaç tane kenet montajlandığını giriniz (Yapılan butonuna tıklayarak).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // Checkbox'ı geri al
                    return;
                }

                int montajlananKenetSayisi = request.ActualClampCount.Value;
                
                // Montajlanan kenet sayısı = Oluşan montaj sayısı (1:1 oran)
                // Eğer ResultedAssemblyCount farklı bir değerse, ActualClampCount'a eşitle
                if (!request.ResultedAssemblyCount.HasValue || request.ResultedAssemblyCount.Value != montajlananKenetSayisi)
                {
                    request.ResultedAssemblyCount = montajlananKenetSayisi;
                }

                // İstenen ile kontrol
                if (istenen != montajlananKenetSayisi)
                {
                    MessageBox.Show($"İstenen adet ({istenen}) ile montajlanan kenet sayısı ({montajlananKenetSayisi}) eşleşmiyor! Montaj tamamlandı olarak işaretlenemez.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // Checkbox'ı geri al
                    return;
                }

                // Status'u Tamamlandı yap (stok tüketimi ProductionDetailForm'da yapılacak)
                request.Status = "Tamamlandı";
                request.CompletionDate = DateTime.Now;
                _assemblyRequestRepository.Update(request);

                MessageBox.Show("Montaj talebi tamamlandı olarak işaretlendi. Stok tüketimi için Üretim Ayrıntı sayfasındaki Montaj tab'ından 'Montaj Onayla' butonunu kullanın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

