using System.Collections.Generic;

namespace ERP.UI.Models
{
    public class MenuItem
    {
        public string Text { get; set; }
        public string Tag { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public List<MenuItem> SubMenuItems { get; set; }
        public bool HasSubMenu => SubMenuItems != null && SubMenuItems.Count > 0;

        public MenuItem(string text, string tag, string icon = "", int order = 0)
        {
            Text = text;
            Tag = tag;
            Icon = icon;
            Order = order;
            SubMenuItems = new List<MenuItem>();
        }

        public void AddSubMenuItem(MenuItem subMenuItem)
        {
            SubMenuItems.Add(subMenuItem);
        }
    }
}

