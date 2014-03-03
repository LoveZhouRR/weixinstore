using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Services.Security;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var vm = new HomePageVM();
            var svc = Service;
            var tenant = svc.SelectOrEmpty(new TenantQuery() {IDs = new long[] {UserID}, Includes = new []{"Owner","Owner.Config"}}).FirstOrDefault();
            vm.EmailVerified = tenant.EmailVerified ?? false;
            vm.UsedTime = DateTime.Now - (tenant.CreatedAt ?? DateTime.Now);
            vm.Account = tenant.Account;
            vm.Type = (TenantAccountType)tenant.AccountType.Value;
            vm.MobileVerified = tenant.MobileVerified ?? false;
            if (tenant.Owner != null && tenant.Owner.Config != null)
            {
                vm.TradeCount = tenant.Owner.Config.TradeCount ?? 0;
            }
            var paymentMethod =
                svc.SelectOrEmpty(new PaymentMethodConfigQuery() { OwnerIDs = new long[] { OwnerID } }).FirstOrDefault();
            vm.AlipayAccountBinded = (paymentMethod != null);
            return View(vm);
        }
        public ActionResult Error(string message)
        {
            return View();
        }
        public ActionResult ChangePassword(string oldPwd, string newPwd)
        {
            var svc = Service;
            var account = svc.SelectOrEmpty(new TenantQuery() { IDs = new long[] { UserID } }).FirstOrDefault();
            if (!Encryption.IsMatch(account.Password, oldPwd))
            {
                return Json(new { success = false, msg = "原密码错误" }, JsonRequestBehavior.AllowGet);
            }
            account.Password = newPwd.Hash();
            try
            {
                svc.Update(account);
            }
            catch (Exception error)
            {
                return Json(new { success = false, msg = error.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
