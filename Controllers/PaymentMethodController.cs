using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class PaymentMethodController : BaseController
    {
        //
        // GET: /PaymentMethod/
        [HttpGet]
        public ActionResult Index()
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var paymentMethod = new PaymentMethodConfig() {Enabled = true};
            var config = svc.SelectOrEmpty(new PaymentMethodConfigQuery() {OwnerIDs = new long[] {OwnerID}}).FirstOrDefault();
            if (config != null) paymentMethod = config;

            return View(paymentMethod);
        }
        [HttpPost]
        public ActionResult Index(PaymentMethodConfig payment)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            //payment.AsynCallbackUrl = null;
            payment.Code = PaymentMethods.Alipay;
            payment.CreatedAt = null;
            payment.CreatorID = null;
            payment.LastModifiedAt = null;
            payment.LastModifierID = null;
            payment.Name = "支付宝";
            payment.OwnerID = OwnerID;
            
            //payment.SyncCallbackUrl = null;
            var current = svc.SelectOrEmpty(new PaymentMethodConfigQuery() {OwnerIDs = new long[] {OwnerID}}).FirstOrDefault();
            try
            {
                if (current != null)
                {
                    payment.ID = current.ID;
                    svc.Update(payment);
                }
                else
                {
                    svc.Create(payment);
                }
            }
            catch (RuleViolatedException ex)
            {
                return Json(new {success = false, msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {success = false, msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
    }
}
