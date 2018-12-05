using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Middleware.Accounts
{
    [DataContract]
    public class BusinessPartnerSync : MiddlewareSyncBase
    {
        public BusinessPartnerSync(string externalId) : base(externalId)
        {
        }

        [DataMember(Name = "accountDetails")]
        public AccountDetailsIncoming AccountDetails { get; set; }

        [DataMember(Name = "locations")]
        public List<LocationDetails> Locations { get; set; }

        [DataMember(Name = "contacts")]
        public List<ContactDetails> Contacts { get; set; }
    }
}
