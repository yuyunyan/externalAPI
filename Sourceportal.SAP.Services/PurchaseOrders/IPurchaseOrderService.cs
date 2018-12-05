using Sourceportal.SAP.Domain.Models.Requests.PurchaseOrders;
using Sourceportal.SAP.Domain.Models.Responses.PurchaseOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.PurchaseOrders
{
    public interface IPurchaseOrderService
    {
        PurchaseOrderResponse GetPurchaseOrder(string externalId);

        PurchaseOrderResponse SetPurchaseOrder(PurchaseOrderRequest purchaseOrder);
    }
}
