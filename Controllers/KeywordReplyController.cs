using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DBC.Utils.Serialization;
using DBC.WeChat.Models.Conversation;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class KeywordReplyController : BaseController
    {
        //
        // GET: /KeywordReply/
        IEnumerable<KeywordReplyVM> QueryAll()
        {
            var svc = Service;
            var rules = svc.Select(new RuleQuery() { OwnerID = OwnerID, Type = (int)RuleType.Normal });
            if (rules.Any())
            {
                var keywords =
                    svc.Select(new KeyWordQuery() { RuleIDs = rules.Select(o => o.ID).OfType<long>().ToArray() });
                var replies = svc.Select(new ReplyQuery() { RuleIDs = rules.Select(o => o.ID).OfType<long>().ToArray() });

                var textreplies =
                    svc.Select(new TextReplyItemQuery() { ParentIDs = replies.Select(o => o.ID).OfType<long>().ToArray() });
                var resourcereplyitems =
                    svc.Select(new ReplyResourceItemQuery()
                        {
                            ReplyIDs = replies.Select(o => o.ID).OfType<long>().ToArray()
                        });
                //Parallel.For
                var audio =
                    svc.Select(new AudioResourceQuery()
                        {
                            IDs =
                                resourcereplyitems.Where(o => o.ResourceType == (int)ResourceType.Audio)
                                                  .Select(o => o.ResourceID)
                                                  .OfType<long>()
                                                  .ToArray()
                        });
                var video = svc.Select(new VideoResourceQuery()
                        {
                            IDs =
                                resourcereplyitems.Where(o => o.ResourceType == (int)ResourceType.Video)
                                                  .Select(o => o.ResourceID)
                                                  .OfType<long>()
                                                  .ToArray()
                        });
                var picture = svc.Select(new PictureResourceQuery()
                    {
                        IDs =
                            resourcereplyitems.Where(o => o.ResourceType == (int)ResourceType.Picture)
                                              .Select(o => o.ResourceID)
                                              .OfType<long>()
                                              .ToArray()
                    });
                var ftp = ConfigurationManager.AppSettings["ftp"];
                return rules.Select(o =>
                    {
                        var reply = replies.FirstOrDefault(p => p.RuleID == o.ID);
                        var res = resourcereplyitems.Where(r => r.RuleID == o.ID).Select(p => new ReplyResourceItemVM()
                            {
                                ID = p.ID,
                                OwnerID = p.OwnerID,
                                ReplyID = p.ReplyID,
                                ResourceID = p.ResourceID,
                                RuleID = p.RuleID,
                                ResourceType = p.ResourceType,
                            }).ToArray();
                        foreach (var r in res)
                        {
                            switch (r.ResourceType)
                            {
                                case (int)ResourceType.Audio:
                                    var r1 = audio.FirstOrDefault(p => p.ID == r.ResourceID);
                                    r.Path = Path.Combine(ftp, r1.Path, r1.Name);
                                    break;
                                case (int)ResourceType.News:

                                    break;
                                case (int)ResourceType.Picture:
                                    var r2 = picture.FirstOrDefault(p => p.ID == r.ResourceID);
                                    r.Path = Path.Combine(ftp, r2.Path, r2.Name);
                                    break;
                                case (int)ResourceType.Video:
                                    var r3 = video.FirstOrDefault(p => p.ID == r.ResourceID);
                                    r.Path = Path.Combine(ftp, r3.Path, r3.Name);
                                    break;

                            }
                        }
                        var ret = new KeywordReplyVM()
                            {
                                ID = o.ID,
                                Name = o.Name,
                                Keywords = keywords.Where(k => k.RuleID == o.ID).ToArray(),
                                TextReply = textreplies.Where(t => t.RuleID == o.ID).ToArray(),
                                ResourceItems = res,
                                ReplyAll = reply.ReplyAll,
                                ReplyID = reply.ID
                            };
                        return ret;
                    }).ToArray();
            }
            return Enumerable.Empty<KeywordReplyVM>();
        }
        public ActionResult Index()
        {
            var obj = QueryAll();
            var jsonStr = ServiceLocator.Resolve<ISerializer>("Json").Serialize(obj);
            ViewData["Json"] = jsonStr;
            return View();
        }


        public ActionResult GetResource(int type, int page)
        {
            var svc = Service;
            if (!Enum.IsDefined(typeof(ResourceType), type))
            {
                return Json(new { success = false, msg = "" }, JsonRequestBehavior.AllowGet);
            }
            int perPage = 10;
            if (page <= 0)
            {
                page = 1;
            }
            var skip = (page - 1) * perPage;
            var take = 10;
            long count = 0;
            ViewData["Type"] = type;
            IEnumerable<Resource> resources = null;
            string ftp;
            switch (type)
            {
                case (int)ResourceType.Audio:
                    var q = new AudioResourceQuery() { OwnerID = OwnerID, Take = take, Skip = skip };
                    resources = svc.Select(q);
                    count = q.Count.Value;
                    ftp = ConfigurationManager.AppSettings["ftp"];
                    foreach (var r in resources)
                    {
                        var t = r as FileResource;
                        t.Path = Path.Combine(ftp, t.Path, t.Name);
                    }
                    break;
                case (int)ResourceType.News:
                    var q2 = new NewsResourceQuery() { OwnerID = OwnerID, Take = take, Skip = skip };
                    resources = svc.Select(q2);
                    count = q2.Count.Value;

                    break;
                case (int)ResourceType.Picture:
                    var q3 = new PictureResourceQuery() { OwnerID = OwnerID, Take = take, Skip = skip };
                    resources = svc.Select(q3);
                    ftp = ConfigurationManager.AppSettings["ftp"];
                    foreach (var r in resources)
                    {
                        var t = r as FileResource;
                        t.Path = Path.Combine(ftp, t.Path, t.Name);
                    }
                    count = q3.Count.Value;
                    break;
                case (int)ResourceType.Video:
                    var q4 = new VideoResourceQuery() { OwnerID = OwnerID, Take = take, Skip = skip };
                    resources = svc.Select(q4);
                    ftp = ConfigurationManager.AppSettings["ftp"];
                    foreach (var r in resources)
                    {
                        var t = r as FileResource;
                        t.Path = Path.Combine(ftp, t.Path, t.Name);
                    }
                    count = q4.Count.Value;
                    break;
            }
            return Json(new { page = page, rows = resources, total = count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Modify(KeywordReplyVM item)
        {
            if (item == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            var svc = Service;
            if (!(item.ID > 0))
            {
                //创建
                try
                {
                    var rule = new Rule()
                        {
                            Name = item.Name,
                            OwnerID = OwnerID,
                            Type = (int)RuleType.Normal,
                            KeyWords = item.Keywords
                        };
                    svc.Create(rule);
                    svc.Create(new Reply()
                        {
                            ReplyAll = item.ReplyAll,
                            OwnerID = OwnerID,
                            RuleID = rule.ID,
                            TextReplyItems = item.TextReply,
                            ResourceItems = (item.ResourceItems != null) ? item.ResourceItems.Select(o => (ReplyResourceItem)o).ToList() : null
                        });
                }
                catch (Exception error)
                {
                    return Json(new { success = false, msg = error.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //更新
                try
                {
                    var rule = new Rule()
                        {
                            Name = item.Name,
                            ID = item.ID
                        };
                    svc.Update(rule);
                    if (item.Keywords != null)
                    {
                        var keywordToUpdate = item.Keywords.Where(o => o.ID > 0).ToArray();
                        if (keywordToUpdate.Any())
                        {
                            svc.Update(keywordToUpdate);
                        }
                        var keywordToCreate = item.Keywords.Where(o => !(o.ID > 0)).ToArray();
                        if (keywordToCreate.Any())
                        {
                            foreach (var k in keywordToCreate)
                            {
                                k.OwnerID = OwnerID;
                                k.RuleID = rule.ID;
                            }
                            svc.Create(keywordToCreate);
                        }
                    }
                    if (item.DeletedKeywords != null)
                    {
                        var keywordToDelete = item.DeletedKeywords.Select(o => new KeyWord() { ID = o }).ToArray();
                        if (keywordToDelete.Any())
                        {
                            svc.Delete(keywordToDelete);
                        }
                    }
                    var reply = new Reply()
                        {
                            ID = item.ReplyID,
                            ReplyAll = item.ReplyAll
                        };
                    svc.Update(reply);
                    if (item.TextReply != null)
                    {
                        var toUpdate = item.TextReply.Where(o => o.ID > 0).ToArray();
                        if (toUpdate.Any())
                        {
                            svc.Update(toUpdate);
                        }
                        var toCreate = item.TextReply.Where(o => !(o.ID > 0)).ToArray();
                        if (toCreate.Any())
                        {
                            foreach (var t in toCreate)
                            {
                                t.RuleID = item.ID;
                                t.ParentID = item.ReplyID;
                            }
                            svc.Create(toCreate);
                        }
                    }
                    if (item.ResourceItems != null)
                    {
                        //current donot support update
                        //var toUpdate = item.ResourceItems.Where(o => o.ID > 0).ToArray();
                        //if (toUpdate.Any())
                        //{
                        //    svc.Update(toUpdate);
                        //}
                        var toCreate = item.ResourceItems.Where(o => !(o.ID > 0)).ToArray();
                        if (toCreate.Any())
                        {
                            foreach (var t in toCreate)
                            {
                                t.RuleID = item.ID;
                                t.ReplyID = item.ReplyID;
                            }
                            svc.Create<ReplyResourceItem>(
                                toCreate.Select(
                                    o =>
                                    new ReplyResourceItem()
                                        {
                                            RuleID = o.RuleID,
                                            ReplyID = o.ReplyID,
                                            OwnerID = o.OwnerID,
                                            ResourceID = o.ResourceID,
                                            ResourceType = o.ResourceType
                                        }).ToArray());
                        }
                    }
                    if (item.DeletedResourceItems != null)
                    {
                        var toDelete = item.DeletedResourceItems.Select(o => new ReplyResourceItem() { ID = o }).ToArray();
                        if (toDelete.Any())
                        {
                            svc.Delete(toDelete);
                        }

                    }
                    if (item.DeletedTextReplies != null)
                    {
                        var toDelete = item.DeletedTextReplies.Select(o => new TextReplyItem() { ID = o }).ToArray();
                        if (toDelete.Any())
                        {
                            svc.Delete(toDelete);
                        }
                    }
                }
                catch (Exception error)
                {
                    return Json(new { success = false, msg = error.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(long ruleId)
        {
            var svc = Service;
            try
            {
                svc.Delete(new Rule() { ID = ruleId });
            }
            catch (Exception error)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
