using System.Drawing;
using System.Windows.Forms;
using ERP.UI.UI;

namespace ERP.UI.Factories
{
    public static class ButtonFactory
    {
        public static Button CreateActionButton(string text, Color backColor, Color foreColor, int width = 120, int height = 40)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand
            };

            UIHelper.ApplyRoundedButton(button, 6);
            return button;
        }

        public static Button CreateSuccessButton(string text)
        {
            return CreateActionButton(text, ThemeColors.Success, Color.White);
        }

        public static Button CreateCancelButton(string text)
        {
            return CreateActionButton(text, ThemeColors.SurfaceDark, ThemeColors.TextPrimary);
        }
    }
}

