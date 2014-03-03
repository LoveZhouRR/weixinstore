using DBC.Ors.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.WMS;
using DBC.Ors.Models.Infrastructures;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetPickOrderData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["DeliveryOrderIDs"] == null)
                return;
            var svc = ServiceLocator.Resolve<IModelService>();
            var orderModels = svc.SelectOrEmpty(new DeliveryOrderQuery()
            {
                IDs = Request["DeliveryOrderIDs"].ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
            });
            StringBuilder sbxml = new StringBuilder();
            sbxml.Append("<xml>");
            foreach (var orderModel in orderModels)
            {
                var pickItems = svc.SelectOrEmpty(new PickOrderItemQuery()
                    {
                        PickOrderIDs = new long[] {long.Parse(orderModel.PickOrderID+"")}
                    });
                foreach (var pickOrderItem in pickItems)
                {
                    var materials = svc.SelectOrEmpty(new MaterialQuery()
                        {
                            IDs = new long[] {long.Parse(pickOrderItem.MaterialID+"")}
                        });
                    foreach (var material in materials)
                    {
                        var spis = svc.Select<StockPositionInventory>(new StockPositionInventoryQuery() {
                            IDs = new long[] { long.Parse(pickOrderItem.StockPositionInventoryID + "") }
                        }).ToList();
                        var sps = svc.Select<StockPosition>(new StockPositionQuery() { 
                            IDs = new long[] { long.Parse(spis[0].StockPositionID+"")}
                        }).ToList();
                        sbxml.Append("<row>")
                             .Append("<DeliveryOrderID>").Append(orderModel.Code).Append("</DeliveryOrderID>")
                             .Append("<MaterialID>").Append(material.Code).Append("</MaterialID>")
                             .Append("<MaterialName>").Append(material.Name).Append("</MaterialName>")
                             .Append("<Quantity>").Append(pickOrderItem.Quantity).Append("</Quantity>")
                             .Append("<StockPositionInventoryID>")
                             .Append(sps[0].Code)
                             .Append("</StockPositionInventoryID>")
                             .Append("</row>");
                    }
                }
            }
            
            sbxml.Append("</xml>");
            Response.Write(sbxml.ToString());
        }
    }
}