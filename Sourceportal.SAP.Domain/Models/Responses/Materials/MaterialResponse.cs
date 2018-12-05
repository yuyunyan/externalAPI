using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.Materials
{
    [DataContract]
    public class MaterialResponse : BaseResponse
    {
        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }
    }
}
