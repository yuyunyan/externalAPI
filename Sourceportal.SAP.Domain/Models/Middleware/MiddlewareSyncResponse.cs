using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Middleware
{
    public class MiddlewareSyncResponse : MiddlewareResponse
    {
        [DataMember(Name = "transactionId")]
        public string TransactionId { get; set; }

        [DataMember(Name = "objectId")]
        public int ObjectId { get; set; }

        [DataMember(Name = "objectType")]
        public string ObjectType { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
