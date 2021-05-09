using System;
using System.Collections.Generic;
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
    [RoleAuthorize]
    public class NghiPhepController : Controller
    {
        // GET: NghiPhep
        // Phep cua ban
        public ActionResult NghiPhepCuaBan()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> gvNghiPhepPartial(string TuNgay, string DenNgay)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                TuNgay = TuNgay != null ? TuNgay : "01/01/1900";
                DenNgay = DenNgay != null ? DenNgay : DateTime.Now.ToString("MM/dd/yyyy");
                int NguoiTao = (int)Users.GetNguoiDung(User.Identity.Name).NguoiDung;
                var model = db.Proc_QuaTrinhNghiPhep(NguoiTao, TuNgay, DenNgay).OrderByDescending(it => it.IDNghiPhep).ToList();
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }
        }
        [RoleAuthorize(Roles = "0=0,42=2")]
        public async Task<ActionResult> Create()
        {
            var nguoidung = Users.GetNguoiDung(User.Identity.Name);
            if (nguoidung.NguoiDung < 1)
            {
                return Content("Tài khoản không tạo được");
            }

            return View();
        }
        public async Task<JsonResult> GetIntDate(string TuNgay, string DenNgay)
        {
            int kq = 0;
            List<NghiPhepChiTiet> model = new List<NghiPhepChiTiet>();
            try
            {
                kq = (DateTime.Parse(DenNgay).Date - DateTime.Parse(TuNgay).Date).Days;
                DateTime temp = DateTime.Parse(TuNgay), temp2;
                for (int j = 0; j < kq + 1; j++)
                {
                    NghiPhepChiTiet add = new NghiPhepChiTiet();
                    temp2 = temp.AddDays(j);
                    if (temp2.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }
                    if ((temp2.DayOfWeek == DayOfWeek.Saturday))
                        add.GhiChu = "Thứ 7";
                    add.IDNghiPhep = j;
                    add.Ngay = temp.AddDays(j);
                    add.NgayS = temp.AddDays(j).ToString("dd/MM/yyyy");
                    add.ChuKyCaLamViec = null;
                    add.Check = true;

                    model.Add(add);
                }
            }
            catch (Exception)
            {
                kq = 0;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> BuoiLamViec()
        {

            List<BuoiLamViec> model = new List<BuoiLamViec>();

            BuoiLamViec add1 = new BuoiLamViec();
            add1.ChuKyCaLamViec = null;
            add1.TenBuoiLamViec = "Cả ngày";
            BuoiLamViec add2 = new BuoiLamViec();
            add2.ChuKyCaLamViec = 1;
            add2.TenBuoiLamViec = "Sáng";

            BuoiLamViec add3 = new BuoiLamViec();
            add3.ChuKyCaLamViec = 2;
            add3.TenBuoiLamViec = "Chiều";

            model.Add(add1);
            model.Add(add2);
            model.Add(add3);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,42=2")]
        public async Task<JsonResult> AddNghiPhep(NghiPhep item)
        {
            using (var db = new SaveDB())
            {
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                rs.text = "Thất bại";
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                string str = "";
                var ng = Users.GetNguoiDung(User.Identity.Name);
                int NguoiTao = (int)ng.NguoiDung;
                if (NguoiTao < 0)
                {
                    //return Content("Bạn đã hết thời gian thao tác trên web xin hãy đăng nhập lại");
                    rs.text = "Bạn đã hết thời gian thao tác trên web xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                if (clsFunction.checkKyCongNguoiDung((DateTime)item.TuNgay))
                {
                    rs.text = "Kỳ công đã đóng không thể thêm nghỉ phép";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                    //return Content("Kỳ công đã đóng không thể thêm nghỉ phép");
                }
                List<TTF_NghiBu> NghiBu = null;
                double dSoPhut = 0;
                try
                {
                    if (item.NPCT != null && item.NPCT.Count > 0)
                    {

                        double iSoNgayNghi = 0.0;


                        foreach (NghiPhepChiTiet it in item.NPCT)
                        {
                            if (iSoNgayNghi == 0)
                            {
                                if (item.TuNgay.Value.Date.ToString("dd/MM/yyyy") != it.NgayS)
                                {
                                    rs.text = "Ngày bắt đầu nghỉ không hợp lệ";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                    //return Content("Ngày bắt đầu nghỉ không hợp lệ");
                                }
                            }
                            if (it.Check == true)
                            {
                                if (it.ChuKyCaLamViec == null)
                                {
                                    iSoNgayNghi += 1;
                                }
                                else
                                {
                                    iSoNgayNghi += 0.5;
                                }
                            }
                        }
                        if (iSoNgayNghi % 1 > 0 && item.MaLoaiNghiPhep != "P" && item.MaLoaiNghiPhep != "Ro" && item.MaLoaiNghiPhep != "NB")
                        {
                            rs.text = "Loại phép bạn chọn không được nghỉ 0.5 ngày";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                            // return Content("Loại phép bạn chọn không được nghỉ 0.5 ngày");
                        }
                        if (item.MaLoaiNghiPhep.Trim() == "NB")
                        {
                            dSoPhut = iSoNgayNghi * 8 * 60;
                            if (iSoNgayNghi > 1)
                            {
                                rs.text = "Nghỉ bù chỉ đăng ký nghỉ tối đa là 1 ngày";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                                //return Content("Nghỉ bù chỉ đăng ký nghỉ tối đa là 1 ngày");
                            }
                            NghiBu = db.TTF_NghiBu.Where(obj => obj.SoPhutConLai > 0 && obj.MaNV == item.MaNhanVien && DateTime.Now <= obj.NgayHetHieuLuc && obj.TCDaHuy != true).OrderBy(obj => obj.NgayHieuLuc).ToList();
                            if (NghiBu.Count == 0 || NghiBu.Sum(it => it.SoPhutConLai) < dSoPhut)
                            {
                                rs.text = "Số ngày nghỉ bù vượt quá thời gian nghỉ bù";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                                ///return Content("Số ngày nghỉ bù vượt quá thời gian nghỉ bù");
                            }
                        }
                        if (iSoNgayNghi < 0.5)
                        {
                            rs.text = "Số ngày nghỉ không hợp lệ";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                            //return Content("Số ngày nghỉ không hợp lệ");
                        }

                        //if ((item.MaLoaiNghiPhep == "P/2" || item.MaLoaiNghiPhep == "Ro/2") && iSoNgayNghi > 0.5)
                        //{
                        //    return Content("Bạn có số ngày nghỉ vượt quá quy định loại nghỉ phép đang chọn không thể lưu");
                        //}
                        //if ((item.MaLoaiNghiPhep == "P" || item.MaLoaiNghiPhep == "Ro") && iSoNgayNghi < 1)
                        //{
                        //    return Content("Bạn nên chọn nghỉ phép nửa ngày cho số ngày nghỉ là 0.5 ngày");

                        //}
                        //// kiểm tra có thông tin nghỉ phép chưa duyệt ko

                        //var temp1 = db.TTF_NghiPhep.Where(it => it.NhanSu == item.NhanSu && it.MaTrangThaiDuyet != "3" && it.MaTrangThaiDuyet != "4" && it.Del != true).ToList();
                        //if (temp1 != null && temp1.Count > 0)
                        //{
                        //    return Content("Bạn đang có một thông tin nghỉ phép chưa được duyệt không thể thêm vào");
                        //}
                        // kiểm tra ngày phép cần tạo có tồn tại trong database chua

                        var kiemTraTonTai = (from nghiphep in db.TTF_NghiPhep
                                             join nghiphepchitiet in db.TTF_NghiPhepChiTiet on nghiphep.IDNghiPhep equals nghiphepchitiet.IDNghiPhep
                                             where nghiphep.NhanSu == item.NhanSu && nghiphepchitiet.Ngay >= item.TuNgay && nghiphepchitiet.Ngay <= item.DenNgay && nghiphep.Del != true && nghiphep.MaTrangThaiDuyet != "4" && nghiphepchitiet.Del != true
                                             select new { nghiphepchitiet.Ngay }).ToList();
                        str = "Đã tồn tại thông tin nghỉ phép các ngày sau nên không thể thêm <br>";
                        if (kiemTraTonTai != null && kiemTraTonTai.Count > 0)
                        {
                            for (int i = 0; i < kiemTraTonTai.Count; i++)
                            {
                                str += "<b>" + kiemTraTonTai[i].Ngay.Date.ToString("dd/MM/yyyy") + "</b><br>";
                            }
                            rs.text = MvcHtmlString.Create(str).ToHtmlString();
                            return Json(rs, JsonRequestBehavior.AllowGet);
                            // return Content(MvcHtmlString.Create(str).ToHtmlString());
                        }
                        TTF_NhanSu nhansu = null;
                        if (item.MaLoaiNghiPhep == "P" || item.MaLoaiNghiPhep == "P/2")
                        {
                            nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);

                            TimeSpan dd = Convert.ToDateTime(item.TuNgay) - Convert.ToDateTime(nhansu.NgayVaoCongTy);
                            double dTem = dd.TotalDays;

                            if (nhansu.SoNgayThuViec > 0)
                            {
                                rs.text = MvcHtmlString.Create("Bạn <b>" + item.HoVaTen + "</b> đang thử việc không thể tạo phép hưởng lương").ToHtmlString();
                                return Json(rs, JsonRequestBehavior.AllowGet);
                                //return Content(MvcHtmlString.Create("Bạn <b>" + item.HoVaTen + "</b> đang thử việc không thể tạo phép hưởng lương").ToHtmlString());
                            }
                            double PhepConLai = nhansu.SoNgayPhepConLai == null ? 0 : nhansu.SoNgayPhepConLai.Value;
                            if (iSoNgayNghi > PhepConLai)
                            {
                                rs.text = "Số ngày phép bạn yêu cầu nghỉ được hưởng lương vượt quá ngày phép còn lại";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                                //return Content("Số ngày phép bạn yêu cầu nghỉ được hưởng lương vượt quá ngày phép còn lại");
                            }
                            //if(KienTraMuonPhep < -9)
                            //{
                            //    return Content("Số ngày phép bạn yêu cầu nghỉ được hưởng lương vượt quá số ngày phép tạm ứng cho bạn");
                            //}
                        }
                        using (var tran = new TransactionScope())
                        {
                            int sr = 0;
                            TTF_NghiPhep add = new TTF_NghiPhep();
                            add.NhanSu = item.NhanSu;
                            add.TuNgay = item.TuNgay;
                            add.DenNgay = item.DenNgay;
                            add.NgayTao = DateTime.Now;
                            add.MayTao = System.Net.Dns.GetHostName();
                            add.NguoiTao = NguoiTao;
                            add.SoNgayNghi = iSoNgayNghi;
                            add.MaLoaiNghiPhep = item.MaLoaiNghiPhep;
                            add.MaTrangThaiDuyet = "1";
                            // add.IDNguoiDuyetKeTiep = nguoiduyetketiep;
                            add.LyDoNghi = item.LyDoNghi;
                            db.TTF_NghiPhep.Add(add);
                            sr = db.SaveChanges();
                            if (sr > 0)
                            {
                                //var nghiphep = db.TTF_NghiPhep.Where(it => it.NhanSu == item.NhanSu && it.TuNgay == item.TuNgay && it.DenNgay == item.DenNgay).OrderByDescending(it => it.IDNghiPhep).ToList();

                                foreach (NghiPhepChiTiet it in item.NPCT)
                                {
                                    if (it.Check == null || it.Check == false)
                                        continue;
                                    TTF_NghiPhepChiTiet add1 = new TTF_NghiPhepChiTiet();
                                    add1.ChuKyCaLamViec = it.ChuKyCaLamViec;
                                    add1.Ngay = DateTime.ParseExact(it.NgayS, "dd/MM/yyyy", new CultureInfo("en-US"));
                                    if (it.ChuKyCaLamViec != null)
                                        add1.SoNgay = 0.5;
                                    else
                                        add1.SoNgay = 1;
                                    add1.IDNghiPhep = add.IDNghiPhep;
                                    db.TTF_NghiPhepChiTiet.Add(add1);
                                    db.SaveChanges();
                                }
                                // trừ ngày phép
                                if (item.MaLoaiNghiPhep.Trim() == "P" || item.MaLoaiNghiPhep == "P/2")
                                {
                                    //nhansu.SoNgayPhepConLai = nhansu.SoNgayPhepConLai - iSoNgayNghi > 0 ? nhansu.SoNgayPhepConLai - iSoNgayNghi : 0;
                                    nhansu.SoNgayPhepConLai = nhansu.SoNgayPhepConLai - iSoNgayNghi;// duoc phep âm phep do tam ung truoc
                                    db.SaveChanges();
                                }
                                // trừ ngày nghỉ bù
                                if (item.MaLoaiNghiPhep == "NB")
                                {
                                    string sCapNhatNB = "", sCapNhatNghiBuChiTiet = "";
                                    double soPhut = 0;

                                    foreach (var obj in NghiBu)
                                    {
                                        if (dSoPhut == 0)
                                            break;

                                        if (dSoPhut >= obj.SoPhutConLai)
                                        {
                                            sCapNhatNB += "UPDATE dbo.TTF_NghiBu SET SoPhutConLai = 0 WHERE MaNV = '" + obj.MaNV + "' AND IDTangCa = '" + obj.IDTangCa + "' AND NgayPhatSinh = '" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "' \n";
                                            sCapNhatNghiBuChiTiet += "INSERT INTO dbo.TTF_NghiBuChiTiet ( IDNghiPhep , IDTangCa ,MaNV , NgayPhatSinh , SoPhut,Del )VALUES  (" + add.IDNghiPhep + ",'" + obj.IDTangCa + "',N'" + obj.MaNV + "','" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "','" + obj.SoPhutConLai + "',0) \n";
                                            dSoPhut -= (double)obj.SoPhutConLai;
                                        }
                                        else
                                        {
                                            soPhut = (double)obj.SoPhutConLai - dSoPhut;
                                            sCapNhatNB += "UPDATE dbo.TTF_NghiBu SET SoPhutConLai = '" + soPhut + "' WHERE MaNV = '" + obj.MaNV + "' AND IDTangCa = '" + obj.IDTangCa + "' AND NgayPhatSinh = '" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "' \n";
                                            sCapNhatNghiBuChiTiet += "INSERT INTO dbo.TTF_NghiBuChiTiet ( IDNghiPhep , IDTangCa ,MaNV , NgayPhatSinh , SoPhut,Del )VALUES  (" + add.IDNghiPhep + ",'" + obj.IDTangCa + "',N'" + obj.MaNV + "','" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "','" + dSoPhut + "',0) \n";
                                            dSoPhut = 0;
                                        }

                                    }
                                    if (sCapNhatNghiBuChiTiet != "" && sCapNhatNB != "")
                                    {
                                        db.Database.ExecuteSqlCommand(sCapNhatNB);
                                        db.Database.ExecuteSqlCommand(sCapNhatNghiBuChiTiet);

                                    }
                                }
                                tran.Complete();
                                rs.code = 1;
                                rs.description = add.IDNghiPhep.ToString();
                                rs.text = "Thành công";
                            }
                        }
                    }
                    else
                    {
                        rs.text = "Không có dữ liệu nghỉ phép để lưu thông tin";
                    }
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "AddNghiPhep");
                    str = "Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT để được hỗ trợ ";
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "0=0,42=1")]
        public async Task<ActionResult> Edit(long? id,int? op)
        {
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                return Content("Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại");
            }
            NghiPhep model = new NghiPhep();

            List<NghiPhepChiTiet> list = new List<NghiPhepChiTiet>();
            if (id != null)
            {
                using (var db = new SaveDB())
                {
                    db.GhiChu = "Thêm nghỉ phép";
                    var NP = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id && it.Del != true);
                    if (NP == null) {
                        return Content("Không tìn thấy thông tin nghỉ phép");
                    }
                    var NPCT = db.TTF_NghiPhepChiTiet.Where(it => it.IDNghiPhep == id && it.Del != true).ToList();

                    var NhanSu = (from nhansu in db.TTF_NhanSu
                                  join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                  where nhansu.NhanSu == NP.NhanSu
                                  select new { nhansu.MaNV, nhansu.HoVaTen, pb.TenPhong_PhanXuong, nhansu.SoNgayPhepConLai }).ToList();
                    if (NhanSu.Count == 0)
                    {
                        return Content("Lỗi không tìm thấy nhân sự");
                    }

                    model.MaNhanVien = NhanSu[0].MaNV;
                    model.NhanSu = NP.NhanSu;
                    model.HoVaTen = NhanSu[0].HoVaTen;
                    model.TenPhong_PhanXuong = NhanSu[0].TenPhong_PhanXuong;

                    model.TuNgay = NP.TuNgay;
                    model.DenNgay = NP.DenNgay;
                    model.SoNgayNghi = NP.SoNgayNghi;
                    model.IDNghiPhep = NP.IDNghiPhep;
                    model.MaLoaiNghiPhep = NP.MaLoaiNghiPhep;
                    model.MaTrangThaiDuyet = NP.MaTrangThaiDuyet;
                    model.LyDoNghi = NP.LyDoNghi;
                    model.SoNgayPhepDuocNghi = NhanSu[0].SoNgayPhepConLai;
                    model.Block = NP.Block;
                    model.NguoiTao = NP.NguoiTao;
                    model.IDNguoiDuyetKeTiep = NP.IDNguoiDuyetKeTiep;

                    foreach (TTF_NghiPhepChiTiet item in NPCT)
                    {
                        NghiPhepChiTiet add = new NghiPhepChiTiet();
                        add.ChuKyCaLamViec = item.ChuKyCaLamViec;
                        add.Ngay = item.Ngay;
                        add.NgayS = item.Ngay.ToString("dd/MM/yyyy");
                        add.IDNghiPhep = item.IDNghiPhep;
                        add.SoNgay = item.SoNgay;
                        add.Check = true;
                        DateTime temp = (DateTime)(item.Ngay);
                        if (temp.DayOfWeek == DayOfWeek.Saturday)
                            add.GhiChu = "Thứ 7";
                        list.Add(add);

                    }
                    model.NPCT = list;
                    ViewBag.Op = op;
                    return View(model);
                }
            }
            else
            {
                return Content("Chưa chọn thông tin nghỉ phép");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,42=3")]
        public async Task<JsonResult> SaveNghiPhep(NghiPhep item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            if (clsFunction.checkKyCongNguoiDung((DateTime)item.TuNgay))
            {
                rs.text = "Kỳ công đã đóng không thể sửa thông tin nghỉ phép";
                return Json(rs, JsonRequestBehavior.AllowGet);
                // return Content("Kỳ công đã đóng không thể sửa thông tin nghỉ phép");
            }

            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa nghỉ phép";
                if (item.NPCT != null && item.NPCT.Count > 0)
                {
                    try
                    {
                        var nghiphep = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == item.IDNghiPhep);
                        var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                        if (nghiphep.NguoiTao != nguoidung.NguoiDung)
                        {
                            rs.text = "Bạn không có quyền sửa thông tin nghỉ phép này";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }

                        if (nghiphep != null && nghiphep.Block != true)
                        {

                            nghiphep.LyDoNghi = item.LyDoNghi;
                            int irs = db.SaveChanges();

                            //irs = db.Database.ExecuteSqlCommand("Delete TTF_NghiPhepChiTiet Where IDNghiPhep ='" + item.IDNghiPhep + "'");
                            //if (irs > 0)
                            //{
                            //    foreach (NghiPhepChiTiet it in item.NPCT)
                            //    {
                            //        if (it.Check == null || it.Check == false)
                            //            continue;
                            //        TTF_NghiPhepChiTiet add1 = new TTF_NghiPhepChiTiet();
                            //        add1.ChuKyCaLamViec = it.ChuKyCaLamViec;
                            //        add1.Ngay = it.Ngay.Date;
                            //        if (it.ChuKyCaLamViec != null)
                            //            add1.SoNgay = 0.5;
                            //        else
                            //            add1.SoNgay = 1;
                            //        add1.IDNghiPhep = item.IDNghiPhep;
                            //        db.TTF_NghiPhepChiTiet.Add(add1);
                            //        db.SaveChanges();
                            //    }
                            //    str = "Cập nhật thông tin thành công";
                            //}

                            rs.text = "Thành công";
                            rs.code = 1;
                        }
                        else
                        {
                            rs.text = "Yêu cầu nghỉ phép đã gửi mail không thể sửa thông tin";
                        }

                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "SaveNghiPhep");
                        rs.text = "Đã có lỗi trên hệ thống không thể lưu. Hãy liên hệ phòng HTTT để được hỗ trợ ";
                    }
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            var db = new SaveDB();
            db.GhiChu = "Gửi mail nghỉ phép";
            try
            {
                //string rs = "";
                var item = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id);
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                if (item.NguoiTao != nguoidung.NguoiDung)
                {
                    rs.text = "Bạn không có quyền gửi mail duyệt nghỉ phép này";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                if (item.Del == true)
                {
                    rs.text = "Thông tin nghỉ phép đã xóa. Không thể gửi mail";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                if (clsFunction.checkKyCongNguoiDung((DateTime)item.TuNgay))
                {
                    rs.text = "Kỳ công đã đóng không thể gửi thông tin nghỉ phép";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                if (item.Block != true)
                {
                    var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == (int)item.NhanSu);
                    // kiem tra phòng nhân sự yêu cầu có tạo matrix chưa

                    var matrix = db.TTF_MaTrixDuyetNghiPhep.Where(it => it.MaPhong_PhanXuong.Trim() == nhansu.MaPhong_PhanXuong.Trim()).ToList();
                    if (matrix == null || matrix.Count == 0)
                    {
                        rs.text = "Chưa có thiết lập người duyệt nghỉ phép. Liên hệ phòng HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep(Convert.ToInt32(item.NhanSu), 0, "NP", 0);
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

                    var CC = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(item.NguoiTao));
                    var To = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.IDNguoiDuyetKeTiep);
                    string body = GetEmailDuyetString("NP", item, To);

                    ht = clsFunction.Get_HT_HETHONG();
                    clsFunction.GuiMail(ht.MailTitleNghiPhep, To.MailCongTy, CC.MailCongTy, body);

                    //if (clsFunction.GetDBName() == "TTF_FACEID")
                    //{
                    //    clsFunction.GuiMail("HRMS - Nghỉ phép", To.MailCongTy, CC.MailCongTy, body);
                    //}
                    //else
                    //{
                    //    clsFunction.GuiMail("Test hệ thống HRMS - Nghỉ phép", To.MailCongTy, CC.MailCongTy, body);
                    //}

                    int i = db.SaveChanges();
                    if (i > 0)
                    {

                        i = clsFunction.LuuLichSuDuyet(item.IDNghiPhep, "NP", false, (int)nguoiduyetketiep, DateTime.Now);
                        if (i > 0)
                        {
                            rs.text = "Thành công";
                            rs.code = 1;
                            rs.description = id.ToString();
                        }
                        else
                        {
                            rs.text = "Thất bại";
                        }
                    }
                   // rs.text = "Thành công";
                }
                else
                {
                    rs.text = "Thông tin này đã gửi mail không thể gửi lại";
                }

                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "GuiMail");
                rs.text = "Đã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        public string GetEmailDuyetString(string Loai, TTF_NghiPhep item, TTF_NhanSu NguoiDuyet)
        {
            string rv = "";
            var db = new TTF_FACEIDEntities();
            if (Loai == "NP")
            {
                var LyDo = db.TTF_LoaiNghiPhep.FirstOrDefault(it => it.MaLoaiNghiPhep == item.MaLoaiNghiPhep);
                string body = "";
                string NguoiNhan = "";
                string NguoiNghiPhep = "";
                if (NguoiDuyet != null)
                {
                    NguoiNhan = NguoiDuyet.HoVaTen;
                }
                //var vitem = db.v_TangCa.Where(it => it.IDNghiPhep == id).FirstOrDefault();
                var NguoiYeuCau = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                if (NguoiYeuCau != null)
                {
                    NguoiNghiPhep = NguoiYeuCau.HoVaTen;
                }
                var HeThong = db.HT_HETHONG.ToList();
                StringBuilder sb = new StringBuilder();
                sb.Append("<table width='100%' border='0'>");
                sb.Append("<tr>");
                sb.Append("<td>Anh/Chị " + NguoiNhan + " thân mến</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Bạn có yêu cầu nghỉ phép được gửi từ <b>" + NguoiNghiPhep + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td> Từ Ngày: <b>" + Convert.ToDateTime(item.TuNgay).Date.ToString("dd/MM/yyyy") + "</b> - Đến Ngày: <b>" + Convert.ToDateTime(item.DenNgay).Date.ToString("dd/MM/yyyy") + "</b> </td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Số ngày nghỉ: <b>" + item.SoNgayNghi.ToString() + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Loại phép: <b>" + LyDo.TenLoaiNghiPhep.ToString() + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Lý do: <b>" + item.LyDoNghi + "<b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Vui lòng truy cập vào hệ thống nghỉ phép để xem thông tin chi tiết hơn và xem xét duyệt yêu cầu này (Click vào link sau để vào hệ thống)</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>" + HeThong[0].WEBSITE + "/NghiPhep/DuyetPublic?NguoiDuyet=" + NguoiDuyet.MailCongTy.ToString().ToLower().Replace("@truongthanh.com", "") + "</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>&nbsp;</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>&nbsp;</td>");
                sb.Append("</tr>");

                //sb.Append("<tr>");
                //sb.Append("<td><a href='http://faceid.ttf.com.vn/NghiPhep/nghiphepchitiet?id=" + item.IDNghiPhep + "'>Đồng ý!</a></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Dear Mr./Ms. <b>" + NguoiNhan + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>You have an Leave request from <b>" + NguoiTao + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Date From: <b>" + Convert.ToDateTime(item.TuNgay).Date.ToString("dd/MM/yyyy") + "</b> - Date To <b>" + Convert.ToDateTime(item.DenNgay).Date.ToString("dd/MM/yyyy") + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Day Number: <b>" + item.SoNgayNghi.ToString() + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Kind of leaving:<b>" + LyDo.TenLoaiNghiPhep.ToString() + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Reason: <b>" + item.LyDoNghi + "</b></td>");
                //sb.Append("</tr>");
                //sb.Append("<tr>");
                //sb.Append("<td>Please visit Leave system to get the detail and consider for approval it (Leave system link)</td>");
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
            return rv;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,42=4")]
        public JsonResult Delete(long id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            // Delete
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.code = 0;
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var db = new SaveDB();
                db.GhiChu = "Xóa nghỉ phép";
                var model = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id);
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                if (model.NguoiTao != nguoidung.NguoiDung)
                {
                    rs.text = "Bạn không có quyền xóa nghỉ phép này";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                var ng = Users.GetNguoiDung(User.Identity.Name);
                if (clsFunction.checkKyCongNguoiDung((DateTime)model.TuNgay))
                {
                    rs.text = "Kỳ công đã đóng không thể xóa";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                    //return Json("Kỳ công đã đóng không thể thêm nghỉ phép", JsonRequestBehavior.AllowGet);
                }
                if (ng.NguoiDung < 0)
                {
                    rs.text = "Bạn đã hết thời gian thao tác trên web xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                    //return Json("Bạn đã hết thời gian thao tác trên web xin hãy đăng nhập lại", JsonRequestBehavior.AllowGet);
                }
                if (model != null)
                {
                    using (var tran = new TransactionScope())
                    {
                        model.Del = true;
                        model.NguoiThayDoiLanCuoi = (int)ng.NguoiDung;
                        model.NgayThayDoiLanCuoi = DateTime.Now;
                        model.Block = true;
                        model.NgayBlock = model.NgayThayDoiLanCuoi;
                        int iKq = db.SaveChanges();
                        if (iKq > 0)
                        {

                            db.Database.ExecuteSqlCommand("UPDATE dbo.TTF_NghiPhepChiTiet SET Del = 1 WHERE IDNghiPhep = '" + model.IDNghiPhep + "' ");
                            var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == model.NhanSu);

                            if (model.MaLoaiNghiPhep.Trim() == "P" || model.MaLoaiNghiPhep.Trim() == "P/2")
                            {
                                nhansu.SoNgayPhepConLai += model.SoNgayNghi;
                                db.SaveChanges();
                            }
                            if (model.MaLoaiNghiPhep.Trim() == "NB")
                            {
                                var NghiBu = db.TTF_NghiBuChiTiet.Where(it => it.IDNghiPhep == model.IDNghiPhep).ToList();
                                if (NghiBu != null && NghiBu.Count > 0)
                                {
                                    string sNghiBuChiTiet = "Update TTF_NghiBuChiTiet set Del = 1,NguoiDel = '" + ng.NguoiDung + "',NgayDel = GETDATE() Where IDNghiPhep = '" + model.IDNghiPhep + "' ";
                                    string sNghiBu = "";
                                    foreach (var item in NghiBu)
                                    {
                                        sNghiBu += "UPDATE dbo.TTF_NghiBu SET SoPhutConLai = SoPhutConLai + " + item.SoPhut + " WHERE MaNV = '" + nhansu.MaNV + "' AND IDTangCa = '" + item.IDTangCa + "' AND NgayPhatSinh = '" + item.NgayPhatSinh.ToString("MM/dd/yyyy") + "' \n";
                                    }
                                    if (sNghiBu != "" && sNghiBuChiTiet != "")
                                    {
                                        db.Database.ExecuteSqlCommand(sNghiBu);
                                        db.Database.ExecuteSqlCommand(sNghiBuChiTiet);
                                    }
                                }
                            }
                        }
                        tran.Complete();
                        rs.text = "Thành công";
                        rs.code = 1;
                    }
                }
                else
                {
                    rs.text = "Không tìm thấy dữ liệu cần xóa";
                    //str = "Không tìm thấy dữ liệu cần xóa";
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Delete");
                return Json("Đã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ", JsonRequestBehavior.AllowGet);
            }
        }

        [RoleAuthorize(Roles = "0=0,43=1")]
        public ActionResult DuyetNghiPhep()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,43=1")]
        public async Task<JsonResult> GetDuyetNghiPhep(string TuNgay, string DenNgay, string MaNV, string MaPhongBan) {
            var nguoidung = Users.GetNguoiDung(User.Identity.Name);
            TuNgay =  TuNgay == "" ? "01/01/2020" : TuNgay;
            DenNgay = DenNgay == "" ? DateTime.Now.ToString("MM/dd/yyyy") : DenNgay;
            using (var db = new SaveDB())
            {
                var model = db.Proc_GetDanhSachDuyetNghiPhep(nguoidung.NhanSu, TuNgay, DenNgay, MaNV, MaPhongBan).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "0=0,43=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Duyet_NghiPhepAll(List<int> lid)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db = new TTF_FACEIDEntities())
            {
                int iDem = 0; HT_HETHONG ht = null;
                List<GuiMail> listGuiMail = new List<GuiMail>();
                int idong = 1;
                if (lid != null && lid.Count > 0)
                {
                    try
                    {

                        using (var tran = new TransactionScope())
                        {
                            foreach (var id in lid)
                            {
                                var model = db.TTF_NghiPhep;
                                var ditem = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id);
                                if (clsFunction.checkKyCongNguoiDung((DateTime)ditem.TuNgay))
                                {
                                    rs.text = "Dòng thứ " + idong.ToString() + " có kỳ công đã đóng không thể duyệt nghỉ phép";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }

                                string body = "";
                                string To = "";
                                string CC = "";
                                try
                                {
                                    if (ditem.Block == true)
                                    {
                                        var ng = Users.GetNguoiDung(User.Identity.Name);
                                        int NhanSu = (int)ng.NhanSu;
                                        if (NhanSu == -1)
                                        {
                                            rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                                            return Json(rs, JsonRequestBehavior.AllowGet);
                                        }
                                        clsFunction.CapNhatDuyetVaoLichSuDuyet(ditem.IDNghiPhep, "NP", NhanSu, "");
                                        int? nguoiduyetketiep = clsFunction.LayCapDuyetKeTiep((int)ditem.NhanSu, (float)(ditem.SoNgayNghi), "NP", Convert.ToInt32(ditem.IDNghiPhep));
                                        var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                                        if (nguoiduyetketiep == -3)
                                        {
                                            rs.text = "không tìm thấy cấp duyệt tiếp theo. Liên hệ phòng HTTT để được hỗ trợ";
                                            return Json(rs, JsonRequestBehavior.AllowGet);
                                        }
                                        else if (nguoiduyetketiep == -1)
                                        {
                                            rs.text = "Nhân viên chưa cập nhật cấp quản lý trực tiếp không thể lưu";
                                            return Json(rs, JsonRequestBehavior.AllowGet);
                                        }
                                        else if (nguoiduyetketiep == -2) // Hoan thanh
                                        {

                                            //Cập nhật thạng thái hoàn tất duyệt
                                            ditem.MaTrangThaiDuyet = "3";
                                            ditem.IDNguoiDuyetKeTiep = -1;

                                            int temp = db.SaveChanges();
                                            if (temp > 0)
                                            {
                                                if (NguoiTao != null)
                                                {
                                                    To = NguoiTao.MailCongTy;
                                                }
                                                body = GetEmailDuyetHoanTatString("NP", ditem);

                                                ht = clsFunction.Get_HT_HETHONG();
                                                //clsFunction.GuiMail(ht.MailTitleNghiPhep, To, "", body);
                                                GuiMail a1 = new GuiMail();
                                                a1.MailTitle = ht.MailTitleNghiPhep;
                                                a1.To = To;
                                                a1.CC = "";
                                                a1.Body = body;
                                                listGuiMail.Add(a1);
                                                iDem++;
                                            }
                                        }
                                        else // con cap duyet 
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
                                            // gui mail cho cap duyet ke tiep
                                            body = GetEmailDuyetString("NP", ditem, NguoiDuyet);
                                            //if (clsFunction.GetDBName() == "TTF_FACEID")

                                            ht = clsFunction.Get_HT_HETHONG();
                                            //clsFunction.GuiMail(ht.MailTitleNghiPhep, To, "", body);
                                            clsFunction.LuuLichSuDuyet(ditem.IDNghiPhep, "NP", false, (int)nguoiduyetketiep, DateTime.Now);
                                            int i = db.SaveChanges();
                                            if (i > 0)
                                            {
                                                if (NguoiTao != null)
                                                {
                                                    To = NguoiTao.MailCongTy;
                                                }
                                                //body = GetEmailDuyetHoanTatString("NP", ditem);

                                                ht = clsFunction.Get_HT_HETHONG();
                                                //clsFunction.GuiMail(ht.MailTitleNghiPhep, To, "", body);
                                                GuiMail a1 = new GuiMail();
                                                a1.MailTitle = ht.MailTitleNghiPhep;
                                                a1.To = To;
                                                a1.CC = CC;
                                                a1.Body = body;
                                                listGuiMail.Add(a1);
                                                iDem++;

                                            }
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Duyet_NghiPhepAll");
                                    rs.text = ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                            }
                            tran.Complete();
                            if (iDem > 0)
                            {
                                if (listGuiMail.Count > 0)
                                {
                                    foreach (var item in listGuiMail)
                                    {
                                        clsFunction.GuiMail(item.MailTitle, item.To, item.CC, item.Body);
                                    }
                                }
                                rs.code = 1;
                                rs.text = "Duyệt thành công " + iDem.ToString() + " nhân sự xin nghỉ phép";

                            }
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Duyet_NghiPhepAll");
                        rs.text = ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                }
            }
             
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        public string GetEmailDuyetHoanTatString(string Loai, TTF_NghiPhep item)
        {
            string rv = "";
            if (Loai == "NP")
            {
                string body = "";
                string NguoiNghiPhep = "";
                var db = new SaveDB();
                var LyDo = db.TTF_LoaiNghiPhep.FirstOrDefault(it => it.MaLoaiNghiPhep == item.MaLoaiNghiPhep);
                var HeThong = db.HT_HETHONG.ToList();
                var NguoiYeuCau = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                if (NguoiYeuCau != null)
                {
                    NguoiNghiPhep = NguoiYeuCau.HoVaTen;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("<table width='100%' border='0'>");
                sb.Append("<tr>");
                sb.Append("<td>Anh/Chị " + NguoiNghiPhep + " thân mến</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Yêu cầu nghỉ phép bên dưới của anh/chị đã hoàn tất</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td> Từ Ngày: <b>" + Convert.ToDateTime(item.TuNgay).Date.ToString("dd/MM/yyyy") + "</b> - Đến Ngày: <b>" + Convert.ToDateTime(item.DenNgay).Date.ToString("dd/MM/yyyy") + "</b> </td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Số ngày nghỉ:<b>" + item.SoNgayNghi.ToString() + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Loại phép:<b>" + LyDo.TenLoaiNghiPhep.ToString() + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Lý do: <b>" + item.LyDoNghi + "<b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Vui lòng truy cập vào hệ thống nghỉ phép để xem thông tin chi tiết hơn (Click vào link sau để vào hệ thống)</td>");
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
        [RoleAuthorize(Roles = "0=0,43=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Mass_Cancel_NghiNghep(List<int> lid, string LyDo)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            List<GuiMail> listMail = new List<GuiMail>();
            using (TransactionScope tran = new TransactionScope())
            {
                string sNghiBuChiTiet = "", sNghiBu = "";
                int kq = 0;

                int idong = 0;
                var db = new SaveDB();
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NhanSu = (int)nguoidung.NguoiDung;
                if (NhanSu == -1)
                {
                    rs.text = "Tài khoản bạn đang nhập chưa có gán cho thông tin nhân viên không thể tạo";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                foreach (var id in lid)
                {
                    idong += 1;
                    sNghiBuChiTiet = sNghiBu = "";
                    var ditem = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id);
                    if (clsFunction.checkKyCongNguoiDung((DateTime)ditem.TuNgay))
                    {
                        return Json("Dòng thứ " + idong.ToString() + " có kỳ công đã đóng không thể duyệt nghỉ phép", JsonRequestBehavior.AllowGet);
                    }
                    var model = db.TTF_TangCa;

                    string body = "";
                    string To = "";
                    int iDNguoiDuyet = 0;
                    try
                    {
                        if (ditem.Block == true)
                        {
                           
                           

                            clsFunction.CapNhatTuChoiVaoLichSuDuyet(ditem.IDNghiPhep, "NP", NhanSu, LyDo);
                            iDNguoiDuyet = (int)ditem.IDNguoiDuyetKeTiep;
                            ditem.LyDoHuy = LyDo;
                            ditem.MaTrangThaiDuyet = "4";
                            ditem.IDNguoiDuyetKeTiep = -1;
                            ditem.NguoiTuChoi = NhanSu;
                            ditem.NgayTuChoi = DateTime.Now;

                            int i = db.SaveChanges();
                            if (i > 0)
                            {
                                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == ditem.NhanSu);
                                if (ditem.MaLoaiNghiPhep.Trim() == "P" || ditem.MaLoaiNghiPhep.Trim() == "P/2")//Cap nhat lai so ngay phep da tru
                                {
                                    nhansu.SoNgayPhepConLai = nhansu.SoNgayPhepConLai + ditem.SoNgayNghi;
                                    db.SaveChanges();
                                }
                                if (ditem.MaLoaiNghiPhep.Trim() == "NB")
                                {
                                    var NghiBu = db.TTF_NghiBuChiTiet.Where(it => it.IDNghiPhep == ditem.IDNghiPhep).ToList();
                                    if (NghiBu != null && NghiBu.Count > 0)
                                    {
                                        sNghiBuChiTiet = "Update TTF_NghiBuChiTiet set Del = 1,NguoiDel = '" + nguoidung.NguoiDung + "',NgayDel = GETDATE() Where IDNghiPhep = '" + ditem.IDNghiPhep + "' ";
                                        foreach (var obj in NghiBu)
                                        {
                                            sNghiBu += "UPDATE dbo.TTF_NghiBu SET SoPhutConLai = SoPhutConLai + " + obj.SoPhut + " WHERE MaNV = '" + nhansu.MaNV + "' AND IDTangCa = '" + obj.IDTangCa + "' AND NgayPhatSinh = '" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "' \n";
                                        }
                                    }
                                }
                                var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                                if (NguoiTao != null)
                                {
                                    To = NguoiTao.MailCongTy;
                                }

                                body = GetEmailTuChoiString("NP", ditem, iDNguoiDuyet);

                                HT_HETHONG ht = clsFunction.Get_HT_HETHONG();
                                GuiMail a1 = new GuiMail();
                                a1.MailTitle = ht.MailTitleNghiPhep;
                                a1.To = To;
                                a1.CC = "";
                                a1.Body = body;
                                listMail.Add(a1);
                                //clsFunction.GuiMail(ht.MailTitleNghiPhep, To, "", body);
                                if (i > 0)
                                {
                                    if (sNghiBu != "" && sNghiBuChiTiet != "")
                                    {
                                        db.Database.ExecuteSqlCommand(sNghiBu);
                                        db.Database.ExecuteSqlCommand(sNghiBuChiTiet);
                                    }
                                    kq++;
                                }

                            }
                        }
                        else
                        {
                            rs.text = "Yêu cầu này chưa được xác nhận hoàn tất từ người tạo";
                        }

                    }
                    catch (Exception ex)
                    {
                        clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Mass_Cancel_NghiNghep");
                        rs.text = ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
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
                    rs.text = "Từ chối thành công " + kq.ToString() + " yêu cầu nghỉ phép";
                    rs.code = 1;
                }

                return Json(rs, JsonRequestBehavior.AllowGet);

            }
        }
        public string GetEmailTuChoiString(string Loai, TTF_NghiPhep vitem, int iDNguoiDuyet)
        {
            using (var db = new SaveDB())
            {
                var NguoiNghiPhep = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == vitem.NhanSu);
                var ThongTinNguoiDuyet = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == iDNguoiDuyet);
                var HeThong = db.HT_HETHONG.ToList();
                string rv = "";
                if (Loai == "NP")
                {
                    string body = "";
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table width='100%' border='0'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Anh/Chị " + NguoiNghiPhep.HoVaTen + " thân mến</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Yêu cầu nghỉ phép bên dưới của anh/chị đã bị từ chối bởi <b>" + ThongTinNguoiDuyet.HoVaTen + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do từ chối: <b>" + vitem.LyDoHuy + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Nội dung yêu cầu nghỉ phép</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Từ Ngày: <b>" + Convert.ToDateTime(vitem.TuNgay).Date.ToString("dd/MM/yyyy") + "</b> Đến ngày: <b>" + Convert.ToDateTime(vitem.DenNgay).Date.ToString("dd/MM/yyyy") + " </b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Số ngày nghỉ:<b>" + vitem.SoNgayNghi + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Lý do: <b>" + vitem.LyDoNghi + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Vui lòng truy cập vào hệ thống nghỉ phép để xem thông tin chi tiết hơn (Click vào link sau để vào hệ thống)</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>" + HeThong[0].WEBSITE + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Đây là email tự động từ hệ thống - vui lòng không phản hồi.</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>&nbsp;</td>");
                    sb.Append("</tr>");

                    sb.Append("</table>");
                    body = sb.ToString();
                    db.Dispose();
                    rv = body;
                }
                return rv;
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
                return RedirectToAction("DuyetNghiPhep", "NghiPhep");
            }
        }
        [RoleAuthorize(Roles = "0=0,44=1")]
        public ActionResult QuanLyNghiPhep() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> GetQuanLyNghiPhep(string TuNgay,string DenNgay,string MaNV, string MaPhongBan ,string MaLoaiNghiPhep) {
            using (var db = new SaveDB())
            {
                if (TuNgay == null || TuNgay == "")
                {
                    TuNgay = "1990-01-01";
                }
                if (DenNgay == null || DenNgay == "")
                {
                    DenNgay = DateTime.Now.ToString("yyyy-MM-dd");
                }
                var model = db.Proc_QuanLyNghiPhep(TuNgay, DenNgay, MaNV, MaPhongBan, MaLoaiNghiPhep).ToList();
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            }

        }

        [RoleAuthorize(Roles = "0=0,44=1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveNghiPhepNhanSu(NghiPhep item) {

            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs,JsonRequestBehavior.AllowGet);
            }
            using (var db = new SaveDB())
            {
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NguoiSua = (int)nguoidung.NguoiDung;
                if (clsFunction.checkKyCongNhanSu((DateTime)item.TuNgay.Value))
                {
                    rs.text = "Kỳ công đã đóng không thể cập nhật thông tin nghỉ phép";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                var nghiphep = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == item.IDNghiPhep);
                var nghiphepchitiet = db.TTF_NghiPhepChiTiet.Where(it => it.IDNghiPhep == item.IDNghiPhep).ToList();

                double dNgayPhep = 0, dNgayPhepNS = 0; string sCapNhat = "";
                for (int i = 0; i < item.NPCT.Count; i++)
                {
                    if (item.NPCT[i].ChuKyCaLamViec != null && item.NPCT[i].ChuKyCaLamViec > 0)
                        dNgayPhep = dNgayPhep + 0.5;
                    else
                    {
                        dNgayPhep = dNgayPhep + 1;
                    }
                    if (item.NPCT[i].ChuKyCaLamViec == null || item.NPCT[i].ChuKyCaLamViec == 0)
                    {
                        sCapNhat += "UPDATE dbo.TTF_NghiPhepChiTiet SET ChuKyCaLamViec = NULL,SoNgay = 1 WHERE IDNghiPhep = '" + item.IDNghiPhep + "' And Ngay = '" + (DateTime.ParseExact(item.NPCT[i].NgayS, "dd/MM/yyyy", new CultureInfo("en-US"))).ToString("yyyy-MM-dd")+ "' \n";
                    }
                    else
                        sCapNhat += "UPDATE dbo.TTF_NghiPhepChiTiet SET ChuKyCaLamViec = '" + item.NPCT[i].ChuKyCaLamViec + "',SoNgay = 0.5 WHERE IDNghiPhep = '" + item.IDNghiPhep + "' And Ngay = '" + (DateTime.ParseExact(item.NPCT[i].NgayS, "dd/MM/yyyy", new CultureInfo("en-US"))).ToString("yyyy-MM-dd") + "' \n";
                }
                if (dNgayPhep % 1 > 0 && item.MaLoaiNghiPhep != "P" && item.MaLoaiNghiPhep != "Ro")
                {
                    rs.text = "Loại phép bạn chọn không được nghỉ 0.5 ngày";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                var NhanSu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                bool bNhanSu = false;
                //dang la phep ko huong luong thanh phep huong luong
                using (var tran = new TransactionScope())
                {
                    if (nghiphep.MaLoaiNghiPhep.Trim() == "P" && item.MaLoaiNghiPhep != "P")
                    {
                        //Chuyển loai tu nghỉ phép hưởng lương sang phép không hưởng lương
                        dNgayPhepNS = (double)NhanSu.SoNgayPhepConLai + (double)nghiphepchitiet.Sum(it => it.SoNgay);
                        bNhanSu = true;
                        // db.SaveChanges();
                    }
                    else if (nghiphep.MaLoaiNghiPhep.Trim() != "P" && item.MaLoaiNghiPhep.Trim() == "P")
                    {
                        // chuyển từ phép không hưởng lương sang phép hưởng lương
                        if (dNgayPhep > NhanSu.SoNgayPhepConLai)
                        {
                            rs.text = "Số ngày phép nghỉ vượt quá số ngày phép còn lại";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                            //return Content("Số ngày phép nghỉ vượt quá số ngày phép còn lại");
                        }

                        dNgayPhepNS = (double)NhanSu.SoNgayPhepConLai - dNgayPhep;
                        bNhanSu = true;
                    }
                    else if (nghiphep.MaLoaiNghiPhep.Trim() == "P" && item.MaLoaiNghiPhep.Trim() == "P" && nghiphep.SoNgayNghi != dNgayPhep)
                    {
                        dNgayPhepNS = (double)NhanSu.SoNgayPhepConLai + ((double)nghiphepchitiet.Sum(it => it.SoNgay) - dNgayPhep);
                        bNhanSu = true;
                    }


                    sCapNhat += "UPDATE dbo.TTF_NghiPhep SET MaLoaiNghiPhep ='" + item.MaLoaiNghiPhep + "',SoNgayNghi = '" + dNgayPhep.ToString().Replace(",", ".") + "',NguoiThayDoiLanCuoi = '" + NguoiSua + "', NgayThayDoiLanCuoi = GETDATE() WHERE IDNghiPhep = '" + nghiphep.IDNghiPhep + "' \n";
                    if (bNhanSu)
                    {
                        sCapNhat += "UPDATE dbo.TTF_NhanSu SET SoNgayPhepConLai = '" + dNgayPhepNS.ToString().Replace(",", ".") + "' WHERE NhanSu = '" + NhanSu.NhanSu + "' ";
                    }
                    int irs = db.Database.ExecuteSqlCommand(sCapNhat);
                    if (irs > 0)
                    {
                        tran.Complete();

                        rs.text = "Thành công";
                        rs.code = 1;
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        rs.text = "Không có dữ liệu thay đổi để cập nhật";
                        rs.code = 1;
                        return Json(rs, JsonRequestBehavior.AllowGet);
                        //return Content("Không có dữ liệu thay đổi để cập nhật");
                    }
                }
            }
            //return Json(rs, JsonRequestBehavior.AllowGet);
        }
        
        [RoleAuthorize(Roles = "0=0,44=1")]
        public async Task<JsonResult> TuChoiNghiPhepNhanSu(NghiPhep item)
        {
            using (var db = new SaveDB()) {
                db.GhiChu = "Nhân sự từ chối phép";
                JsonStatus rs = new JsonStatus();
                rs.code = 0;
                var ditem = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == item.IDNghiPhep);
                string body = "";
                string To = "";
                string sNghiBuChiTiet = "", sNghiBu = "";
                string MaTrangThaiDuyet = ditem.MaTrangThaiDuyet.Trim();
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs,JsonRequestBehavior.AllowGet);
                }
                if (clsFunction.checkKyCongNhanSu((DateTime)item.TuNgay))
                {
                    rs.text = "Kỳ công đã đóng không thể hủy nghỉ phép";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                    //return Content("Kỳ công đã đóng không thể hủy nghỉ phép");
                }
                try
                {
            
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    if (nguoidung == null)
                    {
                        rs.text = "Tài khoản bạn đăng nhập chưa có gán cho thông tin nhân viên không thể từ chối";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                        //return Content("Tài khoản bạn đăng nhập chưa có gán cho thông tin nhân viên không thể từ chối"); ;
                    }

                    clsFunction.CapNhatTuChoiVaoLichSuDuyet(item.IDNghiPhep, "NP", nguoidung.NhanSu, item.LyDoHuy);
                    ditem.LyDoHuy = item.LyDoHuy;
                    ditem.MaTrangThaiDuyet = "4";
                    ditem.IDNguoiDuyetKeTiep = -1;
                    ditem.NguoiTuChoi = (int)nguoidung.NguoiDung;
                    ditem.NgayTuChoi = DateTime.Now;
                    var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == ditem.NhanSu);
                    if (ditem.MaLoaiNghiPhep.Trim() == "P" && MaTrangThaiDuyet != "4")// Cap nhat lai so ngay phep nghi
                    {
                        nhansu.SoNgayPhepConLai += +ditem.SoNgayNghi;
                    }
                    //if (ditem.MaLoaiNghiPhep.Trim() == "NB")
                    //{

                    //    var NghiBu = db.TTF_NghiBuChiTiet.Where(it => it.IDNghiPhep == ditem.IDNghiPhep).ToList();
                    //    if (NghiBu != null && NghiBu.Count > 0)
                    //    {
                    //        sNghiBuChiTiet = "Update TTF_NghiBuChiTiet set Del = 1,NguoiDel = '" + nguoidung.NguoiDung+ "',NgayDel = GETDATE() Where IDNghiPhep = '" + ditem.IDNghiPhep + "' ";
                    //        foreach (var obj in NghiBu)
                    //        {
                    //            sNghiBu += "UPDATE dbo.TTF_NghiBu SET SoPhutConLai = SoPhutConLai + " + obj.SoPhut + " WHERE MaNV = '" + nhansu.MaNV + "' AND IDTangCa = '" + obj.IDTangCa + "' AND NgayPhatSinh = '" + obj.NgayPhatSinh.ToString("MM/dd/yyyy") + "' \n";
                    //        }
                    //    }
                    //}
                    var NguoiTao = clsFunction.LayThongTinNhanSuNguoiTao(Convert.ToInt32(ditem.NguoiTao));
                    if (NguoiTao != null)
                    {
                        To = NguoiTao.MailCongTy;
                    }

                    body = GetEmailTuChoiString("NP", ditem, nguoidung.NhanSu);

                    HT_HETHONG ht = clsFunction.Get_HT_HETHONG();


                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //if (sNghiBu != "" && sNghiBuChiTiet != "")
                        //{
                        //    db.Database.ExecuteSqlCommand(sNghiBu);
                        //    db.Database.ExecuteSqlCommand(sNghiBuChiTiet);
                        //}
                        clsFunction.GuiMail(ht.MailTitleNghiPhep, To, "", body);
                        rs.text = "Thành công";
                        rs.code = 1;
                    }
                    else
                    {
                        rs.text = "Thất bại";
                    }

                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "TuChoiNghiPhepNhanSu");
                    rs.text = ex.Message + "\r\nĐã có lỗi trong quá trình gửi mail. Liên hệ phòng HTTT để được hỗ trợ";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles ="0=0,44=1")]
        public JsonResult XoaNghiPhepChiTiet(string dk)
        {
            using (var db = new SaveDB()) {
                db.GhiChu = "Nhân sự xóa nghỉ phép chi tiết";
                var rs = new JsonStatus();
                rs.code = 0;
                if (User.Identity.Name == null || User.Identity.Name == "")
                { rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                string[] sArry = dk.Split('_');
                long id = Convert.ToInt64(sArry[0].Trim().Replace("'", ""));
                DateTime ngay = DateTime.ParseExact(sArry[1].Trim().Replace("'", "").ToString(), "dd/MM/yyyy", new CultureInfo("en-US"));
                string MaPhep = sArry[2].Trim().Replace("'", "");
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
               // int NhanSuXoa = (int)clsUserActivesInternal.clsUserActive.GetNhanSu(User.Identity.Name);
                var phepchitiet = db.TTF_NghiPhepChiTiet.FirstOrDefault(it => it.IDNghiPhep == id && it.Ngay == ngay.Date && it.Del != true);

                if (phepchitiet != null)
                {
                    if (clsFunction.checkKyCongNhanSu(phepchitiet.Ngay))
                    {
                        rs.text = "Kỳ công đã đóng"; //
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    double dsoNgay = (double)phepchitiet.SoNgay;
                    phepchitiet.Del = true;
                    phepchitiet.NguoiDel = nguoidung.NhanSu;
                    phepchitiet.NgayDel = DateTime.Now;

                    var NP = db.TTF_NghiPhep.FirstOrDefault(it => it.IDNghiPhep == id);
                    if (NP != null)
                    {
                        NP.SoNgayNghi = NP.SoNgayNghi - dsoNgay;
                        var NhanSu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NP.NhanSu);
                        if (MaPhep.Trim() == "P" || MaPhep.Trim() == "P/2")
                        {
                            NhanSu.SoNgayPhepConLai = NhanSu.SoNgayPhepConLai + dsoNgay;
                        }
                    }

                    int irs = db.SaveChanges();
                    if (irs > 0)
                    {
                        rs.text = "Thành công";//thành công
                        rs.code = 1;
                    }
                    else
                        rs.text = "Thất bại"; // thất bại
                }
                else
                    rs.text = "Thất bại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles ="0=0,44-1")]
        public async Task<JsonResult> ImportExcelUploadNghiPhep(HttpPostedFileBase FileInbox, string TuNgay, string DenNgay, string LyDo)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs,JsonRequestBehavior.AllowGet);
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
                using (var db = new SaveDB())
                {
                    try
                    {
                        var NhanSu = db.TTF_NhanSu.Where(it => it.Del != true).ToList();
                        FileInbox.SaveAs(resultFilePath);
                        var NghiPhep = getDataTableFromExcel(resultFilePath, NhanSu, DateTime.Parse(TuNgay), DateTime.Parse(DenNgay));
                        if (NghiPhep.Loi.Length > 0)
                        {
                            rs.text = MvcHtmlString.Create(NghiPhep.Loi).ToHtmlString();
                            return Json(rs, JsonRequestBehavior.AllowGet);
                            //return Content(MvcHtmlString.Create(NghiPhep.Loi).ToHtmlString());
                        }
                        if (NghiPhep.ImportNgayPhep.Count > 0)
                        {
                            var NhanSuTemp = NghiPhep.ImportNgayPhep.Select(it => it.NhanSu).Distinct().ToList();
                            if (NhanSuTemp.Count > 0)
                            {
                                double SoNgayNghi = 0;
                                int iSTT = 0;
                                const TransactionScopeOption opt = new TransactionScopeOption();
                                TimeSpan span = new TimeSpan(0, 0, 30, 30);
                                using (var tran = new TransactionScope(opt, span))
                                {
                                    //  db.Database.CommandTimeout = 3600;
                                    foreach (var item in NhanSuTemp)
                                    {
                                        iNhanSu = item;
                                        var LoaiPhepNghi = NghiPhep.ImportNgayPhep.Where(it => it.NhanSu == item).Select(it => it.MaLoaiNghiPhep).Distinct().ToList();

                                        if (LoaiPhepNghi.Count > 0)
                                        {
                                            foreach (var lp in LoaiPhepNghi)
                                            {

                                                DateTime dTuNgay = new DateTime(), dDenNgay = new DateTime();
                                                var NgayNghi = NghiPhep.ImportNgayPhep.Where(it => it.NhanSu == item && it.MaLoaiNghiPhep == lp).ToList();

                                                SoNgayNghi = NghiPhep.ImportNgayPhep.Where(it => it.NhanSu == item && it.MaLoaiNghiPhep == lp).Sum(it => it.SoNgay);
                                                dTuNgay = NghiPhep.ImportNgayPhep.Where(it => it.NhanSu == item && it.MaLoaiNghiPhep == lp).Select(it => it.Ngay).Min();
                                                dDenNgay = NghiPhep.ImportNgayPhep.Where(it => it.NhanSu == item && it.MaLoaiNghiPhep == lp).Select(it => it.Ngay).Max();

                                                TTF_NghiPhep add = new TTF_NghiPhep();
                                                add.Block = true;
                                                add.MaLoaiNghiPhep = getMaNghiPhep(lp);
                                                add.NhanSu = item;
                                                add.LyDoNghi = LyDo;
                                                add.MaTrangThaiDuyet = "3";
                                                add.SoNgayNghi = SoNgayNghi;
                                                add.IDNguoiDuyetKeTiep = -1;
                                                add.TuNgay = dTuNgay;
                                                add.DenNgay = dDenNgay;
                                                add.NguoiTao = NguoiTao;
                                                add.NgayTao = DateTime.Now;
                                                add.MayTao = System.Net.Dns.GetHostName();
                                                add.NgayBlock = DateTime.Now;
                                                db.TTF_NghiPhep.Add(add);
                                                int sr = db.SaveChanges();
                                                if (sr > 0)
                                                {
                                                    iSTT++;
                                                    //var nghiphepTemp = db.TTF_NghiPhep.FirstOrDefault(it => it.NhanSu == item && it.TuNgay == dTuNgay && it.DenNgay == dDenNgay && it.MaLoaiNghiPhep == lp && it.MaTrangThaiDuyet !="4");
                                                    foreach (var np in NgayNghi)
                                                    {
                                                        TTF_NghiPhepChiTiet npChiTiet = new TTF_NghiPhepChiTiet();

                                                        npChiTiet.Ngay = np.Ngay;
                                                        npChiTiet.SoNgay = np.SoNgay;
                                                        if (np.SoNgay == 0.5)
                                                        {
                                                            npChiTiet.ChuKyCaLamViec = 1;
                                                        }
                                                        npChiTiet.IDNghiPhep = add.IDNghiPhep;
                                                        db.TTF_NghiPhepChiTiet.Add(npChiTiet);
                                                    }
                                                    if (lp == "P")
                                                    {
                                                        var CapNhatPhepNhanSu = NhanSu.FirstOrDefault(it => it.NhanSu == item);
                                                        CapNhatPhepNhanSu.SoNgayPhepConLai = CapNhatPhepNhanSu.SoNgayPhepConLai - SoNgayNghi;
                                                        // CapNhatPhepNhanSu.GhiChu = CapNhatPhepNhanSu.SoNgayPhepConLai - SoNgayNghi < 0 ? "Mượn phép 9 ngày" : "";
                                                    }
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                    tran.Complete();
                                    rs.code = 1;
                                    rs.text = "Thành công";
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                rs.text = "Không tìm thấy thông tin nhân sự import";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            rs.text = "Không tìm thấy thông tin nhân sự import";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        rs.text = "Lỗi hệ thống ở nhân sự " + iNhanSu.ToString() + ex.ToString();
                        return Json(rs, JsonRequestBehavior.AllowGet);

                    }
                }

            }
            else
            {
                rs.text = "Không tìm thấy file dữ liệu import";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            
        }
        public ImportNgayPhepNhanh getDataTableFromExcel(string path, List<TTF_NhanSu> NhanSu, DateTime TuNgay, DateTime DenNgay)
        {
            using (var db = new SaveDB()) {
                var existingFile = new FileInfo(path);
                using (var pck = new OfficeOpenXml.ExcelPackage(existingFile))
                {
                    List<ImportNgayPhep> tbl = new List<ImportNgayPhep>();
                    var ws = pck.Workbook.Worksheets.First();
                    var startRow = 2;

                    string sLoi = ""; double dSoNgayPhepNghi = 0;
                    var LoaiPhep = db.TTF_LoaiNghiPhep.ToList();
                    int iRowEnd = (int)(DenNgay - TuNgay).TotalDays + 6;
                    Double dSoNgayPhepConLai = 0;
                    var listDanhSachNghiPhep = (from np in db.TTF_NghiPhep
                                                join npct in db.TTF_NghiPhepChiTiet on np.IDNghiPhep equals npct.IDNghiPhep
                                                where npct.Ngay >= TuNgay && npct.Ngay <= DenNgay && np.Del != true && np.MaTrangThaiDuyet != "4" && npct.Del != true
                                                select new { np.NhanSu, npct.Ngay }).ToList();
                    string MaNV, sPhep, sLoaiPhep; DateTime dtTemp;
                    for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        MaNV = ws.Cells[rowNum, 4].Text.Trim(); sPhep = ""; ;
                        if (MaNV == "")
                            break;
                        var checkNhanSu = NhanSu.FirstOrDefault(it => it.MaNV == MaNV);

                        if (checkNhanSu == null)
                        {
                            sLoi += "Không tìm thấy thông tin nhân viên: <b>" + MaNV + " " + ws.Cells[rowNum, 5].Text.Trim() + "</b><br>";
                            continue;
                        }
                        else
                        {
                            dtTemp = TuNgay; sLoaiPhep = ""; dSoNgayPhepNghi = 0;
                            for (int i = 6; i <= iRowEnd; i++)
                            {
                                sLoaiPhep = ws.Cells[rowNum, i].Text.Trim().ToUpper();

                                if (dtTemp > DenNgay)
                                    break;

                                if (sLoaiPhep == @"P\2")
                                    sLoaiPhep = "P/2";

                                //if (sLoaiPhep == "" || sLoaiPhep == "L")
                                //{
                                //    dtTemp = dtTemp.AddDays(1);
                                //    continue;
                                //}
                                if (sLoaiPhep == "")
                                {
                                    dtTemp = dtTemp.AddDays(1);
                                    continue;
                                }
                                else
                                {
                                    ImportNgayPhep add = new ImportNgayPhep();
                                    if (sLoaiPhep == "P/2")
                                    {
                                        dSoNgayPhepNghi += 0.5;
                                        add.SoNgay = 0.5;
                                        sPhep = sLoaiPhep = "P";

                                    }
                                    else if (sLoaiPhep == "P")
                                    {
                                        sPhep = "P";
                                        dSoNgayPhepNghi += 1;
                                        add.SoNgay = 1;
                                    }
                                    else
                                    {
                                        add.SoNgay = 1;
                                    }

                                    var testLoaiPhep = LoaiPhep.FirstOrDefault(it => it.MaLoaiNghiPhep.ToUpper().Trim() == sLoaiPhep);
                                    if (testLoaiPhep == null)
                                    {
                                        sLoi += "Không tìm thấy mã loại phép " + sLoaiPhep + "Của nhân viên <b>" + MaNV + " " + ws.Cells[rowNum, 4].Text.Trim() + dtTemp.ToString("dd/MM/yyyy") + "</b><br>";
                                        dtTemp = dtTemp.AddDays(1);
                                        continue;
                                    }
                                    else
                                    {
                                        var checkTonTai = listDanhSachNghiPhep.Where(it => it.NhanSu == checkNhanSu.NhanSu && it.Ngay == dtTemp).ToList();
                                        if (checkTonTai.Count > 0)
                                        {
                                            sLoi += "Đã tồn tại phép Của nhân viên <b>" + MaNV + " ngày " + dtTemp.ToString("dd/MM/yyyy") + "</b><br>";
                                        }
                                        add.MaLoaiNghiPhep = sLoaiPhep;
                                        add.NhanSu = checkNhanSu.NhanSu;
                                        add.Ngay = dtTemp;
                                        add.SoNgayNghi = dSoNgayPhepNghi;
                                        tbl.Add(add);
                                        dtTemp = dtTemp.AddDays(1);
                                    }
                                }
                            }
                            dSoNgayPhepConLai = checkNhanSu.SoNgayPhepConLai == null ? 0 : checkNhanSu.SoNgayPhepConLai.Value;

                            if (dSoNgayPhepNghi > dSoNgayPhepConLai && sPhep == "P")
                            {
                                sLoi += " Số ngày phép nghỉ (" + dSoNgayPhepNghi.ToString() + ") vượt quá số ngày phép còn lại (" + checkNhanSu.SoNgayPhepConLai.ToString() + ") " + "Của nhân viên <b>" + MaNV + " " + ws.Cells[rowNum, 5].Text.Trim() + "</b><br>";
                                //dtTemp = dtTemp.AddDays(1);
                                //continue;
                            }
                        }
                    }
                    ImportNgayPhepNhanh List = new ImportNgayPhepNhanh();
                    List.ImportNgayPhep = tbl;
                    List.Loi = sLoi;
                    return List;
                }
            }
        }
        public string getMaNghiPhep(string Chuoi)
        {
            string sk = "";
            switch (Chuoi)
            {
                case "B": sk = "B"; break;
                case "NB": sk = "NB"; break;
                case "P": sk = "P"; break;
                case "R": sk = "R"; break;
                case "RN": sk = "Rn"; break;
                case "RO": sk = "Ro"; break;
                case "TN": sk = "TN"; break;
                case "TS": sk = "TS"; break;
                case "L": sk = "L"; break;
            }
            return sk;
        }
    }
    public class BuoiLamViec
    {
        public int? ChuKyCaLamViec { get; set; }
        public string TenBuoiLamViec { get; set; }
    }
    public class ImportNgayPhep
    {
        public double SoNgayNghi { get; set; }
        public int NhanSu { get; set; }
        public string MaLoaiNghiPhep { get; set; }
        public double SoNgay { get; set; }
        public DateTime Ngay { get; set; }
    }
    public class ImportNgayPhepNhanh
    {
        public List<ImportNgayPhep> ImportNgayPhep { get; set; }
        public string Loi { get; set; }
    }
}