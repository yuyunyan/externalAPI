using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.SalesOrders
{
    [DataContract]
    public class SalesOrderDetailsResponse : BaseResponse
    {
        [DataMember(Name = "externalId")]
        public string SalesOrderExternalId { get; set; }

        [DataMember(Name = "lines")]
        public List<ItemProductSpecs> Lines { get; set; }

        [DataMember(Name = "items")]
        public List<PairedIdsResponse> Items { get; set; }

    }

    [DataContract]
    public class ItemProductSpecs
    {
        [DataMember(Name = "lineNum")]
        public int LineNum { get; set; }

        [DataMember(Name = "productSpec")]
        public string ProductSpec { get; set; }
    }
}
