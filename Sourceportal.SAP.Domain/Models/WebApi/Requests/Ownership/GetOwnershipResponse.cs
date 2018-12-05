using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.WebApi.Requests.Ownership
{
    [DataContract]
    public class GetOwnershipResponse : BaseResponse
    {
        [DataMember(Name = "objectId")]
        public int ObjectID { get; set; }

        [DataMember(Name = "objectTypeId")]
        public int ObjectTypeID { get; set; }

        [DataMember(Name = "owners")]
        public List<Owner> Owners { get; set; }
    }
}
