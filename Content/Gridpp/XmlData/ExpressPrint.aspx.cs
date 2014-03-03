using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBC.Ors.Services;
using DBC.Ors.Models.WMS;
using DBC.Ors.Models.Sales;
using DBC.Ors.UI.Web.Mvc.ERP.Areas.WMS.Controllers.ExpressProxy;
using DBC.Ors.UI.Web.Mvc.ERP.Areas.WMS.Controllers.ExpressProxy.Fedex;

namespace DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData
{
    public partial class ExpressPrint : System.Web.UI.Page
    {
        protected string OnloadFunction = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //long doID = Request["DOID"] == null ? 0 : Convert.ToInt64(Request["DOID"]);
                int sequenceNumber = Request["PackageID"] == null ? 1 : Convert.ToInt32(Request["PackageID"]);
                //var svc = ServiceLocator.Resolve<IModelService>("Internal");
                //DeliveryOrder deliveryOrder = svc.SelectOrEmpty(new DeliveryOrderQuery() { ID = doID, Includes = new string[] { "Items" } }).FirstOrDefault();
                //List<Package> package = svc.SelectOrEmpty(new PackageQuery() { DeliveryOrderIDs = new long[] { deliveryOrder.ID??0 }, OrderField = "ID" }).ToList();
                //OrderItem orderItem = svc.Select(new OrderItemQuery() { IDs = deliveryOrder.Items.Select(o => o.ReferID ?? 0).ToArray(), Includes = new string[] { "Order" } }).FirstOrDefault();

                ShipmentInfo shipmentInfo = new ShipmentInfo();
                shipmentInfo.DOCode = "";
                shipmentInfo.Units = ShipmentInfo.WeightUnits.LB;
                shipmentInfo.Weight = 50M;

                shipmentInfo.OriginPersonName = "Sender Name";
                shipmentInfo.OriginCompanyName = "Sender Company Name";
                shipmentInfo.OriginPhoneNumber = "0805522713";
                shipmentInfo.OriginStreetLines = new string[1] { "Address Line 1" };
                shipmentInfo.OriginCity = "SHANGHAI";
                shipmentInfo.OriginStateProvince = "";
                shipmentInfo.OriginPostalCode = "200233";
                shipmentInfo.OriginCountryCode = "CN";

                shipmentInfo.DestinationPersonName = "Recipient Name";
                shipmentInfo.DestinationCompanyName = "Recipient Company Name";
                shipmentInfo.DestinationPhoneNumber = "9012637906";
                shipmentInfo.DestinationStreetLines = new string[1] { "Address Line 1" };
                shipmentInfo.DestinationCity = "Windsor";
                shipmentInfo.DestinationStateProvince = "CT";
                shipmentInfo.DestinationPostalCode = "06006";
                shipmentInfo.DestinationCountryCode = "US";

                shipmentInfo.PackageInfos = new List<PackageInfo>();
                shipmentInfo.SequenceNumber = sequenceNumber;
                shipmentInfo.PackageInfos.Add(new PackageInfo() { Weight = 50M, Height = 12, Width = 13, Length = 14, Units = PackageInfo.LinearUnits.IN });
                shipmentInfo.PackageInfos.Add(new PackageInfo() { Weight = 50M, Height = 12, Width = 13, Length = 14, Units = PackageInfo.LinearUnits.IN });

                try
                {
                    this.imgExpress.Src = FedexUtil.Get(shipmentInfo);
                    //this.imgExpress.Src = "http://192.168.1.14:8013/Content/794809821138.png";
                    OnloadFunction = "PrintPage()";
                }
                catch (Exception err)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = err.Message;
                }
                OnloadFunction = "PrintPage()";
            }
        }
    }
}