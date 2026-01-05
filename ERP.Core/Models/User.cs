using System;

namespace ERP.Core.Models
{
    public class User : BaseModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
    }
}

