using System;

namespace ERP.Core.Models
{
    public class Clamping : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için kenetlendi
        public Order? Order { get; set; }
        
        public Guid? PressingId { get; set; } // Hangi pres işleminden geldiği
        public Pressing? Pressing { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int ClampCount { get; set; } // Kenetleme Adedi
        public int UsedPlateCount { get; set; } // Kullanılan Plaka Adedi
        
        public Guid? EmployeeId { get; set; } // Operatör
        public Employee? Employee { get; set; }
        
        public DateTime ClampingDate { get; set; } // Kenetleme tarihi
    }
}

