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
    public class AccountDetailsResponse : BaseResponse
    {
        [DataMember(Name = "accountExternalId")]
        public string AccountExternalId { get; set; }

        [DataMember(Name = "notes")]
        public string Notes { get; set; }

        [DataMember(Name = "locationExternalIds")]
        public List<string> LocationExternalIds { get; set; }

        [DataMember(Name = "contactExternalIds")]
        public List<string> ContactExternalIds { get; set; }
    }
}
