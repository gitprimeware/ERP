using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class StockTrackingContainerForm : UserControl
    {
        private TabControl _tabControl;
        private FormResolverService _formResolver;

        public StockTrackingContainerForm()
        {
            _formResolver = new FormResolverService();
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

            // Rulo Stok Takip Tab
            var tabRulo = new TabPage("📦 Rulo Stok Takip");
            tabRulo.Padding = new Padding(0);
            tabRulo.BackColor = Color.White;
            tabRulo.UseVisualStyleBackColor = false;
            CreateStockTab(tabRulo, "RuloStokTakip", 
                "Rulo Stok Takip modülü, ham rulo malzemelerin giriş-çıkış işlemlerini ve mevcut stok durumunu detaylı olarak takip eder. " +
                "Bu sayfada rulo seri numarası, malzeme tipi (Alüminyum, Galvaniz vb.), kalınlık ve ölçü bilgilerine göre gelişmiş filtreleme yapabilirsiniz. " +
                "Her rulo için giriş miktarı (kg), kesilen miktar (kg), hurda miktarı ve kalan miktar bilgileri görüntülenir. " +
                "Ayrıca her işlem için tarih, firma bilgisi ve işlem tipi (Giriş, Kesim vb.) detayları kayıt altında tutulur.");
            _tabControl.TabPages.Add(tabRulo);

            // Kesilmiş Stok Takip Tab
            var tabKesilmis = new TabPage("✂️ Kesilmiş Stok Takip");
            tabKesilmis.Padding = new Padding(0);
            tabKesilmis.BackColor = Color.White;
            tabKesilmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabKesilmis, "KesilmisStokTakip",
                "Kesilmiş Stok Takip modülü, kesim işlemi sonrası oluşan plakaların stok durumunu ve kullanım bilgilerini takip eder. " +
                "Bu sayfada sipariş numarası, hatve (H/D/M/L değerleri), ölçü ve rulo seri numarasına göre gelişmiş filtreleme yapabilirsiniz. " +
                "Kesilmiş plakaların miktarı, kalan miktarı, pres işlemine gönderilen miktarı ve hurda bilgileri detaylı olarak görüntülenir. " +
                "Her kesim işlemi için kesim tarihi, kullanılan makine, operatör bilgisi ve plaka adedi kayıt altında tutulur. " +
                "Ayrıca hangi plakaların pres işlemine gönderildiği ve hangilerinin henüz kullanılmadığı bilgisi de takip edilir.");
            _tabControl.TabPages.Add(tabKesilmis);

            // Preslenmiş Stok Takip Tab
            var tabPreslenmis = new TabPage("📦 Preslenmiş Stok Takip");
            tabPreslenmis.Padding = new Padding(0);
            tabPreslenmis.BackColor = Color.White;
            tabPreslenmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabPreslenmis, "PreslenmisStokTakip",
                "Preslenmiş Stok Takip modülü, pres işlemi sonrası oluşan plakaların stok durumunu, kullanım bilgilerini ve kenetleme işlemlerini takip eder. " +
                "Bu sayfada preslenmiş plakaların detaylı bilgilerini görüntüleyebilir, yeni kenetleme işlemleri ekleyebilir ve stok hareketlerini izleyebilirsiniz. " +
                "Her pres işlemi için pres tarihi, pres numarası, basınç değeri, pres adedi, hurda miktarı ve operatör bilgisi kayıt altında tutulur. " +
                "Preslenmiş plakaların hangi kenetleme işlemlerinde kullanıldığı ve kalan miktarı detaylı olarak görüntülenir. " +
                "Sistem, preslenmiş plakaların hangi siparişlere tahsis edildiğini ve stok durumunu gerçek zamanlı olarak günceller.");
            _tabControl.TabPages.Add(tabPreslenmis);

            // Kenetlenmiş Stok Takip Tab
            var tabKenetlenmis = new TabPage("🔗 Kenetlenmiş Stok Takip");
            tabKenetlenmis.Padding = new Padding(0);
            tabKenetlenmis.BackColor = Color.White;
            tabKenetlenmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabKenetlenmis, "KenetlenmisStokTakip",
                "Kenetlenmiş Stok Takip modülü, kenetleme işlemi sonrası oluşan ürünlerin stok durumunu, montaj işlemlerine gönderilen miktarları ve müşteri bilgilerini takip eder. " +
                "Bu sayfada sipariş numarası, hatve (H/D/M/L değerleri), ölçü, uzunluk, plaka kalınlığı ve müşteri bilgilerine göre gelişmiş filtreleme yapabilirsiniz. " +
                "Her kenetleme işlemi için kenetleme tarihi, sipariş bilgisi, hatve, ölçü, uzunluk, kenetleme adedi, kullanılan plaka sayısı ve operatör bilgisi kayıt altında tutulur. " +
                "Kenetlenmiş ürünlerin hangi montaj işlemlerinde kullanıldığı, kalan miktarı ve müşteriye tahsis durumu detaylı olarak görüntülenir. " +
                "Ayrıca kenetlenmiş ürünlerin birleştirme (Kenetleme 2) ve bölme işlemlerine uygunluğu da bu modül üzerinden kontrol edilebilir.");
            _tabControl.TabPages.Add(tabKenetlenmis);

            this.Controls.Add(_tabControl);
        }

        private void CreateStockTab(TabPage tab, string formName, string description)
        {
            // Ana panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            // Form container
            var formContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Açıklama paneli
            var descriptionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(20, 10, 20, 10)
            };

            var descriptionLabel = new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            descriptionPanel.Controls.Add(descriptionLabel);

            // Form'u yükle
            var control = _formResolver.ResolveForm(formName);
            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                formContainer.Controls.Add(control);
            }

            mainPanel.Controls.Add(formContainer);
            mainPanel.Controls.Add(descriptionPanel);
            formContainer.BringToFront();

            tab.Controls.Add(mainPanel);
        }
    }
}

