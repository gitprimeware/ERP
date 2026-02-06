using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;
using ERP.UI.Utilities;

namespace ERP.UI.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private Button _btnLogin;
        private Label _lblError;
        private UserRepository _userRepository;
        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            _userRepository = new UserRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"{AppInfo.Title} - Giriş";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;

            CreateLoginPanel();
        }

        private void CreateLoginPanel()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(50, 50, 50, 80) // Alt padding artırıldı
            };

            int yPos = 50;

            // Başlık
            var lblTitle = new Label
            {
                Text = AppInfo.FullTitle,
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point((mainPanel.Width - 250) / 2, yPos),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 80;

            // Alt başlık
            var lblSubtitle = new Label
            {
                Text = "Lütfen giriş yapın",
                Font = new Font("Segoe UI", 13F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point((mainPanel.Width - 150) / 2, yPos),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(lblSubtitle);
            yPos += 60;

            // Kullanıcı Adı
            var lblUsername = new Label
            {
                Text = "Kullanıcı Adı:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(50, yPos)
            };
            mainPanel.Controls.Add(lblUsername);
            yPos += 35;

            _txtUsername = new TextBox
            {
                Location = new Point(50, yPos),
                Width = mainPanel.Width - 100,
                Height = 40,
                Font = new Font("Segoe UI", 12F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            mainPanel.Controls.Add(_txtUsername);
            yPos += 55;

            // Şifre
            var lblPassword = new Label
            {
                Text = "Şifre:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(50, yPos)
            };
            mainPanel.Controls.Add(lblPassword);
            yPos += 35;

            _txtPassword = new TextBox
            {
                Location = new Point(50, yPos),
                Width = mainPanel.Width - 100,
                Height = 40,
                Font = new Font("Segoe UI", 12F),
                UseSystemPasswordChar = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _txtPassword.KeyDown += TxtPassword_KeyDown;
            mainPanel.Controls.Add(_txtPassword);
            yPos += 60;

            // Hata mesajı
            _lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.Error,
                AutoSize = true,
                Location = new Point(50, yPos),
                Height = 25,
                Visible = false
            };
            mainPanel.Controls.Add(_lblError);
            yPos += 40;

            // Giriş Butonu
            _btnLogin = new Button
            {
                Text = "🔓 Giriş Yap",
                Location = new Point(50, yPos),
                Width = mainPanel.Width - 100,
                Height = 50,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            UIHelper.ApplyRoundedButton(_btnLogin, 6);
            _btnLogin.Click += BtnLogin_Click;
            mainPanel.Controls.Add(_btnLogin);
            yPos += 60; // Butonun altında boşluk

            // Panel resize event
            mainPanel.Resize += (s, e) =>
            {
                lblTitle.Left = (mainPanel.Width - lblTitle.Width) / 2;
                lblSubtitle.Left = (mainPanel.Width - lblSubtitle.Width) / 2;
                _txtUsername.Width = mainPanel.Width - 100;
                _txtPassword.Width = mainPanel.Width - 100;
                _btnLogin.Width = mainPanel.Width - 100;
            };

            this.Controls.Add(mainPanel);
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;
            _lblError.Text = "";

            if (string.IsNullOrWhiteSpace(_txtUsername.Text))
            {
                ShowError("Lütfen kullanıcı adınızı giriniz.");
                _txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                ShowError("Lütfen şifrenizi giriniz.");
                _txtPassword.Focus();
                return;
            }

            try
            {
                var user = _userRepository.Authenticate(_txtUsername.Text.Trim(), _txtPassword.Text);
                if (user != null)
                {
                    LoggedInUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Kullanıcı adı veya şifre hatalı!");
                    _txtPassword.Text = "";
                    _txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError("Giriş yapılırken bir hata oluştu: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            _lblError.Text = message;
            _lblError.Visible = true;
        }
    }
}

