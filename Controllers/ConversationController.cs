using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBC.WeChat.Models.Conversation;
using DBC.Ors.Services;
using DBC.WeChat.UI.Mvc;
using System.Configuration;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class ConversationController : BaseController
    {
        private readonly IModelService svc = ServiceLocator.Resolve<IModelService>("Internal");

        public ActionResult Index()
        {
            var list = svc.Select(new RuleQuery() { OwnerID = 1, Includes = new string[] { "KeyWords" } });
            return View(list);
        }

        [HttpPost]
        public ActionResult AddRule(Rule rule)
        {
            svc.Create(rule);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditRule(Rule rule)
        {
            svc.Update(rule);
            return PartialView("Conversation/RuleEdit", rule);
        }

        public ActionResult GetReply()
        {
            if (Request.Form["RuleID"] != null)
            {
                var Textreplys = new List<TextReplyItem>();
                long ruleID = Convert.ToInt64(Request.Form["RuleID"]);
                var reply = svc.Select(new ReplyQuery()
                    {
                        OwnerID = 1,
                        RuleID = ruleID,
                        Type = (int)ReplyType.Text,
                    });
                if (reply.Any())
                {
                    var x = reply.Select(o => o.ID).OfType<long>().ToArray();
                    Textreplys = svc.Select(new TextReplyItemQuery()
                        {
                            OwnerID = 1,
                            RuleID = ruleID,
                            ParentIDs = reply.Select(o => o.ID).OfType<long>().ToArray()
                        }).ToList();
                }
                ViewBag.RuleID = ruleID;
                return PartialView("Conversation/TextReply", Textreplys);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult EditTextReply(TextReplyItem reply)
        {
            svc.Update(reply);
            return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { success = true }
                };
        }

        [HttpPost]
        public ActionResult AddTextReply(TextReplyItem textreply)
        {
            svc.Create(textreply);
            ViewBag.RuleID = textreply.RuleID;
            var Textreplys = svc.Select(new TextReplyItemQuery() { OwnerID = 1, RuleID = textreply.RuleID });
            return PartialView("Conversation/TextReply", Textreplys);
        }

        [HttpPost]
        public JsonResult DeleteTextReply(TextReplyItem reply)
        {
            svc.Delete(reply);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = true }
            };
        }

        /// <summary>
        /// 被关注自动回复
        /// 不存在，则补充完备数据，保证进入页面时，数据是完备的
        /// </summary>
        /// <returns></returns>
        public ActionResult FollowedIndex()
        {
            //获取KeyWordGroup信息
            var _KeyWordGroup = svc.Select(
                new RuleQuery()
                {
                    OwnerID = OwnerID,
                    Type = (int)RuleType.Subscribe
                }).FirstOrDefault();

            // 不存在，则写入新纪录
            if (_KeyWordGroup == null)
            {
                Rule _Model = new Rule();
                _Model.Name = "被关注自动回复";
                _Model.OwnerID = OwnerID;
                _Model.Type = (int)RuleType.Subscribe;

                //插入KeyWordGroup
                svc.Create(_Model);

                //获取新插入的KeyWordGroup记录
                _KeyWordGroup = _Model;
            }

            //获取Reply信息
            var _Reply = svc.Select(
                new ReplyQuery()
                {
                    OwnerID = OwnerID,
                    Type = (int)ReplyType.Text,
                    RuleID = _KeyWordGroup.ID
                }).FirstOrDefault();

            //不存在，则写入一条新纪录
            if (_Reply == null)
            {
                Reply _ReplyModel = new Reply();
                _ReplyModel.OwnerID = OwnerID;
                _ReplyModel.Type = (int)ReplyType.Text;
                _ReplyModel.RuleID = _KeyWordGroup.ID;

                svc.Create(_ReplyModel);

                _Reply = _ReplyModel;
            }

            //获取TextReply信息
            var _TextReplyItem = svc.Select(
                new TextReplyItemQuery()
                {
                    RuleID = _KeyWordGroup.ID,
                    ParentID = _Reply.ID,
                    OwnerID = OwnerID
                }).FirstOrDefault();

            //不存在，则写入新纪录
            if (_TextReplyItem == null)
            {
                TextReplyItem _TextReplyItemModel = new TextReplyItem();
                _TextReplyItemModel.RuleID = _KeyWordGroup.ID;
                _TextReplyItemModel.ParentID = _Reply.ID;
                _TextReplyItemModel.OwnerID = OwnerID;
                _TextReplyItemModel.Content = "";

                svc.Create(_TextReplyItemModel);

                _TextReplyItem = _TextReplyItemModel;
            }

            return View(_TextReplyItem);
        }

        /// <summary>
        /// 更新被关注自动回复
        /// </summary>
        /// <param name="content">自动回复内容</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public JsonResult UpdateFollowedReply(long id, string content)
        {
            JsonResult result = new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            try
            {
                var _Model = svc.Select(
                    new TextReplyItemQuery()
                    {
                        ID = id
                    }).FirstOrDefault();

                _Model.Content = content;

                svc.Update(_Model);

                result.Data = new
                {
                    Success = true
                };

                return result;
            }
            catch (Exception e)
            {
                result.Data = new
                {
                    Success = false,
                    Message = e.Message
                };

                return result;
            }
        }
    }
}
