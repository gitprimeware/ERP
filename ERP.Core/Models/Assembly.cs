using System;

namespace ERP.Core.Models
{
    public class Assembly : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için montajlandı
        public Order? Order { get; set; }
        
        public Guid? ClampingId { get; set; } // Hangi kenetleme işleminden geldiği
        public Clamping? Clamping { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int AssemblyCount { get; set; } // Montaj Adedi
        public int UsedClampCount { get; set; } // Kullanılan Kenet Adedi
        
        public Guid? EmployeeId { get; set; } // Operatör
        public Employee? Employee { get; set; }
        
        public DateTime AssemblyDate { get; set; } // Montaj tarihi
    }
}

