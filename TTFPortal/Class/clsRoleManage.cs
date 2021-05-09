using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using TTFPortal.Models;

namespace TTFPortal.Class
{
    public class clsRoleManage : RoleProvider
    {
        public override string[] GetRolesForUser(string sTenDangNhap)
        {
            if (sTenDangNhap.Equals("administrator"))
                return new String[] { "0=0" };
            else
            {
                try
                {
                    TTF_FACEIDEntities db = new TTF_FACEIDEntities();
                    var sQuyen = (from nguoidung in db.HT_NGUOIDUNG
                                  join nhomnguoidung in db.HT_NHOMNGUOIDUNG on nguoidung.NHOMNGUOIDUNG equals nhomnguoidung.NHOMNGUOIDUNG into nguoi
                                  from n in nguoi.DefaultIfEmpty()
                                  where nguoidung.TAIKHOAN == sTenDangNhap
                                  select new
                                  {
                                      Quyen = nguoidung.QUYEN,
                                      Quyen2 = n.QUYEN,
                                      MaNhomQuyen = n.TENNHOMNGUOIDUNG
                                  }).FirstOrDefault();
                    string q = "";
                    if (sQuyen.MaNhomQuyen.ToString().ToLower() == "admin")
                    {
                        return new String[] { "0=0" };
                    }
                    else
                    {
                        q = !String.IsNullOrEmpty(sQuyen.Quyen2) ? sQuyen.Quyen + "|" + sQuyen.Quyen2 : sQuyen.Quyen;
                        return q.Split('|');
                    }

                }
                catch
                {
                    return new String[] { "" };
                }
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}