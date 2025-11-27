using System;

namespace ERP.Core.Models
{
    public class Pressing : BaseModel
    {
        public Guid? OrderId { get; set; } // Hangi sipariş için preslendi
        public Order? Order { get; set; }
        
        public decimal PlateThickness { get; set; } // Plaka Kalınlığı
        public decimal Hatve { get; set; } // Hatve
        public decimal Size { get; set; } // Ölçü
        
        public Guid? SerialNoId { get; set; } // Rulo Seri No
        public SerialNo? SerialNo { get; set; }
        
        public string PressNo { get; set; } // Pres No
        public decimal Pressure { get; set; } // Basınç
        public int PressCount { get; set; } // Pres Adedi
        public decimal WasteAmount { get; set; } // Hurda Miktarı
        
        public Guid? EmployeeId { get; set; } // Operatör
        public Employee? Employee { get; set; }
        
        public DateTime PressingDate { get; set; } // Pres tarihi
    }
}

