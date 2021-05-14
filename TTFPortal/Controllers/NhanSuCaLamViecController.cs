using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Class;
using TTFPortal.Models;
using System.Transactions;
using System.Globalization;

namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "0=0,52=1")]
    [Authorize]
    public class NhanSuCaLamViecController : Controller
    {
        [RoleAuthorize(Roles = "0=0,52=1")]
        public ActionResult QuanLyCalamViec()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,52=1")]
        public async Task<JsonResult> GetQuanLyCalamViec(string tuNgay, string denNgay, string maNV)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_GetNhanSuCaLamViec(tuNgay, denNgay, maNV).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public async  Task<JsonResult> ImportExcelUploadNhanSuCaLamViec(HttpPostedFileBase FileInbox)
        {
            JsonStatus rs = new JsonStatus();
            List<NhanSuCaLamViec> model = new List<NhanSuCaLamViec>();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs,JsonRequestBehavior.AllowGet);
            }
            var nguoidung = Users.GetNguoiDung(User.Identity.Name);

            int iNguoiDung = (int)nguoidung.NguoiDung;
            if (iNguoiDung<0)
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            string Loi = "", CapNhat = "";
            if (FileInbox != null && FileInbox.ContentLength > 0 && (Path.GetExtension(FileInbox.FileName).Equals(".xlsx")))
            {
                using (var db  = new TTF_FACEIDEntities())
                {
                    string fileName = FileInbox.FileName;
                    string UploadDirectory = Server.MapPath("~/Content/Temps/");
                    bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                    if (!folderExists)
                        System.IO.Directory.CreateDirectory(UploadDirectory);
                    string resultFilePath = UploadDirectory + fileName;
                    try
                    {

                        FileInbox.SaveAs(resultFilePath);
                        DataTable dt = clsFunction.getDataTableFromExcel(resultFilePath);
                        DateTime Ngay = DateTime.Now;
                        var listCaLamViec = db.TTF_CaLamViec.ToList();
                     
                        string[] formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                        if (dt.Rows.Count > 0)
                        {
                            var NhanSu = (from a in db.TTF_NhanSu
                                          where a.Del != true
                                          select new { a.MaNV, a.NhanSu, a.HoVaTen }).ToList();
                            TimeSpan GioVao, GioRa;

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                NhanSuCaLamViec add = new NhanSuCaLamViec();
                                if (dt.Rows[i][0].ToString().Trim().Length == 0)
                                    break;
                                if (!DateTime.TryParseExact(dt.Rows[i][2].ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out Ngay))
                                {
                                    Loi += "Không tìm thấy ngày xác nhận công <b>" + dt.Rows[i][2].ToString() + "</b> dòng " + (i + 1).ToString();
                                    continue;
                                }
                                add.Ngay = Ngay.ToString("dd/MM/yyyy");
                                try
                                {
                                    GioVao = TimeSpan.Parse(dt.Rows[i][3].ToString());
                                    add.GioVao = GioVao.ToString();
                                }
                                catch (Exception)
                                {
                                    Loi += "Lỗi giờ vào không hợp lệ <b>" + dt.Rows[i][3].ToString() + "</b> dòng " + (i + 1).ToString();
                                    continue;
                                }
                                try
                                {
                                    GioRa = TimeSpan.Parse(dt.Rows[i][4].ToString());
                                    add.GioRa = GioRa.ToString();
                                }
                                catch (Exception)
                                {
                                    Loi += "Lỗi giờ ra không hợp lệ <b>" + dt.Rows[i][4].ToString() + "</b> dòng " + (i + 1).ToString();
                                    continue;
                                }
                                var nstemp = NhanSu.FirstOrDefault(it => it.MaNV == dt.Rows[i][0].ToString());
                                if (nstemp == null)
                                {
                                    Loi += "Không tìm thấy nhân sự có mã <b>" + dt.Rows[i][0].ToString() + "</b> ở dòng " + (i + 1).ToString() + "<br>";
                                    continue;
                                }
                                add.NhanSu = nstemp.NhanSu;
                                add.HoVaTen = nstemp.HoVaTen;
                                add.MaNV = nstemp.MaNV;
                                add.GhiChu = dt.Rows[i][5].ToString().Trim();
                                if (clsFunction.checkKyCongNhanSu(Ngay))
                                {
                                    Loi += "Kỳ công đã đóng <b> dòng " + (i + 1).ToString() + " " + nstemp.HoVaTen + "</b><br>";
                                    continue;
                                }

                                var KiemTra = db.TTF_NhanSuCaLamViec.FirstOrDefault(it => it.NhanSu == nstemp.NhanSu && it.NgayCong == Ngay);
                                if (KiemTra == null)
                                {
                                    add.SQL =  "INSERT INTO dbo.TTF_NhanSuCaLamViec(NhanSu, NgayCong, GhiChu, NgayImport, NguoiDungImport,GioVao,GioRa) " +
                                             "VALUES(" + nstemp.NhanSu + ",'" + Ngay.ToString("MM/dd/yyyy") + "',N'" + dt.Rows[i][5].ToString().Trim() + "',GETDATE(),'" + iNguoiDung + "','" + GioVao + "','" + GioRa + "')";
                                }
                                model.Add(add);
                            }
                        }

                        if (Loi.Length > 0)
                        {
                            rs.text = Loi;
                            //return Content(MvcHtmlString.Create(Loi).ToHtmlString());
                        }
                        //else
                        //{
                        //    using (var tran = new System.Transactions.TransactionScope())
                        //    {
                        //        if (CapNhat.Length > 0)
                        //        {
                        //            int sb = db.Database.ExecuteSqlCommand(CapNhat);
                        //            tran.Complete();
                        //            rs.text = "Cập nhật thành công " + sb + " dòng thông tin";
                        //        }
                        //        else
                        //        {
                        //            rs.text = "Không có thông tin cập nhật";
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(resultFilePath);
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString() + CapNhat, "NSCLV", "ImportExcelUploadChamCongKhongThanh");
                        rs.text = ex.Message;
                    }
                }
               
            }
            else
            {
                rs.text = "Chưa chọn file import";
            }
            rs.data = model;
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportFileMauNhanSuCaLamViec()
        {
            string filename = Server.MapPath("~/Content/upload/FileMau/NhanSuCaLamViec.xlsx");
            return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NhanSuCaLamViec.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,52=1")]
        public async Task<JsonResult> SaveNhanSuCaLamViec(List<NhanSuCaLamViec> list)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new TTF_FACEIDEntities())
            {
                try
                {
                    string[] sCapNhat = list.Where(it => it.SQL != "" && it.SQL != null).Select(it => it.SQL).ToArray();
                  
                    if (sCapNhat != null && sCapNhat.Length>0)
                    {
                        string s = String.Join(" ", sCapNhat);
                        db.Database.ExecuteSqlCommand(s);
                        int ikq = db.SaveChanges();
                        if (ikq > 0)
                        {
                            rs.code = 1;
                            rs.text = "Cập nhật thành công " + ikq.ToString();
                        }
                    }
                    else
                    {
                        rs.text = "Không có dữ liệu cập nhật";
                    }
                   
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,52=1")]
        public async Task<JsonResult> DelNhanSuCaLamViec(List<NhanSuCaLamViec> list)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            int ikq = 0;
            using (var tran = new TransactionScope())
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Xóa nhân sự ca làm việc";
                    try
                    {
                        string[] formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                        DateTime ngay = new DateTime();
                        string sLoi = "";
                        foreach (var item in list)
                        {
                            if (!DateTime.TryParseExact(item.Ngay, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngay))
                            {
                                sLoi += "Không tìm thấy ngày xác nhận công ngày <b>" + item.Ngay + "</b> Của nhân sự " + item.HoVaTen;
                                continue;
                            }
                            if (clsFunction.checkKyCongNhanSu(ngay))
                            {
                                sLoi += "Ngày <b>" + ngay.ToString("dd/MM/yyyy") + " " + " của nhân sự " + item.HoVaTen + "</b> kỳ công đã khóa <br>";
                                continue;
                            }
                            var del = db.TTF_NhanSuCaLamViec.FirstOrDefault(it => it.NhanSu == item.NhanSu && it.NgayCong == ngay);
                            if (del != null)
                            {
                                db.TTF_NhanSuCaLamViec.Remove(del);
                                db.SaveChanges();
                                ikq++;
                            }
                        }
                        if (sLoi.Length == 0)
                        {
                            rs.code = 1;
                            rs.text = "Xóa thành công " + ikq.ToString();
                            tran.Complete();
                        }
                        else
                        {
                            rs.text = sLoi;
                        }
                    }
                    catch (Exception ex)
                    {
                        rs.text = ex.Message;
                    }
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
    }
}