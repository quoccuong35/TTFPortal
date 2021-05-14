using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TTFPortal.Models;

namespace TTFPortal.Class
{
    public class PhanQuyenModel
    {
        public IEnumerable<HT_NHOMQUYEN> NhomQuyen { get; set; }
        public IEnumerable<QuyenSuDungModel> QuyenSuDung { get; set; }
    }
    public class QuyenSuDungModel
    {
        public int QuyenSuDung { get; set; }
        public string TenQuyenSuDung { get; set; }
        public Nullable<int> NhomQuyen { get; set; }
        public string TableLink { get; set; }
        public Nullable<bool> IsDel { get; set; }
        public int Quyen { get; set; }
        public Nullable<bool> IsThuocNhom { get; set; }
    }
}