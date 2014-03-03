using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Services;
using DBC.WeChat.Models.Infrastructures;
using DBC.WeChat.Models.Sales;

namespace DBC.WeChat.UI.Store.Content.Gridpp.XmlData
{
    public partial class GetSaleOrderData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (Request["SaleOrderID"] == null)
            {
                return;
            }
            long id;
            long.TryParse(Request["SaleOrderID"].Trim(), out id);
            var svc = ServiceLocator.Resolve<IModelService>("Internal");
            var saleOrder = svc.SelectOrEmpty(new SaleOrderQuery() {IDs = new long[] {id}}).FirstOrDefault();
            var orderItems = svc.SelectOrEmpty(new SaleOrderItemQuery()
                {
                    OrderIDs = new long[] {saleOrder.ID.Value},
                    Includes = new string[]{"Specification"}
                });
            var deliverymethod =
                svc.SelectOrEmpty(new DeliveryMethodQuery() {IDs = new long[] {saleOrder.DeliveryMethodID.Value}})
                   .FirstOrDefault();
            var areas =
                svc.SelectOrEmpty(new AreaPathQuery()
                    {
                        AreaIDs = new long[] {saleOrder.AreaID.Value},
                        Includes = new string[] {"Path"}
                    }).Select(o => o.Path).OrderBy(o => o.Depth).ToArray();
            string state;
            switch (saleOrder.State)
            {
                case (int)OrderState.ToBePaid:state = "待付款";break;
                case (int)OrderState.ToBeShipped:state = "待发货";break;
                case (int)OrderState.ToBeReturn:state = "待退款";break;
                case (int)OrderState.Finished:state="已完成";break;
                case (int)OrderState.Canceled:
                    state = "已作废";break;
                case (int)OrderState.ToBeAccept:
                    state = "待收货";break;
                default:
                    state = "";
                    break;
            }
            var address = string.Join("", areas.Select(o => o.Name).ToArray()) + "  " + saleOrder.Address;
            var totalNumber = orderItems.Sum(o => o.Quantity);
            
            var xml = new StringBuilder();
            
            xml.Append("<report>");
            xml.Append("<xml>");
            foreach (var orderitem in orderItems)
            {
                xml.Append("<row>");
                xml.Append("<ProductName>").Append(orderitem.Specification.ProductName).Append("</ProductName>");
                xml.Append("<SpecName>").Append(orderitem.Specification.Name).Append("</SpecName>");
                xml.Append("<Quantity>").Append(orderitem.Quantity).Append("</Quantity>");
                xml.Append("<Price>").Append(orderitem.Price).Append("</Price>");
                xml.Append("<ProductTotal>").Append(orderitem.Price * orderitem.Quantity).Append("</ProductTotal>");
                xml.Append("</row>");
            }
            xml.Append("</xml>");
            //xml.Append(string.Format("<xml><row><Quantity>1</Quantity><SpecName>11</SpecName><ProductName>111</ProductName><Price>11</Price><ProductTotal>11</ProductTotal></row></xml>"));
            xml.Append(string.Format("<_grparam>" +
                                     "<code>{0}</code><CreatedAt>{1}</CreatedAt><State>{2}</State>" +
                                     "<Receiver>{3}</Receiver><Mobile>{4}</Mobile><Address>{5}</Address>" +
                                     "<DeliveryMethod>{6}</DeliveryMethod><DeliveryFee>{7}</DeliveryFee>" +
                                     "<TotalNumber>{8}</TotalNumber><Total>{9}</Total>" +
                                     "</_grparam>",
                                     saleOrder.Code, saleOrder.CreatedAt, state,
                                     saleOrder.Name, saleOrder.Mobile, address,
                                     deliverymethod.Name, saleOrder.DeliveryFee,
                                     totalNumber, saleOrder.Amount + saleOrder.DeliveryFee));
            xml.Append("</report>");
            Response.Write(xml.ToString());
        }
    }
}