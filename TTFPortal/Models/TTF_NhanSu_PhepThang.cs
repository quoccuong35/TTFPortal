//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TTFPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TTF_NhanSu_PhepThang
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public long NhanSu { get; set; }
        public Nullable<double> SoNgayPhep { get; set; }
        public Nullable<System.DateTime> NgayCapNhat { get; set; }
        public Nullable<bool> LamDu { get; set; }
        public string GhiChu { get; set; }
    }
}