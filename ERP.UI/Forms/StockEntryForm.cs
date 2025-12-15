using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class StockEntryForm : UserControl
    {
        private Panel mainPanel;
        private TableLayoutPanel tableLayout;
        private ComboBox cmbCompany;
        private Button btnAddCompany;
        private TextBox txtCustomerOrderNo;
        private TextBox txtTrexOrderNo;
        private TextBox txtDeviceName;
        private DateTimePicker dtpOrderDate;
        private DateTimePicker dtpTermDate;
        private Button btnProductCode;
        private TextBox txtProductCode;
        private TextBox txtBypassSize;
        private TextBox txtBypassType;
        private ComboBox cmbLamelThickness;
        private ComboBox cmbProductType;
        private NumericUpDown nudQuantity;
        private Button btnSave;
        private Button btnCancel;
        private Label lblTitle;

        private Guid _orderId = Guid.Empty;
        private bool _isEditMode = false;

        public Guid OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                _isEditMode = value != Guid.Empty;
                UpdateFormTitle();
            }
        }

        public StockEntryForm() : this(Guid.Empty)
        {
        }

        public StockEntryForm(Guid orderId)
        {
            _orderId = orderId;
            _isEditMode = orderId != Guid.Empty;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            // WinForms Designer için gerekli
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            GenerateTrexOrderNo();
            SetDefaultDates();
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            // Başlık
            lblTitle = new Label
            {
                Text = _isEditMode ? "Stok Güncelle" : "Yeni Stok Girişi",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // TableLayoutPanel oluştur
            CreateTableLayout();

            // Butonlar - tableLayout'un altına yerleştir
            var buttonPanel = CreateButtonPanel();
            
            // TableLayout'un boyutunu hesapla ve buton panelini konumlandır
            void UpdateButtonPanelLocation()
            {
                if (buttonPanel != null && tableLayout != null)
                {
                    buttonPanel.Location = new Point(30, tableLayout.Bottom + 20);
                }
            }
            
            tableLayout.SizeChanged += (s, e) => UpdateButtonPanelLocation();
            tableLayout.LocationChanged += (s, e) => UpdateButtonPanelLocation();
            
            // İlk konumlandırma
            UpdateButtonPanelLocation();

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(tableLayout);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();
        }

        private void CreateTableLayout()
        {
            tableLayout = new TableLayoutPanel
            {
                Location = new Point(30, 80),
                Width = mainPanel.Width - 60,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Label genişliği
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Input 1
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Label 2
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Input 2

            int row = 0;

            // Firma satırı
            AddTableRow("Firma:", CreateCompanyControl(), "+ Yeni Firma Ekle", CreateAddCompanyButton(), row++);

            // Müşteri Sipariş No (Readonly - "Stok")
            txtCustomerOrderNo = new TextBox { Text = "Stok", ReadOnly = true, BackColor = ThemeColors.SurfaceDark };
            AddTableRow("Müşteri Sipariş No:", CreateReadOnlyTextBox(txtCustomerOrderNo), 
                       "Trex Sipariş No:", CreateTextBox(txtTrexOrderNo = new TextBox()), row++);

            // Cihaz Adı (Readonly - "Stok") ve Sipariş tarihi
            txtDeviceName = new TextBox { Text = "Stok", ReadOnly = true, BackColor = ThemeColors.SurfaceDark };
            AddTableRow("Cihaz Adı:", CreateReadOnlyTextBox(txtDeviceName),
                       "Sipariş Tarihi:", CreateDateTimePicker(dtpOrderDate = new DateTimePicker()), row++);

            // Termin tarihi
            AddTableRow("Termin Tarihi:", CreateDateTimePicker(dtpTermDate = new DateTimePicker()),
                       "", new Panel(), row++);

            // Ürün kodu ve Bypass ölçüsü
            AddTableRow("Ürün Kodu:", CreateProductCodeControl(),
                       "Bypass Ölçüsü:", CreateTextBox(txtBypassSize = new TextBox()), row++);

            // Bypass türü ve Lamel kalınlığı
            AddTableRow("Bypass Türü:", CreateTextBox(txtBypassType = new TextBox()),
                       "Lamelle Kalınlığı:", CreateLamelThicknessCombo(), row++);

            // Ürün türü ve Miktar
            AddTableRow("Ürün Türü:", CreateProductTypeCombo(),
                       "Miktar:", CreateQuantityControl(), row++);

            // Fiyat alanları kaldırıldı - stok girişinde gösterilmiyor, arka planda otomatik 0

            mainPanel.Resize += (s, e) =>
            {
                tableLayout.Width = mainPanel.Width - 60;
            };
        }

        private void AddTableRow(string label1Text, Control control1, string label2Text, Control control2, int row)
        {
            tableLayout.RowCount = row + 1;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            var lbl1 = new Label
            {
                Text = label1Text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };

            var lbl2 = new Label
            {
                Text = label2Text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };

            control1.Dock = DockStyle.Fill;
            control1.Margin = new Padding(5, 10, 5, 10);
            control2.Dock = DockStyle.Fill;
            control2.Margin = new Padding(5, 10, 5, 10);

            tableLayout.Controls.Add(lbl1, 0, row);
            tableLayout.Controls.Add(control1, 1, row);
            tableLayout.Controls.Add(lbl2, 2, row);
            tableLayout.Controls.Add(control2, 3, row);
        }

        private Control CreateCompanyControl()
        {
            cmbCompany = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            LoadCompanies();
            return cmbCompany;
        }

        private Control CreateAddCompanyButton()
        {
            btnAddCompany = new Button
            {
                Text = "+ Yeni Firma Ekle",
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(btnAddCompany, 4);
            btnAddCompany.Click += BtnAddCompany_Click;
            return btnAddCompany;
        }

        private Control CreateProductCodeControl()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            
            txtProductCode = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 75, 0)
            };

            btnProductCode = new Button
            {
                Text = "Seç",
                Width = 70,
                Font = new Font("Segoe UI", 10F),
                BackColor = ThemeColors.Info,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Margin = new Padding(0)
            };
            UIHelper.ApplyRoundedButton(btnProductCode, 4);
            btnProductCode.Click += BtnProductCode_Click;

            panel.Controls.Add(txtProductCode);
            panel.Controls.Add(btnProductCode);
            
            return panel;
        }

        private Control CreateLamelThicknessCombo()
        {
            cmbLamelThickness = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbLamelThickness.Items.AddRange(new[] { "0.10", "0.12", "0.15", "0.165", "0.180" });
            return cmbLamelThickness;
        }

        private Control CreateProductTypeCombo()
        {
            cmbProductType = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbProductType.Items.AddRange(new[] { "Normal", "Epoksi Boyalı" });
            return cmbProductType;
        }

        private Control CreateQuantityControl()
        {
            nudQuantity = new NumericUpDown
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Minimum = 1,
                Maximum = 9999,
                Value = 1
            };
            return nudQuantity;
        }

        private Control CreateTextBox(TextBox textBox)
        {
            textBox.Height = 30;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            return textBox;
        }

        private Control CreateReadOnlyTextBox(TextBox textBox)
        {
            textBox.Height = 30;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.ReadOnly = true;
            textBox.BackColor = ThemeColors.SurfaceDark;
            return textBox;
        }

        private Control CreateDateTimePicker(DateTimePicker dateTimePicker)
        {
            dateTimePicker.Height = 30;
            dateTimePicker.Font = new Font("Segoe UI", 10F);
            dateTimePicker.Format = DateTimePickerFormat.Short;
            dateTimePicker.CalendarMonthBackground = Color.White;
            return dateTimePicker;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            if (mainPanel != null)
            {
                panel.Width = mainPanel.Width - 60;
            }
            else
            {
                panel.Width = 800;
            }

            mainPanel.Resize += (s, e) =>
            {
                if (mainPanel != null && panel != null)
                {
                    panel.Width = mainPanel.Width - 60;
                    UpdateButtonPositions(panel);
                }
            };

            btnSave = ButtonFactory.CreateSuccessButton("Stok Girişini Tamamla");
            btnSave.Height = 35;
            btnSave.Width = 180;
            btnSave.Anchor = AnchorStyles.None;
            btnSave.Click += BtnSave_Click;
            btnSave.Visible = true;

            btnCancel = ButtonFactory.CreateCancelButton("İptal");
            btnCancel.Height = 35;
            btnCancel.Width = 120;
            btnCancel.Anchor = AnchorStyles.None;
            btnCancel.Click += BtnCancel_Click;
            btnCancel.Visible = true;

            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnCancel);

            UpdateButtonPositions(panel);

            return panel;
        }

        private void UpdateButtonPositions(Panel panel)
        {
            if (panel == null) return;

            int panelWidth = panel.Width > 0 ? panel.Width : 800;
            int rightMargin = 10;
            int buttonSpacing = 10;
            int yPos = 5;

            int currentX = panelWidth - rightMargin;

            if (btnCancel != null && btnCancel.Visible)
            {
                currentX -= btnCancel.Width;
                btnCancel.Location = new Point(currentX, yPos);
                currentX -= buttonSpacing;
            }

            if (btnSave != null && btnSave.Visible)
            {
                currentX -= btnSave.Width;
                btnSave.Location = new Point(currentX, yPos);
            }
        }

        private void LoadCompanies()
        {
            try
            {
                cmbCompany.Items.Clear();
                var companyRepository = new CompanyRepository();
                var companies = companyRepository.GetAll();
                
                foreach (var company in companies)
                {
                    cmbCompany.Items.Add(new { Id = company.Id, Name = company.Name });
                }
                
                cmbCompany.DisplayMember = "Name";
                cmbCompany.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateTrexOrderNo()
        {
            if (string.IsNullOrWhiteSpace(txtTrexOrderNo.Text) || !_isEditMode)
            {
                int year = DateTime.Now.Year;
                int orderNumber = GetNextStockNumber();
                txtTrexOrderNo.Text = $"YM-{year}-{orderNumber:0000}";
            }
        }

        private int GetNextStockNumber()
        {
            try
            {
                var orderRepository = new OrderRepository();
                return orderRepository.GetNextStockNumber(DateTime.Now.Year);
            }
            catch
            {
                return 1;
            }
        }

        private void SetDefaultDates()
        {
            dtpOrderDate.Value = DateTime.Now;
            dtpTermDate.Value = DateTime.Now;
        }

        private void UpdateFormTitle()
        {
            if (lblTitle != null)
            {
                lblTitle.Text = _isEditMode ? "Stok Güncelle" : "Yeni Stok Girişi";
            }
        }

        private void BtnAddCompany_Click(object sender, EventArgs e)
        {
            using (var dialog = new Form
            {
                Text = "Yeni Firma Ekle",
                Width = 400,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblName = new Label
                {
                    Text = "Firma Adı:",
                    Location = new Point(20, 30),
                    AutoSize = true
                };

                var txtName = new TextBox
                {
                    Location = new Point(100, 27),
                    Width = 250,
                    Height = 25
                };

                var btnOk = new Button
                {
                    Text = "Kaydet",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 70),
                    Width = 80
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 70),
                    Width = 80
                };

                dialog.Controls.AddRange(new Control[] { lblName, txtName, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    try
                    {
                        var companyRepository = new CompanyRepository();
                        var newCompany = new ERP.Core.Models.Company { Name = txtName.Text };
                        var companyId = companyRepository.Insert(newCompany);
                        
                        LoadCompanies();
                        
                        foreach (var item in cmbCompany.Items)
                        {
                            var idProperty = item.GetType().GetProperty("Id");
                            if (idProperty != null && idProperty.GetValue(item).Equals(companyId))
                            {
                                cmbCompany.SelectedItem = item;
                                break;
                            }
                        }
                        
                        MessageBox.Show("Firma başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Firma eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnProductCode_Click(object sender, EventArgs e)
        {
            using (var dialog = new ProductCodeSelectionDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtProductCode.Text = dialog.SelectedProductCode;
                }
            }
        }

        private bool ValidateForm()
        {
            if (cmbCompany.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir firma seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCompany.Focus();
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var orderRepository = new OrderRepository();
                var order = MapFormToOrder();

                if (_isEditMode)
                {
                    orderRepository.Update(order);
                    UpdateFormTitle();
                    MessageBox.Show("Stok girişi tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var orderId = orderRepository.Insert(order);
                    _orderId = orderId;
                    _isEditMode = true;
                    UpdateFormTitle();
                    MessageBox.Show("Stok girişi tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stok girişi kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ERP.Core.Models.Order MapFormToOrder()
        {
            var order = new ERP.Core.Models.Order
            {
                Id = _orderId != Guid.Empty ? _orderId : Guid.NewGuid(),
                CustomerOrderNo = "Stok", // Her zaman "Stok"
                TrexOrderNo = txtTrexOrderNo.Text.Trim(),
                DeviceName = "Stok", // Her zaman "Stok"
                OrderDate = dtpOrderDate.Value,
                TermDate = dtpTermDate.Value,
                ProductCode = string.IsNullOrWhiteSpace(txtProductCode.Text) ? null : txtProductCode.Text.Trim(),
                BypassSize = string.IsNullOrWhiteSpace(txtBypassSize.Text) ? null : txtBypassSize.Text.Trim(),
                BypassType = string.IsNullOrWhiteSpace(txtBypassType.Text) ? null : txtBypassType.Text.Trim(),
                ProductType = cmbProductType.SelectedItem?.ToString(),
                Quantity = (int)nudQuantity.Value,
                SalesPrice = 0, // Otomatik 0
                TotalPrice = 0, // Otomatik 0
                CurrencyRate = 0, // Otomatik 0
                IsStockOrder = true, // Her zaman true
                Status = "Üretimde" // Stok girişi direkt üretime gönderilir
            };

            // CompanyId
            if (cmbCompany.SelectedItem != null)
            {
                var idProperty = cmbCompany.SelectedItem.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    order.CompanyId = (Guid)idProperty.GetValue(cmbCompany.SelectedItem);
                }
            }

            // LamelThickness - Değeri olduğu gibi kaydet (değişiklik yok)
            if (cmbLamelThickness.SelectedItem != null)
            {
                var thicknessStr = cmbLamelThickness.SelectedItem.ToString();
                thicknessStr = thicknessStr.Replace(",", ".");
                if (decimal.TryParse(thicknessStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal thickness))
                {
                    order.LamelThickness = thickness;
                }
            }

            return order;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Formu temizle
            cmbCompany.SelectedIndex = -1;
            dtpOrderDate.Value = DateTime.Now;
            dtpTermDate.Value = DateTime.Now;
            txtProductCode.Clear();
            txtBypassSize.Clear();
            txtBypassType.Clear();
            cmbLamelThickness.SelectedIndex = -1;
            cmbProductType.SelectedIndex = -1;
            nudQuantity.Value = 1;
            GenerateTrexOrderNo(); // Yeni numara oluştur
        }

        public void LoadOrderData(Guid orderId)
        {
            try
            {
                var orderRepository = new OrderRepository();
                var order = orderRepository.GetById(orderId);

                if (order == null)
                {
                    MessageBox.Show("Stok girişi bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _orderId = order.Id;
                _isEditMode = true;
                
                UpdateFormTitle();

                // Company seç
                LoadCompanies();
                foreach (var item in cmbCompany.Items)
                {
                    var idProperty = item.GetType().GetProperty("Id");
                    if (idProperty != null && idProperty.GetValue(item).Equals(order.CompanyId))
                    {
                        cmbCompany.SelectedItem = item;
                        break;
                    }
                }

                txtTrexOrderNo.Text = order.TrexOrderNo;
                dtpOrderDate.Value = order.OrderDate;
                dtpTermDate.Value = order.TermDate;
                txtProductCode.Text = order.ProductCode ?? "";
                txtBypassSize.Text = order.BypassSize ?? "";
                txtBypassType.Text = order.BypassType ?? "";

                // LamelThickness - Değeri olduğu gibi göster
                if (order.LamelThickness.HasValue)
                {
                    string thicknessStr = order.LamelThickness.Value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                    
                    for (int i = 0; i < cmbLamelThickness.Items.Count; i++)
                    {
                        var itemValue = cmbLamelThickness.Items[i].ToString();
                        if (itemValue.Replace(",", ".") == thicknessStr)
                        {
                            cmbLamelThickness.SelectedIndex = i;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(order.ProductType))
                {
                    for (int i = 0; i < cmbProductType.Items.Count; i++)
                    {
                        if (cmbProductType.Items[i].ToString() == order.ProductType)
                        {
                            cmbProductType.SelectedIndex = i;
                            break;
                        }
                    }
                }

                nudQuantity.Value = order.Quantity;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stok girişi yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


