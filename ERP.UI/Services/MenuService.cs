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

            var productionItem = new MenuItem("🏭 Üretim Planlama", "Production", "🏭", 5);
            productionItem.AddSubMenuItem(new MenuItem("📐 Formül", "ProductionFormul", "📐", 1));
            productionItem.AddSubMenuItem(new MenuItem("📄 Rapor", "ProductionReport", "📄", 2));

            var consumptionItem = new MenuItem("⚡ Sarfiyat", "Consumption", "⚡", 6);

            var reportsItem = new MenuItem("📈 Raporlar", "Reports", "📈", 10);
            reportsItem.AddSubMenuItem(new MenuItem("📊 MRP Raporu", "MRPReport", "📊", 1));
            reportsItem.AddSubMenuItem(new MenuItem("🏢 Cari Raporu", "CustomerReport", "🏢", 2));
            reportsItem.AddSubMenuItem(new MenuItem("📅 Yıllık Rapor", "AnnualReport", "📅", 3));
            reportsItem.AddSubMenuItem(new MenuItem("📋 Genel Rapor", "GeneralReport", "📋", 4));

            return new List<MenuItem>
            {
                new MenuItem("🏠 Ana Sayfa", "Home", "🏠", 1),
                orderEntryItem,
                new MenuItem("💰 Muhasebe", "Accounting", "💰", 3),
                new MenuItem("📦 Stok Yönetimi", "Stock", "📦", 4),
                productionItem,
                consumptionItem,
                new MenuItem("📊 Satış Yönetimi", "Sales", "📊", 7),
                new MenuItem("🛒 Satın Alma", "Purchase", "🛒", 8),
                new MenuItem("👥 Müşteriler", "Customers", "👥", 9),
                new MenuItem("🏢 Tedarikçiler", "Suppliers", "🏢", 10),
                reportsItem,
                new MenuItem("⚙️ Ayarlar", "Settings", "⚙️", 11)
            };
        }
    }
}

