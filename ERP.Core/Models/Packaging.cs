using System;

namespace ERP.Core.Models
{
    public class Packaging : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için paketlendi
        public Order? Order { get; set; }
        
        public Guid? AssemblyId { get; set; } // Hangi montaj işleminden geldiği
        public Assembly? Assembly { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        public decimal Length { get; set; } // Uzunluk
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public int PackagingCount { get; set; } // Paketleme Adedi
        public int UsedAssemblyCount { get; set; } // Kullanılan Montaj Adedi
        
        public Guid? EmployeeId { get; set; } // Operatör
        public Employee? Employee { get; set; }
        
        public DateTime PackagingDate { get; set; } // Paketleme tarihi
    }
}

