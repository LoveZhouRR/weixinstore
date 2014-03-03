using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.IO;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class RegisterController : Controller
    {
        //
        // GET: /Register/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var account = form["Account"];
            var pasword = form["Password"];
            var code = form["ValidateCode"];
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(pasword) ||
                string.IsNullOrWhiteSpace(code))
            {
                return Json(new {success = false, msg = "账户名/密码为空"}, JsonRequestBehavior.AllowGet);
            }
            account = account.Trim();
            pasword = pasword.Trim();
            code = code.Trim();
            if (!Captcha.CompareAndDestroy(Session, code))
            {
                return Json(new { success = false, msg = "验证码错误" }, JsonRequestBehavior.AllowGet);
            }
            var tenant = new Tenant()
                {
                    Account = account,
                    AccountType = (int) TenantAccountType.Email,
                    Password = pasword,
                    Email = account,
                    Owner = new DBC.WeChat.Models.Infrastructures.Store()
                        {
                            Enabled = false
                        }
                };
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            try
            {
                svc.Create(tenant);
                tenant.CreatorID = null;
                tenant.LastModifiedAt = null;
                tenant.LastModifierID = null;
                RegEmail.Send(tenant, Request);
                Session["Tenant"] = tenant;
            }
            catch (RuleViolatedException ex)
            {
                return Json(new {success = false, msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {success = false, msg = "未知错误"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Check(string account)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var item = svc.SelectOrEmpty(new TenantQuery() {Account = account}).FirstOrDefault();
            return Json(item == null ? new {success = true} : new {success = false}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MobileRegister(FormCollection form)
        {
            var account = form["M_Account"];
            var pasword = form["M_Password"];
            var code = form["M_Code"];
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(pasword) ||
                string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, msg = "账户名/密码为空" }, JsonRequestBehavior.AllowGet);
            }
            account = account.Trim();
            pasword = pasword.Trim();
            code = code.Trim();
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var verify =
                svc.Select(new SMSVerifyQuery()
                    {
                        Type = SMSVerifyTypes.Account,
                        Mobile = account,
                        Take = 1,
                        Skip = 0,
                        OrderDirection = OrderDirection.Desc,
                        OrderField = "CreatedAt",
                        CreatedAtRange = new Range<DateTime>() {Left = DateTime.Now.AddMinutes(-2), Right = DateTime.Now}
                    })
                   .FirstOrDefault();
            
            if (verify== null)
            {
                return Json(new {succes = false, msg = "验证码错误或已失效"}, JsonRequestBehavior.AllowGet);
            }
            if (string.Compare(code, verify.Code, System.StringComparison.OrdinalIgnoreCase) != 0)
            {
                return Json(new {success = false, msg = "验证码错误"}, JsonRequestBehavior.AllowGet);
            }
            var tenant = new Tenant()
            {
                Account = account,
                AccountType = (int)TenantAccountType.Mobile,
                Password = pasword,
                Mobile = account,
                Owner = new DBC.WeChat.Models.Infrastructures.Store()
                {
                    Enabled = false
                },
                MobileVerified = true
            };
            
            try
            {
                svc.Create(tenant);

            }
            catch (RuleViolatedException ex)
            {
                return Json(new { success = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "未知错误" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Confirm()
        {
            if (Session["Tenant"] == null)
            {
                return View("error");
            }
            var tenant = (Tenant) Session["Tenant"];
            return View(tenant);
        }
        //TODO
        [HttpPost]
        public ActionResult SendMobileCode(string mobile)
        {
            if (Regex.IsMatch(mobile, "^(13[0-9]|15[012356789]|18[0123456789]|14[57])[0-9]{8}$"))
            {
                //send message
                if (Session["LastSendSMS"] != null)
                {
                    var time = (DateTime)Session["LastSendSMS"];
                    if (DateTime.Now - time < new TimeSpan(0, 1, 0))
                    {
                        return Json(new { success = false, msg = "发送频率过快" }, JsonRequestBehavior.AllowGet);
                    }
                }
                Session["LastSendSMS"] = DateTime.Now;
                var svc = ServiceLocator.Resolve<IModelService>("Internal");
                var tenant = svc.Select(new TenantQuery() {Account = mobile}).FirstOrDefault();
                if (tenant != null)
                {
                    return Json(new {success = false, msg = "该手机号已经注册"}, JsonRequestBehavior.AllowGet);
                }
                var verify = new SMSVerify()
                    {
                        Code = RandomUtil.Number(),
                        Mobile = mobile,
                        Type = SMSVerifyTypes.Account
                    };
                
                svc.Create(verify);
                return Json(new {success = true, msg = verify.Code}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {success = false, msg = "手机号错误"}, JsonRequestBehavior.AllowGet);
        }

        
        bool CompareCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var verify = Session["SMSVerify"];
            if (verify == null) return false;
            return System.String.Compare((string) verify, code, System.StringComparison.OrdinalIgnoreCase) == 0;
        }

        [HttpGet]
        public ActionResult success()
        {
            return View();
        }
    }
}
