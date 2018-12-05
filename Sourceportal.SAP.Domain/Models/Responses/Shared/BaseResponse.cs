using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.Shared
{
    [DataContract]
    public class BaseResponse
    {
        [DataMember(Name = "objectId")]
        public int ObjectId { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "errors")]
        public List<string> Errors { get; set; }

        [DataMember(Name = "warnings")]
        public List<string> Warnings { get; set; }

        [DataMember(Name = "note")]
        public string Note { get; set; }
    }
}
