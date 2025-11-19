using System.Linq;
using System.Windows.Forms;
using ERP.DAL.Repositories;
using ERP.UI.Services;
using ERP.UI.UI;

namespace ERP.UI.Managers
{
    public class ContentManager
    {
        private readonly Panel _contentPanel;
        private readonly FormResolverService _formResolver;

        public ContentManager(Panel contentPanel, FormResolverService formResolver)
        {
            _contentPanel = contentPanel ?? throw new System.ArgumentNullException(nameof(contentPanel));
            _formResolver = formResolver ?? throw new System.ArgumentNullException(nameof(formResolver));
            InitializeContentPanel();
        }

        private void InitializeContentPanel()
        {
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.BackColor = ThemeColors.Background;
            _contentPanel.Padding = new Padding(20);
        }

        public void ShowForm(string formName, Guid orderId = default)
        {
            _contentPanel.Controls.Clear();

            var control = _formResolver.ResolveForm(formName, orderId);
            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(control);
                control.BringToFront();

                // OrderListForm için event'leri bağla
                if (control is Forms.OrderListForm orderListForm)
                {
                    orderListForm.OrderUpdateRequested += (s, id) => ShowOrderUpdate(id);
                    orderListForm.OrderDeleteRequested += (s, id) => HandleOrderDelete(id);
                }
            }
        }

        private void ShowOrderUpdate(Guid orderId)
        {
            ShowForm("OrderCreate", orderId);
            
            // OrderEntryForm'a veri yükle
            var control = _contentPanel.Controls.OfType<Forms.OrderEntryForm>().FirstOrDefault();
            if (control != null)
            {
                control.LoadOrderData(orderId);
            }
        }

        private void HandleOrderDelete(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                orderRepository.Delete(orderId);
                MessageBox.Show("Sipariş silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Listeyi yenile
                ShowForm("OrderList");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowWelcomePanel()
        {
            _contentPanel.Controls.Clear();
            var welcomePanel = new Components.WelcomePanel();
            welcomePanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(welcomePanel);
            welcomePanel.BringToFront();
        }
    }
}

