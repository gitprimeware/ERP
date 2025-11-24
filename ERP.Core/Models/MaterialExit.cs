using System;

namespace ERP.Core.Models
{
    public class MaterialExit : BaseModel
    {
        public string TransactionType { get; set; } // "Hurda Çıkış" veya "Düzenleme Çıkış"
        public string MaterialType { get; set; } // "Alüminyum" veya "Galvaniz"
        public string MaterialSize { get; set; } // "Alü. 614X0,150" formatında
        public int Size { get; set; } // 214, 213, 313, vb.
        public decimal Thickness { get; set; } // 0.120, 0.150, 0.165, vb.
        
        public Guid? CompanyId { get; set; } // Firma (müşteri)
        public Company? Company { get; set; }
        
        public string TrexInvoiceNo { get; set; } // Trex Fatura No
        public DateTime ExitDate { get; set; } // Çıkış tarihi (otomatik)
        public decimal Quantity { get; set; } // Malzeme miktarı
    }
}

