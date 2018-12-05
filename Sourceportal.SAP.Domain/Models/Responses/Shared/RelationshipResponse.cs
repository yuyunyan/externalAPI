using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [DataContract]
    public class RelationshipResponse
    {
        [DataMember(Name = "uuid")]
        public string UUID { get; set; }

        [DataMember(Name = "internalId")]
        public string InternalId { get; set; }

        [DataMember(Name = "roleCode")]
        public string RoleCode { get; set; }

    }
}
