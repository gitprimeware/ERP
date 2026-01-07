using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class AccountingEntryForm : UserControl
    {
        private Panel mainPanel;
        private TableLayoutPanel tableLayout;
        private Label lblTitle;
        
        // Readonly alanlar
        private TextBox txtCompany;
        private TextBox txtCustomerOrderNo;
        private TextBox txtTrexOrderNo;
        private TextBox txtDeviceName;
        private TextBox txtProductCode;
        private TextBox txtProductType;
        private TextBox txtQuantity;
        private TextBox txtSalesPriceUSD;
        
        // Editable alanlar
        private TextBox txtCurrencyRate;
        private TextBox txtSalesPriceTL;
        private TextBox txtTotalPriceTL;
        
        private Button btnSave;
        private Button btnCancel;
        private Button btnSendToShipment;

        private Guid _orderId = Guid.Empty;
        private OrderRepository _orderRepository;
        private Order _order;

        public event EventHandler AccountingEntrySaved;
        public event EventHandler<Guid> OrderSendToShipmentRequested;

        public AccountingEntryForm(Guid orderId)
        {
            _orderId = orderId;
            _orderRepository = new OrderRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadOrderData();
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

            // Başlık (LoadOrderData'da güncellenecek)
            lblTitle = new Label
            {
                Text = "Muhasebe İşlemi",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // TableLayoutPanel oluştur
            CreateTableLayout();

            // Butonlar
            var buttonPanel = CreateButtonPanel();

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(tableLayout);
            mainPanel.Controls.Add(buttonPanel);

            // TableLayout boyutu değiştiğinde buton panelini güncelle
            tableLayout.SizeChanged += (s, e) =>
            {
                if (buttonPanel != null)
                {
                    buttonPanel.Location = new Point(30, tableLayout.Bottom + 30);
                    buttonPanel.BringToFront();
                }
            };

            // İlk konumlandırma
            if (tableLayout.Height > 0)
            {
                buttonPanel.Location = new Point(30, tableLayout.Bottom + 30);
            }
            else
            {
                // TableLayout henüz boyutlanmamışsa, bir süre sonra güncelle
                tableLayout.Layout += (s, e) =>
                {
                    if (buttonPanel != null && tableLayout.Height > 0)
                    {
                        buttonPanel.Location = new Point(30, tableLayout.Bottom + 30);
                        buttonPanel.BringToFront();
                    }
                };
            }

            buttonPanel.BringToFront();

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

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            int row = 0;

            // Firma (Readonly)
            AddTableRow("Firma:", CreateReadOnlyTextBox(txtCompany = new TextBox()), 
                       "Müşteri Sipariş No:", CreateReadOnlyTextBox(txtCustomerOrderNo = new TextBox()), row++);

            // Trex Sipariş No (Readonly)
            AddTableRow("Trex Sipariş No:", CreateReadOnlyTextBox(txtTrexOrderNo = new TextBox()),
                       "Cihaz Adı:", CreateReadOnlyTextBox(txtDeviceName = new TextBox()), row++);

            // Ürün Kodu (Readonly)
            AddTableRow("Ürün Kodu:", CreateReadOnlyTextBox(txtProductCode = new TextBox()),
                       "Ürün Türü:", CreateReadOnlyTextBox(txtProductType = new TextBox()), row++);

            // Adet (Readonly)
            AddTableRow("Adet:", CreateReadOnlyTextBox(txtQuantity = new TextBox()),
                       "Satış Fiyatı (USD):", CreateReadOnlyTextBox(txtSalesPriceUSD = new TextBox()), row++);

            // Kur (Editable)
            AddTableRow("Kur:", CreateCurrencyRateTextBox(),
                       "Satış Fiyatı (TL):", CreateReadOnlyTextBox(txtSalesPriceTL = new TextBox()), row++);

            // Toplam Fiyat (TL) (Readonly)
            AddTableRow("Toplam Fiyat (TL):", CreateReadOnlyTextBox(txtTotalPriceTL = new TextBox()),
                       "", new Panel(), row++);
        }

        private Control CreateCurrencyRateTextBox()
        {
            txtCurrencyRate = new TextBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtCurrencyRate.TextChanged += TxtCurrencyRate_TextChanged;
            return txtCurrencyRate;
        }

        private void TxtCurrencyRate_TextChanged(object sender, EventArgs e)
        {
            CalculatePrices();
        }

        private void CalculatePrices()
        {
            try
            {
                // Kur'u al
                if (!decimal.TryParse(txtCurrencyRate.Text.Replace(",", "."), 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out decimal currencyRate))
                {
                    txtSalesPriceTL.Text = "0,00";
                    txtTotalPriceTL.Text = "0,00";
                    return;
                }

                // Satış Fiyatı USD'yi al
                if (!decimal.TryParse(txtSalesPriceUSD.Text.Replace(",", "."), 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out decimal salesPriceUSD))
                {
                    txtSalesPriceTL.Text = "0,00";
                    txtTotalPriceTL.Text = "0,00";
                    return;
                }

                // Adet'i al
                if (!int.TryParse(txtQuantity.Text, out int quantity))
                {
                    txtSalesPriceTL.Text = "0,00";
                    txtTotalPriceTL.Text = "0,00";
                    return;
                }

                // Satış Fiyatı TL = Satış Fiyatı USD * Kur
                decimal salesPriceTL = salesPriceUSD * currencyRate;
                txtSalesPriceTL.Text = salesPriceTL.ToString("N2");

                // Toplam Fiyat TL = Satış Fiyatı TL * Adet
                decimal totalPriceTL = salesPriceTL * quantity;
                txtTotalPriceTL.Text = totalPriceTL.ToString("N2");
            }
            catch
            {
                txtSalesPriceTL.Text = "0,00";
                txtTotalPriceTL.Text = "0,00";
            }
        }

        private void AddTableRow(string labelText1, Control control1, string labelText2, Control control2, int row)
        {
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            if (!string.IsNullOrEmpty(labelText1))
            {
                var label1 = new Label
                {
                    Text = labelText1,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ThemeColors.TextPrimary,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 10, 0)
                };

                control1.Dock = DockStyle.Fill;
                control1.Margin = new Padding(5, 5, 5, 5);

                tableLayout.Controls.Add(label1, 0, row);
                tableLayout.Controls.Add(control1, 1, row);
            }

            if (!string.IsNullOrEmpty(labelText2) && control2 is not Panel)
            {
                var label2 = new Label
                {
                    Text = labelText2,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ThemeColors.TextPrimary,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 10, 0)
                };

                control2.Dock = DockStyle.Fill;
                control2.Margin = new Padding(5, 5, 5, 5);

                tableLayout.Controls.Add(label2, 2, row);
                tableLayout.Controls.Add(control2, 3, row);
            }
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

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Height = 60,
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

            btnSave = ButtonFactory.CreateSuccessButton("💾 Kaydet");
            btnSave.Height = 40;
            btnSave.Width = 140;
            btnSave.Anchor = AnchorStyles.None;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            btnSave.Visible = true;
            btnSave.BringToFront();

            btnCancel = ButtonFactory.CreateCancelButton("❌ İptal");
            btnCancel.Height = 40;
            btnCancel.Width = 140;
            btnCancel.Anchor = AnchorStyles.None;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancel.Click += BtnCancel_Click;
            btnCancel.Visible = true;
            btnCancel.BringToFront();

            // Siparişe Dön (sadece muhasebede ise)
            btnSendToShipment = ButtonFactory.CreateActionButton("📦 Siparişe Dön", ThemeColors.Success, Color.White, 180, 40);
            btnSendToShipment.Anchor = AnchorStyles.None;
            btnSendToShipment.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSendToShipment.Click += BtnSendToShipment_Click;
            btnSendToShipment.Visible = false; // LoadOrderData'da güncellenecek

            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnCancel);
            panel.Controls.Add(btnSendToShipment);

            UpdateButtonPositions(panel);

            // Panel'i en üste getir
            panel.BringToFront();

            return panel;
        }

        private void UpdateButtonPositions(Panel panel)
        {
            if (panel == null) return;

            int panelWidth = panel.Width > 0 ? panel.Width : 800;
            int rightMargin = 30;
            int buttonSpacing = 15;
            int yPos = 10;

            // Butonları sağdan sola yerleştir
            int currentX = panelWidth - rightMargin;

            // İptal butonu (en sağda)
            if (btnCancel != null && btnCancel.Visible)
            {
                currentX -= btnCancel.Width;
                btnCancel.Location = new Point(currentX, yPos);
                btnCancel.BringToFront();
                currentX -= buttonSpacing;
            }

            // Kaydet butonu
            if (btnSave != null && btnSave.Visible)
            {
                currentX -= btnSave.Width;
                btnSave.Location = new Point(currentX, yPos);
                btnSave.BringToFront();
                currentX -= buttonSpacing;
            }

            // Siparişe Geri Gönder butonu
            if (btnSendToShipment != null && btnSendToShipment.Visible)
            {
                currentX -= btnSendToShipment.Width;
                btnSendToShipment.Location = new Point(currentX, yPos);
                btnSendToShipment.BringToFront();
            }

            // Panel'i en üste getir
            panel.BringToFront();
        }

        private void LoadOrderData()
        {
            try
            {
                _order = _orderRepository.GetById(_orderId);
                if (_order == null)
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                txtCompany.Text = _order.Company?.Name ?? "";
                txtCustomerOrderNo.Text = _order.CustomerOrderNo;
                txtTrexOrderNo.Text = _order.TrexOrderNo;
                txtDeviceName.Text = _order.DeviceName ?? "";
                txtProductCode.Text = _order.ProductCode ?? "";
                txtProductType.Text = _order.ProductType ?? "";
                txtQuantity.Text = _order.Quantity.ToString();
                txtSalesPriceUSD.Text = _order.SalesPrice?.ToString("N2") ?? "0,00";

                // Kur varsa göster
                if (_order.CurrencyRate.HasValue)
                {
                    txtCurrencyRate.Text = _order.CurrencyRate.Value.ToString("N4");
                }

                // Fiyatları hesapla
                CalculatePrices();

                // Sipariş durumuna göre kur alanını ve butonları güncelle
                bool isInAccounting = _order.Status == "Muhasebede";
                
                // Başlık güncelle
                if (lblTitle != null)
                {
                    if (isInAccounting)
                    {
                        lblTitle.Text = "Muhasebe İşlemi";
                        lblTitle.ForeColor = ThemeColors.Primary;
                    }
                    else
                    {
                        lblTitle.Text = "Muhasebe İşlemi (Sadece Görüntüleme - Sipariş Muhasebede Değil)";
                        lblTitle.ForeColor = ThemeColors.Warning;
                    }
                }
                
                // Kur alanını sadece muhasebede ise düzenlenebilir yap
                if (txtCurrencyRate != null)
                {
                    txtCurrencyRate.ReadOnly = !isInAccounting;
                    txtCurrencyRate.BackColor = isInAccounting ? Color.White : ThemeColors.SurfaceDark;
                    txtCurrencyRate.Enabled = isInAccounting;
                }

                // Kaydet butonunu sadece muhasebede ise göster
                if (btnSave != null)
                {
                    btnSave.Visible = isInAccounting;
                    btnSave.Enabled = isInAccounting;
                }

                // Siparişe Dön butonunu güncelle (sadece muhasebede ise)
                if (btnSendToShipment != null)
                {
                    btnSendToShipment.Visible = isInAccounting;
                    
                    // Buton pozisyonlarını güncelle
                    var buttonPanel = btnSave?.Parent as Panel;
                    if (buttonPanel != null)
                    {
                        UpdateButtonPositions(buttonPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Sipariş muhasebede değilse kaydetme
            if (_order == null || _order.Status != "Muhasebede")
            {
                MessageBox.Show("Bu sipariş muhasebede değil, kur güncellenemez!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCurrencyRate.Text))
            {
                MessageBox.Show("Lütfen kur değerini girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCurrencyRate.Focus();
                return;
            }

            if (!decimal.TryParse(txtCurrencyRate.Text.Replace(",", "."), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal currencyRate))
            {
                MessageBox.Show("Geçerli bir kur değeri girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCurrencyRate.Focus();
                return;
            }

            try
            {
                var order = _orderRepository.GetById(_orderId);
                if (order != null)
                {
                    order.CurrencyRate = currencyRate;
                    
                    // Toplam fiyat TL'yi kaydet
                    if (decimal.TryParse(txtTotalPriceTL.Text.Replace(",", "."), 
                        System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        out decimal totalPriceTL))
                    {
                        order.TotalPrice = totalPriceTL;
                    }
                    
                    _orderRepository.Update(order);
                    MessageBox.Show("Muhasebe işlemi kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Event'i tetikle - muhasebe listesini yenilemek için
                    AccountingEntrySaved?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSendToShipment_Click(object sender, EventArgs e)
        {
            if (_order == null) return;

            var result = MessageBox.Show(
                $"Sipariş {_order.TrexOrderNo} siparişe döndürülecek (Sevkiyata Hazır). Emin misiniz?",
                "Siparişe Dön",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                OrderSendToShipmentRequested?.Invoke(this, _orderId);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Formu kapat - ContentManager'a geri dön
        }
    }
}

