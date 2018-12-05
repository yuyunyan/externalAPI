using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.WebApi.Requests.Ownership
{
    [DataContract]
    public class Owner
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "id")]
        public string UserId { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalID { get; set; }

        [DataMember(Name = "percentage")]
        public decimal Percentage { get; set; }
    }
}
