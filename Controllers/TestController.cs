using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.Models.Conversation.Msg;
using DBC.WeChat.Services.Conversation.Components;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            var conersation = new ConversationService()
            {
                ModelService = Service,
            };
            
            conersation.SendKeyResponse(new TextRequest()
            {
                FromUserName = "oFujfjp1j_JJrJBmap0gM3zOlEug",
                Content = "a"
            },OwnerID);
            return View();
        }

    }
}
