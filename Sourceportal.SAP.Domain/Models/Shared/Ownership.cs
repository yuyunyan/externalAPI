using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [DataContract]
    public class Ownership
    {
        [DataMember(Name = "leadOwner")]
        public Owner LeadOwner { get; set; }

        [DataMember(Name = "secondOwner")]
        public Owner SecondOwner { get; set; }
    }


    [DataContract]
    public class Owner
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "percentage")]
        public decimal Percentage { get; set; }
    }
}
