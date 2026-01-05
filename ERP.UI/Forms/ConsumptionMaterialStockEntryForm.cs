using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ConsumptionMaterialStockEntryForm : UserControl
    {
        private TabControl _tabControl;
        private CoverStockRepository _coverStockRepository;
        private SideProfileStockRepository _sideProfileStockRepository;
        private SideProfileRemnantRepository _sideProfileRemnantRepository;
        private IsolationStockRepository _isolationStockRepository;

        // Kapak tab kontrolleri
        private ComboBox _cmbKapakProfileType;
        private ComboBox _cmbKapakSize;
        private ComboBox _cmbKapakLength;
        private TextBox _txtKapakQuantity;
        private Button _btnKapakSave;

        // Yan Profil tab kontrolleri
        private ComboBox _cmbYanProfilProfileType;
        private TextBox _txtYanProfilLength;
        private TextBox _txtYanProfilQuantity;
        private Button _btnYanProfilSave;
        private DataGridView _dgvYanProfilRemnants;

        // İzolasyon Sıvısı tab kontrolleri
        private ComboBox _cmbIzolasyonType;
        private TextBox _txtIzolasyonQuantity;
        private Button _btnIzolasyonSave;

        public ConsumptionMaterialStockEntryForm()
        {
            _coverStockRepository = new CoverStockRepository();
            _sideProfileStockRepository = new SideProfileStockRepository();
            _sideProfileRemnantRepository = new SideProfileRemnantRepository();
            _isolationStockRepository = new IsolationStockRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            CreateTabControl();
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(15, 5),
                Appearance = TabAppearance.FlatButtons
            };

            // Tab çizim modu
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.DrawItem += (s, e) =>
            {
                var tabPage = _tabControl.TabPages[e.Index];
                var tabRect = _tabControl.GetTabRect(e.Index);
                var textColor = e.Index == _tabControl.SelectedIndex ? ThemeColors.Primary : ThemeColors.TextSecondary;
                var backColor = e.Index == _tabControl.SelectedIndex ? Color.White : Color.FromArgb(245, 245, 245);

                // Arka plan
                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, tabRect);
                }

                // Metin
                using (var emojiFont = new Font("Segoe UI Emoji", 10F))
                {
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, emojiFont, 
                        tabRect, textColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                
                e.DrawFocusRectangle();
            };

            // Kapak Tab
            var tabKapak = new TabPage("🪟 Kapak");
            tabKapak.Padding = new Padding(0);
            tabKapak.BackColor = Color.White;
            tabKapak.UseVisualStyleBackColor = false;
            CreateTabContent(tabKapak, "Kapak");
            _tabControl.TabPages.Add(tabKapak);

            // Yan Profil Tab
            var tabYanProfil = new TabPage("📐 Yan Profil");
            tabYanProfil.Padding = new Padding(0);
            tabYanProfil.BackColor = Color.White;
            tabYanProfil.UseVisualStyleBackColor = false;
            CreateTabContent(tabYanProfil, "Yan Profil");
            _tabControl.TabPages.Add(tabYanProfil);

            // İzolasyon Sıvısı Tab
            var tabIzolasyon = new TabPage("💧 İzolasyon Sıvısı");
            tabIzolasyon.Padding = new Padding(0);
            tabIzolasyon.BackColor = Color.White;
            tabIzolasyon.UseVisualStyleBackColor = false;
            CreateTabContent(tabIzolasyon, "İzolasyon Sıvısı");
            _tabControl.TabPages.Add(tabIzolasyon);

            this.Controls.Add(_tabControl);
        }

        private void CreateTabContent(TabPage tab, string tabName)
        {
            if (tabName == "Kapak")
            {
                CreateKapakTabContent(tab);
            }
            else if (tabName == "Yan Profil")
            {
                CreateYanProfilTabContent(tab);
            }
            else if (tabName == "İzolasyon Sıvısı")
            {
                CreateIzolasyonTabContent(tab);
            }
            else
            {
                // Diğer tab'lar için placeholder
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(20)
                };

                var titleLabel = new Label
                {
                    Text = $"➕ {tabName} Stok Gir",
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = ThemeColors.Primary,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };

                var infoLabel = new Label
                {
                    Text = "Bu sayfa yakında eklenecek...",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = ThemeColors.TextSecondary,
                    AutoSize = true,
                    Location = new Point(20, 60)
                };

                panel.Controls.Add(titleLabel);
                panel.Controls.Add(infoLabel);
                tab.Controls.Add(panel);
            }
        }

        private void CreateYanProfilTabContent(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // İki kolonlu TableLayoutPanel
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Sol Panel - Stok Girişi
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            CreateYanProfilLeftPanel(leftPanel);
            tableLayout.Controls.Add(leftPanel, 0, 0);

            // Sağ Panel - Kalan Parçalar
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            CreateYanProfilRightPanel(rightPanel);
            tableLayout.Controls.Add(rightPanel, 1, 0);

            mainPanel.Controls.Add(tableLayout);
            tab.Controls.Add(mainPanel);
        }

        private void CreateYanProfilLeftPanel(Panel panel)
        {
            int yPos = 30;
            int labelWidth = 150;
            int controlWidth = 250;
            int controlHeight = 32;
            int spacing = 40;

            // Başlık
            var titleLabel = new Label
            {
                Text = "➕ Yan Profil Stok Gir",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(titleLabel);
            yPos += 50;

            // Profil Tipi
            var lblProfileType = new Label
            {
                Text = "Profil Tipi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbYanProfilProfileType = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbYanProfilProfileType.Items.Add("Standart");
            _cmbYanProfilProfileType.Items.Add("Geniş");
            panel.Controls.Add(lblProfileType);
            panel.Controls.Add(_cmbYanProfilProfileType);
            yPos += spacing;

            // Uzunluk (Metre)
            var lblLength = new Label
            {
                Text = "Uzunluk (m):",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtYanProfilLength = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            panel.Controls.Add(lblLength);
            panel.Controls.Add(_txtYanProfilLength);
            yPos += spacing;

            // Adet
            var lblQuantity = new Label
            {
                Text = "Adet:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtYanProfilQuantity = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            panel.Controls.Add(lblQuantity);
            panel.Controls.Add(_txtYanProfilQuantity);
            yPos += spacing + 20;

            // Kaydet butonu
            _btnYanProfilSave = new Button
            {
                Text = "💾 Kaydet",
                Location = new Point(180, yPos),
                Width = 150,
                Height = 40,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnYanProfilSave, 4);
            _btnYanProfilSave.Click += BtnYanProfilSave_Click;
            panel.Controls.Add(_btnYanProfilSave);
        }

        private void CreateYanProfilRightPanel(Panel panel)
        {
            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 Kalan Parçalar",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 30)
            };
            panel.Controls.Add(titleLabel);

            // DataGridView
            _dgvYanProfilRemnants = new DataGridView
            {
                Location = new Point(20, 80),
                Width = panel.Width - 40,
                Height = panel.Height - 120,
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

            // Kolonlar
            var idColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                Name = "Id",
                Visible = false
            };
            _dgvYanProfilRemnants.Columns.Add(idColumn);

            _dgvYanProfilRemnants.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileType",
                HeaderText = "Profil Tipi",
                Name = "ProfileType",
                Width = 120
            });

            _dgvYanProfilRemnants.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "Uzunluk (m)",
                Name = "Length",
                Width = 150
            });

            _dgvYanProfilRemnants.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Adet",
                Name = "Quantity",
                Width = 100
            });

            // Hurda işaretleme butonu kolonu
            var btnColumn = new DataGridViewButtonColumn
            {
                HeaderText = "İşlem",
                Name = "MarkAsWaste",
                Text = "Hurda İşaretle",
                UseColumnTextForButtonValue = true,
                Width = 120
            };
            _dgvYanProfilRemnants.Columns.Add(btnColumn);

            // Stil ayarları
            _dgvYanProfilRemnants.DefaultCellStyle.BackColor = Color.White;
            _dgvYanProfilRemnants.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvYanProfilRemnants.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvYanProfilRemnants.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvYanProfilRemnants.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvYanProfilRemnants.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvYanProfilRemnants.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvYanProfilRemnants.EnableHeadersVisualStyles = false;

            _dgvYanProfilRemnants.CellContentClick += DgvYanProfilRemnants_CellContentClick;

            panel.Controls.Add(_dgvYanProfilRemnants);

            // Verileri yükle
            LoadYanProfilRemnants();
        }

        private void BtnYanProfilSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (_cmbYanProfilProfileType.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen profil tipi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtYanProfilLength.Text) || !decimal.TryParse(_txtYanProfilLength.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal length) || length <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir uzunluk giriniz (metre cinsinden).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtYanProfilQuantity.Text) || !int.TryParse(_txtYanProfilQuantity.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir adet giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // SideProfileStock oluştur
                var stock = new SideProfileStock
                {
                    ProfileType = _cmbYanProfilProfileType.SelectedItem.ToString(),
                    Length = length,
                    InitialQuantity = quantity,
                    UsedLength = 0,
                    WastedLength = 0
                };

                // Aynı uzunlukta ve profil tipinde varsa adedi artır, yoksa yeni kayıt oluştur
                _sideProfileStockRepository.InsertOrUpdate(stock);

                MessageBox.Show("Yan profil stoku başarıyla kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Formu temizle
                _cmbYanProfilProfileType.SelectedIndex = -1;
                _txtYanProfilLength.Text = "";
                _txtYanProfilQuantity.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yan profil stoku kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadYanProfilRemnants()
        {
            try
            {
                var remnants = _sideProfileRemnantRepository.GetAll(includeWaste: false); // Sadece hurda olmayanları göster
                
                var data = remnants.Select(r => new
                {
                    Id = r.Id,
                    ProfileType = r.ProfileType,
                    Length = $"{r.Length.ToString("F2", CultureInfo.InvariantCulture)} m",
                    Quantity = r.Quantity.ToString()
                }).ToList();

                _dgvYanProfilRemnants.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kalan parçalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvYanProfilRemnants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _dgvYanProfilRemnants.Columns["MarkAsWaste"].Index)
                return;

            try
            {
                var row = _dgvYanProfilRemnants.Rows[e.RowIndex];
                if (row.DataBoundItem == null)
                    return;

                // Id'yi al (reflection ile)
                var idProperty = row.DataBoundItem.GetType().GetProperty("Id");
                if (idProperty == null)
                    return;

                Guid id = (Guid)idProperty.GetValue(row.DataBoundItem);

                var result = MessageBox.Show("Bu parçayı hurda olarak işaretlemek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _sideProfileRemnantRepository.MarkAsWaste(id);
                    MessageBox.Show("Parça hurda olarak işaretlendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadYanProfilRemnants(); // Listeyi yenile
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hurda işaretleme sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateKapakTabContent(TabPage tab)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            int yPos = 30;
            int labelWidth = 150;
            int controlWidth = 300;
            int controlHeight = 32;
            int spacing = 40;

            // Başlık
            var titleLabel = new Label
            {
                Text = "➕ Kapak Stok Gir",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            panel.Controls.Add(titleLabel);
            yPos += 50;

            // Profil Tipi
            var lblProfileType = new Label
            {
                Text = "Profil Tipi:",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbKapakProfileType = new ComboBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbKapakProfileType.Items.Add("Standart");
            _cmbKapakProfileType.Items.Add("Geniş");
            _cmbKapakProfileType.SelectedIndexChanged += CmbKapakProfileType_SelectedIndexChanged;
            panel.Controls.Add(lblProfileType);
            panel.Controls.Add(_cmbKapakProfileType);
            yPos += spacing;

            // Ölçü
            var lblSize = new Label
            {
                Text = "Kapak Ölçüsü mm:",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbKapakSize = new ComboBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            panel.Controls.Add(lblSize);
            panel.Controls.Add(_cmbKapakSize);
            yPos += spacing;

            // Kapak Modeli
            var lblCoverLength = new Label
            {
                Text = "Kapak Modeli:",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbKapakLength = new ComboBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbKapakLength.Items.Add(new LengthItem { Value = 2, Display = "2mm-düz kapak" });
            _cmbKapakLength.Items.Add(new LengthItem { Value = 30, Display = "30mm-normal kapak" });
            _cmbKapakLength.DisplayMember = "Display";
            _cmbKapakLength.ValueMember = "Value";
            panel.Controls.Add(lblCoverLength);
            panel.Controls.Add(_cmbKapakLength);
            yPos += spacing;

            // Adet
            var lblQuantity = new Label
            {
                Text = "Adet:",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtKapakQuantity = new TextBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            panel.Controls.Add(lblQuantity);
            panel.Controls.Add(_txtKapakQuantity);
            yPos += spacing + 20;

            // Kaydet butonu
            _btnKapakSave = new Button
            {
                Text = "💾 Kaydet",
                Location = new Point(190, yPos),
                Width = 150,
                Height = 40,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnKapakSave, 4);
            _btnKapakSave.Click += BtnKapakSave_Click;
            panel.Controls.Add(_btnKapakSave);

            tab.Controls.Add(panel);
        }

        private void CmbKapakProfileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbKapakProfileType.SelectedItem == null)
            {
                _cmbKapakSize.Enabled = false;
                _cmbKapakSize.Items.Clear();
                return;
            }

            _cmbKapakSize.Items.Clear();
            bool isStandart = _cmbKapakProfileType.SelectedItem.ToString() == "Standart";
            string profileText = isStandart ? "Standart" : "Geniş";

            if (isStandart)
            {
                // Standart: 200, 300, 400, 500, 600, 700, 800, 1000
                var sizes = new[] { 200, 300, 400, 500, 600, 700, 800, 1000 };
                foreach (var size in sizes)
                {
                    _cmbKapakSize.Items.Add(new SizeItem { Value = size, Display = $"{size} mm ({profileText})" });
                }
            }
            else
            {
                // Geniş: 211, 311, 411, 511, 611, 711, 811, 1011
                var sizes = new[] { 211, 311, 411, 511, 611, 711, 811, 1011 };
                foreach (var size in sizes)
                {
                    _cmbKapakSize.Items.Add(new SizeItem { Value = size, Display = $"{size} mm ({profileText})" });
                }
            }

            _cmbKapakSize.DisplayMember = "Display";
            _cmbKapakSize.ValueMember = "Value";
            _cmbKapakSize.Enabled = true;
            if (_cmbKapakSize.Items.Count > 0)
                _cmbKapakSize.SelectedIndex = 0;
        }

        private class SizeItem
        {
            public int Value { get; set; }
            public string Display { get; set; }
        }

        private class LengthItem
        {
            public int Value { get; set; }
            public string Display { get; set; }
        }

        private void CreateIzolasyonTabContent(TabPage tab)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            int yPos = 30;
            int labelWidth = 150;
            int controlWidth = 300;
            int controlHeight = 32;
            int spacing = 40;

            // Başlık
            var titleLabel = new Label
            {
                Text = "➕ İzolasyon Sıvısı Stok Gir",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            panel.Controls.Add(titleLabel);
            yPos += 50;

            // Ürün Türü
            var lblLiquidType = new Label
            {
                Text = "Ürün Türü:",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _cmbIzolasyonType = new ComboBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbIzolasyonType.Items.Add("İzosiyanat");
            _cmbIzolasyonType.Items.Add("Poliol");
            _cmbIzolasyonType.Items.Add("MS Silikon");
            panel.Controls.Add(lblLiquidType);
            panel.Controls.Add(_cmbIzolasyonType);
            yPos += spacing;

            // Kilogram
            var lblQuantity = new Label
            {
                Text = "Kilogram (kg):",
                Location = new Point(30, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _txtIzolasyonQuantity = new TextBox
            {
                Location = new Point(190, yPos - 3),
                Width = controlWidth,
                Height = controlHeight,
                Font = new Font("Segoe UI", 10F)
            };
            panel.Controls.Add(lblQuantity);
            panel.Controls.Add(_txtIzolasyonQuantity);
            yPos += spacing + 20;

            // Kaydet butonu
            _btnIzolasyonSave = new Button
            {
                Text = "💾 Kaydet",
                Location = new Point(190, yPos),
                Width = 150,
                Height = 40,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnIzolasyonSave, 4);
            _btnIzolasyonSave.Click += BtnIzolasyonSave_Click;
            panel.Controls.Add(_btnIzolasyonSave);

            tab.Controls.Add(panel);
        }

        private void BtnIzolasyonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (_cmbIzolasyonType.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen ürün türü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtIzolasyonQuantity.Text) || !decimal.TryParse(_txtIzolasyonQuantity.Text, out decimal kilogram) || kilogram <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir kilogram değeri giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // IsolationStock oluştur
                var stock = new IsolationStock
                {
                    LiquidType = _cmbIzolasyonType.SelectedItem.ToString(),
                    Kilogram = kilogram,
                    Quantity = 0, // Geriye uyumluluk için 0 (artık kullanılmıyor)
                    Liter = 0 // Geriye uyumluluk için 0 (artık kullanılmıyor)
                };

                _isolationStockRepository.Insert(stock);

                MessageBox.Show("İzolasyon sıvısı stoku başarıyla kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Formu temizle
                _cmbIzolasyonType.SelectedIndex = -1;
                _txtIzolasyonQuantity.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzolasyon sıvısı stoku kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKapakSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (_cmbKapakProfileType.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen profil tipi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_cmbKapakSize.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen ölçü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_cmbKapakLength.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen kapak modeli seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtKapakQuantity.Text) || !int.TryParse(_txtKapakQuantity.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Lütfen geçerli bir adet giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Değerleri al
                string profileType = _cmbKapakProfileType.SelectedItem.ToString();
                var sizeItem = (SizeItem)_cmbKapakSize.SelectedItem;
                int size = sizeItem.Value;
                var lengthItem = (LengthItem)_cmbKapakLength.SelectedItem;
                int coverLength = lengthItem.Value;

                // CoverStock oluştur
                var coverStock = new CoverStock
                {
                    ProfileType = profileType,
                    Size = size,
                    CoverLength = coverLength,
                    Quantity = quantity
                };

                // InsertOrUpdate: Varsa stoğa ekle, yoksa yeni kayıt oluştur
                _coverStockRepository.InsertOrUpdate(coverStock);

                MessageBox.Show("Kapak stoku başarıyla kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Formu temizle
                _cmbKapakProfileType.SelectedIndex = -1;
                _cmbKapakSize.Items.Clear();
                _cmbKapakSize.Enabled = false;
                _cmbKapakLength.SelectedIndex = -1;
                _txtKapakQuantity.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kapak stoku kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

