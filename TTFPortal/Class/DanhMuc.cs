using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TTFPortal.Models;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace TTFPortal.Class
{
    public class DanhMuc
    {
        [OutputCache(Duration = int.MaxValue)]
        public static string DMPhongBan()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_PhongBan_PhanXuong.Where(it => it.Del != true).Select(it=>new { it.MaKhoi, MaPhong_PhanXuong = it.MaPhong_PhanXuong.Trim(),it.TenPhong_PhanXuong}).ToList();
                string MaPhongBan = "";
                var ng = Users.GetNguoiDung(System.Web.HttpContext.Current.User.Identity.Name);
                MaPhongBan = ng.MaPhongPhanXuong == null ? "" : ng.MaPhongPhanXuong;
                if (MaPhongBan != "HCNS" && !System.Web.HttpContext.Current.User.IsInRole("0=0"))
                {
                    int iNhanSu = ng.NhanSu;
                    List<string> listPhamVi = db.TTF_PhamVi.Where(it => it.NhanSu == iNhanSu).Select(o => o.MaPhong_PhanXuong.Trim()).ToList();
                    listPhamVi.Add(MaPhongBan.Trim());
                    model = model.Where(it => listPhamVi.Contains(it.MaPhong_PhanXuong.Trim())).ToList();

                }
                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMChuVu()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_ChucVu.Where(it => it.Del != true).ToList();
                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMTinhTrang()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_TinhTrang.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMCoSoLamViec()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_CoSoLamViec.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMKhoi()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_Khoi.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMLoaiLaoDong()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_LoaiLaoDong.Where(it=>it.Del!= true).ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMCaLamViec()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_CaLamViec.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        [OutputCache(Duration = int.MaxValue)]
        public static string DMNganHang()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_NganHang.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMBenhVien()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_BenhVienBHYT.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMQuocTich()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_QuocTich.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMDanToc()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_DanToc.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMTonGiao()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_TonGiao.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMTrinhDoHocVan()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_TrinhDoHocVan.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMCapQuanLyTrucTiep()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.v_capquanly.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMLoaiTangCa()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_LoaiTangCa.ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string DMLoaiNghiPhep()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_LoaiNghiPhep.Where(it=>it.MaLoaiNghiPhep != "L").ToList();

                return JsonConvert.SerializeObject(model);
            }
        }

        public static string DMDuAN()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_Mails.Where(it => it.Del != true).ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static string NhomNguoiDung()
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.HT_NHOMNGUOIDUNG.Where(it => it.ISDEL != true).ToList();

                return JsonConvert.SerializeObject(model);
            }
        }
        public static HT_NHOMNGUOIDUNG GetNhomNguoiDungByID(int id)
        {
            try
            {
                TTF_FACEIDEntities db = new TTF_FACEIDEntities();
                return Task.Run(() => db.HT_NHOMNGUOIDUNG.FirstOrDefault(it => it.NHOMNGUOIDUNG == id)).Result;
            }
            catch { return null; }
        }

        public static List<TTF_ThongBao_Result> ThongBao(int nhanSu, string tuNgay, string denNgay)
        {
            using (var db = new TTF_FACEIDEntities())
            {
                var model = db.TTF_ThongBao(tuNgay, denNgay, nhanSu).ToList();
                return model;
            }
        }
    }
    public class NhanSu
    {
        public int IDCapQuanLyTrucTiep { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
    }

}