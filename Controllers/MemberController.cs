using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Models.Sales;
using DBC.Ors.Models.Infrastructures.MemberShip;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Services;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Excetion;
using DBC.WeChat.Models.Sales;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Services.Conversation.Models;
using DBC.WeChat.Services.Components;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class MemberController : BaseController
    {
        //
        // GET: /Member/

        /// <summary>
        /// 会员信息列表页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //对接主系统，获取客户
            var svc = Service;
            var query = new FanQuery()
            {
                OwnerIDs = new long[] { OwnerID },
                Take = PageSize,
                OrderField = "ID",
                OrderDirection = OrderDirection.Asc
            };
            var WeChatMember = svc.SelectOrEmpty(query);
            ICollection<MemberMasterVM> MasterModel = new Collection<MemberMasterVM>();
            ViewData["Pagination"] = Pagination.FromQuery(query);

            if (WeChatMember != null && WeChatMember.Any())
            {
                MasterModel = AttachMemberWebInfo(WeChatMember);
                return View(MasterModel);
            }

            return View(MasterModel);
        }

        /// <summary>
        /// 根据微信端客户信息获取绑定的主系统信息（如果已绑定）
        /// </summary>
        /// <param name="FanList">微信端客户列表</param>
        /// <returns>会员完整信息</returns>
        private ICollection<MemberMasterVM> AttachMemberWebInfo(IEnumerable<Fan> FanList)
        {
            Collection<MemberMasterVM> VM = new Collection<MemberMasterVM>();
            var OSvc = ORSService;
            var svc = Service;

            if (FanList != null && FanList.Any())
            {
                #region 微信端订单数量信息

                var idSets = FanList.Select(c => c.ID ?? 0);
                long[] fanids = new long[idSets.Count()];
                for (int i = 0; i < fanids.Length; i++)
                {
                    fanids[i] = idSets.ToList()[i];
                }

                //获取微信端客户订单情况
                var weChatOrderQuery = new SaleOrderQuery
                {
                    FanIDs = fanids,
                    State = (int)OrderState.Finished
                };
                var weChatOrder = svc.SelectOrEmpty(weChatOrderQuery);

                #endregion

                #region 获取微信端客户绑定主系统情况

                var relateQuery = new MemberRelatedQuery
                {
                    FanIDs = fanids
                };

                //memberSet 微信端和主系统绑定记录 id-fanid-customerid
                var memberSet = svc.SelectOrEmpty(relateQuery).OrderBy(c => c.FanID);

                #endregion

                #region 绑定信息不为空，则获取对应绑定的主系统上的基本信息

                if (memberSet != null && memberSet.Any())
                {
                    var memberids = memberSet.Select(c => c.CustomerID ?? 0).Distinct().OfType<long>().ToArray();

                    var CIQuery = new CustomerIdentityQuery
                    {
                        CustomerIDs = memberids
                    };
                    //CustomerIdentity表 记录不同的登陆Code
                    var CIdentity = OSvc.SelectOrEmpty(CIQuery);//.OrderBy(c => c.CustomerID);

                    //主系统订单情况
                    var WebOrderQuery = new OrderQuery
                    {
                        CustomerIDs = memberids
                    };
                    var webOrder = OSvc.SelectOrEmpty(WebOrderQuery);

                    //主系统会员积分信息
                    //CustomerBalance表和Customer表一一对应
                    //建立在这个前提下，下文直接以关联下标代替balance下标信息
                    var balanceQuery = new CustomerBalanceQuery
                    {
                        IDs = memberids
                    };
                    var balanceSets = OSvc.SelectOrEmpty(balanceQuery);

                    MemberMasterVM item = new MemberMasterVM();
                    int webCount = 0;//线上登陆方式遍历计数
                    int relatedCount = memberSet.Count();
                    long _FanId = -1;
                    long _CustomerId = -1;

                    if (CIdentity != null && CIdentity.Any())
                        webCount = CIdentity.Count();
                    for (int i = 0; i < FanList.Count(); i++)
                    {
                        item = new MemberMasterVM();
                        item.CustomerCode = new Collection<string>();
                        //微信端基本信息
                        item.WeChatMember = FanList.ToList()[i];
                        _FanId = item.WeChatMember.ID ?? -1;

                        var relatedItem = memberSet.Where(c => c.FanID == _FanId).FirstOrDefault();
                        _CustomerId = -1;
                        if (relatedItem != null)
                        {
                            _CustomerId = relatedItem.CustomerID ?? -1;
                        }

                        //微信端订单数量信息
                        item.WeChatOrderCount = GetWechatOrderCount(weChatOrder, _FanId);

                        //主系统订单数量信息
                        item.WebOrderCount = GetWebOrderCount(webOrder, _CustomerId);

                        //补齐系统编号到八位，显示用
                        item.DisplayID = string.Format("{0:D8}", item.WeChatMember.ID ?? 0);

                        //主系统联名登陆信息的获取----一并获取积分信息
                        item.CustomerCode = GetIdentities(CIdentity, _CustomerId);

                        //积分信息
                        item.WebCreadit = GetCredits(balanceSets, _CustomerId);

                        VM.Add(item);
                    }
                }

                #endregion

                #region 绑定信息为空，则直接将微信端用户信息绑定返回

                else
                {
                    MemberMasterVM item = new MemberMasterVM();
                    for (int i = 0; i < FanList.Count(); i++)
                    {
                        item = new MemberMasterVM();
                        //微信端用户基本信息
                        item.WeChatMember = FanList.ToList()[i];

                        //微信端订单数量信息
                        item.WeChatOrderCount = GetWechatOrderCount(weChatOrder, item.WeChatMember.ID ?? -1);

                        //补齐系统编号到八位，显示用
                        item.DisplayID = string.Format("{0:D8}", item.WeChatMember.ID ?? 0);

                        //补齐线上默认信息
                        item.CustomerCode = new Collection<string>();
                        item.CustomerCode.Add("--");
                        item.WebCreadit = 0;
                        item.WebOrderCount = 0;

                        VM.Add(item);
                    }
                }

                #endregion

                return VM;
            }
            else
                return null;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        public ActionResult AjaxQuery(FanQuery FQuery)
        {
            //对接主系统，获取客户
            var svc = Service;
            var query = new FanQuery()
            {
                OwnerIDs = new long[] { OwnerID },
                Take = PageSize,
                Skip = FQuery.Skip,
                OrderField = "ID",
                OrderDirection = OrderDirection.Asc
            };
            var WeChatMember = svc.SelectOrEmpty(query);
            if (WeChatMember != null && WeChatMember.Any())
            {
                ICollection<MemberMasterVM> MasterModel = AttachMemberWebInfo(WeChatMember);

                ViewData["Pagination"] = Pagination.FromQuery(query);
                return View("Member/MemberList", MasterModel);
            }

            return View("Member/MemberList", null);
        }

        //public ActionResult UnBind

        /// <summary>
        /// 会员个人信息详情页
        /// <param name="id">会员微信系统编号</param>
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberDetail(long id)
        {
            var svc = Service;
            var OSvc = ORSService;
            var CSvc = CONService;
            var Member = svc.SelectOrEmpty(new FanQuery() { IDs = new long[] { id } }).FirstOrDefault();
            MemberPersonalVM PersonalVM = new MemberPersonalVM();

            #region 微信端用户基本信息

            if (Member != null)
            {
                UserInfo UInfo = CSvc.RequeUserInfo(OwnerID, Member.Code);
                //微信系统用户基本信息
                PersonalVM.WeChatMember = Member;

                PersonalVM.WeChatExtra = new WeChatPersonalExtra();

                PersonalVM.WeChatExtra.City = UInfo.city;
                PersonalVM.WeChatExtra.Province = UInfo.province;
                PersonalVM.WeChatExtra.Country = UInfo.country;
                PersonalVM.WeChatExtra.Gender = UInfo.sex;
                PersonalVM.WeChatExtra.GenderName = ((MemberGender)UInfo.sex).GetDescription();
                PersonalVM.WeChatExtra.NickName = UInfo.nickname;
                PersonalVM.WeChatExtra.FollowDate = FollowTime(UInfo.subscribe_time);
            }

            #endregion

            #region 主系统关联信息

            var relatedQuery = new MemberRelatedQuery { FanIDs = new long[] { id } };
            var relatedRecord = svc.SelectOrEmpty(relatedQuery).FirstOrDefault();
            if (relatedRecord != null)
            {
                var webQuery = new CustomerQuery
                {
                    IDs = new long[] { relatedRecord.CustomerID ?? -1 }
                };
                var webMember = OSvc.SelectOrEmpty(webQuery).FirstOrDefault();
                if (webMember != null)
                {
                    PersonalVM.WebExtra = new WebPersonalExtra();

                    PersonalVM.WebExtra.CustomerMobile = webMember.Mobile;
                    PersonalVM.WebExtra.CustomerEmail = webMember.Email;

                    //主系统会员等级信息
                    var rankQuery = new CustomerRankQuery
                    {
                        IDs = new long[] { webMember.RankID ?? -1 }
                    };
                    var rankItem = OSvc.SelectOrEmpty(rankQuery).FirstOrDefault();
                    if (rankItem != null)
                        PersonalVM.WebExtra.CustomerRank = rankItem.Name;

                    //主系统会员积分信息
                    var balanceQuery = new CustomerBalanceQuery
                    {
                        IDs = new long[] { webMember.ID ?? -1 }
                    };
                    var balanceItem = OSvc.SelectOrEmpty(balanceQuery).FirstOrDefault();
                    if (balanceItem != null)
                        PersonalVM.WebExtra.CustomerCredit = balanceItem.Point;

                    //主系统会员登陆名信息
                    var identityQuery = new CustomerIdentityQuery
                    {
                        CustomerIDs = new long[] { webMember.ID ?? -1 }
                    };
                    var identityList = OSvc.SelectOrEmpty(identityQuery);
                    if (identityList != null && identityList.Any())
                    {
                        PersonalVM.WebExtra.CustomerId = new List<string>();
                        foreach (var identityItem in identityList)
                        {
                            PersonalVM.WebExtra.CustomerId.Add(identityItem.Code);
                        }
                    }
                }

                return View(PersonalVM);
            }

            #endregion

            return View(PersonalVM);
        }

        /// <summary>
        /// 会员订单详情页
        /// <param name="id">会员微信系统编号</param>
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberOrder(long id)
        {
            var svc = Service;
            var OSvc = ORSService;

            Collection<MemberSaleOrderVM> MasterModel = new Collection<MemberSaleOrderVM>();

            #region 微信端订单

            var WeChatOrderQuery = new SaleOrderQuery
            {
                FanIDs = new long[] { id },
                OrderField = "CreatedAt",
                OrderDirection = OrderDirection.Asc,
                Take = PageSize
            };
            var WeChatOrderList = svc.SelectOrEmpty(WeChatOrderQuery);
            if (WeChatOrderList != null && WeChatOrderList.Any())
            {
                MemberSaleOrderVM ItemModel = new MemberSaleOrderVM();
                var DMetholds = GetDeliveryMethods();
                foreach (var item in WeChatOrderList)
                {
                    ItemModel = new MemberSaleOrderVM();
                    ItemModel.Code = item.Code;
                    ItemModel.Amount = item.Amount;
                    ItemModel.CreateTime = item.CreatedAt;
                    ItemModel.DeliveryFee = item.DeliveryFee;
                    ItemModel.DeliveryCode = item.DeliveryCode;
                    ItemModel.DeliveryMethodID = item.DeliveryMethodID ?? -1;
                    foreach (var DItem in DMetholds)
                    {
                        if (DItem.ID == ItemModel.DeliveryMethodID)
                        {
                            ItemModel.DeliveryMethodName = DItem.Name;
                            break;
                        }
                    }
                    ItemModel.Status = item.State ?? -1;
                    ItemModel.StatusName = ((OrderState)ItemModel.Status).GetDescription();
                    ItemModel.OrderSource = "微信商城";

                    MasterModel.Add(ItemModel);
                }
            }

            #endregion

            #region 线上商城订单
            ////微信端订单数目不足以显示一页时，从线上商城拉取数据
            //if (MasterModel.Count() < PageSize)
            //{

            //    //需从线上商城拉取的数据条数
            //    int _Take = PageSize - MasterModel.Count();

            //    var relatedQuery = new MemberRelatedQuery
            //    {
            //        FanIDs = new long[] { id }
            //    };
            //    var relatedMember = svc.SelectOrEmpty(relatedQuery).FirstOrDefault();
            //    //如果有关联，则获取关联信息
            //    if (relatedMember != null)
            //    {
            //        //临时记录线上商城订单信息
            //        //线上商城订单信息需要拼接，故而需要临时信息
            //        Collection<MemberSaleOrderVM> WebMasterModel = new Collection<MemberSaleOrderVM>();

            //        var WebOrderQuery = new OrderQuery
            //        {
            //            CustomerIDs = new long[] { relatedMember.CusomerID ?? -1 },
            //            Take = _Take,
            //            OrderField = "CreatedAt",
            //            OrderDirection = OrderDirection.Asc
            //        };
            //        var WebOrderList = OSvc.SelectOrEmpty(WebOrderQuery);
            //        if (WebOrderList != null && WebOrderList.Any())
            //        {
            //            MemberSaleOrderVM ItemModel = new MemberSaleOrderVM();
            //            int idCount = WebOrderList.Count();
            //            long[] ids = new long[idCount];
            //            int idIndex = 0;
            //            foreach (var item in WebOrderList)
            //            {
            //                ItemModel = new MemberSaleOrderVM();
            //                ItemModel.Code = item.Code;
            //                ItemModel.CreateTime = item.CreatedAt;
            //                ItemModel.OrderSource = "在线商城";
            //                ItemModel.OrderId = item.ID;
            //                ids[idIndex++] = ItemModel.OrderId ?? -1;

            //                WebMasterModel.Add(ItemModel);
            //            }

            //            var WebFundQuery = new OrderFundQuery
            //            {
            //                IDs = ids
            //            };
            //            var WebFundList = OSvc.SelectOrEmpty(WebFundQuery);
            //            if (WebFundList != null)
            //            {
            //                for (int i = 0; i < WebMasterModel.Count(); i++)
            //                {
            //                    for (int j = 0; j < WebFundList.Count(); j++)
            //                    {
            //                        if (WebMasterModel[i].OrderId == WebFundList.ToList()[j].ID)
            //                        {
            //                            WebMasterModel[i].Amount = WebFundList.ToList()[j].PayableAmount;
            //                            WebMasterModel[i].DeliveryFee = WebFundList.ToList()[j].DeliveryFee;
            //                        }
            //                    }

            //                    MasterModel.Add(WebMasterModel[i]);
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion


            ViewData["Pagination"] = Pagination.FromQuery(WeChatOrderQuery);
            ViewData["CId"] = id;

            return View(MasterModel);
        }

        /// <summary>
        /// 定制化查询
        /// 类型：微信，线上
        /// 分页情况
        /// </summary>
        /// <param name="MOQuery"></param>
        /// <returns></returns>
        public ActionResult AjaxMemberOrder(MemberSaleOrderVMQuery MOQuery)
        {
            var svc = Service;
            var OSvc = ORSService;

            Collection<MemberSaleOrderVM> MasterModel = new Collection<MemberSaleOrderVM>();
            long id = -1;
            string OrderSource = "wechat";
            //获取查看用户编号
            if (Request.Form["FanId"] != null)
            {
                if (!long.TryParse(Request.Form["FanId"].ToString(), out id))
                    id = -1;
            }
            //获取订单来源
            //wechat--web
            if (Request.Form["OrderSource"] != null)
            {
                OrderSource = Request.Form["OrderSource"].ToString();
            }

            #region 微信端订单
            if (OrderSource == "wechat")
            {
                var WeChatOrderQuery = new SaleOrderQuery
                {
                    FanIDs = new long[] { id },
                    Take = PageSize,
                    Skip = MOQuery.Skip,
                    OrderField = "CreatedAt",
                    OrderDirection = OrderDirection.Asc
                };
                var WeChatOrderList = svc.SelectOrEmpty(WeChatOrderQuery);
                if (WeChatOrderList != null && WeChatOrderList.Any())
                {
                    MemberSaleOrderVM ItemModel = new MemberSaleOrderVM();
                    var DMetholds = GetDeliveryMethods();
                    foreach (var item in WeChatOrderList)
                    {
                        ItemModel = new MemberSaleOrderVM();
                        ItemModel.Code = item.Code;
                        ItemModel.Amount = item.Amount;
                        ItemModel.CreateTime = item.CreatedAt;
                        ItemModel.DeliveryFee = item.DeliveryFee;
                        ItemModel.DeliveryCode = item.DeliveryCode;
                        ItemModel.DeliveryMethodID = item.DeliveryMethodID ?? -1;
                        foreach (var DItem in DMetholds)
                        {
                            if (DItem.ID == ItemModel.DeliveryMethodID)
                            {
                                ItemModel.DeliveryMethodName = DItem.Name;
                                break;
                            }
                        }
                        ItemModel.Status = item.State ?? -1;
                        ItemModel.StatusName = ((OrderState)ItemModel.Status).GetDescription();
                        ItemModel.OrderSource = "微信商城";

                        MasterModel.Add(ItemModel);
                    }
                }

                ViewData["Pagination"] = Pagination.FromQuery(WeChatOrderQuery);
            }
            #endregion

            #region 线上商城订单

            else if (OrderSource == "web")
            {

                var relatedQuery = new MemberRelatedQuery
                {
                    FanIDs = new long[] { id }
                };
                var relatedMember = svc.SelectOrEmpty(relatedQuery).FirstOrDefault();
                //如果有关联，则获取关联信息
                if (relatedMember != null)
                {
                    //临时记录线上商城订单信息
                    //线上商城订单信息需要拼接，故而需要临时信息
                    Collection<MemberSaleOrderVM> WebMasterModel = new Collection<MemberSaleOrderVM>();

                    var WebOrderQuery = new OrderQuery
                    {
                        CustomerIDs = new long[] { relatedMember.CustomerID ?? -1 },
                        Take = PageSize,
                        Skip = MOQuery.Skip,
                        OrderField = "CreatedAt",
                        OrderDirection = OrderDirection.Asc
                    };
                    var WebOrderList = OSvc.SelectOrEmpty(WebOrderQuery);
                    if (WebOrderList != null && WebOrderList.Any())
                    {
                        MemberSaleOrderVM ItemModel = new MemberSaleOrderVM();
                        int idCount = WebOrderList.Count();
                        long[] ids = new long[idCount];
                        int idIndex = 0;
                        foreach (var item in WebOrderList)
                        {
                            ItemModel = new MemberSaleOrderVM();
                            ItemModel.Code = item.Code;
                            ItemModel.CreateTime = item.CreatedAt;
                            ItemModel.OrderSource = "在线商城";
                            ItemModel.OrderId = item.ID;
                            ids[idIndex++] = ItemModel.OrderId ?? -1;

                            WebMasterModel.Add(ItemModel);
                        }

                        var WebFundQuery = new OrderFundQuery
                        {
                            IDs = ids
                        };
                        var WebFundList = OSvc.SelectOrEmpty(WebFundQuery);
                        if (WebFundList != null)
                        {
                            for (int i = 0; i < WebMasterModel.Count(); i++)
                            {
                                for (int j = 0; j < WebFundList.Count(); j++)
                                {
                                    if (WebMasterModel[i].OrderId == WebFundList.ToList()[j].ID)
                                    {
                                        WebMasterModel[i].Amount = WebFundList.ToList()[j].PayableAmount;
                                        WebMasterModel[i].DeliveryFee = WebFundList.ToList()[j].DeliveryFee;
                                    }
                                }

                                MasterModel.Add(WebMasterModel[i]);
                            }
                        }
                    }

                    ViewData["Pagination"] = Pagination.FromQuery(WebOrderQuery);
                }
                else//保证在未绑定的情况下，页面的一致性
                {
                    var WebOrderQuery = new OrderQuery
                    {
                        CustomerIDs = new long[] { -1 },
                        Take = PageSize,
                    };

                    ViewData["Pagination"] = Pagination.FromQuery(WebOrderQuery);
                }
            }

            #endregion

            ViewData["CId"] = id;

            return View("Member/MemberOrderList", MasterModel);
        }

        /// <summary>
        /// 会员积分相关
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MemberCredit(long id)
        {
            var svc = Service;
            Collection<MemberCredit> MemberCreditVM = new Collection<MemberCredit>();

            //获取积分获取方式
            var creditTypeQuery = new FanCreditsTypeQuery
            {
                OrderField = "ID",
                OrderDirection = OrderDirection.Asc
            };
            var creditType = svc.SelectOrEmpty(creditTypeQuery);

            //获取积分明细
            var creditQuery = new FanCreditsQuery()
            {
                FanIds = new long[] { id },
                Take = PageSize,
                OrderField = "CreatedAt",
                OrderDirection = OrderDirection.Asc
            };
            var creditList = svc.SelectOrEmpty(creditQuery);

            if (creditList != null && creditList.Any())
            {
                FanCreditsType _type = new FanCreditsType();
                MemberCredit _Model = new MemberCredit();
                foreach (var item in creditList)
                {
                    _Model = new MemberCredit();
                    if (creditType != null && creditType.Any())
                    {
                        _type = creditType.Where(c => c.TypeCode == item.CreditTypeCode).FirstOrDefault();
                    }
                    _Model.RemainAmount = item.RemainAmount;
                    _Model.CreditAmount = item.CreditAmount;
                    _Model.CreditReason = (_type == null) ? "" : _type.TypeName;
                    _Model.CreateTime = item.CreatedAt;

                    MemberCreditVM.Add(_Model);
                }
            }

            ViewData["Pagination"] = Pagination.FromQuery(creditQuery);
            ViewData["CId"] = id;
            return View(MemberCreditVM);
        }

        /// <summary>
        /// 积分修改
        /// 翻页等
        /// </summary>
        /// <param name="FCQuery"></param>
        /// <returns></returns>
        public ActionResult AjaxMemberCredit(FanCreditsQuery FCQuery)
        {
            long id = -1;
            //获取查看用户编号
            if (Request.Form["FanId"] != null)
            {
                if (!long.TryParse(Request.Form["FanId"].ToString(), out id))
                    id = -1;
            }

            var svc = Service;

            if (Request.Form["modifyCredits"] != null)
            {
                int CreditAdd = 0;
                if (int.TryParse(Request.Form["modifyCredits"].ToString(), out CreditAdd))
                {
                    //增加手动修改积分记录
                    var addCreditQuery = new FanCreditsQuery
                    {
                        FanIds = new long[] { id },
                        OrderField = "ID",
                        OrderDirection = OrderDirection.Desc
                    };
                    var lastCredit = svc.SelectOrEmpty(addCreditQuery).FirstOrDefault();
                    FanCredits _creditForCreate = new FanCredits();
                    _creditForCreate.FanId = id;
                    _creditForCreate.CreditAmount = CreditAdd;
                    _creditForCreate.RemainAmount = (lastCredit == null) ? CreditAdd : CreditAdd + lastCredit.RemainAmount;
                    _creditForCreate.CreditTypeCode = 4;

                    svc.Create(_creditForCreate);

                    //更新Fans表中用户积分信息
                    var fanForUpdate = svc.SelectOrEmpty(new FanQuery() { IDs = new long[] { id } }).FirstOrDefault();
                    if (fanForUpdate != null)
                    {
                        fanForUpdate.Credits = _creditForCreate.RemainAmount;
                        svc.Update(fanForUpdate);
                    }
                }
            }

            Collection<MemberCredit> MemberCreditVM = new Collection<MemberCredit>();

            //获取积分获取方式
            var creditTypeQuery = new FanCreditsTypeQuery
            {
                OrderField = "ID",
                OrderDirection = OrderDirection.Asc
            };
            var creditType = svc.SelectOrEmpty(creditTypeQuery);

            //获取积分明细
            var creditQuery = new FanCreditsQuery()
            {
                FanIds = new long[] { id },
                Take = PageSize,
                Skip = FCQuery.Skip,
                OrderField = "CreatedAt",
                OrderDirection = OrderDirection.Asc
            };
            var creditList = svc.SelectOrEmpty(creditQuery);


            if (creditList != null && creditList.Any())
            {
                FanCreditsType _type = new FanCreditsType();
                MemberCredit _Model = new MemberCredit();
                foreach (var item in creditList)
                {
                    _Model = new MemberCredit();
                    if (creditType != null && creditType.Any())
                    {
                        _type = creditType.Where(c => c.TypeCode == item.CreditTypeCode).FirstOrDefault();
                    }
                    _Model.RemainAmount = item.RemainAmount;
                    _Model.CreditAmount = item.CreditAmount;
                    _Model.CreditReason = (_type == null) ? "" : _type.TypeName;
                    _Model.CreateTime = item.CreatedAt;

                    MemberCreditVM.Add(_Model);
                }
            }

            ViewData["Pagination"] = Pagination.FromQuery(creditQuery);
            ViewData["CId"] = id;
            return View("Member/MemberCreditList", MemberCreditVM);
        }

        /// <summary>
        /// 会员地址簿管理
        /// <param name="id">会员微信系统编号</param>
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberAddress(long id)
        {
            var svc = Service;
            List<WeChatContactVM> MemberAddressList = new List<WeChatContactVM>();
            var contactQuery = new FanContactQuery
            {
                FanID = id
            };
            var contactList = svc.SelectOrEmpty(contactQuery);
            if (contactList != null && contactList.Any())
            {
                WeChatContactVM MemberAddressItem = new WeChatContactVM();
                foreach (var item in contactList)
                {
                    MemberAddressItem = new WeChatContactVM();
                    MemberAddressItem.Address = item.Address;
                    MemberAddressItem.Mobile = item.Mobile;
                    MemberAddressItem.isDefault = item.IsDefault;
                    MemberAddressItem.AddressPrefix = GetAreaNameByAreaID(item.AreaID ?? -1);

                    MemberAddressList.Add(MemberAddressItem);
                }
            }

            return View(MemberAddressList);
        }

        /// <summary>
        /// 根据地区编号获取地区详细名称
        /// </summary>
        /// <param name="areaId">地区编号</param>
        /// <returns>给定地区编号对应的地区详细名称</returns>
        private string GetAreaNameByAreaID(long areaId = -1)
        {
            string AreaName = "";
            var svc = Service;
            var pathQuery = new AreaPathQuery
            {
                IDs = new long[] { areaId }
            };
            var paths = svc.SelectOrEmpty(pathQuery);
            if (paths != null && paths.Any())
            {
                var pathIdset = paths.Select(c => c.AreaID ?? -1);
                long[] pathIds = new long[pathIdset.Count()];
                for (int i = 0; i < pathIds.Length; i++)
                {
                    pathIds[i] = pathIdset.ToList()[i];
                }
                var _areaQuery = new AreaQuery
                {
                    IDs = pathIds
                };
                var areaList = svc.SelectOrEmpty(_areaQuery).OrderBy(c => c.Depth);
                if (areaList != null && areaList.Any())
                {
                    foreach (var item in areaList)
                    {
                        AreaName += item.Name;
                    }
                }
            }

            return AreaName;
        }

        /// <summary>
        /// 获取商户关联的物流公司信息
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DeliveryMethod> GetDeliveryMethods()
        {
            var response = Service.Select(new DeliveryMethodQuery()
            {
                OwnerIDs = new long[] { OwnerID },
                Enabled = true
            });
            return response;
        }

        /// <summary>
        /// 获取用户关注时间
        /// </summary>
        /// <param name="MyTimeSpan">时间差</param>
        /// <returns>关注时间</returns>
        private DateTime FollowTime(int MyTimeSpan)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            DateTime followTime = startTime.AddSeconds(MyTimeSpan);

            return followTime;
        }

        /// <summary>
        /// 获取会员线上绑定积分信息
        /// </summary>
        /// <returns></returns>
        private int GetCredits(IEnumerable<CustomerBalance> balanceSets, long customerId)
        {
            int credits = 0;

            if (balanceSets != null && balanceSets.Any() && customerId != -1)
            {
                foreach (var item in balanceSets)
                {
                    if (item.ID == customerId)
                    {
                        credits = item.Point ?? 0;
                        break;
                    }
                }
            }

            return credits;
        }

        /// <summary>
        /// 获取会员微信端订单数目信息
        /// </summary>
        /// <param name="weChatOrders">微信端订单集合信息</param>
        /// <param name="fanId">微信fanid</param>
        /// <returns>订单数目</returns>
        private int GetWechatOrderCount(IEnumerable<SaleOrder> weChatOrders, long fanId)
        {
            int count = 0;

            if (weChatOrders != null && weChatOrders.Any() && fanId != -1)
            {
                count = weChatOrders.Where(c => c.FanID == fanId).Count();
            }

            return count;
        }

        /// <summary>
        /// 获取会员线上订单数目信息
        /// </summary>
        /// <param name="webOrders">线上订单集合信息</param>
        /// <param name="customerId">线上会员编号信息</param>
        /// <returns>订单数目</returns>
        private int GetWebOrderCount(IEnumerable<Order> webOrders, long customerId)
        {
            int count = 0;

            if (webOrders != null && webOrders.Any() && customerId != -1)
            {
                count = webOrders.Where(c => c.CustomerID == customerId).Count();
            }

            return count;
        }

        /// <summary>
        /// 获取线上会员登陆信息
        /// </summary>
        /// <param name="IdentitySets">用户登陆方式集合信息</param>
        /// <param name="customerId">线上会员编号信息</param>
        /// <returns>用户登陆方式联名信息</returns>
        private ICollection<string> GetIdentities(IEnumerable<CustomerIdentity> IdentitySets, long customerId)
        {
            Collection<string> identityString = new Collection<string>();

            if (IdentitySets != null && IdentitySets.Any() && customerId != -1)
            {
                var cIdentity = IdentitySets.Where(c => c.CustomerID == customerId);
                if (cIdentity != null && cIdentity.Any())
                {
                    foreach (var item in cIdentity)
                    {
                        identityString.Add(item.Code);
                    }
                }
            }
            else
            {
                identityString.Add("--");
            }

            return identityString;
        }
    }
}
