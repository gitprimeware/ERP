using System;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.Services;

namespace ERP.UI.Services
{
    public static class EventFeedService
    {
        private static EventFeedRepository _repository = new EventFeedRepository();

        public static void CreateEvent(string eventType, string title, string message, string requiredPermission, 
            Guid? relatedEntityId = null, string relatedEntityType = null)
        {
            try
            {
                var eventFeed = new EventFeed
                {
                    EventType = eventType,
                    Title = title,
                    Message = message,
                    RequiredPermission = requiredPermission,
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    CreatedByUserId = UserSessionService.IsLoggedIn ? UserSessionService.CurrentUser.Id : (Guid?)null,
                    EventDate = DateTime.Now
                };

                _repository.Insert(eventFeed);
            }
            catch (Exception ex)
            {
                // Event feed kaydı başarısız olsa bile ana işlem devam etsin
                System.Diagnostics.Debug.WriteLine($"Event feed kaydı oluşturulamadı: {ex.Message}");
            }
        }

        // Sipariş olayları
        public static void OrderCreated(Guid orderId, string orderNo, string companyName)
        {
            CreateEvent(
                "OrderCreated",
                "Yeni Sipariş Oluşturuldu",
                $"{orderNo} siparişi {companyName} için oluşturuldu",
                "OrderEntry",
                orderId,
                "Order"
            );
        }

        public static void OrderSentToProduction(Guid orderId, string orderNo)
        {
            CreateEvent(
                "OrderSentToProduction",
                "Sipariş Üretime Gönderildi",
                $"{orderNo} siparişi üretime gönderildi",
                "ProductionPlanning",
                orderId,
                "Order"
            );
        }

        public static void OrderSentToAccounting(Guid orderId, string orderNo)
        {
            CreateEvent(
                "OrderSentToAccounting",
                "Sipariş Muhasebeye Gönderildi",
                $"{orderNo} siparişi muhasebeye gönderildi",
                "Accounting",
                orderId,
                "Order"
            );
        }

        public static void OrderReturnedFromProduction(Guid orderId, string orderNo)
        {
            CreateEvent(
                "OrderReturnedFromProduction",
                "Sipariş Üretimden Döndü",
                $"{orderNo} siparişi üretimden döndü, fatura kesimi bekliyor",
                "OrderEntry",
                orderId,
                "Order"
            );
        }

        public static void OrderReadyForShipment(Guid orderId, string orderNo)
        {
            CreateEvent(
                "OrderReadyForShipment",
                "Sipariş Muhasebeden Döndü",
                $"{orderNo} siparişi için irsaliye kesildi, sevk tarihi bekleniyor",
                "OrderEntry,Accounting",
                orderId,
                "Order"
            );
        }

        // Kesim olayları
        public static void CuttingRequestCreated(Guid cuttingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "CuttingRequestCreated",
                "Kesim Emri Oluşturuldu",
                $"{orderNo} siparişi için kesim emri paylaşıldı",
                "CuttingRequests",
                cuttingRequestId,
                "CuttingRequest"
            );
        }

        public static void CuttingCompleted(Guid cuttingRequestId, Guid orderId, string orderNo, int actualCutCount)
        {
            CreateEvent(
                "CuttingCompleted",
                "Kesim Tamamlandı",
                $"{orderNo} siparişi için {actualCutCount} adet kesim tamamlandı, onay bekliyor",
                "ProductionPlanning",
                cuttingRequestId,
                "CuttingRequest"
            );
        }

