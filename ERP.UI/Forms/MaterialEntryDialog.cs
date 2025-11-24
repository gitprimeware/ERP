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
    public partial class MaterialEntryDialog : Form
    {
        private ComboBox _cmbTransactionType;
        private ComboBox _cmbMaterialType;
        private ComboBox _cmbSize;
        private ComboBox _cmbThickness;
        private TextBox _txtMaterialSize;
        private ComboBox _cmbSupplier;
        private Button _btnAddSupplier;
        private TextBox _txtInvoiceNo;
        private TextBox _txtTrexPurchaseNo;
        private DateTimePicker _dtpEntryDate;
        private TextBox _txtQuantity;
        private Button _btnSave;
        private Button _btnCancel;
        private SupplierRepository _supplierRepository;
        private MaterialEntry _materialEntry;
        private bool _isEditMode;

        public MaterialEntry MaterialEntry { get; private set; }

        public MaterialEntryDialog(SupplierRepository supplierRepository, MaterialEntry entry = null)
        {
            _supplierRepository = supplierRepository;
            _materialEntry = entry;
            _isEditMode = entry != null;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Malzeme Giriş Düzenle" : "Yeni Malzeme Giriş";
            this.Width = 600;
            this.Height = 500;
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
            _cmbTransactionType.Items.Add("Satın Alma Giriş");
            _cmbTransactionType.Items.Add("Düzenleme Giriş");
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

            // Tedarikçi
            var lblSupplier = new Label
            {
                Text = "Tedarikçi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            var supplierPanel = new Panel
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30
            };
            _cmbSupplier = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = controlWidth - 120,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            _btnAddSupplier = new Button
            {
                Text = "+ Ekle",
                Dock = DockStyle.Right,
                Width = 110,
                Height = 30,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnAddSupplier, 4);
            _btnAddSupplier.Click += BtnAddSupplier_Click;
            LoadSuppliers();
            supplierPanel.Controls.Add(_cmbSupplier);
            supplierPanel.Controls.Add(_btnAddSupplier);
            this.Controls.Add(lblSupplier);
            this.Controls.Add(supplierPanel);
            yPos += spacing;

            // Fatura No
            var lblInvoiceNo = new Label
            {
                Text = "Fatura No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtInvoiceNo = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblInvoiceNo);
            this.Controls.Add(_txtInvoiceNo);
            yPos += spacing;

            // Trex Satın Alma No
            var lblTrexPurchaseNo = new Label
            {
                Text = "Trex Satın Alma No:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _txtTrexPurchaseNo = new TextBox
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblTrexPurchaseNo);
            this.Controls.Add(_txtTrexPurchaseNo);
            yPos += spacing;

            // Giriş Tarihi
            var lblEntryDate = new Label
            {
                Text = "Giriş Tarihi:",
                Location = new Point(20, yPos),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10F)
            };
            _dtpEntryDate = new DateTimePicker
            {
                Location = new Point(180, yPos - 3),
                Width = controlWidth,
                Height = 30,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };
            _dtpEntryDate.Value = DateTime.Now;
            this.Controls.Add(lblEntryDate);
            this.Controls.Add(_dtpEntryDate);
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

        private void LoadSuppliers()
        {
            try
            {
                _cmbSupplier.Items.Clear();
                var suppliers = _supplierRepository.GetAll();
                
                foreach (var supplier in suppliers)
                {
                    _cmbSupplier.Items.Add(new { Id = supplier.Id, Name = supplier.Name });
                }
                
                _cmbSupplier.DisplayMember = "Name";
                _cmbSupplier.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tedarikçiler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddSupplier_Click(object sender, EventArgs e)
        {
            using (var dialog = new Form
            {
                Text = "Yeni Tedarikçi Ekle",
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
                    Text = "Tedarikçi Adı:",
                    Location = new Point(20, 30),
                    AutoSize = true
                };

                var txtName = new TextBox
                {
                    Location = new Point(120, 27),
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
                        var newSupplier = new Supplier { Name = txtName.Text };
                        var supplierId = _supplierRepository.Insert(newSupplier);
                        
                        LoadSuppliers();
                        
                        foreach (var item in _cmbSupplier.Items)
                        {
                            var idProperty = item.GetType().GetProperty("Id");
                            if (idProperty != null && idProperty.GetValue(item).Equals(supplierId))
                            {
                                _cmbSupplier.SelectedItem = item;
                                break;
                            }
                        }
                        
                        MessageBox.Show("Tedarikçi başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Tedarikçi eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadData()
        {
            if (_isEditMode && _materialEntry != null)
            {
                _cmbTransactionType.Text = _materialEntry.TransactionType;
                _cmbMaterialType.Text = _materialEntry.MaterialType;
                
                // Size ve Thickness'i bul
                for (int i = 0; i < _cmbSize.Items.Count; i++)
                {
                    if ((int)_cmbSize.Items[i] == _materialEntry.Size)
                    {
                        _cmbSize.SelectedIndex = i;
                        break;
                    }
                }
                
                for (int i = 0; i < _cmbThickness.Items.Count; i++)
                {
                    if (decimal.Parse(_cmbThickness.Items[i].ToString(), CultureInfo.InvariantCulture) == _materialEntry.Thickness)
                    {
                        _cmbThickness.SelectedIndex = i;
                        break;
                    }
                }
                
                _txtMaterialSize.Text = _materialEntry.MaterialSize;
                
                if (_materialEntry.SupplierId.HasValue)
                {
                    foreach (var item in _cmbSupplier.Items)
                    {
                        var idProperty = item.GetType().GetProperty("Id");
                        if (idProperty != null && idProperty.GetValue(item).Equals(_materialEntry.SupplierId.Value))
                        {
                            _cmbSupplier.SelectedItem = item;
                            break;
                        }
                    }
                }
                
                _txtInvoiceNo.Text = _materialEntry.InvoiceNo ?? "";
                _txtTrexPurchaseNo.Text = _materialEntry.TrexPurchaseNo ?? "";
                _dtpEntryDate.Value = _materialEntry.EntryDate;
                _txtQuantity.Text = _materialEntry.Quantity.ToString("F3", CultureInfo.InvariantCulture);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var entry = _isEditMode ? _materialEntry : new MaterialEntry();
                
                entry.TransactionType = _cmbTransactionType.SelectedItem.ToString();
                entry.MaterialType = _cmbMaterialType.SelectedItem.ToString();
                entry.MaterialSize = _txtMaterialSize.Text;
                entry.Size = (int)_cmbSize.SelectedItem;
                entry.Thickness = decimal.Parse(_cmbThickness.SelectedItem.ToString(), CultureInfo.InvariantCulture);
                
                if (_cmbSupplier.SelectedItem != null)
                {
                    var idProperty = _cmbSupplier.SelectedItem.GetType().GetProperty("Id");
                    entry.SupplierId = (Guid)idProperty.GetValue(_cmbSupplier.SelectedItem);
                }
                
                entry.InvoiceNo = _txtInvoiceNo.Text;
                entry.TrexPurchaseNo = _txtTrexPurchaseNo.Text;
                entry.EntryDate = _dtpEntryDate.Value;
                entry.Quantity = decimal.Parse(_txtQuantity.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                MaterialEntry = entry;
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

