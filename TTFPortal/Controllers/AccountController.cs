using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.Web.Security;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace TTFPortal.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public AccountController()
        {
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult GetLogin(string Username, string Password)
        {
            string[] rs = { "1", Url.Action("Index", "Home") };
            try
            {
                var db = new TTF_FACEIDEntities();
                string sTenDangNhap = Username.ToLower(), sMatKhau = clsSecurity.Encrypt(Password, "CuongLQ");
                if (sTenDangNhap.Equals("administrator"))
                {
                    var item = Task.Run(() => db.HT_NGUOIDUNG.FirstOrDefault(m => m.TAIKHOAN == sTenDangNhap && m.MATKHAU == sMatKhau)).Result;
                    if (item != null)
                    {
                        FormsAuthentication.SetAuthCookie(sTenDangNhap, true);
                        var ng = new NguoiDungModel();
                        ng.NguoiDung = item.NGUOIDUNG;
                        ng.TenHienThi = item.HoTen;
                        ng.TaiKhoan = item.TAIKHOAN;
                        ng.NhomNguoiDung = item.NHOMNGUOIDUNG.Value;
                        ng.NhanSu = item.NhanSu.Value;
                        ng.MaPhongPhanXuong = "";
                        Users.SetNguoiDung(ng);
                    }
                    else
                    {
                        rs[0] = "0";
                        rs[1] = "Thông tin đăng nhập không đúng.";
                    }
                }
                else
                {
                    string dataname = clsFunction.GetDBName().ToLower();
                    if (dataname == "ttf_faceid") // bo qua mat khau
                    {
                        var item = db.V_Users.FirstOrDefault(it => it.TAIKHOAN == sTenDangNhap);
                        if (item != null)
                        {
                            FormsAuthentication.SetAuthCookie(sTenDangNhap, true);
                            var ng = new NguoiDungModel();
                            ng.NguoiDung = item.NGUOIDUNG;
                            ng.TenHienThi = item.HoVaTen;
                            ng.TaiKhoan = item.TAIKHOAN;
                            ng.NhomNguoiDung = item.NHOMNGUOIDUNG.Value;
                            ng.NhanSu = item.NhanSu.Value;
                            ng.MaPhongPhanXuong = item.MaPhong_PhanXuong;
                            ng.Image = item.Images;
                            ng.MaNV = item.MaNV;
                            ng.TenPhongBan = item.TenPhong;
                            ng.PhamVi = item.MaPhong_PhanXuong.Trim() + "," + string.Join(",", (db.TTF_PhamVi.Where(it => it.NhanSu == ng.NhanSu).Select(it => it.MaPhong_PhanXuong.Trim())).ToArray());
                            Users.SetNguoiDung(ng);
                        }
                        else
                        {
                            rs[0] = "0";
                            rs[1] = "Thông tin đăng nhập không đúng.";
                        }

                    }
                    else
                    {
                        try
                        {
                            string sDomian;
                             //sDomian = "truongthanh.com";
                            sDomian = "10.0.2.1";
                            var context = new PrincipalContext(ContextType.Domain, sDomian, sTenDangNhap.ToLower().Trim(), Password);
                            var searcher = new PrincipalSearcher(new UserPrincipal(context));
                            var users = searcher.FindAll().Where(u => u.SamAccountName.ToLower() == "" + sTenDangNhap.ToLower() + "").ToList();
                            if (users.Count == 1)
                            {
                                FormsAuthentication.SetAuthCookie(sTenDangNhap, true);
                                var item = db.V_Users.FirstOrDefault(it => it.TAIKHOAN == sTenDangNhap);
                                var ng = new NguoiDungModel();
                                ng.NguoiDung = item.NGUOIDUNG;
                                ng.TenHienThi = item.HoVaTen;
                                ng.TaiKhoan = item.TAIKHOAN;
                                ng.NhomNguoiDung = item.NHOMNGUOIDUNG.Value;
                                ng.NhanSu = item.NhanSu.Value;
                                ng.MaPhongPhanXuong = item.MaPhong_PhanXuong; ;
                                ng.MaNV = item.MaNV;
                                ng.TenPhongBan = item.TenPhong;
                                ng.PhamVi = item.MaPhong_PhanXuong.Trim() + ","+string.Join(",", (db.TTF_PhamVi.Where(it => it.NhanSu == ng.NhanSu).Select(it => it.MaPhong_PhanXuong.Trim())).ToArray());
                                ng.Image = item.Images;
                                Users.SetNguoiDung(ng);
                            }
                            else
                            {
                                var dt = db.HT_NGUOIDUNG.Where(p => p.TAIKHOAN == sTenDangNhap);
                                if (dt.ToList().Count > 0)
                                {
                                    rs[0] = "0";
                                    rs[1] = "Bạn chưa được phân quyền liên hệ P.GPHT để được phân quyền.";
                                    //ModelState.AddModelError("", "Bạn chưa được phân quyền liên hệ P.GPHT để được phân quyền");
                                }
                                else
                                {
                                    rs[0] = "0";
                                    rs[1] = "Lỗi hệ thống hãy liên hệ nhà quản trị.";
                                }
                            }
                        }
                        catch 
                        {
                            rs[0] = "0";
                            rs[1] = "Sai tên đăng nhập hoặc mật khẩu";

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rs[0] = "0";
                rs[1] = "Lỗi hệ thống hãy liên hệ nhà quản trị." + ex.Message;
            }
            return Json(rs);
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}