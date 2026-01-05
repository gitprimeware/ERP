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
    public partial class IsolationDialog : Form
    {
        private RadioButton _rdoMsSilikon;
        private RadioButton _rdoIzosiyanatPoliol;
        private Label _lblMsSilikonInfo;
        private Label _lblIzosiyanatInfo;
        private Label _lblPoliolInfo;
        private NumericUpDown _nudIzosiyanatRatio;
        private NumericUpDown _nudPoliolRatio;
        private Button _btnSave;
        private Button _btnCancel;

        private Assembly _assembly;
        private IsolationStockRepository _isolationStockRepository;

        public string SelectedMethod { get; private set; }
        public decimal IsolationLiquidAmount { get; private set; }
        public int IsolationCount { get; private set; }
        public int IzosiyanatRatio { get; private set; }
        public int PoliolRatio { get; private set; }

        public IsolationDialog(Assembly assembly, IsolationStockRepository isolationStockRepository)
        {
            _assembly = assembly;
            _isolationStockRepository = isolationStockRepository;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "İzolasyon Yöntemi Seç";
            this.Width = 580;
            this.Height = 520;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;

            CreateControls();
            LoadData();
        }

        private void CreateControls()
        {
            int yPos = 30;
            int labelWidth = 480;
            int spacing = 35;

            // Başlık
            var lblTitle = new Label
            {
                Text = "İzolasyon Yöntemi Seçin",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            yPos += 50;

            // Montaj bilgileri
            decimal lengthInMeters = _assembly.Length / 1000m;
            IsolationCount = _assembly.AssemblyCount;
            decimal totalLengthM = lengthInMeters * IsolationCount;
            decimal totalLiquidAmountKg = totalLengthM * 2m; // 1 metre = 2 kg

            var lblAssemblyInfo = new Label
            {
                Text = $"Montaj Bilgileri:\nUzunluk: {lengthInMeters:F2} m\nMontaj Adedi: {IsolationCount} adet\nToplam Uzunluk: {totalLengthM:F2} m\nToplam İzolasyon Sıvısı: {totalLiquidAmountKg:F2} kg",
                Location = new Point(20, yPos),
                Width = 500,
                Height = 100,
                Font = new Font("Segoe UI", 10F),
                AutoSize = false
            };
            this.Controls.Add(lblAssemblyInfo);
            yPos += 110;

            // MS Silikon seçeneği
            _rdoMsSilikon = new RadioButton
            {
                Text = "MS Silikon",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Checked = false
            };
            _rdoMsSilikon.CheckedChanged += RdoMethod_CheckedChanged;
            this.Controls.Add(_rdoMsSilikon);
            yPos += 30;

            // MS Silikon bilgisi
            // 1 metre = 2 kg izolasyon sıvısı = 0.95 kg MS Silikon tüketimi
            decimal msSilikonNeededKg = totalLengthM * 2m * 0.95m / 2m; // Toplam kg'dan MS Silikon kg'ına çevir
            _lblMsSilikonInfo = new Label
            {
                Text = $"Gereken MS Silikon: {msSilikonNeededKg:F3} kg - (1 metre = 0.95 kg MS Silikon)",
                Location = new Point(50, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Visible = false
            };
            this.Controls.Add(_lblMsSilikonInfo);
            yPos += 40;

            // İzosiyanat+Poliol seçeneği
            _rdoIzosiyanatPoliol = new RadioButton
            {
                Text = "İzosiyanat + Poliol",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Checked = true // Varsayılan seçim
            };
            _rdoIzosiyanatPoliol.CheckedChanged += RdoMethod_CheckedChanged;
            this.Controls.Add(_rdoIzosiyanatPoliol);
            yPos += 30;

            // Oran girişi
            var lblRatio = new Label
            {
                Text = "Oran (İzosiyanat:Poliol):",
                Location = new Point(50, yPos),
                Width = 180,
                Font = new Font("Segoe UI", 9F),
                AutoSize = false
            };
            this.Controls.Add(lblRatio);

            _nudIzosiyanatRatio = new NumericUpDown
            {
                Location = new Point(230, yPos - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                Font = new Font("Segoe UI", 9F)
            };
            _nudIzosiyanatRatio.ValueChanged += Ratio_ValueChanged;
            this.Controls.Add(_nudIzosiyanatRatio);

            var lblColon = new Label
            {
                Text = ":",
                Location = new Point(295, yPos),
                Width = 10,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false
            };
            this.Controls.Add(lblColon);

            _nudPoliolRatio = new NumericUpDown
            {
                Location = new Point(305, yPos - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                Font = new Font("Segoe UI", 9F)
            };
            _nudPoliolRatio.ValueChanged += Ratio_ValueChanged;
            this.Controls.Add(_nudPoliolRatio);
            yPos += 35;

            // İzosiyanat ve Poliol bilgileri (oran gösterimi ile)
            _lblIzosiyanatInfo = new Label
            {
                Text = "",
                Location = new Point(50, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Visible = true
            };
            this.Controls.Add(_lblIzosiyanatInfo);
            yPos += 30;

            _lblPoliolInfo = new Label
            {
                Text = "",
                Location = new Point(50, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Visible = true
            };
            this.Controls.Add(_lblPoliolInfo);
            yPos += 50;

            // Butonlar
            int buttonWidth = 100;
            int buttonSpacing = 20;
            int startX = (this.Width - (buttonWidth * 2 + buttonSpacing)) / 2;

            _btnSave = new Button
            {
                Text = "Kaydet",
                Location = new Point(startX, yPos),
                Width = buttonWidth,
                Height = 32,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnSave, 4);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "İptal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(startX + buttonWidth + buttonSpacing, yPos),
                Width = buttonWidth,
                Height = 32,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnCancel, 4);

            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
            this.AcceptButton = _btnSave;
            this.CancelButton = _btnCancel;

            this.Height = yPos + _btnSave.Height + 50;
        }

        private void LoadData()
        {
            // Varsayılan olarak İzosiyanat+Poliol seçili
            SelectedMethod = "İzosiyanat+Poliol";
            IzosiyanatRatio = 1;
            PoliolRatio = 1;
            decimal lengthInMeters = _assembly.Length / 1000m;
            decimal totalLengthM = lengthInMeters * IsolationCount;
            IsolationLiquidAmount = totalLengthM * 2m; // kg cinsinden
            
            // İlk hesaplamayı yap
            UpdateIzosiyanatPoliolInfo();
        }

        private void RdoMethod_CheckedChanged(object sender, EventArgs e)
        {
            if (_rdoMsSilikon.Checked)
            {
                SelectedMethod = "MS Silikon";
                _lblMsSilikonInfo.Visible = true;
                _lblIzosiyanatInfo.Visible = false;
                _lblPoliolInfo.Visible = false;
                _nudIzosiyanatRatio.Enabled = false;
                _nudPoliolRatio.Enabled = false;

                // MS Silikon için kg cinsinden hesapla
                decimal lengthInMeters = _assembly.Length / 1000m;
                decimal totalLengthM = lengthInMeters * IsolationCount;
                IsolationLiquidAmount = totalLengthM * 2m; // kg cinsinden (1 metre = 2 kg izolasyon sıvısı)
            }
            else if (_rdoIzosiyanatPoliol.Checked)
            {
                SelectedMethod = "İzosiyanat+Poliol";
                _lblMsSilikonInfo.Visible = false;
                _lblIzosiyanatInfo.Visible = true;
                _lblPoliolInfo.Visible = true;
                _nudIzosiyanatRatio.Enabled = true;
                _nudPoliolRatio.Enabled = true;

                // İzosiyanat+Poliol için kg cinsinden hesapla
                decimal lengthInMeters = _assembly.Length / 1000m;
                decimal totalLengthM = lengthInMeters * IsolationCount;
                IsolationLiquidAmount = totalLengthM * 2m; // kg cinsinden
                
                UpdateIzosiyanatPoliolInfo();
            }
        }

        private void Ratio_ValueChanged(object sender, EventArgs e)
        {
            if (_rdoIzosiyanatPoliol.Checked)
            {
                UpdateIzosiyanatPoliolInfo();
            }
        }

        private void UpdateIzosiyanatPoliolInfo()
        {
            if (!_rdoIzosiyanatPoliol.Checked)
                return;

            decimal lengthInMeters = _assembly.Length / 1000m;
            decimal totalLengthM = lengthInMeters * IsolationCount;
            decimal totalLiquidAmountKg = totalLengthM * 2m; // Toplam kg

            int izosiyanatRatio = (int)_nudIzosiyanatRatio.Value;
            int poliolRatio = (int)_nudPoliolRatio.Value;
            int totalRatio = izosiyanatRatio + poliolRatio;

            // Oranlara göre dağıt
            decimal izosiyanatKg = (totalLiquidAmountKg * izosiyanatRatio) / totalRatio;
            decimal poliolKg = (totalLiquidAmountKg * poliolRatio) / totalRatio;

            _lblIzosiyanatInfo.Text = $"Gereken İzosiyanat: {izosiyanatKg:F3} kg";
            _lblPoliolInfo.Text = $"Gereken Poliol: {poliolKg:F3} kg";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckStock())
                {
                    return; // Hata mesajı zaten gösterildi
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CheckStock()
        {
            try
            {
                if (SelectedMethod == "MS Silikon")
                {
                    // MS Silikon stok kontrolü (kg cinsinden)
                    decimal lengthInMeters = _assembly.Length / 1000m;
                    decimal totalLengthM = lengthInMeters * IsolationCount;
                    decimal msSilikonNeededKg = totalLengthM * 2m * 0.95m / 2m; // Toplam kg'dan MS Silikon kg'ına çevir

                    var msSilikonStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "MS Silikon" && s.Kilogram > 0)
                        .ToList();

                    decimal totalAvailableKg = msSilikonStocks.Sum(s => s.Kilogram);

                    if (totalAvailableKg < msSilikonNeededKg)
                    {
                        MessageBox.Show($"Yetersiz MS Silikon stoku!\nGereken: {msSilikonNeededKg:F3} kg\nMevcut: {totalAvailableKg:F3} kg", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                else if (SelectedMethod == "İzosiyanat+Poliol")
                {
                    // İzosiyanat+Poliol stok kontrolü (oranlara göre)
                    decimal lengthInMeters = _assembly.Length / 1000m;
                    decimal totalLengthM = lengthInMeters * IsolationCount;
                    decimal totalLiquidAmountKg = totalLengthM * 2m;

                    int izosiyanatRatio = (int)_nudIzosiyanatRatio.Value;
                    int poliolRatio = (int)_nudPoliolRatio.Value;
                    int totalRatio = izosiyanatRatio + poliolRatio;

                    // Oranlara göre dağıt
                    decimal izosiyanatKg = (totalLiquidAmountKg * izosiyanatRatio) / totalRatio;
                    decimal poliolKg = (totalLiquidAmountKg * poliolRatio) / totalRatio;

                    // İzosiyanat kontrolü (kg cinsinden)
                    var izosiyanatStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "İzosiyanat" && s.Kilogram > 0)
                        .ToList();
                    decimal totalAvailableIzosiyanatKg = izosiyanatStocks.Sum(s => s.Kilogram);

                    // Poliol kontrolü (kg cinsinden)
                    var poliolStocks = _isolationStockRepository.GetAll()
                        .Where(s => s.LiquidType == "Poliol" && s.Kilogram > 0)
                        .ToList();
                    decimal totalAvailablePoliolKg = poliolStocks.Sum(s => s.Kilogram);

                    if (totalAvailableIzosiyanatKg < izosiyanatKg)
                    {
                        MessageBox.Show($"Yetersiz İzosiyanat stoku!\nGereken: {izosiyanatKg:F3} kg\nMevcut: {totalAvailableIzosiyanatKg:F3} kg", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (totalAvailablePoliolKg < poliolKg)
                    {
                        MessageBox.Show($"Yetersiz Poliol stoku!\nGereken: {poliolKg:F3} kg\nMevcut: {totalAvailablePoliolKg:F3} kg", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    IzosiyanatRatio = izosiyanatRatio;
                    PoliolRatio = poliolRatio;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stok kontrolü yapılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}

