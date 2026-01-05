using System;

namespace ERP.Core.Models
{
    public class UserPermission : BaseModel
    {
        public Guid UserId { get; set; }
        public string PermissionKey { get; set; } // Örn: "OrderEntry", "StockEntry", "Accounting", vb.
    }
}

