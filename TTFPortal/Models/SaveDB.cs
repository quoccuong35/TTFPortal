using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TTFPortal.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;
using TTFPortal.Class;
using System.Text;

namespace TTFPortal.Models
{
    public class SaveDB :TTF_FACEIDEntities
    {
        public virtual DbSet<TTF_ChangeLogs> ChangeLogs { get; set; }
        public string GhiChu { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //omitted for brevity
        }

        string GetPrimaryKeyValue(DbEntityEntry entry)
        {
            string sq = "";
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            for (int i = 0; i < objectStateEntry.EntityKey.EntityKeyValues.Length; i++)
            {
                sq += objectStateEntry.EntityKey.EntityKeyValues[i].Value.ToString()+";";
            }
            return sq.Substring(0,sq.Length - 1);
        }
        public override int SaveChanges()
        {
            string tablename = "", type = ""; 
            try
            {
                //var modifiedEntities = ChangeTracker.Entries()
                //    .Where(p => p.State == System.Data.Entity.EntityState.Modified).ToList();
                var modifiedEntities = ChangeTracker.Entries().ToList();
                var now = DateTime.Now;
                List<TTF_ChangeLogs> LichSu = new List<TTF_ChangeLogs>();
                var NguoiDung = Users.GetNguoiDung(HttpContext.Current.User.Identity.Name);
                foreach (var change in modifiedEntities)
                {
                    if (change.State == System.Data.Entity.EntityState.Modified)
                    {
                        tablename = change.Entity.GetType().Name;
                        type = "Sửa";
                        var primaryKey = GetPrimaryKeyValue(change);

                        foreach (var prop in change.OriginalValues.PropertyNames)
                        {
                            var originalValue = change.OriginalValues[prop] != null? change.OriginalValues[prop].ToString():"";
                            var currentValue = change.CurrentValues[prop] != null? change.CurrentValues[prop].ToString():"";
                            if (originalValue != currentValue)
                            {
                                TTF_ChangeLogs log = new TTF_ChangeLogs()
                                {
                                    TableName = tablename,
                                    PrimaryKeyValue = primaryKey.ToString(),
                                    ColumName = prop,

                                    OldValue = originalValue,
                                    NewValue = currentValue,
                                    DateChanged = now,
                                    GhiChu = this.GhiChu,
                                    Type = "Modified",
                                    NguoiDung = (int)NguoiDung.NguoiDung
                                };
                                LichSu.Add(log);
                               // ChangeLogs.Add(log);
                            }
                        }
                    }
                    //else if (change.State == System.Data.Entity.EntityState.Added)
                    //{
                    //    tablename = change.Entity.GetType().Name;
                    //    type = "Thêm";
                    //   // var primaryKey = GetPrimaryKeyValue(change);

                    //    foreach (var prop in change.CurrentValues.PropertyNames)
                    //    {
                    //        // var originalValue = change.OriginalValues[prop].ToString();
                    //        var currentValue = change.CurrentValues[prop];
                    //        if (currentValue == null)
                    //            continue;
                    //        TTF_ChangeLogs log = new TTF_ChangeLogs()
                    //        {
                    //            TableName = tablename,
                    //            PrimaryKeyValue = "",
                    //            ColumName = prop,
                    //            OldValue = "",
                    //            NewValue = currentValue.ToString(),
                    //            DateChanged = now,
                    //            GhiChu = this.GhiChu,
                    //            Type = "Added",
                    //            NguoiDung = (int)NguoiDung.NguoiDung
                    //        };
                    //        LichSu.Add(log);
                    //        //ChangeLogs.Add(log);
                    //    }
                    //}
                    else if (change.State == System.Data.Entity.EntityState.Deleted)
                    {
                        tablename = change.Entity.GetType().Name;
                        type = "Xóa";
                        var primaryKey = GetPrimaryKeyValue(change);

                        foreach (var prop in change.OriginalValues.PropertyNames)
                        {
                            var originalValue = change.OriginalValues[prop].ToString();
                            // var currentValue = change.CurrentValues[prop].ToString();
                            TTF_ChangeLogs log = new TTF_ChangeLogs()
                            {
                                TableName = tablename,
                                PrimaryKeyValue = primaryKey.ToString(),
                                ColumName = prop,
                                OldValue = originalValue,
                                NewValue = "",
                                DateChanged = now,
                                GhiChu = this.GhiChu,
                                Type = "Deleted",
                                NguoiDung = (int)NguoiDung.NguoiDung
                            };
                             LichSu.Add(log); 
                            //ChangeLogs.Add(log);
                        }
                    }

                }
                ChangeLogs.AddRange(LichSu);
                return base.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                    eve.Entry.Entity.GetType().Name,
                                                    eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                    ve.PropertyName,
                                                    ve.ErrorMessage));
                    }
                }
                clsFunction.NhatkyLoi(DateTime.Now, HttpContext.Current.User.Identity.Name, sb.ToString(), tablename, type);
                return 0;
            }

        }
        public override Task<int> SaveChangesAsync()
        {
            string tablename = "", type = ""; ;
            try
            {
                //var modifiedEntities = ChangeTracker.Entries()
                //    .Where(p => p.State == System.Data.Entity.EntityState.Modified).ToList();
                var modifiedEntities = ChangeTracker.Entries().ToList();
                var now = DateTime.UtcNow;
                var NguoiDung = Users.GetNguoiDung(HttpContext.Current.User.Identity.Name);
                foreach (var change in modifiedEntities)
                {
                    if (change.State == System.Data.Entity.EntityState.Modified)
                    {
                        tablename = change.Entity.GetType().Name;
                        type = "Sửa";
                        var primaryKey = GetPrimaryKeyValue(change);

                        foreach (var prop in change.OriginalValues.PropertyNames)
                        {
                            var originalValue = change.OriginalValues[prop] != null ? change.OriginalValues[prop].ToString() : "";
                            var currentValue = change.CurrentValues[prop] != null ? change.CurrentValues[prop].ToString() : "";
                            if (originalValue != currentValue)
                            {
                                TTF_ChangeLogs log = new TTF_ChangeLogs()
                                {
                                    TableName = tablename,
                                    PrimaryKeyValue = primaryKey.ToString(),
                                    ColumName = prop,

                                    OldValue = originalValue,
                                    NewValue = currentValue,
                                    DateChanged = now,
                                    GhiChu = this.GhiChu,
                                    NguoiDung = (int)NguoiDung.NguoiDung,
                                    Type = "Modified",
                                };
                                ChangeLogs.Add(log);
                            }
                        }
                    }
                    //else if (change.State == System.Data.Entity.EntityState.Added)
                    //{
                    //    tablename = change.Entity.GetType().Name;
                    //    type = "Thêm";
                    //    //   var primaryKey = GetPrimaryKeyValue(change);

                    //    foreach (var prop in change.CurrentValues.PropertyNames)
                    //    {
                    //        // var originalValue = change.OriginalValues[prop].ToString();
                    //        var currentValue = change.CurrentValues[prop];
                    //        if (currentValue == null)
                    //            continue;
                    //        TTF_ChangeLogs log = new TTF_ChangeLogs()
                    //        {
                    //            TableName = tablename,
                    //            PrimaryKeyValue = "",
                    //            ColumName = prop,
                    //            OldValue = "",
                    //            NewValue = currentValue.ToString(),
                    //            DateChanged = now,
                    //            GhiChu = this.GhiChu,
                    //            Type = "Added",
                    //            NguoiDung = (int)NguoiDung.NguoiDung
                    //        };
                    //        ChangeLogs.Add(log);
                    //    }
                    //}
                    else if (change.State == System.Data.Entity.EntityState.Deleted)
                    {
                        tablename = change.Entity.GetType().Name;
                        type = "Xóa";
                        var primaryKey = GetPrimaryKeyValue(change);

                        foreach (var prop in change.OriginalValues.PropertyNames)
                        {
                            var originalValue = change.OriginalValues[prop].ToString();
                            // var currentValue = change.CurrentValues[prop].ToString();
                            TTF_ChangeLogs log = new TTF_ChangeLogs()
                            {
                                TableName = tablename,
                                PrimaryKeyValue = primaryKey.ToString(),
                                ColumName = prop,
                                OldValue = originalValue,
                                NewValue = "",
                                DateChanged = now,
                                GhiChu = this.GhiChu,
                                Type = "Deleted",
                                NguoiDung = (int)NguoiDung.NguoiDung
                            };
                            ChangeLogs.Add(log);
                        }
                    }

                }
                return base.SaveChangesAsync();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                    eve.Entry.Entity.GetType().Name,
                                                    eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                    ve.PropertyName,
                                                    ve.ErrorMessage));
                    }
                }
                clsFunction.NhatkyLoi(DateTime.Now, HttpContext.Current.User.Identity.Name, sb.ToString(), tablename, type);
                return Task.FromResult(0);
            }
        }
    }
}