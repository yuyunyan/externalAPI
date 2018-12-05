using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Middleware
{
    [DataContract]
    public class MiddlewareSyncBase
    {
        public MiddlewareSyncBase(string externalId)
        {
            ExternalId = externalId;
        }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; private set; }

    }
}
