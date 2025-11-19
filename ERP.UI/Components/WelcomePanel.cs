using System.Drawing;
using System.Windows.Forms;
using ERP.UI.UI;

namespace ERP.UI.Components
{
    public class WelcomePanel : Panel
    {
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
                Text = "ERP/MRP Yönetim Sistemine hoş geldiniz. Sol menüden istediğiniz modüle erişebilirsiniz.",
                Font = new Font("Segoe UI", 12F),
                ForeColor = ThemeColors.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 70),
                MaximumSize = new Size(700, 0)
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
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var card1 = UIHelper.CreateCardPanel("📦 Stok Yönetimi", "Stok takibi ve envanter yönetimi", ThemeColors.Primary);
            var card2 = UIHelper.CreateCardPanel("🏭 Üretim", "Üretim planlama ve takibi", ThemeColors.Secondary);
            var card3 = UIHelper.CreateCardPanel("📊 Satış", "Satış işlemleri ve raporlama", ThemeColors.Accent);
            var card4 = UIHelper.CreateCardPanel("📈 Raporlar", "Detaylı analiz ve raporlar", ThemeColors.Info);

            cardsPanel.Controls.Add(card1);
            cardsPanel.Controls.Add(card2);
            cardsPanel.Controls.Add(card3);
            cardsPanel.Controls.Add(card4);

            return cardsPanel;
        }
    }
}

