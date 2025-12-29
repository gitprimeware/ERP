using System;

namespace ERP.Core.Models
{
    public class IsolationStock : BaseModel
    {
        public string LiquidType { get; set; } // "İzosiyanat", "Poliol", "İzolasyon"
        public decimal Liter { get; set; } // Litre cinsinden miktar
        public DateTime EntryDate { get; set; } // Giriş tarihi
    }
}

