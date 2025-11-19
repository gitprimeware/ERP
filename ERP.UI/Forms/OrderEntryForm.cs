using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class OrderEntryForm : UserControl
    {
        private Panel mainPanel;
        private TextBox txtOrderNo;
        private DateTimePicker dtpOrderDate;
        private ComboBox cmbCustomer;
        private DataGridView dgvOrderItems;
        private Button btnSave;
        private Button btnCancel;
        private Label lblTotal;
        private Label lblTitle;

        private int _orderId = 0;
        private bool _isEditMode = false;

        public int OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                _isEditMode = value > 0;
                UpdateFormTitle();
            }
        }

        public OrderEntryForm() : this(0)
        {
        }

        public OrderEntryForm(int orderId)
        {
            _orderId = orderId;
            _isEditMode = orderId > 0;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
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

            // Sipariş Bilgileri Grubu
            var orderGroup = CreateOrderInfoGroup();
            orderGroup.Location = new Point(30, 80);

            // Sipariş Kalemleri
            var itemsGroup = CreateOrderItemsGroup();
            itemsGroup.Location = new Point(30, 200);

            // Butonlar
            var buttonPanel = CreateButtonPanel();
            buttonPanel.Location = new Point(30, 0);
            buttonPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(orderGroup);
            mainPanel.Controls.Add(itemsGroup);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();
        }

        private Panel CreateOrderInfoGroup()
        {
            var panel = new Panel
            {
                Height = 100,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            mainPanel.Resize += (s, e) => {
                panel.Width = mainPanel.Width - 60;
            };

            // Sipariş No
            var lblOrderNo = new Label
            {
                Text = "Sipariş No:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            txtOrderNo = new TextBox
            {
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(100, 8),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Sipariş Tarihi
            var lblOrderDate = new Label
            {
                Text = "Sipariş Tarihi:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(320, 10)
            };

            dtpOrderDate = new DateTimePicker
            {
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(430, 8),
                Format = DateTimePickerFormat.Short
            };

            // Müşteri
            var lblCustomer = new Label
            {
                Text = "Müşteri:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 50)
            };

            cmbCustomer = new ComboBox
            {
                Width = 300,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(100, 48),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCustomer.Items.AddRange(new[] { "Müşteri 1", "Müşteri 2", "Müşteri 3" });

            panel.Controls.Add(lblOrderNo);
            panel.Controls.Add(txtOrderNo);
            panel.Controls.Add(lblOrderDate);
            panel.Controls.Add(dtpOrderDate);
            panel.Controls.Add(lblCustomer);
            panel.Controls.Add(cmbCustomer);

            return panel;
        }

        private Panel CreateOrderItemsGroup()
        {
            var panel = new Panel
            {
                Height = 300,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            mainPanel.Resize += (s, e) => {
                panel.Width = mainPanel.Width - 60;
            };

            var lblItems = new Label
            {
                Text = "Sipariş Kalemleri:",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            dgvOrderItems = new DataGridView
            {
                Location = new Point(0, 30),
                Width = panel.Width,
                Height = 220,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = ThemeColors.Surface,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = true,
                RowHeadersVisible = false
            };

            dgvOrderItems.Columns.Add("ProductCode", "Ürün Kodu");
            dgvOrderItems.Columns.Add("ProductName", "Ürün Adı");
            dgvOrderItems.Columns.Add("Quantity", "Miktar");
            dgvOrderItems.Columns.Add("UnitPrice", "Birim Fiyat");
            dgvOrderItems.Columns.Add("Total", "Toplam");

            foreach (DataGridViewColumn column in dgvOrderItems.Columns)
            {
                column.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            }

            // Toplam Label
            lblTotal = new Label
            {
                Text = "Toplam: 0,00 ₺",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(panel.Width - 200, 260),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            panel.Controls.Add(lblItems);
            panel.Controls.Add(dgvOrderItems);
            panel.Controls.Add(lblTotal);

            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            
            mainPanel.Resize += (s, e) => {
                panel.Width = mainPanel.Width - 60;
            };

            btnSave = new Button
            {
                Text = "Kaydet",
                Width = 120,
                Height = 40,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(panel.Width - 260, 5)
            };
            UIHelper.ApplyRoundedButton(btnSave, 6);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "İptal",
                Width = 120,
                Height = 40,
                Font = new Font("Segoe UI", 10F),
                BackColor = ThemeColors.SurfaceDark,
                ForeColor = ThemeColors.TextPrimary,
                Cursor = Cursors.Hand,
                Location = new Point(panel.Width - 130, 5)
            };
            UIHelper.ApplyRoundedButton(btnCancel, 6);
            btnCancel.Click += BtnCancel_Click;

            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnCancel);

            return panel;
        }

        private void UpdateFormTitle()
        {
            if (lblTitle != null)
            {
                lblTitle.Text = _isEditMode ? $"Sipariş Güncelle (#{_orderId})" : "Yeni Sipariş";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                if (_isEditMode)
                {
                    // Update işlemi
                    MessageBox.Show($"Sipariş #{_orderId} güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create işlemi
                    MessageBox.Show("Yeni sipariş kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtOrderNo.Text))
            {
                MessageBox.Show("Sipariş numarası boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrderNo.Focus();
                return false;
            }

            if (cmbCustomer.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCustomer.Focus();
                return false;
            }

            return true;
        }

        public void LoadOrderData(int orderId)
        {
            _orderId = orderId;
            _isEditMode = true;
            UpdateFormTitle();

            // Şimdilik örnek veri - sonra DAL'dan gelecek
            txtOrderNo.Text = $"ORD-{orderId:0000}";
            dtpOrderDate.Value = DateTime.Now;
            cmbCustomer.SelectedIndex = 0;

            // Örnek sipariş kalemleri
            dgvOrderItems.Rows.Clear();
            dgvOrderItems.Rows.Add("PRD-001", "Ürün 1", 10, 150.00, 1500.00);
            dgvOrderItems.Rows.Add("PRD-002", "Ürün 2", 5, 200.00, 1000.00);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Formu temizle
            txtOrderNo.Clear();
            dtpOrderDate.Value = DateTime.Now;
            cmbCustomer.SelectedIndex = -1;
            dgvOrderItems.Rows.Clear();
        }
    }
}

