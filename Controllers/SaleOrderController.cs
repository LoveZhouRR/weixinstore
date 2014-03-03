using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using DBC.Ors.Models;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Services;
using DBC.WeChat.Models;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Models.Sales;
using DBC.WeChat.Services.Components;
using DBC.WeChat.Services.Components.Picture;
using DBC.WeChat.Services.Sales.Components;
using DBC.WeChat.UI.Components;
using DBC.WeChat.UI.Mvc;
using DBC.WeChat.UI.Store.Models;
using DBC.WeChat.UI.Store.Properties;

namespace DBC.WeChat.UI.Store.Controllers
{
    public class SaleOrderController : BaseController
    {
        private int _pageSize = 15;
        private readonly string ftp = ConfigurationManager.AppSettings["ftp"];
        private readonly IPictureService _pictureService = ServiceLocator.Resolve<IPictureService>("Ftp");
        private PictureSize ListSize = ServiceLocator.Resolve<PictureSize>("ListSize");
        private ISaleOrderService saleOrderService = ServiceLocator.Resolve<ISaleOrderService>("SaleOrderService");

        //提示字符串
        private const string SaleOrderCannotEdit = "已作废订单不能编辑";


        public ActionResult Index()
        {
            var query = new SaleOrderQuery()
            {
                OwnerIDs = new long[] { GetOwnerID() },
                Take = _pageSize,
                OrderField = "CreatedAt",
                OrderDirection = OrderDirection.Desc,
            };
            var initList = Service.Select(query).ToArray();
            ViewData["Pagination"] = Pagination.FromQuery(query);
            List<SelectListItem> stateList = new List<SelectListItem>()
                {
                    new SelectListItem()
                        {
                            Value = "-1",
                            Text = "请选择",
                        }
                };
            stateList.AddRange(Enum.GetValues(typeof(OrderState)).Cast<OrderState>().Select(o => new SelectListItem()
            {
                Value = ((int)o).ToString(),
                Text = o.GetDescription(),
            }).ToList());
            ViewData["stateList"] = stateList;
            GetDeliveryMehtods(initList);
            return View(initList);
        }

        public ActionResult AjaxQuery(SaleOrderQuery query)
        {
            if (query == null)
                query = new SaleOrderQuery();

            Boolean hasQueryed = false;
            if (Request.Form["hasQuery"] != null)
            {
                hasQueryed = Convert.ToBoolean(Request.Form["hasQuery"]);
            }
            if (hasQueryed)
            {
                if (query.AmountRange.Left == null && query.AmountRange.Right == null)
                {
                    query.AmountRange = null;
                }
                if (query.CreatedAtRange.Right != null)
                {
                    query.CreatedAtRange.Right = query.CreatedAtRange.Right.Value.AddDays(1);
                }
                if (query.CreatedAtRange.Left == null && query.CreatedAtRange.Right == null)
                {
                    query.CreatedAtRange = null;
                }
                if (query.CodePattern != null)
                {
                    query.CodePattern = query.CodePattern.Trim();
                }
                if (query.NamePattern != null)
                {
                    query.NamePattern = query.NamePattern.Trim();
                }
                if (query.State == -1)
                {
                    query.State = null;
                }
            }
            else
            {
                query.AmountRange = null;
                query.CodePattern = null;
                query.NamePattern = null;
                query.CreatedAtRange = null;
                query.State = null;
            }
            query.States = FilterStates();
            query.OwnerIDs = new long[] { GetOwnerID() };
            query.Take = _pageSize;
            if (string.IsNullOrWhiteSpace(query.OrderField))
            {
                query.OrderField = "CreatedAt";
                query.OrderDirection = OrderDirection.Desc;
            }
            var saleorders = Service.Select(query).ToArray();
            var pro = Pagination.FromQuery(query);
            ViewData["Pagination"] = pro;
            GetDeliveryMehtods(saleorders);
            return PartialView("SaleOrder/OrderList", saleorders);
        }



        public ActionResult Edit(int id)
        {
            try
            {
                var order = Service.Select(new SaleOrderQuery()
                {
                    IDs = new long[] { id },
                    OwnerIDs = new long[] { GetOwnerID() }
                }).FirstOrDefault();
                if (order != null)
                {
                    var Items = Service.Select(new SaleOrderItemQuery()
                    {
                        OrderIDs = new long[] { id },
                        Includes = new string[] { "Specification" },
                    });
                    List<string> paths = new List<string>();
                    foreach (var saleOrderItem in Items)
                    {
                        var pic = Service.SelectOrEmpty(new ProductPictureQuery()
                        {
                            OwnerID = GetOwnerID(),
                            ProductIDs = new long[]{saleOrderItem.Specification.ProductID.Value},
                            IsFirst = true,
                        }).FirstOrDefault();
                        if (pic != null)
                        {
                            paths.Add(System.IO.Path.Combine(ftp, pic.Path, _pictureService.GetName(pic, ListSize)));
                        }
                        else
                        {
                            paths.Add(Resources.DefaultListPicPath);
                        }
                    }
                    ViewData["paths"] = paths;
                    order.Items = Items.ToArray();
                    GetDeliveryMehtods(order);
                    ViewBag.DeliveryMethods = GetDeliveryMethods();
                }
                return View(order);
            }
            catch (Exception e)
            {
                return View("Error", new ErrorView() { message = e.Message });
            }
        }

