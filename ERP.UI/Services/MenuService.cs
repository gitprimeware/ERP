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

            var stockEntryItem = new MenuItem("📦 Stok Girişi", "StockEntry", "📦", 2);

            var productionItem = new MenuItem("🏭 Üretim Planlama", "Production", "🏭", 5);
            productionItem.AddSubMenuItem(new MenuItem("📦 Stok Takip", "StockTracking", "📦", 1));
            productionItem.AddSubMenuItem(new MenuItem("📋 Üretim Ayrıntı", "Production", "📋", 2));

            var consumptionItem = new MenuItem("⚡ Sarfiyat", "Consumption", "⚡", 6);

            var stockItem = new MenuItem("📦 Stok Yönetimi", "Stock", "📦", 4);
            stockItem.AddSubMenuItem(new MenuItem("📥 Malzeme Giriş", "MaterialEntry", "📥", 1));
            stockItem.AddSubMenuItem(new MenuItem("📤 Malzeme Çıkış", "MaterialExit", "📤", 2));
            stockItem.AddSubMenuItem(new MenuItem("📊 Stok Ayrıntı", "StockDetail", "📊", 3));

            var reportsItem = new MenuItem("📈 Raporlar", "Reports", "📈", 10);
            reportsItem.AddSubMenuItem(new MenuItem("📊 Üretim Raporu", "MRPReport", "📊", 1));
            reportsItem.AddSubMenuItem(new MenuItem("🏢 Cari Raporu", "CustomerReport", "🏢", 2));
            reportsItem.AddSubMenuItem(new MenuItem("📅 Yıllık Rapor", "AnnualReport", "📅", 3));
            reportsItem.AddSubMenuItem(new MenuItem("📋 Genel Rapor", "GeneralReport", "📋", 4));

            return new List<MenuItem>
            {
                new MenuItem("🏠 Ana Sayfa", "Home", "🏠", 1),
                orderEntryItem,
                stockEntryItem,
                new MenuItem("💰 Muhasebe", "Accounting", "💰", 3),
                stockItem,
                productionItem,
                new MenuItem("📋 Kesim Talepleri", "CuttingRequests", "📋", 5),
                new MenuItem("📋 Pres Talepleri", "PressingRequests", "📋", 6),
                new MenuItem("📋 Kenetleme Talepleri", "ClampingRequests", "📋", 7),
                new MenuItem("📋 Kenetleme 2 Talepleri", "Clamping2Requests", "📋", 8),
                new MenuItem("📋 Montaj Talepleri", "AssemblyRequests", "📋", 9),
                consumptionItem,
                reportsItem,
                new MenuItem("⚙️ Ayarlar", "Settings", "⚙️", 11)
            };
        }
    }
}

