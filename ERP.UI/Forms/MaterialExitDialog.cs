using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class MaterialExitDialog : Form
    {
        private ComboBox _cmbTransactionType;
        private ComboBox _cmbMaterialType;
        private ComboBox _cmbSize;
        private ComboBox _cmbThickness;
        private TextBox _txtMaterialSize;
        private ComboBox _cmbCompany;
        private TextBox _txtTrexInvoiceNo;
        private DateTimePicker _dtpExitDate;
        private TextBox _txtQuantity;
        private Button _btnSave;
        private Button _btnCancel;
        private CompanyRepository _companyRepository;
        private MaterialExit _materialExit;
        private bool _isEditMode;

        public MaterialExit MaterialExit { get; private set; }

        public MaterialExitDialog(CompanyRepository companyRepository, MaterialExit exit = null)
        {
            _companyRepository = companyRepository;
            _materialExit = exit;
            _isEditMode = exit != null;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Malzeme Çıkış Düzenle" : "Yeni Malzeme Çıkış";
            this.Width = 600;
            this.Height = 450;
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
            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 300;
            int spacing = 35;

            // İşlem
            var lblTransactionType = new Label
            {
                Text = "İşlem:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbTransactionType = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbTransactionType.Items.Add("Hurda Çıkış");
            _cmbTransactionType.Items.Add("Düzenleme Çıkış");
            _cmbTransactionType.SelectedIndex = 0;
            this.Controls.Add(lblTransactionType);
            this.Controls.Add(_cmbTransactionType);
            yPos += spacing;

            // Malzeme Türü
            var lblMaterialType = new Label
            {
                Text = "Malzeme Türü:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbMaterialType = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbMaterialType.Items.Add("Alüminyum");
            _cmbMaterialType.Items.Add("Galvaniz");
            _cmbMaterialType.SelectedIndex = 0;
            _cmbMaterialType.SelectedIndexChanged += (s, e) => UpdateMaterialSize();
            this.Controls.Add(lblMaterialType);
            this.Controls.Add(_cmbMaterialType);
            yPos += spacing;

            // Boyut
            var lblSize = new Label
            {
                Text = "Boyut:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbSize = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            LoadSizes();
            _cmbSize.SelectedIndexChanged += (s, e) => UpdateMaterialSize();
            this.Controls.Add(lblSize);
            this.Controls.Add(_cmbSize);
            yPos += spacing;

            // Kalınlık
            var lblThickness = new Label
            {
                Text = "Kalınlık:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbThickness = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            LoadThicknesses();
            _cmbThickness.SelectedIndexChanged += (s, e) => UpdateMaterialSize();
            this.Controls.Add(lblThickness);
            this.Controls.Add(_cmbThickness);
            yPos += spacing;

            // Malzeme Boyutu (Readonly)
            var lblMaterialSize = new Label
            {
                Text = "Malzeme Boyutu:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtMaterialSize = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                ReadOnly = true,
                BackColor = ThemeColors.SurfaceDark,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblMaterialSize);
            this.Controls.Add(_txtMaterialSize);
            yPos += spacing;

            // Firma (firma ekleme butonu yok)
            var lblCompany = new Label
            {
                Text = "Firma:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _cmbCompany = new ComboBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            LoadCompanies();
            this.Controls.Add(lblCompany);
            this.Controls.Add(_cmbCompany);
            yPos += spacing;

            // Trex Fatura No
            var lblTrexInvoiceNo = new Label
            {
                Text = "Trex Fatura No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtTrexInvoiceNo = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblTrexInvoiceNo);
            this.Controls.Add(_txtTrexInvoiceNo);
            yPos += spacing;

            // Çıkış Tarihi
            var lblExitDate = new Label
            {
                Text = "Çıkış Tarihi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _dtpExitDate = new DateTimePicker
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };
            _dtpExitDate.Value = DateTime.Now;
            this.Controls.Add(lblExitDate);
            this.Controls.Add(_dtpExitDate);
            yPos += spacing;

            // Malzeme Miktarı
            var lblQuantity = new Label
            {
                Text = "Malzeme Miktarı:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtQuantity = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblQuantity);
            this.Controls.Add(_txtQuantity);
            yPos += spacing + 10;

            // Butonlar
            _btnCancel = new Button
            {
                Text = "İptal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(380, yPos),
                Width = 100,
                Height = 35,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnCancel, 4);

            _btnSave = new Button
            {
                Text = "Kaydet",
                Location = new Point(270, yPos),
                Width = 100,
                Height = 35,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnSave, 4);
            _btnSave.Click += BtnSave_Click;

            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
            this.AcceptButton = _btnSave;
            this.CancelButton = _btnCancel;

            UpdateMaterialSize();
        }

        private void LoadSizes()
        {
            _cmbSize.Items.Clear();
            var sizes = new[] { 214, 213, 313, 314, 413, 414, 513, 514, 613, 614, 713, 714, 813, 814, 1013, 1014, 251, 351, 451, 551, 651, 751, 851, 1051 };
            foreach (var size in sizes)
            {
                _cmbSize.Items.Add(size);
            }
            if (_cmbSize.Items.Count > 0)
                _cmbSize.SelectedIndex = 0;
        }

        private void LoadThicknesses()
        {
            _cmbThickness.Items.Clear();
            var thicknesses = new[] { 0.120m, 0.150m, 0.165m, 0.185m, 0.85m, 1.0m };
            foreach (var thickness in thicknesses)
            {
                _cmbThickness.Items.Add(thickness.ToString("F3", CultureInfo.InvariantCulture));
            }
            if (_cmbThickness.Items.Count > 0)
                _cmbThickness.SelectedIndex = 0;
        }

        private void UpdateMaterialSize()
        {
            if (_cmbMaterialType.SelectedItem == null || _cmbSize.SelectedItem == null || _cmbThickness.SelectedItem == null)
                return;

            string materialType = _cmbMaterialType.SelectedItem.ToString();
            int size = (int)_cmbSize.SelectedItem;
            decimal thickness = decimal.Parse(_cmbThickness.SelectedItem.ToString(), CultureInfo.InvariantCulture);

            string prefix = materialType == "Alüminyum" ? "Alü." : "Gal.";
            _txtMaterialSize.Text = $"{prefix} {size}X{thickness.ToString("F3", CultureInfo.InvariantCulture).Replace(".", ",")}";
        }

        private void LoadCompanies()
        {
            try
            {
                _cmbCompany.Items.Clear();
                var companies = _companyRepository.GetAll();
                
                foreach (var company in companies)
                {
                    _cmbCompany.Items.Add(new { Id = company.Id, Name = company.Name });
                }
                
                _cmbCompany.DisplayMember = "Name";
                _cmbCompany.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            if (_isEditMode && _materialExit != null)
            {
                _cmbTransactionType.Text = _materialExit.TransactionType;
                _cmbMaterialType.Text = _materialExit.MaterialType;
                
                // Size ve Thickness'i bul
                for (int i = 0; i < _cmbSize.Items.Count; i++)
                {
                    if ((int)_cmbSize.Items[i] == _materialExit.Size)
                    {
                        _cmbSize.SelectedIndex = i;
                        break;
                    }
                }
                
                for (int i = 0; i < _cmbThickness.Items.Count; i++)
                {
                    if (decimal.Parse(_cmbThickness.Items[i].ToString(), CultureInfo.InvariantCulture) == _materialExit.Thickness)
                    {
                        _cmbThickness.SelectedIndex = i;
                        break;
                    }
                }
                
                _txtMaterialSize.Text = _materialExit.MaterialSize;
                
                if (_materialExit.CompanyId.HasValue)
                {
                    foreach (var item in _cmbCompany.Items)
                    {
                        var idProperty = item.GetType().GetProperty("Id");
                        if (idProperty != null && idProperty.GetValue(item).Equals(_materialExit.CompanyId.Value))
                        {
                            _cmbCompany.SelectedItem = item;
                            break;
                        }
                    }
                }
                
                _txtTrexInvoiceNo.Text = _materialExit.TrexInvoiceNo ?? "";
                _dtpExitDate.Value = _materialExit.ExitDate;
                _txtQuantity.Text = _materialExit.Quantity.ToString("F3", CultureInfo.InvariantCulture);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var exit = _isEditMode ? _materialExit : new MaterialExit();
                
                exit.TransactionType = _cmbTransactionType.SelectedItem.ToString();
                exit.MaterialType = _cmbMaterialType.SelectedItem.ToString();
                exit.MaterialSize = _txtMaterialSize.Text;
                exit.Size = (int)_cmbSize.SelectedItem;
                exit.Thickness = decimal.Parse(_cmbThickness.SelectedItem.ToString(), CultureInfo.InvariantCulture);
                
                if (_cmbCompany.SelectedItem != null)
                {
                    var idProperty = _cmbCompany.SelectedItem.GetType().GetProperty("Id");
                    exit.CompanyId = (Guid)idProperty.GetValue(_cmbCompany.SelectedItem);
                }
                
                exit.TrexInvoiceNo = _txtTrexInvoiceNo.Text;
                exit.ExitDate = _dtpExitDate.Value;
                exit.Quantity = decimal.Parse(_txtQuantity.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                MaterialExit = exit;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Form doğrulama hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (_cmbTransactionType.SelectedItem == null)
            {
                MessageBox.Show("Lütfen işlem seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbMaterialType.SelectedItem == null)
            {
                MessageBox.Show("Lütfen malzeme türü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbSize.SelectedItem == null)
            {
                MessageBox.Show("Lütfen boyut seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cmbThickness.SelectedItem == null)
            {
                MessageBox.Show("Lütfen kalınlık seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtQuantity.Text) || !decimal.TryParse(_txtQuantity.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir malzeme miktarı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}