        [HttpPost]
        public ActionResult Edit(SaleOrder saleOrder)
        {
            if (saleOrder.State != null && saleOrder.State.Value == (int)OrderState.Canceled)
                return View("Error", new ErrorView() { message = SaleOrderCannotEdit });
            try
            {
                ViewBag.DeliveryMethods = GetDeliveryMethods();
                Service.Update(saleOrder);

                //加载订单商品信息
                //*********************************************************//
                var Items = Service.Select(new SaleOrderItemQuery()
                {
                    OrderIDs = new long[] { saleOrder.ID ?? 0 },
                    Includes = new string[] { "Specification" },
                });
                var picList = Service.Select(new ProductPictureQuery()
                {
                    ProductIDs = Items.Select(o => o.Specification.ProductID.Value).ToArray(),
                    IsFirst = true,
                });
                List<string> paths = new List<string>();
                foreach (var saleOrderItem in Items)
                {
                    var pic = Service.SelectOrEmpty(new ProductPictureQuery()
                    {
                        OwnerID = GetOwnerID(),
                        ProductIDs = new long[] { saleOrderItem.Specification.ProductID.Value },
                        IsFirst = true,
                    }).FirstOrDefault();
                    if (pic != null)
                    {
                        paths.Add(System.IO.Path.Combine(ftp, pic.Path, _pictureService.GetName(pic, ListSize)));
                    }
                    else
                    {
                        paths.Add(Resources.DefaultListPicPath);
                    }
                }
                ViewData["paths"] = paths;
                saleOrder.Items = Items.ToArray();
                //*********************************************************//

                return View(saleOrder);
            }
            catch (Exception e)
            {
                return View("Error", new ErrorView() { message = e.Message });
            }
        }

        [HttpPost]
        public JsonResult ChangeState(int originstate, int state, bool isCOD)
        {
            var response = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
            try
            {
                //订单状态验证
                if (CheckState(originstate, state, isCOD))
                {
                    var requestIDs = Request.Form.GetValues("ids[]");
                    if (requestIDs != null)
                    {
                        long[] ids = requestIDs.Select(p => Convert.ToInt64(p)).ToArray();
                        var orders = ids.Select(o => new SaleOrder() { ID = o, State = state }).ToArray();
                        if (state == (int)OrderState.Canceled)
                        {
                            foreach (var id in ids)
                            {
                                saleOrderService.Cancel(id);
                            }
                        }
                        else
                        {
                            Service.Update(orders);
                        }
                        response.Data = new
                        {
                            success = true,
                            IDs = ids,
                            StateView = ((OrderState)state).GetDescription(),
                            message = "操作成功"
                        };
                    }

                }
                else
                {
                    response.Data = new { success = false, message = "操作无效" };
                }
                return response;
            }
            catch (Exception e)
            {
                response.Data = new { success = false, message = e.Message };
                return response;
            }
        }


        private bool CheckState(int originstate, int state, bool isCOD)
        {
            bool canChange = false;
            switch (originstate)
            {
                case (int)OrderState.ToBePaid:
                case (int)OrderState.ToBeShipped:
                    canChange = state == (int)OrderState.ToBeAccept || state == (int)OrderState.Canceled;
                    break;
                case (int)OrderState.ToBeReturn:
                    canChange = state == (int)OrderState.Finished;
                    break;
                case (int)OrderState.ToBeAccept:
                    canChange = state == (int)OrderState.Canceled && isCOD;
                    break;
                case (int)OrderState.Finished:
                case (int)OrderState.Canceled:
                    canChange = false;
                    break;
                default:
                    canChange = false;
                    break;
            }

            return canChange;
        }

        private long GetOwnerID()
        {
            return OwnerID;
        }


        private int[] FilterStates()
        {
            var isShowFinished = Convert.ToBoolean(Request.Form["isShowFinished"]);
            if (isShowFinished)
            {
                return new[]
                {
                    (int) OrderState.ToBeAccept,
                    (int) OrderState.ToBePaid,
                    (int) OrderState.ToBeReturn,
                    (int) OrderState.ToBeShipped
                };
            }
            else
            {
                return null;
            }
        }

        private void GetDeliveryMehtods(params SaleOrder[] orders)
        {
            if (!orders.Any())
                return;
            var deliveryMethods = Service.SelectOrEmpty(new DeliveryMethodQuery()
            {
                OwnerIDs = new long[] { OwnerID },
                IDs = orders.Select(o => o.DeliveryMethodID).OfType<long>().ToArray(),
            }).ToArray();
            if (!deliveryMethods.Any())
                return;
            foreach (SaleOrder item in orders)
            {
                item.DeliveryMethod = deliveryMethods.FirstOrDefault(o => o.ID == item.DeliveryMethodID);
            }
        }

        private IEnumerable<DeliveryMethod> GetDeliveryMethods()
        {
            var response = Service.Select(new DeliveryMethodQuery()
            {
                OwnerIDs = new long[] { OwnerID },
                Enabled = true
            });
            return response;
        }
    }
}
