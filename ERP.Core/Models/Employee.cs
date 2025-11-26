using System;

namespace ERP.Core.Models
{
    public class Employee : BaseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
    }
}

