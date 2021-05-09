using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class TangCaGrid
    {
        public long IDTangCa { get; set; }
        public Nullable<System.DateTime> NgayTangCa { get; set; }
        public Nullable<System.TimeSpan> GioBatDau { get; set; }
        public Nullable<System.TimeSpan> GioKetThuc { get; set; }
        public string LyDo { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public Nullable<int> NguoiTao { get; set; }
        public string LyDoHuy { get; set; }
        public Nullable<int> IDNguoiDuyetKeTiep { get; set; }
        public string MaPhong_PhanXuong { get; set; }
        public string MaTrangThaiDuyet { get; set; }
        public Nullable<bool> DotXuat { get; set; }
        public Nullable<bool> Block { get; set; }
        public Nullable<System.DateTime> NgayBlock { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
        public string TenPhong_PhanXuong { get; set; }
        public string TenTrangThaiDuyet { get; set; }
        public string HoVaTenNguoiDuyetKeTiep { get; set; }
        public Nullable<System.DateTime> NgayKiemTra { get; set; }
        public Nullable<int> NguoiKiemTra { get; set; }
        public string HoTenNguoiKiemTra { get; set; }
        public string MieuTaKiemTra { get; set; }
        public string GhiChuKiemTra { get; set; }
        public string MaDuAn { get; set; }
    }
}