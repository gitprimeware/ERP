using System;
using System.Collections.Generic;
using ERP.Core.Models;

namespace ReportsLib.Data
{
    public class ReportData
    {
        // Ana sipariş bilgileri
        public string OrderNo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        
        // Sipariş detay satırları
        public List<ReportRowData> Rows { get; set; } = new List<ReportRowData>();
        
        // Order modelinden ReportData oluştur
        public static ReportData FromOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            var reportData = new ReportData
            {
                OrderNo = order.TrexOrderNo ?? string.Empty,
                CompanyName = order.Company?.Name ?? string.Empty,
                OrderDate = order.OrderDate,
                DeliveryDate = order.TermDate,
                ReportDate = DateTime.Now
            };
            
            // Sipariş detaylarını ekle
            if (!string.IsNullOrEmpty(order.CustomerOrderNo) || 
                !string.IsNullOrEmpty(order.DeviceName) || 
                !string.IsNullOrEmpty(order.ProductCode))
            {
                reportData.Rows.Add(new ReportRowData
                {
                    CustomerOrderNo = order.CustomerOrderNo ?? string.Empty,
                    DeviceName = order.DeviceName ?? string.Empty,
                    ProductCode = order.ProductCode ?? string.Empty,
                    BypassOlcusu = order.BypassSize ?? string.Empty,
                    BypassTuru = order.BypassType ?? string.Empty,
                    Pieces = order.Quantity,
                    ProductType = order.ProductType ?? string.Empty
                });
            }
            
            // OrderItems varsa onları da ekle
            if (order.OrderItems != null && order.OrderItems.Count > 0)
            {
                foreach (var item in order.OrderItems)
                {
                    reportData.Rows.Add(new ReportRowData
                    {
                        CustomerOrderNo = order.CustomerOrderNo ?? string.Empty,
                        DeviceName = order.DeviceName ?? string.Empty,
                        ProductCode = item.ProductCode ?? order.ProductCode ?? string.Empty,
                        BypassOlcusu = order.BypassSize ?? string.Empty,
                        BypassTuru = order.BypassType ?? string.Empty,
                        Pieces = item.Quantity,
                        ProductType = order.ProductType ?? string.Empty
                    });
                }
            }
            
            return reportData;
        }
        
        // Birden fazla Order'dan ReportData oluştur (toplu iş emri için)
        public static ReportData FromOrders(List<Order> orders)
        {
            if (orders == null || orders.Count == 0)
                throw new ArgumentException("En az bir sipariş gerekli", nameof(orders));
            
            // İlk siparişin bilgilerini ana bilgi olarak kullan
            var firstOrder = orders[0];
            var reportData = new ReportData
            {
                OrderNo = orders.Count == 1 
                    ? firstOrder.TrexOrderNo ?? string.Empty 
                    : $"TOPLU İŞ EMRİ ({orders.Count} Sipariş)",
                CompanyName = firstOrder.Company?.Name ?? string.Empty,
                OrderDate = orders.Min(o => o.OrderDate),
                DeliveryDate = orders.Max(o => o.TermDate),
                ReportDate = DateTime.Now
            };
            
            // Tüm siparişlerin detaylarını ekle
            foreach (var order in orders)
            {
                // Ana sipariş bilgisi
                if (!string.IsNullOrEmpty(order.CustomerOrderNo) || 
                    !string.IsNullOrEmpty(order.DeviceName) || 
                    !string.IsNullOrEmpty(order.ProductCode))
                {
                    reportData.Rows.Add(new ReportRowData
                    {
                        CustomerOrderNo = order.CustomerOrderNo ?? string.Empty,
                        DeviceName = order.DeviceName ?? string.Empty,
                        ProductCode = order.ProductCode ?? string.Empty,
                        BypassOlcusu = order.BypassSize ?? string.Empty,
                        BypassTuru = order.BypassType ?? string.Empty,
                        Pieces = order.Quantity,
                        ProductType = order.ProductType ?? string.Empty
                    });
                }
                
                // OrderItems varsa onları da ekle
                if (order.OrderItems != null && order.OrderItems.Count > 0)
                {
                    foreach (var item in order.OrderItems)
                    {
                        reportData.Rows.Add(new ReportRowData
                        {
                            CustomerOrderNo = order.CustomerOrderNo ?? string.Empty,
                            DeviceName = order.DeviceName ?? string.Empty,
                            ProductCode = item.ProductCode ?? order.ProductCode ?? string.Empty,
                            BypassOlcusu = order.BypassSize ?? string.Empty,
                            BypassTuru = order.BypassType ?? string.Empty,
                            Pieces = item.Quantity,
                            ProductType = order.ProductType ?? string.Empty
                        });
                    }
                }
            }
            
            return reportData;
        }
    }
    
    public class ReportRowData
    {
        public string CustomerOrderNo { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string BypassOlcusu { get; set; } = string.Empty;
        public string BypassTuru { get; set; } = string.Empty;
        public int Pieces { get; set; }
        public string ProductType { get; set; } = string.Empty;
    }
}
