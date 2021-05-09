using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Libs;
using System.Web.Mvc;
using TTFPortal.Class;

namespace TTFPortal.Controllers
{
    [RoleAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            var nguoidung = Users.GetNguoiDung(User.Identity.Name);


            return View();
        }
       

    }
}