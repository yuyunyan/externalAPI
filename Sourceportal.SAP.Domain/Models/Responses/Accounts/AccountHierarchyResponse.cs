using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.Accounts
{
    [DataContract]
    public class AccountHierarchyResponse : BaseResponse
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "parentName")]
        public string ParentName { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentGroupId")]
        public string ParentGroupId { get; set; }

        [DataMember(Name = "parentUUID")]
        public string ParentUUID { get; set; }

        [DataMember(Name = "children")]
        public List<AccountHierarchyChildResponse> Children { get; set; }
    }

    [DataContract]
    public class AccountHierarchyChildResponse
    {
        [DataMember(Name = "childName")]
        public string ChildName { get; set; }

        [DataMember(Name = "childId")]
        public string ChildId { get; set; }

        [DataMember(Name = "assignedToAccount")]
        public bool AssignedToAccount { get; set; }

    }
}
