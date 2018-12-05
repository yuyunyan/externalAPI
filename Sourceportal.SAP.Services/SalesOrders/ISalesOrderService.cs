using Sourceportal.SAP.Domain.Models.Requests.SalesOrders;
using Sourceportal.SAP.Domain.Models.Responses.SalesOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.SalesOrders
{
    public interface ISalesOrderService
    {
        SalesOrderDetailsResponse GetSalesOrder(string soExternalId);
        SalesOrderDetailsResponse SetSalesOrder(SalesOrderDetailsRequest salesOrder);
        SalesOrderDetailsResponse DeleteSalesOrderLine(string salesOrderExternalId, int salesOrderLineNum);
        //SalesOrderDetailsResponse UpdateSalesOrderLine(string salesOrderExternalId, SalesOrderLineDetail line);
    }
}
