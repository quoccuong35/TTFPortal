using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TTFPortal.Models;
using TTFPortal.Class;
using System.IO;
using System.Globalization;
using System.Web.Libs;

namespace TTFPortal.Controllers
{
    [RoleAuthorize(Roles = "0=0,52=1")]
    public class CongCongTrinhController : Controller
    {
        // GET: CongCongTrinh
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetCongCongTrinh(string tuNgay, string denNgay, string maDuAn, string maNV, string hoTen)
        {
            using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
            {
                var model = db.proc_ChamCongKhuonMat(tuNgay, denNgay, maNV, hoTen, maDuAn).ToList();
                var rs = Json(model, JsonRequestBehavior.AllowGet);
                rs.MaxJsonLength = int.MaxValue;
                return rs;
            } 
        }
        public ActionResult ExportFileMau()
        {
            string filename = Server.MapPath("~/Content/Upload/FileMau/MauImPortCongNgoaiCongTrinh.xlsx");
            return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauImPortCongNgoaiCongTrinh.xlsx");
        }
        public DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {

                    csvReader.SetDelimiters(new string[] { "," });

                    csvReader.HasFieldsEnclosedInQuotes = true;

                    string[] colFields = csvReader.ReadFields();

                    foreach (string column in colFields)
                    {

                        DataColumn datecolumn = new DataColumn(column.Replace(" ", "").Replace("/", "").Replace("-", "").ToLower());

                        datecolumn.AllowDBNull = true;

                        csvData.Columns.Add(datecolumn);

                    }

                    while (!csvReader.EndOfData)
                    {

                        string[] fieldData = csvReader.ReadFields();

                        //Making empty value as null

                        for (int i = 0; i < fieldData.Length; i++)
                        {

                            if (fieldData[i] == "")
                            {

                                fieldData[i] = null;

                            }

                        }
                        csvData.Rows.Add(fieldData);

                    }

                }
            }

            catch
            {
            }

