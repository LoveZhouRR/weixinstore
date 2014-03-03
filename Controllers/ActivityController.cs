using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Services;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ActivityController : BaseController
    {

        public ActionResult Index()
        {
            var query = new ActivityQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var initList = Service.Select(query).ToArray();
            ViewData["Pagination"] = Pagination.FromQuery(query);
            ViewData["Query"] = query;
            return View(initList);
        }

        [HttpPost]
        public ActionResult AjaxQuery(ActivityQuery query)
        {
            query.OwnerID = OwnerID;
            var activityList=Service.Select(query);
            var pro = Pagination.FromQuery(query);
            ViewData["Pagination"] = pro;
            return PartialView("Activity/ActivityList", activityList);
        }


        public ActionResult Add(int type)
        {
            var activity = new Activity()
            {
                OwnerID = OwnerID,
                Type = type,
            };
            return PartialView("Add",activity);
        }

        [HttpPost]
        public ActionResult Add(Activity activity)
        {
            activity.AwardTypes = GetTypes();
            var query = new ActivityQuery()
            {
                OwnerID = OwnerID,
                Take = PageSize,
                OrderField = "LastModifiedAt",
                OrderDirection = OrderDirection.Desc,
            };
            try
            {
                activity.RuleType = activity.RuleType ?? 1;
                Service.Create(activity);
                return AjaxQuery(query);
            }
            catch (RuleViolatedException exception)
            {
                ViewBag.Error = exception.Message;
                return AjaxQuery(query);
            }
        }


        public ActionResult Edit(long id)
        {
            var activity = Service.SelectOrEmpty(new ActivityQuery()
            {
                OwnerID = OwnerID,
                IDs = new long[]{id},
                Includes = new []{"AwardTypes"},
            }).FirstOrDefault();
            return View(activity);
        }

        [HttpPost]
        public ActionResult Edit(Activity activity)
        {
            try
            {
                activity.AwardTypes = GetTypes();
                if (activity.State == (int)ActivityState.Started || activity.State == (int)ActivityState.End)
                {
                    ViewBag.Error = "启用或终止状态的活动不能编辑";
                }
                else
                {
                    Service.Update(activity);
                }
                return PartialView(activity);
            }
            catch (RuleViolatedException exception)
            {
                ViewBag.Error = exception.Message;
                activity = Service.SelectOrEmpty(new ActivityQuery()
                {
                    OwnerID = OwnerID,
                    IDs = new long[] { activity.ID.Value },
                    Includes = new[] { "AwardTypes" },
                }).FirstOrDefault();
                return PartialView(activity);
            }
        }

        [HttpPost]
        public JsonResult ChangeState(long id, int state)
        {
            var response = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            if (CheckState(id, state))
            {
                Service.Update(new Activity()
                {
                    ID=id,
                    State = state
                });
                response.Data = new {success = true};
            }
            else
            {
                response.Data = new {success = false,msg="同类型活动只能有一个启用"};
            }
            return response;
        }

        [HttpPost]
        public JsonResult DeleteAward(long activityid,long id)
        {
            var response = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
            var activity = Service.SelectOrEmpty(new ActivityQuery()
            {
                IDs = new long[] {activityid}
            }).FirstOrDefault();
            if (activity == null||activity.State==(int)ActivityState.Started)
            {
                response.Data = new {success = false,msg="活动启用中或者不存在"};
                return response;
            }
            Service.Delete(new AwardType()
            {
                ID=id
            });
            response.Data = new {success=true};
            return response;
        }


        private bool CheckState(long id, int state)
        {
            var activity = Service.SelectOrEmpty(new ActivityQuery()
            {
                IDs=new long[]{id},
                OwnerID = OwnerID,
            }).FirstOrDefault();
            if (activity == null)
                return false;
            if (activity.State == (int) ActivityState.End)
                return false;
            if (state == (int) ActivityState.Started)
            {
                //当前类型的活动只有一个启用状态
                var started = Service.SelectOrEmpty(new ActivityQuery()
                {
                    Type = activity.Type,
                    State = (int)ActivityState.Started,
                    OwnerID = OwnerID,
                }).FirstOrDefault();
                if (started != null)
                {
                    return false;
                }
            }
            return true;
        }

        private ICollection<AwardType> GetTypes()
        {
            var response = new Collection<AwardType>();
            var count = Convert.ToInt32(Request.Form["AwardTypeCount"]);
            for (int i = 0; i < count; i++)
            {
                var awardType = new AwardType()
                {
                    ID= !string.IsNullOrEmpty(Request.Form["ID"+i])?Convert.ToInt32(Request.Form["ID"+i]):0,
                    OwnerID = OwnerID,
                    TypeName = Request.Form["TypeName"+i],
                    Name = Request.Form["Name"+i],
                    Count = !string.IsNullOrEmpty(Request.Form["Count" + i])? Convert.ToInt32(Request.Form["Count" + i]):0,
                    Probability = !string.IsNullOrEmpty(Request.Form["Probality" + i]) ? Convert.ToDecimal(Request.Form["Probality" + i]) : 0,
                    Reply = Request.Form["Reply"+i],
                };
                response.Add(awardType);
            }
            return response;
        }
    }
}
