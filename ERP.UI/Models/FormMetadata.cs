using System;

namespace ERP.UI.Models
{
    public class FormMetadata
    {
        public string FormName { get; set; }
        public Type FormType { get; set; }
        public string DisplayName { get; set; }
        public bool RequiresAuthentication { get; set; } = false;

        public FormMetadata(string formName, Type formType, string displayName = null)
        {
            FormName = formName;
            FormType = formType;
            DisplayName = displayName ?? formName;
        }
    }
}

