using System.Collections.Generic;
using System.Linq;
using ERP.UI.Interfaces;
using ERP.UI.Models;

namespace ERP.UI.Services
{
    public class MenuService : IMenuProvider
    {
        private readonly List<MenuItem> _menuItems;

        public MenuService()
        {
            _menuItems = InitializeMenuItems();
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            return _menuItems.Where(x => x.IsVisible).OrderBy(x => x.Order);
        }

        private List<MenuItem> InitializeMenuItems()
        {
            var orderEntryItem = new MenuItem("📝 Sipariş Girişi", "OrderEntry", "📝", 2);
            orderEntryItem.AddSubMenuItem(new MenuItem("📋 Siparişleri Görüntüle", "OrderList", "📋", 1));
            orderEntryItem.AddSubMenuItem(new MenuItem("➕ Yeni Sipariş", "OrderCreate", "➕", 2));

            return new List<MenuItem>
            {
                new MenuItem("🏠 Ana Sayfa", "Home", "🏠", 1),
                orderEntryItem,
                new MenuItem("📦 Stok Yönetimi", "Stock", "📦", 3),
                new MenuItem("🏭 Üretim Planlama", "Production", "🏭", 4),
                new MenuItem("📊 Satış Yönetimi", "Sales", "📊", 5),
                new MenuItem("🛒 Satın Alma", "Purchase", "🛒", 6),
                new MenuItem("👥 Müşteriler", "Customers", "👥", 7),
                new MenuItem("🏢 Tedarikçiler", "Suppliers", "🏢", 8),
                new MenuItem("📈 Raporlar", "Reports", "📈", 9),
                new MenuItem("⚙️ Ayarlar", "Settings", "⚙️", 10)
            };
        }
    }
}

