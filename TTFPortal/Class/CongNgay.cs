using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class CongNgay
    {
        public bool check { get; set; }
        public Int64 IDCongThangChiTiet { get; set; }
        public DateTime Date { get; set; }
        public Int32 STT { get; set; }
        public string MaChamCong { get; set; }
        public string Name { get; set; }
        public string InDate { get; set; }
        public string InTime { get; set; }
        public string InTimeHC { get; set; }
        public string OutDate { get; set; }
        public string OutDateHC { get; set; }
        public string OutTime { get; set; }
        public string OutTimeHC { get; set; }
        public string GioVaoChuan { get; set; }
        public string GioRaChuan { get; set; }
        public double GioQuyDoi { get; set; }
        public double PhutQuyDoi { get; set; }
        public double TongGioCong { get; set; }
        public double Cong { get; set; }
        public double NgoaiGioHC { get; set; }
        public double SoGioTangCa { get; set; }
        public double TangCa { get; set; }
        public double TangCaSau22H { get; set; }
        public string MaNV { get; set; }
        public string MaPhong_PhanXuong { get; set; }
        public string TenPhong_PhanXuong { get; set; }
        public string Thu { get; set; }
        public int TuanThuMayCuaThang { get; set; }
        public int Thu7LanMay { get; set; }
        public bool CoYCTangCa { get; set; }
        public bool CoXNCVao { get; set; }
        public bool CoXNCRa { get; set; }
        public string TrangThai { get; set; }
        public string CongHC { get; set; }
        public string TangCaHC { get; set; }
        public string TangCaSau22HHC { get; set; }
        public string CongFinal { get; set; }
        public string TangCaFinal { get; set; }
        public string TangCaSau22HFinal { get; set; }
        public bool ThieuVanTay { get; set; }
        public int? CongTac { get; set; }
        public string TenChucVu { get; set; }
        public bool VaoTreVeSom { get; set; }

        public DateTime? InTimeHC1 { get; set; }
        public DateTime? OutTimeHC1 { get; set; }
        public string Loi { get; set; }
        public string LeOrPhep { get; set; }
        //public DateTime OutDateHC1 { get; set; }
    }

}