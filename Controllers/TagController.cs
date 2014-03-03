using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Services;
using DBC.WeChat.Models.Sales;
using DBC.WeChat.UI.Mvc;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class TagController : BaseController
    {
        public JsonResult Add(string name,long id)
        {
            var response = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
            try
            {
                var tag = Service.Select(new TagQuery()
                {
                    OwnerID = OwnerID,
                    Name = name,
                }).FirstOrDefault();
                if (tag != null)
                {
                    var exist = Service.SelectOrEmpty(new ProductTagQuery()
                    {
                        ProductID = id,
                        TagIDs = new[] { tag.ID.Value },
                    }).FirstOrDefault();
                    if (exist == null)
                    {
                        Service.Create(new ProductTag()
                        {
                            ProductID = id,
                            TagID = tag.ID,
                        });
                        response.Data = new { success = true, tagID = tag.ID };
                    }
                    else
                    {
                        response.Data = new { success = false };
                    }
                }
                else
                {
                    var newTag = new Tag()
                    {
                        OwnerID = OwnerID,
                        Name = name,
                    };
                    Service.Create(newTag);
                    Service.Create(new ProductTag()
                    {
                        ProductID = id,
                        TagID = newTag.ID,
                    });
                    response.Data = new { success = true, tagID = newTag.ID };
                }
                return response;
            }
            catch (Exception e)
            {
                response.Data = new { success = false };
                return response;
            }
            
        }

        public JsonResult Delete(long productid, long tagid)
        {
            var response = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
            var producttag = Service.SelectOrEmpty(new ProductTagQuery()
            {
                ProductID = productid,
                TagIDs = new []{tagid},
            }).FirstOrDefault();
            if (producttag == null)
            {
                response.Data = new {success = false};
            }
            else
            {
                Service.Delete(producttag);
                response.Data = new {success = true};
            }
            return response;
        }
       

    }
}
