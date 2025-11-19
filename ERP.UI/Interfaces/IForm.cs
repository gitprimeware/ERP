using System.Windows.Forms;

namespace ERP.UI.Interfaces
{
    public interface IForm
    {
        string FormName { get; }
        UserControl GetControl();
        void Initialize();
        void Cleanup();
    }
}

