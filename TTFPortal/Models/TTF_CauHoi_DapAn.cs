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
    
    public partial class TTF_CauHoi_DapAn
    {
        public long Id { get; set; }
        public long IdCauHoi { get; set; }
        public string TenDapAn { get; set; }
        public Nullable<long> IsDung { get; set; }
        public Nullable<System.DateTime> Ngay1 { get; set; }
        public Nullable<int> NguoiDung1 { get; set; }
        public Nullable<System.DateTime> Ngay2 { get; set; }
        public Nullable<int> NguoiDung2 { get; set; }
    }
}