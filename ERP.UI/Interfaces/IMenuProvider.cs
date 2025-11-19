using System.Collections.Generic;
using ERP.UI.Models;

namespace ERP.UI.Interfaces
{
    public interface IMenuProvider
    {
        IEnumerable<MenuItem> GetMenuItems();
    }
}

