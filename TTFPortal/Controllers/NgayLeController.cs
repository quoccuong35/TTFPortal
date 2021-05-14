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
    [RoleAuthorize(Roles = "52=1,0=0")]
    [Authorize]
    public class NgayLeController : Controller
    {
        [RoleAuthorize(Roles = "52=1,0=0")]
        // GET: NgayLe
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [RoleAuthorize(Roles = "52=1,0=0")]
        public async Task<JsonResult> GetNgayLe(int? nam)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_NgayLe(nam).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "52=1,0=0")]
        public async Task<JsonResult> Save_NgayLe(Proc_NgayLe_Result item)
        {
            // var KyCong = db.TTF_TimekeepingPeriod.FirstOrDefault(it => it.FromDate <= item.Ngay && item.Ngay <= it.ToDate);
            JsonStatus rs = new JsonStatus();
            rs.code = 0;

            DateTime ngay = DateTime.Parse(item.Ngay);
            if (clsFunction.checkKyCongNhanSu(ngay))
            {
                rs.text = "Kỳ công đã đóng không thể thêm";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                try
                {
                    db.GhiChu = "Sửa ngày lễ";
                    var model = db.TTF_NgayLe.FirstOrDefault(it => it.Date == ngay);
                    if (model != null)
                    {
                        model.GhiChu = item.GhiChu;
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                    }
                    else
                    {
                        TTF_NgayLe add = new TTF_NgayLe();
                        add.Date = ngay;
                        add.GhiChu = item.GhiChu;

                        db.TTF_NgayLe.Add(add);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                    }
                }
                catch (Exception ex)
                {

                    rs.code = 0;
                    rs.text = ex.Message;
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "52=1,0=0")]
        public async Task<JsonResult> XoaNgayLe(string ngay)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            DateTime dtemp;
            if (ngay == null || ngay == "")
            {
                rs.text = "Bạn chưa chọn ngày cần xóa";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            dtemp = DateTime.Parse(ngay);
            if (clsFunction.checkKyCongNhanSu(dtemp))
            {
                rs.text = "Kỳ công đã đóng không thể xóa";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                db.GhiChu = "Xóa ngày lễ";
                try
                {
                    var model = db.TTF_NgayLe.FirstOrDefault(it => it.Date == dtemp);
                    if (model != null)
                    {
                        db.TTF_NgayLe.Remove(model);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    else {
                        rs.text = "Không có thông tin để xóa";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                       
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                    rs.code = 0;
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}