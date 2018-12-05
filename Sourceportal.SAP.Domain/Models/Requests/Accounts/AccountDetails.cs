using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Accounts
{
    [DataContract]
    public class AccountDetails
    {
        [DataMember(Name = "externalId")]
        public string ExternalID { get; set; }

        [DataMember(Name = "companyType")]
        public string CompanyType { get; set; }

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

        [DataMember(Name = "currencyCodeExternalId")]
        public string CurrencyCodeExternalId { get; set; }

        [DataMember(Name = "ownership")]
        public Ownership Ownership { get; set; }

        [DataMember(Name = "approvedVendor")]
        public bool ApprovedVendor { get; set; }

        [DataMember(Name = "shippingInstructions")]
        public string ShippingInstructions { get; set; }

        [DataMember(Name = "hierarchy")]
        public AccountHierarchyRequest Hierarchy { get; set; }
    }

}
