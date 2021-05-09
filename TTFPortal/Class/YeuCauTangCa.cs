using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class TangCa
    {
        //public TangCa()
        //{
        //    Files = new List<HttpPostedFileBase>();
        //    DuAn = true;
        //}
        public Int64 IDTangCa { get; set; }
        public string MaPhong_PhanXuong { get; set; }
        //public TTF_PhongBan_PhanXuong TTF_PhongBan_PhanXuong { get; set; }
        public DateTime NgayTangCa { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }
        public string LyDo { get; set; }
        public bool DotXuat { get; set; }
        public List<YeuCauTangCaChiTiet> TCChiTiet { get; set; }
        public string GhiChu { get; set; }
        public string GhiChuKiemTra { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
        public bool Del { get; set; }
        public bool DuAn { get; set; }
        public string MaDuAn { get; set; }
        public bool CongTac { get; set; }
        public bool Block { get; set; }
        public int NguoiTao { get; set; }
        public int? IDNguoiDuyetKeTiep { get; set; }
    }
    public class YeuCauTangCaChiTiet
    {
        public Int32 STT { get; set; }
        public Int32 NhanSu { get; set; }
        public string MaPhong_PhanXuong { get; set; }
        public string TenPhongBan { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
    }
}