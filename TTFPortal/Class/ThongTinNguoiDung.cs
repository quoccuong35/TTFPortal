using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

using TTFPortal.Models;

namespace TTFPortal.Class
{
    public class ThongTinNguoiDung
    {
        public void SetNguoiDung(NguoiDungModel nguoiDung)
        {
            try
            {
                MemoryCache.Default.Add(nguoiDung.TaiKhoan, nguoiDung, DateTime.Now.AddDays(1));
            }
            catch
            {

            }
        }
        public NguoiDungModel GetNguoiDung(string Username)
        {
            try
            {
                return (NguoiDungModel)MemoryCache.Default.Get(Username);
            }
            catch
            {
                return null;
            }
        }
    }
    public class Users
    {

        public static void SetNguoiDung(NguoiDungModel nguoiDung)
        {
            ThongTinNguoiDung thongtin = new ThongTinNguoiDung();
            thongtin.SetNguoiDung(nguoiDung);
        }
        public static NguoiDungModel GetNguoiDung(string Username)
        {
            ThongTinNguoiDung thongtin = new ThongTinNguoiDung();
            var ng = thongtin.GetNguoiDung(Username);
            if (ng == null)
            {
                TTF_FACEIDEntities db = new TTF_FACEIDEntities();
                var nguoidung = db.V_Users.FirstOrDefault(m => m.TAIKHOAN == Username);
                if (nguoidung != null)
                {
                    ng = new NguoiDungModel();
                    ng.NguoiDung = nguoidung.NGUOIDUNG;
                    ng.TaiKhoan = nguoidung.TAIKHOAN;
                    //ng.MatKhau = nguoidung.MatKhau;
                    ng.Email = nguoidung.MailCongTy;
                    ng.TenHienThi = nguoidung.HoVaTen;
                    ng.NhomNguoiDung = nguoidung.NHOMNGUOIDUNG.Value;
                    ng.MaPhongPhanXuong = nguoidung.MaPhong_PhanXuong;
                    ng.NhanSu = nguoidung.NhanSu.Value;
                    ng.MaNV = nguoidung.MaNV;
                    ng.TenPhongBan = nguoidung.TenPhong;
                    ng.Image = nguoidung.Images;
                    //ng.TenNhomNguoiDung = nguoidung.TenNhomNguoiDung;
                    SetNguoiDung(ng);
                }
                else
                {
                    ng.NguoiDung = -1;
                    ng.NhanSu = -1;
                }
            }
            return ng;
        }
    }
}