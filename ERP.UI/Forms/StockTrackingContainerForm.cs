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
            CreateStockTab(tabRulo, "RuloStokTakip", "");
            _tabControl.TabPages.Add(tabRulo);

            // Kesilmiş Stok Takip Tab
            var tabKesilmis = new TabPage("✂️ Kesilmiş Stok Takip");
            tabKesilmis.Padding = new Padding(0);
            tabKesilmis.BackColor = Color.White;
            tabKesilmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabKesilmis, "KesilmisStokTakip", "");
            _tabControl.TabPages.Add(tabKesilmis);

            // Preslenmiş Stok Takip Tab
            var tabPreslenmis = new TabPage("📦 Preslenmiş Stok Takip");
            tabPreslenmis.Padding = new Padding(0);
            tabPreslenmis.BackColor = Color.White;
            tabPreslenmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabPreslenmis, "PreslenmisStokTakip", "");
            _tabControl.TabPages.Add(tabPreslenmis);

            // Kenetlenmiş Stok Takip Tab
            var tabKenetlenmis = new TabPage("🔗 Kenetlenmiş Stok Takip");
            tabKenetlenmis.Padding = new Padding(0);
            tabKenetlenmis.BackColor = Color.White;
            tabKenetlenmis.UseVisualStyleBackColor = false;
            CreateStockTab(tabKenetlenmis, "KenetlenmisStokTakip", "");
            _tabControl.TabPages.Add(tabKenetlenmis);

            this.Controls.Add(_tabControl);
        }

        private void CreateStockTab(TabPage tab, string formName, string description)
        {
            // Form'u direkt tab'a ekle
            var control = _formResolver.ResolveForm(formName);
            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                tab.Controls.Add(control);
            }
        }
    }
}

