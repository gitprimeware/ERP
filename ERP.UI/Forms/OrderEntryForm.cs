using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class OrderEntryForm : UserControl
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
        private TextBox txtSalesPriceUSD;
        private TextBox txtTotalPriceUSD;
        private TextBox txtCurrencyRate;
        private TextBox txtTotalPriceTL;
        private DateTimePicker dtpShipmentDate;
        private Button btnSave;
        private Button btnCancel;
        private Button btnDelete;
        private Button btnSendToProduction;
        private Button btnGetWorkOrder;
        private Label lblTitle;

        private Guid _orderId = Guid.Empty;
        private bool _isEditMode = false;
        private bool _isReadOnly = false; // Üretimde veya Muhasebede ise true

        public event EventHandler<Guid> OrderDeleteRequested;
        public event EventHandler<Guid> OrderSendToProductionRequested;
        public event EventHandler<Guid> OrderGetWorkOrderRequested;

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

        public OrderEntryForm() : this(Guid.Empty)
        {
        }

        public OrderEntryForm(Guid orderId)
        {
            _orderId = orderId;
            _isEditMode = orderId != Guid.Empty;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
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
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30),
                AutoScroll = true
            };

            UIHelper.ApplyCardStyle(mainPanel, 12);

            // Başlık
            lblTitle = new Label
            {
                Text = _isEditMode ? "Sipariş Güncelle" : "Yeni Sipariş",
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

            // Sipariş numaraları
            AddTableRow("Müşteri Sipariş No:", CreateTextBox(txtCustomerOrderNo = new TextBox()), 
                       "Trex Sipariş No:", CreateReadOnlyTextBox(txtTrexOrderNo = new TextBox()), row++);

            // Cihaz adı ve Sipariş tarihi
            AddTableRow("Cihaz Adı:", CreateTextBox(txtDeviceName = new TextBox()),
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

            // Satış fiyatı (USD) ve Toplam fiyat (USD)
            AddTableRow("Satış Fiyatı (USD):", CreateTextBox(txtSalesPriceUSD = new TextBox()),
                       "Toplam Fiyat (USD):", CreateReadOnlyTextBox(txtTotalPriceUSD = new TextBox()), row++);
            txtSalesPriceUSD.TextChanged += TxtSalesPriceUSD_TextChanged;

            // Kur ve Toplam fiyat (TL)
            AddTableRow("Kur:", CreateReadOnlyTextBox(txtCurrencyRate = new TextBox()),
                       "Toplam Fiyat (TL):", CreateReadOnlyTextBox(txtTotalPriceTL = new TextBox()), row++);
            txtCurrencyRate.Text = "0,00";
            txtTotalPriceTL.Text = "0,00";
            
            // Kur değiştiğinde TL fiyatını hesapla (sadece gösterim için)
            // Not: Kur readonly olduğu için bu event çalışmayacak ama yine de ekleyelim

            // Sevk tarihi
            AddTableRow("Sevk Tarihi:", CreateDisabledDateTimePicker(dtpShipmentDate = new DateTimePicker()),
                       "", new Panel(), row++);

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
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };

            var lbl2 = new Label
            {
                Text = label2Text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
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
                DropDownStyle = ComboBoxStyle.DropDownList
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
            var panel = new Panel();
            txtProductCode = new TextBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Dock = DockStyle.Left,
                Width = 300
            };

            btnProductCode = new Button
            {
                Text = "Seç",
                Width = 80,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                BackColor = ThemeColors.Info,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left
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
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLamelThickness.Items.AddRange(new[] { "0.10", "0.12", "0.15", "0.165" });
            return cmbLamelThickness;
        }

        private Control CreateProductTypeCombo()
        {
            cmbProductType = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
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
            nudQuantity.ValueChanged += NudQuantity_ValueChanged;
            return nudQuantity;
        }

        private Control CreateTextBox(TextBox textBox)
        {
            textBox.Height = 30;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.BorderStyle = BorderStyle.FixedSingle;
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
            return dateTimePicker;
        }

        private Control CreateDisabledDateTimePicker(DateTimePicker dateTimePicker)
        {
            dateTimePicker.Height = 30;
            dateTimePicker.Font = new Font("Segoe UI", 10F);
            dateTimePicker.Format = DateTimePickerFormat.Short;
            dateTimePicker.Enabled = false;
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

            // Panel genişliğini başlangıçta ayarla
            if (mainPanel != null)
            {
                panel.Width = mainPanel.Width - 60;
            }
            else
            {
                panel.Width = 800; // Varsayılan genişlik
            }

            // Panel genişliğini güncelle
            mainPanel.Resize += (s, e) =>
            {
                if (mainPanel != null && panel != null)
                {
                    panel.Width = mainPanel.Width - 60;
                    UpdateButtonPositions(panel);
                }
            };

            btnSave = ButtonFactory.CreateSuccessButton("Siparişi Tamamla");
            btnSave.Height = 35;
            btnSave.Width = 140;
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

            // Buton konumlarını güncelle
            UpdateButtonPositions(panel);

            return panel;
        }

        private void UpdateButtonPositions(Panel panel)
        {
            if (panel == null) return;

            // Panel genişliği yoksa veya çok küçükse, varsayılan genişlik kullan
            int panelWidth = panel.Width > 0 ? panel.Width : 800;
            
            int rightMargin = 10;
            int buttonSpacing = 10;
            int yPos = 5;

            // Butonları sağdan sola yerleştir
            int currentX = panelWidth - rightMargin;

            // İptal butonu (en sağda)
            if (btnCancel != null && btnCancel.Visible)
            {
                currentX -= btnCancel.Width;
                btnCancel.Location = new Point(currentX, yPos);
                currentX -= buttonSpacing;
            }

            // Siparişi Tamamla butonu
            if (btnSave != null && btnSave.Visible)
            {
                currentX -= btnSave.Width;
                btnSave.Location = new Point(currentX, yPos);
                currentX -= buttonSpacing;
            }

            // İş Emri Al butonu
            if (btnGetWorkOrder != null && btnGetWorkOrder.Visible)
            {
                currentX -= btnGetWorkOrder.Width;
                btnGetWorkOrder.Location = new Point(currentX, yPos);
                currentX -= buttonSpacing;
            }

            // Üretime Gönder butonu
            if (btnSendToProduction != null && btnSendToProduction.Visible)
            {
                currentX -= btnSendToProduction.Width;
                btnSendToProduction.Location = new Point(currentX, yPos);
                currentX -= buttonSpacing;
            }

            // Sil butonu
            if (btnDelete != null && btnDelete.Visible)
            {
                currentX -= btnDelete.Width;
                btnDelete.Location = new Point(currentX, yPos);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_orderId == Guid.Empty) return;

            var result = MessageBox.Show(
                $"Sipariş {txtTrexOrderNo.Text} silinecek. Emin misiniz?",
                "Sipariş Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                OrderDeleteRequested?.Invoke(this, _orderId);
            }
        }

        private void BtnSendToProduction_Click(object sender, EventArgs e)
        {
            if (_orderId == Guid.Empty) return;

            var result = MessageBox.Show(
                $"Sipariş {txtTrexOrderNo.Text} üretime gönderilecek. Emin misiniz?",
                "Üretime Gönder",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                OrderSendToProductionRequested?.Invoke(this, _orderId);
            }
        }

        private void BtnGetWorkOrder_Click(object sender, EventArgs e)
        {
            if (_orderId == Guid.Empty) return;
            OrderGetWorkOrderRequested?.Invoke(this, _orderId);
        }

        private void LoadCompanies()
        {
            try
            {
                cmbCompany.Items.Clear();
                var companyRepository = new ERP.DAL.Repositories.CompanyRepository();
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
            if (!_isEditMode)
            {
                int year = DateTime.Now.Year;
                int orderNumber = GetNextOrderNumber();
                txtTrexOrderNo.Text = $"SP-{year}-{orderNumber:0000}";
            }
        }

        private int GetNextOrderNumber()
        {
            try
            {
                var orderRepository = new OrderRepository();
                return orderRepository.GetNextOrderNumber(DateTime.Now.Year);
            }
            catch
            {
                return 1; // Hata durumunda 1 döndür
            }
        }

        private void SetDefaultDates()
        {
            dtpOrderDate.Value = DateTime.Now;
            dtpTermDate.Value = DateTime.Now.AddDays(7);
        }

        private void UpdateFormTitle()
        {
            if (lblTitle != null)
            {
                lblTitle.Text = _isEditMode ? $"Sipariş Güncelle" : "Yeni Sipariş";
            }
        }

        private void TxtSalesPriceUSD_TextChanged(object sender, EventArgs e)
        {
            CalculateTotalPriceUSD();
        }

        private void NudQuantity_ValueChanged(object sender, EventArgs e)
        {
            CalculateTotalPriceUSD();
        }

        private void CalculateTotalPriceUSD()
        {
            if (decimal.TryParse(txtSalesPriceUSD.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal salesPrice))
            {
                decimal quantity = nudQuantity.Value;
                decimal totalUSD = salesPrice * quantity;
                txtTotalPriceUSD.Text = totalUSD.ToString("N2") + " USD";
            }
            else
            {
                txtTotalPriceUSD.Text = "0,00 USD";
            }
            
            // TL fiyatını da hesapla
            CalculateTotalPriceTL();
        }

        private void CalculateTotalPriceTL()
        {
            try
            {
                // Kur'u al
                if (!decimal.TryParse(txtCurrencyRate.Text.Replace(",", "."), 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out decimal currencyRate) || currencyRate == 0)
                {
                    txtTotalPriceTL.Text = "0,00";
                    return;
                }

                // Satış Fiyatı USD'yi al
                if (!decimal.TryParse(txtSalesPriceUSD.Text.Replace(",", "."), 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out decimal salesPriceUSD))
                {
                    txtTotalPriceTL.Text = "0,00";
                    return;
                }

                // Adet'i al
                decimal quantity = nudQuantity.Value;

                // Toplam Fiyat TL = Satış Fiyatı USD * Kur * Adet
                decimal totalPriceTL = salesPriceUSD * currencyRate * quantity;
                txtTotalPriceTL.Text = totalPriceTL.ToString("N2");
            }
            catch
            {
                txtTotalPriceTL.Text = "0,00";
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
                        
                        LoadCompanies(); // Tüm firmaları yeniden yükle
                        
                        // Yeni eklenen firmayı seç
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

            if (string.IsNullOrWhiteSpace(txtCustomerOrderNo.Text))
            {
                MessageBox.Show("Müşteri sipariş numarası boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerOrderNo.Focus();
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Readonly modda kaydetme yapılamaz (Sevkiyata Hazır hariç)
            if (_isReadOnly)
            {
                var orderRepository = new OrderRepository();
                var currentOrder = orderRepository.GetById(_orderId);
                
                if (currentOrder != null && (currentOrder.Status == "Üretimde" || currentOrder.Status == "Muhasebede"))
                {
                    MessageBox.Show("Bu sipariş üretimde veya muhasebede olduğu için güncellenemez!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Sevkiyata Hazır durumunda sadece sevk tarihi güncellenebilir
                if (currentOrder != null && currentOrder.Status == "Sevkiyata Hazır")
                {
                    try
                    {
                        currentOrder.ShipmentDate = dtpShipmentDate.Value;
                        // Sevk tarihi seçildiğinde durumu "Sevk Edildi" olarak güncelle
                        currentOrder.Status = "Sevk Edildi";
                        orderRepository.Update(currentOrder);
                        MessageBox.Show("Sevk tarihi güncellendi ve sipariş 'Sevk Edildi' durumuna getirildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Sevk tarihi güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            if (!ValidateForm())
                return;

            try
            {
                var orderRepository = new OrderRepository();
                var order = MapFormToOrder();

                if (_isEditMode)
                {
                    orderRepository.Update(order);
                    UpdateFormTitle(); // Buton metnini güncelle
                    MessageBox.Show("Sipariş tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var orderId = orderRepository.Insert(order);
                    _orderId = orderId;
                    _isEditMode = true;
                    UpdateFormTitle();
                    // Buton konumlarını güncelle
                    if (btnSave != null)
                    {
                        var buttonPanel = btnSave.Parent as Panel;
                        if (buttonPanel != null)
                        {
                            UpdateButtonPositions(buttonPanel);
                        }
                    }
                    MessageBox.Show("Sipariş tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ERP.Core.Models.Order MapFormToOrder()
        {
            var order = new ERP.Core.Models.Order
            {
                Id = _orderId != Guid.Empty ? _orderId : Guid.NewGuid(),
                CustomerOrderNo = txtCustomerOrderNo.Text.Trim(),
                TrexOrderNo = txtTrexOrderNo.Text.Trim(),
                DeviceName = string.IsNullOrWhiteSpace(txtDeviceName.Text) ? null : txtDeviceName.Text.Trim(),
                OrderDate = dtpOrderDate.Value,
                TermDate = dtpTermDate.Value,
                ProductCode = string.IsNullOrWhiteSpace(txtProductCode.Text) ? null : txtProductCode.Text.Trim(),
                BypassSize = string.IsNullOrWhiteSpace(txtBypassSize.Text) ? null : txtBypassSize.Text.Trim(),
                BypassType = string.IsNullOrWhiteSpace(txtBypassType.Text) ? null : txtBypassType.Text.Trim(),
                ProductType = cmbProductType.SelectedItem?.ToString(),
                Quantity = (int)nudQuantity.Value
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

            // LamelThickness
            if (cmbLamelThickness.SelectedItem != null && decimal.TryParse(cmbLamelThickness.SelectedItem.ToString(), out decimal thickness))
            {
                order.LamelThickness = thickness;
            }

            // SalesPrice
            if (decimal.TryParse(txtSalesPriceUSD.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal salesPrice))
            {
                order.SalesPrice = salesPrice;
            }

            // TotalPrice
            if (decimal.TryParse(txtTotalPriceUSD.Text.Replace(" USD", "").Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal totalPrice))
            {
                order.TotalPrice = totalPrice;
            }

            // CurrencyRate
            if (decimal.TryParse(txtCurrencyRate.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
            {
                order.CurrencyRate = rate;
            }

            // ShipmentDate
            if (dtpShipmentDate.Enabled && dtpShipmentDate.Value != null)
            {
                order.ShipmentDate = dtpShipmentDate.Value;
            }

            // Status
            if (!_isEditMode)
            {
                // Yeni sipariş için "Yeni" status
                order.Status = "Yeni";
            }
            else
            {
                // Update modunda mevcut status'u veritabanından al ve koru
                // Eğer sevk tarihi girildiyse ve status "Sevkiyata Hazır" ise "Sevk Edildi" yap
                var orderRepository = new OrderRepository();
                var currentOrder = orderRepository.GetById(_orderId);
                if (currentOrder != null)
                {
                    // Sevkiyata Hazır durumunda sevk tarihi girildiyse "Sevk Edildi" yap
                    if (currentOrder.Status == "Sevkiyata Hazır" && dtpShipmentDate.Enabled && dtpShipmentDate.Value != null)
                    {
                        order.Status = "Sevk Edildi";
                    }
                    else
                    {
                        // Diğer durumlarda mevcut status'u koru
                        order.Status = currentOrder.Status;
                    }
                }
            }

            return order;
        }


        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Formu temizle
            cmbCompany.SelectedIndex = -1;
            txtCustomerOrderNo.Clear();
            txtDeviceName.Clear();
            dtpOrderDate.Value = DateTime.Now;
            dtpTermDate.Value = DateTime.Now.AddDays(7);
            txtProductCode.Clear();
            txtBypassSize.Clear();
            txtBypassType.Clear();
            cmbLamelThickness.SelectedIndex = -1;
            cmbProductType.SelectedIndex = -1;
            nudQuantity.Value = 1;
            txtSalesPriceUSD.Clear();
            txtTotalPriceUSD.Text = "0,00 USD";
            txtCurrencyRate.Text = "0,00";
            txtTotalPriceTL.Text = "0,00";
        }

        public void LoadOrderData(Guid orderId)
        {
            try
            {
                var orderRepository = new OrderRepository();
                var order = orderRepository.GetById(orderId);

                if (order == null)
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _orderId = order.Id;
                _isEditMode = true;
                
                // Üretimde veya Muhasebede ise readonly yap
                // Sevkiyata Hazır ise sadece sevk tarihi editable
                _isReadOnly = order.Status == "Üretimde" || order.Status == "Muhasebede";
                bool isReadyForShipment = order.Status == "Sevkiyata Hazır";
                
                UpdateFormTitle();
                SetFormReadOnly(_isReadOnly, isReadyForShipment);
                
                // Buton görünürlüklerini güncelle
                if (btnDelete != null) btnDelete.Visible = _isEditMode && !_isReadOnly;
                if (btnSendToProduction != null) btnSendToProduction.Visible = _isEditMode && !_isReadOnly;
                if (btnGetWorkOrder != null) btnGetWorkOrder.Visible = _isEditMode;
                
                // Buton pozisyonlarını güncelle
                var buttonPanel = btnSave?.Parent as Panel;
                if (buttonPanel != null)
                {
                    UpdateButtonPositions(buttonPanel);
                }

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

                txtCustomerOrderNo.Text = order.CustomerOrderNo;
                txtTrexOrderNo.Text = order.TrexOrderNo;
                txtDeviceName.Text = order.DeviceName ?? "";
                dtpOrderDate.Value = order.OrderDate;
                dtpTermDate.Value = order.TermDate;
                txtProductCode.Text = order.ProductCode ?? "";
                txtBypassSize.Text = order.BypassSize ?? "";
                txtBypassType.Text = order.BypassType ?? "";

                if (order.LamelThickness.HasValue)
                {
                    // Decimal değeri string'e çevir (InvariantCulture kullan)
                    var thicknessStr = order.LamelThickness.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    
                    // ComboBox'taki değerlerle karşılaştır
                    for (int i = 0; i < cmbLamelThickness.Items.Count; i++)
                    {
                        var itemValue = cmbLamelThickness.Items[i].ToString();
                        // Hem string hem de decimal karşılaştırması yap
                        if (itemValue == thicknessStr || 
                            (decimal.TryParse(itemValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal itemDecimal) && 
                             itemDecimal == order.LamelThickness.Value))
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
                txtSalesPriceUSD.Text = order.SalesPrice?.ToString("N2") ?? "0,00";
                CalculateTotalPriceUSD();
                txtCurrencyRate.Text = order.CurrencyRate?.ToString("N4") ?? "0,00";
                
                // Toplam fiyat TL'yi hesapla ve göster
                CalculateTotalPriceTL();

                if (order.ShipmentDate.HasValue)
                {
                    dtpShipmentDate.Value = order.ShipmentDate.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetFormReadOnly(bool readOnly, bool isReadyForShipment = false)
        {
            // Sevkiyata Hazır durumunda sadece sevk tarihi editable
            if (isReadyForShipment)
            {
                // Tüm alanları readonly yap
                if (cmbCompany != null) cmbCompany.Enabled = false;
                if (btnAddCompany != null) btnAddCompany.Enabled = false;
                if (txtCustomerOrderNo != null) txtCustomerOrderNo.ReadOnly = true;
                if (txtDeviceName != null) txtDeviceName.ReadOnly = true;
                if (dtpOrderDate != null) dtpOrderDate.Enabled = false;
                if (dtpTermDate != null) dtpTermDate.Enabled = false;
                if (btnProductCode != null) btnProductCode.Enabled = false;
                if (txtProductCode != null) txtProductCode.ReadOnly = true;
                if (txtBypassSize != null) txtBypassSize.ReadOnly = true;
                if (txtBypassType != null) txtBypassType.ReadOnly = true;
                if (cmbLamelThickness != null) cmbLamelThickness.Enabled = false;
                if (cmbProductType != null) cmbProductType.Enabled = false;
                if (nudQuantity != null) nudQuantity.Enabled = false;
                if (txtSalesPriceUSD != null) txtSalesPriceUSD.ReadOnly = true;
                
                // Sadece sevk tarihi editable
                if (dtpShipmentDate != null) dtpShipmentDate.Enabled = true;
                
                // Kaydet butonu görünür (sevk tarihi güncellemek için)
                if (btnSave != null) btnSave.Visible = true;
                
                // Başlık güncelle
                if (lblTitle != null)
                {
                    lblTitle.Text = "Sipariş Detayları - Sevkiyata Hazır (Sadece Sevk Tarihi Düzenlenebilir)";
                    lblTitle.ForeColor = ThemeColors.Info;
                }
                
                return;
            }
            
            // Normal readonly modu (Üretimde veya Muhasebede)
            if (cmbCompany != null) cmbCompany.Enabled = !readOnly;
            if (btnAddCompany != null) btnAddCompany.Enabled = !readOnly;
            if (txtCustomerOrderNo != null) txtCustomerOrderNo.ReadOnly = readOnly;
            if (txtDeviceName != null) txtDeviceName.ReadOnly = readOnly;
            if (dtpOrderDate != null) dtpOrderDate.Enabled = !readOnly;
            if (dtpTermDate != null) dtpTermDate.Enabled = !readOnly;
            if (btnProductCode != null) btnProductCode.Enabled = !readOnly;
            if (txtProductCode != null) txtProductCode.ReadOnly = readOnly;
            if (txtBypassSize != null) txtBypassSize.ReadOnly = readOnly;
            if (txtBypassType != null) txtBypassType.ReadOnly = readOnly;
            if (cmbLamelThickness != null) cmbLamelThickness.Enabled = !readOnly;
            if (cmbProductType != null) cmbProductType.Enabled = !readOnly;
            if (nudQuantity != null) nudQuantity.Enabled = !readOnly;
            if (txtSalesPriceUSD != null) txtSalesPriceUSD.ReadOnly = readOnly;
            if (dtpShipmentDate != null) dtpShipmentDate.Enabled = !readOnly;
            
            // Kaydet butonunu gizle
            if (btnSave != null) btnSave.Visible = !readOnly;
            
            // Başlık güncelle
            if (lblTitle != null && readOnly)
            {
                lblTitle.Text = "Sipariş Detayları (Sadece Görüntüleme)";
                lblTitle.ForeColor = ThemeColors.Warning;
            }
        }
    }
}
