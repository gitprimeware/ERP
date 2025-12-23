using System;

namespace ERP.Core.Models
{
    public class PressingRequest : BaseModel
    {
        public Guid OrderId { get; set; } // Hangi sipariş için pres talebi
        public Order? Order { get; set; }
        
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? CuttingId { get; set; } // Hangi kesim kaydından pres yapılacak
        public Cutting? Cutting { get; set; }
        
        public int RequestedPressCount { get; set; } // İstenen Pres Adedi (mühendis tarafından girilir)
        public int? ActualPressCount { get; set; } // Gerçekte preslenen adet (kaç tane kesilmiş plaka kullanıldı - işçi tarafından girilir)
        public int? ResultedPressCount { get; set; } // Oluşan preslenmiş adet (kaç tane preslenmiş plaka oluştu - işçi tarafından girilir)
        public int? WasteCount { get; set; } // Hurda adedi (işçi tarafından girilir)
        
        public string PressNo { get; set; } = ""; // Pres No
        public decimal Pressure { get; set; } // Basınç
        public decimal WasteAmount { get; set; } // Hurda Miktarı (deprecated - artık WasteCount kullanılıyor)
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Presde, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

