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
    public partial class MaterialExitForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private Button _btnAdd;
        private MaterialExitRepository _repository;
        private CompanyRepository _companyRepository;
        private TextBox _txtSearch;
        private ComboBox _cmbCompanyFilter;
        private Button _btnSearch;
        private Button _btnRefresh;

        public MaterialExitForm()
        {
            _repository = new MaterialExitRepository();
            _companyRepository = new CompanyRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadMaterialExits();
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
                Text = "Malzeme Çıkış",
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
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Firma:
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Firma combo
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

            // Firma
            var lblCompany = new Label
            {
                Text = "Firma:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(5, 15, 0, 0)
            };
            tableLayout.Controls.Add(lblCompany, 2, 0);

            _cmbCompanyFilter = new ComboBox
            {
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 12, 5, 8)
            };
            LoadCompaniesForFilter();
            tableLayout.Controls.Add(_cmbCompanyFilter, 3, 0);

            // Ara butonu
            _btnSearch = ButtonFactory.CreateActionButton("🔍 Ara", ThemeColors.Primary, Color.White, 80, 30);
            _btnSearch.Click += (s, e) => PerformSearch();
            tableLayout.Controls.Add(_btnSearch, 4, 0);

            // Yenile butonu
            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Success, Color.White, 90, 30);
            _btnRefresh.Click += (s, e) =>
            {
                _txtSearch.Text = "";
                _cmbCompanyFilter.SelectedIndex = 0;
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

        private void LoadCompaniesForFilter()
        {
            try
            {
                _cmbCompanyFilter.Items.Clear();
                
                // Tüm Firmalar seçeneği
                _cmbCompanyFilter.Items.Add(new { Id = (Guid?)null, Name = "Tüm Firmalar" });
                
                // Firmaları yükle
                var companies = _companyRepository.GetAll().OrderBy(c => c.Name).ToList();
                foreach (var company in companies)
                {
                    _cmbCompanyFilter.Items.Add(new { Id = (Guid?)company.Id, Name = company.Name });
                }
                
                _cmbCompanyFilter.DisplayMember = "Name";
                _cmbCompanyFilter.ValueMember = "Id";
                _cmbCompanyFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaterialExits()
        {
            try
            {
                var exits = _repository.GetAll();
                LoadDataGridView(exits);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Malzeme çıkışları yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformSearch()
        {
            try
            {
                string searchTerm = _txtSearch?.Text?.Trim() ?? "";
                Guid? companyId = null;

                if (_cmbCompanyFilter?.SelectedItem != null)
                {
                    var selected = _cmbCompanyFilter.SelectedItem;
                    var idProperty = selected.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(selected);
                        if (idValue != null && idValue != DBNull.Value)
                        {
                            companyId = (Guid?)idValue;
                        }
                    }
                }

                // Filtreleme ile malzeme çıkışlarını getir
                var exits = _repository.GetAll(
                    string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    companyId
                );
                LoadDataGridView(exits);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Malzeme çıkışları yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataGridView(List<MaterialExit> exits)
        {
            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();

            if (exits.Count == 0)
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
                DataPropertyName = "CompanyName",
                HeaderText = "Firma",
                Name = "CompanyName",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TrexInvoiceNo",
                HeaderText = "Trex Fatura No",
                Name = "TrexInvoiceNo",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ExitDate",
                HeaderText = "Çıkış Tarihi",
                Name = "ExitDate",
                Width = 120
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Malzeme Miktarı",
                Name = "Quantity",
                Width = 120
            });

            var dataSource = exits.Select(e => new
            {
                e.Id,
                e.TransactionType,
                e.MaterialType,
                e.MaterialSize,
                CompanyName = e.Company?.Name ?? "",
                e.TrexInvoiceNo,
                ExitDate = e.ExitDate.ToString("dd.MM.yyyy"),
                Quantity = e.Quantity.ToString("F3", CultureInfo.InvariantCulture)
            }).ToList();

            _dataGridView.DataSource = dataSource;
            _dataGridView.Tag = exits;

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
            using (var dialog = new MaterialExitDialog(_companyRepository))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.MaterialExit != null)
                {
                    try
                    {
                        _repository.Insert(dialog.MaterialExit);
                        MessageBox.Show("Malzeme çıkışı başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        PerformSearch();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Malzeme çıkışı eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_dataGridView.Tag is List<MaterialExit> exits && e.RowIndex < exits.Count)
            {
                var exit = exits[e.RowIndex];
                using (var dialog = new MaterialExitDialog(_companyRepository, exit))
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.MaterialExit != null)
                    {
                        try
                        {
                            _repository.Update(dialog.MaterialExit);
                            MessageBox.Show("Malzeme çıkışı başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            PerformSearch();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Malzeme çıkışı güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}

