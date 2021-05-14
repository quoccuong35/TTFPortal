using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.Threading.Tasks;
using System.Web.Libs;

namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "49=1,0=0")]
    [Authorize]
    public class KyCongController : Controller
    {
        [RoleAuthorize(Roles = "49=1,0=0")]
        // GET: KyCong
        public ActionResult QLKyCong()
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> GetKyCongNhanSu(int? nam) {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_GetKyCongNhanSu(nam).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> Add_KyCong(Proc_GetKyCongNhanSu_Result item) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs,JsonRequestBehavior.AllowGet);
            }
            using (var db = new TTF_FACEIDEntities())
            {
                var KiemTra = db.TTF_TimekeepingPeriod.FirstOrDefault(it => it.Year == item.Year && it.Month == item.Month);
                if (KiemTra != null)
                {
                    rs.text = "Tháng tính công bạn đang thêm đã tồn tại trong hệ thống";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                TTF_TimekeepingPeriod add = new TTF_TimekeepingPeriod();
                add.Status = true;
                add.EmployeeStatus = true;
                add.Month = item.Month;
                add.Year = item.Year;
                add.FromDate = DateTime.Parse(item.TuNgay);
                add.ToDate = DateTime.Parse(item.DenNgay);
                db.TTF_TimekeepingPeriod.Add(add);
                if (db.SaveChanges() > 0)
                {
                    rs.code = 1;
                    rs.text = "Thành công";
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> EditKyCong(Proc_GetKyCongNhanSu_Result edit) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text =("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                try
                {
                    db.GhiChu = "Sửa kỳ công nhân sự";
                    var model = db.TTF_TimekeepingPeriod.FirstOrDefault(it => it.Year == edit.Year && it.Month == edit.Month);
                    int iThangHienTai = DateTime.Now.Month;
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NguoiDung1 = (int)nguoidung.NguoiDung;
                    if (edit.Status == true && model.Status == false) // kiểm tra trạng thái đang đóng mà mở
                    {

                        if (model.Month == iThangHienTai || model.Month + 1 == iThangHienTai)
                        {
                            model.Status = edit.Status;
                            model.LastChangedBy = 1;
                            model.LastChangedOn = DateTime.Now;
                            model.LastChangedBy = NguoiDung1;
                            db.SaveChanges();
                            rs.text = "Mở tháng tính công thành công";
                            rs.code = 1;
                        }
                        else
                        {
                            rs.text = ("Tháng tính công bạn đang chọn vượt quá thời gian cho phép mở");
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (edit.Status != null)
                            model.Status = edit.Status;
                        model.FromDate = DateTime.Parse(edit.TuNgay);
                        model.ToDate = DateTime.Parse(edit.DenNgay);
                        model.LastChangedOn = DateTime.Now;
                        model.LastChangedBy = NguoiDung1;
                        db.SaveChanges();
                        rs.text = ("Cập nhật thông tin thành công");
                        rs.code = 1;
                    }
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                    rs.code = 0;
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "49=1,0=0")]
        public ActionResult KyCongNguoiDung()
        {
            return View();
        }
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> GetKyCongNguoiDung(int? nam,int? thang)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_GetKyCongNguoiDung(nam,thang).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> Add_KyCongNguoiDung(Proc_GetKyCongNguoiDung_Result item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new TTF_FACEIDEntities())
            {
                DateTime dtTuNgay = DateTime.Parse(item.TuNgay), dtDenNgay = DateTime.Parse(item.DenNgay);
                var checkTuNgay = db.TTF_KyCongNguoiDung.Where(it => it.DenNgay >= dtTuNgay).ToList();
                var checkDenNgay = db.TTF_KyCongNguoiDung.Where(it => it.DenNgay >= dtDenNgay).ToList();
                if (checkTuNgay.Count > 0 || checkDenNgay.Count > 0 || dtTuNgay >= dtDenNgay)
                {
                    rs.text = ("Từ ngày hoặc đến ngày không hợp lệ");
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                TTF_KyCongNguoiDung add = new TTF_KyCongNguoiDung();
                add.Dong = item.Dong == null?false:item.Dong;
                add.Thang = item.Thang;
                add.Nam = item.Nam;
                add.TuNgay = dtTuNgay;
                add.DenNgay = dtDenNgay;
                db.TTF_KyCongNguoiDung.Add(add);
                if (db.SaveChanges() > 0)
                {
                    rs.code = 1;
                    rs.text = "Thành công";
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "49=1,0=0")]
        public async Task<JsonResult> EditKyCongNguoiDung(Proc_GetKyCongNguoiDung_Result item) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text= ("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa kỳ công người dùng";
                try
                {
                    var model = db.TTF_KyCongNguoiDung.FirstOrDefault(it => it.Id == item.Id);
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    DateTime dtTuNgay = DateTime.Parse(item.TuNgay), dtDenNgay = DateTime.Parse(item.DenNgay);
                    var checkTuNgay = db.TTF_KyCongNguoiDung.Where(it => it.DenNgay >= dtTuNgay && it.Id != item.Id).ToList();
                    var checkDenNgay = db.TTF_KyCongNguoiDung.Where(it => it.DenNgay >= dtDenNgay && it.Id != item.Id).ToList();
                    if (checkTuNgay.Count > 0 || checkDenNgay.Count > 0 || dtTuNgay >= dtDenNgay)
                    {
                        rs.text = ("Từ ngày đến ngày không hợp lệ hoặc tồn tại kỳ công có thời gian trên");
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    model.Dong = item.Dong;
                    model.TuNgay = dtTuNgay;
                    model.DenNgay = dtDenNgay;
                    if (item.Dong == true)
                    {
                        model.NguoiDungDong = (int)nguoidung.NguoiDung;
                        model.NgayDong = DateTime.Now;
                    }
                    else
                    {
                        model.NguoiDungMo = (int)nguoidung.NguoiDung;
                        model.NgayMo = DateTime.Now;
                    }

                    if (db.SaveChanges() > 0)
                    {
                        rs.text =  ("Thành công");
                        rs.code = 1;
                    }
                    else
                    {
                        rs.text = ("Không có dữ liệu cập nhật");
                    }
                }
                catch (Exception ex)
                {

                    rs.text = ("Lỗi " + ex.Message);
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
    }
}