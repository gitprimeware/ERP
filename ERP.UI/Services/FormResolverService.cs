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

            // AccountingEntryForm için özel işlem
            if (metadata.FormType == typeof(AccountingEntryForm) && orderId != Guid.Empty)
            {
                return new AccountingEntryForm(orderId);
            }

            // ProductionDetailForm için özel işlem
            if (metadata.FormType == typeof(ProductionDetailForm) && orderId != Guid.Empty)
            {
                return new ProductionDetailForm(orderId);
            }

            // ConsumptionDetailForm için özel işlem
            if (metadata.FormType == typeof(ConsumptionDetailForm) && orderId != Guid.Empty)
            {
                return new ConsumptionDetailForm(orderId);
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
            RegisterForm("Accounting", typeof(AccountingForm), "Muhasebe");
            RegisterForm("AccountingEntry", typeof(AccountingEntryForm), "Muhasebe İşlemi");
            RegisterForm("Production", typeof(ProductionListForm), "Üretim");
            RegisterForm("ProductionDetail", typeof(ProductionDetailForm), "Üretim Detayları");
            RegisterForm("RuloStokTakip", typeof(RuloStokTakipForm), "Rulo Stok Takip");
            RegisterForm("PreslenmisStokTakip", typeof(PreslenmisStokTakipForm), "Preslenmiş Stok Takip");
            RegisterForm("Consumption", typeof(ConsumptionListForm), "Sarfiyat");
            RegisterForm("ConsumptionDetail", typeof(ConsumptionDetailForm), "Sarfiyat Detayları");
            RegisterForm("MaterialEntry", typeof(MaterialEntryForm), "Malzeme Giriş");
            RegisterForm("MaterialExit", typeof(MaterialExitForm), "Malzeme Çıkış");
            RegisterForm("StockDetail", typeof(StockDetailForm), "Stok Ayrıntı");
            RegisterForm("MRPReport", typeof(MRPReportForm), "MRP Raporu");
            RegisterForm("CustomerReport", typeof(CustomerReportForm), "Cari Raporu");
            RegisterForm("AnnualReport", typeof(AnnualReportForm), "Yıllık Rapor");
            RegisterForm("GeneralReport", typeof(GeneralReportForm), "Genel Rapor");
            RegisterForm("Reports", typeof(MRPReportForm), "Raporlar"); // Ana raporlar sayfası
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

