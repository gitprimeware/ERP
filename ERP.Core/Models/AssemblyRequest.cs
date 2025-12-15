using System;

namespace ERP.Core.Models
{
    public class AssemblyRequest : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için montaj talebi (stok için null olabilir)
        public Order? Order { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? ClampingId { get; set; } // Hangi kenetlenmiş plakadan montaj yapılacak
        public Clamping? Clamping { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int RequestedAssemblyCount { get; set; } // İstenen Montaj Adedi (mühendis tarafından girilir)
        public int? ActualClampCount { get; set; } // Gerçekte kullanılan kenet adedi (işçi tarafından girilir - kenetlenmiş stoktan düşecek)
        public int? ResultedAssemblyCount { get; set; } // Gerçekte oluşan montaj adedi (işçi tarafından girilir - montajlanmış stoğa eklenecek)
        
        public Guid? EmployeeId { get; set; } // Operatör (mühendis)
        public Employee? Employee { get; set; }
        
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Montajda, Tamamlandı, İptal
        
        public DateTime RequestDate { get; set; } // Talep tarihi
        public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    }
}

