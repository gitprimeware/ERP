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
                    orderListForm.OrderSendToProductionRequested += (s, id) => HandleSendToProduction(id);
                    orderListForm.OrderGetWorkOrderRequested += (s, id) => HandleGetWorkOrder(id);
                }

                // AccountingForm için event'leri bağla
                if (control is Forms.AccountingForm accountingForm)
                {
                    accountingForm.AccountingEntryRequested += (s, id) => ShowAccountingEntry(id);
                    accountingForm.OrderSendToShipmentRequested += (s, id) => HandleSendToShipment(id);
                }

                // ProductionListForm için event'leri bağla
                if (control is Forms.ProductionListForm productionListForm)
                {
                    productionListForm.ProductionDetailRequested += (s, id) => ShowProductionDetail(id);
                    productionListForm.ProductionSendToAccountingRequested += (s, id) => HandleSendToAccounting(id);
                }

                // ProductionDetailForm için event'leri bağla
                if (control is Forms.ProductionDetailForm productionDetailForm)
                {
                    productionDetailForm.BackRequested += (s, e) => ShowForm("Production");
                    productionDetailForm.ReportRequested += (s, id) => HandleProductionReport(id);
                    productionDetailForm.SendToAccountingRequested += (s, id) => HandleSendToAccounting(id);
                }

                // OrderEntryForm için event'leri bağla
                if (control is Forms.OrderEntryForm orderEntryForm)
                {
                    orderEntryForm.OrderDeleteRequested += (s, id) => HandleOrderDelete(id);
                    orderEntryForm.OrderSendToProductionRequested += (s, id) => HandleSendToProduction(id);
                    orderEntryForm.OrderGetWorkOrderRequested += (s, id) => HandleGetWorkOrder(id);
                }

                // AccountingEntryForm için event'leri bağla
                if (control is Forms.AccountingEntryForm accountingEntryForm)
                {
                    accountingEntryForm.AccountingEntrySaved += (s, e) =>
                    {
                        // Muhasebe listesini yenile
                        ShowForm("Accounting");
                    };
                    accountingEntryForm.OrderSendToShipmentRequested += (s, id) => HandleSendToShipment(id);
                }
            }
        }

        private void ShowOrderUpdate(Guid orderId)
        {
            try
            {
                // Önce siparişin status'unu kontrol et
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null && (order.Status == "Üretimde" || order.Status == "Muhasebede"))
                {
                    MessageBox.Show(
                        $"Bu sipariş {order.Status} durumunda olduğu için güncellenemez. Sadece görüntüleme modunda açılacak.",
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                
                ShowForm("OrderCreate", orderId);
                
                // OrderEntryForm'a veri yükle
                var control = _contentPanel.Controls.OfType<Forms.OrderEntryForm>().FirstOrDefault();
                if (control != null)
                {
                    control.LoadOrderData(orderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void HandleSendToProduction(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    order.Status = "Üretimde";
                    orderRepository.Update(order);
                    MessageBox.Show("Sipariş üretime gönderildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Listeyi yenile
                    ShowForm("OrderList");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş üretime gönderilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleSendToAccounting(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Status'u "Muhasebede" yap
                    order.Status = "Muhasebede";
                    orderRepository.Update(order);
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} muhasebeye gönderildi.", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Listeyi yenile
                    ShowForm("OrderList");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş muhasebeye gönderilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleGetWorkOrder(Guid orderId)
        {
            try
            {
                // İş emri raporu oluşturma - Status değişikliği yapılmaz
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Burada iş emri raporu oluşturulacak (PDF, Excel vb.)
                    // Şimdilik sadece mesaj göster
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} için iş emri raporu oluşturulacak. (Rapor oluşturma özelliği eklenecek)", 
                        "İş Emri Raporu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İş emri raporu oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAccountingEntry(Guid orderId)
        {
            ShowForm("AccountingEntry", orderId);
        }

        private void HandleSendToShipment(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Status'u "Sevkiyata Hazır" yap
                    order.Status = "Sevkiyata Hazır";
                    orderRepository.Update(order);
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} sevkiyata hazır durumuna getirildi.", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Muhasebe listesini yenile
                    ShowForm("Accounting");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş sevkiyata hazır durumuna getirilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowProductionDetail(Guid orderId)
        {
            ShowForm("ProductionDetail", orderId);
        }

        private void HandleProductionReport(Guid orderId)
        {
            try
            {
                // Rapor oluşturma - Şimdilik sadece mesaj göster
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Burada rapor oluşturulacak (PDF, Excel vb.)
                    // Şimdilik sadece mesaj göster
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} için rapor oluşturulacak. (Rapor oluşturma özelliği eklenecek)", 
                        "Rapor Oluştur", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

