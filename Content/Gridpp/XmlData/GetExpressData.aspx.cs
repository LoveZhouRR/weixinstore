using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Models.Infrastructures.Shared;
using DBC.Ors.Models.WMS;
using DBC.Ors.Services;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class GetExpressData : System.Web.UI.Page
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
            var contactIds = "";
            foreach (var orderModel in orderModels)
            {
                contactIds = contactIds + orderModel.ContactID + ",";
            }
            var models = svc.SelectOrEmpty(new DeliveryOrderContactQuery()
            {
                IDs = contactIds.ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
            });
            StringBuilder sbxml = new StringBuilder();
            sbxml.Append("<xml>");
            foreach (var model in models)
            {
                var location = new Area[] { new Area(), new Area(), new Area(), new Area() };
                var tempAreaID = model.AreaID;
                var tempModel = svc.SelectOrEmpty(new AreaQuery()
                {
                    IDs = (tempAreaID + ",").ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
                });
                location[int.Parse(tempModel.FirstOrDefault().Depth.ToString()) - 1] = tempModel.FirstOrDefault();
                while (tempModel.FirstOrDefault().ParentID > 0)
                {
                    tempAreaID = tempModel.FirstOrDefault().ParentID;
                    tempModel = svc.SelectOrEmpty(new AreaQuery()
                    {
                        IDs = (tempAreaID + ",").ToString().Trim(',').Split(',').Select(o => Convert.ToInt64(o)).ToArray()
                    });
                    location[int.Parse(tempModel.FirstOrDefault().Depth.ToString()) - 1] = tempModel.FirstOrDefault();
                }

                sbxml.Append("<row>")
               .Append("<ID>").Append(model.ID).Append("</ID>")
               .Append("<SenderName>").Append("").Append("</SenderName>")
               .Append("<SenderCompany>").Append("").Append("</SenderCompany>")
               .Append("<SenderAddress>").Append("").Append("</SenderAddress>")
               .Append("<SenderCompanyAddress>").Append("").Append("</SenderCompanyAddress>")
               .Append("<SenderCity>").Append("").Append("</SenderCity>")
               .Append("<SenderProvince>").Append("").Append("</SenderProvince>")
               .Append("<SenderCountry>").Append("").Append("</SenderCountry>")
               .Append("<SenderPostalCode>").Append("").Append("</SenderPostalCode>")
               .Append("<ReceiverName>").Append(model.Name).Append("</ReceiverName>")
               .Append("<ReceiverAddress>").Append(model.Address).Append("</ReceiverAddress>")
               .Append("<ReceiverCompanyAddress>").Append(model.Address).Append("</ReceiverCompanyAddress>")
               .Append("<ReceiverPhoneNumber>").Append(model.Phone+"  "+model.Mobile).Append("</ReceiverPhoneNumber>")
               .Append("<ReceiverPostalCode>").Append(model.Zip).Append("</ReceiverPostalCode>")
               .Append("<ReceiverCity>").Append(location[2].Name).Append("</ReceiverCity>")
               .Append("<ReceiverProvince>").Append(location[1].Name).Append("</ReceiverProvince>")
               .Append("<ReceiverCountry>").Append(location[0].Name).Append("</ReceiverCountry>")
               .Append("<ItemNameCount>").Append("").Append("</ItemNameCount>")
                .Append("</row>");
            }
            sbxml.Append("</xml>");
            Response.Write(sbxml.ToString());
        }
    }
}