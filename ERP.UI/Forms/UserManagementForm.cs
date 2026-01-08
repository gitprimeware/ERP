using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class UserManagementForm : UserControl
    {
        private DataGridView _dgvUsers;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtFullName;
        private CheckBox _chkIsAdmin;
        private CheckedListBox _clbPermissions;
        private Button _btnSave;
        private Button _btnDelete;
        private Button _btnNew;
        private UserRepository _userRepository;
        private UserPermissionRepository _permissionRepository;
        private User _selectedUser;
        private List<string> _allPermissionKeys;

        public UserManagementForm()
        {
            _userRepository = new UserRepository();
            _permissionRepository = new UserPermissionRepository();
            _allPermissionKeys = new List<string>
            {
                "OrderEntry",
                "StockEntry",
                "Accounting",
                "StockManagement",
                "ProductionPlanning",
                "CuttingRequests",
                "PressingRequests",
                "ClampingRequests",
                "Clamping2Requests",
                "AssemblyRequests",
                "PackagingRequests",
                "Consumption",
                "ConsumptionMaterialStock",
                "Reports",
                "Settings"
            };
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadUsers();
        }

        private void CreateMainPanel()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Başlık
            var lblTitle = new Label
            {
                Text = "👥 Kullanıcı Yönetimi",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            mainPanel.Controls.Add(lblTitle);

            // Split container - Sol: Liste, Sağ: Form
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Location = new Point(30, 80),
                Size = new Size(mainPanel.Width - 60, mainPanel.Height - 110),
                SplitterDistance = (mainPanel.Width - 60) / 2,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.None
            };

            // Sol Panel - Kullanıcı Listesi
            CreateUsersListPanel(splitContainer.Panel1);

            // Sağ Panel - Kullanıcı Formu
            CreateUserFormPanel(splitContainer.Panel2);

            mainPanel.Controls.Add(splitContainer);

            this.Controls.Add(mainPanel);
        }

        private void CreateUsersListPanel(Panel panel)
        {
            panel.BackColor = Color.White;
            panel.Padding = new Padding(10);

            // Başlık
            var lblListTitle = new Label
            {
                Text = "Kullanıcılar",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            panel.Controls.Add(lblListTitle);

            // DataGridView
            _dgvUsers = new DataGridView
            {
                Location = new Point(10, 50),
                Width = panel.Width - 20,
                Height = panel.Height - 60,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false
            };

            // Kolonlar
            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Username",
                HeaderText = "Kullanıcı Adı",
                Name = "Username",
                Width = 150
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FullName",
                HeaderText = "Ad Soyad",
                Name = "FullName",
                Width = 200
            });

            _dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsAdmin",
                HeaderText = "Admin",
                Name = "IsAdmin",
                Width = 80
            });

            // Stil ayarları
            _dgvUsers.DefaultCellStyle.BackColor = Color.White;
            _dgvUsers.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dgvUsers.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dgvUsers.EnableHeadersVisualStyles = false;

            _dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;
            panel.Controls.Add(_dgvUsers);
        }

        private void CreateUserFormPanel(Panel panel)
        {
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20);
            panel.AutoScroll = true;

            int yPos = 20;

            // Başlık
            var lblFormTitle = new Label
            {
                Text = "Kullanıcı Bilgileri",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lblFormTitle);
            yPos += 40;

            // Kullanıcı Adı
            var lblUsername = new Label
            {
                Text = "Kullanıcı Adı:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lblUsername);
            yPos += 25;

            _txtUsername = new TextBox
            {
                Location = new Point(20, yPos),
                Width = panel.Width - 40,
                Height = 32,
                Font = new Font("Segoe UI", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(_txtUsername);
            yPos += 45;

            // Şifre
            var lblPassword = new Label
            {
                Text = "Şifre:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lblPassword);
            yPos += 25;

            _txtPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Width = panel.Width - 40,
                Height = 32,
                Font = new Font("Segoe UI", 10F),
                UseSystemPasswordChar = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(_txtPassword);
            yPos += 45;

            // Ad Soyad
            var lblFullName = new Label
            {
                Text = "Ad Soyad:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lblFullName);
            yPos += 25;

            _txtFullName = new TextBox
            {
                Location = new Point(20, yPos),
                Width = panel.Width - 40,
                Height = 32,
                Font = new Font("Segoe UI", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(_txtFullName);
            yPos += 45;

            // Admin CheckBox
            _chkIsAdmin = new CheckBox
            {
                Text = "Yönetici (Tüm izinlere sahip)",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            _chkIsAdmin.CheckedChanged += ChkIsAdmin_CheckedChanged;
            panel.Controls.Add(_chkIsAdmin);
            yPos += 40;

            // İzinler
            var lblPermissions = new Label
            {
                Text = "İzinler:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lblPermissions);
            yPos += 25;

            _clbPermissions = new CheckedListBox
            {
                Location = new Point(20, yPos),
                Width = panel.Width - 40,
                Height = 200,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // İzin isimlerini ekle
            var permissionNames = new Dictionary<string, string>
            {
                { "OrderEntry", "📝 Sipariş Girişi" },
                { "StockEntry", "📦 Stok Girişi" },
                { "Accounting", "💰 Muhasebe" },
                { "StockManagement", "📦 Stok Yönetimi" },
                { "ProductionPlanning", "🏭 Üretim Planlama" },
                { "CuttingRequests", "📋 Kesim Talepleri" },
                { "PressingRequests", "📋 Pres Talepleri" },
                { "ClampingRequests", "📋 Kenetleme Talepleri" },
                { "Clamping2Requests", "📋 Kenetleme 2 Talepleri" },
                { "AssemblyRequests", "📋 Montaj Talepleri" },
                { "PackagingRequests", "📦 Paketleme Talepleri" },
                { "Consumption", "⚡ Sarfiyat" },
                { "ConsumptionMaterialStock", "📦 Sarfiyat Malzeme Stok" },
                { "Reports", "📈 Raporlar" },
                { "Settings", "⚙️ Ayarlar" }
            };

            foreach (var key in _allPermissionKeys)
            {
                var displayName = permissionNames.ContainsKey(key) ? permissionNames[key] : key;
                _clbPermissions.Items.Add(displayName, false);
            }

            panel.Controls.Add(_clbPermissions);
            yPos += 220;

            // Butonlar
            var buttonPanel = new Panel
            {
                Location = new Point(20, yPos),
                Width = panel.Width - 40,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnNew = new Button
            {
                Text = "➕ Yeni",
                Location = new Point(0, 5),
                Width = 100,
                Height = 40,
                BackColor = ThemeColors.Secondary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnNew, 4);
            _btnNew.Click += BtnNew_Click;
            buttonPanel.Controls.Add(_btnNew);

            _btnSave = new Button
            {
                Text = "💾 Kaydet",
                Location = new Point(110, 5),
                Width = 100,
                Height = 40,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnSave, 4);
            _btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(_btnSave);

            _btnDelete = new Button
            {
                Text = "🗑️ Sil",
                Location = new Point(220, 5),
                Width = 100,
                Height = 40,
                BackColor = ThemeColors.Error,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnDelete, 4);
            _btnDelete.Click += BtnDelete_Click;
            buttonPanel.Controls.Add(_btnDelete);

            panel.Controls.Add(buttonPanel);
        }

        private void LoadUsers()
        {
            try
            {
                var users = _userRepository.GetAll();
                _dgvUsers.DataSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcılar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvUsers.SelectedRows.Count > 0)
            {
                var selectedRow = _dgvUsers.SelectedRows[0];
                if (selectedRow.DataBoundItem is User user)
                {
                    LoadUserDetails(user);
                }
            }
        }

        private void LoadUserDetails(User user)
        {
            _selectedUser = user;
            _txtUsername.Text = user.Username;
            _txtPassword.Text = ""; // Şifreyi gösterme
            _txtFullName.Text = user.FullName;
            _chkIsAdmin.Checked = user.IsAdmin;

            // İzinleri yükle
            if (user.IsAdmin)
            {
                // Admin ise tüm izinleri işaretle
                for (int i = 0; i < _clbPermissions.Items.Count; i++)
                {
                    _clbPermissions.SetItemChecked(i, true);
                }
            }
            else
            {
                var userPermissions = _permissionRepository.GetPermissionKeysByUserId(user.Id);
                for (int i = 0; i < _clbPermissions.Items.Count; i++)
                {
                    var permissionKey = _allPermissionKeys[i];
                    _clbPermissions.SetItemChecked(i, userPermissions.Contains(permissionKey));
                }
            }

            UpdateFormEnabled();
        }

        private void ChkIsAdmin_CheckedChanged(object sender, EventArgs e)
        {
            if (_chkIsAdmin.Checked)
            {
                // Admin ise tüm izinleri işaretle ve devre dışı bırak
                for (int i = 0; i < _clbPermissions.Items.Count; i++)
                {
                    _clbPermissions.SetItemChecked(i, true);
                }
            }
            _clbPermissions.Enabled = !_chkIsAdmin.Checked;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            _selectedUser = null;
            _txtUsername.Text = "";
            _txtPassword.Text = "";
            _txtFullName.Text = "";
            _chkIsAdmin.Checked = false;
            for (int i = 0; i < _clbPermissions.Items.Count; i++)
            {
                _clbPermissions.SetItemChecked(i, false);
            }
            _clbPermissions.Enabled = true;
            _dgvUsers.ClearSelection();
            UpdateFormEnabled();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (string.IsNullOrWhiteSpace(_txtUsername.Text))
                {
                    MessageBox.Show("Lütfen kullanıcı adını giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtFullName.Text))
                {
                    MessageBox.Show("Lütfen ad soyad giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_selectedUser == null)
                {
                    // Yeni kullanıcı
                    if (string.IsNullOrWhiteSpace(_txtPassword.Text))
                    {
                        MessageBox.Show("Lütfen şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var newUser = new User
                    {
                        Username = _txtUsername.Text.Trim(),
                        PasswordHash = _txtPassword.Text, // Repository'de hash'lenecek
                        FullName = _txtFullName.Text.Trim(),
                        IsAdmin = _chkIsAdmin.Checked
                    };

                    _userRepository.Insert(newUser);

                    // İzinleri kaydet
                    try
                    {
                        if (newUser.IsAdmin)
                        {
                            // Admin ise tüm izinleri ekle
                            _permissionRepository.SetUserPermissions(newUser.Id, _allPermissionKeys);
                        }
                        else
                        {
                            var selectedPermissions = new List<string>();
                            for (int i = 0; i < _clbPermissions.Items.Count; i++)
                            {
                                if (_clbPermissions.GetItemChecked(i))
                                {
                                    selectedPermissions.Add(_allPermissionKeys[i]);
                                }
                            }
                            
                            if (selectedPermissions.Count > 0)
                            {
                                _permissionRepository.SetUserPermissions(newUser.Id, selectedPermissions);
                            }
                        }
                    }
                    catch (Exception permEx)
                    {
                        MessageBox.Show("Kullanıcı eklendi ancak izinler kaydedilirken hata oluştu: " + permEx.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    MessageBox.Show("Kullanıcı başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Güncelle
                    _selectedUser.Username = _txtUsername.Text.Trim();
                    _selectedUser.FullName = _txtFullName.Text.Trim();
                    _selectedUser.IsAdmin = _chkIsAdmin.Checked;

                    _userRepository.Update(_selectedUser);

                    // Şifre değiştirildiyse güncelle
                    if (!string.IsNullOrWhiteSpace(_txtPassword.Text))
                    {
                        _userRepository.UpdatePassword(_selectedUser.Id, _txtPassword.Text);
                    }

                    // İzinleri güncelle
                    if (!_selectedUser.IsAdmin)
                    {
                        var selectedPermissions = new List<string>();
                        for (int i = 0; i < _clbPermissions.Items.Count; i++)
                        {
                            if (_clbPermissions.GetItemChecked(i))
                            {
                                selectedPermissions.Add(_allPermissionKeys[i]);
                            }
                        }
                        _permissionRepository.SetUserPermissions(_selectedUser.Id, selectedPermissions);
                    }
                    else
                    {
                        // Admin ise tüm izinleri ekle
                        _permissionRepository.SetUserPermissions(_selectedUser.Id, _allPermissionKeys);
                    }

                    MessageBox.Show("Kullanıcı başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadUsers();
                BtnNew_Click(sender, e); // Formu temizle
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Lütfen silmek istediğiniz kullanıcıyı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"{_selectedUser.Username} kullanıcısını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _userRepository.Delete(_selectedUser.Id);
                    MessageBox.Show("Kullanıcı başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                    BtnNew_Click(sender, e); // Formu temizle
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kullanıcı silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateFormEnabled()
        {
            bool hasSelection = _selectedUser != null;
            _btnDelete.Enabled = hasSelection;
        }
    }
}

