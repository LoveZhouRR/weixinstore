using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    [FilterError]
    public class LogoutController : Controller
    {
        //
        // GET: /Logout/

        public ActionResult Index()
        {
            Session["UserID"] = null;
            return View();
        }

    }
}
