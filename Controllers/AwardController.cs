using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class AwardController : BaseController
    {

        public ActionResult Index(long activityID)
        {
            ViewBag.ActivityID = activityID;
            var query=new AwardQuery()
            {
                Take =  PageSize,
                OrderField =  "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var awardtypes = Service.Select(new AwardTypeQuery()
            {
                ActivityID = activityID,
                OwnerID = OwnerID,
                Take = PageSize,
            }).ToArray();
            if (!awardtypes.Any())
            {
                var pro = Pagination.FromQuery(query);
                ViewData["Pagination"] = pro;
                ViewData["Query"] = query;
                return View();
            }
            else
            {
                query.Take = query.Take ?? PageSize;
                query.OrderField = query.OrderField ?? "LastModifiedAt";
                query.OrderDirection = query.OrderDirection ?? OrderDirection.Desc;
                query.AwardTypeIDs = awardtypes.Select(o => o.ID).OfType<long>().ToArray();
                var initList = Service.Select(query).ToArray();
                initList = initList.Select(o =>
                {
                    o.AwardType = awardtypes.FirstOrDefault(p => p.ID == o.AwardTypeID);
                    return o;
                }).ToArray();
                var pro = Pagination.FromQuery(query);
                ViewData["Pagination"] = pro;
                ViewData["Query"] = query;
                return View(initList);
            }   
        }

        [HttpPost]
        public ActionResult AjaxQuery(AwardQuery query)
        {
            long activityID = Convert.ToInt64(Request.Form["ActivityID"]);
            var awardtypes = Service.Select(new AwardTypeQuery()
            {
                ActivityID = activityID,
                OwnerID = OwnerID,
                Take = PageSize,
            }).ToArray();
            if (!awardtypes.Any())
            {
                ViewData["Pagination"] = Pagination.FromQuery(query);
                ViewData["Query"] = query;
                return PartialView("Award/AwardList");
            }
            else
            {
                query.Take = query.Take ?? PageSize;
                query.OrderField = query.OrderField ?? "LastModifiedAt";
                query.OrderDirection = query.OrderDirection ?? OrderDirection.Desc;
                query.AwardTypeIDs = awardtypes.Select(o => o.ID).OfType<long>().ToArray();
                var initList = Service.Select(query).ToArray();
                initList = initList.Select(o =>
                {
                    o.AwardType = awardtypes.FirstOrDefault(p => p.ID == o.AwardTypeID);
                    return o;
                }).ToArray();
                var pro = Pagination.FromQuery(query);
                ViewData["Pagination"] = pro;
                ViewData["Query"] = query;
                return PartialView("Award/AwardList",initList);
            }   
          
        }

        [HttpPost]
        public JsonResult ChangeState(long id, int state)
        {
            Service.Update(new Award()
            {
                ID = id,State = state
            });
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new {success=true}
            };
        }
    }
}
