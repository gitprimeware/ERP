using System;

namespace ERP.Core.Models
{
    public class IsolationStock : BaseModel
    {
        public string LiquidType { get; set; } // "İzosiyanat", "Poliol", "MS Silikon"
        public decimal Kilogram { get; set; } // Kilogram cinsinden miktar
        public int Quantity { get; set; } // Geriye uyumluluk için (deprecated, Kilogram kullanılmalı)
        public decimal Liter { get; set; } // Geriye uyumluluk için (deprecated, Kilogram kullanılmalı)
        public DateTime EntryDate { get; set; } // Giriş tarihi
    }
}

