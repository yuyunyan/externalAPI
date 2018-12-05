using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Accounts
{
    [DataContract]
    public class AccountHierarchyRequest
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "parentName")]
        public string ParentName { get; set; }

        [DataMember(Name = "sapHierarchyId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentGroupId")]
        public string ParentGroupId { get; set; }

        [DataMember(Name = "regionId")]
        public int AssignTo { get; set; }

        [DataMember(Name = "childGroupId")]
        public string childGroupId { get; set; }

    }
}
