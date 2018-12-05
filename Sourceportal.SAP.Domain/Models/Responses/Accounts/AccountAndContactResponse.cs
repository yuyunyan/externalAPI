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
    public class AccountAndContactResponse : BaseResponse
    {
        [DataMember(Name = "accountExternalId")]
        public string AccountExternalId { get; set; }

        [DataMember(Name = "contactExternalId")]
        public string ContactExternalId { get; set; }
    }
}
