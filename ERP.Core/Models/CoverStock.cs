using System;

namespace ERP.Core.Models
{
    public class CoverStock : BaseModel
    {
        public string ProfileType { get; set; } // "Standart" veya "Geniş"
        public int Size { get; set; } // 200, 300, 400, 500, 600, 700, 800, 1000 (Standart) veya 211, 311, 411, 511, 611, 711, 811, 1011 (Geniş)
        public int CoverLength { get; set; } // 2 veya 30 (mm)
        public int Quantity { get; set; } // Stok adedi
        public DateTime EntryDate { get; set; } // Giriş tarihi (son güncelleme)
    }
}

