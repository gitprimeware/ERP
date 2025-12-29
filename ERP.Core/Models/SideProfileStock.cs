using System;

namespace ERP.Core.Models
{
    public class SideProfileStock : BaseModel
    {
        public decimal Length { get; set; } // Metre cinsinden (örn: 6)
        public int InitialQuantity { get; set; } // Başlangıç adedi (örn: 500)
        public decimal UsedLength { get; set; } // Kullanılan toplam metre
        public decimal WastedLength { get; set; } // Hurda olarak işaretlenen toplam metre
        public decimal RemainingLength { get; set; } // Kalan toplam metre (InitialQuantity * Length - UsedLength - WastedLength)
        public DateTime EntryDate { get; set; } // Giriş tarihi
    }
}

