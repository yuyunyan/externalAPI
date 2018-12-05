using Sourceportal.SAP.Domain.Models.Requests.PurchaseOrders;
using Sourceportal.SAP.Domain.Models.Responses.PurchaseOrders;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.PurchaseOrders;
using Sourceportal.SAP.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Sourceportal.SAP.API.Controllers
{
    public class PurchaseOrdersController : ApiController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        [Route("purchaseorder/get")]
        public PurchaseOrderResponse GetPurchaseOrder(string externalId)
        {
            return _purchaseOrderService.GetPurchaseOrder(externalId);
        }

        [HttpPost]
        [Route("purchaseorder/set")]
        public PurchaseOrderResponse SetPurchaseOrder(PurchaseOrderRequest po)
        {
            return _purchaseOrderService.SetPurchaseOrder(po);
        }

        [HttpPost]
        [Route("sap/purchaseorder/set")]
        public void SapUpdatePurchaseOrder(Trigger request)
        {
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, ObjectTypeEnums.PurchaseOrder.ToString(), host);
        }

    }
}