using DevExpress.XtraReports.UI;
using ReportsLib.Data;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace ReportsLib.Reports
{
    public partial class WorkOrderReport : DevExpress.XtraReports.UI.XtraReport
    {
        public WorkOrderReport(ReportData data)
        {
            InitializeComponent();
            LoadData(data);
            
            // Rows subreport'u için veri yükle
            if (data != null && data.Rows != null && data.Rows.Count > 0)
            {
                var rowsReport = new Rows(data);
                Rows.ReportSource = rowsReport;
            }
        }
        
        private void LoadData(ReportData data) 
        {
            if (data == null) return;
            
            // Sipariş bilgilerini doldur
            orderNo.Text = data.OrderNo;
            company.Text = data.CompanyName;
            orderDate.Text = data.OrderDate.ToString("dd.MM.yyyy");
            deliveryDate.Text = data.DeliveryDate.ToString("dd.MM.yyyy");
            dateLabel.Text = data.ReportDate.ToString("dd.MM.yyyy HH:mm");
        } 
    }
}
