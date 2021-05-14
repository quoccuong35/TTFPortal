using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.Data;
using System.Web.Libs;
using System.Text;
using System.Transactions;
using TTFPortal.Class;

namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "0=0,47=1,44=1,48=1")]
    [Authorize]
    public class TangCaController : Controller
    {
        // GET: TangCa
        [RoleAuthorize(Roles = "0=0,47=1")]
        public ActionResult TangCaCuaBan()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,47=1")]
        public ActionResult Create()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,47=1")]
        public async Task<JsonResult> GVTangCaCuaBan(string tuNgay, string denNgay)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                var model = db.Proc_TangCaCuaBan((int)nguoidung.NguoiDung, tuNgay, denNgay).ToList();
                var js = Json(model, JsonRequestBehavior.AllowGet);
                js.MaxJsonLength = int.MaxValue;
                return js;
            }

        }
        public JsonResult LoadDuAn()
        {
            TTF_TimeTracKingEntities dcCon = new TTF_TimeTracKingEntities();
            var model = (from duan in dcCon.TTF_DuAn
                         where (duan.MaLoaiDuAn.Trim() == "XK" || duan.MaLoaiDuAn.Trim() == "CT")
                         select new { duan.MaDuAn }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AddNhanVien(string maNV)
        {
            using (var db = new SaveDB())
            {
                var model = (from nv in db.TTF_NhanSu
                             join pb in db.TTF_PhongBan_PhanXuong on nv.MaPhong_PhanXuong equals pb.MaPhong_PhanXuong
                             where nv.MaTinhTrang == "1" && nv.Del != true && nv.MaNV == maNV.Trim()
                             select new { nv.MaNV, nv.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, nv.NhanSu, IDTangCa = 0 }).FirstOrDefault();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<JsonResult> ImportFileExcel(HttpPostedFileBase UploadedFile)
        {
            using (var db = new SaveDB())
            {
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                string sMaKhongTonTai = "";
                if (UploadedFile != null && UploadedFile.ContentLength > 0 && (Path.GetExtension(UploadedFile.FileName).Equals(".xlsx")))
                {
                    string fileName = UploadedFile.FileName;
                    fileName = User.Identity.Name + "_" + fileName;
                    string UploadDirectory = Server.MapPath("~/Content/upload/Temps/");
                    bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                    if (!folderExists)
                        System.IO.Directory.CreateDirectory(UploadDirectory);
                    string resultFilePath = UploadDirectory + fileName;
                    try
                    {
                        UploadedFile.SaveAs(resultFilePath);
                        List<string> MaNV = getDataTableFromExcel(resultFilePath);
                        System.IO.File.Delete(resultFilePath);
                        if (MaNV.Count > 0)
                        {
                            var model = (from nv in db.TTF_NhanSu
                                         join pb in db.TTF_PhongBan_PhanXuong on nv.MaPhong_PhanXuong equals pb.MaPhong_PhanXuong
                                         where nv.MaTinhTrang == "1" && nv.Del != true && MaNV.Contains(nv.MaNV)
                                         select new { nv.MaNV, nv.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, nv.NhanSu }).ToList();
                            List<string> temp = model.Select(it => it.MaNV).ToList();
                            if (temp.Count < MaNV.Count)
                            {
                                var test = MaNV.Where(it => temp.Contains(it.ToString()) == false).ToList();
                                if (test.Count > 0)
                                {
                                    foreach (var item in test)
                                    {
                                        sMaKhongTonTai += "Không tìm thấy mã <b>" + item + "</b> </br>";
                                    }
                                }
                            }
                            rs.data = model;
                            rs.code = 1;
                            rs.description = sMaKhongTonTai;

                        }
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(resultFilePath);
                        rs.text = ex.Message;
                        // ViewBag.Error = "Lỗi dòng: " + row + "\r\n" + ex.Message;
                    }
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public static List<string> getDataTableFromExcel(string path)
        {

            var existingFile = new FileInfo(path);
            using (var pck = new OfficeOpenXml.ExcelPackage(existingFile))
            {
                var ws = pck.Workbook.Worksheets.First();
                List<string> kq = new List<string>();
                bool hasHeader = true;
                //foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                //{
                //    tbl.Columns.Add(firstRowCell.Text.Replace(" ", "").Replace("/", "").Replace("-", "").ToLower());
                //}
                var startRow = hasHeader ? 2 : 1;
                string value = "";
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    value = wsRow[rowNum, 1].Text.Trim();
                    if (value == null || value == "")
                        break;
                    kq.Add(value);
                    //foreach (var cell in wsRow)
                    //{
                    //    row[cell.Start.Column - 1] = cell.Text.Trim();
                    //}
                    //tbl.Rows.Add(row);
                }

                return kq.Distinct().ToList();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,47=2")]
        public async Task<JsonResult> Add_TangCa(TangCa item)
        {
            using (var db = new SaveDB())
            {
                Int64 id = 0;
                TTF_TangCa objH = null;
                TTF_TangCaChiTiet objD = null;
                TimeSpan GioBatDau;
                TimeSpan GioKetThuc;
                string msg = "";
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                try
                {
                    GioBatDau = new TimeSpan(Convert.ToDateTime(item.GioBatDau).Hour, Convert.ToDateTime(item.GioBatDau).Minute, 0);
                    GioKetThuc = new TimeSpan(Convert.ToDateTime(item.GioKetThuc).Hour, Convert.ToDateTime(item.GioKetThuc).Minute, 0);
                    if (GioKetThuc < GioBatDau)
                    {
                        ViewBag.Error = "Thời gian tăng ca từ giờ đến giờ không họp lệ";
                        ViewBag.Action = "Add_TangCa";
                    }
                    ////////////////////////////////////////////////Validation////////////////////////////////////////////////////
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NhanSu = (int)nguoidung.NhanSu;
                    if (NhanSu == -1)
                    {
                        //return Content("Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên");
                        rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    //int? nguoiduyetketiep = clsUserActivesInternal.clsUserActive.GetCapQuanLyTrucTiep(User.Identity.Name);
                    //if (nguoiduyetketiep < 1)
                    //{
                    //    return Content("Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu");
                    //}
                    int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(NhanSu, 0, "TC", 0);
                    if (nguoiduyetketiep == -1)
                    {
                        //return Content("Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu");
                        rs.text = "Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu";

                        return Json(rs, JsonRequestBehavior.AllowGet); ;
                    }
                    if (item.TCChiTiet == null || item.TCChiTiet.Count == 0)
                    {
                        //return Content("Chưa gán nhân sự nào vào phiếu tăng ca");
                        rs.text = "Chưa gán nhân sự nào vào phiếu tăng ca";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    msg = KiemTraTangCa(NhanSu, item.NgayTangCa, GioBatDau, GioKetThuc, 0, item.MaPhong_PhanXuong);
                    if (msg != "")
                    {
                        //return Content(msg);
                        rs.text = msg;
                        return Json(rs, JsonRequestBehavior.AllowGet); ;
                    }

                    if (clsFunction.checkKyCongNguoiDung(item.NgayTangCa))
                    {
                        rs.text = "Kỳ công đã đóng";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                    if (item.DuAn == true)
                    {
                        TTF_TimeTracKingEntities dbCon = new TTF_TimeTracKingEntities();

                        if (item.MaDuAn == null || item.MaDuAn.Trim() == "")
                        {
                            rs.text = "Chưa chọn mã dự án";

                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        item.MaDuAn = item.MaDuAn.ToUpper().Replace(" ", "");
                        var Project = dbCon.TTF_DuAn.FirstOrDefault(it => it.MaDuAn == item.MaDuAn);
                        if (Project == null)
                        {
                            rs.text = "Mã dự án chưa có tồn tại";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (Project.MaLoaiDuAn.Trim() != "XK" && Project.MaLoaiDuAn.Trim() != "CT")
                            {
                                rs.text = "Mã dự án không hợp lệ";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //if (Project.MaNV == null || Project.MaNV.Trim() == "")
                        //{
                        //    msg = "Mã dự án chưa có trưởng nhóm";
                        //    ViewBag.Error = msg;
                        //    ViewBag.Action = "Add_TangCa";
                        //    return View("ChiTietHoSoTangCa", item);
                        //}
                        dbCon.Dispose();
                    }

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    using (var tran = new TransactionScope())
                    {

                        string Loi = "";
                        db.GhiChu = "Add tang ca";
                        objH = new TTF_TangCa();
                        objH.MaPhong_PhanXuong = item.MaPhong_PhanXuong.Trim();
                        objH.NgayTangCa = item.NgayTangCa;
                        objH.GioBatDau = GioBatDau;//new TimeSpan(Convert.ToDateTime(item.GioBatDau).Hour, Convert.ToDateTime(item.GioBatDau).Minute, 0);
                        objH.GioKetThuc = GioKetThuc;//new TimeSpan(Convert.ToDateTime(item.GioKetThuc).Hour, Convert.ToDateTime(item.GioKetThuc).Minute, 0);
                        objH.LyDo = item.LyDo;
                        objH.NgayTao = DateTime.Now;
                        objH.NguoiTao = (int)nguoidung.NguoiDung;
                        objH.MaTrangThaiDuyet = "1";
                        //objH.IDNguoiDuyetKeTiep = nguoiduyetketiep;
                        objH.DotXuat = item.DotXuat;
                        objH.Block = false;
                        objH.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                        objH.NgayThayDoiLanCuoi = DateTime.Now;
                        objH.NguoiDeXetDuyet = NhanSu;
                        objH.GhiChu = item.GhiChu;
                        objH.CongTac = item.CongTac;
                        db.TTF_TangCa.Add(objH);
                        objH.MaDuAn = item.MaDuAn;
                        objH.DuAn = item.DuAn;
                        db.SaveChanges();
                        foreach (var ct in item.TCChiTiet)
                        {
                            objD = new TTF_TangCaChiTiet();
                            var NhanSuTem = db.TTF_NhanSu.FirstOrDefault(o => o.MaNV.Trim() == ct.MaNV.Trim());
                            objD.NhanSu = NhanSuTem.NhanSu;
                            var KiemTra = db.TTF_KiemTraThoiGianTangCaTRung((int)objD.NhanSu, item.NgayTangCa, TimeSpan.Parse(item.GioBatDau), TimeSpan.Parse(item.GioKetThuc)).ToList();
                            if (KiemTra.Count > 0)
                            {
                                Loi += "Nhân viên " + ct.HoVaTen + " có thời gian tăng ca bị trùng \n ";
                            }
                            objD.IDTangCa = objH.IDTangCa;
                            //objD.NhanSu = db.TTF_NhanSu.Where(o => o.MaNV.Trim() == ct.MaNV.Trim()).FirstOrDefault().NhanSu;
                            db.TTF_TangCaChiTiet.Add(objD);

                            if (item.CongTac == true)
                            {
                                if (item.NgayTangCa.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    TTF_TimeTracKingEntities dbCon = new TTF_TimeTracKingEntities();
                                    // nếu là tăng ca chủ nhật có tạo yêu cầu công tác chưa.
                                    var KiemTraCT = (from a in db.TTF_CongTac
                                                     where a.NhanSu == objD.NhanSu && a.Del != true && a.MaTrangThaiDuyet != "4" && a.MaTrangThaiDuyet != "1" && a.TuNgay <= item.NgayTangCa && item.NgayTangCa <= a.DenNgay
                                                     select new { a.TuNgay }
                                                   ).ToList();
                                    if (KiemTraCT.Count == 0)
                                    {
                                        Loi += "Yêu cầu tạo tăng ca " + NhanSuTem.HoVaTen + " không hợp lệ chưa có khai báo đi công tác";
                                    }
                                }
                                else
                                {
                                    var KiemTraCT = (from a in db.TTF_CongTac
                                                     join b in db.TTF_CongTacChiTiet on a.IDCongTac equals b.IDCongTac
                                                     where a.NhanSu == objD.NhanSu && a.Del != true && a.MaTrangThaiDuyet != "4" && a.MaTrangThaiDuyet != "1" && b.Del != true && b.Ngay == item.NgayTangCa
                                                     select new { a.TuNgay }
                                                   ).ToList();
                                    if (KiemTraCT.Count == 0)
                                    {
                                        Loi += "Yêu cầu tạo tăng ca " + NhanSuTem.HoVaTen + " không hợp lệ chưa có khai báo đi công tác";
                                    }
                                }
                            }
                        }
                        if (Loi.Length == 0)
                        {
                            db.SaveChanges();
                            tran.Complete();
                            rs.code = 1;
                            rs.text = "Thành công";
                            rs.description = objH.IDTangCa.ToString();
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            rs.text = Loi;
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                        eve.Entry.Entity.GetType().Name,
                                                        eve.Entry.State));
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                        ve.PropertyName,
                                                        ve.ErrorMessage));
                        }
                    }
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "Add_TangCa");
                    //return Content("Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString());
                    rs.text = "Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + sb.ToString();
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public static string KiemTraTangCa(int NguoiDeXetDuyet, DateTime NgayTangCa, TimeSpan GioBatDau, TimeSpan GioKetThuc, Int64 IDTangCa, string MaPhongBan)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                string rv = "";
                var ListTangCa = db.TTF_TangCa.Where(o => o.NguoiDeXetDuyet == NguoiDeXetDuyet
                    && o.NgayTangCa == NgayTangCa.Date
                    && o.IDTangCa != IDTangCa && o.MaPhong_PhanXuong == MaPhongBan.Trim()
                    && o.Del == false
                    && (o.MaTrangThaiDuyet == "1" || o.MaTrangThaiDuyet == "2")).ToList();
                if (ListTangCa.Count > 0)
                {
                    foreach (var item in ListTangCa)
                    {
                        if (GioBatDau > item.GioBatDau && GioBatDau < item.GioKetThuc
                            || GioKetThuc > item.GioBatDau && GioKetThuc < item.GioKetThuc
                            || GioBatDau <= item.GioBatDau && GioKetThuc >= item.GioKetThuc
                            || GioBatDau >= item.GioBatDau && GioKetThuc <= item.GioKetThuc)
                        {
                            rv = "Đã có yêu cầu tăng ca cùng ngày và thời gian trùng lắp";
                            break;
                        }
                    }
                }
                return rv;
            }

        }

        [RoleAuthorize(Roles = "0=0,47=1")]
        public ActionResult Edit(long? id, int? op)
        {
            if (id == null)
            {
                return Content("Chưa chọn thông tin tăng ca");
            }
            else
            {

                TangCa model = new TangCa();
                YeuCauTangCaChiTiet ct = new YeuCauTangCaChiTiet();
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                using (var db = new TTF_FACEIDEntities())
                {
                    bool ShowDefaultFunctions = false;
                    if (User.IsInRole("44=1") || User.IsInRole("0=0"))
                    {
                        ShowDefaultFunctions = true;
                    }
                    var tc = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == id);
                    model.IDTangCa = tc.IDTangCa;
                    model.MaPhong_PhanXuong = tc.MaPhong_PhanXuong.Trim();
                    model.NgayTangCa = Convert.ToDateTime(tc.NgayTangCa);
                    model.GioBatDau = tc.GioBatDau.ToString();
                    model.GioKetThuc = tc.GioKetThuc.ToString();
                    model.LyDo = tc.LyDo;
                    model.DotXuat = Convert.ToBoolean(tc.DotXuat);
                    model.DuAn = tc.DuAn;
                    model.MaDuAn = tc.MaDuAn;
                    model.CongTac = tc.CongTac == null ? false : (bool)tc.CongTac;
                    model.Block = tc.Block == null ? false : (bool)tc.Block;
                    model.NguoiTao = tc.NguoiTao.Value;
                    model.IDNguoiDuyetKeTiep = tc.IDNguoiDuyetKeTiep;
                    model.GhiChu = tc.GhiChu;
                    model.GhiChuKiemTra = tc.GhiChuKiemTra;
                    //  List<YeuCauTangCaChiTiet> list = new List<YeuCauTangCaChiTiet>();
                    // var tcct = db.TTF_TangCaChiTiet.Where(o => o.IDTangCa == model.IDTangCa).ToList();
                    //   Int32 i = 1;

                    List<YeuCauTangCaChiTiet> list = (from tangca in db.TTF_TangCaChiTiet
                                                      join nhansu in db.TTF_NhanSu on tangca.NhanSu equals nhansu.NhanSu
                                                      join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong equals pb.MaPhong_PhanXuong
                                                      where tangca.IDTangCa == model.IDTangCa
                                                      select new YeuCauTangCaChiTiet { MaNV = nhansu.MaNV, HoVaTen = nhansu.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, NhanSu = nhansu.NhanSu, MaPhong_PhanXuong = nhansu.MaPhong_PhanXuong }).ToList();
                    //foreach (var item in tcct)
                    //{
                    //    ct = new YeuCauTangCaChiTiet();
                    //    ct.STT = i;
                    //    var ns = db.TTF_NhanSu.Where(o => o.NhanSu == item.NhanSu).FirstOrDefault();
                    //    if (ns != null)
                    //    {
                    //        ct.MaNV = ns.MaNV;
                    //        ct.HoVaTen = ns.HoVaTen;
                    //        var phong = db.TTF_PhongBan_PhanXuong.Where(o => o.MaPhong_PhanXuong.Trim() == ns.MaPhong_PhanXuong.Trim()).FirstOrDefault();
                    //        if (phong != null)
                    //        {
                    //            ct.MaPhong_PhanXuong = phong.MaPhong_PhanXuong;
                    //            ct.TenPhong_PhanXuong = phong.TenPhong_PhanXuong;
                    //        }
                    //    }
                    //    list.Add(ct);
                    //    i++;
                    //}
                    model.TCChiTiet = list;
                    ViewBag.Op = op;

                    return View(model);
                }
            }
        }
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,47=3")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Save_TangCa(TangCa item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                db.GhiChu = " Edit thông tin tăng ca";
                var tc = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == item.IDTangCa);
                if (tc.Block == true)
                {
                    rs.text = "Yêu cầu tăng ca đã khóa không thể sửa";
                }
                else
                {
                    try
                    {
                        TimeSpan GioBatDau;
                        TimeSpan GioKetThuc;
                        GioBatDau = new TimeSpan(Convert.ToDateTime(item.GioBatDau).Hour, Convert.ToDateTime(item.GioBatDau).Minute, 0);
                        GioKetThuc = new TimeSpan(Convert.ToDateTime(item.GioKetThuc).Hour, Convert.ToDateTime(item.GioKetThuc).Minute, 0);
                        string msg = KiemTraTangCa(Convert.ToInt32(tc.NguoiDeXetDuyet), item.NgayTangCa, GioBatDau, GioKetThuc, item.IDTangCa, item.MaPhong_PhanXuong);
                        if (msg != "")
                        {
                            rs.text = msg;
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        if (clsFunction.checkKyCongNguoiDung(item.NgayTangCa))
                        {
                            rs.text = "Kỳ công đã đóng";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        
                        if (item.DuAn == true)
                        {
                            if (item.MaDuAn == null || item.MaDuAn.Trim() == "")
                            {
                                msg = "Chưa chọn mã dự án";
                                rs.text = msg;
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                        }
                        TTF_NhanSu ns;
                        foreach (var ct in item.TCChiTiet)
                        {
                            ns = clsFunction.GetNS(ct.MaNV);
                            if (ns.KhongTinhTC == true
                                && item.NgayTangCa.DayOfWeek != DayOfWeek.Sunday
                                && clsFunction.CheckNgayLe(item.NgayTangCa) == false)
                            {
                                rs.text = "Nhân sự MSNV: " + ct.MaNV + " không được tăng ca ngày " + item.NgayTangCa.Date.ToString("dd/MM/yyyy");
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }
                        using (var tran = new TransactionScope())
                        {
                            try
                            {
                                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                                tc.MaPhong_PhanXuong = item.MaPhong_PhanXuong.Trim();
                                tc.NgayTangCa = item.NgayTangCa;
                                tc.GioBatDau = GioBatDau;// new TimeSpan(Convert.ToDateTime(item.GioBatDau).Hour, Convert.ToDateTime(item.GioBatDau).Minute, 0);
                                tc.GioKetThuc = GioKetThuc;// new TimeSpan(Convert.ToDateTime(item.GioKetThuc).Hour, Convert.ToDateTime(item.GioKetThuc).Minute, 0);
                                tc.LyDo = item.LyDo;
                                tc.MaTrangThaiDuyet = "1";
                                tc.DotXuat = item.DotXuat;
                                tc.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                                tc.NgayThayDoiLanCuoi = DateTime.Now;
                                tc.GhiChu = item.GhiChu;
                                tc.MaDuAn = item.MaDuAn;
                                tc.DuAn = item.DuAn;
                                tc.CongTac = item.CongTac;
                                //  db.SaveChanges();
                                // db.Database.ExecuteSqlCommand("delete from TTF_TangCaChiTiet where IDTangCa = " + id);
                                var deltangca = db.TTF_TangCaChiTiet.Where(it => it.IDTangCa == item.IDTangCa).ToList();
                                db.TTF_TangCaChiTiet.RemoveRange(deltangca);
                                List<TTF_TangCaChiTiet> listAdd = new List<TTF_TangCaChiTiet>();
                                string sLoi = "";
                                if (item.TCChiTiet != null)
                                {
                                    foreach (var ct in item.TCChiTiet)
                                    {
                                        var KiemTra = db.TTF_KiemTraThoiGianTangCaTrungEdit((int)ct.NhanSu, item.NgayTangCa, GioBatDau, GioKetThuc,(int)item.IDTangCa).ToList();
                                        if (KiemTra.Count > 0)
                                        {
                                            sLoi += "Nhân viên " + ct.HoVaTen + " có thời gian tăng ca bị trùng \n ";
                                        }
                                        listAdd.Add(new TTF_TangCaChiTiet { IDTangCa = item.IDTangCa, NhanSu = ct.NhanSu });
                                    }
                                }
                                if (sLoi.Length == 0)
                                {
                                    listAdd = listAdd.Distinct().ToList();
                                    db.TTF_TangCaChiTiet.AddRange(listAdd);
                                    db.SaveChanges();
                                    tran.Complete();
                                    rs.code = 1;
                                    rs.text = "Thành công";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    rs.text = sLoi;
                                    rs.code = 0;
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                               
                            }
                            catch (Exception ex)
                            {
                                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "Save_TangCa");
                                //return Content("Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString());
                                rs.text = "Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString();
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "Save_TangCa");
                        //return Content("Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString());
                        rs.text = "Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT" + ex.ToString();
                        //ViewBag.Action = "Save_TangCa";
                    }
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GuiMail(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.description = id.ToString();
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                rs.code = 0;
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            HT_HETHONG ht = null;
            string CCwithTo = "";
            try
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Gửi mail" + id.ToString();
                    var model = db.TTF_TangCa;
                    var item = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == id);
                    if (item.Del == true)
                    {
                        rs.text = "Thông tin tăng ca đã xóa không thể gửi mail";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    string body = "";
                    string To = "";
                    string CC = "";
                    if (item.Block != true)
                    {
                        int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(Convert.ToInt32(item.NguoiDeXetDuyet), 0, "TC", 0);
                        if (nguoiduyetketiep == -1)
                        {
                            rs.text = "Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        if (nguoiduyetketiep == -2)
                        {
                            rs.text = "Yêu cầu đã hoàn tất";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        item.IDNguoiDuyetKeTiep = nguoiduyetketiep;
                        item.MaTrangThaiDuyet = "2";//Đang xử lý vào quy trình duyệt                   
                        item.Block = true;
                        item.NgayBlock = DateTime.Now;

                        body = GetEmailDuyetString("TC", id, Convert.ToInt32(nguoiduyetketiep));
                        var NguoiDuyet = db.TTF_NhanSu.Where(o => o.NhanSu == nguoiduyetketiep).FirstOrDefault();
                        if (NguoiDuyet != null)
                        {
                            To = NguoiDuyet.MailCongTy;
                        }
                        var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(item.NguoiTao));
                        if (NguoiTao != null)
                        {
                            CC = NguoiTao.MailCongTy;
                        }
                        CCwithTo = clsFunction.GetCCwithTo("TC", To);
                        if (CCwithTo != "")
                        {
                            CC = CC + ";" + CCwithTo;
                            CC = CC.TrimStart(';');
                        }
                        ht = clsFunction.Get_HT_HETHONG();
                        clsFunction.GuiMail(ht.MailTitleTC, To, CC, body);


                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            i = clsFunction.LuuLichSuDuyet(item.IDTangCa, "TC", false, (int)nguoiduyetketiep, DateTime.Now);
                        }
                        rs.code = 1;
                        rs.text = "Thành công";
                    }
                    else
                    {
                        rs.text = "Thông tin này đã gửi mail tăng ca không thể gửi lại";
                    }
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "GuiMail");
                return Json(ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ", JsonRequestBehavior.AllowGet);
            }
        }
        public string GetEmailDuyetString(string Loai, Int32 id, Int32 nguoiduyetketiep)
        {
            string rv = "";
            using (var db = new TTF_FACEIDEntities())
            {
                if (Loai == "TC")
                {
                    var item = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == id);
                    string body = "";
                    string NguoiNhan = "";
                    string NguoiTao = "";
                    var nhanSu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == nguoiduyetketiep);
                    if (nhanSu != null)
                    {
                        NguoiNhan = nhanSu.HoVaTen;
                    }
                    var vitem = db.TTF_TangCa.Where(it => it.IDTangCa == id).FirstOrDefault();
                    var nguoidexetduyet = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NguoiDeXetDuyet);
                    if (nguoidexetduyet != null)
                    {
                        NguoiTao = nguoidexetduyet.HoVaTen;
                    }
                    string link = "";
                    var ht = db.HT_HETHONG.Where(o => o.ID == 1).FirstOrDefault();
                    if (ht != null)
                    {
                        link = ht.WEBSITE.Trim();
                    }
                    link += "/TangCa/DuyetPublic?NguoiDuyet=" + nhanSu.MailCongTy.ToLower().Replace("@truongthanh.com", "");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiNhan + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Bạn có yêu cầu tăng ca được gửi từ " + NguoiTao + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>ID: " + id.ToString() + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Ngày: " + Convert.ToDateTime(vitem.NgayTangCa).Date.ToString("dd/MM/yyyy") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Từ giờ: " + new TimeSpan(vitem.GioBatDau.Value.Hours, vitem.GioBatDau.Value.Minutes, 0).ToString(@"hh\:mm")
                        + " Đến giờ: " + new TimeSpan(vitem.GioKetThuc.Value.Hours, vitem.GioKetThuc.Value.Minutes, 0).ToString(@"hh\:mm") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do: " + vitem.LyDo + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống Tăng ca để xem thông tin chi tiết hơn và xem xét duyệt yêu cầu này (" + link + ")</td>");
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
        [RoleAuthorize(Roles = "0=0,47=4")]
        [ValidateAntiForgeryToken]
        public JsonResult Delete_TangCa(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            string msg = "";
            try
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Xóa tăng ca";
                    var item = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == id);
                    if (item.Block != true)
                    {
                        var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                        int NhanSu = (int)nguoidung.NhanSu;
                        if (NhanSu == -1)
                        {
                            rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        if (clsFunction.checkKyCongNguoiDung(item.NgayTangCa.Value))
                        {
                            msg = "Kỳ công đã đóng";
                            rs.text = msg;
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        //if (User.IsInRole("44=1") == false)
                        //{
                        //    if (clsFunction.checkKyCongNguoiDung(item.NgayTangCa.Value))
                        //    {
                        //        msg = "Kỳ công đã đóng";
                        //        rs = msg;
                        //        return Json(rs, JsonRequestBehavior.AllowGet);
                        //    }
                        //}

                        //if (CheckNgayTC(Convert.ToDateTime(item.NgayTangCa).ToString("dd/MM/yyyy")) == false)
                        //{
                        //    msg = "Kỳ công đã đóng";
                        //    rs = msg;
                        //    return Json(rs, JsonRequestBehavior.AllowGet);
                        //}

                        item.Del = true;
                        item.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                        item.NgayThayDoiLanCuoi = DateTime.Now;
                        int sc = db.SaveChanges();
                        rs.text = "Thành công";
                        rs.code = 1;

                    }
                    else
                    {
                        rs.text = "Thông tin này đã gửi mail xác nhận công không thể xóa";
                    }
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "Delete");
                return Json("Lỗi trong quá trình xóa liên hệ phòng HTTT để được hỗ trợ", JsonRequestBehavior.AllowGet);
            }
        }

        [RoleAuthorize(Roles = "0=0,48=1")]
        public ActionResult DuyetTangCa()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,48=1")]
        public async Task<JsonResult> GetDuyetTangCa(string tuNgay, string denNgay, string maNV, string maPhongBan)
        {

            using (var db = new TTF_FACEIDEntities())
            {
                JsonStatus js = new JsonStatus();
                js.code = 0;
                try
                {
                    if (maPhongBan == null)
                    {
                        maPhongBan = "";
                    }
                    if (maNV == null)
                    {
                        maNV = "";
                    }
                    if (tuNgay == null)
                    {
                        tuNgay = "";
                    }
                    if (denNgay == null)
                    {
                        denNgay = "";
                    }

                    //string sWhere = "ISNULL(TTF_TangCa.Del,0) = 0 And ";
                    //if (maPhongBan != "")
                    //{
                    //    sWhere += "TTF_TangCa.MaPhong_PhanXuong = N'" + maPhongBan + "' And ";
                    //}
                    //if (maNV != "")
                    //{
                    //    sWhere += "MaNV + ' ' + HoVaTen Like N'%" + maNV + "%' And ";
                    //}
                    //if (tuNgay == "" && denNgay == "")
                    //{
                    //    tuNgay = "1990-01-01";
                    //    denNgay = "9999-01-01";
                    //}
                    //var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    //sWhere += "CONVERT(DATE,NgayTangCa) BETWEEN CONVERT(DATE,'" + tuNgay + "') AND CONVERT(DATE,'" + denNgay + "') And ";
                    //sWhere += "TTF_TangCa.IDNguoiDuyetKeTiep = " + nguoidung.NhanSu + " And ";

                    //sWhere += "TTF_TangCa.MaTrangThaiDuyet = '2' And ";

                    //if (sWhere.Length > 0)
                    //{
                    //    sWhere = sWhere.Substring(0, sWhere.Length - 4);
                    //}
                    //string sql = "SELECT TTF_TangCa.*, TTF_NhanSu.MaNV, TTF_NhanSu.HoVaTen, TTF_PhongBan_PhanXuong.TenPhong_PhanXuong, TTF_TrangThaiDuyet.TenTrangThaiDuyet, TTF_KiemTra.MieuTaKiemTra "
                    //                + "FROM TTF_TangCa "
                    //                + "left join HT_NGUOIDUNG "
                    //                + "on TTF_TangCa.NguoiTao = HT_NGUOIDUNG.NGUOIDUNG "
                    //                + "left join TTF_NhanSu "
                    //                + "on HT_NGUOIDUNG.NhanSu = TTF_NhanSu.NhanSu "
                    //                + "left join TTF_PhongBan_PhanXuong "
                    //                + "on TTF_TangCa.MaPhong_PhanXuong = TTF_PhongBan_PhanXuong.MaPhong_PhanXuong "
                    //                + "left join TTF_TrangThaiDuyet "
                    //                + "on TTF_TangCa.MaTrangThaiDuyet = TTF_TrangThaiDuyet.MaTrangThaiDuyet "
                    //                + "left join TTF_KiemTra "
                    //                + "on TTF_TangCa.MaKiemTra = TTF_KiemTra.MaKiemTra "
                    //                + "WHERE  " + sWhere;
                    //var model = db.Database.SqlQuery<TangCaGrid>(sql).OrderByDescending(m => m.IDTangCa).ToList();
                    //Int32 IDNguoiDuyetKeTiep = 0;
                    //foreach (var item in model)
                    //{
                    //    IDNguoiDuyetKeTiep = Convert.ToInt32(item.IDNguoiDuyetKeTiep);
                    //    var ndkt = db.TTF_NhanSu.Where(o => o.NhanSu == item.IDNguoiDuyetKeTiep).FirstOrDefault();
                    //    if (ndkt != null)
                    //    {
                    //        item.HoVaTenNguoiDuyetKeTiep = ndkt.HoVaTen;
                    //    }
                    //    var nkt = db.TTF_NhanSu.Where(o => o.NhanSu == item.NguoiKiemTra).FirstOrDefault();
                    //    if (nkt != null)
                    //    {
                    //        item.HoTenNguoiKiemTra = nkt.HoVaTen;
                    //    }
                    //}
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    var model = db.Proc_DuyetTangCa(tuNgay, denNgay, maPhongBan, maNV, nguoidung.NhanSu).ToList();
                    js.data = model;
                    js.code = 1;
                }
                catch (Exception ex)
                {
                    js.text = ex.Message;
                }
                return Json(js, JsonRequestBehavior.AllowGet);
            }

        }

        [RoleAuthorize(Roles = "0=0,48=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Mass_Duyet_TangCa(List<int> lid)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Content("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
            }
            string body = "";
            string To = "";
            string CC = "";
            StringBuilder error = new StringBuilder();
            int i = 0;
            List<TangCa> listYeuCauTangCa = null;
            TangCa objYeuCauTangCa = null;
            string LineBreak = "<br/>";
            HT_HETHONG ht = null;
            string CCwithTo = "";
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                using (var tran = new TransactionScope())
                {
                    try
                    {
                        var list = db.TTF_TangCa.Join(lid, o => o.IDTangCa, id => id, (o, id) => o).ToList();
                        listYeuCauTangCa = new List<TangCa>();
                        foreach (var tc in list)
                        {
                            objYeuCauTangCa = new TangCa();
                            objYeuCauTangCa.IDTangCa = tc.IDTangCa;
                            listYeuCauTangCa.Add(objYeuCauTangCa);
                        }
                        //Validation
                        var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                        int NhanSu = (int)nguoidung.NhanSu;
                        if (NhanSu == -1)
                        {
                            return Content("Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên");
                        }
                        foreach (var item in listYeuCauTangCa)
                        {
                            var ditem = list.FirstOrDefault(it => it.IDTangCa == item.IDTangCa);
                            try
                            {
                                if (ditem.Block != true)
                                {
                                    error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: " + ditem.IDTangCa
                                        + " do Yêu cầu này chưa được xác nhận hoàn tất từ người tạo");
                                    item.Del = true;
                                    continue;
                                }

                                clsFunction.CapNhatDuyetVaoLichSuDuyet(ditem.IDTangCa, "TC", NhanSu, "");
                                int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(Convert.ToInt32(ditem.NguoiDeXetDuyet), 0, "TC", Convert.ToInt32(ditem.IDTangCa));
                                if (nguoiduyetketiep == -1)
                                {
                                    error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: " + ditem.IDTangCa
                                        + " do Nhân viên chưa cập nhật cấp quản lý trực tiếp");
                                    item.Del = true;
                                    continue;
                                }

                                if (nguoiduyetketiep == -3)
                                {
                                    error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: " + ditem.IDTangCa
                                        + " do Không tìm thấy cấp duyệt tiếp theo. Liên hệ phòng HTTT để được hỗ trợ");
                                    item.Del = true;
                                    continue;
                                }
                                if (clsFunction.checkKyCongNguoiDung(ditem.NgayTangCa.Value))
                                {
                                    error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: "
                                        + ditem.IDTangCa + " do Kỳ công đã đóng");
                                    item.Del = true;
                                    continue;
                                }
                                //if (User.IsInRole("44=1") == false)
                                //{
                                //    if (clsFunction.checkKyCongNguoiDung(ditem.NgayTangCa.Value))
                                //    {
                                //        error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: "
                                //            + ditem.IDTangCa + " do Kỳ công đã đóng");
                                //        item.Del = true;
                                //        continue;
                                //    }
                                //}

                                //if (CheckNgayTC(Convert.ToDateTime(ditem.NgayTangCa).ToString("dd/MM/yyyy")) == false)
                                //{
                                //    error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: "
                                //        + ditem.IDTangCa + " do Kỳ công đã đóng");
                                //    item.Del = true;
                                //    continue;
                                //}
                            }
                            catch (Exception ex)
                            {
                                error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: "
                                    + ditem.IDTangCa + " do " + ex.Message);
                            }
                        }
                        foreach (var item in listYeuCauTangCa)
                        {
                            try
                            {
                                if (item.Del != true)
                                {
                                    var ditem = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == item.IDTangCa);
                                    item.NgayTangCa = Convert.ToDateTime(ditem.NgayTangCa);

                                    var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));

                                    int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(Convert.ToInt32(ditem.NguoiDeXetDuyet), 0, "TC", Convert.ToInt32(ditem.IDTangCa));
                                    if (nguoiduyetketiep == -2)
                                    {
                                        //Cập nhật thạng thái hoàn tất duyệt
                                        ditem.MaTrangThaiDuyet = "3";
                                        ditem.IDNguoiDuyetKeTiep = -1;
                                        db.SaveChanges();

                                        ////Phát sinh ngày nghỉ bù cho nhân sự Không tính tăng ca và (ngày chủ nhật hoặc ngày lễ)
                                        //var tcct = db.TTF_TangCaChiTiet.Where(o => o.IDTangCa == item.IDTangCa).ToList();
                                        //foreach (var ct in tcct)
                                        //{
                                        //    ns = clsFunction.GetNS(ct.NhanSu);
                                        //    if (ns.KhongTinhTC == true
                                        //        && (item.NgayTangCa.Date.DayOfWeek == DayOfWeek.Sunday
                                        //        || clsFunction.CheckNgayLe(item.NgayTangCa) == true))
                                        //    {
                                        //        objTTF_NghiBu = new TTF_NghiBu();
                                        //        objTTF_NghiBu.MaNV = ns.MaNV;
                                        //        objTTF_NghiBu.NgayPhatSinh = item.NgayTangCa;
                                        //        objTTF_NghiBu.SoNgay = 1;
                                        //        if (clsFunction.CheckNgayLe(item.NgayTangCa) == true)
                                        //        {
                                        //            objTTF_NghiBu.SoNgay = 2;
                                        //        }
                                        //        objTTF_NghiBu.NgayHieuLuc = item.NgayTangCa.AddDays(1);
                                        //        objTTF_NghiBu.NgayHetHieuLuc = objTTF_NghiBu.NgayHieuLuc.Value.Date.AddDays(90);
                                        //        objTTF_NghiBu.IDTangCa = item.IDTangCa;
                                        //        objTTF_NghiBu.DaDungChoPhep = false;
                                        //        objTTF_NghiBu.TCDaHuy = false;
                                        //        objTTF_NghiBu.ThayDoiLanCuoiBoi = clsUserActivesInternal.clsUserActive.GetNguoiDung(User.Identity.Name);
                                        //        objTTF_NghiBu.ThayDoiLanCuoiLuc = DateTime.Now;
                                        //        db.TTF_NghiBu.Add(objTTF_NghiBu);
                                        //        db.SaveChanges();
                                        //    }
                                        //}

                                        if (NguoiTao != null)
                                        {
                                            To = NguoiTao.MailCongTy;
                                        }

                                        body = GetEmailDuyetHoanTatString("TC", Convert.ToInt32(ditem.IDTangCa));

                                        ht = clsFunction.Get_HT_HETHONG();
                                        clsFunction.GuiMail(ht.MailTitleTC, To, "", body);

                                        rs.text = "Thành công";
                                    }
                                    else
                                    {
                                        ditem.IDNguoiDuyetKeTiep = nguoiduyetketiep;

                                        var NguoiDuyet = db.TTF_NhanSu.Where(o => o.NhanSu == nguoiduyetketiep).FirstOrDefault();
                                        if (NguoiDuyet != null)
                                        {
                                            To = NguoiDuyet.MailCongTy;
                                        }
                                        if (NguoiTao != null)
                                        {
                                            CC = NguoiTao.MailCongTy;
                                        }
                                        CCwithTo = clsFunction.GetCCwithTo("TC", To);
                                        if (CCwithTo != "")
                                        {
                                            CC = CC + ";" + CCwithTo;
                                            CC = CC.TrimStart(';');
                                        }
                                        body = GetEmailDuyetString("TC", Convert.ToInt32(ditem.IDTangCa), Convert.ToInt32(nguoiduyetketiep));

                                        ht = clsFunction.Get_HT_HETHONG();

                                        clsFunction.GuiMail(ht.MailTitleTC, To, CC, body);

                                        ditem.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                                        ditem.NgayThayDoiLanCuoi = DateTime.Now;
                                        i = db.SaveChanges();
                                        if (i > 0)
                                        {
                                            if (i > 0)
                                            {
                                                i = clsFunction.LuuLichSuDuyet(item.IDTangCa, "TC", false, (int)nguoiduyetketiep, DateTime.Now);
                                                if (i > 0)
                                                {
                                                    rs.text = "Thành công";
                                                }
                                                else
                                                {
                                                    error.Append(LineBreak + "Xử lý yêu cầu tăng ca: "
                                                        + item.IDTangCa + " thất bại");
                                                }
                                            }
                                            else
                                            {
                                                error.Append(LineBreak + "Xử lý yêu cầu tăng ca: "
                                                    + item.IDTangCa + " thất bại");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                error.Append(LineBreak + "Không xử lý được yêu cầu tăng ca: "
                                    + item.IDTangCa + " do " + ex.Message);
                            }
                        }
                        if (error.ToString() == null || error.ToString() == "")
                        {
                            rs.text = "Thành công";
                            rs.code = 1;
                            // rs.description = 
                            tran.Complete();
                        }
                        else
                        {
                            rs.text = error.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        rs.text = ex.Message;
                    }

                }

                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public string GetEmailDuyetHoanTatString(string Loai, Int32 id)
        {
            string rv = "";
            using (var db = new TTF_FACEIDEntities())
            {
                if (Loai == "TC")
                {
                    var item = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == id);
                    string body = "";
                    string NguoiTao = "";
                    var vitem = db.TTF_TangCa.Where(it => it.IDTangCa == id).FirstOrDefault();
                    var nguoidexetduyet = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NguoiDeXetDuyet);
                    if (nguoidexetduyet != null)
                    {
                        NguoiTao = nguoidexetduyet.HoVaTen;
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiTao + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu tăng ca bên dưới của anh/chị đã hoàn tất</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>ID: " + id.ToString() + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Ngày: " + Convert.ToDateTime(vitem.NgayTangCa).Date.ToString("dd/MM/yyyy") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Từ giờ: " + new TimeSpan(vitem.GioBatDau.Value.Hours, vitem.GioBatDau.Value.Minutes, 0).ToString(@"hh\:mm")
                        + " Đến giờ: " + new TimeSpan(vitem.GioKetThuc.Value.Hours, vitem.GioKetThuc.Value.Minutes, 0).ToString(@"hh\:mm") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do: " + vitem.LyDo + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Dear Mr./Ms. " + NguoiNhan + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Your following Overtime request has been completed</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Date: " + Convert.ToDateTime(vitem.NgayTangCa).Date.ToString("dd/MM/yyyy") + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Time from: " + new TimeSpan(vitem.GioBatDau.Value.Hours, vitem.GioBatDau.Value.Minutes, 0).ToString(@"hh\:mm")
                    //    + " Time to: " + new TimeSpan(vitem.GioKetThuc.Value.Hours, vitem.GioKetThuc.Value.Minutes, 0).ToString(@"hh\:mm") + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Reason: " + vitem.LyDo + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>This is an automated message - please do not reply directly to this email.</td>");
                    //sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
            }
            return rv;
        }
        [RoleAuthorize(Roles = "0=0,48=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Mass_Cancel_TangCa(List<int> lid, string LyDo)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Json("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại", JsonRequestBehavior.AllowGet);
            }
            HT_HETHONG ht = null;
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                int kq = 0;
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                using (var Tran = new TransactionScope())
                {

                    try
                    {
                        foreach (var tc in lid)
                        {
                            var model = db.TTF_TangCa;

                            var ditem = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == tc);
                            string body = "";
                            string To = "";
                            string HoVaTenNguoiTuChoi = "";
                            string msg = "";
                            if (ditem.Block != true
                               && User.IsInRole("44=1") == false && User.IsInRole("0=0") == false)
                            {
                                rs.text = "Yêu cầu này chưa được xác nhận hoàn tất từ người tạo";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                            else if (ditem.MaTrangThaiDuyet == "3"
                                && User.IsInRole("44=1") == false && User.IsInRole("0=0") == false)
                            {
                                rs.text = "Bạn không có quyền từ chối yêu cầu tăng ca đã hoàn tất";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                int NhanSu = (int)nguoidung.NhanSu;
                                if (NhanSu == -1)
                                {
                                    rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên";
                                    return Json(rs, JsonRequestBehavior.AllowGet); ;
                                }
                                if (clsFunction.checkKyCongNguoiDung(ditem.NgayTangCa.Value))
                                {
                                    rs.text = "Kỳ công đã đóng không thể hủy";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                                //if (User.IsInRole("44=1") == false)
                                //{
                                //    if (clsFunction.checkKyCongNguoiDung(ditem.NgayTangCa.Value))
                                //    {
                                //        msg = "Kỳ công đã đóng không thể hủy";
                                //        rs = msg;
                                //        return Json(rs, JsonRequestBehavior.AllowGet);
                                //    }
                                //}

                                //if (CheckNgayTC(Convert.ToDateTime(ditem.NgayTangCa).ToString("dd/MM/yyyy")) == false)
                                //{
                                //    msg = "Kỳ công đã đóng không thể hủy";
                                //    rs = msg;
                                //    return Json(rs, JsonRequestBehavior.AllowGet);
                                //}

                                var ntc = db.TTF_NhanSu.Where(o => o.NhanSu == NhanSu).FirstOrDefault();
                                if (ntc != null)
                                {
                                    HoVaTenNguoiTuChoi = ntc.HoVaTen;
                                }

                                clsFunction.CapNhatTuChoiVaoLichSuDuyet(ditem.IDTangCa, "TC", NhanSu, LyDo);

                                ditem.LyDoHuy = LyDo;
                                ditem.NguoiTuChoi = NhanSu;
                                ditem.NgayTuChoi = DateTime.Now;
                                ditem.MaTrangThaiDuyet = "4";
                                ditem.IDNguoiDuyetKeTiep = -1;
                                ditem.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                                ditem.NgayThayDoiLanCuoi = DateTime.Now;

                                int i = db.SaveChanges();
                                if (i > 0)
                                {
                                    kq++;
                                    var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                                    if (NguoiTao != null)
                                    {
                                        To = NguoiTao.MailCongTy;
                                    }

                                    body = GetEmailTuChoiString("TC", Convert.ToInt32(ditem.IDTangCa), HoVaTenNguoiTuChoi);

                                    ht = clsFunction.Get_HT_HETHONG();
                                    clsFunction.GuiMail(ht.MailTitleTC, To, "", body);
                                }
                            }
                        }

                        if (kq > 0)
                        {
                            rs.text = "Từ chối thành công " + kq.ToString() + " yêu cầu tăng ca";
                            rs.code = 1;
                            Tran.Complete();
                        }
                        return Json(rs, JsonRequestBehavior.AllowGet);

                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "TC", "Mass_Cancel_TangCa");
                        rs.text = ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
            }


        }

        public string GetEmailTuChoiString(string Loai, Int32 id, string NguoiTuChoi)
        {
            string rv = "";
            using (var db = new TTF_FACEIDEntities())
            {
                if (Loai == "TC")
                {
                    string body = "";

                    var vitem = db.TTF_TangCa.Where(it => it.IDTangCa == id).FirstOrDefault();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu tăng ca bên dưới của anh/chị đã bị từ chối bởi: " + NguoiTuChoi + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do: " + vitem.LyDoHuy + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>ID: " + id.ToString() + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Ngày: " + Convert.ToDateTime(vitem.NgayTangCa).Date.ToString("dd/MM/yyyy") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Từ giờ: " + new TimeSpan(vitem.GioBatDau.Value.Hours, vitem.GioBatDau.Value.Minutes, 0).ToString(@"hh\:mm")
                        + " Đến giờ: " + new TimeSpan(vitem.GioKetThuc.Value.Hours, vitem.GioKetThuc.Value.Minutes, 0).ToString(@"hh\:mm") + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do: " + vitem.LyDo + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Your following Overtime request has been rejected</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Reason: " + vitem.LyDoHuy + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Date: " + Convert.ToDateTime(vitem.NgayTangCa).Date.ToString("dd/MM/yyyy") + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Time from: " + new TimeSpan(vitem.GioBatDau.Value.Hours, vitem.GioBatDau.Value.Minutes, 0).ToString(@"hh\:mm")
                    //    + " Time to: " + new TimeSpan(vitem.GioKetThuc.Value.Hours, vitem.GioKetThuc.Value.Minutes, 0).ToString(@"hh\:mm") + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>Reason: " + vitem.LyDo + "</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>&nbsp;</td>");
                    //sb.Append("</tr>");
                    //sb.Append("<tr>");
                    //sb.Append("<td>This is an automated message - please do not reply directly to this email.</td>");
                    //sb.Append("</tr>");
                    sb.Append("</table>");
                    body = sb.ToString();
                    rv = body;
                }
            }
            return rv;
        }

        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult QuanLyTangCa()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> GetQuanLyTangCa(string tuNgay, string denNgay, string maNV, string maPhongBan)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                JsonStatus model = new JsonStatus();
                model.code = 0;
                try
                {
                    model.data = db.Proc_QuanLyTangCa(tuNgay, denNgay, maPhongBan, maNV).ToList();
                    model.code = 1;
                }
                catch (Exception ex)
                {
                    model.text = ex.Message;
                }
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult TangCaThongTinChiTiet()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> GetTangCaThongTinChiTiet(string tuNgay, string denNgay, string maNV, string maPhongBan)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                JsonStatus model = new JsonStatus();
                model.data = db.Proc_ThongTinTangCaChiTiet(tuNgay, denNgay, maPhongBan, maNV).ToList();
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }
        }
    }
}