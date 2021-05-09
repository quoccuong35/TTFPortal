using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class TangCaCong
    {
        public int NhanSu { get; set; }
        public Nullable<System.DateTime> NgayTangCa { get; set; }
        public Nullable<System.TimeSpan> GioBatDau { get; set; }
        public Nullable<System.TimeSpan> GioKetThuc { get; set; }
    }
}