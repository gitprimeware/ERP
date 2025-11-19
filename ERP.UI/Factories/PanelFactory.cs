using System.Drawing;
using System.Windows.Forms;
using ERP.UI.UI;

namespace ERP.UI.Factories
{
    public static class PanelFactory
    {
        public static Panel CreateTransparentPanel(AnchorStyles anchor = AnchorStyles.None)
        {
            return new Panel
            {
                BackColor = Color.Transparent,
                Anchor = anchor
            };
        }

        public static Panel CreateCardPanel(Color backgroundColor, int padding = 30)
        {
            var panel = new Panel
            {
                BackColor = backgroundColor,
                Padding = new Padding(padding)
            };

            UIHelper.ApplyCardStyle(panel, 12);
            return panel;
        }
    }
}

