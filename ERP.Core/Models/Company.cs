using System;

namespace ERP.Core.Models
{
    public class Company : BaseModel
    {
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TaxNumber { get; set; }
    }
}

