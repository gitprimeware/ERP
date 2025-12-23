using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Components
{
    public class WelcomePanel : Panel
    {
        public event EventHandler<string> CardClicked;

        public WelcomePanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(20);

            var title = new Label
            {
                Text = "Hoş Geldiniz!",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var subtitle = new Label
            {
                Text = "ERP Yönetim Sistemine hoş geldiniz. Aşağıdaki kartlardan istediğiniz modüle erişebilirsiniz.",
                Font = new Font("Segoe UI", 12F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 70),
                MaximumSize = new Size(900, 0)
            };

            // Kartlar
            var cardsPanel = CreateCardsPanel();

            this.Controls.Add(title);
            this.Controls.Add(subtitle);
            this.Controls.Add(cardsPanel);
        }

        private FlowLayoutPanel CreateCardsPanel()
        {
            var cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 120),
                Width = this.Width - 40,
                Height = this.Height - 140,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Padding = new Padding(5)
            };

            // Kart tanımlamaları
            var cards = new[]
            {
                new { Title = "📝 Sipariş Giriş", Description = "Yeni sipariş oluşturma ve giriş işlemleri", Color = Color.FromArgb(52, 152, 219), Tag = "OrderCreate" },
                new { Title = "📋 Sipariş Takip", Description = "Mevcut siparişleri görüntüleme ve takip", Color = Color.FromArgb(41, 128, 185), Tag = "OrderList" },
                new { Title = "📦 Stok Giriş", Description = "Stok girişi ve takip işlemleri", Color = Color.FromArgb(46, 204, 113), Tag = "StockEntry" },
                new { Title = "💰 Muhasebe", Description = "Muhasebe ve finansal işlemler", Color = Color.FromArgb(241, 196, 15), Tag = "Accounting" },
                new { Title = "📊 Stok Ayrıntı", Description = "Detaylı stok bilgileri ve raporları", Color = Color.FromArgb(155, 89, 182), Tag = "StockDetail" },
                new { Title = "🏭 Üretim Ayrıntı", Description = "Üretim planlama ve takip işlemleri", Color = Color.FromArgb(231, 76, 60), Tag = "Production" },
                new { Title = "📦 Rulo Stok Takip", Description = "Rulo stok durumu ve takip işlemleri", Color = Color.FromArgb(52, 73, 94), Tag = "RuloStokTakip" },
                new { Title = "✂️ Kesilmiş Stok Takip", Description = "Kesilmiş stok durumu ve takip işlemleri", Color = Color.FromArgb(230, 126, 34), Tag = "KesilmisStokTakip" },
                new { Title = "📦 Preslenmiş Stok Takip", Description = "Preslenmiş stok durumu ve takip işlemleri", Color = Color.FromArgb(26, 188, 156), Tag = "PreslenmisStokTakip" },
                new { Title = "🔗 Kenetlenmiş Stok Takip", Description = "Kenetlenmiş stok durumu ve takip işlemleri", Color = Color.FromArgb(142, 68, 173), Tag = "KenetlenmisStokTakip" },
                new { Title = "⚡ Sarfiyat", Description = "Sarfiyat takip ve yönetimi", Color = Color.FromArgb(192, 57, 43), Tag = "Consumption" },
                new { Title = "📋 Kesim Talepleri", Description = "Kesim talep ve takip işlemleri", Color = Color.FromArgb(52, 73, 94), Tag = "CuttingRequests" },
                new { Title = "📋 Pres Talepleri", Description = "Pres talep ve takip işlemleri", Color = Color.FromArgb(230, 126, 34), Tag = "PressingRequests" },
                new { Title = "📋 Kenetleme Talepleri", Description = "Kenetleme talep ve takip işlemleri", Color = Color.FromArgb(26, 188, 156), Tag = "ClampingRequests" },
                new { Title = "📋 Kenetleme 2 Talepleri", Description = "Kenetleme 2 talep ve takip işlemleri", Color = Color.FromArgb(142, 68, 173), Tag = "Clamping2Requests" },
                new { Title = "📋 Montaj Talepleri", Description = "Montaj talep ve takip işlemleri", Color = Color.FromArgb(39, 174, 96), Tag = "AssemblyRequests" },
                new { Title = "📊 Üretim Raporu", Description = "Üretim raporları ve analiz işlemleri", Color = Color.FromArgb(41, 128, 185), Tag = "MRPReport" },
                new { Title = "🏢 Cari Raporu", Description = "Cari hesap raporları ve analiz işlemleri", Color = Color.FromArgb(52, 152, 219), Tag = "CustomerReport" },
                new { Title = "📅 Yıllık Rapor", Description = "Yıllık raporlar ve analiz işlemleri", Color = Color.FromArgb(155, 89, 182), Tag = "AnnualReport" }
            };

            foreach (var cardInfo in cards)
            {
                var card = CreateClickableCard(cardInfo.Title, cardInfo.Description, cardInfo.Color, cardInfo.Tag);
                card.Margin = new Padding(5);
                cardsPanel.Controls.Add(card);
            }

            // Malzeme Giriş/Çıkış (Yarı yarıya bölünmüş özel kart)
            var materialCard = CreateMaterialEntryExitCard();
            materialCard.Margin = new Padding(5);
            cardsPanel.Controls.Add(materialCard);

            this.Resize += (s, e) =>
            {
                cardsPanel.Width = this.Width - 40;
                cardsPanel.Height = this.Height - 140;
            };

            return cardsPanel;
        }

        private Panel CreateClickableCard(string title, string description, Color accentColor, string formTag)
        {
            var card = new Panel
            {
                Width = 240,  // 4 sığacak şekilde küçülttük (280 -> 240)
                Height = 140, // Yüksekliği de küçülttük
                BackColor = Color.White,
                Padding = new Padding(15),
                Cursor = Cursors.Hand
            };

            // Gölge olmadan, sadece border
            card.Paint += (s, e) =>
            {
                var pnl = s as Panel;
                if (pnl == null) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

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

            // Sol tarafta renkli çizgi
            var accentLine = new Panel
            {
                Width = 4,
                Height = card.Height,
                BackColor = accentColor,
                Location = new Point(0, 0),
                Dock = DockStyle.Left
            };
            card.Controls.Add(accentLine);

            // Başlık
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Location = new Point(19, 15),
                Width = card.Width - 39,
                Height = 25
            };
            card.Controls.Add(titleLabel);

            // Açıklama
            var descLabel = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = false,
                Location = new Point(19, 45),
                Width = card.Width - 39,
                Height = 70,
                TextAlign = ContentAlignment.TopLeft
            };
            card.Controls.Add(descLabel);

            // Tıklama olayı
            card.Click += (s, e) =>
            {
                CardClicked?.Invoke(this, formTag);
            };

            // Tüm child kontrollere de tıklama olayını ekle
            foreach (Control control in card.Controls)
            {
                control.Click += (s, e) =>
                {
                    CardClicked?.Invoke(this, formTag);
                };
                control.Cursor = Cursors.Hand;
            }

            return card;
        }

        private Panel CreateMaterialEntryExitCard()
        {
            var card = new Panel
            {
                Width = 240,
                Height = 140,
                BackColor = Color.White,
                Padding = new Padding(0),
                Cursor = Cursors.Hand
            };

            // Gölge olmadan, sadece border
            card.Paint += (s, e) =>
            {
                var pnl = s as Panel;
                if (pnl == null) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

            // Sol tarafta renkli çizgi (mor)
            var accentLine = new Panel
            {
                Width = 4,
                Height = card.Height,
                BackColor = Color.FromArgb(155, 89, 182),
                Location = new Point(0, 0),
                Dock = DockStyle.Left
            };
            card.Controls.Add(accentLine);

            // Başlık
            var titleLabel = new Label
            {
                Text = "📥 Malzeme Giriş/Çıkış",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Location = new Point(19, 15),
                Width = card.Width - 39,
                Height = 25
            };
            card.Controls.Add(titleLabel);

            // Yarı yarıya bölünmüş butonlar
            var entryButton = new Panel
            {
                Location = new Point(19, 50),
                Width = (card.Width - 39) / 2 - 5,
                Height = 60,
                BackColor = Color.FromArgb(46, 204, 113),
                Cursor = Cursors.Hand
            };
            entryButton.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(entryButton.BackColor))
                {
                    e.Graphics.FillRectangle(brush, entryButton.ClientRectangle);
                }
            };

            var entryLabel = new Label
            {
                Text = "📥 Giriş",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            entryButton.Controls.Add(entryLabel);
            entryButton.Click += (s, e) => CardClicked?.Invoke(this, "MaterialEntry");
            entryLabel.Click += (s, e) => CardClicked?.Invoke(this, "MaterialEntry");

            var exitButton = new Panel
            {
                Location = new Point(19 + (card.Width - 39) / 2 + 5, 50),
                Width = (card.Width - 39) / 2 - 5,
                Height = 60,
                BackColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            exitButton.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(exitButton.BackColor))
                {
                    e.Graphics.FillRectangle(brush, exitButton.ClientRectangle);
                }
            };

            var exitLabel = new Label
            {
                Text = "📤 Çıkış",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            exitButton.Controls.Add(exitLabel);
            exitButton.Click += (s, e) => CardClicked?.Invoke(this, "MaterialExit");
            exitLabel.Click += (s, e) => CardClicked?.Invoke(this, "MaterialExit");

            // Hover efekti
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(250, 250, 250);
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
            };

            entryButton.MouseEnter += (s, e) =>
            {
                entryButton.BackColor = Color.FromArgb(39, 174, 96);
                entryButton.Invalidate();
            };
            entryButton.MouseLeave += (s, e) =>
            {
                entryButton.BackColor = Color.FromArgb(46, 204, 113);
                entryButton.Invalidate();
            };

            exitButton.MouseEnter += (s, e) =>
            {
                exitButton.BackColor = Color.FromArgb(192, 57, 43);
                exitButton.Invalidate();
            };
            exitButton.MouseLeave += (s, e) =>
            {
                exitButton.BackColor = Color.FromArgb(231, 76, 60);
                exitButton.Invalidate();
            };

            card.Controls.Add(entryButton);
            card.Controls.Add(exitButton);

            return card;
        }
    }
}
