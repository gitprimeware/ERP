using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class MaterialEntryForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private Button _btnAdd;
        private MaterialEntryRepository _repository;
        private SupplierRepository _supplierRepository;

        public MaterialEntryForm()
        {
            _repository = new MaterialEntryRepository();
            _supplierRepository = new SupplierRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadMaterialEntries();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "Malzeme Giriş",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Ekle butonu
            _btnAdd = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Success, Color.White, 120, 40);
            _btnAdd.Location = new Point(30, 80);
            _btnAdd.Click += BtnAdd_Click;

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 130),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 180,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None
            };
            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            _mainPanel.Resize += (s, e) =>
            {
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 180;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_btnAdd);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadMaterialEntries()
        {
            try
            {
                var entries = _repository.GetAll();
                LoadDataGridView(entries);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Malzeme girişleri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataGridView(List<MaterialEntry> entries)
        {
            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();

            if (entries.Count == 0)
            {
                return;
            }

            _dataGridView.AutoGenerateColumns = false;

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransactionType",
                HeaderText = "İşlem",
                Name = "TransactionType",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaterialType",
                HeaderText = "Malzeme Türü",
                Name = "MaterialType",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaterialSize",
                HeaderText = "Malzeme Boyutu",
                Name = "MaterialSize",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SupplierName",
                HeaderText = "Tedarikçi",
                Name = "SupplierName",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InvoiceNo",
                HeaderText = "Fatura No",
                Name = "InvoiceNo",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrexPurchaseNo",
                HeaderText = "Trex Satın Alma No",
                Name = "TrexPurchaseNo",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EntryDate",
                HeaderText = "Giriş Tarihi",
                Name = "EntryDate",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Malzeme Miktarı",
                Name = "Quantity",
                Width = 120
            });

            var dataSource = entries.Select(e => new
            {
                e.Id,
                e.TransactionType,
                e.MaterialType,
                e.MaterialSize,
                SupplierName = e.Supplier?.Name ?? "",
                e.InvoiceNo,
                e.TrexPurchaseNo,
                EntryDate = e.EntryDate.ToString("dd.MM.yyyy"),
                Quantity = e.Quantity.ToString("F3", CultureInfo.InvariantCulture)
            }).ToList();

            _dataGridView.DataSource = dataSource;
            _dataGridView.Tag = entries;

            // Stil ayarları
            _dataGridView.DefaultCellStyle.BackColor = ThemeColors.Surface;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new MaterialEntryDialog(_supplierRepository))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.MaterialEntry != null)
                {
                    try
                    {
                        _repository.Insert(dialog.MaterialEntry);
                        MessageBox.Show("Malzeme girişi başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMaterialEntries();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Malzeme girişi eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_dataGridView.Tag is List<MaterialEntry> entries && e.RowIndex < entries.Count)
            {
                var entry = entries[e.RowIndex];
                using (var dialog = new MaterialEntryDialog(_supplierRepository, entry))
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.MaterialEntry != null)
                    {
                        try
                        {
                            _repository.Update(dialog.MaterialEntry);
                            MessageBox.Show("Malzeme girişi başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadMaterialEntries();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Malzeme girişi güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}

