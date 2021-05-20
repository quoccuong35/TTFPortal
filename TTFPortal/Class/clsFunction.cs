using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using TTFPortal.Models;


namespace TTFPortal.Class
{
    public class clsFunction
    {
        public static string GetDBName()
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.Database.Connection.ConnectionString);
            string dbName = builder.InitialCatalog;
            return dbName;
        }
        public static void NhatkyLoi(DateTime Ngay, string TaiKhoan, string Loi, string LoaiChucNang, string NoiDung)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            TTF_NhatkyLoi add = new TTF_NhatkyLoi();
            add.Ngay = Ngay;
            add.TaiKhoan = TaiKhoan;
            add.Loi = Loi;
            add.LoaiChucNang = LoaiChucNang;
            add.NoiDung = NoiDung;
            db.TTF_NhatkyLoi.Add(add);
            db.SaveChanges();
        }
        public static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static string GenerateSlug(string phrase)
        {
            string str = convertToUnSign3(phrase).ToLower().Replace(".", "-");
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into once space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }
        public static bool checkKyCongNguoiDung(DateTime ngay)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            bool b = false;
            var model = db.TTF_KyCongNguoiDung.Where(it => it.TuNgay <= ngay.Date && it.DenNgay >= ngay.Date).ToList();
            if ((model.Where(it => it.Dong == true).ToList().Count > 0) || model.Count == 0)
            {
                b = true;
            }
            return b;
        }
        public static int LayCapDuyetKeTiep(int NhanSu, float SoNgay, string Loai, int IDQuyTrinh)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            int ikq = 0;
            if (Loai == "NP") // duyệt nghỉ phép
            {
                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NhanSu);
                if (nhansu.IDCapQuanLyTrucTiep == null) // chua có cấp quản lý trực tiếp
                    ikq = -1;//
                else
                {
                    if (IDQuyTrinh == 0) // Lấy cấp duyệt đầu
                    {
                        ikq = (int)nhansu.IDCapQuanLyTrucTiep;
                    }
                    else // Lấy cấp duyệt từ 2
                    {
                        var lichsuduyet = db.TTF_LichSuDuyet.Where(it => it.IDQuyTrinh == IDQuyTrinh && it.MaQuyTrinh.Trim() == Loai.Trim() && it.TrangThaiDuyet == true).ToList();

                        int iCapduyetcanlay = lichsuduyet.Count + 1;

                        var TTF_MaTrixDuyetNghiPhep = db.TTF_MaTrixDuyetNghiPhep.Where(it => it.LoaiQuyTrinh.Trim() == Loai
                                                                                  && it.MaPhong_PhanXuong.Trim() == nhansu.MaPhong_PhanXuong.Trim()
                                                                                  && it.TuNgay < SoNgay && it.DenNgay > SoNgay).ToList();
                        if (TTF_MaTrixDuyetNghiPhep.Count() == 0)
                        {
                            ikq = -3; // không tồn tại cay ma tric duyet hiện tại
                            return ikq;
                        }
                        if (iCapduyetcanlay > TTF_MaTrixDuyetNghiPhep.Count && TTF_MaTrixDuyetNghiPhep.Count > 0)
                        {
                            ikq = -2;
                            return ikq;
                        }
                        while (iCapduyetcanlay <= TTF_MaTrixDuyetNghiPhep.Count())
                        {
                            // lay thông tin người duyệt kế tiếp
                            var matrix = TTF_MaTrixDuyetNghiPhep.FirstOrDefault(
                                                                it => it.LoaiQuyTrinh.Trim() == Loai.Trim() && it.MaPhong_PhanXuong.Trim() == nhansu.MaPhong_PhanXuong.Trim()
                                                                && it.CapDuyet == iCapduyetcanlay && it.TuNgay < SoNgay && it.DenNgay > SoNgay);

                            // lấy thông tin nhân sự người duyệt kế tiếp
                            if (matrix == null)
                            {
                                return ikq = -3;
                            }
                            var nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == matrix.NguoiDuyet.Trim());

                            // Kiểm tra người duyệt kế tiếp có duyệt trong lịch sử duyệt chưa: cấp quản lý  trở lên sẽ có thể không đi qua đủ số cấp duyệt
                            var checkLichSuDuyet = lichsuduyet.FirstOrDefault(it => it.NguoiDuyet == nhansutemp.NhanSu);
                            if (checkLichSuDuyet != null)
                            {
                                iCapduyetcanlay++;
                                ikq = -2; // cập nhật duyệt hoàn thành
                            }
                            else
                            {
                                ikq = nhansutemp.NhanSu;
                                break;
                            }
                        }
                    }
                }
            }
            else if (Loai == "TC") // Tăng ca
            {
                var tc = db.TTF_TangCa.FirstOrDefault(it => it.IDTangCa == IDQuyTrinh);
                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NhanSu);

                //bool bHoanThanh = false;
                if (nhansu.IDCapQuanLyTrucTiep == null) // chua có cấp quản lý trực tiếp
                    ikq = -1;
                else
                {
                    if (IDQuyTrinh == 0) // Lấy cấp duyệt đầu
                    {
                        ikq = (int)nhansu.IDCapQuanLyTrucTiep;
                    }
                    else
                    {
                        var lichsuduyet = db.TTF_LichSuDuyet.Where(it => it.IDQuyTrinh == IDQuyTrinh && it.MaQuyTrinh.Trim() == Loai.Trim() && it.TrangThaiDuyet == true).ToList();
                        if (tc.DuAn == true && tc.MaDuAn.Trim() == "NB-0078-WORKFLOW" && lichsuduyet.FirstOrDefault(it => it.NguoiDuyet == 2014) != null)
                        {
                            ikq = -2; // cập nhật duyệt hoàn thành
                            return ikq;
                        }
                        int iCapduyetcanlay = lichsuduyet.Count + 1;

                        var TTF_MaTrixDuyetTangCa = db.TTF_MaTrixDuyetTangCa.Where(it => it.LoaiQuyTrinh.Equals(Loai) && it.LoaiTangCa.Trim() == nhansu.LoaiLaoDongTangCa.Trim());

                        if (TTF_MaTrixDuyetTangCa.Count() == 0)
                        {
                            ikq = -3; // không tồn tại cay ma tric duyet hiện tại
                            return ikq;
                        }

                        if (iCapduyetcanlay > TTF_MaTrixDuyetTangCa.Count())
                        {
                            ikq = -2; // cập nhật duyệt hoàn thành
                            return ikq;
                        }

                        while (iCapduyetcanlay <= TTF_MaTrixDuyetTangCa.Count())
                        {
                            // lay thông tin người duyệt kế tiếp
                            var matrix = TTF_MaTrixDuyetTangCa.FirstOrDefault(
                                                                it => it.LoaiQuyTrinh.Trim() == Loai.Trim() && it.LoaiTangCa.Trim() == nhansu.LoaiLaoDongTangCa.Trim()
                                                                && it.CapDuyet == iCapduyetcanlay);
                            if (tc.DuAn == false && matrix.NguoiDuyet.ToLower().Trim() == "pm")
                            {
                                iCapduyetcanlay++;
                                continue;
                            }
                            // lấy thông tin nhân sự người duyệt kế tiếp
                            TTF_NhanSu nhansutemp;
                            if (tc.DuAn == false)
                            {
                                nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == matrix.NguoiDuyet.Trim());
                            }
                            else // là dự án có trưởng nhóm quản lý
                            {
                                if (matrix.NguoiDuyet != null && matrix.NguoiDuyet.ToLower().Trim() == "pm")
                                {
                                    //TTF_TimeTracKingEntities dbcom = new Models.TTF_TimeTracKingEntities();
                                    //var temp = dbcom.TTF_DuAn.FirstOrDefault(it => it.MaDuAn == tc.MaDuAn.Trim());
                                    var DuyetDacBiet = db.TTF_DuyetTangCaPMDacBiet.FirstOrDefault(it => it.MaDuAn.Trim() == tc.MaDuAn.Trim() && it.Del != true);
                                    if (DuyetDacBiet != null)
                                    {
                                        nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == DuyetDacBiet.MaNhanVien.Trim());
                                        //bHoanThanh = DuyetDacBiet.HoanThanh == null?false:DuyetDacBiet.HoanThanh.Value;
                                    }
                                    else
                                    {
                                        var temp = db.TTF_DuyetTangCaPM.FirstOrDefault(it => it.Del != true);
                                        nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == temp.MaNV.Trim());
                                    }
                                }
                                else
                                {
                                    nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == matrix.NguoiDuyet.Trim());
                                }
                            }

                            // Kiểm tra người duyệt kế tiếp có duyệt trong lịch sử duyệt chưa: cấp quản lý  trở lên sẽ có thể không đi qua đủ số cấp duyệt
                            var checkLichSuDuyet = lichsuduyet.FirstOrDefault(it => it.NguoiDuyet == nhansutemp.NhanSu);
                            if (checkLichSuDuyet != null)
                            {
                                iCapduyetcanlay++;
                                ikq = -2; // cập nhật duyệt hoàn thành
                            }
                            else
                            {
                                ikq = nhansutemp.NhanSu;
                                break;
                            }
                        }
                    }
                }
            }
            else if (Loai == "XNC")
            {
                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NhanSu);

                if (nhansu.IDCapQuanLyTrucTiep == null) // chua có cấp quản lý trực tiếp
                    ikq = -1;
                else
                {
                    ikq = (int)nhansu.IDCapQuanLyTrucTiep;
                }
            }
            else if (Loai == "CT") // CongTac
            {
                var tc = db.TTF_CongTac.FirstOrDefault(it => it.IDCongTac == IDQuyTrinh);
                var nhansu = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == NhanSu);

                if (nhansu.IDCapQuanLyTrucTiep == null) // chua có cấp quản lý trực tiếp
                    ikq = -1;
                else
                {
                    if (IDQuyTrinh == 0) // Lấy cấp duyệt đầu
                    {
                        ikq = (int)nhansu.IDCapQuanLyTrucTiep;
                    }
                    else
                    {

                        var lichsuduyet = db.TTF_LichSuDuyet.Where(it => it.IDQuyTrinh == IDQuyTrinh && it.MaQuyTrinh.Trim() == Loai.Trim() && it.TrangThaiDuyet == true).ToList();

                        int iCapduyetcanlay = lichsuduyet.Count + 1;

                        var TTF_MaTrixDuyetCongTac = db.TTF_MaTrixDuyetCongTac.Where(it => it.LoaiQuyTrinh.Equals(Loai) && it.LoaiCongTac.Trim() == nhansu.LoaiLaoDongTangCa.Trim()).ToList();

                        if (TTF_MaTrixDuyetCongTac.Count() == 0)
                        {
                            ikq = -3; // không tồn tại cay ma tric duyet hiện tại
                            return ikq;
                        }

                        if (iCapduyetcanlay > TTF_MaTrixDuyetCongTac.Count())
                        {
                            ikq = -2; // cập nhật duyệt hoàn thành
                            return ikq;
                        }

                        while (iCapduyetcanlay <= TTF_MaTrixDuyetCongTac.Count())
                        {
                            // lay thông tin người duyệt kế tiếp
                            var matrix = TTF_MaTrixDuyetCongTac.FirstOrDefault(
                                                                it => it.LoaiQuyTrinh.Trim() == Loai.Trim() && it.LoaiCongTac.Trim() == nhansu.LoaiLaoDongTangCa.Trim()
                                                                && it.CapDuyet == iCapduyetcanlay);
                            if (matrix.NguoiDuyet.Trim().ToLower() == "head")
                            {
                                var ns = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == tc.NhanSu);
                                ikq = -(int)ns.IDCapQuanLyTrucTiep;
                                break;
                            }
                            if (tc.DuAn == false && matrix.NguoiDuyet.ToLower().Trim() == "pm")
                            {
                                iCapduyetcanlay++;
                                continue;
                            }
                            // lấy thông tin nhân sự người duyệt kế tiếp
                            TTF_NhanSu nhansutemp;
                            if (tc.DuAn == false)
                            {
                                nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == matrix.NguoiDuyet.Trim());
                            }
                            else // là dự án có trưởng nhóm quản lý
                            {
                                if (matrix.NguoiDuyet != null && matrix.NguoiDuyet.ToLower().Trim() == "pm")
                                {
                                    var temp = db.TTF_DuyetTangCaPM.FirstOrDefault(it => it.Del != true);
                                    nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == temp.MaNV.Trim());
                                }
                                else
                                {
                                    nhansutemp = db.TTF_NhanSu.FirstOrDefault(it => it.MaNV.Trim() == matrix.NguoiDuyet.Trim());
                                }
                            }

                            // Kiểm tra người duyệt kế tiếp có duyệt trong lịch sử duyệt chưa: cấp quản lý  trở lên sẽ có thể không đi qua đủ số cấp duyệt
                            var checkLichSuDuyet = lichsuduyet.FirstOrDefault(it => it.NguoiDuyet == nhansutemp.NhanSu);
                            if (checkLichSuDuyet != null)
                            {
                                iCapduyetcanlay++;
                                continue;
                            }
                            else
                            {
                                ikq = nhansutemp.NhanSu;
                                break;
                            }
                        }
                    }
                }
            }
            return ikq;
        }
        public static TTF_NhanSu LayThongTinNhanSuNguoiTao(int IDNguoiTao)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            TTF_NhanSu rv = new TTF_NhanSu();
            var user = db.HT_NGUOIDUNG.Where(o => o.NGUOIDUNG == IDNguoiTao).FirstOrDefault();
            if (user != null)
            {
                rv = db.TTF_NhanSu.Where(o => o.NhanSu == user.NhanSu).FirstOrDefault();
            }
            return rv;
        }
        public static HT_HETHONG Get_HT_HETHONG()
        {
            HT_HETHONG rv = new HT_HETHONG();
            try
            {
                using (var db = new TTF_FACEIDEntities())
                {
                    var obj = db.HT_HETHONG.Where(o => o.ID == 1).FirstOrDefault();
                    if (obj != null)
                    {
                        rv = obj;
                    }
                }
                return rv;
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, "", ex.ToString(), "", MethodBase.GetCurrentMethod().Name);
                return rv;
            }
            finally
            {

            }
        }
        public static void GuiMail(string Subject, string To, string CC, string Body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("mail.truongthanh.com");
                mail.From = new System.Net.Mail.MailAddress("qlcc@truongthanh.com");
                mail.To.Add(To);
                if (CC.Trim().Length > 0)
                {
                    string[] CCList = CC.Trim().Split(';');
                    foreach (var item in CCList)
                    {
                        mail.CC.Add(item);
                    }
                }
                mail.Subject = Subject;
                mail.IsBodyHtml = true;
                mail.Body = Body;

                //System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment(url);
                //mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("qlcc@truongthanh.com", "ttfBD@2018");
                SmtpServer.EnableSsl = false;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
                mail.Dispose();
            }
            catch
            {
            }

        }
        public static int LuuLichSuDuyet(long IDQuyTrinh, string MaQuyTrinh, bool TrangThaiDuyet, int NguoiDuyet, DateTime NgayGui)
        {
            var db = new SaveDB();
            db.GhiChu = "Lưu lịch sử duyệt";
            TTF_LichSuDuyet add = new TTF_LichSuDuyet();
            add.IDQuyTrinh = IDQuyTrinh;
            add.MaQuyTrinh = MaQuyTrinh;
            add.TrangThaiDuyet = TrangThaiDuyet;
            add.NguoiDuyet = Math.Abs(NguoiDuyet);
            add.NgayGui = NgayGui;
            if (MaQuyTrinh == "CT")
            {
                if (NguoiDuyet < 0)
                {
                    add.SoLanDuyet = 2;
                }
                else
                {
                    add.SoLanDuyet = 1;
                }
            }
            else
                add.SoLanDuyet = 0;
            db.TTF_LichSuDuyet.Add(add);
            int rs = db.SaveChanges();
            return rs;
        }
        public static int CapNhatDuyetVaoLichSuDuyet(long IDQuyTrinh, string MaQuyTrinh, int NguoiDuyet, string GhiChuDuyet)
        {
            int rv = -1;
            using (SaveDB db = new SaveDB())
            {
                db.GhiChu = "Lưu lịch sử duyệt " + MaQuyTrinh;
                var obj = db.TTF_LichSuDuyet.Where(o => o.IDQuyTrinh == IDQuyTrinh
                    && o.MaQuyTrinh == MaQuyTrinh
                    && o.NguoiDuyet == NguoiDuyet && o.TrangThaiDuyet != true).FirstOrDefault();
                if (obj != null)
                {
                    obj.TrangThaiDuyet = true;
                    obj.NgayDuyet = DateTime.Now;
                    obj.GhiChuDuyet = GhiChuDuyet;
                    rv = db.SaveChanges();
                }
                else
                {
                    rv = -1;
                }
                return rv;
            }
        }
        public static int CapNhatTuChoiVaoLichSuDuyet(long IDQuyTrinh, string MaQuyTrinh, int NguoiDuyet, string GhiChuDuyet)
        {
            int rv = -1;
            SaveDB db = new SaveDB();
            var obj = db.TTF_LichSuDuyet.Where(o => o.IDQuyTrinh == IDQuyTrinh
                && o.MaQuyTrinh == MaQuyTrinh
                && o.NguoiDuyet == NguoiDuyet).FirstOrDefault();
            if (obj != null)
            {
                obj.NgayDuyet = DateTime.Now;
                obj.LyDoHuy = GhiChuDuyet;
                rv = db.SaveChanges();
            }
            else
            {
                rv = -1;
            }
            return rv;
        }
        public static bool checkKyCongNhanSu(DateTime ngay)
        {
            bool b = false;
            using (var db = new SaveDB())
            {
                var model = db.Database.SqlQuery<TTF_TimekeepingPeriod>("SELECT * FROM dbo.TTF_TimekeepingPeriod WHERE CONVERT(DATE,FromDate) <= '" + ngay.ToString("MM/dd/yyy") + "' AND CONVERT(DATE,ToDate) >= '" + ngay.ToString("MM/dd/yyy") + "'").ToList();
                if (model.Count > 0 && model[0].Status != true)
                {
                    b = true;
                }
            }
            return b;
        }
        public static TTF_NhanSu GetNS(string MaNV)
        {
            TTF_NhanSu ns = null;
            try
            {
                using (var db = new TTF_FACEIDEntities())
                {
                    ns = db.TTF_NhanSu.Where(o => o.MaNV.Trim() == MaNV.Trim()).FirstOrDefault();
                }
                return ns;
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, "", ex.ToString(), "", MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }
        public static bool CheckNgayLe(DateTime date)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            bool rv = false;
            var check = db.TTF_NgayLe.Where(o => o.Date == date.Date).FirstOrDefault();
            if (check != null)
            {
                rv = true;
            }
            return rv;
        }
        public static string GetCCwithTo(string Type, string To)
        {
            string rv = "";
            try
            {
                using (var db = new TTF_FACEIDEntities())
                {
                    var l = db.v_CCwithTo.Where(o => o.MailCongTy_To.Trim() == To.Trim()).ToList();
                    if (l.Count > 0)
                    {
                        foreach (var item in l)
                        {
                            rv += item.MailCongTy_CC.Trim() + ";";
                        }
                        rv = rv.TrimEnd(';');
                    }
                }
                return rv;
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, "", ex.ToString(), "", MethodBase.GetCurrentMethod().Name);
                return rv;
            }
        }
        public static CongNgay TinhCongNgay1(Proc_NhanSuForCong_Result ns, DateTime ngay, DateTime NgayBatDau, DateTime NgayChotCong,
           List<DLVT> DLVTCaNgayTongHop1_final, List<Proc_CongCaDem_Result> DataCaDem,
           List<Proc_XacNhanCong_Result> XacNhanCong, List<Proc_CongNgayHieuChinh_Result> CongNgayHC, List<Proc_TangCaChiTiet_Result> TangCa,
           List<TTF_TimekeepingPeriod> KyCong, List<Proc_NhanSuCaLamViec_Result> listNhanSuCaLamViec, List<Proc_CanLamViecCongTrinh_Result> CaLamViecKM, List<ttf_CongKhuonMat_Result> CongKM)
        {
            //TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            CongNgay objCongNgay;
            //TimeSpan GioVao = new TimeSpan(0, 0, 0);
            //TimeSpan GioRa = new TimeSpan(0, 0, 0);
            TimeSpan GioVao;
            TimeSpan GioRa;
            DateTime CDGioVao = new DateTime(1990, 1, 1);
            DateTime CDGioRa = new DateTime(1990, 1, 1);
            DateTime CDGioVaoChuan = new DateTime(1990, 1, 1);
            DateTime CDGioRaChuan = new DateTime(1990, 1, 1);
            DateTime CDGioVaoFinal = new DateTime(1990, 1, 1);
            DateTime CDGioRaFinal = new DateTime(1990, 1, 1);
            Double dCong = 0;
            int PhutDuocTre = 0;
            objCongNgay = new CongNgay();
            string MSNV = ns.MaNV;
            string InTimeHC = "";
            string OutDateHC = "";
            string OutTimeHC = "";
            bool CD = false;
            bool QuetVTLienTiep = false;
            string MaCaLamViec = "";
            DateTime CDGioVaoTCTT = new DateTime(1990, 1, 1);
            DateTime CDGioRaTCTT = new DateTime(1990, 1, 1);
            DateTime CDGioVaoTCFinal = new DateTime(1990, 1, 1);
            DateTime CDGioRaTCFinal = new DateTime(1990, 1, 1);
            DateTime CDGioVaoTCPhieu = new DateTime(1990, 1, 1);
            DateTime CDGioRaTCPhieu = new DateTime(1990, 1, 1);
            DateTime CDNgayCheckTC = new DateTime(1990, 1, 1);
            DateTime FullGioVao = new DateTime(1990, 1, 1);
            DateTime FullGioRa = new DateTime(1990, 1, 1);
            TimeSpan CanTrenTC = new TimeSpan(0, 0, 0);
            TimeSpan CanDuoiTC = new TimeSpan(0, 0, 0);
            TimeSpan tsTemp;
            double ThoiGianTru = 0, dCongDu = 0.0;
            if (ns != null)
            {
                MaCaLamViec = ns.MaCaLamViec.Trim();
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
                objCongNgay.MaNV = MSNV;
                objCongNgay.Thu = clsFunction.LayThuTiengViet(ngay);
                //objCongNgay.TuanThuMayCuaThang = clsFunction.GetWeekNumberOfMonth(ngay);
                if (ngay.DayOfWeek == DayOfWeek.Saturday)
                {
                    objCongNgay.Thu7LanMay = clsFunction.GetThu7LanMayCuaKyCong(ngay, KyCong);
                }
                objCongNgay.MaChamCong = ns.MaChamCong;
                objCongNgay.Name = ns.HoVaTen;
                objCongNgay.TenPhong_PhanXuong = ns.TenPhong_PhanXuong;
                objCongNgay.InDate = ngay.ToString("dd/MM/yyyy");
                var cnhc = CongNgayHC.Where(o => o.MaNV == ns.MaNV && o.Date == ngay).FirstOrDefault();
                if (cnhc != null)
                {
                    InTimeHC = cnhc.InTime;
                    if (InTimeHC == null)
                    {
                        InTimeHC = "";
                    }
                    else
                    {
                        InTimeHC = InTimeHC.Trim();
                        if (InTimeHC != "")
                            objCongNgay.InTimeHC1 = DateTime.Parse(InTimeHC);
                    }
                    OutDateHC = cnhc.OutDate;
                    if (OutDateHC == null)
                    {
                        OutDateHC = "";
                    }
                    else
                    {
                        OutDateHC = OutDateHC.Trim();
                    }
                    OutTimeHC = cnhc.OutTime;
                    if (OutTimeHC == null)
                    {
                        OutTimeHC = "";
                    }
                    else
                    {
                        OutTimeHC = OutTimeHC.Trim();
                        if (OutTimeHC != "")
                        {
                            objCongNgay.OutTimeHC1 = DateTime.Parse(OutTimeHC);
                            //objCongNgay.OutTimeHC = objCongNgay.OutTimeHC1.ToString("")
                        }
                            
                    }
                    if (cnhc.TangCaSau22H != null && cnhc.TangCaSau22H.ToString().Trim() == "CD")
                    {
                        CD = true;
                    }
                }
                var dlvt = DLVTCaNgayTongHop1_final.Where(o => o.UID == ns.MaChamCong && o.Date.Value.Date == ngay.Date).FirstOrDefault();
                var vtcd = DataCaDem.Where(o => o.UID == ns.MaChamCong && o.InDate == ngay).FirstOrDefault();
                if (InTimeHC == "")
                {
                    Proc_XacNhanCong_Result xnc = XacNhanCong.SingleOrDefault(o => o.NhanSu == ns.NhanSu
                            && o.Ngay == ngay && o.GioVao == true);
                    if (xnc != null)
                    {   // xác nhận cong ca ngày
                        objCongNgay.InTime = xnc.ThoiGian.ToString();
                        objCongNgay.InDate = ngay.ToString("dd/MM/yyyy");
                        objCongNgay.CoXNCVao = true;
                        if (xnc.CaDem == true)
                            CD = true;
                        //var vtcd = DataCaDem.Where(o => o.UID == ns.MaChamCong && o.InDate == ngay).FirstOrDefault();
                        if (vtcd != null)
                        {
                            if (vtcd.InTime != vtcd.OutTime && vtcd.InTime != null && vtcd.InTime.Trim() != "")
                            {
                                // trường hợp không thiếu dữ liệu vâng tay mới lấy bên bản dữ liệu ca đêm
                                objCongNgay.InTime = vtcd.InTime;
                            }
                            //if (xnc != null && xnc.CaDem.Value == true && xnc.Ngay == vtcd.InDate.Date)
                            //{
                            //    objCongNgay.InTime = xnc.ThoiGian.ToString();
                            //    objCongNgay.CoXNCVao = true;
                            //}
                            CD = true;
                        }
                    }
                    else
                    {
                        //var vtcd = DataCaDem.Where(o => o.UID == ns.MaChamCong && o.InDate == ngay).FirstOrDefault();
                        if (vtcd != null)
                        {
                            objCongNgay.InTime = vtcd.InTime == null ? "" : vtcd.InTime;
                            objCongNgay.InDate = vtcd.InDate.ToString("dd/MM/yyyy");
                            CD = true;
                            if (xnc != null && xnc.CaDem == true && xnc.Ngay == vtcd.InDate.Date && objCongNgay.InTime == "")
                            {
                                objCongNgay.InTime = xnc.ThoiGian.ToString();
                                objCongNgay.CoXNCVao = true;
                            }
                        }
                        else
                        {
                            if (ngay.Date == ns.NgayVaoCongTy.Value.Date)//Ngày vào nhận việc mặc định giờ vào 07:30
                            {
                                objCongNgay.InTime = ns.GioBatDauCa.Value.ToString();
                            }
                            else
                            {
                                if (dlvt != null)
                                {
                                    objCongNgay.InTime = dlvt.GioVao;
                                    objCongNgay.OutDate = objCongNgay.InDate;
                                }
                            }
                        }

                    }
                }
                else
                {
                    objCongNgay.InTime = InTimeHC;
                    objCongNgay.InTimeHC = InTimeHC;
                }
                if (OutTimeHC == "")
                {

                    Proc_XacNhanCong_Result xnc;
                    xnc = XacNhanCong.SingleOrDefault(o => o.NhanSu == ns.NhanSu
                            && o.Ngay == ngay && o.GioVao == false && (o.CaDem == null || o.CaDem == false));
                    if (xnc != null)
                    {
                        objCongNgay.OutTime = xnc.ThoiGian.ToString();
                        objCongNgay.OutDate = xnc.Ngay.ToString("dd/MM/yyyy");
                        objCongNgay.CoXNCRa = true;

                        if (vtcd != null)
                        {
                            objCongNgay.OutDate = Convert.ToDateTime(vtcd.OutDate).ToString("dd/MM/yyyy");
                            if (vtcd.InTime != vtcd.OutTime && vtcd.OutTime != null && vtcd.OutTime.Trim() != "")
                            {
                                // trường hợp không thiếu dữ liệu vâng tay mới lấy bên bản dữ liệu ca đêm
                                objCongNgay.OutTime = vtcd.OutTime == null ? "" : vtcd.OutTime;
                            }
                            // kiem tra xac nhan cong  

                            xnc = XacNhanCong.SingleOrDefault(it => it.NhanSu == ns.NhanSu && it.Ngay == vtcd.OutDate && it.GioVao == false && it.CaDem != null && it.CaDem == true);
                            if (xnc != null)
                            {
                                objCongNgay.OutTime = xnc.ThoiGian.ToString().Trim();
                                objCongNgay.CoXNCRa = true;
                            }
                        }
                        //trường hop quyên chấm vào ra luôn ca đêm
                        if (CD && objCongNgay.CoXNCRa)
                        {
                            xnc = XacNhanCong.SingleOrDefault(it => it.NhanSu == ns.NhanSu && it.Ngay == ngay.AddDays(1) && it.GioVao == false && it.CaDem != null && it.CaDem == true);
                            if (xnc != null)
                            {
                                objCongNgay.OutTime = xnc.ThoiGian.ToString().Trim();
                                objCongNgay.OutDate = xnc.Ngay.AddDays(1).ToString("dd/MM/yyyy");
                                objCongNgay.CoXNCRa = true;
                            }
                        }
                    }
                    else
                    {
                        //var vtcd = DataCaDem.Where(o => o.UID == ns.MaChamCong && o.InDate == ngay).FirstOrDefault();
                        if (vtcd != null)
                        {
                            objCongNgay.OutDate = Convert.ToDateTime(vtcd.OutDate).ToString("dd/MM/yyyy");
                            objCongNgay.OutTime = vtcd.OutTime == null ? "" : vtcd.OutTime;
                            // kiem tra xac nhan cong
                            xnc = XacNhanCong.SingleOrDefault(it => it.NhanSu == ns.NhanSu && it.Ngay == vtcd.OutDate && it.GioVao == false && it.CaDem != null && it.CaDem == true);
                            if (xnc != null)
                            {
                                objCongNgay.OutTime = xnc.ThoiGian.ToString().Trim();
                                objCongNgay.OutDate = xnc.Ngay.ToString("dd/MM/yyyy");
                                objCongNgay.CoXNCRa = true;
                            }
                            //if (vtcd.OutTime == "")
                            //{
                            //    xnc = XacNhanCong.Where(it => it.NhanSu == ns.NhanSu && it.Ngay == vtcd.OutDate && it.GioVao == false && it.CaDem == null ? false : it.CaDem == true).FirstOrDefault();
                            //    objCongNgay.OutTime = xnc.ThoiGian.ToString().Trim();
                            //    objCongNgay.OutDate = xnc.Ngay.ToString("dd/MM/yyyy");
                            //    objCongNgay.CoXNCRa = true;
                            //}
                        }
                        else
                        {

                            if (dlvt != null)
                            {
                                objCongNgay.OutTime = dlvt.GioRa;
                                // objCongNgay.OutDate = dlvt.Date.Value.ToString("dd/MM/yyyy");
                                if (ngay.Date == ns.NgayVaoCongTy.Value.Date && ns.MaLoaiLaoDong != null && ns.MaLoaiLaoDong.Trim() == "KTTSX" && ngay.Date < new DateTime(2020, 04, 02) && (ns.MaPhong_PhanXuong.Trim() == "PX1" || ns.MaPhong_PhanXuong.Trim() == "PX2" || ns.MaPhong_PhanXuong.Trim() == "PX3"
                                             || ns.MaPhong_PhanXuong.Trim() == "PX4" || ns.MaPhong_PhanXuong.Trim() == "PX5" || ns.MaPhong_PhanXuong.Trim() == "PX6"
                                             || ns.MaPhong_PhanXuong.Trim() == "PX7" || ns.MaPhong_PhanXuong.Trim() == "PXMAU")
                                && TimeSpan.Parse(objCongNgay.OutTime) < ns.GioKetThucCa)//Ngày vào nhận việc mặc định giờ vào 07:30
                                {
                                    objCongNgay.OutTime = ns.GioKetThucCa.Value.ToString();
                                }
                            }
                            else // khong co cham van tay
                            {
                                if (ngay.Date == ns.NgayVaoCongTy.Value.Date && ns.MaLoaiLaoDong.Trim() == "KTTSX" && ngay.Date < new DateTime(2020, 04, 02) && (ns.MaPhong_PhanXuong.Trim() == "PX1" || ns.MaPhong_PhanXuong.Trim() == "PX2" || ns.MaPhong_PhanXuong.Trim() == "PX3"
                                             || ns.MaPhong_PhanXuong.Trim() == "PX4" || ns.MaPhong_PhanXuong.Trim() == "PX5" || ns.MaPhong_PhanXuong.Trim() == "PX6"
                                             || ns.MaPhong_PhanXuong.Trim() == "PX7" || ns.MaPhong_PhanXuong.Trim() == "PXMAU"))
                                {
                                    objCongNgay.OutTime = ns.GioKetThucCa.Value.ToString();
                                }
                            }
                            // xu ly ca đêm quên chấm vào ra
                            if (CD && (objCongNgay.OutTime == null || objCongNgay.OutTime == ""))
                            {
                                DateTime dayTemp = ngay.AddDays(1);
                                xnc = XacNhanCong.SingleOrDefault(it => it.NhanSu == ns.NhanSu && it.Ngay == dayTemp.Date && it.GioVao == false && it.CaDem != null && it.CaDem == true);
                                if (xnc != null)
                                {
                                    objCongNgay.OutTime = xnc.ThoiGian.ToString().Trim();
                                    objCongNgay.OutDate = xnc.Ngay.ToString("dd/MM/yyyy");
                                    objCongNgay.CoXNCRa = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    objCongNgay.OutTime = OutTimeHC;
                    objCongNgay.OutTimeHC = OutTimeHC;
                    if (OutDateHC != "")
                    {
                        objCongNgay.OutDate = OutDateHC;
                        //objCongNgay.OutDateHC = OutDateHC;
                        try
                        {

                            objCongNgay.OutDateHC = (DateTime.ParseExact(OutDateHC, "dd/MM/yyyy", new CultureInfo("en-US"))).ToString("yyyy-MM-dd");
                        }
                        catch
                        {

                        }

                    }
                    else
                    {
                        objCongNgay.OutDate = objCongNgay.InDate;
                    }
                }


                /// xu lý ca làm việc ngoài công trình theo ca làm việc
                /// giơ vào check công trình nào tính công của công trình đó
                var checkTinhCong = true;
                var checkNgay = CongKM.FirstOrDefault(it => it.UID == ns.MaChamCong && it.Date.Date == ngay && it.Time.ToString() == objCongNgay.InTime);
                if (checkNgay != null)
                {

                    if ((ns.MaCoSo == "3" || checkNgay.CongTac == 1))
                    {
                        // tính công đối với nhân viên đi công tác và nhân viên ngoài công trình
                        var calamviec = CaLamViecKM.FirstOrDefault(it => it.MaDuAn == checkNgay.Construction);
                        if (calamviec != null)
                        {

                            objCongNgay.GioVaoChuan = calamviec.GioBacDau.Value.ToString();
                            objCongNgay.GioRaChuan = calamviec.GioKetThuc.Value.ToString();
                            ns.GioRaGiuaCa = calamviec.GioRaGiuaCa.Value;
                            ns.GioVaoGiuaCa = calamviec.GioVaoGiuaCa.Value;
                            ns.GioBatDauCa = calamviec.GioBacDau;
                            ns.GioKetThucCa = calamviec.GioKetThuc;
                        }
                        else
                        {
                            objCongNgay.Loi = "Công Ngày của " + ns.HoVaTen + checkNgay.Construction + " chưa setup ca làm việc \n";
                            checkTinhCong = false;


                        }
                    }
                    else if (ns.ChamCongKM == true)
                    {
                        // nếu làm chấm công ở công ty và có check là chấm công khuôn mặt tính theo ca làm việc của nhân sự
                    }
                    else
                    {
                        checkTinhCong = false;
                        // không tính công ngày này do dữ liệu khuôn mặt ko phải là công trình củng ko có đi công tác

                    }
                }
                ///    
                var tempNhanSuCaLamViec = listNhanSuCaLamViec.FirstOrDefault(it => it.NhanSu == ns.NhanSu && it.NgayCong == ngay);
                if (tempNhanSuCaLamViec != null)
                {
                    objCongNgay.GioVaoChuan = tempNhanSuCaLamViec.GioVao.ToString();
                    objCongNgay.GioRaChuan = tempNhanSuCaLamViec.GioRa.ToString();

                    ns.GioBatDauCa = tempNhanSuCaLamViec.GioVao;
                    ns.GioKetThucCa = tempNhanSuCaLamViec.GioRa;
                }
                else
                {
                    objCongNgay.GioVaoChuan = ns.GioBatDauCa.Value.ToString();
                    objCongNgay.GioRaChuan = ns.GioKetThucCa.Value.ToString();
                }
                objCongNgay.ThieuVanTay = false;
                if (objCongNgay.InTime == null)
                {
                    objCongNgay.InTime = "";
                }
                if (objCongNgay.OutTime == null)
                {
                    objCongNgay.OutTime = "";
                }
                if (objCongNgay.InTime != "" && objCongNgay.OutTime != "" && checkTinhCong)
                {
                    if (ns.MaLoaiDacBiet == 2 && (ns.NgayBatDau == null || ngay >= ns.NgayBatDau))
                    {
                        GioVao = TimeSpan.Parse(objCongNgay.InTime);
                        GioRa = TimeSpan.Parse(objCongNgay.OutTime);
                        if (ns.DuocDiTre == true)
                        {
                            PhutDuocTre = Convert.ToInt32(ns.SoPhut);
                            if ((TimeSpan.Parse(objCongNgay.InTime) - TimeSpan.Parse(objCongNgay.GioVaoChuan)).TotalMinutes <= PhutDuocTre)
                            {
                                GioVao = TimeSpan.Parse(objCongNgay.GioVaoChuan);
                            }
                        }
                        // trường hop  gio cong đac biet là 2 ma co trang gio vào

                        // truong hop chan gio ra
                        if (ns.CanGioRa != null)
                        {
                            if (GioRa > ns.CanGioRa.Value)
                                GioRa = ns.CanGioRa.Value;
                        }
                    }
                    else
                    {
                        GioVao = TimeSpan.Parse(objCongNgay.GioVaoChuan);
                        if (TimeSpan.Parse(objCongNgay.InTime) > GioVao)
                        {
                            if (!CD)
                            {
                                objCongNgay.VaoTreVeSom = true;
                            }
                            GioVao = TimeSpan.Parse(objCongNgay.InTime);
                            if (ns.DuocDiTre == true)
                            {
                                PhutDuocTre = Convert.ToInt32(ns.SoPhut);
                                if ((TimeSpan.Parse(objCongNgay.InTime) - TimeSpan.Parse(objCongNgay.GioVaoChuan)).TotalMinutes <= PhutDuocTre)
                                {
                                    GioVao = TimeSpan.Parse(objCongNgay.GioVaoChuan);
                                }
                            }
                        }

                        GioRa = TimeSpan.Parse(objCongNgay.GioRaChuan);
                        if (TimeSpan.Parse(objCongNgay.OutTime) < GioRa)
                        {
                            if (!CD)
                            {
                                objCongNgay.VaoTreVeSom = true;
                            }
                            GioRa = TimeSpan.Parse(objCongNgay.OutTime);
                        }
                    }
                    objCongNgay.GioQuyDoi = Math.Floor((GioRa - GioVao).TotalHours);
                    if (objCongNgay.GioQuyDoi < 0)
                    {
                        objCongNgay.GioQuyDoi = 0;
                    }
                    objCongNgay.PhutQuyDoi = Math.Floor((GioRa - GioVao).TotalMinutes - (objCongNgay.GioQuyDoi * 60) + 0.5);
                    if (objCongNgay.PhutQuyDoi < 0)
                    {
                        objCongNgay.PhutQuyDoi = 0;
                    }
                    if (CD == true)//Ca đêm
                    {
                        TTF_FACEIDEntities db = new TTF_FACEIDEntities();
                        var CaDem = db.TTF_CaLamViec.FirstOrDefault(it => it.MaCaLamViec.Trim() == "CD");
                        db.Dispose();
                        //objCongNgay.GioVaoChuan = CaDem.GioBacDau.Value.ToString();
                        //objCongNgay.GioRaChuan = CaDem.GioKetThuc.Value.ToString();
                        CDGioVao = DateTime.ParseExact(objCongNgay.InDate + " " + objCongNgay.InTime, "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));
                        CDGioRa = DateTime.ParseExact(objCongNgay.OutDate + " " + objCongNgay.OutTime, "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));
                        CDGioVaoChuan = DateTime.ParseExact(ngay.ToString("dd/MM/yyyy") + " " + objCongNgay.GioVaoChuan, "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));
                        CDGioRaChuan = DateTime.ParseExact(objCongNgay.OutDate + " " + objCongNgay.GioRaChuan, "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));

                        if (TimeSpan.Parse(objCongNgay.InTime) > CaDem.GioBacDau.Value || TimeSpan.Parse(objCongNgay.OutTime) < CaDem.GioKetThuc.Value)
                        {
                            objCongNgay.VaoTreVeSom = true;
                        }
                        // truong hop ra truoc 12 gio dem
                        if (CDGioVaoChuan.Date == CDGioRaChuan.Date)
                        {
                            CDGioRaChuan = CDGioRaChuan.AddDays(1);
                        }

                        objCongNgay.TongGioCong = (CDGioRa - CDGioVao).TotalHours;
                        objCongNgay.TongGioCong = Math.Round(objCongNgay.TongGioCong, 2);
                        if (objCongNgay.TongGioCong < 0)
                        {
                            objCongNgay.TongGioCong = 0;
                        }
                        CDGioVaoFinal = CDGioVao;
                        if (CDGioVaoChuan > CDGioVao)
                        {
                            CDGioVaoFinal = CDGioVaoChuan;
                        }
                        CDGioRaFinal = CDGioRa;
                        if (CDGioRaChuan < CDGioRa)
                        {
                            CDGioRaFinal = CDGioRaChuan;
                        }
                        objCongNgay.Cong = (CDGioRaFinal - CDGioVaoFinal).TotalHours;
                        objCongNgay.Cong = Math.Round(objCongNgay.Cong, 2);
                        if (CDGioRa <= CDGioRaChuan)
                        {
                            //Nếu Giờ ra thực tế - Giờ ra chuẩn <= 0 -> = 0
                            objCongNgay.NgoaiGioHC = 0;
                        }
                        else
                        {
                            //Ngược lại = (Giờ ra thực tế - Giờ ra chuẩn) -> Quy ra giờ, tính tới phút
                            objCongNgay.NgoaiGioHC = (CDGioRa - CDGioRaChuan).TotalHours;
                        }
                        objCongNgay.NgoaiGioHC = Math.Round(objCongNgay.NgoaiGioHC, 2);
                        //objCongNgay.TangCa = objCongNgay.NgoaiGioHC;

                        if (objCongNgay.InTime == objCongNgay.OutTime)
                        {
                            objCongNgay.ThieuVanTay = true;
                        }
                        if (MaCaLamViec.Trim() == "Ca8_T1")
                        {
                            objCongNgay.Cong = objCongNgay.TongGioCong;
                            if (objCongNgay.Cong > 8)
                            {
                                objCongNgay.Cong = 8;
                                objCongNgay.SoGioTangCa = objCongNgay.TongGioCong - 8;
                                objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                if (objCongNgay.TangCa > 4)
                                {
                                    objCongNgay.TangCa = 4;
                                }
                            }
                            else if (objCongNgay.Cong < 8 && objCongNgay.NgoaiGioHC > 0) // cong khong trên 8 giờ mà có ngoài giờ hành chính thì không tính ngoài giờ hành chính và tăng ca
                            {
                                objCongNgay.NgoaiGioHC = 0;
                                objCongNgay.TangCa = 0;
                            }
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                objCongNgay.TangCaSau22H = objCongNgay.Cong;
                                objCongNgay.Cong = 0.0;
                            }

                            if (objCongNgay.ThieuVanTay == true)
                            {
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(19, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(22, 0, 0))
                                {
                                    //Thiếu ra
                                    objCongNgay.Cong = 0;
                                    objCongNgay.NgoaiGioHC = 0;
                                    objCongNgay.TangCa = 0;
                                }
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(3, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(8, 0, 0))
                                {
                                    //Thiếu vào

                                    objCongNgay.Cong = 0;
                                }
                            }
                        }
                        else if (MaCaLamViec.Trim() == "Ca12_T1")
                        {
                            objCongNgay.Cong = objCongNgay.TongGioCong;
                            if (objCongNgay.Cong > 12)
                            {
                                objCongNgay.Cong = 12;
                            }
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                objCongNgay.TangCa = objCongNgay.Cong;
                                objCongNgay.Cong = 0.0;
                            }

                            if (objCongNgay.ThieuVanTay == true)
                            {
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(19, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(22, 0, 0))
                                {
                                    //Thiếu ra
                                    objCongNgay.Cong = 0;
                                    objCongNgay.NgoaiGioHC = 0;
                                    objCongNgay.TangCa = 0;
                                }
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(3, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(8, 0, 0))
                                {
                                    //Thiếu vào
                                    objCongNgay.Cong = 0;
                                }
                            }
                        }
                        else if (MaCaLamViec.Trim() == "Ca1" || MaCaLamViec.Trim() == "Ca2")
                        {
                            objCongNgay.Cong = objCongNgay.TongGioCong - 1.5;
                            if (objCongNgay.Cong < 0)
                            {
                                objCongNgay.Cong = 0;
                            }
                            if (objCongNgay.Cong > 8 || ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                objCongNgay.Cong = ngay.DayOfWeek != DayOfWeek.Sunday ? 8 : objCongNgay.Cong;
                                objCongNgay.SoGioTangCa = ngay.DayOfWeek != DayOfWeek.Sunday ? objCongNgay.TongGioCong - 8 : 0;

                                CDNgayCheckTC = ngay.DayOfWeek == DayOfWeek.Sunday ? ngay : ngay.Date.AddDays(1);
                                Proc_TangCaChiTiet_Result CheckTangCa = null;
                                if (ngay.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    CheckTangCa = TangCa.Where(o => o.NhanSu == ns.NhanSu
                                    && o.NgayTangCa == ngay.Date && o.GioBatDau > new TimeSpan(18, 59, 0) && o.GioBatDau < new TimeSpan(20, 05, 00)).FirstOrDefault();
                                }
                                else
                                {
                                    // lấy các trường họp tang ca cua ca không phai la chu nhat
                                    CheckTangCa = TangCa.Where(o => o.NhanSu == ns.NhanSu
                                    && o.NgayTangCa == CDNgayCheckTC.Date && o.GioBatDau <= new TimeSpan(6, 01, 00)).FirstOrDefault();
                                }
                                if (CheckTangCa != null && ns.KhongTinhTC == false)//Tồn tại yêu cầu tăng ca đã Duyệt hoàn tất
                                {
                                    // ngày chủ nhật lấy giờ vào là 
                                    CDGioVaoTCTT = ngay.DayOfWeek == DayOfWeek.Sunday ? CDGioVaoFinal : CDGioVaoFinal.AddHours(9.5);

                                    CDGioRaTCTT = CDGioRa;

                                    CDGioVaoTCPhieu = new DateTime(CheckTangCa.NgayTangCa.Value.Year,
                                        CheckTangCa.NgayTangCa.Value.Month,
                                        CheckTangCa.NgayTangCa.Value.Day,
                                        CheckTangCa.GioBatDau.Value.Hours,
                                        CheckTangCa.GioBatDau.Value.Minutes,
                                        CheckTangCa.GioBatDau.Value.Seconds);
                                    // nếu là chủ nhật Add thêm 1 ngày đề lấy giờ ra
                                    DateTime dateTemp = CDGioVao.Date.AddDays(1);
                                    CDGioRaTCPhieu = ngay.DayOfWeek == DayOfWeek.Sunday ?
                                        new DateTime(dateTemp.Year,
                                        dateTemp.Month,
                                        dateTemp.Day,
                                        CheckTangCa.GioKetThuc.Value.Hours,
                                        CheckTangCa.GioKetThuc.Value.Minutes,
                                        CheckTangCa.GioKetThuc.Value.Seconds)
                                        :
                                        new DateTime(CheckTangCa.NgayTangCa.Value.Year,
                                        CheckTangCa.NgayTangCa.Value.Month,
                                        CheckTangCa.NgayTangCa.Value.Day,
                                        CheckTangCa.GioKetThuc.Value.Hours,
                                        CheckTangCa.GioKetThuc.Value.Minutes,
                                        CheckTangCa.GioKetThuc.Value.Seconds);
                                    CDGioVaoTCFinal = CDGioVaoTCTT;
                                    if (CDGioVaoTCPhieu > CDGioVaoTCFinal)
                                    {
                                        CDGioVaoTCFinal = CDGioVaoTCPhieu;
                                    }
                                    CDGioRaTCFinal = CDGioRaTCTT;
                                    if (CDGioRaTCPhieu < CDGioRaTCFinal)
                                    {
                                        CDGioRaTCFinal = CDGioRaTCPhieu;
                                    }
                                    objCongNgay.TangCa = Math.Round((CDGioRaTCFinal - CDGioVaoTCFinal).TotalHours, 2);
                                    if (objCongNgay.TangCa < 0)
                                    {
                                        objCongNgay.TangCa = 0;
                                    }
                                }
                            }
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                //objCongNgay.TangCa = objCongNgay.TangCa + objCongNgay.Cong;
                                // objCongNgay.TangCa = objCongNgay.Cong;
                                objCongNgay.SoGioTangCa = objCongNgay.TangCa;
                                if (objCongNgay.TangCa > 4)
                                {
                                    objCongNgay.TangCa = Math.Round(objCongNgay.TangCa - 1.5, 2);
                                }
                                objCongNgay.Cong = 0.0;
                            }
                            if (objCongNgay.ThieuVanTay == true)
                            {
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(19, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(22, 0, 0))
                                {
                                    //Thiếu ra
                                    objCongNgay.Cong = 0;
                                    objCongNgay.NgoaiGioHC = 0;
                                    objCongNgay.TangCa = 0;
                                }
                                if (TimeSpan.Parse(objCongNgay.InTime) >= new TimeSpan(3, 0, 0) && TimeSpan.Parse(objCongNgay.InTime) <= new TimeSpan(8, 0, 0))
                                {
                                    //Thiếu vào
                                    objCongNgay.Cong = 0;
                                }
                            }
                        }

                        //if (objCongNgay.InDate == objCongNgay.OutDate && objCongNgay.InTime == objCongNgay.OutTime)
                        //{
                        //    objCongNgay.OutDate = "";
                        //    objCongNgay.OutTime = "";
                        //}
                    }
                    else // tính công ca ngày
                    {
                        if (GioRa <= ns.GioRaGiuaCa || GioVao >= ns.GioVaoGiuaCa)
                            ThoiGianTru = 0;
                        else if (GioVao >= ns.GioRaGiuaCa && GioVao <= ns.GioVaoGiuaCa)
                        {
                            ThoiGianTru = (ns.GioVaoGiuaCa.Value - GioVao).TotalHours;
                        }
                        else if (GioRa >= ns.GioRaGiuaCa && GioRa <= ns.GioVaoGiuaCa)
                        {
                            ThoiGianTru = (GioRa - ns.GioRaGiuaCa.Value).TotalHours;
                        }
                        else
                        {
                            ThoiGianTru = (ns.GioVaoGiuaCa.Value - ns.GioRaGiuaCa.Value).TotalHours;
                        }
                        objCongNgay.TongGioCong = (GioRa - GioVao).TotalHours;
                        if (MaCaLamViec.Trim() == "Ca8_T1" || MaCaLamViec.Trim() == "Ca12_T1")
                        {
                            objCongNgay.TongGioCong = (TimeSpan.Parse(objCongNgay.OutTime) - GioVao).TotalHours;
                        }
                        dCongDu = Math.Round((ns.GioKetThucCa.Value - ns.GioBatDauCa.Value).TotalHours - (ns.SoPhutDiTre.Value * 1.0 / 60), 2);
                        // ThoiGianTru = Math.Round(ThoiGianTru, 2);
                        dCong = Math.Round(objCongNgay.TongGioCong, 2);
                        objCongNgay.TongGioCong = Math.Round(objCongNgay.TongGioCong, 2);
                        if (objCongNgay.TongGioCong < 0)
                        {
                            objCongNgay.TongGioCong = 0;
                        }
                        if (MaCaLamViec == "Ca1")//Nếu nhóm 1:
                        {
                            //objCongNgay.TongGioCong >= 8.33
                            if (objCongNgay.TongGioCong >= dCongDu)
                            {
                                objCongNgay.Cong = 8;
                            }
                            else
                            {
                                objCongNgay.Cong = Math.Round(dCong - ThoiGianTru, 2);
                            }

                            //if (objCongNgay.TongGioCong >= 8.33)//Nếu Tổng giờ công >= 8.33 -> 8
                            //{
                            //    objCongNgay.Cong = 8;
                            //}
                            //else if (objCongNgay.TongGioCong <= 4)//Nếu Tổng giờ công <= 4 -> = Tổng giờ công
                            //{
                            //    objCongNgay.Cong = objCongNgay.TongGioCong;
                            //}
                            //else if (TimeSpan.Parse(objCongNgay.OutTime) <= new TimeSpan(12, 0, 0))
                            //{
                            //    //Nếu Giờ ra thực tế <=12 -> 4.5
                            //    objCongNgay.Cong = 4.5;
                            //}
                            //else if (TimeSpan.Parse(objCongNgay.InTime) > new TimeSpan(11, 0, 0) && objCongNgay.TongGioCong < 4.5)
                            //{
                            //    //Nếu Giờ vào thực tế > 11 và Tổng giờ công < 4.5 -> = 4
                            //    objCongNgay.Cong = 4;
                            //}
                            //else//Ngược lại -> Tổng công / 8.5 * 8
                            //{
                            //    objCongNgay.Cong = (objCongNgay.TongGioCong / 8.5 * 8) > 8 ? 8 : (objCongNgay.TongGioCong / 8.5 * 8);
                            //}
                        }
                        else if (MaCaLamViec == "Ca2")
                        {
                            if (objCongNgay.TongGioCong >= dCongDu)//Nếu Tổng giờ công >= 8.83 -> 8
                            {
                                objCongNgay.Cong = 8;
                            }
                            else //Nếu Tổng giờ công <= 4 -> = Tổng giờ công
                            {
                                objCongNgay.Cong = Math.Round(dCong - ThoiGianTru, 2);
                            }
                            //if (objCongNgay.TongGioCong >= 8.83)//Nếu Tổng giờ công >= 8.83 -> 8
                            //{
                            //    objCongNgay.Cong = 8;
                            //}
                            //else if (objCongNgay.TongGioCong <= 4)//Nếu Tổng giờ công <= 4 -> = Tổng giờ công
                            //{
                            //    objCongNgay.Cong = objCongNgay.TongGioCong;
                            //}
                            //else if (TimeSpan.Parse(objCongNgay.OutTime) <= new TimeSpan(12, 0, 0))
                            //{
                            //    //Nếu Giờ ra thực tế <= 12 -> = 4
                            //    objCongNgay.Cong = 4;
                            //}
                            //else if (TimeSpan.Parse(objCongNgay.InTime) > new TimeSpan(11, 0, 0) && objCongNgay.TongGioCong < 4.5)
                            //{
                            //    //Nếu Giờ vào thực tế > 11 và Tổng giờ công < 4.5 -> = 4
                            //    objCongNgay.Cong = 4;
                            //}
                            //else//Ngược lại -> Tổng công / 9 * 8
                            //{
                            //    objCongNgay.Cong = (objCongNgay.TongGioCong / 9 * 8) > 8 ? 8 : (objCongNgay.TongGioCong / 9 * 8);
                            //}
                        }
                        else if (MaCaLamViec.Trim() == "Ca8_T1")
                        {
                            objCongNgay.Cong = objCongNgay.TongGioCong;
                            if (objCongNgay.Cong > 8)
                            {
                                objCongNgay.Cong = 8;
                                objCongNgay.SoGioTangCa = objCongNgay.TongGioCong - 8;
                                objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                if (objCongNgay.TangCa > 4)
                                {
                                    objCongNgay.TangCa = 4;
                                }
                            }
                        }
                        else if (MaCaLamViec.Trim() == "Ca12_T1")
                        {
                            objCongNgay.Cong = objCongNgay.TongGioCong;
                            if (objCongNgay.Cong > 12)
                            {
                                objCongNgay.Cong = 12;
                            }
                        }

                        if (ns.MaLoaiDacBiet == 2 && (ns.NgayBatDau == null || ngay >= ns.NgayBatDau))//Làm nhiêu tính nhiêu
                        {
                            //objCongNgay.Cong = objCongNgay.TongGioCong - 0.5 ;//-30 phút nghỉ trưa
                            objCongNgay.Cong = objCongNgay.TongGioCong - ThoiGianTru;//-30 phút nghỉ trưa
                            if (objCongNgay.TongGioCong >= dCongDu)//Nếu Tổng giờ công >= 8.33 -> 8
                            {
                                objCongNgay.Cong = 8;
                            }
                        }
                        objCongNgay.Cong = Math.Round(objCongNgay.Cong, 2);
                        if (objCongNgay.Cong < 0)
                        {
                            objCongNgay.Cong = 0;
                        }
                        if (TimeSpan.Parse(objCongNgay.OutTime) <= TimeSpan.Parse(objCongNgay.GioRaChuan))
                        {
                            //Nếu Giờ ra thực tế - Giờ ra chuẩn <= 0 -> = 0
                            objCongNgay.NgoaiGioHC = 0;
                        }
                        else
                        {
                            //Ngược lại = (Giờ ra thực tế - Giờ ra chuẩn) -> Quy ra giờ, tính tới phút
                            objCongNgay.NgoaiGioHC = (TimeSpan.Parse(objCongNgay.OutTime) - TimeSpan.Parse(objCongNgay.GioRaChuan)).TotalHours;
                        }
                        if (objCongNgay.NgoaiGioHC < 0)
                        {
                            objCongNgay.NgoaiGioHC = 0;
                        }
                        objCongNgay.NgoaiGioHC = Math.Round(objCongNgay.NgoaiGioHC, 2);

                        var ListTangCa = (TangCa.Where(o => o.NhanSu == ns.NhanSu
                                        && o.NgayTangCa == ngay.Date
                                        )).OrderBy(it => it.GioBatDau).OrderBy(p => p.GioKetThuc).ToList();
                        if (ListTangCa.Count > 0)
                        {
                            if (ListTangCa.Count == 1)
                            {
                                var CheckTangCa = TangCa.Where(o => o.NhanSu == ns.NhanSu
                                    && o.NgayTangCa == ngay.Date).FirstOrDefault();
                                if (CheckTangCa != null && ns.KhongTinhTC == false)//Tồn tại yêu cầu tăng ca đã Duyệt hoàn tất
                                {
                                    objCongNgay.CoYCTangCa = true;
                                    if (ngay.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        if (GioVao < (TimeSpan)ns.GioBatDauCa)
                                            GioVao = (TimeSpan)ns.GioBatDauCa;
                                        if (ns.MaLoaiDacBiet == 2 && (ns.NgayBatDau == null || ngay >= ns.NgayBatDau))
                                        {
                                            GioVao = TimeSpan.Parse(objCongNgay.InTime);
                                        }
                                        if (CheckTangCa.GioBatDau.Value > GioVao)
                                        {
                                            GioVao = CheckTangCa.GioBatDau.Value;
                                        }
                                        GioRa = TimeSpan.Parse(objCongNgay.OutTime);
                                        if (CheckTangCa.GioKetThuc.Value < GioRa)
                                        {
                                            GioRa = CheckTangCa.GioKetThuc.Value;
                                        }

                                        if (MaCaLamViec.Trim() == "Ca1")
                                        {
                                            dCong = (GioRa - GioVao).TotalHours;
                                            //dCong >= dCongDu && dCong <= 8.5
                                            if (dCong >= dCongDu && dCong <= ns.TongGioCong)
                                            {
                                                objCongNgay.SoGioTangCa = 8;
                                            }
                                            else
                                            {
                                                objCongNgay.SoGioTangCa = dCong - ThoiGianTru;
                                            }
                                            //if (dCong >= 8.33 && dCong <= 8.5)//Nếu Tổng giờ công >= 8.33 -> 8
                                            //{
                                            //    objCongNgay.SoGioTangCa = 8;
                                            //}
                                            //else if (dCong > 8.5)
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong - 0.5;
                                            //}
                                            //else if (dCong <= 4)//Nếu Tổng giờ công <= 4 -> = Tổng giờ công
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong;
                                            //}
                                            //else if (TimeSpan.Parse(objCongNgay.OutTime) <= new TimeSpan(12, 0, 0))
                                            //{
                                            //    //Nếu Giờ ra thực tế <=12 -> 4.5
                                            //    objCongNgay.SoGioTangCa = 4.5;
                                            //}
                                            //else if (TimeSpan.Parse(objCongNgay.InTime) > new TimeSpan(11, 0, 0) && dCong < 4.5)
                                            //{
                                            //    //Nếu Giờ vào thực tế > 11 và Tổng giờ công < 4.5 -> = 4
                                            //    objCongNgay.SoGioTangCa = 4;
                                            //}
                                            //else//Ngược lại -> Tổng công / 8.5 * 8
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong / 8.5 * 8;
                                            //}
                                            //objCongNgay.SoGioTangCa = (GioRa - GioVao).TotalHours - 30
                                        }
                                        else if (MaCaLamViec.Trim() == "Ca2")
                                        {
                                            dCong = (GioRa - GioVao).TotalHours;
                                            if (dCong >= dCongDu && dCong <= ns.TongGioCong.Value) //Nếu Tổng giờ công >= 8.83 -> 8
                                            {
                                                objCongNgay.SoGioTangCa = 8;
                                            }
                                            else
                                            {
                                                objCongNgay.SoGioTangCa = dCong - ThoiGianTru;
                                            }

                                            //if (dCong >= 8.83 && dCong <= 9) //Nếu Tổng giờ công >= 8.83 -> 8
                                            //{
                                            //    objCongNgay.SoGioTangCa = 8;
                                            //}
                                            //else if (dCong > 9)
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong - 1;
                                            //}
                                            //else if (dCong <= 4)//Nếu Tổng giờ công <= 4 -> = Tổng giờ công
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong;
                                            //}
                                            //else if (TimeSpan.Parse(objCongNgay.OutTime) <= new TimeSpan(12, 0, 0))
                                            //{
                                            //    //Nếu Giờ ra thực tế <= 12 -> = 4
                                            //    objCongNgay.SoGioTangCa = 4;
                                            //}
                                            //else if (TimeSpan.Parse(objCongNgay.InTime) > new TimeSpan(11, 0, 0) && dCong < 4.5)
                                            //{
                                            //    //Nếu Giờ vào thực tế > 11 và Tổng giờ công < 4.5 -> = 4
                                            //    objCongNgay.SoGioTangCa = 4;
                                            //}
                                            //else//Ngược lại -> Tổng công / 9 * 8
                                            //{
                                            //    objCongNgay.SoGioTangCa = dCong / 9 * 8;
                                            //}
                                            //objCongNgay.SoGioTangCa = (GioRa - GioVao).TotalHours - 1;//-60 phút nghỉ trưa
                                        }
                                        else
                                        {

                                        }
                                        objCongNgay.SoGioTangCa = Math.Round(objCongNgay.SoGioTangCa, 2);
                                        if (objCongNgay.SoGioTangCa < 0)
                                        {
                                            objCongNgay.SoGioTangCa = 0;
                                        }
                                    }
                                    else // tang ca ko phai chu nhat
                                    {
                                        GioVao = CheckTangCa.GioBatDau.Value;
                                        if (ns.MaLoaiDacBiet == 2 && (ns.NgayBatDau == null || ngay >= ns.NgayBatDau))
                                        {
                                            // gio vao doi voi ca ca dat biet 
                                            tsTemp = TimeSpan.FromHours(objCongNgay.Cong + 0.5);
                                            tsTemp = TimeSpan.Parse(objCongNgay.InTime).Add(tsTemp);
                                            GioVao = tsTemp;
                                            if (objCongNgay.Cong < 8 && ns.CanGioRa != null)
                                            {
                                                GioVao = ns.CanGioRa.Value;
                                            }
                                        }
                                        if (TimeSpan.Parse(objCongNgay.GioRaChuan) > CheckTangCa.GioBatDau.Value)//Giờ ra chuẩn chính là Giờ bắt đầu tính tăng ca
                                        {
                                            GioVao = TimeSpan.Parse(objCongNgay.GioRaChuan);
                                        }
                                        GioRa = CheckTangCa.GioKetThuc.Value;
                                        if (TimeSpan.Parse(objCongNgay.OutTime) < CheckTangCa.GioKetThuc.Value)
                                        {
                                            GioRa = TimeSpan.Parse(objCongNgay.OutTime);
                                        }
                                        objCongNgay.SoGioTangCa = (GioRa - GioVao).TotalHours;
                                        objCongNgay.SoGioTangCa = Math.Round(objCongNgay.SoGioTangCa, 2);
                                        if (objCongNgay.SoGioTangCa < 0)
                                        {
                                            objCongNgay.SoGioTangCa = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    ////Ca8_T1 không check phiếu tăng ca, tăng ca = ngoài giờ HC, công tối đa 8, tăng ca tối đa 4
                                    //if (MaCaLamViec.Trim() == "Ca8_T1")
                                    //{
                                    //    objCongNgay.NgoaiGioHC = (TimeSpan.Parse(objCongNgay.OutTime) - TimeSpan.Parse(objCongNgay.GioRaChuan)).TotalHours;
                                    //    objCongNgay.SoGioTangCa = objCongNgay.NgoaiGioHC;
                                    //    objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                    //    if(objCongNgay.TangCa > 4)
                                    //    {
                                    //        objCongNgay.TangCa = 4;
                                    //    }
                                    //}
                                }
                            }
                            else//Xử lý 1 ngày nhiều yêu cầu tăng ca
                            {
                                //1. Xác định 2 cận của tăng ca: cận trên = giờ ra theo ca làm việc (VD: ca1: 16h), 
                                //cận dưới = max giờ quét vân tay
                                //2. Duyệt qua các yêu cầu tăng ca, so sánh giờ vào, giờ ra của phiếu với cận trên, 
                                //cận dưới của 1, giờ vào = giờ vào lớn hơn, giờ ra = giờ ra nhỏ hơn -> sum các khoảng thời gian này
                                double SoGioTangCa = 0.0;
                                if (ngay.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    CanTrenTC = (TimeSpan)ns.GioBatDauCa;
                                    if (GioVao > CanTrenTC)
                                        CanTrenTC = GioVao;
                                    if (ns.MaLoaiDacBiet == 2)
                                    {
                                        CanTrenTC = TimeSpan.Parse(objCongNgay.InTime);
                                    }
                                }
                                else
                                {
                                    CanTrenTC = TimeSpan.Parse(objCongNgay.GioRaChuan);
                                    if (ns.MaLoaiDacBiet == 2 && (ns.NgayBatDau == null || ngay >= ns.NgayBatDau))
                                    {
                                        // gio vao doi voi ca ca dat biet 
                                        tsTemp = TimeSpan.FromHours(objCongNgay.Cong + 0.5);
                                        tsTemp = TimeSpan.Parse(objCongNgay.InTime).Add(tsTemp);
                                        CanTrenTC = tsTemp;
                                        if (objCongNgay.Cong < 8 && ns.CanGioRa != null)
                                        {
                                            CanTrenTC = ns.CanGioRa.Value;
                                        }
                                    }
                                }
                                CanDuoiTC = TimeSpan.Parse(objCongNgay.OutTime);
                                int iTem = 0;
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
                                    if (ngay.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        if (MaCaLamViec.Trim() == "Ca1")
                                        {
                                            if (GioVao < ns.GioKetThucCa && GioRa > GioVao)
                                            {
                                                SoGioTangCa += (GioRa - GioVao).TotalHours - ThoiGianTru;//-30 phút nghỉ trưa
                                            }
                                            else if (GioVao >= ns.GioKetThucCa && GioRa > GioVao)
                                            {
                                                SoGioTangCa += (GioRa - GioVao).TotalHours;//không trừ giờ nghĩ trưa sau giờ kết thúc phút nghỉ trưa
                                            }

                                        }
                                        else if (MaCaLamViec.Trim() == "Ca2")
                                        {
                                            if (GioVao < ns.GioKetThucCa && GioRa > GioVao)
                                            {
                                                SoGioTangCa += (GioRa - GioVao).TotalHours - ThoiGianTru;//-60 phút nghỉ trưa
                                            }
                                            else if (GioVao >= ns.GioKetThucCa && GioRa > GioVao)
                                            {
                                                SoGioTangCa += (GioRa - GioVao).TotalHours;//-không trừ giờ nghĩ trưa sau giờ kết thúc phút nghỉ trưa
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {
                                        if (GioRa > GioVao)
                                            SoGioTangCa += (GioRa - GioVao).TotalHours;
                                    }
                                    iTem++;
                                }
                                objCongNgay.SoGioTangCa = Math.Round(SoGioTangCa, 2);
                                if (objCongNgay.SoGioTangCa < 0)
                                {
                                    objCongNgay.SoGioTangCa = 0;
                                }
                            }
                        }

                        if (MaCaLamViec.Trim() == "Ca1")//Nhóm 1:
                        {
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                if (objCongNgay.SoGioTangCa > 14)//Nếu Số giờ tăng ca > 14 -> = 14
                                {
                                    objCongNgay.TangCa = 14;
                                    objCongNgay.TangCaSau22H = objCongNgay.SoGioTangCa - 14;//Nhóm 1 = Số giờ tăng ca – 14
                                }
                                else//Ngược lại = chính nó
                                {
                                    objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                }
                            }
                            else
                            {
                                if (objCongNgay.SoGioTangCa > 6)//Nếu Số giờ tăng ca > 6 -> = 6
                                {
                                    objCongNgay.TangCa = 6;
                                    objCongNgay.TangCaSau22H = objCongNgay.SoGioTangCa - 6;//Nhóm 1 = Số giờ tăng ca – 6
                                }
                                else//Ngược lại = chính nó
                                {
                                    objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                }
                            }
                        }
                        else if (MaCaLamViec.Trim() == "Ca2")//Nhóm 2:
                        {
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                if (objCongNgay.SoGioTangCa > 13.5)//Nếu Số giờ tăng ca > 13.5 -> = 13.5
                                {
                                    objCongNgay.TangCa = 13.5;
                                    objCongNgay.TangCaSau22H = objCongNgay.SoGioTangCa - 13.5;//Nhóm 1 = Số giờ tăng ca – 13.5
                                }
                                else//Ngược lại = chính nó
                                {
                                    objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                }
                            }
                            else
                            {
                                if (objCongNgay.SoGioTangCa > 5.5)//Nếu Số giờ tăng ca > 5.5 -> = 5.5
                                {
                                    objCongNgay.TangCa = 5.5;
                                    objCongNgay.TangCaSau22H = objCongNgay.SoGioTangCa - 5.5;//Nhóm 2 = Số giờ tăng ca – 5.5
                                }
                                else//Ngược lại = chính nó
                                {
                                    objCongNgay.TangCa = objCongNgay.SoGioTangCa;
                                }
                            }
                        }
                        else if (MaCaLamViec.Trim() == "Ca8_T1" || MaCaLamViec.Trim() == "Ca12_T1")
                        {
                            if (ngay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                objCongNgay.TangCa = objCongNgay.Cong + objCongNgay.TangCa;
                                objCongNgay.Cong = 0.0;
                            }
                        }
                        else
                        {

                        }
                        if (objCongNgay.TangCa < 0.5)
                        {
                            objCongNgay.TangCa = 0;//Chỉ tính tăng ca từ 30 phút trở lên
                        }
                        if (ngay.DayOfWeek == DayOfWeek.Sunday)
                        {
                            objCongNgay.Cong = 0;
                        }
                    }
                    if (objCongNgay.InTime == objCongNgay.OutTime)
                    {

                        objCongNgay.OutTime = "";
                    }

                }
                else
                {
                    //if (objCongNgay.OutTime.Trim() == "")
                    //{
                    //    objCongNgay.OutDate = "";
                    //}
                }

                //Xử lý trường hợp thuộc nhóm Không cần chấm công
                //if (clsFunction.NhanSuKhongCanChamCong(ns.MaNV) == true)
                if (ns.MaLoaiDacBiet == 1)
                {
                    if (ngay.Date < DateTime.Now.Date)
                    {
                        if (ns.NgayCongChuan == 24)
                        {
                            if (ngay.DayOfWeek != DayOfWeek.Saturday && ngay.DayOfWeek != DayOfWeek.Sunday)//Thứ 2->6
                            {
                                objCongNgay.Cong = 8;
                            }
                            else if (ngay.DayOfWeek == DayOfWeek.Saturday)//thứ 7
                            {
                                //if (objCongNgay.TuanThuMayCuaThang == 1 || objCongNgay.TuanThuMayCuaThang == 3)
                                if (objCongNgay.Thu7LanMay == 1 || objCongNgay.Thu7LanMay == 3)
                                {
                                    //Thứ 7 tuần 1 và 3
                                    objCongNgay.Cong = 8;
                                }
                            }
                        }
                        else if (ns.NgayCongChuan == 26)
                        {
                            if (ngay.DayOfWeek != DayOfWeek.Sunday)//Thứ 2->7
                            {
                                objCongNgay.Cong = 8;
                            }
                        }
                    }
                    objCongNgay.ThieuVanTay = false;
                }

                //Nếu đã hiệu chỉnh thì ưu tiên lấy trong TTF_CongNgayHieuChinh
                objCongNgay.TrangThai = "Gốc";
                var checkcnhc = CongNgayHC.Where(o => o.MaNV == MSNV
                            && o.Date == ngay).FirstOrDefault();
                if (checkcnhc != null)
                {
                    if (checkcnhc.Cong != null && checkcnhc.Cong.Trim() != "")
                    {
                        objCongNgay.CongHC = checkcnhc.Cong.Trim();
                    }
                    else
                    {
                        objCongNgay.CongHC = "";
                    }
                    if (checkcnhc.TangCa != null)
                    {
                        objCongNgay.TangCaHC = checkcnhc.TangCa.Trim();
                    }
                    else
                    {
                        objCongNgay.TangCaHC = "";
                    }
                    if (checkcnhc.TangCaSau22H != null)
                    {
                        objCongNgay.TangCaSau22HHC = checkcnhc.TangCaSau22H.Trim();
                    }
                    else
                    {
                        objCongNgay.TangCaSau22HHC = "";
                    }
                    objCongNgay.TrangThai = "HC";
                }
                else
                {
                    objCongNgay.CongHC = "";
                    objCongNgay.TangCaHC = "";
                    objCongNgay.TangCaSau22HHC = "";
                }
                //if (CD == true && ngay.DayOfWeek != DayOfWeek.Sunday)
                //{
                //    objCongNgay.TangCaSau22HHC = "CD";
                //}
                if (CD == true)
                {
                    objCongNgay.TangCaSau22HHC = "CD";
                }
                objCongNgay.MaPhong_PhanXuong = ns.MaPhong_PhanXuong;

                if (objCongNgay.InTime != "" && objCongNgay.OutTime != "")
                {
                    if ((TimeSpan.Parse(objCongNgay.OutTime) - TimeSpan.Parse(objCongNgay.InTime)).TotalMinutes < 3)
                    {
                        QuetVTLienTiep = true;
                    }
                }
                if ((objCongNgay.InTime == "" && objCongNgay.OutTime != "")
                    || (objCongNgay.OutTime == "" && objCongNgay.InTime != "") || QuetVTLienTiep == true)
                {
                    if (ns.NgayCongChuan == 24)
                    {
                        if ((ngay.DayOfWeek != DayOfWeek.Sunday && ngay.DayOfWeek != DayOfWeek.Saturday)
                        || (ngay.DayOfWeek == DayOfWeek.Saturday && objCongNgay.Thu7LanMay == 1)
                        || (ngay.DayOfWeek == DayOfWeek.Saturday && objCongNgay.Thu7LanMay == 3))
                        {
                            objCongNgay.ThieuVanTay = true;
                        }
                    }
                    else
                    {
                        if (ngay.DayOfWeek != DayOfWeek.Sunday)
                        {

                            objCongNgay.ThieuVanTay = true;
                        }
                    }
                }
            }

            // kiem tra di tre ve som
            if (objCongNgay.SoGioTangCa > 0)
            {
                objCongNgay.SoGioTangCa = Math.Round(objCongNgay.SoGioTangCa, 2);
            }
            return objCongNgay;
        }
        public static string LayThuTiengViet(DateTime date)
        {
            string rv = "";
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    rv = "hai";
                    break;
                case DayOfWeek.Tuesday:
                    rv = "ba";
                    break;
                case DayOfWeek.Wednesday:
                    rv = "tư";
                    break;
                case DayOfWeek.Thursday:
                    rv = "năm";
                    break;
                case DayOfWeek.Friday:
                    rv = "sáu";
                    break;
                case DayOfWeek.Saturday:
                    rv = "bảy";
                    break;
                case DayOfWeek.Sunday:
                    rv = "cn";
                    break;

            }
            return rv;
        }
        public static int GetThu7LanMayCuaKyCong(DateTime thu7, List<TTF_TimekeepingPeriod> KyCong)
        {
            int rv = 0;
            DateTime ngay = new DateTime(1990, 1, 1);
            int Year;
            int Month;
            ObjectParameter YearParameter = new ObjectParameter("Year", "");
            ObjectParameter MonthParameter = new ObjectParameter("Month", "");
            try
            {
                using (var db = new TTF_FACEIDEntities())
                {
                    db.CheckNgayThuocKyCongNao(thu7.ToString("dd/MM/yyyy"), YearParameter, MonthParameter);
                    if (YearParameter.Value.ToString().Trim() != "")
                    {
                        Year = Convert.ToInt32(YearParameter.Value.ToString().Trim());
                        Month = Convert.ToInt32(MonthParameter.Value.ToString().Trim());
                        var period = db.TTF_TimekeepingPeriod.Where(o => o.Year == Year && o.Month == Month).FirstOrDefault();
                        if (period != null)
                        {
                            ngay = Convert.ToDateTime(period.FromDate);
                            rv = 1;
                            while (ngay <= period.ToDate.Value)
                            {
                                if (ngay == thu7)
                                {
                                    break;
                                }
                                if (ngay.DayOfWeek == DayOfWeek.Saturday)
                                {
                                    rv += 1;
                                }
                                ngay = ngay.AddDays(1);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                clsFunction.NhatkyLoi(DateTime.Now, "", ex.ToString(), "", MethodBase.GetCurrentMethod().Name);
            }
            finally
            {

            }
            return rv;
        }
        public static string GetMaNghiPhepForCong1(Proc_NhanSuForCong_Result nhansu, DateTime NgayCong, DateTime NgayCanTinh, List<NghiPhepForCong> NghiPhepForCong, List<TTF_NgayLe> ListNgayLe)
        {
            string MaLoaiNghiPhep = "";
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            string skq = "";
            // Kiem tra co có dang thử việc không.
            if (NgayCanTinh < nhansu.NgayVaoCongTy && NgayCanTinh.DayOfWeek != System.DayOfWeek.Sunday)
                skq = "M";
            else if (NgayCanTinh >= nhansu.NgayNghiViec && NgayCanTinh.DayOfWeek != System.DayOfWeek.Sunday)// kiểm tra có nghỉ việc chưa
                skq = "NV";
            if (skq != "")
                return skq;
            //var NgayLe = ListNgayLe.FirstOrDefault(it => it.Date == NgayCanTinh); // Kiem tra co phai ngay le ko
            //if (NgayLe != null && skq == "" && NgayCanTinh.DayOfWeek != System.DayOfWeek.Sunday)
            //    skq = "L";
            //if (skq == "")
            //{
            //    var NghiPhep = NghiPhepForCong.Where(o => o.Ngay == NgayCanTinh && o.NhanSu == nhansu.NhanSu).FirstOrDefault();
            //    if (NghiPhep != null)
            //        skq = NghiPhep.SoNgay == 0.5 ? NghiPhep.MaLoaiNghiPhep.Trim().ToString() + "/2" : NghiPhep.MaLoaiNghiPhep.Trim().ToString();

            //}

            var NghiPhep = NghiPhepForCong.Where(o => o.Ngay == NgayCanTinh && o.NhanSu == nhansu.NhanSu).FirstOrDefault();
            if (NghiPhep != null)
            {
                MaLoaiNghiPhep = NghiPhep.MaLoaiNghiPhep.Trim().ToString();
            }
            if (MaLoaiNghiPhep == "TS")
            {
                skq = MaLoaiNghiPhep;
            }
            else
            {
                var NgayLe = ListNgayLe.FirstOrDefault(it => it.Date == NgayCanTinh); // Kiem tra co phai ngay le ko
                if (NgayLe != null && skq == "" && NgayCanTinh.DayOfWeek != System.DayOfWeek.Sunday)
                    skq = "L";
                if (skq == "")
                {
                    if (MaLoaiNghiPhep != "")
                        skq = NghiPhep.SoNgay == 0.5 ? MaLoaiNghiPhep + "/2" : MaLoaiNghiPhep;

                }
            }

            return skq;
        }
        public static bool CheckDouble(string s)
        {
            bool rv = false;
            try
            {

                double price;
                bool isDouble = Double.TryParse(s, out price);
                if (isDouble)
                {
                    rv = true;
                }
            }
            catch { }
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
        public static void GetKyCong(int Year, int Month, ref DateTime FromDate, ref DateTime ToDate)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            try
            {

                var check = db.TTF_TimekeepingPeriod.Where(o => o.Year == Year
                    && o.Month == Month).FirstOrDefault();
                if (check != null)
                {
                    FromDate = Convert.ToDateTime(check.FromDate);
                    ToDate = Convert.ToDateTime(check.ToDate);
                }
            }
            catch
            {

            }
        }
    }
    public class NhanVien {
        public int? NhanSu { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
        public string TenPhongBan { get; set; }
        public string EMail { get; set; }
        public double? SoNgayPhepConlai { get; set; }
        public TimeSpan? GioVao { get; set; }
        public TimeSpan? GioRa { get; set; }
    }
}