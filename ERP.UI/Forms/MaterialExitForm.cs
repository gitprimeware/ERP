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
                        LoadMaterialExits();
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
                            LoadMaterialExits();
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

