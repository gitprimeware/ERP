using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.UI;
using ERP.UI.Services;
using System.Reflection;
using ERP.UI.Utilities;

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

            // Kullanıcı Yönetimi Tab (İlk açılan)
            var tabUserManagement = new TabPage("👥 Kullanıcı Yönetimi");
            tabUserManagement.Padding = new Padding(0);
            tabUserManagement.BackColor = Color.White;
            tabUserManagement.UseVisualStyleBackColor = false;
            CreateUserManagementTab(tabUserManagement);
            _tabControl.TabPages.Add(tabUserManagement);

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

            // İlk açılan tab'ı Kullanıcı Yönetimi yap
            _tabControl.SelectedIndex = 0;

            this.Controls.Add(_tabControl);
        }

        private void CreateAboutTab(TabPage tab)
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var mainPanel = new Panel
            {
                AutoSize = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Width = scrollPanel.Width
            };
            scrollPanel.Controls.Add(mainPanel);

            // Hero Section - Gradient arka plan
            var heroPanel = new Panel
            {
                Height = 180,
                Width = mainPanel.Width,
                Dock = DockStyle.Top,
                BackColor = ThemeColors.Primary,
                Padding = new Padding(50, 0, 50, 0) // Sağdan ve soldan padding ekle
            };
            heroPanel.Paint += (s, e) =>
            {
                var rect = heroPanel.ClientRectangle;
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, 
                    ThemeColors.Primary, 
                    Color.FromArgb(
                        Math.Min(255, ThemeColors.Primary.R + 20),
                        Math.Min(255, ThemeColors.Primary.G + 20),
                        Math.Min(255, ThemeColors.Primary.B + 20)
                    ),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };
            mainPanel.Controls.Add(heroPanel);

            // Hero içeriği - Padding içinde
            var heroTitle = new Label
            {
                Text = AppInfo.FullTitle,
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(0, 40),
                BackColor = Color.Transparent,
                MaximumSize = new Size(heroPanel.Width - 100, 0) // Maksimum genişlik belirle
            };
            heroPanel.Controls.Add(heroTitle);

            var heroSubtitle = new Label
            {
                Text = AppInfo.Description,
                Font = new Font("Segoe UI", 13F),
                ForeColor = Color.FromArgb(240, 240, 240),
                AutoSize = true,
                Location = new Point(0, 85),
                BackColor = Color.Transparent,
                MaximumSize = new Size(heroPanel.Width - 100, 0) // Maksimum genişlik belirle
            };
            heroPanel.Controls.Add(heroSubtitle);
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            var versionLabel = new Label
            {
                Text = AppInfo.VersionString,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 220, 220),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(0, 110)
            };
            heroPanel.Controls.Add(versionLabel);

            // İçerik paneli
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(40, 30, 40, 40),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(contentPanel);

            int yPos = 180;

            // Program Açıklaması Kartı - Modern tasarım
            var descriptionCard = CreateModernCard(
                "📖 Hakkında",
                "Bu ERP (Kurumsal Kaynak Planlama) yönetim sistemi, üretim süreçlerinizi optimize etmek ve stok takibini " +
                "kolaylaştırmak için tasarlanmıştır. Sistem, sipariş yönetiminden üretim planlamasına, " +
                "stok takibinden muhasebe işlemlerine kadar geniş bir yelpazede hizmet sunmaktadır.",
                0, yPos, contentPanel.Width, Color.FromArgb(52, 152, 219));
            contentPanel.Controls.Add(descriptionCard);
            yPos += descriptionCard.Height + 40;

            // Özellikler Başlığı - Emoji ve yazı yan yana
            var featuresTitlePanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(contentPanel.Width, 40),
                BackColor = Color.Transparent
            };
            
            var featuresEmoji = new Label
            {
                Text = "✨",
                Font = new Font("Segoe UI Emoji", 22F),
                AutoSize = true,
                Location = new Point(0, 8)
            };
            featuresTitlePanel.Controls.Add(featuresEmoji);
            
            var lblFeaturesTitle = new Label
            {
                Text = "Özellikler",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(featuresEmoji.Right + 8, 5)
            };
            featuresTitlePanel.Controls.Add(lblFeaturesTitle);
            contentPanel.Controls.Add(featuresTitlePanel);
            yPos += 50;

            // Özellikler Kartları - Modern grid
            var features = new[]
            {
                ("📝", "Sipariş Yönetimi", "Sipariş yönetimi ve takibi", Color.FromArgb(46, 204, 113)),
                ("🏭", "Üretim Planlama", "Üretim planlama ve kontrolü", Color.FromArgb(230, 126, 34)),
                ("📦", "Stok Takibi", "Stok takip ve yönetimi", Color.FromArgb(52, 152, 219)),
                ("💰", "Muhasebe", "Muhasebe entegrasyonu", Color.FromArgb(155, 89, 182)),
                ("📊", "Raporlama", "Raporlama ve analiz", Color.FromArgb(241, 196, 15)),
                ("✨", "Kullanıcı Arayüzü", "Kullanıcı dostu arayüz", Color.FromArgb(26, 188, 156)),
                ("⚡", "Gerçek Zamanlı", "Gerçek zamanlı veri güncelleme", Color.FromArgb(192, 57, 43))
            };

            int cardWidth = (contentPanel.Width - 20) / 3; // Üç sütun (padding'i hesaba kat)
            int currentX = 0;
            int cardHeight = 90; // Yatay düzen için daha düşük
            int startY = yPos;
            var featureCards = new List<Panel>();

            for (int i = 0; i < features.Length; i++)
            {
                var (emoji, title, desc, color) = features[i];
                var featureCard = CreateModernFeatureCard(emoji, title, desc, currentX, yPos, cardWidth - 10, cardHeight, color);
                contentPanel.Controls.Add(featureCard);
                featureCards.Add(featureCard);

                if ((i + 1) % 3 == 0)
                {
                    currentX = 0;
                    yPos += cardHeight + 20;
                }
                else
                {
                    currentX += cardWidth;
                }
            }

            if (features.Length % 3 != 0)
            {
                yPos += cardHeight + 20;
            }

            yPos += 30;
            // Resize event'leri
            scrollPanel.Resize += (s, e) =>
            {
                mainPanel.Width = scrollPanel.Width;
                heroPanel.Width = mainPanel.Width;
                heroPanel.Padding = new Padding(50, 0, 50, 0); // Resize'da da padding'i güncelle
                contentPanel.Width = mainPanel.Width;
                
                descriptionCard.Width = contentPanel.Width;
                
                // Hero içindeki label'ların maksimum genişliğini güncelle
                foreach (Control ctrl in heroPanel.Controls)
                {
                    if (ctrl is Label lbl && lbl.MaximumSize.Width > 0)
                    {
                        lbl.MaximumSize = new Size(heroPanel.Width - 100, 0);
                    }
                }
                
                cardWidth = (contentPanel.Width - 20) / 3;
                currentX = 0;
                int tempY = startY;
                for (int i = 0; i < featureCards.Count; i++)
                {
                    var card = featureCards[i];
                    if (card != null)
                    {
                        card.Location = new Point(currentX, tempY);
                        card.Width = cardWidth - 10;
                        if ((i + 1) % 3 == 0)
                        {
                            currentX = 0;
                            tempY += cardHeight + 20;
                        }
                        else
                        {
                            currentX += cardWidth;
                        }
                    }
                }
            };

            tab.Controls.Add(scrollPanel);
        }

        private void CreateContactTab(TabPage tab)
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var mainPanel = new Panel
            {
                AutoSize = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Width = scrollPanel.Width
            };
            scrollPanel.Controls.Add(mainPanel);

            // Hero Section
            var heroPanel = new Panel
            {
                Height = 160,
                Width = mainPanel.Width,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(50, 0, 50, 0) // Sağdan ve soldan padding ekle
            };
            heroPanel.Paint += (s, e) =>
            {
                var rect = heroPanel.ClientRectangle;
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect,
                    Color.FromArgb(52, 152, 219),
                    Color.FromArgb(41, 128, 185),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };
            mainPanel.Controls.Add(heroPanel);

            var heroTitle = new Label
            {
                Text = "📧 İletişim",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(0, 30),
                MaximumSize = new Size(heroPanel.Width - 100, 0) // Maksimum genişlik belirle
            };
            heroPanel.Controls.Add(heroTitle);

            var heroSubtitle = new Label
            {
                Text = "Bizimle iletişime geçin, size yardımcı olmaktan mutluluk duyarız",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(240, 240, 240),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(0, 75),
                MaximumSize = new Size(heroPanel.Width - 100, 0) // Maksimum genişlik belirle
            };
            heroPanel.Controls.Add(heroSubtitle);

            // İçerik paneli
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(20, 30, 20, 40),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(contentPanel);

            int yPos = 160;

            // Şirket Bilgileri Kartı
            var companyCard = CreateModernCard(
                "🏢 Primeware Mühendislik Yazılımları",
                "Web & Masaüstü Mühendislik Yazılımları\n" +
                "HVAC Ürünleri Seçim ve Tasarım Programları\n" +
                "BIM Entegrasyonu ve 3D Modelleme\n\n" +
                "Daha fazla bilgi için web sitemizi ziyaret edebilir veya bizimle iletişime geçebilirsiniz.",
                0, yPos, contentPanel.Width, Color.FromArgb(46, 204, 113));
            contentPanel.Controls.Add(companyCard);
            yPos += companyCard.Height + 40;

            // İletişim Bilgileri Başlığı - Emoji ve yazı yan yana
            var contactInfoTitlePanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(contentPanel.Width, 40),
                BackColor = Color.Transparent
            };
            
            // Emoji genişliğini önceden hesapla
            var contactInfoEmojiFont = new Font("Segoe UI Emoji", 22F);
            var contactInfoEmojiSize = TextRenderer.MeasureText("📞", contactInfoEmojiFont);
            var contactInfoEmojiX = 0;
            var contactInfoEmojiY = 8;
            
            var contactInfoEmoji = new Label
            {
                Text = "📞",
                Font = contactInfoEmojiFont,
                AutoSize = true,
                Location = new Point(contactInfoEmojiX, contactInfoEmojiY)
            };
            contactInfoTitlePanel.Controls.Add(contactInfoEmoji);
            
            // Yazı - Emoji'nin sağında, dinamik konumlandırma
            var contactInfoTitleX = contactInfoEmojiX + contactInfoEmojiSize.Width + 12; // Emoji'nin sağından 12 piksel boşluk
            var contactInfoTitle = new Label
            {
                Text = "İletişim Bilgileri",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(contactInfoTitleX, 5)
            };
            contactInfoTitlePanel.Controls.Add(contactInfoTitle);
            contentPanel.Controls.Add(contactInfoTitlePanel);
            yPos += 50;

            // İletişim Bilgileri Grid
            var contactCards = new[]
            {
                ("🌐", "Web Sitesi", "https://www.primeware.com.tr", Color.FromArgb(52, 152, 219)),
                ("📧", "E-posta", "primeware@outlook.com.tr", Color.FromArgb(230, 126, 34)),
                ("📞", "Telefon", "+90 (533) 655 92 87", Color.FromArgb(155, 89, 182))
            };

            int contactCardWidth = contentPanel.Width / 3; // Üç sütun, tam genişlik kullan
            int contactX = 0;
            int contactStartY = yPos;

            foreach (var (emoji, title, info, color) in contactCards)
            {
                var contactCard = CreateContactInfoCard(emoji, title, info, contactX, yPos, contactCardWidth, 120, color);
                contentPanel.Controls.Add(contactCard);
                contactX += contactCardWidth;
            }

            yPos += 150; // Kart yüksekliği azaltıldı, boşluk da azaltıldı

            // Web Sitesine Git Butonu
            var btnVisitWebsite = new Button
            {
                Text = "🌐 Web Sitesine Git",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Size = new Size(280, 50),
                Location = new Point((contentPanel.Width - 280) / 2, yPos),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnVisitWebsite.FlatAppearance.BorderSize = 0;
            UIHelper.ApplyRoundedButton(btnVisitWebsite, 10);
            btnVisitWebsite.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.primeware.com.tr",
                    UseShellExecute = true
                });
            };
            contentPanel.Controls.Add(btnVisitWebsite);

            // Resize event'leri
            scrollPanel.Resize += (s, e) =>
            {
                mainPanel.Width = scrollPanel.Width;
                heroPanel.Width = mainPanel.Width;
                heroPanel.Padding = new Padding(50, 0, 50, 0); // Resize'da da padding'i güncelle
                contentPanel.Width = mainPanel.Width;
                
                companyCard.Width = contentPanel.Width;
                
                // Hero içindeki label'ların maksimum genişliğini güncelle
                foreach (Control ctrl in heroPanel.Controls)
                {
                    if (ctrl is Label lbl && lbl.MaximumSize.Width > 0)
                    {
                        lbl.MaximumSize = new Size(heroPanel.Width - 100, 0);
                    }
                }
                
                contactCardWidth = contentPanel.Width / 3;
                contactX = 0;
                foreach (Control control in contentPanel.Controls)
                {
                    if (control is Panel card && card.Tag?.ToString()?.StartsWith("contact_") == true)
                    {
                        card.Location = new Point(contactX, contactStartY);
                        card.Width = contactCardWidth;
                        contactX += contactCardWidth;
                    }
                }
                btnVisitWebsite.Left = (contentPanel.Width - btnVisitWebsite.Width) / 2;
            };

            tab.Controls.Add(scrollPanel);
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

        private Panel CreateModernCard(string title, string content, int x, int y, int width, Color accentColor)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 200),
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            UIHelper.ApplyCardStyle(card, 12);

            // Sol tarafta renkli çizgi
            var accentLine = new Panel
            {
                Width = 5,
                Height = card.Height,
                BackColor = accentColor,
                Location = new Point(0, 0),
                Dock = DockStyle.Left
            };
            card.Controls.Add(accentLine);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(30, 25)
            };
            card.Controls.Add(titleLabel);

            // Label genişliğini doğru hesapla: 
            // Kart genişliği (width) - sol padding (25) - çizgi (5) - sağ padding (25) = width - 55
            // Ama label X konumu 30 olduğu için (25 padding + 5 çizgi), sağ taraftan da 25 padding var
            // Yani label genişliği: width - 30 (sol) - 25 (sağ) = width - 55
            var labelWidth = width - 55;
            
            var contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = false,
                Location = new Point(30, 60),
                Width = labelWidth,
                Height = 200,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            card.Controls.Add(contentLabel);
            
            // İçeriğe göre yüksekliği ayarla
            using (var g = card.CreateGraphics())
            {
                var textSize = g.MeasureString(content, contentLabel.Font, labelWidth);
                var neededHeight = (int)textSize.Height + 100;
                card.Height = Math.Max(200, neededHeight);
                contentLabel.Height = card.Height - 80;
            }

            return card;
        }

        private Panel CreateModernFeatureCard(string emoji, string title, string description, int x, int y, int width, int height, Color accentColor)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(0),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyCardStyle(card, 10);

            // Hover efekti
            var originalBackColor = card.BackColor;
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 250, 250);
            card.MouseLeave += (s, e) => card.BackColor = originalBackColor;

            // Üstte renkli çizgi
            var accentLine = new Panel
            {
                Width = card.Width,
                Height = 4,
                BackColor = accentColor,
                Location = new Point(0, 0),
                Dock = DockStyle.Top
            };
            card.Controls.Add(accentLine);

            // Emoji genişliğini önceden hesapla
            var emojiFont = new Font("Segoe UI Emoji", 24F);
            var emojiSize = TextRenderer.MeasureText(emoji, emojiFont);
            var emojiX = 12;
            var emojiY = (height - emojiSize.Height) / 2;
            
            // Emoji - Solda
            var emojiLabel = new Label
            {
                Text = emoji,
                Font = emojiFont,
                AutoSize = true,
                Location = new Point(emojiX, emojiY),
                BackColor = Color.Transparent,
            };
            card.Controls.Add(emojiLabel);
            
            // Yazılar - Emoji'nin sağında, dinamik konumlandırma
            var textX = emojiX + emojiSize.Width + 12; // Emoji'nin sağından 12 piksel boşluk
            var textY = (height - 40) / 2;
            
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoSize = true,
                Location = new Point(textX, textY),
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            card.Controls.Add(titleLabel);

            var descLabel = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new Point(textX, textY + 20),
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            card.Controls.Add(descLabel);

            return card;
        }

        private Panel CreateContactInfoCard(string emoji, string title, string info, int x, int y, int width, int height, Color accentColor)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(15),
                Cursor = Cursors.Hand,
                Tag = "contact_" + title
            };
            UIHelper.ApplyCardStyle(card, 10);

            // Hover efekti
            var originalBackColor = card.BackColor;
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(250, 250, 250);
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = originalBackColor;
            };

            // Üstte renkli çizgi
            var accentLine = new Panel
            {
                Width = card.Width,
                Height = 4,
                BackColor = accentColor,
                Location = new Point(0, 0),
                Dock = DockStyle.Top
            };
            card.Controls.Add(accentLine);

            // Emoji genişliğini önceden hesapla
            var emojiFont = new Font("Segoe UI Emoji", 32F); // Biraz küçült
            var emojiSize = TextRenderer.MeasureText(emoji, emojiFont);
            var emojiX = 15;
            var emojiY = (height - emojiSize.Height) / 2; // Dikey ortalama
            
            // Emoji - Solda
            var emojiLabel = new Label
            {
                Text = emoji,
                Font = emojiFont,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(emojiX, emojiY)
            };
            card.Controls.Add(emojiLabel);

            // Yazılar - Emoji'nin sağında, dinamik konumlandırma
            var textX = emojiX + emojiSize.Width + 15; // Emoji'nin sağından 15 piksel boşluk
            var textY = (height - 50) / 2; // Dikey ortalama için

            //Başlık - Solda, emoji'nin yanında
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoSize = true,
                Location = new Point(textX, textY),
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            card.Controls.Add(titleLabel);
            // Başlık yüksekliğini ölç
            using (var g = card.CreateGraphics())
            {
                var titleSize = TextRenderer.MeasureText(g, title, titleLabel.Font, new Size(titleLabel.Width, 0), TextFormatFlags.WordBreak | TextFormatFlags.Top | TextFormatFlags.Left);
                titleLabel.Height = Math.Max(25, titleSize.Height + 5);
            }
            card.Controls.Add(titleLabel);

            // Bilgi - Başlığın altında, solda hizalı
            var infoY = titleLabel.Bottom + 5;
            Control infoControl;
            if (info.StartsWith("http"))
            {
                var infoLabel = new Label
                {
                    Text = info,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = accentColor,
                    AutoSize = true,
                    Location = new Point(textX, infoY),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.TopLeft,
                    UseMnemonic = false
                };
                infoLabel.Click += (s, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = info,
                        UseShellExecute = true
                    });
                };
                infoControl = infoLabel;
            }
            else
            {
                infoControl = new Label
                {
                    Text = info,
                    Font = new Font("Segoe UI", 10.5F),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    AutoSize = true,
                    Location = new Point(textX, infoY),
                    //Width = width - textX - 15, // Emoji ile aynı hizada, sağdan 15 piksel boşluk
                    //Height = height - infoY - 10,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.TopLeft,
                    UseMnemonic = false
                };
            }
            card.Controls.Add(infoControl);
            
            // İçeriğe göre kart yüksekliğini ayarla
            var totalContentHeight = infoControl.Bottom + 10;
            if (totalContentHeight > height)
            {
                card.Height = totalContentHeight;
                infoControl.Height = card.Height - infoY - 10;
            }

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
