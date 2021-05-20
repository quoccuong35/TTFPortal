using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.Threading.Tasks;

namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "0=0,13=1")]
    [Authorize]

    public class QLDanhMucController : Controller
    {
        [RoleAuthorize(Roles = "0=0,13=1")]
        // GET: QLDanhMuc
        public ActionResult DMPhongBan()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,13=1")]
        public async Task<JsonResult> GetDMPhongBan() {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                using (var db = new TTF_FACEIDEntities())
                {
                    rs.data = db.TTF_PhongBan_PhanXuong.ToList();
                    rs.code = 1;
                }
            }
            catch (Exception ex)
            {
                rs.text = ex.ToString();
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "0=0,13=2,13=3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SavePhongBan(TTF_PhongBan_PhanXuong item, int loai)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Sửa phòng ban";
                    if (loai == 1)
                    {
                        if (!User.IsInRole("0=0") && !User.IsInRole("13=2"))
                        {
                            rs.text = "Bạn không có quyền thêm";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        var check = db.TTF_PhongBan_PhanXuong.FirstOrDefault(it => it.MaPhong_PhanXuong.Trim() == item.MaPhong_PhanXuong.Trim());
                        if (check != null)
                        {
                            rs.text = "Mã Phòng/Phân xưởng " + item.MaPhong_PhanXuong + " đã tồn tại không thể thêm";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        item.Del = false;
                        db.TTF_PhongBan_PhanXuong.Add(item);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.data = "Thành công";
                    }
                    else
                    {
                        if (!User.IsInRole("0=0") && !User.IsInRole("13=3"))
                        {
                            rs.text = "Bạn không có quyền sửa";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        var check = db.TTF_PhongBan_PhanXuong.FirstOrDefault(it => it.MaPhong_PhanXuong.Trim() == item.MaPhong_PhanXuong.Trim());
                        if(check != null)
                        {
                            check.TenPhongRutGon = item.TenPhongRutGon;
                            check.TenPhong_PhanXuong = item.TenPhong_PhanXuong.Trim();
                            check.MaKhoi = item.MaKhoi;
                            check.Del = item.Del;
                            check.CostCenter = item.CostCenter;
                            db.SaveChanges();
                            rs.code = 1;
                            rs.data = "Thành công";
                        }
                    }
                }
                
            }
            catch (Exception ex )
            {
                rs.code = 0;
                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "0=0,13=4")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DelPhongBan(string ma)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Xóa phòng ban";
                    var check = db.TTF_PhongBan_PhanXuong.FirstOrDefault(it => it.MaPhong_PhanXuong.Trim() == ma);
                    if (check != null)
                    {
                        db.TTF_PhongBan_PhanXuong.Remove(check);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                    }
                }

            }
            catch (Exception ex)
            {
                rs.code = 0;
                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
    }
}