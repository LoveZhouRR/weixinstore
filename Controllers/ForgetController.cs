using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Services.Components.Email;
using DBC.WeChat.Services.Components.Email.Senders;
using DBC.WeChat.Services.Components.Security;
using DBC.WeChat.Services.Infrastructures.Configurations;
using DBC.WeChat.Services.Security;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ForgetController : Controller
    {
        //
        // GET: /Forget/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var account = form["Account"];
            if (string.IsNullOrWhiteSpace(account))
                return Json(new {success = false, msg = "帐号不存在"}, JsonRequestBehavior.AllowGet);
            var code = form["ValidateCode"];
            if (!Captcha.CompareAndDestroy(Session, code, true, "ForgetImg"))
            {
                return Json(new { success = false, msg = "验证码错误" }, JsonRequestBehavior.AllowGet);
            }
            account = account.Trim();
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            //匹配邮箱 匹配手机号
            if (Regex.IsMatch(account, @"^([\w\-]+)(\.[\w]+)*@([\w\-]+\.){1,5}([A-Za-z]){2,4}$") 
                || Regex.IsMatch(account, @"^(13[0-9]|15[012356789]|18[0123456789]|14[57])[0-9]{8}$"))
            {
                var tenant = svc.SelectOrEmpty(new TenantQuery() {Account = account}).FirstOrDefault();
                if (tenant == null) return Json(new {success = false, msg = "帐号不存在"}, JsonRequestBehavior.AllowGet);
                if (tenant.AccountType == (int) TenantAccountType.Email && tenant.EmailVerified != true)
                {
                    return Json(new {success = false, msg = "该邮箱帐号未验证，无法找回"}, JsonRequestBehavior.AllowGet);
                }
                if (tenant.AccountType == (int) TenantAccountType.Mobile && tenant.MobileVerified != true)
                {
                    return Json(new { success = false, msg = "该手机帐号未验证，无法找回" }, JsonRequestBehavior.AllowGet);
                }
                Session["Account"] = account;
                Session["Type"] = tenant.AccountType;
                Session["_TenantID"] = tenant.ID.Value;
            }
            else
            {
                return Json(new {success = false, msg = "帐号格式错误"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidateCode()
        {
            var image = Captcha.CreateAndSave(Session, "ForgetImg");
            return File(image.GetBuffer(), "image/jpeg");
        }

        public ActionResult Success()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Send()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Send(FormCollection form)
        {
            if (Session["Account"] == null) RedirectToAction("Index", "Login");
            if ((int?) Session["AccountType"] != (int) TenantAccountType.Email)
            {
                RedirectToAction("Index", "Login");
            }
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var tenant = svc.Select(new TenantQuery() {Account = (string) Session["Account"]}).FirstOrDefault();
            var verify = new EmailVerify()
                {
                    Code = Guid.NewGuid().ToString().Hash(),
                    Email = tenant.Account,
                    Name = tenant.Name,
                    ReferID = tenant.ID,
                    Type = (int) EmailVerifyTypes.TenantResetPwd,
                    Returl = ""
                };
            verify.Returl = new UriBuilder("http", Request.Url.Host, Request.Url.Port, "Forget/Verify",
                                     "?code=" + verify.Code).ToString();
            svc.Create(verify);
            Session["Account"] = null;
            Session["AccountType"] = null;
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return View();
            }
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var verify =
                svc.SelectOrEmpty(new EmailVerifyQuery()
                    {
                        Code = code,
                        Type = (int) EmailVerifyTypes.TenantResetPwd,
                        OrderDirection = OrderDirection.Desc,
                        OrderField = "ID",
                        Take = 1,
                        Skip = 0
                    }).FirstOrDefault();
            
            if (verify == null)
            {
                return View();
            }
            Session["VerifyID"] = verify.ID;
            return View();
        }
        [HttpPost]
        public ActionResult Verify(FormCollection form)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            if (Session["_TenantID"] == null)
            {
                return Json(new {success = false, msg=""}, JsonRequestBehavior.AllowGet);
            }
            var newPwd = form["NewPwd"];
            if (string.IsNullOrWhiteSpace(newPwd))
            {
                return Json(new {success = false, msg = "密码错误"}, JsonRequestBehavior.AllowGet);
            }
            var tenant =
                svc.SelectOrEmpty(new TenantQuery() {IDs = new[] {(long) Session["_TenantID"]}}).FirstOrDefault();
            
            tenant.Password = newPwd.Hash();
            Session["_TenantID"] = null;
            svc.Update(tenant);
            if (Session["VerifyID"] != null)
            {
                Guardian.Invoke(() => svc.Delete(new EmailVerify() {ID = (long?) Session["VerifyID"]}));
                Session["VerifyID"] = null;
            }
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendSMS()
        {
            if (Session["Account"] == null)
                return Json(new {success = false, msg = ""}, JsonRequestBehavior.AllowGet);
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var verify = new SMSVerify()
                {
                    Mobile = (string) Session["Account"],
                    Code = RandomUtil.Number(),
                    Type = SMSVerifyTypes.ResetPwd
                };
            svc.Create(verify);
            return Json(new {success = true, msg = ""}, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult MobileVerify(FormCollection form)
        {
            var code = form["Code"];
            var newPwd = form["NewPwd"];
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPwd))
            {
                return Json(new {success = false, msg = ""}, JsonRequestBehavior.AllowGet);
            }
            if (Session["_TenantID"] == null)
            {
                return Json(new {success = false, msg = ""}, JsonRequestBehavior.AllowGet);
            }
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var verify = svc.SelectOrEmpty(new SMSVerifyQuery()
                {
                    Mobile = (string)Session["Account"],
                    Type = SMSVerifyTypes.ResetPwd,
                    Take = 1,
                    Skip = 0,
                    OrderDirection = OrderDirection.Desc,
                    OrderField = "ID"
                }).FirstOrDefault();
            if (verify == null)
            {
                return Json(new {success = false, msg = "验证码已过期"}, JsonRequestBehavior.AllowGet);
            }
            if (verify.Code != code)
            {
                return Json(new {success = false, msg = "验证码错误"}, JsonRequestBehavior.AllowGet);
            }
            var tenant = new Tenant()
                {
                    ID = (long) Session["_TenantID"],
                    Password = newPwd.Hash()
                };
            Session["_TenantID"] = null;
            svc.Update(tenant);
            return Json(new {success = true, msg = ""}, JsonRequestBehavior.AllowGet);
        }
    }
}
