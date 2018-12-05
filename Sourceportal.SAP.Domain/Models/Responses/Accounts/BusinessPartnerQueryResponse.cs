using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.Accounts
{
    [DataContract]
    public class BusinessPartnerQueryResponse : BusinessPartnerResponse
    {
        [DataMember(Name ="isSupplier")]
        public bool IsSupplier { get; set; }

        [DataMember(Name = "isCustomer")]
        public bool IsCustomer { get; set; }
    }
}
