using Sourceportal.SAP.Domain.Models.Requests.SalesOrders;
using Sourceportal.SAP.Domain.Models.Responses.SalesOrders;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.ProductSpec;
using Sourceportal.SAP.Services.SalesOrders;
using Sourceportal.SAP.Services.Utils;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Sourceportal.SAP.API.Controllers
{
    public class SalesOrdersController : ApiController
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IProductSpecService _productSpecService;

        public SalesOrdersController(ISalesOrderService salesOrderService, IProductSpecService productSpecService)
        {
            _salesOrderService = salesOrderService;
            _productSpecService = productSpecService;
        }

        [HttpGet]
        [Route("salesorder/get")]
        public SalesOrderDetailsResponse GetSalesOrder(string externalId)
        {
            return _salesOrderService.GetSalesOrder(externalId);
        }

        [HttpPost]
        [Route("salesorder/set")]
        public SalesOrderDetailsResponse SetSalesOrder(SalesOrderDetailsRequest salesOrder)
        {
            return _salesOrderService.SetSalesOrder(salesOrder);
        }

        [HttpPost]
        [Route("salesorder/deleteline")]
        public SalesOrderDetailsResponse DeleteSalesOrderLine(string salesOrderExternalId, int salesOrderLineNum)
        {
            return _salesOrderService.DeleteSalesOrderLine(salesOrderExternalId, salesOrderLineNum);
        }

        [HttpPost]
        [Route("sap/salesorder/set")]
        public void SapUpdateSalesOrder(Trigger request)
        {
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, ObjectTypeEnums.SalesOrder.ToString(), host);
        }


    }
}