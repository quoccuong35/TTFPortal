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
    public class CongController : Controller
    {
        // GET: COng
        public ActionResult Index()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,51=1")]
        public ActionResult CongNgay()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,51=1")]
        [HttpGet]
        public async Task<JsonResult> XemCongNgay(string tuNgay,string denNgay,string hoVaTen, string maPhongBan,string maNV)
        {
            var rs = new JsonStatus();
            var model = Json(rs, JsonRequestBehavior.AllowGet);
            List<CongNgay> DLCong = new List<CongNgay>();
            var nguoidung = Users.GetNguoiDung(User.Identity.Name);
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
                db.Database.CommandTimeout = 3600;
                DateTime TuNgay = new DateTime();
                DateTime DenNgay = new DateTime();
                if (tuNgay != null)
                {
                    TuNgay = DateTime.Parse(tuNgay);
                }
                if (denNgay != null)
                {
                    DenNgay = DateTime.Parse(denNgay);
                }
                if (hoVaTen == null)
                    hoVaTen = "";
                if ((maNV == null || maNV=="") && !User.IsInRole("0=0") && !User.IsInRole("52=1") && !User.IsInRole("57=1"))
                    maNV = nguoidung.MaNV;
                if ((maPhongBan == null || maPhongBan=="") && !User.IsInRole("0=0") && !User.IsInRole("52=1"))
                    maPhongBan = nguoidung.PhamVi;
                if (maPhongBan == null)
                    maPhongBan = "";
                if (hoVaTen == null)
                    hoVaTen = "";
                if (maNV == null)
                    maNV = "";
                DateTime ngay = new DateTime();

                DateTime NgayBatDau = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime NgayChotCong = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                NgayBatDau = TuNgay;
                NgayChotCong = DenNgay;
                NgayChotCong = NgayChotCong.AddDays(1);

                var ListNhanSu = db.Proc_NhanSuForCong(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();
                if (ListNhanSu.Count > 0)
                {
                    var DataCaDem = new List<Proc_CongCaDem_Result>();
                    try
                    {
                        DataCaDem = db.Proc_CongCaDem(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();
                    }
                    catch
                    {

                    }
                    var XacNhanCong = db.Proc_XacNhanCong(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();

                    //  var CongNgayHC = db.TTF_CongNgayHieuChinh.Where(o => o.Date >= NgayBatDau && o.Date <= NgayChotCong).ToList();
                    var CongNgayHC = db.Proc_CongNgayHieuChinh(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();

                    //IEnumerable<TangCaCong> TangCaTemp = (from a in db.TTF_TangCa
                    //                                      join b in db.TTF_TangCaChiTiet on a.IDTangCa equals b.IDTangCa
                    //                                      where a.Del != true && a.MaTrangThaiDuyet == "3"
                    //                                      && a.NgayTangCa >= NgayBatDau && a.NgayTangCa <= NgayChotCong
                    //                                      select new TangCaCong
                    //                                      {
                    //                                          NhanSu = b.NhanSu,
                    //                                          NgayTangCa = a.NgayTangCa,
                    //                                          GioBatDau = a.GioBatDau,
                    //                                          GioKetThuc = a.GioKetThuc
                    //                                      }).ToList();
                    // List<proc_tang> TangCa = TangCaTemp.Cast<TangCaCong>().ToList();
                    List<Proc_TangCaChiTiet_Result> TangCa = db.Proc_TangCaChiTiet(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();

                    var DLVTCaNgay1 = db.Proc_CongBinhDuong(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();
                    var DLVTCaNgayTongHop1 = DLVTCaNgay1;
                    // var DLVTSALA1 = new List<Proc_CongBinhDuong_Result>();
                    try
                    {
                        var congtem = db.Proc_CongThuDuc(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();
                        foreach (var item in congtem)
                        {
                            DLVTCaNgayTongHop1.Add(new Proc_CongBinhDuong_Result { UID = item.UID, Name = item.Name, Date = item.Date, Time = item.Time });
                        }
                    }
                    catch
                    {

                    }

                    //DLVTCaNgayTongHop1.AddRange(DLVTSALA1);

                    // cong khuon mặt

                    List<ttf_CongKhuonMat_Result> DLVTKM = new List<ttf_CongKhuonMat_Result>();
                    try
                    {
                        // DLVTKM = db.Database.SqlQuery<RALog>("EXEC dbo.ttf_CongKhuonMat @TuNgay,@DenNgay", new SqlParameter("TuNgay", NgayBatDau.Date), new SqlParameter("DenNgay", NgayChotCong.Date)).ToList();
                        DLVTKM = db.ttf_CongKhuonMat(NgayBatDau.Date, NgayChotCong.Date).ToList();
                        foreach (var item in DLVTKM)
                        {
                            DLVTCaNgayTongHop1.Add(new Proc_CongBinhDuong_Result { UID = item.UID, Time = item.Time, Date = item.Date });
                        }

                    }
                    catch
                    {
                    }

                    //--- du lieu ca làm việc công trinh

                    List<Proc_CanLamViecCongTrinh_Result> CaLamViecCongTrinh = db.Proc_CanLamViecCongTrinh().ToList();


                    //--- 

                    //foreach (var item in DLVTSALA1)
                    //{
                    //    DLVTCaNgayTongHop1.Add(item);
                    //}
                    var DLVTCaNgayTongHop1_final = (from o in DLVTCaNgayTongHop1
                                                    group o by new { o.UID, o.Date.Value.Date }
                                                    into grp
                                                    select new DLVT
                                                    {
                                                        UID = grp.Key.UID,
                                                        Date = grp.Key.Date,
                                                        GioVao = grp.Min(o => o.Time),
                                                        GioRa = grp.Max(o => o.Time)
                                                    }).ToList();

                    var NghiPhep = (from nghiphep in db.TTF_NghiPhep
                                    join nghiphepchitiet in db.TTF_NghiPhepChiTiet on nghiphep.IDNghiPhep equals nghiphepchitiet.IDNghiPhep
                                    where nghiphepchitiet.Ngay >= NgayBatDau && nghiphepchitiet.Ngay <= NgayChotCong
                                    && nghiphepchitiet.Del != true
                                    && nghiphep.Del != true
                                    && nghiphep.MaTrangThaiDuyet == "3"
                                    select new NghiPhepForCong
                                    {
                                        NhanSu = nghiphep.NhanSu.Value,
                                        MaLoaiNghiPhep = nghiphep.MaLoaiNghiPhep,
                                        SoNgay = nghiphepchitiet.SoNgay.Value,
                                        Ngay = nghiphepchitiet.Ngay
                                    }).ToList();

                    List<NghiPhepForCong> NghiPhepForCong = new List<NghiPhepForCong>();
                    NghiPhepForCong.AddRange(NghiPhep);

                    var NgayLe = db.TTF_NgayLe.Where(o => o.Date >= NgayBatDau && o.Date <= NgayChotCong).OrderBy(o => o.Date).ToList();

                    var KyCong = db.TTF_TimekeepingPeriod.ToList();

                    IEnumerable<Pro_CongTac_Result> listCongTac = new List<Pro_CongTac_Result>();

                    listCongTac = db.Pro_CongTac(NgayBatDau, NgayChotCong).ToList().OrderBy(it => it.TuGio);

                    List<Pro_CongTac_Result> listCongTacResult = listCongTac.ToList();

                    //List<TTF_NhanSuCaLamViec> listNhanSuCaLamViec = db.TTF_NhanSuCaLamViec.Where(it => it.NgayCong >= NgayBatDau && it.NgayCong <= NgayChotCong).ToList();
                    List<Proc_NhanSuCaLamViec_Result> listNhanSuCaLamViec = db.Proc_NhanSuCaLamViec(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();

                    string maChamCongCu1 = "", maNVCu1 = "", maNV1 = "", maChamCong1 = "", MaNghiPhep = "";
                    DateTime dpMoc = new DateTime(2020, 10, 01);

                    CongNgay objCongNgay = null;
                    Int32 STT = 0;

                    STT = 1;
                    ngay = NgayBatDau;
                    TimeSpan GioBatDau = new TimeSpan(0, 0, 0);
                    TimeSpan GioKetThuc = new TimeSpan(0, 0, 0);
                    TimeSpan GioRaGiuaCa = new TimeSpan(0, 0, 0);
                    TimeSpan GioVaoGiuaCa = new TimeSpan(0, 0, 0);
                    foreach (var ns in ListNhanSu)
                    {

                        try
                        {
                            ngay = NgayBatDau;
                            GioBatDau = ns.GioBatDauCa.Value;
                            GioKetThuc = ns.GioKetThucCa.Value;
                            GioRaGiuaCa = ns.GioRaGiuaCa.Value;
                            GioVaoGiuaCa = ns.GioVaoGiuaCa.Value;
                            maChamCong1 = ns.MaChamCong;
                            maChamCongCu1 = ns.MaChamCongCu;
                            maNV1 = ns.MaNV;
                            maNVCu1 = ns.MaNVCu;
                          
                            while (ngay.Date < NgayChotCong.Date)
                            {
                                objCongNgay = new CongNgay();
                                ns.GioBatDauCa = GioBatDau;
                                ns.GioKetThucCa = GioKetThuc;
                                ns.GioRaGiuaCa = GioRaGiuaCa;
                                ns.GioVaoGiuaCa = GioVaoGiuaCa;
                                MaNghiPhep = "";
                                if (ngay < dpMoc)
                                {
                                    if (maChamCongCu1 != null && maChamCongCu1.Trim() != "")
                                    {
                                        ns.MaChamCong = maChamCongCu1;
                                    }
                                    if (maNVCu1 != null && maNVCu1.Trim() != "")
                                    {
                                        ns.MaNV = maNVCu1;
                                    }
                                }
                                else
                                {
                                    ns.MaChamCong = maChamCong1;
                                    ns.MaNV = maNV;
                                }
                                objCongNgay = clsFunction.TinhCongNgay1(ns, ngay, NgayBatDau, NgayChotCong,
                                    DLVTCaNgayTongHop1_final, DataCaDem, XacNhanCong, CongNgayHC, TangCa, KyCong, listNhanSuCaLamViec, CaLamViecCongTrinh, DLVTKM);
                                objCongNgay.STT = STT;
                                objCongNgay.Date = ngay;
                                objCongNgay.TenChucVu = ns.TenChucVu;
                                objCongNgay.TenPhong_PhanXuong = ns.TenPhong_PhanXuong;
                                DLCong.Add(objCongNgay);
                                MaNghiPhep = clsFunction.GetMaNghiPhepForCong1(ns, NgayChotCong, ngay, NghiPhepForCong, NgayLe);
                                if (MaNghiPhep != "")
                                {
                                    objCongNgay.LeOrPhep = MaNghiPhep;
                                }
                                ngay = ngay.AddDays(1);
                                STT += 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            rs.code = 0;
                            rs.text = "Lỗi nhân viên " + ns.MaNV + " Ngày công" + ngay.ToString("dd/MM/yyyy") + ex.ToString();
                        }

                    }
                }
               
            }
            rs.data = DLCong;
            rs.code = 1;
            model.MaxJsonLength = int.MaxValue;
            return model;
           // return View();
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
                var startRow = hasHeader ? 3 : 1;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,52-2")]
        public async Task<JsonResult> ImportExcelUploadCongNgay(HttpPostedFileBase FileInbox, string TuNgay, string DenNgay, string LyDo)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            bool hc = false;
            TTF_CongNgayHieuChinh objTTF_CongNgayHieuChinh = null;
            string InDate = "";
            DateTime Date;
            TimeSpan ts;
            string CongHC = "";
            string TangCaHC = "";
            string TangCaSau22HHC = "";
            string InTimeHC = "";
            string OutDateHC = "";
            string OutTimeHC = "";
            string STT = "", MSNV = "";
            Int32 row = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            if (FileInbox != null && FileInbox.ContentLength > 0 && (Path.GetExtension(FileInbox.FileName).Equals(".xlsx")))
            {
                string fileName = FileInbox.FileName;
                string UploadDirectory = Server.MapPath("~/Content/upload/");
                bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(UploadDirectory);
                string resultFilePath = UploadDirectory + fileName;
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NguoiTao = (int)nguoidung.NguoiDung;
                int iNhanSu = 0;
                int kq = 0;
                using (var db = new SaveDB())
                {
                    try
                    {
                        var NhanSu = db.TTF_NhanSu.Where(it => it.Del != true).ToList();
                        FileInbox.SaveAs(resultFilePath);
                        var dt = getDataTableFromExcel(resultFilePath);
                        if (dt.Rows.Count > 0)
                        {
                            row = 1;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                STT = dt.Rows[i][0].ToString();
                                MSNV = dt.Rows[i][1].ToString();
                                InDate = dt.Rows[i][5].ToString();
                                InTimeHC = dt.Rows[i][7].ToString();
                                OutDateHC = dt.Rows[i][9].ToString();
                                OutTimeHC = dt.Rows[i][11].ToString();
                                CongHC = dt.Rows[i][19].ToString();
                                TangCaHC = dt.Rows[i][23].ToString();
                                TangCaSau22HHC = dt.Rows[i][25].ToString();
                                if (CongHC == null)
                                {
                                    CongHC = "";
                                }
                                if (TangCaHC == null)
                                {
                                    TangCaHC = "";
                                }
                                if (TangCaSau22HHC == null)
                                {
                                    TangCaSau22HHC = "";
                                }
                                if (CongHC == "Ro")
                                {
                                    CongHC = CongHC.Trim();
                                }
                                else
                                {
                                    CongHC = CongHC.Trim().ToUpper();
                                }

                                if (TangCaHC == "Ro")
                                {
                                    TangCaHC = TangCaHC.Trim();
                                }
                                else
                                {
                                    TangCaHC = TangCaHC.Trim().ToUpper();
                                }

                                if (TangCaSau22HHC == "Ro")
                                {
                                    TangCaSau22HHC = TangCaSau22HHC.Trim();
                                }
                                else
                                {
                                    TangCaSau22HHC = TangCaSau22HHC.Trim().ToUpper();
                                }

                                if (InTimeHC == null)
                                {
                                    InTimeHC = "";
                                }
                                if (OutDateHC == null)
                                {
                                    OutDateHC = "";
                                }
                                if (OutTimeHC == null)
                                {
                                    OutTimeHC = "";
                                }
                                if (OutDateHC != "")
                                {
                                    Date = DateTime.ParseExact(OutDateHC, "dd/MM/yyyy", new CultureInfo("en-US"));
                                }
                                if (InTimeHC != "")
                                {
                                    ts = TimeSpan.Parse(InTimeHC);
                                }
                                if (OutTimeHC != "")
                                {
                                    ts = TimeSpan.Parse(OutTimeHC);
                                }

                                Date = DateTime.ParseExact(InDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                                var checkcnhc = db.TTF_CongNgayHieuChinh.Where(o => o.MaNV == MSNV
                                    && o.Date == Date).FirstOrDefault();
                                if (checkcnhc == null)
                                {
                                    objTTF_CongNgayHieuChinh = new TTF_CongNgayHieuChinh();
                                    objTTF_CongNgayHieuChinh.Date = Date;
                                    objTTF_CongNgayHieuChinh.MaNV = MSNV;
                                    if (CongHC != ""
                                        || TangCaHC != ""
                                        || TangCaSau22HHC != ""
                                        || InTimeHC != ""
                                        || OutDateHC != ""
                                        || OutTimeHC != "")
                                    {
                                        hc = true;
                                    }
                                    if (hc == true)
                                    {
                                        objTTF_CongNgayHieuChinh.Cong = CongHC;
                                        objTTF_CongNgayHieuChinh.TangCa = TangCaHC;
                                        objTTF_CongNgayHieuChinh.TangCaSau22H = TangCaSau22HHC;
                                        objTTF_CongNgayHieuChinh.InTime = InTimeHC;
                                        objTTF_CongNgayHieuChinh.OutDate = OutDateHC;
                                        objTTF_CongNgayHieuChinh.OutTime = OutTimeHC;
                                        objTTF_CongNgayHieuChinh.NguoiHC = NguoiTao;
                                        objTTF_CongNgayHieuChinh.NgayHC = DateTime.Now;
                                        db.TTF_CongNgayHieuChinh.Add(objTTF_CongNgayHieuChinh);
                                        db.SaveChanges();
                                        kq++;
                                    }
                                }
                                else
                                {
                                    checkcnhc.Cong = CongHC;
                                    checkcnhc.TangCa = TangCaHC;
                                    checkcnhc.TangCaSau22H = TangCaSau22HHC;
                                    checkcnhc.InTime = InTimeHC;
                                    checkcnhc.OutDate = OutDateHC;
                                    checkcnhc.OutTime = OutTimeHC;
                                    checkcnhc.NguoiHC = NguoiTao;
                                    checkcnhc.NgayHC = DateTime.Now;
                                    db.SaveChanges();
                                    kq++;
                                }
                                row += 1;
                            }
                            if (kq > 0)
                            {
                                rs.code = 1;
                                rs.text = "Thành công";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rs.text = "Lỗi hệ thống ở nhân sự " + iNhanSu.ToString() + ex.ToString();
                        rs.code = 0;
                        return Json(rs, JsonRequestBehavior.AllowGet);

                    }
                }

            }
            else
            {
                rs.text = "Không tìm thấy file dữ liệu import";
                rs.code = 0;
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "0=0,51=1")]
        public ActionResult CongThang()
        {
            return View();
        }

        [RoleAuthorize(Roles = "0=0,51=1")]
        [HttpGet]
        public async Task<JsonResult> XemCongThang(int thang,int nam, string hoVaTen, string maPhongBan, string maNV)
        {
            var rs = new JsonStatus();
            CongThangModel model = new CongThangModel();
            List<CongThang> CongThang = new List<CongThang>();
            List<NgayCong> listCongNgay = new List<NgayCong>();
            //  var model = Json(rs, JsonRequestBehavior.AllowGet);
            List<CongNgay> DLCong = new List<CongNgay>();
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                db.Database.CommandTimeout = 3600;
                DateTime TuNgay = new DateTime();
                DateTime DenNgay = new DateTime();

                clsFunction.GetKyCong(nam, thang,ref TuNgay, ref DenNgay);
                if (hoVaTen == null)
                    hoVaTen = "";
                if ((maNV == null || maNV == "") && !User.IsInRole("0=0") && !User.IsInRole("52=1") && !User.IsInRole("57=1"))
                    maNV = nguoidung.MaNV;
                if ((maPhongBan == null || maPhongBan == "") && !User.IsInRole("0=0") && !User.IsInRole("52=1"))
                    maPhongBan = nguoidung.PhamVi;

                if (maPhongBan == null)
                    maPhongBan = "";
                if (hoVaTen == null)
                    hoVaTen = "";
                if (maNV == null)
                    maNV = "";


                DateTime ngay = new DateTime();

                DateTime NgayBatDau = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime NgayChotCong = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                NgayBatDau = TuNgay;
                NgayChotCong = DenNgay;
                NgayChotCong = NgayChotCong.AddDays(1);

                var ListNhanSu = db.Proc_NhanSuForCong(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();
                if (ListNhanSu.Count > 0)
                {
                    var DataCaDem = new List<Proc_CongCaDem_Result>();
                    try
                    {
                        DataCaDem = db.Proc_CongCaDem(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();
                    }
                    catch
                    {

                    }
                    var XacNhanCong = db.Proc_XacNhanCong(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();

                    //  var CongNgayHC = db.TTF_CongNgayHieuChinh.Where(o => o.Date >= NgayBatDau && o.Date <= NgayChotCong).ToList();
                    var CongNgayHC = db.Proc_CongNgayHieuChinh(NgayBatDau.Date, NgayChotCong.Date, maNV.Trim(), hoVaTen.Trim(), maPhongBan.Trim()).ToList();

                    //IEnumerable<TangCaCong> TangCaTemp = (from a in db.TTF_TangCa
                    //                                      join b in db.TTF_TangCaChiTiet on a.IDTangCa equals b.IDTangCa
                    //                                      where a.Del != true && a.MaTrangThaiDuyet == "3"
                    //                                      && a.NgayTangCa >= NgayBatDau && a.NgayTangCa <= NgayChotCong
                    //                                      select new TangCaCong
                    //                                      {
                    //                                          NhanSu = b.NhanSu,
                    //                                          NgayTangCa = a.NgayTangCa,
                    //                                          GioBatDau = a.GioBatDau,
                    //                                          GioKetThuc = a.GioKetThuc
                    //                                      }).ToList();
                    // List<proc_tang> TangCa = TangCaTemp.Cast<TangCaCong>().ToList();
                    List<Proc_TangCaChiTiet_Result> TangCa = db.Proc_TangCaChiTiet(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();

                    var DLVTCaNgay1 = db.Proc_CongBinhDuong(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();
                    var DLVTCaNgayTongHop1 = DLVTCaNgay1;
                    // var DLVTSALA1 = new List<Proc_CongBinhDuong_Result>();
                    try
                    {
                        var congtem = db.Proc_CongThuDuc(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();
                        foreach (var item in congtem)
                        {
                            DLVTCaNgayTongHop1.Add(new Proc_CongBinhDuong_Result { UID = item.UID, Name = item.Name, Date = item.Date, Time = item.Time });
                        }
                    }
                    catch
                    {

                    }

                    //DLVTCaNgayTongHop1.AddRange(DLVTSALA1);

                    // cong khuon mặt

                    List<ttf_CongKhuonMat_Result> DLVTKM = new List<ttf_CongKhuonMat_Result>();
                    try
                    {
                        // DLVTKM = db.Database.SqlQuery<RALog>("EXEC dbo.ttf_CongKhuonMat @TuNgay,@DenNgay", new SqlParameter("TuNgay", NgayBatDau.Date), new SqlParameter("DenNgay", NgayChotCong.Date)).ToList();
                        DLVTKM = db.ttf_CongKhuonMat(NgayBatDau.Date, NgayChotCong.Date).ToList();
                        foreach (var item in DLVTKM)
                        {
                            DLVTCaNgayTongHop1.Add(new Proc_CongBinhDuong_Result { UID = item.UID, Time = item.Time, Date = item.Date });
                        }

                    }
                    catch
                    {
                    }

                    //--- du lieu ca làm việc công trinh

                    List<Proc_CanLamViecCongTrinh_Result> CaLamViecCongTrinh = db.Proc_CanLamViecCongTrinh().ToList();


                    //--- 

                    //foreach (var item in DLVTSALA1)
                    //{
                    //    DLVTCaNgayTongHop1.Add(item);
                    //}
                    var DLVTCaNgayTongHop1_final = (from o in DLVTCaNgayTongHop1
                                                    group o by new { o.UID, o.Date.Value.Date }
                                                    into grp
                                                    select new DLVT
                                                    {
                                                        UID = grp.Key.UID,
                                                        Date = grp.Key.Date,
                                                        GioVao = grp.Min(o => o.Time),
                                                        GioRa = grp.Max(o => o.Time)
                                                    }).ToList();

                    var NghiPhep = (from nghiphep in db.TTF_NghiPhep
                                    join nghiphepchitiet in db.TTF_NghiPhepChiTiet on nghiphep.IDNghiPhep equals nghiphepchitiet.IDNghiPhep
                                    where nghiphepchitiet.Ngay >= NgayBatDau && nghiphepchitiet.Ngay <= NgayChotCong
                                    && nghiphepchitiet.Del != true
                                    && nghiphep.Del != true
                                    && nghiphep.MaTrangThaiDuyet == "3"
                                    select new NghiPhepForCong
                                    {
                                        NhanSu = nghiphep.NhanSu.Value,
                                        MaLoaiNghiPhep = nghiphep.MaLoaiNghiPhep,
                                        SoNgay = nghiphepchitiet.SoNgay.Value,
                                        Ngay = nghiphepchitiet.Ngay
                                    }).ToList();

                    List<NghiPhepForCong> NghiPhepForCong = new List<NghiPhepForCong>();
                    NghiPhepForCong.AddRange(NghiPhep);

                    var NgayLe = db.TTF_NgayLe.Where(o => o.Date >= NgayBatDau && o.Date <= NgayChotCong).OrderBy(o => o.Date).ToList();

                    var KyCong = db.TTF_TimekeepingPeriod.ToList();

                    IEnumerable<Pro_CongTac_Result> listCongTac = new List<Pro_CongTac_Result>();

                    listCongTac = db.Pro_CongTac(NgayBatDau, NgayChotCong).ToList().OrderBy(it => it.TuGio);

                    List<Pro_CongTac_Result> listCongTacResult = listCongTac.ToList();

                    //List<TTF_NhanSuCaLamViec> listNhanSuCaLamViec = db.TTF_NhanSuCaLamViec.Where(it => it.NgayCong >= NgayBatDau && it.NgayCong <= NgayChotCong).ToList();
                    List<Proc_NhanSuCaLamViec_Result> listNhanSuCaLamViec = db.Proc_NhanSuCaLamViec(NgayBatDau.Date, NgayChotCong.Date, maNV, hoVaTen, maPhongBan).ToList();

                    string  MaNghiPhep = "";
                    DateTime dpMoc = new DateTime(2020, 10, 01);
                    var LoaiCong = db.TTF_LoaiCong.ToList();
                    CongNgay objCongNgay = null;
                    Int32 STT = 0;

                    STT = 1;
                    ngay = NgayBatDau;
                    TimeSpan GioBatDau = new TimeSpan(0, 0, 0);
                    TimeSpan GioKetThuc = new TimeSpan(0, 0, 0);
                    TimeSpan GioRaGiuaCa = new TimeSpan(0, 0, 0);
                    TimeSpan GioVaoGiuaCa = new TimeSpan(0, 0, 0);
                    double CountO = 0;
                    Int32 Cot = 0;
                    double CountNV = 0;
                    double CountB = 0;
                    double CountTN = 0;
                    double CountRo = 0;
                    double CountM = 0;
                    double CountTS = 0;
                    double CountP = 0;
                    double CountR = 0;
                    double CountL = 0;
                    double CountP_2 = 0;
                    double CountCTR = 0;
                    bool CoCong = false;
                    bool CoTC = false;
                    double et = 0.0;
                    bool RemoveL = false;
                    string MaCaLamViec = "", maChamCongCu = "", maNVCu = "", maChamCong = "";
                    var CongThangHC = db.TTF_CongThangHieuChinh.Where(o => o.Nam == nam.ToString() && o.Thang == thang.ToString()).ToList();
                    var NhanSu_PhepThang = db.TTF_NhanSu_PhepThang.Where(it => it.Thang == thang && it.Nam == nam).ToList();
                    CongThangNgayTheoDong objCongThangNgayTheoDong = null;
                    List<CongThangNgayTheoDong> ListCongThangNgayTheoDong = new List<CongThangNgayTheoDong>();
                    bool addNgayCong = true;
                    foreach (var ns in ListNhanSu)
                    {
                        
                        try
                        {
                            GioBatDau = ns.GioBatDauCa.Value;
                            GioKetThuc = ns.GioKetThucCa.Value;
                            GioVaoGiuaCa = ns.GioVaoGiuaCa.Value;
                            GioRaGiuaCa = ns.GioRaGiuaCa.Value;
                            maChamCong = ns.MaChamCong;
                            maChamCongCu = ns.MaChamCongCu;
                            maNV = ns.MaNV;
                            maNVCu = ns.MaNVCu;
                            if (MaCaLamViec == "Ca8_T2")
                            {
                                MaCaLamViec = "Ca8_T1";
                            }
                            else if (MaCaLamViec == "Ca12_T2" || MaCaLamViec == "Ca12_T3" || MaCaLamViec == "Ca12_T4")
                            {
                                MaCaLamViec = "Ca12_T1";
                            }
                            else if (MaCaLamViec == "Ca2_1" || MaCaLamViec == "Ca2_2")
                            {
                                MaCaLamViec = "Ca2";
                            }
                            ngay = NgayBatDau;
                            var objNhanSu_PhepThang = NhanSu_PhepThang.FirstOrDefault(o => o.Nam == nam
                                   && o.Thang == thang && o.NhanSu == ns.NhanSu
                                  );
                            
                            while (ngay.Date < NgayChotCong.Date)
                            {
                                if (addNgayCong)
                                {
                                    NgayCong addNgay = new NgayCong();
                                    addNgay.STT = ngay.Date.Day;
                                    addNgay.HeaderText = ngay.ToString("dd/MM");
                                    if (ngay.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        addNgay.ChuNhat = true;
                                    }
                                    listCongNgay.Add(addNgay);
                                }
                                
                                if (ngay < dpMoc)
                                {
                                    if (maChamCongCu != null && maChamCongCu.Trim() != "")
                                    {
                                        ns.MaChamCong = maChamCongCu;
                                    }
                                    if (maNVCu != null && maNVCu.Trim() != "")
                                    {
                                        ns.MaNV = maNVCu;
                                    }
                                }
                                else
                                {
                                    ns.MaChamCong = maChamCong;
                                    ns.MaNV = maNV;
                                }
                                ns.GioBatDauCa = GioBatDau;
                                ns.GioKetThucCa = GioKetThuc;
                                ns.GioVaoGiuaCa = GioVaoGiuaCa;
                                ns.GioRaGiuaCa = GioRaGiuaCa;
                                objCongThangNgayTheoDong = new CongThangNgayTheoDong();
                                objCongThangNgayTheoDong.MSNV = ns.MaNV;
                                objCongThangNgayTheoDong.MaChamCong = ns.MaChamCong;
                                objCongThangNgayTheoDong.HoVaTen = ns.HoVaTen;
                                objCongThangNgayTheoDong.TenPhong_PhanXuong = ns.TenPhong_PhanXuong;
                                //objCongThangNgayTheoDong.TenToChuyen = ns.TenToChuyen;
                                objCongThangNgayTheoDong.TenToChuyen = ns.MaToChuyen;//Tổ chuyền nhập text trực tiếp
                                objCongThangNgayTheoDong.TenChucVu = ns.TenChucVu;
                                //objCongThangNgayTheoDong.SoNgayPhepConLai = Convert.ToDouble(ns.SoNgayPhepConLai);
                                if (objNhanSu_PhepThang != null)
                                {
                                    objCongThangNgayTheoDong.SoNgayPhepConLai = Convert.ToDouble(objNhanSu_PhepThang.SoNgayPhep);
                                }
                                else
                                {
                                    objCongThangNgayTheoDong.SoNgayPhepConLai = 0;
                                }

                                objCongThangNgayTheoDong.NgayCongChuan = Convert.ToInt32(ns.NgayCongChuan);
                                objCongThangNgayTheoDong.ngay = ngay;
                                objCongNgay = clsFunction.TinhCongNgay1(ns, ngay, NgayBatDau, NgayChotCong,
                                    DLVTCaNgayTongHop1_final, DataCaDem, XacNhanCong, CongNgayHC, TangCa, KyCong, listNhanSuCaLamViec, CaLamViecCongTrinh, DLVTKM);
                                objCongNgay.STT = STT;
                                objCongNgay.Date = ngay;
                                objCongNgay.TenChucVu = ns.TenChucVu;
                                objCongNgay.TenPhong_PhanXuong = ns.TenPhong_PhanXuong;
                                objCongThangNgayTheoDong.CongTac = objCongNgay.CongTac.ToString();
                                objCongThangNgayTheoDong.VaoTreVeSom = objCongNgay.VaoTreVeSom;
                                if (objCongNgay.InTime.Trim() != "")
                                {
                                    objCongThangNgayTheoDong.GioVao = TimeSpan.Parse(objCongNgay.InTime);
                                }
                                if (objCongNgay.OutTime.Trim() != "")
                                {
                                    objCongThangNgayTheoDong.GioRa = TimeSpan.Parse(objCongNgay.OutTime);
                                }
                                if (objCongNgay.CongHC == "")
                                {
                                    objCongThangNgayTheoDong.Cong = objCongNgay.Cong.ToString();
                                }
                                else
                                {
                                    objCongThangNgayTheoDong.Cong = objCongNgay.CongHC;
                                }
                                if (objCongNgay.TangCaHC == "")
                                {
                                    objCongThangNgayTheoDong.TangCa = objCongNgay.TangCa.ToString();
                                }
                                else
                                {
                                    objCongThangNgayTheoDong.TangCa = objCongNgay.TangCaHC;
                                }
                                //if (clsFunction.CheckDouble(objCongThangNgayTheoDong.TangCa.Trim()) == true)
                                //{

                                //}
                                if (objCongNgay.TangCaSau22HHC == "")
                                {
                                    objCongThangNgayTheoDong.TangCaSau22H = objCongNgay.TangCaSau22H.ToString();
                                }
                                else
                                {
                                    objCongThangNgayTheoDong.TangCaSau22H = objCongNgay.TangCaSau22HHC;
                                }

                                MaNghiPhep = clsFunction.GetMaNghiPhepForCong1(ns, NgayChotCong, ngay, NghiPhepForCong, NgayLe);
                                if (MaNghiPhep != "")
                                {
                                    //objCongNgay.CongHC != "TS" -> TH người nghỉ TS cũ không có pyc thì ngày lễ bị ưu tiên lấy = L -> muốn lấy = TS
                                    //Ưu tiên lấy phép trước
                                    switch (MaNghiPhep)
                                    {
                                        case "L"://Nếu ngày lễ -> dòng TC = Công + TC, dòng Công = L, trừ 
                                                 //trường hợp nhân sự đặc biệt loại Không chấm công   
                                            var checkTangCa = TangCa.Where(it => it.NhanSu == ns.NhanSu && it.NgayTangCa.Value.Date == ngay.Date).ToList();
                                            if (clsFunction.CheckDouble(objCongThangNgayTheoDong.Cong) == true
                                                && clsFunction.CheckDouble(objCongThangNgayTheoDong.TangCa) == true
                                                && ns.MaLoaiDacBiet != 1 && checkTangCa.Count > 0)
                                            {
                                                objCongThangNgayTheoDong.TangCa = (Convert.ToDouble(objCongThangNgayTheoDong.Cong)
                                                    + Convert.ToDouble(objCongThangNgayTheoDong.TangCa)).ToString();
                                            }
                                            else
                                            {
                                                objCongThangNgayTheoDong.Cong = objCongThangNgayTheoDong.TangCa = "";
                                            }
                                            if (objCongNgay.CongHC != "TS" && objCongNgay.CongHC != "CTR")
                                            {
                                                objCongThangNgayTheoDong.Cong = MaNghiPhep;
                                            }
                                            break;
                                        case "P/2":
                                            CoCong = false;
                                            if (clsFunction.CheckDouble(objCongThangNgayTheoDong.Cong) == true)
                                            {
                                                if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 0)
                                                {
                                                    CoCong = true;
                                                    //(Ca1 hoặc Ca2) và (P/2 hoặc Ro/2) thì Công tối đa là 8
                                                    //(Ca8_T1 hoặc Ca12_T1) và (P/2 hoặc Ro/2) thì Công tối đa là 8
                                                    if (MaCaLamViec.Trim() == "Ca1" || MaCaLamViec.Trim() == "Ca2")
                                                    {
                                                        if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 4)
                                                        {
                                                            objCongThangNgayTheoDong.Cong = "4";
                                                        }
                                                    }
                                                    else if (MaCaLamViec.Trim() == "Ca8_T1" || MaCaLamViec.Trim() == "Ca12_T1")
                                                    {
                                                        if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 8)
                                                        {
                                                            objCongThangNgayTheoDong.Cong = "8";
                                                        }
                                                    }
                                                }
                                            }
                                            CoTC = false;
                                            if (clsFunction.CheckDouble(objCongThangNgayTheoDong.TangCa) == true)
                                            {
                                                if (Convert.ToDouble(objCongThangNgayTheoDong.TangCa) > 0)
                                                {
                                                    CoTC = true;
                                                }
                                            }
                                            if (CoCong == false)
                                            {
                                                objCongThangNgayTheoDong.Cong = MaNghiPhep;
                                            }
                                            else if (CoCong == true && CoTC == false)
                                            {
                                                objCongThangNgayTheoDong.TangCa = MaNghiPhep;
                                            }
                                            else if (CoCong == true && CoTC == true)
                                            {
                                                objCongThangNgayTheoDong.TangCaSau22H = MaNghiPhep;
                                            }
                                            break;
                                        case "Ro/2":
                                            CoCong = false;
                                            if (clsFunction.CheckDouble(objCongThangNgayTheoDong.Cong) == true)
                                            {
                                                if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 0)
                                                {
                                                    CoCong = true;
                                                    //(Ca1 hoặc Ca2) và (P/2 hoặc Ro/2) thì Công tối đa là 8
                                                    //(Ca8_T1 hoặc Ca12_T1) và (P/2 hoặc Ro/2) thì Công tối đa là 8
                                                    if (MaCaLamViec.Trim() == "Ca1" || MaCaLamViec.Trim() == "Ca2")
                                                    {
                                                        if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 4)
                                                        {
                                                            objCongThangNgayTheoDong.Cong = "4";
                                                        }
                                                    }
                                                    else if (MaCaLamViec.Trim() == "Ca8_T1" || MaCaLamViec.Trim() == "Ca12_T1")
                                                    {
                                                        if (Convert.ToDouble(objCongThangNgayTheoDong.Cong) > 8)
                                                        {
                                                            objCongThangNgayTheoDong.Cong = "8";
                                                        }
                                                    }
                                                }
                                            }
                                            CoTC = false;
                                            if (clsFunction.CheckDouble(objCongThangNgayTheoDong.TangCa) == true)
                                            {
                                                if (Convert.ToDouble(objCongThangNgayTheoDong.TangCa) > 0)
                                                {
                                                    CoTC = true;
                                                }
                                            }
                                            if (CoCong == false)
                                            {
                                                objCongThangNgayTheoDong.Cong = MaNghiPhep;
                                            }
                                            else if (CoCong == true && CoTC == false)
                                            {
                                                objCongThangNgayTheoDong.TangCa = MaNghiPhep;
                                            }
                                            else if (CoCong == true && CoTC == true)
                                            {
                                                objCongThangNgayTheoDong.TangCaSau22H = MaNghiPhep;
                                            }
                                            break;
                                        default:
                                            objCongThangNgayTheoDong.Cong = MaNghiPhep;
                                            objCongThangNgayTheoDong.TangCa = "";
                                            objCongThangNgayTheoDong.TangCaSau22H = "";
                                            break;
                                    }
                                }
                                //Ngược lại lấy bên công theo ngày
                                //Ngược lại = O
                                else if (objCongThangNgayTheoDong.Cong.Trim() == "0")
                                {
                                    if (objCongNgay.ThieuVanTay == true)
                                    {
                                        objCongThangNgayTheoDong.Cong = "TVT";
                                    }
                                    else
                                    {
                                        if (ns.NgayCongChuan == 24)
                                        {
                                            if (((ngay.DayOfWeek != DayOfWeek.Sunday && ngay.DayOfWeek != DayOfWeek.Saturday)
                                            || (ngay.DayOfWeek == DayOfWeek.Saturday && objCongNgay.TuanThuMayCuaThang == 1)
                                            || (ngay.DayOfWeek == DayOfWeek.Saturday && objCongNgay.TuanThuMayCuaThang == 3))
                                                && ngay <= DateTime.Now)
                                            {
                                                objCongThangNgayTheoDong.Cong = "O";
                                            }
                                            else
                                            {
                                                objCongThangNgayTheoDong.Cong = "";
                                            }
                                        }
                                        else if (ns.NgayCongChuan == 26)
                                        {
                                            if (ngay.DayOfWeek != DayOfWeek.Sunday && ngay <= DateTime.Now)
                                            {
                                                objCongThangNgayTheoDong.Cong = "O";
                                            }
                                            else
                                            {
                                                objCongThangNgayTheoDong.Cong = "";
                                            }
                                        }
                                        else if (ns.NgayCongChuan == 22)
                                        {
                                            if (ngay.DayOfWeek != DayOfWeek.Sunday && ngay.DayOfWeek != DayOfWeek.Saturday && ngay <= DateTime.Now)
                                            {
                                                objCongThangNgayTheoDong.Cong = "O";
                                            }
                                            else
                                            {
                                                objCongThangNgayTheoDong.Cong = "";
                                            }
                                        }
                                        else
                                        {
                                            if (ngay.DayOfWeek != DayOfWeek.Sunday && ngay <= DateTime.Now)
                                            {
                                                objCongThangNgayTheoDong.Cong = "O";
                                            }
                                            else
                                            {
                                                objCongThangNgayTheoDong.Cong = "";
                                            }
                                        }
                                    }
                                }

                                if (objCongThangNgayTheoDong.TangCa.Trim() == "0")
                                {
                                    objCongThangNgayTheoDong.TangCa = "";
                                }
                                if (objCongThangNgayTheoDong.TangCaSau22H.Trim() == "0")
                                {
                                    objCongThangNgayTheoDong.TangCaSau22H = "";
                                }
                                ListCongThangNgayTheoDong.Add(objCongThangNgayTheoDong);
                                ngay = ngay.AddDays(1);
                            }
                            addNgayCong = false;
                            //Xử lý ngày thứ 7 đối với nhân sự có ngày công chuẩn 24
                            if (ns.NgayCongChuan == 24 && ns.KhongTinhTC != true)
                            {
                                int SoNgayT7DaTinhTC = 0;
                                TimeSpan GioVao;
                                TimeSpan GioRa;
                                List<CongThangNgayTheoDong> ListCongThangNgayTheoDong1NST7CC = new List<CongThangNgayTheoDong>();
                                var ListCongThangNgayTheoDong1NST7 = ListCongThangNgayTheoDong.Where(o => o.MSNV.Trim() == ns.MaNV.Trim()
                                    && o.ngay.DayOfWeek == DayOfWeek.Saturday).ToList();
                                foreach (var item in ListCongThangNgayTheoDong1NST7)
                                {
                                    if (clsFunction.CheckDouble(item.Cong) == true
                                        || item.Cong.Trim() == "P"
                                        || item.Cong.Trim() == "B"
                                        || item.Cong.Trim() == "R"
                                        || item.Cong.Trim() == "CT")
                                    {
                                        ListCongThangNgayTheoDong1NST7CC.Add(item);
                                    }
                                }
                                foreach (var item in ListCongThangNgayTheoDong1NST7CC)
                                {
                                    //var CheckTangCa = TangCa.Where(o => o.NhanSu == ns.NhanSu
                                    //            && o.NgayTangCa == item.ngay).FirstOrDefault();

                                    var ListTangCa = (TangCa.Where(o => o.NhanSu == ns.NhanSu
                                           && o.NgayTangCa == item.ngay
                                           )).OrderBy(it => it.GioBatDau).OrderBy(p => p.GioKetThuc).ToList();
                                    var CheckTangCa = ListTangCa.OrderBy(it => it.GioBatDau).FirstOrDefault();
                                    if (CheckTangCa != null && item.GioVao != new TimeSpan(0, 0, 0) && item.GioRa != new TimeSpan(0, 0, 0) && CheckTangCa.GioBatDau < ns.GioKetThucCa)
                                    {
                                        if (ListCongThangNgayTheoDong1NST7CC.Count >= (SoNgayT7DaTinhTC + 3) && ListTangCa.Count > 0)
                                        {
                                            if (ListTangCa.Count == 1)
                                            {
                                                GioVao = item.GioVao;
                                                if (ListTangCa[0].GioBatDau > GioVao)
                                                {
                                                    GioVao = (TimeSpan)ListTangCa[0].GioBatDau;
                                                }
                                                GioRa = item.GioRa;
                                                if (ListTangCa[0].GioKetThuc < GioRa)
                                                {
                                                    GioRa = (TimeSpan)ListTangCa[0].GioKetThuc;
                                                }
                                                double dTruGio = 0;
                                                if (GioRa >= new TimeSpan(12, 30, 0))
                                                {
                                                    dTruGio = 0.5;
                                                }
                                                else if ((GioRa >= new TimeSpan(12, 0, 0)) && GioRa < new TimeSpan(12, 30, 0))
                                                {
                                                    dTruGio = (GioRa - (new TimeSpan(12, 0, 0))).TotalHours;
                                                }
                                                item.TangCa = Math.Round((GioRa - GioVao).TotalHours - dTruGio, 2).ToString();
                                                item.Cong = "";

                                                SoNgayT7DaTinhTC++;
                                            }
                                            else//Xử lý 1 ngày nhiều yêu cầu tăng ca
                                            {
                                                double SoGioTangCa = 0.0;
                                                TimeSpan CanDuoiTC, CanTrenTC;
                                                CanTrenTC = item.GioVao;
                                                CanDuoiTC = item.GioRa;
                                                int iTem = 0;
                                                double dTruGio = 0;
                                                foreach (var tc in ListTangCa)
                                                {
                                                    GioVao = CanTrenTC;
                                                    if (tc.GioBatDau.Value > GioVao)
                                                    {
                                                        GioVao = tc.GioBatDau.Value;
                                                    }
                                                    GioRa = CanDuoiTC;
                                                    if (tc.GioKetThuc < GioRa)
                                                    {
                                                        GioRa = (TimeSpan)tc.GioKetThuc;
                                                    }
                                                    if (iTem > 0)
                                                    {
                                                        if (GioVao <= ListTangCa[iTem - 1].GioKetThuc)
                                                        {
                                                            GioVao = (TimeSpan)(ListTangCa[iTem - 1].GioKetThuc);
                                                        }
                                                    }
                                                    dTruGio = 0;
                                                    if (GioRa >= new TimeSpan(12, 30, 0) && iTem == 0)
                                                    {
                                                        dTruGio = 0.5;
                                                    }
                                                    else if ((GioRa >= new TimeSpan(12, 0, 0)) && GioRa < new TimeSpan(12, 30, 0) && iTem == 0)
                                                    {
                                                        dTruGio = (GioRa - (new TimeSpan(12, 0, 0))).TotalHours;
                                                    }
                                                    if (GioRa > GioVao)
                                                        SoGioTangCa += (GioRa - GioVao).TotalHours - dTruGio;
                                                    iTem++;
                                                }
                                                item.TangCa = Math.Round(SoGioTangCa, 2).ToString();
                                                item.Cong = "";
                                                SoNgayT7DaTinhTC++;
                                            }
                                        }
                                    }
                                    // code cu
                                    //if (CheckTangCa != null && item.GioVao != new TimeSpan(0,0,0) && item.GioRa != new TimeSpan(0,0,0) && CheckTangCa.GioBatDau < ns.GioKetThucCa)
                                    //{
                                    //    //vd: có 3 ngày thứ 7, ngày thứ 7 đang xét muốn tính tăng ca thì
                                    //    //phải có tối thiểu 3 ngày thứ 7 và chưa có ngày thứ 7 nào đã tính tăng ca
                                    //    //và giờ bắt đầu tăng ca trên phiếu phải <= 09:00
                                    //    //if (ListCongThangNgayTheoDong1NST7CC.Count >= (SoNgayT7DaTinhTC + 3)
                                    //    //&& TimeSpan.Compare((TimeSpan)CheckTangCa.GioBatDau, new TimeSpan(9, 0, 0)) <= 0)
                                    //    if (ListCongThangNgayTheoDong1NST7CC.Count >= (SoNgayT7DaTinhTC + 3))
                                    //    {
                                    //        GioVao = item.GioVao;
                                    //        if (CheckTangCa.GioBatDau > GioVao)
                                    //        {
                                    //            GioVao = (TimeSpan)CheckTangCa.GioBatDau;
                                    //        }
                                    //        GioRa = item.GioRa;
                                    //        if (CheckTangCa.GioKetThuc < GioRa)
                                    //        {
                                    //            GioRa = (TimeSpan)CheckTangCa.GioKetThuc;
                                    //        }
                                    //        double dTruGio = 0;
                                    //        if (GioRa >= new TimeSpan(12, 30, 0))
                                    //        {
                                    //            dTruGio = 0.5;
                                    //        }
                                    //        else if ((GioRa >= new TimeSpan(12, 0, 0)) && GioRa < new TimeSpan(12, 30, 0))
                                    //        {
                                    //            dTruGio = (GioRa - (new TimeSpan(12, 0, 0))).TotalHours;
                                    //        }
                                    //        item.TangCa = Math.Round((GioRa - GioVao).TotalHours - dTruGio, 2).ToString();
                                    //        item.Cong = "";

                                    //        SoNgayT7DaTinhTC++;
                                    //    }
                                    //}
                                }
                            }

                            // xử lý công sau ngàylễ tính chuyên cần add cong ngay
                            try
                            {
                                //MaCaLamViec = ns.MaCaLamViec.Trim();
                                var LCongThangNgayTheoDong = ListCongThangNgayTheoDong.Where(o => o.MSNV == ns.MaNV).OrderBy(o => o.ngay).ToList();

                                //sau Lễ mà O/NV (ko có công) thì L bỏ
                                var LCongThangNgayTheoDongCheckL = LCongThangNgayTheoDong;
                                bool StopCheckL = false;
                                CountO = 0;
                                bool IsNV = false;
                                int SoItemCanKtTruocL = 0;
                                CountO = 0;
                                CountNV = 0;
                                CountB = 0;
                                CountTN = 0;
                                CountRo = 0;
                                CountM = 0;
                                CountTS = 0;
                                CountP = 0;
                                CountR = 0;
                                CountL = 0;
                                CountP_2 = 0;
                                CountCTR = 0;
                                CongThang Cong = new CongThang();
                                CongThang TC = new CongThang();
                                CongThang TCS22H = new CongThang();
                                for (int i = 0; i < LCongThangNgayTheoDong.Count; i++)
                                {
                                    // Xữ lý ngày lễ
                                    if (LCongThangNgayTheoDong[i].Cong.Trim() == "L")
                                    {
                                        RemoveL = true;
                                        //Các ngày sau L có ngày nào có công (có số / P / P/2 / TN) thì tính L
                                        for (int j = i + 1; j < LCongThangNgayTheoDongCheckL.Count; j++)
                                        {
                                            if (clsFunction.CheckDouble(LCongThangNgayTheoDongCheckL[j].Cong.Trim()) == true
                                                || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "P"
                                                || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "P/2"
                                                || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "TN"
                                                || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "L"
                                                || ns.MaTinhTrang == "1"
                                                )
                                            {
                                                RemoveL = false;
                                                StopCheckL = true;
                                                break;
                                            }
                                        }
                                        if (i == LCongThangNgayTheoDong.Count - 1 && ns.MaTinhTrang == "1" && ns.NgayNghiViec1 > LCongThangNgayTheoDong[i].ngay)
                                        {
                                            RemoveL = false;
                                            StopCheckL = true;
                                        }
                                        if (StopCheckL == false)
                                        {
                                            //Nếu ngày kế tiếp ngày Lễ L là ngày nghỉ việc NV: 
                                            //Count O các ngày từ ngày L-5 đến ngày L , nếu O>2, bỏ L
                                            CountO = 0;
                                            if (i < LCongThangNgayTheoDong.Count - 1)
                                            {
                                                if (LCongThangNgayTheoDong[i + 1].Cong.Trim() == "NV")
                                                {
                                                    SoItemCanKtTruocL = i;
                                                    if (SoItemCanKtTruocL > 5)
                                                    {
                                                        SoItemCanKtTruocL = 5;
                                                    }
                                                    if (SoItemCanKtTruocL >= 0)
                                                    {
                                                        for (int k = i - 1; k >= i - SoItemCanKtTruocL; k--)
                                                        {
                                                            if (LCongThangNgayTheoDongCheckL[k].Cong.Trim() == "O")
                                                            {
                                                                CountO += 1;
                                                            }
                                                        }
                                                    }
                                                    if (CountO <= 2)
                                                    {
                                                        RemoveL = false;
                                                    }
                                                    else
                                                    {
                                                        RemoveL = true;
                                                    }
                                                    StopCheckL = true;
                                                }
                                            }
                                        }
                                        if (StopCheckL == false)
                                        {
                                            //Tìm ngày nghỉ việc NV: count O các ngày trong khoảng L và NV, 
                                            //nếu  = 0 thì giữ L, nếu > 0 thì bỏ L
                                            CountO = 0;
                                            for (int j = i + 1; j < LCongThangNgayTheoDongCheckL.Count; j++)
                                            {
                                                if (LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "NV")
                                                {
                                                    IsNV = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "O")
                                                    {
                                                        CountO += 1;
                                                    }
                                                }
                                            }
                                            if (IsNV == true)
                                            {
                                                if (CountO == 0)
                                                {
                                                    RemoveL = false;
                                                }
                                                else
                                                {
                                                    RemoveL = true;
                                                }
                                                StopCheckL = true;
                                            }
                                        }
                                        if (RemoveL == true)
                                        {
                                            LCongThangNgayTheoDong[i].Cong = "";
                                        }
                                    }

                                    // xữ lý chuyên cần
                                    if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].Cong) == true)
                                    {
                                        Cong.DLCong += Convert.ToDouble(LCongThangNgayTheoDong[i].Cong);
                                        if (ns.MaLoaiLaoDong == "KTTSX" && LCongThangNgayTheoDong[i].ngay.DayOfWeek != DayOfWeek.Sunday && double.Parse(LCongThangNgayTheoDong[i].Cong) < 8)
                                        {
                                            if (Cong.SoLanDiTreVeSo == null)
                                                Cong.SoLanDiTreVeSo = 0;
                                            Cong.SoLanDiTreVeSo = Cong.SoLanDiTreVeSo + 1;
                                        }
                                    }
                                    //Số ngày Công tác/Nghỉ bù = Count(CT) + Count(NB)
                                    if (LCongThangNgayTheoDong[i].Cong == "CT" || LCongThangNgayTheoDong[i].Cong == "NB")
                                    {
                                        Cong.SoNgayCT_NB += 1;
                                    }
                                    //Tăng ca CN = Tổng dòng Tăng ca các ô ngày chủ nhật
                                    if (LCongThangNgayTheoDong[i].ngay.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCa) == true)
                                        {
                                            Cong.TCCN += Convert.ToDouble(LCongThangNgayTheoDong[i].TangCa);
                                        }
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCaSau22H) == true)
                                        {
                                            Cong.TCCNSau22H += Convert.ToDouble(LCongThangNgayTheoDong[i].TangCaSau22H);
                                        }
                                    }
                                    else if (clsFunction.GetMaNghiPhepForCong1(ns, NgayChotCong, LCongThangNgayTheoDong[i].ngay, NghiPhepForCong, NgayLe) == "L")//Ngày lễ
                                    {
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCa) == true
                                            && clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCaSau22H) == false)
                                        {
                                            Cong.TCLe += Convert.ToDouble(LCongThangNgayTheoDong[i].TangCa);
                                        }
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCa) == true
                                            && clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCaSau22H) == true)
                                        {
                                            Cong.TCLe += Convert.ToDouble(LCongThangNgayTheoDong[i].TangCa)
                                                + Convert.ToDouble(LCongThangNgayTheoDong[i].TangCaSau22H);
                                        }
                                    }
                                    else
                                    {
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCa) == true)
                                        {
                                            Cong.TCThuong += Math.Round( Convert.ToDouble(LCongThangNgayTheoDong[i].TangCa),2);
                                        }
                                        if (clsFunction.CheckDouble(LCongThangNgayTheoDong[i].TangCaSau22H) == true)
                                        {
                                            Cong.TCSau22H += Convert.ToDouble(LCongThangNgayTheoDong[i].TangCaSau22H);
                                        }
                                    }
                                    //SỐ NGÀY LÀM ĐÊM = Count dòng Tăng ca sau 22h giá trị CD, 
                                    //nhân sự sửa vào cột Tăng ca sau 22h giá trị CD sẽ tự lấy qua báo cáo tổng
                                    if (LCongThangNgayTheoDong[i].TangCaSau22H == "CD")
                                    {
                                        Cong.SoNgayLamDem += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "O")
                                    {
                                        CountO += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "NV")
                                    {
                                        CountNV += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "B")
                                    {
                                        CountB += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "TN")
                                    {
                                        CountTN += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "Ro" || LCongThangNgayTheoDong[i].TangCa == "Ro" || LCongThangNgayTheoDong[i].TangCaSau22H == "Ro")
                                    {
                                        CountRo += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "Ro/2" || LCongThangNgayTheoDong[i].TangCa == "Ro/2" || LCongThangNgayTheoDong[i].TangCaSau22H == "Ro/2")
                                    {
                                        CountRo += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "M")
                                    {
                                        CountM += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "TS")
                                    {
                                        CountTS += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "P")
                                    {
                                        CountP += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "R")
                                    {
                                        CountR += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "L" || LCongThangNgayTheoDong[i].TangCa == "L")
                                    {
                                        CountL += 1;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "P/2" || LCongThangNgayTheoDong[i].TangCa == "P/2" || LCongThangNgayTheoDong[i].TangCaSau22H == "P/2")
                                    {
                                        CountP_2 += 0.5;
                                    }
                                    if (LCongThangNgayTheoDong[i].Cong == "CTR")
                                    {
                                        CountCTR += 1;
                                    }
                                }
                               
                                
                                #region add Cong
                                Cong.MSNV = ns.MaNV;
                                Cong.NhanSu = ns.NhanSu;
                                Cong.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                Cong.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                Cong.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                Cong.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                Cong.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                Cong.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                Cong.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                Cong.MaLoaiCong = "Cong";
                                var tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == Cong.MaLoaiCong).FirstOrDefault();
                                if (tlc != null)
                                {
                                    Cong.TenLoaiCong = tlc.TenLoaiCong;
                                }
                                Cong.LoaiCongOrder = 1;
                                if (MaCaLamViec == "Ca12_T1")
                                {
                                    Cong.DLCong = Math.Round(Cong.DLCong / 12, 2);
                                }
                                else
                                {
                                    Cong.DLCong = Math.Round(Cong.DLCong / 8, 2);
                                }
                                //TỔNG NGÀY CÔNG = = Dữ liệu công + Số ngày Công tác/Nghỉ bù
                                Cong.TongNC = Cong.DLCong + Cong.SoNgayCT_NB;

                                if (ns.MaLoaiLaoDong == "KTTSX")//Loại lao động = Trực tiếp SX mới tính chuyên cần
                                {
                                    if (CountM > 0)
                                    {
                                        if (CountNV == 0 && CountO == 0 && Cong.DLCong >= 13)
                                            Cong.ChuyenCan = (1000000 / 26) * Cong.DLCong;
                                        else
                                        {
                                            Cong.ChuyenCan = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (CountRo > 2 || CountO > 0 || CountNV > 0 || (CountB + CountTN + CountTS) > 5 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 2) || ((CountB + CountTN + CountTS) > 3 && CountRo > 0))
                                        {
                                            //Nếu tồn tại O hoặc NV (kiểm tra ngày nghỉ việc nhân sự cập nhật trong thông tin nhân viên 
                                            //-> nếu ngày xét >= ngày nghỉ việc thì set NV vào dòng Công) hoặc B > 5, TN > 5, Ro>2, CTR>2, TS>2 -> = 0
                                            Cong.ChuyenCan = 0;
                                        }
                                        else if (CountRo == 2 || (CountB + CountTN + CountTS) == 4 || (CountB + CountTN + CountTS) == 5 ||
                                            ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 1))
                                        {
                                            //Nếu Count(Ro) + Count(TS) + Count(M) = 2 hoặc Count(B)=5 -> 300.000
                                            Cong.ChuyenCan = 600000;
                                        }
                                        else if (CountRo == 1 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4))
                                        {
                                            //Nếu Count(Ro) + Count(TS) + Count(M) = 1 hoặc Count(B)=3 -> 400.000
                                            Cong.ChuyenCan = 800000;
                                        }
                                        else if (CountRo + CountB + CountTS + CountTN + CountO + CountNV + CountM == 0)
                                        {
                                            //Nếu Không có Ro và không có M và không có TS -> = 500.000
                                            Cong.ChuyenCan = 1000000;
                                        }
                                        else
                                        {
                                            //Ngược lại = 0
                                            Cong.ChuyenCan = 0;
                                        }
                                    }

                                }
                                //Nghỉ Không phép = Count(O) dòng Công;
                                Cong.NghiKhongPhep = CountO;
                                Cong.PhepNam_Le_TNLD = CountP + CountR + CountL + CountP_2 + CountTN;
                                //objCongThang.XNC5LThang = clsFunction.CountXacNhanCong(ns, NgayBatDau, NgayChotCong);
                                Cong.XNC5LThang = XacNhanCong.Where(o => o.NhanSu == ns.NhanSu).ToList().Count;

                                var temp = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).Select(it => it.Ngay).Distinct().ToList();
                                Cong.SoNgayDiCongTac = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).ToList().Select(it => it.Ngay).Distinct().Count();

                                var cthc = CongThangHC.Where(o => o.MaNV == ns.MaNV).FirstOrDefault();
                                if (cthc != null)
                                {
                                    Cong.TongNCHC = Convert.ToDouble(cthc.TongNC);
                                }
                               
                                #endregion
                                #region Add Tăng ca
                                TC.NhanSu = 0;
                                TC.MSNV = ns.MaNV;
                                TC.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                TC.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                TC.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                TC.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                TC.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                TC.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                TC.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                TC.MaLoaiCong = "TC";
                                tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == TC.MaLoaiCong).FirstOrDefault();
                                if (tlc != null)
                                {
                                    TC.TenLoaiCong = tlc.TenLoaiCong;
                                }
                                TC.LoaiCongOrder = 2;
                                #endregion
                                #region Add Tang ca sau 22h
                                TCS22H = new CongThang();
                                TCS22H.MSNV = ns.MaNV;
                                TCS22H.NhanSu = 0;
                                TCS22H.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                TCS22H.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                TCS22H.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                TCS22H.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                TCS22H.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                TCS22H.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                TCS22H.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                TCS22H.MaLoaiCong = "Sau22H";
                                tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == TCS22H.MaLoaiCong).FirstOrDefault();
                                if (tlc != null)
                                {
                                    TCS22H.TenLoaiCong = tlc.TenLoaiCong;
                                }
                                TCS22H.LoaiCongOrder = 3;
                                #endregion
                                Cot = 1;
                                ngay = NgayBatDau;
                                while (ngay < NgayChotCong)
                                {
                                    switch (Cot)
                                    {
                                        case 1:
                                            Cong.Cot1 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot1 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 2:
                                            Cong.Cot2 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 3:
                                            Cong.Cot3 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 4:
                                            Cong.Cot4 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 5:
                                            Cong.Cot5 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 6:
                                            Cong.Cot6 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 7:
                                            Cong.Cot7 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 8:
                                            Cong.Cot8 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 9:
                                            Cong.Cot9 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 10:
                                            Cong.Cot10 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 11:
                                            Cong.Cot11 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 12:
                                            Cong.Cot12 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 13:
                                            Cong.Cot13 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 14:
                                            Cong.Cot14 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 15:
                                            Cong.Cot15 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 16:
                                            Cong.Cot16 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 17:
                                            Cong.Cot17 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 18:
                                            Cong.Cot18 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot18 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 19:
                                            Cong.Cot19 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 20:
                                            Cong.Cot20 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 21:
                                            Cong.Cot21 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 22:
                                            Cong.Cot22 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 23:
                                            Cong.Cot23 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 24:
                                            Cong.Cot24 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 25:
                                            Cong.Cot25 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 26:
                                            Cong.Cot26 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 27:
                                            Cong.Cot27 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 28:
                                            Cong.Cot28 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 29:
                                            Cong.Cot29 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 30:
                                            Cong.Cot30 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                        case 31:
                                            Cong.Cot31 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                            TC.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                            TCS22H.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                            break;
                                    }
                                    ngay = ngay.AddDays(1);
                                    Cot += 1;
                                }
                                Cong.STT = STT;
                                CongThang.Add(Cong);
                                STT++;
                                TC.STT = STT;
                                CongThang.Add(TC);
                                STT++;
                                CongThang.Add(TCS22H);
                                TCS22H.STT = STT;
                                STT++;
                                #region cod cu ad cong thang
                                //Loai = 1;
                                //while (Loai <= 3)
                                //{
                                //    switch (Loai)
                                //    {
                                //        case 1:
                                //            objCongThang = new CongThang();
                                //            objCongThang.MSNV = ns.MaNV;
                                //            objCongThang.NhanSu = ns.NhanSu;
                                //            objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                //            objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                //            objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                //            objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                //            objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                //            objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                //            objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                //            objCongThang.MaLoaiCong = "Cong";
                                //            //if (ns.MaLoaiLaoDong == "KTTSX")
                                //            //{
                                //            //    var vaotre = LCongThangNgayTheoDong.Where(it => it.VaoTreVeSom == true).ToList();
                                //            //    objCongThang.SoLanDiTreVeSo = vaotre.Count();
                                //            //}

                                //            var tlc1 = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                                //            if (tlc1 != null)
                                //            {
                                //                objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                                //            }
                                //            objCongThang.LoaiCongOrder = Loai;
                                //            objCongThang.ListCong = new List<CongThangTheoNgayModel>();
                                //            Cot = 1;
                                //            ngay = NgayBatDau;
                                //            while (ngay < NgayChotCong)
                                //            {
                                //                switch (Cot)
                                //                {
                                //                    case 1:
                                //                        objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 2:
                                //                        objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 3:
                                //                        objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 4:
                                //                        objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 5:
                                //                        objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 6:
                                //                        objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 7:
                                //                        objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 8:
                                //                        objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 9:
                                //                        objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 10:
                                //                        objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 11:
                                //                        objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 12:
                                //                        objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 13:
                                //                        objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 14:
                                //                        objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 15:
                                //                        objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 16:
                                //                        objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 17:
                                //                        objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 18:
                                //                        objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 19:
                                //                        objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 20:
                                //                        objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 21:
                                //                        objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 22:
                                //                        objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 23:
                                //                        objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 24:
                                //                        objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 25:
                                //                        objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 26:
                                //                        objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 27:
                                //                        objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 28:
                                //                        objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 29:
                                //                        objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 30:
                                //                        objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                    case 31:
                                //                        objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                                //                        break;
                                //                }
                                //                ngay = ngay.AddDays(1);
                                //                Cot += 1;
                                //            }
                                //            CountO = 0;
                                //            CountNV = 0;
                                //            CountB = 0;
                                //            CountTN = 0;
                                //            CountRo = 0;
                                //            CountM = 0;
                                //            CountTS = 0;
                                //            CountP = 0;
                                //            CountR = 0;
                                //            CountL = 0;
                                //            CountP_2 = 0;
                                //            CountCTR = 0;
                                //            foreach (var ctntd in LCongThangNgayTheoDong)
                                //            {
                                //                if (clsFunction.CheckDouble(ctntd.Cong) == true)
                                //                {
                                //                    objCongThang.DLCong += Convert.ToDouble(ctntd.Cong);
                                //                    if (ns.MaLoaiLaoDong == "KTTSX" && ctntd.ngay.DayOfWeek != DayOfWeek.Sunday && double.Parse(ctntd.Cong) < 8)
                                //                    {
                                //                        if (objCongThang.SoLanDiTreVeSo == null)
                                //                            objCongThang.SoLanDiTreVeSo = 0;
                                //                        objCongThang.SoLanDiTreVeSo = objCongThang.SoLanDiTreVeSo + 1;
                                //                    }
                                //                }
                                //                //Số ngày Công tác/Nghỉ bù = Count(CT) + Count(NB)
                                //                if (ctntd.Cong == "CT" || ctntd.Cong == "NB")
                                //                {
                                //                    objCongThang.SoNgayCT_NB += 1;
                                //                }
                                //                //Tăng ca CN = Tổng dòng Tăng ca các ô ngày chủ nhật
                                //                if (ctntd.ngay.DayOfWeek == DayOfWeek.Sunday)
                                //                {
                                //                    if (clsFunction.CheckDouble(ctntd.TangCa) == true)
                                //                    {
                                //                        objCongThang.TCCN += Convert.ToDouble(ctntd.TangCa);
                                //                    }
                                //                    if (clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                                //                    {
                                //                        objCongThang.TCCNSau22H += Convert.ToDouble(ctntd.TangCaSau22H);
                                //                    }
                                //                }
                                //                else if (clsFunction.GetMaNghiPhepForCong1(ns, NgayChotCong, ctntd.ngay, NghiPhepForCong, NgayLe) == "L")//Ngày lễ
                                //                {
                                //                    if (clsFunction.CheckDouble(ctntd.TangCa) == true
                                //                        && clsFunction.CheckDouble(ctntd.TangCaSau22H) == false)
                                //                    {
                                //                        objCongThang.TCLe += Convert.ToDouble(ctntd.TangCa);
                                //                    }
                                //                    if (clsFunction.CheckDouble(ctntd.TangCa) == true
                                //                        && clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                                //                    {
                                //                        objCongThang.TCLe += Convert.ToDouble(ctntd.TangCa)
                                //                            + Convert.ToDouble(ctntd.TangCaSau22H);
                                //                    }
                                //                }
                                //                else
                                //                {
                                //                    if (clsFunction.CheckDouble(ctntd.TangCa) == true)
                                //                    {
                                //                        objCongThang.TCThuong += Convert.ToDouble(ctntd.TangCa);
                                //                    }
                                //                    if (clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                                //                    {
                                //                        objCongThang.TCSau22H += Convert.ToDouble(ctntd.TangCaSau22H);
                                //                    }
                                //                }
                                //                //SỐ NGÀY LÀM ĐÊM = Count dòng Tăng ca sau 22h giá trị CD, 
                                //                //nhân sự sửa vào cột Tăng ca sau 22h giá trị CD sẽ tự lấy qua báo cáo tổng
                                //                if (ctntd.TangCaSau22H == "CD")
                                //                {
                                //                    objCongThang.SoNgayLamDem += 1;
                                //                }
                                //                if (ctntd.Cong == "O")
                                //                {
                                //                    CountO += 1;
                                //                }
                                //                if (ctntd.Cong == "NV")
                                //                {
                                //                    CountNV += 1;
                                //                }
                                //                if (ctntd.Cong == "B")
                                //                {
                                //                    CountB += 1;
                                //                }
                                //                if (ctntd.Cong == "TN")
                                //                {
                                //                    CountTN += 1;
                                //                }
                                //                if (ctntd.Cong == "Ro" || ctntd.TangCa == "Ro" || ctntd.TangCaSau22H == "Ro")
                                //                {
                                //                    CountRo += 1;
                                //                }
                                //                if (ctntd.Cong == "Ro/2" || ctntd.TangCa == "Ro/2" || ctntd.TangCaSau22H == "Ro/2")
                                //                {
                                //                    CountRo += 1;
                                //                }
                                //                if (ctntd.Cong == "M")
                                //                {
                                //                    CountM += 1;
                                //                }
                                //                if (ctntd.Cong == "TS")
                                //                {
                                //                    CountTS += 1;
                                //                }
                                //                if (ctntd.Cong == "P")
                                //                {
                                //                    CountP += 1;
                                //                }
                                //                if (ctntd.Cong == "R")
                                //                {
                                //                    CountR += 1;
                                //                }
                                //                if (ctntd.Cong == "L" || ctntd.TangCa == "L")
                                //                {
                                //                    CountL += 1;
                                //                }
                                //                if (ctntd.Cong == "P/2" || ctntd.TangCa == "P/2" || ctntd.TangCaSau22H == "P/2")
                                //                {
                                //                    CountP_2 += 0.5;
                                //                }
                                //                if (ctntd.Cong == "CTR")
                                //                {
                                //                    CountCTR += 1;
                                //                }
                                //            }
                                //            //Dữ liệu công = Sum các ngày = số / 8
                                //            if (MaCaLamViec == "Ca12_T1")
                                //            {
                                //                objCongThang.DLCong = Math.Round(objCongThang.DLCong / 12, 2);
                                //            }
                                //            else
                                //            {
                                //                objCongThang.DLCong = Math.Round(objCongThang.DLCong / 8, 2);
                                //            }
                                //            //TỔNG NGÀY CÔNG = = Dữ liệu công + Số ngày Công tác/Nghỉ bù
                                //            objCongThang.TongNC = objCongThang.DLCong + objCongThang.SoNgayCT_NB;

                                //            if (ns.MaLoaiLaoDong == "KTTSX")//Loại lao động = Trực tiếp SX mới tính chuyên cần
                                //            {

                                //                #region Code cu
                                //                //Kiểm tra 3 dòng Công, Tăng ca, Tăng ca sau 22h                                
                                //                //if (CountO > 0 || CountNV > 0 || CountB > 5 || CountTN > 5 || CountRo > 2 || CountCTR > 2 || CountTS > 2)
                                //                //{
                                //                //    //Nếu tồn tại O hoặc NV (kiểm tra ngày nghỉ việc nhân sự cập nhật trong thông tin nhân viên 
                                //                //    //-> nếu ngày xét >= ngày nghỉ việc thì set NV vào dòng Công) hoặc B > 5, TN > 5, Ro>2, CTR>2, TS>2 -> = 0
                                //                //    objCongThang.ChuyenCan = 0;
                                //                //}
                                //                //else if ((CountRo + CountM + CountTS) > 2)
                                //                //{
                                //                //    objCongThang.ChuyenCan = 0;
                                //                //}
                                //                //else if ((CountRo + CountM + CountTS) == 2 || CountB == 5)
                                //                //{
                                //                //    //Nếu Count(Ro) + Count(TS) + Count(M) = 2 hoặc Count(B)=5 -> 300.000
                                //                //    objCongThang.ChuyenCan = 300000;
                                //                //}
                                //                //else if ((CountRo + CountM + CountTS) == 1 || CountB == 3 || CountB == 4)
                                //                //{
                                //                //    //Nếu Count(Ro) + Count(TS) + Count(M) = 1 hoặc Count(B)=3 -> 400.000
                                //                //    objCongThang.ChuyenCan = 400000;
                                //                //}
                                //                //else if (CountRo == 0 && CountM == 0 && CountTS == 0)
                                //                //{
                                //                //    //Nếu Không có Ro và không có M và không có TS -> = 500.000
                                //                //    objCongThang.ChuyenCan = 500000;
                                //                //}
                                //                //else
                                //                //{
                                //                //    //Ngược lại = 0
                                //                //    objCongThang.ChuyenCan = 0;
                                //                //}
                                //                #endregion

                                //                if (CountM > 0)
                                //                {
                                //                    if (CountNV == 0 && CountO == 0 && objCongThang.DLCong >= 13)
                                //                        objCongThang.ChuyenCan = (1000000 / 26) * objCongThang.DLCong;
                                //                    else
                                //                    {
                                //                        objCongThang.ChuyenCan = 0;
                                //                    }
                                //                }
                                //                else
                                //                {
                                //                    if (CountRo > 2 || CountO > 0 || CountNV > 0 || (CountB + CountTN + CountTS) > 5 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 2) || ((CountB + CountTN + CountTS) > 3 && CountRo > 0))
                                //                    {
                                //                        //Nếu tồn tại O hoặc NV (kiểm tra ngày nghỉ việc nhân sự cập nhật trong thông tin nhân viên 
                                //                        //-> nếu ngày xét >= ngày nghỉ việc thì set NV vào dòng Công) hoặc B > 5, TN > 5, Ro>2, CTR>2, TS>2 -> = 0
                                //                        objCongThang.ChuyenCan = 0;
                                //                    }
                                //                    else if (CountRo == 2 || (CountB + CountTN + CountTS) == 4 || (CountB + CountTN + CountTS) == 5 ||
                                //                        ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 1))
                                //                    {
                                //                        //Nếu Count(Ro) + Count(TS) + Count(M) = 2 hoặc Count(B)=5 -> 300.000
                                //                        objCongThang.ChuyenCan = 600000;
                                //                    }
                                //                    else if (CountRo == 1 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4))
                                //                    {
                                //                        //Nếu Count(Ro) + Count(TS) + Count(M) = 1 hoặc Count(B)=3 -> 400.000
                                //                        objCongThang.ChuyenCan = 800000;
                                //                    }
                                //                    else if (CountRo + CountB + CountTS + CountTN + CountO + CountNV + CountM == 0)
                                //                    {
                                //                        //Nếu Không có Ro và không có M và không có TS -> = 500.000
                                //                        objCongThang.ChuyenCan = 1000000;
                                //                    }
                                //                    else
                                //                    {
                                //                        //Ngược lại = 0
                                //                        objCongThang.ChuyenCan = 0;
                                //                    }
                                //                }

                                //            }
                                //            //Nghỉ Không phép = Count(O) dòng Công;
                                //            objCongThang.NghiKhongPhep = CountO;
                                //            objCongThang.PhepNam_Le_TNLD = CountP + CountR + CountL + CountP_2 + CountTN;
                                //            //objCongThang.XNC5LThang = clsFunction.CountXacNhanCong(ns, NgayBatDau, NgayChotCong);
                                //            objCongThang.XNC5LThang = XacNhanCong.Where(o => o.NhanSu == ns.NhanSu).ToList().Count;

                                //            var temp = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).Select(it => it.Ngay).Distinct().ToList();
                                //            objCongThang.SoNgayDiCongTac = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).ToList().Select(it => it.Ngay).Distinct().Count();

                                //            var cthc = CongThangHC.Where(o => o.MaNV == ns.MaNV).FirstOrDefault();
                                //            if (cthc != null)
                                //            {
                                //                objCongThang.TongNCHC = Convert.ToDouble(cthc.TongNC);
                                //            }

                                //            break;
                                //        case 2:
                                //            objCongThang = new CongThang();
                                //            objCongThang.NhanSu = 0;
                                //            objCongThang.MSNV = ns.MaNV;
                                //            objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                //            objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                //            objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                //            objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                //            objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                //            objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                //            objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                //            objCongThang.MaLoaiCong = "TC";
                                //            tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                                //            if (tlc != null)
                                //            {
                                //                objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                                //            }
                                //            objCongThang.LoaiCongOrder = Loai;
                                //            Cot = 1;
                                //            ngay = NgayBatDau;
                                //            while (ngay < NgayChotCong)
                                //            {
                                //                switch (Cot)
                                //                {
                                //                    case 1:
                                //                        objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 2:
                                //                        objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 3:
                                //                        objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 4:
                                //                        objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 5:
                                //                        objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 6:
                                //                        objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 7:
                                //                        objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 8:
                                //                        objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 9:
                                //                        objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 10:
                                //                        objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 11:
                                //                        objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 12:
                                //                        objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 13:
                                //                        objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 14:
                                //                        objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 15:
                                //                        objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 16:
                                //                        objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 17:
                                //                        objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 18:
                                //                        objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 19:
                                //                        objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 20:
                                //                        objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 21:
                                //                        objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 22:
                                //                        objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 23:
                                //                        objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 24:
                                //                        objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 25:
                                //                        objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 26:
                                //                        objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 27:
                                //                        objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 28:
                                //                        objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 29:
                                //                        objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 30:
                                //                        objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                    case 31:
                                //                        objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                                //                        break;
                                //                }
                                //                ngay = ngay.AddDays(1);
                                //                Cot += 1;
                                //            }

                                //            break;
                                //        case 3:
                                //            objCongThang = new CongThang();
                                //            objCongThang.MSNV = ns.MaNV;
                                //            objCongThang.NhanSu = 0;
                                //            objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                                //            objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                                //            objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                                //            objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                                //            objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                                //            objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                                //            objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                                //            objCongThang.MaLoaiCong = "Sau22H";
                                //            tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                                //            if (tlc != null)
                                //            {
                                //                objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                                //            }
                                //            objCongThang.LoaiCongOrder = Loai;
                                //            Cot = 1;
                                //            ngay = NgayBatDau;
                                //            while (ngay < NgayChotCong)
                                //            {
                                //                switch (Cot)
                                //                {
                                //                    case 1:
                                //                        objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 2:
                                //                        objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 3:
                                //                        objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 4:
                                //                        objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 5:
                                //                        objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 6:
                                //                        objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 7:
                                //                        objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 8:
                                //                        objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 9:
                                //                        objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 10:
                                //                        objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 11:
                                //                        objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 12:
                                //                        objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 13:
                                //                        objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 14:
                                //                        objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 15:
                                //                        objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 16:
                                //                        objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 17:
                                //                        objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 18:
                                //                        objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 19:
                                //                        objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 20:
                                //                        objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 21:
                                //                        objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 22:
                                //                        objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 23:
                                //                        objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 24:
                                //                        objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 25:
                                //                        objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 26:
                                //                        objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 27:
                                //                        objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 28:
                                //                        objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 29:
                                //                        objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 30:
                                //                        objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                    case 31:
                                //                        objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                                //                        break;
                                //                }
                                //                ngay = ngay.AddDays(1);
                                //                Cot += 1;
                                //            }
                                //            break;
                                //    }
                                //    objCongThang.STT = STT;
                                //    CongThang.Add(objCongThang);
                                //    //  model.ListCongThang.Add(objCongThang);
                                //    Loai += 1;
                                //    STT += 1;
                                //}
                                #endregion

                            }
                            catch (Exception ex)
                            {
                                rs.code = 0;
                                rs.text = "Lỗi xử lý MSNV: " + ns.MaNV + ", " + ex.Message;
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            rs.code = 0;
                            rs.text = "Lỗi nhân viên " + ns.MaNV + " Ngày công" + ngay.ToString("dd/MM/yyyy") + ex.ToString();
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }

                    }
                    #region code cu
                    //try
                    //{
                    //    ngay = NgayBatDau;
                    //    Cot = 1;
                    //    while (ngay < NgayChotCong)
                    //    {
                    //        switch (Cot)
                    //        {
                    //            case 1:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot1 = objCot;
                    //                break;
                    //            case 2:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot2 = objCot;
                    //                break;
                    //            case 3:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot3 = objCot;
                    //                break;
                    //            case 4:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot4 = objCot;
                    //                break;
                    //            case 5:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot5 = objCot;
                    //                break;
                    //            case 6:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot6 = objCot;
                    //                break;
                    //            case 7:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot7 = objCot;
                    //                break;
                    //            case 8:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot8 = objCot;
                    //                break;
                    //            case 9:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot9 = objCot;
                    //                break;
                    //            case 10:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot10 = objCot;
                    //                break;
                    //            case 11:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot11 = objCot;
                    //                break;
                    //            case 12:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot12 = objCot;
                    //                break;
                    //            case 13:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot13 = objCot;
                    //                break;
                    //            case 14:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot14 = objCot;
                    //                break;
                    //            case 15:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot15 = objCot;
                    //                break;
                    //            case 16:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot16 = objCot;
                    //                break;
                    //            case 17:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot17 = objCot;
                    //                break;
                    //            case 18:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot18 = objCot;
                    //                break;
                    //            case 19:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot19 = objCot;
                    //                break;
                    //            case 20:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot20 = objCot;
                    //                break;
                    //            case 21:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot21 = objCot;
                    //                break;
                    //            case 22:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot22 = objCot;
                    //                break;
                    //            case 23:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot23 = objCot;
                    //                break;
                    //            case 24:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot24 = objCot;
                    //                break;
                    //            case 25:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot25 = objCot;
                    //                break;
                    //            case 26:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot26 = objCot;
                    //                break;
                    //            case 27:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot27 = objCot;
                    //                break;
                    //            case 28:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot28 = objCot;
                    //                break;
                    //            case 29:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot29 = objCot;
                    //                break;
                    //            case 30:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot30 = objCot;
                    //                break;
                    //            case 31:
                    //                objCot = new Cot();
                    //                objCot.HeaderText = ngay.ToString("dd/MM");
                    //                objCot.ngay = ngay;
                    //                model.Cot31 = objCot;
                    //                break;
                    //        }
                    //        ngay = ngay.AddDays(1);
                    //        Cot += 1;
                    //    }

                    //    STT = 1;
                    //    model.ListCongThang = new List<CongThang>();
                    //    foreach (var ns in ListNhanSu)
                    //    {
                    //        try
                    //        {
                    //            //MaCaLamViec = ns.MaCaLamViec.Trim();
                    //            var LCongThangNgayTheoDong = ListCongThangNgayTheoDong.Where(o => o.MSNV == ns.MaNV).OrderBy(o => o.ngay).ToList();

                    //            //sau Lễ mà O/NV (ko có công) thì L bỏ
                    //            var LCongThangNgayTheoDongCheckL = LCongThangNgayTheoDong;
                    //            bool StopCheckL = false;
                    //            CountO = 0;
                    //            bool IsNV = false;
                    //            int SoItemCanKtTruocL = 0;

                    //            for (int i = 0; i < LCongThangNgayTheoDong.Count; i++)
                    //            {
                    //                if (LCongThangNgayTheoDong[i].Cong.Trim() == "L")
                    //                {
                    //                    RemoveL = true;
                    //                    //Các ngày sau L có ngày nào có công (có số / P / P/2 / TN) thì tính L
                    //                    for (int j = i + 1; j < LCongThangNgayTheoDongCheckL.Count; j++)
                    //                    {
                    //                        if (clsFunction.CheckDouble(LCongThangNgayTheoDongCheckL[j].Cong.Trim()) == true
                    //                            || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "P"
                    //                            || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "P/2"
                    //                            || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "TN"
                    //                            || LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "L"
                    //                            || ns.MaTinhTrang == "1"
                    //                            )
                    //                        {
                    //                            RemoveL = false;
                    //                            StopCheckL = true;
                    //                            break;
                    //                        }
                    //                    }
                    //                    if (i == LCongThangNgayTheoDong.Count - 1 && ns.MaTinhTrang == "1" && ns.NgayNghiViec1 > LCongThangNgayTheoDong[i].ngay)
                    //                    {
                    //                        RemoveL = false;
                    //                        StopCheckL = true;
                    //                    }
                    //                    if (StopCheckL == false)
                    //                    {
                    //                        //Nếu ngày kế tiếp ngày Lễ L là ngày nghỉ việc NV: 
                    //                        //Count O các ngày từ ngày L-5 đến ngày L , nếu O>2, bỏ L
                    //                        CountO = 0;
                    //                        if (i < LCongThangNgayTheoDong.Count - 1)
                    //                        {
                    //                            if (LCongThangNgayTheoDong[i + 1].Cong.Trim() == "NV")
                    //                            {
                    //                                SoItemCanKtTruocL = i;
                    //                                if (SoItemCanKtTruocL > 5)
                    //                                {
                    //                                    SoItemCanKtTruocL = 5;
                    //                                }
                    //                                if (SoItemCanKtTruocL >= 0)
                    //                                {
                    //                                    for (int k = i - 1; k >= i - SoItemCanKtTruocL; k--)
                    //                                    {
                    //                                        if (LCongThangNgayTheoDongCheckL[k].Cong.Trim() == "O")
                    //                                        {
                    //                                            CountO += 1;
                    //                                        }
                    //                                    }
                    //                                }
                    //                                if (CountO <= 2)
                    //                                {
                    //                                    RemoveL = false;
                    //                                }
                    //                                else
                    //                                {
                    //                                    RemoveL = true;
                    //                                }
                    //                                StopCheckL = true;
                    //                            }
                    //                        }
                    //                    }
                    //                    if (StopCheckL == false)
                    //                    {
                    //                        //Tìm ngày nghỉ việc NV: count O các ngày trong khoảng L và NV, 
                    //                        //nếu  = 0 thì giữ L, nếu > 0 thì bỏ L
                    //                        CountO = 0;
                    //                        for (int j = i + 1; j < LCongThangNgayTheoDongCheckL.Count; j++)
                    //                        {
                    //                            if (LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "NV")
                    //                            {
                    //                                IsNV = true;
                    //                                break;
                    //                            }
                    //                            else
                    //                            {
                    //                                if (LCongThangNgayTheoDongCheckL[j].Cong.Trim() == "O")
                    //                                {
                    //                                    CountO += 1;
                    //                                }
                    //                            }
                    //                        }
                    //                        if (IsNV == true)
                    //                        {
                    //                            if (CountO == 0)
                    //                            {
                    //                                RemoveL = false;
                    //                            }
                    //                            else
                    //                            {
                    //                                RemoveL = true;
                    //                            }
                    //                            StopCheckL = true;
                    //                        }
                    //                    }
                    //                    if (RemoveL == true)
                    //                    {
                    //                        LCongThangNgayTheoDong[i].Cong = "";
                    //                    }
                    //                }
                    //            }
                    //            Loai = 1;
                    //            while (Loai <= 3)
                    //            {
                    //                switch (Loai)
                    //                {
                    //                    case 1:
                    //                        objCongThang = new CongThang();
                    //                        objCongThang.MSNV = ns.MaNV;
                    //                        objCongThang.NhanSu = ns.NhanSu;
                    //                        objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                    //                        objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                    //                        objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                    //                        objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                    //                        objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                    //                        objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                    //                        objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                    //                        objCongThang.MaLoaiCong = "Cong";
                    //                        //if (ns.MaLoaiLaoDong == "KTTSX")
                    //                        //{
                    //                        //    var vaotre = LCongThangNgayTheoDong.Where(it => it.VaoTreVeSom == true).ToList();
                    //                        //    objCongThang.SoLanDiTreVeSo = vaotre.Count();
                    //                        //}

                    //                        var tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                    //                        if (tlc != null)
                    //                        {
                    //                            objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                    //                        }
                    //                        objCongThang.LoaiCongOrder = Loai;
                    //                        objCongThang.ListCong = new List<CongThangTheoNgayModel>();
                    //                        Cot = 1;
                    //                        ngay = NgayBatDau;
                    //                        while (ngay < NgayChotCong)
                    //                        {
                    //                            switch (Cot)
                    //                            {
                    //                                case 1:
                    //                                    objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 2:
                    //                                    objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 3:
                    //                                    objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 4:
                    //                                    objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 5:
                    //                                    objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 6:
                    //                                    objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 7:
                    //                                    objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 8:
                    //                                    objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 9:
                    //                                    objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 10:
                    //                                    objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 11:
                    //                                    objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 12:
                    //                                    objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 13:
                    //                                    objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 14:
                    //                                    objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 15:
                    //                                    objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 16:
                    //                                    objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 17:
                    //                                    objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 18:
                    //                                    objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 19:
                    //                                    objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 20:
                    //                                    objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 21:
                    //                                    objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 22:
                    //                                    objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 23:
                    //                                    objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 24:
                    //                                    objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 25:
                    //                                    objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 26:
                    //                                    objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 27:
                    //                                    objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 28:
                    //                                    objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 29:
                    //                                    objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 30:
                    //                                    objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                                case 31:
                    //                                    objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].Cong.ToString();
                    //                                    break;
                    //                            }
                    //                            ngay = ngay.AddDays(1);
                    //                            Cot += 1;
                    //                        }
                    //                        CountO = 0;
                    //                        CountNV = 0;
                    //                        CountB = 0;
                    //                        CountTN = 0;
                    //                        CountRo = 0;
                    //                        CountM = 0;
                    //                        CountTS = 0;
                    //                        CountP = 0;
                    //                        CountR = 0;
                    //                        CountL = 0;
                    //                        CountP_2 = 0;
                    //                        CountCTR = 0;
                    //                        foreach (var ctntd in LCongThangNgayTheoDong)
                    //                        {
                    //                            if (clsFunction.CheckDouble(ctntd.Cong) == true)
                    //                            {
                    //                                objCongThang.DLCong += Convert.ToDouble(ctntd.Cong);
                    //                                if (ns.MaLoaiLaoDong == "KTTSX" && ctntd.ngay.DayOfWeek != DayOfWeek.Sunday && double.Parse(ctntd.Cong) < 8)
                    //                                {
                    //                                    if (objCongThang.SoLanDiTreVeSo == null)
                    //                                        objCongThang.SoLanDiTreVeSo = 0;
                    //                                    objCongThang.SoLanDiTreVeSo = objCongThang.SoLanDiTreVeSo + 1;
                    //                                }
                    //                            }
                    //                            //Số ngày Công tác/Nghỉ bù = Count(CT) + Count(NB)
                    //                            if (ctntd.Cong == "CT" || ctntd.Cong == "NB")
                    //                            {
                    //                                objCongThang.SoNgayCT_NB += 1;
                    //                            }
                    //                            //Tăng ca CN = Tổng dòng Tăng ca các ô ngày chủ nhật
                    //                            if (ctntd.ngay.DayOfWeek == DayOfWeek.Sunday)
                    //                            {
                    //                                if (clsFunction.CheckDouble(ctntd.TangCa) == true)
                    //                                {
                    //                                    objCongThang.TCCN += Convert.ToDouble(ctntd.TangCa);
                    //                                }
                    //                                if (clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                    //                                {
                    //                                    objCongThang.TCCNSau22H += Convert.ToDouble(ctntd.TangCaSau22H);
                    //                                }
                    //                            }
                    //                            else if (clsFunction.GetMaNghiPhepForCong1(ns, NgayChotCong, ctntd.ngay, NghiPhepForCong, NgayLe) == "L")//Ngày lễ
                    //                            {
                    //                                if (clsFunction.CheckDouble(ctntd.TangCa) == true
                    //                                    && clsFunction.CheckDouble(ctntd.TangCaSau22H) == false)
                    //                                {
                    //                                    objCongThang.TCLe += Convert.ToDouble(ctntd.TangCa);
                    //                                }
                    //                                if (clsFunction.CheckDouble(ctntd.TangCa) == true
                    //                                    && clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                    //                                {
                    //                                    objCongThang.TCLe += Convert.ToDouble(ctntd.TangCa)
                    //                                        + Convert.ToDouble(ctntd.TangCaSau22H);
                    //                                }
                    //                            }
                    //                            else
                    //                            {
                    //                                if (clsFunction.CheckDouble(ctntd.TangCa) == true)
                    //                                {
                    //                                    objCongThang.TCThuong += Convert.ToDouble(ctntd.TangCa);
                    //                                }
                    //                                if (clsFunction.CheckDouble(ctntd.TangCaSau22H) == true)
                    //                                {
                    //                                    objCongThang.TCSau22H += Convert.ToDouble(ctntd.TangCaSau22H);
                    //                                }
                    //                            }
                    //                            //SỐ NGÀY LÀM ĐÊM = Count dòng Tăng ca sau 22h giá trị CD, 
                    //                            //nhân sự sửa vào cột Tăng ca sau 22h giá trị CD sẽ tự lấy qua báo cáo tổng
                    //                            if (ctntd.TangCaSau22H == "CD")
                    //                            {
                    //                                objCongThang.SoNgayLamDem += 1;
                    //                            }
                    //                            if (ctntd.Cong == "O")
                    //                            {
                    //                                CountO += 1;
                    //                            }
                    //                            if (ctntd.Cong == "NV")
                    //                            {
                    //                                CountNV += 1;
                    //                            }
                    //                            if (ctntd.Cong == "B")
                    //                            {
                    //                                CountB += 1;
                    //                            }
                    //                            if (ctntd.Cong == "TN")
                    //                            {
                    //                                CountTN += 1;
                    //                            }
                    //                            if (ctntd.Cong == "Ro" || ctntd.TangCa == "Ro" || ctntd.TangCaSau22H == "Ro")
                    //                            {
                    //                                CountRo += 1;
                    //                            }
                    //                            if (ctntd.Cong == "Ro/2" || ctntd.TangCa == "Ro/2" || ctntd.TangCaSau22H == "Ro/2")
                    //                            {
                    //                                CountRo += 1;
                    //                            }
                    //                            if (ctntd.Cong == "M")
                    //                            {
                    //                                CountM += 1;
                    //                            }
                    //                            if (ctntd.Cong == "TS")
                    //                            {
                    //                                CountTS += 1;
                    //                            }
                    //                            if (ctntd.Cong == "P")
                    //                            {
                    //                                CountP += 1;
                    //                            }
                    //                            if (ctntd.Cong == "R")
                    //                            {
                    //                                CountR += 1;
                    //                            }
                    //                            if (ctntd.Cong == "L" || ctntd.TangCa == "L")
                    //                            {
                    //                                CountL += 1;
                    //                            }
                    //                            if (ctntd.Cong == "P/2" || ctntd.TangCa == "P/2" || ctntd.TangCaSau22H == "P/2")
                    //                            {
                    //                                CountP_2 += 0.5;
                    //                            }
                    //                            if (ctntd.Cong == "CTR")
                    //                            {
                    //                                CountCTR += 1;
                    //                            }
                    //                        }
                    //                        //Dữ liệu công = Sum các ngày = số / 8
                    //                        if (MaCaLamViec == "Ca12_T1")
                    //                        {
                    //                            objCongThang.DLCong = Math.Round(objCongThang.DLCong / 12, 2);
                    //                        }
                    //                        else
                    //                        {
                    //                            objCongThang.DLCong = Math.Round(objCongThang.DLCong / 8, 2);
                    //                        }
                    //                        //TỔNG NGÀY CÔNG = = Dữ liệu công + Số ngày Công tác/Nghỉ bù
                    //                        objCongThang.TongNC = objCongThang.DLCong + objCongThang.SoNgayCT_NB;

                    //                        if (ns.MaLoaiLaoDong == "KTTSX")//Loại lao động = Trực tiếp SX mới tính chuyên cần
                    //                        {

                    //                            #region Code cu
                    //                            //Kiểm tra 3 dòng Công, Tăng ca, Tăng ca sau 22h                                
                    //                            //if (CountO > 0 || CountNV > 0 || CountB > 5 || CountTN > 5 || CountRo > 2 || CountCTR > 2 || CountTS > 2)
                    //                            //{
                    //                            //    //Nếu tồn tại O hoặc NV (kiểm tra ngày nghỉ việc nhân sự cập nhật trong thông tin nhân viên 
                    //                            //    //-> nếu ngày xét >= ngày nghỉ việc thì set NV vào dòng Công) hoặc B > 5, TN > 5, Ro>2, CTR>2, TS>2 -> = 0
                    //                            //    objCongThang.ChuyenCan = 0;
                    //                            //}
                    //                            //else if ((CountRo + CountM + CountTS) > 2)
                    //                            //{
                    //                            //    objCongThang.ChuyenCan = 0;
                    //                            //}
                    //                            //else if ((CountRo + CountM + CountTS) == 2 || CountB == 5)
                    //                            //{
                    //                            //    //Nếu Count(Ro) + Count(TS) + Count(M) = 2 hoặc Count(B)=5 -> 300.000
                    //                            //    objCongThang.ChuyenCan = 300000;
                    //                            //}
                    //                            //else if ((CountRo + CountM + CountTS) == 1 || CountB == 3 || CountB == 4)
                    //                            //{
                    //                            //    //Nếu Count(Ro) + Count(TS) + Count(M) = 1 hoặc Count(B)=3 -> 400.000
                    //                            //    objCongThang.ChuyenCan = 400000;
                    //                            //}
                    //                            //else if (CountRo == 0 && CountM == 0 && CountTS == 0)
                    //                            //{
                    //                            //    //Nếu Không có Ro và không có M và không có TS -> = 500.000
                    //                            //    objCongThang.ChuyenCan = 500000;
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    //Ngược lại = 0
                    //                            //    objCongThang.ChuyenCan = 0;
                    //                            //}
                    //                            #endregion

                    //                            if (CountM > 0)
                    //                            {
                    //                                if (CountNV == 0 && CountO == 0 && objCongThang.DLCong >= 13)
                    //                                    objCongThang.ChuyenCan = (1000000 / 26) * objCongThang.DLCong;
                    //                                else
                    //                                {
                    //                                    objCongThang.ChuyenCan = 0;
                    //                                }
                    //                            }
                    //                            else
                    //                            {
                    //                                if (CountRo > 2 || CountO > 0 || CountNV > 0 || (CountB + CountTN + CountTS) > 5 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 2) || ((CountB + CountTN + CountTS) > 3 && CountRo > 0))
                    //                                {
                    //                                    //Nếu tồn tại O hoặc NV (kiểm tra ngày nghỉ việc nhân sự cập nhật trong thông tin nhân viên 
                    //                                    //-> nếu ngày xét >= ngày nghỉ việc thì set NV vào dòng Công) hoặc B > 5, TN > 5, Ro>2, CTR>2, TS>2 -> = 0
                    //                                    objCongThang.ChuyenCan = 0;
                    //                                }
                    //                                else if (CountRo == 2 || (CountB + CountTN + CountTS) == 4 || (CountB + CountTN + CountTS) == 5 ||
                    //                                    ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4 && CountRo == 1))
                    //                                {
                    //                                    //Nếu Count(Ro) + Count(TS) + Count(M) = 2 hoặc Count(B)=5 -> 300.000
                    //                                    objCongThang.ChuyenCan = 600000;
                    //                                }
                    //                                else if (CountRo == 1 || ((CountB + CountTN + CountTS) > 0 && (CountB + CountTN + CountTS) < 4))
                    //                                {
                    //                                    //Nếu Count(Ro) + Count(TS) + Count(M) = 1 hoặc Count(B)=3 -> 400.000
                    //                                    objCongThang.ChuyenCan = 800000;
                    //                                }
                    //                                else if (CountRo + CountB + CountTS + CountTN + CountO + CountNV + CountM == 0)
                    //                                {
                    //                                    //Nếu Không có Ro và không có M và không có TS -> = 500.000
                    //                                    objCongThang.ChuyenCan = 1000000;
                    //                                }
                    //                                else
                    //                                {
                    //                                    //Ngược lại = 0
                    //                                    objCongThang.ChuyenCan = 0;
                    //                                }
                    //                            }

                    //                        }
                    //                        //Nghỉ Không phép = Count(O) dòng Công;
                    //                        objCongThang.NghiKhongPhep = CountO;
                    //                        objCongThang.PhepNam_Le_TNLD = CountP + CountR + CountL + CountP_2 + CountTN;
                    //                        //objCongThang.XNC5LThang = clsFunction.CountXacNhanCong(ns, NgayBatDau, NgayChotCong);
                    //                        objCongThang.XNC5LThang = XacNhanCong.Where(o => o.NhanSu == ns.NhanSu).ToList().Count;

                    //                        var temp = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).Select(it => it.Ngay).Distinct().ToList();
                    //                        objCongThang.SoNgayDiCongTac = listCongTacResult.Where(it => it.NhanSu == ns.NhanSu).ToList().Select(it => it.Ngay).Distinct().Count();

                    //                        var cthc = CongThangHC.Where(o => o.MaNV == ns.MaNV).FirstOrDefault();
                    //                        if (cthc != null)
                    //                        {
                    //                            objCongThang.TongNCHC = Convert.ToDouble(cthc.TongNC);
                    //                        }

                    //                        break;
                    //                    case 2:
                    //                        objCongThang = new CongThang();
                    //                        objCongThang.NhanSu = 0;
                    //                        objCongThang.MSNV = ns.MaNV;
                    //                        objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                    //                        objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                    //                        objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                    //                        objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                    //                        objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                    //                        objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                    //                        objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                    //                        objCongThang.MaLoaiCong = "TC";
                    //                        tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                    //                        if (tlc != null)
                    //                        {
                    //                            objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                    //                        }
                    //                        objCongThang.LoaiCongOrder = Loai;
                    //                        Cot = 1;
                    //                        ngay = NgayBatDau;
                    //                        while (ngay < NgayChotCong)
                    //                        {
                    //                            switch (Cot)
                    //                            {
                    //                                case 1:
                    //                                    objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 2:
                    //                                    objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 3:
                    //                                    objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 4:
                    //                                    objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 5:
                    //                                    objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 6:
                    //                                    objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 7:
                    //                                    objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 8:
                    //                                    objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 9:
                    //                                    objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 10:
                    //                                    objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 11:
                    //                                    objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 12:
                    //                                    objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 13:
                    //                                    objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 14:
                    //                                    objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 15:
                    //                                    objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 16:
                    //                                    objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 17:
                    //                                    objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 18:
                    //                                    objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 19:
                    //                                    objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 20:
                    //                                    objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 21:
                    //                                    objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 22:
                    //                                    objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 23:
                    //                                    objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 24:
                    //                                    objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 25:
                    //                                    objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 26:
                    //                                    objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 27:
                    //                                    objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 28:
                    //                                    objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 29:
                    //                                    objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 30:
                    //                                    objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                                case 31:
                    //                                    objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCa.ToString();
                    //                                    break;
                    //                            }
                    //                            ngay = ngay.AddDays(1);
                    //                            Cot += 1;
                    //                        }

                    //                        break;
                    //                    case 3:
                    //                        objCongThang = new CongThang();
                    //                        objCongThang.MSNV = ns.MaNV;
                    //                        objCongThang.NhanSu = 0;
                    //                        objCongThang.MaChamCong = LCongThangNgayTheoDong[0].MaChamCong;
                    //                        objCongThang.HoVaTen = LCongThangNgayTheoDong[0].HoVaTen;
                    //                        objCongThang.TenPhong_PhanXuong = LCongThangNgayTheoDong[0].TenPhong_PhanXuong;
                    //                        objCongThang.TenToChuyen = LCongThangNgayTheoDong[0].TenToChuyen;
                    //                        objCongThang.TenChucVu = LCongThangNgayTheoDong[0].TenChucVu;
                    //                        objCongThang.SoNgayPhepConLai = Convert.ToDouble(LCongThangNgayTheoDong[0].SoNgayPhepConLai);
                    //                        objCongThang.NgayCongChuan = Convert.ToInt32(LCongThangNgayTheoDong[0].NgayCongChuan);
                    //                        objCongThang.MaLoaiCong = "Sau22H";
                    //                        tlc = LoaiCong.Where(o => o.MaLoaiCong.Trim() == objCongThang.MaLoaiCong).FirstOrDefault();
                    //                        if (tlc != null)
                    //                        {
                    //                            objCongThang.TenLoaiCong = tlc.TenLoaiCong;
                    //                        }
                    //                        objCongThang.LoaiCongOrder = Loai;
                    //                        Cot = 1;
                    //                        ngay = NgayBatDau;
                    //                        while (ngay < NgayChotCong)
                    //                        {
                    //                            switch (Cot)
                    //                            {
                    //                                case 1:
                    //                                    objCongThang.Cot1 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 2:
                    //                                    objCongThang.Cot2 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 3:
                    //                                    objCongThang.Cot3 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 4:
                    //                                    objCongThang.Cot4 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 5:
                    //                                    objCongThang.Cot5 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 6:
                    //                                    objCongThang.Cot6 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 7:
                    //                                    objCongThang.Cot7 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 8:
                    //                                    objCongThang.Cot8 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 9:
                    //                                    objCongThang.Cot9 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 10:
                    //                                    objCongThang.Cot10 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 11:
                    //                                    objCongThang.Cot11 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 12:
                    //                                    objCongThang.Cot12 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 13:
                    //                                    objCongThang.Cot13 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 14:
                    //                                    objCongThang.Cot14 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 15:
                    //                                    objCongThang.Cot15 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 16:
                    //                                    objCongThang.Cot16 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 17:
                    //                                    objCongThang.Cot17 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 18:
                    //                                    objCongThang.Cot18 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 19:
                    //                                    objCongThang.Cot19 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 20:
                    //                                    objCongThang.Cot20 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 21:
                    //                                    objCongThang.Cot21 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 22:
                    //                                    objCongThang.Cot22 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 23:
                    //                                    objCongThang.Cot23 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 24:
                    //                                    objCongThang.Cot24 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 25:
                    //                                    objCongThang.Cot25 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 26:
                    //                                    objCongThang.Cot26 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 27:
                    //                                    objCongThang.Cot27 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 28:
                    //                                    objCongThang.Cot28 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 29:
                    //                                    objCongThang.Cot29 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 30:
                    //                                    objCongThang.Cot30 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                                case 31:
                    //                                    objCongThang.Cot31 = LCongThangNgayTheoDong[Cot - 1].TangCaSau22H.ToString();
                    //                                    break;
                    //                            }
                    //                            ngay = ngay.AddDays(1);
                    //                            Cot += 1;
                    //                        }
                    //                        break;
                    //                }
                    //                objCongThang.STT = STT;
                    //                model.ListCongThang.Add(objCongThang);
                    //                Loai += 1;
                    //                STT += 1;
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            ViewBag.Error = "Lỗi xử lý MSNV: " + ns.MaNV + ", " + ex.Message;
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    rs.text = ex.Message;
                    //    rs.code = 0;
                    //}
                    #endregion
                }

            }
            model.ListCongThang = CongThang;
            model.ListNgayCong = listCongNgay;
            rs.data = model; ;
            rs.code = 1;
            var js = Json(rs, JsonRequestBehavior.AllowGet);
            js.MaxJsonLength = int.MaxValue;
            return js;
            // return View();
        }

        [RoleAuthorize(Roles = "0=0,52=2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LuuCongNgayHieuChinh(List<CongNgay> DLCong) {
            using (SaveDB db = new SaveDB())
            {
                using (var tran = new TransactionScope())
                {
                    db.GhiChu = "Công ngày hiệu chỉnh";
                    JsonStatus rs = new JsonStatus();
                    rs.code = 0;
                    rs.text = "Thất bại";
                    string sLoi = "";
                    string MSNV = "";
                    try
                    {
                        bool hc = false;
                        TTF_CongNgayHieuChinh objTTF_CongNgayHieuChinh = null;
                        var NguoiDung = Users.GetNguoiDung(User.Identity.Name);
                        int kq = 0;
                        foreach (var item in DLCong)
                        {
                            if (item.CongHC == null)
                            {
                                item.CongHC = "";
                            }
                            if (item.TangCaHC == null)
                            {
                                item.TangCaHC = "";
                            }
                            if (item.TangCaSau22HHC == null)
                            {
                                item.TangCaSau22HHC = "";
                            }
                            if (item.InTimeHC == null)
                            {
                                item.InTimeHC = "";
                            }
                            if (item.OutDateHC == null)
                            {
                                item.OutDateHC = "";
                            }
                            if (item.OutTimeHC == null)
                            {
                                item.OutTimeHC = "";
                            }
                            DateTime dateTemp;
                            string[] formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                            if (!DateTime.TryParseExact(item.InDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTemp))
                            {
                                sLoi += "Lỗi dòng: <b>" + item.Name + " cột InDate " + item.InDate + "<b/><br>";
                            }
                            var checkcnhc = db.TTF_CongNgayHieuChinh.Where(o => o.MaNV == item.MaNV
                                && o.Date == dateTemp.Date).FirstOrDefault();
                            if (checkcnhc == null)
                            {
                                MSNV = item.MaNV;
                                objTTF_CongNgayHieuChinh = new TTF_CongNgayHieuChinh();
                                objTTF_CongNgayHieuChinh.Date = dateTemp;
                                objTTF_CongNgayHieuChinh.MaNV = item.MaNV;
                                if (item.CongHC != ""
                                    || item.TangCaHC != ""
                                    || item.TangCaSau22HHC != ""
                                    || item.InTimeHC != null
                                    || item.OutDateHC != ""
                                    || item.OutTimeHC != null)
                                {
                                    hc = true;
                                }
                                if (hc == true)
                                {
                                    if (item.CongHC.Trim() == "Ro")
                                    {
                                        objTTF_CongNgayHieuChinh.Cong = item.CongHC.Trim();
                                    }
                                    else
                                    {
                                        objTTF_CongNgayHieuChinh.Cong = item.CongHC.Trim().ToUpper();
                                    }
                                    if (item.TangCaHC.Trim() == "Ro")
                                    {
                                        objTTF_CongNgayHieuChinh.TangCa = item.TangCaHC.Trim();
                                    }
                                    else
                                    {
                                        objTTF_CongNgayHieuChinh.TangCa = item.TangCaHC.Trim().ToUpper();
                                    }
                                    if (item.TangCaSau22HHC.Trim() == "Ro")
                                    {
                                        objTTF_CongNgayHieuChinh.TangCaSau22H = item.TangCaSau22HHC.Trim();
                                    }
                                    else
                                    {
                                        objTTF_CongNgayHieuChinh.TangCaSau22H = item.TangCaSau22HHC.Trim().ToUpper();
                                    }
                                    objTTF_CongNgayHieuChinh.InTime = item.InTimeHC;
                                    objTTF_CongNgayHieuChinh.OutDate = item.OutDateHC != "" && item.OutDateHC != null ? DateTime.Parse(item.OutDateHC).ToString("dd/MM/yyyy") : null;
                                    objTTF_CongNgayHieuChinh.OutTime = item.OutTimeHC;
                                    objTTF_CongNgayHieuChinh.NguoiHC = (int)NguoiDung.NguoiDung;
                                    objTTF_CongNgayHieuChinh.NgayHC = DateTime.Now;
                                    db.TTF_CongNgayHieuChinh.Add(objTTF_CongNgayHieuChinh);
                                    db.SaveChanges();
                                    kq++;
                                }
                            }
                            else
                            {
                                MSNV = checkcnhc.MaNV;
                                if (item.CongHC.Trim() == "Ro")
                                {
                                    checkcnhc.Cong = item.CongHC.Trim();
                                }
                                else
                                {
                                    checkcnhc.Cong = item.CongHC.Trim().ToUpper();
                                }
                                if (item.TangCaHC.Trim() == "Ro")
                                {
                                    checkcnhc.TangCa = item.TangCaHC.Trim();
                                }
                                else
                                {
                                    checkcnhc.TangCa = item.TangCaHC.Trim().ToUpper();
                                }
                                if (item.TangCaSau22HHC.Trim() == "Ro")
                                {
                                    checkcnhc.TangCaSau22H = item.TangCaSau22HHC.Trim();
                                }
                                else
                                {
                                    checkcnhc.TangCaSau22H = item.TangCaSau22HHC.Trim().ToUpper();
                                }
                                checkcnhc.InTime = item.InTimeHC;
                                checkcnhc.OutDate = item.OutDateHC!="" && item.OutDateHC!=null? DateTime.Parse(item.OutDateHC).ToString("dd/MM/yyyy"):null;
                                checkcnhc.OutTime = item.OutTimeHC;
                                checkcnhc.InTime = item.InTimeHC;
                                checkcnhc.OutTime = item.OutTimeHC;
                                checkcnhc.NguoiHC = (int)NguoiDung.NguoiDung;
                                checkcnhc.NgayHC = DateTime.Now;
                                db.SaveChanges();
                                kq++;
                            }
                        }
                        if (sLoi.Length == 0)
                        {
                            tran.Complete();
                            rs.text = "Hiệu chỉnh thành công " + kq.ToString();
                            rs.code = 1;
                        }
                        else
                        {
                            rs.text = sLoi;
                            rs.code = 0;
                        }


                    }
                    catch (Exception ex)
                    {
                        rs.text = "Thất bại dòng MSNV: " + MSNV + "\r\n" + ex.Message;
                        rs.code = 0;
                    }
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [RoleAuthorize(Roles = "0=0,52=2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> XoaCongNgayHieuChinh(List<CongNgay> DLCong)
        {
            using (SaveDB db = new SaveDB())
            {
                using (var tran = new TransactionScope())
                {
                    db.GhiChu = "Xóa công hiệu chỉnh";
                    JsonStatus rs = new JsonStatus();
                    rs.code = 0;
                    rs.text = "Thất bại";
                    string sLoi = "";
                    string MSNV = "";
                    try
                    {
                        TTF_CongNgayHieuChinh objTTF_CongNgayHieuChinh = null;
                        var NguoiDung = Users.GetNguoiDung(User.Identity.Name);
                        int kq = 0;
                        foreach (var item in DLCong)
                        {
                            DateTime dateTemp;
                            string[] formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                            if (!DateTime.TryParseExact(item.InDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTemp))
                            {
                                sLoi += "Lỗi dòng: <b>" + item.Name + " cột InDate " + item.InDate + "<b/><br>";
                            }
                            var checkcnhc = db.TTF_CongNgayHieuChinh.Where(o => o.MaNV == item.MaNV
                                && o.Date == dateTemp.Date).FirstOrDefault();
                            if (checkcnhc != null)
                            {
                                db.TTF_CongNgayHieuChinh.Remove(checkcnhc);
                                db.SaveChanges();
                                kq++;
                            }
                        }
                        if (sLoi.Length == 0)
                        {
                            tran.Complete();
                            rs.text = "Xóa công hiệu chỉnh thành công " + kq.ToString();
                            rs.code = 1;
                        }
                        else
                        {
                            rs.text = sLoi;
                            rs.code = 0;
                        }


                    }
                    catch (Exception ex)
                    {
                        rs.text = "Thất bại dòng MSNV: " + MSNV + "\r\n" + ex.Message;
                        rs.code = 0;
                    }
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult ImportDLVTCaDem()
        {
            return View();
        }
        [RoleAuthorize(Roles = "0=0,51=2")]
        [HttpPost]
        public async Task<JsonResult> GetImportDLVTCaDem(HttpPostedFileBase UploadedFile) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            string sLoi = "";
            var model = new List<DLVT_CaDem>();
            try
            {
                string resultFilePath = "";
                if (UploadedFile != null && UploadedFile.ContentLength > 0
                    && ((Path.GetExtension(UploadedFile.FileName).Equals(".xlsx")) || (Path.GetExtension(UploadedFile.FileName).Equals(".xlx"))))
                {
                    string fileName = UploadedFile.FileName;
                    string UploadDirectory = Server.MapPath("~/Content/Temps/");
                    bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                    if (!folderExists)
                        System.IO.Directory.CreateDirectory(UploadDirectory);
                    resultFilePath = UploadDirectory + fileName;
                    Int32 row = 0;
                  
                    try
                    {
                        UploadedFile.SaveAs(resultFilePath);
                        DataTable dt = clsFunction.getDataTableFromExcel(resultFilePath);
                       
                        DLVT_CaDem objDLVT_CaDem = null;
                        DateTime dateTemp;
                        string InDate = "";
                        string OutDate = "";
                        string InTime = "";
                        string OutTime = "";
                        string[] formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                       
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            row = 1;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                objDLVT_CaDem = new DLVT_CaDem();
                                objDLVT_CaDem.Check = false;
                                objDLVT_CaDem.GID = Convert.ToInt32(dt.Rows[i]["GID"].ToString());
                                objDLVT_CaDem.UID = dt.Rows[i]["UID"].ToString();
                                objDLVT_CaDem.Name = dt.Rows[i]["Name"].ToString();

                                if (dt.Rows[i]["InDate"].ToString().Trim() != "")
                                {
                                    InDate = dt.Rows[i]["InDate"].ToString().Trim();
                                    InDate = InDate.Replace("-", "/");
                                    if (!DateTime.TryParseExact(InDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTemp))
                                    {
                                        sLoi += "Lỗi dòng: <b>" + row + " cột InDate " + InDate + "<b/><br>";
                                    }
                                    InDate = dateTemp.ToString("dd/MM/yyyy");
                                    InTime = dt.Rows[i]["InTime"] != null && dt.Rows[i]["InTime"].ToString().Trim() != "" ? TimeSpan.Parse(dt.Rows[i]["InTime"].ToString().Trim()).ToString() : null;
                                }
                                if (dt.Rows[i]["OutDate"].ToString().Trim() != "")
                                {
                                    OutDate = dt.Rows[i]["OutDate"].ToString().Trim();

                                    OutDate = OutDate.Replace('-', '/');
                                    //OutDate = DateTime.ParseExact(OutDate, "dd-MMM-yyyy", new CultureInfo("en-US")).ToString("dd/MM/yyyy");
                                    if (!DateTime.TryParseExact(OutDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTemp))
                                    {
                                        sLoi += "Lỗi dòng: <b>" + row + " cột OutDate " + OutDate + "<b/><br>";
                                    }
                                    OutDate = dateTemp.ToString("dd/MM/yyyy");
                                    OutTime = dt.Rows[i]["OutTime"] != null && dt.Rows[i]["OutTime"].ToString().Trim() != "" ? TimeSpan.Parse(dt.Rows[i]["OutTime"].ToString().Trim()).ToString() : null;
                                }

                                if (dt.Rows[i]["InDate"].ToString().Trim() != "" && dt.Rows[i]["OutDate"].ToString().Trim() != "")
                                {
                                    objDLVT_CaDem.InDate = InDate;
                                    objDLVT_CaDem.InTime = InTime;
                                    objDLVT_CaDem.OutDate = OutDate;
                                    objDLVT_CaDem.OutTime = OutTime;
                                }
                                else if (dt.Rows[i]["InDate"].ToString().Trim() != "" && dt.Rows[i]["OutDate"].ToString().Trim() == "")
                                {
                                    objDLVT_CaDem.InDate = InDate;
                                    objDLVT_CaDem.InTime = InTime;
                                    objDLVT_CaDem.OutDate = InDate;
                                    objDLVT_CaDem.OutTime = InTime;
                                }
                                else if (dt.Rows[i]["InDate"].ToString().Trim() == "" && dt.Rows[i]["OutDate"].ToString().Trim() != "")
                                {
                                    objDLVT_CaDem.OutDate = OutDate;
                                    objDLVT_CaDem.OutTime = OutTime;
                                    objDLVT_CaDem.InDate = OutDate;
                                    objDLVT_CaDem.InTime = OutTime;
                                }
                                model.Add(objDLVT_CaDem);
                                row += 1;
                            }
                            System.IO.File.Delete(resultFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(resultFilePath);
                        sLoi += "Lỗi dòng: " + row + "\r\n" + ex.Message;
                    }
                }
                else
                {
                    System.IO.File.Delete(resultFilePath);
                    sLoi += "Vui lòng sử dụng file Excel có đuôi .xlsx";
                }
            }
            catch
            {
                sLoi += "Vui lòng sử dụng file Excel có đuôi .xlsx";
            }
            rs.text = sLoi;
            rs.data = model;
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "0=0,52=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LuuDLVTCaDem(List<DLVT_CaDem> list)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var tran = new TransactionScope())
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Import dữ liệu ca đêm";
                    DateTime ngay = new DateTime();
                    TTF_DLVT_CaDem objTTF_DLVT_CaDem = null;
                    string UID = "";
                    string d = "";
                    int kq = 0;
                    try
                    {
                        foreach (var item in list)
                        {
                            UID = item.UID;
                            d = item.InDate;
                            ngay = DateTime.ParseExact(item.InDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                            var CheckExist = db.TTF_DLVT_CaDem.Where(o => o.UID == item.UID && o.InDate == ngay).FirstOrDefault();
                            if (CheckExist == null)
                            {
                                objTTF_DLVT_CaDem = new TTF_DLVT_CaDem();
                                objTTF_DLVT_CaDem.UID = item.UID;
                                objTTF_DLVT_CaDem.InDate = DateTime.ParseExact(item.InDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                                objTTF_DLVT_CaDem.OutDate = DateTime.ParseExact(item.OutDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                                objTTF_DLVT_CaDem.InTime = item.InTime != null && item.InTime.Trim() != "" ? TimeSpan.Parse(item.InTime).ToString() : null;
                                objTTF_DLVT_CaDem.OutTime = item.OutTime != null && item.OutDate.Trim() != "" ? TimeSpan.Parse(item.OutTime).ToString() : null;
                                db.TTF_DLVT_CaDem.Add(objTTF_DLVT_CaDem);
                                db.SaveChanges();
                                kq++;

                            }
                            else
                            {
                                CheckExist.OutDate = DateTime.ParseExact(item.OutDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                                CheckExist.InTime = item.InTime != null && item.InTime.Trim() != "" ? TimeSpan.Parse(item.InTime).ToString() : null;
                                CheckExist.OutTime = item.OutTime != null && item.OutDate.Trim() != "" ? TimeSpan.Parse(item.OutTime).ToString() : null;
                                db.SaveChanges();
                                kq++;
                            }
                        }
                       
                        if (kq > 0)
                        {
                            tran.Complete();
                            rs.code = 1;
                            rs.text = "Thành công " + kq;
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
                        //clsFunction.NhatkyLoi(DateTime.Now, HttpContext.Current.User.Identity.Name, sb.ToString(), tablename, type);
                        //return 0;
                        rs.text = sb.ToString();
                    }
                    catch (Exception ex)
                    {
                        rs.text = "Thất bại dòng: " + UID + "," + d + "\r\n" + ex.Message;
                    }
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [RoleAuthorize(Roles = "0=0,52=1")]
        public async Task<JsonResult> GetDataDLVTCaDem(string maNV, string ngay)
        {
            JsonStatus rs = new JsonStatus();
            string sWhere1 = "1 = 1 And Convert(Date,InDate) = '" + ngay + "'", sWhere2 = "1 = 1 ";
            if (maNV != null && maNV != "")
            {
                sWhere2 += " And MaNV = '" + maNV + "' ";
            }
            string sql = "SELECT 0 GID,CONVERT(NVARCHAR(10),InDate,103) InDate,InTime,CONVERT(NVARCHAR(10),OutDate,103)  OutDate,OutTime,UID,TTF_NhanSu.HoVaTen AS Name " +
                            " FROM (SELECT * FROM dbo.TTF_DLVT_CaDem WHERE " + sWhere1 + " )TTF_DLVT_CaDem " +
                            " INNER JOIN (SELECT MaChamCong,HoVaTen FROM dbo.TTF_NhanSu WHERE " + sWhere2 + ")TTF_NhanSu ON TTF_DLVT_CaDem.UID = TTF_NhanSu.MaChamCong ";
            using (var db = new TTF_FACEIDEntities())
            {
                rs.data = db.Database.SqlQuery<DLVT_CaDem>(sql).ToList();
            }
            var js = Json(rs, JsonRequestBehavior.AllowGet);
            js.MaxJsonLength = int.MaxValue;
           
            return Json(js, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,52=1")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> XoaCongCaDem(List<DLVT_CaDem> list) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var tran = new TransactionScope())
            {
                using (var db = new SaveDB())
                {
                    try
                    {
                        db.GhiChu = "Xóa dữ liệu công ca đêm";
                        int ikq = 0;
                        DateTime dtemp;
                        foreach (var item in list)
                        {
                            dtemp = DateTime.ParseExact(item.InDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                            var del = db.TTF_DLVT_CaDem.FirstOrDefault(it => it.UID == item.UID && it.InDate == dtemp.Date);
                            if (del != null)
                            {
                                db.TTF_DLVT_CaDem.Remove(del);
                                db.SaveChanges();
                                ikq++;
                            }
                        }
                        if (ikq > 0)
                        {
                            tran.Complete();
                            rs.code = 1;
                            rs.text = "Thành công " + ikq.ToString();
                        }
                        else
                        {
                            rs.text = "Không có dữ liệu xóa ";
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