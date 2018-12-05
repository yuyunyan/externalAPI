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
    public class BusinessPartnerResponse : BaseResponse
    {
        [DataMember(Name = "externalId")]
        public string BpExternalId { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "customerDetails")]
        public AccountTypeDetails CustomerDetails { get; set; }

        [DataMember(Name = "supplierDetails")]
        public AccountTypeDetails SupplierDetails { get; set; }

        [DataMember(Name = "contactExternalIds")]
        public List<PairedIdsResponse> ContactExternalIds { get; set; }

        [DataMember(Name = "locationExternalIds")]
        public List<PairedIdsResponse> LocationExternalIds { get; set; }

        [DataMember(Name = "hierarchy")]
        public AccountHierarchyResponse Hierarchy { get; set; }
    }

    [DataContract]
    public class PairedIdsResponse
    {
        [DataMember(Name = "id")]
        public int LocalId { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }
    }

}
