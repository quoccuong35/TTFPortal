//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TTFPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TTF_ChangeLogs
    {
        public string TableName { get; set; }
        public string ColumName { get; set; }
        public System.DateTime DateChanged { get; set; }
        public string PrimaryKeyValue { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Type { get; set; }
        public string GhiChu { get; set; }
        public long ID { get; set; }
        public Nullable<int> NguoiDung { get; set; }
    }
}