using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class LoginController :Controller
    {
        //
        // GET: /Login/
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            Session["UserID"] = null;
            Session["OwnerID"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var account = form["Code"];
            var password = form["Password"];
            var type = form["LoginType"];
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(type) || (type != "email" && type != "mobile"))
            {
                return Json(new { content = "", title = "请输入用户名/密码", success = false }, JsonRequestBehavior.AllowGet);
            }
            var returnUrl = Request["returnUrl"];
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var logon = new TenantLogon()
                {
                    AccountType = (type=="email")?(int)TenantAccountType.Email:(int)TenantAccountType.Mobile,
                    Account = account,
                    Password = password,
                    IP = Request.UserHostAddress,
                    CreatedAt = DateTime.Now
                };
            try
            {
                svc.Create(logon);
                Session["OwnerID"] = logon.Tenant.OwnerID.Value;
                Session["UserID"] = logon.Tenant.ID.Value;
            }
            catch (RuleViolatedException error)
            {
                return Json(new { success = false, content = "", title = error.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception error)
            {
                return Json(new { success = false, content = "", title = error.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = returnUrl, success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
