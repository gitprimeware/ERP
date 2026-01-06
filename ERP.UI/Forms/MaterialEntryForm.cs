using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Factories;
using ERP.UI.Services;
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
        private TextBox _txtSearch;
        private ComboBox _cmbSupplierFilter;
        private Button _btnSearch;
        private Button _btnRefresh;

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

            // Arama paneli (içinde Ekle butonu da var)
            var searchPanel = CreateSearchPanel();
            searchPanel.Location = new Point(30, 80);

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 140),
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
                searchPanel.Width = _mainPanel.Width - 60;
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 180;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(searchPanel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private Panel CreateSearchPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // TableLayoutPanel ile responsive yapı
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RowCount = 1,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Kolon genişliklerini yüzdelik olarak ayarla
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Arama kutusu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Tedarikçi:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Tedarikçi combo
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ara butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Yenile butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ekle butonu
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Sağ boşluk

            // Ara
            var lblSearch = new Label
            {
                Text = "Ara:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(0, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblSearch, 0, 0);

            _txtSearch = new TextBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformSearch(); };
            tableLayout.Controls.Add(_txtSearch, 1, 0);

            // Tedarikçi
            var lblSupplier = new Label
            {
                Text = "Tedarikçi:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblSupplier, 2, 0);

            _cmbSupplierFilter = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            LoadSuppliersForFilter();
            tableLayout.Controls.Add(_cmbSupplierFilter, 3, 0);

            // Ara butonu
            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Primary, Color.White, 80, 30);
            _btnSearch.Click += (s, e) => PerformSearch();
            tableLayout.Controls.Add(_btnSearch, 4, 0);

            // Yenile butonu
            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Success, Color.White, 90, 30);
            _btnRefresh.Click += (s, e) =>
            {
                _txtSearch.Text = "";
                _cmbSupplierFilter.SelectedIndex = 0;
                PerformSearch();
            };
            tableLayout.Controls.Add(_btnRefresh, 5, 0);

            // Ekle butonu
            _btnAdd = ButtonFactory.CreateActionButton("➕ Ekle", ThemeColors.Success, Color.White, 100, 30);
            _btnAdd.Click += BtnAdd_Click;
            tableLayout.Controls.Add(_btnAdd, 6, 0);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private void LoadSuppliersForFilter()
        {
            try
            {
                _cmbSupplierFilter.Items.Clear();
                
                // Tüm Tedarikçiler seçeneği
                _cmbSupplierFilter.Items.Add(new { Id = (Guid?)null, Name = "Tüm Tedarikçiler" });
                
                // Tedarikçileri yükle
                var suppliers = _supplierRepository.GetAll().OrderBy(s => s.Name).ToList();
                foreach (var supplier in suppliers)
                {
                    _cmbSupplierFilter.Items.Add(new { Id = (Guid?)supplier.Id, Name = supplier.Name });
                }
                
                _cmbSupplierFilter.DisplayMember = "Name";
                _cmbSupplierFilter.ValueMember = "Id";
                _cmbSupplierFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tedarikçiler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void PerformSearch()
        {
            try
            {
                string searchTerm = _txtSearch?.Text?.Trim() ?? "";
                Guid? supplierId = null;

                if (_cmbSupplierFilter?.SelectedItem != null)
                {
                    var selected = _cmbSupplierFilter.SelectedItem;
                    var idProperty = selected.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(selected);
                        if (idValue != null && idValue != DBNull.Value)
                        {
                            supplierId = (Guid?)idValue;
                        }
                    }
                }

                // Filtreleme ile malzeme girişlerini getir
                var entries = _repository.GetAll(
                    string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    supplierId
                );
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
                        var materialEntryId = _repository.Insert(dialog.MaterialEntry);
                        
                        // Event feed kaydı ekle
                        var serialNoRepository = new SerialNoRepository();
                        var serialNo = dialog.MaterialEntry.SerialNoId.HasValue 
                            ? serialNoRepository.GetById(dialog.MaterialEntry.SerialNoId.Value) 
                            : null;
                        var serialNumber = serialNo?.SerialNumber ?? "Bilinmeyen";
                        EventFeedService.MaterialEntryCreated(materialEntryId, serialNumber, dialog.MaterialEntry.Quantity);
                        
                        MessageBox.Show("Malzeme girişi başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        PerformSearch();
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
                            PerformSearch();
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

