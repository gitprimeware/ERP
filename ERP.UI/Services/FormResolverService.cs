using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ERP.UI.Forms;
using ERP.UI.Models;

namespace ERP.UI.Services
{
    public class FormResolverService
    {
        private readonly Dictionary<string, FormMetadata> _formRegistry;

        public FormResolverService()
        {
            _formRegistry = new Dictionary<string, FormMetadata>();
            RegisterForms();
        }

        public UserControl ResolveForm(string formName, Guid orderId = default)
        {
            if (!_formRegistry.ContainsKey(formName))
            {
                return CreatePlaceholderControl(formName);
            }

            var metadata = _formRegistry[formName];
            
            // OrderEntryForm için özel işlem
            if (metadata.FormType == typeof(OrderEntryForm) && orderId != Guid.Empty)
            {
                return new OrderEntryForm(orderId);
            }

            return (UserControl)Activator.CreateInstance(metadata.FormType);
        }

        public bool FormExists(string formName)
        {
            return _formRegistry.ContainsKey(formName);
        }

        private void RegisterForms()
        {
            RegisterForm("OrderList", typeof(OrderListForm), "Siparişleri Görüntüle");
            RegisterForm("OrderCreate", typeof(OrderEntryForm), "Yeni Sipariş");
            RegisterForm("OrderEntry", typeof(OrderEntryForm), "Sipariş Girişi"); // Backward compatibility
            // Diğer formlar buraya eklenecek
        }

        private void RegisterForm(string formName, Type formType, string displayName = null)
        {
            _formRegistry[formName] = new FormMetadata(formName, formType, displayName);
        }

        private UserControl CreatePlaceholderControl(string formName)
        {
            var placeholder = new UserControl
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };

            var label = new Label
            {
                Text = $"{formName} modülü yakında eklenecek...",
                Font = new System.Drawing.Font("Segoe UI", 14F),
                ForeColor = UI.ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new System.Drawing.Point(50, 50)
            };

            placeholder.Controls.Add(label);
            return placeholder;
        }
    }
}

