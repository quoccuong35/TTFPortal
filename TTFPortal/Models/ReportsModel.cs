using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Models
{
    public class ReportsModel
    {
        public string ReportName { get; set; }
        public XtraReport Report { get; set; }
    }
}