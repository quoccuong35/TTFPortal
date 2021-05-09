using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Class;
using TTFPortal.Models;
namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "0=0,55=1,46=1,44=1")]
    public class ChamCongKhongThanhController : Controller
    {
        // GET: ChamCongKhongThanh
        public ActionResult QLChamCongKhongThanh()
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> getChamCongKhongThanh(string tuNgay, string denNgay, string hoVaTen)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
                DateTime dtTemp = DateTime.Now;
                int day = dtTemp.Day-1;
                if (denNgay == null || denNgay == "")
                {
                    denNgay = dtTemp.ToString("yyyy-MM-dd");
                }
                if (tuNgay == null || tuNgay == "")
                {
                    tuNgay = dtTemp.AddDays(-day).ToString("yyyy-MM-dd");
                }
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                var model = db.Proc_ChamCongKhongThanh((int)nguoidung.NguoiDung, tuNgay, denNgay, hoVaTen).ToList();
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }
        }
        public ActionResult XuatFileMau()
        {
            string filename = Server.MapPath("~/Content/Upload/FileMau/ChamCongKhongThanh.xlsx");
            return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ChamCongKhongThanh.xlsx");
        }
        [RoleAuthorize(Roles = "0=0,55=2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ImportExcelUploadChamCongKhongThanh(HttpPostedFileBase importFile, bool? caDem)
        {
            string sCamDem = caDem == null || caDem == false ? "0" : "1";
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            var nguoidung = Users.GetNguoiDung(User.Identity.Name);
            int iNguoiDung = (int)nguoidung.NguoiDung;

            string Loi = "", CapNhat = "", sPhongBan = nguoidung.MaPhongPhanXuong;

            int iNhanSu = (int)nguoidung.NhanSu;

            //
            if (iNhanSu  <0 )
            {
                rs.text = "Tài khoản đăng nhập chưa có thông tin nhân sự. Liên hệ phòng HTTT để được hỗ trợ";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }

            if (importFile != null && importFile.ContentLength > 0 && (Path.GetExtension(importFile.FileName).Equals(".xlsx")))
            {
                string fileName = importFile.FileName;
                string UploadDirectory = Server.MapPath("~/Content/Upload/Temps/");
                bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(UploadDirectory);
                string resultFilePath = UploadDirectory + fileName;
                try
                {
                    importFile.SaveAs(resultFilePath);
                    DataTable dt = getDataTableFromExcel(resultFilePath);
                    DateTime Ngay = DateTime.Now;
                    DateTime NgayTemp = DateTime.Now; string NgayCanXacNhan = "";
                    DataRow[] checkTable;
                    using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
                    {
                        var NguoiTao = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == iNhanSu);
                        if (dt.Rows.Count > 0)
                        {
                           
                            List<string> PhamVi = new List<string>();
                            PhamVi = db.TTF_PhamVi.Where(it => it.NhanSu == iNhanSu).Select(it => it.MaPhong_PhanXuong).ToList();
                            PhamVi.Add(NguoiTao.MaPhong_PhanXuong.Trim());
                            if (User.IsInRole("0=0"))
                            {
                                PhamVi = db.TTF_PhongBan_PhanXuong.Where(it => it.Del != true).Select(it => it.MaPhong_PhanXuong).ToList();
                            }
                            var NhanSu = (from a in db.TTF_NhanSu
                                          where a.Del != true
                                          && PhamVi.Contains(a.MaPhong_PhanXuong.Trim())
                                          select new { a.MaNV, a.NhanSu, a.HoVaTen }).ToList();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i][0].ToString().Trim().Length == 0)
                                    break;
                                try
                                {
                                    Ngay = (DateTime.ParseExact(dt.Rows[i][2].ToString(), "dd/MM/yyyy", new CultureInfo("en-US")));
                                }
                                catch (Exception)
                                {
                                    Loi += "Không tìm thấy ngày xác nhận công <b>" + dt.Rows[i][2].ToString() + "</b> dòng " + (i + 1).ToString();
                                    continue;
                                }
                                if (clsFunction.checkKyCongNguoiDung(Ngay))
                                {
                                    Loi += "Dòng thứ " + (i + 1).ToString() + " " + Ngay.ToString("dd/MM/yyyy") + " khóa kỳ công không thể Import ";
                                    continue;
                                }
                                var nstemp = NhanSu.FirstOrDefault(it => it.MaNV == dt.Rows[i][0].ToString());
                                if (nstemp == null)
                                {
                                    Loi += "Không tìm thấy nhân sự có mã <b>" + dt.Rows[i][0].ToString() + "</b> ở dòng " + (i + 1).ToString() + "<br>";
                                    continue;
                                }

                                if (clsFunction.checkKyCongNhanSu(Ngay))
                                {
                                    Loi += "Kỳ công đã đống <b> dòng " + (i + 1).ToString() + " " + nstemp.HoVaTen + "</b><br>";
                                    continue;
                                }
                                if (dt.Rows[i][3].ToString().Trim() != "") // giời vào
                                {
                                    checkTable = dt.Select("Ngay = '" + dt.Rows[i]["Ngay"].ToString() + "' And GioVao = '" + dt.Rows[i]["GioVao"].ToString().Trim() + "' And MaNV = '" + dt.Rows[i][0].ToString() + "'");
                                    if (checkTable.Length > 1)
                                    {
                                        Loi += "Dòng thứ  " + (i + 1).ToString() + " <b>" + nstemp.HoVaTen + "</b> trùng thông tin xác nhận công trong file excel. <br>";
                                    }
                                    var KiemTra = db.TTF_XacNhanCong.FirstOrDefault(it => it.NhanSu == nstemp.NhanSu && it.Ngay == Ngay && it.MaTrangThaiDuyet.Trim() != "4" && it.Del != true && it.GioVao == true);
                                    if (KiemTra == null)
                                    {
                                        CapNhat += "INSERT INTO dbo.TTF_XacNhanCong( NhanSu ,Ngay , ThoiGian , GioVao ,NguyenNhan ,GhiChu , NguoiTao ,NgayTao ,MayTao ,MaTrangThaiDuyet ,IDNguoiDuyetKeTiep ,Block ,NgayBlock , Del , XacNhanCong,CaDem  ) " +
                                       "VALUES  (" + nstemp.NhanSu + ",'" + Ngay.ToString("MM/dd/yyyy") + "','" + dt.Rows[i][3].ToString().Trim() + "','1',N'" + dt.Rows[i][5].ToString().Trim() + "',N'" + dt.Rows[i][6].ToString().Trim() + "', " +
                                       " '" + iNguoiDung + "',GETDATE(),'','2','" + NguoiTao.IDCapQuanLyTrucTiep + "','1',GetDate(),'0','1','" + sCamDem + "') \n";
                                    }
                                    else
                                    {
                                        Loi += "Dòng thứ  " + (i + 1).ToString() + " <b>" + nstemp.HoVaTen + "</b> đã có thông tin xác nhận công. Liên hệ P.HCNS để được hỗ trợ <br>";
                                    }

                                }
                                if (dt.Rows[i][4].ToString().Trim() != "") // giời ra
                                {
                                    checkTable = dt.Select("Ngay = '" + dt.Rows[i]["Ngay"].ToString() + "' And GioRa = '" + dt.Rows[i]["GioRa"].ToString().Trim() + "' And MaNV = '" + dt.Rows[i][0].ToString() + "'");
                                    if (checkTable.Length > 1)
                                    {
                                        Loi += "Dòng thứ  " + (i + 1).ToString() + " <b>" + nstemp.HoVaTen + "</b> trùng thông tin xác nhận công trong file excel. <br>";
                                    }
                                    var KiemTra = db.TTF_XacNhanCong.FirstOrDefault(it => it.NhanSu == nstemp.NhanSu && it.Ngay == Ngay && it.MaTrangThaiDuyet.Trim() != "4" && it.Del != true && it.GioVao != true);
                                    if (KiemTra == null)
                                    {
                                        CapNhat += "INSERT INTO dbo.TTF_XacNhanCong( NhanSu ,Ngay , ThoiGian , GioVao ,NguyenNhan ,GhiChu , NguoiTao ,NgayTao ,MayTao ,MaTrangThaiDuyet ,IDNguoiDuyetKeTiep ,Block ,NgayBlock , Del , XacNhanCong,CaDem ) " +
                                                                  "VALUES  (" + nstemp.NhanSu + ",'" + Ngay.ToString("MM/dd/yyyy") + "','" + dt.Rows[i][4].ToString().Trim() + "','0',N'" + dt.Rows[i][5].ToString().Trim() + "',N'" + dt.Rows[i][6].ToString().Trim() + "', " +
                                                                  " '" + iNguoiDung + "',GETDATE(),'','2','" + NguoiTao.IDCapQuanLyTrucTiep + "','1',GetDate(),'0','1','" + caDem + "') \n";
                                    }
                                    else
                                    {
                                        Loi += "Dòng thứ  " + (i + 1).ToString() + " <b>" + nstemp.HoVaTen + "</b> đã có thông tin xác nhận công. Liên hệ P.HCNS để được hỗ trợ <br>";
                                    }

                                }
                                if (i == 0)
                                {
                                    NgayCanXacNhan += Ngay.ToString("dd/MM/yyyy");
                                    NgayTemp = Ngay;
                                }
                                else
                                {
                                    if (Ngay != NgayTemp)
                                    {
                                        NgayCanXacNhan += ";" + Ngay.ToString("dd/MM/yyyy");
                                        NgayTemp = Ngay;
                                    }
                                }
                            }
                        }

                        if (Loi.Length > 0)
                        {
                            rs.text = Loi;
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var NguoiDuyet = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NguoiTao.IDCapQuanLyTrucTiep);
                            HT_HETHONG ht = clsFunction.Get_HT_HETHONG();

                            using (var tran = new System.Transactions.TransactionScope())
                            {
                                if (CapNhat.Length > 0)
                                {
                                    int sb = db.Database.ExecuteSqlCommand(CapNhat);

                                    string body = GetEmailDuyetString("XNC", sb, NguoiTao, NguoiDuyet, NgayCanXacNhan);
                                    clsFunction.GuiMail(ht.MailTitleXNC, NguoiDuyet.MailCongTy, NguoiTao.MailCongTy, body);
                                    tran.Complete();
                                    rs.text = "Cập nhật thành công " + sb + " dòng thông tin";
                                    rs.code = 1;
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    rs.text = "Không có thông tin cập nhật";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    System.IO.File.Delete(resultFilePath);
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString() + CapNhat, "CCKT", "ImportExcelUploadChamCongKhongThanh");
                    rs.text = ex.Message;
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                rs.text = "Chưa chọn file import";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }

        }
        public string GetEmailDuyetString(string Loai, int soLuong, TTF_NhanSu NguoiCanXacNhanCong, TTF_NhanSu nhanSuDuyet, string NgayCanXacNhan)
        {
            string rv = "";
            if (Loai == "XNC")
            {
                string body = "";
                string NguoiNhan = "";
                string NguoiTao = "";
                using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
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
                    sb.Append("<td>Anh/Chị có yêu cầu cần xác nhận chấm công không thành từ <b>" + NguoiTao + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Các ngày cần xác nhận công: <b>" + NgayCanXacNhan + "</b>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td> Số lượng cần xác nhận chấm công không thành: <b>" + soLuong.ToString() + "</b>");
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
            }
            return rv;
        }
        public static DataTable getDataTableFromExcel(string path)
        {
            var existingFile = new FileInfo(path);
            using (var pck = new OfficeOpenXml.ExcelPackage(existingFile))
            {
                var ws = pck.Workbook.Worksheets.First();
                DataTable tbl = new DataTable();
                bool hasHeader = true;
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(firstRowCell.Text.Replace(" ", "").Replace("/", "").Replace("-", "").ToLower());
                }
                var startRow = hasHeader ? 2 : 1;
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = tbl.NewRow();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text.Trim();
                    }
                    tbl.Rows.Add(row);
                }
                return tbl;
            }
        }
        [RoleAuthorize(Roles = "0=0,46=1")]
        public ActionResult DuyetChamCongKhongThanh()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,46=1")]
        [HttpGet]
        public async Task<JsonResult> GetDuyetChamCongKhongThanh(string tuNgay, string denNgay, string maPhongBan, string maNV)
        {
            using (var db = new SaveDB())
            {
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);

                if (nguoidung.NhanSu > 0)
                {
                    var model = db.Proc_GetDuyetChamCongKhongThanh(nguoidung.NhanSu, tuNgay, denNgay, maPhongBan, maNV).ToList();
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
        [RoleAuthorize(Roles = "0=0,46=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Duyet_ChamCongKhongThanhAll(List<long> lid)
        {
            JsonStatus rs = new JsonStatus();
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            int iSoLuong = 0;
            if (lid != null && lid.Count > 0)
            {
                try
                {
                    using (SaveDB db = new SaveDB())
                    {
                        db.GhiChu = "Duyệt chấm công không thành";
                        using (var tran = new TransactionScope())
                        {
                            foreach (var idXacNhanCong in lid)
                            {
                                var ditem = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == idXacNhanCong);
                                if (clsFunction.checkKyCongNguoiDung(ditem.Ngay))
                                {
                                    rs.text = "Kỳ tính công đã đóng không thể duyệt chấm công không thành";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                                if (ditem.Block == true)
                                {
                                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                                    int NhanSu = nguoidung.NhanSu;
                                    if (NhanSu == -1)
                                    {
                                        rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                                        return Json(rs, JsonRequestBehavior.AllowGet);
                                    }
                                    ditem.MaTrangThaiDuyet = "3";
                                    ditem.IDNguoiDuyetKeTiep = -1;

                                    int irs = db.SaveChanges();
                                    if (irs > 0)
                                    {
                                        iSoLuong++;
                                        rs.code = 1;
                                    }
                                    else
                                    {
                                        rs.code = 0;
                                        rs.text = "Không có thông tin chấm công không thành cần duyệt";
                                    }
                                }
                            }
                            tran.Complete();
                        }
                    }
                  
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "CCKT", "Duyet_ChamCongKhongThanh");
                    rs.code = 0;
                    rs.text = "Đã có lỗi trong quá trình duyệt chấm công không thành. Liên hệ P.HTTT để được hỗ trợ";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            rs.text += "Số lượng duyệt thành công là <b>" + iSoLuong.ToString() + "</b>";
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "0=0,46=1,44=2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Mass_Cancel_ChamCongKhongThanh(List<long> lid, string LyDo)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (SaveDB db = new SaveDB())
            {
                db.GhiChu = "Hủy chấm công không thành";
                using (var tran = new TransactionScope())
                {
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    try
                    {
                        var ditem = db.TTF_XacNhanCong.Where(it =>lid.Contains(it.IDXacNhanCong)).ToList();
                        foreach (var xnc in ditem)
                        {
                           
                            if (clsFunction.checkKyCongNguoiDung(xnc.Ngay))
                            {
                                rs.text = "Kỳ tính công đã đóng không thể hủy chấm công không thành";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                            if (xnc.Block == true)
                            {
                                int iNhanSu = (int)nguoidung.NhanSu;
                                if (iNhanSu == -1)
                                {
                                    rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                                    return Json(rs, JsonRequestBehavior.AllowGet); ;
                                }
                                //clsFunction.CapNhatTuChoiVaoLichSuDuyet(ditem.IDXacNhanCong, "XNC", iNhanSu, LyDo);
                                //var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == ditem.NhanSu);
                                //var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                                //To = NguoiTao.MailCongTy;
                                //ditem.LyDoHuy = LyDo;
                                //ditem.IDNguoiDuyetKeTiep = iNhanSu;
                                //body = GetEmailHuyXacNhan("XNC", nhansu, ditem);

                                //ht = clsFunction.Get_HT_HETHONG();
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
                                xnc.MaTrangThaiDuyet = "4";
                                xnc.IDNguoiDuyetKeTiep = -1;
                                xnc.LyDoHuy = LyDo;
                                xnc.NguoiTuChoi = iNhanSu;
                                xnc.NgayTuChoi = DateTime.Now;
                            }
                        }
                        int irs = db.SaveChanges();
                        if (irs > 0)
                        {
                            tran.Complete();
                            rs.text = "Từ chối thành công " + irs.ToString() + " yêu cầu chấm công không thành";
                            rs.code = 1;
                        }
                        else
                        {
                            rs.text = "Không có yêu cầu hủy";
                            rs.code = 0;
                        }
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "CCKT", "Mass_Cancel_ChamCongKhongThanh");
                        rs.text = "Đã có lỗi trong quá trình duyệt chấm công không thành. Liên hệ P.HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
            }
          
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult NhanSuChamCongKhongThanh() {
            return View();
        }
        [HttpGet]
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> GetNhanSuChamCongKhongThanh(string tuNgay, string denNgay, string maPhongBan, string maNV)
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
                var model = db.Proc_QuanLyXacNhanCong(tuNgay, denNgay, maPhongBan.Trim(), maNV.Trim(), true).ToList();
                var js = Json(model, JsonRequestBehavior.AllowGet);
                js.MaxJsonLength = int.MaxValue;
                return js;
            }
        }

        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult NhanSuChamCongKhongThanhChiTiet(int id)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Content("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
            }
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
                var item = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == id);
                var NhanSu = (from nhansu in db.TTF_NhanSu
                              join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                              join calamviec in db.TTF_CaLamViec on nhansu.MaCaLamViec equals calamviec.MaCaLamViec
                              where nhansu.NhanSu == item.NhanSu
                              select new { nhansu.MaNV, nhansu.HoVaTen, pb.TenPhong_PhanXuong, calamviec.GioBacDau, calamviec.GioKetThuc }).ToList();
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
                item1.TGRa = NhanSu[0].GioKetThuc.ToString();
                item1.TGVao = NhanSu[0].GioBacDau.ToString();
                item1.Block = item.Block;
                item1.CaDem = item.CaDem;
                return View(item1);
            }
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> Save_ChamCongKhongThanh(XacNhanCong item)
        {
            using (var db = new SaveDB()) {
                db.GhiChu = "Nhân sự sửa chấm công không thành";
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                try
                {
                    if (User.Identity.Name == null || User.Identity.Name == "")
                    {
                        rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                        return Json(rs,JsonRequestBehavior.AllowGet);
                    }
                    if (clsFunction.checkKyCongNhanSu(item.Ngay))
                    {
                        rs.text = "Kỳ tính công đã đóng không thể cập nhật chấm công không thành vào hệ thống";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    var model = db.TTF_XacNhanCong.FirstOrDefault(it => it.IDXacNhanCong == item.IDXacNhanCong);

                    var checkCCKT = db.TTF_XacNhanCong.Where(it => it.Ngay == item.Ngay && it.NhanSu == item.NhanSu && it.GioVao == item.GioVao && it.IDXacNhanCong != item.IDXacNhanCong && it.CaDem == item.CaDem).ToList();
                    if (checkCCKT.Count > 0)
                    {
                        rs.text = "Có tồn tại một thông tin chấm công không thành không thể lưu";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    model.NguyenNhan = item.NguyenNhan;
                    model.CaDem = item.CaDem == null ? false : item.CaDem;
                    model.Ngay = item.Ngay;
                    model.GioVao = item.GioVao;
                    model.NguoiThayDoiLanCuoi = (int)nguoidung.NguoiDung;
                    model.NgayThayDoiLanCuoi = DateTime.Now;
                    model.ThoiGian = new TimeSpan(Convert.ToDateTime(item.ThoiGian).Hour, Convert.ToDateTime(item.ThoiGian).Minute, 0);
                    int r = db.SaveChanges();
                    if (r > 0)
                    {
                        rs.text = "Thành công";
                        rs.code = 1;
                        rs.description = model.IDXacNhanCong.ToString();
                    }
                    else
                    {
                        rs.text = "Thất bại";
                    }
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "CCKT", "Save_ChamCongKhongThanh");
                    rs.code = 0;
                    rs.text = "Lỗi trong quá trình cập nhật liên hệ phòng HTTT để được hỗ trợ";
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult ChamCongKhongThanhBaoCao()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,44-1")]
        [HttpGet]
        public async Task<JsonResult> GetChamCongKhongThanhBaoCao(string TuNgay, string DenNgay, string MaPhongBan, int SoLan)
        {
            JsonStatus js = new JsonStatus();
            try
            {
                using (var db = new SaveDB())
                {
                    var model = db.Proc_BaoCaoXacNhanCong(TuNgay, DenNgay, MaPhongBan, SoLan, true).ToList();
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