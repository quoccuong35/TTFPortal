using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TTFPortal.Class;
using TTFPortal.Models;

namespace TTFPortal.Controllers
{
    public class HeThongController : Controller
    {
        // GET: HeThong
        public ActionResult Index()
        {
            return View();
        }
        public async Task<JsonResult> TimNhanVien(string MaNV)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
               
                JsonStatus js = new JsonStatus();
                js.code = 0;
              
                try
                {
                    var ng = Users.GetNguoiDung(User.Identity.Name);
                    if (User.IsInRole("44=1"))
                    {
                        js.data = (from nhansu in db.TTF_NhanSu
                                   join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                   join calamviec in db.TTF_CaLamViec on nhansu.MaCaLamViec equals calamviec.MaCaLamViec
                                   where nhansu.MaNV == MaNV
                                         && nhansu.MaTinhTrang == "1"
                                   select new NhanVien { MaNV = nhansu.MaNV, HoVaTen = nhansu.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, NhanSu = nhansu.NhanSu, SoNgayPhepConlai = nhansu.SoNgayPhepConLai, GioVao = calamviec.GioBacDau, GioRa = calamviec.GioKetThuc }).First();
                        js.code =   1;
                    }
                    else
                    {
                        List<string> PhamVi = new List<string>();
                        PhamVi = db.TTF_PhamVi.Where(it => it.NhanSu == ng.NhanSu).Select(it => it.MaPhong_PhanXuong).ToList();
                        PhamVi.Add(ng.MaPhongPhanXuong);
                        js.data = (from nhansu in db.TTF_NhanSu
                                   join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                   join calamviec in db.TTF_CaLamViec on nhansu.MaCaLamViec equals calamviec.MaCaLamViec
                                   where nhansu.MaNV == MaNV
                                        && PhamVi.Contains(nhansu.MaPhong_PhanXuong.Trim()) && nhansu.MaTinhTrang == "1"
                                   select new NhanVien { MaNV = nhansu.MaNV, HoVaTen = nhansu.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, NhanSu = nhansu.NhanSu, SoNgayPhepConlai = nhansu.SoNgayPhepConLai, GioVao = calamviec.GioBacDau , GioRa = calamviec.GioKetThuc }).First();
                        js.code = 1;
                    }
                }
                catch (Exception ex)
                {
                    js.text = "Không tìm thấy nhân sự " + ex.Message;
                    js.code = 0;
                }
                return Json(js, JsonRequestBehavior.AllowGet);
            }
        }

        
    }
   
}