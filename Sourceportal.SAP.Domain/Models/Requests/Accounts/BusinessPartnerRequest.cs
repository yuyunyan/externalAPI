using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Accounts
{
    [DataContract]
    public class BusinessPartnerRequest
    {
        [DataMember(Name = "accountDetails")]
        public AccountDetails BpDetails { get; set; }

        [DataMember(Name = "locations")]
        public List<LocationDetails> Locations { get; set; }

        [DataMember(Name = "contacts")]
        public List<ContactDetails> Contacts { get; set; }
    }
}
