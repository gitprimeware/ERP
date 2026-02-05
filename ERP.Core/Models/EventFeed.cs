using System;

namespace ERP.Core.Models
{
    public class EventFeed : BaseModel
    {
        public string EventType { get; set; } // Örn: "OrderCreated", "CuttingRequestCreated", "CuttingCompleted", vb.
        public string Title { get; set; } // Örn: "Kesim Emri Oluşturuldu"
        public string Message { get; set; } // Örn: "SP-2024-0001 siparişi için kesim emri paylaşıldı"
        public string RequiredPermission { get; set; } // Bu event'i görmek için gerekli izin anahtarı
        public Guid? RelatedEntityId { get; set; } // İlgili entity ID (OrderId, CuttingRequestId, vb.)
        public string RelatedEntityType { get; set; } // İlgili entity tipi (Order, CuttingRequest, vb.)
        public Guid? CreatedByUserId { get; set; } // Event'i oluşturan kullanıcı (User in separate database)
        public DateTime EventDate { get; set; } // Event'in oluştuğu tarih
        public bool IsRead { get; set; } // Kullanıcı tarafından okundu mu? (Gelecekte kullanılabilir)
    }
}

