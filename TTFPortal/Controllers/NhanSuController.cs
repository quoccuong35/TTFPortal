using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Class;
using TTFPortal.Models;
using ClosedXML.Excel;
using System.IO;
using System.Data;
using System.Transactions;
using System.Globalization;
using System.Drawing;
using DevExpress.XtraReports.UI;

namespace TTFPortal.Controllers
{
    [RoleAuthorize]
    [RoleAuthorize(Roles = "0=0,11=1")]
    public class NhanSuController : Controller
    {
        // GET: NhanSu
        [RoleAuthorize(Roles = "0=0,11=1")]
        public ActionResult Index()
        {
            var user = Users.GetNguoiDung(User.Identity.Name);
            ViewBag.MaPhongBan = user.MaPhongPhanXuong;
            return View();

        }
        [RoleAuthorize(Roles = "0=0,11=1")]
        public async Task<JsonResult> GetDanhSachNhanSu(string MaPhongBan, string MaChucVu, string HoVaTen, string MaTinhTrang, string Del)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                if (Del == null || Del == "")
                    Del = "0";
                //if (MaTinhTrang == null || MaTinhTrang == "")
                //    MaTinhTrang = "1";
                var model = db.TTF_GET_ALL_NhanSu(MaChucVu, MaPhongBan, HoVaTen, MaTinhTrang, Del).ToList();
                var ng = Users.GetNguoiDung(User.Identity.Name);
                string DonViCuaUser = ng.MaPhongPhanXuong.Trim() != null ? ng.MaPhongPhanXuong.Trim() : "";
                int iNhanSu = ng.NhanSu;
                if (DonViCuaUser != "HCNS" && !User.IsInRole("0=0"))
                {

                    List<string> listPhamVi = db.TTF_PhamVi.Where(it => it.NhanSu == iNhanSu).Select(o => o.MaPhong_PhanXuong.Trim()).ToList();
                    listPhamVi.Add(DonViCuaUser);
                    if (DonViCuaUser.Length > 0)
                    {
                        model = model.Where(it => listPhamVi.Contains(it.MaPhong_PhanXuong.Trim())).ToList();
                    }
                    
                }
                if (!User.IsInRole("0=0") && !User.IsInRole("57=1") && DonViCuaUser != "HCNS")
                {
                    model = model.Where(it => it.NhanSu == iNhanSu).ToList();
                }
                var json = Json(model, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
        }
        public ActionResult ExportFileImportNhanSu()
        {
            string filename = Server.MapPath("~/Content/Upload/FileMau/FileMauImportNhanSu.xlsx");
            //using (XLWorkbook wb = new XLWorkbook(filename))
            //{

            //    wb.Worksheet("");
            //}
            return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FileMauImportNhanSu.xlsx");
        }
        public void XuatExelDanhDSNhanSu(string machucvu, string maphongban, string nhansu, string MaTinhTrang, string Del)
        {
            TTF_FACEIDEntities db = new TTF_FACEIDEntities();
            using (XLWorkbook wb = new XLWorkbook(Server.MapPath("~/Content/upload/FileMau/FileMauImportNhanSu.xlsx")))
            {
                IXLWorksheet FromBaoCao = wb.Worksheet("FormMau");
                maphongban = maphongban.Replace("null", "");
                nhansu = nhansu.Replace("null", "");
                machucvu = machucvu.Replace("null", "");
                MaTinhTrang = MaTinhTrang.Replace("null", "");
                if (MaTinhTrang == null || MaTinhTrang == "")
                    MaTinhTrang = "1";
                Del = Del.Replace("null", "");
                if (Del == null || Del == "")
                {
                    Del = "0";
                }

                var model = db.TTF_GET_ALL_NhanSu(machucvu, maphongban, nhansu, MaTinhTrang, Del).ToList();
                var NhanSu = db.TTF_NhanSu.Where(it => it.MaTinhTrang == "1" && it.Del != true).ToList();
                int y = 2, itt = 1;
                var cellWithFormulaR1C1 = FromBaoCao.Cell("V2");
                foreach (var item in model)
                {

                    FromBaoCao.Cell("A" + y).Value = item.MaNV;
                    FromBaoCao.Cell("B" + y).Value = item.MaChamCong;
                    FromBaoCao.Cell("C" + y).Value = item.MaLuuHoSo;
                    FromBaoCao.Cell("D" + y).Value = item.HoVaTen;
                    FromBaoCao.Cell("E" + y).Value = item.MaTinhTrang;
                    FromBaoCao.Cell("F" + y).Value = item.MaCoSo;
                    FromBaoCao.Cell("G" + y).Value = item.MaChucVu;
                    FromBaoCao.Cell("H" + y).Value = item.MaKhoi;
                    FromBaoCao.Cell("I" + y).Value = item.MaPhong_PhanXuong;
                    FromBaoCao.Cell("J" + y).Value = item.MaBoPhan;
                    if (item.IDCapQuanLyTrucTiep != null)
                    {
                        var TempNhanSu = NhanSu.FirstOrDefault(it => it.NhanSu == item.IDCapQuanLyTrucTiep);
                        if (TempNhanSu != null)
                            FromBaoCao.Cell("K" + y).Value = TempNhanSu.MaNV;
                    }

                    FromBaoCao.Cell("L" + y).Value = item.MaLoaiLaoDong;
                    FromBaoCao.Cell("M" + y).Value = item.MaToChuyen;
                    FromBaoCao.Cell("N" + y).Value = item.MaCaLamViec;
                    FromBaoCao.Cell("O" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("O" + y).Value = item.NgayVaoCongTy != null ? ((DateTime)(item.NgayVaoCongTy)).ToString("dd/MM/yyyy") : null; ;

                    FromBaoCao.Cell("P" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("P" + y).Value = item.NgayNghiViec != null ? ((DateTime)(item.NgayNghiViec)).ToString("dd/MM/yyyy") : null;


                    FromBaoCao.Cell("Q" + y).Value = item.GioiTinh == true ? 1 : 0;

                    FromBaoCao.Cell("R" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("R" + y).Value = item.NgaySinh;

                    FromBaoCao.Cell("S" + y).Value = item.NoiSinh;
                    FromBaoCao.Cell("T" + y).Value = item.NguyenQuan;
                    FromBaoCao.Cell("U" + y).Value = item.MaQuocTich;
                    FromBaoCao.Cell("V" + y).Value = item.MaDanToc;
                    FromBaoCao.Cell("W" + y).Value = item.MaTonGiao;
                    //FromBaoCao.Cell("X" + y).Value = (bool)item.IsKetHon == true ? 1 : 0;
                    FromBaoCao.Cell("Y" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("Y" + y).Value = item.SoCMND;
                    FromBaoCao.Cell("Z" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("Z" + y).Value = item.NgayCap != null ? ((DateTime)item.NgayCap).ToString("dd/MM/yyyy") : null;
                    FromBaoCao.Cell("AA" + y).Value = item.NoiCap;
                    FromBaoCao.Cell("AB" + y).Value = item.DCThuongTru;
                    FromBaoCao.Cell("AC" + y).Value = item.DCCuTru;
                    FromBaoCao.Cell("AD" + y).Value = item.DCHoKhau;
                    FromBaoCao.Cell("AE" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("AE" + y).Value = item.MaTrinhDoHocVan;
                    FromBaoCao.Cell("AF" + y).Value = item.ChuyenNganh;
                    FromBaoCao.Cell("AG" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("AG" + y).Value = item.TGThuViecTuNgay != null ? ((DateTime)(item.TGThuViecTuNgay)).ToString("dd/MM/yyyy") : null;
                    FromBaoCao.Cell("AH" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("AH" + y).Value = item.TGThuViecDenNgay != null ? ((DateTime)(item.TGThuViecDenNgay)).ToString("dd/MM/yyyy") : null;
                    FromBaoCao.Cell("AI" + y).Value = item.SoBHXH;
                    FromBaoCao.Cell("AJ" + y).Value = item.MaBV;
                    FromBaoCao.Cell("AK" + y).Value = item.MaSoThue;
                    FromBaoCao.Cell("AL" + y).Value = item.SoTaiKhoanNganHang;
                    FromBaoCao.Cell("AM" + y).Value = item.MaNganHang;
                    FromBaoCao.Cell("AN" + y).Value = item.MaChiNhanhNganHang;
                    FromBaoCao.Cell("AO" + y).Value = item.MailCongTy;
                    FromBaoCao.Cell("AP" + y).Value = item.Email;
                    FromBaoCao.Cell("AQ" + y).Value = item.So3CX;
                    FromBaoCao.Cell("AR" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("AR" + y).Value = item.DienThoai;
                    FromBaoCao.Cell("AS" + y).Value = item.Skype;
                    FromBaoCao.Cell("AT" + y).Value = item.Dropbox;
                    FromBaoCao.Cell("AU" + y).Style.NumberFormat.Format = "@";
                    FromBaoCao.Cell("AU" + y).Value = item.SoDTNguoiThan;
                    FromBaoCao.Cell("AV" + y).Value = item.Images;
                    FromBaoCao.Cell("AW" + y).Value = item.NgayCongChuan;
                    //FromBaoCao.Cell("AX" + y).Value = item.SoNgayPhepConLai;
                    FromBaoCao.Cell("AX" + y).Value = item.Del == null || item.Del == false ? 0 : 1;
                    FromBaoCao.Cell("AY" + y).Value = item.GhiChu;
                    FromBaoCao.Cell("AZ" + y).Value = item.LoaiLaoDongTangCa;
                    FromBaoCao.Cell("BA" + y).Value = item.MaNVCu;
                    // chạy công thức thiết lập trong excel
                    //if (itt > 1)
                    //    FromBaoCao.Cell("V" + y).FormulaR1C1 = cellWithFormulaR1C1.FormulaR1C1;
                    y++; itt++;
                }
                wb.Worksheet("FormMau").Select();
                y--;
                var s = FromBaoCao.Range("A2:BB" + y);
                s.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=NhanSu_" + DateTime.Now.ToString("yyyy_MM_dd") + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            //return View();
        }
        [RoleAuthorize(Roles = "0=0,11=2")]
        public async Task<JsonResult> ImportFileExcel(HttpPostedFileBase importFile)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            int dong = 0;
            string sLoi = "", str = "";
            try
            {
                if (importFile != null && importFile.ContentLength > 0 && (Path.GetExtension(importFile.FileName).Equals(".xlsx")))
                {
                    string fileName = importFile.FileName;
                    fileName = User.Identity.Name + "_" + fileName;
                    string UploadDirectory = Server.MapPath("~/Content/Upload/Temps/");
                    bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                    if (!folderExists)
                        System.IO.Directory.CreateDirectory(UploadDirectory);
                    string resultFilePath = UploadDirectory + fileName;
                    importFile.SaveAs(resultFilePath);

                    DataTable dt;
                    dt = getDataTableFromExcelNhanSu(resultFilePath, out sLoi);

                    if (sLoi.Length > 0)
                    {
                        rs.text = sLoi;
                    }
                    else if (dt != null && dt.Rows.Count > 0)
                    {

                        int kq = 0;
                        var ng = Users.GetNguoiDung(User.Identity.Name);
                        try
                        {
                            using (var tran = new TransactionScope())
                            {
                                TTF_FACEIDEntities db = new TTF_FACEIDEntities();
                                foreach (DataRow item in dt.Rows)
                                {
                                    str = item["SQL"].ToString().Replace("$NguoiDung$", ng.NguoiDung.ToString());
                                    db.Database.ExecuteSqlCommand(str.ToString());

                                    str = item["LogChange"].ToString().Replace("$NguoiDung$", ng.NguoiDung.ToString());
                                    if (str.Trim() != "")
                                    {
                                        //str = "INSERT INTO dbo.TTF_LogChangeEmployees( NhanSu , NgayThayDoi , NoiDungThayDoi)VALUES  ('" + iNhanSu + "',getdate(),'"+ item["LogChange"].ToString().Trim() + "')";
                                        db.Database.ExecuteSqlCommand(str.ToString());
                                    }
                                    kq++;
                                }
                                tran.Complete();
                                rs.text = "Cập nhật thông tin thành công " + kq.ToString();
                            }

                        }
                        catch (Exception ex)
                        {
                            clsFunction.NhatkyLoi(DateTime.Now, User.Identity.Name, ex.ToString() + ">>>>" + str, "NhanSu", "ImportExcelUpload");
                            rs.text = "Lỗi hệ thống liên hệ P.HTTT để được hỗ trợ";
                        }
                    }
                    else
                    {
                        str = "Không có dữ liệu cập nhật";
                    }

                    FileInfo fDel = new FileInfo(resultFilePath);
                    fDel.Delete();
                }
            }
            catch (Exception ex)
            {
                rs.text = ex.Message + sLoi;
            }
            var json = Json(rs, JsonRequestBehavior.AllowGet);
            return json;
        }
        public static DataTable getDataTableFromExcelNhanSu(string path, out string sLoi)
        {
            var existingFile = new FileInfo(path);
            TTF_FACEIDEntities con = new TTF_FACEIDEntities();
            sLoi = "";
            using (var pck = new OfficeOpenXml.ExcelPackage(existingFile))
            {
                var LoaiTangCa = con.TTF_LoaiTangCa.ToList();
                //var BenhVienBHYT = con.TTF_BenhVienBHYT.ToList();
                var CaLamViec = con.TTF_CaLamViec.ToList();
                var ChucVu = con.TTF_ChucVu.ToList();
                var CoSoLamViec = con.TTF_CoSoLamViec.ToList();
                // var DanToc = con.TTF_DanToc.ToList();
                //var Khoi = con.TTF_Khoi.ToList();
                var LoaiLaoDong = con.TTF_LoaiLaoDong.ToList();
                //var NganHang = con.TTF_NganHang.ToList();
                var PhongBan_PhanXuong = con.TTF_PhongBan_PhanXuong.ToList();
                //var QuocTich = con.TTF_QuocTich.ToList();
                var TinhTrang = con.TTF_TinhTrang.ToList();
                //var TonGiao = con.TTF_TonGiao.ToList();
                //var TrinhDoHocVan = con.TTF_TrinhDoHocVan.ToList();
                var CapQuanLyTrucTiep = con.TTF_NhanSu.Where(it => it.Del != true).ToList();
                TTF_NhanSu objNhanSu;
                string sNguoiDung = "$NguoiDung$", str = "";
                string kiemtra = "", edit = " \n BEGIN UPDATE dbo.TTF_NhanSu SET NguoiDung2 = '" + sNguoiDung + "',Ngay2 = GETDATE(),  ", insert = "\n END ELSE BEGIN  INSERT INTO	dbo.TTF_NhanSu(  NguoiDung1,Ngay1,", insertvalue = "\n VALUES  ( '" + sNguoiDung + "',GETDATE(),", manv = "";
                var ws = pck.Workbook.Worksheets.First();
                DataTable tbl = new DataTable();
                bool hasHeader = true;
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(firstRowCell.Text.Replace(" ", "").Replace("/", "").Replace("-", "").ToLower());
                }
                tbl.Columns.Add("SQL", typeof(string));
                tbl.Columns.Add("LogChange", typeof(string));
                var startRow = hasHeader ? 2 : 1;
                int index = 0;
                string value = "", value1 = "";
                DateTime dTemp = new DateTime();
                string changes = "";
                string svalue1 = "", svalue2 = "";
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    changes = "";
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = tbl.NewRow();
                    objNhanSu = null;
                    for (int cell = 1; cell <= ws.Dimension.End.Column; cell++)
                    {
                        svalue1 = ""; svalue2 = "";
                        if (index > tbl.Columns.Count)
                        {
                            break;
                        }


                        value = ws.Cells[rowNum, cell].Text.Trim();
                        row[cell - 1] = value;

                        if (tbl.Columns[index].ColumnName == "del")
                        {
                            index++;
                            continue;
                        }

                        if (tbl.Columns[index].ColumnName == "manv")
                        {
                            objNhanSu = CapQuanLyTrucTiep.FirstOrDefault(it => it.MaNV == value);
                        }
                        if (tbl.Columns[index].ColumnName == "idcapquanlytructiep")
                        {
                            var isNhanSu = CapQuanLyTrucTiep.Where(it => it.MaNV.Trim() == row["idcapquanlytructiep"].ToString().Trim()).ToList();
                            if (isNhanSu.Count > 0)
                            {
                                row["idcapquanlytructiep"] = isNhanSu[0].NhanSu;
                                value = isNhanSu[0].NhanSu.ToString();
                                if (objNhanSu != null && objNhanSu.IDCapQuanLyTrucTiep.ToString() != value)
                                {
                                    svalue1 = value;
                                    svalue2 = objNhanSu.IDCapQuanLyTrucTiep.ToString();
                                    changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                                }
                            }
                            else
                            {
                                row["idcapquanlytructiep"] = 0;
                                sLoi += "Không tìm thấy cấp quản lý trực tiếp cột IDCapQuanLyTrucTiep dòng " + (startRow - 1).ToString();
                                value = "0";
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "loailaodongtangca")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Loại lao động tăng ca không tồn tại ở cột loailaodongtangca dòng " + (startRow - 1).ToString() + " ;";
                            }
                            if (LoaiTangCa.Where(it => it.LoaiLaoDongTangCa.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Loại lao động tăng ca không tồn tại ở cột loailaodongtangca dòng " + (startRow - 1).ToString() + " ;";
                            }
                            else if (objNhanSu != null && objNhanSu.LoaiLaoDongTangCa.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.LoaiLaoDongTangCa.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "manv")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập mã nhân viên cột MaNV dòng " + (startRow - 1).ToString() + ";";
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "machamcong")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập mã chấm công cột MaChamCong dòng " + (startRow - 1).ToString() + ";";
                            }
                            else
                            {
                                if (objNhanSu != null && objNhanSu.MaChamCong.ToString().Trim().ToLower() != value.Trim().ToLower())
                                {
                                    svalue1 = value;
                                    svalue2 = objNhanSu.MaChamCong.ToString();
                                    changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                                }
                            }

                        }
                        else if (tbl.Columns[index].ColumnName == "ngayvaocongty")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chua có nhập ngày vào công ty cột NgayVaoCongTy dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && ((DateTime)objNhanSu.NgayVaoCongTy).ToString("dd/MM/yyyy").Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.NgayVaoCongTy.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "hovaten")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập Họ Và Tên cột HoVaTen dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.HoVaTen.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.HoVaTen.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "gioitinh")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập giới tính cột GioiTinh dòng " + (startRow - 1).ToString() + ";";
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "ngaysinh")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập ngày sinh cột NgaySinh" + (startRow - 1).ToString() + ";";
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "socmnd")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập số CMND Cột SoCMND dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.SoCMND !=null && objNhanSu.SoCMND.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.SoCMND.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                            else if (value.Length > 12)
                            {
                                sLoi += "Số CMND vượt quá 12 ký tự Cột SoCMND dòng " + (startRow - 1).ToString() + ";";
                            }
                        }
                        //if (tbl.Columns[index].ColumnName == "mabv")
                        //{
                        //    if (BenhVienBHYT.Where(it => it.MaBV.Trim() == value).ToList().Count == 0)
                        //    {
                        //        sLoi += "Bệnh viện không tồn tại " + ";";
                        //        bLoi = true;
                        //    }
                        //}
                        else if (tbl.Columns[index].ColumnName == "macalamviec")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Ca làm việc không tồn tại cột MaCaLamViec dòng" + (startRow - 1).ToString() + ";";
                            }
                            if (CaLamViec.Where(it => it.MaCaLamViec.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Ca làm việc không tồn tại cột MaCaLamViec dòng" + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaCaLamViec.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaCaLamViec.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "machucvu")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chức vụ không tồn tại cột MaChucVu dòng" + (startRow - 1).ToString() + ";";
                            }
                            if (ChucVu.Where(it => it.MaChucVu.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Chức vụ không tồn tại cột MaChucVu dòng" + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaChucVu.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaChucVu.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "macoso")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Cơ sở làm việc không tồn tại cột MaCoSo dòng " + (startRow - 1).ToString() + ";";
                            }
                            if (CoSoLamViec.Where(it => it.MaCoSo.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Cơ sở làm việc không tồn tại cột MaCoSo dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaCoSo.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaCoSo.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        //if (tbl.Columns[index].ColumnName == "madantoc")
                        //{
                        //    if (DanToc.Where(it => it.MaDanToc.Trim() == value).ToList().Count == 0)
                        //    {
                        //        sLoi += "Dân tộc không tồn tại " + ";";
                        //    }
                        //}
                        //if (tbl.Columns[index].ColumnName == "makhoi")
                        //{
                        //    if (Khoi.Where(it => it.MaKhoi.Trim() == value).ToList().Count == 0)
                        //    {
                        //        sLoi += "Khối không tồn tại " + ";";
                        //    }
                        //}
                        else if (tbl.Columns[index].ColumnName == "maloailaodong")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Loại lao động không tồn tại cột MaLoaiLaoDong dòng " + (startRow - 1).ToString() + ";";
                            }
                            if (LoaiLaoDong.Where(it => it.MaLoaiLaoDong.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Loại lao động không tồn tại cột MaLoaiLaoDong dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaLoaiLaoDong.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaLoaiLaoDong.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "maphong_phanxuong")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Phòng/phân xưởng không tồn tại cột MaPhong_PhanXuong dòng" + (startRow - 1).ToString() + ";";
                            }
                            if (PhongBan_PhanXuong.Where(it => it.MaPhong_PhanXuong.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Phòng/phân xưởng không tồn tại cột MaPhong_PhanXuong dòng" + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaPhong_PhanXuong.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaPhong_PhanXuong.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (tbl.Columns[index].ColumnName == "matinhtrang")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Tình trạng không tồn tại cột MaTinhTrang" + (startRow - 1).ToString() + ";";
                            }
                            if (TinhTrang.Where(it => it.MaTinhTrang.Trim() == value).ToList().Count == 0)
                            {
                                sLoi += "Tình trạng không tồn tại cột MaTinhTrang" + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.MaTinhTrang.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.MaTinhTrang.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        else if (objNhanSu != null && tbl.Columns[index].ColumnName == "ngaycongchuan")
                        {
                            if (value == null || value == "")
                            {
                                sLoi += "Chưa nhập ngày công chuẩn cột NgayCongChuan dòng " + (startRow - 1).ToString() + ";";
                            }
                            else if (objNhanSu != null && objNhanSu.NgayCongChuan.ToString().Trim().ToLower() != value.Trim().ToLower())
                            {
                                svalue1 = value;
                                svalue2 = objNhanSu.NgayCongChuan.ToString();
                                changes += string.Format("<li>{0} changed from {1} to {2}</li> \n", tbl.Columns[index].ColumnName, svalue2, svalue1);
                            }
                        }
                        //if (tbl.Columns[index].ColumnName == "matongiao")
                        //{
                        //    if (TonGiao.Where(it => it.MaTonGiao.Trim() == value).ToList().Count == 0)
                        //    {
                        //        sLoi += "Tôn giáo không tồn tại " + ";";
                        //    }
                        //}
                        //if (tbl.Columns[index].ColumnName == "matrinhdohocvan")
                        //{
                        //    if (TrinhDoHocVan.Where(it => it.MaTrinhDoHocVan.Trim() == value).ToList().Count == 0)
                        //    {
                        //        sLoi += "Trình độ học vấn không tồn tại " + ";";
                        //    }
                        //}

                        /// Anh Du Lieu Vao table

                        if (tbl.Columns[index].ColumnName == "manv")
                        {
                            manv = value;
                            kiemtra = "IF EXISTS (SELECT MaNV FROM dbo.TTF_NhanSu WHERE MaNV = '" + manv + "')";
                            insert += "" + tbl.Columns[index].ColumnName + ",";
                            insertvalue += "'" + manv + "',";
                        }
                        else if (value.Trim().Length > 0)
                        {
                            value1 = getValue(tbl.Columns[index].ColumnName, value.Replace("'", "''"));
                            if (tbl.Columns[index].ColumnName == "ngayvaocongty" || tbl.Columns[index].ColumnName == "ngaycap" || tbl.Columns[index].ColumnName == "ngaynghiviec" || tbl.Columns[index].ColumnName == "tgthuviectungay" || tbl.Columns[index].ColumnName == "tgthuviecdenngay")
                            {
                                try
                                {
                                    dTemp = (DateTime.ParseExact(value, "dd/MM/yyyy", new CultureInfo("en-US")));
                                }
                                catch (Exception)
                                {
                                    sLoi += "Lỗi định dạng ngày tháng năm cột " + tbl.Columns[index].ColumnName.ToString() + " dòng " + (startRow - 1).ToString() + ";";
                                }
                                edit += "" + tbl.Columns[index].ColumnName + " = '" + dTemp.ToString("MM/dd/yyyy") + "',";
                                insert += "" + tbl.Columns[index].ColumnName + ",";
                                insertvalue += "'" + dTemp.ToString("MM/dd/yyyy") + "',";
                            }
                            else
                            {
                                if (tbl.Columns[index].ColumnName != "machamcong")
                                {
                                    edit += "" + tbl.Columns[index].ColumnName + " = N" + value1 + ",";
                                }
                                insert += "" + tbl.Columns[index].ColumnName + ",";
                                insertvalue += "N" + value1 + ",";
                            }
                        }
                        index++;
                    }
                    edit = edit.Substring(0, edit.Length - 1) + " Where MaNV = '" + manv + "'";
                    insert = insert.Substring(0, insert.Length - 1) + ")";
                    insertvalue = insertvalue.Substring(0, insertvalue.Length - 1) + ")";
                    str += kiemtra + edit + insert + insertvalue + " END \n";

                    row["SQL"] = str;
                    if (changes != "")
                    {
                        row["LogChange"] = "INSERT INTO dbo.TTF_LogChangeEmployees( NhanSu , NgayThayDoi , NoiDungThayDoi,NguoiDungThayDoi)VALUES  ('" + objNhanSu.NhanSu + "',getdate(),N'" + changes + "','$NguoiDung$')";
                    }
                    manv = kiemtra = value = "";
                    edit = " \n BEGIN UPDATE dbo.TTF_NhanSu SET NguoiDung2 = '" + sNguoiDung + "',Ngay2 = GETDATE(),  ";
                    insert = "\n END ELSE BEGIN  INSERT INTO	dbo.TTF_NhanSu(  NguoiDung1,Ngay1,";
                    insertvalue = "\n VALUES  ( '" + sNguoiDung + "',GETDATE(),";
                    str = "";
                    row["loi"] = sLoi;
                    index = 0;
                    tbl.Rows.Add(row);
                }
                return tbl;
            }
        }
        public static string getValue(string DATA_TYPE, string value)
        {
            string kq = "";
            try
            {
                if (DATA_TYPE == "bit")
                {
                    kq = "'" + value + "'";
                }
                else if (DATA_TYPE == "date" || DATA_TYPE == "datetime")
                {
                    string y = value.Substring(6, 4);
                    string m = value.Substring(3, 2);
                    string d = value.Substring(0, 2);
                    kq = "'" + m + "/" + d + "/" + y + "'";
                }
                else if (DATA_TYPE == "int")
                {
                    kq = "'" + value + "'";
                }
                else if (DATA_TYPE == "nchar" || DATA_TYPE == "nvarchar")
                {
                    kq = "N'" + value.Trim() + "'";
                }
                else
                {
                    kq = "'" + value + "'";
                }
            }
            catch (Exception)
            {
                return kq;
            }
            return kq;
        }

        [RoleAuthorize(Roles = "0=0,11=2")]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,11=2")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddNhanSu(TTF_NhanSu item, string skey)
        {
            JsonStatus js = new JsonStatus();
            js.code = 0;
            js.text = "Thất bại";
            using (SaveDB save = new SaveDB())
            {
                using (var tran = new TransactionScope())
                {
                    try
                    {
                        var check = save.TTF_NhanSu.Where(it => it.MaNV == item.MaNV).ToList();
                        if (check.Count > 0)
                        {
                            js.code = 0;
                            js.text = "Mã " + item.MaNV + " đã tồn tại không thể lưu ";
                            return Json(js, JsonRequestBehavior.AllowGet);
                        }
                        try
                        {
                            var ng = Users.GetNguoiDung(User.Identity.Name);
                            item.NguoiDung1 = ng.NguoiDung.ToString();
                            item.Ngay1 = DateTime.Now;
                            save.GhiChu = "Thêm nhân sự";
                            save.TTF_NhanSu.Add(item);

                            int i = save.SaveChanges();
                            if (i > 0)
                            {
                                js.text = item.NhanSu.ToString();
                                var nhansu = save.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == item.NhanSu);
                                string p = Server.MapPath("~/Content/upload/temps/imgs/") + skey;
                                if (Directory.Exists(p))
                                {
                                    string file = System.IO.Directory.GetFiles(p)[0];
                                    string loaifile = file.Substring(file.LastIndexOf('.') + 1);
                                    if (file != null && file != "")
                                    {
                                        System.IO.FileInfo fi = new System.IO.FileInfo(file);
                                        fi.MoveTo(Server.MapPath("/Editor/UploadFolder/" + item.NhanSu + "." + loaifile));
                                        nhansu.Images = "/Editor/UploadFolder/" + item.NhanSu + "." + loaifile;
                                        Directory.Delete(p);
                                        save.SaveChanges();
                                    }
                                }
                            }
                            js.code = 1;
                            js.text = "Thành công";
                            js.description = item.NhanSu.ToString();
                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            js.text = ex.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        js.code = 0;
                        js.text = ex.ToString();
                    }
                }
            }

            return Json(js, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,11=2")]
        public JsonResult UploadHinh(string id, HttpPostedFileBase importFile)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            try
            {
                string p = Server.MapPath("~/Content/upload/temps/imgs/" + id);
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
                else
                {
                    Directory.Delete(p);
                    Directory.CreateDirectory(p);
                }
                if (importFile != null && importFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(importFile.FileName);
                    var fileName = clsFunction.GenerateSlug(importFile.FileName.Replace(extension, ""));
                    var path = Path.Combine(p, fileName + extension);
                    Bitmap bitmap = new Bitmap(importFile.InputStream);
                    bitmap.Save(path);
                    rs.code = 1;
                    rs.text = "Thành công";
                }

            }
            catch (Exception ex)
            {
                rs.code = 0;
                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult XoaFile(int id)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            try
            {
                string p = Server.MapPath("~/Content/upload/temps/imgs/" + id);
                if (Directory.Exists(p))
                {
                    rs.code = 1;
                    rs.text = "Thành công";
                    Directory.Delete(p, true);
                }
            }
            catch (Exception ex)
            {

                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int? id)
        {
            using (var db = new SaveDB())
            {
                if (id == null)
                {
                    return Content("Chưa chọn nhân sự");
                }
                var model = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == id);

                //if (model.Images != null && model.Images.Length > 0)
                //{
                //    model.Images = Server.MapPath(model.Images);
                //}
                return View(model);
            }

        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,11=3")]
        public JsonResult CapNhatHinh(int id, HttpPostedFileBase importFile)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            try
            {
                bool folderExists = System.IO.Directory.Exists(Server.MapPath("/Editor/UploadFolder/"));
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(Server.MapPath("/Editor/UploadFolder/"));
                if (importFile != null && importFile.ContentLength > 0)
                {
                    try
                    {
                        var fileName = id + Path.GetExtension(importFile.FileName);
                        importFile.SaveAs(Path.Combine(Server.MapPath("/Editor/UploadFolder/"), fileName));
                        SaveDB db = new SaveDB();
                        db.GhiChu = "Cập nhật hình ảnh";
                        var model = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == id);
                        if (model != null)
                            model.Images = "/Editor/UploadFolder/" + fileName;
                        int sc = db.SaveChanges();
                        if (sc > 0)
                        {
                            rs.code = 1;
                            rs.text = "Thành công";
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {

                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles="0=0,11=3")]
        public async Task<JsonResult> EditNhanSu(TTF_NhanSu item,int id )
        {
            JsonStatus js = new JsonStatus();
            js.code = 0;
            js.text = "Thất bại";
            using (var db = new SaveDB())
            {
                db.GhiChu = "Sửa thông tin nhân sự";
                var model = db.TTF_NhanSu.FirstOrDefault(it => it.NhanSu == id);
                var ng = Users.GetNguoiDung(User.Identity.Name);
                if (model != null)
                {
                    model.MaNV = item.MaNV.Trim();
                    model.MaChamCong = item.MaChamCong.Trim();
                    model.HoVaTen = item.HoVaTen;
                    model.GioiTinh = item.GioiTinh == null ? false : item.GioiTinh;
                    model.NgaySinh = item.NgaySinh;
                    model.SoCMND = item.SoCMND;
                    model.NoiSinh = item.NoiSinh;
                    model.DienThoai = item.DienThoai;
                    model.Email = item.Email;
                    model.MaPhong_PhanXuong = item.MaPhong_PhanXuong;
                    model.MaChucVu = item.MaChucVu;
                    model.NgayBoNhiem = item.NgayBoNhiem;
                    model.MaCoSo = item.MaCoSo;
                    model.NgayVaoCongTy = item.NgayVaoCongTy;
                    model.ChuyenNganh = item.ChuyenNganh;
                    model.GhiChu = item.GhiChu;
                    model.NgayNghiViec = item.NgayNghiViec;
                    model.MaTinhTrang = item.MaTinhTrang;
                    //boxung
                    model.NgayCap = item.NgayCap;
                    model.NoiCap = item.NoiCap;
                    model.MaDanToc = item.MaDanToc;
                    model.MaTonGiao = item.MaTonGiao;
                    model.MaTrinhDoHocVan = item.MaTrinhDoHocVan;
                    model.MaQuocTich = item.MaQuocTich;
                    model.SoTaiKhoanNganHang = item.SoTaiKhoanNganHang;
                    model.MaNganHang = item.MaNganHang;
                    model.MaChiNhanhNganHang = item.MaChiNhanhNganHang;
                    model.MaSoThue = item.MaSoThue;
                    model.MaLuuHoSo = item.MaLuuHoSo;
                    model.NgayCongChuan = item.NgayCongChuan;
                    model.MailCongTy = item.MailCongTy;
                    model.So3CX = item.So3CX;
                    model.Skype = item.Skype;
                    model.Dropbox = item.Dropbox;
                    model.SoDTNguoiThan = item.SoDTNguoiThan;
                    model.SoBHXH = item.SoBHXH;
                    model.TGThuViecTuNgay = item.TGThuViecTuNgay;
                    model.TGThuViecDenNgay = item.TGThuViecDenNgay;
                    model.NguyenQuan = item.NguyenQuan;
                    model.DCThuongTru = item.DCThuongTru;
                    model.DCCuTru = item.DCCuTru;
                    model.DCHoKhau = item.DCHoKhau;
                    model.MaBV = item.MaBV;
                    model.IsKetHon = item.IsKetHon;
                    model.IDCapQuanLyTrucTiep = item.IDCapQuanLyTrucTiep;
                    model.Del = item.Del;
                    model.MaChamCongCu = item.MaChamCongCu;
                    if (item.SoNgayPhepConLai != null)
                    {
                        model.SoNgayPhepConLai = item.SoNgayPhepConLai;
                    }
                    if (item.Del == true)
                    {
                        model.Del = true;
                        model.NguoiDel = (int)ng.NguoiDung;
                        model.NgayDel = DateTime.Now;
                    }
                    model.NguoiDung2 = ng.NguoiDung.ToString();
                    model.Ngay2 = DateTime.Now;
                    model.MaKhoi = item.MaKhoi;
                    model.MaBoPhan = item.MaBoPhan;
                    model.MaToChuyen = item.MaToChuyen;
                    model.MaLoaiLaoDong = item.MaLoaiLaoDong;
                    model.MaCaLamViec = item.MaCaLamViec;
                    model.LoaiLaoDongTangCa = item.LoaiLaoDongTangCa;
                    model.SoNgayThuViec = item.SoNgayThuViec;
                    model.KhongTinhTC = item.KhongTinhTC;
                    model.ChamCongKM = item.ChamCongKM;
                    model.DocHaiTuNgay = item.DocHaiTuNgay;
                    model.DocHaiDenNgay = item.DocHaiDenNgay;
                    if (db.SaveChanges() > 0)
                    {
                        js.code = 1;
                        js.text = "Thành công";
                        js.description = model.NhanSu.ToString();
                    }
                }
            }
            return Json(js, JsonRequestBehavior.AllowGet);
        }
        public JsonResult XoaHinh(int id, HttpPostedFileBase importFile)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            rs.text = "Thất bại";
            try
            {
                string p = Server.MapPath("~//Editor/UploadFolder/" + id);
                if (Directory.Exists(p))
                {
                    rs.code = 1;
                    rs.text = "Thành công";
                    Directory.Delete(p, true);
                }
            }
            catch (Exception ex)
            {

                rs.text = ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        [RoleAuthorize(Roles = "0=0,11=2")]
        public ActionResult CapNhatHinhNen() {
            return View();
        }
        [HttpPost]
        [RoleAuthorize(Roles = "0=0,11=3")]
        public ActionResult CapNhatHinhNen(IEnumerable<HttpPostedFileBase> myFiles)
        {
            if (myFiles == null || myFiles.Count() == 0)
            {
                return Content("Chưa chọn file hình");
            }
            using (var db = new SaveDB()) {
                string sURL = @"/Editor/UploadFolder/";
                string sLoi = "", sCapNhat = "", MaNV = "", FileName = "";
                var NhanSu = db.TTF_NhanSu.ToList();
                string UploadDirectory = Server.MapPath("~/Editor/UploadFolder/");
                bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(UploadDirectory);

                foreach (var file in myFiles)
                {

                    if (file != null && file.ContentLength > 0)
                    {
                        FileName = file.FileName;
                        MaNV = FileName.Substring(0, FileName.LastIndexOf('.'));

                        var checkNhanSu = NhanSu.FirstOrDefault(it => it.MaNV == MaNV);
                        if (checkNhanSu == null)
                        {
                            sLoi += "Không tìm thấy mã nhân viên <b>" + MaNV + "</b><br>";
                            continue;
                        }
                        else
                        {
                            checkNhanSu.Images = sURL + FileName;
                           // sCapNhat += "UPDATE dbo.TTF_NhanSu SET Images = '" + sURL + FileName + "' Where MaNV = '" + checkNhanSu.MaNV.Trim() + "'";
                            file.SaveAs(UploadDirectory + FileName);
                        }
                    }
                }


                if (sLoi.Length > 0)
                {
                    return Content(MvcHtmlString.Create(sLoi).ToHtmlString());
                }
                else
                {
                    db.SaveChanges();
                    return Content("Thành công");
                }
            }
            return View();
        }
        public ActionResult DSInTheTen()
        {
            return View();
        }
        public async System.Threading.Tasks.Task<JsonResult> getDSInTheTen(String MaPhongBan, string HoVaTen, string MaChucVu, string TuNgay, string DenNgay)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities()) {
                var model = db.TTF_InTheTen(MaPhongBan, MaChucVu, HoVaTen, TuNgay, DenNgay).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public async System.Threading.Tasks.Task<JsonResult> AddSession(List<TTF_InTheTen_Result> list)
        {
            string rs = "0";
            try
            {
                Session["InTheTen"] = null;
                Session["InTheTen"] = list;
                rs = "1";
            }
            catch (Exception)
            {
                rs = "0";
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        public async System.Threading.Tasks.Task<ActionResult> InTheTenMatTruoc(int LoaiThe)
        {
            var data = (List<TTF_InTheTen_Result>)(Session["InTheTen"]);
            var reportModel = new ReportsModel();
            var model = data.Where(it => it.LoaiThe == LoaiThe).ToList();
            reportModel.ReportName = "InTheTenMatTruoc";
            if (LoaiThe == 1)
            {
                reportModel.Report = XtraReport.FromFile(Server.MapPath("~/Content/Upload/reports/TheTenNVMatTruoc.repx"), true);
            }
            else
            {
                reportModel.Report = XtraReport.FromFile(Server.MapPath("~/Content/Upload/reports/TheTenCNMatTruoc.repx"), true);
            }

            //reportModel.Report = XtraReport.FromFile(Server.MapPath("~/Content/upload/reports/TheTenNVMatTruoc.repx"), true);
            reportModel.Report.DataSource = model;
            return View(reportModel);
        }
        public async System.Threading.Tasks.Task<ActionResult> InTheTenMatSau(int LoaiThe)
        {
            var data = (List<TTF_InTheTen_Result>)(Session["InTheTen"]);
            var model = data.Where(it => it.LoaiThe == LoaiThe).ToList();
            var reportModel = new ReportsModel();
            reportModel.ReportName = "InTheTenMatSau";
            if (LoaiThe == 1)
            {
                reportModel.Report = XtraReport.FromFile(Server.MapPath("~/Content/Upload/reports/TheTenNVMatSau.repx"), true);
            }
            else
            {
                reportModel.Report = XtraReport.FromFile(Server.MapPath("~/Content/Upload/reports/TheTenCNMatSau.repx"), true);
            }

            reportModel.Report.DataSource = model;
            return View(reportModel);
        }
    }
}