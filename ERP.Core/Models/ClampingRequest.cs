using System;

namespace ERP.Core.Models
{
    public class ClampingRequest : BaseModel
    {
        public Guid OrderId { get; set; } // Hangi sipariş için kenetleme talebi
        public Order? Order { get; set; }
        
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? PressingId { get; set; } // Hangi preslenmiş plakadan kenetlenecek
        public Pressing? Pressing { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int RequestedClampCount { get; set; } // İstenen Kenetleme Adedi (mühendis tarafından girilir)
        public int? ActualClampCount { get; set; } // Gerçekte kenetlenen adet (kaç tane preslenmiş plaka kullanıldı - işçi tarafından girilir)
        public int? ResultedClampCount { get; set; } // Oluşan kenetlenmiş adet (kaç tane kenetlenmiş plaka oluştu - işçi tarafından girilir)
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Kenetmede, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

