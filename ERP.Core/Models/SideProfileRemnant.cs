using System;

namespace ERP.Core.Models
{
    public class SideProfileRemnant : BaseModel
    {
        public decimal Length { get; set; } // Metre cinsinden kalan parça uzunluğu (örn: 1, 5)
        public int Quantity { get; set; } // Bu uzunluktan kaç adet var (örn: 2 tane 1 metrelik)
        public bool IsWaste { get; set; } // Hurda olarak işaretlendi mi?
        public DateTime CreatedDate { get; set; } // Oluşma tarihi (işlem sırasında)
        public DateTime? WasteDate { get; set; } // Hurda işaretleme tarihi
    }
}

