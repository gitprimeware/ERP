using System;

namespace ERP.Core.Models
{
    public class Cutting : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için kesildi
        public Order? Order { get; set; }
        
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        
        public Guid? MachineId { get; set; } // Makina
        public Machine? Machine { get; set; }
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public decimal TotalKg { get; set; } // Toplam kg (otomatik)
        public decimal CutKg { get; set; } // Kesilen kg
        public int CuttingCount { get; set; } // Kesim adedi
        public decimal WasteKg { get; set; } // Hurda kg
        public decimal RemainingKg { get; set; } // Kalan kg (otomatik hesaplanır)
        
        public Guid? EmployeeId { get; set; } // Operatör
        public Employee? Employee { get; set; }
        
        public DateTime CuttingDate { get; set; } // Kesim tarihi
    }
}

