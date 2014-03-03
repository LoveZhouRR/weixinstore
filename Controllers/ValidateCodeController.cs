using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ValidateCodeController : Controller
    {
        //
        // GET: /ValidateCode/

        public ActionResult Index()
        {
            var ms = Captcha.CreateAndSave(Session);
            return File(ms.GetBuffer(), "image/JPEG");
        }

    }
}
