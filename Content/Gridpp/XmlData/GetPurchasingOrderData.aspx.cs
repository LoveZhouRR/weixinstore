using DBC.Ors.Models.Purchasing;
using DBC.Ors.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using DBC.Ors.Models.WorkFlow;
using DBC.Ors.Models.Infrastructures;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetPurchasingOrderData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["PurchasingOrderIDs"] == null)
                return;
            var svc = ServiceLocator.Resolve<IModelService>();
            var orderModels = svc.SelectOrEmpty(new PurchaseOrderQuery()
            {
                IDs = Request["PurchasingOrderIDs"].ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
            });
            StringBuilder sbxml = new StringBuilder();
            sbxml.Append("<xml>");
            foreach (var model in orderModels)
            {
                var step = svc.Select<Step>(new StepQuery(){ IDs = new long[]{long.Parse(model.State+"")}}).ToList()[0];
                var items = svc.Select<PurchaseOrderItem>(new PurchaseOrderItemQuery() { PurchaseOrderIDs = new long[] {long.Parse(model.ID+"")}});
                decimal total = 0;
                int count = 0;
                foreach (var i in items)
                {
                    count++;
                    decimal itemTotal = 0;
                    var specification = svc.Select<Material>(new MaterialQuery()
                    {
                        IDs = new long[] { long.Parse(i.MaterialID + "") }
                    }).ToList();
                    itemTotal = decimal.Parse(i.UnitPrice + "") * int.Parse(i.OrderedQty+"");
                    total += itemTotal;
                    sbxml.Append("<row>");
                    sbxml.Append("<Code>").Append(model.Code).Append("</Code>")   
                        .Append("<Note>").Append(model.Note).Append("</Note>")
                        .Append("<State>").Append(step.Name).Append("</State>")
                        .Append("<Memo>").Append(model.Memo).Append("</Memo>");
                    sbxml.Append("<SKU>").Append(specification[0].Code).Append("</SKU>")
                        .Append("<ProductName>").Append(specification[0].Name).Append("</ProductName>")
                        .Append("<Quantity>").Append(i.OrderedQty).Append("</Quantity>")
                        .Append("<Price>").Append(i.UnitPrice).Append("</Price>")
                        .Append("<itemTotal>").Append(itemTotal).Append("</itemTotal>");
                    sbxml.Append("</row>");
                }
                
            }
            sbxml.Append("</xml>");
            Response.Write(sbxml.ToString());
        }
    }
}