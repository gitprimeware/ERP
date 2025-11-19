using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class ProductCodeSelectionDialog : Form
    {
        private ComboBox cmbType; // CR veya CN
        private ComboBox cmbPlateRange; // Plaka aralığı
        private ComboBox cmbProfile; // Profil (S veya G)
        private ComboBox cmbModelSize; // Model tipi ölçüsü
        private NumericUpDown nudExchangerLength; // Eşanjör dış uzunluğu
        private ComboBox cmbCoverSize; // Saç kapak toplam ölçüsü
        private TextBox txtProductCode; // Oluşturulan ürün kodu
        private Button btnGenerate;
        private Button btnOK;
        private Button btnCancel;

        public string SelectedProductCode { get; private set; }

        public ProductCodeSelectionDialog()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Ürün Kodu Seçimi";
            this.Width = 600;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            // Kontroller oluşturulduktan sonra ürün kodunu oluştur
            if (cmbType != null && cmbPlateRange != null && cmbProfile != null && 
                cmbModelSize != null && cmbCoverSize != null && nudExchangerLength != null)
            {
                GenerateProductCode();
            }
        }

        private void CreateControls()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = ThemeColors.Surface
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                AutoSize = true
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int row = 0;

            // Type (CR/CN)
            AddTableRow(tableLayout, "Tip (CR/CN):", CreateTypeCombo(), row++);

            // Plaka Aralığı
            AddTableRow(tableLayout, "Plaka Aralığı:", CreatePlateRangeCombo(), row++);

            // Profil
            AddTableRow(tableLayout, "Profil:", CreateProfileCombo(), row++);

            // Model Tipi Ölçüsü
            AddTableRow(tableLayout, "Model Tipi Ölçüsü:", CreateModelSizeCombo(), row++);

            // Eşanjör Dış Uzunluğu
            AddTableRow(tableLayout, "Eşanjör Dış Uzunluğu:", CreateExchangerLengthControl(), row++);

            // Saç Kapak Toplam Ölçüsü
            AddTableRow(tableLayout, "Saç Kapak Toplam Ölçüsü:", CreateCoverSizeCombo(), row++);

            // Oluşturulan Ürün Kodu
            AddTableRow(tableLayout, "Ürün Kodu:", CreateProductCodeTextBox(), row++);

            // Butonlar
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            btnGenerate = ButtonFactory.CreateActionButton("Kodu Oluştur", ThemeColors.Info, Color.White, 120, 35);
            btnGenerate.Location = new Point(10, 10);
            btnGenerate.Click += BtnGenerate_Click;

            btnOK = ButtonFactory.CreateSuccessButton("Tamam");
            btnOK.Location = new Point(buttonPanel.Width - 180, 10);
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Click += BtnOK_Click;

            btnCancel = ButtonFactory.CreateCancelButton("İptal");
            btnCancel.Location = new Point(buttonPanel.Width - 90, 10);
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Click += BtnCancel_Click;

            buttonPanel.Controls.Add(btnGenerate);
            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnCancel);

            mainPanel.Controls.Add(tableLayout);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void AddTableRow(TableLayoutPanel table, string labelText, Control control, int row)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(5, 5, 5, 5);

            table.Controls.Add(label, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private Control CreateTypeCombo()
        {
            cmbType = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new[] { "CR", "CN" });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += (s, e) => GenerateProductCode();
            return cmbType;
        }

        private Control CreatePlateRangeCombo()
        {
            cmbPlateRange = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPlateRange.Items.AddRange(new[] { "3.1(H)", "4.3(D)", "6.3(M)", "8.7(L)" });
            cmbPlateRange.SelectedIndex = 0;
            cmbPlateRange.SelectedIndexChanged += (s, e) => GenerateProductCode();
            return cmbPlateRange;
        }

        private Control CreateProfileCombo()
        {
            cmbProfile = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProfile.Items.AddRange(new[] { "Standart(S)", "Geniş(G)" });
            cmbProfile.SelectedIndex = 0;
            cmbProfile.SelectedIndexChanged += (s, e) => UpdateModelSizeOptions();
            return cmbProfile;
        }

        private Control CreateModelSizeCombo()
        {
            cmbModelSize = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UpdateModelSizeOptions();
            cmbModelSize.SelectedIndexChanged += (s, e) => GenerateProductCode();
            return cmbModelSize;
        }

        private void UpdateModelSizeOptions()
        {
            if (cmbProfile == null || cmbModelSize == null)
                return;

            cmbModelSize.Items.Clear();
            
            bool isStandard = cmbProfile.SelectedIndex == 0; // Standart(S)
            
            if (isStandard)
            {
                // S Profil değerleri
                cmbModelSize.Items.AddRange(new[] { "200", "300", "400", "500", "600", "700", "800", "1000", "1200", "1400", "1600", "2000" });
            }
            else
            {
                // G Profil değerleri
                cmbModelSize.Items.AddRange(new[] { "200", "300", "411", "511", "611", "711", "811", "1011", "1222", "1422", "1622", "2022" });
            }
            
            if (cmbModelSize.Items.Count > 0)
                cmbModelSize.SelectedIndex = 0;
            
            // Ürün kodunu güncelle, ancak sadece tüm kontroller hazırsa
            if (cmbType != null && cmbPlateRange != null && cmbProfile != null && 
                cmbModelSize != null && cmbCoverSize != null && nudExchangerLength != null)
            {
                GenerateProductCode();
            }
        }

        private Control CreateExchangerLengthControl()
        {
            nudExchangerLength = new NumericUpDown
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                Minimum = 1,
                Maximum = 9999,
                Value = 630
            };
            nudExchangerLength.ValueChanged += (s, e) => GenerateProductCode();
            return nudExchangerLength;
        }

        private Control CreateCoverSizeCombo()
        {
            cmbCoverSize = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCoverSize.Items.AddRange(new[] { "030", "002" });
            cmbCoverSize.SelectedIndex = 0;
            cmbCoverSize.SelectedIndexChanged += (s, e) => GenerateProductCode();
            return cmbCoverSize;
        }

        private Control CreateProductCodeTextBox()
        {
            txtProductCode = new TextBox
            {
                Height = 35,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                ForeColor = ThemeColors.Primary,
                TextAlign = HorizontalAlignment.Center
            };
            return txtProductCode;
        }

        private void GenerateProductCode()
        {
            // Kontrollerin null olup olmadığını ve seçili item olup olmadığını kontrol et
            if (cmbType == null || cmbPlateRange == null || cmbProfile == null || 
                cmbModelSize == null || cmbCoverSize == null || nudExchangerLength == null)
                return;

            if (cmbType.SelectedIndex < 0 || 
                cmbPlateRange.SelectedIndex < 0 || 
                cmbProfile.SelectedIndex < 0 || 
                cmbModelSize.SelectedIndex < 0 || 
                cmbCoverSize.SelectedIndex < 0)
                return;

            // Plaka aralığından sadece harf kısmını al (H, D, M, L - büyük harf)
            string plateRange = cmbPlateRange.SelectedItem.ToString();
            string plateLetter = plateRange.Contains("(H)") ? "H" :
                               plateRange.Contains("(D)") ? "D" :
                               plateRange.Contains("(M)") ? "M" : "L";

            // Profil'den sadece harf kısmını al (S veya G)
            string profile = cmbProfile.SelectedItem.ToString();
            string profileLetter = profile.Contains("(S)") ? "S" : "G";

            // Ürün kodu oluştur: TREX-CR-HS-1422-1900-030 formatında
            // Format: TREX-CR/CN-plaka_harfi+profil_harfi-model_ölçüsü-eşanjör_uzunluğu-kapak_ölçüsü
            string productCode = $"TREX-{cmbType.SelectedItem}-{plateLetter}{profileLetter}-{cmbModelSize.SelectedItem}-{nudExchangerLength.Value}-{cmbCoverSize.SelectedItem}";
            
            txtProductCode.Text = productCode;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            GenerateProductCode();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurup ürün kodu oluşturun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedProductCode = txtProductCode.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

