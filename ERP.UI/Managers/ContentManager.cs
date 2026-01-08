using System;
using System.Linq;
using System.Windows.Forms;
using ERP.DAL.Repositories;
using ERP.UI.Services;
using ERP.UI.UI;
using ReportsLib.Data;
using ReportsLib.Reports;
using DevExpress.XtraReports.UI;

namespace ERP.UI.Managers
{
    public class ContentManager
    {
        private readonly Panel _contentPanel;
        private readonly FormResolverService _formResolver;
        private Forms.RuloStokTakipForm _currentRuloStokTakipForm;

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
                    orderListForm.OrderSendToAccountingRequested += (s, id) => HandleSendToAccountingFromOrder(id);
                    orderListForm.OrderGetWorkOrderRequested += (s, id) => HandleGetWorkOrder(id);
                    orderListForm.OrderGetBulkWorkOrderRequested += (s, ids) => HandleGetBulkWorkOrder(ids);
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
                    productionListForm.ProductionReturnToOrderRequested += (s, id) => HandleReturnToOrderFromProduction(id);
                    productionListForm.ProductionReportRequested += (s, id) => HandleProductionReport(id);
                }

                // ProductionDetailForm için event'leri bağla
                if (control is Forms.ProductionDetailForm productionDetailForm)
                {
                    productionDetailForm.BackRequested += (s, e) => ShowForm("Production");
                    productionDetailForm.ReportRequested += (s, id) => HandleProductionReport(id);
                    productionDetailForm.ReturnToOrderRequested += (s, id) => HandleReturnToOrderFromProduction(id);
                }

                // ConsumptionListForm için event'leri bağla
                if (control is Forms.ConsumptionListForm consumptionListForm)
                {
                    consumptionListForm.ConsumptionDetailRequested += (s, id) => ShowConsumptionDetail(id);
                }

