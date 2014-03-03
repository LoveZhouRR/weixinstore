using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    [FilterError]
    public class BindEmailController : Controller
    {
        //
        // GET: /BindEmail/

        public ActionResult Index(string code)
        {
            var ret = new Ret();
            if (string.IsNullOrWhiteSpace(code))
            {
                ret.msg = "该链接已失效";
                return View(ret);
            }
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var verify =
                svc.SelectOrEmpty(new EmailVerifyQuery()
                    {
                        Code = code,
                        Type = (int) EmailVerifyTypes.TenantRegister,
                        OrderDirection = OrderDirection.Desc,
                        OrderField = "ID"
                    }).FirstOrDefault();
            if (verify == null)
            {
                ret.msg = "该链接已失效";
            }
            else
            {
                svc.Update(new Tenant() {ID = verify.ReferID, EmailVerified = true});
                svc.Delete(verify);
                ret.success = true;
                ret.msg = "恭喜您，邮箱验证成功";
            }
            return View(ret);
        }

        public class Ret
        {
            public bool success;
            public string msg;
        }
    }
}
