using System;

namespace ERP.Core.Models
{
    public class PackagingRequest : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için paketleme talebi
        public Order? Order { get; set; }
        
        public Guid? IsolationId { get; set; } // Hangi izolasyon işleminden paketleme yapılacak
        public Isolation? Isolation { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int RequestedPackagingCount { get; set; } // İstenen Paketleme Adedi (mühendis tarafından girilir)
        public int? ActualPackagingCount { get; set; } // Gerçekte yapılan paketleme adedi (işçi tarafından girilir)
        public int? UsedIsolationCount { get; set; } // Kullanılan izolasyon adedi
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Paketlemede, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

