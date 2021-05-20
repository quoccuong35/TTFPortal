using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Models
{
    public class NguoiDungModel
    {
        public decimal NguoiDung { get; set; }
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string TenHienThi { get; set; }
        public int NhomNguoiDung { get; set; }
        public string MaPhongPhanXuong { get; set; }
        public int NhanSu { get; set; }
        public int? CapQuanLyTrucTiep { get; set; }
        public string Image { get; set; }
        public string MaNV { get; set; }
        public string TenPhongBan { get; set; }
        public string PhamVi { get; set; }
    }
}