                // ConsumptionDetailForm için event'leri bağla
                if (control is Forms.ConsumptionDetailForm consumptionDetailForm)
                {
                    consumptionDetailForm.BackRequested += (s, e) => ShowForm("Consumption");
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

                // RuloStokTakipForm için event'leri bağla
                if (control is Forms.RuloStokTakipForm ruloStokTakipForm)
                {
                    // Mevcut instance'ı sakla (yenileme için)
                    _currentRuloStokTakipForm = ruloStokTakipForm;
                }
                else
                {
                    // Başka bir form gösterildiğinde referansı temizle
                    _currentRuloStokTakipForm = null;
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
                    
                    // Event feed kaydı ekle
                    EventFeedService.OrderSentToProduction(orderId, order.TrexOrderNo);
                    
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

        // Üretimden siparişe dön (Status: "Üretimde" → "Yeni")
        private void HandleReturnToOrderFromProduction(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Status'u "Yeni" yap (üretim tamamlandı, siparişe döndü)
                    order.Status = "Yeni";
                    orderRepository.Update(order);
                    
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} siparişe döndürüldü.", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Üretim listesini yenile
                    ShowForm("Production");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş siparişe döndürülürken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Siparişten muhasebeye gönder (Status: "Yeni" → "Muhasebede")
        private void HandleSendToAccountingFromOrder(Guid orderId)
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
                    
                    // Event feed kaydı ekle
                    EventFeedService.OrderSentToAccounting(orderId, order.TrexOrderNo);
                    
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} muhasebeye gönderildi.", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Sipariş listesini yenile
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
                    // ReportData oluştur
                    var reportData = ReportsLib.Data.ReportData.FromOrder(order);
                    
                    // WorkOrderReport oluştur
                    var report = new ReportsLib.Reports.WorkOrderReport(reportData);
                    
                    // Raporu göster - DevExpress 22.1 kullanıyoruz
                    // ReportPrintTool muhtemelen ayrı bir Win DLL'inde veya farklı bir namespace'de
                    // Önce reflection ile bulmayı deniyoruz
                    try
                    {
                        var assembly = typeof(DevExpress.XtraReports.UI.XtraReport).Assembly;
                        
                        // Farklı namespace'leri dene
                        var printToolType = assembly.GetType("DevExpress.XtraReports.UI.ReportPrintTool") 
                                         ?? assembly.GetType("DevExpress.XtraReports.Win.ReportPrintTool")
                                         ?? assembly.GetType("DevExpress.XtraReports.ReportPrintTool");
                        
                        if (printToolType != null)
                        {
                            var printTool = Activator.CreateInstance(printToolType, report);
                            var showPreviewMethod = printToolType.GetMethod("ShowPreviewDialog");
                            if (showPreviewMethod != null)
                            {
                                showPreviewMethod.Invoke(printTool, null);
                                return; // Başarılı oldu, çık
                            }
                        }
                        
                        // Alternatif: XtraReport'un ShowPreview metodunu dene
                        var showPreviewMethod2 = typeof(DevExpress.XtraReports.UI.XtraReport).GetMethod("ShowPreview");
                        if (showPreviewMethod2 != null)
                        {
                            showPreviewMethod2.Invoke(report, null);
                            return;
                        }
                    }
                    catch
                    {
                        // Reflection başarısız oldu, PDF export'a geç
                    }
                    
                    // ReportPrintTool çalışmadı, PDF export'u dene
                    // Not: DevExpress 22.1 .NET 8.0 ile PrintingPermission hatası verebilir
                    string tempPath = null;
                    bool exportSuccess = false;
                    Exception lastException = null;
                    
                    // PDF export'u dene
                    try
                    {
                        tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"WorkOrder_{order.TrexOrderNo}.pdf");
                        report.ExportToPdf(tempPath);
                        exportSuccess = true;
                    }
                    catch (TypeLoadException tle) when (tle.Message.Contains("PrintingPermission"))
                    {
                        lastException = tle;
                        // PrintingPermission hatası - HTML export'u dene
                        try
                        {
                            tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"WorkOrder_{order.TrexOrderNo}.html");
                            report.ExportToHtml(tempPath);
                            exportSuccess = true;
                        }
                        catch (Exception htmlEx)
                        {
                            lastException = htmlEx;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                    
                    if (exportSuccess && tempPath != null && System.IO.File.Exists(tempPath))
                    {
                        // Dosyayı varsayılan görüntüleyici ile aç
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath) 
                            { 
                                UseShellExecute = true 
                            });
                            
