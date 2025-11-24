using DevExpress.XtraReports.UI;
using ReportsLib.Data;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace ReportsLib.Reports
{
    public partial class Rows : DevExpress.XtraReports.UI.XtraReport
    {
        private ReportData _data;
        
        public Rows(ReportData data)
        {
            InitializeComponent();
            _data = data;
            
            if (data != null && data.Rows != null && data.Rows.Count > 0)
            {
                // DataSource'u ayarla
                this.DataSource = data.Rows;
                
                // Binding'leri ayarla
                customerOrderNo.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.CustomerOrderNo));
                DeviceName.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.DeviceName));
                xrTableCell8.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.ProductCode));
                bypasOlcusu.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.BypassOlcusu));
                bypasTuru.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.BypassTuru));
                pieces.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.Pieces));
                productType.DataBindings.Add("Text", data.Rows, nameof(ReportRowData.ProductType));
            }
        }
    }
}
