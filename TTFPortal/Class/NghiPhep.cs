using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class NghiPhep
    {
        public long IDNghiPhep { get; set; }
        public Nullable<int> NhanSu { get; set; }
        public Nullable<System.DateTime> TuNgay { get; set; }
        public Nullable<System.DateTime> DenNgay { get; set; }
        public Nullable<double> SoNgayNghi { get; set; }
        public string MaLoaiNghiPhep { get; set; }
        public Nullable<int> NguoiTao { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public string MayTao { get; set; }
        public string MaTrangThaiDuyet { get; set; }
        public string LyDoNghi { get; set; }
        public string LyDoHuy { get; set; }
        public Nullable<int> IDNguoiDuyetKeTiep { get; set; }
        public Nullable<bool> Block { get; set; }
        public Nullable<System.DateTime> NgayBlock { get; set; }
        public Nullable<bool> Del { get; set; }
        public Nullable<int> NguoiThayDoiLanCuoi { get; set; }
        public Nullable<System.DateTime> NgayThayDoiLanCuoi { get; set; }
        public List<NghiPhepChiTiet> NPCT { get; set; }
        public string Error { get; set; }
        public string MaNhanVien { get; set; }
        public string HoVaTen { get; set; }
        public Nullable<double> SoNgayPhepDuocNghi { get; set; }
        public string TenPhong_PhanXuong { get; set; }
    }
    public class NghiPhepChiTiet
    {
        public long IDNghiPhep { get; set; }
        public System.DateTime Ngay { get; set; }
        public Nullable<double> SoNgay { get; set; }
        public Nullable<int> ChuKyCaLamViec { get; set; }
        public string GhiChu { get; set; }
        public bool? Check { get; set; }
        public System.String NgayS { get; set; }
    }

    public class NhanSuPhep
    {
        public Nullable<int> NhanSu { get; set; }
        public string MaNV { get; set; }
        public string MaChamCong { get; set; }
        public string HoVaTen { get; set; }
        public string TenChucVu { get; set; }
        public string MaLoaiLaoDong { get; set; }
        public string TenLoaiLaoDong { get; set; }
        public string TenBoPhan { get; set; }
        public Nullable<int> NgayCongChuan { get; set; }
        public System.DateTime? NgayVaoCongTy { get; set; }
        public System.DateTime? NgayNghiViec { get; set; }

    }

    public class NghiPhepTongHop
    {
        public Nullable<int> NhanSu { get; set; }
        public Nullable<double> SoNgayNghi { get; set; }
        public string MaLoaiNghiPhep { get; set; }
        public System.DateTime Ngay { get; set; }
        public Nullable<double> SoNgay { get; set; }
        public long IDNghiPhep { get; set; }
    }

    public class TongHopPhepThang
    {
        public List<p_PhepThang> TongHop { get; set; }
    }
    public partial class p_PhepThang
    {
        public string TenBoPhan { get; set; }
        public int NhanSu { get; set; }
        public string TenChucVu { get; set; }
        public string MaNV { get; set; }
        public string MaChamCong { get; set; }
        public string HoVaTen { get; set; }
        public string MaLoaiLaoDong { get; set; }
        public string TenLoaiLaoDong { get; set; }
        public string TinhTrang { get; set; }
        public decimal PhepDauKy { get; set; }
        public string NgayVaoCongTy { get; set; }
        public Nullable<System.DateTime> NgayNghiViec { get; set; }
        public int LamDu { get; set; }
        public string Ngay26 { get; set; }
        public string Ngay27 { get; set; }
        public string Ngay28 { get; set; }
        public string Ngay29 { get; set; }
        public string Ngay30 { get; set; }
        public string Ngay31 { get; set; }
        public string Ngay1 { get; set; }
        public string Ngay2 { get; set; }
        public string Ngay3 { get; set; }
        public string Ngay4 { get; set; }
        public string Ngay5 { get; set; }
        public string Ngay6 { get; set; }
        public string Ngay7 { get; set; }
        public string Ngay8 { get; set; }
        public string Ngay9 { get; set; }
        public string Ngay10 { get; set; }
        public string Ngay11 { get; set; }
        public string Ngay12 { get; set; }
        public string Ngay13 { get; set; }
        public string Ngay14 { get; set; }
        public string Ngay15 { get; set; }
        public string Ngay16 { get; set; }
        public string Ngay17 { get; set; }
        public string Ngay18 { get; set; }
        public string Ngay19 { get; set; }
        public string Ngay20 { get; set; }
        public string Ngay21 { get; set; }
        public string Ngay22 { get; set; }
        public string Ngay23 { get; set; }
        public string Ngay24 { get; set; }
        public string Ngay25 { get; set; }
        public decimal TongPhep { get; set; }
        public decimal TongRo_TS { get; set; }
        public Nullable<int> NgayCongChuan { get; set; }
        public decimal PhepConLaiCuoiKy { get; set; }
        public string Color { get; set; }

    }
    public class NhanSuNgayPhep
    {
        public int NhanSu { get; set; }
        public double SoNgayPhep { get; set; }
        public bool? LamDu { get; set; }
    }
    public class TTF_DuyetNghiPhep
    {
        public string LyDoNghi { get; set; }
        public Nullable<System.DateTime> TuNgay { get; set; }
        public Nullable<System.DateTime> DenNgay { get; set; }
        public Nullable<double> SoNgayNghi { get; set; }
        public string TenLoaiNghiPhep { get; set; }
        public long IDNghiPhep { get; set; }
        public string HoVaTen { get; set; }
        public string MaNV { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public string TenPhong_PhanXuong { get; set; }
    }
}