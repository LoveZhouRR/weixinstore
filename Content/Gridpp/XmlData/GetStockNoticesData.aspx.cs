using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Models.WMS;
using DBC.Ors.Services;
using System.Text;
using DBC.Ors.Models.WorkFlow;
using DBC.Ors.Models.Sales;
using DBC.Ors.Models.Infrastructures;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetStockNoticesData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String[] referTypes = { "采购单", "退货单", "借货单", "移仓单", "转换单", "损益单" };
            if (Request["StockNoticeIDs"] == null)
                return;
            var svc = ServiceLocator.Resolve<IModelService>();
            var StockNoticeModels = svc.SelectOrEmpty(new StockNoticeQuery()
            {
                IDs = Request["StockNoticeIDs"].ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
            });

            StringBuilder sbxml = new StringBuilder();
            sbxml.Append("<xml>");
            foreach (var model in StockNoticeModels)
            {
                var step = svc.Select<Step>(new StepQuery() { IDs = new long[] { long.Parse(model.State + "") } }).ToList()[0];
                var items = svc.Select<StockNoticeItem>(new StockNoticeItemQuery() { StockNoticeIDs = new long[] { long.Parse(model.ID + "") } });
                foreach (var i in items)
                {
                    var specification = svc.Select<Material>(new MaterialQuery()
                    {
                        IDs = new long[] { long.Parse(i.MaterialID + "") }
                    }).ToList();
                    sbxml.Append("<row>");
                    sbxml.Append("<Code>").Append(model.Code).Append("</Code>")
                        .Append("<ReferType>").Append(referTypes[int.Parse(model.ReferType + "") - 1]).Append("</ReferType>")
                        .Append("<State>").Append(step.Name).Append("</State>")
                        .Append("<Note>").Append(model.Note).Append("</Note>");
                    sbxml.Append("<SysNo>").Append(i.ID).Append("</SysNo>")
                        .Append("<SKU>").Append(specification[0].Code).Append("</SKU>")
                        .Append("<ProductName>").Append(specification[0].Name).Append("</ProductName>")
                        .Append("<StockQuantity>").Append(i.Quantity).Append("</StockQuantity>")
                        .Append("<ReceiveQuantity>").Append(i.ReceivedQty).Append("</ReceiveQuantity>");

                    sbxml.Append("</row>");
                }
            }
            sbxml.Append("</xml>");
            Response.Write(sbxml.ToString());
        }
    }
}