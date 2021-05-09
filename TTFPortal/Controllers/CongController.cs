using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public ActionResult CongNgay()
        {
            return View();
        }

        [RoleAuthorize(Roles = "0=0,51=2")]
        [HttpGet]
        public async Task<JsonResult> XemCongNgay(string tuNgay,string denNgay,string hoVaTen, string maPhongBan,string maNV)
        {
            var rs = new JsonStatus();
            var model = Json(rs, JsonRequestBehavior.AllowGet);
            List<CongNgay> DLCong = new List<CongNgay>();
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
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
                if (maNV == null)
                    maNV = "";
                if (maPhongBan == null)
                    maPhongBan = "";
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

        [RoleAuthorize(Roles = "0=0,51=2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LuuCongNgayHieuChinh(List<CongNgay> DLCong) {
            using (SaveDB db = new SaveDB())
            {
                db.GhiChu = "Công hiệu chỉnh";
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                rs.text = "Thất bại";
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
                        var checkcnhc = db.TTF_CongNgayHieuChinh.Where(o => o.MaNV == item.MaNV
                            && o.Date == item.Date).FirstOrDefault();
                        if (checkcnhc == null)
                        {
                            MSNV = item.MaNV;
                            objTTF_CongNgayHieuChinh = new TTF_CongNgayHieuChinh();
                            objTTF_CongNgayHieuChinh.Date = item.Date;
                            objTTF_CongNgayHieuChinh.MaNV = item.MaNV;
                            if (item.CongHC != ""
                                || item.TangCaHC != ""
                                || item.TangCaSau22HHC != ""
                                || item.InTimeHC1 != null
                                || item.OutDateHC != ""
                                || item.OutTimeHC1 != null)
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
                                objTTF_CongNgayHieuChinh.OutDate = item.OutDateHC;
                                objTTF_CongNgayHieuChinh.OutTime = item.OutTimeHC ;
                                objTTF_CongNgayHieuChinh.NguoiHC = (int)NguoiDung.NguoiDung;
                                objTTF_CongNgayHieuChinh.NgayHC = DateTime.Now;
                                db.TTF_CongNgayHieuChinh.Add(objTTF_CongNgayHieuChinh);
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
                            checkcnhc.OutDate = item.OutDateHC;
                            checkcnhc.OutTime = item.OutTimeHC;
                            checkcnhc.InTime = item.InTimeHC;
                            checkcnhc.OutTime = item.OutTimeHC;
                            checkcnhc.NguoiHC = (int)NguoiDung.NguoiDung;
                            checkcnhc.NgayHC = DateTime.Now;
                        }
                    }
                    kq = db.SaveChanges();
                    rs.text  = "Hiệu chỉnh thành công " + kq.ToString();
                }
                catch (Exception ex)
                {
                    rs.text = "Thất bại dòng MSNV: " + MSNV + "\r\n" + ex.Message;
                    rs.code = 0;
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ImportDLVTCaDem()
        {
            return View();
        }
        public async Task<JsonResult> GetImportDLVTCaDem(HttpPostedFileBase UploadedFile) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            string sLoi = "";
            var model = new List<DLVT_CaDem>();
            try
            {
                string resultFilePath = "";
                if (UploadedFile != null && UploadedFile.ContentLength > 0
                    && (Path.GetExtension(UploadedFile.FileName).Equals(".xlsx")))
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
                        string[] formats = { "d/MMM/yy","dd/MMM/yy","MMM/dd/yy","yy/MMM/dd","d/MMM/yyyy","dd/MMM/yyyy","dd/MM/yyyy", "d/MM/yyyy",
                            "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy" };
                       
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

    }
}