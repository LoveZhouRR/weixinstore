using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Services;
using DBC.Utils.Serialization;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class DeliveryMethodController : BaseController
    {
        //
        // GET: /DeliveryMethod/
        [HttpGet]
        public ActionResult Index(DeliveryMethodQuery query)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            query.OwnerIDs = new long[] {OwnerID};
            var models = svc.SelectOrEmpty(query);
            //获取最大能创建的配送方式数量
            var maxNumber = 10;
            //maxNumber = svc.SelectOrEmpty(new StoreConfiguration(){IDs=new []{StoreID}}).FirstOrDefault().DeliveryMethodNumber;
            ViewData["MaxNumber"] = maxNumber;
            return View(models);
        }
        [HttpGet]
        public ActionResult Add()
        {
            var svc = Service;
            var areas = svc.SelectOrEmpty(new AreaQuery() {Depths = new int[] {1, 2, 3},Enabled = true});
            var areaJson = ServiceLocator.Resolve<ISerializer>("Json").Serialize(areas);
            ViewData["AreaJson"] = areaJson;
            return View();
        }
        [HttpPost]
        public ActionResult Add(DeliveryMethod method)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var areas = Request["SelectAreaIDs"];
            if (areas != null)
            {
                try
                {
                    var ids = areas.Split(',').Select(o => long.Parse(o)).ToArray();
                    method.Areas = ids.Select(o => new DeliveryMethodArea() {AreaID = o}).ToList();
                }
                catch
                {
                    
                }

            }
            try
            {
                svc.Create(method);
            }
            catch (RuleViolatedException ex)
            {
                return Json(new {success = false, msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {success = false, msg = "未知错误"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {success = true,msg=""}, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Edit(long id)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var item = svc.SelectOrEmpty(new DeliveryMethodQuery() {IDs = new[] {id}}).FirstOrDefault();
            var areas =
                svc.SelectOrEmpty(new DeliveryMethodAreaQuery() {DeliveryMethodIDs = new long[] {item.ID.Value}})
                   .ToArray();
            ViewData["SelectAreaIDs"] = string.Join(",", areas.Select(o => o.AreaID.Value));
            var areaData = svc.SelectOrEmpty(new AreaQuery() { Depths = new int[] { 1, 2, 3 }, Enabled = true });
            var areaJson = ServiceLocator.Resolve<ISerializer>("Json").Serialize(areaData);
            ViewData["AreaJson"] = areaJson;
            return View(item);
        }
        [HttpPost]
        public ActionResult Edit(DeliveryMethod method)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            
            try
            {
                method.OwnerID = OwnerID;
                svc.Update(method);
                var areas = Request["SelectAreaIDs"];
                if (areas != null)
                {
                    try
                    {
                        var ids = areas.Split(',').Select(o => long.Parse(o)).ToArray();
                        var currents =
                            svc.SelectOrEmpty(new DeliveryMethodAreaQuery()
                                {
                                    DeliveryMethodIDs = new long[] {method.ID.Value}
                                }).ToArray();
                        if(currents.Any()) svc.Delete(currents);
                        if (ids.Any())
                        {
                            svc.Create(ids.Select(o=>new DeliveryMethodArea(){AreaID = o, DeliveryMethodID = method.ID}).ToArray());
                        }
                    }
                    catch
                    {

                    }

                }
            }
            catch (RuleViolatedException ex)
            {
                return Json(new { success = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "未知错误" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, msg = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetState(long id, bool enable)
        {
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var item = svc.SelectOrEmpty(new DeliveryMethodQuery() {IDs = new[] {id},OwnerIDs = new long[]{OwnerID}}).FirstOrDefault();
            if (item == null)
            {
                return Json(new {message = "指定配送方式不存在", success = false}, JsonRequestBehavior.AllowGet);
            }
            if (item.Enabled == enable)
                return Json(new {success = true, currentState = item.Enabled}, JsonRequestBehavior.AllowGet);
            item.Enabled = enable;
            try
            {
                svc.Update(item);
                return Json(new {success = true, currentState = item.Enabled}, JsonRequestBehavior.AllowGet);
            }
            catch (RuleViolatedException ex)
            {
                return Json(new {message = ex.Message, success = false}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {message = "未知错误", success = false}, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
