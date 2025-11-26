using System;

namespace ERP.Core.Models
{
    public class MaterialEntry : BaseModel
    {
        public string TransactionType { get; set; } // "Satın Alma Giriş" veya "Düzenleme Giriş"
        public string MaterialType { get; set; } // "Alüminyum" veya "Galvaniz"
        public string MaterialSize { get; set; } // "Alü. 614X0,150" formatında
        public int Size { get; set; } // 214, 213, 313, vb.
        public decimal Thickness { get; set; } // 0.120, 0.150, 0.165, vb.
        
        public Guid? SupplierId { get; set; } // Tedarikçi
        public Supplier? Supplier { get; set; }
        
        public Guid? SerialNoId { get; set; } // Seri No
        public SerialNo? SerialNo { get; set; }
        
        public string InvoiceNo { get; set; } // Fatura No
        public string TrexPurchaseNo { get; set; } // Trex Satın Alma No
        public DateTime EntryDate { get; set; } // Giriş tarihi (otomatik)
        public decimal Quantity { get; set; } // Malzeme miktarı
    }
}

