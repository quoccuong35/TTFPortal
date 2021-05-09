using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class XacNhanCong
    {
        public long IDXacNhanCong { get; set; }
        public int NhanSu { get; set; }
        public System.DateTime Ngay { get; set; }
        public Nullable<bool> GioVao { get; set; }
        public string NguyenNhan { get; set; }
        public string GhiChu { get; set; }
        public Nullable<int> NguoiTao { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public string MayTao { get; set; }
        public string MaTrangThaiDuyet { get; set; }
        public string LyDoHuy { get; set; }
        public Nullable<int> IDNguoiDuyetKeTiep { get; set; }
        public Nullable<bool> Block { get; set; }
        public Nullable<System.DateTime> NgayBlock { get; set; }
        public Nullable<bool> Del { get; set; }
        public Nullable<int> NguoiThayDoiLanCuoi { get; set; }
        public Nullable<System.DateTime> NgayThayDoiLanCuoi { get; set; }
        public string HoVaTen { get; set; }
        public string MaNhanVien { get; set; }
        public string ThoiGian { get; set; }
        public string TGVao { get; set; }
        public string TGRa { get; set; }
        public string TenPhong_PhanXuong { get; set; }
        public bool? CaDem { get; set; }
    }
    public class BaoCaoSoLanXacNhanCong
    {
        public int NhanSu { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
        public string GioiTinh { get; set; }
        public string MaNhanVien { get; set; }
        public string TenPhong_PhanXuong { get; set; }
        public string TenChucVu { get; set; }
        public int SoLan { get; set; }
    }
    public class TTF_DuyetXacNhanCong
    {
        public long IDXacNhanCong { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
        public System.DateTime Ngay { get; set; }
        public string NguyenNhan { get; set; }
        public string ThoiGian { get; set; }
        public Nullable<System.TimeSpan> Gio { get; set; }
        public string TenPhong_PhanXuong { get; set; }
    }
}