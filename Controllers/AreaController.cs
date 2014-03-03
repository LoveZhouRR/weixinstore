using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Services;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class AreaController : BaseController
    {
        //
        // GET: /Area/
        private readonly IModelService svc = ServiceLocator.Resolve<IModelService>("Internal");
        public JsonResult GetItems(long parentID)
        {
            IEnumerable<Area> items = svc.Select(new AreaQuery() {ParentIDs = new long[] {parentID}});
            var itemViews = items.Select(o=>new AreaView
                {
                    ID = o.ID,
                    Parent = o.ParentID,
                    Name = o.Name,
                }).ToArray();
            return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new {AreaViews=itemViews}
                };
        }


        public JsonResult GetAreaTrees(long ID)
        {
            var response = new JsonResult() {JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            if (ID == 0 || ID == 1)
            {
                response.Data = new {Success = true};
                return response;
            }
            var target = svc.Select(new AreaQuery() { IDs = new long[] { ID } }).FirstOrDefault();
            if (target == null)
            {
                response.Data = new {Success = false};
                return response;
            }
            var ancestors = GetAncestors(target);
            if (!ancestors.Any())
            {
                response.Data = new { Success = true };
            }
            if (ancestors.Count() == 1)
            {
                response.Data = new {Success = true, Province = ancestors[0]};
            }
            if (ancestors.Count() == 2)
            {
                response.Data = new {Success = true, City = ancestors[0], Province = ancestors[1]};
            }
            return response;
        }

        /// <summary>
        /// 此处认为地址深度为3
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        private long[] GetAncestors(Area area)
        {
            List<long> response=new List<long>();
            if (area.ParentID != null && area.ParentID.Value != 0)
            {
                response.Add(area.ParentID.Value);
                var parent = svc.Select(new AreaQuery() {IDs = new long[] {area.ParentID.Value}}).FirstOrDefault();
                if ((parent != null && parent.ParentID != null)
                    && parent.ParentID.Value != 0)
                    response.Add(parent.ParentID.Value);
            }
            return response.ToArray();
        }
    }
}
