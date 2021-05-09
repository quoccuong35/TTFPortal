using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.Text;
using System.Transactions;

namespace TTFPortal.Controllers
{
    [RoleAuthorize]
    public class XacNhanCongController : Controller
    {
        // GET: XacNhanCong
        [RoleAuthorize(Roles = "0=0,45=1")]
        public ActionResult XacNhanCongCuaBan()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,45=1")]
        [HttpGet]
        public async Task<JsonResult> GVXacNhanCongCuaBan(string TuNgay,string DenNgay)
        {
            using (var db = new SaveDB())
            {
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                var model = db.Proc_QuaTrinhXacNhanCong((int)nguoidung.NguoiDung, TuNgay, DenNgay).ToList();
                 var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }
        }
        [RoleAuthorize(Roles = "0=0,45=2")]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,45=2")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult>  Add_XacNhanCong(XacNhanCong item)
        {

            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Thêm xác nhận công";
                try
                {
                    if (User.Identity.Name == null || User.Identity.Name == "")
                    {
                        rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                        return Json(rs,JsonRequestBehavior.AllowGet);
                    }
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NhanSu = (int)nguoidung.NhanSu;
                    if (NhanSu < 0)
                    {
                        rs.text = "Tài khoản bạn đang dùng chưa có gán cho thông tin nhân viên không thể tạo";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    if (clsFunction.checkKyCongNguoiDung(item.Ngay))
                    {
                        rs.text =("Kỳ tính công đã đóng không thể thêm xác nhận công vào hệ thống");
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    string sGioVao = item.GioVao == true ? "1" : "0";
                    var tempTest = db.Database.SqlQuery<TTF_XacNhanCong>("Select * from TTF_XacNhanCong Where Convert(Date,Ngay) = '" + item.Ngay.Date.ToString("MM/dd/yyyy") + "' And ISNULL(GioVao,0) = '" + sGioVao + "' And NhanSu = '" + item.NhanSu + "' And IsNull(Del,0) =0 And MaTrangThaiDuyet != '4' ").ToList();
                    if (tempTest.Count > 0)
                    {
                        rs.text = ("Ngày xác nhận công đã cập nhật trước đó không thể lưu");
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    //DateTime temp = (DateTime)item.Ngay;
                    //int Ngay = temp.Day;
                    //int Thang = temp.Month;
                    //DateTime tuNgay = new DateTime(), denNgay = new DateTime();

                    //// kiem tra qua 5 lần ko cho lưu
                    //if (Ngay > 25 && Thang < 12)
                    //{
                    //    tuNgay = new DateTime(temp.Year, Thang, 26);
                    //    denNgay = new DateTime(temp.Year, Thang + 1, 25);
                    //}
                    //else if (Ngay < 26 && Thang < 12)
                    //{
                    //    tuNgay = new DateTime(temp.Year, Thang - 1, 26);
                    //    denNgay = new DateTime(temp.Year, Thang, 25);
                    //}
                    //else if (Ngay > 25 && Thang == 12)
                    //{
                    //    tuNgay = new DateTime(temp.Year, Thang, 26);
                    //    denNgay = new DateTime(temp.Year + 1, 1, 25);
                    //}
                    //else if (Ngay < 26 && Thang == 12)
                    //{
                    //    tuNgay = new DateTime(temp.Year, Thang - 1, 26);
                    //    denNgay = new DateTime(temp.Year, Thang, 25);
                    //}
                    //tempTest = db.Database.SqlQuery<TTF_XacNhanCong>("Select * from TTF_XacNhanCong Where Convert(Date,Ngay) BETWEEN '" + tuNgay.ToString("MM/dd/yyyy") + "' And '" + denNgay.ToString("MM/dd/yyyy") + "' And NhanSu = '" + item.NhanSu + "'  And IsNull(Del,0) =0 And MaTrangThaiDuyet != '4' ").ToList();
                    //if (tempTest.Count > 4)
                    //{
                    //    return Content("Số lần xác nhận công vượt quá 5 lần trong tháng không thể lưu");
                    //}

                    TTF_XacNhanCong add = new TTF_XacNhanCong();
                    add.NguoiTao = (int)nguoidung.NguoiDung;
                    add.NgayTao = DateTime.Now;
                    add.MayTao = System.Net.Dns.GetHostName();
                    add.MaTrangThaiDuyet = "1";
                    //add.IDNguoiDuyetKeTiep = nguoiduyetketiep;
                    add.Ngay = item.Ngay;
                    add.GioVao = item.GioVao;
                    add.NguyenNhan = item.NguyenNhan;
                    add.NhanSu = item.NhanSu;
                    add.ThoiGian = new TimeSpan(Convert.ToDateTime(item.ThoiGian).Hour, Convert.ToDateTime(item.ThoiGian).Minute, 0);
                    add.XacNhanCong = false;
                    add.CaDem = item.CaDem == null ? false : item.CaDem;
                    db.TTF_XacNhanCong.Add(add);

                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                    rs.description = add.IDXacNhanCong.ToString();
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Add_XacNhanCong");
                    rs.text = ("Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString());
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [RoleAuthorize(Roles = "0=0,45=3")]
        public ActionResult Edit(long? id, int? op)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Content("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
            }
            if (id == null)
            {
                return Content("Chưa chọn thông tin xác nhận công");
            }
            else
            {
                using (var db = new SaveDB())
                {
                    var item = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == id);
                    var NhanSu = (from nhansu in db.TTF_NhanSu
                                  join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                  where nhansu.NhanSu == item.NhanSu && nhansu.MaTinhTrang == "1"
                                  select new { nhansu.MaNV, nhansu.HoVaTen, pb.TenPhong_PhanXuong }).ToList();
                    if (NhanSu == null)
                    {
                        return Content("Không tìm thấy thông tin xác nhận công " + id.ToString());
                    }
                    XacNhanCong item1 = new XacNhanCong();
                    item1.NhanSu = item.NhanSu;
                    item1.Ngay = item.Ngay;
                    item1.GioVao = item.GioVao;
                    item1.NguyenNhan = item.NguyenNhan;
                    item1.HoVaTen = NhanSu[0].HoVaTen;
                    item1.MaNhanVien = NhanSu[0].MaNV;
                    item1.IDXacNhanCong = item.IDXacNhanCong;
                    item1.MaTrangThaiDuyet = item.MaTrangThaiDuyet;
                    item1.ThoiGian = item.ThoiGian.ToString();
                    item1.TenPhong_PhanXuong = NhanSu[0].TenPhong_PhanXuong;
                    item1.Block = item.Block;
                    item1.NguoiTao = item.NguoiTao;
                    item1.IDNguoiDuyetKeTiep = item.IDNguoiDuyetKeTiep;
                    ViewBag.OP = op;
                    return View(item1);
                }
            }
           
        }

        [RoleAuthorize(Roles = "0=0,45=3")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> Save_XacNhanCong(XacNhanCong item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
               
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = ("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
                    return Json (rs,JsonRequestBehavior.AllowGet);
                }
                if (clsFunction.checkKyCongNguoiDung(item.Ngay))
                {
                    rs.text = ("Kỳ tính công đã đóng không thể cập nhật xác nhận công vào hệ thống");
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Sửa xác nhận công";
                    var model = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == item.IDXacNhanCong);
                    if (model.Block != true)
                    {
                        var check = db.TTF_XacNhanCong.Where(it => it.NhanSu == item.NhanSu && it.GioVao == item.GioVao && it.CaDem == item.CaDem && it.Ngay == item.Ngay && it.IDXacNhanCong != item.IDXacNhanCong && it.Del != true && it.MaTrangThaiDuyet != "4").ToList();
                        if (check.Count > 0)
                        {
                            rs.text = "Có tồn tại xác nhận công đang sửa không thể lưu";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                        model.NguyenNhan = item.NguyenNhan;
                        model.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                        model.NgayThayDoiLanCuoi = DateTime.Now;
                        model.ThoiGian = new TimeSpan(Convert.ToDateTime(item.ThoiGian).Hour, Convert.ToDateTime(item.ThoiGian).Minute, 0);
                        model.GioVao = item.GioVao;
                        model.CaDem = item.CaDem == null ? false : item.CaDem;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            rs.text = ("Thành công");
                            rs.code = 1;
                            rs.description = item.IDXacNhanCong.ToString();
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            rs.text = ("Thất bại");
                            rs.code = 0;
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        rs.text = ("Thông tin bạn đang chọn đã khóa không thể cập nhật");
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Save_XacNhanCong");
                rs.text = ("Lỗi trong quá trình cập nhật liên hệ phòng HTTT để được hỗ trợ");
                return Json(rs, JsonRequestBehavior.AllowGet);
            }

        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult GuiMail(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            HT_HETHONG ht = null;
            try
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Gửi mail";
                    var model = db.TTF_XacNhanCong;
                    var item = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == id);
                    if (item.Del == true)
                    {
                        rs.text = "Xác nhận công đã xóa không thể gửi mail";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    if (clsFunction.checkKyCongNguoiDung(item.Ngay))
                    {
                        rs.text = "Kỳ tính công đã đóng không thể gửi mail xác nhận công";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    if (item.Block != true)
                    {
                        int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(item.NhanSu, 0, "XNC", 0);
                        if (nguoiduyetketiep < 1)
                        {
                            rs.text = "Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu";
                        }
                        var nhanSu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                        var cc = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(item.NguoiTao));


                        var to = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == (int)nguoiduyetketiep);
                        string body = GetEmailDuyetString("XNC", item, nhanSu, to);

                        ht = clsFunction.Get_HT_HETHONG();
                        clsFunction.GuiMail(ht.MailTitleXNC, to.MailCongTy, cc.MailCongTy, body);

                        //if (clsFunction.GetDBName() == "TTF_FACEID")
                        //{
                        //    clsFunction.GuiMail("HRMS - Xác nhận công", to.MailCongTy, cc.MailCongTy, body);
                        //}
                        //else
                        //{
                        //    clsFunction.GuiMail("Test HRMS - Xác nhận công", to.MailCongTy, cc.MailCongTy, body);
                        //}

                        item.Block = true;
                        item.NgayBlock = DateTime.Now;
                        item.IDNguoiDuyetKeTiep = nguoiduyetketiep;
                        item.MaTrangThaiDuyet = "2";
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            i = clsFunction.LuuLichSuDuyet(item.IDXacNhanCong, "XNC", false, (int)item.IDNguoiDuyetKeTiep, DateTime.Now);
                            if (i > 0)
                            {
                                rs.text = "Thành công";
                                rs.code = 1;
                                rs.description = item.IDXacNhanCong.ToString();
                            }
                            else
                            {
                                rs.text = "Thất bại";
                            }
                        }
                       // rs = "Thành công";
                    }
                    else
                    {
                        rs.text = "Thông tin này đã gửi mail xác nhận công không thể gửi lại";
                    }

                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "GuiMail");
                rs.text = "Đã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public string GetEmailDuyetString(string Loai, TTF_XacNhanCong item, TTF_NhanSu NguoiCanXacNhanCong, TTF_NhanSu nhanSuDuyet)
        {
            string rv = "";
            if (Loai == "XNC")
            {
                using (var db = new SaveDB())
                {
                    string body = "";
                    string NguoiNhan = "";
                    string NguoiTao = "";
                    var HeThong = db.HT_HETHONG.ToList();
                    if (nhanSuDuyet != null)
                    {
                        NguoiNhan = nhanSuDuyet.HoVaTen;
                    }

                    //var vitem = db.v_TangCa.Where(it => it.IDNghiPhep == id).FirstOrDefault();
                    if (NguoiCanXacNhanCong != null)
                    {
                        NguoiTao = NguoiCanXacNhanCong.HoVaTen;
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiNhan + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị có yêu cầu cần xác nhận công được từ <b>" + NguoiTao + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Ngày: <b>" + Convert.ToDateTime(item.Ngay).Date.ToString("dd/MM/yyyy") + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Thời gian: <b>" + item.ThoiGian.ToString() + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nguyên nhân:<b>" + item.NguyenNhan.ToString() + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống xác nhận công xem thông tin chi tiết hơn và xem xét duyệt yêu cầu này (Click vào link sau để vào hệ thống)</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>" + HeThong[0].WEBSITE + "/XacNhanCong/DuyetPublic?NguoiDuyet=" + nhanSuDuyet.MailCongTy.ToLower().Replace("@truongthanh.com", "") + "</td>");
                    //sb.Append("<td>" + "http://localhost:9666/XacNhanCong/DuyetPublic?NguoiDuyet="+ nhanSuDuyet.MailCongTy.ToLower().Replace("@truongthanh.com","") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
            }
            return rv;
        }

        [HttpPost]
        [RoleAuthorize(Roles = "0=0,45=4")]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                using (var db = new SaveDB())
                {
                    //  var model = db.TTF_XacNhanCong;
                    if (User.Identity.Name == null || User.Identity.Name == "")
                    {
                        rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                    var model = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == id);
                    if (model.Block != true)
                    {
                        var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                        // model.Remove(item);
                        model.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                        model.NgayThayDoiLanCuoi = DateTime.Now;
                        model.Del = true;
                        model.MaTrangThaiDuyet = "4";
                        int sc = db.SaveChanges();
                        if (sc > 0)
                        {
                            rs.text = "Thành công";
                            rs.code = 1;
                        }
                        else
                            rs.text = "Thất bại";
                    }
                    else
                    {
                        rs.text = "Thông tin này đã gửi mail xác nhận công không thể xóa";
                    }
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Delete");
                rs.text = "Lỗi trong quá trình xóa liên hệ phòng HTTT để được hỗ trợ";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DuyetPublic(string NguoiDuyet)
        {
            if ((User.Identity.Name == null || User.Identity.Name == ""))
            {
                //Session["Url"] = "/XacNhanCong/DuyetXacNhanCong";
                return RedirectToAction("Login", "Account");

            }
            else if (User.Identity.Name.ToLower() != NguoiDuyet)
            {
                //Session["Url"] = "/XacNhanCong/DuyetXacNhanCong";
                return RedirectToAction("Logout", "Account");
            }
            else
            {
                return RedirectToAction("DuyetXacNhanCong", "XacNhanCong");
            }
        }

        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult QuanLyXacNhanCong()
        {
            return View();
        }
        [HttpGet]
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> GetQuanLyXacNhanCong(string tuNgay, string denNgay, string maPhongBan, string maNV)
        {
            using (var db = new SaveDB())
            {
                if (tuNgay == null || tuNgay == "")
                {
                    tuNgay = "1990-10-26";
                }
                if (denNgay == null || denNgay == "")
                {
                    denNgay = DateTime.Now.ToString("yyyy-MM-dd");
                }
                var model = db.Proc_QuanLyXacNhanCong(tuNgay, denNgay, maPhongBan.Trim(), maNV.Trim(), false).ToList();
                 var js = Json(model, JsonRequestBehavior.AllowGet);
                js.MaxJsonLength = int.MaxValue;
                return js;
            }
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Huy_NhanSuXacNhanCong(XacNhanCong item)
        {
            var rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs,JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                var model = db.TTF_XacNhanCong;
                db.GhiChu = "Hủy xác nhận công nhân sự";
                HT_HETHONG ht = null;
                try
                {
                    var ditem = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == item.IDXacNhanCong && it.XacNhanCong != true);
                    string body = "";
                    string To = "";
                    if (clsFunction.checkKyCongNhanSu(ditem.Ngay))
                    {
                        rs.text = "Kỳ tính công đã đóng không thể hủy xác nhận công";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    //if (ditem.Block == true)
                    //{
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int iNhanSu = nguoidung.NhanSu;
                    if (iNhanSu == -1)
                    {
                        rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    //clsFunction.CapNhatTuChoiVaoLichSuDuyet(item.IDXacNhanCong, "XNC", iNhanSu, item.LyDoHuy);
                    var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                    var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));// thong tin nguoi tao
                    To = NguoiTao.MailCongTy;
                    ditem.LyDoHuy = item.LyDoHuy;
                    ditem.IDNguoiDuyetKeTiep = iNhanSu;

                    body = GetEmailHuyXacNhanNhanSu("XNC", nhansu, ditem, iNhanSu);

                    ht = clsFunction.Get_HT_HETHONG();
                    clsFunction.GuiMail(ht.MailTitleXNC, To, "", body);

                    //if (clsFunction.GetDBName() == "TTF_FACEID")
                    //{
                    //    clsFunction.GuiMail("HRMS - Xác nhận công", To, "", body);
                    //}
                    //else
                    //{
                    //    clsFunction.GuiMail("Test HRMS - Xác nhận công", To, "", body);
                    //}

                    // Cập nhật duyệt thành công
                    ditem.MaTrangThaiDuyet = "4";
                    ditem.IDNguoiDuyetKeTiep = -1;
                    ditem.LyDoHuy = item.LyDoHuy;
                    ditem.NguoiTuChoi = iNhanSu;
                    ditem.NgayTuChoi = DateTime.Now;
                    int irs = db.SaveChanges();
                    if (irs > 0)
                    {
                        rs.text = "Thành công";
                        rs.code = 1;
                    }
                    else
                    {
                        rs.text = "Không có thông tin cập nhật";
                    }
                    //}
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Huy_NhanSuXacNhanCong");
                    rs.text = "Đã có lỗi trong quá trình duyệt xác nhận công. Liên hệ P.HTTT để được hỗ trợ";
                }
            }
            return Json(rs,JsonRequestBehavior.AllowGet);
        }
        public string GetEmailHuyXacNhanNhanSu(string Loai, TTF_NhanSu NhanSu, TTF_XacNhanCong item, int iNhanSu)
        {
            string rv = "";
            using (var db = new SaveDB())
            {
                if (Loai == "XNC")
                {

                    string body = "", nguoiduyet = "", NguoiTao = "";
                    var ThongTinNguoiHuy = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == iNhanSu);
                    var HeThong = db.HT_HETHONG.ToList();
                    if (ThongTinNguoiHuy != null)
                    {
                        nguoiduyet = ThongTinNguoiHuy.HoVaTen;
                    }
                    NguoiTao = NhanSu.HoVaTen;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiTao + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu xác nhận công bên dưới của anh/chị đã bị từ chối bởi <b>" + ThongTinNguoiHuy.HoVaTen + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do từ chối: <b>" + item.LyDoHuy + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nội dung yêu cầu xác nhận công</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Ngày: <b>" + Convert.ToDateTime(item.Ngay).Date.ToString("dd/MM/yyyy") + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Thời gian: <b>" + item.ThoiGian.ToString() + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nguyên nhân:<b>" + item.NguyenNhan.ToString() + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống xác nhận công để xem thông tin chi tiết (Click vào link sau để vào hệ thống)</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>" + HeThong[0].WEBSITE + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
            }
            return rv;
        }

        [RoleAuthorize(Roles = "0=0,46=1")]
        public ActionResult DuyetXacNhanCong() {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                //Session["Url"] = Request.Url.PathAndQuery;
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [RoleAuthorize(Roles = "0=0,46=1")]
        [HttpGet]
        public async Task<JsonResult> GetDuyetXacNhanCong(string tuNgay, string denNgay, string maPhongBan, string maNV)
        {
            using (var db = new SaveDB())
            {
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);

                if (nguoidung.NhanSu > 0)
                {
                    var model = db.Proc_GetDuyetXacNhanCong(nguoidung.NhanSu, tuNgay, denNgay, maPhongBan, maNV, false).ToList();
                    rs.data = model;
                    rs.code = 1;
                }
                else
                {
                    rs.text = "Hết thời gian thao tác xin đăng nhập lại";
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,46=1")]
        public JsonResult Duyet_XacNhanCongAll(List<long> lid)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
           
            int iSoLuong = 0;
            HT_HETHONG ht = clsFunction.Get_HT_HETHONG();
            List<GuiMail> listMail = new List<GuiMail>();
            string sName = "";
            if (lid != null && lid.Count > 0)
            {
                try
                {
                    // Duyet all
                    using (var db = new SaveDB())
                    {
                        db.GhiChu = "Duyệt nghỉ phép";
                        int iDong = 1;
                        string duyet = "";
                        using (var tran = new TransactionScope())
                        {
                            foreach (var idXacNhanCong in lid)
                            {
                                var ditem = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == idXacNhanCong);
                                string body = "";
                                string To = "";
                                if (clsFunction.checkKyCongNguoiDung(ditem.Ngay))
                                {
                                    rs.text = "Dòng thứ " + iDong.ToString() + " có kỳ công đã đóng không thể duyệt xác nhận công";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                                if (ditem.Block == true)
                                {
                                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                                    int NhanSu = (int)nguoidung.NhanSu;
                                    if (NhanSu == -1)
                                    {
                                        rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                                        return Json(rs, JsonRequestBehavior.AllowGet);
                                    }
                                    clsFunction.CapNhatDuyetVaoLichSuDuyet(idXacNhanCong, "XNC", NhanSu, "");
                                    var NguoiTao = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == ditem.NhanSu);
                                    sName = NguoiTao.HoVaTen;
                                    To = NguoiTao.MailCongTy;
                                    body = GetEmailThanhCong("XNC", NguoiTao, ditem);

                                    GuiMail a1 = new GuiMail();
                                    a1.MailTitle = ht.MailTitleNghiPhep;
                                    a1.To = To;
                                    a1.CC = "";
                                    a1.Body = body;
                                    listMail.Add(a1);
                                    // clsFunction.GuiMail(ht.MailTitleXNC, To, "", body);

                                    //if (clsFunction.GetDBName() == "TTF_FACEID")
                                    //{
                                    //    clsFunction.GuiMail("HRMS - Xác nhận công", To, "", body);
                                    //}
                                    //else
                                    //{
                                    //    clsFunction.GuiMail("Test HRMS - Xác nhận công", To, "", body);
                                    //}

                                    // Cập nhật duyệt thành công
                                    ditem.MaTrangThaiDuyet = "3";
                                    ditem.IDNguoiDuyetKeTiep = -1;

                                    try
                                    {
                                        db.SaveChanges();
                                        if (db.SaveChanges() > 0)
                                        {
                                            duyet += "<li>"+ sName + "</li>";
                                            iDong++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Duyet_XacNhanCong");
                                        rs.text = "Đã có lỗi trong quá trình duyệt xác nhận công nhân viên <b>" + sName + "</b>. Liên hệ P.HTTT để được hỗ trợ";
                                        return Json(rs, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            tran.Complete();
                            if (iDong > 0)
                            {
                                if (listMail.Count > 0)
                                {
                                    foreach (var item in listMail)
                                    {
                                        clsFunction.GuiMail(item.MailTitle, item.To, item.CC, item.Body);
                                    }
                                }
                                rs.text = MvcHtmlString.Create("Duyệt thành công " + iDong.ToString() + "</br>" + duyet).ToHtmlString();
                                rs.code = 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Duyet_XacNhanCong");
                    rs.text = "Đã có lỗi trong quá trình duyệt xác nhận công nhân viên <b>" + sName + "</b>. Liên hệ P.HTTT để được hỗ trợ";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            rs.text += "Số lượng duyệt thành công là <b>" + iSoLuong.ToString() + "</b>";
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        public string GetEmailThanhCong(string Loai, TTF_NhanSu NhanSu, TTF_XacNhanCong item)
        {
            using (var db = new SaveDB())
            {
                string rv = "";
                if (Loai == "XNC")
                {

                    string body = "", NguoiTao = "";
                    var HeThong = db.HT_HETHONG.ToList();

                    NguoiTao = NhanSu.HoVaTen;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiTao + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu xác nhận công bên dưới của anh/chị đã hoàn tất</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nội dung yêu cầu xác nhận công</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Ngày: <b>" + Convert.ToDateTime(item.Ngay).Date.ToString("dd/MM/yyyy") + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Thời gian: <b>" + item.ThoiGian.ToString() + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nguyên nhân:<b>" + item.NguyenNhan.ToString() + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống xác nhận công để xem thông tin chi tiết hơn (Click vào link sau để đăng nhập vào hệ thống )</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>" + HeThong[0].WEBSITE + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
                return rv;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,46=1")]
        [RoleAuthorize(Roles = "0=0,46=1")]
        public JsonResult Mass_Cancel_XacNhanCong(List<int> lid, string LyDo)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Json("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại", JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                db.GhiChu = "hủy xác nhận công";
                List<GuiMail> listMail = new List<GuiMail>();
                using (var tran = new TransactionScope())
                {
                    JsonStatus rs = new JsonStatus();
                    rs.code = 0;
                    int kq = 0;
                    HT_HETHONG ht = clsFunction.Get_HT_HETHONG();
                    try
                    {
                        foreach (var xnc in lid)
                        {
                            var ditem = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == xnc);
                            if (clsFunction.checkKyCongNguoiDung(ditem.Ngay))
                            {
                                rs.text = "Kỳ tính công đã đóng không thể hủy xác nhận công";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                            string body = "";
                            string To = "";

                            if (ditem.Block == true)
                            {
                                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                                int iNhanSu = (int)nguoidung.NhanSu;
                                if (iNhanSu == -1)
                                {
                                    rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                                    return Json(rs, JsonRequestBehavior.AllowGet); ;
                                }
                                clsFunction.CapNhatTuChoiVaoLichSuDuyet(ditem.IDXacNhanCong, "XNC", iNhanSu, LyDo);
                                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == ditem.NhanSu);
                                var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                                To = NguoiTao.MailCongTy;
                                ditem.LyDoHuy = LyDo;
                                ditem.IDNguoiDuyetKeTiep = iNhanSu;
                                body = GetEmailHuyXacNhan("XNC", nhansu, ditem);

                                GuiMail a1 = new GuiMail();
                                a1.MailTitle = ht.MailTitleNghiPhep;
                                a1.To = To;
                                a1.CC = "";
                                a1.Body = body;
                                listMail.Add(a1);
                                //clsFunction.GuiMail(ht.MailTitleXNC, To, "", body);

                                //if (clsFunction.GetDBName() == "TTF_FACEID")
                                //{
                                //    clsFunction.GuiMail("HRMS - Xác nhận công", To, "", body);
                                //}
                                //else
                                //{
                                //    clsFunction.GuiMail("Test HRMS - Xác nhận công", To, "", body);
                                //}

                                // Cập nhật duyệt thành công
                                ditem.MaTrangThaiDuyet = "4";
                                ditem.IDNguoiDuyetKeTiep = -1;
                                ditem.LyDoHuy = LyDo;
                                ditem.NguoiTuChoi = iNhanSu;
                                ditem.NgayTuChoi = DateTime.Now;
                                int irs = db.SaveChanges();
                                if (irs > 0)
                                {
                                    kq++;
                                }

                            }
                        }
                        tran.Complete();
                        if (kq > 0)
                        {
                            if (listMail.Count > 0)
                            {
                                foreach (var item in listMail)
                                {
                                    clsFunction.GuiMail(item.MailTitle, item.To, item.CC, item.Body);
                                }
                            }
                            rs.text = "Từ chối thành công " + kq.ToString() + " yêu cầu xác nhận công ";
                            rs.code = 1;
                        }
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "Mass_Cancel_XacNhanCong");
                        rs.text = "Đã có lỗi trong quá trình duyệt xác nhận công. Liên hệ P.HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
            }
        }
        public string GetEmailHuyXacNhan(string Loai, TTF_NhanSu NhanSu, TTF_XacNhanCong item)
        {
            using (var db = new SaveDB())
            {
                string rv = "";
                if (Loai == "XNC")
                {

                    string body = "", nguoiduyet = "", NguoiTao = "";
                    var ThongTinNguoiDuyet = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.IDNguoiDuyetKeTiep);
                    var HeThong = db.HT_HETHONG.ToList();
                    if (ThongTinNguoiDuyet != null)
                    {
                        nguoiduyet = ThongTinNguoiDuyet.HoVaTen;
                    }
                    NguoiTao = NhanSu.HoVaTen;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiTao + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu xác nhận công bên dưới của anh/chị đã bị từ chối bởi <b>" + ThongTinNguoiDuyet.HoVaTen + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do từ chối: <b>" + item.LyDoHuy + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nội dung yêu cầu xác nhận công</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Ngày: <b>" + Convert.ToDateTime(item.Ngay).Date.ToString("dd/MM/yyyy") + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Thời gian: <b>" + item.ThoiGian.ToString() + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nguyên nhân:<b>" + item.NguyenNhan.ToString() + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống xác nhận công để xem thông tin chi tiết (Click vào link sau để vào hệ thống)</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>" + HeThong[0].WEBSITE + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
                return rv;
            }
        }

        public ActionResult BaoCaoSoLanXacNhanCong() {
            return View();
        }
        [RoleAuthorize(Roles ="0=0,44-1")]
        [HttpGet]
        public async Task<JsonResult> BaoCaoSoLanXacNhanCongPartial(string TuNgay, string DenNgay, string MaPhongBan, int SoLan)
        {
            JsonStatus js = new JsonStatus();
            try
            {
                using (var db = new SaveDB())
                {
                    var model = db.Proc_BaoCaoXacNhanCong(TuNgay, DenNgay, MaPhongBan, SoLan, false).ToList();
                    js.code = 1;
                    js.data = model;
                    js.code = 0;
                }
            }
            catch (Exception ex)
            {
                js.code = 0;
                js.text = "Đã có lỗi liên hệ P.HTTT để được hỗ trợ" + ex.ToString() + TuNgay + DenNgay;
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "XNC", "BaoCaoSoLanXacNhanCongPartial");
               
            }
            return Json(js, JsonRequestBehavior.AllowGet); ;
        }
    }
}