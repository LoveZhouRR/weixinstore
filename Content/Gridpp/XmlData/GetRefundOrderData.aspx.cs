using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.Finance;
using DBC.Ors.Models.Sales;
using DBC.Ors.Services;
using System.Text;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Models.RMA;
using DBC.Ors.Models.WorkFlow;
using DBC.Ors.Models.Infrastructures.MemberShip;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetRefundOrderData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["IDs"] == null)
                return;
            var svc = ServiceLocator.Resolve<IModelService>();
            var models = svc.SelectOrEmpty(new RefundOrderQuery()
            {
                IDs = Request["IDs"].ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
            });
            var orders = svc.SelectOrEmpty(new OrderQuery() { IDs = models.Select(o => o.OrderID ?? 0).ToArray(), Includes = new string[] { "Fund" } });
            var paymentMethods = svc.SelectOrEmpty(new PaymentMethodQuery());
            var returnOrders = svc.SelectOrEmpty(new ReturnOrderQuery() { IDs = models.Select(o => o.ReturnOrderID ?? 0).ToArray() });
            var steps = svc.SelectOrEmpty(new StepQuery() { IDs = models.Where(o => o.StateID != null).Select(o => o.StateID ?? 0).ToArray() });
            var customers = svc.SelectOrEmpty(new CustomerQuery() { IDs = orders.Select(o => o.CustomerID ?? 0).ToArray() });
            foreach (var item in models)
            {
                item.Step = steps.FirstOrDefault(o => o.ID == item.StateID);
                item.PaymentMethod = paymentMethods.FirstOrDefault(o => o.ID == item.PaymentMethodID);
                item.Order = orders.FirstOrDefault(o => o.ID == item.OrderID);
                item.Order.Customer = customers.FirstOrDefault(o => o.ID == item.Order.CustomerID);
                item.ReturnOrder = returnOrders.FirstOrDefault(o => o.ID == item.ReturnOrderID);
            }

            StringBuilder sbXML = new StringBuilder();
            sbXML.Append("<xml>");
            foreach (var model in models)
            {
                sbXML.Append("<row>")
                    .Append("<RefundCode>").Append(model.Code).Append("</RefundCode>")
                    .Append("<OrderCode>").Append(model.Order.Code).Append("</OrderCode>")
                    .Append("<OrderDate>").Append(model.Order.CreatedAt).Append("</OrderDate>")
                    .Append("<ReturnOrderCode>").Append(model.ReturnOrder == null ? "" : model.ReturnOrder.Code).Append("</ReturnOrderCode>")
                    .Append("<RefundAmount>").Append(model.Amount).Append("</RefundAmount>")
                    .Append("<PaymentMethodName>").Append(model.PaymentMethod.Name).Append("</PaymentMethodName>")
                    .Append("<CustomerName>").Append(model.Order.Customer.Name).Append("</CustomerName>")
                    .Append("<CustomerEmail>").Append(model.Order.Customer.Email).Append("</CustomerEmail>")
                    .Append("<CustomerPhone>").Append(model.Order.Customer.Phone).Append("</CustomerPhone>")
                    .Append("<CustomerMobile>").Append(model.Order.Customer.Mobile).Append("</CustomerMobile>")
                    .Append("<OrderAmount>").Append(model.Order.Fund.Amount).Append("</OrderAmount>")
                    .Append("<PayableAmount>").Append(model.Order.Fund.PayableAmount).Append("</PayableAmount>")
                    .Append("<BankName>").Append(model.BankName).Append("</BankName>") //开户行
                    .Append("<AccountName>").Append(model.AccountName).Append("</AccountName>") //账户名
                    .Append("<AccountNo>").Append(model.AccountNo).Append("</AccountNo>") //银行账户
                    .Append("<Note>").Append(model.Note).Append("</Note>") //退款信息
                    .Append("</row>");
            }
            sbXML.Append("</xml>");
            Response.Write(sbXML.ToString());
        }
    }
}