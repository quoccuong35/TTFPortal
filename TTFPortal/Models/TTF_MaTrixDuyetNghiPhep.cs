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
    
    public partial class TTF_MaTrixDuyetNghiPhep
    {
        public int ID { get; set; }
        public string LoaiQuyTrinh { get; set; }
        public string MaPhong_PhanXuong { get; set; }
        public double TuNgay { get; set; }
        public double DenNgay { get; set; }
        public int CapDuyet { get; set; }
        public string NguoiDuyet { get; set; }
        public string GhiChu { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public Nullable<System.DateTime> NgayThayDoiLanCuoi { get; set; }
        public Nullable<int> NguoiTao { get; set; }
        public Nullable<int> NguoiThayDoiLanCuoi { get; set; }
    }
}
