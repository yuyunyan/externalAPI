using Sourceportal.SAP.Domain.Models.Requests.Accounts;
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
    public class BusinessPartnerIncomingResponse : BaseResponse
    {
        [DataMember(Name = "accountDetails")]
        public AccountDetailsIncoming AccountDetails { get; set; }

        [DataMember(Name = "locations")]
        public List<LocationDetails> Locations { get; set; }

        [DataMember(Name = "contacts")]
        public List<ContactDetails> Contacts { get; set; }
    }

    [DataContract]
    public class AccountDetailsIncoming
    {
        [DataMember(Name = "externalId")]
        public string ExternalID { get; set; }

        [DataMember(Name = "companyType")]
        public string CompanyType { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "accountTypes")]
        public List<string> AccountTypes { get; set; }

        [DataMember(Name = "website")]
        public string Website { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "vendorNum")]
        public string VendorNum { get; set; }

        [DataMember(Name = "approvedVendor")]
        public bool ApprovedVendor { get; set; }

        [DataMember(Name = "customerDetails")]
        public AccountTypeDetails CustomerDetails { get; set; }

        [DataMember(Name = "supplierDetails")]
        public AccountTypeDetails SupplierDetails { get; set; }

        [DataMember(Name = "hierarchy")]
        public AccountHierarchyResponse Hierarchy { get; set; }
    }
}
