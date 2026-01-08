using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class EventFeedForm : UserControl
    {
        private Panel _mainPanel;
        private FlowLayoutPanel _eventsPanel;
        private EventFeedRepository _eventFeedRepository;
        private Button _btnRefresh;
        private System.Windows.Forms.Timer _refreshTimer;
        private Label _lblLastRefresh;

        public EventFeedForm()
        {
            _eventFeedRepository = new EventFeedRepository();
            InitializeCustomComponents();
            LoadEvents();
            
            // 15 saniyede bir otomatik yenile
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 15000; // 15 saniye
            _refreshTimer.Tick += (s, e) => 
            {
                LoadEvents();
                UpdateLastRefreshLabel();
            };
            _refreshTimer.Start();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Başlık
            var titleLabel = new Label
            {
                Text = "📋 Olay Akışı",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            _mainPanel.Controls.Add(titleLabel);

            // Yenile butonu
            _btnRefresh = new Button
            {
                Text = "🔄 Yenile",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ThemeColors.Primary,
                Size = new Size(100, 35),
                Location = new Point(30, 80),
                Cursor = Cursors.Hand
            };
            UIHelper.ApplyRoundedButton(_btnRefresh, 4);
            _btnRefresh.Click += (s, e) => 
            {
                LoadEvents();
                UpdateLastRefreshLabel();
            };
            _mainPanel.Controls.Add(_btnRefresh);

            // Son yenileme zamanı label'ı
            _lblLastRefresh = new Label
            {
                Text = "Son yenileme: " + DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(140, 88)
            };
            _mainPanel.Controls.Add(_lblLastRefresh);

            // Events panel
            _eventsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 130),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 160,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };
            _mainPanel.Controls.Add(_eventsPanel);

            // Resize event
            _mainPanel.Resize += (s, e) =>
            {
                _eventsPanel.Width = _mainPanel.Width - 60;
                _eventsPanel.Height = _mainPanel.Height - 160;
            };

            this.Controls.Add(_mainPanel);
        }

        private void LoadEvents()
        {
            try
            {
                if (!UserSessionService.IsLoggedIn)
                {
                    _eventsPanel.Controls.Clear();
                    var noUserLabel = new Label
                    {
                        Text = "Lütfen giriş yapın",
                        Font = new Font("Segoe UI", 12F),
                        ForeColor = ThemeColors.TextSecondary,
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };
                    _eventsPanel.Controls.Add(noUserLabel);
                    return;
                }

                var user = UserSessionService.CurrentUser;
                var events = _eventFeedRepository.GetByUserPermissions(user.Id, user.IsAdmin, limit: 100);

                _eventsPanel.Controls.Clear();

                if (events.Count == 0)
                {
                    var noEventsLabel = new Label
                    {
                        Text = "Henüz olay bulunmuyor",
                        Font = new Font("Segoe UI", 12F),
                        ForeColor = ThemeColors.TextSecondary,
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };
                    _eventsPanel.Controls.Add(noEventsLabel);
                    return;
                }

                foreach (var eventItem in events)
                {
                    var eventCard = CreateEventCard(eventItem);
                    eventCard.Margin = new Padding(0, 0, 0, 15);
                    _eventsPanel.Controls.Add(eventCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Olaylar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateEventCard(EventFeed eventItem)
        {
            var card = new Panel
            {
                Width = _eventsPanel.Width - 40,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                Cursor = Cursors.Hand
            };

            // Border
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

            // Sol tarafta renkli çizgi (event tipine göre)
            var accentColor = GetEventColor(eventItem.EventType);
            var accentLine = new Panel
            {
                Width = 4,
                Height = card.Height,
                BackColor = accentColor,
                Location = new Point(0, 0),
                Dock = DockStyle.Left
            };
            card.Controls.Add(accentLine);

            // Tarih - Sağ üste
            var dateText = eventItem.EventDate.ToString("dd.MM.yyyy HH:mm");
            var dateLabel = new Label
            {
                Text = dateText,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true
            };
            card.Controls.Add(dateLabel);
            // Tarih label'ının genişliğini hesapla ve konumunu ayarla
            using (var g = card.CreateGraphics())
            {
                var dateSize = TextRenderer.MeasureText(g, dateText, dateLabel.Font);
                dateLabel.Location = new Point(card.Width - dateSize.Width - 15, 15);
            }

            // Başlık - Tarih için yer bırakarak
            var titleLabel = new Label
            {
                Text = eventItem.Title,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = false,
                Location = new Point(19, 15),
                Width = card.Width - dateLabel.Width - 50,
                Height = 25
            };
            card.Controls.Add(titleLabel);

            // Mesaj
            var messageLabel = new Label
            {
                Text = eventItem.Message,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = false,
                Location = new Point(19, 45),
                Width = card.Width - 39,
                Height = 30
            };
            card.Controls.Add(messageLabel);

            // Kart yeniden boyutlandığında tarih konumunu ve label genişliklerini güncelle
            card.Resize += (s, e) =>
            {
                using (var g = card.CreateGraphics())
                {
                    var dateSize = TextRenderer.MeasureText(g, dateText, dateLabel.Font);
                    dateLabel.Location = new Point(card.Width - dateSize.Width - 15, 15);
                }
                titleLabel.Width = card.Width - dateLabel.Width - 50;
                messageLabel.Width = card.Width - 39;
            };

            // Tıklama olayı - ilgili sayfaya yönlendir
            card.Click += (s, e) => HandleEventClick(eventItem);

            // Tüm child kontrollere de tıklama olayını ekle
            foreach (Control control in card.Controls)
            {
                control.Click += (s, e) => HandleEventClick(eventItem);
                control.Cursor = Cursors.Hand;
            }

            return card;
        }

        private Color GetEventColor(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
                return ThemeColors.Primary;

            // Event tipine göre renk belirle
            return eventType switch
            {
                "OrderCreated" => Color.FromArgb(52, 152, 219),      // Mavi
                "OrderSentToProduction" => Color.FromArgb(46, 204, 113), // Yeşil
                "CuttingRequestCreated" => Color.FromArgb(230, 126, 34),  // Turuncu
                "CuttingCompleted" => Color.FromArgb(39, 174, 96),       // Yeşil
                "CuttingApproved" => Color.FromArgb(46, 204, 113),      // Açık Yeşil
                "PressingRequestCreated" => Color.FromArgb(155, 89, 182), // Mor
                "PressingCompleted" => Color.FromArgb(39, 174, 96),       // Yeşil
                "PressingApproved" => Color.FromArgb(46, 204, 113),      // Açık Yeşil
                "ClampingRequestCreated" => Color.FromArgb(26, 188, 156), // Turkuaz
                "ClampingCompleted" => Color.FromArgb(39, 174, 96),      // Yeşil
                "ClampingApproved" => Color.FromArgb(46, 204, 113),      // Açık Yeşil
                "Clamping2RequestCreated" => Color.FromArgb(142, 68, 173), // Mor
                "Clamping2Completed" => Color.FromArgb(39, 174, 96),      // Yeşil
                "Clamping2Approved" => Color.FromArgb(46, 204, 113),      // Açık Yeşil
                "AssemblyRequestCreated" => Color.FromArgb(241, 196, 15), // Sarı
                "AssemblyCompleted" => Color.FromArgb(39, 174, 96),       // Yeşil
                "AssemblyApproved" => Color.FromArgb(46, 204, 113),      // Açık Yeşil
                "OrderSentToAccounting" => Color.FromArgb(52, 73, 94),     // Koyu Gri
                "OrderReadyForShipment" => Color.FromArgb(192, 57, 43),  // Kırmızı
                "OrderShipped" => Color.FromArgb(39, 174, 96),            // Yeşil
                "IsolationCompleted" => Color.FromArgb(52, 152, 219),      // Mavi
                "PackagingCompleted" => Color.FromArgb(155, 89, 182),      // Mor
                "MaterialEntryCreated" => Color.FromArgb(46, 204, 113),    // Yeşil
                "CoverStockEntryCreated" => Color.FromArgb(241, 196, 15),  // Sarı
                "SideProfileStockEntryCreated" => Color.FromArgb(230, 126, 34), // Turuncu
                "IsolationStockEntryCreated" => Color.FromArgb(26, 188, 156),   // Turkuaz
                _ => ThemeColors.Primary
            };
        }

        private void HandleEventClick(EventFeed eventItem)
        {
            // İlgili entity'ye yönlendir
            if (eventItem.RelatedEntityId.HasValue && !string.IsNullOrEmpty(eventItem.RelatedEntityType))
            {
                // Bu event'i ContentManager'a bildir, o ilgili sayfayı açsın
                // Şimdilik sadece mesaj göster
                MessageBox.Show(
                    $"Olay: {eventItem.Title}\n\n" +
                    $"Mesaj: {eventItem.Message}\n\n" +
                    $"Tarih: {eventItem.EventDate:dd.MM.yyyy HH:mm}",
                    "Olay Detayı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void UpdateLastRefreshLabel()
        {
            if (_lblLastRefresh != null)
            {
                _lblLastRefresh.Text = "Son yenileme: " + DateTime.Now.ToString("HH:mm:ss");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

