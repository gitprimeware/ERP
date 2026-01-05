using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.UI;
using ERP.UI.Services;

namespace ERP.UI.Forms
{
    public partial class SettingsForm : UserControl
    {
        private TabControl _tabControl;

        public SettingsForm()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            CreateTabControl();
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(15, 5),
                Appearance = TabAppearance.FlatButtons
            };

            // Tab çizim modu
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.DrawItem += (s, e) =>
            {
                var tabPage = _tabControl.TabPages[e.Index];
                var tabRect = _tabControl.GetTabRect(e.Index);
                var textColor = e.Index == _tabControl.SelectedIndex ? ThemeColors.Primary : ThemeColors.TextSecondary;
                var backColor = e.Index == _tabControl.SelectedIndex ? Color.White : Color.FromArgb(245, 245, 245);

                // Arka plan
                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, tabRect);
                }

                // Metin
                using (var emojiFont = new Font("Segoe UI Emoji", 10F))
                {
                    TextRenderer.DrawText(e.Graphics, tabPage.Text, emojiFont, 
                        tabRect, textColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                
                e.DrawFocusRectangle();
            };

            // Program Hakkında Tab
            var tabAbout = new TabPage("ℹ️ Program Hakkında");
            tabAbout.Padding = new Padding(0);
            tabAbout.BackColor = Color.White;
            tabAbout.UseVisualStyleBackColor = false;
            CreateAboutTab(tabAbout);
            _tabControl.TabPages.Add(tabAbout);

            // İletişim Tab
            var tabContact = new TabPage("📧 İletişim");
            tabContact.Padding = new Padding(0);
            tabContact.BackColor = Color.White;
            tabContact.UseVisualStyleBackColor = false;
            CreateContactTab(tabContact);
            _tabControl.TabPages.Add(tabContact);

            // Kullanıcı Yönetimi Tab
            var tabUserManagement = new TabPage("👥 Kullanıcı Yönetimi");
            tabUserManagement.Padding = new Padding(0);
            tabUserManagement.BackColor = Color.White;
            tabUserManagement.UseVisualStyleBackColor = false;
            CreateUserManagementTab(tabUserManagement);
            _tabControl.TabPages.Add(tabUserManagement);

            this.Controls.Add(_tabControl);
        }

        private void CreateAboutTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            int yPos = 30;

            // Başlık
            var lblTitle = new Label
            {
                Text = "Program Hakkında",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 70;

            // Program Açıklaması Kartı
            var descriptionCard = CreateCardPanel("📖 Program Açıklaması", 
                "Bu ERP (Kurumsal Kaynak Planlama) yönetim sistemi, üretim süreçlerinizi optimize etmek ve stok takibini " +
                "kolaylaştırmak için tasarlanmıştır. Sistem, sipariş yönetiminden üretim planlamasına, " +
                "stok takibinden muhasebe işlemlerine kadar geniş bir yelpazede hizmet sunmaktadır.",
                30, yPos, mainPanel.Width - 60);
            mainPanel.Controls.Add(descriptionCard);
            yPos += descriptionCard.Height + 30;

            // Özellikler Başlığı
            var lblFeaturesTitle = new Label
            {
                Text = "Özellikler",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            mainPanel.Controls.Add(lblFeaturesTitle);
            yPos += 50;

            // Özellikler Kartları
            string[] features = new[]
            {
                "📝 Sipariş yönetimi ve takibi",
                "🏭 Üretim planlama ve kontrolü",
                "📦 Stok takip ve yönetimi",
                "💰 Muhasebe entegrasyonu",
                "📊 Raporlama ve analiz",
                "✨ Kullanıcı dostu arayüz",
                "⚡ Gerçek zamanlı veri güncelleme"
            };

            int cardWidth = (mainPanel.Width - 80) / 2; // İki sütun için
            int currentX = 30;
            int cardHeight = 100;
            var featureCards = new List<Panel>();

            for (int i = 0; i < features.Length; i++)
            {
                var featureCard = CreateFeatureCard(features[i], currentX, yPos, cardWidth - 20, cardHeight, i);
                mainPanel.Controls.Add(featureCard);
                featureCards.Add(featureCard);

                if (i % 2 == 0)
                {
                    currentX += cardWidth;
                }
                else
                {
                    currentX = 30;
                    yPos += cardHeight + 20;
                }
            }

            if (features.Length % 2 == 1)
            {
                yPos += cardHeight + 20;
            }

            yPos += 30;

            mainPanel.Resize += (s, e) =>
            {
                descriptionCard.Width = mainPanel.Width - 60;
                cardWidth = (mainPanel.Width - 80) / 2;
                currentX = 30;
                int tempY = lblFeaturesTitle.Bottom + 50;
                for (int i = 0; i < featureCards.Count; i++)
                {
                    var card = featureCards[i];
                    if (card != null)
                    {
                        card.Location = new Point(currentX, tempY);
                        card.Width = cardWidth - 20;
                        if (i % 2 == 0)
                        {
                            currentX += cardWidth;
                        }
                        else
                        {
                            currentX = 30;
                            tempY += cardHeight + 20;
                        }
                    }
                }
            };

            tab.Controls.Add(mainPanel);
        }

        private void CreateContactTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            int yPos = 30;

            // Başlık
            var lblTitle = new Label
            {
                Text = "İletişim",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 70;

            // Şirket Bilgileri Kartı
            var companyCard = CreateCardPanel("🏢 Primeware Mühendislik Yazılımları",
                "Web & Masaüstü Mühendislik Yazılımları\n" +
                "HVAC Ürünleri Seçim ve Tasarım Programları\n" +
                "BIM Entegrasyonu ve 3D Modelleme\n\n" +
                "Daha fazla bilgi için web sitemizi ziyaret edebilir veya iletişim formunu kullanabilirsiniz.",
                30, yPos, mainPanel.Width - 60);
            mainPanel.Controls.Add(companyCard);
            yPos += companyCard.Height + 40;

            // Web Sitesi Linki Kartı
            var websiteCard = CreateCardPanel("🌐 Web Sitesi",
                "Web sitemizi ziyaret ederek ürünlerimiz, hizmetlerimiz ve son gelişmeler hakkında daha fazla bilgi alabilirsiniz.",
                30, yPos, mainPanel.Width - 60);
            
            var linkWebsite = new LinkLabel
            {
                Text = "https://www.primeware.com.tr",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(50, websiteCard.Height - 80),
                AutoSize = true,
                LinkColor = ThemeColors.Primary,
                VisitedLinkColor = ThemeColors.Primary,
                ActiveLinkColor = ThemeColors.Primary
            };
            linkWebsite.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.primeware.com.tr",
                    UseShellExecute = true
                });
            };
            websiteCard.Controls.Add(linkWebsite);
            mainPanel.Controls.Add(websiteCard);
            yPos += websiteCard.Height + 30;

            // Web Sitesine Git Butonu
            var btnVisitWebsite = new Button
            {
                Text = "🌐 Web Sitesine Git",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Size = new Size(250, 45),
                Location = new Point((mainPanel.Width - 250) / 2, yPos),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnVisitWebsite.FlatAppearance.BorderSize = 0;
            UIHelper.ApplyRoundedButton(btnVisitWebsite, 8);
            btnVisitWebsite.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.primeware.com.tr",
                    UseShellExecute = true
                });
            };
            mainPanel.Controls.Add(btnVisitWebsite);

            mainPanel.Resize += (s, e) =>
            {
                companyCard.Width = mainPanel.Width - 60;
                websiteCard.Width = mainPanel.Width - 60;
                btnVisitWebsite.Left = (mainPanel.Width - btnVisitWebsite.Width) / 2;
            };

            tab.Controls.Add(mainPanel);
        }

        private Panel CreateCardPanel(string title, string content, int x, int y, int width)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 200),
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            UIHelper.ApplyCardStyle(card, 8);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            card.Controls.Add(titleLabel);

            var contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Location = new Point(20, 50),
                Size = new Size(width - 40, 130),
                TextAlign = ContentAlignment.TopLeft
            };
            card.Controls.Add(contentLabel);

            // İçeriğe göre yüksekliği ayarla
            using (var g = card.CreateGraphics())
            {
                var textSize = g.MeasureString(content, contentLabel.Font, width - 40);
                card.Height = Math.Max(200, (int)textSize.Height + 80);
                contentLabel.Height = card.Height - 70;
            }

            return card;
        }

        private Panel CreateFeatureCard(string feature, int x, int y, int width, int height, int index)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(20),
                Tag = $"feature_{index}"
            };
            UIHelper.ApplyCardStyle(card, 8);

            var featureLabel = new Label
            {
                Text = feature,
                Font = new Font("Segoe UI", 11F),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(featureLabel);

            return card;
        }

        private void CreateUserManagementTab(TabPage tab)
        {
            // UserManagementForm'u dinamik olarak yükle
            var formResolver = new Services.FormResolverService();
            var userManagementForm = formResolver.ResolveForm("UserManagement");
            tab.Controls.Add(userManagementForm);
        }
    }
}