        public static void CuttingApproved(Guid cuttingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "CuttingApproved",
                "Kesim Onaylandı",
                $"{orderNo} siparişi için kesim onaylandı",
                "CuttingRequests,ProductionPlanning",
                cuttingRequestId,
                "CuttingRequest"
            );
        }

        // Pres olayları
        public static void PressingRequestCreated(Guid pressingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "PressingRequestCreated",
                "Pres Emri Oluşturuldu",
                $"{orderNo} siparişi için pres emri paylaşıldı",
                "PressingRequests",
                pressingRequestId,
                "PressingRequest"
            );
        }

        public static void PressingCompleted(Guid pressingRequestId, Guid orderId, string orderNo, int resultedPressCount)
        {
            CreateEvent(
                "PressingCompleted",
                "Pres Tamamlandı",
                $"{orderNo} siparişi için {resultedPressCount} adet pres tamamlandı, onay bekliyor",
                "ProductionPlanning",
                pressingRequestId,
                "PressingRequest"
            );
        }

        public static void PressingApproved(Guid pressingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "PressingApproved",
                "Pres Onaylandı",
                $"{orderNo} siparişi için pres onaylandı",
                "PressingRequests,ProductionPlanning",
                pressingRequestId,
                "PressingRequest"
            );
        }

        // Kenetleme olayları
        public static void ClampingRequestCreated(Guid clampingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "ClampingRequestCreated",
                "Kenetleme Emri Oluşturuldu",
                $"{orderNo} siparişi için kenetleme emri paylaşıldı",
                "ClampingRequests",
                clampingRequestId,
                "ClampingRequest"
            );
        }

        public static void ClampingCompleted(Guid clampingRequestId, Guid orderId, string orderNo, int actualClampCount)
        {
            CreateEvent(
                "ClampingCompleted",
                "Kenetleme Tamamlandı",
                $"{orderNo} siparişi için {actualClampCount} adet kenetleme tamamlandı, onay bekliyor",
                "ProductionPlanning",
                clampingRequestId,
                "ClampingRequest"
            );
        }

        public static void ClampingApproved(Guid clampingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "ClampingApproved",
                "Kenetleme Onaylandı",
                $"{orderNo} siparişi için kenetleme onaylandı",
                "ClampingRequests,ProductionPlanning",
                clampingRequestId,
                "ClampingRequest"
            );
        }

        // Kenetleme 2 olayları
        public static void Clamping2RequestCreated(Guid clamping2RequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "Clamping2RequestCreated",
                "Kenetleme 2 Emri Oluşturuldu",
                $"{orderNo} siparişi için kenetleme 2 emri paylaşıldı",
                "Clamping2Requests",
                clamping2RequestId,
                "Clamping2Request"
            );
        }

        public static void Clamping2Completed(Guid clamping2RequestId, Guid orderId, string orderNo, int resultedCount)
        {
            CreateEvent(
                "Clamping2Completed",
                "Kenetleme 2 Tamamlandı",
                $"{orderNo} siparişi için {resultedCount} adet kenetleme 2 tamamlandı, onay bekliyor",
                "ProductionPlanning",
                clamping2RequestId,
                "Clamping2Request"
            );
        }

        public static void Clamping2Approved(Guid clamping2RequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "Clamping2Approved",
                "Kenetleme 2 Onaylandı",
                $"{orderNo} siparişi için kenetleme 2 onaylandı",
                "Clamping2Requests,ProductionPlanning",
                clamping2RequestId,
                "Clamping2Request"
            );
        }

        // Montaj olayları
        public static void AssemblyRequestCreated(Guid assemblyRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "AssemblyRequestCreated",
                "Montaj Emri Oluşturuldu",
                $"{orderNo} siparişi için montaj emri paylaşıldı",
                "AssemblyRequests",
                assemblyRequestId,
                "AssemblyRequest"
            );
        }

        public static void AssemblyCompleted(Guid assemblyRequestId, Guid orderId, string orderNo, int actualClampCount)
        {
            CreateEvent(
                "AssemblyCompleted",
                "Montaj Tamamlandı",
                $"{orderNo} siparişi için {actualClampCount} adet montaj tamamlandı, onay bekliyor",
                "ProductionPlanning",
                assemblyRequestId,
                "AssemblyRequest"
            );
        }

        public static void AssemblyApproved(Guid assemblyRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "AssemblyApproved",
                "Montaj Onaylandı",
                $"{orderNo} siparişi için montaj onaylandı",
                "AssemblyRequests,ProductionPlanning",
                assemblyRequestId,
                "AssemblyRequest"
            );
        }

        // Sipariş sevk edildi
        public static void OrderShipped(Guid orderId, string orderNo)
        {
            CreateEvent(
                "OrderShipped",
                "Sipariş Sevk Edildi",
                $"{orderNo} siparişi sevk edildi",
                "Accounting",
                orderId,
                "Order"
            );
        }

        // İzolasyon tamamlandı
        public static void IsolationCompleted(Guid isolationId, Guid orderId, string orderNo, int isolationCount)
        {
            CreateEvent(
                "IsolationCompleted",
                "İzolasyon Tamamlandı",
                $"{orderNo} siparişi için {isolationCount} adet izolasyon tamamlandı, onay bekliyor",
                "ProductionPlanning",
                isolationId,
                "Isolation"
            );
        }

        // Paketleme talebi oluşturuldu
        public static void PackagingRequestCreated(Guid packagingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "PackagingRequestCreated",
                "Paketleme Emri Oluşturuldu",
                $"{orderNo} siparişi için paketleme emri paylaşıldı",
                "PackagingRequests",
                packagingRequestId,
                "PackagingRequest"
            );
        }

        // Paketleme tamamlandı
        public static void PackagingCompleted(Guid packagingRequestId, Guid orderId, string orderNo, int packagingCount)
        {
            CreateEvent(
                "PackagingCompleted",
                "Paketleme Tamamlandı",
                $"{orderNo} siparişi için {packagingCount} adet paketleme tamamlandı, onay bekliyor",
                "ProductionPlanning",
                packagingRequestId,
                "PackagingRequest"
            );
        }

        // Paketleme onaylandı
        public static void PackagingApproved(Guid packagingRequestId, Guid orderId, string orderNo)
        {
            CreateEvent(
                "PackagingApproved",
                "Paketleme Onaylandı",
                $"{orderNo} siparişi için paketleme onaylandı",
                "PackagingRequests,ProductionPlanning",
                packagingRequestId,
                "PackagingRequest"
            );
        }

        // Stok girişleri
        public static void MaterialEntryCreated(Guid materialEntryId, string serialNumber, decimal quantity)
        {
            CreateEvent(
                "MaterialEntryCreated",
                "Rulo Stok Girişi",
                $"{serialNumber} seri numaralı rulo için {quantity:F2} kg stok girişi yapıldı",
                "StockManagement",
                materialEntryId,
                "MaterialEntry"
            );
        }

        public static void CoverStockEntryCreated(int quantity, string profileType, int size)
        {
            CreateEvent(
                "CoverStockEntryCreated",
                "Kapak Stok Girişi",
                $"{profileType} profil, {size}mm ölçü için {quantity} adet kapak stok girişi yapıldı",
                "ConsumptionMaterialStock",
                null,
                "CoverStock"
            );
        }

        public static void SideProfileStockEntryCreated(string profileType, decimal length, int quantity)
        {
            CreateEvent(
                "SideProfileStockEntryCreated",
                "Yan Profil Stok Girişi",
                $"{profileType} profil, {length:F2}m uzunluk için {quantity} adet yan profil stok girişi yapıldı",
                "ConsumptionMaterialStock",
                null,
                "SideProfileStock"
            );
        }

        public static void IsolationStockEntryCreated(string liquidType, decimal kilogram)
        {
            CreateEvent(
                "IsolationStockEntryCreated",
                "İzolasyon Sıvısı Stok Girişi",
                $"{liquidType} için {kilogram:F2} kg izolasyon sıvısı stok girişi yapıldı",
                "ConsumptionMaterialStock",
                null,
                "IsolationStock"
            );
        }
    }
}

