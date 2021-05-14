using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Class;
using TTFPortal.Models;

namespace TTFPortal.Controllers
{
    public class HeThongController : Controller
    {
        // GET: HeThong
        public ActionResult Index()
        {
            return View();
        }
        public async Task<JsonResult> TimNhanVien(string MaNV)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
               
                JsonStatus js = new JsonStatus();
                js.code = 0;
              
                try
                {
                    var ng = Users.GetNguoiDung(User.Identity.Name);
                    if (User.IsInRole("44=1"))
                    {
                        js.data = (from nhansu in db.TTF_NhanSu
                                   join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                   join calamviec in db.TTF_CaLamViec on nhansu.MaCaLamViec equals calamviec.MaCaLamViec
                                   where nhansu.MaNV == MaNV
                                         && nhansu.MaTinhTrang == "1"
                                   select new NhanVien { MaNV = nhansu.MaNV, HoVaTen = nhansu.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, NhanSu = nhansu.NhanSu, SoNgayPhepConlai = nhansu.SoNgayPhepConLai, GioVao = calamviec.GioBacDau, GioRa = calamviec.GioKetThuc }).First();
                        js.code =   1;
                    }
                    else
                    {
                        List<string> PhamVi = new List<string>();
                        PhamVi = db.TTF_PhamVi.Where(it => it.NhanSu == ng.NhanSu).Select(it => it.MaPhong_PhanXuong).ToList();
                        PhamVi.Add(ng.MaPhongPhanXuong);
                        js.data = (from nhansu in db.TTF_NhanSu
                                   join pb in db.TTF_PhongBan_PhanXuong on nhansu.MaPhong_PhanXuong.Trim() equals pb.MaPhong_PhanXuong.Trim()
                                   join calamviec in db.TTF_CaLamViec on nhansu.MaCaLamViec equals calamviec.MaCaLamViec
                                   where nhansu.MaNV == MaNV
                                        && PhamVi.Contains(nhansu.MaPhong_PhanXuong.Trim()) && nhansu.MaTinhTrang == "1"
                                   select new NhanVien { MaNV = nhansu.MaNV, HoVaTen = nhansu.HoVaTen, TenPhongBan = pb.TenPhong_PhanXuong, NhanSu = nhansu.NhanSu, SoNgayPhepConlai = nhansu.SoNgayPhepConLai, GioVao = calamviec.GioBacDau , GioRa = calamviec.GioKetThuc }).First();
                        js.code = 1;
                    }
                }
                catch (Exception ex)
                {
                    js.text = "Không tìm thấy nhân sự " + ex.Message;
                    js.code = 0;
                }
                return Json(js, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public ActionResult MatrixNghiPhep()
        {
            return View();
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public async Task<JsonResult> GetMatrixNghiPhep(string maPhongBan,int? tuNgay,int? denNgay)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_MaTrixNghiPhep(maPhongBan, tuNgay, denNgay).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> AddMatrixNghiPhep(Proc_MaTrixNghiPhep_Result item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new TTF_FACEIDEntities())
            {
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs,JsonRequestBehavior.AllowGet);
                }
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NguoiDung1 = (int)nguoidung.NguoiDung;
                if (item.CapDuyet == 1 && item.NguoiDuyet.ToString().Trim().ToLower() != "head")
                {
                    rs.text = "Cấp duyệt số 1 phải là Head";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }

                var checkMaTrix = db.TTF_MaTrixDuyetNghiPhep.Where(it => it.MaPhong_PhanXuong.Trim() == item.MaPhong_PhanXuong.Trim() && it.TuNgay == item.TuNgay && it.DenNgay == item.DenNgay).ToList();
                if (checkMaTrix != null && checkMaTrix.Count > 0)
                {
                    var check = checkMaTrix.Where(it => it.CapDuyet == item.CapDuyet).ToList();
                    if (check != null && check.Count > 0)
                    {
                        rs.text= "Đã tồn tại cấp duyệt số " + item.CapDuyet.ToString();
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                    check = checkMaTrix.Where(it => it.NguoiDuyet.Trim().ToLower() == item.NguoiDuyet.Trim().ToLower()).ToList();
                    if (check != null && check.Count > 0)
                    {
                        rs.text = "Người duyệt " + item.NguoiDuyet.ToString() + " đã tồn tại ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
                if (item.NguoiDuyet.ToLower().Trim() != "pm" && item.NguoiDuyet.ToLower().Trim() != "head")
                {
                    var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV == item.NguoiDuyet.Trim() && it.Del != true && it.MaTinhTrang == "1");
                    if (nhansu == null)
                    {
                        rs.text = "Mã nhân viên " + item.NguoiDuyet.ToString().ToUpper() + " không tồn tại";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                }
                try
                {
                    TTF_MaTrixDuyetNghiPhep add = new TTF_MaTrixDuyetNghiPhep();
                    add.TuNgay = item.TuNgay;
                    add.DenNgay = item.DenNgay;
                    add.CapDuyet = item.CapDuyet;
                    add.GhiChu = item.GhiChu;
                    add.NguoiDuyet = item.NguoiDuyet;
                    add.LoaiQuyTrinh = "NP";
                    add.MaPhong_PhanXuong = item.MaPhong_PhanXuong.Trim();
                    add.NgayTao = DateTime.Now;
                    add.NguoiTao = NguoiDung1;
                    db.TTF_MaTrixDuyetNghiPhep.Add(add);
                    db.SaveChanges();
                    rs.text = ("Thành công");
                    rs.code = 1;
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    rs.text = ex.ToString();
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Add_MaTrixNghiPhep");
                    rs.code = 0;
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> SaveMatrixNghiPhep(Proc_MaTrixNghiPhep_Result item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa cấp duyệt nghỉ phép";
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NguoiDung1 = (int)nguoidung.NguoiDung;
                    var model = db.TTF_MaTrixDuyetNghiPhep.FirstOrDefault(it => it.ID == item.ID);

                    if (model != null)
                    {
                        if (item.CapDuyet == 1 && item.NguoiDuyet.ToString().Trim().ToLower() != "head")
                        {
                            rs.text = "Cấp duyệt số 1 phải là Head";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        var checkMaTrix = db.TTF_MaTrixDuyetNghiPhep.Where(it => it.MaPhong_PhanXuong.Trim() == item.MaPhong_PhanXuong.Trim() && it.TuNgay == item.TuNgay && it.DenNgay == item.DenNgay).ToList();
                        if (checkMaTrix != null && checkMaTrix.Count > 0)
                        {

                            var check = checkMaTrix.Where(it => it.CapDuyet == item.CapDuyet && it.ID != item.ID).ToList();
                            if (check != null && check.Count > 0)
                            {
                                rs.text = "Cấp duyệt " + item.CapDuyet.ToString() + " đã tồn tại ";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                            check = checkMaTrix.Where(it => it.NguoiDuyet.Trim().ToLower() == item.NguoiDuyet.Trim().ToLower() && it.ID != item.ID).ToList();
                            if (check != null && check.Count > 0)
                            {
                                rs.text = "Người duyệt " + item.NguoiDuyet.ToString() + " đã tồn tại ";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                        }
                        if (item.NguoiDuyet.ToLower().Trim() != "pm" && item.NguoiDuyet.ToLower().Trim() != "head")
                        {
                            var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV == item.NguoiDuyet.Trim() && it.Del != true && it.MaTinhTrang == "1");
                            if (nhansu == null)
                            {
                                rs.text = "Mã nhân viên " + item.NguoiDuyet.ToString().ToUpper() + " không tồn tại";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }

                        model.NguoiDuyet = item.NguoiDuyet;
                        model.GhiChu = item.GhiChu;
                        //model.CapDuyet = item.CapDuyet;
                        model.NgayThayDoiLanCuoi = DateTime.Now;
                        model.NguoiThayDoiLanCuoi = NguoiDung1;
                        db.SaveChanges();
                        rs.text =("Thành công");
                        rs.code = 1;
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        rs.text = "Không có dữ liệu cập nhật";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    rs.text = ex.ToString();
                    rs.code = 0;
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "Save_MaTrixNghiPhep");
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> XoaMatrixNghiPhep(int Id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Xóa matrix nghỉ phép";
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    var del = db.TTF_MaTrixDuyetNghiPhep.FirstOrDefault(it => it.ID == Id);
                    db.TTF_MaTrixDuyetNghiPhep.Remove(del);
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                    rs.code = 0;
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "XoaMatrixNghiPhep");
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }

            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public ActionResult MaTrixTangCa()
        {
            return View();
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public async Task<JsonResult> GetMatrixTangCa(string maLoaiTangCa)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_MaTrixTangCa(maLoaiTangCa).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> AddMatrixTangCa(Proc_MaTrixTangCa_Result item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new TTF_FACEIDEntities())
            {
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NguoiDung1 = (int)nguoidung.NguoiDung;
                if (item.CapDuyet == 1 && item.NguoiDuyet.ToString().Trim().ToLower() != "head")
                {
                    rs.text = "Cấp duyệt số 1 phải là Head";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }

                var checkMaTrix = db.TTF_MaTrixDuyetTangCa.Where(it => it.LoaiTangCa.Trim() == item.LoaiTangCa.Trim()).ToList();
                if (checkMaTrix != null && checkMaTrix.Count > 0)
                {
                    var check = checkMaTrix.Where(it => it.CapDuyet == item.CapDuyet).ToList();
                    if (check != null && check.Count > 0)
                    {
                        rs.text = "Đã tồn tại cấp duyệt số " + item.CapDuyet.ToString();
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                    check = checkMaTrix.Where(it => it.NguoiDuyet.Trim().ToLower() == item.NguoiDuyet.Trim().ToLower()).ToList();
                    if (check != null && check.Count > 0)
                    {
                        rs.text = "Người duyệt " + item.NguoiDuyet.ToString() + " đã tồn tại ";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
                if (item.NguoiDuyet.ToLower().Trim() != "pm" && item.NguoiDuyet.ToLower().Trim() != "head")
                {
                    var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV == item.NguoiDuyet.Trim() && it.Del != true && it.MaTinhTrang == "1");
                    if (nhansu == null)
                    {
                        rs.text = "Mã nhân viên " + item.NguoiDuyet.ToString().ToUpper() + " không tồn tại";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                }
                try
                {
                    TTF_MaTrixDuyetTangCa add = new TTF_MaTrixDuyetTangCa();

                    add.LoaiQuyTrinh = "TC";
                    add.LoaiTangCa = item.LoaiTangCa ;
                    add.CapDuyet = item.CapDuyet;
                    add.NguoiDuyet = item.NguoiDuyet;
                    add.GhiChu = item.GhiChu;
                    db.TTF_MaTrixDuyetTangCa.Add(add);
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    rs.text = ex.ToString();
                    rs.code = 0;
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "AddMatrixTangCa");
                    return Json(rs, JsonRequestBehavior.AllowGet);


                }
            }
        }

        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> SaveMatrixTangCa(Proc_MaTrixTangCa_Result item) {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (User.Identity.Name == null || User.Identity.Name == "")
            {
                rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            using (var db  = new SaveDB())
            {
                db.GhiChu = "Sửa matrix tăng ca";
                try
                {
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NguoiDung1 = (int)nguoidung.NguoiDung;
                    var model = db.TTF_MaTrixDuyetTangCa.FirstOrDefault(it => it.ID == item.ID);

                    if (model != null)
                    {
                        if (item.CapDuyet == 1 && item.NguoiDuyet.ToString().Trim().ToLower() != "head")
                        {
                            rs.text = "Cấp duyệt số 1 phải là Head";
                            return Json(rs, JsonRequestBehavior.AllowGet);
                        }
                        var checkMaTrix = db.TTF_MaTrixDuyetTangCa.Where(it => it.LoaiTangCa.Trim() == item.LoaiTangCa.Trim()).ToList();
                        if (checkMaTrix != null && checkMaTrix.Count > 0)
                        {

                            var check = checkMaTrix.Where(it => it.CapDuyet == item.CapDuyet && it.ID != item.ID).ToList();
                            if (check != null && check.Count > 0)
                            {
                                rs.text = "Cấp duyệt " + item.CapDuyet.ToString() + " đã tồn tại ";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                            check = checkMaTrix.Where(it => it.NguoiDuyet.Trim().ToLower() == item.NguoiDuyet.Trim().ToLower() && it.ID != item.ID).ToList();
                            if (check != null && check.Count > 0)
                            {
                                rs.text = "Người duyệt " + item.NguoiDuyet.ToString() + " đã tồn tại ";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }

                        }
                        if (item.NguoiDuyet.ToLower().Trim() != "pm" && item.NguoiDuyet.ToLower().Trim() != "head")
                        {
                            var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV == item.NguoiDuyet.Trim() && it.Del != true && it.MaTinhTrang == "1");
                            if (nhansu == null)
                            {
                                rs.text = "Mã nhân viên " + item.NguoiDuyet.ToString().ToUpper() + " không tồn tại";
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                        }

                        model.NguoiDuyet = item.NguoiDuyet;
                        model.GhiChu = item.GhiChu;
                        model.CapDuyet = item.CapDuyet;
                        model.NguoiThayDoiLanCuoi = NguoiDung1;
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        rs.text = "Không có dữ liệu cập nhật";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    rs.text = ex.ToString();
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "SaveMatrixTangCa");
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }
            }
        }

        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> XoaMatrixTangCa(int Id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Xóa matrix tăng ca";
                if (User.Identity.Name == null || User.Identity.Name == "")
                {
                    rs.text = "Đã hết thời gian thao tác phần mềm. Xin hãy đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    var del = db.TTF_MaTrixDuyetTangCa.FirstOrDefault(it => it.ID == Id);
                    db.TTF_MaTrixDuyetTangCa.Remove(del);
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                    rs.code = 0;
                    clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString(), "NP", "XoaMatrixTangCa");
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }

            }
        }

        [RoleAuthorize(Roles = "8=1,0=0")]
        public ActionResult DuAn() {
            return View();
        }
        public async Task<JsonResult> GetDuAn(string maDuAn,string maCaLamViec)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.Proc_GetDuAn(maDuAn, maCaLamViec).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LuuThongTinDuAn(Proc_GetDuAn_Result item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                try
                {
                    db.GhiChu = "Sửa thông tin dự án";
                    var check = db.TTF_Mails.FirstOrDefault(it => it.MaDuAn == item.MaDuAn);
                    if (check == null)
                    {
                        var add = new TTF_Mails();
                        add.MaDuAn = item.MaDuAn;
                        add.HoTen = item.HoTen;
                        add.Email = item.Email;
                        add.GhiChu = item.GhiChu;
                        add.Del = item.Del;
                        add.TinhCong = false;
                        add.MaCaLamViec = item.MaCaLamViec;
                        db.TTF_Mails.Add(add);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
                    }
                    else
                    {
                        check.MaDuAn = item.MaDuAn;
                        check.HoTen = item.HoTen;
                        check.Email = item.Email;
                        check.GhiChu = item.GhiChu;
                        check.Del = item.Del;
                        check.TinhCong = false;
                        check.MaCaLamViec = item.MaCaLamViec;
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
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

        [RoleAuthorize(Roles = "8=1,0=0")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> XoaDuAn(string maDuAn )
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                try
                {
                    db.GhiChu = "Xóa dự án";
                    var check = db.TTF_Mails.FirstOrDefault(it => it.MaDuAn == maDuAn.Trim());
                    if (check == null)
                    {
                        rs.code = 0;
                        rs.text = "Không tìm thấy dự án xóa";
                    }
                    else
                    {
                        db.TTF_Mails.Remove(check);
                        db.SaveChanges();
                        rs.code = 1;
                        rs.text = "Thành công";
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

        [RoleAuthorize(Roles = "8=1,0=0")]
        public ActionResult NhomNguoiDung()
        {
            return View();
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public async Task<JsonResult> GetNhomNguoiDung()
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.HT_NHOMNGUOIDUNG.ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> LuuNhomNguoiDung(HT_NHOMNGUOIDUNG item)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa nhóm người dùng";
                try
                {
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    if (item.NHOMNGUOIDUNG > 0)
                    {
                        // sua têm nhom
                        var edit = db.HT_NHOMNGUOIDUNG.FirstOrDefault(it => it.NHOMNGUOIDUNG == item.NHOMNGUOIDUNG);
                        edit.TENNHOMNGUOIDUNG = item.TENNHOMNGUOIDUNG;
                        edit.NGUOIDUNG2 = (int)nguoidung.NguoiDung;
                        edit.NGAY2 = DateTime.Now;
                    }
                    else
                    {
                        var add = new HT_NHOMNGUOIDUNG();
                        add.TENNHOMNGUOIDUNG = item.TENNHOMNGUOIDUNG;
                        add.ISDEL = false;
                        add.NGUOIDUNG1 = (int)nguoidung.NguoiDung;
                        add.NGAY1 = DateTime.Now;
                        db.HT_NHOMNGUOIDUNG.Add(add);
                    }
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> XoaNhomNguoiDung(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Xóa nhóm người dùng";
                try
                {
                    var del = db.HT_NHOMNGUOIDUNG.FirstOrDefault(it => it.NHOMNGUOIDUNG == id);
                    db.HT_NHOMNGUOIDUNG.Remove(del);
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";
                }
                catch (Exception ex)
                {
                    rs.text = ex.Message;
                }
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [RoleAuthorize(Roles = "8=1,0=0")]
        public ActionResult NhomNguoiDungPhanQuyen(string id)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.HT_QUYENSUDUNG.Where(m => m.ISDEL != true);
                List<QuyenSuDungModel> quyensudung = new List<QuyenSuDungModel>();
                List<QuyenSuDungModel> quyentam = new List<QuyenSuDungModel>();
                if (!string.IsNullOrEmpty(id))
                {
                    int iNhomNguoiDung = int.Parse(id);
                    var item = db.HT_NHOMNGUOIDUNG.SingleOrDefault(m => m.NHOMNGUOIDUNG == iNhomNguoiDung);
                    ViewBag.NhomNguoiDung = iNhomNguoiDung;
                    ViewBag.TenNhomNguoiDung = item.TENNHOMNGUOIDUNG;
                    if (string.IsNullOrEmpty(item.QUYEN))
                    {
                        foreach (var i in model)
                        {
                            quyensudung.Add(new QuyenSuDungModel()
                            {
                                QuyenSuDung = i.QUYENSUDUNG,
                                TenQuyenSuDung = i.TENQUYENSUDUNG,
                                NhomQuyen = i.NHOMQUYEN,
                                Quyen = 0
                            });
                        }
                    }
                    else
                    {
                        string[] sQuyen = item.QUYEN.Split(new char[] { '|' });
                        foreach (var i in model)
                        {
                            bool IsQuyen = false;
                            foreach (string j in sQuyen)
                            {
                                string[] sMang = j.Split(new char[] { '=' });
                                int Quyen = Convert.ToInt32(sMang[1]);
                                if (sMang[0] == i.QUYENSUDUNG.ToString())
                                {
                                    sQuyen = sQuyen.Where(m => m != j).ToArray();
                                    if (Quyen == 5 || Quyen == 6)
                                        quyensudung.Add(new QuyenSuDungModel()
                                        {
                                            QuyenSuDung = i.QUYENSUDUNG,
                                            TenQuyenSuDung = i.TENQUYENSUDUNG,
                                            NhomQuyen = i.NHOMQUYEN,
                                            Quyen = 5
                                        });
                                    else
                                    {
                                        if (quyensudung.Count == 0)
                                        {
                                            quyensudung.Add(new QuyenSuDungModel()
                                            {
                                                QuyenSuDung = i.QUYENSUDUNG,
                                                TenQuyenSuDung = i.TENQUYENSUDUNG,
                                                NhomQuyen = i.NHOMQUYEN,
                                                Quyen = Quyen
                                            });
                                        }
                                        else if (quyensudung.LastOrDefault().QuyenSuDung == i.QUYENSUDUNG)
                                        {
                                            quyensudung.LastOrDefault().Quyen = Quyen;
                                        }
                                        else
                                        {
                                            quyensudung.Add(new QuyenSuDungModel()
                                            {
                                                QuyenSuDung = i.QUYENSUDUNG,
                                                TenQuyenSuDung = i.TENQUYENSUDUNG,
                                                NhomQuyen = i.NHOMQUYEN,
                                                Quyen = Quyen
                                            });
                                        }
                                    }
                                    IsQuyen = true;
                                    if (Quyen == 4)
                                        break;
                                }
                            }
                            if (!IsQuyen)
                                quyensudung.Add(new QuyenSuDungModel()
                                {
                                    QuyenSuDung = i.QUYENSUDUNG,
                                    TenQuyenSuDung = i.TENQUYENSUDUNG,
                                    NhomQuyen = i.NHOMQUYEN,
                                    Quyen = 0
                                });
                        }
                    }
                }
                else
                {
                    foreach (var i in model)
                    {
                        quyensudung.Add(new QuyenSuDungModel()
                        {
                            QuyenSuDung = i.QUYENSUDUNG,
                            TenQuyenSuDung = i.TENQUYENSUDUNG,
                            NhomQuyen = i.NHOMQUYEN,
                            Quyen = 0
                        });
                    }
                }
                PhanQuyenModel phanquyen = new PhanQuyenModel();
                phanquyen.NhomQuyen = (from s in db.HT_NHOMQUYEN orderby s.STT ascending select s).ToList();
                phanquyen.QuyenSuDung = quyensudung;
                return View(phanquyen);
            }
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "8=1,0=0")]
        public async Task<JsonResult> LuuPhanQuyenNhom(string NhomNguoiDung, string sQuyen)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa phân quyền";
                try
                {
                    int iNhomNguoiDung = int.Parse(NhomNguoiDung);
                    HT_NHOMNGUOIDUNG Nhom = db.HT_NHOMNGUOIDUNG.SingleOrDefault(m => m.NHOMNGUOIDUNG == iNhomNguoiDung);
                    if (Nhom == null)
                    {
                        rs.text = "Không tìm thấy nhóm quyền cần phân";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    Nhom.QUYEN = sQuyen;
                    Nhom.NGAY2 = DateTime.Now;
                   // Nhom.MAY2 = QuanLyChamCong.Class.clsUserActivesInternal.clsUserActive.GetIP(User.Identity.Name);
                    db.SaveChanges();
                    rs.code = 1;
                    rs.text = "Thành công";

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
   
}