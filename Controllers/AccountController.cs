using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.WeChat.Services.Conversation.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Conversation;
using DBC.Ors.Services;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IModelService svc = ServiceLocator.Resolve<IModelService>("Internal");

        public ActionResult Index()
        {
            ViewBag.Url = ConfigurationManager.AppSettings["ConversationUrl"]+OwnerID;
            ViewBag.Token = GetToken(OwnerID);
            var account = svc.SelectOrEmpty(new WeChatAccountQuery() {OwnerID = OwnerID}).FirstOrDefault();
            return View(account);
        }
        
        [HttpPost]
        public ActionResult Index(WeChatAccount account)
        {
            try
            {
                if (account.ID == null)
                {
                    svc.Create(account);
                }
                else
                {
                    svc.Update(account);
                }
                ViewBag.Url = account.Url;
                ViewBag.Token = account.Token;
                return View(account);
            }
            catch (RuleViolatedException e)
            {
                ViewBag.Error = e.Message;
                ViewBag.Url = account.Url;
                ViewBag.Token = account.Token;
                return View(account);
            }
        }


        private string GetToken(long ownerID)
        {
            return "Token"+ownerID;
        }

    }
}
