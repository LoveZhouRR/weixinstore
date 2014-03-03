using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Malls;
using DBC.WeChat.Models.Sales;
using DBC.WeChat.Services.Components.EF;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class CommentController : BaseController
    {
        //
        // GET: /Comment/

        public ActionResult Index(long id)
        {
            var query = new CommentQuery()
            {
                OwnerID = OwnerID,
                ProductID = id,
                Includes = new[] {"Product"},
                Take = PageSize,
            };
            var comments = Service.SelectOrEmpty(query);
            ViewData["Pagination"] = Pagination.FromQuery(query);
            return View(comments.ToArray());
        }

        [HttpPost]
        public JsonResult ShowOrNot(long id,bool isShow)
        {
            Service.Update(new Comment()
            {
                ID=id,
                IsShow = isShow
            });
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new {success=true},
            };
        }

    }
}
