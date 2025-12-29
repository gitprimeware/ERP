using System;

namespace ERP.Core.Models
{
    public class Clamping2RequestItem : BaseModel
    {
        public Guid Clamping2RequestId { get; set; } // Hangi Clamping2Request'e ait
        public Clamping2Request? Clamping2Request { get; set; }
        
        public Guid ClampingId { get; set; } // Hangi kenetlenmiş ürün
        public Clamping? Clamping { get; set; }
        
        public int Sequence { get; set; } // Sıralama (1, 2, 3, ...)
    }
}

