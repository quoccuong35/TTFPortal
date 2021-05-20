using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class CongThang
    {
        public bool check { get; set; }
        public Int32 STT { get; set; }
        public string TenPhong_PhanXuong { get; set; }
        public string TenToChuyen { get; set; }
        public string TenChucVu { get; set; }
        public int NhanSu { get; set; }
        public string MSNV { get; set; }
        public string MaChamCong { get; set; }
        public string HoVaTen { get; set; }
        public double SoNgayPhepConLai { get; set; }
        public Int32 NgayCongChuan { get; set; }
        public string MaLoaiCong { get; set; }
        public string TenLoaiCong { get; set; }
        public Int32 LoaiCongOrder { get; set; }
        public string Cot1 { get; set; }
        public string Cot2 { get; set; }
        public string Cot3 { get; set; }
        public string Cot4 { get; set; }
        public string Cot5 { get; set; }
        public string Cot6 { get; set; }
        public string Cot7 { get; set; }
        public string Cot8 { get; set; }
        public string Cot9 { get; set; }
        public string Cot10 { get; set; }
        public string Cot11 { get; set; }
        public string Cot12 { get; set; }
        public string Cot13 { get; set; }
        public string Cot14 { get; set; }
        public string Cot15 { get; set; }
        public string Cot16 { get; set; }
        public string Cot17 { get; set; }
        public string Cot18 { get; set; }
        public string Cot19 { get; set; }
        public string Cot20 { get; set; }
        public string Cot21 { get; set; }
        public string Cot22 { get; set; }
        public string Cot23 { get; set; }
        public string Cot24 { get; set; }
        public string Cot25 { get; set; }
        public string Cot26 { get; set; }
        public string Cot27 { get; set; }
        public string Cot28 { get; set; }
        public string Cot29 { get; set; }
        public string Cot30 { get; set; }
        public string Cot31 { get; set; }
        public string DateCot1 { get; set; }
        public string DateCot2 { get; set; }
        public string DateCot3 { get; set; }
        public string DateCot4 { get; set; }
        public string DateCot5 { get; set; }
        public string DateCot6 { get; set; }
        public string DateCot7 { get; set; }
        public string DateCot8 { get; set; }
        public string DateCot9 { get; set; }
        public string DateCot10 { get; set; }
        public string DateCot11 { get; set; }
        public string DateCot12 { get; set; }
        public string DateCot13 { get; set; }
        public string DateCot14 { get; set; }
        public string DateCot15 { get; set; }
        public string DateCot16 { get; set; }
        public string DateCot17 { get; set; }
        public string DateCot18 { get; set; }
        public string DateCot19 { get; set; }
        public string DateCot20 { get; set; }
        public string DateCot21 { get; set; }
        public string DateCot22 { get; set; }
        public string DateCot23 { get; set; }
        public string DateCot24 { get; set; }
        public string DateCot25 { get; set; }
        public string DateCot26 { get; set; }
        public string DateCot27 { get; set; }
        public string DateCot28 { get; set; }
        public string DateCot29 { get; set; }
        public string DateCot30 { get; set; }
        public string DateCot31 { get; set; }

        public double DLCong { get; set; }
        public double SoNgayCT_NB { get; set; }
        public double TongNC { get; set; }
        public double TongNCHC { get; set; }
        public double TCThuong { get; set; }
        public double TCSau22H { get; set; }
        public double TCCN { get; set; }
        public double TCCNSau22H { get; set; }
        public double TCLe { get; set; }
        public double PhepNam_Le_TNLD { get; set; }
        public double SoNgayLamDem { get; set; }
        public double ChuyenCan { get; set; }
        public double XNC5LThang { get; set; }
        public double NghiKhongPhep { get; set; }
        public int? SoNgayDiCongTac { get; set; }
        public int? SoLanDiTreVeSo { get; set; }

        public List<CongThangTheoNgayModel> ListCong { get; set; }
        public List<CongThangTheoNgayModel> ListTC { get; set; }
        public List<CongThangTheoNgayModel> ListSau22H { get; set; }
    }
    public class CongThangTheoNgayModel
    {
        public string MaLoaiCong { get; set; }
        public int Cot { get; set; }
        public string Value { get; set; }
        public DateTime ngay { get; set; }
    }
    public class CongThangNgayTheoDong
    {
        public string TenPhong_PhanXuong { get; set; }
        public string TenToChuyen { get; set; }
        public string TenChucVu { get; set; }
        public string MSNV { get; set; }
        public string MaChamCong { get; set; }
        public string HoVaTen { get; set; }
        public double SoNgayPhepConLai { get; set; }
        public Int32 NgayCongChuan { get; set; }
        public string Cong { get; set; }
        public string TangCa { get; set; }
        public string TangCaSau22H { get; set; }
        public DateTime ngay { get; set; }
        public string HeaderText { get; set; }
        public TimeSpan GioVao { get; set; }
        public TimeSpan GioRa { get; set; }
        public string CongTac { get; set; }
        public bool VaoTreVeSom { get; set; }

    }
    public class CongThangModel{
        public List<CongThang> ListCongThang { get; set; }
        public List<NgayCong> ListNgayCong { get; set; }
    }
    public class Cot
    {
        public string HeaderText { get; set; }
        public DateTime ngay { get; set; }
    }
    public class NgayCong {
        public string HeaderText { get; set; }
        public bool? ChuNhat { get; set; }
        public int STT { get; set; }
        public string Ngay { get; set; }
    }
}