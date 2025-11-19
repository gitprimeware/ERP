using System;
using System.Collections.Generic;

namespace ERP.Core.Models
{
    public class Order : BaseModel
    {
        public Guid CompanyId { get; set; }
        public Company? Company { get; set; }
        
        public string CustomerOrderNo { get; set; } // Manuel girilen müşteri sipariş no
        public string TrexOrderNo { get; set; } // Otomatik üretilen SP-YYYY-XXXX formatı
        
        public string? DeviceName { get; set; } // Cihaz adı
        
        public DateTime OrderDate { get; set; } // Sipariş tarihi
        public DateTime TermDate { get; set; } // Termin tarihi
        
        public string? ProductCode { get; set; } // Ürün kodu
        public string? BypassSize { get; set; } // Bypass ölçüsü
        public string? BypassType { get; set; } // Bypass türü
        
        public decimal? LamelThickness { get; set; } // Lamel kalınlığı (0.10, 0.12, 0.15)
        public string? ProductType { get; set; } // Ürün türü (Normal, Epoksi, Boyalı)
        
        public int Quantity { get; set; }
        public decimal? SalesPrice { get; set; } // Readonly - başka ekranda girilecek
        public decimal TotalPrice { get; set; } // Hesaplanan
        
        public DateTime? ShipmentDate { get; set; } // Sevk tarihi
        public decimal? CurrencyRate { get; set; } // Kur - Readonly
        
        public string Status { get; set; } = "Yeni"; // Sipariş durumu (Yeni, Onaylandı, Üretimde, Tamamlandı, İptal)
        
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