                            string format = tempPath.EndsWith(".pdf") ? "PDF" : "HTML";
                            MessageBox.Show($"İş emri raporu başarıyla oluşturuldu ve {format} olarak açıldı.\n\n" +
                                $"Sipariş No: {order.TrexOrderNo}\n" +
                                $"Dosya Konumu: {tempPath}",
                                "İş Emri Raporu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception openEx)
                        {
                            MessageBox.Show($"Rapor oluşturuldu ancak açılamadı: {openEx.Message}\n\n" +
                                $"Dosya Konumu: {tempPath}",
                                "İş Emri Raporu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        // Export başarısız - kullanıcıya detaylı bilgi ver
                        string errorDetails = lastException != null ? lastException.Message : "Bilinmeyen hata";
                        string problemDescription = "";
                        
                        // Hata tipine göre açıklama
                        if (errorDetails.Contains("PrintingPermission"))
                        {
                            problemDescription = "Sorun: DevExpress 22.1 .NET 6.0 ile tam uyumlu değil.\n" +
                                "System.Drawing.Printing.PrintingPermission tipi .NET Core/.NET 6.0'da kaldırıldı.";
                        }
                        else if (errorDetails.Contains("AspNetHostingPermission"))
                        {
                            problemDescription = "Sorun: DevExpress 22.1 .NET 6.0 ile tam uyumlu değil.\n" +
                                "System.Web.AspNetHostingPermission tipi .NET Core/.NET 6.0'da kaldırıldı.";
                        }
                        else
                        {
                            problemDescription = "Sorun: DevExpress 22.1 .NET 6.0 ile tam uyumlu değil.\n" +
                                "Bazı güvenlik izin tipleri .NET Core/.NET 6.0'da kaldırıldı.";
                        }
                        
                        MessageBox.Show($"İş emri raporu oluşturulamadı.\n\n" +
                            $"Sipariş No: {order.TrexOrderNo}\n\n" +
                            $"Hata Detayı: {errorDetails}\n\n" +
                            $"{problemDescription}\n\n" +
                            "Çözüm Önerileri:\n" +
                            "1. DevExpress'in daha yeni bir versiyonunu kullanın (23.1 veya üzeri)\n" +
                            "2. Projeyi .NET Framework 4.8'e geçirin\n" +
                            "3. Raporu manuel olarak oluşturun",
                            "İş Emri Raporu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Sipariş bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İş emri raporu oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleGetBulkWorkOrder(List<Guid> orderIds)
        {
            try
            {
                if (orderIds == null || orderIds.Count == 0)
                {
                    MessageBox.Show("Lütfen en az bir sipariş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var orders = new List<ERP.Core.Models.Order>();
                
                foreach (var orderId in orderIds)
                {
                    var order = orderRepository.GetById(orderId);
                    if (order != null)
                    {
                        orders.Add(order);
                    }
                }

                if (orders.Count == 0)
                {
                    MessageBox.Show("Seçili siparişler bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Toplu ReportData oluştur
                var reportData = ReportsLib.Data.ReportData.FromOrders(orders);
                
                // WorkOrderReport oluştur
                var report = new ReportsLib.Reports.WorkOrderReport(reportData);
                
                // Raporu göster - DevExpress 23.1 kullanıyoruz
                try
                {
                    var assembly = typeof(DevExpress.XtraReports.UI.XtraReport).Assembly;
                    
                    // Farklı namespace'leri dene
                    var printToolType = assembly.GetType("DevExpress.XtraReports.UI.ReportPrintTool") 
                                     ?? assembly.GetType("DevExpress.XtraReports.Win.ReportPrintTool")
                                     ?? assembly.GetType("DevExpress.XtraReports.ReportPrintTool");
                    
                    if (printToolType != null)
                    {
                        var printTool = Activator.CreateInstance(printToolType, report);
                        var showPreviewMethod = printToolType.GetMethod("ShowPreviewDialog");
                        if (showPreviewMethod != null)
                        {
                            showPreviewMethod.Invoke(printTool, null);
                            return; // Başarılı oldu, çık
                        }
                    }
                    
                    // Alternatif: XtraReport'un ShowPreview metodunu dene
                    var showPreviewMethod2 = typeof(DevExpress.XtraReports.UI.XtraReport).GetMethod("ShowPreview");
                    if (showPreviewMethod2 != null)
                    {
                        showPreviewMethod2.Invoke(report, null);
                        return;
                    }
                }
                catch
                {
                    // Reflection başarısız oldu, PDF export'a geç
                }
                
                // ReportPrintTool çalışmadı, PDF export'u dene
                string tempPath = null;
                bool exportSuccess = false;
                Exception lastException = null;
                
                // PDF export'u dene
                try
                {
                    var orderNos = string.Join("_", orders.Select(o => o.TrexOrderNo).Take(3));
                    tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"BulkWorkOrder_{orderNos}_{orders.Count}Orders.pdf");
                    report.ExportToPdf(tempPath);
                    exportSuccess = true;
                }
                catch (TypeLoadException tle) when (tle.Message.Contains("PrintingPermission") || tle.Message.Contains("AspNetHostingPermission"))
                {
                    lastException = tle;
                    // Permission hatası - HTML export'u dene
                    try
                    {
                        var orderNos = string.Join("_", orders.Select(o => o.TrexOrderNo).Take(3));
                        tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"BulkWorkOrder_{orderNos}_{orders.Count}Orders.html");
                        report.ExportToHtml(tempPath);
                        exportSuccess = true;
                    }
                    catch (Exception htmlEx)
                    {
                        lastException = htmlEx;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                
                if (exportSuccess && tempPath != null && System.IO.File.Exists(tempPath))
                {
                    // Dosyayı varsayılan görüntüleyici ile aç
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath) 
                        { 
                            UseShellExecute = true 
                        });
                        
                        string format = tempPath.EndsWith(".pdf") ? "PDF" : "HTML";
                        MessageBox.Show($"Toplu iş emri raporu başarıyla oluşturuldu ve {format} olarak açıldı.\n\n" +
                            $"Seçili Sipariş Sayısı: {orders.Count}\n" +
                            $"Dosya Konumu: {tempPath}",
                            "Toplu İş Emri Raporu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception openEx)
                    {
                        MessageBox.Show($"Rapor oluşturuldu ancak açılamadı: {openEx.Message}\n\n" +
                            $"Dosya Konumu: {tempPath}",
                            "Toplu İş Emri Raporu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Export başarısız - kullanıcıya detaylı bilgi ver
                    string errorDetails = lastException != null ? lastException.Message : "Bilinmeyen hata";
                    string problemDescription = "";
                    
                    // Hata tipine göre açıklama
                    if (errorDetails.Contains("PrintingPermission"))
                    {
                        problemDescription = "Sorun: DevExpress 23.1 .NET 6.0 ile tam uyumlu değil.\n" +
                            "System.Drawing.Printing.PrintingPermission tipi .NET Core/.NET 6.0'da kaldırıldı.";
                    }
                    else if (errorDetails.Contains("AspNetHostingPermission"))
                    {
                        problemDescription = "Sorun: DevExpress 23.1 .NET 6.0 ile tam uyumlu değil.\n" +
                            "System.Web.AspNetHostingPermission tipi .NET Core/.NET 6.0'da kaldırıldı.";
                    }
                    else
                    {
                        problemDescription = "Sorun: DevExpress 23.1 .NET 6.0 ile tam uyumlu değil.\n" +
                            "Bazı güvenlik izin tipleri .NET Core/.NET 6.0'da kaldırıldı.";
                    }
                    
                    MessageBox.Show($"Toplu iş emri raporu oluşturulamadı.\n\n" +
                        $"Seçili Sipariş Sayısı: {orders.Count}\n\n" +
                        $"Hata Detayı: {errorDetails}\n\n" +
                        $"{problemDescription}\n\n" +
                        "Çözüm Önerileri:\n" +
                        "1. DevExpress'in daha yeni bir versiyonunu kullanın (23.2 veya üzeri)\n" +
                        "2. Projeyi .NET Framework 4.8'e geçirin\n" +
                        "3. Raporu manuel olarak oluşturun",
                        "Toplu İş Emri Raporu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Toplu iş emri raporu oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAccountingEntry(Guid orderId)
        {
            ShowForm("AccountingEntry", orderId);
        }

        // Muhasebeden siparişe dön (Status: "Muhasebede" → "Sevkiyata Hazır")
        private void HandleSendToShipment(Guid orderId)
        {
            try
            {
                var orderRepository = new ERP.DAL.Repositories.OrderRepository();
                var order = orderRepository.GetById(orderId);
                
                if (order != null)
                {
                    // Status'u "Sevkiyata Hazır" yap (muhasebe tamamlandı, siparişe döndü)
                    order.Status = "Sevkiyata Hazır";
                    orderRepository.Update(order);
                    
                    // Event feed kaydı ekle
                    EventFeedService.OrderReadyForShipment(orderId, order.TrexOrderNo);
                    
                    MessageBox.Show($"Sipariş {order.TrexOrderNo} siparişe döndürüldü (Sevkiyata Hazır).", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Muhasebe listesini yenile
                    ShowForm("Accounting");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş siparişe döndürülürken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowProductionDetail(Guid orderId)
        {
            ShowForm("ProductionDetail", orderId);
        }

        private void ShowConsumptionDetail(Guid orderId)
        {
            ShowForm("ConsumptionDetail", orderId);
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
            _currentRuloStokTakipForm = null;
            
            // EventFeedForm göster
            var eventFeedForm = _formResolver.ResolveForm("EventFeed");
            eventFeedForm.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(eventFeedForm);
            eventFeedForm.BringToFront();
            
            // Eski WelcomePanel yerine EventFeedForm kullanıyoruz
            // Eğer WelcomePanel'e geri dönmek isterseniz, aşağıdaki kodu kullanabilirsiniz:
            /*
            var welcomePanel = new Components.WelcomePanel();
            welcomePanel.Dock = DockStyle.Fill;
            welcomePanel.CardClicked += (s, formTag) => HandleCardClick(formTag);
            _contentPanel.Controls.Add(welcomePanel);
            welcomePanel.BringToFront();
            */
        }

        private void HandleCardClick(string formTag)
        {
            // MainForm'daki MenuManager_MenuItemClicked mantığıyla aynı
            if (formTag == "OrderList")
            {
                ShowForm("OrderList");
            }
            else if (formTag == "OrderCreate")
            {
                ShowForm("OrderCreate");
            }
            else if (formTag == "StockEntry")
            {
                ShowForm("StockEntry");
            }
            else if (formTag == "Accounting")
            {
                ShowForm("Accounting");
            }
            else if (formTag == "StockDetail")
            {
                ShowForm("StockDetail");
            }
            else if (formTag == "MaterialEntry")
            {
                ShowForm("MaterialEntry");
            }
            else if (formTag == "MaterialExit")
            {
                ShowForm("MaterialExit");
            }
            else if (formTag == "Production")
            {
                ShowForm("Production");
            }
            else if (formTag == "StockTracking" || formTag == "RuloStokTakip" || formTag == "KesilmisStokTakip" || 
                     formTag == "PreslenmisStokTakip" || formTag == "KenetlenmisStokTakip")
            {
                ShowStockTrackingContainer();
            }
            else if (formTag == "Consumption")
            {
                ShowForm("Consumption");
            }
            else if (formTag == "CuttingRequests")
            {
                ShowForm("CuttingRequests");
            }
            else if (formTag == "PressingRequests")
            {
                ShowForm("PressingRequests");
            }
            else if (formTag == "ClampingRequests")
            {
                ShowForm("ClampingRequests");
            }
            else if (formTag == "Clamping2Requests")
            {
                ShowForm("Clamping2Requests");
            }
            else if (formTag == "AssemblyRequests")
            {
                ShowForm("AssemblyRequests");
            }
            else if (formTag == "PackagingRequests")
            {
                ShowForm("PackagingRequests");
            }
            else if (formTag == "MRPReport")
            {
                ShowForm("MRPReport");
            }
            else if (formTag == "CustomerReport")
            {
                ShowForm("CustomerReport");
            }
            else if (formTag == "AnnualReport")
            {
                ShowForm("AnnualReport");
            }
            else
            {
                ShowForm(formTag);
            }
        }

        public void ShowStockTrackingContainer()
        {
            _contentPanel.Controls.Clear();
            _currentRuloStokTakipForm = null;
            var container = new Forms.StockTrackingContainerForm();
            container.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(container);
            container.BringToFront();
        }

        public void RefreshRuloStokTakip()
        {
            // Rulo Stok Takip sayfası açıksa yenile
            if (_currentRuloStokTakipForm != null)
            {
                _currentRuloStokTakipForm.RefreshData();
            }
        }
    }
}

