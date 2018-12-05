using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.PurchaseOrders
{
    [DataContract]
    public class PurchaseOrderResponse : BaseResponse
    {
        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "items")]
        public List<PairedIdsResponse> Items { get; set; }
    }
}
