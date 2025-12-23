using System;

namespace ERP.Core.Models
{
    public class CuttingRequest : BaseModel
    {
        public Guid OrderId { get; set; } // Hangi sipariş için kesim talebi
        public Order? Order { get; set; }
        
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public int RequestedPlateCount { get; set; } // İstenen Plaka Adedi (mühendis tarafından girilir)
        public decimal OnePlateWeight { get; set; } // Bir Plaka Ağırlığı (hesaplanır)
        public decimal TotalRequiredPlateWeight { get; set; } // Toplam Gereken Plaka Ağırlığı (hesaplanır)
        public decimal RemainingKg { get; set; } // Kalan kg (rulo stoğundan)
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public int? ActualCutCount { get; set; } // Gerçekte kesilen adet (işçi tarafından girilir)
        public int? WasteCount { get; set; } // Hurda adedi (işçi tarafından girilir)
        public bool IsRollFinished { get; set; } // Rulo bitti mi? (işçi tarafından işaretlenir)
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Kesimde, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

