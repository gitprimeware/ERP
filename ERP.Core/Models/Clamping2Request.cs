using System;

namespace ERP.Core.Models
{
    public class Clamping2Request : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için kenetleme 2 talebi (stok için null olabilir)
        public Order? Order { get; set; }
        
        public decimal Hatve { get; set; } // Hatve (seçilecek)
        public decimal PlateThickness { get; set; } // Lamel Kalınlığı (seçilecek)
        
        public Guid? FirstClampingId { get; set; } // İlk kenetlenmiş ürün
        public Clamping? FirstClamping { get; set; }
        
        public Guid? SecondClampingId { get; set; } // İkinci kenetlenmiş ürün
        public Clamping? SecondClamping { get; set; }
        
        public decimal ResultedSize { get; set; } // Sonuç ölçü (iki ürünün ölçüsü aynı olmalı)
        public decimal ResultedLength { get; set; } // Sonuç uzunluk (iki uzunluğun toplamı)
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int RequestedCount { get; set; } // İstenen adet (ikisinden aynı miktar işleme sokulacak)
        public int? ActualCount { get; set; } // Gerçekte kullanılan adet (işçi tarafından girilir)
        public int? ResultedCount { get; set; } // Oluşan kenetlenmiş adet (işçi tarafından girilir)
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Kenetmede, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

