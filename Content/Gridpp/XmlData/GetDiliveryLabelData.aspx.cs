using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.WMS;
using DBC.Ors.Services;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetDiliveryLabelData : System.Web.UI.Page
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
                
                var packages = svc.SelectOrEmpty(new PackageQuery()
                    {
                        DeliveryOrderIDs = new long[] { long.Parse(orderModel.ID+"")}
                    }).ToList();
                long begin = 0;
                begin = long.Parse(packages[0].ID + "");
                foreach (var package in packages)
                {
                    sbxml.Append("<row>")
                     .Append("<Code>").Append(orderModel.Code).Append("</Code>")
                     .Append("<PackageID>").Append(long.Parse(package.ID + "") - begin + 1).Append("</PackageID>")
                     .Append("<Number>").Append(packages.Count).Append("</Number>")
                     .Append("</row>");
                }
                
            }
            sbxml.Append("</xml>");
            Response.Write(sbxml.ToString());
        }
    }
}