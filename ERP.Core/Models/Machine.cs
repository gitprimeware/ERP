using System;

namespace ERP.Core.Models
{
    public class Machine : BaseModel
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}