            return csvData;

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ImportExcelUploadNcheck(HttpPostedFileBase FileInbox)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (FileInbox != null && FileInbox.ContentLength > 0 && (Path.GetExtension(FileInbox.FileName).Equals(".xlsx") || Path.GetExtension(FileInbox.FileName).Equals(".csv")))
            {
                string fileName = FileInbox.FileName;
                string UploadDirectory = Server.MapPath("~/Content/Temps/");
                bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(UploadDirectory);
                string resultFilePath = UploadDirectory + fileName;
                var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                int NguoiTao = (int)nguoidung.NguoiDung;
                if (NguoiTao < 0)
                {
                    rs.text = "Hết thời gian thao tác xin đăng nhập lại";
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    FileInbox.SaveAs(resultFilePath);
                    DataTable dt = GetDataTabletFromCSVFile(resultFilePath);
                    DateTime dttime = new DateTime();
                    string d, m, y, t, time = "";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
                        {
                            int i = 0; string str = ""; string error = ""; int kq = 0;
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                try
                                {
                                    if (dt.Rows[i]["employeecode"].ToString().Trim() != null && dt.Rows[i]["time"].ToString().Trim().Length > 0)
                                    {
                                        //d = dt.Rows[i]["time"].ToString().Substring(0, 2);
                                        //m = dt.Rows[i]["time"].ToString().Substring(3, 2);
                                        //y = dt.Rows[i]["time"].ToString().Substring(6, 4);
                                        //t = dt.Rows[i]["time"].ToString().Substring(10);
                                        time = dt.Rows[i]["time"].ToString();
                                        time = time.Replace(".", "/");
                                        //dttime = DateTime.Parse(dt.Rows[i]["time"].ToString().Trim());
                                        string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                                        if (!DateTime.TryParseExact(time, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dttime))
                                        {
                                            rs.text = "Lỗi thời gian dòng " + (i + 1).ToString() + time;
                                            return Json(rs,JsonRequestBehavior.AllowGet);
                                        }
                                        // dttime = DateTime.Parse(time);
                                        str += "IF EXISTS (SELECT *  FROM dbo.TTF_EvenLog WHERE EmployeeCode = '" + dt.Rows[i]["employeecode"].ToString().Trim() + "' AND Time = '" + dttime.ToString("MM/dd/yyyy HH:mm:ss") + "') "
                                       + "BEGIN  UPDATE dbo.TTF_EvenLog SET ShiftName = '" + dt.Rows[i]["shiftname"].ToString() + "',DirectionAndStatus = N'" + dt.Rows[i]["directionandstatus"].ToString() + "',Time = N'" + dttime.ToString("MM/dd/yyyy HH:mm:ss") + "',TimeZone = N'" + dt.Rows[i]["timezone"].ToString() + "',Location = N'" + dt.Rows[i]["latitude"].ToString() + "," + dt.Rows[i]["longitude"].ToString() + "', "
                                       + " Address = N'" + dt.Rows[i]["deviceAddress"].ToString().Replace("'", "") + "',Description = N'" + dt.Rows[i]["description"].ToString().Replace("'", "") + "',Type = N'" + dt.Rows[i]["contracttype"] + "',Date = '" + dttime.ToString("MM/dd/yyyy") + "',Construction = N'" + dt.Rows[i]["devicegroupname"] + "',Remark = '" + dt.Rows[i]["remark"].ToString() + "',EmployeeID = '" + dt.Rows[i]["employeeid"].ToString() + "',Position = '" + dt.Rows[i]["position"].ToString() + "'  WHERE EmployeeCode = '" + dt.Rows[i]["employeeid"] + "' AND Time = '" + dttime.ToString("MM/dd/yyyy HH:mm:ss") + "' "
                                       + " END ELSE  begin INSERT INTO dbo.TTF_EvenLog ( EmployeeCode , ShiftName , DirectionAndStatus ,Time , TimeZone ,Location ,Address ,Description ,Type ,Date ,Construction,Remark,EmployeeID,Position,NguoiDungTao,NgayTao) "
                                       + " VALUES  ('" + dt.Rows[i]["employeecode"].ToString().Trim() + "','" + dt.Rows[i]["shiftname"].ToString() + "',N'" + dt.Rows[i]["directionandstatus"].ToString() + "','" + dttime.ToString("MM/dd/yyyy HH:mm:ss") + "',N'" + dt.Rows[i]["timezone"].ToString() + "', N'" + dt.Rows[i]["latitude"].ToString() + "," + dt.Rows[i]["longitude"].ToString() + "',"
                                       + "N'" + dt.Rows[i]["deviceAddress"].ToString().Replace("'", "") + "',N'" + dt.Rows[i]["description"].ToString().Replace("'", "") + "', N'" + dt.Rows[i]["contracttype"] + "','" + dttime.ToString("MM/dd/yyyy") + "',N'" + dt.Rows[i]["devicegroupname"] + "','" + dt.Rows[i]["remark"] + "','" + dt.Rows[i]["employeeid"] + "','" + dt.Rows[i]["position"] + "','" + NguoiTao + "',GETDATE()) End \n";

                                        //str += "IF EXISTS (SELECT *  FROM dbo.TTF_EvenLog WHERE EmployeeCode = '" + dt.Rows[i]["ID"] + "' AND Time = '" + dttime.ToString("MM/dd/yyyy hh:mm:ss tt") + "') "
                                        //    + "BEGIN  UPDATE dbo.TTF_EvenLog SET ShiftName = '" + dt.Rows[i]["ShiftName"] + "',DirectionAndStatus = N'" + dt.Rows[i]["DirectionAndStatus"] + "',Time = N'" + dttime.ToString("MM/dd/yyyy hh:mm:ss tt") + "',TimeZone = N'" + dt.Rows[i]["TimeZone"] + "',Location = N'" + dt.Rows[i]["Location"] + "',"
                                        //    + " Address = N'" + dt.Rows[i]["Address"] + "',Description = N'" + dt.Rows[i]["Description"] + "',Type = N'" + dt.Rows[i]["Type"] + "',Date = '" + dttime.ToString("MM/dd/yyyy") + "',Construction = '" + dt.Rows[i]["Construction"] + "' WHERE EmployeeCode = '" + dt.Rows[i]["ID"] + "' AND Time = '" + dttime.ToString("MM/dd/yyyy hh:mm:ss tt") + "' "
                                        //    + " END ELSE  begin INSERT INTO dbo.TTF_EvenLog ( EmployeeCode , ShiftName , DirectionAndStatus ,Time , TimeZone ,Location ,Address ,Description ,Type ,Date ,Construction) "
                                        //    + " VALUES  ('" + dt.Rows[i]["ID"] + "','" + dt.Rows[i]["ShiftName"] + "',N'" + dt.Rows[i]["DirectionAndStatus"] + "','" + dttime.ToString("MM/dd/yyyy hh:mm:ss tt") + "',N'" + dt.Rows[i]["TimeZone"] + "',N'" + dt.Rows[i]["Location"] + "',"
                                        //    + "N'" + dt.Rows[i]["Address"] + "',N'" + dt.Rows[i]["Description"] + "', N'" + dt.Rows[i]["Type"] + "','" + dttime.ToString("MM/dd/yyyy") + "','" + dt.Rows[i]["Construction"] + "') End ";
                                        //str += "'" + dt.Rows[i]["ID"] + "','" + dt.Rows[i]["ShiftName"] + "',N'" + dt.Rows[i]["DirectionAndStatus"] + "','" + dt.Rows[i]["Time"] + "',N'" + dt.Rows[i]["TimeZone"] + "',N'" + dt.Rows[i]["Location"] + "'";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rs.text = ex.Message + "dòng " + (i + 1).ToString() + time;
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                            }
                            try
                            {
                                if (str.Length > 0)
                                {
                                    kq = db.Database.ExecuteSqlCommand(str.ToString());
                                    if (kq > 0)
                                    {
                                        System.IO.File.Delete(resultFilePath);
                                        rs.text = "Thành công!<br>" + kq + " items!,";
                                        rs.code = 1;
                                        return Json(rs, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                rs.text = ex.Message;
                                return Json(rs, JsonRequestBehavior.AllowGet);
                            }
                            //int sc = Task.Run(() => db.SaveChangesAsync()).Result;
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.Delete(resultFilePath);
                    rs.text = ex.Message;
                    return Json(rs, JsonRequestBehavior.AllowGet);
                    

                }
            }
            return Json(rs,JsonRequestBehavior.AllowGet);
        }
        public DataTable getDataTableFromExcelNhanSu(string path, ref string sLoi)
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
                tbl.Columns.Add("sql", typeof(string));
                var startRow = hasHeader ? 2 : 1;
                int iIndex = 0, stt = 1;
                using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
                {
                    var dsCongTrinh = db.TTF_Mails.ToList();
                    var NhanSu = db.TTF_NhanSu.Where(it => it.MaTinhTrang == "1").ToList();
                    string value = "", time = "", loi = "";
                    string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                    DateTime dttime = new DateTime();
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        var row = tbl.NewRow();
                        foreach (var cell in wsRow)
                        {
                            value = cell.Text.Trim();
                            if (tbl.Columns[iIndex].ColumnName == "manv")
                            {
                                var checkNhanSu = NhanSu.FirstOrDefault(it => it.MaNV == value);
                                if (checkNhanSu == null)
                                {
                                    loi += "Dòng " + stt.ToString() + " có mã nv <b>" + value + "</b> không hợp lệ </br>";
                                }

                            }
                            if (tbl.Columns[iIndex].ColumnName == "congtrinh")
                            {
                                var checkCongTrinh = dsCongTrinh.FirstOrDefault(it => it.MaDuAn == value);
                                if (checkCongTrinh == null)
                                {
                                    loi += "Dòng " + stt.ToString() + " có mã công trình <b>" + value + "</b> không hợp lệ </br>";
                                }

                            }
                            row[cell.Start.Column - 1] = cell.Text.Trim();
                            iIndex++;
                        }
                        time = row["ngay"].ToString() + " " + row["thoigian"].ToString();


                        if (!DateTime.TryParseExact(time, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dttime))
                        {
                            loi += "Lỗi thời gian dòng " + (stt).ToString() + "<b>" + row["ngay"].ToString() + " " + row["thoigian"].ToString() + "</b></br>";
                        }
                        if (loi == "")
                        {
                            if (clsFunction.checkKyCongNhanSu(dttime.Date))
                            {
                                loi = "Lỗi kỳ công đã đóng dòng " +" "+ (stt).ToString() + "<b>" + row["ngay"].ToString() + " " + row["thoigian"].ToString() + "</b></br>";
                            }
                            else
                            {
                                row["sql"] = "INSERT INTO dbo.TTF_EvenLog (EmployeeCode,Time,Date,Construction,EmployeeID,NguoiDungTao,NgayTao)VALUES  ('" + row["manv"].ToString() + "','" + dttime.ToString("MM/dd/yyyy HH:mm:ss") + "','" + dttime.ToString("MM/dd/yyyy") + "','" + row["congtrinh"].ToString() + "','" + row["manv"].ToString() + "','" + nguoidung.NguoiDung + "','" + DateTime.Now.ToString("MM/dd/yyyy") + "')";
                            }
                           
                        }
                        sLoi += loi;
                        tbl.Rows.Add(row);
                        stt++;
                        iIndex = 0;
                        loi = "";
                    }
                }
               
                return tbl;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ImportExcelUploadNhanSu(HttpPostedFileBase FileInbox)
        {
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            if (FileInbox != null && FileInbox.ContentLength > 0 && (Path.GetExtension(FileInbox.FileName).Equals(".xlsx") || Path.GetExtension(FileInbox.FileName).Equals(".csv")))
            {
                string fileName = FileInbox.FileName;
                string UploadDirectory = Server.MapPath("~/Content/Temps/");
                bool folderExists = System.IO.Directory.Exists(UploadDirectory);
                if (!folderExists)
                    System.IO.Directory.CreateDirectory(UploadDirectory);
                string sLoi = "";
                string resultFilePath = UploadDirectory + fileName;
                try
                {
                    FileInbox.SaveAs(resultFilePath);
                    DataTable dt;
                    dt = getDataTableFromExcelNhanSu(resultFilePath, ref sLoi);
                    if (sLoi.Length > 0)
                    {
                        rs.text = sLoi;
                        return Json(rs,JsonRequestBehavior.AllowGet);
                    }
                    DateTime dptemp = new DateTime();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        using (TTF_FACEIDEntities db = new TTF_FACEIDEntities())
                        {
                            int i = 0; string str = ""; string error = ""; int kq = 0;
                            foreach (DataRow r in dt.Rows)
                            {
                                try
                                {
                                    //if (r["employeecode"].ToString().Trim() != null && dt.Rows[i]["time"].ToString().Trim().Length > 0)
                                    //{
                                    //    dptemp = DateTime.Parse(r["time"].ToString());
                                    //    str += "IF EXISTS (SELECT *  FROM dbo.TTF_EvenLog WHERE EmployeeCode = '" + r["employeecode"].ToString().Trim() + "' AND Time = '" + dptemp.ToString("MM/dd/yyyy hh:mm:ss tt") + "') "
                                    //        + "BEGIN  UPDATE dbo.TTF_EvenLog SET ShiftName = '" + r["shift"].ToString() + "',DirectionAndStatus = N'" + r["eventtype"].ToString() + "',Time = N'" + dptemp.ToString("MM/dd/yyyy hh:mm:ss tt") + "',TimeZone = N'" + r["timezone"].ToString() + "',Location = N'" + r["latitude"].ToString() + "," + r["longitude"].ToString() + "', "
                                    //        + " Address = N'" + r["address"].ToString() + "',Description = N'" + r["description"].ToString() + "',Type = N'" + r["contracttype"] + "',Date = '" + dptemp.ToString("MM/dd/yyyy") + "',Construction = '" + r["contructionid"].ToString().Replace(" ", "") + "',Remark = '" + r["remark"].ToString() + "',EmployeeID = '" + r["employeeid"].ToString() + "',Position = '" + r["position"].ToString() + "'  WHERE EmployeeCode = '" + r["employeecode"] + "' AND Time = '" + dptemp.ToString("MM/dd/yyyy hh:mm:ss tt") + "' "
                                    //        + " END ELSE  begin INSERT INTO dbo.TTF_EvenLog ( EmployeeCode , ShiftName , DirectionAndStatus ,Time , TimeZone ,Location ,Address ,Description ,Type ,Date ,Construction,Remark,EmployeeID,Position) "
                                    //        + " VALUES  ('" + r["employeecode"].ToString().Trim() + "','" + r["shift"].ToString() + "',N'" + r["eventtype"].ToString() + "','" + dptemp.ToString("MM/dd/yyyy hh:mm:ss tt") + "',N'" + r["timezone"].ToString() + "',N'" + r["latitude"].ToString() + "," + r["longitude"].ToString() + "',"
                                    //        + "N'" + r["address"].ToString() + "',N'" + r["description"].ToString() + "', N'" + r["contracttype"] + "','" + dptemp.ToString("MM/dd/yyyy") + "','" + r["contructionid"].ToString().Replace(" ", "") + "','" + r["remark"] + "','" + r["employeeid"] + "','" + r["position"] + "') End \n";
                                    //}
                                    //try
                                    //{
                                    //    db.Database.ExecuteSqlCommand(str.ToString());
                                    //    str = "";
                                    //    kq++;
                                    //}
                                    //catch 
                                    //{
                                    //    error += str + "/n";
                                    //    str = "";
                                    //}
                                    if (r["sql"].ToString().Length > 0)
                                    {
                                        str += r["sql"].ToString() + "\n";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rs.text = ex.Message + "dòng " + (i + 1).ToString();
                                    return Json(rs, JsonRequestBehavior.AllowGet);
                                }
                            }
                            kq = db.Database.ExecuteSqlCommand(str.ToString());
                            //int sc = Task.Run(() => db.SaveChangesAsync()).Result;
                            if (kq > 0)
                            {
                                System.IO.File.Delete(resultFilePath);
                                rs.text = "Success!<br>" + kq + " items!," + error;
                                rs.code = 1;
                                return Json(rs, JsonRequestBehavior.AllowGet);
                                //return Content(MvcHtmlString.Create("Success!<br>" + kq + " items!," + error).ToHtmlString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.Delete(resultFilePath);
                   // return Content(ex.Message);
                    rs.text = ex.Message;
                    return Json(rs, JsonRequestBehavior.AllowGet);

                }
                // System.IO.File.Delete(resultFilePath);
            }
            return Json(rs,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CapNhatCongTac(string NoiDung)
        {
            string sq = "";
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                string[] CongTac = NoiDung.Split(';'), temp;
                if (CongTac.Length > 0)
                {
                    string sCapNhat = "";
                    DateTime dttemp;
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NguoiEdit = (int)nguoidung.NguoiDung;
                    if (NguoiEdit < 0)
                    {
                        rs.text = "Hết thời gian thao tác đăng nhập lại";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                    foreach (var item in CongTac)
                    {
                        if (item == "")
                            break;
                        temp = item.Split('-');
                        if (!DateTime.TryParseExact(temp[1], Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dttemp))
                        {
                            sq += "Lỗi ngày <b>" + temp[1] + temp[0] + "<b/><br>";
                            // return View();
                        }
                        else
                        {
                            if (clsFunction.checkKyCongNhanSu(dttemp))
                            {
                                sq += "Thông tin <b>" + dttemp.ToString("dd/MM/yyyy") + " </b>kỳ công đã đống <br>";
                            }
                        }
                        //dttemp = DateTime.Parse(temp[1]);
                        sCapNhat += "UPDATE dbo.TTF_EvenLog SET CongTac = 1,NguoiCapNhatCongTac = '" + NguoiEdit + "',NgayCapNhatCongTac = GETDATE() WHERE EmployeeID = '" + temp[0] + "' AND Date = '" + dttemp.ToString("MM/dd/yyyy") + "' \n";
                    }
                    if (sq == "")
                    {
                        using (var db = new TTF_FACEIDEntities())
                        {
                            int kq = db.Database.ExecuteSqlCommand(sCapNhat);
                            if (kq > 0)
                            {
                                rs.text = "Thành công " + kq.ToString();
                                rs.code = 1;
                            }

                        }
                    }
                    else
                    {
                        rs.text = sq;
                    }
                }

            }
            catch (Exception ex)
            {
                rs.text = "Lỗi " + ex.Message;
            }

            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> HuyCapNhatCongTac(string NoiDung)
        {
            string sq = "";
            JsonStatus rs = new JsonStatus();
            rs.code = 0;
            try
            {
                string[] CongTac = NoiDung.Split(';'), temp;
                if (CongTac.Length > 0)
                {
                    string sCapNhat = "";
                    DateTime dttemp;
                    string[] Formats = System.Configuration.ConfigurationManager.AppSettings["DayFormat"].ToString().Split(',');
                    var nguoidung = Users.GetNguoiDung(User.Identity.Name);
                    int NguoiEdit = (int)nguoidung.NguoiDung;
                    if (NguoiEdit < 0)
                    {
                        rs.text = "Hết thời gian thao tác đăng nhập lại";
                        return Json(rs, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var item in CongTac)
                    {
                        if (item == "")
                            break;
                        temp = item.Split('-');
                        if (!DateTime.TryParseExact(temp[1], Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dttemp))
                        {
                            sq += "Lỗi ngày <b>" + temp[1] + temp[0] + "<b/><br>";
                            // return View();
                        }
                        else
                        {
                            if (clsFunction.checkKyCongNhanSu(dttemp))
                            {
                                sq += "Thông tin <b>" + dttemp.ToString("dd/MM/yyyy") + " </b>kỳ công đã đống <br>";
                            }
                        }
                        // dttemp = DateTime.Parse(temp[1]);
                        sCapNhat += "UPDATE dbo.TTF_EvenLog SET CongTac = 0,NguoiHuy = '" + NguoiEdit + "',NgayHuy = GETDATE() WHERE EmployeeID = '" + temp[0] + "' AND Date = '" + dttemp.ToString("MM/dd/yyyy") + "' \n";
                    }
                    if (sq == "")
                    {
                        using (var db = new TTF_FACEIDEntities())
                        {
                            int kq = db.Database.ExecuteSqlCommand(sCapNhat);
                            if (kq > 0)
                            {
                                rs.text = "Thành công " + kq.ToString();
                                rs.code = 1;
                            }
                        }
                    }
                    else
                    {
                        rs.text = sq;
                    }
                }

            }
            catch (Exception ex)
            {
                rs.text = "Lỗi " + ex.Message;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
    }
}